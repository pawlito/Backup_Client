using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NASClientTCP
{
    [Serializable]
    class ClientInfo
    {
        private string _hostName = string.Empty;
        private string _IPAddress = string.Empty;
        ClientInfo()
        {
            HostName = GetHostName();
            IpAddress = GetIpAddress();
        }

        public string HostName

        {

            get { return _hostName; }
            set { _hostName = value; }

        }

        public string IpAddress

        {

            get { return _IPAddress; }
            set { _IPAddress = value; }

        }

        public string GetHostName()
        {
            return Environment.MachineName;
        }
        public string GetIpAddress()
        {
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress addr in localIPs)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    return addr.AddressFamily.ToString();
                }
            }
            return null;
        }
    }
}
