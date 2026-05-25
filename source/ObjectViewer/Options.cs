using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Formats.OpenBve;
using ObjectViewer.Graphics;
using OpenBveApi;
using OpenBveApi.Input;
using Path = OpenBveApi.Path;

namespace ObjectViewer
{
	/// <summary>Holds the program specific options</summary>
	internal class Options : BaseOptions
	{
		private ObjectOptimizationMode objectOptimizationMode;

		internal string ObjectSearchDirectory;

		internal Key CameraMoveLeft;

		internal Key CameraMoveRight;

		internal Key CameraMoveUp;

		internal Key CameraMoveDown;

		internal Key CameraMoveForward;

		internal Key CameraMoveBackward;

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
						ObjectOptimizationFullThreshold = 0;
						break;
					case ObjectOptimizationMode.Low:
						ObjectOptimizationBasicThreshold = 1000;
						ObjectOptimizationFullThreshold = 250;
						break;
					case ObjectOptimizationMode.High:
						ObjectOptimizationBasicThreshold = 10000;
						ObjectOptimizationFullThreshold = 1000;
						break;
				}
			}
		}

		internal Options()
		{
			ObjectOptimizationMode = ObjectOptimizationMode.Low;
			// Shadow settings use synced base defaults
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
				Builder.AppendLine("; Object Viewer specific options file");
				Builder.AppendLine();
				Builder.AppendLine("[display]");
				Builder.AppendLine("windowWidth = " + Program.Renderer.Screen.Width.ToString(Culture));
				Builder.AppendLine("windowHeight = " + Program.Renderer.Screen.Height.ToString(Culture));
				Builder.AppendLine("isUseNewRenderer = " + (IsUseNewRenderer ? "true" : "false"));
				Builder.AppendLine("nearclipbase = " + NearClipBase.ToString(Culture));
				Builder.AppendLine("autoReloadObjects = " + (AutoReloadObjects ? "true" : "false"));
				Builder.AppendLine();
				Builder.AppendLine("[quality]");
				Builder.AppendLine("interpolation = " + Interpolation);
				Builder.AppendLine("anisotropicfilteringlevel = " + AnisotropicFilteringLevel.ToString(Culture));
				Builder.AppendLine("antialiasinglevel = " + AntiAliasingLevel.ToString(Culture));
				Builder.AppendLine("transparencyMode = " + ((int)TransparencyMode).ToString(Culture));
				Builder.AppendLine("shadowresolution = " + (int)ShadowResolution);
				Builder.AppendLine("shadowdrawdistance = " + ShadowDrawDistance);
				Builder.AppendLine("shadowcascades = " + (int)ShadowCascades);
				Builder.AppendLine("shadowstrength = " + ShadowStrength.ToString("0.00", Culture));
				Builder.AppendLine("shadowbias = " + ShadowBias.ToString("0.000000", Culture));
				Builder.AppendLine("shadownormalbias = " + ShadowNormalBias.ToString("0.00", Culture));
				Builder.AppendLine("lightazimuth = " + LightAzimuth.ToString(Culture));
				Builder.AppendLine("lightelevation = " + LightElevation.ToString(Culture));
				Builder.AppendLine("viewingDistance = " + ViewingDistance.ToString(Culture));
				Builder.AppendLine();
				Builder.AppendLine("[Parsers]");
				Builder.AppendLine("xObject = " + CurrentXParser);
				Builder.AppendLine("objObject = " + CurrentObjParser);
				Builder.AppendLine();
				Builder.AppendLine("[objectOptimization]");
				Builder.AppendLine($"mode = {ObjectOptimizationMode}");
				Builder.AppendLine();
				Builder.AppendLine("[Folders]");
				Builder.AppendLine($"objectsearch = {ObjectSearchDirectory}");
				Builder.AppendLine("[Keys]");
				Builder.AppendLine("left = " + CameraMoveLeft);
				Builder.AppendLine("right = " + CameraMoveRight);
				Builder.AppendLine("up = " + CameraMoveUp);
				Builder.AppendLine("down = " + CameraMoveDown);
				Builder.AppendLine("forward = " + CameraMoveForward);
				Builder.AppendLine("backward = " + CameraMoveBackward);
				File.WriteAllText(fileName, Builder.ToString(), new System.Text.UTF8Encoding(true));
			}
			catch
			{
				MessageBox.Show("An error occured whilst saving the options to disk." + Environment.NewLine +
								"Please ensure you have write permission.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		internal static void LoadOptions()
		{
			Interface.CurrentOptions = new Options
			{
				ViewingDistance = 1000,
				CameraMoveLeft = Key.A,
				CameraMoveRight = Key.D,
				CameraMoveUp = Key.W,
				CameraMoveDown = Key.S,
				CameraMoveForward = Key.Q,
				CameraMoveBackward = Key.E
			};
			string optionsFolder = Path.CombineDirectory(Program.FileSystem.SettingsFolder, "1.5.0");
			if (!Directory.Exists(optionsFolder))
			{
				Directory.CreateDirectory(optionsFolder);
			}
			string configFile = Path.CombineFile(optionsFolder, "options_ov.cfg");
			if (!File.Exists(configFile))
			{
				//Attempt to load and upgrade a prior configuration file
				string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				configFile = Path.CombineFile(Path.CombineDirectory(Path.CombineDirectory(assemblyFolder, "UserData"), "Settings"), "options_ov.cfg");

				if (!File.Exists(configFile))
				{
					//If no object viewer specific configuration file exists, then try the main OpenBVE configuration file
					//Write out to a new viewer specific file though
					configFile = Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/options.cfg");
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
							block.TryGetValue(OptionsKey.WindowWidth, ref Interface.CurrentOptions.WindowWidth, NumberRange.Positive);
							block.TryGetValue(OptionsKey.WindowHeight, ref Interface.CurrentOptions.WindowHeight, NumberRange.Positive);
							block.TryGetValue(OptionsKey.IsUseNewRenderer, ref Interface.CurrentOptions.IsUseNewRenderer);
							block.TryGetValue(OptionsKey.NearClipBase, ref Interface.CurrentOptions.NearClipBase, NumberRange.Positive);
							// ensure viewing distance is greater than the near clipping plane to avoid rendering issues
							if (Interface.CurrentOptions.ViewingDistance <= Interface.CurrentOptions.NearClipBase)

							{
								Interface.CurrentOptions.ViewingDistance = (int)Math.Ceiling(Interface.CurrentOptions.NearClipBase) + 1;
							}

							block.GetValue(OptionsKey.IsUseNewRenderer, out Interface.CurrentOptions.IsUseNewRenderer);
							block.GetValue(OptionsKey.AutoReloadObjects, out Interface.CurrentOptions.AutoReloadObjects);
							block.TryGetValue(OptionsKey.ViewingDistance, ref Interface.CurrentOptions.ViewingDistance, NumberRange.Positive);
							break;
						case OptionsSection.Quality:
							block.GetEnumValue(OptionsKey.Interpolation, out Interface.CurrentOptions.Interpolation);
							block.TryGetValue(OptionsKey.AnisotropicFilteringLevel, ref Interface.CurrentOptions.AnisotropicFilteringLevel);
							block.TryGetValue(OptionsKey.AntiAliasingLevel, ref Interface.CurrentOptions.AntiAliasingLevel);
							block.GetEnumValue(OptionsKey.TransparencyMode, out Interface.CurrentOptions.TransparencyMode);
							block.TryGetEnumValue(OptionsKey.ShadowResolution, ref Interface.CurrentOptions.ShadowResolution);
							block.TryGetEnumValue(OptionsKey.ShadowDrawDistance, ref Interface.CurrentOptions.ShadowDrawDistance);
							block.TryGetEnumValue(OptionsKey.ShadowCascades, ref Interface.CurrentOptions.ShadowCascades);
							block.TryGetValue(OptionsKey.ShadowStrength, ref Interface.CurrentOptions.ShadowStrength, NumberRange.Positive);
							block.TryGetValue(OptionsKey.ShadowBias, ref Interface.CurrentOptions.ShadowBias);
							block.TryGetValue(OptionsKey.ShadowNormalBias, ref Interface.CurrentOptions.ShadowNormalBias);
							block.TryGetValue(OptionsKey.LightAzimuth, ref Interface.CurrentOptions.LightAzimuth);
							block.TryGetValue(OptionsKey.LightElevation, ref Interface.CurrentOptions.LightElevation);
							block.TryGetValue(OptionsKey.ViewingDistance, ref Interface.CurrentOptions.ViewingDistance, NumberRange.Positive);
							break;
						case OptionsSection.Parsers:
							block.GetEnumValue(OptionsKey.XObject, out Interface.CurrentOptions.CurrentXParser);
							block.GetEnumValue(OptionsKey.ObjObject, out Interface.CurrentOptions.CurrentObjParser);
							block.GetValue(OptionsKey.GDIPlus, out Interface.CurrentOptions.UseGDIDecoders);
							break;
						case OptionsSection.ObjectOptimization:
							block.GetEnumValue(OptionsKey.Mode, out ObjectOptimizationMode mode);
							Interface.CurrentOptions.ObjectOptimizationMode = mode; // can't set an accessor value directly
							break;
						case OptionsSection.Folders:
							block.GetValue(OptionsKey.ObjectSearch, out string folder);
							if (Directory.Exists(folder))
							{
								Interface.CurrentOptions.ObjectSearchDirectory = folder;
							}
							break;
						case OptionsSection.Keys:
							block.GetEnumValue(OptionsKey.Left, out Interface.CurrentOptions.CameraMoveLeft);
							block.GetEnumValue(OptionsKey.Right, out Interface.CurrentOptions.CameraMoveRight);
							block.GetEnumValue(OptionsKey.Up, out Interface.CurrentOptions.CameraMoveUp);
							block.GetEnumValue(OptionsKey.Down, out Interface.CurrentOptions.CameraMoveDown);
							block.GetEnumValue(OptionsKey.Forward, out Interface.CurrentOptions.CameraMoveForward);
							block.GetEnumValue(OptionsKey.Backward, out Interface.CurrentOptions.CameraMoveBackward);
							break;

					}
				}
			}
		}
	}
}
