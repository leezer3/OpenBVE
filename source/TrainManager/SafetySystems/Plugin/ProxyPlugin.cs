using System.Runtime.InteropServices;
using OpenBveApi.Runtime;
using SoundManager;
using TrainManager.Trains;

namespace TrainManager.SafetySystems {
	/// <summary>Represents a legacy Win32 plugin.</summary>
	internal class ProxyPlugin : Plugin {
		
		private static class SoundInstructions {
			internal const int Stop = -10000;
			internal const int PlayLooping = 0;
			internal const int PlayOnce = 1;
			internal const int Continue = 2;
		}
		
		// --- members ---
		private readonly string PluginFile;
		private int[] Sound;
		private readonly int[] LastSound;
		private GCHandle PanelHandle;
		private GCHandle SoundHandle;

		private readonly Win32ProxyPlugin pluginProxy = new Win32ProxyPlugin();

		// --- constructors ---
		internal ProxyPlugin(string pluginFile, TrainBase train) {
			base.PluginTitle = System.IO.Path.GetFileName(pluginFile);
			//Load the plugin via the proxy callback
			pluginProxy.setPluginFile(pluginFile);
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
			this.PluginFile = pluginFile;		
			this.Sound = new int[256];
			this.LastSound = new int[256];
			this.PanelHandle = new GCHandle();
			this.SoundHandle = new GCHandle();
		}

		
		
		// --- functions ---
		public override bool Load(VehicleSpecs specs, InitializationModes mode)
		{
			if (pluginProxy.load(specs, mode))
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
			pluginProxy.unload();
		}
		public override void BeginJump(InitializationModes mode) {
			pluginProxy.beginJump(mode);
		}

		public override void EndJump()
		{
			//EndJump is not relevant to legacy plugins, but we must implement it as an API member
		}

		protected override void Elapse(ref ElapseData data)
		{
			if (pluginProxy.externalCrashed)
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
			if (pluginProxy.callback.lastError != string.Empty)
			{
				
				//TrainManagerBase.currentHost.A("ERROR: The proxy plugin " + PluginFile + " generated the following error:");
				//Program.FileSystem.AppendToLogFile(pluginProxy.callback.lastError);
				pluginProxy.callback.lastError = string.Empty;
			}

			if (pluginProxy.callback.Unload == true)
			{
				return;
			}
			ElapseProxy e = new ElapseProxy(data, this.Panel, this.Sound);
			ElapseProxy proxyData = pluginProxy.elapse(e);
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

			data = proxyData.Data;
		}

		protected override void SetReverser(int reverser) {
			pluginProxy.setReverser(reverser);
		}

		protected override void SetPower(int powerNotch) {
			pluginProxy.setPowerNotch(powerNotch);
		}

		protected override void SetBrake(int brakeNotch) {
			pluginProxy.setBrake(brakeNotch);
		}
		public override void KeyDown(VirtualKeys key) {
			pluginProxy.keyDown(key);
		}
		public override void KeyUp(VirtualKeys key) {
			pluginProxy.keyUp(key);
		}
		public override void HornBlow(HornTypes type) {
			pluginProxy.hornBlow(type);
		}
		public override void DoorChange(DoorStates oldState, DoorStates newState) {
			pluginProxy.doorChange(oldState, newState);
		}

		protected override void SetSignal(SignalData[] signal) {
			if (base.LastAspects.Length == 0 || signal[0].Aspect != base.LastAspects[0])
			{
				pluginProxy.setSignal(signal[0].Aspect);
			}
		}

		protected override void SetBeacon(BeaconData beacon) {
			pluginProxy.setBeacon(beacon);
		}

		protected override void PerformAI(AIData data)
		{
			//PerformAI is not relevant to legacy plugins, but we must implement it as an API member
		}
		
	}
}
