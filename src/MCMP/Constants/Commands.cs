using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MCMP.Constants
{
	public enum Commands
	{
		[MCMP.Attributes.Command(Method = "STATUS", Path = "/", RequiredKeys = new string[] { "JvmRoute", "Load" })]
		Status,

		[MCMP.Attributes.Command(Method = "PING", Path = "/", RequiredKeys = new string[] { "JvmRoute" })]
		Ping,

		[MCMP.Attributes.Command(Method = "DUMP", Path = "/", RequiredKeys = new string[] { })]
		Dump,

		[MCMP.Attributes.Command(Method = "ENABLE-APP", Path = "/", RequiredKeys = new string[] { "JvmRoute", "Alias", "Context" })]
		EnableApp,

		[MCMP.Attributes.Command(Method = "STOP-APP", Path = "/", RequiredKeys = new string[] { "JvmRoute", "Alias", "Context" })]
		StopApp,

		[MCMP.Attributes.Command(Method = "STOP-APP", Path = "/*", RequiredKeys = new string[] { "JvmRoute" })]
		StopAllApps,

		[MCMP.Attributes.Command(Method = "DISABLE-APP", Path = "/", RequiredKeys = new string[] { "JvmRoute", "Alias", "Context" })]
		DisableApp,

		[MCMP.Attributes.Command(Method = "REMOVE-APP", Path = "/", RequiredKeys = new string[] { "JvmRoute", "Alias", "Context" })]
		RemoveApp,

		[MCMP.Attributes.Command(Method = "REMOVE-APP", Path = "/*", RequiredKeys = new string[] { "JvmRoute" })]
		RemoveAllApps,

		[MCMP.Attributes.Command(Method = "CONFIG", Path = "/",
			RequiredKeys = new string[] { "JvmRoute", "Context", "Balancer", "Ping", "Domain", "Host", "Port", "Alias", 
					"StickySessionCookie", "StickySessionPath", "Type", "StickySession", "StickySessionRemove", 
					"StickySessionPath", "WaitWorker", "StickySessionCookie", "StickySessionForce", 
					"MaxAttempts", "FlushWait", "FlushPackets", "Smax", "Ttl", "Timeout" })]
		Config,
	}
}
