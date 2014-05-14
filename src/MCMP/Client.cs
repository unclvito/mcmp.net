using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using log4net;
using global::MCMP.Extensions;

namespace MCMP
{
	public class Client : Interfaces.IClient
	{
		#region ctors
		public Client()
			: this(new Configuration.ClientConfiguration())
		{
		}

		public Client(Configuration.ClientConfiguration clientConfig)
		{
			this.clientConfig = clientConfig;
			Initialize();
		}

		~Client()
		{
			Dispose(false);
		}
		#endregion

		#region Private Data Members
		private bool initialized { get; set; }
		private Configuration.ClientConfiguration clientConfig { get; set; }

		private static readonly ILog log = LogManager.GetLogger(typeof(Client));
		#endregion

		#region Public Properties
		/// <summary>
		/// The clusters currently registered with
		/// </summary>
		public List<Cluster> Clusters { get; private set; }

		/// <summary>
		/// Client supplied Func returning an int that represents the current load of the server 
		/// from 100 to 1. 0=Failed Node, 1=HIGH Load, 100=LOW Load, -1=Do NOT Send this node requests.
		/// Defaults to null.
		/// For more info, see mod cluster documentation at <see cref="http://docs.jboss.org/mod_cluster/1.1.0/html/java.load.html"/>.
		/// </summary>
		/// <example>
		/// Here's an example of how to return current CPU load as the Load to the cluster.
		/// <code>
		/// client.CalculateStatusLoad = new Func<MCMP.ClientState, int>((state) => { return 100 - GetCurrentCPUPercentage(); });
		/// </code>
		/// </example>
		public Interfaces.IClientLoad ClientLoadModule
		{
			get { return clientConfig.ClientLoadModule; }
			set { clientConfig.ClientLoadModule = value; }
		}
		#endregion

		#region Singleton Factory
		private static Client _mcmp { get; set; }
		private static object _clientInstanceLockObject = new object();
		private static object _globalLockObject = new object();
		public static Client ClientInstance
		{
			private set { _mcmp = value; }
			get
			{
				lock (_clientInstanceLockObject)
				{
					if (_mcmp == null)
						_mcmp = new global::MCMP.Client(GetClientConfig());
					return _mcmp;
				}
			}
		}

		private static Configuration.ClientConfiguration GetClientConfig()
		{
			Configuration.ClientConfiguration cfg = new Configuration.ClientConfiguration();

			// Set a few defaults that depend on a live IIS context
			if (HttpContext.Current != null && System.Web.Hosting.HostingEnvironment.ApplicationHost != null)
			{
				string siteName = System.Web.Hosting.HostingEnvironment.ApplicationHost.GetSiteName().Replace(" ", "");
				string siteID = System.Web.Hosting.HostingEnvironment.ApplicationHost.GetSiteID();

				if (string.IsNullOrEmpty(cfg.JvmRoute))
					cfg.JvmRoute = siteName + (siteName + ":" + Environment.MachineName.ToLower()).GetHashCode();

				// Get host, etc from site binding
				// TODO: Test with IP hosting and name based with multiple hostnames!
				if (string.IsNullOrEmpty(cfg.Host))
				{
					var bindings = global::MCMP.Utilities.HostingEnvironment.GetBindings(HttpContext.Current);
					if (bindings != null)
						foreach (var binding in bindings)
							switch (binding.Key)
							{
								case "http":
								case "https":
									cfg.Type = binding.Value.Scheme;
									cfg.Host = binding.Value.Host;
									cfg.Port = binding.Value.Port;
									break;
							}
				}

				cfg.Host = (cfg.Host != "localhost" && cfg.Host != "*" && cfg.Host.IsNotNullOrEmpty() ? cfg.Host : Utilities.Networking.GetLocalIPAddress());
				cfg.Alias = (!string.IsNullOrEmpty(cfg.Alias) ? cfg.Alias : global::MCMP.Utilities.HostingEnvironment.GetServerAlias().Replace(" ", ""));
				if (cfg.Alias == "localhost" || cfg.Alias.IsNullOrEmpty()) cfg.Alias = Utilities.Networking.GetFQDN();
				cfg.AppRootPath = (!string.IsNullOrEmpty(cfg.AppRootPath) ? cfg.AppRootPath : global::MCMP.Utilities.HostingEnvironment.GetAppRootPath());
			}
			else
			{
				if (string.IsNullOrEmpty(cfg.JvmRoute))
					cfg.JvmRoute = (System.IO.Path.GetFileNameWithoutExtension(cfg.GetType().Assembly.Location).Replace(".", "_").TakeUpTo(10)
						+ (System.IO.Path.GetFileNameWithoutExtension(cfg.GetType().Assembly.Location) + ":" + Environment.MachineName.ToLower()).GetHashCode().ToString()).TakeUpTo(20).ToLower();
				cfg.Alias = (cfg.Alias.IsNotNullOrEmpty() ? cfg.Alias : cfg.Alias = Utilities.Networking.GetFQDN());

				if (log != null && log.IsDebugEnabled) log.Debug("GetClientConfig() has null HttpContext.Current or System.Web.Hosting.HostingEnvironment.ApplicationHost, skipping most autodetect stuff");
			}
			return cfg;
		}

		private void Initialize()
		{
			if (initialized) throw new InvalidOperationException("Already initialized!");
			if (log != null && log.IsDebugEnabled) log.Debug("Starting Initialize()");

			// Set stuff up
			this.Clusters = new List<Cluster>();
			this.ClientLoadModule = new ClientLoad();

			// Add any configured clusters in config file?
			if (clientConfig.Clusters != null && clientConfig.Clusters.Count > 0)
				foreach (var cluster in clientConfig.Clusters)
					lock (_globalLockObject)
						Clusters.Add(new Cluster(clientConfig) { ClusterUri = cluster });

			// Enable Multicast client
			if (clientConfig.MulticastEnabled)
				StartClusterMulticastClient();

			// Any contexts loaded from config? If so, start them.
			if (clientConfig.RegisteredContextsFromConfig.Count > 0)
				RegisterRoute(clientConfig.RegisteredContextsFromConfig);

			// Done init
			initialized = true;
			if (log != null && log.IsDebugEnabled) log.Debug("Done Initialize()");
		}

		#endregion

		#region Route Management
		private void RegisterRoute(IEnumerable<string> contexts)
		{
			if (log != null && log.IsDebugEnabled) log.Debug("Starting RegisterRoute(" + string.Join(",", contexts) + ")");
			// We will do all actions and raise multiple exceptions
			Dictionary<Cluster, Exception> exceptions = new Dictionary<Cluster, Exception>();

			//TODO: Query if it exists already before trying to CONFIG it
			// Enum all clusters
			foreach (var cluster in Clusters.ToList())
			{
				try
				{
					cluster.RegisterRoute(contexts);
				}
				catch (Exception ex)
				{
					exceptions.Add(cluster, ex);
				}
			}
			// Handle exceptions
			if (exceptions.Count == 0)
				return;
			else
				throw new Exceptions.MultiClusterException(exceptions);
		}

		private void DeregisterRoute()
		{
			// We will do all actions and raise multiple exceptions
			Dictionary<Cluster, Exception> exceptions = new Dictionary<Cluster, Exception>();

			// Enum all clusters
			foreach (var cluster in Clusters.ToList())
			{
				try
				{
					cluster.DeregisterRoute();
					lock (_globalLockObject)
					{
						cluster.Dispose();
						Clusters.Remove(cluster);
					}
				}
				catch (Exception ex)
				{
					exceptions.Add(cluster, ex);
				}
			}

			// Handle exceptions
			if (exceptions.Count == 0)
				return;
			else
				throw new Exceptions.MultiClusterException(exceptions);
		}
		#endregion

		#region Context Management
		public void RegisterAppRootContext()
		{
			RegisterContextPath("~");
		}

		/// <summary>
		/// Register context paths on all routes
		/// </summary>
		/// <param name="contexts"></param>
		public void RegisterContextPath(params string[] contexts)
		{
			RegisterContextPath(contexts);
		}

		/// <summary>
		/// Register context paths on all routes
		/// </summary>
		/// <param name="contexts"></param>
		public void RegisterContextPath(IEnumerable<string> contexts)
		{
			if (contexts.Count() == 0)
			{
				RegisterAppRootContext();
				return;
			}

			// Now make sure each context is registered
			foreach (var context in contexts)
				RegisterContextPath(context);
		}

		public void RegisterContextPath(string context)
		{
			if (string.IsNullOrEmpty(context))
				throw new InvalidOperationException("Cannot register an empty context");
			if (!clientConfig.RegisteredContexts.Contains(context))
				clientConfig.RegisteredContexts.Add(context);

			// We will do all actions and raise multiple exceptions
			Dictionary<Cluster, Exception> exceptions = new Dictionary<Cluster, Exception>();

			// Push to each route
			foreach (var cluster in Clusters.ToList())
			{
				try { cluster.RegisterContextPath(context); }
				catch (Exception ex) { exceptions.Add(cluster, ex); }
			}

			// Handle exceptions
			if (exceptions.Count == 0)
				return;
			else
				throw new Exceptions.MultiClusterException(exceptions);
		}

		public void DeregisterContext(string context)
		{
			if (clientConfig.RegisteredContexts.Contains(context))
				clientConfig.RegisteredContexts.Remove(context);

			// We will do all actions and raise multiple exceptions
			Dictionary<Cluster, Exception> exceptions = new Dictionary<Cluster, Exception>();

			// Deregister on cluster
			foreach (var cluster in Clusters.ToList())
			{
				try { cluster.DeregisterContext(context); }
				catch (Exception ex) { exceptions.Add(cluster, ex); }
			}

			// Handle exceptions
			if (exceptions.Count == 0)
				return;
			else
				throw new Exceptions.MultiClusterException(exceptions);
		}

		public void DeregisterContexts()
		{
			foreach (string context in clientConfig.RegisteredContexts.ToList())
				DeregisterContext(context);
		}
		#endregion

		#region MCMP Commands

		public List<string> PingAll()
		{
			List<string> pingResult = new List<string>();
			foreach (var cluster in Clusters.ToList())
				pingResult.Add(cluster.Ping());
			return pingResult;
		}

		public Dictionary<Uri, Dictionary<string, List<Dictionary<string, string>>>> DumpAll()
		{
			Dictionary<Uri, Dictionary<string, List<Dictionary<string, string>>>> dumpResult = new Dictionary<Uri, Dictionary<string, List<Dictionary<string, string>>>>();
			foreach (var cluster in Clusters.ToList())
				dumpResult.Add(cluster.ClusterUri, cluster.Dump());
			return dumpResult;
		}
		#endregion

		#region Cluster Multicast Client
		private UdpClient clusterMulticastClient = null;
		private IPEndPoint clusterMulticastClientEndPoint = null;
		private Timer clusterMulticastClientDelayedStart = null;

		private void StartClusterMulticastClient()
		{
			if (log != null && log.IsInfoEnabled) log.Info("StartClusterMulticastClient()");
			clusterMulticastClientDelayedStart = new Timer(StartClusterMulticastClientStart, new object(), (int)clientConfig.MulticastClientDelayedStart.TotalMilliseconds, Timeout.Infinite);
		}

		private void StartClusterMulticastClientStart(object state)
		{
			// Clean up
			if (clusterMulticastClientDelayedStart != null)
				clusterMulticastClientDelayedStart.Dispose();
			clusterMulticastClientDelayedStart = null;

			// Start listener
			try
			{
				if (log != null && log.IsDebugEnabled) log.Debug("StartClusterMulticastClientStart() starting UdpClient");
				clusterMulticastClient = new UdpClient();
				clusterMulticastClientEndPoint = new IPEndPoint(IPAddress.Any, clientConfig.MulticastAddress.Port);
				clusterMulticastClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
				clusterMulticastClient.Client.Bind(new IPEndPoint(IPAddress.Any, clientConfig.MulticastAddress.Port));
				clusterMulticastClient.JoinMulticastGroup(IPAddress.Parse(clientConfig.MulticastAddress.Host));// TODO: use ntds1r.mcast.net
				clusterMulticastClient.BeginReceive(new AsyncCallback(ClusterMulticastClientReceive), null);
			}
			catch (Exception ex)
			{
				if (log != null && log.IsErrorEnabled) log.Error("StartClusterMulticastClientStart() fail", ex);
			}
		}

		private void StopClusterMulticastClient()
		{
			if (clusterMulticastClient != null)
			{
				try { clusterMulticastClient.Close(); }
				catch { }
				clusterMulticastClient = null;
			}
		}

		private void ClusterMulticastClientReceive(IAsyncResult ar)
		{
			byte[] received = null;

			try
			{
				// Closing?
				if (clusterMulticastClient == null) return;

				// Receive message
				try
				{
					received = clusterMulticastClient.EndReceive(ar, ref clusterMulticastClientEndPoint);
					//	if (log != null && log.IsDebugEnabled) log.Info("Multicast received from " + clusterMulticastClientEndPoint.Address.ToString() + ": " + received.GetString());
				}
				catch (NullReferenceException) { }
				catch (ObjectDisposedException) { return; }
				catch (Exception ex) { if (log != null && log.IsErrorEnabled) log.Error("ClusterMulticastClientReceive() fail", ex); }

				// This may fall thru here if disposing or quitting
				if (disposed) return;

				// Empty? Quit.
				if (received == null) return;

				try
				{
					// Ignoring message?
					if (!clientConfig.MulticastEnabled)
						return;

					// Process received message
					// Message looks like the following:
					//  HTTP/1.0 200 OK
					//  Date: Wed, 30 Apr 2014 15:23:38 GMT
					//  Sequence: 77
					//  Digest: 29da46a8d26397116bcb0a8d06e73a89
					//  Server: c70d84cb-5f51-8544-885d-b2a1451b9aba
					//  X-Manager-Address: 127.0.0.1:6666
					//  X-Manager-Url: /c70d84cb-5f51-8544-885d-b2a1451b9aba
					//  X-Manager-Protocol: http
					//  X-Manager-Host: myservername
					string httpCode = received.GetString().Split('\n')[0];
					if (!(httpCode.StartsWith("HTTP/1.0 200 OK") || httpCode.StartsWith("HTTP/1.1 200 OK")))
					{
						Dictionary<string, string> goneDataDict = received.ToDictionary();
						Cluster goodByeCluster = Clusters.Where(c => c.MulticastServerId == goneDataDict["Server"]).FirstOrDefault();
						if (goodByeCluster != null)
						{
							if (log.IsDebugEnabled)
								log.Debug("ClusterMulticastClient caught server " + goodByeCluster.ClusterUri + " (" + goneDataDict["Server"] + ") is going away");
							goodByeCluster.RouteRegistered = false;
						}
						else
						{
							if (log.IsDebugEnabled)
								log.Debug("ClusterMulticastClient caught server " + goneDataDict["Server"] + " is going away");
						}
						return;
					}
					Dictionary<string, string> dataDict = received.ToDictionary();
					Uri clusterUri = null;
					try { clusterUri = dataDict.GetUriFromBroadcast(); }
					catch (Exception ex)
					{
						if (log.IsDebugEnabled)
							log.Debug("ClusterMulticastClient got data that created an invalid uri. Cannot autoregister.", ex);
						return;
					}

					// Scan to see if we need to add it
					if (Clusters.Count(c => c.ClusterUri.ToString() == clusterUri.ToString()) > 0)
						return;
					if (log != null && log.IsInfoEnabled) log.Info("Multicast received, registering to " + clusterUri);

					lock (_globalLockObject)
					{
						// Do not start on cluster if no contexts are registered
						if (clientConfig.RegisteredContexts.Count() + clientConfig.RegisteredContextsFromConfig.Count() == 0)
						{
							if (log.IsDebugEnabled)
								log.Debug("ClusterMulticastClient did not auto-register cluster " + clusterUri
									+ " because there are no contexts to register, will try again on next multicast receive");
							return;
						}

						// Initialize New Cluster
						try
						{
							Cluster cluster = new Cluster(clientConfig) { ClusterUri = clusterUri, MulticastServerId = dataDict["Server"] };
							cluster.RegisterRoute(clientConfig.RegisteredContexts.Union(clientConfig.RegisteredContextsFromConfig).Distinct());
							Clusters.Add(cluster);
						}
						catch (Exception ex)
						{
							log.Error("ClusterMulticastClient could not auto-register cluster " + clusterUri + ", will try again on next multicast receive", ex);
							return;
						}
					}
				}
				finally
				{
					// Listen for the next message
					clusterMulticastClient.BeginReceive(new AsyncCallback(ClusterMulticastClientReceive), null);
				}
			}
			catch (Exception ex)
			{
				if (log != null && log.IsErrorEnabled) log.Error("ClusterMulticastClientReceive() unhandled exception", ex);
			}
		}
		#endregion

		#region IDisposable
		private bool disposed = false; // to detect redundant calls

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					// Dispose managed resources
					StopClusterMulticastClient();

					// Close all clusters
					foreach (var cluster in Clusters)
						cluster.Dispose();
				}

				// Unmanaged resources to release
			}
			disposed = true;
		}

		#endregion
	}
}
