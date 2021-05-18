using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AutoMapper;
using CsvHelper;
using FMS.Mapper;
using FMS.Models;
using FMS.Models.RequestParam;
using FMS.Models.ResponseMsg;
using FMS.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FMS.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileManangerController : ControllerBase
    {
        private readonly FMSDbContext _context;
        List<InvalidTransactionResponse> _invalidtrasactionResponse;
        private readonly ILogger<FileManagerResponse> _logger;
        public FileManangerController(ILogger<FileManagerResponse> logger, FMSDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpPost("UploadFile")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            string extension = "." + file.FileName.Split(".")[file.FileName.Split(".").Length - 1];

            if (extension != ".csv" && extension != ".xml")
            {
                ErrorMessageResponse response = new ErrorMessageResponse();
                response.StatusCode = "Error";
                response.Message = "Unknown format";

                return NotFound(response);
            }

            List<FileModel> files = extension == ".csv" ? readCSVfile(file) : readXMLfile(file);

            if (!_ValidateData(files, extension))
            {
                return BadRequest(_invalidtrasactionResponse);
            }
            else
            {
                bool result = await CreateTransaction(files, extension);

                UploadResponse response = new UploadResponse();

                if (result)
                {
                    response.StatusCode = "200";
                    response.Message = "Upload Success";
                    return Ok(response);
                }
                else
                {
                    response.StatusCode = "400";
                    response.Message = "Upload Fail";
                    return BadRequest(response);
                }
            }
        }

        private async Task<bool> CreateTransaction(List<FileModel> files, string extension)
        {
            try
            {
                List<POTransaction> POTransactions = new List<POTransaction>();

                foreach (var item in files)
                {
                    POTransaction PO = new POTransaction();
                    PO.transactionid = item.TransactionId;
                    PO.amount = decimal.Parse(item.Amount);
                    PO.transactiondate = DateTime.ParseExact(item.TransactionDate, extension == ".csv" ? "dd/MM/yyyy hh:mm:ss" : "yyyy-MM-ddTHH:mm:ss", null);
                    PO.currencycode = item.CurrencyCode;
                    PO.status = item.Status;

                    POTransactions.Add(PO);
                }

                if (POTransactions.Count > 0)
                {
                    _context.POTransaction.AddRange(POTransactions);
                    await _context.SaveChangesAsync();

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Can't CreateTransaction. Error Message : " + e.Message);
                throw new Exception(e.Message);
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> getAll()
        {
            try
            {
                FileManagerResponse fileManagerResponse = new FileManagerResponse();

                var POTransactions = await _context.POTransaction.ToListAsync();

                foreach (var item in POTransactions)
                {
                    var model = new FileManagerModel();
                    model.id = item.transactionid;
                    model.payment = item.amount.ToString("#,###0.00") + " " + item.currencycode;

                    switch (item.status)
                    {
                        case "Approved":
                            model.Status = "A";
                            break;
                        case "Failed":
                            model.Status = "R";
                            break;
                        case "Rejected":
                            model.Status = "R";
                            break;
                        case "Finished":
                            model.Status = "D";
                            break;
                        case "Done":
                            model.Status = "D";
                            break;
                    }

                    fileManagerResponse.Data.Add(model);
                }

                fileManagerResponse.StatusCode = "200";
                fileManagerResponse.Message = "Success";

                return Ok(fileManagerResponse);
            }
            catch (Exception e)
            {
                _logger.LogError("Can't Get TransactionbyCurrency. Error Message : " + e.Message);
                throw new Exception(e.Message);
            }
        }

        [HttpGet("transactionbycurrency")]
        public async Task<IActionResult> getTransactionbyCurrency(string currency)
        {
            currency = currency == null ? "" : currency;
            try
            {
                FileManagerResponse fileManagerResponse = new FileManagerResponse();

                var POTransactions = await _context.POTransaction.Where(x => x.currencycode.Contains(currency)).ToListAsync();

                foreach (var item in POTransactions)
                {
                    var model = new FileManagerModel();
                    model.id = item.transactionid;
                    model.payment = item.amount.ToString("#,###0.00") + " " + item.currencycode;

                    switch (item.status)
                    {
                        case "Approved":
                            model.Status = "A";
                            break;
                        case "Failed":
                            model.Status = "R";
                            break;
                        case "Rejected":
                            model.Status = "R";
                            break;
                        case "Finished":
                            model.Status = "D";
                            break;
                        case "Done":
                            model.Status = "D";
                            break;
                    }

                    fileManagerResponse.Data.Add(model);
                }

                fileManagerResponse.StatusCode = "200";
                fileManagerResponse.Message = "Success";

                return Ok(fileManagerResponse);
            }
            catch (Exception e)
            {
                _logger.LogError("Can't Get TransactionbyCurrency. Error Message : " + e.Message);
                throw new Exception(e.Message);
            }
        }

        [HttpGet("transactionbydate")]
        public async Task<IActionResult> getTransactionbyDate(string startdate, string enddate)
        {
            DateTime datetimestart = DateTime.ParseExact(startdate, "yyyyMMdd", null);
            DateTime datetimeend = DateTime.ParseExact(enddate, "yyyyMMdd", null);

            try
            {
                FileManagerResponse fileManagerResponse = new FileManagerResponse();

                var POTransactions = await _context.POTransaction.Where(x => x.transactiondate >= datetimestart.Date && x.transactiondate.Date <= datetimeend.Date).ToListAsync();

                foreach (var item in POTransactions)
                {
                    var model = new FileManagerModel();
                    model.id = item.transactionid;
                    model.payment = item.amount.ToString("#,###0.00") + " " + item.currencycode;

                    switch (item.status)
                    {
                        case "Approved":
                            model.Status = "A";
                            break;
                        case "Failed":
                            model.Status = "R";
                            break;
                        case "Rejected":
                            model.Status = "R";
                            break;
                        case "Finished":
                            model.Status = "D";
                            break;
                        case "Done":
                            model.Status = "D";
                            break;
                    }

                    fileManagerResponse.Data.Add(model);
                }

                fileManagerResponse.StatusCode = "200";
                fileManagerResponse.Message = "Success";

                return Ok(fileManagerResponse);
            }
            catch (Exception e)
            {
                _logger.LogError("Can't Get TransactionbyDate. Error Message : " + e.Message);
                throw new Exception(e.Message);
            }
        }

        [HttpGet("transactionbystatus")]
        public async Task<IActionResult> getTransactionbyStatus(string status)
        {
            try
            {
                FileManagerResponse fileManagerResponse = new FileManagerResponse();

                List<POTransaction> list;

                if (status == "All")
                {
                    list = await _context.POTransaction.ToListAsync();
                }
                else
                {
                    list = await _context.POTransaction.Where(x => x.status == status).ToListAsync();
                }

                foreach (var item in list)
                {
                    var model = new FileManagerModel();
                    model.id = item.transactionid;
                    model.payment = item.amount.ToString("#,###0.00") + " " + item.currencycode;

                    switch (item.status)
                    {
                        case "Approved":
                            model.Status = "A";
                            break;
                        case "Failed":
                            model.Status = "R";
                            break;
                        case "Rejected":
                            model.Status = "R";
                            break;
                        case "Finished":
                            model.Status = "D";
                            break;
                        case "Done":
                            model.Status = "D";
                            break;
                    }

                    fileManagerResponse.Data.Add(model);
                }

                fileManagerResponse.StatusCode = "200";
                fileManagerResponse.Message = "Success";

                return Ok(fileManagerResponse);
            }
            catch (Exception e)
            {
                _logger.LogError("Can't Get TransactionbyStatus. Error Message : " + e.Message);
                throw new Exception(e.Message);
            }
        }

        private List<FileModel> readCSVfile(IFormFile file)
        {
            List<FileModel> csvFileList;

            try
            {
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.CreateSpecificCulture("enUS")))
                    {
                        csv.Context.RegisterClassMap<CSVMapper>();
                        csvFileList = csv.GetRecords<FileModel>().ToList();
                    }
                }

            }
            catch (Exception e)
            {
                _logger.LogError("Can't Read Csv file. Error Message : " + e.Message);
                throw new Exception(e.Message);
            }

            return csvFileList;
        }

        private List<FileModel> readXMLfile(IFormFile file)
        {
            List<FileModel> xmlFileList = new List<FileModel>();

            try
            {
                using (XmlReader reader = XmlReader.Create(file.OpenReadStream()))
                {
                    FileModel model = new FileModel();

                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {
                            switch (reader.Name.ToString())
                            {
                                case "Transaction":
                                    model = new FileModel();
                                    model.TransactionId = reader.GetAttribute("id");
                                    break;
                                case "TransactionDate":
                                    model.TransactionDate = reader.ReadString();
                                    break;
                                case "Amount":
                                    model.Amount = reader.ReadString();
                                    break;
                                case "CurrencyCode":
                                    model.CurrencyCode = reader.ReadString();
                                    break;
                                case "Status":
                                    model.Status = reader.ReadString();
                                    xmlFileList.Add(model);
                                    break;
                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {
                _logger.LogError("Can't Read XML file. Error Message : " + e.Message);
                throw new Exception(e.Message);
            }

            return xmlFileList;
        }

        private bool _ValidateData(List<FileModel> files, string extension)
        {
            bool validateresult = true;
            int counter = 0;

            _invalidtrasactionResponse = new List<InvalidTransactionResponse>();

            foreach (var item in files)
            {
                counter++;

                StringBuilder sb = new StringBuilder();
                string invalidmessage = "";

                decimal decimalresult;
                DateTime datetimeresult;
                string currencyresult;

                if (string.IsNullOrWhiteSpace(item.TransactionId))
                {
                    validateresult = false;
                    sb.AppendLine(" - Transaction Id is empty.");
                    invalidmessage += "Transaction Id is empty,";
                }
                else if (item.TransactionId.Length > 50)
                {
                    validateresult = false;
                    sb.AppendLine(" - Length of Transaction Id is more than 50 characters.");
                    invalidmessage += "Length of Transaction Id is more than 50 characters,";
                }

                if (!decimal.TryParse(item.Amount, out decimalresult))
                {
                    validateresult = false;
                    sb.AppendLine(" - Amount [" + item.Amount + "] is not decimal format.");
                    invalidmessage += "Amount is not decimal format,";
                }

                if (string.IsNullOrWhiteSpace(item.CurrencyCode))
                {
                    validateresult = false;
                    sb.AppendLine(" - Currency Code is empty.");
                    invalidmessage += "Currency Code is empty,";
                }
                else if (!_TryGetCurrencySymbol(item.CurrencyCode, out currencyresult))
                {
                    validateresult = false;
                    sb.AppendLine(" - Currency Code [" + item.CurrencyCode + "] is not currency symbol format.");
                    invalidmessage += "Currency Code is not currency symbol format,";
                }

                string format = extension == ".csv" ? "dd/MM/yyyy hh:mm:ss" : "yyyy-MM-ddTHH:mm:ss";

                if (string.IsNullOrWhiteSpace(item.TransactionDate))
                {
                    validateresult = false;
                    sb.AppendLine(" - Transaction Date is empty.");
                    invalidmessage += "Transaction Date is empty,";
                }
                else if (!DateTime.TryParseExact(item.TransactionDate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out datetimeresult))
                {
                    validateresult = false;
                    sb.AppendLine(" - Transaction Date [" + item.TransactionDate + "] is not datetime format.");
                    invalidmessage += "Transaction Date is not datetime format,";
                }

                if (string.IsNullOrWhiteSpace(item.Status))
                {
                    validateresult = false;
                    sb.AppendLine(" - Status is empty.");
                    invalidmessage += "Status is empty,";
                }
                else
                {
                    switch (extension)
                    {
                        case ".csv":
                            if (item.Status != "Approved" && item.Status != "Failed" && item.Status != "Finished")
                            {
                                validateresult = false;
                                sb.AppendLine(" - Status [" + item.Status + "] is not matched.");
                                invalidmessage += "Status is not matched,";
                            }
                            break;
                        case ".xml":
                            if (item.Status != "Approved" && item.Status != "Rejected" && item.Status != "Done")
                            {
                                validateresult = false;
                                sb.AppendLine(" - Status [" + item.Status + "] is not matched.");
                                invalidmessage += "Status is not matched,";
                            }
                            break;
                    }
                }

                if (sb.Length > 0)
                {
                    sb.Insert(0, "Record [" + counter + "]: Transaction Id - " + item.TransactionId + "\n");
                    _logger.LogWarning(sb.ToString());

                    _invalidtrasactionResponse.Add(new InvalidTransactionResponse()
                    {
                        Transactionid = item.TransactionId,
                        Remark = invalidmessage.Substring(0, invalidmessage.Length - 1)
                    });
                }
            }

            return validateresult;
        }

        private bool _TryGetCurrencySymbol(string ISOCurrencySymbol, out string symbol)
        {
            symbol = CultureInfo
                .GetCultures(CultureTypes.AllCultures)
                .Where(c => !c.IsNeutralCulture)
                .Select(culture =>
                {
                    try
                    {
                        return new RegionInfo(culture.Name);
                    }
                    catch
                    {
                        return null;
                    }
                })
                .Where(ri => ri != null && ri.ISOCurrencySymbol == ISOCurrencySymbol)
                .Select(ri => ri.CurrencySymbol)
                .FirstOrDefault();
            return symbol != null;
        }
    }
}