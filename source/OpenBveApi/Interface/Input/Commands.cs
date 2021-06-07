using System;
using OpenBveApi.Runtime;

namespace OpenBveApi.Interface {
	public static partial class Translations
	{
		/// <summary>Defines the available commands which may be callled by a player during a simulation session</summary>
		public enum Command
		{
			//Basic controls
			/// <summary>No command specified</summary>
			None = 0,
			/// <summary>Increases the power notch on a train with separate power and brake handles</summary>
			PowerIncrease,
			/// <summary>Decreases the power notch on a train with separate power and brake handles</summary>
			PowerDecrease,
			/// <summary>Controls the power notch using half a joystick axis</summary>
			PowerHalfAxis,
			/// <summary>Controls the power notch using a full joystick axis</summary>
			PowerFullAxis,
			/// <summary>Increases the brake notch on a train with separate power and brake handles</summary>
			BrakeIncrease,
			/// <summary>Decreases the brake notch on a train with separate power and brake handles</summary>
			BrakeDecrease,
			/// <summary>Applies full emergency brake on a train with separate power and brake handles (Power notch remains at the driver's setting)</summary>
			BrakeEmergency,
			/// <summary>Controls the brake notch using half a joystick axis</summary>
			BrakeHalfAxis,
			/// <summary>Controls the brake notch using a full joystick axis</summary>
			BrakeFullAxis,
			/// <summary>Advances the handle towards the maximum power notch on a train with a combined power and brake handle</summary>
			SinglePower,
			/// <summary>Moves the handle towards neutral on a train with a combined power and brake handle (Will not apply brakes)</summary>
			SingleNeutral,
			/// <summary>Returns the handle towards the maxium brake notch on a train with a combined power and brake handle</summary>
			SingleBrake,
			/// <summary>Applies full emergency brake on a train with a combined power and brake handle</summary>
			SingleEmergency,
			/// <summary>Controls a combined power and brake handle using a full joystick axis</summary>
			SingleFullAxis,
			/// <summary>Adjust to the power notch directly from command option value</summary>
			PowerAnyNotch,
			/// <summary>Adjust to the brake notch directly from command option value</summary>
			BrakeAnyNotch,
			/// <summary>Adjust to the reverser directly from command option value</summary>
			ReverserAnyPostion,
			/// <summary>Hold Brake</summary>
			HoldBrake,
			/// <summary>Moves the reverser in the forwards direction</summary>
			ReverserForward,
			/// <summary>Moves the reverser in the backwards direction</summary>
			ReverserBackward,
			/// <summary>Controls the reverser using a full joystick axis</summary>
			ReverserFullAxis,
			/// <summary>Opens or closes the left-hand doors</summary>
			DoorsLeft,
			/// <summary>Opens or closes the right-hand doors</summary>
			DoorsRight,
			/// <summary>Activates the primary horn</summary>
			HornPrimary,
			/// <summary>Activates the secondary horn</summary>
			HornSecondary,
			/// <summary>Activates the music horn</summary>
			HornMusic,
			/// <summary>Enables or disables the constant-speed device</summary>
			DeviceConstSpeed,
			/// <summary>Increases the locomotive brake notch</summary>
			LocoBrakeIncrease,
			/// <summary>Decreases the locomotive brake notch</summary>
			LocoBrakeDecrease,
			/// <summary>Activate microphone audio playback</summary>
			PlayMicSounds,

			//Simulation controls
			/// <summary>Change to the interior (Cab) camera mode</summary>
			CameraInterior,
			/// <summary>Change to the interior (Cab) camera mode. However, the panel is not displayed.</summary>
			CameraInteriorNoPanel,
			/// <summary>Change to the exterior (Attached to train) camera mode</summary>
			CameraExterior,
			/// <summary>Change to the track based camera mode</summary>
			CameraTrack,
			/// <summary>Change to fly-by camera mode</summary>
			CameraFlyBy,
			/// <summary>Move the camera forwards</summary>
			CameraMoveForward,
			/// <summary>Move the camera backwards</summary>
			CameraMoveBackward,
			/// <summary>Move the camera left</summary>
			CameraMoveLeft,
			/// <summary>Move the camera right</summary>
			CameraMoveRight,
			/// <summary>Move the camera up</summary>
			CameraMoveUp,
			/// <summary>Move the camera down</summary>
			CameraMoveDown,
			/// <summary>Rotate the camera left</summary>
			CameraRotateLeft,
			/// <summary>Rotate the camera right</summary>
			CameraRotateRight,
			/// <summary>Rotate the camera up</summary>
			CameraRotateUp,
			/// <summary>Rotate the camera down</summary>
			CameraRotateDown,
			/// <summary>Rotate the camera counter-clockwise</summary>
			CameraRotateCCW,
			/// <summary>Rotate the camera clockwise</summary>
			CameraRotateCW,
			/// <summary>Zoom the camera in</summary>
			CameraZoomIn,
			/// <summary>Zoom the camera out</summary>
			CameraZoomOut,
			/// <summary>Shift to the previous Point of Interest camera view</summary>
			CameraPreviousPOI,
			/// <summary>Shift to the next Point of Interest camera view</summary>
			CameraNextPOI,
			/// <summary>Reset the camera to pointing immediately forwards at track-level</summary>
			CameraReset,
			/// <summary>Toggle camera restriction mode</summary>
			CameraRestriction,
			/// <summary>Shows or hides the in-game timetable</summary>
			TimetableToggle,
			/// <summary>Scrolls the in-game timetable up</summary>
			TimetableUp,
			/// <summary>Scrolls the in-game timetable down</summary>
			TimetableDown,
			/// <summary>Shows or hides the in-game clock</summary>
			MiscClock,
			/// <summary>Shows or hides the in-game speed display</summary>
			MiscSpeed,
			/// <summary>Shows or hides the in-game gradient display</summary>
			MiscGradient,
			/// <summary>Show / hide the in-game remain distance of the next station</summary>
			MiscDistanceToNextStation,
			/// <summary>Shows or hides the in-game FPS display</summary>
			MiscFps,
			/// <summary>Toggles AI control of the player train</summary>
			MiscAI,
			/// <summary>Switches between the different interface modes</summary>
			MiscInterfaceMode,
			/// <summary>Toggles the backface-culling mode</summary>
			MiscBackfaceCulling,
			/// <summary>Switches between low and high CPU modes</summary>
			MiscCPUMode,
			/// <summary>Switches between the normal and accelerated time-factors</summary>
			MiscTimeFactor,
			/// <summary>Pauses/ unpauses the game</summary>
			MiscPause,
			/// <summary>Mutes/ unmutes the game</summary>
			MiscMute,
			/// <summary>Toggles fullscreen mode</summary>
			MiscFullscreen,
			/// <summary>Quits the game</summary>
			MiscQuit,
			/// <summary>Activates the in-game menu</summary>
			MenuActivate,
			/// <summary>Moves up within a menu</summary>
			MenuUp,
			/// <summary>Moves down within a menu</summary>
			MenuDown,
			/// <summary>Enters a submenu</summary>
			MenuEnter,
			/// <summary>Returns from a submenu</summary>
			MenuBack,
			/// <summary>Toggles wireframe rendering</summary>
			DebugWireframe,
			/// <summary>Toggles rendering of a visual representation of vertex normals</summary>
			DebugNormals,
			/// <summary>Shows or hides debug information on the brake-system performance</summary>
			DebugBrakeSystems,
			/// <summary>Shows or hides the ATS (plugin) debugn screen</summary>
			DebugATS,
			/// <summary>Shows or hides the touch range</summary>
			DebugTouchMode,
			/// <summary>Shows or hides a visual representation of all events on the track</summary>
			ShowEvents,
			/// <summary>Toggles the renderer</summary>
			DebugRendererMode,
			/*
			 * The following keys must be handled by the train-plugin.
			 * They have no specified purpose.
			 * 
			 */

			/// <summary>The security (Plugin) S-Key</summary>
			[Obsolete("Please use the function specific keys. These are largely maintained for backwards compatibility")]
			SecurityS,
			/// <summary>The security (Plugin) A1-Key</summary>
			[Obsolete("Please use the function specific keys. These are largely maintained for backwards compatibility")]
			SecurityA1,
			/// <summary>The security (Plugin) A2-Key</summary>
			[Obsolete("Please use the function specific keys. These are largely maintained for backwards compatibility")]
			SecurityA2,
			/// <summary>The security (Plugin) B1-Key</summary>
			[Obsolete("Please use the function specific keys. These are largely maintained for backwards compatibility")]
			SecurityB1,
			/// <summary>The security (Plugin) B2-Key</summary>
			[Obsolete("Please use the function specific keys. These are largely maintained for backwards compatibility")]
			SecurityB2,
			/// <summary>The security (Plugin) C1-Key</summary>
			[Obsolete("Please use the function specific keys. These are largely maintained for backwards compatibility")]
			SecurityC1,
			/// <summary>The security (Plugin) C2-Key</summary>
			[Obsolete("Please use the function specific keys. These are largely maintained for backwards compatibility")]
			SecurityC2,
			/// <summary>The security (Plugin) D-Key</summary>
			[Obsolete("Please use the function specific keys. These are largely maintained for backwards compatibility")]
			SecurityD,
			/// <summary>The security (Plugin) E-Key</summary>
			[Obsolete("Please use the function specific keys. These are largely maintained for backwards compatibility")]
			SecurityE,
			/// <summary>The security (Plugin) F-Key</summary>
			[Obsolete("Please use the function specific keys. These are largely maintained for backwards compatibility")]
			SecurityF,
			/// <summary>The security (Plugin) G-Key</summary>
			[Obsolete("Please use the function specific keys. These are largely maintained for backwards compatibility")]
			SecurityG,
			/// <summary>The security (Plugin) H-Key</summary>
			[Obsolete("Please use the function specific keys. These are largely maintained for backwards compatibility")]
			SecurityH,
			/// <summary>The security (Plugin) I-Key</summary>
			[Obsolete("Please use the function specific keys. These are largely maintained for backwards compatibility")]
			SecurityI,
			/// <summary>The security (Plugin) J-Key</summary>
			[Obsolete("Please use the function specific keys. These are largely maintained for backwards compatibility")]
			SecurityJ,
			/// <summary>The security (Plugin) K-Key</summary>
			[Obsolete("Please use the function specific keys. These are largely maintained for backwards compatibility")]
			SecurityK,
			/// <summary>The security (Plugin) L-Key</summary>
			[Obsolete("Please use the function specific keys. These are largely maintained for backwards compatibility")]
			SecurityL,
			/// <summary>The security (Plugin) M-Key</summary>
			[Obsolete("Please use the function specific keys. These are largely maintained for backwards compatibility")]
			SecurityM,
			/// <summary>The security (Plugin) N-Key</summary>
			[Obsolete("Please use the function specific keys. These are largely maintained for backwards compatibility")]
			SecurityN,
			/// <summary>The security (Plugin) O-Key</summary>
			[Obsolete("Please use the function specific keys. These are largely maintained for backwards compatibility")]
			SecurityO,
			/// <summary>The security (Plugin) P-Key</summary>
			[Obsolete("Please use the function specific keys. These are largely maintained for backwards compatibility")]
			SecurityP,

			/* 
			 * These keys were added in 1.4.4.0
			 * Plugins should refer to these rather than the deprecated Security keys
			 * Doing this means that key assignments can be changed globally for all locomotives
			 * that support this method, rather than the haphazard and non-standard use
			 * of the security keys
			 * 
			 */

			//Common Keys
			/// <summary>Increase the speed of the windscreen wipers</summary>
			WiperSpeedUp,
			/// <summary>Decrease the speed of the windscreen wipers</summary>
			WiperSpeedDown,
			/// <summary>Fill fuel</summary>
			FillFuel,
			/// <summary>Toggles the headlights</summary>
			Headlights,
			//Steam locomotive
			/// <summary>The live-steam injector</summary>
			LiveSteamInjector,
			/// <summary>The exhaust-steam injector</summary>
			ExhaustSteamInjector,
			/// <summary>Increase cutoff</summary>
			IncreaseCutoff,
			/// <summary>Decrease cutoff</summary>
			DecreaseCutoff,
			/// <summary>The blowers</summary>
			Blowers,
			//Diesel Locomotive
			/// <summary>Start the engine</summary>
			EngineStart,
			/// <summary>Stop the engine</summary>
			EngineStop,
			/// <summary>Shift up a gear</summary>
			GearUp,
			/// <summary>Shift down a gear</summary>
			GearDown,
			//Electric Locomotive
			/// <summary>Raise the pantograph</summary>
			RaisePantograph,
			/// <summary>Lower the pantograph</summary>
			LowerPantograph,
			/// <summary>The main breaker</summary>
			MainBreaker,
			//Other
			/// <summary>Shows or hides the route information window</summary>
			RouteInformation,
			/// <summary>Toggles the speed units in the RailDriver's display</summary>
			RailDriverSpeedUnits,
			/*
			 * These keys were added in 1.8.1.0
			 * Used to trigger accessibility messages
			 */
			/// <summary>Triggers a screen reader message with the current speed</summary>
			AccessibilityCurrentSpeed,
			/// <summary>Triggers a screen reader message with the distance and aspect to the next signal</summary>
			AccessibilityNextSignal,
			/// <summary>Triggers a screen reader message with the distance and aspect to the next station</summary>
			AccessibilityNextStation
		}

		/// <summary>Defines the possible command types</summary>
		public enum CommandType
		{
			/// <summary>A digital input, comprising of ON and OFF states</summary>
			Digital,
			/// <summary>One-half of an analog joystick or gamepad axis</summary>
			AnalogHalf,
			/// <summary>A full analog joystick or gamepad axis</summary>
			AnalogFull
		}

		

		/// <summary>Converts the specified security command to a virtual key.</summary>
		/// <returns>Virtual key for plugins.</returns>
		/// <param name="cmd">The security command. If this isn't a recognized security command, ArgumentException will be thrown.</param>
		public static VirtualKeys SecurityToVirtualKey(Command cmd)
		{
			string cmdname = Enum.GetName(typeof(Command), cmd);
			if (cmdname == null) throw new ArgumentNullException("cmd");
			if (cmdname.StartsWith("Security", StringComparison.Ordinal))
				cmdname = cmdname.Substring(8).ToUpperInvariant();
			VirtualKeys key;
			if (!Enum.TryParse(cmdname, out key))
				throw new ArgumentException("VirtualKeys does not contain the following key: " +
					cmdname, "cmd");
			return key;
		}
	}
}
