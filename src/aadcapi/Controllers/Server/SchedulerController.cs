using SMM.Automation;
using SMM.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Mvc;

namespace aadcapi.Controllers.Server
{   
    public class SchedulerController : ApiController
    {
        // GET api/<controller>
        public dynamic Get()
        {
            var runner = new SimpleScriptRunner("Import-Module ADSync; Get-ADSyncScheduler");
            runner.Run();
            return Ok(runner.Results.ToDict());
        }

        // POST api/<controller>
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody] string value)
        {
        }
    }
}