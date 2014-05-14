using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCMP.Exceptions
{
	public class MultiClusterException : ApplicationException
	{
		public MultiClusterException()
		{
		}

		internal MultiClusterException(IEnumerable<Exception> exceptions)
			: base("Multiple exceptions: " + string.Join("; ", exceptions.Select(e => e.GetType().Name + ": " + e.Message)))
		{
			if (exceptions.Count() == 1)
				throw exceptions.First();
		}

		internal MultiClusterException(Dictionary<Cluster, Exception> exceptions)
			: base("Multiple exceptions: " + string.Join("; ", exceptions.Select(e => e.Key.ToString() + ": " + e.Value.GetType().Name + ": " + e.Value.Message)))
		{
			if (exceptions.Count() == 1)
				throw exceptions.First().Value;
		}
	}
}
