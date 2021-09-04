using aadcapi.Utils.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace aadcapi.Models
{
    public class RoleControllerModel
    {
        public string Role { get; set; }

        public string Controller { get; set; }

        public string ModelProperty { get; set; }

        public string ModelValue { get; set; }

        public IEnumerable<string> ModelValues { get; set; }

        public RoleControllerModel() {
            ModelValues = new List<string>();
        }

        /*  
        // Leaving this here because once again I forgot that json is a thing....let's just do that.
        // Maybe part of me just likes regular expressions?
        public RoleControllerModel(string Input)
        {
            //
            // Assumed format: pipe delimited set of key:value pairs that match
            // property names on the model.
            // ModelValues: is assumed to be a comma separated list of values.
            //

            if (String.IsNullOrEmpty(Input)) return;

            Regex tokenPattern      = new Regex(@"(?<!\\)\|", RegexOptions.Compiled);
            Regex rolePattern       = new Regex(@"(?i)^Role\s*:", RegexOptions.Compiled);
            Regex controllerPattern = new Regex(@"(?i)^Controller\s*:", RegexOptions.Compiled);
            Regex propertyPattern   = new Regex(@"(?i)^ModelProperty\s*:", RegexOptions.Compiled);
            Regex valuePattern      = new Regex(@"(?i)^ModelValue\s*:", RegexOptions.Compiled);
            Regex valuesPattern     = new Regex(@"(?i)^ModelValues\s*:", RegexOptions.Compiled);

            var inputTokens = tokenPattern.Split(Input);

            try
            {
                foreach (var t in inputTokens)
                {
                    string token = t?.TrimStart();
                    if (String.IsNullOrEmpty(token)) { continue; }

                    string value = String.Concat(t.Split(new char[] { ':' }).Skip(1)).Trim();
                    if (String.IsNullOrEmpty(value)) { continue; }

                    if (rolePattern.IsMatch(token)) { Role = value; continue; }

                    if (controllerPattern.IsMatch(token)) { Controller = value; continue; }

                    if (propertyPattern.IsMatch(token)) { ModelProperty = value; continue; }

                    if (valuePattern.IsMatch(token)) { ModelValue = value; continue; }

                    if (valuesPattern.IsMatch(token))
                    {
                        ModelValues = value.Split(new char[] { ',' }).Select(x => x.Trim()).Where(x => !String.IsNullOrEmpty(x));
                        continue;
                    }
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine($"RoleControllerModel: Error parsing intput: {Input}");
            }

            if (ModelValues == null) {
                ModelValues = new List<string>();
            }
        }
        */
    }
}