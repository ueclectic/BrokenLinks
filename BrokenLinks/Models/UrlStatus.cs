using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokenLinks.Models
{
    public class UrlStatus
    {
        public UrlStatus(string url, string status)
        {
            Url = url;
            Status = status;
        }
        public string Url;
        public string Status;
    }
}
