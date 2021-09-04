
Add-Type -Language CSharp -TypeDefinition  @"
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

    public class RoleControllerModel
    {
        public string Role { get; set; }

        public string Controller { get; set; }

        public string ModelProperty { get; set; }

        public string ModelValue { get; set; }

        public List<string> ModelValues { get; set; }

        public RoleControllerModel() {
            ModelValues = new List<string>();
        }

        public RoleControllerModel(string Input)
        {
            // Assumed format: pipe delimited set of key:value pairs that match
            // property names on the model. ModelValues is assumed to be a comma separated
            // list of values.

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
                    string token = t.TrimStart();
                    if (String.IsNullOrEmpty(token)) { continue; }

                    string value = String.Concat(t.Split(new char[] { ':' }).Skip(1)).Trim();
                    if (String.IsNullOrEmpty(value)) { continue; }

                    if (rolePattern.IsMatch(token)) { Role = value; continue; }

                    if (controllerPattern.IsMatch(token)) { Controller = value; continue; }

                    if (propertyPattern.IsMatch(token)) { ModelProperty = value; continue; }

                    if (valuePattern.IsMatch(token)) { ModelValue = value; continue; }

                    if (valuesPattern.IsMatch(token))
                    {
                        ModelValues = value.Split(new char[] { ',' }).Select(x => x.Trim()).Where(x => !String.IsNullOrEmpty(x)).ToList(); 
                        continue;
                    }
                }
            }
            catch // (Exception e)
            {
                //Trace.WriteLine($"");
            }

            if (ModelValues == null) {
                ModelValues = new List<string>();
            }
        }
    }
"@ 


$pattern = [Regex]"(?<!\\)\|"


$valueString = "Role : Admin | Controller : Connector | ModelProperty : Name | ModelValues : garage.mcardletech.com"

$pattern.Split($valueString)


[RoleControllerModel]::new($valueString)

<#

public string Role { get; set; }

public string Controller { get; set; }

public string ModelProperty { get; set; }

public string ModelValue { get; set; }

#>


[RoleControllerModel]$parsed =  @"
{
Role          : "Admin",
Controller    : "Connector",
ModelProperty : "Name",
ModelValues   : [ "garage.mcardletech.com" ]
}
"@ | ConvertFrom-Json

[RoleControllerModel]$parsed = @"
{
Role          : "Admin",
Controller    : "Connector",
ModelProperty : "Name",
ModelValue    : "*"
}
"@ | ConvertFrom-Json

$parsed | ConvertTo-Json -Compress