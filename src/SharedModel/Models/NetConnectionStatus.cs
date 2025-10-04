using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace LaparoscopeShared.Models {
    public class NetConnectionStatus {
        public NetConnectionStatus() { }
        public string ComputerName { get; set; }
        public string RemoteAddress { get; set; }
        public bool PingSucceeded { get; set; }
        public object TcpClientSocket { get; set; }
        public bool TcpTestSucceeded { get; set; }
        public UInt32 RemotePort { get; set; }
        public bool Detailed { get; set; }
        public string InterfaceAlias { get; set; }
        public UInt32 InterfaceIndex { get; set; }
        public string InterfaceDescription { get; set; }
        public string SourceAddress { get; set; }
        public bool NameResolutionSucceeded { get; set; }
        public string NetworkIsolationContext { get; set; }
    }
}
