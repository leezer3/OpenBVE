using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace OpenBve
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
			/// <summary>The interface to load textures as exposed by the plugin, or a null reference.</summary>
			internal OpenBveApi.Textures.TextureInterface Texture;
			/// <summary>The interface to load sounds as exposed by the plugin, or a null reference.</summary>
			internal OpenBveApi.Sounds.SoundInterface Sound;
			// --- constructors ---
			/// <summary>Creates a new instance of this class.</summary>
			/// <param name="file">The plugin file.</param>
			internal Plugin(string file)
			{
				this.File = file;
				this.Title = Path.GetFileName(file);
				this.Texture = null;
				this.Sound = null;
			}
			// --- functions ---
			/// <summary>Loads all interfaces this plugin supports.</summary>
			internal void Load()
			{
				if (this.Texture != null)
				{
					this.Texture.Load(Program.CurrentHost);
				}
				if (this.Sound != null)
				{
					this.Sound.Load(Program.CurrentHost);
				}
			}
			/// <summary>Unloads all interfaces this plugin supports.</summary>
			internal void Unload()
			{
				if (this.Texture != null)
				{
					this.Texture.Unload();
				}
				if (this.Sound != null)
				{
					this.Sound.Unload();
				}
			}
		}


		// --- members ---

		/// <summary>A list of all non-runtime plugins that are currently loaded, or a null reference.</summary>
		internal static Plugin[] LoadedPlugins = null;


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
					try
					{
						assembly = Assembly.LoadFile(file);
					}
					catch
					{
						builder.Append("Plugin ").Append(Path.GetFileName(file)).AppendLine(" is not a .Net assembly.");
						continue;
					}
					Type[] types = assembly.GetTypes();
					bool iruntime = false;
					foreach (Type type in types)
					{
						if (type.IsSubclassOf(typeof(OpenBveApi.Textures.TextureInterface)))
						{
							plugin.Texture = (OpenBveApi.Textures.TextureInterface)assembly.CreateInstance(type.FullName);
						}
						if (type.IsSubclassOf(typeof(OpenBveApi.Sounds.SoundInterface)))
						{
							plugin.Sound = (OpenBveApi.Sounds.SoundInterface)assembly.CreateInstance(type.FullName);
						}
						if (typeof(OpenBveApi.Runtime.IRuntime).IsAssignableFrom(type))
						{
							iruntime = true;
						}
					}
					if (plugin.Texture != null | plugin.Sound != null)
					{
						plugin.Load();
						list.Add(plugin);
					}
					else if (!iruntime)
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
				MessageBox.Show("No available texture & sound loader plugins were found." + Environment.NewLine +
								" Please re-download openBVE.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return false;
			}
			string message = builder.ToString().Trim();
			if (message.Length != 0)
			{
				return MessageBox.Show(message, Application.ProductName, MessageBoxButtons.OKCancel, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button2) == DialogResult.OK;
			}
			else
			{
				return true;
			}
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
			string message = builder.ToString().Trim();
			if (message.Length != 0)
			{
				MessageBox.Show(message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}

	}
}
