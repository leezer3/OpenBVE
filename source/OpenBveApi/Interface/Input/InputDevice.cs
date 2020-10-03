using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Forms;
using OpenBveApi.Runtime;

namespace OpenBveApi.Interface
{
	/// <summary>
	/// The class of event argument with control's information
	/// </summary>
	public class InputEventArgs : EventArgs
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="control">Control's information</param>
		public InputEventArgs(InputControl control)
		{
			Control = control;
		}

		/// <summary>
		/// Control's information
		/// </summary>
		public InputControl Control { get; }
	}

	/// <summary>
	/// A structure within the control's information
	/// </summary>
	public struct InputControl
	{
		/// <summary>
		/// Command
		/// </summary>
		public Translations.Command Command;

		/// <summary>
		/// Command option
		/// </summary>
		public int Option;
	}

	/// <summary>
	/// An interface for Input Device Plugin
	/// </summary>
	public interface IInputDevice
	{
		/// <summary>
		/// Define KeyDown event
		/// </summary>
		event EventHandler<InputEventArgs> KeyDown;

		/// <summary>
		/// Define KeyUp event
		/// </summary>
		event EventHandler<InputEventArgs> KeyUp;

		/// <summary>
		/// The control list that is using for plugin
		/// </summary>
		InputControl[] Controls { get; }

		/// <summary>
		/// A function called when the plugin is loading
		/// </summary>
		/// <param name="fileSystem">The instance of FileSystem class</param>
		/// <returns>Check the plugin loading process is successfully</returns>
		bool Load(FileSystem.FileSystem fileSystem);

		/// <summary>
		/// A function call when the plugin is unload
		/// </summary>
		void Unload();

		/// <summary>
		/// A function called when the Config button is pressed
		/// </summary>
		/// <param name="owner">The owner of the window</param>
		void Config(IWin32Window owner);

		/// <summary>
		/// The function what the notify to the plugin that the train maximum notches
		/// </summary>
		/// <param name="powerNotch">Maximum power notch number</param>
		/// <param name="brakeNotch">Maximum brake notch number</param>
		void SetMaxNotch(int powerNotch, int brakeNotch);

		/// <summary>
		/// The function what notify to the plugin that the train existing status
		/// </summary>
		/// <param name="data">Data</param>
		void SetElapseData(ElapseData data);

		/// <summary>
		/// A function that calls each frame
		/// </summary>
		void OnUpdateFrame();
	}

	/// <summary>An interface for Input Device plugins which require the train properties to function</summary>
	public interface ITrainInputDevice : IInputDevice
	{
		/// <summary>
		/// Called after the train has loaded in order to inform the plugin of it's specs
		/// </summary>
		/// <param name="mySpecs"></param>
		void SetVehicleSpecs(VehicleSpecs mySpecs);
	}

	/// <summary>
	/// The class of the input device plugin
	/// </summary>
	public class InputDevicePlugin
	{
		/// <summary>
		/// Enumerators of the plugin status
		/// </summary>
		public enum Status
		{
			/// <summary>
			/// Failed the loading
			/// </summary>
			Failure = 0,

			/// <summary>
			/// Disabled
			/// </summary>
			Disable = 1,

			/// <summary>
			/// Enabled
			/// </summary>
			Enable = 2
		};

		/// <summary>
		/// The class of the plugin's information
		/// </summary>
		public class Info
		{
			/// <summary>
			/// Plugin's name
			/// </summary>
			public AssemblyTitleAttribute Name { get; }

			/// <summary>
			/// Plugin's state
			/// </summary>
			public Status Status { get; internal set; }

			/// <summary>
			/// Version information of the plugin
			/// </summary>
			public AssemblyFileVersionAttribute Version { get; }

			/// <summary>
			/// Provider of the plugin
			/// </summary>
			public AssemblyCopyrightAttribute Provider { get; }

			/// <summary>
			/// Filename of the plugin
			/// </summary>
			public string FileName { get; }

			/// <summary>
			/// The instance of the plugin
			/// </summary>
			public IInputDevice Api { get; }

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="assembly">Assembly information of the plugin</param>
			/// <param name="api">The instance of the plugin</param>
			internal Info(Assembly assembly, IInputDevice api)
			{
				Name = (AssemblyTitleAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyTitleAttribute));
				Status = Status.Disable;
				Version = (AssemblyFileVersionAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyFileVersionAttribute));
				Provider = (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCopyrightAttribute));
				FileName = System.IO.Path.GetFileName(assembly.Location);
				Api = api;
			}
		}

		/// <summary>
		/// The instance of FileSystem class
		/// </summary>
		private readonly FileSystem.FileSystem fileSystem;

		/// <summary>
		/// The entity of AvailableInfos
		/// </summary>
		private readonly List<Info> availableInfos;

		/// <summary>
		/// The storing list of the plugin information that can use
		/// </summary>
		public ReadOnlyCollection<Info> AvailableInfos { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="fileSystem">The instance of FileSystem class</param>
		public InputDevicePlugin(FileSystem.FileSystem fileSystem)
		{
			this.fileSystem = fileSystem;
			availableInfos = new List<Info>();
			AvailableInfos = availableInfos.AsReadOnly();
		}

		/// <summary>
		/// The function that is load the plugin what can use
		/// </summary>
		public void LoadPlugins()
		{
			string pluginsFolder = fileSystem.GetDataFolder("InputDevicePlugins");

			if (!System.IO.Directory.Exists(pluginsFolder))
			{
				return;
			}

			foreach (string File in System.IO.Directory.GetFiles(pluginsFolder, "*.dll"))
			{
				Assembly assembly;
				try
				{
					assembly = Assembly.LoadFrom(File);
				}
				catch
				{
					continue;
				}

				Type[] types;
				try
				{
					types = assembly.GetTypes();
				}
				catch
				{
					continue;
				}

				foreach (Type type in types)
				{
					if (typeof(IInputDevice).IsAssignableFrom(type))
					{
						if (type.FullName == null)
						{
							continue;
						}

						availableInfos.Add(new Info(assembly, assembly.CreateInstance(type.FullName) as IInputDevice));
					}
				}
			}
		}

		/// <summary>
		/// The function that calls the plugin's load function
		/// </summary>
		/// <param name="index">The index number which can use the plugins</param>
		public void CallPluginLoad(int index)
		{
			if (index < 0 || index >= AvailableInfos.Count)
			{
				return;
			}

			CallPluginLoad(AvailableInfos[index]);
		}

		/// <summary>
		/// The function that calls the plugin's load function
		/// </summary>
		/// <param name="info">The instance of Info class</param>
		public void CallPluginLoad(Info info)
		{
			if (info.Status == Status.Enable)
			{
				return;
			}

			info.Status = info.Api.Load(fileSystem) ? Status.Enable : Status.Failure;
		}

		/// <summary>
		/// The function that calls the plugin's unload function
		/// </summary>
		public void CallPluginUnload()
		{
			foreach (Info info in AvailableInfos)
			{
				CallPluginUnload(info);
			}
		}

		/// <summary>
		/// The function that calls the plugin's unload function
		/// </summary>
		/// <param name="index">The index number which can use the plugins</param>
		public void CallPluginUnload(int index)
		{
			if (index < 0 || index >= AvailableInfos.Count)
			{
				return;
			}

			CallPluginUnload(AvailableInfos[index]);
		}

		/// <summary>
		/// The function that calls the plugin's unload function
		/// </summary>
		/// <param name="info">The instance of Info class</param>
		public void CallPluginUnload(Info info)
		{
			if (info.Status != Status.Enable)
			{
				return;
			}

			info.Api.Unload();
			info.Status = Status.Disable;
		}

		/// <summary>
		/// The function that calls the plugin's configuration function
		/// </summary>
		/// <param name="owner">The owner of the window</param>
		/// <param name="index">The index number which can use the plugins</param>
		public void CallPluginConfig(IWin32Window owner, int index)
		{
			if (index < 0 || index >= AvailableInfos.Count)
			{
				return;
			}

			Info info = AvailableInfos[index];

			if (info.Status != Status.Enable)
			{
				return;
			}

			info.Api.Config(owner);
		}
	}
}
