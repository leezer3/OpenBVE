using OpenBveApi.Objects;
using RouteManager2.SignalManager;

namespace Bve5RouteParser
{
	/// <summary>Defines a BVE 4 standard signal:
	/// A signal has a face based mesh and glow
	/// Textures are then substituted according to the aspect
	/// </summary>
	internal class CompatibilitySignalData : SignalObject
	{
		internal readonly int[] Numbers;
		internal readonly StaticObject[] Objects;
		internal readonly string Key;
		internal CompatibilitySignalData(int[] Numbers, StaticObject[] Objects, string Key)
		{
			this.Numbers = Numbers;
			this.Objects = Objects;
			this.Key = Key;
		}
	}
}
