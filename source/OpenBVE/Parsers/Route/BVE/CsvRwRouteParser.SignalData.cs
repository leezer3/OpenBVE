using OpenBveApi.Textures;

namespace OpenBve
{
	internal partial class CsvRwRouteParser
	{
		/// <summary>An abstract signal - All signals must inherit from this class</summary>
		private abstract class SignalData
		{
		}


		// ---SIGNAL TYPES---

		/// <summary>Defines a BVE 4 standard signal:
		/// A signal has a face based mesh and glow
		/// Textures are then substituted according to the aspect
		/// </summary>
		private class Bve4SignalData : SignalData
		{
			internal ObjectManager.StaticObject BaseObject;
			internal ObjectManager.StaticObject GlowObject;
			internal Texture[] SignalTextures;
			internal Texture[] GlowTextures;
		}
		/// <summary>Defines a default Japanese signal (See the documentation)</summary>
		private class CompatibilitySignalData : SignalData
		{
			internal readonly int[] Numbers;
			internal readonly ObjectManager.StaticObject[] Objects;
			internal CompatibilitySignalData(int[] Numbers, ObjectManager.StaticObject[] Objects)
			{
				this.Numbers = Numbers;
				this.Objects = Objects;
			}
		}
		/// <summary>Defines an animated signal object:
		/// The object is provided with the aspect number, and then should deal with the rest
		/// </summary>
		private class AnimatedObjectSignalData : SignalData
		{
			internal ObjectManager.AnimatedObjectCollection Objects;
		}
	}
}
