#pragma warning disable 0659, 0661

namespace OpenBveApi.Sounds
{
	/// <summary>Represents the origin where the sound can be loaded from.</summary>
	public abstract class SoundOrigin {
		// --- functions ---
		/// <summary>Gets the sound from this origin.</summary>
		/// <param name="sound">Receives the sound.</param>
		/// <returns>Whether the sound could be obtained successfully.</returns>
		public abstract bool GetSound(out Sound sound);
		// --- operators ---
		/// <summary>Checks whether two origins are equal.</summary>
		/// <param name="a">The first origin.</param>
		/// <param name="b">The second origin.</param>
		/// <returns>Whether the two origins are equal.</returns>
		public static bool operator ==(SoundOrigin a, SoundOrigin b)
		{
			if (a is PathOrigin & b is PathOrigin) {
				return (PathOrigin)a == (PathOrigin)b;
			}
			return object.ReferenceEquals(a, b);
		}

		/// <summary>Checks whether two origins are unequal.</summary>
		/// <param name="a">The first origin.</param>
		/// <param name="b">The second origin.</param>
		/// <returns>Whether the two origins are unequal.</returns>
		public static bool operator !=(SoundOrigin a, SoundOrigin b)
		{
			if (a is PathOrigin & b is PathOrigin) {
				return (PathOrigin)a != (PathOrigin)b;
			}
			return !object.ReferenceEquals(a, b);
		}

		/// <summary>Checks whether this instance is equal to the specified object.</summary>
		/// <param name="obj">The object.</param>
		/// <returns>Whether this instance is equal to the specified object.</returns>
		public override bool Equals(object obj)
		{
			if (this is PathOrigin & obj is PathOrigin) {
				return (PathOrigin)this == (PathOrigin)obj;
			}
			return object.ReferenceEquals(this, obj);
		}
	}
}
