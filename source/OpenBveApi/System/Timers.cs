using System;
using System.Runtime.InteropServices;

namespace OpenBveApi
{
	/// <summary>This class implements a high-precision, multi-platform timer</summary>
	public static class CPreciseTimer
	{
		private static readonly bool UseEnvTicks;

		//UNSAFE ZONE//
		[DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall), System.Security.SuppressUnmanagedCodeSecurity]
		private static extern bool QueryPerformanceFrequency(ref long PerformanceFrequency);  //gets the clock frequency for ticks per second
		[DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall), System.Security.SuppressUnmanagedCodeSecurity]
		private static extern bool QueryPerformanceCounter(ref long PerformanceCount);  //gets the number of elapsed ticks for future calculations
		//UNSAFE ZONE//

		static readonly long _ticksPerSecond = 0;  //initialize variables
		static long _previousElapsedTime = 0;

		static CPreciseTimer()
		{
			//Enclose this in a try/ catch block, and if it barfs, we're on Linux or OSX
			try
			{
				QueryPerformanceFrequency(ref _ticksPerSecond);
				//gets the number of ticks per second (frequency) after calling the C function in the constructor
				GetElapsedTime(); //Get rid of first rubbish result
			}
			catch
			{
				//We're running on Linux/ OSX, so we must use the environment ticks
				//This actually has much better precision than under Windows too
				UseEnvTicks = true;
				GetElapsedTime();
			}
		}

		private static long Ticks = Environment.TickCount;
		private static long OldTicks = 0;
		private static double DeltaTime = 0;
		private const int MinWait = 0;

		/// <summary>Gets the elapsed time in seconds since the last call to GetElapsedTime</summary>
		public static double GetElapsedTime()
		{
			if (UseEnvTicks)
			{
				OldTicks = Ticks;
				Ticks = Environment.TickCount;

				while (MinWait > Ticks - OldTicks)
				{
					System.Threading.Thread.Sleep(0);
					Ticks = Environment.TickCount;
				}

				DeltaTime = (Ticks - OldTicks)/ 1000.0;
				return DeltaTime;
			}
			long time = 0;
			QueryPerformanceCounter(ref time); //gets the number of ticks elapsed, pulled from the cloop
			double elapsedTime = (double) (time - _previousElapsedTime)/(double) _ticksPerSecond;
			//gets the total elapsed ticks by subtracting the current number of ticks from the last elapsed number of ticks.  it then divides it by ticks per second to get the actual amount of time that has passed.
			_previousElapsedTime = time; //sets the previous elapsed ticks for the next calculation
			return elapsedTime;
		}

		/// <summary>Gets the elapsed time in seconds between two ticks</summary>
		public static double GetElapsedTime(int oldTicks, int newTicks)
		{
			if (UseEnvTicks)
			{
				return (newTicks - oldTicks) / 1000.0;
			}
			else
			{
				return (newTicks - oldTicks) / (double)_ticksPerSecond;
			}
		}

		/// <summary>Gets the current environment tick count</summary>
		/// <returns></returns>
		public static int GetClockTicks()
		{
			return Environment.TickCount;
		}


	}
}
