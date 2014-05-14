using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using log4net;
using global::MCMP.Extensions;
using global::MCMP.Responses;

namespace MCMP
{
	public class Cluster : Interfaces.ICluster
	{
		#region ctors
		public Cluster(Configuration.ClientConfiguration clientConfig)
		{
			// Initialize Things
			RegisteredContexts = new List<string>();
			contextsToRegister = new System.Collections.Concurrent.ConcurrentQueue<string>();
			ClientConfig = clientConfig;

			// Initialize Timers
			registrationTimer = new Timer(RegistrationTimerCallback, this, Timeout.Infinite, Timeout.Infinite);
			statusCheckTimer = new Timer(StatusCheckTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
		}

		~Cluster()
		{
			Dispose(false);
		}
		#endregion

		#region Public/Internal Properties
		public Uri ClusterUri { get; internal set; }

		public bool RouteRegistered { get; internal set; }

		public List<string> RegisteredContexts { get; internal set; }

		public bool StickySessionsEnabled { get { return ClientConfig.StickySession; } }

		public string StickySessionCookieName { get { return ClientConfig.StickySessionCookie; } }

		internal string MulticastServerId { get; set; }
		#endregion

		#region Private Properties/Fields
		private readonly ILog log = LogManager.GetLogger(typeof(Cluster));
		private Configuration.ClientConfiguration ClientConfig = null;
		private bool registerRouteAnywayIfNodeExists = true;
		private Timer registrationTimer = null;
		private System.Collections.Concurrent.ConcurrentQueue<string> contextsToRegister = null;
		#endregion

		public System.Web.HttpCookie GetStickySessionCookie()
		{
			return new System.Web.HttpCookie(StickySessionCookieName, "\"" + System.Web.HttpContext.Current.Session.SessionID + "." + ClientConfig.JvmRoute + "\"") { HttpOnly = true };
		}

		internal void RegisterRoute(IEnumerable<string> contexts)
		{
			if (RouteRegistered)
				return;

			// Stop timers so they dont screw us up
			StopRegistrationTimer();

			// Register our route
			try
			{
				// Query if this route/node exists already only if we're going to not register if we find it
				if (!registerRouteAnywayIfNodeExists)
				{
					try
					{
						var dump = Dump();
						if (dump.ContainsKey("node"))
						{
							var node = dump["node"];
							foreach (var item in node)
							{
								if (item["JVMRoute"] == ClientConfig.JvmRoute)
								{
									log.Warn("Found our node already registered: " + string.Join(",", item.Select(i => i.Key + "=" + i.Value)));
									RouteRegistered = true;
									break;
								}
							}
						}
					}
					catch (Exceptions.CommandFailureException ex)
					{
						if (ex.McmpResponse.ErrorMessage == "MEM: Can't read node")
						{
							// This is ok, it means the node doesnt really exist, which would be the case if we were just registering
							RouteRegistered = false;
						}
						else
							throw;
					}
				}

				// If we arent registered, do it now
				if (!RouteRegistered)
				{
					if (log != null && log.IsInfoEnabled) log.Info("RegisterRoute() Cluster:" + ClusterUri.ToString() + " Contexts:" + string.Join(",", contexts.Select(c => GetRootedContextPath(c))));
					McmpResponse result = ClientConfig.ExecuteCommand(ClusterUri, Constants.Commands.Config, "Context", string.Join(",", contexts.Select(c => GetRootedContextPath(c))));
					if (log != null && log.IsDebugEnabled) log.Debug("RegisterRoute() Result: " + result.ToString());

					// Mark as registered
					RouteRegistered = true;

					// Add contexts to our list of contexts
					//RegisteredContexts.AddRange(contexts.Select(c => GetRootedContextPath(c)));
				}

				foreach (string context in contexts)
					RegisterContextPath(context);
			}
			catch (Exceptions.CommandFailureException ex)
			{
				if (ex.InnerException is WebException && (
					(ex.InnerException as WebException).Status == WebExceptionStatus.ConnectFailure
					|| (ex.InnerException as WebException).Status == WebExceptionStatus.Timeout
					))
					if (log != null && log.IsErrorEnabled && ex.McmpResponse != null)
						try { log.Error("PerformStatusUpdate() Could not connect to mod_cluster for " + ex.McmpResponse.Command + " command - " + ex.Message + " on " + ClusterUri); }
						catch { }
					else
						if (log != null && log.IsErrorEnabled)
							try { log.Error("PerformStatusUpdate() CommandFailureException on " + ClusterUri, ex); }
							catch { }

				// Failure, append list of contexts to try again later
				AppendContextsToRegister(contexts);
				return;
			}
			catch (Exception ex)
			{
				if (log != null && log.IsErrorEnabled) log.Error("RegisterRoute(" + string.Join(",", contexts) + ") fail", ex);

				// Failure, append list of contexts to try again later
				AppendContextsToRegister(contexts);
			}
			finally
			{
				// Start timers
				StartRegistrationTimer();

				// Start status processes -- if started already, this will cause a recheck at statusInitialDueTime
				BeginStatusChecks();
			}
		}

		private void AppendContextsToRegister(IEnumerable<string> contexts)
		{
			foreach (var context in contexts)
				if (!contextsToRegister.Contains(context))
					contextsToRegister.Enqueue(context);
		}

		public override string ToString()
		{
			return ClusterUri.ToString();
		}

		internal void DeregisterRoute()
		{
			if (!RouteRegistered)
				return;
			try
			{
				McmpResponse result = ClientConfig.ExecuteCommand(ClusterUri, Constants.Commands.StopAllApps);
				if (log != null && log.IsInfoEnabled) log.Info("DeregisterRoute() Result: " + result.ToString());
				result = ClientConfig.ExecuteCommand(ClusterUri, Constants.Commands.RemoveAllApps);
				if (log != null && log.IsInfoEnabled) log.Info("DeregisterRoute() Result: " + result.ToString());
				RouteRegistered = false;
				StopStatusChecks();
			}
			catch (Exception ex)
			{
				if (log != null && log.IsErrorEnabled) log.Error("DeregisterRoute() fail", ex);
				throw;
			}
		}

		private string GetRootedContextPath(string context)
		{
			return (!context.StartsWith("~") ? context : context.Replace("~", ClientConfig.AppRootPath).Replace("//", "/"));
		}

		internal void RegisterContextPath(string context)
		{
			// Root if necessary
			context = GetRootedContextPath(context);

			if (RegisteredContexts.Contains(context))
				return;

			McmpResponse result = null;
			try
			{
				// If route not reigstered, we will wait
				if (!RouteRegistered)
				{
					// Queue up context
					if (!contextsToRegister.Contains(context))
						contextsToRegister.Enqueue(context);
					return;
				}

				// Register individual context on route
				result = ClientConfig.ExecuteCommand(ClusterUri, Constants.Commands.EnableApp, "Context", context);
				if (log != null && log.IsInfoEnabled) log.Info("RegisterContextPath(" + context + ") Result: " + result.ToString());
			}
			catch (Exceptions.CommandFailureException ex)
			{
				if (ex.McmpResponse.ErrorMessage == "MEM: Can't read node")
					log.Error("Cannot register context to cluster because this node does not exist?", ex);
				else
					if (log != null && log.IsErrorEnabled) log.Error("RegisterContextPath(" + context + ") Result: " + ex.McmpResponse, ex);

				// Queue up context
				if (!contextsToRegister.Contains(context))
					contextsToRegister.Enqueue(context);

				throw;
			}
			catch (Exception ex)
			{
				if (log != null && log.IsErrorEnabled) log.Error("RegisterContextPath(" + context + ") Result: " + result, ex);

				// Queue up context
				if (!contextsToRegister.Contains(context))
					contextsToRegister.Enqueue(context);

				throw;
			}

			// Store context in our list
			RegisteredContexts.Add(context);
		}

		internal void DeregisterContext(string context)
		{
			context = GetRootedContextPath(context);

			try
			{
				McmpResponse disableresult = ClientConfig.ExecuteCommand(ClusterUri, Constants.Commands.DisableApp, "Context", context);
				if (log != null && log.IsInfoEnabled) log.Info("DeregisterContext(" + context + ") Result: " + disableresult.ToString());
				McmpResponse stopresult = ClientConfig.ExecuteCommand(ClusterUri, Constants.Commands.StopApp, "Context", context);
				if (log != null && log.IsInfoEnabled) log.Info("DeregisterContext(" + context + ") Result: " + stopresult.ToString());
				McmpResponse removeresult = ClientConfig.ExecuteCommand(ClusterUri, Constants.Commands.RemoveApp, "Context", context);
				if (log != null && log.IsInfoEnabled) log.Info("DeregisterContext(" + context + ") Result: " + removeresult.ToString());

				if (disableresult.HttpStatusCode == HttpStatusCode.OK && removeresult.HttpStatusCode == HttpStatusCode.OK)
					RegisteredContexts.Remove(context);
			}
			catch (Exception ex)
			{
				if (log != null && log.IsErrorEnabled) log.Error("DeregisterContext(" + context + ") fail", ex);
				throw;
			}
		}

		public string Ping()
		{
			// TODO: can we do this if not registered?
			if (!RouteRegistered)
				return null;
			McmpResponse result = ClientConfig.ExecuteCommand(ClusterUri, Constants.Commands.Ping);
			if (log != null && log.IsDebugEnabled) log.Debug("Ping(" + ClusterUri.ToString() + ") " + result.ToString());
			return result.ResponseBody;
		}

		public Dictionary<string, List<Dictionary<string, string>>> Dump()
		{
			Dictionary<string, List<Dictionary<string, string>>> dump = new Dictionary<string, List<Dictionary<string, string>>>();
			McmpResponse result = ClientConfig.ExecuteCommand(ClusterUri, Constants.Commands.Dump);
			if (log != null && log.IsInfoEnabled) log.Info("Dump(" + ClusterUri.ToString() + ") " + result.ToString());
			if (result.ResponseBody == null)
				return dump;

			//balancer: [1] Name: mybalancer Sticky: 0 [NSESSION]/[nsession] remove: 0 force: 0 Timeout: 0 maxAttempts: 1
			//node: [1:1],Balancer: mybalancer,JVMRoute: MyIISRoute,LBGroup: [],Host: localhost,Port: 17508,Type: http,flushpackets: 0,flushwait: 10,ping: 5,smax: 1,ttl: 60,timeout: 0
			//host: 1 [localhost] vhost: 1 node: 1
			//context: 1 [/] vhost: 1 node: 1 status: 1

			foreach (string line in result.ResponseBody.Split('\n'))
			{
				// Get our TYPE
				if (string.IsNullOrEmpty(line))
					continue;
				string type = line.Substring(0, line.IndexOf(':'));
				if (!dump.ContainsKey(type))
					dump.Add(type, new List<Dictionary<string, string>>());

				switch (type)
				{
					case "node":
						dump[type].Add(line.ToDictionary(separator: ',', equals: ": "));
						break;
					case "balancer":
						Dictionary<string, string> balancer = new Dictionary<string, string>();
						balancer.Add("RAW", line);
						Queue<string> balanceritems = new Queue<string>(line.Split(' '));
						balancer.Add(balanceritems.Dequeue().TrimEnd(':'), balanceritems.Dequeue());
						balancer.Add(balanceritems.Dequeue().TrimEnd(':'), balanceritems.Dequeue());
						balancer.Add(balanceritems.Dequeue().TrimEnd(':'), balanceritems.Dequeue() + " " + balanceritems.Dequeue());
						while (balanceritems.Count > 0)
							balancer.Add(balanceritems.Dequeue().TrimEnd(':'), balanceritems.Dequeue());
						dump[type].Add(balancer);
						break;
					case "host":
					case "context":
						Dictionary<string, string> item = new Dictionary<string, string>();
						item.Add("RAW", line);
						Queue<string> dataitems = new Queue<string>(line.Split(' '));
						item.Add(dataitems.Dequeue().TrimEnd(':'), dataitems.Dequeue() + " " + dataitems.Dequeue());
						while (dataitems.Count > 0)
							item.Add(dataitems.Dequeue().TrimEnd(':'), dataitems.Dequeue());
						dump[type].Add(item);
						break;
				}
			}
			return dump;
		}

		#region Status Check Timers
		private Timer statusCheckTimer = null;

		private void StatusCheckTimerCallback(object state)
		{
			PerformStatusUpdates();
		}

		private void PerformStatusUpdates()
		{
			if (disposed) return;
			if (!RouteRegistered) return;

			int load = 0;

			try { load = ClientConfig.ClientLoadModule.Load; }
			catch (Exception ex) { load = 50; log.Error("Error in getting value from ClientLoadModule.Load, using dummy load=" + load, ex); }

			McmpResponse reply = null;
			try
			{
				reply = ClientConfig.ExecuteCommand(ClusterUri, Constants.Commands.Status, "Load", load.ToString());
			}
			catch (Exception ex)
			{
				//TODO: if exception says node not found or whatever, set not registered and turn on registration timer and stop status checks
				//RouteRegistered = false;
				//StartRegistrationTimer();
				if (log != null && log.IsErrorEnabled)
					log.Error("PerformStatusUpdate() Exception on " + ClusterUri, ex);
				return;
			}

			if (!string.IsNullOrEmpty(reply.ErrorMessage))
			{
				if (log != null && log.IsErrorEnabled)
					log.Error("PerformStatusUpdate() Status returned error: " + reply.ErrorMessage + " on " + ClusterUri);
				return;
			}

			// Verify Status Response Message
			try
			{
				if (reply.GetResponseValue("Type") != "STATUS-RSP")
					if (log != null && log.IsErrorEnabled)
						log.Error("PerformStatusUpdate() Status did not return STATUS_RSP, got bad response type: " + reply.GetResponseValue("Type") + " on " + ClusterUri);
			}
			catch (Exception ex)
			{
				if (log != null && log.IsErrorEnabled)
					log.Error("PerformStatusUpdate() Status returned error on " + ClusterUri + ": " + reply.ErrorMessage, ex);
				return;
			}

			try { if (log != null && log.IsDebugEnabled) log.Debug("STATUS " + reply.GetResponseValue("state") + " on " + ClusterUri); }
			catch { }
		}

		private void StopStatusChecks()
		{
			if (log != null && log.IsInfoEnabled) log.Info("StopStatusChecks() Cluster: " + ClusterUri.ToString());
			if (statusCheckTimer != null)
				statusCheckTimer.Change(Timeout.Infinite, Timeout.Infinite);
		}

		private void BeginStatusChecks()
		{
			if (!RouteRegistered) return;
			if (log != null && log.IsInfoEnabled) log.Info("BeginStatusChecks() Cluster: " + ClusterUri.ToString());
			// Start status process -- if started already, this will cause a recheck at statusInitialDueTime
			statusCheckTimer.Change((int)ClientConfig.StatusInitialDueTime.TotalMilliseconds, (int)ClientConfig.StatusPeriod.TotalMilliseconds);
		}

		private void RegistrationTimerCallback(object state)
		{
			if (disposed) return;

			try
			{
				StopRegistrationTimer();
				if (!RouteRegistered)
				{
					try
					{
						RegisterRoute(RegisteredContexts.Union(contextsToRegister.ToList()));
					}
					catch (Exceptions.CommandFailureException ex)
					{
						if (ex.InnerException is WebException && (
							(ex.InnerException as WebException).Status == WebExceptionStatus.ConnectFailure
							|| (ex.InnerException as WebException).Status == WebExceptionStatus.Timeout
							))
						{
							if (log != null && log.IsErrorEnabled)
								log.Error("RegistrationTimerCallback() Could not connect to mod_cluster for STATUS command - " + ex.Message + " on " + ClusterUri);
							return;
						}
						else
							if (log != null && log.IsErrorEnabled)
								log.Error("RegistrationTimerCallback() CommandFailureException on " + ClusterUri, ex);
						return;
					}
					catch (Exception ex)
					{
						if (log != null && log.IsErrorEnabled) log.Error("RegistrationTimerCallback() failed to RegisterRoute, wait til next timer callback");
						return;
					}
				}
				while (RouteRegistered && contextsToRegister.Count > 0)
				{
					try
					{
						string context = null;
						if (!contextsToRegister.TryDequeue(out context))
							return;
						if (log != null && log.IsDebugEnabled) log.Debug("RegistrationTimerCallback() trying to register context " + context);
						RegisterContextPath(context);
					}
					catch (Exception ex)
					{
						if (log != null && log.IsErrorEnabled) log.Error("RegistrationTimerCallback() failed to add context, wait til next timer callback");
						return;
					}
				}
			}
			finally
			{
				StartRegistrationTimer();
			}
		}

		private void StartRegistrationTimer()
		{
			if (registrationTimer != null)
				registrationTimer.Change(3000, 3000);
		}

		private void StopRegistrationTimer()
		{
			if (registrationTimer != null)
				registrationTimer.Change(Timeout.Infinite, Timeout.Infinite);
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
					try
					{
						if (statusCheckTimer != null)
						{
							StopStatusChecks();
							//statusCheckTimer.Dispose();
							statusCheckTimer = null;
						}
						if (registrationTimer != null)
						{
							StopRegistrationTimer();
							//workTimer.Dispose();
							registrationTimer = null;
						}

						if (ClientConfig.UnregisterOnDispose && RouteRegistered)
							DeregisterRoute();
					}
					catch { }
				}

				// Unmanaged resources to release
			}
			disposed = true;
		}

		#endregion
	}
}
