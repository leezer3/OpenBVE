using System.Collections.Generic;

namespace OpenBveApi.Interface
{
	/// <summary>Provides encoding and decoding of the press order of keyboard modifiers</summary>
	public static class ModifierOrder
	{
		private static readonly KeyboardModifier[] All = { KeyboardModifier.Shift, KeyboardModifier.Ctrl, KeyboardModifier.Alt };

		private static int Factorial(int n)
		{
			int result = 1;
			for (int i = 2; i <= n; i++)
			{
				result *= i;
			}
			return result;
		}

		/// <summary>Computes the one-based lexicographic rank of the press sequence restricted to the currently held modifier set</summary>
		/// <param name="sequence">The modifiers in the order they were pressed</param>
		/// <param name="count">The number of valid entries in the sequence</param>
		/// <param name="heldSet">The bitmask of modifiers currently held down</param>
		public static int GetRank(IList<KeyboardModifier> sequence, int count, KeyboardModifier heldSet)
		{
			if (heldSet == KeyboardModifier.None)
			{
				return 0;
			}
			List<KeyboardModifier> ordered = new List<KeyboardModifier>();
			for (int i = 0; i < count; i++)
			{
				KeyboardModifier m = sequence[i];
				if ((heldSet & m) == m && !ordered.Contains(m))
				{
					ordered.Add(m);
				}
			}
			foreach (KeyboardModifier m in All)
			{
				if ((heldSet & m) == m && !ordered.Contains(m))
				{
					// Modifiers held but missing from the recorded sequence (e.g. pressed before tracking started) are appended in canonical order
					ordered.Add(m);
				}
			}
			int[] positions = new int[ordered.Count];
			for (int i = 0; i < ordered.Count; i++)
			{
				positions[i] = IndexOf(All, ordered[i]);
			}
			int rank = 0;
			for (int i = 0; i < positions.Length; i++)
			{
				int smaller = 0;
				for (int j = i + 1; j < positions.Length; j++)
				{
					if (positions[j] < positions[i])
					{
						smaller++;
					}
				}
				rank += smaller * Factorial(positions.Length - 1 - i);
			}
			return rank + 1;
		}

		/// <summary>Returns the members of the modifier set in their required press order for the given one-based rank</summary>
		public static List<KeyboardModifier> FromRank(int set, int rank)
		{
			List<KeyboardModifier> remaining = new List<KeyboardModifier>();
			foreach (KeyboardModifier m in All)
			{
				if ((set & (int)m) != 0)
				{
					remaining.Add(m);
				}
			}
			List<KeyboardModifier> result = new List<KeyboardModifier>();
			int r = rank - 1;
			while (remaining.Count > 0)
			{
				int f = Factorial(remaining.Count - 1);
				result.Add(remaining[r / f]);
				remaining.RemoveAt(r / f);
				r %= f;
			}
			return result;
		}

		private static int IndexOf(KeyboardModifier[] array, KeyboardModifier value)
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == value)
				{
					return i;
				}
			}
			return -1;
		}
	}
}
