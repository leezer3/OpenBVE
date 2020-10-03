using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using OpenBveApi.Runtime;

namespace OpenBve
{
	internal partial class PluginManager
	{
		private class Win32Runtime : IRuntime, IDisposable
		{
			private enum InitialHandlePosition
			{
				Service = 0,
				Emergency = 1,
				Removed = 2
			}

			private static class SoundInstruction
			{
				internal const int Stop = -10000;
				internal const int PlayLooping = 0;
				internal const int Play = 1;
				internal const int Continue = 2;
			}

			private enum ConstSpeedInstruction
			{
				Continue = 0,
				Enable = 1,
				Disable = 2
			}

			private enum Win32HornType
			{
				Primary = 0,
				Secondary = 1,
				Music = 2
			}

			[StructLayout(LayoutKind.Sequential)]
			private struct Win32VehicleSpec
			{
				internal int BrakeNotches;
				internal int PowerNotches;
				internal int AtsNotch;
				internal int B67Notch;
				internal int Cars;
			}

			[StructLayout(LayoutKind.Sequential)]
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

			[StructLayout(LayoutKind.Sequential)]
			private readonly struct Win32Handles
			{
				internal readonly int Brake;
				internal readonly int Power;
				internal readonly int Reverser;
				internal readonly ConstSpeedInstruction ConstantSpeed;
			}

			[StructLayout(LayoutKind.Sequential)]
			private struct Win32BeaconData
			{
				internal int Type;
				internal int Signal;
				internal float Distance;
				internal int Optional;
			}

			#region Definition of Win32 plugin's delegate

			private delegate void Win32Load();

			private delegate void Win32Dispose();

			private delegate int Win32GetPluginVersion();

			private delegate void Win32SetVehicleSpec([In] Win32VehicleSpec vehicleSpec);

			private delegate void Win32Initialize(InitialHandlePosition brake);

			private delegate Win32Handles Win32Elapse([In] Win32VehicleState vehicleState, [In, Out] int[] panel, [In, Out] int[] sound);

			private delegate void Win32SetPower(int notch);

			private delegate void Win32SetBrake(int notch);

			private delegate void Win32SetReverser(int pos);

			private delegate void Win32KeyDown(VirtualKeys atsKeyCode);

			private delegate void Win32KeyUp(VirtualKeys atsKeyCode);

			private delegate void Win32HornBlow(Win32HornType hornType);

			private delegate void Win32DoorOpen();

			private delegate void Win32DoorClose();

			private delegate void Win32SetSignal(int signal);

			private delegate void Win32SetBeaconData([In] Win32BeaconData beaconData);

			#endregion

			private const int PLUGIN_VERSION = 0x20000;

			private string pluginFilePath;

			private IntPtr dllHandle;

			private bool disposed;

			#region Instance of Win32 plugin's delegate

			private Win32Load win32Load;

			private Win32Dispose win32Dispose;

			private Win32GetPluginVersion win32GetPluginVersion;

			private Win32SetVehicleSpec win32SetVehicleSpec;

			private Win32Initialize win32Initialize;

			private Win32Elapse win32Elapse;

			private Win32SetPower win32SetPower;

			private Win32SetBrake win32SetBrake;

			private Win32SetReverser win32SetReverser;

			private Win32KeyDown win32KeyDown;

			private Win32KeyUp win32KeyUp;

			private Win32HornBlow win32HornBlow;

			private Win32DoorOpen win32DoorOpen;

			private Win32DoorClose win32DoorClose;

			private Win32SetSignal win32SetSignal;

			private Win32SetBeaconData win32SetBeaconData;

			#endregion

			private readonly int[] lastAspects;

			private LoadProperties loadProperties;

			private int[] soundInstructions;

			private int[] lastSoundInstructions;

			private SoundHandle[] soundHandles;

			internal Win32Runtime(string pluginFilePath, int[] lastAspects)
			{
				this.pluginFilePath = pluginFilePath;
				dllHandle = IntPtr.Zero;
				this.lastAspects = lastAspects;

				LoadDll(true);
			}

			private void LoadDll(bool retry)
			{
				try
				{
					dllHandle = NativeMethods.LoadLibraryW(pluginFilePath);
				}
				catch
				{
					// Blank try / catch in case our library will only load with ANSI & crashes with Unicode
					// This seems to be the case with some specific versions of OS_ATS1.dll
					// These were likely compiled on XP with Bloodshed Dev C++ (??)
				}

				if (dllHandle == IntPtr.Zero)
				{
					try
					{
						dllHandle = NativeMethods.LoadLibraryA(pluginFilePath);
					}
					catch
					{
						// Presumably both Unicode and ANSI crashed
					}

					if (dllHandle == IntPtr.Zero)
					{
						if (!retry)
						{
							throw new Win32Exception(Marshal.GetLastWin32Error());
						}

						/*
						 * Win32 plugin loading is unreliable on some systems
						 * Unable to reproduce this, but let's try a sleep & single retry attempt
						 * */
						Thread.Sleep(100);
						LoadDll(false);
						return;
					}
				}

				win32Load = GetDelegate<Win32Load>("Load");
				win32Dispose = GetDelegate<Win32Dispose>("Dispose");
				win32GetPluginVersion = GetDelegate<Win32GetPluginVersion>("GetPluginVersion");
				win32SetVehicleSpec = GetDelegate<Win32SetVehicleSpec>("SetVehicleSpec");
				win32Initialize = GetDelegate<Win32Initialize>("Initialize");
				win32Elapse = GetDelegate<Win32Elapse>("Elapse");
				win32SetPower = GetDelegate<Win32SetPower>("SetPower");
				win32SetBrake = GetDelegate<Win32SetBrake>("SetBrake");
				win32SetReverser = GetDelegate<Win32SetReverser>("SetReverser");
				win32KeyDown = GetDelegate<Win32KeyDown>("KeyDown");
				win32KeyUp = GetDelegate<Win32KeyUp>("KeyUp");
				win32HornBlow = GetDelegate<Win32HornBlow>("HornBlow");
				win32DoorOpen = GetDelegate<Win32DoorOpen>("DoorOpen");
				win32DoorClose = GetDelegate<Win32DoorClose>("DoorClose");
				win32SetSignal = GetDelegate<Win32SetSignal>("SetSignal");
				win32SetBeaconData = GetDelegate<Win32SetBeaconData>("SetBeaconData");
			}

			private T GetDelegate<T>(string lpProcName) where T : Delegate
			{
				IntPtr functionHandle = NativeMethods.GetProcAddress(dllHandle, lpProcName);

				if (functionHandle == IntPtr.Zero)
				{
					return null;
				}

				return Marshal.GetDelegateForFunctionPointer<T>(functionHandle);
			}

			public bool Load(LoadProperties properties)
			{
				loadProperties = properties;
				loadProperties.Panel = new int[256];
				soundInstructions = new int[256];
				lastSoundInstructions = new int[256];
				soundHandles = new SoundHandle[256];

				win32Load?.Invoke();

				if (win32GetPluginVersion != null && win32GetPluginVersion() == PLUGIN_VERSION)
				{
					return true;
				}

				if (win32GetPluginVersion == null && Path.GetFileName(pluginFilePath).ToLowerInvariant() == "ats2.dll")
				{
					return true;
				}

				Unload();
				return false;
			}

			public void Unload()
			{
				win32Dispose?.Invoke();
			}

			public void SetVehicleSpecs(VehicleSpecs specs)
			{
				win32SetVehicleSpec?.Invoke(
					new Win32VehicleSpec
					{
						BrakeNotches = specs.BrakeNotches,
						PowerNotches = specs.PowerNotches,
						AtsNotch = specs.AtsNotch,
						B67Notch = specs.B67Notch,
						Cars = specs.Cars
					}
				);
			}

			public void Initialize(InitializationModes mode)
			{
				win32Initialize?.Invoke((InitialHandlePosition)((int)mode + 1));
			}

			public void Elapse(ElapseData data)
			{
				if (win32Elapse == null)
				{
					return;
				}

				double time = data.TotalTime.Milliseconds;

				Win32Handles win32Handles = win32Elapse(
					new Win32VehicleState
					{
						Location = data.Vehicle.Location,
						Speed = (float)data.Vehicle.Speed.KilometersPerHour,
						Time = (int)Math.Floor(time - 2073600000.0 * Math.Floor(time / 2073600000.0)),
						BcPressure = (float)data.Vehicle.BcPressure,
						MrPressure = (float)data.Vehicle.MrPressure,
						ErPressure = (float)data.Vehicle.ErPressure,
						BpPressure = (float)data.Vehicle.BpPressure,
						SapPressure = (float)data.Vehicle.SapPressure,
						Current = 0.0f
					},
					loadProperties.Panel,
					soundInstructions
				);

				data.Handles.BrakeNotch = win32Handles.Brake;
				data.Handles.PowerNotch = win32Handles.Power;
				data.Handles.Reverser = win32Handles.Reverser;

				switch (win32Handles.ConstantSpeed)
				{
					case ConstSpeedInstruction.Continue:
						// Keep the last state.
						break;
					case ConstSpeedInstruction.Enable:
						data.Handles.ConstSpeed = true;
						break;
					case ConstSpeedInstruction.Disable:
						data.Handles.ConstSpeed = false;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				/*
				 * Process the sound instructions
				 * */
				for (int i = 0; i < soundInstructions.Length; i++)
				{
					if (soundInstructions[i] == lastSoundInstructions[i])
					{
						if (soundInstructions[i] < SoundInstruction.Stop || soundInstructions[i] > SoundInstruction.Continue)
						{
							throw new InvalidOperationException($"The value {soundInstructions[i]} of sound instruction {i} is out of range.");
						}

						continue;
					}

					switch (soundInstructions[i])
					{
						case SoundInstruction.Stop:
							soundHandles[i]?.Stop();
							break;
						case SoundInstruction.Play:
							soundHandles[i] = loadProperties.PlaySound(i, 1.0, 1.0, false);
							break;
						case SoundInstruction.Continue:
							// Keep the last state.
							break;
						default:
							if (soundInstructions[i] > SoundInstruction.Stop && soundInstructions[i] <= SoundInstruction.PlayLooping)
							{
								double volume = (soundInstructions[i] - SoundInstruction.Stop) / (double)(SoundInstruction.PlayLooping - SoundInstruction.Stop);

								if (soundHandles[i] == null || soundHandles[i].Stopped)
								{
									soundHandles[i] = loadProperties.PlaySound(i, volume, 1.0, true);
								}
								else
								{
									soundHandles[i].Volume = volume;
								}
							}
							else
							{
								throw new InvalidOperationException($"The value {soundInstructions[i]} of sound instruction {i} is out of range.");
							}
							break;
					}

					lastSoundInstructions[i] = soundInstructions[i];
				}
			}

			public void SetReverser(int reverser)
			{
				win32SetReverser?.Invoke(reverser);
			}

			public void SetPower(int powerNotch)
			{
				win32SetPower?.Invoke(powerNotch);
			}

			public void SetBrake(int brakeNotch)
			{
				win32SetBrake?.Invoke(brakeNotch);
			}

			public void KeyDown(VirtualKeys key)
			{
				win32KeyDown?.Invoke(key);
			}

			public void KeyUp(VirtualKeys key)
			{
				win32KeyUp?.Invoke(key);
			}

			public void HornBlow(HornTypes type)
			{
				win32HornBlow?.Invoke((Win32HornType)((int)type - 1));
			}

			public void DoorChange(DoorStates oldState, DoorStates newState)
			{
				if (oldState == DoorStates.None && newState != DoorStates.None)
				{
					win32DoorOpen?.Invoke();
				}
				else if (oldState != DoorStates.None && newState == DoorStates.None)
				{
					win32DoorClose?.Invoke();
				}
			}

			public void SetSignal(SignalData[] data)
			{
				if (lastAspects.Length == 0 || data[0].Aspect != lastAspects[0])
				{
					win32SetSignal?.Invoke(data[0].Aspect);
				}
			}

			public void SetBeacon(BeaconData data)
			{
				win32SetBeaconData?.Invoke(
					new Win32BeaconData
					{
						Type = data.Type,
						Signal = data.Signal.Aspect,
						Distance = (float)data.Signal.Distance,
						Optional = data.Optional
					}
				);
			}

			public void PerformAI(AIData data)
			{
			}

			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			private void Dispose(bool disposing)
			{
				if (disposed)
				{
					return;
				}

				if (disposing)
				{
					// managed resources
				}

				if (dllHandle != IntPtr.Zero)
				{
					NativeMethods.FreeLibrary(dllHandle);
					dllHandle = IntPtr.Zero;
				}

				disposed = true;
			}

			~Win32Runtime()
			{
				Dispose(false);
			}
		}
	}
}
