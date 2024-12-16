using Laparoscope.Models;
using System.IO.Pipes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StreamJsonRpc;

namespace Laparoscope.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class NslookupController : ControllerBase
    {
        [HttpGet]
        public async Task<IEnumerable<Dictionary<string,object>>> Get(string Name, string Server=null, string Type=null)
        {
            using (var stream = new NamedPipeClientStream(".", "Laparoscope", PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                await stream.ConnectAsync();
                using (var jsonRpc = JsonRpc.Attach(stream))
                {
                    string function = "NslookupShort";
                    var result = await jsonRpc.InvokeAsync<IEnumerable<Dictionary<string, object>>>(function, Name.Trim(), Server != null ? new[] { Server?.Trim() } : null, Type?.Trim());
                    foreach (var kvp in result)
                    {
                        if (kvp.ContainsKey("Type"))
                        {
                            int type = int.Parse(kvp["Type"]?.ToString() ?? "666");
                            string typeName = RecordTypeName(type);
                            kvp["Type"] = typeName;
                        }
                    }
                    return result;
                }
            }

            return null;
        }

        static string RecordTypeName(int recordType)
        {
            switch (recordType)
            {
                case 1: return "A";
                case 2: return "NS";
                case 3: return "MD";
                case 4: return "MF";
                case 5: return "CNAME";
                case 6: return "SOA";
                case 11: return "WKS";
                case 12: return "PTR";
                case 13: return "HINFO";
                case 15: return "MX";
                case 16: return "TXT";
                case 17: return "RP";
                case 19: return "X25";
                case 20: return "ISDN";
                case 24: return "SIG";
                case 28: return "AAAA";
                case 29: return "LOC";
                case 33: return "SRV";
                case 43: return "DS";
                case 46: return "RRSIG";
                case 47: return "NSEC";
                case 50: return "NSEC3";
                case 51: return "NSEC3PARAM";
                default:
                    return $"UNKNOWN:{recordType}";
            }
        }
    }
}
