using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace OpenBveApi
{
	/// <summary>This class implements a high-precision, multi-platform timer</summary>
	public static class CPreciseTimer
	{
		private static readonly bool UseStopWatch;

		//UNSAFE ZONE//
		[DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall), System.Security.SuppressUnmanagedCodeSecurity]
		private static extern bool QueryPerformanceFrequency(ref long PerformanceFrequency);  //gets the clock frequency for ticks per second
		[DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall), System.Security.SuppressUnmanagedCodeSecurity]
		private static extern bool QueryPerformanceCounter(ref long PerformanceCount);  //gets the number of elapsed ticks for future calculations
		//UNSAFE ZONE//

		private static readonly long _ticksPerSecond = 0;  //initialize variables
		private static long _previousElapsedTime = 0;

		private static readonly Stopwatch stopWatch;

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
				//We're running on Linux/ OSX, so we must use the stopwatch
				UseStopWatch = true;
				stopWatch = new Stopwatch();
				stopWatch.Start();
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
			if (UseStopWatch)
			{
				OldTicks = Ticks;
				Ticks = stopWatch.ElapsedMilliseconds;

				if (MinWait > Ticks - OldTicks)
				{
					System.Threading.Thread.Sleep(5);
					Ticks = stopWatch.ElapsedMilliseconds;
				}

				DeltaTime = Ticks - OldTicks;
				return DeltaTime / 1000.0;
			}
			long time = 0;
			QueryPerformanceCounter(ref time); //gets the number of ticks elapsed, pulled from the cloop
			double elapsedTime = (time - _previousElapsedTime)/(double) _ticksPerSecond;
			//gets the total elapsed ticks by subtracting the current number of ticks from the last elapsed number of ticks.  it then divides it by ticks per second to get the actual amount of time that has passed.
			_previousElapsedTime = time; //sets the previous elapsed ticks for the next calculation
			return elapsedTime;
		}

		/// <summary>Gets the elapsed time in seconds between two ticks</summary>
		public static double GetElapsedTime(int oldTicks, int newTicks)
		{
			return (newTicks - oldTicks) / (double)_ticksPerSecond;
		}

		/// <summary>Gets the current environment tick count</summary>
		/// <returns></returns>
		public static int GetClockTicks()
		{
			return Environment.TickCount;
		}


	}

	/// <summary>
	/// Cross-platform frame rate limiter.
	/// Uses a bulk Thread.Sleep followed by a short Thread.Yield wait, mirroring the approach used by modern OpenTK,
	/// in order to avoid the high power consumption of a pure spin-wait based limiter.
	/// </summary>
	public static class FrameLimiter
	{
		// Tolerance as a fraction of the scheduler period, left unslept so that we do not overshoot the target
		private const double Tolerance = 0.02;
		// On Windows, raise the timer resolution to 1ms for accurate frame pacing
		private const uint WindowsTimerPeriod = 1;
		// Hard cap applied even when the user selects 'Unlimited'
		private const int HardFpsLimit = 540;

		private static bool timerResolutionRaised;
		private static int schedulerPeriod = 1;
		private static long frameStartTimestamp;

		[DllImport("winmm")]
		private static extern uint timeBeginPeriod(uint uPeriod);

		[DllImport("winmm")]
		private static extern uint timeEndPeriod(uint uPeriod);

		/// <summary>Marks the start of a frame. Must be called once at the beginning of each rendered frame.</summary>
		public static void StartFrame()
		{
			frameStartTimestamp = Stopwatch.GetTimestamp();
		}

		/// <summary>Waits until the end of the current frame's allotted timeslot.</summary>
		/// <param name="fpsLimit">The maximum frames per second selected by the user. A value of zero or less means unlimited, subject to the hard cap.</param>
		public static void ApplyLimit(int fpsLimit)
		{
			int limit = fpsLimit > 0 ? System.Math.Min(fpsLimit, HardFpsLimit) : HardFpsLimit;
			if (frameStartTimestamp == 0)
			{
				return;
			}
			long now = Stopwatch.GetTimestamp();
			long target = frameStartTimestamp + (long)((double)Stopwatch.Frequency / limit);
			double remainingMs = (double)(target - now) * 1000.0 / Stopwatch.Frequency;
			if (remainingMs <= 0.0)
			{
				// The frame overran its timeslot (or was throttled by VSync) - nothing to wait for,
				// so do not raise the system timer resolution needlessly
				return;
			}
			RaiseTimerResolution();
			double sleepMs = remainingMs - schedulerPeriod * Tolerance;
			int ticks = (int)(sleepMs / schedulerPeriod);
			if (ticks > 0)
			{
				Thread.Sleep(ticks * schedulerPeriod);
			}
			while (Stopwatch.GetTimestamp() < target)
			{
				Thread.Yield();
			}
		}

		/// <summary>Restores the system timer resolution, if it was previously raised.</summary>
		public static void RestoreTimerResolution()
		{
			if (timerResolutionRaised)
			{
				timerResolutionRaised = false;
				try
				{
					timeEndPeriod(WindowsTimerPeriod);
				}
				catch
				{
					// Not on Windows, or winmm unavailable
				}
			}
		}

		private static void RaiseTimerResolution()
		{
			if (!timerResolutionRaised)
			{
				timerResolutionRaised = true;
				try
				{
					if (Environment.OSVersion.Platform == PlatformID.Win32NT)
					{
						timeBeginPeriod(WindowsTimerPeriod);
						schedulerPeriod = (int)WindowsTimerPeriod;
					}
					else
					{
						// Linux and macOS can accurately sleep for around 1ms
						schedulerPeriod = 1;
					}
				}
				catch
				{
					schedulerPeriod = 1;
				}
			}
		}
	}
}
