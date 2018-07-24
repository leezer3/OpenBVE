using System;

namespace OpenBve
{
	internal static partial class HUD
	{
		[Flags]
		internal enum Transition
		{
			None = 0,
			Move = 1,
			Fade = 2,
			MoveAndFade = 3
		}
	}
}
