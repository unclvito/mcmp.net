using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MCMP
{
	public class ClientLoad : Interfaces.IClientLoad
	{
		#region Public Properties
		public int Load 
		{
			get
			{
				SampleProcessorTime();
				return CalculateStatusLoad();
			}
		}
		#endregion

		#region Private Data Members
		private TimeSpan myProcessLastProcessorTime = TimeSpan.Zero;
		private TimeSpan myProcessLastProcessorTimeSample = TimeSpan.Zero;
		private DateTime myProcessLastProcessorTimeSampled = DateTime.MinValue;
		private TimeSpan myProcessLastElapsedProcessorTime = TimeSpan.Zero;
		private Process myProcess = null;
		#endregion

		#region ctors
		public ClientLoad()
		{
			// Get process and last process time
			myProcess = Process.GetCurrentProcess();
			SampleProcessorTime();
		}
		#endregion

		/// <summary>
		/// Samples the total processor time of this process, stores values and returns percentage of CPU used during last sample span
		/// </summary>
		/// <returns></returns>
		private double SampleProcessorTime()
		{
			//TODO: Improve this calculation - See http://docs.jboss.org/mod_cluster/1.1.0/html/java.load.html
			//TODO: Add history and decay

			// Calculate elapsed processor time
			myProcessLastElapsedProcessorTime = myProcess.TotalProcessorTime - myProcessLastProcessorTime;
			myProcessLastProcessorTime = myProcess.TotalProcessorTime;

			// Calculate sample duration
			myProcessLastProcessorTimeSample = DateTime.UtcNow - myProcessLastProcessorTimeSampled;
			myProcessLastProcessorTimeSampled = DateTime.UtcNow;

			// Return avg
			return LastSampleProcessorCpuPercentage;
		}

		/// <summary>
		/// Returns percentage of CPU used during last sample
		/// </summary>
		private double LastSampleProcessorCpuPercentage
		{
			get { return myProcessLastElapsedProcessorTime.TotalMilliseconds / myProcessLastProcessorTimeSample.TotalMilliseconds; }
		}

		private int CalculateStatusLoad()
		{
			// TODO: right now just uses cpu use of process. should also include the cpu use of the entire server somehow.
			return 100 - Math.Min(100, 1 + (int)Math.Ceiling(LastSampleProcessorCpuPercentage * 100));
		}
	}
}