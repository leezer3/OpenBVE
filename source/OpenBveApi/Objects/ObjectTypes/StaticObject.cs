using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using OpenBveApi.Colors;
using OpenBveApi.Hosts;
using OpenBveApi.Math;
using OpenBveApi.World;

namespace OpenBveApi.Objects
{
	// <summary>Represents a static (e.g. non-animated) object within the world</summary>
	/// <inheritdoc />
	public class StaticObject : UnifiedObject
	{
		/// <summary>Whether the object is optimized</summary>
		private bool isOptimized;
		/// <summary>The mesh of the object</summary>
		public Mesh Mesh;
		/// <summary>The starting track position, for static objects only.</summary>
		public float StartingTrackDistance;
		/// <summary>The ending track position, for static objects only.</summary>
		public float EndingTrackDistance;
		/// <summary>Whether the object is dynamic, i.e. not static.</summary>
		public bool Dynamic;
		/// <summary> Stores the author for this object.</summary>
		public string Author;
		/// <summary> Stores the copyright information for this object.</summary>
		public string Copyright;

		private readonly HostInterface currentHost;

		/// <summary>Creates a new empty object</summary>
		public StaticObject(HostInterface host)
		{
			currentHost = host;
			Mesh = new Mesh();
		}

		/// <summary>Creates a clone of this object.</summary>
		/// <param name="daytimeTexture">The replacement daytime texture</param>
		/// <param name="nighttimeTexture">The replacement nighttime texture</param>
		/// <returns></returns>
		public StaticObject Clone(Textures.Texture daytimeTexture, Textures.Texture nighttimeTexture) //Prefix is required or else MCS barfs
		{
			StaticObject cloneResult = new StaticObject(currentHost)
			{
				StartingTrackDistance = StartingTrackDistance,
				EndingTrackDistance = EndingTrackDistance,
				Dynamic = Dynamic,
				Mesh = {Vertices = new VertexTemplate[Mesh.Vertices.Length]},
				isOptimized = isOptimized
			};
			// vertices
			for (int j = 0; j < Mesh.Vertices.Length; j++)
			{
				cloneResult.Mesh.Vertices[j] = Mesh.Vertices[j].Clone();
			}

			// faces
			cloneResult.Mesh.Faces = new MeshFace[Mesh.Faces.Length];
			for (int j = 0; j < Mesh.Faces.Length; j++)
			{
				cloneResult.Mesh.Faces[j].Flags = Mesh.Faces[j].Flags;
				cloneResult.Mesh.Faces[j].Material = Mesh.Faces[j].Material;
				cloneResult.Mesh.Faces[j].Vertices = new MeshFaceVertex[Mesh.Faces[j].Vertices.Length];
				for (int k = 0; k < Mesh.Faces[j].Vertices.Length; k++)
				{
					cloneResult.Mesh.Faces[j].Vertices[k] = Mesh.Faces[j].Vertices[k];
				}
			}

			// materials
			cloneResult.Mesh.Materials = new MeshMaterial[Mesh.Materials.Length];
			for (int j = 0; j < Mesh.Materials.Length; j++)
			{
				cloneResult.Mesh.Materials[j] = Mesh.Materials[j];
				cloneResult.Mesh.Materials[j].DaytimeTexture = daytimeTexture ?? Mesh.Materials[j].DaytimeTexture;
				cloneResult.Mesh.Materials[j].NighttimeTexture = nighttimeTexture ?? Mesh.Materials[j].NighttimeTexture;
			}

			return cloneResult;
		}

		/// <summary>Creates a clone of this object.</summary>
		public override UnifiedObject Clone()
		{
			StaticObject cloneResult = new StaticObject(currentHost)
			{
				StartingTrackDistance = StartingTrackDistance,
				EndingTrackDistance = EndingTrackDistance,
				Dynamic = Dynamic,
				Mesh = {Vertices = new VertexTemplate[Mesh.Vertices.Length]},
				isOptimized = isOptimized
			};
			// vertices
			for (int j = 0; j < Mesh.Vertices.Length; j++)
			{
				cloneResult.Mesh.Vertices[j] = Mesh.Vertices[j].Clone();
			}

			// faces
			cloneResult.Mesh.Faces = new MeshFace[Mesh.Faces.Length];
			for (int j = 0; j < Mesh.Faces.Length; j++)
			{
				cloneResult.Mesh.Faces[j].Flags = Mesh.Faces[j].Flags;
				cloneResult.Mesh.Faces[j].Material = Mesh.Faces[j].Material;
				cloneResult.Mesh.Faces[j].Vertices = new MeshFaceVertex[Mesh.Faces[j].Vertices.Length];
				for (int k = 0; k < Mesh.Faces[j].Vertices.Length; k++)
				{
					cloneResult.Mesh.Faces[j].Vertices[k] = Mesh.Faces[j].Vertices[k];
				}
			}

			// materials
			cloneResult.Mesh.Materials = new MeshMaterial[Mesh.Materials.Length];
			for (int j = 0; j < Mesh.Materials.Length; j++)
			{
				cloneResult.Mesh.Materials[j] = Mesh.Materials[j];
			}

			return cloneResult;
		}

		/// <summary>Creates a mirrored clone of this object</summary>
		public override UnifiedObject Mirror()
		{
			StaticObject mirrorResult = (StaticObject)Clone();
			for (int i = 0; i < mirrorResult.Mesh.Vertices.Length; i++)
			{
				mirrorResult.Mesh.Vertices[i].Coordinates.X = -mirrorResult.Mesh.Vertices[i].Coordinates.X;
			}
			for (int i = 0; i < mirrorResult.Mesh.Faces.Length; i++)
			{
				for (int k = 0; k < mirrorResult.Mesh.Faces[i].Vertices.Length; k++)
				{
					mirrorResult.Mesh.Faces[i].Vertices[k].Normal.X = -mirrorResult.Mesh.Faces[i].Vertices[k].Normal.X;
				}
				mirrorResult.Mesh.Faces[i].Flip();
			}
			mirrorResult.isOptimized = isOptimized;
			return mirrorResult;
		}

		/// <inheritdoc/>
		public override UnifiedObject Transform(double nearDistance, double farDistance)
		{
			/* ** ORIGINAL ALGORITHM**
			 *
			 * A brief description on how this works:
			 *
			 * Objects are implicitly assumed to be left or right handed.
			 * They must follow the following vertex windings, having a total of 4 or 8 vertices:
			 *
			 * LEFT-HANDED
			 * ============
			 *
			 * TopLeft, BottomLeft, BottomRight, TopRight
			 *
			 * RIGHT-HANDED
			 * ============
			 *
			 * BottomRight, TopRight, TopLeft, BottomLeft
			 *
			 * We then go through the vertex list, and our first two vertices in each are transformed.
			 * The *new* position is now the corresponding X of the other vertex MINUS the distance.
			 *
			 * NOTES:
			 * This algorithm is totally broken for anything other than objects containing 4 / 8 vertices
			 * If our vertex windings do not conform, it's also broken.
			 *
			 */
			StaticObject transformResult = (StaticObject)this.Clone();
			int n = 0;
			double x2 = 0.0, x3 = 0.0, x6 = 0.0, x7 = 0.0;
			for (int i = 0; i < transformResult.Mesh.Vertices.Length; i++)
			{
				if (n == 2)
				{
					x2 = transformResult.Mesh.Vertices[i].Coordinates.X;
				}
				else if (n == 3)
				{
					x3 = transformResult.Mesh.Vertices[i].Coordinates.X;
				}
				else if (n == 6)
				{
					x6 = transformResult.Mesh.Vertices[i].Coordinates.X;
				}
				else if (n == 7)
				{
					x7 = transformResult.Mesh.Vertices[i].Coordinates.X;
				}
				n++;
				if (n == 8)
				{
					break;
				}
			}
			if (n >= 4)
			{
				int m = 0;
				for (int i = 0; i < transformResult.Mesh.Vertices.Length; i++)
				{
					if (m == 0)
					{
						transformResult.Mesh.Vertices[i].Coordinates.X = nearDistance - x3;
					}
					else if (m == 1)
					{
						transformResult.Mesh.Vertices[i].Coordinates.X = farDistance - x2;
						if (n < 8)
						{
							break;
						}
					}
					else if (m == 4)
					{
						transformResult.Mesh.Vertices[i].Coordinates.X = nearDistance - x7;
					}
					else if (m == 5)
					{
						transformResult.Mesh.Vertices[i].Coordinates.X = farDistance - x6;
						break;
					}
					m++;
					if (m == 8)
					{
						break;
					}
				}
			}
			return transformResult;
		}

		/// <inheritdoc/>
		public override UnifiedObject TransformLeft(double nearDistance, double farDistance)
		{
			/*
			 * **NEW ALGORITHM**
			 *
			 * This works *better* than the original algorithm, but is still not happy with objects
			 * not conforming to 4-vertex faces.
			 *
			 * To be improved.....
			 */

			bool vertical = true;
			double zPos = Mesh.Vertices[0].Coordinates.Z;
			double minX = double.MaxValue, maxX = double.MinValue;
			for (int i = 0; i < Mesh.Vertices.Length; i++)
			{
				minX = System.Math.Min(Mesh.Vertices[i].Coordinates.X, minX);
				maxX = System.Math.Min(Mesh.Vertices[i].Coordinates.X, maxX);
				if (System.Math.Abs(Mesh.Vertices[i].Coordinates.Z - zPos) > 0.1)
				{
					vertical = false;
				}
			}

			StaticObject transformResult = (StaticObject)Clone();

			if (vertical || System.Math.Abs(nearDistance - farDistance) > 0.1)
			{
				// If vertical, or both distances are within 0.1m use scale instead (this works for all object types)
				double width = maxX - minX;
				transformResult.ApplyScale(width / (nearDistance + width), 1,1);
				return transformResult;
			}


			for (int i = 0; i < Mesh.Vertices.Length; i += 4)
			{
				List<VertexTemplate> tempList = Mesh.Vertices.Skip(i).Take(4).ToList();
				// find vertices to base transform on
				int bottomLeft = tempList.IndexOf(tempList.OrderByDescending(c => c.Coordinates.Z).ThenBy(c => c.Coordinates.X).First());
				int bottomRight = tempList.IndexOf(tempList.OrderByDescending(c => c.Coordinates.Z).ThenByDescending(c => c.Coordinates.X).First());
				int topRight = tempList.IndexOf(tempList.OrderBy(c => c.Coordinates.Z).ThenByDescending(c => c.Coordinates.X).First());
				int topLeft = tempList.IndexOf(tempList.OrderBy(c => c.Coordinates.Z).ThenBy(c => c.Coordinates.X).First());

				// for a left-handed transform, we need to transform the right-side coords
				transformResult.Mesh.Vertices[i + bottomRight].Coordinates.X = farDistance - transformResult.Mesh.Vertices[i + bottomLeft].Coordinates.X;
				transformResult.Mesh.Vertices[i + topRight].Coordinates.X = nearDistance - transformResult.Mesh.Vertices[i + topLeft].Coordinates.X;
			}

			return transformResult;
		}

		/// <inheritdoc/>
		public override UnifiedObject TransformRight(double nearDistance, double farDistance)
		{
			bool vertical = true;
			double zPos = Mesh.Vertices[0].Coordinates.Z;
			double minX = double.MaxValue, maxX = double.MinValue;
			for (int i = 0; i < Mesh.Vertices.Length; i++)
			{
				minX = System.Math.Min(Mesh.Vertices[i].Coordinates.X, minX);
				maxX = System.Math.Min(Mesh.Vertices[i].Coordinates.X, maxX);
				if (System.Math.Abs(Mesh.Vertices[i].Coordinates.Z - zPos) > 0.1)
				{
					vertical = false;
				}
			}

			StaticObject transformResult = (StaticObject)Clone();

			if (vertical || System.Math.Abs(nearDistance - farDistance) > 0.1)
			{
				double width = maxX - minX;
				transformResult.ApplyScale(width / (nearDistance + width), 1, 1);
				return transformResult;
			}

			for (int i = 0; i < Mesh.Vertices.Length; i += 4)
			{
				List<VertexTemplate> tempList = Mesh.Vertices.Skip(i).Take(4).ToList();
				// find vertices to base transform on
				int bottomLeft = tempList.IndexOf(tempList.OrderByDescending(c => c.Coordinates.Z).ThenBy(c => c.Coordinates.X).First());
				int bottomRight = tempList.IndexOf(tempList.OrderByDescending(c => c.Coordinates.Z).ThenByDescending(c => c.Coordinates.X).First());
				int topRight = tempList.IndexOf(tempList.OrderBy(c => c.Coordinates.Z).ThenByDescending(c => c.Coordinates.X).First());
				int topLeft = tempList.IndexOf(tempList.OrderBy(c => c.Coordinates.Z).ThenBy(c => c.Coordinates.X).First());

				// for a right-handed transform, we need to transform the left-side coords
				transformResult.Mesh.Vertices[i + bottomLeft].Coordinates.X = farDistance - transformResult.Mesh.Vertices[i + bottomRight].Coordinates.X;
				transformResult.Mesh.Vertices[i + topLeft].Coordinates.X = nearDistance - transformResult.Mesh.Vertices[i + topRight].Coordinates.X;
			}

			return transformResult;
		}

		/// <summary>Joins two static objects</summary>
		/// <param name="additionalObject">The static object to join</param>
		/// <param name="animationMatrices">The animation matrices for the object</param>
		public void JoinObjects(StaticObject additionalObject, Matrix4D[] animationMatrices = null)
		{
			if (additionalObject == null)
			{
				return;
			}

			int mf = Mesh.Faces.Length;
			int mm = Mesh.Materials.Length;
			int mv = Mesh.Vertices.Length;
			Array.Resize(ref Mesh.Faces, mf + additionalObject.Mesh.Faces.Length);
			Array.Resize(ref Mesh.Materials, mm + additionalObject.Mesh.Materials.Length);
			Array.Resize(ref Mesh.Vertices, mv + additionalObject.Mesh.Vertices.Length);
			for (int i = 0; i < additionalObject.Mesh.Faces.Length; i++)
			{
				Mesh.Faces[mf + i] = additionalObject.Mesh.Faces[i];
				for (int j = 0; j < Mesh.Faces[mf + i].Vertices.Length; j++)
				{
					Mesh.Faces[mf + i].Vertices[j].Index += mv;
				}

				Mesh.Faces[mf + i].Material += (ushort) mm;
			}

			for (int i = 0; i < additionalObject.Mesh.Materials.Length; i++)
			{
				Mesh.Materials[mm + i] = additionalObject.Mesh.Materials[i];
			}

			for (int i = 0; i < additionalObject.Mesh.Vertices.Length; i++)
			{
				if (additionalObject.Mesh.Vertices[i] is AnimatedVertex av)
				{
					Vector3 transformedCoordinates = new Vector3(av.Coordinates);
					for (int j = 0; j < av.MatrixChain.Length; j++)
					{
						if (animationMatrices != null && av.MatrixChain[j] >= 0 && av.MatrixChain[j] < 255)
						{
							transformedCoordinates.Transform(animationMatrices[av.MatrixChain[j]], false); // use the static matrix, not the animated one
						}
					}
					Mesh.Vertices[mv + i] = new Vertex(transformedCoordinates, av.TextureCoordinates);
				}
				else
				{
					Mesh.Vertices[mv + i] = additionalObject.Mesh.Vertices[i].Clone();
				}

			}
		}

		/// <summary>Applies scale</summary>
		public void ApplyScale(Vector3 scale)
		{
			ApplyScale(scale.X, scale.Y, scale.Z);
		}

		/// <summary>Applies scale</summary>
		public void ApplyScale(double x, double y, double z)
		{
			float rx = (float) (1.0 / x);
			float ry = (float) (1.0 / y);
			float rz = (float) (1.0 / z);
			float rx2 = rx * rx;
			float ry2 = ry * ry;
			float rz2 = rz * rz;
			bool reverse = x * y * z < 0.0;
			for (int j = 0; j < Mesh.Vertices.Length; j++)
			{
				Mesh.Vertices[j].Coordinates.X *= x;
				Mesh.Vertices[j].Coordinates.Y *= y;
				Mesh.Vertices[j].Coordinates.Z *= z;
			}

			for (int j = 0; j < Mesh.Faces.Length; j++)
			{
				for (int k = 0; k < Mesh.Faces[j].Vertices.Length; k++)
				{
					double nx2 = Mesh.Faces[j].Vertices[k].Normal.X * Mesh.Faces[j].Vertices[k].Normal.X;
					double ny2 = Mesh.Faces[j].Vertices[k].Normal.Y * Mesh.Faces[j].Vertices[k].Normal.Y;
					double nz2 = Mesh.Faces[j].Vertices[k].Normal.Z * Mesh.Faces[j].Vertices[k].Normal.Z;
					double u = nx2 * rx2 + ny2 * ry2 + nz2 * rz2;
					if (u != 0.0)
					{
						u = (float) System.Math.Sqrt((nx2 + ny2 + nz2) / u);
						Mesh.Faces[j].Vertices[k].Normal.X *= rx * u;
						Mesh.Faces[j].Vertices[k].Normal.Y *= ry * u;
						Mesh.Faces[j].Vertices[k].Normal.Z *= rz * u;
					}
				}
			}

			if (reverse)
			{
				for (int j = 0; j < Mesh.Faces.Length; j++)
				{
					Mesh.Faces[j].Flip();
				}
			}
		}

		/// <summary>Applies rotation</summary>
		/// <param name="rotationVector">The rotation vector</param>
		/// <param name="angle">The angle to rotate in degrees</param>
		public void ApplyRotation(Vector3 rotationVector, double angle)
		{
			for (int j = 0; j < Mesh.Vertices.Length; j++)
			{
				Mesh.Vertices[j].Coordinates.Rotate(rotationVector, angle);

			}

			for (int j = 0; j < Mesh.Faces.Length; j++)
			{
				for (int k = 0; k < Mesh.Faces[j].Vertices.Length; k++)
				{
					Mesh.Faces[j].Vertices[k].Normal.Rotate(rotationVector, angle);
				}
			}
		}
		
		/// <summary>Applies translation</summary>
		public override void ApplyTranslation(double x, double y, double z, bool absoluteTranslation = false)
		{
			for (int i = 0; i < Mesh.Vertices.Length; i++)
			{
				Mesh.Vertices[i].Coordinates.X += x;
				Mesh.Vertices[i].Coordinates.Y += y;
				Mesh.Vertices[i].Coordinates.Z += z;
			}
		}

		/// <summary>Applies mirroring</summary>
		/// <param name="vX">Whether to mirror vertices in the X-axis</param>
		/// <param name="vY">Whether to mirror vertices in the Y-axis</param>
		/// <param name="vZ">Whether to mirror vertices in the Z-axis</param>
		/// <param name="nX">Whether to mirror normals in the X-axis</param>
		/// <param name="nY">Whether to mirror normals in the Y-axis</param>
		/// <param name="nZ">Whether to mirror normals in the Z-axis</param>
		public void ApplyMirror(bool vX, bool vY, bool vZ, bool nX, bool nY, bool nZ)
		{
			for (int i = 0; i < Mesh.Vertices.Length; i++)
			{
				if (vX)
				{
					Mesh.Vertices[i].Coordinates.X *= -1;
				}

				if (vY)
				{
					Mesh.Vertices[i].Coordinates.Y *= -1;
				}

				if (vZ)
				{
					Mesh.Vertices[i].Coordinates.Z *= -1;
				}
			}

			for (int i = 0; i < Mesh.Faces.Length; i++)
			{
				for (int j = 0; j < Mesh.Faces[i].Vertices.Length; j++)
				{
					if (nX)
					{
						Mesh.Faces[i].Vertices[j].Normal.X *= -1;
					}

					if (nY)
					{
						Mesh.Faces[i].Vertices[j].Normal.Y *= -1;
					}

					if (nZ)
					{
						Mesh.Faces[i].Vertices[j].Normal.Z *= -1;
					}
				}
			}

			int numFlips = 0;
			if (vX)
			{
				numFlips++;
			}

			if (vY)
			{
				numFlips++;
			}

			if (vZ)
			{
				numFlips++;
			}

			if (numFlips % 2 != 0)
			{
				for (int i = 0; i < Mesh.Faces.Length; i++)
				{
					Array.Reverse(Mesh.Faces[i].Vertices);
				}
			}
		}

		/// <summary>Applies a color to all materials in the mesh</summary>
		/// <param name="newColor">The color</param>
		/// <param name="emissive">Whether this is an emissive color</param>
		public void ApplyColor(Color32 newColor, bool emissive)
		{
			for (int i = 0; i < Mesh.Materials.Length; i++)
			{
				if (emissive)
				{
					Mesh.Materials[i].EmissiveColor = (Color24) newColor;
					Mesh.Materials[i].Flags |= MaterialFlags.Emissive;
				}
				else
				{
					Mesh.Materials[i].Color = newColor;
				}
			}
		}

		/// <summary>Performs shear mapping for all vertices within the StaticObject</summary>
		/// <param name="shearDirection">A vector describing the direction of the plane to be sheared</param>
		/// <param name="shear">A vector describing the shear direction</param>
		/// <param name="ratio">The amount of shear to apply.</param>
		/// <remarks>If Ratio is 0, no transformation is performed. If Direction and Shear are perpendicular, a Ratio of 1 corresponds to a slope of 45 degrees</remarks>
		public void ApplyShear(Vector3 shearDirection, Vector3 shear, double ratio)
		{
			for (int j = 0; j < Mesh.Vertices.Length; j++)
			{
				double n = ratio * (shearDirection.X * Mesh.Vertices[j].Coordinates.X + shearDirection.Y * Mesh.Vertices[j].Coordinates.Y + shearDirection.Z * Mesh.Vertices[j].Coordinates.Z);
				Mesh.Vertices[j].Coordinates += shear * n;
			}

			for (int j = 0; j < Mesh.Faces.Length; j++)
			{
				for (int k = 0; k < Mesh.Faces[j].Vertices.Length; k++)
				{
					if (Mesh.Faces[j].Vertices[k].Normal.X != 0.0f | Mesh.Faces[j].Vertices[k].Normal.Y != 0.0f | Mesh.Faces[j].Vertices[k].Normal.Z != 0.0f)
					{
						double n = ratio * (shear.X * Mesh.Faces[j].Vertices[k].Normal.X + shear.Y * Mesh.Faces[j].Vertices[k].Normal.Y + shear.Z * Mesh.Faces[j].Vertices[k].Normal.Z);
						Mesh.Faces[j].Vertices[k].Normal -= shearDirection * n;
						Mesh.Faces[j].Vertices[k].Normal.Normalize();
					}
				}
			}
		}

		/// <summary>Callback function to create the object within the world</summary>
		public override void CreateObject(Vector3 position, Transformation worldTransformation, Transformation localTransformation,
			int sectionIndex, double startingDistance, double endingDistance,
			double trackPosition, double brightness, bool duplicateMaterials = false)
		{
			currentHost.CreateStaticObject(this, position, worldTransformation, localTransformation, 0.0, startingDistance, endingDistance, trackPosition);
		}

		/// <inheritdoc />
		public override void OptimizeObject(bool preserveVerticies, int faceThreshold, bool vertexCulling)
		{
			if (isOptimized)
			{
				return;
			}
			isOptimized = true;
			int m = Mesh.Materials.Length;
			int f = Mesh.Faces.Length;
			
			if (m >= f / 500 && f >= faceThreshold && f < 20000 && currentHost.Platform != HostPlatform.AppleOSX)
			{
				/*
				 * HACK:
				 * A forwards compatible GL3 context (required on OS-X) only supports tris
				 * No access to the renderer type here, so let's cheat and assume that OS-X
				 * requires an optimized object (therefore decomposed into tris) in all circumstances
				 *
				 * Also *always* optimise objects with more than 20k faces (some .X as otherwise this kills the renderer)
				 * Further, always try to squash where there are more than 500 times faces than materials (Some X trees killing the renderer)
				 */
				return;
			}

			if (Mesh.Vertices.Length > 10000)
			{
				// Don't attempt to de-duplicate where over 10k vertices
				preserveVerticies = true;
			}

			// eliminate invalid faces and reduce incomplete faces
			for (int i = 0; i < f; i++)
			{
				FaceFlags type = Mesh.Faces[i].Flags & FaceFlags.FaceTypeMask;
				bool keep;
				switch (type)
				{
					case FaceFlags.Triangles:
						keep = Mesh.Faces[i].Vertices.Length >= 3;
						if (keep)
						{
							int n = (Mesh.Faces[i].Vertices.Length / 3) * 3;
							if (Mesh.Faces[i].Vertices.Length != n)
							{
								Array.Resize(ref Mesh.Faces[i].Vertices, n);
							}
						}
						break;
					case FaceFlags.Quads:
						keep = Mesh.Faces[i].Vertices.Length >= 4;
						if (keep)
						{
							int n = Mesh.Faces[i].Vertices.Length & ~3;
							if (Mesh.Faces[i].Vertices.Length != n)
							{
								Array.Resize(ref Mesh.Faces[i].Vertices, n);
							}
						}
						break;
					case FaceFlags.QuadStrip:
						keep = Mesh.Faces[i].Vertices.Length >= 4;
						if (keep)
						{
							int n = Mesh.Faces[i].Vertices.Length & ~1;
							if (Mesh.Faces[i].Vertices.Length != n)
							{
								Array.Resize(ref Mesh.Faces[i].Vertices, n);
							}
						}
						break;
					default:
						keep = Mesh.Faces[i].Vertices.Length >= 3;
						break;
				}

				if (!keep)
				{
					for (int j = i; j < f - 1; j++)
					{
						Mesh.Faces[j] = Mesh.Faces[j + 1];
					}

					f--;
					i--;
				}
			}

			// eliminate unused materials
			bool[] materialUsed = new bool[m];
			for (int i = 0; i < f; i++)
			{
				/*
				if (Mesh.Faces[i].Material < m - 1)
				{
					/*
					 * Our material is out of range
					 * Rather than crashing, add a new blank material
					 
					Array.Resize(ref Mesh.Materials, m + 1);
					Mesh.Materials[m] = new MeshMaterial();
					Mesh.Faces[i].Material = (ushort)m;
					m++;
					Array.Resize(ref materialUsed, m + 1);
				}
			*/
				materialUsed[Mesh.Faces[i].Material] = true;
			}

			for (int i = 0; i < m; i++)
			{
				if (!materialUsed[i])
				{
					for (int j = 0; j < f; j++)
					{
						if (Mesh.Faces[j].Material > i)
						{
							Mesh.Faces[j].Material--;
						}
					}

					for (int j = i; j < m - 1; j++)
					{
						Mesh.Materials[j] = Mesh.Materials[j + 1];
						materialUsed[j] = materialUsed[j + 1];
					}

					m--;
					i--;
				}
			}

			// eliminate duplicate materials
			for (int i = 0; i < m - 1; i++)
			{
				for (int j = i + 1; j < m; j++)
				{
					if (Mesh.Materials[i] == Mesh.Materials[j])
					{
						for (int k = 0; k < f; k++)
						{
							if (Mesh.Faces[k].Material == j)
							{
								Mesh.Faces[k].Material = (ushort) i;
							}
							else if (Mesh.Faces[k].Material > j)
							{
								Mesh.Faces[k].Material--;
							}
						}

						for (int k = j; k < m - 1; k++)
						{
							Mesh.Materials[k] = Mesh.Materials[k + 1];
						}

						m--;
						j--;
					}
				}
			}

			// Cull vertices based on hidden option.
			// This is disabled by default because it adds a lot of time to the loading process.
			if (!preserveVerticies && vertexCulling)
			{
				OrderedDictionary newVertices = new OrderedDictionary();
				for (int i = 0; i < Mesh.Vertices.Length; i++)
				{
					if (!newVertices.Contains(Mesh.Vertices[i]))
					{
						newVertices.Add(Mesh.Vertices[i], newVertices.Count);
					}
				}

				for (int i = 0; i < Mesh.Faces.Length; i++)
				{
					for (int j = 0; j < Mesh.Faces[i].Vertices.Length; j++)
					{
						Mesh.Faces[i].Vertices[j].Index = (int)newVertices[Mesh.Vertices[Mesh.Faces[i].Vertices[j].Index]];
					}
				}

				newVertices.Keys.CopyTo(Mesh.Vertices, 0);
				Array.Resize(ref Mesh.Vertices, newVertices.Count);
			}

			// structure optimization
			// Triangularize all polygons and quads into triangles
			for (int i = 0; i < f; ++i)
			{
				FaceFlags type = Mesh.Faces[i].Flags & FaceFlags.FaceTypeMask;
				// Only transform quads and polygons
				if (type == FaceFlags.Quads || type == FaceFlags.Polygon)
				{
					int startingVertexCount = Mesh.Faces[i].Vertices.Length;

					// One triangle for the first three points, then one for each vertex
					// Wind order is maintained.
					// Ex: 0, 1, 2; 0, 2, 3; 0, 3, 4; 0, 4, 5; 
					int triCount = (startingVertexCount - 2);
					int vertexCount = triCount * 3;

					// Copy old array for use as we work
					MeshFaceVertex[] originalPoly = (MeshFaceVertex[]) Mesh.Faces[i].Vertices.Clone();

					// Resize new array
					Array.Resize(ref Mesh.Faces[i].Vertices, vertexCount);

					// Reference to output vertices
					MeshFaceVertex[] outVerts = Mesh.Faces[i].Vertices;

					// Triangularize
					for (int triIndex = 0, vertIndex = 0, oldVert = 2; triIndex < triCount; ++triIndex, ++oldVert)
					{
						// First vertex is always the 0th
						outVerts[vertIndex] = originalPoly[0];
						vertIndex += 1;

						// Second vertex is one behind the current working vertex
						outVerts[vertIndex] = originalPoly[oldVert - 1];
						vertIndex += 1;

						// Third vertex is current working vertex
						outVerts[vertIndex] = originalPoly[oldVert];
						vertIndex += 1;
					}

					// Mark as triangle
					Mesh.Faces[i].Flags &=  ~FaceFlags.FaceTypeMask;
					Mesh.Faces[i].Flags |= FaceFlags.Triangles;
				}
			}

			// decomposite TRIANGLES and QUADS
			for (int i = 0; i < f; i++)
			{
				FaceFlags type = Mesh.Faces[i].Flags & FaceFlags.FaceTypeMask;
				int faceCount = 0;
				FaceFlags faceBit = 0;
				if (type == FaceFlags.Triangles)
				{
					faceCount = 3;
					faceBit = FaceFlags.Triangles;
				}
				else if (type == FaceFlags.Quads)
				{
					faceCount = 4;
					faceBit = FaceFlags.Triangles;
				}

				if (faceCount == 3 || faceCount == 4)
				{
					if (Mesh.Faces[i].Vertices.Length > faceCount)
					{
						int n = (Mesh.Faces[i].Vertices.Length - faceCount) / faceCount;
						while (f + n > Mesh.Faces.Length)
						{
							Array.Resize(ref Mesh.Faces, Mesh.Faces.Length << 1);
						}

						for (int j = 0; j < n; j++)
						{
							Mesh.Faces[f + j].Vertices = new MeshFaceVertex[faceCount];
							for (int k = 0; k < faceCount; k++)
							{
								Mesh.Faces[f + j].Vertices[k] = Mesh.Faces[i].Vertices[faceCount + faceCount * j + k];
							}

							Mesh.Faces[f + j].Material = Mesh.Faces[i].Material;
							Mesh.Faces[f + j].Flags = Mesh.Faces[i].Flags;
							Mesh.Faces[i].Flags &= ~FaceFlags.FaceTypeMask;
							Mesh.Faces[i].Flags |= faceBit;
						}

						Array.Resize(ref Mesh.Faces[i].Vertices, faceCount);
						f += n;
					}
				}
			}

			// Squish faces that have the same material.
			{
				bool[] canMerge = new bool[f];
				for (int i = 0; i < f - 1; ++i)
				{
					int mergeVertices = 0;

					// Type of current face
					FaceFlags type = Mesh.Faces[i].Flags & FaceFlags.FaceTypeMask;
					FaceFlags face = Mesh.Faces[i].Flags & FaceFlags.Face2Mask;

					// Find faces that can be merged
					for (int j = i + 1; j < f; ++j)
					{
						FaceFlags type2 = Mesh.Faces[j].Flags & FaceFlags.FaceTypeMask;
						FaceFlags face2 = Mesh.Faces[j].Flags & FaceFlags.Face2Mask;

						// Conditions for face merger
						bool mergeable = (type == FaceFlags.Triangles) &&
						                 (type == type2) &&
						                 (face == face2) &&
						                 (Mesh.Faces[i].Material == Mesh.Faces[j].Material);

						canMerge[j] = mergeable;
						mergeVertices += mergeable ? Mesh.Faces[j].Vertices.Length : 0;
					}

					if (mergeVertices == 0)
					{
						continue;
					}

					// Current end of array index
					int lastVertexIt = Mesh.Faces[i].Vertices.Length;

					// Resize current face's vertices to have enough room
					Array.Resize(ref Mesh.Faces[i].Vertices, lastVertexIt + mergeVertices);

					// Merge faces
					for (int j = i + 1; j < f; ++j)
					{
						if (canMerge[j])
						{
							// Copy vertices
							Mesh.Faces[j].Vertices.CopyTo(Mesh.Faces[i].Vertices, lastVertexIt);

							// Adjust index
							lastVertexIt += Mesh.Faces[j].Vertices.Length;
						}
					}

					// Remove now unused faces
					int jump = 0;
					for (int j = i + 1; j < f; ++j)
					{
						if (canMerge[j])
						{
							jump += 1;
						}
						else if (jump > 0)
						{
							Mesh.Faces[j - jump] = Mesh.Faces[j];
						}
					}

					// Remove faces removed from face count
					f -= jump;
				}
			}
			// finalize arrays

			if (m != Mesh.Materials.Length)
			{
				Array.Resize(ref Mesh.Materials, m);
			}

			if (f != Mesh.Faces.Length)
			{
				Array.Resize(ref Mesh.Faces, f);
			}
		}
	}
}


