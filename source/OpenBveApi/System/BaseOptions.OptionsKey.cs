// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace OpenBveApi
{
	/// <summary>The keys found in an options file</summary>
    public enum OptionsKey
    {
		// Language
		Code,
		// Interface
		Folder,
		TimetableMode,
		KioskMode,
		KioskModeTimer,
		Accessibility,
		Font,
		DailyBuildUpdates,
		// Display
		PreferNativeBackend,
		Mode,
		VSync,
		WindowWidth,
		WindowHeight,
		FullScreenWidth,
		FullScreenHeight,
		FullScreenBits,
		MainMenuWidth,
		MainMenuHeight,
		LoadInAdvance,
		UnloadTextures,
		ForwardsCompatibleContext,
		IsUseNewRenderer,
		ViewingDistance,
		QuadLeafSize,
		UIScaleFactor,
		// Quality
		Interpolation,
		AnisotropicFilteringLevel,
		AnisotropicFilteringMaximum,
		AntiAliasingLevel,
		TransparencyMode,
		OldTransparencyMode,
		MotionBlur,
		FPSLimit,
		// Object Optimization
		BasicThreshold,
		FullThreshold,
		VertexCulling,
		// Simulation
		Toppling,
		Collisions,
		Derailments,
		LoadingSway,
		BlackBox,
		AcceleratedTimeFactor,
		EnableBVETSHacks,
		EnableBVE5ScriptedTrain,
		// Controls
		UseJoysticks,
		JoystickAxisEB,
		JoystickAxisThreshold,
		KeyRepeatDelay,
		KeyRepeatInterval,
		RailDriverMPH,
		CursorHideDelay,
		// Sound
		Model,
		Range,
		Number,
		// Verbosity
		ShowWarningMessages,
		ShowErrorMessages,
		DebugLog,
		// Loading
		ShowLogo,
		ShowProgressBar,
		ShowBackground,
		// Parsers
		XObject,
		ObjObject,
		GDIPlus,
		// Folders
		ObjectSearch,
		RouteSearch,
		Route,
		Train,
		// Packages
		Compression,
		// Touch
		Cursor,
		Panel2Extended,
		Panel2ExtendedMinSize,
		// Keys
		Left,
		Right,
		Up,
		Down,
		Forward,
		Backward
    }
}
