using OpenBveApi.Colors;
using OpenBveApi.Math;

namespace OpenBveApi.Objects
{
	/// <summary>Holds the template for the base abstract vertex</summary>
	public abstract class VertexTemplate
	{
		/// <summary>The vertex's co-ordinates in 3D space</summary>
		public Vector3 Coordinates;
		/// <summary>The texture co-ordinates for the vertex</summary>
		public Vector2 TextureCoordinates;

		/// <summary>Tests if this vertex template is equal to the supplied object</summary>
		/// <param name="obj">The supplied object</param>
		/// <returns>Trye if they are equal, false otherwise</returns>
		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}

			if (obj.GetType() != GetType())
			{
				return false;
			}

			return obj.Equals(this);
		}


		/// <summary>Tests if two verticies are equal</summary>
		/// <param name="A">The first vertex</param>
		/// <param name="B">The second vertex</param>
		/// <returns>True if they are equal, false otherwise</returns>
		public static bool operator ==(VertexTemplate A, VertexTemplate B)
		{
			if ((object)A != null)
			{
				return A.Equals(B);
			}
			if ((object)B != null)
			{
				return false;
			}
			return true;
		}

		/// <summary>Tests if two verticies are not equal</summary>
		/// <param name="A">The first vertex</param>
		/// <param name="B">The second vertex</param>
		/// <returns>True if they are equal, false otherwise</returns>
		public static bool operator !=(VertexTemplate A, VertexTemplate B)
		{
			if ((object)A != null)
			{
				return !A.Equals(B);
			}
			if ((object)B != null)
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Returns the hashcode for this instance.
		/// </summary>
		/// <returns>A System.Int32 containing the unique hashcode for this instance.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				// ReSharper disable NonReadonlyMemberInGetHashCode
				var hashCode = Coordinates.X.GetHashCode();
				hashCode = (hashCode * 397) ^ Coordinates.Y.GetHashCode();
				hashCode = (hashCode * 397) ^ Coordinates.Z.GetHashCode();
				hashCode = (hashCode * 397) ^ TextureCoordinates.X.GetHashCode();
				hashCode = (hashCode * 397) ^ TextureCoordinates.Y.GetHashCode();
				return hashCode;
			}
		}
	}

	/// <summary>A plain vertex</summary>
	public class Vertex : VertexTemplate
	{
		/// <summary>Creates a new vertex</summary>
		public Vertex()
		{
			this.Coordinates = new Vector3();
			this.TextureCoordinates = new Vector2();
		}

		/// <summary>Creates a new vertex</summary>
		/// <param name="X">The x-coordinate</param>
		/// <param name="Y">The y-coordinate</param>
		/// <param name="Z">The z-coordinate</param>
		public Vertex(double X, double Y, double Z) {
			this.Coordinates = new Vector3(X, Y, Z);
			this.TextureCoordinates = new Vector2(0.0f, 0.0f);
		}

		/// <summary>Creates a new vertex</summary>
		/// <param name="v">A Vector3 containing the coordinates</param>
		public Vertex(Vector3 v)
		{
			this.Coordinates = v;
			this.TextureCoordinates = new Vector2(0.0f, 0.0f);
		}

		/// <summary>Creates a new vertex</summary>
		/// <param name="Coordinates">A Vector3 containing the coordinates</param>
		/// <param name="TextureCoordinates">A Vector2 containing the texture coordinates</param>
		public Vertex(Vector3 Coordinates, Vector2 TextureCoordinates) {
			this.Coordinates = Coordinates;
			this.TextureCoordinates = TextureCoordinates;
		}

		/// <summary>Clones a vertex</summary>
		public Vertex(Vertex v)
		{
			this.Coordinates = v.Coordinates;
			this.TextureCoordinates = v.TextureCoordinates;
		}

		/// <summary>Tests if two verticies are equal</summary>
		/// <param name="A">The first vertex</param>
		/// <param name="B">The second vertex</param>
		/// <returns>True if they are equal, false otherwise</returns>
		public static bool Equals(Vertex A, Vertex B)
		{
			return A.Equals(B);
		}

		/// <summary>Tests if this vertex is equal to the supplied object</summary>
		/// <param name="obj">The supplied object</param>
		/// <returns>Trye if they are equal, false otherwise</returns>
		public override bool Equals(object obj) {
			if (obj == null)
			{
				return false;
			}

			Vertex v = obj as Vertex;
			if (v == null) return false;
			if (v.Coordinates.X != Coordinates.X | v.Coordinates.Y != Coordinates.Y | v.Coordinates.Z != Coordinates.Z) return false;
			if (v.TextureCoordinates.X != TextureCoordinates.X | v.TextureCoordinates.Y != TextureCoordinates.Y) return false;
			return true;
		}

		/// <summary>
		/// Returns the hashcode for this instance.
		/// </summary>
		/// <returns>A System.Int32 containing the unique hashcode for this instance.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				// ReSharper disable NonReadonlyMemberInGetHashCode
				var hashCode = Coordinates.X.GetHashCode();
				hashCode = (hashCode * 397) ^ Coordinates.Y.GetHashCode();
				hashCode = (hashCode * 397) ^ Coordinates.Z.GetHashCode();
				hashCode = (hashCode * 397) ^ TextureCoordinates.X.GetHashCode();
				hashCode = (hashCode * 397) ^ TextureCoordinates.Y.GetHashCode();
				return hashCode;
			}
		}
	}

	/// <summary>Represents a vertex with an associated color (Applied via GL.Color3 when the vertex is painted)</summary>
	public class ColoredVertex : VertexTemplate
	{
		/// <summary>The color of this vertex</summary>
		public Color128 Color;

		/// <summary>Creates a new vertex</summary>
		/// <param name="X">The x-coordinate</param>
		/// <param name="Y">The y-coordinate</param>
		/// <param name="Z">The z-coordinate</param>
		/// <param name="c">The color for the vertex</param>
		public ColoredVertex(double X, double Y, double Z, Color128 c) {
			this.Coordinates = new Vector3(X, Y, Z);
			this.TextureCoordinates = new Vector2(0.0f, 0.0f);
			this.Color = c;
		}

		/// <summary>Creates a new vertex</summary>
		/// <param name="Coordinates">A Vector3 containing the coordinates</param>
		/// <param name="TextureCoordinates">A Vector2 containing the texture coordinates</param>
		/// <param name="color">The color for the vertex</param>
		public ColoredVertex(Vector3 Coordinates, Vector2 TextureCoordinates, Color128 color) {
			this.Coordinates = Coordinates;
			this.TextureCoordinates = TextureCoordinates;
			this.Color = color;
		}

		/// <summary>Creates a colored vertex from a base vertex and a color</summary>
		/// <param name="v">The base vertex</param>
		/// <param name="c">The color</param>
		public ColoredVertex(Vertex v, Color128 c)
		{
			this.Coordinates = v.Coordinates;
			this.TextureCoordinates = v.TextureCoordinates;
			this.Color = c;
		}

		/// <summary>Clones a colored vertex</summary>
		public ColoredVertex(ColoredVertex v)
		{
			this.Coordinates = v.Coordinates;
			this.TextureCoordinates = v.TextureCoordinates;
			this.Color = v.Color;
		}

		/// <summary>Tests if this vertex is equal to the supplied object</summary>
		/// <param name="obj">The supplied object</param>
		/// <returns>Trye if they are equal, false otherwise</returns>
		public override bool Equals(object obj) {
			if (obj == null)
			{
				return false;
			}

			ColoredVertex v = obj as ColoredVertex;
			if (v == null) return false;
			if (v.Coordinates.X != Coordinates.X | v.Coordinates.Y != Coordinates.Y | v.Coordinates.Z != Coordinates.Z) return false;
			if (v.TextureCoordinates.X != TextureCoordinates.X | v.TextureCoordinates.Y != TextureCoordinates.Y) return false;
			if (v.Color != Color) return false;
			return true;
		}

		/// <summary>
		/// Returns the hashcode for this instance.
		/// </summary>
		/// <returns>A System.Int32 containing the unique hashcode for this instance.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				// ReSharper disable NonReadonlyMemberInGetHashCode
				var hashCode = Coordinates.X.GetHashCode();
				hashCode = (hashCode * 397) ^ Coordinates.Y.GetHashCode();
				hashCode = (hashCode * 397) ^ Coordinates.Z.GetHashCode();
				hashCode = (hashCode * 397) ^ TextureCoordinates.X.GetHashCode();
				hashCode = (hashCode * 397) ^ TextureCoordinates.Y.GetHashCode();
				hashCode = (hashCode * 397) ^ Color.R.GetHashCode();
				hashCode = (hashCode * 397) ^ Color.G.GetHashCode();
				hashCode = (hashCode * 397) ^ Color.B.GetHashCode();
				hashCode = (hashCode * 397) ^ Color.A.GetHashCode();
				return hashCode;
			}
		}
	}
}
