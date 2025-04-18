﻿//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2023, odaykufan, The OpenBVE Project
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
//
//Please note that this plugin is based upon code originally released into into the public domain by Odaykufan:
//http://web.archive.org/web/20140225072517/http://odakyufan.zxq.net:80/odakyufanats/index.html

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using OpenBveApi.Runtime;

namespace OpenBveAts {
	/// <summary>Represents a train that is simulated by this plugin.</summary>
	internal class Train {
		
		// --- classes and enumerations ---
		
		/// <summary>Represents handles that can only be read from.</summary>
		internal class ReadOnlyHandles {
			// --- properties ---
			/// <summary>Gets or sets the reverser position.</summary>
			internal readonly int Reverser;

			/// <summary>Gets or sets the power notch.</summary>
			internal readonly int PowerNotch;

			/// <summary>Gets or sets the brake notch.</summary>
			internal readonly int BrakeNotch;

			/// <summary>Gets or sets the brake notch.</summary>
			internal readonly int LocoBrakeNotch;
			/// <summary>Gets or sets whether the const speed system is enabled.</summary>
			internal bool ConstSpeed { get; }
			// --- constructors ---
			/// <summary>Creates a new instance of this class.</summary>
			/// <param name="handles">The handles</param>
			internal ReadOnlyHandles(Handles handles) {
				this.Reverser = handles.Reverser;
				this.PowerNotch = handles.PowerNotch;
				this.BrakeNotch = handles.BrakeNotch;
				this.ConstSpeed = handles.ConstSpeed;
				this.LocoBrakeNotch = handles.LocoBrakeNotch;
			}
		}
		
		
		// --- plugin ---
		
		/// <summary>Whether the plugin is currently initializing. This happens in-between Initialize and Elapse calls, for example when jumping to a station from the menu.</summary>
		internal bool PluginInitializing;
		
		
		// --- train ---

		/// <summary>The train specifications.</summary>
		internal VehicleSpecs Specs;
		
		/// <summary>The current state of the train.</summary>
		internal VehicleState State;
		
		/// <summary>The driver handles at the last Elapse call.</summary>
		internal ReadOnlyHandles Handles;
		
		/// <summary>The current state of the doors.</summary>
		internal DoorStates Doors;

		
		// --- panel and sound ---
		
		/// <summary>The panel variables.</summary>
		internal readonly int[] Panel;

		/// <summary>The sounds used on this train.</summary>
		internal readonly Sounds Sounds;
		
		
		// --- AI ---
		
		/// <summary>The AI component that drives the train.</summary>
		internal readonly AI AI;
		
		
		// --- devices ---
		
		/// <summary>The ATS-Sx device, or a null reference if not installed.</summary>
		internal AtsSx AtsSx;
		
		/// <summary>The ATS-P device, or a null reference if not installed.</summary>
		internal AtsP AtsP;
		
		/// <summary>The ATC device, or a null reference if not installed.</summary>
		internal Atc Atc;

		/// <summary>The EB device, or a null reference if not installed.</summary>
		internal Eb Eb;

		/// <summary>A list of all the devices installed on this train. The devices must be in the order EB, ATC, ATS-P and ATS-Sx.</summary>
		internal Device[] Devices;
		
		
		// --- constructors ---

		/// <summary>Creates a new train without any devices installed.</summary>
		/// <param name="panel">The array of panel variables.</param>
		/// <param name="playSound">The delegate to play sounds.</param>
		internal Train(int[] panel, PlaySoundDelegate playSound) {
			this.PluginInitializing = false;
			this.Specs = new VehicleSpecs(0, BrakeTypes.ElectromagneticStraightAirBrake, 0, false, false, 0);
			this.State = new VehicleState(0.0, new Speed(0.0), 0.0, 0.0, 0.0, 0.0, 0.0,0.0,0.0,0.0);
			this.Handles = new ReadOnlyHandles(new Handles(0, 0, 0, 0, false));
			this.Doors = DoorStates.None;
			this.Panel = panel;
			this.Sounds = new Sounds(playSound);
			this.AI = new AI(this);
		}
		
		
		// --- functions ---
		
		/// <summary>Sets up the devices from the specified train.dat file.</summary>
		/// <param name="file">The train.dat file.</param>
		/// <returns>Whether loading the train.dat file was successful.</returns>
		internal bool LoadTrainDatFile(string file) {
			if (!File.Exists(file))
			{
				return false;
			}
			string[] lines = File.ReadAllLines(file, Encoding.UTF8);
			for (int i = 0; i < lines.Length; i++) {
				int semicolon = lines[i].IndexOf(';');
				if (semicolon >= 0) {
					lines[i] = lines[i].Substring(0, semicolon).Trim();
				} else {
					lines[i] = lines[i].Trim();
				}
			}
			for (int i = 0; i < lines.Length; i++) {
				if (lines[i].Equals("#DEVICE", StringComparison.OrdinalIgnoreCase)) {
					if (i < lines.Length - 1) {
						int value = int.Parse(lines[i + 1], NumberStyles.Integer, CultureInfo.InvariantCulture);
						if (value == 0) {
							this.AtsSx = new AtsSx(this);
						} else if (value == 1) {
							this.AtsSx = new AtsSx(this);
							this.AtsP = new AtsP(this);
						}
					}
					if (i < lines.Length - 2) {
						int value = int.Parse(lines[i + 2], NumberStyles.Integer, CultureInfo.InvariantCulture);
						if (value == 1) {
							this.Atc = new Atc(this, false);
						} else if (value == 2) {
							this.Atc = new Atc(this, true);
						}
					}
					if (i < lines.Length - 3) {
						int value = int.Parse(lines[i + 3], NumberStyles.Integer, CultureInfo.InvariantCulture);
						if (value == 1) {
							this.Eb = new Eb(this);
						}
					}
				}
			}
			// --- devices ---
			List<Device> devices = new List<Device>();
			if (this.Eb != null) {
				devices.Add(this.Eb);
			}
			if (this.Atc != null) {
				devices.Add(this.Atc);
			}
			if (this.AtsP != null) {
				devices.Add(this.AtsP);
			}
			if (this.AtsSx != null) {
				devices.Add(this.AtsSx);
			}
			this.Devices = devices.ToArray();
			return true;
		}
		
		/// <summary>Is called when the system should initialize.</summary>
		/// <param name="mode">The initialization mode.</param>
		internal void Initialize(InitializationModes mode) {
			this.PluginInitializing = true;
			for (int i = this.Devices.Length - 1; i >= 0; i--) {
				this.Devices[i].Initialize(mode);
			}
		}
		
		/// <summary>Is called every frame.</summary>
		/// <param name="data">The data.</param>
		internal void Elapse(ElapseData data) {
			this.PluginInitializing = false;
			if (data.ElapsedTime.Seconds > 0.0 & data.ElapsedTime.Seconds < 1.0) {
				// --- panel ---
				for (int i = 0; i < this.Panel.Length; i++) {
					this.Panel[i] = 0;
				}
				// --- devices ---
				this.State = data.Vehicle;
				this.Handles = new ReadOnlyHandles(data.Handles);
				bool blocking = false;
				foreach (Device device in this.Devices) {
					device.Elapse(data, ref blocking);
				}
				// --- panel ---
				int seconds = (int)Math.Floor(data.TotalTime.Seconds);
				this.Panel[10] = (seconds / 3600) % 24;
				this.Panel[11] = (seconds / 60) % 60;
				this.Panel[12] = seconds % 60;
				this.Panel[269] = data.Handles.ConstSpeed ? 1 : 0;
				if (data.Handles.Reverser != 0 & (this.Handles.PowerNotch > 0 & this.Handles.BrakeNotch == 0 | this.Handles.PowerNotch == 0 & this.Handles.BrakeNotch == 1 & this.Specs.HasHoldBrake)) {
					this.Panel[100] = 1;
				}
				if (data.Handles.BrakeNotch >= this.Specs.AtsNotch & data.Handles.BrakeNotch <= this.Specs.BrakeNotches | data.Handles.Reverser != 0 & data.Handles.BrakeNotch == 1 & this.Specs.HasHoldBrake) {
					this.Panel[101] = 1;
				}
				// --- sound ---
				this.Sounds.Elapse();
			}
		}
		
		/// <summary>Is called when the driver changes the reverser.</summary>
		/// <param name="reverser">The new reverser position.</param>
		internal void SetReverser(int reverser) {
			foreach (Device device in this.Devices) {
				device.SetReverser(reverser);
			}
		}
		
		/// <summary>Is called when the driver changes the power notch.</summary>
		/// <param name="powerNotch">The new power notch.</param>
		internal void SetPower(int powerNotch) {
			foreach (Device device in this.Devices) {
				device.SetPower(powerNotch);
			}
		}
		
		/// <summary>Is called when the driver changes the brake notch.</summary>
		/// <param name="brakeNotch">The new brake notch.</param>
		internal void SetBrake(int brakeNotch) {
			foreach (Device device in this.Devices) {
				device.SetBrake(brakeNotch);
			}
		}
		
		/// <summary>Is called when a key is pressed.</summary>
		/// <param name="key">The key.</param>
		internal void KeyDown(VirtualKeys key) {
			if (key == VirtualKeys.D) {
				// --- enable safety systems ---
				if (this.AtsSx != null) {
					if (this.AtsSx.State == AtsSx.States.Disabled) {
						this.AtsSx.State = AtsSx.States.Suppressed;
					}
				}
				if (this.AtsP != null) {
					if (this.AtsP.State == AtsP.States.Disabled) {
						this.AtsP.State = AtsP.States.Suppressed;
					}
				}
				if (this.Atc != null) {
					if (this.Atc.State == Atc.States.Disabled) {
						this.Atc.State = Atc.States.Suppressed;
					}
				}
			} else if (key == VirtualKeys.E) {
				// --- disable safety systems ---
				if (this.AtsSx != null) {
					this.AtsSx.State = AtsSx.States.Disabled;
				}
				if (this.AtsP != null) {
					this.AtsP.State = AtsP.States.Disabled;
				}
				if (this.Atc != null) {
					this.Atc.State = Atc.States.Disabled;
				}
			} else {
				// --- other functions ---
				foreach (Device device in this.Devices) {
					device.KeyDown(key);
				}
			}
		}
		
		/// <summary>Is called when a key is released.</summary>
		/// <param name="key">The key.</param>
		internal void KeyUp(VirtualKeys key) {
			foreach (Device device in this.Devices) {
				device.KeyUp(key);
			}
		}
		
		/// <summary>Is called when a horn is played or when the music horn is stopped.</summary>
		/// <param name="type">The type of horn.</param>
		internal void HornBlow(HornTypes type) {
			foreach (Device device in this.Devices) {
				device.HornBlow(type);
			}
		}
		
		/// <summary>Is called to inform about signals.</summary>
		/// <param name="signal">The signal data.</param>
		internal void SetSignal(SignalData[] signal) {
			foreach (Device device in this.Devices) {
				device.SetSignal(signal);
			}
		}
		
		/// <summary>Is called when a beacon is passed.</summary>
		/// <param name="beacon">The beacon data.</param>
		internal void SetBeacon(BeaconData beacon) {
			foreach (Device device in this.Devices) {
				device.SetBeacon(beacon);
			}
		}
		
	}
}
