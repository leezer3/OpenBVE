using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using OpenBveApi;
using Path = System.IO.Path;

namespace OpenBve
{
	/// <summary>Represents plugins loaded by the program.</summary>
	internal static class Plugins
	{
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
			List<ContentLoadingPlugin> list = new List<ContentLoadingPlugin>();
			StringBuilder builder = new StringBuilder();
			foreach (string file in files)
			{
				if (file.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
				{
#if !DEBUG
					try {
#endif
					ContentLoadingPlugin plugin = new ContentLoadingPlugin(file);
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
						if (type.FullName == null)
						{
							continue;
						}
						if (type.IsSubclassOf(typeof(OpenBveApi.Textures.TextureInterface)))
						{
							plugin.Texture = (OpenBveApi.Textures.TextureInterface)assembly.CreateInstance(type.FullName);
						}
						if (type.IsSubclassOf(typeof(OpenBveApi.Sounds.SoundInterface)))
						{
							plugin.Sound = (OpenBveApi.Sounds.SoundInterface)assembly.CreateInstance(type.FullName);
						}
						if (type.IsSubclassOf(typeof(OpenBveApi.Objects.ObjectInterface)))
						{
							plugin.Object = (OpenBveApi.Objects.ObjectInterface)assembly.CreateInstance(type.FullName);
						}
						if (typeof(OpenBveApi.Runtime.IRuntime).IsAssignableFrom(type))
						{
							iruntime = true;
						}
					}
					if (plugin.Texture != null | plugin.Sound != null | plugin.Object != null)
					{
						plugin.Load(Program.CurrentHost, Program.FileSystem, Interface.CurrentOptions);
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
			Program.CurrentHost.Plugins = list.ToArray();
			if (Program.CurrentHost.Plugins.Length == 0)
			{
				MessageBox.Show("No available texture & sound loader plugins were found." + Environment.NewLine +
								" Please re-download openBVE.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return false;
			}
			string message = builder.ToString().Trim(new char[] { });
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
			if (Program.CurrentHost.Plugins != null)
			{
				foreach (ContentLoadingPlugin plugin in Program.CurrentHost.Plugins)
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
				Program.CurrentHost.Plugins = null;
			}
			string message = builder.ToString().Trim(new char[] { });
			if (message.Length != 0)
			{
				MessageBox.Show(message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}

	}
}
