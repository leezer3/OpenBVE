using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using OpenBveApi.Interface;

namespace OpenBveApi.Hosts
{
	public abstract partial class HostInterface
	{
		/// <summary>Loads all non-runtime plugins.</summary>
		/// <returns>Whether loading all plugins was successful.</returns>
		public bool LoadPlugins(FileSystem.FileSystem fileSystem, BaseOptions currentOptions, out string errorMessage, object TrainManagerReference = null, object RendererReference = null)
		{
			UnloadPlugins(out errorMessage);
			string folder = fileSystem.GetDataFolder("Plugins");
			string[] files = {};
			try
			{
				files = Directory.GetFiles(folder);
				string[] UserPluginFiles =  Directory.GetFiles(fileSystem.PluginInstallationDirectory, "*.dll");
				if (UserPluginFiles.Length != 0)
				{
					int startIdx = files.Length;
					Array.Resize(ref files, startIdx + UserPluginFiles.Length);
					Array.Copy(UserPluginFiles, 0, files, startIdx, UserPluginFiles.Length);
				}
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
					try
					{
						ContentLoadingPlugin plugin = new ContentLoadingPlugin(file);
						Assembly assembly;
						Type[] types;
						try
						{
							assembly = Assembly.LoadFile(file);
							types = assembly.GetTypes();
						}
						catch (Exception ex)
						{
							if ((ex is ReflectionTypeLoadException))
							{
								/*
								 * This is actually a .Net assembly, it just failed to load a reference
								 * Probably built against a newer API version.
								 */

								builder.Append("Plugin ").Append(System.IO.Path.GetFileName(file)).AppendLine(" failed to load. \n \n Please check that you are using the most recent version of OpenBVE.");

							}
							else
							{
								builder.Append("Plugin ").Append(System.IO.Path.GetFileName(file)).AppendLine(" is not a .Net assembly.");
							}

							continue;
						}
						bool knownType = false;
						foreach (Type type in types)
						{
							if (type.FullName == null)
							{
								continue;
							}
							if (type.IsSubclassOf(typeof(Textures.TextureInterface)))
							{
								plugin.Texture = (Textures.TextureInterface) assembly.CreateInstance(type.FullName);
							}
							if (type.IsSubclassOf(typeof(Sounds.SoundInterface)))
							{
								plugin.Sound = (Sounds.SoundInterface) assembly.CreateInstance(type.FullName);
							}

							if (type.IsSubclassOf(typeof(Objects.ObjectInterface)))
							{
								plugin.Object = (Objects.ObjectInterface) assembly.CreateInstance(type.FullName);
							}

							if (type.IsSubclassOf(typeof(Routes.RouteInterface)))
							{
								plugin.Route = (Routes.RouteInterface) assembly.CreateInstance(type.FullName);
							}

							if (type.IsSubclassOf(typeof(Trains.TrainInterface)))
							{
								plugin.Train = (Trains.TrainInterface) assembly.CreateInstance(type.FullName);
							}

							if (typeof(Runtime.IRuntime).IsAssignableFrom(type) || typeof(IInputDevice).IsAssignableFrom(type))
							{
								knownType = true;
							}
						}

						if (plugin.Texture != null | plugin.Sound != null | plugin.Object != null | plugin.Route != null | plugin.Train != null)
						{
							plugin.Load(this, fileSystem, currentOptions, TrainManagerReference, RendererReference);
							list.Add(plugin);
						}
						else if (!knownType)
						{
							builder.Append("Plugin ").Append(System.IO.Path.GetFileName(file)).AppendLine(" does not implement compatible interfaces.");
							builder.AppendLine();
						}
					}
					catch (Exception ex)
					{
						builder.Append("Could not load plugin ").Append(System.IO.Path.GetFileName(file)).AppendLine(":").AppendLine(ex.Message);
						builder.AppendLine();
					}
				}
			}
			Plugins = list.ToArray();
			if (Plugins.Length == 0)
			{
				errorMessage = "No available content loading plugins were found." + Environment.NewLine + " Please re-download openBVE.";
				return false;
			}
			errorMessage = builder.ToString().Trim(new char[] { });
			if (errorMessage.Length != 0)
			{
				return false;
			}
			return true;
		}

		/// <summary>Unloads all non-runtime plugins.</summary>
		public bool UnloadPlugins(out string errorMessage)
		{
			StringBuilder builder = new StringBuilder();
			if (Plugins != null)
			{
				foreach (ContentLoadingPlugin plugin in Plugins)
				{
					try
					{
						plugin.Unload();
					}
					catch (Exception ex)
					{
						builder.Append("Could not unload plugin ").Append(plugin.Title).AppendLine(":").AppendLine(ex.Message);
						builder.AppendLine();
					}
				}
				Plugins = null;
			}
			errorMessage = builder.ToString().Trim(new char[] { });
			if (errorMessage.Length != 0)
			{
				return false;
			}
			return true;
		}
	}
}
