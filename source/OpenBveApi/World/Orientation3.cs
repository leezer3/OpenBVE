namespace OpenBveApi.Math
{
	/// <summary>Represents an orientation in three-dimensional space.</summary>
	public struct Orientation3
	{
		/// <summary>The vector pointing right.</summary>
		public Vector3 X;
		/// <summary>The vector pointing up.</summary>
		public Vector3 Y;
		/// <summary>The vector pointing forward.</summary>
		public Vector3 Z;

		/// <summary>Represents a null orientation.</summary>
		public static readonly Orientation3 Null = new Orientation3(Vector3.Zero, Vector3.Zero, Vector3.Zero);
		/// <summary>Represents the default orientation with X = {1, 0, 0}, Y = {0, 1, 0} and Z = {0, 0, 1}.</summary>
		public static readonly Orientation3 Default = new Orientation3(Vector3.Right, Vector3.Up, Vector3.Forward);

		/// <summary>Creates a new orientation in three-dimensional space.</summary>
		/// <param name="x">The vector pointing right.</param>
		/// <param name="y">The vector pointing up.</param>
		/// <param name="z">The vector pointing forward.</param>
		public Orientation3(Vector3 x, Vector3 y, Vector3 z)
		{
			this.X = x;
			this.Y = y;
			this.Z = z;
		}
		
		/// <summary>Returns the hash-code for this instance</summary>
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = X.GetHashCode();
				hashCode = (hashCode * 397) ^ Y.GetHashCode();
				hashCode = (hashCode * 397) ^ Z.GetHashCode();
				return hashCode;
			}
		}

		/// <summary>Tests whether this Orientation3 is equal to a second Orientation3</summary>
		public bool Equals(Orientation3 secondOrientation)
		{
			return X == secondOrientation.X &&
			       Y == secondOrientation.Y &&
			       Z == secondOrientation.Z;
		}

		/// <summary>Tests whether the given object is equal to this Orientation3</summary>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Orientation3 && Equals((Orientation3) obj);
		}

		/// <summary>Tests whether two Orientation3 instances are equal</summary>
		public static bool operator ==(Orientation3 firstOrientation, Orientation3 secondOrientation)
		{
			return firstOrientation.Equals(secondOrientation);
		}

		/// <summary>Tests whether two Orientation3 instances are NOT equal</summary>
		public static bool operator !=(Orientation3 firstOrientation, Orientation3 secondOrientation)
		{
			return !firstOrientation.Equals(secondOrientation);
		}
	}
}
