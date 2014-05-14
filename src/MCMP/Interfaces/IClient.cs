using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCMP.Interfaces
{
	public interface IClient : IDisposable
	{
		List<Cluster> Clusters { get; }

		IClientLoad ClientLoadModule { get; set; }

		void RegisterAppRootContext();

		void RegisterContextPath(params string[] contexts);

		void RegisterContextPath(IEnumerable<string> contexts);

		void RegisterContextPath(string context);

		void DeregisterContext(string context);

		void DeregisterContexts();

		List<string> PingAll();

		Dictionary<Uri, Dictionary<string, List<Dictionary<string, string>>>> DumpAll();
	}
}
