using System;
using System.Reflection;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Runtime;
using TrainManager.SafetySystems;

namespace TrainManager.Trains
{
	public partial class TrainBase
	{
		/// <summary>Loads a custom plugin for the specified train.</summary>
		/// <param name="trainFolder">The absolute path to the train folder.</param>
		/// <param name="encoding">The encoding to be used.</param>
		/// <returns>Whether the plugin was loaded successfully.</returns>
		public bool LoadCustomPlugin(string trainFolder, System.Text.Encoding encoding)
		{
			string config = OpenBveApi.Path.CombineFile(trainFolder, "ats.cfg");
			if (!System.IO.File.Exists(config))
			{
				return false;
			}

			string Text = System.IO.File.ReadAllText(config, encoding);
			Text = Text.Replace("\r", "").Replace("\n", "");
			if (Text.Length > 260)
			{
				/*
				 * String length is over max Windows path length, so
				 * comments or ATS plugin docs have been included in here
				 * e.g dlg70v40
				 */
				string[] fileLines = System.IO.File.ReadAllLines(config);
				for (int i = 0; i < fileLines.Length; i++)
				{
					int commentStart = fileLines[i].IndexOf(';');
					if (commentStart != -1)
					{
						fileLines[i] = fileLines[i].Substring(0, commentStart);
					}

					fileLines[i] = fileLines[i].Trim();
					if (fileLines[i].Length != 0)
					{
						Text = fileLines[i];
						break;
					}
				}
			}

			string file;
			try
			{
				file = OpenBveApi.Path.CombineFile(trainFolder, Text);
			}
			catch
			{
				TrainManagerBase.currentHost.AddMessage(MessageType.Error, true, "The train plugin path was malformed in " + config);
				return false;
			}

			string title = System.IO.Path.GetFileName(file);
			if (!System.IO.File.Exists(file))
			{
				if (Text.EndsWith(".dll") && encoding.Equals(System.Text.Encoding.Unicode))
				{
					// Our filename ends with .dll so probably is not mangled Unicode
					TrainManagerBase.currentHost.AddMessage(MessageType.Error, true, "The train plugin " + title + " could not be found in " + config);
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
					TrainManagerBase.currentHost.AddMessage(MessageType.Error, true, "The train plugin path was malformed in " + config);
					return false;
				}

				title = System.IO.Path.GetFileName(file);
				if (!System.IO.File.Exists(file))
				{
					// Nope, still not found
					TrainManagerBase.currentHost.AddMessage(MessageType.Error, true, "The train plugin " + title + " could not be found in " + config);
					return false;
				}

			}

			TrainManagerBase.currentHost.AddMessage(MessageType.Information, false, "Loading train plugin: " + file);
			bool success = LoadPlugin(file, trainFolder);
			if (success == false)
			{
				TrainManagerBase.PluginError = Translations.GetInterfaceString("errors_plugin_failure1").Replace("[plugin]", file);
			}
			else
			{
				TrainManagerBase.currentHost.AddMessage(MessageType.Information, false, "Train plugin loaded successfully.");
			}

			return success;
		}

		/// <summary>Gets the vehicle specs for use in safety system plugins</summary>
		public VehicleSpecs vehicleSpecs()
		{
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
			return new VehicleSpecs(powerNotches, brakeType, brakeNotches, hasHoldBrake, hasLocoBrake, cars);
		}

		/// <summary>Loads the specified plugin for the specified train.</summary>
		/// <param name="pluginFile">The file to the plugin.</param>
		/// <param name="trainFolder">The train folder.</param>
		/// <returns>Whether the plugin was loaded successfully.</returns>
		public bool LoadPlugin(string pluginFile, string trainFolder)
		{
			string pluginTitle = System.IO.Path.GetFileName(pluginFile);
			if (!System.IO.File.Exists(pluginFile))
			{
				TrainManagerBase.currentHost.AddMessage(MessageType.Error, true, "The train plugin " + pluginTitle + " could not be found.");
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

			InitializationModes mode = (InitializationModes) TrainManagerBase.CurrentOptions.TrainStart;
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
				try
				{
					AssemblyName myAssembly = AssemblyName.GetAssemblyName(pluginFile);
					if (IntPtr.Size != 4 && myAssembly.ProcessorArchitecture == ProcessorArchitecture.X86)
					{
						TrainManagerBase.currentHost.AddMessage(MessageType.Error, false, "The train plugin " + pluginTitle + " can only be used with the 32-bit version of OpenBVE");
						return false;
					}
				}
				catch
				{
					//ignored
				}
				
			}
			catch (Exception ex)
			{
				TrainManagerBase.currentHost.AddMessage(MessageType.Error, false, "The train plugin " + pluginTitle + " could not be loaded due to the following exception: " + ex.Message);
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
						TrainManagerBase.currentHost.AddMessage(MessageType.Error, false, "The train plugin " + pluginTitle + " raised an exception on loading: " + e.Message);
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
						if (Plugin.Load(vehicleSpecs(), mode))
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

				TrainManagerBase.currentHost.AddMessage(MessageType.Error, false, "The train plugin " + pluginTitle + " does not export a train interface and therefore cannot be used with openBVE.");
				return false;
			}

			/*
			 * Check if the plugin is a Win32 plugin.
			 * 
			 */
			try
			{
				if (!Win32Plugin.CheckHeader(pluginFile))
				{
					TrainManagerBase.currentHost.AddMessage(MessageType.Error, false, "The train plugin " + pluginTitle + " is of an unsupported binary format and therefore cannot be used with openBVE.");
					return false;
				}
			}
			catch (Exception ex)
			{
				TrainManagerBase.currentHost.AddMessage(MessageType.Error, false, "The train plugin " + pluginTitle + " could not be read due to the following reason: " + ex.Message);
				return false;
			}

			if (TrainManagerBase.currentHost.Platform != HostPlatform.MicrosoftWindows | IntPtr.Size != 4)
			{
				if (TrainManagerBase.currentHost.Platform == HostPlatform.MicrosoftWindows && IntPtr.Size != 4)
				{
					//We can't load the plugin directly on x64 Windows, so use the proxy interface
					Plugin = new ProxyPlugin(pluginFile, this);
					if (Plugin.Load(vehicleSpecs(), mode))
					{
						return true;
					}

					Plugin = null;
					TrainManagerBase.currentHost.AddMessage(MessageType.Error, false, "The train plugin " + pluginTitle + " failed to load.");
					return false;
				}

				//WINE doesn't seem to like the WCF proxy :(
				TrainManagerBase.currentHost.AddMessage(MessageType.Warning, false, "The train plugin " + pluginTitle + " can only be used on Microsoft Windows or compatible.");
				return false;
			}

			if (TrainManagerBase.currentHost.Platform == HostPlatform.MicrosoftWindows && !System.IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\AtsPluginProxy.dll"))
			{
				TrainManagerBase.currentHost.AddMessage(MessageType.Warning, false, "AtsPluginProxy.dll is missing or corrupt- Please reinstall.");
				return false;
			}

			Plugin = new Win32Plugin(pluginFile, this);
			if (Plugin.Load(vehicleSpecs(), mode))
			{
				return true;
			}
			else
			{
				Plugin = null;
				TrainManagerBase.currentHost.AddMessage(MessageType.Error, false, "The train plugin " + pluginTitle + " does not export a train interface and therefore cannot be used with openBVE.");
				return false;
			}
		}

		/// <summary>Loads the default plugin for the specified train.</summary>
		/// <param name="trainFolder">The train folder.</param>
		/// <returns>Whether the plugin was loaded successfully.</returns>
		public void LoadDefaultPlugin(string trainFolder)
		{
			string file = OpenBveApi.Path.CombineFile(TrainManagerBase.FileSystem.GetDataFolder("Plugins"), "OpenBveAts.dll");
			bool success = LoadPlugin(file, trainFolder);
			if (success)
			{
				Plugin.IsDefault = true;
			}
		}

		/// <summary>Unloads the currently loaded plugin, if any.</summary>
		public void UnloadPlugin()
		{
			if (Plugin != null)
			{
				Plugin.Unload();
				Plugin = null;
			}
		}
	}
}
