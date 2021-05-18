using System.Collections.Generic;

namespace FMS.Models.ResponseMsg
{
    public class FileManagerResponse : BaseResponseMsg
    {
        public FileManagerResponse()
        {
            Data = new List<FileManagerModel>();
        }
        public List<FileManagerModel> Data { get; set; }
    }

    public class FileManagerModel{
        public string id { get; set; }
        public string payment { get; set; }
        public string Status { get; set; }
    }
}