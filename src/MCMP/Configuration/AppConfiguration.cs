using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using MCMP.Extensions;

namespace MCMP.Configuration
{
	public class AppConfiguration : System.Configuration.ConfigurationSection
	{
		#region Node
		[ConfigurationProperty("node", IsRequired = false)]
		public NodeElement Node
		{
			get
			{
				return (NodeElement)this["node"];
			}
			set
			{
				this["node"] = value;
			}
		}

		public class NodeElement : ConfigurationElement
		{
			[ConfigurationProperty("host", IsRequired = false)]
			[StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\", MaxLength = 30)]
			public string Host
			{
				get
				{
					return (string)this["host"];
				}
				set
				{
					this["host"] = value;
				}
			}

			[ConfigurationProperty("port", IsRequired = false)]
			public int Port
			{
				get
				{
					return (int)this["port"];
				}
				set
				{
					this["Port"] = value;
				}
			}

			[ConfigurationProperty("type", DefaultValue = "http", IsRequired = true)]
			[StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\", MaxLength = 5)]
			public String Type
			{
				get
				{
					return (String)this["type"];
				}
				set
				{
					this["type"] = value;
				}
			}
		}
		#endregion

		#region Cluster
		[ConfigurationProperty("cluster", IsRequired = false)]
		public ClusterElement Cluster
		{
			get
			{
				return (ClusterElement)this["cluster"];
			}
			set
			{
				this["cluster"] = value;
			}
		}

		public class ClusterElement : ConfigurationElement
		{
			[ConfigurationProperty("hosts", IsRequired = false)]
			//TODO: [RegexStringValidator(InvalidCharacters = "~!@#$%^&*()[]{};'\"|\\")]
			public string Hosts
			{
				get
				{
					return (string)this["hosts"];
				}
				set
				{
					this["hosts"] = value;
				}
			}

			public string[] HostList
			{
				get
				{
					return Hosts.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				}
				set
				{
					Hosts = string.Join(",", value);
				}
			}

			[ConfigurationProperty("contexts", IsRequired = false)]
			//TODO: [RegexStringValidator(InvalidCharacters = "~!@#$%^&*()[]{};'\"|\\")]
			public string Contexts
			{
				get
				{
					return (string)this["contexts"];
				}
				set
				{
					this["contexts"] = value;
				}
			}

			public string[] ContextList
			{
				get
				{
					return Contexts.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				}
				set
				{
					Contexts = string.Join(",", value);
				}
			}

			[ConfigurationProperty("unregisterOnShutdown", IsRequired = false)]
			public bool UnregisterOnShutdown
			{
				get
				{
					return (bool)this["unregisterOnShutdown"];
				}
				set
				{
					this["unregisterOnShutdown"] = value;
				}
			}

			[ConfigurationProperty("multicastEnabled", IsRequired = false)]
			public bool MulticastEnabled
			{
				get
				{
					return (bool)this["multicastEnabled"];
				}
				set
				{
					this["multicastEnabled"] = value;
				}
			}

			[ConfigurationProperty("multicastClientDelayedStartSeconds", DefaultValue = 2, IsRequired = false)]
			public int MulticastClientDelayedStartSeconds
			{
				get
				{
					return (int)this["multicastClientDelayedStartSeconds"];
				}
				set
				{
					this["multicastClientDelayedStartSeconds"] = value;
				}
			}

			[ConfigurationProperty("multicastAddress", DefaultValue = "udp://224.0.1.105:23364", IsRequired = false)]
			public Uri MulticastAddress
			{
				get
				{
					return (Uri)this["multicastAddress"];
				}
				set
				{
					this["multicastAddress"] = value;
				}
			}
		}
		#endregion

		#region Application
		[ConfigurationProperty("application", IsRequired = false)]
		public ApplicationElement Application
		{
			get
			{
				return (ApplicationElement)this["application"];
			}
			set
			{
				this["application"] = value;
			}
		}

		public class ApplicationElement : ConfigurationElement
		{
			[ConfigurationProperty("alias", DefaultValue = null, IsRequired = false)]
			[StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\", MaxLength = 30)]
			public string Alias
			{
				get
				{
					return (string)this["alias"];
				}
				set
				{
					this["alias"] = value;
				}
			}

			[ConfigurationProperty("balancer", DefaultValue = "mybalancer", IsRequired = false)]
			[StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\", MinLength = 1, MaxLength = 30)]
			public String Balancer
			{
				get
				{
					return (String)this["balancer"];
				}
				set
				{
					this["balancer"] = value;
				}
			}

			[ConfigurationProperty("domain", IsRequired = false)]
			[StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\", MinLength = 1, MaxLength = 30)]
			public String Domain
			{
				get
				{
					return (String)this["domain"];
				}
				set
				{
					this["domain"] = value;
				}
			}

			[ConfigurationProperty("jvmroute", IsRequired = false)]
			[StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\", MaxLength = 30)]
			public String JvmRoute
			{
				get
				{
					return (String)this["jvmroute"];
				}
				set
				{
					this["jvmroute"] = value;
				}
			}

			[ConfigurationProperty("appRootPath", IsRequired = false)]
			[StringValidator(InvalidCharacters = "~!@#$%^&*()[]{};'\"|\\", MaxLength = 80)]
			public String AppRootPath
			{
				get
				{
					return (String)this["appRootPath"];
				}
				set
				{
					this["appRootPath"] = value;
				}
			}

			[ConfigurationProperty("paths", IsRequired = false)]
			[StringValidator(InvalidCharacters = "~!@#$%^&*()[]{};'\"|\\")]
			public String Paths
			{
				get
				{
					return (String)this["paths"];
				}
				set
				{
					this["paths"] = value;
				}
			}
			public string[] PathList
			{
				get
				{
					return Paths.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				}
				set
				{
					Paths = string.Join(",", value);
				}
			}
		}
		#endregion

		#region StickySessions
		[ConfigurationProperty("stickySessions", IsRequired = false)]
		public StickySessionElement StickySessions
		{
			get
			{
				return (StickySessionElement)this["stickySessions"];
			}
			set
			{
				this["stickySessions"] = value;
			}
		}

		public class StickySessionElement : ConfigurationElement
		{
			[ConfigurationProperty("enabled", IsRequired = false)]
			public Boolean Enabled
			{
				get
				{
					return (Boolean)this["enabled"];
				}
				set
				{
					this["enabled"] = value;
				}
			}

			[ConfigurationProperty("cookieName", IsRequired = false)]
			[StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\", MinLength = 0, MaxLength = 30)]
			public String CookieName
			{
				get
				{
					return (String)this["cookieName"];
				}
				set
				{
					this["cookieName"] = value;
				}
			}

			[ConfigurationProperty("cookiePath", DefaultValue = "/", IsRequired = false)]
			[StringValidator(InvalidCharacters = "~!@#$%^&*()[]{};'\"|\\", MinLength = 1, MaxLength = 30)]
			public String CookiePath
			{
				get
				{
					return (String)this["cookiePath"];
				}
				set
				{
					this["cookiePath"] = value;
				}
			}
		}
		#endregion

		#region StatusMessages
		[ConfigurationProperty("statusMessages", IsRequired = false)]
		public StatusMessagesElement StatusMessages
		{
			get
			{
				return (StatusMessagesElement)this["statusMessages"];
			}
			set
			{
				this["statusMessages"] = value;
			}
		}

		public class StatusMessagesElement : ConfigurationElement
		{
			[ConfigurationProperty("statusInitialDueTimeSeconds", DefaultValue = 2, IsRequired = false)]
			public int StatusInitialDueTimeSeconds
			{
				get
				{
					return (int)this["statusInitialDueTimeSeconds"];
				}
				set
				{
					this["statusInitialDueTimeSeconds"] = value;
				}
			}

			[ConfigurationProperty("statusPeriodSeconds", DefaultValue = 60, IsRequired = false)]
			public int StatusPeriodSeconds
			{
				get
				{
					return (int)this["statusPeriodSeconds"];
				}
				set
				{
					this["statusPeriodSeconds"] = value;
				}
			}
		}
		#endregion

		internal static AppConfiguration GetConfiguration()
		{
			MCMP.Configuration.AppConfiguration mcmpConfig = null;
			try { mcmpConfig = (MCMP.Configuration.AppConfiguration)System.Configuration.ConfigurationManager.GetSection("clusterConfig"); }
			catch (Exception ex) 
			{
				//if (log != null && log.IsInfoEnabled) log.Info("Multicast received, registering to " + dataDict["X-Manager-Address"]);
				throw;
			}
			return mcmpConfig;
		}

		internal static void GetConfiguration(ClientConfiguration clientConfiguration)
		{
			MCMP.Configuration.AppConfiguration mcmpConfig = GetConfiguration();
			if (mcmpConfig == null) return;
			if (mcmpConfig.Node != null)
			{
				clientConfiguration.Host = mcmpConfig.Node.Host;
				clientConfiguration.Port = mcmpConfig.Node.Port;
				clientConfiguration.Type = mcmpConfig.Node.Type;
			}
			if (mcmpConfig.Cluster != null)
			{
				clientConfiguration.MulticastEnabled = mcmpConfig.Cluster.MulticastEnabled;
				clientConfiguration.UnregisterOnDispose = mcmpConfig.Cluster.UnregisterOnShutdown;
				if (clientConfiguration.MulticastEnabled)
				{
					clientConfiguration.MulticastClientDelayedStart = TimeSpan.FromSeconds(mcmpConfig.Cluster.MulticastClientDelayedStartSeconds);
					clientConfiguration.MulticastAddress = mcmpConfig.Cluster.MulticastAddress;
				}

				if (mcmpConfig.Cluster != null && mcmpConfig.Cluster.Hosts != null && mcmpConfig.Cluster.HostList.Length > 0)
				{
					clientConfiguration.Clusters.Clear();
					foreach (Uri host in mcmpConfig.Cluster.HostList.Select(h => new Uri(h)))
						clientConfiguration.Clusters.Add(host);
				}

				if (mcmpConfig.Cluster != null && mcmpConfig.Cluster.ContextList.Length > 0)
					clientConfiguration.RegisteredContextsFromConfig.AddRange(mcmpConfig.Cluster.ContextList);
			}

			if (mcmpConfig.Application != null)
			{
				if (mcmpConfig.Application.JvmRoute != null)
					clientConfiguration.JvmRoute = mcmpConfig.Application.JvmRoute;
				if (mcmpConfig.Application.Balancer != null)
					clientConfiguration.Balancer = mcmpConfig.Application.Balancer;
				if (mcmpConfig.Application.Domain != null)
					clientConfiguration.Domain = mcmpConfig.Application.Domain;
				if (mcmpConfig.Application.Alias != null)
					clientConfiguration.Alias = mcmpConfig.Application.Alias;
				if (mcmpConfig.Application.AppRootPath != null)
					clientConfiguration.AppRootPath = mcmpConfig.Application.AppRootPath;
				if (mcmpConfig.Application.PathList.Length > 0)
					clientConfiguration.RegisteredContextsFromConfig.AddRange(mcmpConfig.Application.PathList);
			}
			if (mcmpConfig.StickySessions != null)
			{
				clientConfiguration.StickySession = mcmpConfig.StickySessions.Enabled;
				if (mcmpConfig.StickySessions.CookieName.IsNotNullOrEmpty())
					clientConfiguration.StickySessionCookie = mcmpConfig.StickySessions.CookieName;
				if (mcmpConfig.StickySessions.CookiePath.IsNotNullOrEmpty())
					clientConfiguration.StickySessionPath = mcmpConfig.StickySessions.CookiePath;
				//clientConfiguration.StickySessionRemove = mcmpConfig.StickySessions.Remove;
				//clientConfiguration.StickySessionForce = mcmpConfig.StickySessions.Force;
			}
			if (mcmpConfig.StatusMessages != null)
			{
				clientConfiguration.StatusInitialDueTime = TimeSpan.FromSeconds(mcmpConfig.StatusMessages.StatusInitialDueTimeSeconds);
				clientConfiguration.StatusPeriod = TimeSpan.FromSeconds(mcmpConfig.StatusMessages.StatusPeriodSeconds);
			}
		}
	}
}
