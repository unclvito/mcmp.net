using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCMP.Configuration
{
	public class ClientConfiguration
	{
		public ClientConfiguration()
		{
			// Set defaults
			Clusters = new List<Uri>();
			StatusInitialDueTime = Constants.Configuration.DefaultStatusInitialDueTime;
			StatusPeriod = Constants.Configuration.DefaultStatusPeriod;
			MulticastEnabled = Constants.Configuration.DefaultMulticastEnabled;
			MulticastClientDelayedStart = Constants.Configuration.DefaultMulticastClientDelayedStart;
			MulticastAddress = null;
			Balancer = Constants.Configuration.DefaultBalancer;
			StickySession = Constants.Configuration.DefaultStickySession;
			StickySessionCookie = Constants.Configuration.DefaultStickySessionCookie;
			StickySessionPath = Constants.Configuration.DefaultStickySessionPath;
			StickySessionRemove = Constants.Configuration.DefaultStickySessionRemove;
			StickySessionForce = Constants.Configuration.DefaultStickySessionForce;
			WaitWorker = Constants.Configuration.DefaultWaitWorker;
			MaxAttempts = Constants.Configuration.DefaultMaxAttempts;
			JvmRoute = Environment.MachineName.Replace("-", "").ToLower();
			Domain = Constants.Configuration.DefaultDomain;
			Host = Constants.Configuration.DefaultHost;
			Port = Constants.Configuration.DefaultPort;
			Type = Constants.Configuration.DefaultType;
			FlushPackets = Constants.Configuration.DefaultFlushPackets;
			FlushWait = Constants.Configuration.DefaultFlushWait;
			Ping = Constants.Configuration.DefaultPing;
			Smax = Constants.Configuration.DefaultSmax;
			Ttl = Constants.Configuration.DefaultTtl;
			Timeout = Constants.Configuration.DefaultTimeout;
			Alias = Constants.Configuration.DefaultAlias;
			RegisteredContexts = new List<string>();
			RegisteredContextsFromConfig = new List<string>();
			UnregisterOnDispose = Constants.Configuration.DefaultUnregisterOnDispose;

			// Update config from AppConfig
			MCMP.Configuration.AppConfiguration.GetConfiguration(this);
		}

		/// <summary>
		/// Name of the balancer. max size: 40 Default: "mycluster"
		/// </summary>
		public string Balancer { get; set; }

		/// <summary>
		/// Use true: JVMRoute to stick a request to a node, false: ignore JVMRoute. Default: true
		/// </summary>
		public bool StickySession { get; set; }

		/// <summary>
		/// Name of the cookie containing the sessionid. Max size: 30 Default: "ASP.NET_SessionId"
		/// </summary>
		public string StickySessionCookie { get; set; }

		/// <summary>
		/// Name of the parametre containing the sessionid. Max size: 30. Default: ""
		/// </summary>
		public string StickySessionPath { get; set; }

		/// <summary>
		/// True: remove the sessionid (cookie or parameter) when the request can't be routed to the right node. False: send it anyway. Default: "False"
		/// </summary>
		public bool StickySessionRemove { get; set; }

		/// <summary>
		/// True: Return an error if the request can't be routed according to JVMRoute, False: Route it to another node. Default: "True"
		/// </summary>
		public bool StickySessionForce { get; set; }

		/// <summary>
		/// Value in whole seconds: time to wait for an available worker. Default: "0" no wait.
		/// </summary>
		public int WaitWorker { get; set; }

		/// <summary>
		/// Value: number of attemps to send the request to the backend server. Default: "1".
		/// </summary>
		public int MaxAttempts { get; set; }

		/// <summary>
		/// See http://wiki.jboss.org/wiki/Mod-ClusterManagementProtocol Default: Mandatory
		/// </summary>
		public string JvmRoute { get; set; }

		/// <summary>
		/// See http://wiki.jboss.org/wiki/Mod-ClusterManagementProtocol Default: "" empty string
		/// </summary>
		public string Domain { get; set; }

		/// <summary>
		/// See http://wiki.jboss.org/wiki/Mod-ClusterManagementProtocol Default: "localhost"
		/// </summary>
		public string Host { get; set; }

		/// <summary>
		/// See http://wiki.jboss.org/wiki/Mod-ClusterManagementProtocol Default: "8009"
		/// </summary>
		public int Port { get; set; }

		/// <summary>
		/// See http://wiki.jboss.org/wiki/Mod-ClusterManagementProtocol Default: "http"
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// See http://wiki.jboss.org/wiki/Mod-ClusterManagementProtocol Default: "http://localhost:8009"
		/// </summary>
		public List<Uri> Clusters { get; set; }

		/// <summary>
		/// Tell how to flush the packets. True: Send immediately, Auto wait for flushwait time before sending, False don't flush. Default: "False"
		/// </summary>
		public bool FlushPackets { get; set; }

		/// <summary>
		/// Time to wait before flushing. Value in milliseconds. Default: 10
		/// </summary>
		public TimeSpan FlushWait { get; set; }

		/// <summary>
		/// Time to wait for a pong answer to a ping. 0 means we don't try to ping before sending. Value in secondes Default: 10
		/// </summary>
		public TimeSpan Ping { get; set; }

		/// <summary>
		/// Soft max inactive connection over that limit after ttl are closed. Default depends on the mpm configuration (See below for more information)
		/// </summary>
		public int? Smax { get; set; }

		/// <summary>
		/// Max time in seconds to life for connection above smax. Default 60 seconds.
		/// </summary>
		public TimeSpan Ttl { get; set; }

		/// <summary>
		/// Max time httpd will wait for the backend connection. Default 0 no timeout value in seconds.
		/// </summary>
		public TimeSpan Timeout { get; set; }

		/// <summary>
		///  List of currently registered contexts
		/// </summary>
		public List<string> RegisteredContexts { get; set; }

		/// <summary>
		///  List of registered contexts from config file
		/// </summary>
		internal List<string> RegisteredContextsFromConfig { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string Alias { get; set; }

		/// <summary>
		/// Time to wait before sending first STATUS command to balancer.
		/// </summary>
		public TimeSpan StatusInitialDueTime { get; set; }

		/// <summary>
		/// Time to wait before sending each subsequent STATUS command to balancer.
		/// </summary>
		public TimeSpan StatusPeriod { get; set; }

		/// <summary>
		/// Time to wait before starting multicast client.
		/// </summary>
		public TimeSpan MulticastClientDelayedStart { get; set; }

		/// <summary>
		/// Address for multicast from mod_cluster
		/// </summary>
		public Uri MulticastAddress { get; set; }

		/// <summary>
		/// Path of this AppRoot
		/// </summary>
		public string AppRootPath { get; set; }

		/// <summary>
		/// Listen for multicast registrations
		/// </summary>
		public bool MulticastEnabled { get; set; }

		/// <summary>
		/// Custom Client Load Calculation
		/// </summary>
		public Interfaces.IClientLoad ClientLoadModule { get; set; }

		/// <summary>
		/// If set true, shutdown will unregister everything
		/// </summary>
		public bool UnregisterOnDispose { get; set; }

		internal string GetPostData(string[] keys, Dictionary<string, string> seedParams = null)
		{
			return string.Join("&", GeneratePostData(keys, seedParams: seedParams).ToList().Select(s => s.Key + "=" + System.Uri.EscapeDataString(s.Value ?? "")));
		}

		internal Dictionary<string, string> GeneratePostData(string[] keys, Dictionary<string, string> seedParams = null)
		{
			Dictionary<string, string> postData = new Dictionary<string, string>();
			if (seedParams != null)
				seedParams.Keys.ToList().ForEach(key => postData.Add(key, seedParams[key]));

			foreach (string key in keys)
			{
				if (postData.ContainsKey(key)) continue;
				switch (key.ToLower())
				{
					case "jvmroute":
						postData.Add(key, JvmRoute);
						break;
					case "balancer":
						if (!string.IsNullOrEmpty(Balancer))
							postData.Add(key, Balancer);
						break;
					case "domain":
						if (!string.IsNullOrEmpty(Domain))
							postData.Add(key, Domain);
						break;
					case "host":
						if (!string.IsNullOrEmpty(Host))
							postData.Add(key, Host);
						break;
					case "port":
						postData.Add(key, Port.ToString());
						break;
					case "type":
						if (!string.IsNullOrEmpty(Type))
							postData.Add(key, Type);
						break;
					case "alias":
						if (!string.IsNullOrEmpty(Alias))
							postData.Add(key, Alias);
						break;
					case "stickysession":
						postData.Add(key, (StickySession ? "yes" : "no"));
						break;
					case "stickysessionforce":
						postData.Add(key, (StickySessionForce ? "yes" : "no"));
						break;
					case "stickysessionremove":
						postData.Add(key, (StickySessionRemove ? "yes" : "no"));
						break;
					case "flushpackets":
						postData.Add(key, (FlushPackets ? "on" : "off"));
						break;
					case "waitworker":
						postData.Add(key, WaitWorker.ToString());
						break;
					case "flushwait":
						postData.Add(key, Math.Floor(FlushWait.TotalSeconds).ToString());
						break;
					case "maxattempts":
						postData.Add(key, MaxAttempts.ToString());
						break;
					case "stickysessioncookie":
						if (!string.IsNullOrEmpty(StickySessionCookie))
							postData.Add(key, StickySessionCookie);
						break;
					case "stickysessionpath":
						if (!string.IsNullOrEmpty(StickySessionPath))
							postData.Add(key, StickySessionPath);
						break;
					case "ping":
						postData.Add("ping", Math.Floor(Ping.TotalSeconds).ToString());
						break;
					case "smax":
						if (Smax.HasValue) postData.Add(key, Smax.ToString());
						break;
					case "ttl":
						postData.Add(key, Math.Floor(Ttl.TotalSeconds).ToString());
						break;
					case "timeout":
						postData.Add(key, Math.Floor(Timeout.TotalSeconds).ToString());
						break;
					default:
						throw new InvalidProgramException("Cannot map value " + key);
				}
			}
			return postData;
		}
	}
}
