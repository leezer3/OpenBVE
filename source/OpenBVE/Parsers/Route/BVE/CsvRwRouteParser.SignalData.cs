using OpenBveApi.Objects;
using OpenBveApi.Textures;
using RouteManager2.SignalManager;

namespace OpenBve
{
	internal partial class CsvRwRouteParser
	{
		/// <summary>Defines a BVE 4 standard signal:
		/// A signal has a face based mesh and glow
		/// Textures are then substituted according to the aspect
		/// </summary>
		private class Bve4SignalObject : SignalObject
		{
			internal StaticObject BaseObject;
			internal StaticObject GlowObject;
			internal Texture[] SignalTextures;
			internal Texture[] GlowTextures;
		}
		
		/// <summary>Defines an animated signal object:
		/// The object is provided with the aspect number, and then should deal with the rest
		/// </summary>
		private class AnimatedObjectSignalObject : SignalObject
		{
			internal UnifiedObject Objects;
		}
	}
}
