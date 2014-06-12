using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace MCMP
{
	public class Bootstrap : IHttpModule
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Bootstrap));

		static Bootstrap()
		{
			// Note: You can't put this into a thread to speed things up as it wont have access to the hosting environment

			// Configure ModCluster
			try { Client.ClientInstance.RegisterAppRootContext(); }
			// TODO: allow configurable failure on startup
			catch (Exception ex) { throw new ApplicationException("Cannot initialize mod_cluster", ex); }
		}

		public void Init(HttpApplication application)
		{
			application.PostRequestHandlerExecute += application_PostRequestHandlerExecute;
			application.Disposed += (sender, e) => { Client.ClientInstance.Dispose(); };
		}

		void application_PostRequestHandlerExecute(object sender, EventArgs e)
		{
			try
			{
				// If we are in an active session and not cookieless,
				// get a distinct list of cookies required for each mod_cluster
				// and add it to the current cookie collection
				if (HttpContext.Current != null
					&& HttpContext.Current.Session != null
					&& !HttpContext.Current.Session.IsCookieless)
					if (MCMP.Client.ClientInstance.Clusters.Any(c => c.StickySessionsEnabled))
						foreach (HttpCookie stickyCookie in Client.ClientInstance.Clusters.Select(c => c.GetStickySessionCookie()).Distinct())
							if (!HttpContext.Current.Request.Cookies.AllKeys.Contains(stickyCookie.Name))
								HttpContext.Current.Response.Cookies.Add(stickyCookie);
			}
			catch (Exception ex) { if (log != null && log.IsErrorEnabled) log.Error("application_PostRequestHandlerExecute() fail", ex); }
		}

		public void Dispose() { }
	}
}
