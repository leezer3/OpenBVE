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
		private ObjectOptimizationMode objectOptimizationMode;

		internal bool LoadingProgressBar;
		internal bool LoadingLogo;
		internal bool LoadingBackground;
		internal string RouteSearchDirectory;
		internal int FPSLimit;

		/// <summary>
		/// The mode of optimization to be performed on an object
		/// </summary>
		internal ObjectOptimizationMode ObjectOptimizationMode
		{
			get => objectOptimizationMode;
			set
			{
				objectOptimizationMode = value;

				switch (value)
				{
					case ObjectOptimizationMode.None:
						ObjectOptimizationBasicThreshold = 0;
						break;
					case ObjectOptimizationMode.Low:
						ObjectOptimizationBasicThreshold = 1000;
						break;
					case ObjectOptimizationMode.High:
						ObjectOptimizationBasicThreshold = 10000;
						break;
				}
			}
		}

		internal Options()
		{
			VerticalSynchronization = true;
			FPSLimit = 0;
			ObjectOptimizationMode = ObjectOptimizationMode.Low;
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
				Builder.AppendLine("fpslimit = " + FPSLimit.ToString(Culture));
				Builder.AppendLine("windowWidth = " + Program.Renderer.Screen.Width.ToString(Culture));
				Builder.AppendLine("windowHeight = " + Program.Renderer.Screen.Height.ToString(Culture));
				Builder.AppendLine("viewingdistance = " + ViewingDistance);
				Builder.AppendLine("nearclipbase = " + NearClipBase.ToString(Culture));
				Builder.AppendLine("quadleafsize = " + QuadTreeLeafSize);
				Builder.AppendLine();
				Builder.AppendLine("[quality]");
				Builder.AppendLine("interpolation = " + Interpolation);
				Builder.AppendLine("anisotropicfilteringlevel = " + AnisotropicFilteringLevel.ToString(Culture));
				Builder.AppendLine("antialiasinglevel = " + AntiAliasingLevel.ToString(Culture));
				Builder.AppendLine("transparencyMode = " + ((int)TransparencyMode).ToString(Culture));
				Builder.AppendLine("viewingdistance = " + ViewingDistance);
				Builder.AppendLine("nearclipbase = " + NearClipBase.ToString(Culture));
				Builder.AppendLine("quadleafsize = " + QuadTreeLeafSize);
				Builder.AppendLine("shadowresolution = " + (int)ShadowResolution);
				Builder.AppendLine("shadowdrawdistance = " + ShadowDrawDistance);
				Builder.AppendLine("shadowcascades = " + (int)ShadowCascades);
				Builder.AppendLine("shadowstrength = " + ShadowStrength.ToString("0.00", Culture));
				Builder.AppendLine("shadowbias = " + ShadowBias.ToString("0.000000", Culture));
				Builder.AppendLine("shadownormalbias = " + ShadowNormalBias.ToString("0.00", Culture));
				Builder.AppendLine("lightazimuth = " + LightAzimuth.ToString(Culture));
				Builder.AppendLine("lightelevation = " + LightElevation.ToString(Culture));
				Builder.AppendLine();
				Builder.AppendLine("[loading]");
				Builder.AppendLine("showlogo = " + (LoadingLogo ? "true" : "false"));
				Builder.AppendLine("showprogressbar = " + (LoadingProgressBar ? "true" : "false"));
				Builder.AppendLine("showbackground = " + (LoadingBackground ? "true" : "false"));
			Builder.AppendLine("[objectOptimization]");
			Builder.AppendLine($"mode = {ObjectOptimizationMode}");
			Builder.AppendLine();
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

		public override void Load()
		{
			string optionsFolder = Path.CombineDirectory(Program.CurrentHost.FileSystem.SettingsFolder, "1.5.0");
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
					//Write out to a new Route Viewer specific file though
					configFile = Path.CombineFile(Program.CurrentHost.FileSystem.SettingsFolder, "1.5.0/options.cfg");
				}
			}

			if (File.Exists(configFile))
			{
				ConfigFile<OptionsSection, OptionsKey> cfg = new ConfigFile<OptionsSection, OptionsKey>(File.ReadAllLines(configFile, new System.Text.UTF8Encoding()), configFile, Program.CurrentHost);

				while (cfg.RemainingSubBlocks > 0)
				{
					Block<OptionsSection, OptionsKey> block = cfg.ReadNextBlock();
					switch (block.Key)
					{
						case OptionsSection.Display:
							block.TryGetValue(OptionsKey.WindowWidth, ref WindowWidth, NumberRange.Positive);
							block.TryGetValue(OptionsKey.WindowHeight, ref WindowHeight, NumberRange.Positive);
							block.GetValue(OptionsKey.VSync, out VerticalSynchronization);
							block.GetValue(OptionsKey.FPSLimit, out FPSLimit);
							if (FPSLimit < 0)
							{
								FPSLimit = 0;
							}
							block.TryGetValue(OptionsKey.ViewingDistance, ref ViewingDistance, NumberRange.Positive);
							block.TryGetValue(OptionsKey.QuadLeafSize, ref QuadTreeLeafSize, NumberRange.Positive);
							block.TryGetValue(OptionsKey.NearClipBase, ref NearClipBase, NumberRange.Positive);
							// ensure viewing distance is greater than the near clipping plane to avoid rendering issues
							if (ViewingDistance <= NearClipBase)

							{
								ViewingDistance = (int)Math.Ceiling(NearClipBase) + 1;
							}

							break;
						case OptionsSection.Quality:
							block.GetEnumValue(OptionsKey.Interpolation, out Interpolation);
							block.TryGetValue(OptionsKey.AnisotropicFilteringLevel, ref AnisotropicFilteringLevel);
							block.TryGetValue(OptionsKey.AntiAliasingLevel, ref AntiAliasingLevel);
							block.GetEnumValue(OptionsKey.TransparencyMode, out TransparencyMode);
							block.TryGetValue(OptionsKey.ViewingDistance, ref ViewingDistance, NumberRange.Positive);
							block.TryGetValue(OptionsKey.QuadLeafSize, ref QuadTreeLeafSize, NumberRange.Positive);
							block.TryGetValue(OptionsKey.NearClipBase, ref NearClipBase, NumberRange.Positive);
							// ensure viewing distance is greater than the near clipping plane to avoid rendering issues
							if (ViewingDistance <= NearClipBase)

							{
								ViewingDistance = (int)Math.Ceiling(NearClipBase) + 1;
							}
							block.TryGetEnumValue(OptionsKey.ShadowResolution, ref ShadowResolution);
							block.TryGetEnumValue(OptionsKey.ShadowDrawDistance, ref ShadowDrawDistance);
							block.TryGetEnumValue(OptionsKey.ShadowCascades, ref ShadowCascades);
							block.TryGetValue(OptionsKey.ShadowStrength, ref ShadowStrength, NumberRange.Positive);
							block.TryGetValue(OptionsKey.ShadowBias, ref ShadowBias);
							block.TryGetValue(OptionsKey.ShadowNormalBias, ref ShadowNormalBias);
							block.TryGetValue(OptionsKey.LightAzimuth, ref LightAzimuth);
							block.TryGetValue(OptionsKey.LightElevation, ref LightElevation);
							break;
						case OptionsSection.Loading:
							block.GetValue(OptionsKey.ShowLogo, out LoadingLogo);
							block.GetValue(OptionsKey.ShowProgressBar, out LoadingProgressBar);
							block.GetValue(OptionsKey.ShowBackground, out LoadingBackground);
							break;
					case OptionsSection.Parsers:
						block.GetEnumValue(OptionsKey.XObject, out CurrentXParser);
						block.GetEnumValue(OptionsKey.ObjObject, out CurrentObjParser);
						block.GetValue(OptionsKey.GDIPlus, out UseGDIDecoders);
						break;
					case OptionsSection.ObjectOptimization:
						block.GetEnumValue(OptionsKey.Mode, out ObjectOptimizationMode mode);
						ObjectOptimizationMode = mode;
						break;
					case OptionsSection.Folders:
							block.GetValue(OptionsKey.RouteSearch, out string folder);
							if (Directory.Exists(folder))
							{
								RouteSearchDirectory = folder;
							}
							break;
					}
				}
			}
		}
	}
}
