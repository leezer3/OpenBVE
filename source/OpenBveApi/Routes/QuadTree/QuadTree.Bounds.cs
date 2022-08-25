namespace OpenBveApi.Routes
{
	/// <summary>Represents some rectangular bounds on the grid.</summary>
	internal struct QuadTreeBounds
	{
		/// <summary>The left edge, i.e. the smallest x-coordinate.</summary>
		internal double Left;

		/// <summary>The right edge, i.e. the highest x-coordinate.</summary>
		internal double Right;

		/// <summary>The near edge, i.e. the smallest z-coordinate.</summary>
		internal double Near;

		/// <summary>The far edge, i.e. the highest z-coordinate.</summary>
		internal double Far;

		/// <summary>Creates a new instance of this structure.</summary>
		/// <param name="left">The left edge, i.e. the smallest x-coordinate.</param>
		/// <param name="right">The right edge, i.e. the highest x-coordinate.</param>
		/// <param name="near">The near edge, i.e. the smallest z-coordinate.</param>
		/// <param name="far">The far edge, i.e. the highest z-coordinate.</param>
		internal QuadTreeBounds(double left, double right, double near, double far)
		{
			Left = left;
			Right = right;
			Near = near;
			Far = far;
		}

		/// <summary>Represents a bounds that is invalid or has not been initialized.</summary>
		internal static readonly QuadTreeBounds Uninitialized = new QuadTreeBounds(double.MaxValue, double.MinValue, double.MaxValue, double.MinValue);
	}
}
