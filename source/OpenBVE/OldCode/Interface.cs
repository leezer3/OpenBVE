using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using OpenBveApi.Colors;
using OpenTK.Input;

namespace OpenBve {
	internal static partial class Interface {

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

		
		internal static CommandInfo[] CommandInfos = {
			new CommandInfo(Command.PowerIncrease, CommandType.Digital, "POWER_INCREASE"),
			new CommandInfo(Command.PowerDecrease, CommandType.Digital, "POWER_DECREASE"),
			new CommandInfo(Command.PowerHalfAxis, CommandType.AnalogHalf, "POWER_HALFAXIS"),
			new CommandInfo(Command.PowerFullAxis, CommandType.AnalogFull, "POWER_FULLAXIS"),
			new CommandInfo(Command.BrakeDecrease, CommandType.Digital, "BRAKE_DECREASE"),
			new CommandInfo(Command.BrakeIncrease, CommandType.Digital, "BRAKE_INCREASE"),
			new CommandInfo(Command.LocoBrakeDecrease, CommandType.Digital, "LOCOBRAKE_DECREASE"),
			new CommandInfo(Command.LocoBrakeIncrease, CommandType.Digital, "LOCOBRAKE_INCREASE"),
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

//We only want to mark these as obsolete for new users of the API
#pragma warning disable 618
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
#pragma warning restore 618			
			
			//Common Keys
			new CommandInfo(Command.WiperSpeedUp, CommandType.Digital, "WIPER_SPEED_UP"),
			new CommandInfo(Command.WiperSpeedDown, CommandType.Digital, "WIPER_SPEED_DOWN"),
			new CommandInfo(Command.FillFuel, CommandType.Digital, "FILL_FUEL"),
			new CommandInfo(Command.Headlights, CommandType.Digital, "HEADLIGHTS"),
			//Steam locomotive
			new CommandInfo(Command.LiveSteamInjector, CommandType.Digital, "LIVE_STEAM_INJECTOR"),
			new CommandInfo(Command.ExhaustSteamInjector, CommandType.Digital, "EXHAUST_STEAM_INJECTOR"),
			new CommandInfo(Command.IncreaseCutoff, CommandType.Digital, "INCREASE_CUTOFF"),
			new CommandInfo(Command.DecreaseCutoff, CommandType.Digital, "DECREASE_CUTOFF"),
			new CommandInfo(Command.Blowers, CommandType.Digital, "BLOWERS"),
			//Diesel Locomotive
			new CommandInfo(Command.EngineStart, CommandType.Digital, "ENGINE_START"),
			new CommandInfo(Command.EngineStop, CommandType.Digital, "ENGINE_STOP"),
			new CommandInfo(Command.GearUp, CommandType.Digital, "GEAR_UP"),
			new CommandInfo(Command.GearDown, CommandType.Digital, "GEAR_DOWN"),
			
			//Electric Locomotive
			new CommandInfo(Command.RaisePantograph, CommandType.Digital, "RAISE_PANTOGRAPH"),
			new CommandInfo(Command.LowerPantograph, CommandType.Digital, "LOWER_PANTOGRAPH"),
			new CommandInfo(Command.MainBreaker, CommandType.Digital, "MAIN_BREAKER"),
			 
			//Simulation controls
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
			new CommandInfo(Command.MiscGradient, CommandType.Digital, "MISC_GRADIENT"),
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
			new CommandInfo(Command.DebugATS, CommandType.Digital, "DEBUG_ATS"),
			new CommandInfo(Command.RouteInformation, CommandType.Digital, "ROUTE_INFORMATION"),
			new CommandInfo(Command.ShowEvents, CommandType.Digital, "SHOW_EVENTS"),
		};
		internal static Control[] CurrentControls = { };

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

		/// <summary>Saves a control configuration to disk</summary>
		/// <param name="FileOrNull">An absolute file path if we are exporting the controls, or a null reference to save to the default configuration location</param>
		/// <param name="controlsToSave">The list of controls to save</param>
		internal static void SaveControls(string FileOrNull, Control[] controlsToSave) {
			CultureInfo Culture = CultureInfo.InvariantCulture;
			System.Text.StringBuilder Builder = new System.Text.StringBuilder();
			Builder.AppendLine("; Current control configuration");
			Builder.AppendLine("; =============================");
			Builder.AppendLine("; This file was automatically generated. Please modify only if you know what you're doing.");
			Builder.AppendLine("; This file is INCOMPATIBLE with versions older than 1.4.4.");
			Builder.AppendLine();
			for (int i = 0; i < controlsToSave.Length; i++) {
				CommandInfo Info;
				TryGetCommandInfo(controlsToSave[i].Command, out Info);
				Builder.Append(Info.Name + ", ");
				switch (controlsToSave[i].Method) {
					case ControlMethod.Keyboard:
						Builder.Append("keyboard, " + controlsToSave[i].Key + ", " + ((int)controlsToSave[i].Modifier).ToString(Culture));
						break;
					case ControlMethod.Joystick:
						Builder.Append("joystick, " + controlsToSave[i].Device.ToString(Culture) + ", ");
						switch (controlsToSave[i].Component) {
							case JoystickComponent.Axis:
								Builder.Append("axis, " + controlsToSave[i].Element.ToString(Culture) + ", " + controlsToSave[i].Direction.ToString(Culture));
								break;
							case JoystickComponent.Ball:
								Builder.Append("ball, " + controlsToSave[i].Element.ToString(Culture) + ", " + controlsToSave[i].Direction.ToString(Culture));
								break;
							case JoystickComponent.Hat:
								Builder.Append("hat, " + controlsToSave[i].Element.ToString(Culture) + ", " + controlsToSave[i].Direction.ToString(Culture));
								break;
							case JoystickComponent.Button:
								Builder.Append("button, " + controlsToSave[i].Element.ToString(Culture));
								break;
							default:
								Builder.Append("invalid");
								break;
						}
						break;
					case ControlMethod.RailDriver:
						Builder.Append("raildriver, 0, ");
						switch (controlsToSave[i].Component) {
							case JoystickComponent.Axis:
								Builder.Append("axis, " + controlsToSave[i].Element.ToString(Culture) + ", " + controlsToSave[i].Direction.ToString(Culture));
								break;
							case JoystickComponent.Button:
								Builder.Append("button, " + controlsToSave[i].Element.ToString(Culture));
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
				File = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/controls.cfg");
			} else {
				File = FileOrNull;
			}
			System.IO.File.WriteAllText(File, Builder.ToString(), new System.Text.UTF8Encoding(true));
		}


		private static bool ControlsReset;

		/// <summary>Loads the current controls from the controls.cfg file</summary>
		/// <param name="FileOrNull">An absolute path reference to a saved controls.cfg file, or a null reference to check the default locations</param>
		/// <param name="Controls">The current controls array</param>
		internal static void LoadControls(string FileOrNull, out Control[] Controls)
		{
			string File;
			if (FileOrNull == null) {
				File = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/controls.cfg");
				if (!System.IO.File.Exists(File))
				{
					File = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "controls.cfg");
				}
				if (!System.IO.File.Exists(File)) {
					//Load the default key assignments if the user settings don't exist
					File = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Controls"), "Default keyboard assignment.controls");
					if (!System.IO.File.Exists(File))
					{
						MessageBox.Show(Interface.GetInterfaceString("errors_warning") + Environment.NewLine + Interface.GetInterfaceString("errors_controls_missing"),
												Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
						Controls = new Control[0];
						return;
					}
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
						string[] Terms = Lines[i].Split(',');
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
										if (ControlsReset == false)
										{
											MessageBox.Show(Interface.GetInterfaceString("errors_controls_oldversion") + Environment.NewLine + Interface.GetInterfaceString("errors_controls_reset"), Application.ProductName,
												MessageBoxButtons.OK, MessageBoxIcon.Hand);
										}
										var DefaultControls = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Controls"),"Default keyboard assignment.controls");
										if (System.IO.File.Exists(DefaultControls))
										{
											if (ControlsReset == false)
											{
												LoadControls(DefaultControls, out CurrentControls);
												ControlsReset = true;
											}
											else
											{
												MessageBox.Show(Interface.GetInterfaceString("errors_warning") + Environment.NewLine + Interface.GetInterfaceString("errors_controls_default_oldversion"),
												Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
												Controls = new Control[0];
											}
											
										}
										else
										{
											MessageBox.Show(Interface.GetInterfaceString("errors_warning") + Environment.NewLine + Interface.GetInterfaceString("errors_controls_default_missing"),
												Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
												Controls = new Control[0];
										}
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
								else if (Method == "raildriver" & Terms.Length >= 4) {
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

													Controls[Length].Method = ControlMethod.RailDriver;
													Controls[Length].Device = Device;
													Controls[Length].Component = JoystickComponent.Axis;
													Controls[Length].Element = CurrentAxis;
													Controls[Length].Direction = Direction;
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
												Controls[Length].Method = ControlMethod.RailDriver;
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


		/// <summary>Parses a string into OpenBVE's internal time representation (Seconds since midnight on the first day)</summary>
		/// <param name="Expression">The time in string format</param>
		/// <param name="Value">The number of seconds since midnight on the first day this represents, updated via 'out'</param>
		/// <returns>True if the parse succeeds, false if it does not</returns>
		internal static bool TryParseTime(string Expression, out double Value) {
			Expression = TrimInside(Expression);
			if (Expression.Length != 0) {
				CultureInfo Culture = CultureInfo.InvariantCulture;
				int i = Expression.IndexOf('.');
				if (i == -1)
				{
					i = Expression.IndexOf(':');
				}
				if (i >= 1) {
					int h; if (int.TryParse(Expression.Substring(0, i), NumberStyles.Integer, Culture, out h)) {
						int n = Expression.Length - i - 1;
						if (n == 1 | n == 2) {
							uint m; if (uint.TryParse(Expression.Substring(i + 1, n), NumberStyles.None, Culture, out m)) {
								Value = 3600.0 * (double)h + 60.0 * (double)m;
								return true;
							}
						} else if (n >= 3) {
							if (n > 4)
							{
								Interface.AddMessage(Interface.MessageType.Warning, false, "A maximum of 4 digits of precision are supported in TIME values");
								n = 4;
							}
							uint m; if (uint.TryParse(Expression.Substring(i + 1, 2), NumberStyles.None, Culture, out m)) {
								uint s;
								string ss = Expression.Substring(i + 3, n - 2);
								if (Interface.CurrentOptions.EnableBveTsHacks)
								{
									/*
									 * Handles values in the following format:
									 * HH.MM.SS
									 */
									if (ss.StartsWith("."))
									{
										ss = ss.Substring(1, ss.Length - 1);
									}
								}
								if (uint.TryParse(ss, NumberStyles.None, Culture, out s)) {
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
	
		

		// trim inside
		private static string TrimInside(string Expression) {
			System.Text.StringBuilder Builder = new System.Text.StringBuilder(Expression.Length);
			foreach (char c in Expression.Where(c => !char.IsWhiteSpace(c)))
			{
				Builder.Append(c);
			} return Builder.ToString();
		}

		/// <summary>Determines whether the specified string is encoded using Shift_JIS (Japanese)</summary>
		/// <param name="Name">The string to check</param>
		/// <returns>True if Shift_JIS encoded, false otherwise</returns>
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

		/// <summary>Unescapes control characters used in a language file</summary>
		/// <param name="Text">The raw string on which unescaping should be performed</param>
		/// <returns>The unescaped string</returns>
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

		/// <summary>Converts various line-endings to CR-LF format</summary>
		/// <param name="Text">The string for which all line-endings should be converted to CR-LF</param>
		/// <returns>The converted StringBuilder</returns>
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
	}
}
