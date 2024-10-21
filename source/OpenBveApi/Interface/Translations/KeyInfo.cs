using System.Collections.Generic;
using OpenBveApi.Input;
// ReSharper disable MergeCastWithTypeCheck

namespace OpenBveApi.Interface
{
	public partial class Translations
	{
		/// <summary>Defines an available keyboard key</summary>
		public struct KeyInfo
		{
			/// <summary>The key (OpenTK.Input.Key enum member)</summary>
			public readonly Key Key;
			/// <summary>The internal key name</summary>
			internal readonly string Name;
			/// <summary>The translated key description</summary>
			public string Description;
			/// <summary>Creates a new translated key</summary>
			internal KeyInfo(Key Key, string Name)
			{
				this.Key = Key;
				this.Name = Name;
				this.Description = Name;
			}

			/// <returns>ToString for a key binding returns the binding description in textual format, NOT the phyisical key</returns>
			public override string ToString()
			{
				return Description;
			}

			/// <summary>Checks whether the two specified KeyInfo instances are equal</summary>
			/// <remarks>This ignores the translated key description</remarks>
			public static bool operator ==(KeyInfo a, KeyInfo b)
			{
				if (a.Key != b.Key)
				{
					return false;
				}
				if (a.Name != b.Name)
				{
					return false;
				}
				return true;
			}

			/// <summary>Checks whether the two specified KeyInfo instances are NOT equal</summary>
			/// <remarks>This ignores the translated key description</remarks>
			public static bool operator !=(KeyInfo a, KeyInfo b)
			{
				if (a.Key == b.Key)
				{
					return false;
				}
				if (a.Name == b.Name)
				{
					return false;
				}
				return true;
			}

			/// <summary>Returns whether this KeyInfo instance is equal to the specified KeyInfo</summary>
			/// <remarks>This ignores the translated key description</remarks>
			public bool Equals(KeyInfo b)
			{
				return Key == b.Key && Name == b.Name;
			}

			/// <summary>Returns whether this KeyInfo instance is equal to the specified object</summary>
			/// <remarks>This ignores the translated key description</remarks>
			public override bool Equals(object obj)
			{
				if (obj is null) return false;
				return obj is KeyInfo && Equals((KeyInfo) obj);
			}

			/// <summary>Gets the HashCode for this KeyInfo instance</summary>
			/// <remarks>This ignores the translated key description</remarks>
			public override int GetHashCode()
			{
				unchecked
				{
					return ((int) Key * 397) ^ (Name != null ? Name.GetHashCode() : 0);
				}
			}


		}

		/// <summary>Holds the translations for all keys available for assignation</summary>
		public static Dictionary<Key, KeyInfo> TranslatedKeys;
	}
}
