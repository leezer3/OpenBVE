//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2025, Christopher Lees, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using OpenBveApi.Colors;
using OpenBveApi.Math;

namespace OpenBveApi.Objects
{
	/// <summary>Represents a vertex with an associated color</summary>
	/// <remarks>Color is applied via GL.Color3 when the vertex is painted</remarks>
	public class ColoredVertex : VertexTemplate
	{
		/// <summary>The color of this vertex</summary>
		public Color128 Color;

		/// <summary>Creates a new vertex</summary>
		/// <param name="x">The x-coordinate</param>
		/// <param name="y">The y-coordinate</param>
		/// <param name="z">The z-coordinate</param>
		/// <param name="c">The color for the vertex</param>
		public ColoredVertex(double x, double y, double z, Color128 c)
		{
			Coordinates = new Vector3(x, y, z);
			TextureCoordinates = new Vector2(0.0f, 0.0f);
			Color = c;
		}

		/// <summary>Creates a new vertex</summary>
		/// <param name="coordinates">A Vector3 containing the coordinates</param>
		/// <param name="textureCoordinates">A Vector2 containing the texture coordinates</param>
		/// <param name="color">The color for the vertex</param>
		public ColoredVertex(Vector3 coordinates, Vector2 textureCoordinates, Color128 color)
		{
			Coordinates = coordinates;
			TextureCoordinates = textureCoordinates;
			Color = color;
		}

		/// <summary>Creates a colored vertex from a base vertex and a color</summary>
		/// <param name="v">The base vertex</param>
		/// <param name="c">The color</param>
		public ColoredVertex(Vertex v, Color128 c)
		{
			Coordinates = v.Coordinates;
			TextureCoordinates = v.TextureCoordinates;
			Color = c;
		}

		/// <summary>Clones a colored vertex</summary>
		public ColoredVertex(ColoredVertex v)
		{
			Coordinates = v.Coordinates;
			TextureCoordinates = v.TextureCoordinates;
			Color = v.Color;
		}

		/// <summary>Tests if this vertex is equal to the supplied object</summary>
		/// <param name="obj">The supplied object</param>
		/// <returns>Trye if they are equal, false otherwise</returns>
		public override bool Equals(object obj)
		{
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

		/// <summary>Returns the hashcode for this instance.</summary>
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
