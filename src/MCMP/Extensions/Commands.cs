using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using MCMP.Responses;

namespace MCMP.Extensions
{
	internal static class Commands
	{
		internal static McmpResponse ExecuteCommand(this Configuration.ClientConfiguration clientConfig, Uri cluster, Constants.Commands command, Dictionary<string, string> paramList = null)
		{
			string postData = clientConfig.GetPostData((string[])MCMP.Attributes.CommandAttribute.GetInfoFromCommand(command, "RequiredKeys"), paramList);
			return cluster.ExecuteCommand((string)MCMP.Attributes.CommandAttribute.GetInfoFromCommand(command, "Method"), 
				(string)MCMP.Attributes.CommandAttribute.GetInfoFromCommand(command, "Path"), postData);
		}

		internal static McmpResponse ExecuteCommand(this Configuration.ClientConfiguration clientConfig, Uri cluster, Constants.Commands command, params object[] paramList)
		{
			if (paramList.Length % 2 != 0)
				throw new InvalidDataException("paramList must be an even number of parameters");

			Dictionary<string, string> paramStringList = new Dictionary<string, string>();
			for (int idx = 0; idx < paramList.Length; idx += 2)
				paramStringList.Add(paramList[idx].ToString(), paramList[idx + 1].ToString());

			string postData = clientConfig.GetPostData((string[])MCMP.Attributes.CommandAttribute.GetInfoFromCommand(command, "RequiredKeys"), paramStringList);
			return cluster.ExecuteCommand((string)MCMP.Attributes.CommandAttribute.GetInfoFromCommand(command, "Method"), 
				(string)MCMP.Attributes.CommandAttribute.GetInfoFromCommand(command, "Path"), postData);
		}

		internal static McmpResponse ExecuteCommand(this Uri cluster, string method, string path, string postData)
		{
			ServicePointManager.ServerCertificateValidationCallback +=
				delegate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
				{ return true; };

			HttpWebRequest request = WebRequest.Create(cluster.ToString().TrimEnd('/') + path) as HttpWebRequest;
			request.Timeout = (int)Constants.Configuration.CommandRequestTimeout.TotalSeconds;
			request.ReadWriteTimeout = (int)Constants.Configuration.CommandReadWriteTimeout.TotalSeconds;
			request.Method = method;
			request.ContentType = Constants.Configuration.CommandContentType;
			byte[] bytes = postData.ToString().ToBytes();
			request.ContentLength = bytes.Length;

			try
			{
				request.GetRequestStream().Write(bytes, 0, bytes.Length);
				using (WebResponse response = request.GetResponse() as WebResponse)
				using (StreamReader reader = new StreamReader(response.GetResponseStream()))
				{
					try
					{
						return new McmpResponse()
						{
							Cluster = cluster,
							HttpStatusCode = (response as HttpWebResponse).StatusCode,
							HttpStatusDescription = (response as HttpWebResponse).StatusDescription,
							Command = method,
							RequestPath = path,
							RequestBody = postData,
							ResponseBody = reader.ReadToEnd(),
						};
					}
					finally
					{
						response.Close();
					}
				}
			}
			catch (WebException ex)
			{
				if (ex.Response != null)
				{
					using (var errorResponse = (HttpWebResponse)ex.Response)
					using (var reader = new StreamReader(errorResponse.GetResponseStream()))
						throw new Exceptions.CommandFailureException(new McmpResponse()
							{
								Cluster = cluster,
								ErrorMessage = errorResponse.Headers[Constants.Configuration.CommandMessageHeader],
								Command = method,
								RequestPath = path,
								HttpStatusCode = errorResponse.StatusCode,
								HttpStatusDescription = errorResponse.StatusDescription,
								RequestBody = postData,
								ResponseBody = reader.ReadToEnd(),
							}, ex);
				}

				// If no parsable response, throw it generically
				throw new Exceptions.CommandFailureException("Command " + method + " failed", ex);
			}
			catch (Exception ex)
			{
				throw new Exceptions.CommandFailureException("Command " + method + " unhandled exception", ex);
			}
		}
	}
}
