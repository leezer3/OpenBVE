using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using OpenBveApi.Objects;
using OpenBveApi.Sounds;
using OpenBveApi.Textures;

namespace TrainEditor2.Systems.Functions
{
	/// <summary>Represents plugins loaded by the program.</summary>
	internal static class Plugins
	{
		// --- classes ---
		/// <summary>Represents a plugin.</summary>
		internal class Plugin
		{
			// --- members ---
			/// <summary>The plugin file.</summary>
			internal readonly string File;

			/// <summary>The plugin title.</summary>
			internal readonly string Title;

			/// <summary>The interface to load sounds as exposed by the plugin, or a null reference.</summary>
			internal SoundInterface Sound;


			// --- constructors ---
			/// <summary>Creates a new instance of this class.</summary>
			/// <param name="file">The plugin file.</param>
			internal Plugin(string file)
			{
				File = file;
				Title = Path.GetFileName(file);
				Sound = null;
			}


			// --- functions ---
			/// <summary>Loads all interfaces this plugin supports.</summary>
			internal void Load()
			{
				Sound?.Load(Program.CurrentHost);
			}

			/// <summary>Unloads all interfaces this plugin supports.</summary>
			internal void Unload()
			{
				Sound?.Unload();
			}
		}


		// --- members ---
		/// <summary>A list of all non-runtime plugins that are currently loaded, or a null reference.</summary>
		internal static Plugin[] LoadedPlugins;


		// --- functions ---
		/// <summary>Loads all non-runtime plugins.</summary>
		/// <returns>Whether loading all plugins was successful.</returns>
		internal static bool LoadPlugins()
		{
			UnloadPlugins();
			string folder = Program.FileSystem.GetDataFolder("Plugins");
			string[] files = { };

			try
			{
				files = Directory.GetFiles(folder);
			}
			catch
			{
				// ignored
			}

			List<Plugin> list = new List<Plugin>();
			StringBuilder builder = new StringBuilder();

			foreach (string file in files)
			{
				if (file.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
				{
#if !DEBUG
					try {
#endif
					Plugin plugin = new Plugin(file);
					Assembly assembly;
					Type[] types;

					try
					{
						assembly = Assembly.LoadFile(file);
						types = assembly.GetTypes();
					}
					catch
					{
						builder.Append("Plugin ").Append(Path.GetFileName(file)).AppendLine(" is not a .Net assembly.");
						continue;
					}

					bool iTexture = false;
					bool iObject = false;
					bool iRuntime = false;

					foreach (Type type in types)
					{
						if (type.FullName == null)
						{
							continue;
						}

						if (type.IsSubclassOf(typeof(TextureInterface)))
						{
							iTexture = true;
						}

						if (type.IsSubclassOf(typeof(SoundInterface)))
						{
							plugin.Sound = (SoundInterface)assembly.CreateInstance(type.FullName);
						}

						if (type.IsSubclassOf(typeof(ObjectInterface)))
						{
							iObject = true;
						}

						if (typeof(OpenBveApi.Runtime.IRuntime).IsAssignableFrom(type))
						{
							iRuntime = true;
						}
					}

					if (plugin.Sound != null)
					{
						plugin.Load();
						list.Add(plugin);
					}
					else if (!iTexture && !iObject && !iRuntime)
					{
						builder.Append("Plugin ").Append(Path.GetFileName(file)).AppendLine(" does not implement compatible interfaces.");
						builder.AppendLine();
					}
#if !DEBUG
					} catch (Exception ex) {
						builder.Append("Could not load plugin ").Append(Path.GetFileName(file)).AppendLine(":").AppendLine(ex.Message);
						builder.AppendLine();
					}
#endif
				}
			}

			LoadedPlugins = list.ToArray();

			if (LoadedPlugins.Length == 0)
			{
				MessageBox.Show($@"No available texture & sound loader plugins were found.{Environment.NewLine} Please re-download openBVE.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return false;
			}

			string message = builder.ToString().Trim(new char[] { });

			if (message.Length != 0)
			{
				return MessageBox.Show(message, Application.ProductName, MessageBoxButtons.OKCancel, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button2) == DialogResult.OK;
			}

			return true;
		}

		/// <summary>Unloads all non-runtime plugins.</summary>
		internal static void UnloadPlugins()
		{
			StringBuilder builder = new StringBuilder();

			if (LoadedPlugins != null)
			{
				foreach (Plugin plugin in LoadedPlugins)
				{
#if !DEBUG
					try {
#endif
					plugin.Unload();
#if !DEBUG
					} catch (Exception ex) {
						builder.Append("Could not unload plugin ").Append(plugin.Title).AppendLine(":").AppendLine(ex.Message);
						builder.AppendLine();
					}
#endif
				}

				LoadedPlugins = null;
			}

			string message = builder.ToString().Trim(new char[] { });

			if (message.Length != 0)
			{
				MessageBox.Show(message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}
	}
}
