namespace OpenBveShared
{
	public static class World
	{
		/// <summary>The current horizontal viewing angle in radians</summary>
		public static double HorizontalViewingAngle;
		/// <summary>The current vertical viewing angle in radians</summary>
		public static double VerticalViewingAngle;
		/// <summary>The original vertical viewing angle in radians</summary>
		public static double OriginalVerticalViewingAngle;
		/// <summary>The current aspect ratio</summary>
		public static double AspectRatio;
		/// <summary>The current viewing distance in the forward direction.</summary>
		public static double ForwardViewingDistance;
		/// <summary>The current viewing distance in the backward direction.</summary>
		public static double BackwardViewingDistance;
		/// <summary>The extra viewing distance used for determining visibility of animated objects.</summary>
		public static double ExtraViewingDistance;
	}
}
