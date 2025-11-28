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
}
