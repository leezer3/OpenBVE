using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using OpenBveApi.Colors;
using OpenBveApi.Runtime;
using OpenTK.Input;

namespace OpenBve {
	internal static class Interface {

		// messages
		internal enum MessageType {
			Warning,
			Error,
			Critical
		}
		internal struct Message {
			internal MessageType Type;
			internal bool FileNotFound;
			internal string Text;
		}
		internal static Message[] Messages = new Message[] { };
		internal static int MessageCount = 0;
		internal static void AddMessage(MessageType Type, bool FileNotFound, string Text) {
			if (Type == MessageType.Warning & !CurrentOptions.ShowWarningMessages) return;
			if (Type == MessageType.Error & !CurrentOptions.ShowErrorMessages) return;
			if (MessageCount == 0) {
				Messages = new Message[16];
			} else if (MessageCount >= Messages.Length) {
				Array.Resize<Message>(ref Messages, Messages.Length << 1);
			}
			Messages[MessageCount].Type = Type;
			Messages[MessageCount].FileNotFound = FileNotFound;
			Messages[MessageCount].Text = Text;
			MessageCount++;
			
			Program.AppendToLogFile(Text);
			
		}
		internal static void ClearMessages() {
			Messages = new Message[] { };
			MessageCount = 0;
		}

		// ================================

		// options
		internal struct EncodingValue {
			internal int Codepage;
			internal string Value;
		}
		internal enum MotionBlurMode {
			None = 0,
			Low = 1,
			Medium = 2,
			High = 3
		}
		internal enum SoundRange {
			Low = 0,
			Medium = 1,
			High = 2
		}
		internal enum GameMode {
			Arcade = 0,
			Normal = 1,
			Expert = 2
		}
		internal enum InterpolationMode {
			NearestNeighbor,
			Bilinear,
			NearestNeighborMipmapped,
			BilinearMipmapped,
			TrilinearMipmapped,
			AnisotropicFiltering
		}
		internal class Options {
            /// <summary>The ISO 639-1 code for the current user interface language</summary>
			internal string LanguageCode;
            /// <summary>Whether the program is to be run in full-screen mode</summary>
			internal bool FullscreenMode;
            /// <summary>Whether the program is to be rendered using vertical syncronisation</summary>
			internal bool VerticalSynchronization;
            /// <summary>The screen width (Windowed Mode)</summary>
			internal int WindowWidth;
            /// <summary>The screen height (Windowed Mode)</summary>
			internal int WindowHeight;
            /// <summary>The screen width (Fullscreen Mode)</summary>
			internal int FullscreenWidth;
            /// <summary>The screen height (Fullscreen Mode)</summary>
			internal int FullscreenHeight;
            /// <summary>The number of bits per pixel (Only relevant in fullscreen mode)</summary>
			internal int FullscreenBits;
            /// <summary>The on disk folder in which user interface components are stored</summary>
			internal string UserInterfaceFolder;
            /// <summary>The current pixel interpolation mode </summary>
			internal InterpolationMode Interpolation;
            /// <summary>The current transparency quality mode</summary>
			internal Renderer.TransparencyMode TransparencyMode;
            /// <summary>The level of anisotropic filtering to be applied</summary>
			internal int AnisotropicFilteringLevel;
            /// <summary>The maximum level of anisotropic filtering supported by the system</summary>
			internal int AnisotropicFilteringMaximum;
            /// <summary>The accelerated time factor (1x to 5x)</summary>
		    internal int TimeAccelerationFactor;
            /// <summary>The level of antialiasing to be applied</summary>
			internal int AntiAliasingLevel;
            /// <summary>The viewing distance in meters</summary>
			internal int ViewingDistance;
            /// <summary>The current type of motion blur</summary>
			internal MotionBlurMode MotionBlur;
            /*
             * Note: Object optimisation takes time whilst loading, but may increase the render performance of an
             * object by checking for duplicate vertices etc.
             */
            /// <summary>The minimum number of vertices for basic optimisation to be performed on an object</summary>
			internal int ObjectOptimizationBasicThreshold;
            /// <summary>The minimum number of verticies for full optimisation to be performed on an object</summary>
			internal int ObjectOptimizationFullThreshold;
            /// <summary>Whether toppling is enabled</summary>
			internal bool Toppling;
            /// <summary>Whether collisions between trains are enabled</summary>
			internal bool Collisions;
            /// <summary>Whether derailments are enabled</summary>
			internal bool Derailments;
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
			internal Sounds.SoundModels SoundModel;
            /// <summary>The range outside of which sounds will be inaudible</summary>
			internal SoundRange SoundRange;
            /// <summary>The maximum number of sounds playing at any one time</summary>
			internal int SoundNumber;
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
			internal int RecentlyUsedLimit;
            /// <summary>The list of recently used route character encodings</summary>
			internal EncodingValue[] RouteEncodings;
            /// <summary>The list of recently used train character encodings</summary>
			internal EncodingValue[] TrainEncodings;
            /// <summary>The game mode- Affects how the score is calculated</summary>
			internal GameMode GameMode;
            /// <summary>The width of the main menu window</summary>
			internal int MainMenuWidth;
            /// <summary>The height of the main menu window</summary>
			internal int MainMenuHeight;
            /// <summary>Whether the use of OpenGL display lists is disabled</summary>
			internal bool DisableDisplayLists;
            /// <summary>Whether the simulation will load all textures and sounds into system memory on initial load</summary>
			internal bool LoadInAdvance;
            /// <summary>Whether EB application is possible from the use of a joystick axis</summary>
		    internal bool AllowAxisEB;
            /*
             * Note: Disabling texture resizing may produce artifacts at the edges of textures,
             * and may display issues with certain graphics cards.
             */
            /// <summary>Whether textures are to be resized to the power of two rule</summary>
			internal bool NoTextureResize;
            /*
             * Note: The following options were (are) used by the Managed Content system, and are currently non-functional
             */
            /// <summary>The proxy URL to use when retrieving content from the internet</summary>
			internal string ProxyUrl;
            /// <summary>The proxy username to use when retrieving content from the internet</summary>
			internal string ProxyUserName;
            /// <summary>The proxy password to use when retrieving content from the internet</summary>
			internal string ProxyPassword;
			internal Options() {
				this.LanguageCode = "en-US";
				this.FullscreenMode = false;
				this.VerticalSynchronization = true;
				this.WindowWidth = 960;
				this.WindowHeight = 600;
				this.FullscreenWidth = 1024;
				this.FullscreenHeight = 768;
				this.FullscreenBits = 32;
				this.UserInterfaceFolder = "Default";
				this.Interpolation = Interface.InterpolationMode.BilinearMipmapped;
				this.TransparencyMode = Renderer.TransparencyMode.Quality;
				this.AnisotropicFilteringLevel = 0;
				this.AnisotropicFilteringMaximum = 0;
				this.AntiAliasingLevel = 0;
				this.ViewingDistance = 600;
				this.MotionBlur = MotionBlurMode.None;
				this.Toppling = true;
				this.Collisions = true;
				this.Derailments = true;
				this.GameMode = GameMode.Normal;
				this.BlackBox = false;
				this.UseJoysticks = true;
				this.JoystickAxisThreshold = 0.0;
				this.KeyRepeatDelay = 0.5;
				this.KeyRepeatInterval = 0.1;
				this.SoundModel = Sounds.SoundModels.Inverse;
				this.SoundRange = SoundRange.Low;
				this.SoundNumber = 16;
				this.ShowWarningMessages = true;
				this.ShowErrorMessages = true;
				this.ObjectOptimizationBasicThreshold = 10000;
				this.ObjectOptimizationFullThreshold = 1000;
				this.RouteFolder = "";
				this.TrainFolder = "";
				this.RecentlyUsedRoutes = new string[] { };
				this.RecentlyUsedTrains = new string[] { };
				this.RecentlyUsedLimit = 10;
				this.RouteEncodings = new EncodingValue[] { };
				this.TrainEncodings = new EncodingValue[] { };
				this.MainMenuWidth = 0;
				this.MainMenuHeight = 0;
				this.DisableDisplayLists = false;
				this.LoadInAdvance = false;
				this.NoTextureResize = false;
				this.ProxyUrl = string.Empty;
				this.ProxyUserName = string.Empty;
				this.ProxyPassword = string.Empty;
			    this.TimeAccelerationFactor = 5;
			    this.AllowAxisEB = true;
			}
		}
        /// <summary>The current game options</summary>
		internal static Options CurrentOptions;
        /// <summary>Loads the options file from disk</summary>
		internal static void LoadOptions() {
			CurrentOptions = new Options();
			CultureInfo Culture = CultureInfo.InvariantCulture;
			string File = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "options.cfg");
			if (System.IO.File.Exists(File)) {
				// load options
				string[] Lines = System.IO.File.ReadAllLines(File, new System.Text.UTF8Encoding());
				string Section = "";
				for (int i = 0; i < Lines.Length; i++) {
					Lines[i] = Lines[i].Trim();
					if (Lines[i].Length != 0 && !Lines[i].StartsWith(";", StringComparison.OrdinalIgnoreCase)) {
						if (Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal)) {
							Section = Lines[i].Substring(1, Lines[i].Length - 2).Trim().ToLowerInvariant();
						} else {
							int j = Lines[i].IndexOf("=", StringComparison.OrdinalIgnoreCase);
							string Key, Value;
							if (j >= 0) {
								Key = Lines[i].Substring(0, j).TrimEnd().ToLowerInvariant();
								Value = Lines[i].Substring(j + 1).TrimStart();
							} else {
								Key = "";
								Value = Lines[i];
							}
							switch (Section) {
								case "language":
									switch (Key) {
										case "code":
											Interface.CurrentOptions.LanguageCode = Value.Length != 0 ? Value : "en-US";
											break;
									} break;
								case "interface":
									switch (Key) {
										case "folder":
											Interface.CurrentOptions.UserInterfaceFolder = Value.Length != 0 ? Value : "Default";
											break;
									} break;
								case "display":
									switch (Key) {
										case "mode":
											Interface.CurrentOptions.FullscreenMode = string.Compare(Value, "fullscreen", StringComparison.OrdinalIgnoreCase) == 0;
											break;
										case "vsync":
											Interface.CurrentOptions.VerticalSynchronization = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "windowwidth":
											{
												int a;
												if (!int.TryParse(Value, NumberStyles.Integer, Culture, out a)) {
													a = 960;
												}
												Interface.CurrentOptions.WindowWidth = a;
											} break;
										case "windowheight":
											{
												int a;
												if (!int.TryParse(Value, NumberStyles.Integer, Culture, out a)) {
													a = 600;
												}
												Interface.CurrentOptions.WindowHeight = a;
											} break;
										case "fullscreenwidth":
											{
												int a;
												if (!int.TryParse(Value, NumberStyles.Integer, Culture, out a)) {
													a = 1024;
												}
												Interface.CurrentOptions.FullscreenWidth = a;
											} break;
										case "fullscreenheight":
											{
												int a;
												if (!int.TryParse(Value, NumberStyles.Integer, Culture, out a)) {
													a = 768;
												}
												Interface.CurrentOptions.FullscreenHeight = a;
											} break;
										case "fullscreenbits":
											{
												int a;
												if (!int.TryParse(Value, NumberStyles.Integer, Culture, out a)) {
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
										case "disabledisplaylists":
											Interface.CurrentOptions.DisableDisplayLists = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "loadinadvance":
											Interface.CurrentOptions.LoadInAdvance = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "notextureresize":
											Interface.CurrentOptions.NoTextureResize = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
									} break;
								case "quality":
									switch (Key) {
										case "interpolation":
											switch (Value.ToLowerInvariant()) {
													case "nearestneighbor": Interface.CurrentOptions.Interpolation = Interface.InterpolationMode.NearestNeighbor; break;
													case "bilinear": Interface.CurrentOptions.Interpolation = Interface.InterpolationMode.Bilinear; break;
													case "nearestneighbormipmapped": Interface.CurrentOptions.Interpolation = Interface.InterpolationMode.NearestNeighborMipmapped; break;
													case "bilinearmipmapped": Interface.CurrentOptions.Interpolation = Interface.InterpolationMode.BilinearMipmapped; break;
													case "trilinearmipmapped": Interface.CurrentOptions.Interpolation = Interface.InterpolationMode.TrilinearMipmapped; break;
													case "anisotropicfiltering": Interface.CurrentOptions.Interpolation = Interface.InterpolationMode.AnisotropicFiltering; break;
													default: Interface.CurrentOptions.Interpolation = Interface.InterpolationMode.BilinearMipmapped; break;
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
											switch (Value.ToLowerInvariant()) {
													case "sharp": Interface.CurrentOptions.TransparencyMode = Renderer.TransparencyMode.Performance; break;
													case "smooth": Interface.CurrentOptions.TransparencyMode = Renderer.TransparencyMode.Quality; break;
													default: {
														int a;
														if (int.TryParse(Value, NumberStyles.Integer, Culture, out a)) {
															Interface.CurrentOptions.TransparencyMode = (Renderer.TransparencyMode)a;
														} else {
															Interface.CurrentOptions.TransparencyMode = Renderer.TransparencyMode.Quality;
														}
														break;
													}
											} break;
										case "viewingdistance":
											{
												int a;
												int.TryParse(Value, NumberStyles.Integer, Culture, out a);
												Interface.CurrentOptions.ViewingDistance = a;
											} break;
										case "motionblur":
											switch (Value.ToLowerInvariant()) {
													case "low": Interface.CurrentOptions.MotionBlur = MotionBlurMode.Low; break;
													case "medium": Interface.CurrentOptions.MotionBlur = MotionBlurMode.Medium; break;
													case "high": Interface.CurrentOptions.MotionBlur = MotionBlurMode.High; break;
													default: Interface.CurrentOptions.MotionBlur = MotionBlurMode.None; break;
											} break;
									} break;
								case "objectoptimization":
									switch (Key) {
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
									} break;
								case "simulation":
									switch (Key) {
										case "toppling":
											Interface.CurrentOptions.Toppling = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "collisions":
											Interface.CurrentOptions.Collisions = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "derailments":
											Interface.CurrentOptions.Derailments = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "blackbox":
											Interface.CurrentOptions.BlackBox = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "mode":
											switch (Value.ToLowerInvariant()) {
													case "arcade": Interface.CurrentOptions.GameMode = Interface.GameMode.Arcade; break;
													case "normal": Interface.CurrentOptions.GameMode = Interface.GameMode.Normal; break;
													case "expert": Interface.CurrentOptions.GameMode = Interface.GameMode.Expert; break;
													default: Interface.CurrentOptions.GameMode = Interface.GameMode.Normal; break;
											} break;
                                        case "acceleratedtimefactor":
									        int.TryParse(Value, NumberStyles.Integer, Culture, out Interface.CurrentOptions.TimeAccelerationFactor);
									        break;
									} break;
								case "controls":
									switch (Key) {
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
												Interface.CurrentOptions.KeyRepeatDelay = 0.001 * (double)a;
											} break;
										case "keyrepeatinterval":
											{
												int a;
												int.TryParse(Value, NumberStyles.Integer, Culture, out a);
												if (a <= 0) a = 100;
												Interface.CurrentOptions.KeyRepeatInterval = 0.001 * (double)a;
											} break;
									} break;
								case "sound":
									switch (Key) {
										case "model":
											switch (Value.ToLowerInvariant()) {
													case "linear": Interface.CurrentOptions.SoundModel = Sounds.SoundModels.Linear; break;
													default: Interface.CurrentOptions.SoundModel = Sounds.SoundModels.Inverse; break;
											}
											break;
										case "range":
											switch (Value.ToLowerInvariant()) {
													case "low": Interface.CurrentOptions.SoundRange = SoundRange.Low; break;
													case "medium": Interface.CurrentOptions.SoundRange = SoundRange.Medium; break;
													case "high": Interface.CurrentOptions.SoundRange = SoundRange.High; break;
													default: Interface.CurrentOptions.SoundRange = SoundRange.Low; break;
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
									switch (Key) {
										case "showwarningmessages":
											Interface.CurrentOptions.ShowWarningMessages = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "showerrormessages":
											Interface.CurrentOptions.ShowErrorMessages = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
									} break;
								case "folders":
									switch (Key) {
										case "route":
											Interface.CurrentOptions.RouteFolder = Value;
											break;
										case "train":
											Interface.CurrentOptions.TrainFolder = Value;
											break;
									} break;
								case "proxy":
									switch (Key) {
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
								case "recentlyusedroutes":
									{
										int n = Interface.CurrentOptions.RecentlyUsedRoutes.Length;
										Array.Resize<string>(ref Interface.CurrentOptions.RecentlyUsedRoutes, n + 1);
										Interface.CurrentOptions.RecentlyUsedRoutes[n] = Value;
									} break;
								case "recentlyusedtrains":
									{
										int n = Interface.CurrentOptions.RecentlyUsedTrains.Length;
										Array.Resize<string>(ref Interface.CurrentOptions.RecentlyUsedTrains, n + 1);
										Interface.CurrentOptions.RecentlyUsedTrains[n] = Value;
									} break;
								case "routeencodings":
									{
										int a = System.Text.Encoding.UTF8.CodePage;
										int.TryParse(Key, NumberStyles.Integer, Culture, out a);
										int n = Interface.CurrentOptions.RouteEncodings.Length;
										Array.Resize<EncodingValue>(ref Interface.CurrentOptions.RouteEncodings, n + 1);
										Interface.CurrentOptions.RouteEncodings[n].Codepage = a;
										Interface.CurrentOptions.RouteEncodings[n].Value = Value;
									} break;
								case "trainencodings":
									{
										int a = System.Text.Encoding.UTF8.CodePage;
										int.TryParse(Key, NumberStyles.Integer, Culture, out a);
										int n = Interface.CurrentOptions.TrainEncodings.Length;
										Array.Resize<EncodingValue>(ref Interface.CurrentOptions.TrainEncodings, n + 1);
										Interface.CurrentOptions.TrainEncodings[n].Codepage = a;
										Interface.CurrentOptions.TrainEncodings[n].Value = Value;
									} break;
							}
						}
					}
				}
			} else {
				// file not found
				string Code = CultureInfo.CurrentUICulture.Name;
				if (string.IsNullOrEmpty(Code)) Code = "en-US";
				File = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Languages"), Code + ".cfg");
				if (System.IO.File.Exists(File)) {
					CurrentOptions.LanguageCode = Code;
				} else {
					try {
						int i = Code.IndexOf("-", StringComparison.Ordinal);
						if (i > 0) {
							Code = Code.Substring(0, i);
							File = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Languages"), Code + ".cfg");
							if (System.IO.File.Exists(File)) {
								CurrentOptions.LanguageCode = Code;
							}
						}
					} catch {
						CurrentOptions.LanguageCode = "en-US";
					}
				}
			}
		}
		internal static void SaveOptions() {
			CultureInfo Culture = CultureInfo.InvariantCulture;
			System.Text.StringBuilder Builder = new System.Text.StringBuilder();
			Builder.AppendLine("; Options");
			Builder.AppendLine("; =======");
			Builder.AppendLine("; This file was automatically generated. Please modify only if you know what you're doing.");
			Builder.AppendLine();
			Builder.AppendLine("[language]");
			Builder.AppendLine("code = " + CurrentOptions.LanguageCode);
			Builder.AppendLine();
			Builder.AppendLine("[interface]");
			Builder.AppendLine("folder = " + CurrentOptions.UserInterfaceFolder);
			Builder.AppendLine();
			Builder.AppendLine("[display]");
			Builder.AppendLine("mode = " + (CurrentOptions.FullscreenMode ? "fullscreen" : "window"));
			Builder.AppendLine("vsync = " + (CurrentOptions.VerticalSynchronization ? "true" : "false"));
			Builder.AppendLine("windowWidth = " + CurrentOptions.WindowWidth.ToString(Culture));
			Builder.AppendLine("windowHeight = " + CurrentOptions.WindowHeight.ToString(Culture));
			Builder.AppendLine("fullscreenWidth = " + CurrentOptions.FullscreenWidth.ToString(Culture));
			Builder.AppendLine("fullscreenHeight = " + CurrentOptions.FullscreenHeight.ToString(Culture));
			Builder.AppendLine("fullscreenBits = " + CurrentOptions.FullscreenBits.ToString(Culture));
			Builder.AppendLine("mainmenuWidth = " + CurrentOptions.MainMenuWidth.ToString(Culture));
			Builder.AppendLine("mainmenuHeight = " + CurrentOptions.MainMenuHeight.ToString(Culture));
			Builder.AppendLine("disableDisplayLists = " + (CurrentOptions.DisableDisplayLists ? "true" : "false"));
			Builder.AppendLine("loadInAdvance = " + (CurrentOptions.LoadInAdvance ? "true" : "false"));
			Builder.AppendLine("noTextureResize = " + (CurrentOptions.NoTextureResize ? "true" : "false"));
			Builder.AppendLine();
			Builder.AppendLine("[quality]");
			{
				string t; switch (CurrentOptions.Interpolation) {
						case Interface.InterpolationMode.NearestNeighbor: t = "nearestNeighbor"; break;
						case Interface.InterpolationMode.Bilinear: t = "bilinear"; break;
						case Interface.InterpolationMode.NearestNeighborMipmapped: t = "nearestNeighborMipmapped"; break;
						case Interface.InterpolationMode.BilinearMipmapped: t = "bilinearMipmapped"; break;
						case Interface.InterpolationMode.TrilinearMipmapped: t = "trilinearMipmapped"; break;
						case Interface.InterpolationMode.AnisotropicFiltering: t = "anisotropicFiltering"; break;
						default: t = "bilinearMipmapped"; break;
				}
				Builder.AppendLine("interpolation = " + t);
			}
			Builder.AppendLine("anisotropicFilteringLevel = " + CurrentOptions.AnisotropicFilteringLevel.ToString(Culture));
			Builder.AppendLine("anisotropicFilteringMaximum = " + CurrentOptions.AnisotropicFilteringMaximum.ToString(Culture));
			Builder.AppendLine("antiAliasingLevel = " + CurrentOptions.AntiAliasingLevel.ToString(Culture));
			Builder.AppendLine("transparencyMode = " + ((int)CurrentOptions.TransparencyMode).ToString(Culture));
			Builder.AppendLine("viewingDistance = " + CurrentOptions.ViewingDistance.ToString(Culture));
			{
				string t; switch (CurrentOptions.MotionBlur) {
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
			Builder.AppendLine();
			Builder.AppendLine("[simulation]");
			Builder.AppendLine("toppling = " + (CurrentOptions.Toppling ? "true" : "false"));
			Builder.AppendLine("collisions = " + (CurrentOptions.Collisions ? "true" : "false"));
			Builder.AppendLine("derailments = " + (CurrentOptions.Derailments ? "true" : "false"));
			Builder.AppendLine("blackbox = " + (CurrentOptions.BlackBox ? "true" : "false"));
			Builder.Append("mode = ");
			switch (CurrentOptions.GameMode) {
					case Interface.GameMode.Arcade: Builder.AppendLine("arcade"); break;
					case Interface.GameMode.Normal: Builder.AppendLine("normal"); break;
					case Interface.GameMode.Expert: Builder.AppendLine("expert"); break;
					default: Builder.AppendLine("normal"); break;
			}
            Builder.Append("acceleratedtimefactor = " + CurrentOptions.TimeAccelerationFactor);
			Builder.AppendLine();
			Builder.AppendLine("[verbosity]");
			Builder.AppendLine("showWarningMessages = " + (CurrentOptions.ShowWarningMessages ? "true" : "false"));
			Builder.AppendLine("showErrorMessages = " + (CurrentOptions.ShowErrorMessages ? "true" : "false"));
			Builder.AppendLine();
			Builder.AppendLine("[controls]");
			Builder.AppendLine("useJoysticks = " + (CurrentOptions.UseJoysticks ? "true" : "false"));
            Builder.AppendLine("joystickAxisEB = " + (CurrentOptions.AllowAxisEB ? "true" : "false"));
			Builder.AppendLine("joystickAxisthreshold = " + CurrentOptions.JoystickAxisThreshold.ToString(Culture));
			Builder.AppendLine("keyRepeatDelay = " + (1000.0 * CurrentOptions.KeyRepeatDelay).ToString("0", Culture));
			Builder.AppendLine("keyRepeatInterval = " + (1000.0 * CurrentOptions.KeyRepeatInterval).ToString("0", Culture));
			Builder.AppendLine();
			Builder.AppendLine("[sound]");
			Builder.Append("model = ");
			switch (CurrentOptions.SoundModel) {
					case Sounds.SoundModels.Linear: Builder.AppendLine("linear"); break;
					default: Builder.AppendLine("inverse"); break;
			}
			Builder.Append("range = ");
			switch (CurrentOptions.SoundRange) {
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
			Builder.AppendLine("[folders]");
			Builder.AppendLine("route = " + CurrentOptions.RouteFolder);
			Builder.AppendLine("train = " + CurrentOptions.TrainFolder);
			Builder.AppendLine();
			Builder.AppendLine("[recentlyUsedRoutes]");
			for (int i = 0; i < CurrentOptions.RecentlyUsedRoutes.Length; i++) {
				Builder.AppendLine(CurrentOptions.RecentlyUsedRoutes[i]);
			}
			Builder.AppendLine();
			Builder.AppendLine("[recentlyUsedTrains]");
			for (int i = 0; i < CurrentOptions.RecentlyUsedTrains.Length; i++) {
				Builder.AppendLine(CurrentOptions.RecentlyUsedTrains[i]);
			}
			Builder.AppendLine();
			Builder.AppendLine("[routeEncodings]");
			for (int i = 0; i < CurrentOptions.RouteEncodings.Length; i++) {
				Builder.AppendLine(CurrentOptions.RouteEncodings[i].Codepage.ToString(Culture) + " = " + CurrentOptions.RouteEncodings[i].Value);
			}
			Builder.AppendLine();
			Builder.AppendLine("[trainEncodings]");
			for (int i = 0; i < CurrentOptions.TrainEncodings.Length; i++) {
				Builder.AppendLine(CurrentOptions.TrainEncodings[i].Codepage.ToString(Culture) + " = " + CurrentOptions.TrainEncodings[i].Value);
			}
			string File = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "options.cfg");
			System.IO.File.WriteAllText(File, Builder.ToString(), new System.Text.UTF8Encoding(true));
		}

		// ================================

		// load logs
		internal static void LoadLogs() {
			string File = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "logs.bin");
			try {
				using (System.IO.FileStream Stream = new System.IO.FileStream(File, System.IO.FileMode.Open, System.IO.FileAccess.Read)) {
					using (System.IO.BinaryReader Reader = new System.IO.BinaryReader(Stream, System.Text.Encoding.UTF8)) {
						byte[] Identifier = new byte[] { 111, 112, 101, 110, 66, 86, 69, 95, 76, 79, 71, 83 };
						const short Version = 1;
						byte[] Data = Reader.ReadBytes(Identifier.Length);
						for (int i = 0; i < Identifier.Length; i++) {
							if (Identifier[i] != Data[i]) throw new System.IO.InvalidDataException();
						}
						short Number = Reader.ReadInt16();
						if (Version != Number) throw new System.IO.InvalidDataException();
						Game.LogRouteName = Reader.ReadString();
						Game.LogTrainName = Reader.ReadString();
						Game.LogDateTime = DateTime.FromBinary(Reader.ReadInt64());
						Interface.CurrentOptions.GameMode = (Interface.GameMode)Reader.ReadInt16();
						Game.BlackBoxEntryCount = Reader.ReadInt32();
						Game.BlackBoxEntries = new Game.BlackBoxEntry[Game.BlackBoxEntryCount];
						for (int i = 0; i < Game.BlackBoxEntryCount; i++) {
							Game.BlackBoxEntries[i].Time = Reader.ReadDouble();
							Game.BlackBoxEntries[i].Position = Reader.ReadDouble();
							Game.BlackBoxEntries[i].Speed = Reader.ReadSingle();
							Game.BlackBoxEntries[i].Acceleration = Reader.ReadSingle();
							Game.BlackBoxEntries[i].ReverserDriver = Reader.ReadInt16();
							Game.BlackBoxEntries[i].ReverserSafety = Reader.ReadInt16();
							Game.BlackBoxEntries[i].PowerDriver = (Game.BlackBoxPower)Reader.ReadInt16();
							Game.BlackBoxEntries[i].PowerSafety = (Game.BlackBoxPower)Reader.ReadInt16();
							Game.BlackBoxEntries[i].BrakeDriver = (Game.BlackBoxBrake)Reader.ReadInt16();
							Game.BlackBoxEntries[i].BrakeSafety = (Game.BlackBoxBrake)Reader.ReadInt16();
							Game.BlackBoxEntries[i].EventToken = (Game.BlackBoxEventToken)Reader.ReadInt16();
						}
						Game.ScoreLogCount = Reader.ReadInt32();
						Game.ScoreLogs = new Game.ScoreLog[Game.ScoreLogCount];
						Game.CurrentScore.Value = 0;
						for (int i = 0; i < Game.ScoreLogCount; i++) {
							Game.ScoreLogs[i].Time = Reader.ReadDouble();
							Game.ScoreLogs[i].Position = Reader.ReadDouble();
							Game.ScoreLogs[i].Value = Reader.ReadInt32();
							Game.ScoreLogs[i].TextToken = (Game.ScoreTextToken)Reader.ReadInt16();
							Game.CurrentScore.Value += Game.ScoreLogs[i].Value;
						}
						Game.CurrentScore.Maximum = Reader.ReadInt32();
						Identifier = new byte[] { 95, 102, 105, 108, 101, 69, 78, 68 };
						Data = Reader.ReadBytes(Identifier.Length);
						for (int i = 0; i < Identifier.Length; i++) {
							if (Identifier[i] != Data[i]) throw new System.IO.InvalidDataException();
						}
						Reader.Close();
					} Stream.Close();
				}
			} catch {
				Game.LogRouteName = "";
				Game.LogTrainName = "";
				Game.LogDateTime = DateTime.Now;
				Game.BlackBoxEntries = new Game.BlackBoxEntry[256];
				Game.BlackBoxEntryCount = 0;
				Game.ScoreLogs = new Game.ScoreLog[64];
				Game.ScoreLogCount = 0;
			}
		}

		// save logs
		internal static void SaveLogs() {
			string File = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "logs.bin");
			using (System.IO.FileStream Stream = new System.IO.FileStream(File, System.IO.FileMode.Create, System.IO.FileAccess.Write)) {
				using (System.IO.BinaryWriter Writer = new System.IO.BinaryWriter(Stream, System.Text.Encoding.UTF8)) {
					byte[] Identifier = new byte[] { 111, 112, 101, 110, 66, 86, 69, 95, 76, 79, 71, 83 };
					const short Version = 1;
					Writer.Write(Identifier);
					Writer.Write(Version);
					Writer.Write(Game.LogRouteName);
					Writer.Write(Game.LogTrainName);
					Writer.Write(Game.LogDateTime.ToBinary());
					Writer.Write((short)Interface.CurrentOptions.GameMode);
					Writer.Write(Game.BlackBoxEntryCount);
					for (int i = 0; i < Game.BlackBoxEntryCount; i++) {
						Writer.Write(Game.BlackBoxEntries[i].Time);
						Writer.Write(Game.BlackBoxEntries[i].Position);
						Writer.Write(Game.BlackBoxEntries[i].Speed);
						Writer.Write(Game.BlackBoxEntries[i].Acceleration);
						Writer.Write(Game.BlackBoxEntries[i].ReverserDriver);
						Writer.Write(Game.BlackBoxEntries[i].ReverserSafety);
						Writer.Write((short)Game.BlackBoxEntries[i].PowerDriver);
						Writer.Write((short)Game.BlackBoxEntries[i].PowerSafety);
						Writer.Write((short)Game.BlackBoxEntries[i].BrakeDriver);
						Writer.Write((short)Game.BlackBoxEntries[i].BrakeSafety);
						Writer.Write((short)Game.BlackBoxEntries[i].EventToken);
					}
					Writer.Write(Game.ScoreLogCount);
					for (int i = 0; i < Game.ScoreLogCount; i++) {
						Writer.Write(Game.ScoreLogs[i].Time);
						Writer.Write(Game.ScoreLogs[i].Position);
						Writer.Write(Game.ScoreLogs[i].Value);
						Writer.Write((short)Game.ScoreLogs[i].TextToken);
					}
					Writer.Write(Game.CurrentScore.Maximum);
					Identifier = new byte[] { 95, 102, 105, 108, 101, 69, 78, 68 };
					Writer.Write(Identifier);
					Writer.Close();
				} Stream.Close();
			}
		}

		// get score text
		internal static string GetScoreText(Game.ScoreTextToken TextToken) {
			switch (TextToken) {
					case Game.ScoreTextToken.Overspeed: return GetInterfaceString("score_overspeed");
					case Game.ScoreTextToken.PassedRedSignal: return GetInterfaceString("score_redsignal");
					case Game.ScoreTextToken.Toppling: return GetInterfaceString("score_toppling");
					case Game.ScoreTextToken.Derailed: return GetInterfaceString("score_derailed");
					case Game.ScoreTextToken.PassengerDiscomfort: return GetInterfaceString("score_discomfort");
					case Game.ScoreTextToken.DoorsOpened: return GetInterfaceString("score_doors");
					case Game.ScoreTextToken.ArrivedAtStation: return GetInterfaceString("score_station_arrived");
					case Game.ScoreTextToken.PerfectTimeBonus: return GetInterfaceString("score_station_perfecttime");
					case Game.ScoreTextToken.Late: return GetInterfaceString("score_station_late");
					case Game.ScoreTextToken.PerfectStopBonus: return GetInterfaceString("score_station_perfectstop");
					case Game.ScoreTextToken.Stop: return GetInterfaceString("score_station_stop");
					case Game.ScoreTextToken.PrematureDeparture: return GetInterfaceString("score_station_departure");
					case Game.ScoreTextToken.Total: return GetInterfaceString("score_station_total");
					default: return "?";
			}
		}

		// get black box text
		internal static string GetBlackBoxText(Game.BlackBoxEventToken EventToken) {
			switch (EventToken) {
					default: return "";
			}
		}

		// export score
		internal static void ExportScore(string File) {
			CultureInfo Culture = CultureInfo.InvariantCulture;
			System.Text.StringBuilder Builder = new System.Text.StringBuilder();
			string[][] Lines = new string[Game.ScoreLogCount + 1][];
			Lines[0] = new string[] {
				GetInterfaceString("log_time"),
				GetInterfaceString("log_position"),
				GetInterfaceString("log_value"),
				GetInterfaceString("log_cumulative"),
				GetInterfaceString("log_reason")
			};
			int Columns = Lines[0].Length;
			int TotalScore = 0;
			for (int i = 0; i < Game.ScoreLogCount; i++) {
				int j = i + 1;
				Lines[j] = new string[Columns];
				{
					double x = Game.ScoreLogs[i].Time;
					int h = (int)Math.Floor(x / 3600.0);
					x -= (double)h * 3600.0;
					int m = (int)Math.Floor(x / 60.0);
					x -= (double)m * 60.0;
					int s = (int)Math.Floor(x);
					Lines[j][0] = h.ToString("00", Culture) + ":" + m.ToString("00", Culture) + ":" + s.ToString("00", Culture);
				}
				Lines[j][1] = Game.ScoreLogs[i].Position.ToString("0", Culture);
				Lines[j][2] = Game.ScoreLogs[i].Value.ToString(Culture);
				TotalScore += Game.ScoreLogs[i].Value;
				Lines[j][3] = TotalScore.ToString(Culture);
				Lines[j][4] = GetScoreText(Game.ScoreLogs[i].TextToken);
			}
			int[] Widths = new int[Columns];
			for (int i = 0; i < Lines.Length; i++) {
				for (int j = 0; j < Columns; j++) {
					if (Lines[i][j].Length > Widths[j]) {
						Widths[j] = Lines[i][j].Length;
					}
				}
			}
			{ // header rows
				int TotalWidth = 0;
				for (int j = 0; j < Columns; j++) {
					TotalWidth += Widths[j] + 2;
				}
				TotalWidth += Columns - 1;
				Builder.Append('╔');
				Builder.Append('═', TotalWidth);
				Builder.Append("╗\n");
				{
					Builder.Append('║');
					Builder.Append((" " + GetInterfaceString("log_route") + " " + Game.LogRouteName).PadRight(TotalWidth, ' '));
					Builder.Append("║\n║");
					Builder.Append((" " + GetInterfaceString("log_train") + " " + Game.LogTrainName).PadRight(TotalWidth, ' '));
					Builder.Append("║\n║");
					Builder.Append((" " + GetInterfaceString("log_date") + " " + Game.LogDateTime.ToString("yyyy-MM-dd HH:mm:ss", Culture)).PadRight(TotalWidth, ' '));
					Builder.Append("║\n");
				}
				Builder.Append('╠');
				Builder.Append('═', TotalWidth);
				Builder.Append("╣\n");
				{
					double ratio = Game.CurrentScore.Maximum == 0 ? 0.0 : (double)Game.CurrentScore.Value / (double)Game.CurrentScore.Maximum;
					if (ratio < 0.0) ratio = 0.0;
					if (ratio > 1.0) ratio = 1.0;
					int index = (int)Math.Floor(ratio * (double)Interface.RatingsCount);
					if (index >= Interface.RatingsCount) index = Interface.RatingsCount - 1;
					string s;
					switch (Interface.CurrentOptions.GameMode) {
							case Interface.GameMode.Arcade: s = GetInterfaceString("mode_arcade"); break;
							case Interface.GameMode.Normal: s = GetInterfaceString("mode_normal"); break;
							case Interface.GameMode.Expert: s = GetInterfaceString("mode_expert"); break;
							default: s = GetInterfaceString("mode_unknown"); break;
					}
					Builder.Append('║');
					Builder.Append((" " + GetInterfaceString("log_mode") + " " + s).PadRight(TotalWidth, ' '));
					Builder.Append("║\n║");
					Builder.Append((" " + GetInterfaceString("log_score") + " " + Game.CurrentScore.Value.ToString(Culture) + " / " + Game.CurrentScore.Maximum.ToString(Culture)).PadRight(TotalWidth, ' '));
					Builder.Append("║\n║");
					Builder.Append((" " + GetInterfaceString("log_rating") + " " + GetInterfaceString("rating_" + index.ToString(Culture)) + " (" + (100.0 * ratio).ToString("0.00") + "%)").PadRight(TotalWidth, ' '));
					Builder.Append("║\n");
				}
			}
			{ // top border row
				Builder.Append('╠');
				for (int j = 0; j < Columns; j++) {
					if (j != 0) {
						Builder.Append('╤');
					} Builder.Append('═', Widths[j] + 2);
				} Builder.Append("╣\n");
			}
			for (int i = 0; i < Lines.Length; i++) {
				// center border row
				if (i != 0) {
					Builder.Append('╟');
					for (int j = 0; j < Columns; j++) {
						if (j != 0) {
							Builder.Append('┼');
						} Builder.Append('─', Widths[j] + 2);
					} Builder.Append("╢\n");
				}
				// cell content
				Builder.Append('║');
				for (int j = 0; j < Columns; j++) {
					if (j != 0) Builder.Append('│');
					Builder.Append(' ');
					if (i != 0 & j <= 3) {
						Builder.Append(Lines[i][j].PadLeft(Widths[j], ' '));
					} else {
						Builder.Append(Lines[i][j].PadRight(Widths[j], ' '));
					}
					Builder.Append(' ');
				} Builder.Append("║\n");
			}
			{ // bottom border row
				Builder.Append('╚');
				for (int j = 0; j < Columns; j++) {
					if (j != 0) {
						Builder.Append('╧');
					} Builder.Append('═', Widths[j] + 2);
				} Builder.Append('╝');
			}
			System.IO.File.WriteAllText(File, Builder.ToString(), new System.Text.UTF8Encoding(true));
		}

		// export black box
		internal enum BlackBoxFormat {
			CommaSeparatedValue = 0,
			FormattedText = 1
		}
		internal static void ExportBlackBox(string File, BlackBoxFormat Format) {
			switch (Format) {
					// comma separated value
				case BlackBoxFormat.CommaSeparatedValue:
					{
						CultureInfo Culture = CultureInfo.InvariantCulture;
						System.Text.StringBuilder Builder = new System.Text.StringBuilder();
						for (int i = 0; i < Game.BlackBoxEntryCount; i++) {
							Builder.Append(Game.BlackBoxEntries[i].Time.ToString(Culture) + ",");
							Builder.Append(Game.BlackBoxEntries[i].Position.ToString(Culture) + ",");
							Builder.Append(Game.BlackBoxEntries[i].Speed.ToString(Culture) + ",");
							Builder.Append(Game.BlackBoxEntries[i].Acceleration.ToString(Culture) + ",");
							Builder.Append(((short)Game.BlackBoxEntries[i].ReverserDriver).ToString(Culture) + ",");
							Builder.Append(((short)Game.BlackBoxEntries[i].ReverserSafety).ToString(Culture) + ",");
							Builder.Append(((short)Game.BlackBoxEntries[i].PowerDriver).ToString(Culture) + ",");
							Builder.Append(((short)Game.BlackBoxEntries[i].PowerSafety).ToString(Culture) + ",");
							Builder.Append(((short)Game.BlackBoxEntries[i].BrakeDriver).ToString(Culture) + ",");
							Builder.Append(((short)Game.BlackBoxEntries[i].BrakeSafety).ToString(Culture) + ",");
							Builder.Append(((short)Game.BlackBoxEntries[i].EventToken).ToString(Culture));
							Builder.Append("\r\n");
						}
						System.IO.File.WriteAllText(File, Builder.ToString(), new System.Text.UTF8Encoding(true));
					} break;
					// formatted text
				case BlackBoxFormat.FormattedText:
					{
						CultureInfo Culture = CultureInfo.InvariantCulture;
						System.Text.StringBuilder Builder = new System.Text.StringBuilder();
						string[][] Lines = new string[Game.BlackBoxEntryCount + 1][];
						Lines[0] = new string[] {
							GetInterfaceString("log_time"),
							GetInterfaceString("log_position"),
							GetInterfaceString("log_speed"),
							GetInterfaceString("log_acceleration"),
							GetInterfaceString("log_reverser"),
							GetInterfaceString("log_power"),
							GetInterfaceString("log_brake"),
							GetInterfaceString("log_event"),
						};
						int Columns = Lines[0].Length;
						for (int i = 0; i < Game.BlackBoxEntryCount; i++) {
							int j = i + 1;
							Lines[j] = new string[Columns];
							{
								double x = Game.BlackBoxEntries[i].Time;
								int h = (int)Math.Floor(x / 3600.0);
								x -= (double)h * 3600.0;
								int m = (int)Math.Floor(x / 60.0);
								x -= (double)m * 60.0;
								int s = (int)Math.Floor(x);
								x -= (double)s;
								int n = (int)Math.Floor(1000.0 * x);
								Lines[j][0] = h.ToString("00", Culture) + ":" + m.ToString("00", Culture) + ":" + s.ToString("00", Culture) + ":" + n.ToString("000", Culture);
							}
							Lines[j][1] = Game.BlackBoxEntries[i].Position.ToString("0.000", Culture);
							Lines[j][2] = Game.BlackBoxEntries[i].Speed.ToString("0.0000", Culture);
							Lines[j][3] = Game.BlackBoxEntries[i].Acceleration.ToString("0.0000", Culture);
							{
								string[] reverser = new string[2];
								for (int k = 0; k < 2; k++) {
									short r = k == 0 ? Game.BlackBoxEntries[i].ReverserDriver : Game.BlackBoxEntries[i].ReverserSafety;
									switch (r) {
										case -1:
											reverser[k] = QuickReferences.HandleBackward;
											break;
										case 0:
											reverser[k] = QuickReferences.HandleNeutral;
											break;
										case 1:
											reverser[k] = QuickReferences.HandleForward;
											break;
										default:
											reverser[k] = r.ToString(Culture);
											break;
									}
								}
								Lines[j][4] = reverser[0] + " → " + reverser[1];
							}
							{
								string[] power = new string[2];
								for (int k = 0; k < 2; k++) {
									Game.BlackBoxPower p = k == 0 ? Game.BlackBoxEntries[i].PowerDriver : Game.BlackBoxEntries[i].PowerSafety;
									switch (p) {
										case Game.BlackBoxPower.PowerNull:
											power[k] = GetInterfaceString(QuickReferences.HandlePowerNull);
											break;
										default:
											power[k] = GetInterfaceString(QuickReferences.HandlePower) + ((short)p).ToString(Culture);
											break;
									}
								}
								Lines[j][5] = power[0] + " → " + power[1];
							}
							{
								string[] brake = new string[2];
								for (int k = 0; k < 2; k++) {
									Game.BlackBoxBrake b = k == 0 ? Game.BlackBoxEntries[i].BrakeDriver : Game.BlackBoxEntries[i].BrakeSafety;
									switch (b) {
										case Game.BlackBoxBrake.BrakeNull:
											brake[k] = GetInterfaceString(QuickReferences.HandleBrakeNull);
											break;
										case Game.BlackBoxBrake.Emergency:
											brake[k] = GetInterfaceString(QuickReferences.HandleEmergency);
											break;
										case Game.BlackBoxBrake.HoldBrake:
											brake[k] = GetInterfaceString(QuickReferences.HandleHoldBrake);
											break;
										case Game.BlackBoxBrake.Release:
											brake[k] = GetInterfaceString(QuickReferences.HandleRelease);
											break;
										case Game.BlackBoxBrake.Lap:
											brake[k] = GetInterfaceString(QuickReferences.HandleLap);
											break;
										case Game.BlackBoxBrake.Service:
											brake[k] = GetInterfaceString(QuickReferences.HandleService);
											break;
										default:
											brake[k] = GetInterfaceString(QuickReferences.HandleBrake) + ((short)b).ToString(Culture);
											break;
									}
								}
								Lines[j][6] = brake[0] + " → " + brake[1];
							}
							Lines[j][7] = GetBlackBoxText(Game.BlackBoxEntries[i].EventToken);
						}
						int[] Widths = new int[Columns];
						for (int i = 0; i < Lines.Length; i++) {
							for (int j = 0; j < Columns; j++) {
								if (Lines[i][j].Length > Widths[j]) {
									Widths[j] = Lines[i][j].Length;
								}
							}
						}
						{ // header rows
							int TotalWidth = 0;
							for (int j = 0; j < Columns; j++) {
								TotalWidth += Widths[j] + 2;
							}
							TotalWidth += Columns - 1;
							Builder.Append('╔');
							Builder.Append('═', TotalWidth);
							Builder.Append("╗\r\n");
							{
								Builder.Append('║');
								Builder.Append((" " + GetInterfaceString("log_route") + " " + Game.LogRouteName).PadRight(TotalWidth, ' '));
								Builder.Append("║\r\n║");
								Builder.Append((" " + GetInterfaceString("log_train") + " " + Game.LogTrainName).PadRight(TotalWidth, ' '));
								Builder.Append("║\r\n║");
								Builder.Append((" " + GetInterfaceString("log_date") + " " + Game.LogDateTime.ToString("yyyy-MM-dd HH:mm:ss", Culture)).PadRight(TotalWidth, ' '));
								Builder.Append("║\r\n");
							}
						}
						{ // top border row
							Builder.Append('╠');
							for (int j = 0; j < Columns; j++) {
								if (j != 0) {
									Builder.Append('╤');
								} Builder.Append('═', Widths[j] + 2);
							} Builder.Append("╣\r\n");
						}
						for (int i = 0; i < Lines.Length; i++) {
							// center border row
							if (i != 0) {
								Builder.Append('╟');
								for (int j = 0; j < Columns; j++) {
									if (j != 0) {
										Builder.Append('┼');
									} Builder.Append('─', Widths[j] + 2);
								} Builder.Append("╢\r\n");
							}
							// cell content
							Builder.Append('║');
							for (int j = 0; j < Columns; j++) {
								if (j != 0) Builder.Append('│');
								Builder.Append(' ');
								if (i != 0 & j <= 3) {
									Builder.Append(Lines[i][j].PadLeft(Widths[j], ' '));
								} else {
									Builder.Append(Lines[i][j].PadRight(Widths[j], ' '));
								}
								Builder.Append(' ');
							} Builder.Append("║\r\n");
						}
						{ // bottom border row
							Builder.Append('╚');
							for (int j = 0; j < Columns; j++) {
								if (j != 0) {
									Builder.Append('╧');
								} Builder.Append('═', Widths[j] + 2);
							} Builder.Append('╝');
						}
						System.IO.File.WriteAllText(File, Builder.ToString(), new System.Text.UTF8Encoding(true));
					} break;
			}
		}

		// ================================

		// interface strings
		private struct InterfaceString {
			internal string Name;
			internal string Text;
		}
		private static InterfaceString[] InterfaceStrings = new InterfaceString[16];
		private static int InterfaceStringCount = 0;
		private static int CurrentInterfaceStringIndex = 0;
		private static void AddInterfaceString(string Name, string Text) {
			if (InterfaceStringCount >= InterfaceStrings.Length) {
				Array.Resize<InterfaceString>(ref InterfaceStrings, InterfaceStrings.Length << 1);
			}
			InterfaceStrings[InterfaceStringCount].Name = Name;
			InterfaceStrings[InterfaceStringCount].Text = Text;
			InterfaceStringCount++;
		}
		internal static string GetInterfaceString(string Name) {
			int n = Name.Length;
			for (int k = 0; k < InterfaceStringCount; k++) {
				int i;
				if ((k & 1) == 0) {
					i = (CurrentInterfaceStringIndex + (k >> 1) + InterfaceStringCount) % InterfaceStringCount;
				} else {
					i = (CurrentInterfaceStringIndex - (k + 1 >> 1) + InterfaceStringCount) % InterfaceStringCount;
				}
				if (InterfaceStrings[i].Name.Length == n) {
					if (InterfaceStrings[i].Name == Name) {
						CurrentInterfaceStringIndex = (i + 1) % InterfaceStringCount;
						return InterfaceStrings[i].Text;
					}
				}
			}
			return Name;
		}
		internal struct InterfaceQuickReference {
			internal string HandleForward;
			internal string HandleNeutral;
			internal string HandleBackward;
			internal string HandlePower;
			internal string HandlePowerNull;
			internal string HandleBrake;
			internal string HandleBrakeNull;
			internal string HandleRelease;
			internal string HandleLap;
			internal string HandleService;
			internal string HandleEmergency;
			internal string HandleHoldBrake;
			internal string DoorsLeft;
			internal string DoorsRight;
			internal string Score;
		}
		internal static InterfaceQuickReference QuickReferences;
		internal static int RatingsCount = 10;

	    internal static string CurrentControl;
	    internal static string CurrentControlDescription;

		// load language
		internal static void LoadLanguage(string File) {
			string[] Lines = System.IO.File.ReadAllLines(File, new System.Text.UTF8Encoding());
			string Section = "";
			InterfaceStrings = new InterfaceString[16];
			InterfaceStringCount = 0;
			QuickReferences.HandleForward = "F";
			QuickReferences.HandleNeutral = "N";
			QuickReferences.HandleBackward = "B";
			QuickReferences.HandlePower = "P";
			QuickReferences.HandlePowerNull = "N";
			QuickReferences.HandleBrake = "B";
			QuickReferences.HandleBrakeNull = "N";
			QuickReferences.HandleRelease = "RL";
			QuickReferences.HandleLap = "LP";
			QuickReferences.HandleService = "SV";
			QuickReferences.HandleEmergency = "EM";
			QuickReferences.HandleHoldBrake = "HB";
			QuickReferences.DoorsLeft = "L";
			QuickReferences.DoorsRight = "R";
			QuickReferences.Score = "Score: ";
			for (int i = 0; i < Lines.Length; i++) {
				Lines[i] = Lines[i].Trim();
				if (!Lines[i].StartsWith(";")) {
					if (Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal)) {
						Section = Lines[i].Substring(1, Lines[i].Length - 2).Trim().ToLowerInvariant();
					} else {
						int j = Lines[i].IndexOf('=');
						if (j >= 0) {
							string a = Lines[i].Substring(0, j).TrimEnd().ToLowerInvariant();
							string b = Interface.Unescape(Lines[i].Substring(j + 1).TrimStart());
							switch (Section) {
								case "handles":
									switch (a) {
											case "forward": Interface.QuickReferences.HandleForward = b; break;
											case "neutral": Interface.QuickReferences.HandleNeutral = b; break;
											case "backward": Interface.QuickReferences.HandleBackward = b; break;
											case "power": Interface.QuickReferences.HandlePower = b; break;
											case "powernull": Interface.QuickReferences.HandlePowerNull = b; break;
											case "brake": Interface.QuickReferences.HandleBrake = b; break;
											case "brakenull": Interface.QuickReferences.HandleBrakeNull = b; break;
											case "release": Interface.QuickReferences.HandleRelease = b; break;
											case "lap": Interface.QuickReferences.HandleLap = b; break;
											case "service": Interface.QuickReferences.HandleService = b; break;
											case "emergency": Interface.QuickReferences.HandleEmergency = b; break;
											case "holdbrake": Interface.QuickReferences.HandleHoldBrake = b; break;
									} break;
								case "doors":
									switch (a) {
											case "left": Interface.QuickReferences.DoorsLeft = b; break;
											case "right": Interface.QuickReferences.DoorsRight = b; break;
									} break;
								case "misc":
									switch (a) {
											case "score": Interface.QuickReferences.Score = b; break;
									} break;
								case "commands":
									{
										for (int k = 0; k < CommandInfos.Length; k++) {
											if (string.Compare(CommandInfos[k].Name, a, StringComparison.OrdinalIgnoreCase) == 0) {
												CommandInfos[k].Description = b;
												break;
											}
										}
									} break;
								case "keys":
									{
										for (int k = 0; k < Keys.Length; k++) {
											if (string.Compare(Keys[k].Name, a, StringComparison.OrdinalIgnoreCase) == 0) {
												Keys[k].Description = b;
												break;
											}
										}
									} break;
								default:
									AddInterfaceString(Section + "_" + a, b);
									break;
							}
						}
					}
				}
			}
		}

		// ================================

		// commands
		internal enum Command {
			None = 0,
			PowerIncrease, PowerDecrease, PowerHalfAxis, PowerFullAxis,
			BrakeIncrease, BrakeDecrease, BrakeEmergency, BrakeHalfAxis, BrakeFullAxis,
			SinglePower, SingleNeutral, SingleBrake, SingleEmergency, SingleFullAxis,
			ReverserForward, ReverserBackward, ReverserFullAxis,
			DoorsLeft, DoorsRight,
			HornPrimary, HornSecondary, HornMusic,
			DeviceConstSpeed,
			SecurityS, SecurityA1, SecurityA2, SecurityB1, SecurityB2, SecurityC1, SecurityC2,
            SecurityD, SecurityE, SecurityF, SecurityG, SecurityH, SecurityI, SecurityJ, SecurityK, SecurityL, SecurityM,
            SecurityN, SecurityO, SecurityP,
			CameraInterior, CameraExterior, CameraTrack, CameraFlyBy,
			CameraMoveForward, CameraMoveBackward, CameraMoveLeft, CameraMoveRight, CameraMoveUp, CameraMoveDown,
			CameraRotateLeft, CameraRotateRight, CameraRotateUp, CameraRotateDown, CameraRotateCCW, CameraRotateCW,
			CameraZoomIn, CameraZoomOut, CameraPreviousPOI, CameraNextPOI, CameraReset, CameraRestriction,
			TimetableToggle, TimetableUp, TimetableDown,
			MiscClock, MiscSpeed, MiscFps, MiscAI, MiscInterfaceMode, MiscBackfaceCulling, MiscCPUMode,
			MiscTimeFactor, MiscPause, MiscMute, MiscFullscreen, MiscQuit,
			MenuActivate, MenuUp, MenuDown, MenuEnter, MenuBack,
			DebugWireframe, DebugNormals, DebugBrakeSystems,
            /* 
             * These keys were added in 1.4.4.0
             * Plugins should refer to these rather than the deprecated Security keys
             * Doing this means that key assignments can be changed globally for all locomotives
             * that support this method, rather than the haphazard and non-standard use
             * of the security keys
             * 
             */

            //Common Keys
            WiperSpeedUp,WiperSpeedDown,FillFuel,
            //Steam locomotive
            LiveSteamInjector,ExhaustSteamInjector,IncreaseCutoff,DecreaseCutoff,Blowers,
            //Diesel Locomotive
            EngineStart,EngineStop,GearUp,GearDown,
            //Electric Locomotive
            RaisePantograph,LowerPantograph,MainBreaker,
            //Other
            RouteInformation

		}
        /// <summary>
        /// Converts the specified security command to a virtual key.
        /// </summary>
        /// <returns>Virtual key for plugins.</returns>
        /// <param name="cmd">Security command. If this isn't security command, ArgumentException will be thrown.</param>
        internal static VirtualKeys SecurityToVirtualKey(Command cmd)
        {
            string cmdname = Enum.GetName(typeof(Command), cmd);
            if (cmdname == null) throw new ArgumentNullException("cmdname");
            if (!cmdname.StartsWith("Security", StringComparison.InvariantCulture))
                throw new ArgumentException("Command is not a security command.", "cmd");
            string ending = cmdname.Substring(8).ToUpperInvariant();
            VirtualKeys key;
            if (!Enum.TryParse(ending, out key))
                throw new ArgumentException("VirtualKeys does not contain following security key: " + ending, "cmd");
            return key;
        }
		internal enum CommandType { Digital, AnalogHalf, AnalogFull }
		internal struct CommandInfo {
			internal Command Command;
			internal CommandType Type;
			internal string Name;
			internal string Description;
			internal CommandInfo(Command Command, CommandType Type, string Name) {
				this.Command = Command;
				this.Type = Type;
				this.Name = Name;
				this.Description = "N/A";
			}
		}

		// key infos
		internal struct KeyInfo {
			internal int Value;
			internal string Name;
			internal string Description;
			internal KeyInfo(int Value, string Name, string Description) {
				this.Value = Value;
				this.Name = Name;
				this.Description = Description;
			}
		}
		internal static KeyInfo[] Keys = new KeyInfo[] {
            /*
			new KeyInfo(Sdl.SDLK_0, "0", "0"),
			new KeyInfo(Sdl.SDLK_1, "1", "1"),
			new KeyInfo(Sdl.SDLK_2, "2", "2"),
			new KeyInfo(Sdl.SDLK_3, "3", "3"),
			new KeyInfo(Sdl.SDLK_4, "4", "4"),
			new KeyInfo(Sdl.SDLK_5, "5", "5"),
			new KeyInfo(Sdl.SDLK_6, "6", "6"),
			new KeyInfo(Sdl.SDLK_7, "7", "7"),
			new KeyInfo(Sdl.SDLK_8, "8", "8"),
			new KeyInfo(Sdl.SDLK_9, "9", "9"),
			new KeyInfo(Sdl.SDLK_AMPERSAND, "AMPERSAND", "Ampersand"),
			new KeyInfo(Sdl.SDLK_ASTERISK, "ASTERISK", "Asterisk"),
			new KeyInfo(Sdl.SDLK_AT, "AT", "At"),
			new KeyInfo(Sdl.SDLK_BACKQUOTE, "BACKQUOTE", "Backquote"),
			new KeyInfo(Sdl.SDLK_BACKSLASH, "BACKSLASH", "Backslash"),
			new KeyInfo(Sdl.SDLK_BACKSPACE, "BACKSPACE", "Backspace"),
			new KeyInfo(Sdl.SDLK_BREAK, "BREAK", "Break"),
			new KeyInfo(Sdl.SDLK_CAPSLOCK, "CAPSLOCK", "Capslock"),
			new KeyInfo(Sdl.SDLK_CARET, "CARET", "Caret"),
			new KeyInfo(Sdl.SDLK_CLEAR, "CLEAR", "Clear"),
			new KeyInfo(Sdl.SDLK_COLON, "COLON", "Colon"),
			new KeyInfo(Sdl.SDLK_COMMA, "COMMA", "Comma"),
			new KeyInfo(Sdl.SDLK_DELETE, "DELETE", "Delete"),
			new KeyInfo(Sdl.SDLK_DOLLAR, "DOLLAR", "Dollar"),
			new KeyInfo(Sdl.SDLK_DOWN, "DOWN", "Down"),
			new KeyInfo(Sdl.SDLK_END, "END", "End"),
			new KeyInfo(Sdl.SDLK_EQUALS, "EQUALS", "Equals"),
			new KeyInfo(Sdl.SDLK_ESCAPE, "ESCAPE", "Escape"),
			new KeyInfo(Sdl.SDLK_EURO, "EURO", "Euro"),
			new KeyInfo(Sdl.SDLK_EXCLAIM, "EXCLAIM", "Exclamation"),
			new KeyInfo(Sdl.SDLK_F1, "F1", "F1"),
			new KeyInfo(Sdl.SDLK_F2, "F2", "F2"),
			new KeyInfo(Sdl.SDLK_F3, "F3", "F3"),
			new KeyInfo(Sdl.SDLK_F4, "F4", "F4"),
			new KeyInfo(Sdl.SDLK_F5, "F5", "F5"),
			new KeyInfo(Sdl.SDLK_F6, "F6", "F6"),
			new KeyInfo(Sdl.SDLK_F7, "F7", "F7"),
			new KeyInfo(Sdl.SDLK_F8, "F8", "F8"),
			new KeyInfo(Sdl.SDLK_F9, "F9", "F9"),
			new KeyInfo(Sdl.SDLK_F10, "F10", "F10"),
			new KeyInfo(Sdl.SDLK_F11, "F11", "F11"),
			new KeyInfo(Sdl.SDLK_F12, "F12", "F12"),
			new KeyInfo(Sdl.SDLK_F13, "F13", "F13"),
			new KeyInfo(Sdl.SDLK_F14, "F14", "F14"),
			new KeyInfo(Sdl.SDLK_F15, "F15", "F15"),
			new KeyInfo(Sdl.SDLK_GREATER, "GREATER", "Greater"),
			new KeyInfo(Sdl.SDLK_HASH, "HASH", "Hash"),
			new KeyInfo(Sdl.SDLK_HELP, "HELP", "Help"),
			new KeyInfo(Sdl.SDLK_HOME, "HOME", "Home"),
			new KeyInfo(Sdl.SDLK_INSERT, "INSERT", "Insert"),
			new KeyInfo(Sdl.SDLK_KP0, "KP0", "Keypad 0"),
			new KeyInfo(Sdl.SDLK_KP1, "KP1", "Keypad 1"),
			new KeyInfo(Sdl.SDLK_KP2, "KP2", "Keypad 2"),
			new KeyInfo(Sdl.SDLK_KP3, "KP3", "Keypad 3"),
			new KeyInfo(Sdl.SDLK_KP4, "KP4", "Keypad 4"),
			new KeyInfo(Sdl.SDLK_KP5, "KP5", "Keypad 5"),
			new KeyInfo(Sdl.SDLK_KP6, "KP6", "Keypad 6"),
			new KeyInfo(Sdl.SDLK_KP7, "KP7", "Keypad 7"),
			new KeyInfo(Sdl.SDLK_KP8, "KP8", "Keypad 8"),
			new KeyInfo(Sdl.SDLK_KP9, "KP9", "Keypad 9"),
			new KeyInfo(Sdl.SDLK_KP_DIVIDE, "KP_DIVIDE", "Keypad Divide"),
			new KeyInfo(Sdl.SDLK_KP_ENTER, "KP_ENTER", "Keypad Enter"),
			new KeyInfo(Sdl.SDLK_KP_EQUALS, "KP_EQUALS", "Keypad Equals"),
			new KeyInfo(Sdl.SDLK_KP_MINUS, "KP_MINUS", "Keypad Minus"),
			new KeyInfo(Sdl.SDLK_KP_MULTIPLY, "KP_MULTIPLY", "Keypad Multiply"),
			new KeyInfo(Sdl.SDLK_KP_PERIOD, "KP_PERIOD", "Keypad Period"),
			new KeyInfo(Sdl.SDLK_KP_PLUS, "KP_PLUS", "Keypad Plus"),
			new KeyInfo(Sdl.SDLK_LALT, "LALT", "Left Alt"),
			new KeyInfo(Sdl.SDLK_LCTRL, "LCTRL", "Left Ctrl"),
			new KeyInfo(Sdl.SDLK_LEFT, "LEFT", "Left"),
			new KeyInfo(Sdl.SDLK_LEFTBRACKET, "LEFTBRACKET", "Left bracket"),
			new KeyInfo(Sdl.SDLK_LEFTPAREN, "LEFTPAREN", "Left parenthesis"),
			new KeyInfo(Sdl.SDLK_LESS, "LESS", "Less"),
			new KeyInfo(Sdl.SDLK_LMETA, "LMETA", "Left Meta"),
			new KeyInfo(Sdl.SDLK_LSHIFT, "LSHIFT", "Left Shift"),
			new KeyInfo(Sdl.SDLK_LSUPER, "LSUPER", "Left Application"),
			new KeyInfo(Sdl.SDLK_MENU, "MENU", "Menu"),
			new KeyInfo(Sdl.SDLK_MINUS, "MINUS", "Minus"),
			new KeyInfo(Sdl.SDLK_MODE, "MODE", "Alt Gr"),
			new KeyInfo(Sdl.SDLK_NUMLOCK, "NUMLOCK", "Numlock"),
			new KeyInfo(Sdl.SDLK_PAGEDOWN, "PAGEDOWN", "Page down"),
			new KeyInfo(Sdl.SDLK_PAGEUP, "PAGEUP", "Page up"),
			new KeyInfo(Sdl.SDLK_PAUSE, "PAUSE", "Pause"),
			new KeyInfo(Sdl.SDLK_PERIOD, "PERIOD", "Period"),
			new KeyInfo(Sdl.SDLK_PLUS, "PLUS", "Plus"),
			new KeyInfo(Sdl.SDLK_POWER, "POWER", "Power"),
			new KeyInfo(Sdl.SDLK_PRINT, "PRINT", "Print"),
			new KeyInfo(Sdl.SDLK_QUESTION, "QUESTION", "Question"),
			new KeyInfo(Sdl.SDLK_QUOTE, "QUOTE", "Quote"),
			new KeyInfo(Sdl.SDLK_QUOTEDBL, "QUOTEDBL", "Quote double"),
			new KeyInfo(Sdl.SDLK_RALT, "RALT", "Right Alt"),
			new KeyInfo(Sdl.SDLK_RCTRL, "RCTRL", "Right Ctrl"),
			new KeyInfo(Sdl.SDLK_RETURN, "RETURN", "Return"),
			new KeyInfo(Sdl.SDLK_RIGHT, "RIGHT", "Right"),
			new KeyInfo(Sdl.SDLK_RIGHTBRACKET, "RIGHTBRACKET", "Right bracket"),
			new KeyInfo(Sdl.SDLK_RIGHTPAREN, "RIGHTPAREN", "Right parenthesis"),
			new KeyInfo(Sdl.SDLK_RMETA, "RMETA", "Right Meta"),
			new KeyInfo(Sdl.SDLK_RSHIFT, "RSHIFT", "Right Shift"),
			new KeyInfo(Sdl.SDLK_RSUPER, "RSUPER", "Right Application"),
			new KeyInfo(Sdl.SDLK_SCROLLOCK, "SCROLLLOCK", "Scrolllock"),
			new KeyInfo(Sdl.SDLK_SEMICOLON, "SEMICOLON", "Semicolon"),
			new KeyInfo(Sdl.SDLK_SLASH, "SLASH", "Slash"),
			new KeyInfo(Sdl.SDLK_SPACE, "SPACE", "Space"),
			new KeyInfo(Sdl.SDLK_SYSREQ, "SYSREQ", "SysRq"),
			new KeyInfo(Sdl.SDLK_TAB, "TAB", "Tab"),
			new KeyInfo(Sdl.SDLK_UNDERSCORE, "UNDERSCORE", "Underscore"),
			new KeyInfo(Sdl.SDLK_UP, "UP", "Up"),
			new KeyInfo(Sdl.SDLK_a, "a", "A"),
			new KeyInfo(Sdl.SDLK_b, "b", "B"),
			new KeyInfo(Sdl.SDLK_c, "c", "C"),
			new KeyInfo(Sdl.SDLK_d, "d", "D"),
			new KeyInfo(Sdl.SDLK_e, "e", "E"),
			new KeyInfo(Sdl.SDLK_f, "f", "F"),
			new KeyInfo(Sdl.SDLK_g, "g", "G"),
			new KeyInfo(Sdl.SDLK_h, "h", "H"),
			new KeyInfo(Sdl.SDLK_i, "i", "I"),
			new KeyInfo(Sdl.SDLK_j, "j", "J"),
			new KeyInfo(Sdl.SDLK_k, "k", "K"),
			new KeyInfo(Sdl.SDLK_l, "l", "L"),
			new KeyInfo(Sdl.SDLK_m, "m", "M"),
			new KeyInfo(Sdl.SDLK_n, "n", "N"),
			new KeyInfo(Sdl.SDLK_o, "o", "O"),
			new KeyInfo(Sdl.SDLK_p, "p", "P"),
			new KeyInfo(Sdl.SDLK_q, "q", "Q"),
			new KeyInfo(Sdl.SDLK_r, "r", "R"),
			new KeyInfo(Sdl.SDLK_s, "s", "S"),
			new KeyInfo(Sdl.SDLK_t, "t", "T"),
			new KeyInfo(Sdl.SDLK_u, "u", "U"),
			new KeyInfo(Sdl.SDLK_v, "v", "V"),
			new KeyInfo(Sdl.SDLK_w, "w", "W"),
			new KeyInfo(Sdl.SDLK_x, "x", "X"),
			new KeyInfo(Sdl.SDLK_y, "y", "Y"),
			new KeyInfo(Sdl.SDLK_z, "z", "Z")
             */
		};

		// controls
		internal enum ControlMethod {
			Invalid = 0,
			Keyboard = 1,
			Joystick = 2
		}

	    [Flags]
	    internal enum KeyboardModifier {
			None = 0,
			Shift = 1,
			Ctrl = 2,
			Alt = 4
		}
		internal enum JoystickComponent { Invalid, Axis, FullAxis, Ball, Hat, Button }
		internal enum DigitalControlState {
			ReleasedAcknowledged = 0,
			Released = 1,
			Pressed = 2,
			PressedAcknowledged = 3
		}
		internal struct Control {
			internal Command Command;
			internal CommandType InheritedType;
			internal ControlMethod Method;
			internal KeyboardModifier Modifier;
			internal int Device;
			internal JoystickComponent Component;
		    internal int Element;
		    internal bool Pressed;
			internal int Direction;
			internal DigitalControlState DigitalState;
			internal double AnalogState;
            internal Key Key;
		    internal string LastState;
		    internal bool JoystickPressed;
		}

		// control descriptions
		internal static string[] ControlDescriptions = new string[] { };
		internal static CommandInfo[] CommandInfos = new CommandInfo[] {
			new CommandInfo(Command.PowerIncrease, CommandType.Digital, "POWER_INCREASE"),
			new CommandInfo(Command.PowerDecrease, CommandType.Digital, "POWER_DECREASE"),
			new CommandInfo(Command.PowerHalfAxis, CommandType.AnalogHalf, "POWER_HALFAXIS"),
			new CommandInfo(Command.PowerFullAxis, CommandType.AnalogFull, "POWER_FULLAXIS"),
			new CommandInfo(Command.BrakeDecrease, CommandType.Digital, "BRAKE_DECREASE"),
			new CommandInfo(Command.BrakeIncrease, CommandType.Digital, "BRAKE_INCREASE"),
			new CommandInfo(Command.BrakeHalfAxis, CommandType.AnalogHalf, "BRAKE_HALFAXIS"),
			new CommandInfo(Command.BrakeFullAxis, CommandType.AnalogFull, "BRAKE_FULLAXIS"),
			new CommandInfo(Command.BrakeEmergency, CommandType.Digital, "BRAKE_EMERGENCY"),
			new CommandInfo(Command.SinglePower, CommandType.Digital, "SINGLE_POWER"),
			new CommandInfo(Command.SingleNeutral, CommandType.Digital, "SINGLE_NEUTRAL"),
			new CommandInfo(Command.SingleBrake, CommandType.Digital, "SINGLE_BRAKE"),
			new CommandInfo(Command.SingleEmergency, CommandType.Digital, "SINGLE_EMERGENCY"),
			new CommandInfo(Command.SingleFullAxis, CommandType.AnalogFull, "SINGLE_FULLAXIS"),
			new CommandInfo(Command.ReverserForward, CommandType.Digital, "REVERSER_FORWARD"),
			new CommandInfo(Command.ReverserBackward, CommandType.Digital, "REVERSER_BACKWARD"),
			new CommandInfo(Command.ReverserFullAxis, CommandType.AnalogFull, "REVERSER_FULLAXIS"),
			new CommandInfo(Command.DoorsLeft, CommandType.Digital, "DOORS_LEFT"),
			new CommandInfo(Command.DoorsRight, CommandType.Digital, "DOORS_RIGHT"),
			new CommandInfo(Command.HornPrimary, CommandType.Digital, "HORN_PRIMARY"),
			new CommandInfo(Command.HornSecondary, CommandType.Digital, "HORN_SECONDARY"),
			new CommandInfo(Command.HornMusic, CommandType.Digital, "HORN_MUSIC"),
			new CommandInfo(Command.DeviceConstSpeed, CommandType.Digital, "DEVICE_CONSTSPEED"),
			new CommandInfo(Command.SecurityS, CommandType.Digital, "SECURITY_S"),
			new CommandInfo(Command.SecurityA1, CommandType.Digital, "SECURITY_A1"),
			new CommandInfo(Command.SecurityA2, CommandType.Digital, "SECURITY_A2"),
			new CommandInfo(Command.SecurityB1, CommandType.Digital, "SECURITY_B1"),
			new CommandInfo(Command.SecurityB2, CommandType.Digital, "SECURITY_B2"),
			new CommandInfo(Command.SecurityC1, CommandType.Digital, "SECURITY_C1"),
			new CommandInfo(Command.SecurityC2, CommandType.Digital, "SECURITY_C2"),
			new CommandInfo(Command.SecurityD, CommandType.Digital, "SECURITY_D"),
			new CommandInfo(Command.SecurityE, CommandType.Digital, "SECURITY_E"),
			new CommandInfo(Command.SecurityF, CommandType.Digital, "SECURITY_F"),
			new CommandInfo(Command.SecurityG, CommandType.Digital, "SECURITY_G"),
			new CommandInfo(Command.SecurityH, CommandType.Digital, "SECURITY_H"),
			new CommandInfo(Command.SecurityI, CommandType.Digital, "SECURITY_I"),
			new CommandInfo(Command.SecurityJ, CommandType.Digital, "SECURITY_J"),
			new CommandInfo(Command.SecurityK, CommandType.Digital, "SECURITY_K"),
			new CommandInfo(Command.SecurityL, CommandType.Digital, "SECURITY_L"),
            new CommandInfo(Command.SecurityM, CommandType.Digital, "SECURITY_M"),
            new CommandInfo(Command.SecurityN, CommandType.Digital, "SECURITY_N"),
            new CommandInfo(Command.SecurityO, CommandType.Digital, "SECURITY_O"),
            new CommandInfo(Command.SecurityP, CommandType.Digital, "SECURITY_P"),
            new CommandInfo(Command.WiperSpeedUp, CommandType.Digital, "WIPER_SPEED_UP"),
            new CommandInfo(Command.WiperSpeedDown, CommandType.Digital, "WIPER_SPEED_DOWN"),
            /*
            //Common Keys
            WiperSpeedUp,WiperSpeedDown,FillFuel,
            //Steam locomotive
            LiveSteamInjector,ExhaustSteamInjector,IncreaseCutoff,DecreaseCutoff,Blowers,
            //Diesel Locomotive
            EngineStart,EngineStop,GearUp,GearDown,
            //Electric Locomotive
            RaisePantograph,LowerPantograph,MainBreaker
             */
			new CommandInfo(Command.CameraInterior, CommandType.Digital, "CAMERA_INTERIOR"),
			new CommandInfo(Command.CameraExterior, CommandType.Digital, "CAMERA_EXTERIOR"),
			new CommandInfo(Command.CameraTrack, CommandType.Digital, "CAMERA_TRACK"),
			new CommandInfo(Command.CameraFlyBy, CommandType.Digital, "CAMERA_FLYBY"),
			new CommandInfo(Command.CameraMoveForward, CommandType.AnalogHalf, "CAMERA_MOVE_FORWARD"),
			new CommandInfo(Command.CameraMoveBackward, CommandType.AnalogHalf, "CAMERA_MOVE_BACKWARD"),
			new CommandInfo(Command.CameraMoveLeft, CommandType.AnalogHalf, "CAMERA_MOVE_LEFT"),
			new CommandInfo(Command.CameraMoveRight, CommandType.AnalogHalf, "CAMERA_MOVE_RIGHT"),
			new CommandInfo(Command.CameraMoveUp, CommandType.AnalogHalf, "CAMERA_MOVE_UP"),
			new CommandInfo(Command.CameraMoveDown, CommandType.AnalogHalf, "CAMERA_MOVE_DOWN"),
			new CommandInfo(Command.CameraRotateLeft, CommandType.AnalogHalf, "CAMERA_ROTATE_LEFT"),
			new CommandInfo(Command.CameraRotateRight, CommandType.AnalogHalf, "CAMERA_ROTATE_RIGHT"),
			new CommandInfo(Command.CameraRotateUp, CommandType.AnalogHalf, "CAMERA_ROTATE_UP"),
			new CommandInfo(Command.CameraRotateDown, CommandType.AnalogHalf, "CAMERA_ROTATE_DOWN"),
			new CommandInfo(Command.CameraRotateCCW, CommandType.AnalogHalf, "CAMERA_ROTATE_CCW"),
			new CommandInfo(Command.CameraRotateCW, CommandType.AnalogHalf, "CAMERA_ROTATE_CW"),
			new CommandInfo(Command.CameraZoomIn, CommandType.AnalogHalf, "CAMERA_ZOOM_IN"),
			new CommandInfo(Command.CameraZoomOut, CommandType.AnalogHalf, "CAMERA_ZOOM_OUT"),
			new CommandInfo(Command.CameraPreviousPOI, CommandType.Digital, "CAMERA_POI_PREVIOUS"),
			new CommandInfo(Command.CameraNextPOI, CommandType.Digital, "CAMERA_POI_NEXT"),
			new CommandInfo(Command.CameraReset, CommandType.Digital, "CAMERA_RESET"),
			new CommandInfo(Command.CameraRestriction, CommandType.Digital, "CAMERA_RESTRICTION"),
			new CommandInfo(Command.TimetableToggle, CommandType.Digital, "TIMETABLE_TOGGLE"),
			new CommandInfo(Command.TimetableUp, CommandType.AnalogHalf, "TIMETABLE_UP"),
			new CommandInfo(Command.TimetableDown, CommandType.AnalogHalf, "TIMETABLE_DOWN"),
			new CommandInfo(Command.MenuActivate, CommandType.Digital, "MENU_ACTIVATE"),
			new CommandInfo(Command.MenuUp, CommandType.Digital, "MENU_UP"),
			new CommandInfo(Command.MenuDown, CommandType.Digital, "MENU_DOWN"),
			new CommandInfo(Command.MenuEnter, CommandType.Digital, "MENU_ENTER"),
			new CommandInfo(Command.MenuBack, CommandType.Digital, "MENU_BACK"),
			new CommandInfo(Command.MiscClock, CommandType.Digital, "MISC_CLOCK"),
			new CommandInfo(Command.MiscSpeed, CommandType.Digital, "MISC_SPEED"),
			new CommandInfo(Command.MiscFps, CommandType.Digital, "MISC_FPS"),
			new CommandInfo(Command.MiscAI, CommandType.Digital, "MISC_AI"),
			new CommandInfo(Command.MiscFullscreen, CommandType.Digital, "MISC_FULLSCREEN"),
			new CommandInfo(Command.MiscMute, CommandType.Digital, "MISC_MUTE"),
			new CommandInfo(Command.MiscPause, CommandType.Digital, "MISC_PAUSE"),
			new CommandInfo(Command.MiscTimeFactor, CommandType.Digital, "MISC_TIMEFACTOR"),
			new CommandInfo(Command.MiscQuit, CommandType.Digital, "MISC_QUIT"),
			new CommandInfo(Command.MiscInterfaceMode, CommandType.Digital, "MISC_INTERFACE"),
			new CommandInfo(Command.MiscBackfaceCulling, CommandType.Digital, "MISC_BACKFACE"),
			new CommandInfo(Command.MiscCPUMode, CommandType.Digital, "MISC_CPUMODE"),
			new CommandInfo(Command.DebugWireframe, CommandType.Digital, "DEBUG_WIREFRAME"),
			new CommandInfo(Command.DebugNormals, CommandType.Digital, "DEBUG_NORMALS"),
			new CommandInfo(Command.DebugBrakeSystems, CommandType.Digital, "DEBUG_BRAKE"),
            new CommandInfo(Command.RouteInformation, CommandType.Digital, "ROUTE_INFORMATION"),
		};
		internal static Control[] CurrentControls = new Control[] { };

		// try get command info
		internal static bool TryGetCommandInfo(Command Value, out CommandInfo Info) {
			for (int i = 0; i < CommandInfos.Length; i++) {
				if (CommandInfos[i].Command == Value) {
					Info = CommandInfos[i];
					return true;
				}
			}
			Info.Command = Value;
			Info.Type = CommandType.Digital;
			Info.Name = "N/A";
			Info.Description = "N/A";
			return false;
		}

		/// <summary>Saves the current control configuration</summary>
		/// <param name="FileOrNull">An absolute file path if we are exporting the controls, or a null reference to save to the default configuration location</param>
		internal static void SaveControls(string FileOrNull) {
			CultureInfo Culture = CultureInfo.InvariantCulture;
			System.Text.StringBuilder Builder = new System.Text.StringBuilder();
			Builder.AppendLine("; Current control configuration");
			Builder.AppendLine("; =============================");
			Builder.AppendLine("; This file was automatically generated. Please modify only if you know what you're doing.");
            Builder.AppendLine("; This file is INCOMPATIBLE with versions older than 1.4.4.");
			Builder.AppendLine();
			for (int i = 0; i < CurrentControls.Length; i++) {
				CommandInfo Info;
				TryGetCommandInfo(CurrentControls[i].Command, out Info);
				Builder.Append(Info.Name + ", ");
				switch (CurrentControls[i].Method) {
					case ControlMethod.Keyboard:
						Builder.Append("keyboard, " + CurrentControls[i].Key + ", " + ((int)CurrentControls[i].Modifier).ToString(Culture));
                        break;
					case ControlMethod.Joystick:
						Builder.Append("joystick, " + CurrentControls[i].Device.ToString(Culture) + ", ");
						switch (CurrentControls[i].Component) {
							case JoystickComponent.Axis:
								Builder.Append("axis, " + CurrentControls[i].Element.ToString(Culture) + ", " + CurrentControls[i].Direction.ToString(Culture));
								break;
							case JoystickComponent.Ball:
								Builder.Append("ball, " + CurrentControls[i].Element.ToString(Culture) + ", " + CurrentControls[i].Direction.ToString(Culture));
								break;
							case JoystickComponent.Hat:
								Builder.Append("hat, " + CurrentControls[i].Element.ToString(Culture) + ", " + CurrentControls[i].Direction.ToString(Culture));
								break;
							case JoystickComponent.Button:
								Builder.Append("button, " + CurrentControls[i].Element.ToString(Culture));
								break;
							default:
								Builder.Append("invalid");
								break;
						}
						break;
				}
				Builder.Append("\n");
			}
			string File;
			if (FileOrNull == null) {
				File = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "controls.cfg");
			} else {
				File = FileOrNull;
			}
			System.IO.File.WriteAllText(File, Builder.ToString(), new System.Text.UTF8Encoding(true));
		}

		/// <summary>Loads the current controls from the controls.cfg file</summary>
		/// <param name="FileOrNull">An absolute path reference to a saved controls.cfg file, or a null reference to check the default locations</param>
		/// <param name="Controls">The current controls array</param>
        internal static void LoadControls(string FileOrNull, out Control[] Controls)
        {
			string File;
			if (FileOrNull == null) {
				File = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "controls.cfg");
				if (!System.IO.File.Exists(File)) {
                    //Load the default key assignments if the user settings don't exist
					File = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Controls"), "Default keyboard assignment.controls");
				}
			} else {
				File = FileOrNull;
			}
			Controls = new Control[256];
			int Length = 0;
			CultureInfo Culture = CultureInfo.InvariantCulture;
			if (System.IO.File.Exists(File)) {
				string[] Lines = System.IO.File.ReadAllLines(File, new System.Text.UTF8Encoding());
				for (int i = 0; i < Lines.Length; i++) {
					Lines[i] = Lines[i].Trim();
					if (Lines[i].Length != 0 && !Lines[i].StartsWith(";", StringComparison.OrdinalIgnoreCase)) {
						string[] Terms = Lines[i].Split(new char[] { ',' });
						for (int j = 0; j < Terms.Length; j++) {
							Terms[j] = Terms[j].Trim();
						}
						if (Terms.Length >= 2) {
							if (Length >= Controls.Length) {
								Array.Resize<Control>(ref Controls, Controls.Length << 1);
							}
							int j;
							for (j = 0; j < CommandInfos.Length; j++) {
								if (string.Compare(CommandInfos[j].Name, Terms[0], StringComparison.OrdinalIgnoreCase) == 0) break;
							}
							if (j == CommandInfos.Length) {
								Controls[Length].Command = Command.None;
								Controls[Length].InheritedType = CommandType.Digital;
								Controls[Length].Method = ControlMethod.Invalid;
								Controls[Length].Device = -1;
								Controls[Length].Component = JoystickComponent.Invalid;
								Controls[Length].Element = -1;
								Controls[Length].Direction = 0;
								Controls[Length].Modifier = KeyboardModifier.None;
							} else {
								Controls[Length].Command = CommandInfos[j].Command;
								Controls[Length].InheritedType = CommandInfos[j].Type;
								string Method = Terms[1].ToLowerInvariant();
								bool Valid = false;
							    if (Method == "keyboard" & Terms.Length == 4)
							    {
							        Key CurrentKey;
							        int SDLTest;
							        if (int.TryParse(Terms[2], out SDLTest))
							        {
							            //We've discovered a SDL keybinding is present, so reset the loading process with the default keyconfig & show an appropriate error message
							            Thread Message = new Thread(() => MessageBox.Show("An older key-configuration file was found."
							                                                        + Environment.NewLine +
							                                                        "The current key-configuration has been reset to default.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand));
							            Message.Start();
                                        LoadControls(OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Controls"), "Default keyboard assignment.controls"), out CurrentControls);
							            return;
							        }
                                    if (Enum.TryParse(Terms[2], true, out CurrentKey))
                                    {
                                        int Modifiers;
                                        if (int.TryParse(Terms[3], NumberStyles.Integer, Culture, out Modifiers))
							            {
							                Controls[Length].Method = ControlMethod.Keyboard;
							                Controls[Length].Device = -1;
							                Controls[Length].Component = JoystickComponent.Invalid;
							                Controls[Length].Key = CurrentKey;
							                Controls[Length].Direction = 0;
							                Controls[Length].Modifier = (KeyboardModifier) Modifiers;
							                Valid = true;
							            }
                                    }
							    }
                                

								 else if (Method == "joystick" & Terms.Length >= 4) {
									int Device;
									if (int.TryParse(Terms[2], NumberStyles.Integer, Culture, out Device)) {
										string Component = Terms[3].ToLowerInvariant();
									    if (Component == "axis" & Terms.Length == 6)
									    {
									        int CurrentAxis;
									        if (Int32.TryParse(Terms[4], out CurrentAxis))
									        {
									            int Direction;
									            if (int.TryParse(Terms[5], NumberStyles.Integer, Culture, out Direction))
									            {

									                Controls[Length].Method = ControlMethod.Joystick;
									                Controls[Length].Device = Device;
									                Controls[Length].Component = JoystickComponent.Axis;
									                Controls[Length].Element = CurrentAxis;
									                Controls[Length].Direction = Direction;
									                Controls[Length].Modifier = KeyboardModifier.None;
									                Valid = true;
									            }
									        }
									    }
									    else if (Component == "hat" & Terms.Length == 6)
									    {
									        int CurrentHat;
									        if (Int32.TryParse(Terms[4], out CurrentHat))
									        {
									            int HatDirection;
									            if (Int32.TryParse(Terms[5], out HatDirection))
									            {
									                Controls[Length].Method = ControlMethod.Joystick;
									                Controls[Length].Device = Device;
									                Controls[Length].Component = JoystickComponent.Hat;
									                Controls[Length].Element = CurrentHat;
									                Controls[Length].Direction = HatDirection;
									                Controls[Length].Modifier = KeyboardModifier.None;
									                Valid = true;
									            }

									        }
									    }
									    else if (Component == "button" & Terms.Length == 5)
									    {
									        int CurrentButton;
									        if (Int32.TryParse(Terms[4], out CurrentButton))
									        {
									            Controls[Length].Method = ControlMethod.Joystick;
									            Controls[Length].Device = Device;
									            Controls[Length].Component = JoystickComponent.Button;
									            Controls[Length].Element = CurrentButton;
									            Controls[Length].Direction = 0;
									            Controls[Length].Modifier = KeyboardModifier.None;
									            Valid = true;
									        }
									    }

									}
                                          
									
                                          
								}

								if (!Valid) {
									Controls[Length].Method = ControlMethod.Invalid;
									Controls[Length].Device = -1;
									Controls[Length].Component = JoystickComponent.Invalid;
									Controls[Length].Element = -1;
									Controls[Length].Direction = 0;
									Controls[Length].Modifier = KeyboardModifier.None;
								}
							}
							Length++;
						}
					}
				}
			}
			Array.Resize<Control>(ref Controls, Length);
		}

		// add controls
		internal static void AddControls(ref Control[] Base, Control[] Add) {
			for (int i = 0; i < Add.Length; i++) {
				int j;
				for (j = 0; j < Base.Length; j++) {
					if (Add[i].Command == Base[j].Command) break;
				}
				if (j == Base.Length) {
					Array.Resize<Control>(ref Base, Base.Length + 1);
					Base[Base.Length - 1] = Add[i];
				}
			}
		}

		// ================================

		// hud elements
		internal struct HudVector {
			internal int X;
			internal int Y;
		}
		internal struct HudVectorF {
			internal float X;
			internal float Y;
		}
		internal struct HudImage {
			internal Textures.Texture BackgroundTexture;
			internal Textures.Texture OverlayTexture;
		}

	    [Flags]
	    internal enum HudTransition {
			None = 0,
			Move = 1,
			Fade = 2,
			MoveAndFade = 3
		}
		internal class HudElement {
			internal string Subject;
			internal HudVectorF Position;
			internal HudVector Alignment;
			internal HudImage TopLeft;
			internal HudImage TopMiddle;
			internal HudImage TopRight;
			internal HudImage CenterLeft;
			internal HudImage CenterMiddle;
			internal HudImage CenterRight;
			internal HudImage BottomLeft;
			internal HudImage BottomMiddle;
			internal HudImage BottomRight;
			internal Color32 BackgroundColor;
			internal Color32 OverlayColor;
			internal Color32 TextColor;
			internal HudVectorF TextPosition;
			internal HudVector TextAlignment;
			internal Fonts.OpenGlFont Font;
			internal bool TextShadow;
			internal string Text;
			internal float Value1;
			internal float Value2;
			internal HudTransition Transition;
			internal HudVectorF TransitionVector;
			internal double TransitionState;
			internal HudElement() {
				this.Subject = null;
				this.Position.X = 0.0f;
				this.Position.Y = 0.0f;
				this.Alignment.X = -1;
				this.Alignment.Y = -1;
				this.BackgroundColor = new Color32(255, 255, 255, 255);
				this.OverlayColor = new Color32(255, 255, 255, 255);
				this.TextColor = new Color32(255, 255, 255, 255);
				this.TextPosition.X = 0.0f;
				this.TextPosition.Y = 0.0f;
				this.TextAlignment.X = -1;
				this.TextAlignment.Y = 0;
				this.Font = Fonts.VerySmallFont;
				this.TextShadow = true;
				this.Text = null;
				this.Value1 = 0.0f;
				this.Value2 = 0.0f;
				this.Transition = HudTransition.None;
				this.TransitionState = 1.0;
			}
		}
		internal static HudElement[] CurrentHudElements = new HudElement[] { };

		// load hud
		internal static void LoadHUD() {
			CultureInfo Culture = CultureInfo.InvariantCulture;
			string Folder = Program.FileSystem.GetDataFolder("In-game", CurrentOptions.UserInterfaceFolder);
			string File = OpenBveApi.Path.CombineFile(Folder, "interface.cfg");
			CurrentHudElements = new HudElement[16];
			int Length = 0;
			if (System.IO.File.Exists(File)) {
				string[] Lines = System.IO.File.ReadAllLines(File, new System.Text.UTF8Encoding());
				for (int i = 0; i < Lines.Length; i++) {
					int j = Lines[i].IndexOf(';');
					if (j >= 0) {
						Lines[i] = Lines[i].Substring(0, j).Trim();
					} else {
						Lines[i] = Lines[i].Trim();
					}
					if (Lines[i].Length != 0) {
						if (!Lines[i].StartsWith(";", StringComparison.Ordinal)) {
							if (Lines[i].Equals("[element]", StringComparison.OrdinalIgnoreCase)) {
								Length++;
								if (Length > CurrentHudElements.Length) {
									Array.Resize<HudElement>(ref CurrentHudElements, CurrentHudElements.Length << 1);
								}
								CurrentHudElements[Length - 1] = new HudElement();
							} else if (Length == 0) {
								System.Windows.Forms.MessageBox.Show("Line outside of [element] structure encountered at line " + (i + 1).ToString(Culture) + " in " + File);
							} else {
								j = Lines[i].IndexOf("=", StringComparison.Ordinal);
								if (j >= 0) {
									string Command = Lines[i].Substring(0, j).TrimEnd();
									string[] Arguments = Lines[i].Substring(j + 1).TrimStart().Split(new char[] { ',' }, StringSplitOptions.None);
									for (j = 0; j < Arguments.Length; j++) {
										Arguments[j] = Arguments[j].Trim();
									}
									switch (Command.ToLowerInvariant()) {
										case "subject":
											if (Arguments.Length == 1) {
												CurrentHudElements[Length - 1].Subject = Arguments[0];
											} else {
												System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											} break;
										case "position":
											if (Arguments.Length == 2) {
												float x, y;
												if (!float.TryParse(Arguments[0], NumberStyles.Float, Culture, out x)) {
													System.Windows.Forms.MessageBox.Show("X is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												} else if (!float.TryParse(Arguments[1], NumberStyles.Float, Culture, out y)) {
													System.Windows.Forms.MessageBox.Show("Y is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												} else {
													CurrentHudElements[Length - 1].Position.X = x;
													CurrentHudElements[Length - 1].Position.Y = y;
												}
											} else {
												System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											} break;
										case "alignment":
											if (Arguments.Length == 2) {
												int x, y;
												if (!int.TryParse(Arguments[0], NumberStyles.Integer, Culture, out x)) {
													System.Windows.Forms.MessageBox.Show("X is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												} else if (!int.TryParse(Arguments[1], NumberStyles.Integer, Culture, out y)) {
													System.Windows.Forms.MessageBox.Show("Y is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												} else {
													CurrentHudElements[Length - 1].Alignment.X = Math.Sign(x);
													CurrentHudElements[Length - 1].Alignment.Y = Math.Sign(y);
												}
											} else {
												System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											} break;
										case "topleft":
											if (Arguments.Length == 2) {
												if (Arguments[0].Length != 0 & !Arguments[0].Equals("null", StringComparison.OrdinalIgnoreCase)) {
													Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[0]), out CurrentHudElements[Length - 1].TopLeft.BackgroundTexture);
												}
												if (Arguments[1].Length != 0 & !Arguments[1].Equals("null", StringComparison.OrdinalIgnoreCase)) {
													Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[1]), out CurrentHudElements[Length - 1].TopLeft.OverlayTexture);
												}
											} else {
												System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											} break;
										case "topmiddle":
											if (Arguments.Length == 2) {
												if (Arguments[0].Length != 0 & !Arguments[0].Equals("null", StringComparison.OrdinalIgnoreCase)) {
													Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[0]), out CurrentHudElements[Length - 1].TopMiddle.BackgroundTexture);
												}
												if (Arguments[1].Length != 0 & !Arguments[1].Equals("null", StringComparison.OrdinalIgnoreCase)) {
													Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[1]), out CurrentHudElements[Length - 1].TopMiddle.OverlayTexture);
												}
											} else {
												System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											} break;
										case "topright":
											if (Arguments.Length == 2) {
												if (Arguments[0].Length != 0 & !Arguments[0].Equals("null", StringComparison.OrdinalIgnoreCase)) {
													Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[0]), out CurrentHudElements[Length - 1].TopRight.BackgroundTexture);
												}
												if (Arguments[1].Length != 0 & !Arguments[1].Equals("null", StringComparison.OrdinalIgnoreCase)) {
													Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[1]), out CurrentHudElements[Length - 1].TopRight.OverlayTexture);
												}
											} else {
												System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											} break;
										case "centerleft":
											if (Arguments.Length == 2) {
												if (Arguments[0].Length != 0 & !Arguments[0].Equals("null", StringComparison.OrdinalIgnoreCase)) {
													Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[0]), out CurrentHudElements[Length - 1].CenterLeft.BackgroundTexture);
												}
												if (Arguments[1].Length != 0 & !Arguments[1].Equals("null", StringComparison.OrdinalIgnoreCase)) {
													Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[1]), out CurrentHudElements[Length - 1].CenterLeft.OverlayTexture);
												}
											} else {
												System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											} break;
										case "centermiddle":
											if (Arguments.Length == 2) {
												if (Arguments[0].Length != 0 & !Arguments[0].Equals("null", StringComparison.OrdinalIgnoreCase)) {
													Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[0]), out CurrentHudElements[Length - 1].CenterMiddle.BackgroundTexture);
												}
												if (Arguments[1].Length != 0 & !Arguments[1].Equals("null", StringComparison.OrdinalIgnoreCase)) {
													Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[1]), out CurrentHudElements[Length - 1].CenterMiddle.OverlayTexture);
												}
											} else {
												System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											} break;
										case "centerright":
											if (Arguments.Length == 2) {
												if (Arguments[0].Length != 0 & !Arguments[0].Equals("null", StringComparison.OrdinalIgnoreCase)) {
													Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[0]), out CurrentHudElements[Length - 1].CenterRight.BackgroundTexture);
												}
												if (Arguments[1].Length != 0 & !Arguments[1].Equals("null", StringComparison.OrdinalIgnoreCase)) {
													Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[1]), out CurrentHudElements[Length - 1].CenterRight.OverlayTexture);
												}
											} else {
												System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											} break;
										case "bottomleft":
											if (Arguments.Length == 2) {
												if (Arguments[0].Length != 0 & !Arguments[0].Equals("null", StringComparison.OrdinalIgnoreCase)) {
													Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[0]), out CurrentHudElements[Length - 1].BottomLeft.BackgroundTexture);
												}
												if (Arguments[1].Length != 0 & !Arguments[1].Equals("null", StringComparison.OrdinalIgnoreCase)) {
													Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[1]), out CurrentHudElements[Length - 1].BottomLeft.OverlayTexture);
												}
											} else {
												System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											} break;
										case "bottommiddle":
											if (Arguments.Length == 2) {
												if (Arguments[0].Length != 0 & !Arguments[0].Equals("null", StringComparison.OrdinalIgnoreCase)) {
													Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[0]), out CurrentHudElements[Length - 1].BottomMiddle.BackgroundTexture);
												}
												if (Arguments[1].Length != 0 & !Arguments[1].Equals("null", StringComparison.OrdinalIgnoreCase)) {
													Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[1]), out CurrentHudElements[Length - 1].BottomMiddle.OverlayTexture);
												}
											} else {
												System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											} break;
										case "bottomright":
											if (Arguments.Length == 2) {
												if (Arguments[0].Length != 0 & !Arguments[0].Equals("null", StringComparison.OrdinalIgnoreCase)) {
													Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[0]), out CurrentHudElements[Length - 1].BottomRight.BackgroundTexture);
												}
												if (Arguments[1].Length != 0 & !Arguments[1].Equals("null", StringComparison.OrdinalIgnoreCase)) {
													Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, Arguments[1]), out CurrentHudElements[Length - 1].BottomRight.OverlayTexture);
												}
											} else {
												System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											} break;
										case "backcolor":
											if (Arguments.Length == 4) {
												int r, g, b, a;
												if (!int.TryParse(Arguments[0], NumberStyles.Integer, Culture, out r)) {
													System.Windows.Forms.MessageBox.Show("R is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												} else if (!int.TryParse(Arguments[1], NumberStyles.Integer, Culture, out g)) {
													System.Windows.Forms.MessageBox.Show("G is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												} else if (!int.TryParse(Arguments[2], NumberStyles.Integer, Culture, out b)) {
													System.Windows.Forms.MessageBox.Show("B is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												} else if (!int.TryParse(Arguments[3], NumberStyles.Integer, Culture, out a)) {
													System.Windows.Forms.MessageBox.Show("A is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												} else {
													r = r < 0 ? 0 : r > 255 ? 255 : r;
													g = g < 0 ? 0 : g > 255 ? 255 : g;
													b = b < 0 ? 0 : b > 255 ? 255 : b;
													a = a < 0 ? 0 : a > 255 ? 255 : a;
													CurrentHudElements[Length - 1].BackgroundColor = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
												} break;
											}
									        System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
									        break;
										case "overlaycolor":
											if (Arguments.Length == 4) {
												int r, g, b, a;
												if (!int.TryParse(Arguments[0], NumberStyles.Integer, Culture, out r)) {
													System.Windows.Forms.MessageBox.Show("R is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												} else if (!int.TryParse(Arguments[1], NumberStyles.Integer, Culture, out g)) {
													System.Windows.Forms.MessageBox.Show("G is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												} else if (!int.TryParse(Arguments[2], NumberStyles.Integer, Culture, out b)) {
													System.Windows.Forms.MessageBox.Show("B is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												} else if (!int.TryParse(Arguments[3], NumberStyles.Integer, Culture, out a)) {
													System.Windows.Forms.MessageBox.Show("A is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												} else {
													r = r < 0 ? 0 : r > 255 ? 255 : r;
													g = g < 0 ? 0 : g > 255 ? 255 : g;
													b = b < 0 ? 0 : b > 255 ? 255 : b;
													a = a < 0 ? 0 : a > 255 ? 255 : a;
													CurrentHudElements[Length - 1].OverlayColor = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
												} break;
											}
									        System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
									        break;
										case "textcolor":
											if (Arguments.Length == 4) {
												int r, g, b, a;
												if (!int.TryParse(Arguments[0], NumberStyles.Integer, Culture, out r)) {
													System.Windows.Forms.MessageBox.Show("R is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												} else if (!int.TryParse(Arguments[1], NumberStyles.Integer, Culture, out g)) {
													System.Windows.Forms.MessageBox.Show("G is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												} else if (!int.TryParse(Arguments[2], NumberStyles.Integer, Culture, out b)) {
													System.Windows.Forms.MessageBox.Show("B is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												} else if (!int.TryParse(Arguments[3], NumberStyles.Integer, Culture, out a)) {
													System.Windows.Forms.MessageBox.Show("A is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												} else {
													r = r < 0 ? 0 : r > 255 ? 255 : r;
													g = g < 0 ? 0 : g > 255 ? 255 : g;
													b = b < 0 ? 0 : b > 255 ? 255 : b;
													a = a < 0 ? 0 : a > 255 ? 255 : a;
													CurrentHudElements[Length - 1].TextColor = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
												} break;
											}
									        System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
									        break;
										case "textposition":
											if (Arguments.Length == 2) {
												float x, y;
												if (!float.TryParse(Arguments[0], NumberStyles.Float, Culture, out x)) {
													System.Windows.Forms.MessageBox.Show("X is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												} else if (!float.TryParse(Arguments[1], NumberStyles.Float, Culture, out y)) {
													System.Windows.Forms.MessageBox.Show("Y is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												} else {
													CurrentHudElements[Length - 1].TextPosition.X = x;
													CurrentHudElements[Length - 1].TextPosition.Y = y;
												}
											} else {
												System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											} break;
										case "textalignment":
											if (Arguments.Length == 2) {
												int x, y;
												if (!int.TryParse(Arguments[0], NumberStyles.Integer, Culture, out x)) {
													System.Windows.Forms.MessageBox.Show("X is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												} else if (!int.TryParse(Arguments[1], NumberStyles.Integer, Culture, out y)) {
													System.Windows.Forms.MessageBox.Show("Y is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												} else {
													CurrentHudElements[Length - 1].TextAlignment.X = Math.Sign(x);
													CurrentHudElements[Length - 1].TextAlignment.Y = Math.Sign(y);
												}
											} else {
												System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											} break;
										case "textsize":
											if (Arguments.Length == 1) {
												int s;
												if (!int.TryParse(Arguments[0], NumberStyles.Integer, Culture, out s)) {
													System.Windows.Forms.MessageBox.Show("SIZE is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												} else {
													switch (s) {
															case 0: CurrentHudElements[Length - 1].Font = Fonts.VerySmallFont; break;
															case 1: CurrentHudElements[Length - 1].Font = Fonts.SmallFont; break;
															case 2: CurrentHudElements[Length - 1].Font = Fonts.NormalFont; break;
															case 3: CurrentHudElements[Length - 1].Font = Fonts.LargeFont; break;
															case 4: CurrentHudElements[Length - 1].Font = Fonts.VeryLargeFont; break;
															default: CurrentHudElements[Length - 1].Font = Fonts.NormalFont; break;
													}
												}
											} else {
												System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											} break;
										case "textshadow":
											if (Arguments.Length == 1) {
												int s;
												if (!int.TryParse(Arguments[0], NumberStyles.Integer, Culture, out s)) {
													System.Windows.Forms.MessageBox.Show("SHADOW is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												} else {
													CurrentHudElements[Length - 1].TextShadow = s != 0;
												}
											} else {
												System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											} break;
										case "text":
											if (Arguments.Length == 1) {
												CurrentHudElements[Length - 1].Text = Arguments[0];
											} else {
												System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											} break;
										case "value":
											if (Arguments.Length == 1) {
												int n;
												if (!int.TryParse(Arguments[0], NumberStyles.Integer, Culture, out n)) {
													System.Windows.Forms.MessageBox.Show("VALUE1 is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												} else {
													CurrentHudElements[Length - 1].Value1 = n;
												}
											} else if (Arguments.Length == 2) {
												float a, b;
												if (!float.TryParse(Arguments[0], NumberStyles.Float, Culture, out a)) {
													System.Windows.Forms.MessageBox.Show("VALUE1 is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												} else if (!float.TryParse(Arguments[1], NumberStyles.Float, Culture, out b)) {
													System.Windows.Forms.MessageBox.Show("VALUE2 is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												} else {
													CurrentHudElements[Length - 1].Value1 = a;
													CurrentHudElements[Length - 1].Value2 = b;
												}
											} else {
												System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											} break;
										case "transition":
											if (Arguments.Length == 1) {
												int n;
												if (!int.TryParse(Arguments[0], NumberStyles.Integer, Culture, out n)) {
													System.Windows.Forms.MessageBox.Show("TRANSITION is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												} else {
													CurrentHudElements[Length - 1].Transition = (HudTransition)n;
												}
											} else {
												System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											} break;
										case "transitionvector":
											if (Arguments.Length == 2) {
												float x, y;
												if (!float.TryParse(Arguments[0], NumberStyles.Float, Culture, out x)) {
													System.Windows.Forms.MessageBox.Show("X is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												} else if (!float.TryParse(Arguments[1], NumberStyles.Float, Culture, out y)) {
													System.Windows.Forms.MessageBox.Show("Y is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
												} else {
													CurrentHudElements[Length - 1].TransitionVector.X = x;
													CurrentHudElements[Length - 1].TransitionVector.Y = y;
												}
											} else {
												System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + File);
											} break;
										default:
											System.Windows.Forms.MessageBox.Show("Invalid command encountered at line " + (i + 1).ToString(Culture) + " in " + File);
											break;
									}
								} else {
									System.Windows.Forms.MessageBox.Show("Invalid statement encountered at line " + (i + 1).ToString(Culture) + " in " + File);
								}
							}
						}
					}
				}
			}
			Array.Resize<HudElement>(ref CurrentHudElements, Length);
		}

		// ================================

		// encodings
		internal enum Encoding {
			Unknown = 0,
			Utf8 = 1,
			Utf16Le = 2,
			Utf16Be = 3,
			Utf32Le = 4,
			Utf32Be = 5,
		}
		internal static Encoding GetEncodingFromFile(string File) {
			try {
				byte[] Data = System.IO.File.ReadAllBytes(File);
				if (Data.Length >= 3) {
					if (Data[0] == 0xEF & Data[1] == 0xBB & Data[2] == 0xBF) return Encoding.Utf8;
				}
				if (Data.Length >= 2) {
					if (Data[0] == 0xFE & Data[1] == 0xFF) return Encoding.Utf16Be;
					if (Data[0] == 0xFF & Data[1] == 0xFE) return Encoding.Utf16Le;
				}
				if (Data.Length >= 4) {
					if (Data[0] == 0x00 & Data[1] == 0x00 & Data[2] == 0xFE & Data[3] == 0xFF) return Encoding.Utf32Be;
					if (Data[0] == 0xFF & Data[1] == 0xFE & Data[2] == 0x00 & Data[3] == 0x00) return Encoding.Utf32Le;
				}
				return Encoding.Unknown;
			} catch {
				return Encoding.Unknown;
			}
		}
		internal static Encoding GetEncodingFromFile(string Folder, string File) {
			return GetEncodingFromFile(OpenBveApi.Path.CombineFile(Folder, File));
		}

		// ================================

		// try parse vb6
		internal static bool TryParseDoubleVb6(string Expression, out double Value) {
			Expression = TrimInside(Expression);
			CultureInfo Culture = CultureInfo.InvariantCulture;
			for (int n = Expression.Length; n > 0; n--) {
				double a;
				if (double.TryParse(Expression.Substring(0, n), NumberStyles.Float, Culture, out a)) {
					Value = a;
					return true;
				}
			}
			Value = 0.0;
			return false;
		}
		internal static bool TryParseFloatVb6(string Expression, out float Value) {
			Expression = TrimInside(Expression);
			CultureInfo Culture = CultureInfo.InvariantCulture;
			for (int n = Expression.Length; n > 0; n--) {
				float a;
				if (float.TryParse(Expression.Substring(0, n), NumberStyles.Float, Culture, out a)) {
					Value = a;
					return true;
				}
			}
			Value = 0.0f;
			return false;
		}
		internal static bool TryParseIntVb6(string Expression, out int Value) {
			Expression = TrimInside(Expression);
			CultureInfo Culture = CultureInfo.InvariantCulture;
			for (int n = Expression.Length; n > 0; n--) {
				double a;
				if (double.TryParse(Expression.Substring(0, n), NumberStyles.Float, Culture, out a)) {
					if (a >= -2147483648.0 & a <= 2147483647.0) {
						Value = (int)Math.Round(a);
						return true;
					} else break;
				}
			}
			Value = 0;
			return false;
		}

		// try parse time
		internal static bool TryParseTime(string Expression, out double Value) {
			Expression = TrimInside(Expression);
			if (Expression.Length != 0) {
				CultureInfo Culture = CultureInfo.InvariantCulture;
				int i = Expression.IndexOf('.');
				if (i >= 1) {
					int h; if (int.TryParse(Expression.Substring(0, i), NumberStyles.Integer, Culture, out h)) {
						int n = Expression.Length - i - 1;
						if (n == 1 | n == 2) {
							uint m; if (uint.TryParse(Expression.Substring(i + 1, n), NumberStyles.None, Culture, out m)) {
								Value = 3600.0 * (double)h + 60.0 * (double)m;
								return true;
							}
						} else if (n == 3 | n == 4) {
							uint m; if (uint.TryParse(Expression.Substring(i + 1, 2), NumberStyles.None, Culture, out m)) {
								uint s; if (uint.TryParse(Expression.Substring(i + 3, n - 2), NumberStyles.None, Culture, out s)) {
									Value = 3600.0 * (double)h + 60.0 * (double)m + (double)s;
									return true;
								}
							}
						}
					}
				} else if (i == -1) {
					int h; if (int.TryParse(Expression, NumberStyles.Integer, Culture, out h)) {
						Value = 3600.0 * (double)h;
						return true;
					}
				}
			}
			Value = 0.0;
			return false;
		}

		// try parse hex color
		internal static bool TryParseHexColor(string Expression, out Color24 Color) {
			if (Expression.StartsWith("#")) {
				string a = Expression.Substring(1).TrimStart();
				int x; if (int.TryParse(a, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out x)) {
					int r = (x >> 16) & 0xFF;
					int g = (x >> 8) & 0xFF;
					int b = x & 0xFF;
					if (r >= 0 & r <= 255 & g >= 0 & g <= 255 & b >= 0 & b <= 255) {
						Color = new Color24((byte)r, (byte)g, (byte)b);
						return true;
					} else {
						Color = new Color24(0, 0, 255);
						return false;
					}
				} else {
					Color = new Color24(0, 0, 255);
					return false;
				}
			} else {
				Color = new Color24(0, 0, 255);
				return false;
			}
		}
		internal static bool TryParseHexColor(string Expression, out Color32 Color) {
			if (Expression.StartsWith("#")) {
				string a = Expression.Substring(1).TrimStart();
				int x; if (int.TryParse(a, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out x)) {
					int r = (x >> 16) & 0xFF;
					int g = (x >> 8) & 0xFF;
					int b = x & 0xFF;
					if (r >= 0 & r <= 255 & g >= 0 & g <= 255 & b >= 0 & b <= 255) {
						Color = new Color32((byte)r, (byte)g, (byte)b, 255);
						return true;
					} else {
						Color = new Color32(0, 0, 255, 255);
						return false;
					}
				} else {
					Color = new Color32(0, 0, 255, 255);
					return false;
				}
			} else {
				Color = new Color32(0, 0, 255, 255);
				return false;
			}
		}

		// try parse with unit factors
		internal static bool TryParseDouble(string Expression, double[] UnitFactors, out double Value) {
			double a;
			if (double.TryParse(Expression, NumberStyles.Any, CultureInfo.InvariantCulture, out a)) {
				Value = a * UnitFactors[UnitFactors.Length - 1];
				return true;
			} else {
				string[] parameters = Expression.Split(':');
				if (parameters.Length <= UnitFactors.Length) {
					Value = 0.0;
					for (int i = 0; i < parameters.Length; i++) {
						if (double.TryParse(parameters[i].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out a)) {
							int j = i + UnitFactors.Length - parameters.Length;
							Value += a * UnitFactors[j];
						} else {
							return false;
						}
					}
					return true;
				} else {
					Value = 0.0;
					return false;
				}
			}
		}
		internal static bool TryParseDoubleVb6(string Expression, double[] UnitFactors, out double Value) {
			double a;
			if (double.TryParse(Expression, NumberStyles.Any, CultureInfo.InvariantCulture, out a)) {
				Value = a * UnitFactors[UnitFactors.Length - 1];
				return true;
			} else {
				string[] parameters = Expression.Split(':');
				Value = 0.0;
				if (parameters.Length <= UnitFactors.Length) {
					for (int i = 0; i < parameters.Length; i++) {
						if (TryParseDoubleVb6(parameters[i].Trim(), out a)) {
							int j = i + UnitFactors.Length - parameters.Length;
							Value += a * UnitFactors[j];
						} else {
							return false;
						}
					}
					return true;
				} else {
					return false;
				}
			}
		}

		// trim inside
		private static string TrimInside(string Expression) {
			System.Text.StringBuilder Builder = new System.Text.StringBuilder(Expression.Length);
			foreach (char c in Expression.Where(c => !char.IsWhiteSpace(c)))
			{
			    Builder.Append(c);
			} return Builder.ToString();
		}

		// is japanese
		internal static bool IsJapanese(string Name) {
			for (int i = 0; i < Name.Length; i++) {
				int a = char.ConvertToUtf32(Name, i);
				if (a < 0x10000) {
					bool q = false;
					while (true) {
						if (a >= 0x2E80 & a <= 0x2EFF) break;
						if (a >= 0x3000 & a <= 0x30FF) break;
						if (a >= 0x31C0 & a <= 0x4DBF) break;
						if (a >= 0x4E00 & a <= 0x9FFF) break;
						if (a >= 0xF900 & a <= 0xFAFF) break;
						if (a >= 0xFE30 & a <= 0xFE4F) break;
						if (a >= 0xFF00 & a <= 0xFFEF) break;
						q = true; break;
					} if (q) return false;
				} else {
					return false;
				}
			} return true;
		}

		// unescape
		internal static string Unescape(string Text) {
			System.Text.StringBuilder Builder = new System.Text.StringBuilder(Text.Length);
			int Start = 0;
			for (int i = 0; i < Text.Length; i++) {
				if (Text[i] == '\\') {
					Builder.Append(Text, Start, i - Start);
					if (i + 1 < Text.Length) {
						switch (Text[i + 1]) {
								case 'a': Builder.Append('\a'); break;
								case 'b': Builder.Append('\b'); break;
								case 't': Builder.Append('\t'); break;
								case 'n': Builder.Append('\n'); break;
								case 'v': Builder.Append('\v'); break;
								case 'f': Builder.Append('\f'); break;
								case 'r': Builder.Append('\r'); break;
								case 'e': Builder.Append('\x1B'); break;
							case 'c':
								if (i + 2 < Text.Length) {
									int CodePoint = char.ConvertToUtf32(Text, i + 2);
									if (CodePoint >= 0x40 & CodePoint <= 0x5F) {
										Builder.Append(char.ConvertFromUtf32(CodePoint - 64));
									} else if (CodePoint == 0x3F) {
										Builder.Append('\x7F');
									} else {
										//Interface.AddMessage(MessageType.Error, false, "Unrecognized control character found in " + Text.Substring(i, 3));
										return Text;
									} i++;
								} else {
									//Interface.AddMessage(MessageType.Error, false, "Insufficient characters available in " + Text + " to decode control character escape sequence");
									return Text;
								} break;
							case '"':
								Builder.Append('"');
								break;
							case '\\':
								Builder.Append('\\');
								break;
							case 'x':
								if (i + 3 < Text.Length) {
									Builder.Append(char.ConvertFromUtf32(Convert.ToInt32(Text.Substring(i + 2, 2), 16)));
									i += 2;
								} else {
									//Interface.AddMessage(MessageType.Error, false, "Insufficient characters available in " + Text + " to decode hexadecimal escape sequence.");
									return Text;
								} break;
							case 'u':
								if (i + 5 < Text.Length) {
									Builder.Append(char.ConvertFromUtf32(Convert.ToInt32(Text.Substring(i + 2, 4), 16)));
									i += 4;
								} else {
									//Interface.AddMessage(MessageType.Error, false, "Insufficient characters available in " + Text + " to decode hexadecimal escape sequence.");
									return Text;
								} break;
							default:
								//Interface.AddMessage(MessageType.Error, false, "Unrecognized escape sequence found in " + Text + ".");
								return Text;
						}
						i++;
						Start = i + 1;
					} else {
						//Interface.AddMessage(MessageType.Error, false, "Insufficient characters available in " + Text + " to decode escape sequence.");
						return Text;
					}
				}
			}
			Builder.Append(Text, Start, Text.Length - Start);
			return Builder.ToString();
		}

		// ================================

		// convert newlines to crlf
		internal static string ConvertNewlinesToCrLf(string Text) {
			System.Text.StringBuilder Builder = new System.Text.StringBuilder();
			for (int i = 0; i < Text.Length; i++) {
				int a = char.ConvertToUtf32(Text, i);
				if (a == 0xD & i < Text.Length - 1) {
					int b = char.ConvertToUtf32(Text, i + 1);
					if (b == 0xA) {
						Builder.Append("\r\n");
						i++;
					} else {
						Builder.Append("\r\n");
					}
				} else if (a == 0xA | a == 0xC | a == 0xD | a == 0x85 | a == 0x2028 | a == 0x2029) {
					Builder.Append("\r\n");
				} else if (a < 0x10000) {
					Builder.Append(Text[i]);
				} else {
					Builder.Append(Text.Substring(i, 2));
					i++;
				}
			} return Builder.ToString();
		}

		// ================================

//		// get corrected path separation
//		internal static string GetCorrectedPathSeparation(string Expression) {
//			if (Program.CurrentPlatform == Program.Platform.Windows) {
//				if (Expression.Length != 0 && Expression[0] == '\\') {
//					return Expression.Substring(1);
//				} else {
//					return Expression;
//				}
//			} else {
//				if (Expression.Length != 0 && Expression[0] == '\\') {
//					return Expression.Substring(1).Replace("\\", new string(new char[] { System.IO.Path.DirectorySeparatorChar }));
//				} else {
//					return Expression.Replace("\\", new string(new char[] { System.IO.Path.DirectorySeparatorChar }));
//				}
//			}
//		}

//		// get corected folder name
//		internal static string GetCorrectedFolderName(string Folder) {
//			if (Folder.Length == 0) {
//				return "";
//			} else if (Program.CurrentPlatform == Program.Platform.Linux) {
//				// find folder case-insensitively
//				if (System.IO.Directory.Exists(Folder)) {
//					return Folder;
//				} else {
//					string Parent = GetCorrectedFolderName(System.IO.Path.GetDirectoryName(Folder));
//					Folder = System.IO.Path.Combine(Parent, System.IO.Path.GetFileName(Folder));
//					if (Folder != null && System.IO.Directory.Exists(Parent)) {
//						if (System.IO.Directory.Exists(Folder)) {
//							return Folder;
//						} else {
//							string[] Folders = System.IO.Directory.GetDirectories(Parent);
//							for (int i = 0; i < Folders.Length; i++) {
//								if (string.Compare(Folder, Folders[i], StringComparison.OrdinalIgnoreCase) == 0) {
//									return Folders[i];
//								}
//							}
//						}
//					}
//					return Folder;
//				}
//			} else {
//				return Folder;
//			}
//		}
		
//		// get corrected file name
//		internal static string GetCorrectedFileName(string File) {
//			if (File.Length == 0) {
//				return "";
//			} else if (Program.CurrentPlatform == Program.Platform.Linux) {
//				// find file case-insensitively
//				if (System.IO.File.Exists(File)) {
//					return File;
//				} else {
//					string Folder = GetCorrectedFolderName(System.IO.Path.GetDirectoryName(File));
//					File = System.IO.Path.Combine(Folder, System.IO.Path.GetFileName(File));
//					if (System.IO.Directory.Exists(Folder)) {
//						if (System.IO.File.Exists(File)) {
//							return File;
//						} else {
//							string[] Files = System.IO.Directory.GetFiles(Folder);
//							for (int i = 0; i < Files.Length; i++) {
//								if (string.Compare(File, Files[i], StringComparison.OrdinalIgnoreCase) == 0) {
//									return Files[i];
//								}
//							}
//						}
//					}
//					return File;
//				}
//			} else {
//				return File;
//			}
//		}

//		// get combined file name
//		internal static string OpenBveApi.Path.CombineFile(string SafeFolderPart, string UnsafeFilePart) {
//			return GetCorrectedFileName(System.IO.Path.Combine(SafeFolderPart, GetCorrectedPathSeparation(UnsafeFilePart)));
//		}
		
//		// get combined folder name
//		internal static string OpenBveApi.Path.CombineDirectory(string SafeFolderPart, string UnsafeFolderPart) {
//			return GetCorrectedFolderName(System.IO.Path.Combine(SafeFolderPart, GetCorrectedPathSeparation(UnsafeFolderPart)));
//		}
		
		// contains invalid path characters
		internal static bool ContainsInvalidPathChars(string Expression) {
			char[] a = System.IO.Path.GetInvalidFileNameChars();
			char[] b = System.IO.Path.GetInvalidPathChars();
			for (int i = 0; i < Expression.Length; i++) {
				for (int j = 0; j < a.Length; j++) {
					if (Expression[i] == a[j]) {
						for (int k = 0; k < b.Length; k++) {
							if (Expression[i] == b[k]) {
								return true;
							}
						}
					}
				}
			}
			return false;
		}

	}
}