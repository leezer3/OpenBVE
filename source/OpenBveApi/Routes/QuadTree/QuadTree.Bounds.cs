using System;

namespace OpenBveApi.Routes
{
	/// <summary>Represents some rectangular bounds on the grid.</summary>
	public struct QuadTreeBounds
	{
		/// <summary>The left edge, i.e. the smallest x-coordinate.</summary>
		public double Left;

		/// <summary>The right edge, i.e. the highest x-coordinate.</summary>
		public double Right;

		/// <summary>The near edge, i.e. the smallest z-coordinate.</summary>
		public double Near;

		/// <summary>The far edge, i.e. the highest z-coordinate.</summary>
		public double Far;

		/// <summary>Creates a new instance of this structure.</summary>
		/// <param name="left">The left edge, i.e. the smallest x-coordinate.</param>
		/// <param name="right">The right edge, i.e. the highest x-coordinate.</param>
		/// <param name="near">The near edge, i.e. the smallest z-coordinate.</param>
		/// <param name="far">The far edge, i.e. the highest z-coordinate.</param>
		internal QuadTreeBounds(double left, double right, double near, double far)
		{
			if (double.IsInfinity(left))
			{
				throw new Exception("Cannot create a grid with infinity bounds: Left");
			}
			Left = left;
			if (double.IsInfinity(right))
			{
				throw new Exception("Cannot create a grid with infinity bounds: Right");
			}
			Right = right;
			if (double.IsInfinity(near))
			{
				throw new Exception("Cannot create a grid with infinity bounds: Near");
			}
			Near = near;
			if (double.IsInfinity(far))
			{
				throw new Exception("Cannot create a grid with infinity bounds: Far");
			}
			Far = far;
		}

		/// <summary>Represents a bounds that is invalid or has not been initialized.</summary>
		internal static readonly QuadTreeBounds Uninitialized = new QuadTreeBounds(double.MaxValue, double.MinValue, double.MaxValue, double.MinValue);
	}
}
