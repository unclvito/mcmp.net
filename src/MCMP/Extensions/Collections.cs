using System;
using System.Collections.Generic;

namespace MCMP.Extensions
{
	internal static class Collections
	{
		public static Dictionary<string, string> ToDictionary(this byte[] inBytes)
		{
			return inBytes.GetString().ToDictionary();
		}

		public static Dictionary<string, string> ToDictionary(this string inString, char separator = '\n', string equals = ": ")
		{
			Dictionary<string, string> dataDict = new Dictionary<string, string>();
			foreach (var line in inString.Split(separator))
				if (line.Contains(equals))
					dataDict.Add(line.Substring(0, line.IndexOf(equals)), line.Substring(line.IndexOf(equals) + equals.Length).TrimEnd());
			return dataDict;
		}

		public static Uri GetUriFromBroadcast(this IDictionary<string, string> data)
		{
			return new Uri(data["X-Manager-Protocol"] + "://" + data["X-Manager-Address"]);
		}
	}
}
