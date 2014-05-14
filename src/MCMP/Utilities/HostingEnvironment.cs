using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MCMP.Utilities
{
	internal class HostingEnvironment
	{
		public static IEnumerable<KeyValuePair<string, Uri>> GetBindings(HttpContext context)
		{
			// Get the Site name  
			string siteName = System.Web.Hosting.HostingEnvironment.SiteName;

			// Get the sites section from the AppPool.config 
			Microsoft.Web.Administration.ConfigurationSection sitesSection =
				Microsoft.Web.Administration.WebConfigurationManager.GetSection(null, null, "system.applicationHost/sites");

			foreach (Microsoft.Web.Administration.ConfigurationElement site in sitesSection.GetCollection())
			{
				// Find the right Site 
				if (String.Equals((string)site["name"], siteName, StringComparison.OrdinalIgnoreCase))
				{

					// For each binding see if they are http based and return the port and protocol 
					foreach (Microsoft.Web.Administration.ConfigurationElement binding in site.GetCollection("bindings"))
					{
						string protocol = (string)binding["protocol"];
						string bindingInfo = (string)binding["bindingInformation"];

						if (protocol.StartsWith("http", StringComparison.OrdinalIgnoreCase))
						{
							string[] parts = bindingInfo.Split(':');
							if (parts.Length == 3)
							{
								string port = parts[1];
								string host = parts[0];
								if (host == "*")
									host = "localhost";
								if (parts[2] != string.Empty)
									host = parts[2];
								KeyValuePair<string, Uri> reply;
								try { reply = new KeyValuePair<string, Uri>(protocol, new Uri(protocol + "://" + host + ":" + port + "/")); }
								catch (Exception ex) { throw new ApplicationException("Cannot create uri from " + protocol + "://" + host + ":" + port + "/"); }
								yield return reply;
							}
						}
					}
				}
			}
		}

		public static string GetAppRootPath()
		{
			return HttpRuntime.AppDomainAppVirtualPath;

			//TODO: is this right?
			string appRootPath = HttpContext.Current.Server.MapPath("~");
			string appRootRelative = "/";
			if (appRootPath.ToLower().StartsWith(AppDomain.CurrentDomain.BaseDirectory.ToLower().TrimEnd('\\')))
				appRootRelative = appRootPath.Remove(0, AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\').Length).Replace("\\", "/");
			return appRootRelative;
		}

		public static string GetServerAlias()
		{
			List<string> serverAliases = new List<string>();

			// Get the Site name  
			string siteName = System.Web.Hosting.HostingEnvironment.SiteName;

			// Get the sites section from the AppPool.config 
			Microsoft.Web.Administration.ConfigurationSection sitesSection =
				Microsoft.Web.Administration.WebConfigurationManager.GetSection(null, null, "system.applicationHost/sites");

			foreach (Microsoft.Web.Administration.ConfigurationElement site in sitesSection.GetCollection())
			{
				// Find the right Site 
				if (String.Equals((string)site["name"], siteName, StringComparison.OrdinalIgnoreCase))
				{

					// For each binding see if they are http based and return the port and protocol 
					foreach (Microsoft.Web.Administration.ConfigurationElement binding in site.GetCollection("bindings"))
					{
						string protocol = (string)binding["protocol"];
						string bindingInfo = (string)binding["bindingInformation"];

						if (protocol.StartsWith("http", StringComparison.OrdinalIgnoreCase))
						{
							string[] parts = bindingInfo.Split(':');
							if (parts.Length == 3)
								serverAliases.AddRange(parts[2].Split(','));
						}
					}
				}
			}
			return string.Join(",",serverAliases);
		}
	}
}