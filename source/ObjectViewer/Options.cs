using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Formats.OpenBve;
using ObjectViewer.Graphics;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Hosts;
using OpenBveApi.Input;
using Path = OpenBveApi.Path;

namespace ObjectViewer
{
	/// <summary>Holds the program specific options</summary>
	internal class Options : BaseOptions
	{
		private ObjectOptimizationMode objectOptimizationMode;

		internal int FPSLimit;

		internal string ObjectSearchDirectory;

		internal Key CameraMoveLeft;

		internal Key CameraMoveRight;

		internal Key CameraMoveUp;

		internal Key CameraMoveDown;

		internal Key CameraMoveForward;

		internal Key CameraMoveBackward;

		internal Color24 BackgroundColor;

		internal Color32 TextColor;

		/// <summary>Whether the loading screen shows the decode progress bar</summary>
		internal bool LoadingProgressBar = true;

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

		internal Options(HostInterface host) : base(host)
		{
			VerticalSynchronization = true;
			FPSLimit = 0;
			ObjectOptimizationMode = ObjectOptimizationMode.Low;
			ViewingDistance = 1000; // fixed
			CameraMoveLeft = Key.A;
			CameraMoveRight = Key.D;
			CameraMoveUp = Key.W;
			CameraMoveDown = Key.S;
			CameraMoveForward = Key.Q;
			CameraMoveBackward = Key.E;
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
				Builder.AppendLine("vsync = " + (VerticalSynchronization ? "true" : "false"));
				Builder.AppendLine("fpslimit = " + FPSLimit.ToString(Culture));
				Builder.AppendLine("windowWidth = " + Program.Renderer.Screen.Width.ToString(Culture));
				Builder.AppendLine("windowHeight = " + Program.Renderer.Screen.Height.ToString(Culture));
				Builder.AppendLine("nearclipbase = " + NearClipBase.ToString(Culture));
				Builder.AppendLine("autoReloadObjects = " + (AutoReloadObjects ? "true" : "false"));
				Builder.AppendLine("showprogressbar = " + (LoadingProgressBar ? "true" : "false"));
				Builder.AppendLine("backgroundColor = " + BackgroundColor);
				Builder.AppendLine("textColor = " + TextColor);
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

		public override void Load()
		{
			string optionsFolder = Path.CombineDirectory(CurrentHost.FileSystem.SettingsFolder, "1.5.0");
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
					configFile = Path.CombineFile(CurrentHost.FileSystem.SettingsFolder, "1.5.0/options.cfg");
				}
			}

			if (File.Exists(configFile))
			{
				ConfigFile<OptionsSection, OptionsKey> cfg = new ConfigFile<OptionsSection, OptionsKey>(File.ReadAllLines(configFile, new System.Text.UTF8Encoding()), configFile, CurrentHost);

				while (cfg.RemainingSubBlocks > 0)
				{
					Block<OptionsSection, OptionsKey> block = cfg.ReadNextBlock();
					switch (block.Key)
					{
						case OptionsSection.Display:
							block.TryGetValue(OptionsKey.WindowWidth, ref WindowWidth, NumberRange.Positive);
							block.TryGetValue(OptionsKey.WindowHeight, ref WindowHeight, NumberRange.Positive);
							block.TryGetValue(OptionsKey.NearClipBase, ref NearClipBase, NumberRange.Positive);
							block.GetValue(OptionsKey.VSync, out VerticalSynchronization);
							block.GetValue(OptionsKey.FPSLimit, out FPSLimit);
							if (FPSLimit < 0)
							{
								FPSLimit = 0;
							}
							// ensure viewing distance is greater than the near clipping plane to avoid rendering issues
							if (ViewingDistance <= NearClipBase)
							{
								ViewingDistance = (int)Math.Ceiling(NearClipBase) + 1;
							}

							block.GetValue(OptionsKey.AutoReloadObjects, out AutoReloadObjects);
							block.GetValue(OptionsKey.ShowProgressBar, out LoadingProgressBar);
							block.GetColor24(OptionsKey.BackgroundColor, out BackgroundColor);
							block.GetColor32(OptionsKey.TextColor, out TextColor);
							break;
						case OptionsSection.Quality:
							block.GetEnumValue(OptionsKey.Interpolation, out Interpolation);
							block.TryGetValue(OptionsKey.AnisotropicFilteringLevel, ref AnisotropicFilteringLevel);
							block.TryGetValue(OptionsKey.AntiAliasingLevel, ref AntiAliasingLevel);
							block.GetEnumValue(OptionsKey.TransparencyMode, out TransparencyMode);
							block.TryGetEnumValue(OptionsKey.ShadowResolution, ref ShadowResolution);
							block.TryGetEnumValue(OptionsKey.ShadowDrawDistance, ref ShadowDrawDistance);
							block.TryGetEnumValue(OptionsKey.ShadowCascades, ref ShadowCascades);
							block.TryGetValue(OptionsKey.ShadowStrength, ref ShadowStrength, NumberRange.Positive);
							block.TryGetValue(OptionsKey.ShadowBias, ref ShadowBias);
							block.TryGetValue(OptionsKey.ShadowNormalBias, ref ShadowNormalBias);
							block.TryGetValue(OptionsKey.LightAzimuth, ref LightAzimuth);
							block.TryGetValue(OptionsKey.LightElevation, ref LightElevation);
							break;
						case OptionsSection.Parsers:
							block.GetEnumValue(OptionsKey.XObject, out CurrentXParser);
							block.GetEnumValue(OptionsKey.ObjObject, out CurrentObjParser);
							block.GetValue(OptionsKey.GDIPlus, out UseGDIDecoders);
							break;
						case OptionsSection.ObjectOptimization:
							block.GetEnumValue(OptionsKey.Mode, out ObjectOptimizationMode mode);
							ObjectOptimizationMode = mode; // can't set an accessor value directly
							break;
						case OptionsSection.Folders:
							block.GetValue(OptionsKey.ObjectSearch, out string folder);
							if (Directory.Exists(folder))
							{
								ObjectSearchDirectory = folder;
							}
							break;
						case OptionsSection.Keys:
							CameraMoveLeft = GetCameraKey(block, OptionsKey.Left, Key.A);
							CameraMoveRight = GetCameraKey(block, OptionsKey.Right, Key.D);
							CameraMoveUp = GetCameraKey(block, OptionsKey.Up, Key.W);
							CameraMoveDown = GetCameraKey(block, OptionsKey.Down, Key.S);
							CameraMoveForward = GetCameraKey(block, OptionsKey.Forward, Key.Q);
							CameraMoveBackward = GetCameraKey(block, OptionsKey.Backward, Key.E);
							break;

					}
				}
			}
		}

		/// <summary>Reads a camera key: Disabled stays disabled, Unknown / missing / invalid fall back to default</summary>
		private static Key GetCameraKey(Block<OptionsSection, OptionsKey> block, OptionsKey option, Key fallback)
		{
			if (!block.GetValue(option, out string raw))
				return fallback;
			raw = raw.Trim();
			if (Enum.TryParse(raw, true, out Key key) && key != Key.Unknown && key != Key.LastKey)
				return key;
			if (!raw.Equals("unknown", StringComparison.OrdinalIgnoreCase))
				Program.CurrentHost.AddMessage(OpenBveApi.Interface.MessageType.Error, false, "Value " + raw + " is invalid in " + option + " in " + block.Key + " in file " + block.FileName);
			return fallback;
		}
	}
}
