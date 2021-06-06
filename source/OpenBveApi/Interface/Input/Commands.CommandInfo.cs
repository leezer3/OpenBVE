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
			public string Description;
			/// <summary>Whether to enable command options</summary>
			public readonly bool EnableOption;

			internal CommandInfo(Command Command, CommandType Type, string Name)
			{
				this.Command = Command;
				this.Type = Type;
				this.Name = Name;
				this.Description = "N/A";
				this.EnableOption = false;
			}

			internal CommandInfo(Command Command, CommandType Type, string Name, bool EnableOption)
			{
				this.Command = Command;
				this.Type = Type;
				this.Name = Name;
				this.Description = "N/A";
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
				if (obj is CommandInfo)
				{
					CommandInfo newCommandInfo = (CommandInfo) obj;
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
		public static CommandInfo TryGetInfo(this CommandInfo[] commandInfos, Command Value)
		{
			for (int i = 0; i < commandInfos.Length; i++) {
				if (commandInfos[i].Command == Value)
				{
					return commandInfos[i];
				}
			}
			return new CommandInfo(Value, CommandType.Digital, "N/A");
		}

		/// <summary>Contains the list of all known command infos</summary>
		public static CommandInfo[] CommandInfos = {
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
			new CommandInfo(Command.PowerAnyNotch, CommandType.Digital, "POWER_ANY_NOTCH", true),
			new CommandInfo(Command.BrakeAnyNotch, CommandType.Digital, "BRAKE_ANY_NOTCH", true),
			new CommandInfo(Command.ReverserAnyPostion, CommandType.Digital, "REVERSER_ANY_POSITION", true),
			new CommandInfo(Command.HoldBrake,CommandType.Digital, "HOLD_BRAKE"),
			new CommandInfo(Command.ReverserForward, CommandType.Digital, "REVERSER_FORWARD"),
			new CommandInfo(Command.ReverserBackward, CommandType.Digital, "REVERSER_BACKWARD"),
			new CommandInfo(Command.ReverserFullAxis, CommandType.AnalogFull, "REVERSER_FULLAXIS"),
			new CommandInfo(Command.DoorsLeft, CommandType.Digital, "DOORS_LEFT"),
			new CommandInfo(Command.DoorsRight, CommandType.Digital, "DOORS_RIGHT"),
			new CommandInfo(Command.HornPrimary, CommandType.Digital, "HORN_PRIMARY"),
			new CommandInfo(Command.HornSecondary, CommandType.Digital, "HORN_SECONDARY"),
			new CommandInfo(Command.HornMusic, CommandType.Digital, "HORN_MUSIC"),
			new CommandInfo(Command.DeviceConstSpeed, CommandType.Digital, "DEVICE_CONSTSPEED"),
			new CommandInfo(Command.PlayMicSounds, CommandType.Digital, "PLAY_MIC_SOUNDS"),

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
			new CommandInfo(Command.CameraInteriorNoPanel,CommandType.Digital,"CAMERA_INTERIOR_NOPANEL"), 
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
			new CommandInfo(Command.MiscDistanceToNextStation, CommandType.Digital, "MISC_DIST_NEXT_STATION"),
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
			new CommandInfo(Command.DebugTouchMode, CommandType.Digital, "DEBUG_TOUCH_MODE"),
			new CommandInfo(Command.RouteInformation, CommandType.Digital, "ROUTE_INFORMATION"),
			new CommandInfo(Command.ShowEvents, CommandType.Digital, "SHOW_EVENTS"),
			new CommandInfo(Command.DebugRendererMode, CommandType.Digital, "DEBUG_RENDERER_MODE"), 
			new CommandInfo(Command.RailDriverSpeedUnits, CommandType.Digital, "RAILDRIVER_SPEED_UNITS"),
			//Accessibility
			new CommandInfo(Command.AccessibilityCurrentSpeed, CommandType.Digital, "ACCESSIBILITY_CURRENT_SPEED"),
			new CommandInfo(Command.AccessibilityNextSignal, CommandType.Digital, "ACCESSIBILITY_NEXT_SIGNAL"),
			new CommandInfo(Command.AccessibilityNextStation, CommandType.Digital, "ACCESSIBILITY_NEXT_STATION"),
		};
	}
}
