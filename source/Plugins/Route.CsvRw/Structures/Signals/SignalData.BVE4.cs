using OpenBveApi.Objects;
using OpenBveApi.Textures;

namespace OpenBve
{
	/// <summary>Defines a BVE 4 standard signal:
	/// A signal has a face based mesh and glow
	/// Textures are then substituted according to the aspect
	/// </summary>
	internal class Bve4SignalData : SignalData
	{
		internal StaticObject BaseObject;
		internal StaticObject GlowObject;
		internal Texture[] SignalTextures;
		internal Texture[] GlowTextures;
	}
}
