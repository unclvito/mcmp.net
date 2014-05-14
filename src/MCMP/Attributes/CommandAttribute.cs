using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MCMP.Constants;

namespace MCMP.Attributes
{
	public class CommandAttribute : Attribute
	{
		public string Method { get; set; }
		public string Path { get; set; }
		public string[] RequiredKeys { get; set; }

		private static Dictionary<string, object> infoFromCommandCache = new Dictionary<string, object>();

		public static object GetInfoFromCommand(Commands command, string parameterName)
		{
			if (infoFromCommandCache.ContainsKey(command.ToString() + parameterName))
				return infoFromCommandCache[command.ToString() + parameterName];

			foreach (FieldInfo field in typeof(Commands).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly))
			{
				if (field.Name != command.ToString()) continue;
				foreach (Attribute currAttr in field.GetCustomAttributes(true))
				{
					CommandAttribute valueAttribute = currAttr as CommandAttribute;
					if (valueAttribute != null)
					{
						infoFromCommandCache[command.ToString() + "Method"] = valueAttribute.Method;
						infoFromCommandCache[command.ToString() + "RequiredKeys"] = valueAttribute.RequiredKeys;
						infoFromCommandCache[command.ToString() + "Path"] = valueAttribute.Path;
						return infoFromCommandCache[command.ToString() + parameterName];
					}
				}
			}
			return null;
		}
	}
}