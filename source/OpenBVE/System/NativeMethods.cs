using System;
using System.Runtime.InteropServices;

namespace OpenBve
{
	internal static class NativeMethods
	{
		/// <summary>Gets the UID of the current user if running on a Unix based system</summary>
		/// <returns>The UID</returns>
		/// <remarks>Used for checking if we are running as ROOT (don't!)</remarks>
		[DllImport("libc")]
#pragma warning disable IDE1006 // Suppress the VS2017 naming style rule, as this is an external syscall
		internal static extern uint getuid();
#pragma warning restore IDE1006

		// gets the clock frequency for ticks per second
		[DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall), System.Security.SuppressUnmanagedCodeSecurity]
		internal static extern bool QueryPerformanceFrequency(ref long PerformanceFrequency);

		// gets the number of elapsed ticks for future calculations
		[DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall), System.Security.SuppressUnmanagedCodeSecurity]
		internal static extern bool QueryPerformanceCounter(ref long PerformanceCount);

		[DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern IntPtr LoadLibraryA([MarshalAs(UnmanagedType.LPStr), In] string lpLibFileName);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern IntPtr LoadLibraryW([MarshalAs(UnmanagedType.LPWStr), In] string lpLibFileName);

		[DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr), In] string lpProcName);

		[DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool FreeLibrary(IntPtr hModule);
	}
}
