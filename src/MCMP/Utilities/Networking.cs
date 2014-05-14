using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace MCMP.Utilities
{
	internal class Networking
	{
		public static string GetLocalIPAddress()
		{
			IPHostEntry host;
			string localIP = "";
			host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (IPAddress ip in host.AddressList)
			{
				if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
				{
					localIP = ip.ToString();
					break;
				}
			}
			return localIP;
		}

		public static string GetFQDN()
		{
			var ipProperties = IPGlobalProperties.GetIPGlobalProperties();
			return string.IsNullOrWhiteSpace(ipProperties.DomainName) ? ipProperties.HostName : string.Format("{0}.{1}", ipProperties.HostName, ipProperties.DomainName);
		}
	}
}
