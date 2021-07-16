using System;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using LibRender2.MotionBlurs;
using LibRender2.Overlays;
using OpenBveApi;
using OpenBveApi.Graphics;
using OpenBveApi.Objects;
using OpenBveApi.Packages;
using SoundManager;

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
			internal string CursorFileName;
			
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
			/*
			 * Note: The following options were (are) used by the Managed Content system, and are currently non-functional
			 */
			/// <summary>The proxy URL to use when retrieving content from the internet</summary>
			internal string ProxyUrl;
			/// <summary>The proxy username to use when retrieving content from the internet</summary>
			internal string ProxyUserName;
			/// <summary>The proxy password to use when retrieving content from the internet</summary>
			internal string ProxyPassword;
			/// <summary>Creates a new instance of the options class with default values set</summary>
			internal Options()
			{
				this.LanguageCode = "en-US";
				this.FullscreenMode = false;
				this.VerticalSynchronization = true;
				this.WindowWidth = 960;
				this.WindowHeight = 600;
				this.FullscreenWidth = 1024;
				this.FullscreenHeight = 768;
				this.FullscreenBits = 32;
				this.UserInterfaceFolder = "Default";
				this.Interpolation = InterpolationMode.BilinearMipmapped;
				this.TransparencyMode = TransparencyMode.Quality;
				this.AnisotropicFilteringLevel = 0;
				this.AnisotropicFilteringMaximum = 0;
				this.AntiAliasingLevel = 0;
				this.ViewingDistance = 600;
				this.MotionBlur = MotionBlurMode.None;
				this.Toppling = true;
				this.Collisions = true;
				this.Derailments = true;
				this.LoadingSway = false;
				this.GameMode = GameMode.Normal;
				this.BlackBox = false;
				this.UseJoysticks = true;
				this.JoystickAxisThreshold = 0.0;
				this.KeyRepeatDelay = 0.5;
				this.KeyRepeatInterval = 0.1;
				SoundModel = SoundModels.Inverse;
				SoundRange = SoundRange.Low;
				this.SoundNumber = 16;
				this.ShowWarningMessages = true;
				this.ShowErrorMessages = true;
				this.ObjectOptimizationBasicThreshold = 10000;
				this.ObjectOptimizationFullThreshold = 1000;
				this.ObjectOptimizationVertexCulling = false;
				this.RouteFolder = "";
				this.TrainFolder = "";
				this.RecentlyUsedRoutes = new string[] { };
				this.RecentlyUsedTrains = new string[] { };
				this.RecentlyUsedLimit = 10;
				this.RouteEncodings = new TextEncoding.EncodingValue[] { };
				this.TrainEncodings = new TextEncoding.EncodingValue[] { };
				this.MainMenuWidth = 0;
				this.MainMenuHeight = 0;
				this.LoadInAdvance = false;
				this.UnloadUnusedTextures = false;
				this.ProxyUrl = string.Empty;
				this.ProxyUserName = string.Empty;
				this.ProxyPassword = string.Empty;
				this.TimeAccelerationFactor = 5;
				this.AllowAxisEB = true;
				this.TimeTableStyle = TimeTableMode.Default;
				this.packageCompressionType = CompressionType.Zip;
				this.RailDriverMPH = true;
				this.EnableBveTsHacks = true;
				this.OldTransparencyMode = true;
				this.KioskMode = false;
				this.KioskModeTimer = 300;
				this.EnableInputDevicePlugins = new string[] { };
				this.CursorFileName = "nk.png";
				this.Panel2ExtendedMode = false;
				this.Panel2ExtendedMinSize = 128;
				this.CurrentXParser = XParsers.Original; //Set to Michelle's original X parser by default
				this.CurrentObjParser = ObjParsers.Original; //Set to original Obj parser by default
				this.CursorHideDelay = 10;
				this.Accessibility = false;
				this.ScreenReaderAvailable = false;
			}
		}
		/// <summary>The current game options</summary>
		internal static Options CurrentOptions;
		/// <summary>Loads the options file from disk</summary>
		internal static void LoadOptions()
		{
			CurrentOptions = new Options();
			string OptionsDir = Path.CombineDirectory(Program.FileSystem.SettingsFolder, "1.5.0");
			if (!System.IO.Directory.Exists(OptionsDir))
			{
				System.IO.Directory.CreateDirectory(OptionsDir);
			}
			
			CultureInfo Culture = CultureInfo.InvariantCulture;
			string File = Path.CombineFile(OptionsDir, "options.cfg");
			if (!System.IO.File.Exists(File))
			{
				//Attempt to load and upgrade a prior configuration file
				File = Path.CombineFile(Program.FileSystem.SettingsFolder, "options.cfg");
			}
			if (System.IO.File.Exists(File))
			{
				// load options
				string[] Lines = System.IO.File.ReadAllLines(File, new UTF8Encoding());
				string Section = "";
				for (int i = 0; i < Lines.Length; i++)
				{
					Lines[i] = Lines[i].Trim(new char[] { });
					if (Lines[i].Length != 0 && !Lines[i].StartsWith(";", StringComparison.OrdinalIgnoreCase))
					{
						if (Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))
						{
							Section = Lines[i].Substring(1, Lines[i].Length - 2).Trim(new char[] { }).ToLowerInvariant();
						}
						else
						{
							int j = Lines[i].IndexOf("=", StringComparison.OrdinalIgnoreCase);
							string Key, Value;
							if (j >= 0)
							{
								Key = Lines[i].Substring(0, j).TrimEnd().ToLowerInvariant();
								Value = Lines[i].Substring(j + 1).TrimStart();
							}
							else
							{
								Key = "";
								Value = Lines[i];
							}
							switch (Section)
							{
								case "language":
									switch (Key)
									{
										case "code":
											Interface.CurrentOptions.LanguageCode = Value.Length != 0 ? Value : "en-US";
											break;
									} break;
								case "interface":
									switch (Key)
									{
										case "folder":
											Interface.CurrentOptions.UserInterfaceFolder = Value.Length != 0 ? Value : "Default";
											break;
										case "timetablemode":
											switch (Value.ToLowerInvariant())
											{
												case "none":
													Interface.CurrentOptions.TimeTableStyle = TimeTableMode.None;
													break;
												case "default":
													Interface.CurrentOptions.TimeTableStyle = TimeTableMode.Default;
													break;
												case "autogenerated":
													Interface.CurrentOptions.TimeTableStyle = TimeTableMode.AutoGenerated;
													break;
												case "prefercustom":
													Interface.CurrentOptions.TimeTableStyle = TimeTableMode.PreferCustom;
													break;
											}
											break;
										case "kioskmode":
											Interface.CurrentOptions.KioskMode = string.Compare(Value, "true", StringComparison.OrdinalIgnoreCase) == 0;
											break;
										case "kioskmodetimer":
											double d;
											if (!double.TryParse(Value, NumberStyles.Number, Culture, out d))
											{
												d = 300;
											}
											if (d > 1000 || d < 0)
											{
												d = 300;
											}
											Interface.CurrentOptions.KioskModeTimer = d;
											break;
										case "accessibility":
											Interface.CurrentOptions.Accessibility = string.Compare(Value, "true", StringComparison.OrdinalIgnoreCase) == 0;
											break;
									} break;
								case "display":
									switch (Key)
									{
										case "prefernativebackend":
											Interface.CurrentOptions.PreferNativeBackend = string.Compare(Value, "true", StringComparison.OrdinalIgnoreCase) == 0;
											break;
										case "mode":
											Interface.CurrentOptions.FullscreenMode = string.Compare(Value, "fullscreen", StringComparison.OrdinalIgnoreCase) == 0;
											break;
										case "vsync":
											Interface.CurrentOptions.VerticalSynchronization = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "windowwidth":
											{
												int a;
												if (!int.TryParse(Value, NumberStyles.Integer, Culture, out a))
												{
													a = 960;
												}
												Interface.CurrentOptions.WindowWidth = a;
											} break;
										case "windowheight":
											{
												int a;
												if (!int.TryParse(Value, NumberStyles.Integer, Culture, out a))
												{
													a = 600;
												}
												Interface.CurrentOptions.WindowHeight = a;
											} break;
										case "fullscreenwidth":
											{
												int a;
												if (!int.TryParse(Value, NumberStyles.Integer, Culture, out a))
												{
													a = 1024;
												}
												Interface.CurrentOptions.FullscreenWidth = a;
											} break;
										case "fullscreenheight":
											{
												int a;
												if (!int.TryParse(Value, NumberStyles.Integer, Culture, out a))
												{
													a = 768;
												}
												Interface.CurrentOptions.FullscreenHeight = a;
											} break;
										case "fullscreenbits":
											{
												int a;
												if (!int.TryParse(Value, NumberStyles.Integer, Culture, out a))
												{
													a = 32;
												}
												Interface.CurrentOptions.FullscreenBits = a;
											} break;
										case "mainmenuwidth":
											{
												int a;
												int.TryParse(Value, NumberStyles.Integer, Culture, out a);
												Interface.CurrentOptions.MainMenuWidth = a;
											} break;
										case "mainmenuheight":
											{
												int a;
												int.TryParse(Value, NumberStyles.Integer, Culture, out a);
												Interface.CurrentOptions.MainMenuHeight = a;
											} break;
										case "loadinadvance":
											Interface.CurrentOptions.LoadInAdvance = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "unloadtextures":
											Interface.CurrentOptions.UnloadUnusedTextures = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "isusenewrenderer":
											Interface.CurrentOptions.IsUseNewRenderer = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
									} break;
								case "quality":
									switch (Key)
									{
										case "interpolation":
											switch (Value.ToLowerInvariant())
											{
												case "nearestneighbor": Interface.CurrentOptions.Interpolation = InterpolationMode.NearestNeighbor; break;
												case "bilinear": Interface.CurrentOptions.Interpolation = InterpolationMode.Bilinear; break;
												case "nearestneighbormipmapped": Interface.CurrentOptions.Interpolation = InterpolationMode.NearestNeighborMipmapped; break;
												case "bilinearmipmapped": Interface.CurrentOptions.Interpolation = InterpolationMode.BilinearMipmapped; break;
												case "trilinearmipmapped": Interface.CurrentOptions.Interpolation = InterpolationMode.TrilinearMipmapped; break;
												case "anisotropicfiltering": Interface.CurrentOptions.Interpolation = InterpolationMode.AnisotropicFiltering; break;
												default: Interface.CurrentOptions.Interpolation = InterpolationMode.BilinearMipmapped; break;
											} break;
										case "anisotropicfilteringlevel":
											{
												int a;
												int.TryParse(Value, NumberStyles.Integer, Culture, out a);
												Interface.CurrentOptions.AnisotropicFilteringLevel = a;
											} break;
										case "anisotropicfilteringmaximum":
											{
												int a;
												int.TryParse(Value, NumberStyles.Integer, Culture, out a);
												Interface.CurrentOptions.AnisotropicFilteringMaximum = a;
											} break;
										case "antialiasinglevel":
											{
												int a;
												int.TryParse(Value, NumberStyles.Integer, Culture, out a);
												Interface.CurrentOptions.AntiAliasingLevel = a;
											} break;
										case "transparencymode":
											switch (Value.ToLowerInvariant())
											{
												case "sharp": Interface.CurrentOptions.TransparencyMode = TransparencyMode.Performance; break;
												case "smooth": Interface.CurrentOptions.TransparencyMode = TransparencyMode.Quality; break;
												default:
													{
														int a;
														if (int.TryParse(Value, NumberStyles.Integer, Culture, out a))
														{
															Interface.CurrentOptions.TransparencyMode = (TransparencyMode)a;
														}
														else
														{
															Interface.CurrentOptions.TransparencyMode = TransparencyMode.Quality;
														}
														break;
													}
											} break;
										case "oldtransparencymode":
											Interface.CurrentOptions.OldTransparencyMode = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "viewingdistance":
											{
												int a;
												if (int.TryParse(Value, NumberStyles.Integer, Culture, out a))
												{
													if (a >= 100 && a <= 10000)
													{
														Interface.CurrentOptions.ViewingDistance = a;
													}
												}
											} break;
										case "motionblur":
											switch (Value.ToLowerInvariant())
											{
												case "low": Interface.CurrentOptions.MotionBlur = MotionBlurMode.Low; break;
												case "medium": Interface.CurrentOptions.MotionBlur = MotionBlurMode.Medium; break;
												case "high": Interface.CurrentOptions.MotionBlur = MotionBlurMode.High; break;
												default: Interface.CurrentOptions.MotionBlur = MotionBlurMode.None; break;
											} break;
									} break;
								case "objectoptimization":
									switch (Key)
									{
										case "basicthreshold":
											{
												int a;
												int.TryParse(Value, NumberStyles.Integer, Culture, out a);
												Interface.CurrentOptions.ObjectOptimizationBasicThreshold = a;
											} break;
										case "fullthreshold":
											{
												int a;
												int.TryParse(Value, NumberStyles.Integer, Culture, out a);
												Interface.CurrentOptions.ObjectOptimizationFullThreshold = a;
											} break;
										case "vertexCulling":
											{
												Interface.CurrentOptions.ObjectOptimizationVertexCulling = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											} break;
									} break;
								case "simulation":
									switch (Key)
									{
										case "toppling":
											Interface.CurrentOptions.Toppling = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "collisions":
											Interface.CurrentOptions.Collisions = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "derailments":
											Interface.CurrentOptions.Derailments = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "loadingsway":
											Interface.CurrentOptions.LoadingSway = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "blackbox":
											Interface.CurrentOptions.BlackBox = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "mode":
											switch (Value.ToLowerInvariant())
											{
												case "arcade": Interface.CurrentOptions.GameMode = GameMode.Arcade; break;
												case "normal": Interface.CurrentOptions.GameMode = GameMode.Normal; break;
												case "expert": Interface.CurrentOptions.GameMode = GameMode.Expert; break;
												default: Interface.CurrentOptions.GameMode = GameMode.Normal; break;
											} break;
										case "acceleratedtimefactor":
											int tf;
											int.TryParse(Value, NumberStyles.Integer, Culture, out tf);
											if (tf <= 0)
											{
												tf = 5;
											}
											Interface.CurrentOptions.TimeAccelerationFactor = tf;
											break;
										case "enablebvetshacks":
											Interface.CurrentOptions.EnableBveTsHacks = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;

									} break;
								case "controls":
									switch (Key)
									{
										case "usejoysticks":
											Interface.CurrentOptions.UseJoysticks = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "joystickaxiseb":
											Interface.CurrentOptions.AllowAxisEB = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "joystickaxisthreshold":
											{
												double a;
												double.TryParse(Value, NumberStyles.Float, Culture, out a);
												Interface.CurrentOptions.JoystickAxisThreshold = a;
											} break;
										case "keyrepeatdelay":
											{
												int a;
												int.TryParse(Value, NumberStyles.Integer, Culture, out a);
												if (a <= 0) a = 500;
												Interface.CurrentOptions.KeyRepeatDelay = 0.001 * a;
											} break;
										case "keyrepeatinterval":
											{
												int a;
												int.TryParse(Value, NumberStyles.Integer, Culture, out a);
												if (a <= 0) a = 100;
												Interface.CurrentOptions.KeyRepeatInterval = 0.001 * a;
											} break;
										case "raildrivermph":
											Interface.CurrentOptions.RailDriverMPH = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "cursorhidedelay":
											{
												double a;
												double.TryParse(Value, NumberStyles.Float, Culture, out a);
												Interface.CurrentOptions.CursorHideDelay = a;
											}
											break;
									} break;
								case "sound":
									switch (Key)
									{
										case "model":
											switch (Value.ToLowerInvariant())
											{
												case "linear": CurrentOptions.SoundModel = SoundModels.Linear; break;
												default: CurrentOptions.SoundModel = SoundModels.Inverse; break;
											}
											break;
										case "range":
											switch (Value.ToLowerInvariant())
											{
												case "low": CurrentOptions.SoundRange = SoundRange.Low; break;
												case "medium": CurrentOptions.SoundRange = SoundRange.Medium; break;
												case "high": CurrentOptions.SoundRange = SoundRange.High; break;
												default: CurrentOptions.SoundRange = SoundRange.Low; break;
											}
											break;
										case "number":
											{
												int a;
												int.TryParse(Value, NumberStyles.Integer, Culture, out a);
												Interface.CurrentOptions.SoundNumber = a < 16 ? 16 : a;
											} break;
									} break;
								case "verbosity":
									switch (Key)
									{
										case "showwarningmessages":
											Interface.CurrentOptions.ShowWarningMessages = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "showerrormessages":
											Interface.CurrentOptions.ShowErrorMessages = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "debuglog":
											Interface.CurrentOptions.GenerateDebugLogging = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
									} break;
								case "folders":
									switch (Key)
									{
										case "route":
											Interface.CurrentOptions.RouteFolder = Value;
											break;
										case "train":
											Interface.CurrentOptions.TrainFolder = Value;
											break;
									} break;
								case "proxy":
									switch (Key)
									{
										case "url":
											Interface.CurrentOptions.ProxyUrl = Value;
											break;
										case "username":
											Interface.CurrentOptions.ProxyUserName = Value;
											break;
										case "password":
											Interface.CurrentOptions.ProxyPassword = Value;
											break;
									} break;
								case "packages":
									switch (Key)
									{
										case "compression":
											switch (Value.ToLowerInvariant())
											{
												case "zip":
													Interface.CurrentOptions.packageCompressionType = CompressionType.Zip;
													break;
												case "bzip":
													Interface.CurrentOptions.packageCompressionType = CompressionType.BZ2;
													break;
												case "gzip":
													Interface.CurrentOptions.packageCompressionType = CompressionType.TarGZ;
													break;
											}
											break;
									} break;
								case "recentlyusedroutes":
									{
										int n = Interface.CurrentOptions.RecentlyUsedRoutes.Length;
										Array.Resize(ref Interface.CurrentOptions.RecentlyUsedRoutes, n + 1);
										Interface.CurrentOptions.RecentlyUsedRoutes[n] = Value;
									} break;
								case "recentlyusedtrains":
									{
										int n = Interface.CurrentOptions.RecentlyUsedTrains.Length;
										Array.Resize(ref Interface.CurrentOptions.RecentlyUsedTrains, n + 1);
										Interface.CurrentOptions.RecentlyUsedTrains[n] = Value;
									} break;
								case "routeencodings":
									{
										int a;
										if (!int.TryParse(Key, NumberStyles.Integer, Culture, out a))
										{
											a = Encoding.UTF8.CodePage;
										}
										try
										{
#pragma warning disable CS0219
//Used to check that the parsed integer is a valid codepage
											// ReSharper disable once UnusedVariable
											Encoding e = Encoding.GetEncoding(a);
#pragma warning restore CS0219
										}
										catch
										{
											a = Encoding.UTF8.CodePage;
										}
										int n = Interface.CurrentOptions.RouteEncodings.Length;
										Array.Resize(ref Interface.CurrentOptions.RouteEncodings, n + 1);
										Interface.CurrentOptions.RouteEncodings[n].Codepage = a;
										Interface.CurrentOptions.RouteEncodings[n].Value = Value;
									} break;
								case "trainencodings":
									{
										int a;
										if (!int.TryParse(Key, NumberStyles.Integer, Culture, out a))
										{
											a = Encoding.UTF8.CodePage;
										}
										try
										{
#pragma warning disable CS0219
//Used to check that the parsed integer is a valid codepage
											// ReSharper disable once UnusedVariable
											Encoding e = Encoding.GetEncoding(a);
#pragma warning restore CS0219
										}
										catch
										{
											a = Encoding.UTF8.CodePage;
										}
										int n = Interface.CurrentOptions.TrainEncodings.Length;
										Array.Resize(ref Interface.CurrentOptions.TrainEncodings, n + 1);
										Interface.CurrentOptions.TrainEncodings[n].Codepage = a;
										Interface.CurrentOptions.TrainEncodings[n].Value = Value;
									} break;
								case "enableinputdeviceplugins":
									{
										int n = Interface.CurrentOptions.EnableInputDevicePlugins.Length;
										Array.Resize(ref Interface.CurrentOptions.EnableInputDevicePlugins, n + 1);
										Interface.CurrentOptions.EnableInputDevicePlugins[n] = Value;
									} break;
								case "parsers":
									switch (Key)
									{
										case "xobject":
											{
												int p;
												if (!int.TryParse(Value, NumberStyles.Integer, Culture, out p) || p < 0 || p > 3)
												{
													Interface.CurrentOptions.CurrentXParser = XParsers.Original;
												}
												else
												{
													Interface.CurrentOptions.CurrentXParser = (XParsers)p;
												}
												break;
											}
										case "objobject":
											{
												int p;
												if (!int.TryParse(Value, NumberStyles.Integer, Culture, out p) || p < 0 || p > 2)
												{
													Interface.CurrentOptions.CurrentObjParser = ObjParsers.Original;
												}
												else
												{
													Interface.CurrentOptions.CurrentObjParser = (ObjParsers)p;
												}
												break;
											}
									}
									break;
								case "touch":
									switch (Key)
									{
										case "cursor":
											Interface.CurrentOptions.CursorFileName = Value;
											break;
										case "panel2extended":
											Interface.CurrentOptions.Panel2ExtendedMode = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "panel2extendedminsize":
											{
												int a;
												int.TryParse(Value, NumberStyles.Integer, Culture, out a);
												Interface.CurrentOptions.Panel2ExtendedMinSize = a;
											} break;
									}
									break;
							}
						}
					}
				}
			}
			else
			{
				// file not found
				string Code = CultureInfo.CurrentUICulture.Name;
				if (string.IsNullOrEmpty(Code)) Code = "en-US";
				File = Path.CombineFile(Program.FileSystem.GetDataFolder("Languages"), Code + ".xlf");
				if (System.IO.File.Exists(File))
				{
					CurrentOptions.LanguageCode = Code;
				}
				else
				{
					try
					{
						int i = Code.IndexOf("-", StringComparison.Ordinal);
						if (i > 0)
						{
							Code = Code.Substring(0, i);
							File = Path.CombineFile(Program.FileSystem.GetDataFolder("Languages"), Code + ".xlf");
							if (System.IO.File.Exists(File))
							{
								CurrentOptions.LanguageCode = Code;
							}
						}
					}
					catch
					{
						CurrentOptions.LanguageCode = "en-US";
					}
				}
			}
		}
		internal static void SaveOptions()
		{
			CultureInfo Culture = CultureInfo.InvariantCulture;
			StringBuilder Builder = new StringBuilder();
			Builder.AppendLine("; Options");
			Builder.AppendLine("; =======");
			Builder.AppendLine("; This file was automatically generated. Please modify only if you know what you're doing.");
			Builder.AppendLine();
			Builder.AppendLine("[language]");
			Builder.AppendLine("code = " + CurrentOptions.LanguageCode);
			Builder.AppendLine();
			Builder.AppendLine("[interface]");
			Builder.AppendLine("folder = " + CurrentOptions.UserInterfaceFolder);
			{
				string t; switch (CurrentOptions.TimeTableStyle)
				{
					case TimeTableMode.None: t = "none"; break;
					case TimeTableMode.Default: t = "default"; break;
					case TimeTableMode.AutoGenerated: t = "autogenerated"; break;
					case TimeTableMode.PreferCustom: t = "prefercustom"; break;
					default: t = "default"; break;
				}
				Builder.AppendLine("timetablemode = " + t);
			}
			Builder.AppendLine("kioskMode = " + (CurrentOptions.KioskMode ? "true" : "false"));
			Builder.AppendLine("kioskModeTimer = " + CurrentOptions.KioskModeTimer);
			Builder.AppendLine("accessibility = " + (CurrentOptions.Accessibility ? "true" : "false"));
			Builder.AppendLine();
			Builder.AppendLine("[display]");
			Builder.AppendLine("preferNativeBackend = " + (CurrentOptions.PreferNativeBackend ? "true" : "false"));
			Builder.AppendLine("mode = " + (CurrentOptions.FullscreenMode ? "fullscreen" : "window"));
			Builder.AppendLine("vsync = " + (CurrentOptions.VerticalSynchronization ? "true" : "false"));
			Builder.AppendLine("windowWidth = " + CurrentOptions.WindowWidth.ToString(Culture));
			Builder.AppendLine("windowHeight = " + CurrentOptions.WindowHeight.ToString(Culture));
			Builder.AppendLine("fullscreenWidth = " + CurrentOptions.FullscreenWidth.ToString(Culture));
			Builder.AppendLine("fullscreenHeight = " + CurrentOptions.FullscreenHeight.ToString(Culture));
			Builder.AppendLine("fullscreenBits = " + CurrentOptions.FullscreenBits.ToString(Culture));
			Builder.AppendLine("mainmenuWidth = " + CurrentOptions.MainMenuWidth.ToString(Culture));
			Builder.AppendLine("mainmenuHeight = " + CurrentOptions.MainMenuHeight.ToString(Culture));
			Builder.AppendLine("loadInAdvance = " + (CurrentOptions.LoadInAdvance ? "true" : "false"));
			Builder.AppendLine("unloadtextures = " + (CurrentOptions.UnloadUnusedTextures ? "true" : "false"));
			Builder.AppendLine("isUseNewRenderer = " + (CurrentOptions.IsUseNewRenderer ? "true" : "false"));
			Builder.AppendLine();
			Builder.AppendLine("[quality]");
			{
				string t; switch (CurrentOptions.Interpolation)
				{
					case InterpolationMode.NearestNeighbor: t = "nearestNeighbor"; break;
					case InterpolationMode.Bilinear: t = "bilinear"; break;
					case InterpolationMode.NearestNeighborMipmapped: t = "nearestNeighborMipmapped"; break;
					case InterpolationMode.BilinearMipmapped: t = "bilinearMipmapped"; break;
					case InterpolationMode.TrilinearMipmapped: t = "trilinearMipmapped"; break;
					case InterpolationMode.AnisotropicFiltering: t = "anisotropicFiltering"; break;
					default: t = "bilinearMipmapped"; break;
				}
				Builder.AppendLine("interpolation = " + t);
			}
			Builder.AppendLine("anisotropicFilteringLevel = " + CurrentOptions.AnisotropicFilteringLevel.ToString(Culture));
			Builder.AppendLine("anisotropicFilteringMaximum = " + CurrentOptions.AnisotropicFilteringMaximum.ToString(Culture));
			Builder.AppendLine("antiAliasingLevel = " + CurrentOptions.AntiAliasingLevel.ToString(Culture));
			Builder.AppendLine("transparencyMode = " + ((int)CurrentOptions.TransparencyMode).ToString(Culture));
			Builder.AppendLine("oldtransparencymode = " + (CurrentOptions.OldTransparencyMode ? "true" : "false"));
			Builder.AppendLine("viewingDistance = " + CurrentOptions.ViewingDistance.ToString(Culture));
			{
				string t; switch (CurrentOptions.MotionBlur)
				{
					case MotionBlurMode.Low: t = "low"; break;
					case MotionBlurMode.Medium: t = "medium"; break;
					case MotionBlurMode.High: t = "high"; break;
					default: t = "none"; break;
				}
				Builder.AppendLine("motionBlur = " + t);
			}
			Builder.AppendLine();
			Builder.AppendLine("[objectOptimization]");
			Builder.AppendLine("basicThreshold = " + CurrentOptions.ObjectOptimizationBasicThreshold.ToString(Culture));
			Builder.AppendLine("fullThreshold = " + CurrentOptions.ObjectOptimizationFullThreshold.ToString(Culture));
			Builder.AppendLine("vertexCulling = " + CurrentOptions.ObjectOptimizationVertexCulling.ToString(Culture));
			Builder.AppendLine();
			Builder.AppendLine("[simulation]");
			Builder.AppendLine("toppling = " + (CurrentOptions.Toppling ? "true" : "false"));
			Builder.AppendLine("collisions = " + (CurrentOptions.Collisions ? "true" : "false"));
			Builder.AppendLine("derailments = " + (CurrentOptions.Derailments ? "true" : "false"));
			Builder.AppendLine("loadingsway = " + (CurrentOptions.LoadingSway ? "true" : "false"));
			Builder.AppendLine("blackbox = " + (CurrentOptions.BlackBox ? "true" : "false"));
			Builder.Append("mode = ");
			switch (CurrentOptions.GameMode)
			{
				case GameMode.Arcade: Builder.AppendLine("arcade"); break;
				case GameMode.Normal: Builder.AppendLine("normal"); break;
				case GameMode.Expert: Builder.AppendLine("expert"); break;
				default: Builder.AppendLine("normal"); break;
			}
			Builder.AppendLine("acceleratedtimefactor = " + CurrentOptions.TimeAccelerationFactor);
			Builder.AppendLine("enablebvetshacks = " + (CurrentOptions.EnableBveTsHacks ? "true" : "false"));
			Builder.AppendLine();
			Builder.AppendLine("[verbosity]");
			Builder.AppendLine("showWarningMessages = " + (CurrentOptions.ShowWarningMessages ? "true" : "false"));
			Builder.AppendLine("showErrorMessages = " + (CurrentOptions.ShowErrorMessages ? "true" : "false"));
			Builder.AppendLine("debugLog = " + (CurrentOptions.GenerateDebugLogging ? "true" : "false"));
			Builder.AppendLine();
			Builder.AppendLine("[controls]");
			Builder.AppendLine("useJoysticks = " + (CurrentOptions.UseJoysticks ? "true" : "false"));
			Builder.AppendLine("joystickAxisEB = " + (CurrentOptions.AllowAxisEB ? "true" : "false"));
			Builder.AppendLine("joystickAxisthreshold = " + CurrentOptions.JoystickAxisThreshold.ToString(Culture));
			Builder.AppendLine("keyRepeatDelay = " + (1000.0 * CurrentOptions.KeyRepeatDelay).ToString("0", Culture));
			Builder.AppendLine("keyRepeatInterval = " + (1000.0 * CurrentOptions.KeyRepeatInterval).ToString("0", Culture));
			Builder.AppendLine("raildrivermph = " + (CurrentOptions.RailDriverMPH ? "true" : "false"));
			Builder.AppendLine();
			Builder.AppendLine("[sound]");
			Builder.Append("model = ");
			switch (CurrentOptions.SoundModel)
			{
				case SoundModels.Linear: Builder.AppendLine("linear"); break;
				default: Builder.AppendLine("inverse"); break;
			}
			Builder.Append("range = ");
			switch (CurrentOptions.SoundRange)
			{
				case SoundRange.Low: Builder.AppendLine("low"); break;
				case SoundRange.Medium: Builder.AppendLine("medium"); break;
				case SoundRange.High: Builder.AppendLine("high"); break;
				default: Builder.AppendLine("low"); break;
			}
			Builder.AppendLine("number = " + CurrentOptions.SoundNumber.ToString(Culture));
			Builder.AppendLine();
			Builder.AppendLine("[proxy]");
			Builder.AppendLine("url = " + CurrentOptions.ProxyUrl);
			Builder.AppendLine("username = " + CurrentOptions.ProxyUserName);
			Builder.AppendLine("password = " + CurrentOptions.ProxyPassword);
			Builder.AppendLine();
			Builder.AppendLine("[packages]");
			Builder.Append("compression = ");
			switch (CurrentOptions.packageCompressionType)
			{
				case CompressionType.Zip: Builder.AppendLine("zip"); break;
				case CompressionType.TarGZ: Builder.AppendLine("gzip"); break;
				case CompressionType.BZ2: Builder.AppendLine("bzip"); break;
				default: Builder.AppendLine("zip"); break;
			}
			Builder.AppendLine();
			Builder.AppendLine("[folders]");
			Builder.AppendLine("route = " + CurrentOptions.RouteFolder);
			Builder.AppendLine("train = " + CurrentOptions.TrainFolder);
			Builder.AppendLine();
			Builder.AppendLine("[recentlyUsedRoutes]");
			for (int i = 0; i < CurrentOptions.RecentlyUsedRoutes.Length; i++)
			{
				Builder.AppendLine(CurrentOptions.RecentlyUsedRoutes[i]);
			}
			Builder.AppendLine();
			Builder.AppendLine("[recentlyUsedTrains]");
			for (int i = 0; i < CurrentOptions.RecentlyUsedTrains.Length; i++)
			{
				Builder.AppendLine(CurrentOptions.RecentlyUsedTrains[i]);
			}
			Builder.AppendLine();
			Builder.AppendLine("[routeEncodings]");
			for (int i = 0; i < CurrentOptions.RouteEncodings.Length; i++)
			{
				Builder.AppendLine(CurrentOptions.RouteEncodings[i].Codepage.ToString(Culture) + " = " + CurrentOptions.RouteEncodings[i].Value);
			}
			Builder.AppendLine();
			Builder.AppendLine("[trainEncodings]");
			for (int i = 0; i < CurrentOptions.TrainEncodings.Length; i++)
			{
				Builder.AppendLine(CurrentOptions.TrainEncodings[i].Codepage.ToString(Culture) + " = " + CurrentOptions.TrainEncodings[i].Value);
			}
			Builder.AppendLine();
			Builder.AppendLine("[enableInputDevicePlugins]");
			for (int i = 0; i < CurrentOptions.EnableInputDevicePlugins.Length; i++)
			{
				Builder.AppendLine(CurrentOptions.EnableInputDevicePlugins[i]);
			}
			Builder.AppendLine();
			Builder.AppendLine("[Parsers]");
			Builder.AppendLine("xObject = " + (int)Interface.CurrentOptions.CurrentXParser);
			Builder.AppendLine("objObject = " + (int)Interface.CurrentOptions.CurrentObjParser);
			Builder.AppendLine();
			Builder.AppendLine("[Touch]");
			Builder.AppendLine("cursor = " + CurrentOptions.CursorFileName);
			Builder.AppendLine("panel2extended = " + (CurrentOptions.Panel2ExtendedMode ? "true" : "false"));
			Builder.AppendLine("panel2extendedminsize = " + CurrentOptions.Panel2ExtendedMinSize.ToString(Culture));
			string File = Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/options.cfg");
			try
			{
				System.IO.File.WriteAllText(File, Builder.ToString(), new UTF8Encoding(true));
			}
			catch
			{
				MessageBox.Show(@"Failed to write to the Options folder.", @"OpenBVE",  MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
			
		}
	}
}
