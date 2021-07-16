using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ServiceModel;
using OpenBveApi.Hosts;
using OpenBveApi.Interop;
using OpenBveApi.Runtime;
using SoundManager;
using TrainManager.Trains;

namespace TrainManager.SafetySystems {
	/// <summary>Represents a proxied legacy Win32 plugin.</summary>
	internal class ProxyPlugin : Plugin, IAtsPluginCallback 
	{
		/// <summary>The sound instructions on the previous frame</summary>
		private readonly int[] LastSound;
		/// <summary>The plugin proxy interface</summary>
		private readonly IAtsPluginProxy pipeProxy;
		/// <summary>The last error returned by the plugin</summary>
		private string lastError;
		/// <summary>Whether the external plugin proxy has encountered a critical error / crashed</summary>
		private bool externalCrashed;

		internal ProxyPlugin(string pluginFile, TrainBase train)
		{
			externalCrashed = false;
			PluginTitle = System.IO.Path.GetFileName(pluginFile);
			//Load the plugin via the proxy callback
			var handle = Process.GetCurrentProcess().MainWindowHandle;
			try
			{
				var hostProcess = new Process();
				hostProcess.StartInfo.FileName = @"Win32PluginProxy.exe";
				hostProcess.Start();
				HostInterface.Win32PluginHostReady.WaitOne();
				pipeProxy = new DuplexChannelFactory<IAtsPluginProxy>(new InstanceContext(this), new NetNamedPipeBinding(), new EndpointAddress(HostInterface.Win32PluginHostEndpointAddress)).CreateChannel();
				pipeProxy.SetPluginFile(pluginFile, Process.GetCurrentProcess().Id);
				SetForegroundWindow(handle.ToInt32());
			}
			catch
			{
				//That didn't work
				externalCrashed = true;
			}
			PluginValid = true;
			PluginMessage = null;
			Train = train;
			Panel = new int[256];
			SupportsAI = AISupport.None;
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
			LastTime = 0.0;
			LastReverser = -2;
			LastPowerNotch = -1;
			LastBrakeNotch = -1;
			LastAspects = new int[] { };
			LastSection = -1;
			LastException = null;
			Sound = new int[256];
			LastSound = new int[256];
		}

		[DllImport("User32.dll")]
		public static extern Int32 SetForegroundWindow(int hWnd);
		
		public override bool Load(VehicleSpecs specs, InitializationModes mode)
		{
			if (externalCrashed)
			{
				//Most likely the plugin proxy app failed to launch or something
				return false;
			}
			if (pipeProxy.Load(specs, mode))
			{
				UpdatePower();
				UpdateBrake();
				UpdateReverser();
				return true;
			}
			return false;
		}

		public override void Unload()
		{
			pipeProxy.Unload();
		}

		public override void BeginJump(InitializationModes mode)
		{
			pipeProxy.BeginJump(mode);
			if (SupportsAI == AISupport.Program)
			{
				AI.BeginJump(mode);
			}
		}

		public override void EndJump()
		{
			if (SupportsAI == AISupport.Program)
			{
				AI.EndJump();
			}
		}

		protected override void Elapse(ref ElapseData data)
		{
			if (externalCrashed)
			{
				//Yuck
				for (int i = 0; i < Train.Cars[Train.DriverCar].Sounds.Plugin.Length; i++)
				{
					if (Train.Cars[Train.DriverCar].Sounds.Plugin[i].IsPlaying)
					{
						Train.Cars[Train.DriverCar].Sounds.Plugin[i].Stop();
					}
				}
				Train.UnloadPlugin();
				return;
			}
			if (!string.IsNullOrEmpty(lastError))
			{
				
				//TrainManagercurrentHost.A("ERROR: The proxy plugin " + PluginFile + " generated the following error:");
				//Program.FileSystem.AppendToLogFile(pluginProxy.callback.lastError);
				lastError = string.Empty;
			}

			try
			{
				ElapseProxy e = new ElapseProxy(data, Panel, Sound);
				ElapseProxy proxyData = pipeProxy.Elapse(e);
				Panel = proxyData.Panel;
				Sound = proxyData.Sound;
				for (int i = 0; i < Sound.Length; i++)
				{
					if (Sound[i] != LastSound[i])
					{
						if (Sound[i] == SoundInstructions.Stop)
						{
							if (i < Train.Cars[Train.DriverCar].Sounds.Plugin.Length)
							{
								Train.Cars[Train.DriverCar].Sounds.Plugin[i].Stop();
							}
						}
						else if (Sound[i] > SoundInstructions.Stop & Sound[i] <= SoundInstructions.PlayLooping)
						{
							if (i < Train.Cars[Train.DriverCar].Sounds.Plugin.Length)
							{
								SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.Plugin[i].Buffer;
								if (buffer != null)
								{
									double volume = (double) (Sound[i] - SoundInstructions.Stop) / (SoundInstructions.PlayLooping - SoundInstructions.Stop);
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
						else if (Sound[i] == SoundInstructions.PlayOnce)
						{
							if (i < Train.Cars[Train.DriverCar].Sounds.Plugin.Length)
							{
								SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.Plugin[i].Buffer;
								if (buffer != null)
								{
									Train.Cars[Train.DriverCar].Sounds.Plugin[i].Play(1.0, 1.0, Train.Cars[Train.DriverCar], false);
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
				data = proxyData.Data;
			}
			catch
			{
				lastError = externalCrashed.ToString();
				externalCrashed = true;
			}
		}

		protected override void SetReverser(int reverser)
		{
			pipeProxy.SetReverser(reverser);
		}

		protected override void SetPower(int powerNotch)
		{
			pipeProxy.SetPowerNotch(powerNotch);
		}

		protected override void SetBrake(int brakeNotch)
		{
			pipeProxy.SetBrake(brakeNotch);
		}

		public override void KeyDown(VirtualKeys key)
		{
			pipeProxy.KeyDown(key);
		}

		public override void KeyUp(VirtualKeys key)
		{
			pipeProxy.KeyUp(key);
		}

		public override void HornBlow(HornTypes type)
		{
			pipeProxy.HornBlow(type);
		}

		public override void DoorChange(DoorStates oldState, DoorStates newState)
		{
			pipeProxy.DoorChange(oldState, newState);
		}

		protected override void SetSignal(SignalData[] signal)
		{
			if (LastAspects.Length == 0 || signal[0].Aspect != LastAspects[0])
			{
				pipeProxy.SetSignal(signal[0].Aspect);
			}
		}

		protected override void SetBeacon(BeaconData beacon)
		{
			if (AI != null)
			{
				AI.SetBeacon(beacon);
			}
			pipeProxy.SetBeacon(beacon);
		}

		protected override void PerformAI(AIData data)
		{
			if (SupportsAI == AISupport.Program)
			{
				AI.Perform(data);
			}
		}

		public void ReportError(string Error, bool Critical = false)
		{
			lastError = Error;
			if (Critical)
			{
				externalCrashed = true;
			}
		}
	}
}
