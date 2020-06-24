using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using OpenBveApi;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Runtime;
using OpenBveApi.Sounds;
using OpenBveApi.Textures;
using Path = System.IO.Path;

namespace TrainEditor2.Systems.Functions
{
	/// <summary>Represents plugins loaded by the program.</summary>
	internal static class Plugins
	{
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

			List<ContentLoadingPlugin> list = new List<ContentLoadingPlugin>();
			StringBuilder builder = new StringBuilder();

			foreach (string file in files)
			{
				if (file.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
				{
#if !DEBUG
					try
					{
#endif
						ContentLoadingPlugin plugin = new ContentLoadingPlugin(file);
						Assembly assembly;
						Type[] types;

						try
						{
							assembly = Assembly.LoadFile(file);
							types = assembly.GetTypes();
						}
						catch
						{
							builder.AppendLine($"Plugin {Path.GetFileName(file)} is not a .Net assembly.");
							continue;
						}

						bool iTexture = false;
						bool iObject = false;
						bool iRoute = false;
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

							if (type.IsSubclassOf(typeof(RouteInterface)))
							{
								iRoute = true;
							}

							if (typeof(IRuntime).IsAssignableFrom(type))
							{
								iRuntime = true;
							}
						}

						if (plugin.Sound != null)
						{
							// Doesn't actually need a renderer reference at all here.
							plugin.Load(Program.CurrentHost, Program.FileSystem, new Interface.Options());
							list.Add(plugin);
						}
						else if (!iTexture && !iObject && !iRoute && !iRuntime)
						{
							builder.AppendLine($"Plugin {Path.GetFileName(file)} does not implement compatible interfaces.");
							builder.AppendLine();
						}
#if !DEBUG
					}
					catch (Exception ex)
					{
						builder.AppendLine($"Could not load plugin {Path.GetFileName(file)}:{ex.Message}");
						builder.AppendLine();
					}
#endif
				}
			}

			Program.CurrentHost.Plugins = list.ToArray();

			if (Program.CurrentHost.Plugins.Length == 0)
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

			if (Program.CurrentHost.Plugins != null)
			{
				foreach (ContentLoadingPlugin plugin in Program.CurrentHost.Plugins)
				{
#if !DEBUG
					try
					{
#endif
						plugin.Unload();
#if !DEBUG
					}
					catch (Exception ex)
					{
						builder.AppendLine($"Could not unload plugin {plugin.Title}:{ex.Message}");
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
