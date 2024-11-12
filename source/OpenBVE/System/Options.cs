using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Formats.OpenBve;
using LibRender2.MotionBlurs;
using LibRender2.Overlays;
using OpenBveApi;
using OpenBveApi.Graphics;
using OpenBveApi.Hosts;
using OpenBveApi.Objects;
using SoundManager;
using CompressionType = OpenBveApi.Packages.CompressionType;
using Path = OpenBveApi.Path;

namespace OpenBve
{
	internal partial class Interface
	{
		internal class Options : BaseOptions
		{
			/// <summary>The on disk folder in which user interface components are stored</summary>
			internal string UserInterfaceFolder;
			/// <summary>The accelerated time factor (1x to 5x)</summary>
			internal int TimeAccelerationFactor;
			///// <summary>The current type of motion blur</summary>
			internal MotionBlurMode MotionBlur;
			/// <summary>Whether duplicate verticies are culled during loading</summary>
			internal bool ObjectOptimizationVertexCulling;
			/// <summary>Whether collisions between trains are enabled</summary>
			internal bool Collisions;
			/// <summary>Whether the black-box data logger is enabled</summary>
			internal bool BlackBox;
			/// <summary>Whether joystick support is enabled</summary>
			internal bool UseJoysticks;
			/// <summary>The threshold below which joystick axis motion will be disregarded</summary>
			internal double JoystickAxisThreshold;
			/// <summary>The delay after which a held-down key will start to repeat</summary>
			internal double KeyRepeatDelay;
			/// <summary>The interval at which a held down key will repeat after the intial delay</summary>
			internal double KeyRepeatInterval;
			/// <summary>The current sound model</summary>
			internal SoundModels SoundModel;
			/// <summary>The range outside of which sounds will be inaudible</summary>
			internal SoundRange SoundRange;
			/// <summary>Whether warning messages are to be shown</summary>
			internal bool ShowWarningMessages;
			/// <summary>Whether error messages are to be shown</summary>
			internal bool ShowErrorMessages;
			/// <summary>The current route's on-disk folder path</summary>
			internal string RouteFolder;
			/// <summary>The current train's on-disk folder path</summary>
			internal string TrainFolder;
			/// <summary>The list of recently used routes</summary>
			internal string[] RecentlyUsedRoutes;
			/// <summary>The list of recently used trains</summary>
			internal string[] RecentlyUsedTrains;
			/// <summary>The maximum number of recently used routes/ trains to display</summary>
			internal readonly int RecentlyUsedLimit;
			/// <summary>The list of recently used route character encodings</summary>
			internal TextEncoding.EncodingValue[] RouteEncodings;
			/// <summary>The list of recently used train character encodings</summary>
			internal TextEncoding.EncodingValue[] TrainEncodings;
			/// <summary>The previous game mode, used for calculating the score in the main menu</summary>
			/// <remarks>This is loaded from the black-box log if enabled, not the main options file</remarks>
			internal GameMode PreviousGameMode;
			/// <summary>The width of the main menu window</summary>
			internal int MainMenuWidth;
			/// <summary>The height of the main menu window</summary>
			internal int MainMenuHeight;
			/// <summary>Whether the simulation will load all textures and sounds into system memory on initial load</summary>
			internal bool LoadInAdvance;
			/// <summary>Whether the simulation will dynamically unload unused textures</summary>
			internal bool UnloadUnusedTextures;
			/// <summary>Whether EB application is possible from the use of a joystick axis</summary>
			internal bool AllowAxisEB;
			/// <summary>Whether to prefer the native OpenTK operating system backend</summary>
			internal bool PreferNativeBackend = true;
			/// <summary>Stores whether the RailDriver speed display is in MPH (true) or KPH (false)</summary>
			internal bool RailDriverMPH;
			/// <summary>The list of enable Input Device Plugins</summary>
			internal string[] EnableInputDevicePlugins;
			/// <summary>The time in seconds after which the mouse cursor is hidden</summary>
			/// <remarks>Set to zero to never hide the cursor</remarks>
			internal double CursorHideDelay;
			/// <summary>Whether a screen reader is available</summary>
			/// <remarks>Not saved, detected on game init</remarks>
			internal bool ScreenReaderAvailable;
			
			internal TimeTableMode TimeTableStyle;

			internal CompressionType packageCompressionType;
			/*
			 * Only relevant in developer mode, not saved
			 */
			internal bool ShowEvents = false;

			/// <summary>Whether we are currently in kiosk mode</summary>
			internal bool KioskMode;
			/// <summary>The timer before AI controls are enabled in kiosk mode</summary>
			internal double KioskModeTimer; //5 minutes by default set in ctor
			/// <summary>The maximum upper limit for FPS</summary>
			/// https://github.com/leezer3/OpenBVE/issues/941
			internal int FPSLimit;

			internal bool DailyBuildUpdates;
			
			/// <summary>Creates a new instance of the options class with default values set</summary>
			internal Options()
			{
				LanguageCode = "en-US";
				FullscreenMode = false;
				VerticalSynchronization = true;
				WindowWidth = 960;
				WindowHeight = 600;
				FullscreenWidth = 1024;
				FullscreenHeight = 768;
				FullscreenBits = 32;
				UserInterfaceFolder = "Default";
				Interpolation = InterpolationMode.BilinearMipmapped;
				TransparencyMode = TransparencyMode.Quality;
				AnisotropicFilteringLevel = 0;
				AnisotropicFilteringMaximum = 0;
				AntiAliasingLevel = 0;
				ViewingDistance = 600;
				QuadTreeLeafSize = 60;
				MotionBlur = MotionBlurMode.None;
				Toppling = true;
				Collisions = true;
				Derailments = true;
				LoadingSway = false;
				GameMode = GameMode.Normal;
				BlackBox = false;
				UseJoysticks = true;
				JoystickAxisThreshold = 0.0;
				KeyRepeatDelay = 0.5;
				KeyRepeatInterval = 0.1;
				SoundModel = SoundModels.Inverse;
				SoundRange = SoundRange.Low;
				SoundNumber = 16;
				ShowWarningMessages = true;
				ShowErrorMessages = true;
				ObjectOptimizationBasicThreshold = 10000;
				ObjectOptimizationFullThreshold = 1000;
				ObjectOptimizationVertexCulling = false;
				RouteFolder = "";
				TrainFolder = "";
				RecentlyUsedRoutes = new string[] { };
				RecentlyUsedTrains = new string[] { };
				RecentlyUsedLimit = 10;
				RouteEncodings = new TextEncoding.EncodingValue[] { };
				TrainEncodings = new TextEncoding.EncodingValue[] { };
				MainMenuWidth = 0;
				MainMenuHeight = 0;
				LoadInAdvance = false;
				UnloadUnusedTextures = false;
				TimeAccelerationFactor = 5;
				AllowAxisEB = true;
				TimeTableStyle = TimeTableMode.Default;
				packageCompressionType = CompressionType.Zip;
				RailDriverMPH = true;
				EnableBveTsHacks = true;
				OldTransparencyMode = true;
				KioskMode = false;
				KioskModeTimer = 300;
				EnableInputDevicePlugins = new string[] { };
				CursorFileName = "nk.png";
				Panel2ExtendedMode = false;
				Panel2ExtendedMinSize = 128;
				CurrentXParser = XParsers.NewXParser; //Set to new X parser by default
				CurrentObjParser = ObjParsers.Original; //Set to original Obj parser by default
				CursorHideDelay = 10;
				Accessibility = false;
				ScreenReaderAvailable = false;
				ForceForwardsCompatibleContext = false;
				IsUseNewRenderer = true;
				DailyBuildUpdates = false;
				UseGDIDecoders = false;
				EnableBve5ScriptedTrain = true;
				UserInterfaceScaleFactor = 1;
				CultureInfo currentCultureInfo = CultureInfo.CurrentCulture;
				switch (Program.CurrentHost.Platform)
				{
					case HostPlatform.AppleOSX:
						// This gets us a much better Unicode glyph set on Apple
						Font = "Arial Unicode MS";
						break;
					case HostPlatform.FreeBSD:
					case HostPlatform.GNULinux:
						/*
						 * Font support on Mono / Linux is a real mess, nothing with a full Unicode glyph set installed by default
						 * Try and detect + select a sensible Noto Sans variant based upon our current locale
						 */
						switch (currentCultureInfo.Name)
						{
							case "ru-RU":
							case "uk-UA":
							case "ro-RO":
							case "hu-HU":
								// Plain Noto Sans has better Cyrillic glyphs
								Font = "Noto Sans";
								break;
							case "jp-JP":
							case "zh-TW":
							case "zh-CN":
							case "zh-HK":
								// For JP / CN, use the Japanese version of Noto Sans
								Font = "Noto Sans CJK JP";
								break;
							case "ko-KR":
								// Korean version of Noto Sans
								Font = "Noto Sans CJK KR";
								break;
							default:
								// By default, use the Japanese version of Noto Sans- whilst this lacks some glyphs, it's the best overall
								Font = "Noto Sans CJK JP";
								break;
						}
						FPSLimit = 200;
						break;
					case HostPlatform.WINE:
						// WINE reports slightly different font names to native
						switch (currentCultureInfo.Name)
						{
							case "ru-RU":
							case "uk-UA":
							case "ro-RO":
							case "hu-HU":
								// Plain Noto Sans has better Cyrillic glyphs
								Font = "Noto Sans Regular";
								break;
							case "jp-JP":
							case "zh-TW":
							case "zh-CN":
							case "zh-HK":
								// For JP / CN, use the Japanese version of Noto Sans
								Font = "Noto Sans CJK JP Regular";
								break;
							case "ko-KR":
								// Korean version of Noto Sans
								Font = "Noto Sans CJK KR Regular";
								break;
							default:
								// By default, use the Japanese version of Noto Sans- whilst this lacks some glyphs, it's the best overall
								Font = "Noto Sans CJK JP Regular";
								break;
						}
						break;
					default:
						// This is what's set by default for WinForms
						Font = "Microsoft Sans Serif";
						break;
				}
			}

			/// <summary>Saves the options to the specified filename</summary>
			/// <param name="fileName">The filename to save the options to</param>
			public override void Save(string fileName)
			{
				CultureInfo Culture = CultureInfo.InvariantCulture;
				StringBuilder Builder = new StringBuilder();
				Builder.AppendLine("; Options");
				Builder.AppendLine("; =======");
				Builder.AppendLine("; This file was automatically generated. Please modify only if you know what you're doing.");
				Builder.AppendLine();
				Builder.AppendLine("[language]");
				Builder.AppendLine("code = " + LanguageCode);
				Builder.AppendLine();
				Builder.AppendLine("[interface]");
				Builder.AppendLine("folder = " + UserInterfaceFolder);
				{
					string t;
					switch (TimeTableStyle)
					{
						case TimeTableMode.None:
							t = "none";
							break;
						case TimeTableMode.Default:
							t = "default";
							break;
						case TimeTableMode.AutoGenerated:
							t = "autogenerated";
							break;
						case TimeTableMode.PreferCustom:
							t = "prefercustom";
							break;
						default:
							t = "default";
							break;
					}

					Builder.AppendLine("timetablemode = " + t);
				}
				Builder.AppendLine("kioskMode = " + (KioskMode ? "true" : "false"));
				Builder.AppendLine("kioskModeTimer = " + KioskModeTimer);
				Builder.AppendLine("accessibility = " + (Accessibility ? "true" : "false"));
				Builder.AppendLine("font = " + Font);
				Builder.AppendLine("dailybuildupdates = " + (DailyBuildUpdates ? "true" : "false"));
				Builder.AppendLine();
				Builder.AppendLine("[display]");
				Builder.AppendLine("preferNativeBackend = " + (PreferNativeBackend ? "true" : "false"));
				Builder.AppendLine("mode = " + (FullscreenMode ? "fullscreen" : "window"));
				Builder.AppendLine("vsync = " + (VerticalSynchronization ? "true" : "false"));
				Builder.AppendLine("windowWidth = " + WindowWidth.ToString(Culture));
				Builder.AppendLine("windowHeight = " + WindowHeight.ToString(Culture));
				Builder.AppendLine("fullscreenWidth = " + FullscreenWidth.ToString(Culture));
				Builder.AppendLine("fullscreenHeight = " + FullscreenHeight.ToString(Culture));
				Builder.AppendLine("fullscreenBits = " + FullscreenBits.ToString(Culture));
				Builder.AppendLine("mainmenuWidth = " + MainMenuWidth.ToString(Culture));
				Builder.AppendLine("mainmenuHeight = " + MainMenuHeight.ToString(Culture));
				Builder.AppendLine("loadInAdvance = " + (LoadInAdvance ? "true" : "false"));
				Builder.AppendLine("unloadtextures = " + (UnloadUnusedTextures ? "true" : "false"));
				Builder.AppendLine("isUseNewRenderer = " + (IsUseNewRenderer ? "true" : "false"));
				Builder.AppendLine("forwardsCompatibleContext = " + (ForceForwardsCompatibleContext ? "true" : "false"));
				Builder.AppendLine("uiscalefactor = " + UserInterfaceScaleFactor);
				Builder.AppendLine();
				Builder.AppendLine("[quality]");
				{
					string t;
					switch (Interpolation)
					{
						case InterpolationMode.NearestNeighbor:
							t = "nearestNeighbor";
							break;
						case InterpolationMode.Bilinear:
							t = "bilinear";
							break;
						case InterpolationMode.NearestNeighborMipmapped:
							t = "nearestNeighborMipmapped";
							break;
						case InterpolationMode.BilinearMipmapped:
							t = "bilinearMipmapped";
							break;
						case InterpolationMode.TrilinearMipmapped:
							t = "trilinearMipmapped";
							break;
						case InterpolationMode.AnisotropicFiltering:
							t = "anisotropicFiltering";
							break;
						default:
							t = "bilinearMipmapped";
							break;
					}

					Builder.AppendLine("interpolation = " + t);
				}
				Builder.AppendLine("anisotropicFilteringLevel = " + AnisotropicFilteringLevel.ToString(Culture));
				Builder.AppendLine("anisotropicFilteringMaximum = " + AnisotropicFilteringMaximum.ToString(Culture));
				Builder.AppendLine("antiAliasingLevel = " + AntiAliasingLevel.ToString(Culture));
				Builder.AppendLine("transparencyMode = " + ((int)TransparencyMode).ToString(Culture));
				Builder.AppendLine("oldtransparencymode = " + (OldTransparencyMode ? "true" : "false"));
				Builder.AppendLine("viewingDistance = " + ViewingDistance.ToString(Culture));
				Builder.AppendLine("quadLeafSize = " + QuadTreeLeafSize.ToString(Culture));
				{
					string t;
					switch (MotionBlur)
					{
						case MotionBlurMode.Low:
							t = "low";
							break;
						case MotionBlurMode.Medium:
							t = "medium";
							break;
						case MotionBlurMode.High:
							t = "high";
							break;
						default:
							t = "none";
							break;
					}

					Builder.AppendLine("motionBlur = " + t);
				}
				Builder.AppendLine("fpslimit = " + FPSLimit.ToString(Culture));
				Builder.AppendLine();
				Builder.AppendLine("[objectOptimization]");
				Builder.AppendLine("basicThreshold = " + ObjectOptimizationBasicThreshold.ToString(Culture));
				Builder.AppendLine("fullThreshold = " + ObjectOptimizationFullThreshold.ToString(Culture));
				Builder.AppendLine("vertexCulling = " + ObjectOptimizationVertexCulling.ToString(Culture));
				Builder.AppendLine();
				Builder.AppendLine("[simulation]");
				Builder.AppendLine("toppling = " + (Toppling ? "true" : "false"));
				Builder.AppendLine("collisions = " + (Collisions ? "true" : "false"));
				Builder.AppendLine("derailments = " + (Derailments ? "true" : "false"));
				Builder.AppendLine("loadingsway = " + (LoadingSway ? "true" : "false"));
				Builder.AppendLine("blackbox = " + (BlackBox ? "true" : "false"));
				Builder.Append("mode = ");
				switch (GameMode)
				{
					case GameMode.Arcade:
						Builder.AppendLine("arcade");
						break;
					case GameMode.Normal:
						Builder.AppendLine("normal");
						break;
					case GameMode.Expert:
						Builder.AppendLine("expert");
						break;
					default:
						Builder.AppendLine("normal");
						break;
				}

				Builder.AppendLine("acceleratedtimefactor = " + TimeAccelerationFactor);
				Builder.AppendLine("enablebvetshacks = " + (EnableBveTsHacks ? "true" : "false"));
				Builder.AppendLine("enablebve5scriptedtrain = " + (EnableBve5ScriptedTrain ? "true" : "false"));
				Builder.AppendLine();
				Builder.AppendLine("[verbosity]");
				Builder.AppendLine("showWarningMessages = " + (ShowWarningMessages ? "true" : "false"));
				Builder.AppendLine("showErrorMessages = " + (ShowErrorMessages ? "true" : "false"));
				Builder.AppendLine("debugLog = " + (GenerateDebugLogging ? "true" : "false"));
				Builder.AppendLine();
				Builder.AppendLine("[controls]");
				Builder.AppendLine("useJoysticks = " + (UseJoysticks ? "true" : "false"));
				Builder.AppendLine("joystickAxisEB = " + (AllowAxisEB ? "true" : "false"));
				Builder.AppendLine("joystickAxisthreshold = " + JoystickAxisThreshold.ToString(Culture));
				Builder.AppendLine("keyRepeatDelay = " + (1000.0 * KeyRepeatDelay).ToString("0", Culture));
				Builder.AppendLine("keyRepeatInterval = " + (1000.0 * KeyRepeatInterval).ToString("0", Culture));
				Builder.AppendLine("raildrivermph = " + (RailDriverMPH ? "true" : "false"));
				Builder.AppendLine();
				Builder.AppendLine("[sound]");
				Builder.Append("model = ");
				switch (SoundModel)
				{
					case SoundModels.Linear:
						Builder.AppendLine("linear");
						break;
					default:
						Builder.AppendLine("inverse");
						break;
				}

				Builder.Append("range = ");
				switch (SoundRange)
				{
					case SoundRange.Low:
						Builder.AppendLine("low");
						break;
					case SoundRange.Medium:
						Builder.AppendLine("medium");
						break;
					case SoundRange.High:
						Builder.AppendLine("high");
						break;
					default:
						Builder.AppendLine("low");
						break;
				}

				Builder.AppendLine("number = " + SoundNumber.ToString(Culture));
				Builder.AppendLine();
				Builder.AppendLine("[packages]");
				Builder.Append("compression = ");
				switch (packageCompressionType)
				{
					case CompressionType.Zip:
						Builder.AppendLine("zip");
						break;
					case CompressionType.TarGZ:
						Builder.AppendLine("gzip");
						break;
					case CompressionType.BZ2:
						Builder.AppendLine("bzip");
						break;
					default:
						Builder.AppendLine("zip");
						break;
				}

				Builder.AppendLine();
				Builder.AppendLine("[folders]");
				Builder.AppendLine("route = " + RouteFolder);
				Builder.AppendLine("train = " + TrainFolder);
				Builder.AppendLine();
				Builder.AppendLine("[recentlyUsedRoutes]");
				for (int i = 0; i < RecentlyUsedRoutes.Length; i++)
				{
					Builder.AppendLine(RecentlyUsedRoutes[i]);
				}

				Builder.AppendLine();
				Builder.AppendLine("[recentlyUsedTrains]");
				for (int i = 0; i < RecentlyUsedTrains.Length; i++)
				{
					Builder.AppendLine(RecentlyUsedTrains[i]);
				}

				Builder.AppendLine();
				Builder.AppendLine("[routeEncodings]");
				for (int i = 0; i < RouteEncodings.Length; i++)
				{
					Builder.AppendLine(RouteEncodings[i].Codepage.ToString(Culture) + " = " + RouteEncodings[i].Value);
				}

				Builder.AppendLine();
				Builder.AppendLine("[trainEncodings]");
				for (int i = 0; i < TrainEncodings.Length; i++)
				{
					Builder.AppendLine(TrainEncodings[i].Codepage.ToString(Culture) + " = " + TrainEncodings[i].Value);
				}

				Builder.AppendLine();
				Builder.AppendLine("[enableInputDevicePlugins]");
				for (int i = 0; i < EnableInputDevicePlugins.Length; i++)
				{
					Builder.AppendLine(EnableInputDevicePlugins[i]);
				}

				Builder.AppendLine();
				Builder.AppendLine("[Parsers]");
				Builder.AppendLine("xObject = " + (int)CurrentXParser);
				Builder.AppendLine("objObject = " + (int)CurrentObjParser);
				Builder.AppendLine("gdiplus = " + (UseGDIDecoders ? "true" : "false"));
				Builder.AppendLine();
				Builder.AppendLine("[Touch]");
				Builder.AppendLine("cursor = " + CursorFileName);
				Builder.AppendLine("panel2extended = " + (Panel2ExtendedMode ? "true" : "false"));
				Builder.AppendLine("panel2extendedminsize = " + Panel2ExtendedMinSize.ToString(Culture));
				try
				{
					File.WriteAllText(fileName, Builder.ToString(), new UTF8Encoding(true));
				}
				catch
				{
					MessageBox.Show(@"Failed to write to the Options folder.", @"OpenBVE", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
		}

		/// <summary>The current game options</summary>
		internal static Options CurrentOptions;
		/// <summary>Loads the options file from disk</summary>
		internal static void LoadOptions()
		{
			CurrentOptions = new Options();
			string OptionsDir = Path.CombineDirectory(Program.FileSystem.SettingsFolder, "1.5.0");
			if (!Directory.Exists(OptionsDir))
			{
				Directory.CreateDirectory(OptionsDir);
			}
			
			CultureInfo Culture = CultureInfo.InvariantCulture;
			string configFile = Path.CombineFile(OptionsDir, "options.cfg");
			if (!File.Exists(configFile))
			{
				//Attempt to load and upgrade a prior configuration file
				configFile = Path.CombineFile(Program.FileSystem.SettingsFolder, "options.cfg");
			}

			if (File.Exists(configFile))
			{
				ConfigFile<OptionsSection, OptionsKey> cfg = new ConfigFile<OptionsSection, OptionsKey>(File.ReadAllLines(configFile, new UTF8Encoding()), Program.CurrentHost);

				while (cfg.RemainingSubBlocks > 0)
				{
					Block<OptionsSection, OptionsKey> block = cfg.ReadNextBlock();
					switch (block.Key)
					{
						case OptionsSection.Language:
							block.TryGetValue(OptionsKey.Code, ref CurrentOptions.LanguageCode);
							break;
						case OptionsSection.Interface:
							block.TryGetValue(OptionsKey.Folder, ref CurrentOptions.UserInterfaceFolder);
							block.GetEnumValue(OptionsKey.TimetableMode, out CurrentOptions.TimeTableStyle);
							block.GetValue(OptionsKey.KioskMode, out Interface.CurrentOptions.KioskMode);
							block.TryGetValue(OptionsKey.KioskModeTimer, ref CurrentOptions.KioskModeTimer);
							if (CurrentOptions.KioskModeTimer > 1000 || CurrentOptions.KioskModeTimer == 0)
							{
								CurrentOptions.KioskModeTimer = 300;
							}

							block.GetValue(OptionsKey.Accessibility, out Interface.CurrentOptions.Accessibility);
							block.TryGetValue(OptionsKey.Font, ref CurrentOptions.Font);
							block.GetValue(OptionsKey.DailyBuildUpdates, out Interface.CurrentOptions.DailyBuildUpdates);
							break;
						case OptionsSection.Display:
							block.GetValue(OptionsKey.PreferNativeBackend, out CurrentOptions.PreferNativeBackend);
							block.GetValue(OptionsKey.Mode, out string m);
							CurrentOptions.FullscreenMode = string.Compare(m, "fullscreen", StringComparison.OrdinalIgnoreCase) == 0;
							block.TryGetValue(OptionsKey.WindowWidth, ref Interface.CurrentOptions.WindowWidth);
							block.TryGetValue(OptionsKey.WindowHeight, ref Interface.CurrentOptions.WindowHeight);
							block.TryGetValue(OptionsKey.FullScreenWidth, ref Interface.CurrentOptions.FullscreenWidth);
							block.TryGetValue(OptionsKey.FullScreenHeight, ref CurrentOptions.FullscreenHeight);
							block.TryGetValue(OptionsKey.FullScreenBits, ref CurrentOptions.FullscreenBits);
							block.TryGetValue(OptionsKey.MainMenuWidth, ref CurrentOptions.MainMenuWidth);
							block.TryGetValue(OptionsKey.MainMenuHeight, ref CurrentOptions.MainMenuHeight);
							block.GetValue(OptionsKey.LoadInAdvance, out Interface.CurrentOptions.LoadInAdvance);
							block.GetValue(OptionsKey.UnloadTextures, out CurrentOptions.UnloadUnusedTextures);
							block.GetValue(OptionsKey.IsUseNewRenderer, out Interface.CurrentOptions.IsUseNewRenderer);
							block.GetValue(OptionsKey.ForwardsCompatibleContext, out CurrentOptions.ForceForwardsCompatibleContext);
							block.TryGetValue(OptionsKey.ViewingDistance, ref Interface.CurrentOptions.ViewingDistance);
							block.TryGetValue(OptionsKey.QuadLeafSize, ref Interface.CurrentOptions.QuadTreeLeafSize);
							block.TryGetValue(OptionsKey.UIScaleFactor, ref CurrentOptions.UserInterfaceScaleFactor);
							break;
						case OptionsSection.Quality:
							block.GetEnumValue(OptionsKey.Interpolation, out Interface.CurrentOptions.Interpolation);
							block.TryGetValue(OptionsKey.AnisotropicFilteringMaximum, ref CurrentOptions.AnisotropicFilteringMaximum);
							block.TryGetValue(OptionsKey.AnisotropicFilteringLevel, ref Interface.CurrentOptions.AnisotropicFilteringLevel);
							block.TryGetValue(OptionsKey.AntiAliasingLevel, ref Interface.CurrentOptions.AntiAliasingLevel);
							block.GetEnumValue(OptionsKey.TransparencyMode, out Interface.CurrentOptions.TransparencyMode);
							block.GetValue(OptionsKey.OldTransparencyMode, out CurrentOptions.OldTransparencyMode);
							block.TryGetValue(OptionsKey.ViewingDistance, ref Interface.CurrentOptions.ViewingDistance);
							block.TryGetValue(OptionsKey.QuadLeafSize, ref CurrentOptions.QuadTreeLeafSize);
							block.GetEnumValue(OptionsKey.MotionBlur, out CurrentOptions.MotionBlur);
							block.GetValue(OptionsKey.FPSLimit, out CurrentOptions.FPSLimit);
							if (CurrentOptions.FPSLimit < 0)
							{
								CurrentOptions.FPSLimit = 0; // n.b. 0 is unlimited
							}

							break;
						case OptionsSection.ObjectOptimization:
							block.GetValue(OptionsKey.BasicThreshold, out CurrentOptions.ObjectOptimizationBasicThreshold);
							block.GetValue(OptionsKey.FullThreshold, out CurrentOptions.ObjectOptimizationFullThreshold);
							block.GetValue(OptionsKey.VertexCulling, out CurrentOptions.ObjectOptimizationVertexCulling);
							break;
						case OptionsSection.Simulation:
							block.GetValue(OptionsKey.Toppling, out CurrentOptions.Toppling);
							block.GetValue(OptionsKey.Collisions, out CurrentOptions.Collisions);
							block.GetValue(OptionsKey.Derailments, out CurrentOptions.Derailments);
							block.GetValue(OptionsKey.LoadingSway, out CurrentOptions.LoadingSway);
							block.GetValue(OptionsKey.BlackBox, out CurrentOptions.BlackBox);
							block.GetEnumValue(OptionsKey.Mode, out CurrentOptions.GameMode);
							block.GetValue(OptionsKey.AcceleratedTimeFactor, out CurrentOptions.TimeAccelerationFactor);
							if (CurrentOptions.TimeAccelerationFactor <= 0)
							{
								CurrentOptions.TimeAccelerationFactor = 5;
							}

							block.GetValue(OptionsKey.EnableBVETSHacks, out CurrentOptions.EnableBveTsHacks);
							block.GetValue(OptionsKey.EnableBVE5ScriptedTrain, out CurrentOptions.EnableBve5ScriptedTrain);
							break;
						case OptionsSection.Controls:
							block.GetValue(OptionsKey.UseJoysticks, out CurrentOptions.UseJoysticks);
							block.GetValue(OptionsKey.JoystickAxisEB, out CurrentOptions.AllowAxisEB);
							block.GetValue(OptionsKey.JoystickAxisThreshold, out CurrentOptions.JoystickAxisThreshold);
							block.GetValue(OptionsKey.KeyRepeatDelay, out int delay);
							if (delay <= 500) delay = 500;
							CurrentOptions.KeyRepeatDelay = delay * 0.001;
							block.GetValue(OptionsKey.KeyRepeatDelay, out int interval);
							if (interval <= 500) interval = 500;
							CurrentOptions.KeyRepeatInterval = interval * 0.001;
							block.GetValue(OptionsKey.RailDriverMPH, out CurrentOptions.RailDriverMPH);
							block.GetValue(OptionsKey.CursorHideDelay, out CurrentOptions.CursorHideDelay);
							break;
						case OptionsSection.Sound:
							block.GetEnumValue(OptionsKey.Model, out CurrentOptions.SoundModel);
							block.GetEnumValue(OptionsKey.Range, out CurrentOptions.SoundRange);
							block.GetValue(OptionsKey.Number, out CurrentOptions.SoundNumber);
							if (CurrentOptions.SoundNumber < 16) CurrentOptions.SoundNumber = 16;
							break;
						case OptionsSection.Verbosity:
							block.GetValue(OptionsKey.ShowWarningMessages, out CurrentOptions.ShowWarningMessages);
							block.GetValue(OptionsKey.ShowErrorMessages, out CurrentOptions.ShowErrorMessages);
							block.GetValue(OptionsKey.DebugLog, out CurrentOptions.GenerateDebugLogging);
							break;
						case OptionsSection.Folders:
							block.GetValue(OptionsKey.Route, out CurrentOptions.RouteFolder);
							block.GetValue(OptionsKey.Train, out CurrentOptions.TrainFolder);
							break;
						case OptionsSection.Packages:
							block.GetEnumValue(OptionsKey.Compression, out CurrentOptions.packageCompressionType);
							break;
						case OptionsSection.RecentlyUsedRoutes:
							Array.Resize(ref CurrentOptions.RecentlyUsedRoutes, block.RemainingDataValues);
							int num = 0;
							while (block.RemainingDataValues > 0)
							{
								block.GetNextRawValue(out CurrentOptions.RecentlyUsedRoutes[num]);
								num++;
							}

							break;
						case OptionsSection.RecentlyUsedTrains:
							Array.Resize(ref CurrentOptions.RecentlyUsedTrains, block.RemainingDataValues);
							num = 0;
							while (num < CurrentOptions.RecentlyUsedTrains.Length)
							{
								block.GetNextRawValue(out CurrentOptions.RecentlyUsedTrains[num]);
								num++;
							}

							break;
						case OptionsSection.RouteEncodings:
							Array.Resize(ref CurrentOptions.RouteEncodings, block.RemainingDataValues);
							num = 0;
							while (num < CurrentOptions.RouteEncodings.Length)
							{
								block.GetIndexedEncoding(out CurrentOptions.RouteEncodings[num].Codepage, out CurrentOptions.RouteEncodings[num].Value);
								num++;
							}

							break;
						case OptionsSection.TrainEncodings:
							Array.Resize(ref CurrentOptions.TrainEncodings, block.RemainingDataValues);
							num = 0;
							while (num < CurrentOptions.TrainEncodings.Length)
							{
								block.GetIndexedEncoding(out CurrentOptions.TrainEncodings[num].Codepage, out CurrentOptions.TrainEncodings[num].Value);
								num++;
							}

							break;
						case OptionsSection.EnableInputDevicePlugins:
							Array.Resize(ref CurrentOptions.EnableInputDevicePlugins, block.RemainingDataValues);
							num = 0;
							while (num < CurrentOptions.EnableInputDevicePlugins.Length)
							{
								block.GetNextRawValue(out CurrentOptions.EnableInputDevicePlugins[num]);
								num++;
							}

							break;
						case OptionsSection.Parsers:
							block.GetEnumValue(OptionsKey.XObject, out Interface.CurrentOptions.CurrentXParser);
							block.GetEnumValue(OptionsKey.ObjObject, out Interface.CurrentOptions.CurrentObjParser);
							block.GetValue(OptionsKey.GDIPlus, out Interface.CurrentOptions.UseGDIDecoders);
							break;
						case OptionsSection.Touch:
							block.TryGetValue(OptionsKey.Cursor, ref CurrentOptions.CursorFileName);
							block.GetValue(OptionsKey.Panel2Extended, out CurrentOptions.Panel2ExtendedMode);
							block.GetValue(OptionsKey.Panel2ExtendedMinSize, out CurrentOptions.Panel2ExtendedMinSize);
							break;
					}
				}
			}
		}
		
	}
}
