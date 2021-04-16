//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020, Christopher Lees, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ServiceModel;
using OpenBveApi.Runtime;
using OpenBveApi.Interop;

namespace WCFServer
{
	public class AtsPluginProxyService : IAtsPluginProxy
	{
		private readonly int[] Panel = new int[256];
		private readonly int[] Sound = new int[256];
		private GCHandle PanelHandle = new GCHandle();
		private GCHandle SoundHandle = new GCHandle();
		/// <summary>The on-disk path to the proxied plugin</summary>
		private string PluginFile;

		public void SetPluginFile(string fileName)
		{
			Console.WriteLine(@"Setting plugin file " + fileName);
			this.PluginFile = fileName;
		}

		public bool Load(VehicleSpecs specs, InitializationModes mode)
		{
			int result;
			try
			{
				result = Win32LoadDLL(this.PluginFile, this.PluginFile);
			}
			catch (Exception ex)
			{
				Callback.ReportError("Error loading Win32 plugin: " + ex);
				throw;
			}

			if (result == 0)
			{
				int errorCode = Marshal.GetLastWin32Error();
				string errorMessage = new Win32Exception(errorCode).Message;
				Callback.ReportError($"Error loading Win32 plugin: {errorMessage} (0x{errorCode:x})");
				return false;
			}

			try
			{
				Win32Load();
			}
			catch (Exception ex)
			{
				Callback.ReportError("Error loading Win32 plugin: " + ex);
				return false;
			}

			int version;
			try
			{
				version = Win32GetPluginVersion();
			}
			catch (Exception ex)
			{
				Callback.ReportError("Error loading Win32 plugin: " + ex);
				throw;
			}

			if (version == 0 && System.IO.Path.GetFileName(PluginFile).ToLowerInvariant() != "ats2.dll" || version != 131072)
			{
				Callback.ReportError("Win32 plugin " + PluginFile + " is of an unsupported version.");
				try
				{
					Win32Dispose();
				}
				catch
				{
					return false;
				}

				Win32UnloadDLL();
				return false;
			}

			try
			{
				Win32VehicleSpec win32Spec;
				win32Spec.BrakeNotches = specs.BrakeNotches;
				win32Spec.PowerNotches = specs.PowerNotches;
				win32Spec.AtsNotch = specs.AtsNotch;
				win32Spec.B67Notch = specs.B67Notch;
				win32Spec.Cars = specs.Cars;
				Win32SetVehicleSpec(ref win32Spec.BrakeNotches);
			}
			catch (Exception ex)
			{
				Callback.ReportError("Error loading Win32 plugin: " + ex);
				return false;
			}

			try
			{
				Console.WriteLine(@"Initializing in mode: " + mode);
				Win32Initialize((int) mode);
			}
			catch (Exception ex)
			{
				Callback.ReportError("Error loading Win32 plugin: " + ex);
				return false;
			}

			Console.WriteLine(@"Plugin loaded successfully.");
			if (PanelHandle.IsAllocated) {
				PanelHandle.Free();
			}
			if (SoundHandle.IsAllocated) {
				SoundHandle.Free();
			}
			PanelHandle = GCHandle.Alloc(Panel, GCHandleType.Pinned);
			SoundHandle = GCHandle.Alloc(Sound, GCHandleType.Pinned);
			return true;
		}

		public void Unload()
		{
			Console.WriteLine(@"Unloading plugin....");
			try
			{
				Win32UnloadDLL();
			}
			catch (Exception ex)
			{
				Callback.ReportError(ex.ToString());
			}

			Console.WriteLine(@"Plugin unloaded successfuly.");
		}

		public void BeginJump(InitializationModes mode)
		{
			try
			{
				Console.WriteLine(@"Starting jump with mode: " + mode);
				Win32Initialize((int) mode);
			}
			catch (Exception ex)
			{
				Callback.ReportError(ex.ToString());
			}
		}

		public ElapseProxy Elapse(ElapseProxy ProxyData)
		{
			try
			{
				if (ProxyData == null)
				{
					Console.WriteLine(@"DEBUG: The recieved ProxyData was null");
				}
				else if (ProxyData.Data.TotalTime == null)
				{
					Console.WriteLine(@"DEBUG: The total time in the recieved ProxyData was invalid");
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
					Win32Elapse(ref win32Handles.Brake, ref win32State.Location, ref Panel[0], ref Sound[0]);
					ProxyData.Data.Handles.Reverser = win32Handles.Reverser;
					ProxyData.Data.Handles.PowerNotch = win32Handles.Power;
					ProxyData.Data.Handles.BrakeNotch = win32Handles.Brake;
					switch (win32Handles.ConstantSpeed)
					{
						case 0:
							//Not fitted
							break;
						case 1:
							ProxyData.Data.Handles.ConstSpeed = true;
							break;
						case 2:
							ProxyData.Data.Handles.ConstSpeed = false;
							break;
						default:
							Console.WriteLine(@"DEBUG: Invalid ConstantSpeed value recieved from plugin");
							break;
					}
					ProxyData.Panel = Panel;
					ProxyData.Sound = Sound;
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
			try
			{
				Win32SetReverser(reverser);
			}
			catch (Exception ex)
			{
				Callback.ReportError(ex.ToString());
			}
		}

		public void SetPowerNotch(int powerNotch)
		{
			try
			{
				Win32SetPower(powerNotch);
			}
			catch (Exception ex)
			{
				Callback.ReportError(ex.ToString());
			}
		}

		public void SetBrake(int brakeNotch)
		{
			try
			{
				Win32SetBrake(brakeNotch);
			}
			catch (Exception ex)
			{
				Callback.ReportError(ex.ToString());
			}
		}

		public void KeyDown(VirtualKeys key)
		{
			try
			{
				Win32KeyDown((int)key);
			}
			catch (Exception ex)
			{
				Callback.ReportError(ex.ToString());
			}
		}

		public void KeyUp(VirtualKeys key)
		{
			try
			{
				Win32KeyUp((int)key);
			}
			catch (Exception ex)
			{
				Callback.ReportError(ex.ToString());
			}
		}

		public void HornBlow(HornTypes type)
		{
			try
			{
				Win32HornBlow((int)type);
			}
			catch (Exception ex)
			{
				Callback.ReportError(ex.ToString());
			}
		}

		public void DoorChange(DoorStates oldState, DoorStates newState)
		{
			if (oldState == DoorStates.None & newState != DoorStates.None)
			{
				try
				{
					Win32DoorOpen();
				}
				catch (Exception ex)
				{
					Callback.ReportError(ex.ToString());
				}
			}
			else if (oldState != DoorStates.None & newState == DoorStates.None)
			{
				try
				{
					Win32DoorClose();
				}
				catch (Exception ex)
				{
					Callback.ReportError(ex.ToString());
				}
			}
		}

		public void SetSignal(int aspect)
		{
			try
			{
				Win32SetSignal(aspect);
			}
			catch (Exception ex)
			{
				Callback.ReportError(ex.ToString());
			}
		}

		public void SetBeacon(BeaconData beacon)
		{
			try
			{
				Win32BeaconData win32Beacon;
				win32Beacon.Type = beacon.Type;
				win32Beacon.Signal = beacon.Signal.Aspect;
				win32Beacon.Distance = (float) beacon.Signal.Distance;
				win32Beacon.Optional = beacon.Optional;
				Win32SetBeaconData(ref win32Beacon.Type);
			}
			catch (Exception ex)
			{
				Callback.ReportError(ex.ToString());
			}
		}

		IAtsPluginCallback Callback => OperationContext.Current.GetCallbackChannel<IAtsPluginCallback>();

		[DllImport("AtsPluginProxy.dll", EntryPoint = "LoadDLL", ExactSpelling = true, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		private static extern int Win32LoadDLL([MarshalAs(UnmanagedType.LPWStr)] string UnicodeFileName, [MarshalAs(UnmanagedType.LPStr)] string AnsiFileName);

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
		private struct Win32VehicleSpec
		{
			internal int BrakeNotches;
			internal int PowerNotches;
			internal int AtsNotch;
			internal int B67Notch;
			internal int Cars;
		}

		[StructLayout(LayoutKind.Sequential, Size = 40)]
		private struct Win32VehicleState
		{
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
		private struct Win32Handles
		{
			internal int Brake;
			internal int Power;
			internal int Reverser;
			internal int ConstantSpeed;
		}

		[StructLayout(LayoutKind.Sequential, Size = 16)]
		private struct Win32BeaconData
		{
			internal int Type;
			internal int Signal;
			internal float Distance;
			internal int Optional;
		}
	}

	class Program
	{
		static void Main()
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
