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
		public static bool Equals(Vertex A, Vertex B) {
			if (A.Coordinates.X != B.Coordinates.X | A.Coordinates.Y != B.Coordinates.Y | A.Coordinates.Z != B.Coordinates.Z) return false;
			if (A.TextureCoordinates.X != B.TextureCoordinates.X | A.TextureCoordinates.Y != B.TextureCoordinates.Y) return false;
			return true;
		}

		/// <summary>Tests if two verticies are equal</summary>
		/// <param name="A">The first vertex</param>
		/// <param name="B">The second vertex</param>
		/// <returns>True if they are equal, false otherwise</returns>
		public static bool operator ==(Vertex A, Vertex B) {
			if (A.Coordinates.X != B.Coordinates.X | A.Coordinates.Y != B.Coordinates.Y | A.Coordinates.Z != B.Coordinates.Z) return false;
			if (A.TextureCoordinates.X != B.TextureCoordinates.X | A.TextureCoordinates.Y != B.TextureCoordinates.Y) return false;
			return true;
		}

		/// <summary>Tests if two verticies are not equal</summary>
		/// <param name="A">The first vertex</param>
		/// <param name="B">The second vertex</param>
		/// <returns>True if they are equal, false otherwise</returns>
		public static bool operator !=(Vertex A, Vertex B) {
			if (A.Coordinates.X != B.Coordinates.X | A.Coordinates.Y != B.Coordinates.Y | A.Coordinates.Z != B.Coordinates.Z) return true;
			if (A.TextureCoordinates.X != B.TextureCoordinates.X | A.TextureCoordinates.Y != B.TextureCoordinates.Y) return true;
			return false;
		}

		/// <summary>Gets the hash code</summary>
		/// <returns>The hash code</returns>
		public override int GetHashCode() {
			return base.GetHashCode();
		}

		public override bool Equals(object obj) {
			return base.Equals(obj);
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

		/// <summary>Tests if two verticies are equal</summary>
		/// <param name="A">The first vertex</param>
		/// <param name="B">The second vertex</param>
		/// <returns>True if they are equal, false otherwise</returns>
		public static bool Equals(ColoredVertex A, ColoredVertex B) {
			if (A.Coordinates.X != B.Coordinates.X | A.Coordinates.Y != B.Coordinates.Y | A.Coordinates.Z != B.Coordinates.Z) return false;
			if (A.TextureCoordinates.X != B.TextureCoordinates.X | A.TextureCoordinates.Y != B.TextureCoordinates.Y) return false;
			if (A.Color != B.Color) return false;
			return true;
		}

		/// <summary>Tests if two verticies are equal</summary>
		/// <param name="A">The first vertex</param>
		/// <param name="B">The second vertex</param>
		/// <returns>True if they are equal, false otherwise</returns>
		public static bool operator ==(ColoredVertex A, ColoredVertex B) {
			if (A.Coordinates.X != B.Coordinates.X | A.Coordinates.Y != B.Coordinates.Y | A.Coordinates.Z != B.Coordinates.Z) return false;
			if (A.TextureCoordinates.X != B.TextureCoordinates.X | A.TextureCoordinates.Y != B.TextureCoordinates.Y) return false;
			if (A.Color != B.Color) return false;
			return true;
		}

		/// <summary>Tests if two verticies are not equal</summary>
		/// <param name="A">The first vertex</param>
		/// <param name="B">The second vertex</param>
		/// <returns>True if they are equal, false otherwise</returns>
		public static bool operator !=(ColoredVertex A, ColoredVertex B) {
			if (A.Coordinates.X != B.Coordinates.X | A.Coordinates.Y != B.Coordinates.Y | A.Coordinates.Z != B.Coordinates.Z) return true;
			if (A.TextureCoordinates.X != B.TextureCoordinates.X | A.TextureCoordinates.Y != B.TextureCoordinates.Y) return true;
			if (A.Color != B.Color) return false;
			return false;
		}

		/// <summary>Gets the hash code</summary>
		/// <returns>The hash code</returns>
		public override int GetHashCode() {
			return base.GetHashCode();
		}

		public override bool Equals(object obj) {
			return base.Equals(obj);
		}
	}
}
