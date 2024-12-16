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
                        if (kvp.ContainsKey("QueryType"))
                        {
                            int type = int.Parse(kvp["QueryType"]?.ToString() ?? "666");
                            string typeName = RecordTypeName(type);
                            kvp["QueryType"] = typeName;
                        }
                    }
                    return result;
                }
            }

            return null;
        }

        static string RecordTypeName(int recordType) => recordType switch
        {
            1  => "A",
            2  => "NS",
            3  => "MD",
            4  => "MF",
            5  => "CNAME",
            6  => "SOA",
            10 => "NULL",
            11 => "WKS",
            12 => "PTR",
            13 => "HINFO",
            15 => "MX",
            16 => "TXT",
            17 => "RP",
            19 => "X25",
            20 => "ISDN",
            24 => "SIG",
            28 => "AAAA",
            29 => "LOC",
            33 => "SRV",
            43 => "DS",
            46 => "RRSIG",
            47 => "NSEC",
            50 => "NSEC3",
            51 => "NSEC3PARAM",

            // Query types
            252 => "AXFR",
            255 => "ALL",
            _ => $"UNKNOWN:{recordType}",            
        };
    }
}
