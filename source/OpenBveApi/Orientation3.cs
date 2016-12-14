namespace OpenBveApi.Math {
	/// <summary>Represents an orientation in three-dimensional space.</summary>
	public struct Orientation3 {
		
		// --- members ---
		
		/// <summary>The vector pointing right.</summary>
		public Vector3 X;
		
		/// <summary>The vector pointing up.</summary>
		public Vector3 Y;
		
		/// <summary>The vector pointing forward.</summary>
		public Vector3 Z;
		
		
		// --- constructors ---
		
		/// <summary>Creates a new orientation in three-dimensional space.</summary>
		/// <param name="x">The vector pointing right.</param>
		/// <param name="y">The vector pointing up.</param>
		/// <param name="z">The vector pointing forward.</param>
		public Orientation3(Vector3 x, Vector3 y, Vector3 z) {
			this.X = x;
			this.Y = y;
			this.Z = z;
		}
		
		
		// --- read-only fields ---
		
		/// <summary>Represents a null orientation.</summary>
		public static readonly Orientation3 Null = new Orientation3(Vector3.Null, Vector3.Null, Vector3.Null);
		
		/// <summary>Represents the default orientation with X = {1, 0, 0}, Y = {0, 1, 0} and Z = {0, 0, 1}.</summary>
		public static readonly Orientation3 Default = new Orientation3(Vector3.Right, Vector3.Up, Vector3.Forward);
		
	}
}