using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laparoscope.Models
{
    public class DomainReachabilityStatus
    {
        public dynamic FullName { get; set; }
        public dynamic IsReachable { get; set; }
        public dynamic ExceptionMessage { get; set; }
    }
}