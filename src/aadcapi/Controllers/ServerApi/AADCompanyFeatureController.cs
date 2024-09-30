using aadcapi.Utils.Authorization;
using SMM.Automation;
using SMM.Helper;
using StreamJsonRpc;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace aadcapi.Controllers.Server
{
    [Authorize]
    public class AADCompanyFeatureController : ApiController
    {
        /// <summary>
        /// Maps to Get-ADSyncAADCompanyFeature cmdlet. This returns enabled status for
        /// features that are global to the AADC install/tenat. These include: 
        /// PasswordHashSync, ForcePasswordChangeOnLogOn, UserWriteback, DeviceWriteback, UnifiedGroupWriteback, GroupWritebackV2.
        /// </summary>
        [ResponseType(typeof(Dictionary<string, object>))]
        public dynamic Get()
        {
            using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                using (var jsonRpc = JsonRpc.Attach(stream))
                {
                    string function = "GetADSyncAADCompanyFeature";
                    Console.WriteLine($">> {function}()");
                    var resultTask = jsonRpc.InvokeAsync<dynamic>(function);
                    resultTask.Wait();
                    var result = Ok(resultTask.Result);
                    return result;
                }
            }
        }
    }
}
