﻿using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using OpenBveApi.Interface;
using OpenBveApi.Runtime;
using SoundManager;

namespace OpenBve
{
	/// <summary>Represents a legacy Win32 plugin.</summary>
	internal class Win32Plugin : PluginManager.Plugin
	{

		// --- win32 proxy calls ---

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

		private static class SoundInstructions
		{
			internal const int Stop = -10000;
			internal const int PlayLooping = 0;
			internal const int PlayOnce = 1;
			internal const int Continue = 2;
		}

		private static class ConstSpeedInstructions
		{
			internal const int Continue = 0;
			internal const int Enable = 1;
			internal const int Disable = 2;
		}

		// --- members ---
		private readonly string PluginFile;
		private readonly int[] Sound;
		private readonly int[] LastSound;
		private GCHandle PanelHandle;
		private GCHandle SoundHandle;

		// --- constructors ---
		internal Win32Plugin(string pluginFile, TrainManager.Train train)
		{
			PluginTitle = Path.GetFileName(pluginFile);
			PluginValid = true;
			PluginMessage = null;
			Train = train;
			Panel = new int[256];
			SupportsAI = false;
			LastTime = 0.0;
			LastReverser = -2;
			LastPowerNotch = -1;
			LastBrakeNotch = -1;
			LastAspects = new int[] { };
			LastSection = -1;
			LastException = null;
			PluginFile = pluginFile;
			Sound = new int[256];
			LastSound = new int[256];
			PanelHandle = new GCHandle();
			SoundHandle = new GCHandle();
		}

		// --- functions ---
		internal override bool Load(VehicleSpecs specs, InitializationModes mode)
		{
			int result;
			try
			{
				result = Win32LoadDLL(PluginFile, PluginFile);
			}
			catch (Exception ex)
			{
				LastException = ex;
				throw;
			}
			if (result == 0)
			{
				int errorCode = Marshal.GetLastWin32Error();
				string errorMessage = new Win32Exception(errorCode).Message;
				Interface.AddMessage(MessageType.Error, true,
					string.Format("Error loading Win32 plugin: {0} (0x{1})", errorMessage, errorCode.ToString("x")));
				return false;
			}
			try
			{
				Win32Load();
			}
			catch (Exception ex)
			{
				LastException = ex;
				throw;
			}
			int version;
			try
			{
				version = Win32GetPluginVersion();
			}
			catch (Exception ex)
			{
				LastException = ex;
				throw;
			}
			if (version != 131072)
			{
				Interface.AddMessage(MessageType.Error, false, "The train plugin " + PluginTitle + " is of an unsupported version.");
				try
				{
					Win32Dispose();
				}
				catch (Exception ex)
				{
					LastException = ex;
					throw;
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
				LastException = ex;
				throw;
			}
			try
			{
				Win32Initialize((int)mode);
			}
			catch (Exception ex)
			{
				LastException = ex;
				throw;
			}
			UpdatePower();
			UpdateBrake();
			UpdateReverser();
			if (PanelHandle.IsAllocated)
			{
				PanelHandle.Free();
			}
			if (SoundHandle.IsAllocated)
			{
				SoundHandle.Free();
			}
			PanelHandle = GCHandle.Alloc(Panel, GCHandleType.Pinned);
			SoundHandle = GCHandle.Alloc(Sound, GCHandleType.Pinned);
			return true;
		}
		internal override void Unload()
		{
			if (PanelHandle.IsAllocated)
			{
				PanelHandle.Free();
			}
			if (SoundHandle.IsAllocated)
			{
				SoundHandle.Free();
			}
			try
			{
				Win32UnloadDLL();
			}
			catch (Exception ex)
			{
				LastException = ex;
				throw;
			}
		}
		internal override void BeginJump(InitializationModes mode)
		{
			try
			{
				Win32Initialize((int)mode);
			}
			catch (Exception ex)
			{
				LastException = ex;
				throw;
			}
		}
		internal override void EndJump()
		{
		}

		protected override void Elapse(ElapseData data)
		{
			try
			{
				double time = data.TotalTime.Milliseconds;
				Win32VehicleState win32State;
				win32State.Location = data.Vehicle.Location;
				win32State.Speed = (float)data.Vehicle.Speed.KilometersPerHour;
				win32State.Time = (int)Math.Floor(time - 2073600000.0 * Math.Floor(time / 2073600000.0));
				win32State.BcPressure = (float)data.Vehicle.BcPressure;
				win32State.MrPressure = (float)data.Vehicle.MrPressure;
				win32State.ErPressure = (float)data.Vehicle.ErPressure;
				win32State.BpPressure = (float)data.Vehicle.BpPressure;
				win32State.SapPressure = (float)data.Vehicle.SapPressure;
				win32State.Current = 0.0f;
				Win32Handles win32Handles;
				win32Handles.Brake = data.Handles.BrakeNotch;
				win32Handles.Power = data.Handles.PowerNotch;
				win32Handles.Reverser = data.Handles.Reverser;
				win32Handles.ConstantSpeed = data.Handles.ConstSpeed ? ConstSpeedInstructions.Enable : ConstSpeedInstructions.Disable;
				Win32Elapse(ref win32Handles.Brake, ref win32State.Location, ref Panel[0], ref Sound[0]);
				data.Handles.Reverser = win32Handles.Reverser;
				data.Handles.PowerNotch = win32Handles.Power;
				data.Handles.BrakeNotch = win32Handles.Brake;
				if (win32Handles.ConstantSpeed == ConstSpeedInstructions.Enable)
				{
					data.Handles.ConstSpeed = true;
				}
				else if (win32Handles.ConstantSpeed == ConstSpeedInstructions.Disable)
				{
					data.Handles.ConstSpeed = false;
				}
				else if (win32Handles.ConstantSpeed != ConstSpeedInstructions.Continue)
				{
					PluginValid = false;
				}
				/*
				 * Process the sound instructions
				 * */
				for (int i = 0; i < Sound.Length; i++)
				{
					if (Sound[i] != LastSound[i])
					{
						if (Sound[i] == SoundInstructions.Stop)
						{
							if (i < Train.Cars[Train.DriverCar].Sounds.Plugin.Length)
							{
								Program.Sounds.StopSound(Train.Cars[Train.DriverCar].Sounds.Plugin[i].Source);
							}
						}
						else if (Sound[i] > SoundInstructions.Stop & Sound[i] <= SoundInstructions.PlayLooping)
						{
							if (i < Train.Cars[Train.DriverCar].Sounds.Plugin.Length)
							{
								SoundsBase.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.Plugin[i].Buffer;
								if (buffer != null)
								{
									double volume = (Sound[i] - SoundInstructions.Stop) / (double)(SoundInstructions.PlayLooping - SoundInstructions.Stop);
									if (Program.Sounds.IsPlaying(Train.Cars[Train.DriverCar].Sounds.Plugin[i].Source))
									{
										Train.Cars[Train.DriverCar].Sounds.Plugin[i].Source.Volume = volume;
									}
									else
									{
										Train.Cars[Train.DriverCar].Sounds.Plugin[i].Source = Program.Sounds.PlaySound(buffer, 1.0, volume, Train.Cars[Train.DriverCar].Sounds.Plugin[i].Position, Train, Train.DriverCar, true);
									}
								}
							}
						}
						else if (Sound[i] == SoundInstructions.PlayOnce)
						{
							if (i < Train.Cars[Train.DriverCar].Sounds.Plugin.Length)
							{
								SoundsBase.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.Plugin[i].Buffer;
								if (buffer != null)
								{
									Train.Cars[Train.DriverCar].Sounds.Plugin[i].Source = Program.Sounds.PlaySound(buffer, 1.0, 1.0, Train.Cars[Train.DriverCar].Sounds.Plugin[i].Position, Train, Train.DriverCar, false);
								}
							}
							Sound[i] = SoundInstructions.Continue;
						}
						else if (Sound[i] != SoundInstructions.Continue)
						{
							PluginValid = false;
						}
						LastSound[i] = Sound[i];
					}
					else
					{
						if ((Sound[i] < SoundInstructions.Stop | Sound[i] > SoundInstructions.PlayLooping) && Sound[i] != SoundInstructions.PlayOnce & Sound[i] != SoundInstructions.Continue)
						{
							PluginValid = false;
						}
					}
				}
			}
			catch (Exception ex)
			{
				LastException = ex;
				throw;
			}
		}

		protected override void SetReverser(int reverser)
		{
			try
			{
				Win32SetReverser(reverser);
			}
			catch (Exception ex)
			{
				LastException = ex;
				throw;
			}
		}

		protected override void SetPower(int powerNotch)
		{
			try
			{
				Win32SetPower(powerNotch);
			}
			catch (Exception ex)
			{
				LastException = ex;
				throw;
			}
		}

		protected override void SetBrake(int brakeNotch)
		{
			try
			{
				Win32SetBrake(brakeNotch);
			}
			catch (Exception ex)
			{
				LastException = ex;
				throw;
			}
		}
		internal override void KeyDown(VirtualKeys key)
		{
			try
			{
				Win32KeyDown((int)key);
			}
			catch (Exception ex)
			{
				LastException = ex;
				throw;
			}
		}
		internal override void KeyUp(VirtualKeys key)
		{
			try
			{
				Win32KeyUp((int)key);
			}
			catch (Exception ex)
			{
				LastException = ex;
				throw;
			}
		}
		internal override void HornBlow(HornTypes type)
		{
			try
			{
				Win32HornBlow((int)type);
			}
			catch (Exception ex)
			{
				LastException = ex;
				throw;
			}
		}
		internal override void DoorChange(DoorStates oldState, DoorStates newState)
		{
			if (oldState == DoorStates.None & newState != DoorStates.None)
			{
				try
				{
					Win32DoorOpen();
				}
				catch (Exception ex)
				{
					LastException = ex;
					throw;
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
					LastException = ex;
					throw;
				}
			}
		}

		protected override void SetSignal(SignalData[] signal)
		{
			if (LastAspects.Length == 0 || signal[0].Aspect != LastAspects[0])
			{
				try
				{
					Win32SetSignal(signal[0].Aspect);
				}
				catch (Exception ex)
				{
					LastException = ex;
					throw;
				}
			}
		}

		protected override void SetBeacon(BeaconData beacon)
		{
			try
			{
				Win32BeaconData win32Beacon;
				win32Beacon.Type = beacon.Type;
				win32Beacon.Signal = beacon.Signal.Aspect;
				win32Beacon.Distance = (float)beacon.Signal.Distance;
				win32Beacon.Optional = beacon.Optional;
				Win32SetBeaconData(ref win32Beacon.Type);
			}
			catch (Exception ex)
			{
				LastException = ex;
				throw;
			}
		}

		protected override void PerformAI(AIData data)
		{
		}

	}
}
