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
	/// <summary>Represents a legacy Win32 plugin.</summary>
	internal class ProxyPlugin : Plugin, IAtsPluginCallback {
		
		private static class SoundInstructions {
			internal const int Stop = -10000;
			internal const int PlayLooping = 0;
			internal const int PlayOnce = 1;
			internal const int Continue = 2;
		}
		
		// --- members ---
		private int[] Sound;
		private readonly int[] LastSound;
		private GCHandle PanelHandle;
		private GCHandle SoundHandle;

		private readonly IAtsPluginProxy pipeProxy;
		private string lastError;
		private bool externalCrashed;

		// --- constructors ---
		internal ProxyPlugin(string pluginFile, TrainBase train)
		{
			externalCrashed = false;
			base.PluginTitle = System.IO.Path.GetFileName(pluginFile);
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
			base.PluginValid = true;
			base.PluginMessage = null;
			base.Train = train;
			base.Panel = new int[256];
			base.SupportsAI = false;
			base.LastTime = 0.0;
			base.LastReverser = -2;
			base.LastPowerNotch = -1;
			base.LastBrakeNotch = -1;
			base.LastAspects = new int[] { };
			base.LastSection = -1;
			base.LastException = null;
			this.Sound = new int[256];
			this.LastSound = new int[256];
			this.PanelHandle = new GCHandle();
			this.SoundHandle = new GCHandle();
		}

		[DllImport("User32.dll")]
		public static extern Int32 SetForegroundWindow(int hWnd);
		
		// --- functions ---
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
		public override void Unload() {
			if (PanelHandle.IsAllocated) {
				PanelHandle.Free();
			}
			if (SoundHandle.IsAllocated) {
				SoundHandle.Free();
			}
			pipeProxy.Unload();
		}
		public override void BeginJump(InitializationModes mode) {
			pipeProxy.BeginJump(mode);
		}

		public override void EndJump()
		{
			//EndJump is not relevant to legacy plugins, but we must implement it as an API member
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
				
				//TrainManagerBase.currentHost.A("ERROR: The proxy plugin " + PluginFile + " generated the following error:");
				//Program.FileSystem.AppendToLogFile(pluginProxy.callback.lastError);
				lastError = string.Empty;
			}

			try
			{
				ElapseProxy e = new ElapseProxy(data, this.Panel, this.Sound);
				ElapseProxy proxyData = pipeProxy.Elapse(e);
				this.Panel = proxyData.Panel;
				this.Sound = proxyData.Sound;
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
								SoundBuffer buffer = base.Train.Cars[base.Train.DriverCar].Sounds.Plugin[i].Buffer;
								if (buffer != null)
								{
									double volume = (double) (this.Sound[i] - SoundInstructions.Stop) / (double) (SoundInstructions.PlayLooping - SoundInstructions.Stop);
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
								SoundBuffer buffer = base.Train.Cars[base.Train.DriverCar].Sounds.Plugin[i].Buffer;
								if (buffer != null)
								{
									Train.Cars[Train.DriverCar].Sounds.Plugin[i].Play(1.0, 1.0, Train.Cars[Train.DriverCar], false);
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
				data = proxyData.Data;
			}
			catch
			{
				lastError = externalCrashed.ToString();
				externalCrashed = true;
			}
		}

		protected override void SetReverser(int reverser) {
			pipeProxy.SetReverser(reverser);
		}

		protected override void SetPower(int powerNotch) {
			pipeProxy.SetPowerNotch(powerNotch);
		}

		protected override void SetBrake(int brakeNotch) {
			pipeProxy.SetBrake(brakeNotch);
		}
		public override void KeyDown(VirtualKeys key) {
			pipeProxy.KeyDown(key);
		}
		public override void KeyUp(VirtualKeys key) {
			pipeProxy.KeyUp(key);
		}
		public override void HornBlow(HornTypes type) {
			pipeProxy.HornBlow(type);
		}
		public override void DoorChange(DoorStates oldState, DoorStates newState) {
			pipeProxy.DoorChange(oldState, newState);
		}

		protected override void SetSignal(SignalData[] signal) {
			if (base.LastAspects.Length == 0 || signal[0].Aspect != base.LastAspects[0])
			{
				pipeProxy.SetSignal(signal[0].Aspect);
			}
		}

		protected override void SetBeacon(BeaconData beacon) {
			pipeProxy.SetBeacon(beacon);
		}

		protected override void PerformAI(AIData data)
		{
			//PerformAI is not relevant to legacy plugins, but we must implement it as an API member
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
