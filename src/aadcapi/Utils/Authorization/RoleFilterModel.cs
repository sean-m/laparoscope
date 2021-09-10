using aadcapi.Utils.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace aadcapi.Models
{
    public class RoleFilterModel
    {
        // Authorization scoped to role and context
        public string Role { get; set; }

        public string Context { get; set; }

        // Validation scoped to claim properties and/or model properties
        public string ClaimProperty { get; set; }

        public string ClaimValue { get; set; }

        public string ModelProperty { get; set; }

        public string ModelValue { get; set; }

        public IEnumerable<string> ModelValues { get; set; }

        public RoleFilterModel() {
            ModelValues = new List<string>();
        }
    }
}