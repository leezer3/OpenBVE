using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.ServiceModel;
using OpenBveApi.Runtime;
using OpenBveApi.Interop;

namespace WCFServer
{
	public class AtsPluginProxyService : IAtsPluginProxy
	{

		private string PluginFile;

		public void SetPluginFile(string fileName)
		{
			Console.WriteLine(@"Setting plugin file " + fileName);
			this.PluginFile = fileName;
		}

		public bool Load(VehicleSpecs specs, InitializationModes mode)
		{
			int result;
			try {
				result = Win32LoadDLL(this.PluginFile, this.PluginFile);
			} catch (Exception ex) {
				Callback.ReportError("Error loading Win32 plugin: " + ex);
				throw;
			}
			if (result == 0) {
				int errorCode = Marshal.GetLastWin32Error();
				string errorMessage = new Win32Exception(errorCode).Message;
				Callback.ReportError(String.Format("Error loading Win32 plugin: {0} (0x{1})", errorMessage, errorCode.ToString("x")));
				return false;
			}
			try {
				Win32Load();
			} catch (Exception ex) {
				Callback.ReportError("Error loading Win32 plugin: " + ex);
				return false;
			}
			int version;
			try {
				version = Win32GetPluginVersion();
			} catch (Exception ex) {
				Callback.ReportError("Error loading Win32 plugin: " + ex);
				throw;
			}
			if (version != 131072) {
				Callback.ReportError("Win32 plugin " + PluginFile + " is of an unsupported version.");
				try {
					Win32Dispose();
				} catch (Exception ex)
				{
					return false;
				}
				Win32UnloadDLL();
				return false;
			}
			try {
				Win32VehicleSpec win32Spec;
				win32Spec.BrakeNotches = specs.BrakeNotches;
				win32Spec.PowerNotches = specs.PowerNotches;
				win32Spec.AtsNotch = specs.AtsNotch;
				win32Spec.B67Notch = specs.B67Notch;
				win32Spec.Cars = specs.Cars;
				Win32SetVehicleSpec(ref win32Spec.BrakeNotches);
			} catch (Exception ex)
			{
				Callback.ReportError("Error loading Win32 plugin: " + ex);
				return false;
			}
			try {
				Win32Initialize((int)mode);
			} catch (Exception ex)
			{
				Callback.ReportError("Error loading Win32 plugin: " + ex);
				return false;
			}
			Console.WriteLine(@"Plugin loaded successfully.");
			return true;
		}

		public void Unload()
		{
			Console.WriteLine(@"Unloading plugin....");
			try {
				Win32UnloadDLL();
			} catch (Exception ex) {
				Callback.ReportError(ex.ToString());
			}
			Console.WriteLine(@"Plugin unloaded successfuly.");
		}

		public void BeginJump(InitializationModes mode)
		{
			try {
				Win32Initialize((int)mode);
			} catch (Exception ex) {
				Callback.ReportError(ex.ToString());
			}
		}

		public ElapseProxy Elapse(ElapseProxy ProxyData)
		{
			try
			{
				if (ProxyData == null)
				{
					Console.WriteLine("ProxyData was null??");
				}
				else if (ProxyData.Data.TotalTime == null)
				{
					Console.WriteLine("Data was null???");
				}
				else
				{
					double time = ProxyData.Data.TotalTime.Milliseconds;
					Win32VehicleState win32State;
					win32State.Location = ProxyData.Data.Vehicle.Location;
					win32State.Speed = (float) ProxyData.Data.Vehicle.Speed.KilometersPerHour;
					win32State.Time = (int) Math.Floor(time - 2073600000.0 * Math.Floor(time / 2073600000.0));
					win32State.BcPressure = (float) ProxyData.Data.Vehicle.BcPressure;
					win32State.MrPressure = (float) ProxyData.Data.Vehicle.MrPressure;
					win32State.ErPressure = (float) ProxyData.Data.Vehicle.ErPressure;
					win32State.BpPressure = (float) ProxyData.Data.Vehicle.BpPressure;
					win32State.SapPressure = (float) ProxyData.Data.Vehicle.SapPressure;
					win32State.Current = 0.0f;
					Win32Handles win32Handles;
					win32Handles.Brake = ProxyData.Data.Handles.BrakeNotch;
					win32Handles.Power = ProxyData.Data.Handles.PowerNotch;
					win32Handles.Reverser = ProxyData.Data.Handles.Reverser;
					win32Handles.ConstantSpeed = ProxyData.Data.Handles.ConstSpeed ? 1 : 2;
					Win32Elapse(ref win32Handles.Brake, ref win32State.Location, ref ProxyData.Panel[0], ref ProxyData.Sound[0]);
					ProxyData.Data.Handles.Reverser = win32Handles.Reverser;
					ProxyData.Data.Handles.PowerNotch = win32Handles.Power;
					ProxyData.Data.Handles.BrakeNotch = win32Handles.Brake;
					if (win32Handles.ConstantSpeed == 1)
					{
						ProxyData.Data.Handles.ConstSpeed = true;
					}
					else if (win32Handles.ConstantSpeed == 2)
					{
						ProxyData.Data.Handles.ConstSpeed = false;
					}
					else if (win32Handles.ConstantSpeed != 0)
					{
						//this.PluginValid = false;
					}
				}
			}
			catch (Exception ex)
			{
				Callback.ReportError(ex.ToString());
			}
			
			return ProxyData;
		}

		public void SetReverser(int reverser)
		{
			Callback.ReportError("test");
			try {
				Win32SetReverser(reverser);
			} catch (Exception ex) {
				Callback.ReportError(ex.ToString());
			}
		}
		
		public void SetPowerNotch(int powerNotch)
		{
			try {
				Win32SetPower(powerNotch);
			} catch (Exception ex) {
				Callback.ReportError(ex.ToString());
			}
		}

		public void SetBrake(int brakeNotch)
		{
			try {
				Win32SetBrake(brakeNotch);
			} catch (Exception ex) {
				Callback.ReportError(ex.ToString());
			}
		}

		public void KeyDown(int key)
		{
			try {
				Win32KeyDown(key);
			} catch (Exception ex) {
				Callback.ReportError(ex.ToString());
			}
		}

		public void KeyUp(int key)
		{
			try {
				Win32KeyUp(key);
			} catch (Exception ex) {
				Callback.ReportError(ex.ToString());
			}
		}

		public void HornBlow(int type)
		{
			try {
				Win32HornBlow(type);
			} catch (Exception ex) {
				Callback.ReportError(ex.ToString());
			}
		}

		public void DoorChange(int oldState, int newState)
		{
			if (oldState == 0 & newState != 0) {
				try {
					Win32DoorOpen();
				} catch (Exception ex) {
					Callback.ReportError(ex.ToString());
				}
			} else if (oldState != 0 & newState == 0) {
				try {
					Win32DoorClose();
				} catch (Exception ex) {
					Callback.ReportError(ex.ToString());
				}
			}
		}

		public void SetSignal(int aspect)
		{
			try {
				Win32SetSignal(aspect);
			} catch (Exception ex) {
				Callback.ReportError(ex.ToString());
			}
		}

		public void SetBeacon(BeaconData beacon)
		{
			try {
				Win32BeaconData win32Beacon;
				win32Beacon.Type = beacon.Type;
				win32Beacon.Signal = beacon.Signal.Aspect;
				win32Beacon.Distance = (float)beacon.Signal.Distance;
				win32Beacon.Optional = beacon.Optional;
				Win32SetBeaconData(ref win32Beacon.Type);
			} catch (Exception ex) {
				Callback.ReportError(ex.ToString());
			}
		}

		IAtsPluginCallback Callback
		{
			get
			{
				return OperationContext.Current.GetCallbackChannel<IAtsPluginCallback>();
			}
		}

		[DllImport("AtsPluginProxy.dll", EntryPoint = "LoadDLL", ExactSpelling = true, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		private static extern int Win32LoadDLL([MarshalAs(UnmanagedType.LPWStr)]string UnicodeFileName, [MarshalAs(UnmanagedType.LPStr)]string AnsiFileName);
		
		[DllImport("AtsPluginProxy.dll", EntryPoint = "UnloadDLL", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private static extern int Win32UnloadDLL();
		
		[DllImport("AtsPluginProxy.dll", EntryPoint = "Load", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private static extern void Win32Load();
		
		[DllImport("AtsPluginProxy.dll", EntryPoint = "Dispose", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private static extern void Win32Dispose();
		
		[DllImport("AtsPluginProxy.dll", EntryPoint = "GetPluginVersion", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private static extern int Win32GetPluginVersion();
		
		[DllImport("AtsPluginProxy.dll", EntryPoint = "SetVehicleSpec", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private static extern void Win32SetVehicleSpec(ref int spec);
		
		[DllImport("AtsPluginProxy.dll", EntryPoint = "Initialize", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private static extern void Win32Initialize(int brake);
		
		[DllImport("AtsPluginProxy.dll", EntryPoint = "Elapse", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private static extern void Win32Elapse(ref int handles, ref double state, ref int panel, ref int sound);
		
		[DllImport("AtsPluginProxy.dll", EntryPoint = "SetPower", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private static extern void Win32SetPower(int notch);

		[DllImport("AtsPluginProxy.dll", EntryPoint = "SetBrake", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private static extern void Win32SetBrake(int notch);

		[DllImport("AtsPluginProxy.dll", EntryPoint = "SetReverser", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private static extern void Win32SetReverser(int pos);

		[DllImport("AtsPluginProxy.dll", EntryPoint = "KeyDown", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private static extern void Win32KeyDown(int atsKeyCode);

		[DllImport("AtsPluginProxy.dll", EntryPoint = "KeyUp", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private static extern void Win32KeyUp(int atsKeyCode);

		[DllImport("AtsPluginProxy.dll", EntryPoint = "HornBlow", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private static extern void Win32HornBlow(int hornType);

		[DllImport("AtsPluginProxy.dll", EntryPoint = "DoorOpen", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private static extern void Win32DoorOpen();
		
		[DllImport("AtsPluginProxy.dll", EntryPoint = "DoorClose", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private static extern void Win32DoorClose();
		
		[DllImport("AtsPluginProxy.dll", EntryPoint = "SetSignal", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private static extern void Win32SetSignal(int signal);
		
		[DllImport("AtsPluginProxy.dll", EntryPoint = "SetBeaconData", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private static extern void Win32SetBeaconData(ref int beacon);
		
		[StructLayout(LayoutKind.Sequential, Size = 20)]
		private struct Win32VehicleSpec {
			internal int BrakeNotches;
			internal int PowerNotches;
			internal int AtsNotch;
			internal int B67Notch;
			internal int Cars;
		}
		
		[StructLayout(LayoutKind.Sequential, Size = 40)]
		private struct Win32VehicleState {
			internal double Location;
			internal float Speed;
			internal int Time;
			internal float BcPressure;
			internal float MrPressure;
			internal float ErPressure;
			internal float BpPressure;
			internal float SapPressure;
			internal float Current;
		}

		[StructLayout(LayoutKind.Sequential, Size = 16)]
		private struct Win32Handles {
			internal int Brake;
			internal int Power;
			internal int Reverser;
			internal int ConstantSpeed;
		}
		
		[StructLayout(LayoutKind.Sequential, Size = 16)]
		private struct Win32BeaconData {
			internal int Type;
			internal int Signal;
			internal float Distance;
			internal int Optional;
		}

		internal void OnClose()
		{

		}
		
	}

	class Program
	{
		static void Main(string[] args)
		{
			DeleteMenu(GetSystemMenu(GetConsoleWindow(), false),SC_CLOSE, MF_BYCOMMAND);
			using (ServiceHost host = new ServiceHost(typeof(AtsPluginProxyService), new Uri(@"net.pipe://localhost")))
			{

				host.AddServiceEndpoint(typeof(IAtsPluginProxy), new NetNamedPipeBinding(), @"pipename");
				host.Open();
				Shared.eventHostReady.Set();
				
				Console.WriteLine(@"ATS Plugin Proxy Service is available.");
				Shared.eventHostShouldStop.WaitOne();
				host.Close();
			}
		}

		private const int MF_BYCOMMAND = 0x00000000;
		public const int SC_CLOSE = 0xF060;

		[DllImport("user32.dll")]
		public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

		[DllImport("user32.dll")]
		private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

		[DllImport("kernel32.dll", ExactSpelling = true)]
		private static extern IntPtr GetConsoleWindow();

	}
}
