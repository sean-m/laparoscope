using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace aadcapi.Models
{
    public class AadcConnector
    {
        public dynamic Name { get; set; }
        public dynamic Identifier { get; set; }
        public dynamic Description { get; set; }
        public dynamic CreationTime { get; set; }
        public dynamic LastModificationTime { get; set; }
        public dynamic ObjectInclusionList { get; set; }
        public dynamic AttributeInclusionList { get; set; }
        public dynamic AnchorConstructionSettings { get; set; }
        public dynamic CompanyName { get; set; }
        public dynamic Type { get; set; }
        public dynamic Subtype { get; set; }
    }
}