using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

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
			: base()
		{
				this.Control = control;
		}

		/// <summary>
		/// Control's information
		/// </summary>
		public InputControl Control { get; private set; }
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
		/// A function call when the plugin is loading
		/// </summary>
		/// <param name="fileSystem">The instance of FileSytem class</param>
		/// <returns>Check the plugin loading process is successfully</returns>
		bool Load(FileSystem.FileSystem fileSystem);

		/// <summary>
		/// A function call when the plugin is unload
		/// </summary>
		void Unload();

		/// <summary>
		/// A funciton call when the Config button pressed
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
		void SetElapseData(Runtime.ElapseData data);

		/// <summary>
		/// A function that calls each frame
		/// </summary>
		void OnUpdateFrame();
	}

	/// <summary>
	/// The class of the input device plugin
	/// </summary>
	public static class InputDevicePlugin
	{
		/// <summary>
		/// The class of the plugin's information
		/// </summary>
		public class PluginInfo
		{
			/// <summary>
			/// Enumerators of the plugin status
			/// </summary>
			public enum PluginStatus
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
			/// Plugin's name
			/// </summary>
			public AssemblyTitleAttribute Name { get; private set; }

			/// <summary>
			/// Plugin's state
			/// </summary>
			public PluginStatus Status { get; internal set; }

			/// <summary>
			/// Version information of the plugin
			/// </summary>
			public AssemblyFileVersionAttribute Version { get; private set; }

			/// <summary>
			/// Provider of the plugin
			/// </summary>
			public AssemblyCopyrightAttribute Provider { get; private set; }

			/// <summary>
			/// Filename of the plugin
			/// </summary>
			public string FileName { get; private set; }

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="File">Asssembly information of the plugin</param>
			internal PluginInfo(Assembly File)
			{
				Name = (AssemblyTitleAttribute)Attribute.GetCustomAttribute(File, typeof(AssemblyTitleAttribute));
				Status = PluginStatus.Disable;
				Version = (AssemblyFileVersionAttribute)Attribute.GetCustomAttribute(File, typeof(AssemblyFileVersionAttribute));
				Provider = (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(File, typeof(AssemblyCopyrightAttribute));
				FileName = System.IO.Path.GetFileName(File.Location);
			}
		}

		/// <summary>
		/// The instance of FileSystem class
		/// </summary>
		private static FileSystem.FileSystem FileSystem = null;

		/// <summary>
		/// The storing list of the plugin information that can use
		/// </summary>
		public static readonly List<PluginInfo> AvailablePluginInfos = new List<PluginInfo>();

		/// <summary>
		/// The storing list of the plugin instance that can use
		/// </summary>
		public static readonly List<IInputDevice> AvailablePlugins = new List<IInputDevice>();

		/// <summary>
		/// The function that is load the plugin what can use
		/// </summary>
		/// <param name="fileSystem">The instance of FileSystem class</param>
		public static void LoadPlugins(FileSystem.FileSystem fileSystem)
		{
			if (fileSystem == null)
			{
				return;
			}
			FileSystem = fileSystem;
			string PluginsFolder = FileSystem.GetDataFolder("InputDevicePlugins");
			if (!System.IO.Directory.Exists(PluginsFolder))
			{
				return;
			}
			string[] PluginFiles = System.IO.Directory.GetFiles(PluginsFolder, "*.dll");
			foreach (var File in PluginFiles)
			{
				Assembly Plugin = null;
				try
				{
					Plugin = Assembly.LoadFrom(File);
				}
				catch
				{
					continue;
				}
				Type[] Types = null;
				try
				{
					Types = Plugin.GetTypes();
				}
				catch
				{
					continue;
				}
				foreach (var Type in Types)
				{
					if (typeof(IInputDevice).IsAssignableFrom(Type))
					{
						if (Type.FullName == null)
						{
							continue;
						}
						AvailablePluginInfos.Add(new PluginInfo(Plugin));
						AvailablePlugins.Add(Plugin.CreateInstance(Type.FullName) as IInputDevice);
					}
				}
			}
		}

		/// <summary>
		/// The function that calls the plugin's load funcion
		/// </summary>
		/// <param name="index">The index number which can use the plugins</param>
		public static void CallPluginLoad(int index)
		{
			if (index < 0 || index >= AvailablePlugins.Count || index >= AvailablePluginInfos.Count)
			{
				return;
			}
			if (AvailablePluginInfos[index].Status == PluginInfo.PluginStatus.Enable)
			{
				return;
			}
			if (AvailablePlugins[index].Load(FileSystem))
			{
				AvailablePluginInfos[index].Status = PluginInfo.PluginStatus.Enable;
			}
			else
			{
				AvailablePluginInfos[index].Status = PluginInfo.PluginStatus.Failure;
			}
		}

		/// <summary>
		/// The function that calls the plugin's unload funcion
		/// </summary>
		/// <param name="index">The index number which can use the plugins</param>
		public static void CallPluginUnload(int index)
		{
			if (index < 0 || index >= AvailablePlugins.Count || index >= AvailablePluginInfos.Count)
			{
				return;
			}
			if (AvailablePluginInfos[index].Status != PluginInfo.PluginStatus.Enable)
			{
				return;
			}
			AvailablePlugins[index].Unload();
			AvailablePluginInfos[index].Status = PluginInfo.PluginStatus.Disable;
		}

		/// <summary>
		/// The function that calls the plugin's configration funcion
		/// </summary>
		/// <param name="owner">The owner of the window</param>
		/// <param name="index">The index number which can use the plugins</param>
		public static void CallPluginConfig(IWin32Window owner, int index)
		{
			if (index < 0 || index >= AvailablePlugins.Count) {
				return;
			}
			if (AvailablePluginInfos[index].Status != PluginInfo.PluginStatus.Enable)
			{
				return;
			}
			AvailablePlugins[index].Config(owner);
		}
	}
}
