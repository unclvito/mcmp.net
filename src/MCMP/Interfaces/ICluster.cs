using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCMP.Interfaces
{
	public interface ICluster : IDisposable
	{
		Uri ClusterUri { get; }

		bool RouteRegistered { get;  }

		List<string> RegisteredContexts { get; }

		bool StickySessionsEnabled { get; }

		string StickySessionCookieName { get; }

		string Ping();

		Dictionary<string, List<Dictionary<string, string>>> Dump();
	}
}
