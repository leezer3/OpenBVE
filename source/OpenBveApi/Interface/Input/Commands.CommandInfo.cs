using System.Collections.Generic;
using OpenBveApi.Hosts;

namespace OpenBveApi.Interface
{
	/// <summary>Contains functions for providing translations</summary>
	public static partial class Translations
	{
		/// <summary>Information on an in-game command</summary>
		public struct CommandInfo
		{
			/// <summary>The actual command to be performed</summary>
			public readonly Command Command;
			/// <summary>Whether this is a digital or analog control</summary>
			public readonly CommandType Type;
			/// <summary>The command name</summary>
			public readonly string Name;

			/// <summary>The command's description</summary>
			public string Description => Translations.AvailableNewLanguages[CurrentLanguageCode].GetInterfaceString(HostApplication.OpenBve, new[] { "commands", Name.ToLowerInvariant() });

			/// <summary>Whether to enable command options</summary>
			public readonly bool EnableOption;

			/// <summary>Gets the string representation of this command</summary>
			public override string ToString()
			{
				return Name + " - " + Description;
			}

			internal CommandInfo(Command Command, CommandType Type, string Name)
			{
				this.Command = Command;
				this.Type = Type;
				this.Name = Name;
				this.EnableOption = false;
			}

			internal CommandInfo(Command Command, CommandType Type, string Name, bool EnableOption)
			{
				this.Command = Command;
				this.Type = Type;
				this.Name = Name;
				this.EnableOption = EnableOption;
			}

			/// <summary>Checks whether the two specified CommandInfo instances are equal</summary>
			/// <remarks>This ignores the translated command description</remarks>
			public static bool operator ==(CommandInfo a, CommandInfo b)
			{
				if (a.Command != b.Command)
				{
					return false;
				}
				if (a.Name != b.Name)
				{
					return false;
				}
				if (a.EnableOption != b.EnableOption)
				{
					return false;
				}
				return true;
			}

			/// <summary>Checks whether the two specified CommandInfo instances are NOT equal</summary>
			/// <remarks>This ignores the translated command description</remarks>
			public static bool operator !=(CommandInfo a, CommandInfo b)
			{
				if (a.Command == b.Command)
				{
					return false;
				}
				if (a.Name == b.Name)
				{
					return false;
				}
				if (a.EnableOption == b.EnableOption)
				{
					return false;
				}
				return true;
			}

			/// <summary>Returns whether this CommandInfo instance is equal to the specified object</summary>
			/// <remarks>This ignores the translated command description</remarks>
			public override bool Equals(object obj)
			{
				if (obj is CommandInfo newCommandInfo)
				{
					return newCommandInfo.Equals(this);
				}
				return false;

			}

			/// <summary>Returns whether this CommandInfo instance is equal to the specified CommandInfo</summary>
			/// <remarks>This ignores the translated command description</remarks>
			public bool Equals(CommandInfo other)
			{
				return Command == other.Command && Type == other.Type && Name == other.Name && EnableOption == other.EnableOption;
			}

			/// <summary>Gets the HashCode for this CommandInfo instance</summary>
			/// <remarks>This ignores the translated command description</remarks>
			public override int GetHashCode()
			{
				unchecked
				{
					var hashCode = (int) Command;
					hashCode = (hashCode * 397) ^ (int) Type;
					hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ EnableOption.GetHashCode();
					return hashCode;
				}
			}
		}

		/// <summary>Gets the command info for the specified command</summary>
		/// <param name="commandInfos">The array of command infos</param>
		/// <param name="Value">The command</param>
		/// <returns>The command info for Value, or a new command info if this is not in the array</returns>
		public static CommandInfo TryGetInfo(this Dictionary<Command, CommandInfo> commandInfos, Command Value)
		{
			if (commandInfos.ContainsKey(Value))
			{
				return commandInfos[Value];
			}

			return new CommandInfo(Value, CommandType.Digital, "N/A");
		}

		/// <summary>Contains the list of all known command infos</summary>
		public static Dictionary<Command, CommandInfo> CommandInfos = new Dictionary<Command, CommandInfo>
		{
			{ Command.PowerIncrease, new CommandInfo(Command.PowerIncrease, CommandType.Digital, "POWER_INCREASE") },
			{ Command.PowerDecrease, new CommandInfo(Command.PowerDecrease, CommandType.Digital, "POWER_DECREASE") },
			{ Command.PowerHalfAxis, new CommandInfo(Command.PowerHalfAxis, CommandType.AnalogHalf, "POWER_HALFAXIS") },
			{ Command.PowerFullAxis, new CommandInfo(Command.PowerFullAxis, CommandType.AnalogFull, "POWER_FULLAXIS") },
			{ Command.BrakeDecrease, new CommandInfo(Command.BrakeDecrease, CommandType.Digital, "BRAKE_DECREASE") },
			{ Command.BrakeIncrease, new CommandInfo(Command.BrakeIncrease, CommandType.Digital, "BRAKE_INCREASE") },
			{ Command.LocoBrakeDecrease, new CommandInfo(Command.LocoBrakeDecrease, CommandType.Digital, "LOCOBRAKE_DECREASE") },
			{ Command.LocoBrakeIncrease, new CommandInfo(Command.LocoBrakeIncrease, CommandType.Digital, "LOCOBRAKE_INCREASE") },
			{ Command.BrakeHalfAxis, new CommandInfo(Command.BrakeHalfAxis, CommandType.AnalogHalf, "BRAKE_HALFAXIS") },
			{ Command.BrakeFullAxis, new CommandInfo(Command.BrakeFullAxis, CommandType.AnalogFull, "BRAKE_FULLAXIS") },
			{ Command.BrakeEmergency, new CommandInfo(Command.BrakeEmergency, CommandType.Digital, "BRAKE_EMERGENCY") },
			{ Command.SinglePower, new CommandInfo(Command.SinglePower, CommandType.Digital, "SINGLE_POWER") },
			{ Command.SingleNeutral, new CommandInfo(Command.SingleNeutral, CommandType.Digital, "SINGLE_NEUTRAL") },
			{Command.SingleBrake, new CommandInfo(Command.SingleBrake, CommandType.Digital, "SINGLE_BRAKE") },
			{ Command.SingleEmergency, new CommandInfo(Command.SingleEmergency, CommandType.Digital, "SINGLE_EMERGENCY") },
			{ Command.SingleFullAxis, new CommandInfo(Command.SingleFullAxis, CommandType.AnalogFull, "SINGLE_FULLAXIS") },
			{ Command.PowerAnyNotch, new CommandInfo(Command.PowerAnyNotch, CommandType.Digital, "POWER_ANY_NOTCH", true) }, 
			{ Command.BrakeAnyNotch,	new CommandInfo(Command.BrakeAnyNotch, CommandType.Digital, "BRAKE_ANY_NOTCH", true) },
			{ Command.ReverserAnyPosition, new CommandInfo(Command.ReverserAnyPosition, CommandType.Digital, "REVERSER_ANY_POSITION", true) },
			{ Command.HoldBrake, new CommandInfo(Command.HoldBrake,CommandType.Digital, "HOLD_BRAKE") },
			{ Command.ReverserForward, new CommandInfo(Command.ReverserForward, CommandType.Digital, "REVERSER_FORWARD") },
			{ Command.ReverserBackward, new CommandInfo(Command.ReverserBackward, CommandType.Digital, "REVERSER_BACKWARD") },
			{ Command.ReverserFullAxis, new CommandInfo(Command.ReverserFullAxis, CommandType.AnalogFull, "REVERSER_FULLAXIS") },
			{ Command.DoorsLeft, new CommandInfo(Command.DoorsLeft, CommandType.Digital, "DOORS_LEFT") },
			{ Command.DoorsRight, new CommandInfo(Command.DoorsRight, CommandType.Digital, "DOORS_RIGHT") },
			{ Command.HornPrimary, new CommandInfo(Command.HornPrimary, CommandType.Digital, "HORN_PRIMARY") },
			{ Command.HornSecondary, new CommandInfo(Command.HornSecondary, CommandType.Digital, "HORN_SECONDARY") },
			{ Command.HornMusic, new CommandInfo(Command.HornMusic, CommandType.Digital, "HORN_MUSIC") },
			{ Command.DeviceConstSpeed, new CommandInfo(Command.DeviceConstSpeed, CommandType.Digital, "DEVICE_CONSTSPEED") },
			{ Command.PlayMicSounds, new CommandInfo(Command.PlayMicSounds, CommandType.Digital, "PLAY_MIC_SOUNDS") },
			{ Command.Sanders, new CommandInfo(Command.Sanders, CommandType.Digital, "SANDERS") },

//We only want to mark these as obsolete for new users of the API
#pragma warning disable 618
			{ Command.SecurityS, new CommandInfo(Command.SecurityS, CommandType.Digital, "SECURITY_S") },
			{ Command.SecurityA1, new CommandInfo(Command.SecurityA1, CommandType.Digital, "SECURITY_A1") },
			{ Command.SecurityA2, new CommandInfo(Command.SecurityA2, CommandType.Digital, "SECURITY_A2") },
			{ Command.SecurityB1, new CommandInfo(Command.SecurityB1, CommandType.Digital, "SECURITY_B1") },
			{ Command.SecurityB2, new CommandInfo(Command.SecurityB2, CommandType.Digital, "SECURITY_B2") },
			{ Command.SecurityC1, new CommandInfo(Command.SecurityC1, CommandType.Digital, "SECURITY_C1") },
			{ Command.SecurityC2, new CommandInfo(Command.SecurityC2, CommandType.Digital, "SECURITY_C2") },
			{ Command.SecurityD, new CommandInfo(Command.SecurityD, CommandType.Digital, "SECURITY_D") },
			{ Command.SecurityE, new CommandInfo(Command.SecurityE, CommandType.Digital, "SECURITY_E") },
			{ Command.SecurityF, new CommandInfo(Command.SecurityF, CommandType.Digital, "SECURITY_F") },
			{ Command.SecurityG, new CommandInfo(Command.SecurityG, CommandType.Digital, "SECURITY_G") },
			{ Command.SecurityH, new CommandInfo(Command.SecurityH, CommandType.Digital, "SECURITY_H") },
			{ Command.SecurityI, new CommandInfo(Command.SecurityI, CommandType.Digital, "SECURITY_I") },
			{ Command.SecurityJ, new CommandInfo(Command.SecurityJ, CommandType.Digital, "SECURITY_J") },
			{ Command.SecurityK, new CommandInfo(Command.SecurityK, CommandType.Digital, "SECURITY_K") },
			{ Command.SecurityL, new CommandInfo(Command.SecurityL, CommandType.Digital, "SECURITY_L") },
			{ Command.SecurityM, new CommandInfo(Command.SecurityM, CommandType.Digital, "SECURITY_M") },
			{ Command.SecurityN, new CommandInfo(Command.SecurityN, CommandType.Digital, "SECURITY_N") },
			{ Command.SecurityO, new CommandInfo(Command.SecurityO, CommandType.Digital, "SECURITY_O") },
			{ Command.SecurityP, new CommandInfo(Command.SecurityP, CommandType.Digital, "SECURITY_P") },
#pragma warning restore 618			
			{ Command.DriverSupervisionDevice, new CommandInfo(Command.DriverSupervisionDevice, CommandType.Digital, "DRIVER_SUPERVISION") },

			//Common Keys
			{ Command.WiperSpeedUp, new CommandInfo(Command.WiperSpeedUp, CommandType.Digital, "WIPER_SPEED_UP") },
			{ Command.WiperSpeedDown, new CommandInfo(Command.WiperSpeedDown, CommandType.Digital, "WIPER_SPEED_DOWN") },
			{ Command.FillFuel, new CommandInfo(Command.FillFuel, CommandType.Digital, "FILL_FUEL") },
			{ Command.Headlights, new CommandInfo(Command.Headlights, CommandType.Digital, "HEADLIGHTS") },
			//Steam locomotive
			{ Command.LiveSteamInjector, new CommandInfo(Command.LiveSteamInjector, CommandType.Digital, "LIVE_STEAM_INJECTOR") },
			{ Command.ExhaustSteamInjector, new CommandInfo(Command.ExhaustSteamInjector, CommandType.Digital, "EXHAUST_STEAM_INJECTOR") },
			{ Command.IncreaseCutoff, new CommandInfo(Command.IncreaseCutoff, CommandType.Digital, "INCREASE_CUTOFF") },
			{ Command.DecreaseCutoff, new CommandInfo(Command.DecreaseCutoff, CommandType.Digital, "DECREASE_CUTOFF") },
			{ Command.Blowers, new CommandInfo(Command.Blowers, CommandType.Digital, "BLOWERS") },
			{ Command.CylinderCocks, new CommandInfo(Command.CylinderCocks, CommandType.Digital, "CYLINDER_COCKS") },
			//Diesel Locomotive
			{ Command.EngineStart, new CommandInfo(Command.EngineStart, CommandType.Digital, "ENGINE_START") },
			{ Command.EngineStop, new CommandInfo(Command.EngineStop, CommandType.Digital, "ENGINE_STOP") },
			{ Command.GearUp, new CommandInfo(Command.GearUp, CommandType.Digital, "GEAR_UP") },
			{ Command.GearDown, new CommandInfo(Command.GearDown, CommandType.Digital, "GEAR_DOWN") },
			
			//Electric Locomotive
			{ Command.RaisePantograph, new CommandInfo(Command.RaisePantograph, CommandType.Digital, "RAISE_PANTOGRAPH") },
			{ Command.LowerPantograph, new CommandInfo(Command.LowerPantograph, CommandType.Digital, "LOWER_PANTOGRAPH") },
			{ Command.MainBreaker, new CommandInfo(Command.MainBreaker, CommandType.Digital, "MAIN_BREAKER") },
			 
			//Simulation controls
			{ Command.CameraInterior, new CommandInfo(Command.CameraInterior, CommandType.Digital, "CAMERA_INTERIOR") },
			{ Command.CameraInteriorNoPanel, new CommandInfo(Command.CameraInteriorNoPanel,CommandType.Digital,"CAMERA_INTERIOR_NOPANEL") }, 
			{ Command.CameraExterior, new CommandInfo(Command.CameraExterior, CommandType.Digital, "CAMERA_EXTERIOR") },
			{ Command.CameraHeadOutLeft, new CommandInfo(Command.CameraInterior, CommandType.Digital, "CAMERA_HEADOUT_LEFT") },
			{ Command.CameraHeadOutRight, new CommandInfo(Command.CameraInterior, CommandType.Digital, "CAMERA_HEADOUT_RIGHT") },
			{ Command.CameraTrack, new CommandInfo(Command.CameraTrack, CommandType.Digital, "CAMERA_TRACK") },
			{ Command.CameraFlyBy, new CommandInfo(Command.CameraFlyBy, CommandType.Digital, "CAMERA_FLYBY") },
			{ Command.CameraMoveForward, new CommandInfo(Command.CameraMoveForward, CommandType.AnalogHalf, "CAMERA_MOVE_FORWARD") },
			{ Command.CameraMoveBackward, new CommandInfo(Command.CameraMoveBackward, CommandType.AnalogHalf, "CAMERA_MOVE_BACKWARD") },
			{ Command.CameraMoveLeft, new CommandInfo(Command.CameraMoveLeft, CommandType.AnalogHalf, "CAMERA_MOVE_LEFT") },
			{ Command.CameraMoveRight, new CommandInfo(Command.CameraMoveRight, CommandType.AnalogHalf, "CAMERA_MOVE_RIGHT") },
			{ Command.CameraMoveUp, new CommandInfo(Command.CameraMoveUp, CommandType.AnalogHalf, "CAMERA_MOVE_UP") },
			{ Command.CameraMoveDown, new CommandInfo(Command.CameraMoveDown, CommandType.AnalogHalf, "CAMERA_MOVE_DOWN") },
			{ Command.CameraRotateLeft, new CommandInfo(Command.CameraRotateLeft, CommandType.AnalogHalf, "CAMERA_ROTATE_LEFT") },
			{ Command.CameraRotateRight, new CommandInfo(Command.CameraRotateRight, CommandType.AnalogHalf, "CAMERA_ROTATE_RIGHT") },
			{ Command.CameraRotateUp, new CommandInfo(Command.CameraRotateUp, CommandType.AnalogHalf, "CAMERA_ROTATE_UP") },
			{ Command.CameraRotateDown, new CommandInfo(Command.CameraRotateDown, CommandType.AnalogHalf, "CAMERA_ROTATE_DOWN") },
			{ Command.CameraRotateCCW, new CommandInfo(Command.CameraRotateCCW, CommandType.AnalogHalf, "CAMERA_ROTATE_CCW") },
			{ Command.CameraRotateCW, new CommandInfo(Command.CameraRotateCW, CommandType.AnalogHalf, "CAMERA_ROTATE_CW") },
			{ Command.CameraZoomIn, new CommandInfo(Command.CameraZoomIn, CommandType.AnalogHalf, "CAMERA_ZOOM_IN") },
			{ Command.CameraZoomOut, new CommandInfo(Command.CameraZoomOut, CommandType.AnalogHalf, "CAMERA_ZOOM_OUT") },
			{ Command.CameraPOIPrevious, new CommandInfo(Command.CameraPOIPrevious, CommandType.Digital, "CAMERA_POI_PREVIOUS") },
			{ Command.CameraPOINext, new CommandInfo(Command.CameraPOINext, CommandType.Digital, "CAMERA_POI_NEXT") },
			{ Command.CameraReset, new CommandInfo(Command.CameraReset, CommandType.Digital, "CAMERA_RESET") },
			{ Command.CameraRestriction, new CommandInfo(Command.CameraRestriction, CommandType.Digital, "CAMERA_RESTRICTION") },
			{ Command.TimetableToggle, new CommandInfo(Command.TimetableToggle, CommandType.Digital, "TIMETABLE_TOGGLE") },
			{ Command.TimetableUp, new CommandInfo(Command.TimetableUp, CommandType.AnalogHalf, "TIMETABLE_UP") },
			{ Command.TimetableDown, new CommandInfo(Command.TimetableDown, CommandType.AnalogHalf, "TIMETABLE_DOWN") },
			{ Command.MenuActivate, new CommandInfo(Command.MenuActivate, CommandType.Digital, "MENU_ACTIVATE") },
			{ Command.MenuUp, new CommandInfo(Command.MenuUp, CommandType.Digital, "MENU_UP") },
			{ Command.MenuDown, new CommandInfo(Command.MenuDown, CommandType.Digital, "MENU_DOWN") },
			{ Command.MenuEnter, new CommandInfo(Command.MenuEnter, CommandType.Digital, "MENU_ENTER") },
			{ Command.MenuBack, new CommandInfo(Command.MenuBack, CommandType.Digital, "MENU_BACK") },
			{ Command.MiscClock, new CommandInfo(Command.MiscClock, CommandType.Digital, "MISC_CLOCK") },
			{ Command.MiscSpeed, new CommandInfo(Command.MiscSpeed, CommandType.Digital, "MISC_SPEED") },
			{ Command.MiscGradient, new CommandInfo(Command.MiscGradient, CommandType.Digital, "MISC_GRADIENT") },
			{ Command.MiscDistNextStation, new CommandInfo(Command.MiscDistNextStation, CommandType.Digital, "MISC_DIST_NEXT_STATION") },
			{ Command.MiscFps, new CommandInfo(Command.MiscFps, CommandType.Digital, "MISC_FPS") },
			{ Command.MiscAI, new CommandInfo(Command.MiscAI, CommandType.Digital, "MISC_AI") },
			{ Command.MiscFullscreen, new CommandInfo(Command.MiscFullscreen, CommandType.Digital, "MISC_FULLSCREEN") },
			{ Command.MiscMute, new CommandInfo(Command.MiscMute, CommandType.Digital, "MISC_MUTE") },
			{ Command.MiscPause, new CommandInfo(Command.MiscPause, CommandType.Digital, "MISC_PAUSE") },
			{ Command.MiscTimeFactor, new CommandInfo(Command.MiscTimeFactor, CommandType.Digital, "MISC_TIMEFACTOR") },
			{ Command.MiscQuit, new CommandInfo(Command.MiscQuit, CommandType.Digital, "MISC_QUIT") },
			{ Command.MiscInterface, new CommandInfo(Command.MiscInterface, CommandType.Digital, "MISC_INTERFACE") },
			{ Command.MiscBackface, new CommandInfo(Command.MiscBackface, CommandType.Digital, "MISC_BACKFACE") },
			{ Command.MiscCPUMode, new CommandInfo(Command.MiscCPUMode, CommandType.Digital, "MISC_CPUMODE") },
			{ Command.DebugWireframe, new CommandInfo(Command.DebugWireframe, CommandType.Digital, "DEBUG_WIREFRAME") },
			{ Command.DebugNormals, new CommandInfo(Command.DebugNormals, CommandType.Digital, "DEBUG_NORMALS") },
			{ Command.DebugBrakeSystems, new CommandInfo(Command.DebugBrakeSystems, CommandType.Digital, "DEBUG_BRAKE_SYSTEMS") },
			{ Command.DebugATS, new CommandInfo(Command.DebugATS, CommandType.Digital, "DEBUG_ATS") },
			{ Command.DebugTouchMode, new CommandInfo(Command.DebugTouchMode, CommandType.Digital, "DEBUG_TOUCH_MODE") },
			{ Command.RouteInformation, new CommandInfo(Command.RouteInformation, CommandType.Digital, "ROUTE_INFORMATION") },
			{ Command.ShowEvents, new CommandInfo(Command.ShowEvents, CommandType.Digital, "SHOW_EVENTS") },
			{ Command.DebugRendererMode, new CommandInfo(Command.DebugRendererMode, CommandType.Digital, "DEBUG_RENDERER_MODE") }, 
			{ Command.RailDriverSpeedUnits, new CommandInfo(Command.RailDriverSpeedUnits, CommandType.Digital, "RAILDRIVER_SPEED_UNITS") },
			//Accessibility
			{ Command.AccessibilityCurrentSpeed, new CommandInfo(Command.AccessibilityCurrentSpeed, CommandType.Digital, "ACCESSIBILITY_CURRENT_SPEED") },
			{ Command.AccessibilityNextSignal, new CommandInfo(Command.AccessibilityNextSignal, CommandType.Digital, "ACCESSIBILITY_NEXT_SIGNAL") },
			{ Command.AccessibilityNextStation, new CommandInfo(Command.AccessibilityNextStation, CommandType.Digital, "ACCESSIBILITY_NEXT_STATION") },
			{ Command.AccessibilityNextLimit, new CommandInfo(Command.AccessibilityNextLimit, CommandType.Digital, "ACCESSIBILITY_NEXT_LIMIT") },
			// Coupling
			{ Command.UncoupleFront, new CommandInfo(Command.UncoupleFront, CommandType.Digital, "UNCOUPLE_FRONT") },
			{ Command.UncoupleRear, new CommandInfo(Command.UncoupleRear, CommandType.Digital, "UNCOUPLE_REAR") },
			// Switch
			{ Command.SwitchMenu, new CommandInfo(Command.SwitchMenu, CommandType.Digital, "SWITCH_MENU") }
		};

		
		
	}
}
