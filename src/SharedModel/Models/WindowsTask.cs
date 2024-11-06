using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using static System.Collections.Specialized.BitVector32;

namespace LaparoscopeShared.Models
{
    public class WindowsTask
    {
        public string WindowTitle { get; set; }
        public string UserName { get; set; }
        public string ImageName { get; set; }
        public string PID { get; set; }
        public string Status { get; set; }
        public string Session { get; set; }
        public string MemUsage { get; set; }
        public string SessionName { get; set; }

        public WindowsTask() { }
    }
}
