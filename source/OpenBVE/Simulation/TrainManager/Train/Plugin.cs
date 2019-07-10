using System;
using System.Reflection;
using OpenBveApi.Interface;
using OpenBveApi.Runtime;

namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		/// <summary>The root class for a train within the simulation</summary>
		public partial class Train
		{
			/// <summary>Loads a custom plugin for the specified train.</summary>
			/// <param name="trainFolder">The absolute path to the train folder.</param>
			/// <param name="encoding">The encoding to be used.</param>
			/// <returns>Whether the plugin was loaded successfully.</returns>
			internal bool LoadCustomPlugin(string trainFolder, System.Text.Encoding encoding)
			{
				string config = OpenBveApi.Path.CombineFile(trainFolder, "ats.cfg");
				if (!System.IO.File.Exists(config))
				{
					return false;
				}

				string Text = System.IO.File.ReadAllText(config, encoding);
				Text = Text.Replace("\r", "").Replace("\n", "");
				string file;
				try
				{
					file = OpenBveApi.Path.CombineFile(trainFolder, Text);
				}
				catch
				{
					Interface.AddMessage(MessageType.Error, true, "The train plugin path was malformed in " + config);
					return false;
				}

				string title = System.IO.Path.GetFileName(file);
				if (!System.IO.File.Exists(file))
				{
					if (Text.EndsWith(".dll") && encoding.Equals(System.Text.Encoding.Unicode))
					{
						// Our filename ends with .dll so probably is not mangled Unicode
						Interface.AddMessage(MessageType.Error, true, "The train plugin " + title + " could not be found in " + config);
						return false;
					}

					// Try again with ASCII encoding
					Text = System.IO.File.ReadAllText(config, System.Text.Encoding.GetEncoding(1252));
					Text = Text.Replace("\r", "").Replace("\n", "");
					try
					{
						file = OpenBveApi.Path.CombineFile(trainFolder, Text);
					}
					catch
					{
						Interface.AddMessage(MessageType.Error, true, "The train plugin path was malformed in " + config);
						return false;
					}

					title = System.IO.Path.GetFileName(file);
					if (!System.IO.File.Exists(file))
					{
						// Nope, still not found
						Interface.AddMessage(MessageType.Error, true, "The train plugin " + title + " could not be found in " + config);
						return false;
					}

				}

				Program.FileSystem.AppendToLogFile("Loading train plugin: " + file);
				bool success = LoadPlugin(file, trainFolder);
				if (success == false)
				{
					Loading.PluginError = Translations.GetInterfaceString("errors_plugin_failure1").Replace("[plugin]", file);
				}
				else
				{
					Program.FileSystem.AppendToLogFile("Train plugin loaded successfully.");
				}

				return success;
			}

			/// <summary>Loads the specified plugin for the specified train.</summary>
			/// <param name="pluginFile">The file to the plugin.</param>
			/// <param name="trainFolder">The train folder.</param>
			/// <returns>Whether the plugin was loaded successfully.</returns>
			internal bool LoadPlugin(string pluginFile, string trainFolder)
			{
				string pluginTitle = System.IO.Path.GetFileName(pluginFile);
				if (!System.IO.File.Exists(pluginFile))
				{
					Interface.AddMessage(MessageType.Error, true, "The train plugin " + pluginTitle + " could not be found.");
					return false;
				}

				/*
				 * Unload plugin if already loaded.
				 * */
				if (Plugin != null)
				{
					UnloadPlugin();
				}

				/*
				 * Prepare initialization data for the plugin.
				 * */
				BrakeTypes brakeType = (BrakeTypes) Cars[DriverCar].CarBrake.brakeType;
				int brakeNotches;
				int powerNotches;
				bool hasHoldBrake;
				if (brakeType == BrakeTypes.AutomaticAirBrake)
				{
					brakeNotches = 2;
					powerNotches = Handles.Power.MaximumNotch;
					hasHoldBrake = false;
				}
				else
				{
					brakeNotches = Handles.Brake.MaximumNotch + (Handles.HasHoldBrake ? 1 : 0);
					powerNotches = Handles.Power.MaximumNotch;
					hasHoldBrake = Handles.HasHoldBrake;
				}

				bool hasLocoBrake = Handles.HasLocoBrake;
				int cars = Cars.Length;
				VehicleSpecs specs = new VehicleSpecs(powerNotches, brakeType, brakeNotches, hasHoldBrake, hasLocoBrake, cars);
				InitializationModes mode = (InitializationModes) Game.TrainStart;
				/*
				 * Check if the plugin is a .NET plugin.
				 * */
				Assembly assembly;
				try
				{
					assembly = Assembly.LoadFile(pluginFile);
				}
				catch (BadImageFormatException)
				{
					assembly = null;
				}
				catch (Exception ex)
				{
					Interface.AddMessage(MessageType.Error, false, "The train plugin " + pluginTitle + " could not be loaded due to the following exception: " + ex.Message);
					return false;
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
							Interface.AddMessage(MessageType.Error, false, "The train plugin " + pluginTitle + " raised an exception on loading: " + e.Message);
						}

						return false;
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

							IRuntime api = assembly.CreateInstance(type.FullName) as IRuntime;
							Plugin = new NetPlugin(pluginFile, trainFolder, api, this);
							if (Plugin.Load(specs, mode))
							{
								return true;
							}
							else
							{
								Plugin = null;
								return false;
							}
						}
					}

					Interface.AddMessage(MessageType.Error, false, "The train plugin " + pluginTitle + " does not export a train interface and therefore cannot be used with openBVE.");
					return false;
				}

				/*
				 * Check if the plugin is a Win32 plugin.
				 * 
				 */
				try
				{
					if (!PluginManager.CheckWin32Header(pluginFile))
					{
						Interface.AddMessage(MessageType.Error, false, "The train plugin " + pluginTitle + " is of an unsupported binary format and therefore cannot be used with openBVE.");
						return false;
					}
				}
				catch (Exception ex)
				{
					Interface.AddMessage(MessageType.Error, false, "The train plugin " + pluginTitle + " could not be read due to the following reason: " + ex.Message);
					return false;
				}

				if (!Program.CurrentlyRunningOnWindows | IntPtr.Size != 4)
				{
					Interface.AddMessage(MessageType.Warning, false, "The train plugin " + pluginTitle + " can only be used on 32-bit Microsoft Windows or compatible.");
					return false;
				}

				if (Program.CurrentlyRunningOnWindows && !System.IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\AtsPluginProxy.dll"))
				{
					Interface.AddMessage(MessageType.Warning, false, "AtsPluginProxy.dll is missing or corrupt- Please reinstall.");
					return false;
				}

				Plugin = new Win32Plugin(pluginFile, this);
				if (Plugin.Load(specs, mode))
				{
					return true;
				}
				else
				{
					Plugin = null;
					Interface.AddMessage(MessageType.Error, false, "The train plugin " + pluginTitle + " does not export a train interface and therefore cannot be used with openBVE.");
					return false;
				}
			}

			/// <summary>Loads the default plugin for the specified train.</summary>
			/// <param name="trainFolder">The train folder.</param>
			/// <returns>Whether the plugin was loaded successfully.</returns>
			internal void LoadDefaultPlugin(string trainFolder)
			{
				string file = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Plugins"), "OpenBveAts.dll");
				bool success = LoadPlugin(file, trainFolder);
				if (success)
				{
					Plugin.IsDefault = true;
				}
			}

			/// <summary>Unloads the currently loaded plugin, if any.</summary>
			internal void UnloadPlugin()
			{
				if (Plugin != null)
				{
					Plugin.Unload();
					Plugin = null;
				}
			}
		}
	}
}
