using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using SMM.Automation;
using SMM.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Mvc;

namespace aadcapi.Controllers
{
    public class HealthController : ApiController
    {
        /// <summary>
        /// Endpoint for getting basic health information about the application and AADC service
        /// without authentication. Intended to be polled by monitoring services.
        /// </summary>
        /// <returns>Dictionary<string,string>)</returns>
        [ResponseType(typeof(Dictionary<string,string>))]
        public dynamic Get()
        {
            var result = new Dictionary<string, string>();
            result.Add("BuildTimeUTC", Utils.Globals.LinkerTimeUtc);
            result.Add("Version", Utils.Globals.AssemblyVersion);
            result.Add("StagingMode", Utils.Globals.Staging);
            return result;
        }

        /// <summary>
        /// Posting to this endpoint from localhost will refresh the staging mode status.
        /// Requests from non-local endpoints are denied as computing resources are spent 
        /// when refreshing that value. On the topic of freshness, a scheduled
        /// task running at some interval should post to this endpoint. Spinning a long lived
        /// background thread in an iis hosted app is fraught with nightmares whereas Task
        /// Scheduler is a sysadmin's best friend (maybe not best friend but they're at least
        /// aquanted and on reasonable terms).
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public IHttpActionResult Post()
        {
            // We only want local requests to succeed here. There's a computation cost to updating
            // the global variable and this endpoint isn't expected to be authenticated, the compromise
            // is to update when called by a localhost process. 
            if (!this.ActionContext.RequestContext.IsLocal)
            {
                return StatusCode(HttpStatusCode.Unauthorized);
            }

            // This is not thread safe!!!   It's not the end of the world, just don't post here often.
            Utils.Globals.Staging = Utils.Globals.GetStagingMode();                
            return StatusCode(HttpStatusCode.NoContent);
        }

    }
}
