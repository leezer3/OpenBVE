using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NJsonSchema.Validation;
using OpenBveApi;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Resources;
using OpenBveApi.Runtime;
using OpenBveApi.Trains;
using RouteManager2.Stations;
using Path = System.IO.Path;

namespace OpenBve
{
	internal partial class PluginManager
	{
		private static readonly JsonSchema jsonSchema;

		/// <summary>The train plugins is attached to.</summary>
		private readonly TrainManager.Train train;

		private readonly List<Plugin> plugins;

		/// <summary>The last in-game time reported to plugins.</summary>
		private double lastTime;

		/// <summary>The last aspects per relative section reported to the plugin. Section 0 is the current section, section 1 the upcoming section, and so on.</summary>
		private int[] lastAspects;

		private bool stationsLoaded;

		private readonly List<Station> currentRouteStations;

		private Dictionary<int, (int PluginIndex, int SrcPanelIndex)[]> panelIndexDictionary;

		private Dictionary<int, int> panelValueDictionary;

		/// <summary>The file title of the plugin, including the file extension.</summary>
		internal string Title => string.Join(", ", plugins.Select(x => x.Title));

		internal bool Enable
		{
			get;
			private set;
		}

		/// <summary>Whether the plugin returned valid information in the last Elapse call.</summary>
		internal bool Valid
		{
			get;
			private set;
		}

		/// <summary>The debug message the plugin returned in the last Elapse call.</summary>
		internal string Message
		{
			get;
			private set;
		}

		/// <summary>Whether plugins can disable time acceleration.</summary>
		internal static bool DisableTimeAcceleration
		{
			get;
			private set;
		}

		/// <summary>The last reverser reported to the plugin.</summary>
		internal int LastReverser
		{
			get;
			private set;
		}

		/// <summary>The absolute section the train was last.</summary>
		internal int LastSection;

		internal int PanelLength
		{
			get;
			private set;
		}

		/// <summary>Whether the plugin supports the AI.</summary>
		internal bool SupportsAI
		{
			get;
			private set;
		}

		/// <summary>Whether the plugin is the default ATS/ATC plugin.</summary>
		internal bool IsDefault
		{
			get;
			private set;
		}

		/// <summary>The last exception the plugin raised.</summary>
		internal (string title, Exception exception)[] LastExceptions => plugins.Where(x => x.LastException != null).Select(x => (x.Title, x.LastException)).ToArray();

		static PluginManager()
		{
			jsonSchema = JsonSchema.FromJsonAsync(Schemas.ats).Result;
		}

		internal PluginManager(TrainManager.Train train)
		{
			this.train = train;
			plugins = new List<Plugin>();
			currentRouteStations = new List<Station>();
			panelIndexDictionary = new Dictionary<int, (int, int)[]>();
			panelValueDictionary = new Dictionary<int, int>();
		}

		private VehicleSpecs GetVehicleSpecs()
		{
			BrakeTypes brakeType = (BrakeTypes)train.Cars[train.DriverCar].CarBrake.brakeType;
			int brakeNotches;
			int powerNotches;
			bool hasHoldBrake;
			if (brakeType == BrakeTypes.AutomaticAirBrake)
			{
				brakeNotches = 2;
				powerNotches = train.Handles.Power.MaximumNotch;
				hasHoldBrake = false;
			}
			else
			{
				brakeNotches = train.Handles.Brake.MaximumNotch + (train.Handles.HasHoldBrake ? 1 : 0);
				powerNotches = train.Handles.Power.MaximumNotch;
				hasHoldBrake = train.Handles.HasHoldBrake;
			}

			bool hasLocoBrake = train.Handles.HasLocoBrake;
			int cars = train.Cars.Length;
			return new VehicleSpecs(powerNotches, brakeType, brakeNotches, hasHoldBrake, hasLocoBrake, cars);
		}

		/// <summary>Gets the driver handles.</summary>
		/// <returns>The driver handles.</returns>
		private Handles GetHandles()
		{
			int reverser = (int)train.Handles.Reverser.Driver;
			int powerNotch = train.Handles.Power.Driver;
			int brakeNotch;
			if (train.Handles.Brake is TrainManager.AirBrakeHandle)
			{
				brakeNotch = train.Handles.EmergencyBrake.Driver ? 3 : train.Handles.Brake.Driver == (int)TrainManager.AirBrakeHandleState.Service ? 2 : train.Handles.Brake.Driver == (int)TrainManager.AirBrakeHandleState.Lap ? 1 : 0;
			}
			else
			{
				if (train.Handles.HasHoldBrake)
				{
					brakeNotch = train.Handles.EmergencyBrake.Driver ? train.Handles.Brake.MaximumNotch + 2 : train.Handles.Brake.Driver > 0 ? train.Handles.Brake.Driver + 1 : train.Handles.HoldBrake.Driver ? 1 : 0;
				}
				else
				{
					brakeNotch = train.Handles.EmergencyBrake.Driver ? train.Handles.Brake.MaximumNotch + 1 : train.Handles.Brake.Driver;
				}
			}

			return new Handles(reverser, powerNotch, brakeNotch, train.Handles.LocoBrake.Driver, train.Specs.CurrentConstSpeed, train.Handles.HoldBrake.Driver);
		}

		/// <summary>Sets the driver handles or the virtual handles.</summary>
		/// <param name="handles">The handles.</param>
		/// <param name="virtualHandles">Whether to set the virtual handles.</param>
		private void SetHandles(Handles handles, bool virtualHandles)
		{
			/*
			 * Process the handles.
			 */
			if (train.Handles.SingleHandle & handles.BrakeNotch != 0)
			{
				handles.PowerNotch = 0;
			}

			/*
			 * Process the reverser.
			 */
			if (handles.Reverser >= -1 & handles.Reverser <= 1)
			{
				if (virtualHandles)
				{
					train.Handles.Reverser.Actual = (TrainManager.ReverserPosition)handles.Reverser;
				}
				else
				{
					train.ApplyReverser(handles.Reverser, false);
				}
			}
			else
			{
				if (virtualHandles)
				{
					train.Handles.Reverser.Actual = train.Handles.Reverser.Driver;
				}

				Valid = false;
			}

			/*
			 * Process the power.
			 * */
			if (handles.PowerNotch >= 0 & handles.PowerNotch <= train.Handles.Power.MaximumNotch)
			{
				if (virtualHandles)
				{
					train.Handles.Power.Safety = handles.PowerNotch;
				}
				else
				{
					train.ApplyNotch(handles.PowerNotch, false, 0, true, true);
				}
			}
			else
			{
				if (virtualHandles)
				{
					train.Handles.Power.Safety = train.Handles.Power.Driver;
				}

				Valid = false;
			}

			/*
			 * Process the brakes.
			 * */
			if (virtualHandles)
			{
				train.Handles.EmergencyBrake.Safety = false;
				train.Handles.HoldBrake.Actual = false;
			}
			if (train.Handles.Brake is TrainManager.AirBrakeHandle)
			{
				switch (handles.BrakeNotch)
				{
					case 0 when virtualHandles:
						train.Handles.Brake.Safety = (int)TrainManager.AirBrakeHandleState.Release;
						break;
					case 0:
						train.UnapplyEmergencyBrake();
						train.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Release);
						break;
					case 1 when virtualHandles:
						train.Handles.Brake.Safety = (int)TrainManager.AirBrakeHandleState.Lap;
						break;
					case 1:
						train.UnapplyEmergencyBrake();
						train.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Lap);
						break;
					case 2 when virtualHandles:
						train.Handles.Brake.Safety = (int)TrainManager.AirBrakeHandleState.Service;
						break;
					case 2:
						train.UnapplyEmergencyBrake();
						train.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Release);
						break;
					case 3 when virtualHandles:
						train.Handles.Brake.Safety = (int)TrainManager.AirBrakeHandleState.Service;
						train.Handles.EmergencyBrake.Safety = true;
						break;
					case 3:
						train.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Service);
						train.ApplyEmergencyBrake();
						break;
					default:
						Valid = false;
						break;
				}
			}
			else
			{
				if (train.Handles.HasHoldBrake)
				{
					if (handles.BrakeNotch == train.Handles.Brake.MaximumNotch + 2)
					{
						if (virtualHandles)
						{
							train.Handles.EmergencyBrake.Safety = true;
							train.Handles.Brake.Safety = train.Handles.Brake.MaximumNotch;
						}
						else
						{
							train.ApplyHoldBrake(false);
							train.ApplyNotch(0, true, train.Handles.Brake.MaximumNotch, false, true);
							train.ApplyEmergencyBrake();
						}
					}
					else if (handles.BrakeNotch >= 2 & handles.BrakeNotch <= train.Handles.Brake.MaximumNotch + 1)
					{
						if (virtualHandles)
						{
							train.Handles.Brake.Safety = handles.BrakeNotch - 1;
						}
						else
						{
							train.UnapplyEmergencyBrake();
							train.ApplyHoldBrake(false);
							train.ApplyNotch(0, true, handles.BrakeNotch - 1, false, true);
						}
					}
					else if (handles.BrakeNotch == 1)
					{
						if (virtualHandles)
						{
							train.Handles.Brake.Safety = 0;
							train.Handles.HoldBrake.Actual = true;
						}
						else
						{
							train.UnapplyEmergencyBrake();
							train.ApplyNotch(0, true, 0, false, true);
							train.ApplyHoldBrake(true);
						}
					}
					else if (handles.BrakeNotch == 0)
					{
						if (virtualHandles)
						{
							train.Handles.Brake.Safety = 0;
						}
						else
						{
							train.UnapplyEmergencyBrake();
							train.ApplyNotch(0, true, 0, false, true);
							train.ApplyHoldBrake(false);
						}
					}
					else
					{
						if (virtualHandles)
						{
							train.Handles.Brake.Safety = train.Handles.Brake.Driver;
						}

						Valid = false;
					}
				}
				else
				{
					if (handles.BrakeNotch == train.Handles.Brake.MaximumNotch + 1)
					{
						if (virtualHandles)
						{
							train.Handles.EmergencyBrake.Safety = true;
							train.Handles.Brake.Safety = train.Handles.Brake.MaximumNotch;
						}
						else
						{
							train.ApplyHoldBrake(false);
							train.ApplyEmergencyBrake();
						}
					}
					else if (handles.BrakeNotch >= 0 & handles.BrakeNotch <= train.Handles.Brake.MaximumNotch | train.Handles.Brake.DelayedChanges.Length == 0)
					{
						if (virtualHandles)
						{
							train.Handles.Brake.Safety = handles.BrakeNotch;
						}
						else
						{
							train.UnapplyEmergencyBrake();
							train.ApplyNotch(0, true, handles.BrakeNotch, false, true);
						}
					}
					else
					{
						if (virtualHandles)
						{
							train.Handles.Brake.Safety = train.Handles.Brake.Driver;
						}

						Valid = false;
					}
				}
			}

			/*
			 * Process the const speed system.
			 * */
			train.Specs.CurrentConstSpeed = handles.ConstSpeed & train.Specs.HasConstSpeed;
			train.Handles.HoldBrake.Actual = handles.HoldBrake & train.Handles.HasHoldBrake;
		}

		private bool ParseSetting(Encoding encoding, ICollection<Plugin.Entry> entries)
		{
			string jsonFilePath = OpenBveApi.Path.CombineFile(train.TrainFolder, "ats.json");

			if (File.Exists(jsonFilePath))
			{
				Program.FileSystem.AppendToLogFile($"Loading train plugin json config file: {jsonFilePath}");

				if (ParseSettingJson(jsonFilePath, entries))
				{
					Program.FileSystem.AppendToLogFile("Train plugin json config loaded successfully.");
					return true;
				}

				Program.FileSystem.AppendToLogFile("The ats.json file failed to load. Falling back to text config.");
			}

			string cfgFilePath = OpenBveApi.Path.CombineFile(train.TrainFolder, "ats.cfg");

			if (File.Exists(cfgFilePath))
			{
				Program.FileSystem.AppendToLogFile($"Loading train plugin config file: {cfgFilePath}");

				if (ParseSettingCfg(encoding, cfgFilePath, entries))
				{
					Program.FileSystem.AppendToLogFile("Train plugin text config loaded successfully.");
					return true;
				}

				Program.FileSystem.AppendToLogFile("The ats.cfg file failed to load. Falling back to default plugin.");
			}

			return false;
		}

		private bool ParseSettingJson(string filePath, ICollection<Plugin.Entry> entries)
		{
			JObject jObject;

			try
			{
				jObject = JObject.Parse(File.ReadAllText(filePath));
			}
			catch (Exception ex)
			{
				Interface.AddMessage(MessageType.Error, false, $"Parse error: {ex.Message}");
				return false;
			}

			IEnumerable<ValidationError> validationErrors = jsonSchema.Validate(jObject);

			if (validationErrors.Any())
			{
				DisplayValidationErrors(validationErrors);
				return false;
			}

			foreach (JToken pluginJToken in jObject["plugins"].Children())
			{
				Plugin.Entry entry = JsonConvert.DeserializeObject<Plugin.Entry>(pluginJToken.ToString(), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

				string pluginFilePath;

				try
				{
					pluginFilePath = OpenBveApi.Path.CombineFile(train.TrainFolder, entry.FilePath);
				}
				catch
				{
					Interface.AddMessage(MessageType.Error, true, $"The train plugin path {entry.FilePath} was malformed in {filePath}. This plugin will be ignored.");
					continue;
				}

				if (!File.Exists(pluginFilePath))
				{
					Interface.AddMessage(MessageType.Error, true, $"The train plugin {pluginFilePath} could not be found in {filePath}. This plugin will be ignored.");
					continue;
				}

				entry.FilePath = pluginFilePath;

				entries.Add(entry);
			}

			return true;
		}

		private bool ParseSettingCfg(Encoding encoding, string filePath, ICollection<Plugin.Entry> entries)
		{
			string[] lines = File.ReadAllLines(filePath, TextEncoding.GetSystemEncodingFromFile(filePath, encoding));

			for (int i = 0; i < lines.Length; i++)
			{
				int j = lines[i].IndexOf(';');

				if (j >= 0)
				{
					lines[i] = lines[i].Substring(0, j).Trim();
				}
				else
				{
					lines[i] = lines[i].Trim();
				}
			}

			foreach (string line in lines.Where(x => !string.IsNullOrEmpty(x)))
			{
				string pluginFilePath;

				try
				{
					pluginFilePath = OpenBveApi.Path.CombineFile(train.TrainFolder, line);
				}
				catch
				{
					Interface.AddMessage(MessageType.Error, true, $"The train plugin path {line} was malformed in {filePath}. This plugin will be ignored.");
					continue;
				}

				if (!File.Exists(pluginFilePath))
				{
					Interface.AddMessage(MessageType.Error, true, $"The train plugin {pluginFilePath} could not be found in {filePath}. This plugin will be ignored.");
					continue;
				}

				entries.Add(new Plugin.Entry { FilePath = pluginFilePath });
			}

			if (entries.Any())
			{
				return true;
			}

			return !encoding.Equals(Encoding.UTF8) && ParseSettingCfg(Encoding.UTF8, filePath, entries);
		}

		internal void Load(Encoding encoding, bool forcedDefault)
		{
			/*
			 * Unload plugin if already loaded.
			 * */
			Unload();

			Valid = true;
			lastTime = 0.0;
			LastReverser = -2;
			lastAspects = new int[] { };
			LastSection = -1;
			stationsLoaded = false;
			currentRouteStations.Clear();
			panelIndexDictionary.Clear();

			List<Plugin.Entry> entries = new List<Plugin.Entry>();

			if (!forcedDefault && ParseSetting(encoding, entries))
			{
				foreach (Plugin.Entry entry in entries)
				{
					Program.FileSystem.AppendToLogFile($"Loading train plugin: {entry.FilePath}");
					Plugin plugin = Load(entry);

					if (plugin == null)
					{
						Loading.PluginErrors.Add(Translations.GetInterfaceString("errors_plugin_failure1").Replace("[plugin]", entry.FilePath));
						continue;
					}

					plugins.Add(plugin);
					Program.FileSystem.AppendToLogFile($"Train plugin {plugin.Title} loaded successfully.");
				}
			}

			if (plugins.Count == 0)
			{
				Program.FileSystem.AppendToLogFile("Loading default train plugin.");
				Plugin.Entry entry = new Plugin.Entry { FilePath = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Plugins"), "OpenBveAts.dll") };
				Plugin plugin = Load(entry);

				if (plugin == null)
				{
					Enable = false;
					Loading.PluginErrors.Add(Translations.GetInterfaceString("errors_plugin_failure1").Replace("[plugin]", entry.FilePath));
					return;
				}

				plugins.Add(plugin);
				IsDefault = true;
				Loading.PluginErrors.Add(Translations.GetInterfaceString("errors_plugin_failure2"));
			}

			Enable = true;
			UpdatePower();
			UpdateBrake();
			UpdateReverser();
			CreatePanelIndexDictionary();
			SupportsAI = plugins.Any(x => x.SupportsAI);

			if (!train.IsPlayerTrain)
			{
				return;
			}

			foreach (ITrainInputDevice api in Program.InputDevicePlugin.AvailableInfos.Where(x => x.Status == InputDevicePlugin.Status.Enable).Select(x => x.Api).OfType<ITrainInputDevice>())
			{
				api.SetVehicleSpecs(GetVehicleSpecs());
			}
		}

		private Plugin Load(Plugin.Entry entry)
		{
			if (string.IsNullOrEmpty(entry.FilePath))
			{
				Interface.AddMessage(MessageType.Error, true, "The train plugin file path is not specified.");
				return null;
			}

			string title = Path.GetFileName(entry.FilePath);
			if (!File.Exists(entry.FilePath))
			{
				Interface.AddMessage(MessageType.Error, true, $"The train plugin {title} could not be found.");
				return null;
			}

			/*
			 * Prepare initialization data for the plugin.
			 * */
			VehicleSpecs specs = GetVehicleSpecs();
			InitializationModes mode = (InitializationModes)Interface.CurrentOptions.TrainStart;

			/*
			 * Check if the plugin is a .NET plugin.
			 * */
			IRuntime api;
			Plugin plugin;
			Assembly assembly;
			try
			{
				assembly = Assembly.LoadFile(entry.FilePath);
			}
			catch (BadImageFormatException)
			{
				assembly = null;
			}
			catch (Exception ex)
			{
				Interface.AddMessage(MessageType.Error, false, $"The train plugin {title} could not be loaded due to the following exception: {ex.Message}");
				return null;
			}

			if (assembly != null)
			{
				Type[] types;
				try
				{
					types = assembly.GetTypes();
				}
				catch (ReflectionTypeLoadException ex)
				{
					foreach (Exception e in ex.LoaderExceptions)
					{
						Interface.AddMessage(MessageType.Error, false, $"The train plugin {title} raised an exception on loading: {e.Message}");
					}

					return null;
				}

				foreach (Type type in types)
				{
					if (typeof(IRuntime).IsAssignableFrom(type))
					{
						if (type.FullName == null)
						{
							//Should never happen, but static code inspection suggests that it's possible....
							throw new InvalidOperationException();
						}

						api = assembly.CreateInstance(type.FullName) as IRuntime;
						plugin = new Plugin(this, entry, api);
						return plugin.Load(specs, mode) ? plugin : null;
					}
				}

				Interface.AddMessage(MessageType.Error, false, $"The train plugin {title} does not export a train interface and therefore cannot be used with openBVE.");
				return null;
			}

			/*
			 * Check if the plugin is a Win32 plugin.
			 */
			try
			{
				if (!CheckWin32Header(entry.FilePath))
				{
					Interface.AddMessage(MessageType.Error, false, $"The train plugin {title} is of an unsupported binary format and therefore cannot be used with openBVE.");
					return null;
				}
			}
			catch (Exception ex)
			{
				Interface.AddMessage(MessageType.Error, false, $"The train plugin {title} could not be read due to the following reason: {ex.Message}");
				return null;
			}

			if (Program.CurrentHost.Platform != HostPlatform.MicrosoftWindows | IntPtr.Size != 4)
			{
				Interface.AddMessage(MessageType.Warning, false, $"The train plugin {title} can only be used on 32-bit Microsoft Windows or compatible.");
				return null;
			}

			try
			{
				api = new Win32Runtime(entry.FilePath, lastAspects);
			}
			catch (Win32Exception ex)
			{
				Interface.AddMessage(MessageType.Error, false, $"The train Win32 plugin {title} raised an exception on loading: {ex.Message} (0x{ex.NativeErrorCode:x})");
				return null;
			}

			plugin = new Plugin(this, entry, api);
			if (plugin.Load(specs, mode))
			{
				return plugin;
			}

			Interface.AddMessage(MessageType.Error, false, $"The train plugin {title} does not export a train interface and therefore cannot be used with openBVE.");
			return null;
		}

		internal void Unload()
		{
			foreach (Plugin plugin in plugins)
			{
				plugin.Unload();
			}

			plugins.Clear();
		}

		/// <summary>Called before the train jumps to a different location.</summary>
		/// <param name="mode">The initialization mode of the train.</param>
		internal void BeginJump(InitializationModes mode)
		{
			foreach (Plugin plugin in plugins)
			{
				plugin.BeginJump(mode);
			}
		}

		/// <summary>Called when the train has finished jumping to a different location.</summary>
		internal void EndJump()
		{
			foreach (Plugin plugin in plugins)
			{
				plugin.EndJump();
			}
		}

		/// <summary>Called every frame to update the plugin.</summary>
		internal void UpdatePlugin()
		{
			if (train.Cars == null || train.Cars.Length == 0 || plugins.Count == 0)
			{
				return;
			}

			//If the list of stations has not been loaded, do so
			if (!stationsLoaded)
			{
				foreach (RouteStation selectedStation in Program.CurrentRoute.Stations)
				{
					double stopPosition = -1;
					int stopIdx = selectedStation.GetStopIndex(train.NumberOfCars);

					if (selectedStation.Stops.Length != 0)
					{
						stopPosition = selectedStation.Stops[stopIdx].TrackPosition;
					}

					currentRouteStations.Add(new Station(selectedStation, stopPosition));
				}

				stationsLoaded = true;
			}

			/*
			 * Prepare the vehicle state.
			 * */
			double location = train.Cars[0].FrontAxle.Follower.TrackPosition - train.Cars[0].FrontAxle.Position + 0.5 * train.Cars[0].Length;
			double currentRadius = train.Cars[0].FrontAxle.Follower.CurveRadius;
			double currentCant = train.Cars[0].FrontAxle.Follower.CurveCant;
			double currentPitch = train.Cars[0].FrontAxle.Follower.Pitch;
			double speed = train.Cars[train.DriverCar].Specs.CurrentPerceivedSpeed;
			double bcPressure = train.Cars[train.DriverCar].CarBrake.brakeCylinder.CurrentPressure;
			double mrPressure = train.Cars[train.DriverCar].CarBrake.mainReservoir.CurrentPressure;
			double erPressure = train.Cars[train.DriverCar].CarBrake.equalizingReservoir.CurrentPressure;
			double bpPressure = train.Cars[train.DriverCar].CarBrake.brakePipe.CurrentPressure;
			double sapPressure = train.Cars[train.DriverCar].CarBrake.straightAirPipe.CurrentPressure;
			VehicleState vehicle = new VehicleState(location, new Speed(speed), bcPressure, mrPressure, erPressure, bpPressure, sapPressure, currentRadius, currentCant, currentPitch);

			/*
			 * Prepare the preceding vehicle state.
			 * */
			double bestLocation = double.MaxValue;
			double bestSpeed = 0.0;
			PrecedingVehicleState precedingVehicle;
			try
			{
				foreach (TrainManager.Train otherTrain in TrainManager.Trains)
				{
					if (otherTrain == train || otherTrain.State != TrainState.Available || train.Cars.Length <= 0)
					{
						continue;
					}

					int c = otherTrain.Cars.Length - 1;
					double z = otherTrain.Cars[c].RearAxle.Follower.TrackPosition - otherTrain.Cars[c].RearAxle.Position - 0.5 * otherTrain.Cars[c].Length;

					if (z < location || z >= bestLocation)
					{
						continue;
					}

					bestLocation = z;
					bestSpeed = otherTrain.CurrentSpeed;
				}

				precedingVehicle = bestLocation < double.MaxValue ? new PrecedingVehicleState(bestLocation, bestLocation - location, new Speed(bestSpeed)) : null;
			}
			catch
			{
				precedingVehicle = null;
			}

			/*
			 * Get the driver handles.
			 * */
			Handles handles = GetHandles();

			/*
			 * Update the plugin.
			 * */
			double totalTime = Program.CurrentRoute.SecondsSinceMidnight;
			double elapsedTime = Program.CurrentRoute.SecondsSinceMidnight - lastTime;

			ElapseData data = new ElapseData(vehicle, precedingVehicle, handles, train.SafetySystems.DoorInterlockState, new Time(totalTime), new Time(elapsedTime), currentRouteStations, Program.Renderer.Camera.CurrentMode, Translations.CurrentLanguageCode, train.Destination);
			lastTime = Program.CurrentRoute.SecondsSinceMidnight;

			foreach (Plugin plugin in plugins)
			{
				plugin.SetReverser(handles.Reverser);
				plugin.SetPower(handles.PowerNotch);
				plugin.SetBrake(handles.BrakeNotch);
				plugin.Elapse(data);
			}

			Message = data.DebugMessage;
			train.SafetySystems.DoorInterlockState = data.DoorInterlockState;
			DisableTimeAcceleration = data.DisableTimeAcceleration;

			foreach (var entry in panelIndexDictionary)
			{
				var updateValue = entry.Value
					.Select(x => new { Current = plugins[x.PluginIndex].Panel[x.SrcPanelIndex], Last = plugins[x.PluginIndex].LastPanel[x.SrcPanelIndex] })
					.LastOrDefault(x => x.Current != x.Last);

				if (updateValue == null)
				{
					continue;
				}

				panelValueDictionary[entry.Key] = updateValue.Current;
			}

			/*
			 * Update the input device plugin.
			 * */
			foreach (IInputDevice api in Program.InputDevicePlugin.AvailableInfos.Where(x => x.Status == InputDevicePlugin.Status.Enable).Select(x => x.Api))
			{
				api.SetElapseData(data);
			}

			/*
			 * Set the virtual handles.
			 * */
			Valid = true;
			SetHandles(data.Handles, true);
			train.Destination = data.Destination;
		}

		/// <summary>Called to update the reverser. This invokes a call to SetReverser only if a change actually occurred.</summary>
		internal void UpdateReverser()
		{
			int reverser = (int)train.Handles.Reverser.Driver;

			foreach (Plugin plugin in plugins)
			{
				plugin.SetReverser(reverser);
			}

			LastReverser = reverser;
		}

		/// <summary>Called to update the power notch. This invokes a call to SetPower only if a change actually occurred.</summary>
		internal void UpdatePower()
		{
			int powerNotch = train.Handles.Power.Driver;

			foreach (Plugin plugin in plugins)
			{
				plugin.SetPower(powerNotch);
			}
		}

		/// <summary>Called to update the brake notch. This invokes a call to SetBrake only if a change actually occurred.</summary>
		internal void UpdateBrake()
		{
			int brakeNotch;
			if (train.Handles.Brake is TrainManager.AirBrakeHandle)
			{
				if (train.Handles.HasHoldBrake)
				{
					brakeNotch = train.Handles.EmergencyBrake.Driver ? 4 : train.Handles.Brake.Driver == (int)TrainManager.AirBrakeHandleState.Service ? 3 : train.Handles.Brake.Driver == (int)TrainManager.AirBrakeHandleState.Lap ? 2 : train.Handles.HoldBrake.Driver ? 1 : 0;
				}
				else
				{
					brakeNotch = train.Handles.EmergencyBrake.Driver ? 3 : train.Handles.Brake.Driver == (int)TrainManager.AirBrakeHandleState.Service ? 2 : train.Handles.Brake.Driver == (int)TrainManager.AirBrakeHandleState.Lap ? 1 : 0;
				}
			}
			else
			{
				if (train.Handles.HasHoldBrake)
				{
					brakeNotch = train.Handles.EmergencyBrake.Driver ? train.Handles.Brake.MaximumNotch + 2 : train.Handles.Brake.Driver > 0 ? train.Handles.Brake.Driver + 1 : train.Handles.HoldBrake.Driver ? 1 : 0;
				}
				else
				{
					brakeNotch = train.Handles.EmergencyBrake.Driver ? train.Handles.Brake.MaximumNotch + 1 : train.Handles.Brake.Driver;
				}
			}

			foreach (Plugin plugin in plugins)
			{
				plugin.SetBrake(brakeNotch);
			}
		}

		/// <summary>Called when a virtual key is pressed.</summary>
		/// <param name="key">The virtual key that was pressed.</param>
		internal void KeyDown(VirtualKeys key)
		{
			foreach (Plugin plugin in plugins)
			{
				plugin.KeyDown(key);
			}
		}

		/// <summary>Called when a virtual key is released.</summary>
		/// <param name="key">The virtual key that was released.</param>
		internal void KeyUp(VirtualKeys key)
		{
			foreach (Plugin plugin in plugins)
			{
				plugin.KeyUp(key);
			}
		}

		/// <summary>Called when a horn is played or stopped.</summary>
		/// <param name="type">The type of horn.</param>
		internal void HornBlow(HornTypes type)
		{
			foreach (Plugin plugin in plugins)
			{
				plugin.HornBlow(type);
			}
		}

		/// <summary>Called when the state of the doors changes.</summary>
		/// <param name="oldState">The old state of the doors.</param>
		/// <param name="newState">The new state of the doors.</param>
		internal void DoorChange(DoorStates oldState, DoorStates newState)
		{
			foreach (Plugin plugin in plugins)
			{
				plugin.DoorChange(oldState, newState);
			}
		}

		/// <summary>Called to update the aspects of the section. This invokes a call to SetSignal only if a change in aspect occurred or when changing section boundaries.</summary>
		/// <param name="data">The sections to submit to the plugin.</param>
		internal void UpdateSignals(SignalData[] data)
		{
			if (data.Length == 0)
			{
				return;
			}

			bool update = false;
			if (train.CurrentSectionIndex != LastSection)
			{
				update = true;
			}
			else if (data.Length != lastAspects.Length)
			{
				update = true;
			}
			else
			{
				for (int i = 0; i < data.Length; i++)
				{
					if (data[i].Aspect != lastAspects[i])
					{
						update = true;
						break;
					}
				}
			}

			if (!update)
			{
				return;
			}

			foreach (Plugin plugin in plugins)
			{
				plugin.SetSignal(data);
			}

			lastAspects = new int[data.Length];
			for (int i = 0; i < data.Length; i++)
			{
				lastAspects[i] = data[i].Aspect;
			}
		}

		/// <summary>Called when the train passes a beacon.</summary>
		/// <param name="type">The beacon type.</param>
		/// <param name="sectionIndex">The section the beacon is attached to, or -1 for the next red signal.</param>
		/// <param name="optional">Optional data attached to the beacon.</param>
		internal void UpdateBeacon(int type, int sectionIndex, int optional)
		{
			SignalData signal = null;

			if (sectionIndex == -1)
			{
				sectionIndex = train.CurrentSectionIndex + 1;
				while (sectionIndex < Program.CurrentRoute.Sections.Length)
				{
					signal = Program.CurrentRoute.Sections[sectionIndex].GetPluginSignal(train);
					if (signal.Aspect == 0) break;
					sectionIndex++;
				}

				if (sectionIndex >= Program.CurrentRoute.Sections.Length)
				{
					signal = new SignalData(-1, double.MaxValue);
				}
			}
			else if (sectionIndex >= 0)
			{
				if (sectionIndex < Program.CurrentRoute.Sections.Length)
				{
					signal = Program.CurrentRoute.Sections[sectionIndex].GetPluginSignal(train);
				}
				else
				{
					signal = new SignalData(0, double.MaxValue);
				}
			}
			else
			{
				signal = new SignalData(-1, double.MaxValue);
			}

			BeaconData beacon = new BeaconData(type, optional, signal);

			foreach (Plugin plugin in plugins)
			{
				plugin.SetBeacon(beacon);
			}
		}

		/// <summary>Updates the AI.</summary>
		/// <returns>The AI response.</returns>
		internal AIResponse UpdateAI()
		{
			AIData data = new AIData(GetHandles());

			foreach (Plugin plugin in plugins.Where(x => x.SupportsAI))
			{
				plugin.PerformAI(data);
			}

			if (data.Response != AIResponse.None)
			{
				SetHandles(data.Handles, false);
			}

			return data.Response;
		}

		private void CreatePanelIndexDictionary()
		{
			panelIndexDictionary = plugins
				.SelectMany((x, i) => x.GetPanelIndexDictionary().Select(y => new { DestIndex = y.Key, PluginIndex = i, SrcIndex = y.Value }))
				.GroupBy(x => x.DestIndex, x => (x.PluginIndex, x.SrcIndex), (x, y) => new { Key = x, Value = y.ToArray() })
				.ToDictionary(x => x.Key, x => x.Value);

			panelValueDictionary = panelIndexDictionary.ToDictionary(x => x.Key, _ => 0);

			PanelLength = panelIndexDictionary.Keys.Max() + 1;
		}

		internal int GetPanelValue(int index)
		{
			return panelValueDictionary.ContainsKey(index) ? panelValueDictionary[index] : 0;
		}

		private static void DisplayValidationErrors(IEnumerable<ValidationError> errors)
		{
			foreach (ValidationError error in errors)
			{
				switch (error)
				{
					case ChildSchemaValidationError childSchemaError:
						foreach (IEnumerable<ValidationError> childErrors in childSchemaError.Errors.Values)
						{
							DisplayValidationErrors(childErrors);
						}
						break;
					case MultiTypeValidationError multiTypeError:
						foreach (IEnumerable<ValidationError> childErrors in multiTypeError.Errors.Values)
						{
							DisplayValidationErrors(childErrors);
						}
						break;
					default:
						Interface.AddMessage(MessageType.Error, false, $"Validation error: ({error.Kind}: {error.Path}) at character {error.LinePosition} on line {error.LineNumber}");
						break;
				}
			}
		}

		/// <summary>Checks whether a specified file is a valid Win32 plugin.</summary>
		/// <param name="file">The file to check.</param>
		/// <returns>Whether the file is a valid Win32 plugin.</returns>
		private static bool CheckWin32Header(string file)
		{
			using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read))
			using (BinaryReader reader = new BinaryReader(stream))
			{
				if (reader.ReadUInt16() != 0x5A4D)
				{
					/* Not MZ signature */
					return false;
				}

				stream.Position = 0x3C;
				stream.Position = reader.ReadInt32();

				if (reader.ReadUInt32() != 0x00004550)
				{
					/* Not PE signature */
					return false;
				}

				if (reader.ReadUInt16() != 0x014C)
				{
					/* Not IMAGE_FILE_MACHINE_I386 */
					return false;
				}
			}

			return true;
		}
	}
}
