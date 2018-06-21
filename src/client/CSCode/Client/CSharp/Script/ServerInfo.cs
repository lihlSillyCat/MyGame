using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XLua;

namespace War.Script
{
    [LuaCallCSharp]
    public class ServerInfo
    {
        [LuaCallCSharp]
        public struct IpPort
        {
            public string ip;
            public int port;
        }

        private static ServerInfo _instance = null;
        public static ServerInfo Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ServerInfo();
                }
                return _instance;
            }
        }

        private List<IpPort> tcpServerInfo = new List<IpPort>();
        private List<IpPort> udpServerInfo = new List<IpPort>();

        private ServerInfo()
        {

        }

        public void AddTcpIpPort(string ipPort)
        {
            IpPort stIpPort = new IpPort();
            if (ParseIpPort(ipPort, out stIpPort.ip, out stIpPort.port))
            {
                tcpServerInfo.Add(stIpPort);
            }
        }

        public void AddUdpIpPort(string ipPort)
        {
            IpPort stIpPort = new IpPort();
            if (ParseIpPort(ipPort, out stIpPort.ip, out stIpPort.port))
            {
                udpServerInfo.Add(stIpPort);
            }
        }

        public void Clear()
        {
            udpServerInfo.Clear();
            tcpServerInfo.Clear();
        }

        public bool IsValid()
        {
            return udpServerInfo.Count > 0 || tcpServerInfo.Count > 0;
        }

        private bool ParseIpPort(string ipPort, out string ip, out int port)
        {
            ip = null;
            port = 0;
            if (ipPort == null || ipPort == string.Empty)
                return false;

            string[] splitedString = ipPort.Split(':');
            if (splitedString.Length != 2)
            {
                Debug.LogFormat("Invalid server address: {0}", ipPort);
                return false;
            }

            ip = splitedString[0];
            port = System.Convert.ToInt32(splitedString[1]);

            if (ip == string.Empty)
            {
                Debug.LogFormat("Invalid server address: {0}", ipPort);
                return false;
            }

            return true;
        }

        public IpPort[] GetTcpServerInfo()
        {
            return tcpServerInfo.ToArray();
        }

        public IpPort[] GetUdpServerInfo()
        {
            return udpServerInfo.ToArray();
        }
    }
}
