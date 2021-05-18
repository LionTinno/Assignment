using CsvHelper.Configuration;
using FMS.Models.RequestParam;

namespace FMS.Mapper
{
    public sealed class CSVMapper : ClassMap<FileModel>
    {
        public CSVMapper()
        {
            Map(x=> x.TransactionId).Name("Transaction Identificator");
            Map(x=> x.Amount).Name("Amount");
            Map(x=> x.CurrencyCode).Name("Currency Code");
            Map(x=> x.TransactionDate).Name("Transaction Date");
            Map(x=> x.Status).Name("Status");
        }
    }
}