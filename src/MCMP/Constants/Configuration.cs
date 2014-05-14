using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MCMP.Constants
{
	public static class Configuration
	{
		public static TimeSpan CommandRequestTimeout = TimeSpan.FromSeconds(5);
		public static TimeSpan CommandReadWriteTimeout = TimeSpan.FromSeconds(5);
		public static readonly string CommandContentType = "application/x-www-form-urlencoded";
		public static readonly string CommandMessageHeader = "mess";
		public static readonly TimeSpan DefaultStatusInitialDueTime = TimeSpan.FromSeconds(1);
		public static readonly TimeSpan DefaultStatusPeriod = TimeSpan.FromMinutes(1);
		public static readonly bool DefaultMulticastEnabled = false;
		public static readonly TimeSpan DefaultMulticastClientDelayedStart = TimeSpan.FromSeconds(2);
		public static readonly string DefaultBalancer = "mycluster";
		public static readonly string DefaultStickySessionCookie = "NSESSION";
		public static readonly bool DefaultStickySession = true;
		public static readonly string DefaultStickySessionPath = "nsession";
		public static readonly bool DefaultStickySessionRemove = false;
		public static readonly bool DefaultStickySessionForce = false;
		public static readonly int DefaultWaitWorker = 0;
		public static readonly int DefaultMaxAttempts = 1;
		public static readonly string DefaultDomain = "";
		public static readonly string DefaultHost = null;
		public static readonly int DefaultPort = 0;
		public static readonly string DefaultType = null;
		public static readonly bool DefaultFlushPackets = false;
		public static readonly TimeSpan DefaultFlushWait = TimeSpan.FromSeconds(10);
		public static readonly TimeSpan DefaultPing = TimeSpan.FromSeconds(5);
		public static readonly int DefaultSmax = 1;
		public static readonly TimeSpan DefaultTtl = TimeSpan.FromSeconds(60);
		public static readonly TimeSpan DefaultTimeout = TimeSpan.Zero;
		public static readonly string DefaultAlias = null;
		public static readonly bool DefaultUnregisterOnDispose = false;
	}
}
