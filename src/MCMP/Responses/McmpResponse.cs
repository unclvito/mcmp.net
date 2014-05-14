using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MCMP.Responses
{
	internal class McmpResponse
	{
		public Uri Cluster { get; set; }
		public HttpStatusCode HttpStatusCode { get; set; }
		public string HttpStatusDescription { get; set; }
		public string Command { get; set; }
		public string ErrorMessage { get; set; }
		public string RequestPath { get; set; }
		public string ResponseBody { get; set; }
		public string GetResponseValue(string key)
		{
			foreach (string val in ResponseBody.TrimEnd().Split('&'))
			{
				string[] parts = val.Split(new char[] { '=' }, 2);
				if (parts[0].Equals(key, StringComparison.InvariantCultureIgnoreCase))
					return parts[1];
			}
			throw new KeyNotFoundException("Cannot find " + key);
		}
		public string RequestBody { get; set; }
		public override string ToString()
		{
			return "[" + Cluster.ToString() +"] "
				+ ((int)HttpStatusCode).ToString() + " "
				+ HttpStatusDescription + " "
				+ Command + " "
				+ RequestPath + " "
				+ (ErrorMessage == null ? "" : "ErrorMessage: [" + ErrorMessage + "] ")
				+ (string.IsNullOrEmpty(RequestBody) ? "" : "RequestBody: [" + RequestBody.Trim() + "] ")
				+ (string.IsNullOrEmpty(ResponseBody) ? "" : "ResponseBody: [" + ResponseBody.Trim() + "] ");
		}
	}
}
