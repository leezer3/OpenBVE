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
using OpenTK.Graphics.ES20;
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
			/// <summary>Whether duplicate vertices are culled during loading</summary>
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
			/// <summary>The interval at which a held down key will repeat after the initial delay</summary>
			internal double KeyRepeatInterval;
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
			/// <summary>The list of enabled Input Device Plugins</summary>
			internal string[] EnabledInputDevicePlugins;
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

			/// <summary>Whether smooth transitions are enabled for interior camera modes</summary>
			internal bool CameraInteriorTransition;
			/// <summary>Whether smooth transitions are enabled for exterior camera modes</summary>
			internal bool CameraExteriorTransition;
			/// <summary>The duration of camera transitions in seconds</summary>
			internal double CameraTransitionSpeed;
			
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
				SoundRange = SoundRange.Low;
				SoundNumber = 16;
				ShowWarningMessages = true;
				ShowErrorMessages = true;
				ObjectOptimizationBasicThreshold = 10000;
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
				EnabledInputDevicePlugins = new string[] { };
				CursorFileName = "nk.png";
				Panel2ExtendedMode = false;
				Panel2ExtendedMinSize = 128;
				CurrentXParser = XParsers.NewXParser; //Set to new X parser by default
				CurrentObjParser = ObjParsers.Original; //Set to original Obj parser by default
				CursorHideDelay = 10;
				Accessibility = false;
				ScreenReaderAvailable = false;
				ForceForwardsCompatibleContext = false;
				DailyBuildUpdates = false;
				UseGDIDecoders = false;
				EnableBve5ScriptedTrain = true;
				UserInterfaceScaleFactor = 1;
				CameraInteriorTransition = true;
				CameraExteriorTransition = true;
				CameraTransitionSpeed = 0.4;
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
				Builder.AppendLine("timetablemode = " + TimeTableStyle);
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
				Builder.AppendLine("forwardsCompatibleContext = " + (ForceForwardsCompatibleContext ? "true" : "false"));
				Builder.AppendLine("uiscalefactor = " + UserInterfaceScaleFactor);
				Builder.AppendLine("cameraInteriorTransition = " + (CameraInteriorTransition ? "true" : "false"));
				Builder.AppendLine("cameraExteriorTransition = " + (CameraExteriorTransition ? "true" : "false"));
				Builder.AppendLine("cameraTransitionSpeed = " + CameraTransitionSpeed.ToString(Culture));
				Builder.AppendLine();
				Builder.AppendLine("[quality]");
				Builder.AppendLine("interpolation = " + Interpolation);
				Builder.AppendLine("anisotropicFilteringLevel = " + AnisotropicFilteringLevel.ToString(Culture));
				Builder.AppendLine("anisotropicFilteringMaximum = " + AnisotropicFilteringMaximum.ToString(Culture));
				Builder.AppendLine("antiAliasingLevel = " + AntiAliasingLevel.ToString(Culture));
				Builder.AppendLine("transparencyMode = " + ((int)TransparencyMode).ToString(Culture));
				Builder.AppendLine("oldtransparencymode = " + (OldTransparencyMode ? "true" : "false"));
				Builder.AppendLine("viewingDistance = " + ViewingDistance.ToString(Culture));
				Builder.AppendLine("nearclipscenery = " + NearClipScenery.ToString(Culture));
				Builder.AppendLine("nearclipcab = " + NearClipCab.ToString(Culture));
				Builder.AppendLine("nearclipbase = " + NearClipBase.ToString(Culture));
				Builder.AppendLine("quadLeafSize = " + QuadTreeLeafSize.ToString(Culture));
				Builder.AppendLine("motionBlur = " + MotionBlur);
				Builder.AppendLine("shadowresolution = " + ShadowResolution);
				Builder.AppendLine("shadowdrawdistance = " + ShadowDrawDistance);
				Builder.AppendLine("shadowcascades = " + (int)ShadowCascades);
				Builder.AppendLine("shadowstrength = " + ShadowStrength.ToString(Culture));
				Builder.AppendLine("shadowbias = " + ShadowBias.ToString(Culture));
				Builder.AppendLine("shadownormalbias = " + ShadowNormalBias.ToString(Culture));
				Builder.AppendLine("shadowfiltercascades = " + (ShadowFilterCascades ? "true" : "false"));
				Builder.AppendLine("fpslimit = " + FPSLimit.ToString(Culture));
				Builder.AppendLine();
				Builder.AppendLine("[objectOptimization]");
				Builder.AppendLine("basicThreshold = " + ObjectOptimizationBasicThreshold.ToString(Culture));
				Builder.AppendLine("vertexCulling = " + ObjectOptimizationVertexCulling.ToString(Culture));
				Builder.AppendLine();
				Builder.AppendLine("[simulation]");
				Builder.AppendLine("toppling = " + (Toppling ? "true" : "false"));
				Builder.AppendLine("collisions = " + (Collisions ? "true" : "false"));
				Builder.AppendLine("derailments = " + (Derailments ? "true" : "false"));
				Builder.AppendLine("loadingsway = " + (LoadingSway ? "true" : "false"));
				Builder.AppendLine("blackbox = " + (BlackBox ? "true" : "false"));
				Builder.AppendLine("mode = " + GameMode);
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
				Builder.AppendLine("range = " + SoundRange);
				Builder.AppendLine("number = " + SoundNumber.ToString(Culture));
				Builder.AppendLine();
				Builder.AppendLine("[packages]");
				Builder.AppendLine("compression = " + packageCompressionType);
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
					Builder.AppendLine(RouteEncodings[i].Codepage + " = " + RouteEncodings[i].Value);
				}

				Builder.AppendLine();
				Builder.AppendLine("[trainEncodings]");
				for (int i = 0; i < TrainEncodings.Length; i++)
				{
					Builder.AppendLine(TrainEncodings[i].Codepage + " = " + TrainEncodings[i].Value);
				}

				Builder.AppendLine();
				Builder.AppendLine("[enableInputDevicePlugins]");
				for (int i = 0; i < EnabledInputDevicePlugins.Length; i++)
				{
					Builder.AppendLine(EnabledInputDevicePlugins[i]);
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
					Program.ShowMessageBox(@"Failed to write to the Options folder.", Application.ProductName);
				}
			}

			public override void Load()
			{
				string OptionsDir = Path.CombineDirectory(Program.CurrentHost.FileSystem.SettingsFolder, "1.5.0");
				if (!Directory.Exists(OptionsDir))
				{
					Directory.CreateDirectory(OptionsDir);
				}

				string configFile = Path.CombineFile(OptionsDir, "options.cfg");
				if (!File.Exists(configFile))
				{
					//Attempt to load and upgrade a prior configuration file
					configFile = Path.CombineFile(Program.CurrentHost.FileSystem.SettingsFolder, "options.cfg");
				}

				if (File.Exists(configFile))
				{
					ConfigFile<OptionsSection, OptionsKey> cfg = new ConfigFile<OptionsSection, OptionsKey>(File.ReadAllLines(configFile, new UTF8Encoding()), configFile, Program.CurrentHost);

					while (cfg.RemainingSubBlocks > 0)
					{
						Block<OptionsSection, OptionsKey> block = cfg.ReadNextBlock();
						switch (block.Key)
						{
							case OptionsSection.Language:
								block.TryGetValue(OptionsKey.Code, ref LanguageCode);
								break;
							case OptionsSection.Interface:
								block.TryGetValue(OptionsKey.Folder, ref UserInterfaceFolder);
								block.GetEnumValue(OptionsKey.TimetableMode, out TimeTableStyle);
								block.GetValue(OptionsKey.KioskMode, out KioskMode);
								block.TryGetValue(OptionsKey.KioskModeTimer, ref KioskModeTimer, NumberRange.NonNegative);
								if (KioskModeTimer > 1000 || KioskModeTimer == 0)
								{
									KioskModeTimer = 300;
								}

								block.GetValue(OptionsKey.Accessibility, out Accessibility);
								block.TryGetValue(OptionsKey.Font, ref Font);
								block.GetValue(OptionsKey.DailyBuildUpdates, out DailyBuildUpdates);
								break;
							case OptionsSection.Display:
								{
									block.GetValue(OptionsKey.PreferNativeBackend, out PreferNativeBackend);
									block.GetValue(OptionsKey.Mode, out string m);
									FullscreenMode = string.Compare(m, "fullscreen", StringComparison.OrdinalIgnoreCase) == 0;
									block.TryGetValue(OptionsKey.WindowWidth, ref WindowWidth, NumberRange.Positive);
									block.TryGetValue(OptionsKey.WindowHeight, ref WindowHeight, NumberRange.Positive);
									block.TryGetValue(OptionsKey.FullScreenWidth, ref FullscreenWidth, NumberRange.Positive);
									block.TryGetValue(OptionsKey.FullScreenHeight, ref FullscreenHeight, NumberRange.Positive);
									block.TryGetValue(OptionsKey.FullScreenBits, ref FullscreenBits, NumberRange.Positive);
									block.TryGetValue(OptionsKey.MainMenuWidth, ref MainMenuWidth, NumberRange.Positive);
									block.TryGetValue(OptionsKey.MainMenuHeight, ref MainMenuHeight, NumberRange.Positive);
									block.GetValue(OptionsKey.VSync, out VerticalSynchronization);
									block.GetValue(OptionsKey.LoadInAdvance, out LoadInAdvance);
									block.GetValue(OptionsKey.UnloadTextures, out UnloadUnusedTextures);
									block.GetValue(OptionsKey.ForwardsCompatibleContext, out ForceForwardsCompatibleContext);
									block.TryGetValue(OptionsKey.ViewingDistance, ref ViewingDistance, NumberRange.Positive);
									block.TryGetValue(OptionsKey.QuadLeafSize, ref QuadTreeLeafSize, NumberRange.Positive);
									block.TryGetValue(OptionsKey.NearClipScenery, ref NearClipScenery, NumberRange.Positive);
									block.TryGetValue(OptionsKey.NearClipCab, ref NearClipCab, NumberRange.Positive);
									block.TryGetValue(OptionsKey.NearClipBase, ref NearClipBase, NumberRange.Positive);
									// ensure viewing distance is greater than the near clipping plane to avoid rendering issues
									double maxNearClip = Math.Max(NearClipScenery, Math.Max(NearClipCab, NearClipBase));

									if (ViewingDistance <= maxNearClip)
									{
										ViewingDistance = (int)Math.Ceiling(maxNearClip) + 1;
									}

									block.TryGetValue(OptionsKey.UIScaleFactor, ref UserInterfaceScaleFactor);
									block.GetValue(OptionsKey.CameraInteriorTransition, out CameraInteriorTransition);
									block.GetValue(OptionsKey.CameraExteriorTransition, out CameraExteriorTransition);
									block.TryGetValue(OptionsKey.CameraTransitionSpeed, ref CameraTransitionSpeed, NumberRange.Positive);
									if (CameraTransitionSpeed == 0)
									{
										CameraTransitionSpeed = 0.4;
									}
									break;
								}
							case OptionsSection.Quality:
								{
									block.GetEnumValue(OptionsKey.Interpolation, out Interpolation);
									block.TryGetValue(OptionsKey.AnisotropicFilteringMaximum, ref AnisotropicFilteringMaximum);
									block.TryGetValue(OptionsKey.AnisotropicFilteringLevel, ref AnisotropicFilteringLevel);
									block.TryGetValue(OptionsKey.AntiAliasingLevel, ref AntiAliasingLevel);
									block.GetEnumValue(OptionsKey.TransparencyMode, out TransparencyMode);
									block.GetValue(OptionsKey.OldTransparencyMode, out OldTransparencyMode);
									block.TryGetValue(OptionsKey.ViewingDistance, ref ViewingDistance, NumberRange.Positive);
									block.TryGetValue(OptionsKey.QuadLeafSize, ref QuadTreeLeafSize, NumberRange.Positive);
									block.TryGetValue(OptionsKey.NearClipScenery, ref NearClipScenery, NumberRange.Positive);
									block.TryGetValue(OptionsKey.NearClipCab, ref NearClipCab, NumberRange.Positive);
									block.TryGetValue(OptionsKey.NearClipBase, ref NearClipBase, NumberRange.Positive);
									// ensure viewing distance is greater than the near clipping plane to avoid rendering issues
									double maxNearClip = Math.Max(NearClipScenery, Math.Max(NearClipCab, NearClipBase));

									if (ViewingDistance <= maxNearClip)
									{
										ViewingDistance = (int)Math.Ceiling(maxNearClip) + 1;
									}
									block.GetEnumValue(OptionsKey.MotionBlur, out MotionBlur);
									block.TryGetEnumValue(OptionsKey.ShadowResolution, ref ShadowResolution);
									block.TryGetEnumValue(OptionsKey.ShadowDrawDistance, ref ShadowDrawDistance);
									block.TryGetEnumValue(OptionsKey.ShadowCascades, ref ShadowCascades);
									block.TryGetValue(OptionsKey.ShadowStrength, ref ShadowStrength);
									block.TryGetValue(OptionsKey.ShadowBias, ref ShadowBias);
									if (ShadowBias < 0.0) ShadowBias = 0.0;
									if (ShadowBias > 1.0) ShadowBias = 1.0;
									block.TryGetValue(OptionsKey.ShadowNormalBias, ref ShadowNormalBias);
									if (ShadowNormalBias < 0.0) ShadowNormalBias = 0.0;
									block.GetValue(OptionsKey.ShadowFilterCascades, out ShadowFilterCascades);
									block.GetValue(OptionsKey.FPSLimit, out FPSLimit);
									if (FPSLimit < 0)
									{
										FPSLimit = 0; // n.b. 0 is unlimited
									}

									break;
								}
							case OptionsSection.ObjectOptimization:
								{
									block.GetValue(OptionsKey.BasicThreshold, out ObjectOptimizationBasicThreshold);
									block.GetValue(OptionsKey.VertexCulling, out ObjectOptimizationVertexCulling);
									break;
								}
							case OptionsSection.Simulation:
								block.GetValue(OptionsKey.Toppling, out Toppling);
								block.GetValue(OptionsKey.Collisions, out Collisions);
								block.GetValue(OptionsKey.Derailments, out Derailments);
								block.GetValue(OptionsKey.LoadingSway, out LoadingSway);
								block.GetValue(OptionsKey.BlackBox, out BlackBox);
								block.GetEnumValue(OptionsKey.Mode, out GameMode);
								block.GetValue(OptionsKey.AcceleratedTimeFactor, out TimeAccelerationFactor);
								if (TimeAccelerationFactor <= 0)
								{
									TimeAccelerationFactor = 5;
								}

								block.GetValue(OptionsKey.EnableBVETSHacks, out EnableBveTsHacks);
								block.GetValue(OptionsKey.EnableBVE5ScriptedTrain, out EnableBve5ScriptedTrain);
								break;
							case OptionsSection.Controls:
								block.GetValue(OptionsKey.UseJoysticks, out UseJoysticks);
								block.GetValue(OptionsKey.JoystickAxisEB, out AllowAxisEB);
								block.GetValue(OptionsKey.JoystickAxisThreshold, out JoystickAxisThreshold);
								block.GetValue(OptionsKey.KeyRepeatDelay, out int delay);
								if (delay <= 500) delay = 500;
								KeyRepeatDelay = delay * 0.001;
								block.GetValue(OptionsKey.KeyRepeatDelay, out int interval);
								if (interval <= 500) interval = 500;
								KeyRepeatInterval = interval * 0.001;
								block.GetValue(OptionsKey.RailDriverMPH, out RailDriverMPH);
								block.GetValue(OptionsKey.CursorHideDelay, out CursorHideDelay);
								break;
							case OptionsSection.Sound:
								block.GetEnumValue(OptionsKey.Range, out SoundRange);
								block.GetValue(OptionsKey.Number, out SoundNumber);
								if (SoundNumber < 16) SoundNumber = 16;
								break;
							case OptionsSection.Verbosity:
								block.GetValue(OptionsKey.ShowWarningMessages, out ShowWarningMessages);
								block.GetValue(OptionsKey.ShowErrorMessages, out ShowErrorMessages);
								block.GetValue(OptionsKey.DebugLog, out GenerateDebugLogging);
								break;
							case OptionsSection.Folders:
								block.GetValue(OptionsKey.Route, out RouteFolder);
								block.GetValue(OptionsKey.Train, out TrainFolder);
								break;
							case OptionsSection.Packages:
								block.GetEnumValue(OptionsKey.Compression, out packageCompressionType);
								break;
							case OptionsSection.RecentlyUsedRoutes:
								Array.Resize(ref RecentlyUsedRoutes, block.RemainingDataValues);
								int num = 0;
								while (block.RemainingDataValues > 0)
								{
									block.GetNextRawValue(out RecentlyUsedRoutes[num]);
									num++;
								}

								break;
							case OptionsSection.RecentlyUsedTrains:
								Array.Resize(ref RecentlyUsedTrains, block.RemainingDataValues);
								num = 0;
								while (num < RecentlyUsedTrains.Length)
								{
									block.GetNextRawValue(out RecentlyUsedTrains[num]);
									num++;
								}

								break;
							case OptionsSection.RouteEncodings:
								Array.Resize(ref RouteEncodings, block.RemainingDataValues);
								num = 0;
								while (num < RouteEncodings.Length)
								{
									block.GetIndexedEncoding(out RouteEncodings[num].Codepage, out RouteEncodings[num].Value);
									num++;
								}

								break;
							case OptionsSection.TrainEncodings:
								Array.Resize(ref TrainEncodings, block.RemainingDataValues);
								num = 0;
								while (num < TrainEncodings.Length)
								{
									block.GetIndexedEncoding(out TrainEncodings[num].Codepage, out TrainEncodings[num].Value);
									num++;
								}

								break;
							case OptionsSection.EnableInputDevicePlugins:
								Array.Resize(ref EnabledInputDevicePlugins, block.RemainingDataValues);
								num = 0;
								while (num < EnabledInputDevicePlugins.Length)
								{
									block.GetNextRawValue(out EnabledInputDevicePlugins[num]);
									num++;
								}

								break;
							case OptionsSection.Parsers:
								block.GetEnumValue(OptionsKey.XObject, out CurrentXParser);
								block.GetEnumValue(OptionsKey.ObjObject, out CurrentObjParser);
								block.GetValue(OptionsKey.GDIPlus, out UseGDIDecoders);
								break;
							case OptionsSection.Touch:
								block.TryGetValue(OptionsKey.Cursor, ref CursorFileName);
								block.GetValue(OptionsKey.Panel2Extended, out Panel2ExtendedMode);
								block.GetValue(OptionsKey.Panel2ExtendedMinSize, out Panel2ExtendedMinSize);
								break;
						}
					}
				}
			}
		}

		/// <summary>The current game options</summary>
		internal static Options CurrentOptions => Program.CurrentHost.Options as Options;		
	}
}
