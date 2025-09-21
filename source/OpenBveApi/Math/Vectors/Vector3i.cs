namespace OpenBveApi.Math
{
	/// <summary>Represents a three-dimensional vector.</summary>
	public struct Vector3i
    {
	    /// <summary>The x-coordinate.</summary>
	    public int X;

	    /// <summary>The y-coordinate.</summary>
	    public int Y;

	    /// <summary>The z-coordinate.</summary>
	    public int Z;

	    /// <summary>Creates a new three-dimensional vector.</summary>
	    /// <param name="x">The x-coordinate.</param>
	    /// <param name="y">The y-coordinate.</param>
	    /// <param name="z">The z-coordinate.</param>
	    public Vector3i(int x, int y, int z)
	    {
		    this.X = x;
		    this.Y = y;
		    this.Z = z;
	    }

	    /// <summary>Represents a null vector.</summary>
		public static readonly Vector3i Zero = new Vector3i(0, 0, 0);
	}
}
