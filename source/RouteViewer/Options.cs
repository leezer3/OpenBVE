using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Formats.OpenBve;
using OpenBveApi;
using Path = OpenBveApi.Path;

namespace RouteViewer
{
	/// <summary>Holds the program specific options</summary>
	internal class Options : BaseOptions
	{
		internal bool LoadingProgressBar;
		internal bool LoadingLogo;
		internal bool LoadingBackground;
		internal string RouteSearchDirectory;

		internal Options()
		{
			ViewingDistance = 600;
			SoundNumber = 16;
		}

		public override void Save(string fileName)
		{
			try
			{
				CultureInfo Culture = CultureInfo.InvariantCulture;
				System.Text.StringBuilder Builder = new System.Text.StringBuilder();
				Builder.AppendLine("; Options");
				Builder.AppendLine("; =======");
				Builder.AppendLine("; This file was automatically generated. Please modify only if you know what you're doing.");
				Builder.AppendLine("; Route Viewer specific options file");
				Builder.AppendLine();
				Builder.AppendLine("[display]");
				Builder.AppendLine("vsync = " + (VerticalSynchronization ? "true" : "false"));
				Builder.AppendLine("windowWidth = " + Program.Renderer.Screen.Width.ToString(Culture));
				Builder.AppendLine("windowHeight = " + Program.Renderer.Screen.Height.ToString(Culture));
				Builder.AppendLine("isUseNewRenderer = " + (IsUseNewRenderer ? "true" : "false"));
				Builder.AppendLine("viewingdistance = " + ViewingDistance);
				Builder.AppendLine("quadleafsize = " + QuadTreeLeafSize);
				Builder.AppendLine();
				Builder.AppendLine("[quality]");
				Builder.AppendLine("interpolation = " + Interpolation);
				Builder.AppendLine("anisotropicfilteringlevel = " + AnisotropicFilteringLevel.ToString(Culture));
				Builder.AppendLine("antialiasinglevel = " + AntiAliasingLevel.ToString(Culture));
				Builder.AppendLine("transparencyMode = " + ((int)TransparencyMode).ToString(Culture));
				Builder.AppendLine();
				Builder.AppendLine("[loading]");
				Builder.AppendLine("showlogo = " + (LoadingLogo ? "true" : "false"));
				Builder.AppendLine("showprogressbar = " + (LoadingProgressBar ? "true" : "false"));
				Builder.AppendLine("showbackground = " + (LoadingBackground ? "true" : "false"));
				Builder.AppendLine("[parsers]");
				Builder.AppendLine("xObject = " + (int)CurrentXParser);
				Builder.AppendLine("objObject = " + (int)CurrentObjParser);
				Builder.AppendLine();
				Builder.AppendLine("[Folders]");
				Builder.AppendLine($"routesearch = {RouteSearchDirectory}");
				File.WriteAllText(fileName, Builder.ToString(), new System.Text.UTF8Encoding(true));
			}
			catch
			{
				MessageBox.Show("An error occured whilst saving the options to disk." + Environment.NewLine +
								"Please check you have write permission.");
			}
		}

		internal static void LoadOptions()
		{
			Interface.CurrentOptions = new Options();
			string optionsFolder = Path.CombineDirectory(Program.FileSystem.SettingsFolder, "1.5.0");
			if (!Directory.Exists(optionsFolder))
			{
				Directory.CreateDirectory(optionsFolder);
			}
			string configFile = Path.CombineFile(optionsFolder, "options_rv.cfg");
			if (!File.Exists(configFile))
			{
				//Attempt to load and upgrade a prior configuration file
				string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				configFile = Path.CombineFile(Path.CombineDirectory(Path.CombineDirectory(assemblyFolder, "UserData"), "Settings"), "options_rv.cfg");

				if (!File.Exists(configFile))
				{
					//If no route viewer specific configuration file exists, then try the main OpenBVE configuration file
					//Write out to a new routeviewer specific file though
					configFile = Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/options.cfg");
				}
			}

			if (File.Exists(configFile))
			{
				ConfigFile<OptionsSection, OptionsKey> cfg = new ConfigFile<OptionsSection, OptionsKey>(File.ReadAllLines(configFile, new System.Text.UTF8Encoding()), Program.CurrentHost);

				while (cfg.RemainingSubBlocks > 0)
				{
					Block<OptionsSection, OptionsKey> block = cfg.ReadNextBlock();
					switch (block.Key)
					{
						case OptionsSection.Display:
							block.TryGetValue(OptionsKey.WindowWidth, ref Interface.CurrentOptions.WindowWidth, NumberRange.Positive);
							block.TryGetValue(OptionsKey.WindowHeight, ref Interface.CurrentOptions.WindowHeight, NumberRange.Positive);
							block.GetValue(OptionsKey.VSync, out Interface.CurrentOptions.VerticalSynchronization);
							block.GetValue(OptionsKey.IsUseNewRenderer, out Interface.CurrentOptions.IsUseNewRenderer);
							block.TryGetValue(OptionsKey.ViewingDistance, ref Interface.CurrentOptions.ViewingDistance, NumberRange.Positive);
							block.TryGetValue(OptionsKey.QuadLeafSize, ref Interface.CurrentOptions.QuadTreeLeafSize, NumberRange.Positive);
							break;
						case OptionsSection.Quality:
							block.GetEnumValue(OptionsKey.Interpolation, out Interface.CurrentOptions.Interpolation);
							block.TryGetValue(OptionsKey.AnisotropicFilteringLevel, ref Interface.CurrentOptions.AnisotropicFilteringLevel);
							block.TryGetValue(OptionsKey.AntiAliasingLevel, ref Interface.CurrentOptions.AntiAliasingLevel);
							block.GetEnumValue(OptionsKey.TransparencyMode, out Interface.CurrentOptions.TransparencyMode);
							block.TryGetValue(OptionsKey.ViewingDistance, ref Interface.CurrentOptions.ViewingDistance, NumberRange.Positive);
							break;
						case OptionsSection.Loading:
							block.GetValue(OptionsKey.ShowLogo, out Interface.CurrentOptions.LoadingLogo);
							block.GetValue(OptionsKey.ShowProgressBar, out Interface.CurrentOptions.LoadingProgressBar);
							block.GetValue(OptionsKey.ShowBackground, out Interface.CurrentOptions.LoadingBackground);
							break;
						case OptionsSection.Parsers:
							block.GetEnumValue(OptionsKey.XObject, out Interface.CurrentOptions.CurrentXParser);
							block.GetEnumValue(OptionsKey.ObjObject, out Interface.CurrentOptions.CurrentObjParser);
							block.GetValue(OptionsKey.GDIPlus, out Interface.CurrentOptions.UseGDIDecoders);
							break;
						case OptionsSection.Folders:
							block.GetValue(OptionsKey.RouteSearch, out string folder);
							if (Directory.Exists(folder))
							{
								Interface.CurrentOptions.RouteSearchDirectory = folder;
							}
							break;
					}
				}
			}
		}
	}
}
