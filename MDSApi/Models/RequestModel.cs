using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MDSApi.Models
{
    public class RequestModel
    {
        public string domain { get; set; }
        public string userName { get; set; }
        public string modelName { get; set; }
        public string versionName { get; set; }
        public string entity { get; set; }
    }
}
