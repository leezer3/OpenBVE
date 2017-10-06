#pragma warning disable 0659, 0661

namespace OpenBve
{
	internal static partial class Sounds
	{

		/*
		 * A sound origin defines where to load a sound from.
		 * This can be a path (file or directory), or raw data.
		 * */

		// --- sound origin ---

		/// <summary>Represents the origin where the sound can be loaded from.</summary>
		internal abstract class SoundOrigin
		{
			// --- functions ---
			/// <summary>Gets the sound from this origin.</summary>
			/// <param name="sound">Receives the sound.</param>
			/// <returns>Whether the sound could be obtained successfully.</returns>
			internal abstract bool GetSound(out OpenBveApi.Sounds.Sound sound);
			// --- operators ---
			/// <summary>Checks whether two origins are equal.</summary>
			/// <param name="a">The first origin.</param>
			/// <param name="b">The second origin.</param>
			/// <returns>Whether the two origins are equal.</returns>
			public static bool operator ==(SoundOrigin a, SoundOrigin b)
			{
				if (a is PathOrigin & b is PathOrigin)
				{
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
				if (a is PathOrigin & b is PathOrigin)
				{
					return (PathOrigin)a != (PathOrigin)b;
				}
				return !object.ReferenceEquals(a, b);
			}

			/// <summary>Checks whether this instance is equal to the specified object.</summary>
			/// <param name="obj">The object.</param>
			/// <returns>Whether this instance is equal to the specified object.</returns>
			public override bool Equals(object obj)
			{
				if (this is PathOrigin & obj is PathOrigin)
				{
					return (PathOrigin)this == (PathOrigin)obj;
				}
				return object.ReferenceEquals(this, obj);
			}
		}


		// --- path origin ---

		/// <summary>Represents a file or directory where the sound can be loaded from.</summary>
		internal class PathOrigin : SoundOrigin
		{
			// --- members ---
			internal string Path;
			// --- constructors ---
			/// <summary>Creates a new path origin.</summary>
			/// <param name="path">The path to the sound.</param>
			internal PathOrigin(string path)
			{
				this.Path = path;
			}
			// --- functions ---
			/// <summary>Gets the sound from this origin.</summary>
			/// <param name="sound">Receives the sound.</param>
			/// <returns>Whether the sound could be obtained successfully.</returns>
			internal override bool GetSound(out OpenBveApi.Sounds.Sound sound)
			{
				if (!Program.CurrentHost.LoadSound(this.Path, out sound))
				{
					sound = null;
					return false;
				}
				return true;
			}

			// --- operators ---
			/// <summary>Checks whether two origins are equal.</summary>
			/// <param name="a">The first origin.</param>
			/// <param name="b">The second origin.</param>
			/// <returns>Whether the two origins are equal.</returns>
			public static bool operator ==(PathOrigin a, PathOrigin b)
			{
				if (object.ReferenceEquals(a, b)) return true;
				if (object.ReferenceEquals(a, null)) return false;
				if (object.ReferenceEquals(b, null)) return false;
				return a.Path == b.Path;
			}
			/// <summary>Checks whether two origins are unequal.</summary>
			/// <param name="a">The first origin.</param>
			/// <param name="b">The second origin.</param>
			/// <returns>Whether the two origins are unequal.</returns>
			public static bool operator !=(PathOrigin a, PathOrigin b)
			{
				if (object.ReferenceEquals(a, b)) return false;
				if (object.ReferenceEquals(a, null)) return true;
				if (object.ReferenceEquals(b, null)) return true;
				return a.Path != b.Path;
			}
			/// <summary>Checks whether this instance is equal to the specified object.</summary>
			/// <param name="obj">The object.</param>
			/// <returns>Whether this instance is equal to the specified object.</returns>
			public override bool Equals(object obj)
			{
				if (object.ReferenceEquals(this, obj)) return true;
				if (object.ReferenceEquals(this, null)) return false;
				if (object.ReferenceEquals(obj, null)) return false;
				if (!(obj is PathOrigin)) return false;
				return this.Path == ((PathOrigin)obj).Path;
			}
		}


		// --- raw origin ---

		/// <summary>Represents sound raw data.</summary>
		internal class RawOrigin : SoundOrigin
		{
			// --- members ---
			/// <summary>The sound raw data.</summary>
			internal OpenBveApi.Sounds.Sound Sound;
			// --- constructors ---
			/// <summary>Creates a new raw data origin.</summary>
			/// <param name="sound">The sound raw data.</param>
			internal RawOrigin(OpenBveApi.Sounds.Sound sound)
			{
				this.Sound = sound;
			}
			// --- functions ---
			/// <summary>Gets the sound from this origin.</summary>
			/// <param name="sound">Receives the sound.</param>
			/// <returns>Whether the sound could be obtained successfully.</returns>
			internal override bool GetSound(out OpenBveApi.Sounds.Sound sound)
			{
				sound = this.Sound;
				return true;
			}
		}


	}
}
