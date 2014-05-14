using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MCMP.Extensions
{
	internal static class Text
	{
		private static UTF8Encoding utf8NoBOMEncoding = new UTF8Encoding(false);
		//private static UTF8Encoding utf8BOMEncoding = new UTF8Encoding(true);

		public static byte[] ToBytes(this string inString)
		{
			return utf8NoBOMEncoding.GetBytes(inString);
		}

		public static string GetString(this byte[] inBytes)
		{
			return utf8NoBOMEncoding.GetString(inBytes, 0, inBytes.Length);
		}

		public static string TakeUpTo(this string inString, int maxLength)
		{
			if (inString.Length <= maxLength) return inString;
			return inString.Substring(0, maxLength);
		}

		public static bool IsNullOrEmpty(this string str)
		{
			return string.IsNullOrEmpty(str);
		}
		public static bool IsNotNullOrEmpty(this string str)
		{
			return !string.IsNullOrEmpty(str);
		}

		public static bool IsNullOrWhitespace(this string str)
		{
			return string.IsNullOrWhiteSpace(str);
		}
		public static bool IsNotNullOrWhitespace(this string str)
		{
			return !string.IsNullOrWhiteSpace(str);
		}
	}
}
