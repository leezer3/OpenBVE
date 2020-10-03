using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Runtime;
using OpenBveApi.Sounds;
using RouteManager2.MessageManager;
using SoundManager;
using Path = System.IO.Path;
using SoundHandle = OpenBveApi.Runtime.SoundHandle;

namespace OpenBve
{
	internal partial class PluginManager
	{
		[JsonObject(MemberSerialization.OptIn)]
		private abstract class Map<T>
		{
			[JsonObject(MemberSerialization.OptIn)]
			protected class Entry
			{
				[JsonProperty("src", Required = Required.Always)]
				internal T Src
				{
					get;
					set;
				}

				[JsonProperty("dest", Required = Required.Always)]
				internal T Dest
				{
					get;
					set;
				}
			}

			[JsonProperty("entries")]
			protected List<Entry> Entries
			{
				get;
				set;
			}

			protected Map()
			{
				Entries = new List<Entry>();
			}
		}

		[JsonObject(MemberSerialization.OptIn)]
		private class IndexMap : Map<int>
		{
			[JsonProperty("offset")]
			private int Offset
			{
				get;
				set;
			}

			internal (int[] dest, bool listed) ConvertToDest(int src)
			{
				int[] destArray = Entries
					.Where(x => x.Src == src)
					.Select(x => x.Dest)
					.Distinct()
					.ToArray();

				if (destArray.Length != 0)
				{
					return (destArray.Where(x => x >= 0).ToArray(), true);
				}

				int dest = Offset + src;
				return (dest >= 0 ? new[] { dest } : new int[0], false);
			}

			internal bool TryConvertToSrc(int dest, out int src)
			{
				for (int i = Entries.Count - 1; i >= 0; i--)
				{
					if (Entries[i].Dest != dest)
					{
						continue;
					}

					src = Entries[i].Src;
					return src >= 0;
				}

				src = dest - Offset;
				return src >= 0;
			}
		}

		[JsonObject(MemberSerialization.OptIn)]
		private class KeyMap : Map<string>
		{
			internal (VirtualKeys[] dest, bool listed) ConvertToDest(VirtualKeys src)
			{
				string[] destArray = Entries
					.Where(x => x.Src.Equals(src.ToString()))
					.Select(x => x.Dest)
					.Distinct()
					.ToArray();

				if (destArray.Length != 0)
				{
					return (
						destArray
							.Where(x => !x.Equals("None"))
							.Select(x => (VirtualKeys)Enum.Parse(typeof(VirtualKeys), x))
							.ToArray(),
						true
					);
				}

				return (new[] { src }, false);
			}

			internal bool TryConvertToSrc(VirtualKeys dest, out VirtualKeys src)
			{
				for (int i = Entries.Count - 1; i >= 0; i--)
				{
					if (!Entries[i].Dest.Equals(dest.ToString()))
					{
						continue;
					}

					return Enum.TryParse(Entries[i].Src, out src);
				}

				src = dest;
				return true;
			}
		}

		private class Plugin
		{
			[JsonObject(MemberSerialization.OptIn)]
			internal class Entry
			{
				[JsonProperty("filePath", Required = Required.Always)]
				internal string FilePath
				{
					get;
					set;
				}

				[JsonProperty("panelIndexMap")]
				internal IndexMap PanelIndexMap
				{
					get;
					set;
				}

				[JsonProperty("soundIndexMap")]
				internal IndexMap SoundIndexMap
				{
					get;
					set;
				}

				[JsonProperty("keyMap")]
				internal KeyMap KeyMap
				{
					get;
					set;
				}

				internal Entry()
				{
					PanelIndexMap = new IndexMap();
					SoundIndexMap = new IndexMap();
					KeyMap = new KeyMap();
				}
			}

			private class SoundHandleEx : SoundHandle
			{
				internal readonly SoundSource[] Sources;

				internal SoundHandleEx(double volume, double pitch, SoundSource[] sources)
				{
					MyVolume = volume;
					MyPitch = pitch;
					MyValid = true;
					Sources = sources;
				}

				internal new void Stop()
				{
					MyValid = false;

					foreach (SoundSource source in Sources)
					{
						source.Stop();
					}
				}
			}

			private readonly PluginManager manager;

			private readonly Entry entry;

			private readonly IRuntime api;          //.NET&C++

			/// <summary>The file title of the plugin, including the file extension.</summary>
			internal readonly string Title;

			/// <summary>The array of panel variables.</summary>
			internal int[] Panel
			{
				get;
				private set;
			}

			internal int[] LastPanel
			{
				get;
				private set;
			}

			/// <summary>The last reverser reported to the plugin.</summary>
			private int lastReverser;

			/// <summary>The last power notch reported to the plugin.</summary>
			private int lastPowerNotch;

			/// <summary>The last brake notch reported to the plugin.</summary>
			private int lastBrakeNotch;

			private Dictionary<int, int[]> soundIndexDictionary;

			/// <summary>An array containing all of the plugin's current sound handles</summary>
			private SoundHandleEx[] soundHandles;

			/// <summary>The total number of sound handles currently in use</summary>
			private int soundHandlesCount;

			/// <summary>Whether the plugin supports the AI.</summary>
			internal bool SupportsAI
			{
				get;
				private set;
			}

			/// <summary>The last exception the plugin raised.</summary>
			internal Exception LastException
			{
				get;
				// ReSharper disable once UnusedAutoPropertyAccessor.Local
				private set;
			}

			internal Plugin(PluginManager manager, Entry entry, IRuntime api)
			{
				this.manager = manager;
				this.entry = entry;
				this.api = api;
				Title = Path.GetFileName(entry.FilePath);
				soundIndexDictionary = new Dictionary<int, int[]>();
				soundHandles = new SoundHandleEx[16];
			}

			/// <summary>Called to load and initialize the plugin.</summary>
			/// <param name="specs">The train specifications.</param>
			/// <param name="mode">The initialization mode of the train.</param>
			/// <returns>Whether loading the plugin was successful.</returns>
			internal bool Load(VehicleSpecs specs, InitializationModes mode)
			{
				LoadProperties properties = new LoadProperties(Path.GetDirectoryName(entry.FilePath), manager.train.TrainFolder, PlaySound, PlaySound, AddInterfaceMessage, AddScore);

				bool success;
				try
				{
					success = api.Load(properties);
				}
				catch (Exception ex)
				{
					if (ex is ThreadStateException)
					{
						// TTC plugin, broken when multi-threading is used
						success = false;
						properties.FailureReason = "This plugin does not function correctly with current versions of openBVE. Please ask the plugin developer to fix this.";
					}
					else
					{
						success = false;
						properties.FailureReason = ex.Message;
					}
				}

				if (!success)
				{
					if (properties.FailureReason != null)
					{
						Interface.AddMessage(MessageType.Error, false, $"The train plugin {Title} failed to load for the following reason: {properties.FailureReason}");
					}
					else
					{
						Interface.AddMessage(MessageType.Error, false, $"The train plugin {Title} failed to load for an unspecified reason.");
					}

					return false;
				}

				lastReverser = -2;
				lastPowerNotch = -1;
				lastBrakeNotch = -1;
				Panel = properties.Panel ?? new int[0];
				LastPanel = new int[Panel.Length];
				CreateSoundIndexDictionary();
				SupportsAI = properties.AISupport == AISupport.Basic;

#if !DEBUG
				try
#endif
				{
					api.SetVehicleSpecs(specs);
					api.Initialize(mode);
				}
#if !DEBUG
				catch (Exception ex)
				{
					LastException = ex;
				}
#endif

				return true;
			}

			/// <summary>Called to unload the plugin.</summary>
			internal void Unload()
			{
#if !DEBUG
				try
#endif
				{
					api.Unload();
				}
#if !DEBUG
				catch (Exception ex)
				{
					LastException = ex;
				}
#endif
			}

			/// <summary>Called before the train jumps to a different location.</summary>
			/// <param name="mode">The initialization mode of the train.</param>
			internal void BeginJump(InitializationModes mode)
			{
#if !DEBUG
				try
#endif
				{
					api.Initialize(mode);
				}
#if !DEBUG
				catch (Exception ex)
				{
					LastException = ex;
				}
#endif
			}

			/// <summary>Called when the train has finished jumping to a different location.</summary>
			internal void EndJump()
			{
			}

			/// <summary>Called every frame to update the plugin.</summary>
			/// <param name="data">The data passed to the plugin on Elapse.</param>
			/// <remarks>This function should not be called directly. Call UpdatePlugin instead.</remarks>
			internal void Elapse(ElapseData data)
			{
				for (int i = 0; i < Panel.Length; i++)
				{
					LastPanel[i] = Panel[i];
				}

#if !DEBUG
				try
#endif
				{
					api.Elapse(data);
				}
#if !DEBUG
				catch (Exception ex)
				{
					LastException = ex;
				}
#endif

				for (int i = 0; i < soundHandlesCount; i++)
				{
					if (soundHandles[i].Stopped | soundHandles[i].Sources.All(x => x.State == SoundSourceState.Stopped))
					{
						soundHandles[i].Stop();
						soundHandles[i] = soundHandles[soundHandlesCount - 1];
						soundHandlesCount--;
						i--;
					}
					else
					{
						double pitch = Math.Max(0.01, soundHandles[i].Pitch);
						double volume = Math.Max(0.0, soundHandles[i].Volume);

						foreach (SoundSource source in soundHandles[i].Sources)
						{
							source.Pitch = pitch;
							source.Volume = volume;
						}
					}
				}
			}

			/// <summary>Called to indicate a change of the reverser.</summary>
			/// <param name="reverser">The reverser.</param>
			/// <remarks>This function should not be called directly. Call UpdateReverser instead.</remarks>
			internal void SetReverser(int reverser)
			{
				if (reverser == lastReverser)
				{
					return;
				}

#if !DEBUG
				try
#endif
				{
					api.SetReverser(reverser);
				}
#if !DEBUG
				catch (Exception ex)
				{
					LastException = ex;
				}
#endif

				lastReverser = reverser;
			}

			/// <summary>Called to indicate a change of the power notch.</summary>
			/// <param name="powerNotch">The power notch.</param>
			/// <remarks>This function should not be called directly. Call UpdatePower instead.</remarks>
			internal void SetPower(int powerNotch)
			{
				if (powerNotch == lastPowerNotch)
				{
					return;
				}

#if !DEBUG
				try
#endif
				{
					api.SetPower(powerNotch);
				}
#if !DEBUG
				catch (Exception ex)
				{
					LastException = ex;
				}
#endif

				lastPowerNotch = powerNotch;
			}

			/// <summary>Called to indicate a change of the brake notch.</summary>
			/// <param name="brakeNotch">The brake notch.</param>
			/// <remarks>This function should not be called directly. Call UpdateBrake instead.</remarks>
			internal void SetBrake(int brakeNotch)
			{
				if (brakeNotch == lastBrakeNotch)
				{
					return;
				}

#if !DEBUG
				try
#endif
				{
					api.SetBrake(brakeNotch);
				}
#if !DEBUG
				catch (Exception ex)
				{
					LastException = ex;
				}
#endif

				lastBrakeNotch = brakeNotch;
			}

			/// <summary>Called when a virtual key is pressed.</summary>
			/// <param name="key">The virtual key that was pressed.</param>
			internal void KeyDown(VirtualKeys key)
			{
#if !DEBUG
				try
#endif
				{
					if (entry.KeyMap.TryConvertToSrc(key, out VirtualKeys convertedKey))
					{
						api.KeyDown(convertedKey);
					}
				}
#if !DEBUG
				catch (Exception ex)
				{
					LastException = ex;
				}
#endif
			}

			/// <summary>Called when a virtual key is released.</summary>
			/// <param name="key">The virtual key that was released.</param>
			internal void KeyUp(VirtualKeys key)
			{
#if !DEBUG
				try
#endif
				{
					if (entry.KeyMap.TryConvertToSrc(key, out VirtualKeys convertedKey))
					{
						api.KeyUp(convertedKey);
					}
				}
#if !DEBUG
				catch (Exception ex)
				{
					LastException = ex;
				}
#endif
			}

			/// <summary>Called when a horn is played or stopped.</summary>
			/// <param name="type">The type of horn.</param>
			internal void HornBlow(HornTypes type)
			{
#if !DEBUG
				try
#endif
				{
					api.HornBlow(type);
				}
#if !DEBUG
				catch (Exception ex)
				{
					LastException = ex;
				}
#endif
			}

			/// <summary>Called when the state of the doors changes.</summary>
			/// <param name="oldState">The old state of the doors.</param>
			/// <param name="newState">The new state of the doors.</param>
			internal void DoorChange(DoorStates oldState, DoorStates newState)
			{
#if !DEBUG
				try
#endif
				{
					api.DoorChange(oldState, newState);
				}
#if !DEBUG
				catch (Exception ex)
				{
					LastException = ex;
				}
#endif
			}

			/// <summary>Is called when the aspect in the current or any of the upcoming sections changes.</summary>
			/// <param name="signal">Signal information per section. In the array, index 0 is the current section, index 1 the upcoming section, and so on.</param>
			/// <remarks>This function should not be called directly. Call UpdateSignal instead.</remarks>
			internal void SetSignal(SignalData[] signal)
			{
#if !DEBUG
				try
#endif
				{
					api.SetSignal(signal);
				}
#if !DEBUG
				catch (Exception ex)
				{
					LastException = ex;
				}
#endif
			}

			/// <summary>Called when the train passes a beacon.</summary>
			/// <param name="beacon">The beacon data.</param>
			/// <remarks>This function should not be called directly. Call UpdateBeacon instead.</remarks>
			internal void SetBeacon(BeaconData beacon)
			{
#if !DEBUG
				try
#endif
				{
					api.SetBeacon(beacon);
				}
#if !DEBUG
				catch (Exception ex)
				{
					LastException = ex;
				}
#endif
			}

			/// <summary>Called when the AI should be performed.</summary>
			/// <param name="data">The AI data.</param>
			/// <remarks>This function should not be called directly. Call UpdateAI instead.</remarks>
			internal void PerformAI(AIData data)
			{
#if !DEBUG
				try
#endif
				{
					api.PerformAI(data);
				}
#if !DEBUG
				catch (Exception ex)
				{
					LastException = ex;
				}
#endif
			}

			internal Dictionary<int, int> GetPanelIndexDictionary()
			{
				Dictionary<int, (int srcIndex, bool listed)> dictionary = new Dictionary<int, (int, bool)>();

				for (int srcIndex = 0; srcIndex < Panel.Length; srcIndex++)
				{
					(int[] destIndices, bool listed) = entry.PanelIndexMap.ConvertToDest(srcIndex);

					foreach (int destIndex in destIndices)
					{
						if (dictionary.ContainsKey(destIndex))
						{
							if (dictionary[destIndex].listed)
							{
								continue;
							}

							dictionary[destIndex] = (srcIndex, listed);
						}
						else
						{
							dictionary.Add(destIndex, (srcIndex, listed));
						}
					}
				}

				return dictionary.ToDictionary(x => x.Key, x => x.Value.srcIndex);
			}

			private void CreateSoundIndexDictionary()
			{
				soundIndexDictionary = Enumerable.Range(0, manager.train.Cars[manager.train.DriverCar].Sounds.Plugin.Length)
					.Select(x => entry.SoundIndexMap.TryConvertToSrc(x, out int y) ? y : -1)
					.Select((x, i) => new { SrcIndex = x, DestIndex = i })
					.Where(x => x.SrcIndex >= 0)
					.GroupBy(x => x.SrcIndex, x => x.DestIndex, (x, y) => new { Key = x, Value = y.ToArray() })
					.ToDictionary(x => x.Key, x => x.Value);
			}

			/// <summary>May be called from a .Net plugin, in order to play a sound from the driver's car of a train</summary>
			/// <param name="index">The plugin-based of the sound to play</param>
			/// <param name="volume">The volume of the sound- A volume of 1.0 represents nominal volume</param>
			/// <param name="pitch">The pitch of the sound- A pitch of 1.0 represents nominal pitch</param>
			/// <param name="looped">Whether the sound is looped</param>
			/// <returns>The sound handle, or null if not successful</returns>
			private SoundHandleEx PlaySound(int index, double volume, double pitch, bool looped)
			{
				return PlaySound(index, volume, pitch, looped, manager.train.DriverCar);
			}

			/// <summary>May be called from a .Net plugin, in order to play a sound from a specific car of a train</summary>
			/// <param name="index">The plugin-based of the sound to play</param>
			/// <param name="volume">The volume of the sound- A volume of 1.0 represents nominal volume</param>
			/// <param name="pitch">The pitch of the sound- A pitch of 1.0 represents nominal pitch</param>
			/// <param name="looped">Whether the sound is looped</param>
			/// <param name="carIndex">The index of the car which is to emit the sound</param>
			/// <returns>The sound handle, or null if not successful</returns>
			private SoundHandleEx PlaySound(int index, double volume, double pitch, bool looped, int carIndex)
			{
				if (index < 0 || !soundIndexDictionary.ContainsKey(index) || carIndex >= manager.train.Cars.Length || carIndex < 0)
				{
					return null;
				}

				SoundSource[] sources = soundIndexDictionary[index]
					.Select(x => manager.train.Cars[manager.train.DriverCar].Sounds.Plugin[x])
					.Where(x => x.Buffer != null)
					.Select(x => Program.Sounds.PlaySound(x.Buffer, pitch, volume, x.Position, manager.train.Cars[carIndex], looped))
					.ToArray();

				if (soundHandlesCount == soundHandles.Length)
				{
					Array.Resize(ref soundHandles, soundHandles.Length << 1);
				}
				soundHandles[soundHandlesCount] = new SoundHandleEx(volume, pitch, sources);
				soundHandlesCount++;

				return soundHandles[soundHandlesCount - 1];
			}

			/// <summary>May be called from a .Net plugin, in order to add a message to the in-game display</summary>
			/// <param name="message">The message to display</param>
			/// <param name="color">The color in which to display the message</param>
			/// <param name="time">The time in seconds for which to display the message</param>
			private void AddInterfaceMessage(string message, MessageColor color, double time)
			{
				MessageManager.AddMessage(message, MessageDependency.Plugin, GameMode.Expert, color, Program.CurrentRoute.SecondsSinceMidnight + time, null);
			}

			/// <summary>May be called from a .Net plugin, in order to add a score to the post-game log</summary>
			/// <param name="score">The score to add</param>
			/// <param name="message">The message to display in the post-game log</param>
			/// <param name="color">The color of the in-game message</param>
			/// <param name="timeout">The time in seconds for which to display the in-game message</param>
			private void AddScore(int score, string message, MessageColor color, double timeout)
			{
				Game.CurrentScore.CurrentValue += score;

				int n = Game.ScoreMessages.Length;
				Array.Resize(ref Game.ScoreMessages, n + 1);
				Game.ScoreMessages[n] = new Game.ScoreMessage
				{
					Value = score,
					Color = color,
					RendererPosition = new Vector2(0, 0),
					RendererAlpha = 0.0,
					Text = message,
					Timeout = timeout
				};
			}

		}
	}
}
