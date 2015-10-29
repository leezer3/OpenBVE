using System.Runtime.InteropServices;

namespace OpenBve {
    public static class CPreciseTimer
    {
        //UNSAFE ZONE//
        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32")]
        private static extern bool QueryPerformanceFrequency(ref long PerformanceFrequency);  //gets the clock frequency for ticks per second
        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32")]
        private static extern bool QueryPerformanceCounter(ref long PerformanceCount);  //gets the number of elapsed ticks for future calculations
        //UNSAFE ZONE//

        static long _ticksPerSecond = 0;  //initialize variables
        static long _previousElapsedTime = 0;

        static CPreciseTimer()
        {
            QueryPerformanceFrequency(ref _ticksPerSecond);  //gets the number of ticks per second (frequency) after calling the C function in the constructor
            GetElapsedTime(); //Get rid of first rubbish result
        }
        public static double GetElapsedTime()
        {
            long time = 0;
            QueryPerformanceCounter(ref time);  //gets the number of ticks elapsed, pulled from the cloop
            double elapsedTime = (double)(time - _previousElapsedTime) / (double)_ticksPerSecond;  //gets the total elapsed ticks by subtracting the current number of ticks from the last elapsed number of ticks.  it then divides it by ticks per second to get the actual amount of time that has passed.
            _previousElapsedTime = time;  //sets the previous elapsed ticks for the next calculation
            return elapsedTime;
        }


    }
}