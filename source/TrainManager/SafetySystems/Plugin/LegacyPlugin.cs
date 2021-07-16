using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using OpenBveApi.Interface;
using OpenBveApi.Runtime;
using TrainManager.Trains;

namespace TrainManager.SafetySystems
{
	/// <summary>Represents a legacy Win32 plugin.</summary>
	internal class Win32Plugin : Plugin
	{

		// --- win32 proxy calls ---

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
		
		private static class ConstSpeedInstructions
		{
			internal const int Continue = 0;
			internal const int Enable = 1;
			internal const int Disable = 2;
		}

		// --- members ---
		private readonly string PluginFile;
		private readonly int[] LastSound;
		private GCHandle PanelHandle;
		private GCHandle SoundHandle;

		// --- constructors ---
		internal Win32Plugin(string pluginFile, TrainBase train)
		{
			base.PluginTitle = System.IO.Path.GetFileName(pluginFile);
			base.PluginValid = true;
			base.PluginMessage = null;
			base.Train = train;
			base.Panel = new int[256];
			base.SupportsAI = AISupport.None;
			switch (PluginTitle.ToLowerInvariant())
			{
				case "ukdt.dll":
					SupportsAI = AISupport.Program;
					AI = new UKDtAI(this);
					break;
				case "ukspt.dll":
					SupportsAI = AISupport.Program;
					AI = new UKSptAI(this);
					break;
				case "ukmut.dll":
					SupportsAI = AISupport.Program;
					AI = new UKMUtAI(this);
					break;
				case "hei_ats.dll":
					SupportsAI = AISupport.Program;
					AI = new HeiAtsAI(this);
					break;
			}
			
			base.LastTime = 0.0;
			base.LastReverser = -2;
			base.LastPowerNotch = -1;
			base.LastBrakeNotch = -1;
			base.LastAspects = new int[] { };
			base.LastSection = -1;
			base.LastException = null;
			this.PluginFile = pluginFile;
			this.Sound = new int[256];
			this.LastSound = new int[256];
			this.PanelHandle = new GCHandle();
			this.SoundHandle = new GCHandle();
		}

		// --- functions ---
		public override bool Load(VehicleSpecs specs, InitializationModes mode)
		{
			int result;
			bool retry = true;
			retryLoad:
			try
			{
				result = Win32LoadDLL(this.PluginFile, this.PluginFile);
			}
			catch (Exception ex)
			{
				base.LastException = ex;
				throw;
			}

			if (result == 0)
			{
				if (retry)
				{
					/*
					 * Win32 plugin loading is unreliable on some systems
					 * Unable to reproduce this, but let's try a sleep & single retry attempt
					 */
					Thread.Sleep(100);
					retry = false;
					goto retryLoad;
				}

				int errorCode = Marshal.GetLastWin32Error();
				string errorMessage = new Win32Exception(errorCode).Message;
				TrainManagerBase.currentHost.AddMessage(MessageType.Error, true,$"Error loading Win32 plugin: {errorMessage} (0x{errorCode:x})");
				return false;
			}

			try
			{
				Win32Load();
			}
			catch (Exception ex)
			{
				base.LastException = ex;
				throw;
			}

			int version;
			try
			{
				version = Win32GetPluginVersion();
			}
			catch (Exception ex)
			{
				base.LastException = ex;
				throw;
			}

			if (version == 0 && System.IO.Path.GetFileName(PluginFile).ToLowerInvariant() != "ats2.dll" || version != 131072)
			{
				TrainManagerBase.currentHost.AddMessage(MessageType.Error, false, "The train plugin " + base.PluginTitle + " is of an unsupported version.");
				try
				{
					Win32Dispose();
				}
				catch (Exception ex)
				{
					base.LastException = ex;
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
				base.LastException = ex;
				throw;
			}

			try
			{
				Win32Initialize((int) mode);
			}
			catch (Exception ex)
			{
				base.LastException = ex;
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

		public override void Unload()
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
				base.LastException = ex;
				throw;
			}
		}

		public override void BeginJump(InitializationModes mode)
		{
			try
			{
				Win32Initialize((int) mode);
			}
			catch (Exception ex)
			{
				base.LastException = ex;
				throw;
			}
			if (SupportsAI == AISupport.Program)
			{
				AI.BeginJump(mode);
			}
		}

		public override void EndJump()
		{
			AI.EndJump();
		}

		protected override void Elapse(ref ElapseData data)
		{
			try
			{
				double time = data.TotalTime.Milliseconds;
				Win32VehicleState win32State;
				win32State.Location = data.Vehicle.Location;
				win32State.Speed = (float) data.Vehicle.Speed.KilometersPerHour;
				win32State.Time = (int) Math.Floor(time - 2073600000.0 * Math.Floor(time / 2073600000.0));
				win32State.BcPressure = (float) data.Vehicle.BcPressure;
				win32State.MrPressure = (float) data.Vehicle.MrPressure;
				win32State.ErPressure = (float) data.Vehicle.ErPressure;
				win32State.BpPressure = (float) data.Vehicle.BpPressure;
				win32State.SapPressure = (float) data.Vehicle.SapPressure;
				win32State.Current = 0.0f;
				Win32Handles win32Handles;
				win32Handles.Brake = data.Handles.BrakeNotch;
				win32Handles.Power = data.Handles.PowerNotch;
				win32Handles.Reverser = data.Handles.Reverser;
				win32Handles.ConstantSpeed = data.Handles.ConstSpeed ? ConstSpeedInstructions.Enable : ConstSpeedInstructions.Disable;
				Win32Elapse(ref win32Handles.Brake, ref win32State.Location, ref base.Panel[0], ref this.Sound[0]);
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
					this.PluginValid = false;
				}

				/*
				 * Process the sound instructions
				 * */
				for (int i = 0; i < this.Sound.Length; i++)
				{
					if (this.Sound[i] != this.LastSound[i])
					{
						if (this.Sound[i] == SoundInstructions.Stop)
						{
							if (i < base.Train.Cars[base.Train.DriverCar].Sounds.Plugin.Length)
							{
								Train.Cars[Train.DriverCar].Sounds.Plugin[i].Stop();
							}
						}
						else if (this.Sound[i] > SoundInstructions.Stop & this.Sound[i] <= SoundInstructions.PlayLooping)
						{
							if (i < base.Train.Cars[base.Train.DriverCar].Sounds.Plugin.Length)
							{
								if (Train.Cars[Train.DriverCar].Sounds.Plugin[i].Buffer != null)
								{
									double volume = (this.Sound[i] - SoundInstructions.Stop) / (double)(SoundInstructions.PlayLooping - SoundInstructions.Stop);
									if (Train.Cars[Train.DriverCar].Sounds.Plugin[i].IsPlaying)
									{
										Train.Cars[Train.DriverCar].Sounds.Plugin[i].Source.Volume = volume;
									}
									else
									{
										Train.Cars[Train.DriverCar].Sounds.Plugin[i].Play(1.0, volume, Train.Cars[Train.DriverCar], true);
									}
								}
							}
						}
						else if (this.Sound[i] == SoundInstructions.PlayOnce)
						{
							if (i < base.Train.Cars[base.Train.DriverCar].Sounds.Plugin.Length)
							{
								if (Train.Cars[Train.DriverCar].Sounds.Plugin[i].Buffer != null)
								{
									Train.Cars[Train.DriverCar].Sounds.Plugin[i].Play(Train.Cars[Train.DriverCar], false);
								}
							}

							this.Sound[i] = SoundInstructions.Continue;
						}
						else if (this.Sound[i] != SoundInstructions.Continue)
						{
							this.PluginValid = false;
						}

						this.LastSound[i] = this.Sound[i];
					}
					else
					{
						if ((this.Sound[i] < SoundInstructions.Stop | this.Sound[i] > SoundInstructions.PlayLooping) && this.Sound[i] != SoundInstructions.PlayOnce & this.Sound[i] != SoundInstructions.Continue)
						{
							this.PluginValid = false;
						}
					}
				}
			}
			catch (Exception ex)
			{
				base.LastException = ex;
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
				base.LastException = ex;
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
				base.LastException = ex;
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
				base.LastException = ex;
				throw;
			}
		}

		public override void KeyDown(VirtualKeys key)
		{
			try
			{
				Win32KeyDown((int) key);
			}
			catch (Exception ex)
			{
				base.LastException = ex;
				throw;
			}
		}

		public override void KeyUp(VirtualKeys key)
		{
			try
			{
				Win32KeyUp((int) key);
			}
			catch (Exception ex)
			{
				base.LastException = ex;
				throw;
			}
		}

		public override void HornBlow(HornTypes type)
		{
			try
			{
				Win32HornBlow((int) type);
			}
			catch (Exception ex)
			{
				base.LastException = ex;
				throw;
			}
		}

		public override void DoorChange(DoorStates oldState, DoorStates newState)
		{
			if (oldState == DoorStates.None & newState != DoorStates.None)
			{
				try
				{
					Win32DoorOpen();
				}
				catch (Exception ex)
				{
					base.LastException = ex;
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
					base.LastException = ex;
					throw;
				}
			}
		}

		protected override void SetSignal(SignalData[] signal)
		{
			if (base.LastAspects.Length == 0 || signal[0].Aspect != base.LastAspects[0])
			{
				try
				{
					Win32SetSignal(signal[0].Aspect);
				}
				catch (Exception ex)
				{
					base.LastException = ex;
					throw;
				}
			}
		}

		protected override void SetBeacon(BeaconData beacon)
		{
			if (AI != null)
			{
				AI.SetBeacon(beacon);
			}
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
				base.LastException = ex;
				throw;
			}
		}

		protected override void PerformAI(AIData data)
		{
			if (SupportsAI == AISupport.Program)
			{
				AI.Perform(data);
			}
		}

		/// <summary>Checks whether a specified file is a valid Win32 plugin.</summary>
		/// <param name="file">The file to check.</param>
		/// <returns>Whether the file is a valid Win32 plugin.</returns>
		internal static bool CheckHeader(string file) {
			using (System.IO.FileStream stream = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read)) {
				using (System.IO.BinaryReader reader = new System.IO.BinaryReader(stream)) {
					if (reader.ReadUInt16() != 0x5A4D) {
						/* Not MZ signature */
						return false;
					}
					stream.Position = 0x3C;
					stream.Position = reader.ReadInt32();
					if (reader.ReadUInt32() != 0x00004550) {
						/* Not PE signature */
						return false;
					}
					if (reader.ReadUInt16() != 0x014C) {
						/* Not IMAGE_FILE_MACHINE_I386 */
						return false;
					}
				}
			}
			return true;
		}
	}
}
