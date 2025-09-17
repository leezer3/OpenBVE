// ReSharper disable UnusedMember.Global
namespace Train.MsTs
{
	internal enum CabComponentType
	{
		/// <summary>None</summary>
		None = 0,
		/// <summary>Dial based control</summary>
		Dial = 1,
		/// <summary>Lever based control</summary>
		Lever = 2,
		/// <summary>Dial based gauge</summary>
		Gauge = 3,
		/// <summary>Two-state based control</summary>
		TwoState = 4,
		/// <summary>Tri-state based control</summary>
		TriState = 5,
		/// <summary>A display capable of displaying N states</summary>
		MultiStateDisplay = 6,
		/// <summary>In cab signalling / safety system (e.g. AWS)</summary>
		CabSignalDisplay = 7,
		/// <summary>Steam locomotive firebox animation</summary>
		Firebox = 8,
		/// <summary>A digital display</summary>
		Digital = 9,
		/// <summary>A digital clock</summary>
		DigitalClock = 10,
		/// <summary>A combined power + brake controller</summary>
		CombinedControl = 11
	}
}
