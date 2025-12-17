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

using OpenBveApi.Math;

namespace OpenBveApi.Objects
{
	/// <summary>A plain vertex</summary>
	public class Vertex : VertexTemplate
	{
		/// <summary>Creates a new vertex</summary>
		public Vertex()
		{
			Coordinates = new Vector3();
			TextureCoordinates = new Vector2();
		}

		/// <summary>Creates a new vertex</summary>
		/// <param name="x">The x-coordinate</param>
		/// <param name="y">The y-coordinate</param>
		/// <param name="z">The z-coordinate</param>
		public Vertex(double x, double y, double z)
		{
			Coordinates = new Vector3(x, y, z);
			TextureCoordinates = new Vector2(0.0f, 0.0f);
		}

		/// <summary>Creates a new vertex</summary>
		/// <param name="v">A Vector3 containing the coordinates</param>
		public Vertex(Vector3 v)
		{
			Coordinates = v;
			TextureCoordinates = new Vector2(0.0f, 0.0f);
		}

		/// <summary>Creates a new vertex</summary>
		/// <param name="coordinates">A Vector3 containing the coordinates</param>
		/// <param name="textureCoordinates">A Vector2 containing the texture coordinates</param>
		public Vertex(Vector3 coordinates, Vector2 textureCoordinates)
		{
			Coordinates = coordinates;
			TextureCoordinates = textureCoordinates;
		}

		/// <summary>Clones a vertex</summary>
		public Vertex(Vertex v)
		{
			Coordinates = v.Coordinates;
			TextureCoordinates = v.TextureCoordinates;
		}

		/// <summary>Tests if two verticies are equal</summary>
		/// <param name="a">The first vertex</param>
		/// <param name="b">The second vertex</param>
		/// <returns>True if they are equal, false otherwise</returns>
		public static bool Equals(Vertex a, Vertex b)
		{
			return a.Equals(b);
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
}
