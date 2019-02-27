using System;
using System.Text;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Textures;
using OpenBveApi.World;

namespace OpenBve {
	internal static class ObjectManager {

		// static objects
		internal class StaticObject : UnifiedObject {
			internal Mesh Mesh;
			/// <summary>The index to the Renderer.Object array, plus 1. The value of zero represents that the object is not currently shown by the renderer.</summary>
			internal int RendererIndex;
			/// <summary>The starting track position, for static objects only.</summary>
			internal float StartingDistance;
			/// <summary>The ending track position, for static objects only.</summary>
			internal float EndingDistance;
			/// <summary>The block mod group, for static objects only.</summary>
			internal short GroupIndex;
			/// <summary>Whether the object is dynamic, i.e. not static.</summary>
			internal bool Dynamic;

			/// <summary>Creates a new empty object</summary>
			internal StaticObject() 
			{
				Mesh = new Mesh
				{
					Faces = new MeshFace[] {},
					Materials =  new MeshMaterial[] {},
					Vertices =  new VertexTemplate[] {}
				};
			}

			/// <summary>Creates a clone of this object.</summary>
			internal StaticObject Clone()
			{
				StaticObject Result = new StaticObject
				{
					StartingDistance = StartingDistance,
					EndingDistance = EndingDistance,
					Dynamic = Dynamic,
					Mesh = { Vertices = new VertexTemplate[Mesh.Vertices.Length] }
				};
				// vertices
				for (int j = 0; j < Mesh.Vertices.Length; j++)
				{
					if (Mesh.Vertices[j] is ColoredVertex)
					{
						Result.Mesh.Vertices[j] = new ColoredVertex((ColoredVertex)Mesh.Vertices[j]);
					}
					else
					{
						Result.Mesh.Vertices[j] = new Vertex((Vertex)Mesh.Vertices[j]);
					}
				}
				// faces
				Result.Mesh.Faces = new MeshFace[Mesh.Faces.Length];
				for (int j = 0; j < Mesh.Faces.Length; j++)
				{
					Result.Mesh.Faces[j].Flags = Mesh.Faces[j].Flags;
					Result.Mesh.Faces[j].Material = Mesh.Faces[j].Material;
					Result.Mesh.Faces[j].Vertices = new MeshFaceVertex[Mesh.Faces[j].Vertices.Length];
					for (int k = 0; k < Mesh.Faces[j].Vertices.Length; k++)
					{
						Result.Mesh.Faces[j].Vertices[k] = Mesh.Faces[j].Vertices[k];
					}
				}
				// materials
				Result.Mesh.Materials = new MeshMaterial[Mesh.Materials.Length];
				for (int j = 0; j < Mesh.Materials.Length; j++)
				{
					Result.Mesh.Materials[j] = Mesh.Materials[j];
				}
				return Result;
			}
			
			internal void JoinObjects(StaticObject Add)
			{
				if (Add == null)
				{
					return;
				}
				int mf = Mesh.Faces.Length;
				int mm = Mesh.Materials.Length;
				int mv = Mesh.Vertices.Length;
				Array.Resize<MeshFace>(ref Mesh.Faces, mf + Add.Mesh.Faces.Length);
				Array.Resize<MeshMaterial>(ref Mesh.Materials, mm + Add.Mesh.Materials.Length);
				Array.Resize<VertexTemplate>(ref Mesh.Vertices, mv + Add.Mesh.Vertices.Length);
				for (int i = 0; i < Add.Mesh.Faces.Length; i++)
				{
					Mesh.Faces[mf + i] = Add.Mesh.Faces[i];
					for (int j = 0; j < Mesh.Faces[mf + i].Vertices.Length; j++)
					{
						Mesh.Faces[mf + i].Vertices[j].Index += (ushort) mv;
					}
					Mesh.Faces[mf + i].Material += (ushort) mm;
				}
				for (int i = 0; i < Add.Mesh.Materials.Length; i++)
				{
					Mesh.Materials[mm + i] = Add.Mesh.Materials[i];
				}
				for (int i = 0; i < Add.Mesh.Vertices.Length; i++)
				{
					if (Add.Mesh.Vertices[i] is ColoredVertex)
					{
						Mesh.Vertices[mv + i] = new ColoredVertex((ColoredVertex)Add.Mesh.Vertices[i]);
					}
					else
					{
						Mesh.Vertices[mv + i] = new Vertex((Vertex)Add.Mesh.Vertices[i]);
					}
					
				}
			}

			internal void ApplyScale(double x, double y, double z)
            {
	            float rx = (float)(1.0 / x);
	            float ry = (float)(1.0 / y);
	            float rz = (float)(1.0 / z);
	            float rx2 = rx * rx;
	            float ry2 = ry * ry;
	            float rz2 = rz * rz;
	            bool reverse = x * y * z < 0.0;
	            for (int j = 0; j < Mesh.Vertices.Length; j++) {
		            Mesh.Vertices[j].Coordinates.X *= x;
		            Mesh.Vertices[j].Coordinates.Y *= y;
		            Mesh.Vertices[j].Coordinates.Z *= z;
	            }
	            for (int j = 0; j < Mesh.Faces.Length; j++) {
		            for (int k = 0; k < Mesh.Faces[j].Vertices.Length; k++) {
			            double nx2 = Mesh.Faces[j].Vertices[k].Normal.X * Mesh.Faces[j].Vertices[k].Normal.X;
			            double ny2 = Mesh.Faces[j].Vertices[k].Normal.Y * Mesh.Faces[j].Vertices[k].Normal.Y;
			            double nz2 = Mesh.Faces[j].Vertices[k].Normal.Z * Mesh.Faces[j].Vertices[k].Normal.Z;
			            double u = nx2 * rx2 + ny2 * ry2 + nz2 * rz2;
			            if (u != 0.0) {
				            u = (float)Math.Sqrt((double)((nx2 + ny2 + nz2) / u));
				            Mesh.Faces[j].Vertices[k].Normal.X *= rx * u;
				            Mesh.Faces[j].Vertices[k].Normal.Y *= ry * u;
				            Mesh.Faces[j].Vertices[k].Normal.Z *= rz * u;
			            }
		            }
	            }
	            if (reverse) {
		            for (int j = 0; j < Mesh.Faces.Length; j++) {
			            Mesh.Faces[j].Flip();
		            }
	            }
            }

            internal void ApplyRotation(Vector3 Rotation, double Angle)
            {
	            double cosa = Math.Cos(Angle);
	            double sina = Math.Sin(Angle);
	            for (int j = 0; j < Mesh.Vertices.Length; j++)
	            {
		            Mesh.Vertices[j].Coordinates.Rotate(Rotation, cosa, sina);

	            }
	            for (int j = 0; j < Mesh.Faces.Length; j++)
	            {
		            for (int k = 0; k < Mesh.Faces[j].Vertices.Length; k++)
		            {
			            Mesh.Faces[j].Vertices[k].Normal.Rotate(Rotation, cosa, sina);
		            }
	            }
            }

            internal void ApplyTranslation(double x, double y, double z)
            {
	            for (int i = 0; i < Mesh.Vertices.Length; i++)
	            {
		            Mesh.Vertices[i].Coordinates.X += x;
		            Mesh.Vertices[i].Coordinates.Y += y;
		            Mesh.Vertices[i].Coordinates.Z += z;
	            }
            }

			internal void ApplyMirror(bool vX, bool vY, bool vZ, bool nX, bool nY, bool nZ)
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
							Mesh.Faces[i].Vertices[j].Normal.X *= -1;
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

			internal void ApplyShear(Vector3 d, Vector3 s, double r)
			{
				for (int j = 0; j < Mesh.Vertices.Length; j++)
				{
					double n = r * (d.X * Mesh.Vertices[j].Coordinates.X + d.Y * Mesh.Vertices[j].Coordinates.Y + d.Z * Mesh.Vertices[j].Coordinates.Z);
					Mesh.Vertices[j].Coordinates.X += s.X * n;
					Mesh.Vertices[j].Coordinates.Y += s.Y * n;
					Mesh.Vertices[j].Coordinates.Z += s.Z * n;
				}
				for (int j = 0; j < Mesh.Faces.Length; j++)
				{
					for (int k = 0; k < Mesh.Faces[j].Vertices.Length; k++)
					{
						if (Mesh.Faces[j].Vertices[k].Normal.X != 0.0f | Mesh.Faces[j].Vertices[k].Normal.Y != 0.0f | Mesh.Faces[j].Vertices[k].Normal.Z != 0.0f)
						{
							double n = r * (s.X * Mesh.Faces[j].Vertices[k].Normal.X + s.Y * Mesh.Faces[j].Vertices[k].Normal.Y + s.Z * Mesh.Faces[j].Vertices[k].Normal.Z);
							Mesh.Faces[j].Vertices[k].Normal -= d * n;
							Mesh.Faces[j].Vertices[k].Normal.Normalize();
						}
					}
				}
			}

			internal void ApplyData(StaticObject Prototype, Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, bool AccurateObjectDisposal, double AccurateObjectDisposalZOffset, double startingDistance, double endingDistance, double BlockLength, double TrackPosition, double Brightness)
			{
				StartingDistance = float.MaxValue;
				EndingDistance = float.MinValue;
				Mesh.Vertices = new VertexTemplate[Prototype.Mesh.Vertices.Length];
				// vertices
				for (int j = 0; j < Prototype.Mesh.Vertices.Length; j++)
				{
					if (Prototype.Mesh.Vertices[j] is ColoredVertex)
					{
						Mesh.Vertices[j] = new ColoredVertex((ColoredVertex)Prototype.Mesh.Vertices[j]);
					}
					else
					{
						Mesh.Vertices[j] = new Vertex((Vertex)Prototype.Mesh.Vertices[j]);
					}
					if (AccurateObjectDisposal)
					{
						Mesh.Vertices[j].Coordinates.Rotate(AuxTransformation);
						if (Mesh.Vertices[j].Coordinates.Z < StartingDistance)
						{
							StartingDistance = (float)Mesh.Vertices[j].Coordinates.Z;
						}
						if (Mesh.Vertices[j].Coordinates.Z > EndingDistance)
						{
							EndingDistance = (float)Mesh.Vertices[j].Coordinates.Z;
						}
						Mesh.Vertices[j].Coordinates = Prototype.Mesh.Vertices[j].Coordinates;
					}
					Mesh.Vertices[j].Coordinates.Rotate(AuxTransformation);
					Mesh.Vertices[j].Coordinates.Rotate(BaseTransformation);
					Mesh.Vertices[j].Coordinates += Position;
				}
				if (AccurateObjectDisposal)
				{
					StartingDistance += (float)AccurateObjectDisposalZOffset;
					EndingDistance += (float)AccurateObjectDisposalZOffset;
				}
				// faces
				Mesh.Faces = new MeshFace[Prototype.Mesh.Faces.Length];
				for (int j = 0; j < Prototype.Mesh.Faces.Length; j++)
				{
					Mesh.Faces[j].Flags = Prototype.Mesh.Faces[j].Flags;
					Mesh.Faces[j].Material = Prototype.Mesh.Faces[j].Material;
					Mesh.Faces[j].Vertices = new MeshFaceVertex[Prototype.Mesh.Faces[j].Vertices.Length];
					for (int k = 0; k < Prototype.Mesh.Faces[j].Vertices.Length; k++)
					{
						Mesh.Faces[j].Vertices[k] = Prototype.Mesh.Faces[j].Vertices[k];
						if (Mesh.Faces[j].Vertices[k].Normal.NormSquared() != 0.0)
						{
							Mesh.Faces[j].Vertices[k].Normal.Rotate(AuxTransformation);
							Mesh.Faces[j].Vertices[k].Normal.Rotate(BaseTransformation);
						}
					}
				}
				// materials
				Mesh.Materials = new MeshMaterial[Prototype.Mesh.Materials.Length];
				for (int j = 0; j < Prototype.Mesh.Materials.Length; j++)
				{
					Mesh.Materials[j] = Prototype.Mesh.Materials[j];
					Mesh.Materials[j].Color.R = (byte)Math.Round((double)Prototype.Mesh.Materials[j].Color.R * Brightness);
					Mesh.Materials[j].Color.G = (byte)Math.Round((double)Prototype.Mesh.Materials[j].Color.G * Brightness);
					Mesh.Materials[j].Color.B = (byte)Math.Round((double)Prototype.Mesh.Materials[j].Color.B * Brightness);
				}
				const double minBlockLength = 20.0;
				if (BlockLength < minBlockLength)
				{
					BlockLength = BlockLength * Math.Ceiling(minBlockLength / BlockLength);
				}
				if (AccurateObjectDisposal)
				{
					StartingDistance += (float)TrackPosition;
					EndingDistance += (float)TrackPosition;
					double z = BlockLength * Math.Floor(TrackPosition / BlockLength);
					startingDistance = Math.Min(z - BlockLength, (double)StartingDistance);
					endingDistance = Math.Max(z + 2.0 * BlockLength, (double)EndingDistance);
					StartingDistance = (float)(BlockLength * Math.Floor(startingDistance / BlockLength));
					EndingDistance = (float)(BlockLength * Math.Ceiling(endingDistance / BlockLength));
				}
				else
				{
					StartingDistance = (float)startingDistance;
					EndingDistance = (float)endingDistance;
				}
				if (BlockLength != 0.0)
				{
					checked
					{
						GroupIndex = (short)Mod(Math.Floor(StartingDistance / BlockLength), Math.Ceiling(600 / BlockLength));
					}
				}
			}

            public override void CreateObject(Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, int SectionIndex, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness, bool DuplicateMaterials)
            {
	            CreateStaticObject(this, Position, BaseTransformation, AuxTransformation, AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness);
            }

			public override void OptimizeObject(bool PreserveVertices)
	        {
		        int v = Mesh.Vertices.Length;
		        int m = Mesh.Materials.Length;
		        int f = Mesh.Faces.Length;
		        if (f >= Interface.CurrentOptions.ObjectOptimizationBasicThreshold) return;
		        // eliminate invalid faces and reduce incomplete faces
		        for (int i = 0; i < f; i++)
		        {
			        int type = Mesh.Faces[i].Flags & MeshFace.FaceTypeMask;
			        bool keep;
			        if (type == MeshFace.FaceTypeTriangles)
			        {
				        keep = Mesh.Faces[i].Vertices.Length >= 3;
				        if (keep)
				        {
					        int n = (Mesh.Faces[i].Vertices.Length / 3) * 3;
					        if (Mesh.Faces[i].Vertices.Length != n)
					        {
						        Array.Resize<MeshFaceVertex>(ref Mesh.Faces[i].Vertices, n);
					        }
				        }
			        }
			        else if (type == MeshFace.FaceTypeQuads)
			        {
				        keep = Mesh.Faces[i].Vertices.Length >= 4;
				        if (keep)
				        {
					        int n = Mesh.Faces[i].Vertices.Length & ~3;
					        if (Mesh.Faces[i].Vertices.Length != n)
					        {
						        Array.Resize<MeshFaceVertex>(ref Mesh.Faces[i].Vertices, n);
					        }
				        }
			        }
			        else if (type == MeshFace.FaceTypeQuadStrip)
			        {
				        keep = Mesh.Faces[i].Vertices.Length >= 4;
				        if (keep)
				        {
					        int n = Mesh.Faces[i].Vertices.Length & ~1;
					        if (Mesh.Faces[i].Vertices.Length != n)
					        {
						        Array.Resize<MeshFaceVertex>(ref Mesh.Faces[i].Vertices, n);
					        }
				        }
			        }
			        else
			        {
				        keep = Mesh.Faces[i].Vertices.Length >= 3;
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
		        // eliminate unused vertices
		        if (!PreserveVertices)
		        {
			        for (int i = 0; i < v; i++)
			        {
				        bool keep = false;
				        for (int j = 0; j < f; j++)
				        {
					        for (int k = 0; k < Mesh.Faces[j].Vertices.Length; k++)
					        {
						        if (Mesh.Faces[j].Vertices[k].Index == i)
						        {
							        keep = true;
							        break;
						        }
					        }
					        if (keep)
					        {
						        break;
					        }
				        }
				        if (!keep)
				        {
					        for (int j = 0; j < f; j++)
					        {
						        for (int k = 0; k < Mesh.Faces[j].Vertices.Length; k++)
						        {
							        if (Mesh.Faces[j].Vertices[k].Index > i)
							        {
								        Mesh.Faces[j].Vertices[k].Index--;
							        }
						        }
					        }
					        for (int j = i; j < v - 1; j++)
					        {
						        Mesh.Vertices[j] = Mesh.Vertices[j + 1];
					        }
					        v--;
					        i--;
				        }
			        }
		        }
		        // eliminate duplicate vertices
		        if (!PreserveVertices)
		        {
			        for (int i = 0; i < v - 1; i++)
			        {
				        for (int j = i + 1; j < v; j++)
				        {
					        if (Mesh.Vertices[i] == Mesh.Vertices[j])
					        {
						        for (int k = 0; k < f; k++)
						        {
							        for (int h = 0; h < Mesh.Faces[k].Vertices.Length; h++)
							        {
								        if (Mesh.Faces[k].Vertices[h].Index == j)
								        {
									        Mesh.Faces[k].Vertices[h].Index = (ushort)i;
								        }
								        else if (Mesh.Faces[k].Vertices[h].Index > j)
								        {
									        Mesh.Faces[k].Vertices[h].Index--;
								        }
							        }
						        }
						        for (int k = j; k < v - 1; k++)
						        {
							        Mesh.Vertices[k] = Mesh.Vertices[k + 1];
						        }
						        v--;
						        j--;
					        }
				        }
			        }
		        }
		        // eliminate unused materials
		        bool[] materialUsed = new bool[m];
		        for (int i = 0; i < f; i++)
		        {
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
							        Mesh.Faces[k].Material = (ushort)i;
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
		        // structure optimization
		        if (!PreserveVertices & f < Interface.CurrentOptions.ObjectOptimizationFullThreshold)
		        {
			        // create TRIANGLES and QUADS from POLYGON
			        for (int i = 0; i < f; i++)
			        {
				        int type = Mesh.Faces[i].Flags & MeshFace.FaceTypeMask;
				        if (type == MeshFace.FaceTypePolygon)
				        {
					        if (Mesh.Faces[i].Vertices.Length == 3)
					        {
						        unchecked
						        {
							        Mesh.Faces[i].Flags &= (byte)~MeshFace.FaceTypeMask;
							        Mesh.Faces[i].Flags |= MeshFace.FaceTypeTriangles;
						        }
					        }
					        else if (Mesh.Faces[i].Vertices.Length == 4)
					        {
						        unchecked
						        {
							        Mesh.Faces[i].Flags &= (byte)~MeshFace.FaceTypeMask;
							        Mesh.Faces[i].Flags |= MeshFace.FaceTypeQuads;
						        }
					        }
				        }
			        }
			        // decomposit TRIANGLES and QUADS
			        for (int i = 0; i < f; i++)
			        {
				        int type = Mesh.Faces[i].Flags & MeshFace.FaceTypeMask;
				        if (type == MeshFace.FaceTypeTriangles)
				        {
					        if (Mesh.Faces[i].Vertices.Length > 3)
					        {
						        int n = (Mesh.Faces[i].Vertices.Length - 3) / 3;
						        while (f + n > Mesh.Faces.Length)
						        {
							        Array.Resize<MeshFace>(ref Mesh.Faces, Mesh.Faces.Length << 1);
						        }
						        for (int j = 0; j < n; j++)
						        {
							        Mesh.Faces[f + j].Vertices = new MeshFaceVertex[3];
							        for (int k = 0; k < 3; k++)
							        {
								        Mesh.Faces[f + j].Vertices[k] = Mesh.Faces[i].Vertices[3 + 3 * j + k];
							        }
							        Mesh.Faces[f + j].Material = Mesh.Faces[i].Material;
							        Mesh.Faces[f + j].Flags = Mesh.Faces[i].Flags;
							        unchecked
							        {
								        Mesh.Faces[i].Flags &= (byte)~MeshFace.FaceTypeMask;
								        Mesh.Faces[i].Flags |= MeshFace.FaceTypeTriangles;
							        }
						        }
						        Array.Resize<MeshFaceVertex>(ref Mesh.Faces[i].Vertices, 3);
						        f += n;
					        }
				        }
				        else if (type == MeshFace.FaceTypeQuads)
				        {
					        if (Mesh.Faces[i].Vertices.Length > 4)
					        {
						        int n = (Mesh.Faces[i].Vertices.Length - 4) >> 2;
						        while (f + n > Mesh.Faces.Length)
						        {
							        Array.Resize<MeshFace>(ref Mesh.Faces, Mesh.Faces.Length << 1);
						        }
						        for (int j = 0; j < n; j++)
						        {
							        Mesh.Faces[f + j].Vertices = new MeshFaceVertex[4];
							        for (int k = 0; k < 4; k++)
							        {
								        Mesh.Faces[f + j].Vertices[k] = Mesh.Faces[i].Vertices[4 + 4 * j + k];
							        }
							        Mesh.Faces[f + j].Material = Mesh.Faces[i].Material;
							        Mesh.Faces[f + j].Flags = Mesh.Faces[i].Flags;
							        unchecked
							        {
								        Mesh.Faces[i].Flags &= (byte)~MeshFace.FaceTypeMask;
								        Mesh.Faces[i].Flags |= MeshFace.FaceTypeQuads;
							        }
						        }
						        Array.Resize<MeshFaceVertex>(ref Mesh.Faces[i].Vertices, 4);
						        f += n;
					        }
				        }
			        }
			        // optimize for TRIANGLE_STRIP
			        int index = -1;
			        while (true)
			        {
				        // add TRIANGLES to TRIANGLE_STRIP
				        for (int i = 0; i < f; i++)
				        {
					        if (index == i | index == -1)
					        {
						        int type = Mesh.Faces[i].Flags & MeshFace.FaceTypeMask;
						        if (type == MeshFace.FaceTypeTriangleStrip)
						        {
							        int face = Mesh.Faces[i].Flags & MeshFace.Face2Mask;
							        for (int j = 0; j < f; j++)
							        {
								        int type2 = Mesh.Faces[j].Flags & MeshFace.FaceTypeMask;
								        int face2 = Mesh.Faces[j].Flags & MeshFace.Face2Mask;
								        if (type2 == MeshFace.FaceTypeTriangles & face == face2)
								        {
									        if (Mesh.Faces[i].Material == Mesh.Faces[j].Material)
									        {
										        bool keep = true;
										        for (int k = 0; k < 3; k++)
										        {
											        int l = (k + 1) % 3;
											        int n = Mesh.Faces[i].Vertices.Length;
											        if (Mesh.Faces[i].Vertices[0] == Mesh.Faces[j].Vertices[k] & Mesh.Faces[i].Vertices[1] == Mesh.Faces[j].Vertices[l])
											        {
												        Array.Resize<MeshFaceVertex>(ref Mesh.Faces[i].Vertices, n + 1);
												        for (int h = n; h >= 1; h--)
												        {
													        Mesh.Faces[i].Vertices[h] = Mesh.Faces[i].Vertices[h - 1];
												        }
												        Mesh.Faces[i].Vertices[0] = Mesh.Faces[j].Vertices[(k + 2) % 3];
												        keep = false;
											        }
											        else if (Mesh.Faces[i].Vertices[n - 1] == Mesh.Faces[j].Vertices[l] & Mesh.Faces[i].Vertices[n - 2] == Mesh.Faces[j].Vertices[k])
											        {
												        Array.Resize<MeshFaceVertex>(ref Mesh.Faces[i].Vertices, n + 1);
												        Mesh.Faces[i].Vertices[n] = Mesh.Faces[j].Vertices[(k + 2) % 3];
												        keep = false;
											        }
											        if (!keep)
											        {
												        break;
											        }
										        }
										        if (!keep)
										        {
											        for (int k = j; k < f - 1; k++)
											        {
												        Mesh.Faces[k] = Mesh.Faces[k + 1];
											        }
											        if (j < i)
											        {
												        i--;
											        }
											        f--;
											        j--;
										        }
									        }
								        }
							        }
						        }
					        }
				        }
				        // join TRIANGLES into new TRIANGLE_STRIP
				        index = -1;
				        for (int i = 0; i < f - 1; i++)
				        {
					        int type = Mesh.Faces[i].Flags & MeshFace.FaceTypeMask;
					        if (type == MeshFace.FaceTypeTriangles)
					        {
						        int face = Mesh.Faces[i].Flags & MeshFace.Face2Mask;
						        for (int j = i + 1; j < f; j++)
						        {
							        int type2 = Mesh.Faces[j].Flags & MeshFace.FaceTypeMask;
							        int face2 = Mesh.Faces[j].Flags & MeshFace.Face2Mask;
							        if (type2 == MeshFace.FaceTypeTriangles & face == face2)
							        {
								        if (Mesh.Faces[i].Material == Mesh.Faces[j].Material)
								        {
									        for (int ik = 0; ik < 3; ik++)
									        {
										        int il = (ik + 1) % 3;
										        for (int jk = 0; jk < 3; jk++)
										        {
											        int jl = (jk + 1) % 3;
											        if (Mesh.Faces[i].Vertices[ik] == Mesh.Faces[j].Vertices[jl] & Mesh.Faces[i].Vertices[il] == Mesh.Faces[j].Vertices[jk])
											        {
												        unchecked
												        {
													        Mesh.Faces[i].Flags &= (byte)~MeshFace.FaceTypeMask;
													        Mesh.Faces[i].Flags |= MeshFace.FaceTypeTriangleStrip;
												        }
												        Mesh.Faces[i].Vertices = new MeshFaceVertex[] {
													        Mesh.Faces[i].Vertices[(ik + 2) % 3],
													        Mesh.Faces[i].Vertices[ik],
													        Mesh.Faces[i].Vertices[il],
													        Mesh.Faces[j].Vertices[(jk + 2) % 3]
												        };
												        for (int k = j; k < f - 1; k++)
												        {
													        Mesh.Faces[k] = Mesh.Faces[k + 1];
												        }
												        f--;
												        index = i;
												        break;
											        }
										        }
										        if (index >= 0) break;
									        }
								        }
							        }
							        if (index >= 0) break;
						        }
					        }
				        }
				        if (index == -1) break;
			        }
			        // optimize for QUAD_STRIP
			        index = -1;
			        while (true)
			        {
				        // add QUADS to QUAD_STRIP
				        for (int i = 0; i < f; i++)
				        {
					        if (index == i | index == -1)
					        {
						        int type = Mesh.Faces[i].Flags & MeshFace.FaceTypeMask;
						        if (type == MeshFace.FaceTypeQuadStrip)
						        {
							        int face = Mesh.Faces[i].Flags & MeshFace.Face2Mask;
							        for (int j = 0; j < f; j++)
							        {
								        int type2 = Mesh.Faces[j].Flags & MeshFace.FaceTypeMask;
								        int face2 = Mesh.Faces[j].Flags & MeshFace.Face2Mask;
								        if (type2 == MeshFace.FaceTypeQuads & face == face2)
								        {
									        if (Mesh.Faces[i].Material == Mesh.Faces[j].Material)
									        {
										        bool keep = true;
										        for (int k = 0; k < 4; k++)
										        {
											        int l = (k + 1) & 3;
											        int n = Mesh.Faces[i].Vertices.Length;
											        if (Mesh.Faces[i].Vertices[0] == Mesh.Faces[j].Vertices[l] & Mesh.Faces[i].Vertices[1] == Mesh.Faces[j].Vertices[k])
											        {
												        Array.Resize<MeshFaceVertex>(ref Mesh.Faces[i].Vertices, n + 2);
												        for (int h = n + 1; h >= 2; h--)
												        {
													        Mesh.Faces[i].Vertices[h] = Mesh.Faces[i].Vertices[h - 2];
												        }
												        Mesh.Faces[i].Vertices[0] = Mesh.Faces[j].Vertices[(k + 2) & 3];
												        Mesh.Faces[i].Vertices[1] = Mesh.Faces[j].Vertices[(k + 3) & 3];
												        keep = false;
											        }
											        else if (Mesh.Faces[i].Vertices[n - 1] == Mesh.Faces[j].Vertices[l] & Mesh.Faces[i].Vertices[n - 2] == Mesh.Faces[j].Vertices[k])
											        {
												        Array.Resize<MeshFaceVertex>(ref Mesh.Faces[i].Vertices, n + 2);
												        Mesh.Faces[i].Vertices[n] = Mesh.Faces[j].Vertices[(k + 3) & 3];
												        Mesh.Faces[i].Vertices[n + 1] = Mesh.Faces[j].Vertices[(k + 2) & 3];
												        keep = false;
											        }
											        if (!keep)
											        {
												        break;
											        }
										        }
										        if (!keep)
										        {
											        for (int k = j; k < f - 1; k++)
											        {
												        Mesh.Faces[k] = Mesh.Faces[k + 1];
											        }
											        if (j < i)
											        {
												        i--;
											        }
											        f--;
											        j--;
										        }
									        }
								        }
							        }
						        }
					        }
				        }
				        // join QUADS into new QUAD_STRIP
				        index = -1;
				        for (int i = 0; i < f - 1; i++)
				        {
					        int type = Mesh.Faces[i].Flags & MeshFace.FaceTypeMask;
					        if (type == MeshFace.FaceTypeQuads)
					        {
						        int face = Mesh.Faces[i].Flags & MeshFace.Face2Mask;
						        for (int j = i + 1; j < f; j++)
						        {
							        int type2 = Mesh.Faces[j].Flags & MeshFace.FaceTypeMask;
							        int face2 = Mesh.Faces[j].Flags & MeshFace.Face2Mask;
							        if (type2 == MeshFace.FaceTypeQuads & face == face2)
							        {
								        if (Mesh.Faces[i].Material == Mesh.Faces[j].Material)
								        {
									        for (int ik = 0; ik < 4; ik++)
									        {
										        int il = (ik + 1) & 3;
										        for (int jk = 0; jk < 4; jk++)
										        {
											        int jl = (jk + 1) & 3;
											        if (Mesh.Faces[i].Vertices[ik] == Mesh.Faces[j].Vertices[jl] & Mesh.Faces[i].Vertices[il] == Mesh.Faces[j].Vertices[jk])
											        {
												        unchecked
												        {
													        Mesh.Faces[i].Flags &= (byte)~MeshFace.FaceTypeMask;
													        Mesh.Faces[i].Flags |= MeshFace.FaceTypeQuadStrip;
												        }
												        Mesh.Faces[i].Vertices = new MeshFaceVertex[] {
													        Mesh.Faces[i].Vertices[(ik + 2) & 3],
													        Mesh.Faces[i].Vertices[(ik + 3) & 3],
													        Mesh.Faces[i].Vertices[il],
													        Mesh.Faces[i].Vertices[ik],
													        Mesh.Faces[j].Vertices[(jk + 3) & 3],
													        Mesh.Faces[j].Vertices[(jk + 2) & 3]
												        };
												        for (int k = j; k < f - 1; k++)
												        {
													        Mesh.Faces[k] = Mesh.Faces[k + 1];
												        }
												        f--;
												        index = i;
												        break;
											        }
										        }
										        if (index >= 0) break;
									        }
								        }
							        }
							        if (index >= 0) break;
						        }
					        }
				        }
				        if (index == -1) break;
			        }
			        // join TRIANGLES, join QUADS
			        for (int i = 0; i < f - 1; i++)
			        {
				        int type = Mesh.Faces[i].Flags & MeshFace.FaceTypeMask;
				        if (type == MeshFace.FaceTypeTriangles | type == MeshFace.FaceTypeQuads)
				        {
					        int face = Mesh.Faces[i].Flags & MeshFace.Face2Mask;
					        for (int j = i + 1; j < f; j++)
					        {
						        int type2 = Mesh.Faces[j].Flags & MeshFace.FaceTypeMask;
						        int face2 = Mesh.Faces[j].Flags & MeshFace.Face2Mask;
						        if (type == type2 & face == face2)
						        {
							        if (Mesh.Faces[i].Material == Mesh.Faces[j].Material)
							        {
								        int n = Mesh.Faces[i].Vertices.Length;
								        Array.Resize<MeshFaceVertex>(ref Mesh.Faces[i].Vertices, n + Mesh.Faces[j].Vertices.Length);
								        for (int k = 0; k < Mesh.Faces[j].Vertices.Length; k++)
								        {
									        Mesh.Faces[i].Vertices[n + k] = Mesh.Faces[j].Vertices[k];
								        }
								        for (int k = j; k < f - 1; k++)
								        {
									        Mesh.Faces[k] = Mesh.Faces[k + 1];
								        }
								        f--;
								        j--;
							        }
						        }
					        }
				        }
			        }
		        }
		        // finalize arrays
		        if (v != Mesh.Vertices.Length)
		        {
			        Array.Resize<VertexTemplate>(ref Mesh.Vertices, v);
		        }
		        if (m != Mesh.Materials.Length)
		        {
			        Array.Resize<MeshMaterial>(ref Mesh.Materials, m);
		        }
		        if (f != Mesh.Faces.Length)
		        {
			        Array.Resize<MeshFace>(ref Mesh.Faces, f);
		        }
	        }
		}
		internal static StaticObject[] Objects = new StaticObject[16];
		internal static int ObjectsUsed;
		internal static int[] ObjectsSortedByStart = new int[] { };
		internal static int[] ObjectsSortedByEnd = new int[] { };
		internal static int ObjectsSortedByStartPointer = 0;
		internal static int ObjectsSortedByEndPointer = 0;
		internal static double LastUpdatedTrackPosition = 0.0;

		// animated objects
		
		internal struct AnimatedObjectState {
			internal Vector3 Position;
			internal ObjectManager.StaticObject Object;

			internal AnimatedObjectState(StaticObject stateObject, Vector3 position)
			{
				Object = stateObject;
				Position = position;
			}
		}
		internal class AnimatedObject {
			// states
			internal AnimatedObjectState[] States;
			internal FunctionScript StateFunction;
			internal int CurrentState;
			internal Vector3 TranslateXDirection;
			internal Vector3 TranslateYDirection;
			internal Vector3 TranslateZDirection;
			internal FunctionScript TranslateXFunction;
			internal FunctionScript TranslateYFunction;
			internal FunctionScript TranslateZFunction;
			internal Vector3 RotateXDirection;
			internal Vector3 RotateYDirection;
			internal Vector3 RotateZDirection;
			internal FunctionScript RotateXFunction;
			internal FunctionScript RotateYFunction;
			internal FunctionScript RotateZFunction;
			internal Damping RotateXDamping;
			internal Damping RotateYDamping;
			internal Damping RotateZDamping;
			internal Vector2 TextureShiftXDirection;
			internal Vector2 TextureShiftYDirection;
			internal FunctionScript TextureShiftXFunction;
			internal FunctionScript TextureShiftYFunction;
			internal bool LEDClockwiseWinding;
			internal double LEDInitialAngle;
			internal double LEDLastAngle;
			/// <summary>If LEDFunction is used, an array of five vectors representing the bottom-left, up-left, up-right, bottom-right and center coordinates of the LED square, or a null reference otherwise.</summary>
			internal Vector3[] LEDVectors;
			internal FunctionScript LEDFunction;
			internal double RefreshRate;
			internal double SecondsSinceLastUpdate;
			internal int ObjectIndex;
			// methods
			internal bool IsFreeOfFunctions() {
				if (this.StateFunction != null) return false;
				if (this.TranslateXFunction != null | this.TranslateYFunction != null | this.TranslateZFunction != null) return false;
				if (this.RotateXFunction != null | this.RotateYFunction != null | this.RotateZFunction != null) return false;
				if (this.TextureShiftXFunction != null | this.TextureShiftYFunction != null) return false;
				if (this.LEDFunction != null) return false;
				return true;
			}
			internal AnimatedObject Clone() {
				AnimatedObject Result = new AnimatedObject();
				Result.States = new AnimatedObjectState[this.States.Length];
				for (int i = 0; i < this.States.Length; i++) {
					Result.States[i].Position = this.States[i].Position;
					Result.States[i].Object = this.States[i].Object.Clone();
				}
				Result.StateFunction = this.StateFunction == null ? null : this.StateFunction.Clone();
				Result.CurrentState = this.CurrentState;
				Result.TranslateZDirection = this.TranslateZDirection;
				Result.TranslateYDirection = this.TranslateYDirection;
				Result.TranslateXDirection = this.TranslateXDirection;
				Result.TranslateXFunction = this.TranslateXFunction == null ? null : this.TranslateXFunction.Clone();
				Result.TranslateYFunction = this.TranslateYFunction == null ? null : this.TranslateYFunction.Clone();
				Result.TranslateZFunction = this.TranslateZFunction == null ? null : this.TranslateZFunction.Clone();
				Result.RotateXDirection = this.RotateXDirection;
				Result.RotateYDirection = this.RotateYDirection;
				Result.RotateZDirection = this.RotateZDirection;
				Result.RotateXFunction = this.RotateXFunction == null ? null : this.RotateXFunction.Clone();
				Result.RotateXDamping = this.RotateXDamping == null ? null : this.RotateXDamping.Clone();
				Result.RotateYFunction = this.RotateYFunction == null ? null : this.RotateYFunction.Clone();
				Result.RotateYDamping = this.RotateYDamping == null ? null : this.RotateYDamping.Clone();
				Result.RotateZFunction = this.RotateZFunction == null ? null : this.RotateZFunction.Clone();
				Result.RotateZDamping = this.RotateZDamping == null ? null : this.RotateZDamping.Clone();
				Result.TextureShiftXDirection = this.TextureShiftXDirection;
				Result.TextureShiftYDirection = this.TextureShiftYDirection;
				Result.TextureShiftXFunction = this.TextureShiftXFunction == null ? null : this.TextureShiftXFunction.Clone();
				Result.TextureShiftYFunction = this.TextureShiftYFunction == null ? null : this.TextureShiftYFunction.Clone();
				Result.LEDClockwiseWinding = this.LEDClockwiseWinding;
				Result.LEDInitialAngle = this.LEDInitialAngle;
				Result.LEDLastAngle = this.LEDLastAngle;
				if (this.LEDVectors != null) {
					Result.LEDVectors = new Vector3[this.LEDVectors.Length];
					for (int i = 0; i < this.LEDVectors.Length; i++) {
						Result.LEDVectors[i] = this.LEDVectors[i];
					}
				} else {
					Result.LEDVectors = null;
				}
				Result.LEDFunction = this.LEDFunction == null ? null : this.LEDFunction.Clone();
				Result.RefreshRate = this.RefreshRate;
				Result.SecondsSinceLastUpdate = 0.0;
				Result.ObjectIndex = -1;
				return Result;
			}
		}
		internal class AnimatedObjectCollection : UnifiedObject {
			internal AnimatedObject[] Objects;
			public override void CreateObject(Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, int SectionIndex, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness, bool DuplicateMaterials)
			{
				throw new NotImplementedException();
			}

			public override void OptimizeObject(bool PreserveVerticies)
			{
				for (int i = 0; i < Objects.Length; i++) {
					for (int j = 0; j < Objects[i].States.Length; j++) {
						Objects[i].States[j].Object.OptimizeObject(PreserveVerticies);
					}
				}
			}
		}
		internal static void InitializeAnimatedObject(ref AnimatedObject Object, int StateIndex, bool Overlay, bool Show) {
			int i = Object.ObjectIndex;
			Renderer.HideObject(i);
			int t = StateIndex;
			if (t >= 0 && Object.States[t].Object != null) {
				int m = Object.States[t].Object.Mesh.Vertices.Length;
				ObjectManager.Objects[i].Mesh.Vertices = new VertexTemplate[m];
				for (int k = 0; k < m; k++) {
					if (Object.States[t].Object.Mesh.Vertices[k] is ColoredVertex)
					{
						ObjectManager.Objects[i].Mesh.Vertices[k] = new ColoredVertex((ColoredVertex)Object.States[t].Object.Mesh.Vertices[k]);
					}
					else
					{
						ObjectManager.Objects[i].Mesh.Vertices[k] = new Vertex((Vertex)Object.States[t].Object.Mesh.Vertices[k]);
					}
					
				}
				m = Object.States[t].Object.Mesh.Faces.Length;
				ObjectManager.Objects[i].Mesh.Faces = new MeshFace[m];
				for (int k = 0; k < m; k++) {
					ObjectManager.Objects[i].Mesh.Faces[k].Flags = Object.States[t].Object.Mesh.Faces[k].Flags;
					ObjectManager.Objects[i].Mesh.Faces[k].Material = Object.States[t].Object.Mesh.Faces[k].Material;
					int o = Object.States[t].Object.Mesh.Faces[k].Vertices.Length;
					ObjectManager.Objects[i].Mesh.Faces[k].Vertices = new MeshFaceVertex[o];
					for (int h = 0; h < o; h++) {
						ObjectManager.Objects[i].Mesh.Faces[k].Vertices[h] = Object.States[t].Object.Mesh.Faces[k].Vertices[h];
					}
				}
				ObjectManager.Objects[i].Mesh.Materials = Object.States[t].Object.Mesh.Materials;
			} else {
				ObjectManager.Objects[i] = null;
				ObjectManager.Objects[i] = new StaticObject();
			}
			Object.CurrentState = StateIndex;
			if (Show) {
				if (Overlay) {
					Renderer.ShowObject(i, ObjectType.Overlay);
				} else {
					Renderer.ShowObject(i, ObjectType.Dynamic);
				}
			}
		}

		internal static void UpdateAnimatedObject(ref AnimatedObject Object, bool IsPartOfTrain, TrainManager.Train Train, int CarIndex, int SectionIndex, double TrackPosition, Vector3 Position, Vector3 Direction, Vector3 Up, Vector3 Side, bool Overlay, bool UpdateFunctions, bool Show, double TimeElapsed) {
			int s = Object.CurrentState;
			int i = Object.ObjectIndex;
			// state change
			if (Object.StateFunction != null & UpdateFunctions) {
				double sd = Object.StateFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, Object.CurrentState);
				int si = (int)Math.Round(sd);
				int sn = Object.States.Length;
				if (si < 0 | si >= sn) si = -1;
				if (s != si) {
					InitializeAnimatedObject(ref Object, si, Overlay, Show);
					s = si;
				}
			}
			if (s == -1) return;
			// translation
			if (Object.TranslateXFunction != null) {
				double x;
				if (UpdateFunctions) {
					x = Object.TranslateXFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, Object.CurrentState);
				} else  {
					x = Object.TranslateXFunction.LastResult;
				}
				Vector3 translationVector = new Vector3(Object.TranslateXDirection); //Must clone
				translationVector.Rotate(Direction, Up, Side);
				translationVector *= x;
				Position += translationVector;
			}
			if (Object.TranslateYFunction != null) {
				double y;
				if (UpdateFunctions) {
					y = Object.TranslateYFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, Object.CurrentState);
				} else {
					y = Object.TranslateYFunction.LastResult;
				}
				Vector3 translationVector = new Vector3(Object.TranslateYDirection); //Must clone
				translationVector.Rotate(Direction, Up, Side);
				translationVector *= y;
				Position += translationVector;
			}
			if (Object.TranslateZFunction != null) {
				double z;
				if (UpdateFunctions) {
					z = Object.TranslateZFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, Object.CurrentState);
				} else {
					z = Object.TranslateZFunction.LastResult;
				}
				Vector3 translationVector = new Vector3(Object.TranslateZDirection); //Must clone
				translationVector.Rotate(Direction, Up, Side);
				translationVector *= z;
				Position += translationVector;
			}
			// rotation
			bool rotateX = Object.RotateXFunction != null;
			bool rotateY = Object.RotateYFunction != null;
			bool rotateZ = Object.RotateZFunction != null;
			double cosX, sinX;
			if (rotateX) {
				double a;
				if (UpdateFunctions) {
					a = Object.RotateXFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, Object.CurrentState);
				} else {
					a = Object.RotateXFunction.LastResult;
				}
				if (Object.RotateXDamping != null)
				{
					Object.RotateXDamping.Update(TimeElapsed, ref a, true);
				}
				cosX = Math.Cos(a);
				sinX = Math.Sin(a);
			} else {
				cosX = 0.0; sinX = 0.0;
			}
			double cosY, sinY;
			if (rotateY) {
				double a;
				if (UpdateFunctions) {
					a = Object.RotateYFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, Object.CurrentState);
				} else {
					a = Object.RotateYFunction.LastResult;
				}
				if (Object.RotateYDamping != null)
				{
					Object.RotateYDamping.Update(TimeElapsed, ref a, true);
				}
				cosY = Math.Cos(a);
				sinY = Math.Sin(a);
			} else {
				cosY = 0.0; sinY = 0.0;
			}
			double cosZ, sinZ;
			if (rotateZ) {
				double a;
				if (UpdateFunctions) {
					a = Object.RotateZFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, Object.CurrentState);
				} else {
					a = Object.RotateZFunction.LastResult;
				}
				if (Object.RotateZDamping != null)
				{
					Object.RotateZDamping.Update(TimeElapsed, ref a, true);
				}
				cosZ = Math.Cos(a);
				sinZ = Math.Sin(a);
			} else {
				cosZ = 0.0; sinZ = 0.0;
			}
			// texture shift
			bool shiftx = Object.TextureShiftXFunction != null;
			bool shifty = Object.TextureShiftYFunction != null;
			if ((shiftx | shifty) & UpdateFunctions) {
				for (int k = 0; k < ObjectManager.Objects[i].Mesh.Vertices.Length; k++) {
					ObjectManager.Objects[i].Mesh.Vertices[k].TextureCoordinates = Object.States[s].Object.Mesh.Vertices[k].TextureCoordinates;
				}
				if (shiftx) {
					double x = Object.TextureShiftXFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, Object.CurrentState);
					x -= Math.Floor(x);
					for (int k = 0; k < ObjectManager.Objects[i].Mesh.Vertices.Length; k++) {
						ObjectManager.Objects[i].Mesh.Vertices[k].TextureCoordinates.X += (float)(x * Object.TextureShiftXDirection.X);
						ObjectManager.Objects[i].Mesh.Vertices[k].TextureCoordinates.Y += (float)(x * Object.TextureShiftXDirection.Y);
					}
				}
				if (shifty) {
					double y = Object.TextureShiftYFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, Object.CurrentState);
					y -= Math.Floor(y);
					for (int k = 0; k < ObjectManager.Objects[i].Mesh.Vertices.Length; k++) {
						ObjectManager.Objects[i].Mesh.Vertices[k].TextureCoordinates.X += (float)(y * Object.TextureShiftYDirection.X);
						ObjectManager.Objects[i].Mesh.Vertices[k].TextureCoordinates.Y += (float)(y * Object.TextureShiftYDirection.Y);
					}
				}
			}
			// led
			bool led = Object.LEDFunction != null;
			double ledangle;
			if (led) {
				if (UpdateFunctions) {
					// double lastangle = Object.LEDFunction.LastResult;
					ledangle = Object.LEDFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, Object.CurrentState);
				} else {
					ledangle = Object.LEDFunction.LastResult;
				}
			} else {
				ledangle = 0.0;
			}
			// null object
			if (Object.States[s].Object == null) {
				return;
			}
			// initialize vertices
			for (int k = 0; k < Object.States[s].Object.Mesh.Vertices.Length; k++) {
				ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates = Object.States[s].Object.Mesh.Vertices[k].Coordinates;
			}
			// led
			if (led) {
				/*
				 * Edges:         Vertices:
				 * 0 - bottom     0 - bottom-left
				 * 1 - left       1 - top-left
				 * 2 - top        2 - top-right
				 * 3 - right      3 - bottom-right
				 *                4 - center
				 * */
				int v = 1;
				if (Object.LEDClockwiseWinding) {
					/* winding is clockwise*/
					if (ledangle < Object.LEDInitialAngle) {
						ledangle = Object.LEDInitialAngle;
					}
					if (ledangle < Object.LEDLastAngle) {
						double currentEdgeFloat = Math.Floor(0.636619772367582 * (ledangle + 0.785398163397449));
						int currentEdge = ((int)currentEdgeFloat % 4 + 4) % 4;
						double lastEdgeFloat = Math.Floor(0.636619772367582 * (Object.LEDLastAngle + 0.785398163397449));
						int lastEdge = ((int)lastEdgeFloat % 4 + 4) % 4;
						if (lastEdge < currentEdge | lastEdge == currentEdge & Math.Abs(currentEdgeFloat - lastEdgeFloat) > 2.0) {
							lastEdge += 4;
						}
						if (currentEdge == lastEdge) {
							/* current angle to last angle */
							{
								double t = 0.5 + (0.636619772367582 * ledangle) - currentEdgeFloat;
								if (t < 0.0) {
									t = 0.0;
								} else if (t > 1.0) {
									t = 1.0;
								}
								t = 0.5 * (1.0 - Math.Tan(0.25 * (Math.PI - 2.0 * Math.PI * t)));
								double cx = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].X + t * Object.LEDVectors[currentEdge].X;
								double cy = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].Y + t * Object.LEDVectors[currentEdge].Y;
								double cz = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].Z + t * Object.LEDVectors[currentEdge].Z;
								Object.States[s].Object.Mesh.Vertices[v].Coordinates = new Vector3(cx, cy, cz);
								v++;
							}
							{
								double t = 0.5 + (0.636619772367582 * Object.LEDLastAngle) - lastEdgeFloat;
								if (t < 0.0) {
									t = 0.0;
								} else if (t > 1.0) {
									t = 1.0;
								}
								t = 0.5 * (1.0 - Math.Tan(0.25 * (Math.PI - 2.0 * Math.PI * t)));
								double lx = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].X + t * Object.LEDVectors[lastEdge].X;
								double ly = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].Y + t * Object.LEDVectors[lastEdge].Y;
								double lz = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].Z + t * Object.LEDVectors[lastEdge].Z;
								Object.States[s].Object.Mesh.Vertices[v].Coordinates = new Vector3(lx, ly, lz);
								v++;
							}
						} else {
							{
								/* current angle to square vertex */
								double t = 0.5 + (0.636619772367582 * ledangle) - currentEdgeFloat;
								if (t < 0.0) {
									t = 0.0;
								} else if (t > 1.0) {
									t = 1.0;
								}
								t = 0.5 * (1.0 - Math.Tan(0.25 * (Math.PI - 2.0 * Math.PI * t)));
								double cx = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].X + t * Object.LEDVectors[currentEdge].X;
								double cy = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].Y + t * Object.LEDVectors[currentEdge].Y;
								double cz = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].Z + t * Object.LEDVectors[currentEdge].Z;
								Object.States[s].Object.Mesh.Vertices[v + 0].Coordinates = new Vector3(cx, cy, cz);
								Object.States[s].Object.Mesh.Vertices[v + 1].Coordinates = Object.LEDVectors[currentEdge];
								v += 2;
							}
							for (int j = currentEdge + 1; j < lastEdge; j++) {
								/* square-vertex to square-vertex */
								Object.States[s].Object.Mesh.Vertices[v + 0].Coordinates = Object.LEDVectors[(j + 3) % 4];
								Object.States[s].Object.Mesh.Vertices[v + 1].Coordinates = Object.LEDVectors[j % 4];
								v += 2;
							}
							{
								/* square vertex to last angle */
								double t = 0.5 + (0.636619772367582 * Object.LEDLastAngle) - lastEdgeFloat;
								if (t < 0.0) {
									t = 0.0;
								} else if (t > 1.0) {
									t = 1.0;
								}
								t = 0.5 * (1.0 - Math.Tan(0.25 * (Math.PI - 2.0 * Math.PI * t)));
								double lx = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].X + t * Object.LEDVectors[lastEdge % 4].X;
								double ly = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].Y + t * Object.LEDVectors[lastEdge % 4].Y;
								double lz = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].Z + t * Object.LEDVectors[lastEdge % 4].Z;
								Object.States[s].Object.Mesh.Vertices[v + 0].Coordinates = Object.LEDVectors[(lastEdge + 3) % 4];
								Object.States[s].Object.Mesh.Vertices[v + 1].Coordinates = new Vector3(lx, ly, lz);
								v += 2;
							}
						}
					}
				} else {
					/* winding is counter-clockwise*/
					if (ledangle > Object.LEDInitialAngle) {
						ledangle = Object.LEDInitialAngle;
					}
					if (ledangle > Object.LEDLastAngle) {
						double currentEdgeFloat = Math.Floor(0.636619772367582 * (ledangle + 0.785398163397449));
						int currentEdge = ((int)currentEdgeFloat % 4 + 4) % 4;
						double lastEdgeFloat = Math.Floor(0.636619772367582 * (Object.LEDLastAngle + 0.785398163397449));
						int lastEdge = ((int)lastEdgeFloat % 4 + 4) % 4;
						if (currentEdge < lastEdge | lastEdge == currentEdge & Math.Abs(currentEdgeFloat - lastEdgeFloat) > 2.0) {
							currentEdge += 4;
						}
						if (currentEdge == lastEdge) {
							/* current angle to last angle */
							{
								double t = 0.5 + (0.636619772367582 * Object.LEDLastAngle) - lastEdgeFloat;
								if (t < 0.0) {
									t = 0.0;
								} else if (t > 1.0) {
									t = 1.0;
								}
								t = 0.5 * (1.0 - Math.Tan(0.25 * (Math.PI - 2.0 * Math.PI * t)));
								double lx = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].X + t * Object.LEDVectors[lastEdge].X;
								double ly = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].Y + t * Object.LEDVectors[lastEdge].Y;
								double lz = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].Z + t * Object.LEDVectors[lastEdge].Z;
								Object.States[s].Object.Mesh.Vertices[v].Coordinates = new Vector3(lx, ly, lz);
								v++;
							}
							{
								double t = 0.5 + (0.636619772367582 * ledangle) - currentEdgeFloat;
								if (t < 0.0) {
									t = 0.0;
								} else if (t > 1.0) {
									t = 1.0;
								}
								t = t - Math.Floor(t);
								t = 0.5 * (1.0 - Math.Tan(0.25 * (Math.PI - 2.0 * Math.PI * t)));
								double cx = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].X + t * Object.LEDVectors[currentEdge].X;
								double cy = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].Y + t * Object.LEDVectors[currentEdge].Y;
								double cz = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].Z + t * Object.LEDVectors[currentEdge].Z;
								Object.States[s].Object.Mesh.Vertices[v].Coordinates = new Vector3(cx, cy, cz);
								v++;
							}
						} else {
							{
								/* current angle to square vertex */
								double t = 0.5 + (0.636619772367582 * ledangle) - currentEdgeFloat;
								if (t < 0.0) {
									t = 0.0;
								} else if (t > 1.0) {
									t = 1.0;
								}
								t = 0.5 * (1.0 - Math.Tan(0.25 * (Math.PI - 2.0 * Math.PI * t)));
								double cx = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].X + t * Object.LEDVectors[currentEdge % 4].X;
								double cy = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].Y + t * Object.LEDVectors[currentEdge % 4].Y;
								double cz = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].Z + t * Object.LEDVectors[currentEdge % 4].Z;
								Object.States[s].Object.Mesh.Vertices[v + 0].Coordinates = Object.LEDVectors[(currentEdge + 3) % 4];
								Object.States[s].Object.Mesh.Vertices[v + 1].Coordinates = new Vector3(cx, cy, cz);
								v += 2;
							}
							for (int j = currentEdge - 1; j > lastEdge; j--) {
								/* square-vertex to square-vertex */
								Object.States[s].Object.Mesh.Vertices[v + 0].Coordinates = Object.LEDVectors[(j + 3) % 4];
								Object.States[s].Object.Mesh.Vertices[v + 1].Coordinates = Object.LEDVectors[j % 4];
								v += 2;
							}
							{
								/* square vertex to last angle */
								double t = 0.5 + (0.636619772367582 * Object.LEDLastAngle) - lastEdgeFloat;
								if (t < 0.0) {
									t = 0.0;
								} else if (t > 1.0) {
									t = 1.0;
								}
								t = 0.5 * (1.0 - Math.Tan(0.25 * (Math.PI - 2.0 * Math.PI * t)));
								double lx = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].X + t * Object.LEDVectors[lastEdge].X;
								double ly = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].Y + t * Object.LEDVectors[lastEdge].Y;
								double lz = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].Z + t * Object.LEDVectors[lastEdge].Z;
								Object.States[s].Object.Mesh.Vertices[v + 0].Coordinates = new Vector3(lx, ly, lz);
								Object.States[s].Object.Mesh.Vertices[v + 1].Coordinates = Object.LEDVectors[lastEdge % 4];
								v += 2;
							}
						}
					}
				}
				for (int j = v; v < 11; v++) {
					Object.States[s].Object.Mesh.Vertices[j].Coordinates = Object.LEDVectors[4];
				}
			}
			// update vertices
			for (int k = 0; k < Object.States[s].Object.Mesh.Vertices.Length; k++) {
				// rotate
				if (rotateX) {
					ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Rotate(Object.RotateXDirection, cosX, sinX);
				}
				if (rotateY) {
					ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Rotate(Object.RotateYDirection, cosY, sinY);
				}
				if (rotateZ) {
					ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Rotate(Object.RotateZDirection, cosZ, sinZ);
				}
				// translate
				if (Overlay & World.CameraRestriction != World.CameraRestrictionMode.NotAvailable)
				{
					ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates += Object.States[s].Position - Position;
					ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Rotate(World.AbsoluteCameraDirection, World.AbsoluteCameraUp, World.AbsoluteCameraSide);
					double dx = -Math.Tan(World.CameraCurrentAlignment.Yaw) - World.CameraCurrentAlignment.Position.X;
					double dy = -Math.Tan(World.CameraCurrentAlignment.Pitch) - World.CameraCurrentAlignment.Position.Y;
					double dz = -World.CameraCurrentAlignment.Position.Z;
					ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.X += World.AbsoluteCameraPosition.X + dx * World.AbsoluteCameraSide.X + dy * World.AbsoluteCameraUp.X + dz * World.AbsoluteCameraDirection.X;
					ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Y += World.AbsoluteCameraPosition.Y + dx * World.AbsoluteCameraSide.Y + dy * World.AbsoluteCameraUp.Y + dz * World.AbsoluteCameraDirection.Y;
					ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Z += World.AbsoluteCameraPosition.Z + dx * World.AbsoluteCameraSide.Z + dy * World.AbsoluteCameraUp.Z + dz * World.AbsoluteCameraDirection.Z;
				}
				else
				{
					ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates += Object.States[s].Position;
					ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Rotate(Direction, Up, Side);
					ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates += Position;
				}
			}
			// update normals
			for (int k = 0; k < Object.States[s].Object.Mesh.Faces.Length; k++) {
				for (int h = 0; h < Object.States[s].Object.Mesh.Faces[k].Vertices.Length; h++) {
					ObjectManager.Objects[i].Mesh.Faces[k].Vertices[h].Normal = Object.States[s].Object.Mesh.Faces[k].Vertices[h].Normal;
					if (!Vector3.IsZero(Object.States[s].Object.Mesh.Faces[k].Vertices[h].Normal))
					{
						if (rotateX)
						{
							ObjectManager.Objects[i].Mesh.Faces[k].Vertices[h].Normal.Rotate(Object.RotateXDirection, cosX, sinX);
						}

						if (rotateY)
						{
							ObjectManager.Objects[i].Mesh.Faces[k].Vertices[h].Normal.Rotate(Object.RotateYDirection, cosY, sinY);
						}

						if (rotateZ)
						{
							ObjectManager.Objects[i].Mesh.Faces[k].Vertices[h].Normal.Rotate(Object.RotateZDirection, cosZ, sinZ);
						}
						ObjectManager.Objects[i].Mesh.Faces[k].Vertices[h].Normal.Rotate(Direction, Up, Side);
					}
				}
				// visibility changed
				if (Show) {
					if (Overlay) {
						Renderer.ShowObject(i, ObjectType.Overlay);
					} else {
						Renderer.ShowObject(i, ObjectType.Dynamic);
					}
				} else {
					Renderer.HideObject(i);
				}
			}
		}

		// animated world object
		internal class AnimatedWorldObject {
			internal Vector3 Position;
			internal double TrackPosition;
			internal Vector3 Direction;
			internal Vector3 Up;
			internal Vector3 Side;
			internal AnimatedObject Object;
			internal int SectionIndex;
			internal double Radius;
			internal bool Visible;
		}
		internal static AnimatedWorldObject[] AnimatedWorldObjects = new AnimatedWorldObject[4];
		internal static int AnimatedWorldObjectsUsed = 0;
		internal static void CreateAnimatedWorldObjects(AnimatedObject[] Prototypes, Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, int SectionIndex, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness) {
			bool[] free = new bool[Prototypes.Length];
			bool anyfree = false;
			for (int i = 0; i < Prototypes.Length; i++) {
				free[i] = Prototypes[i].IsFreeOfFunctions();
				if (free[i]) anyfree = true;
			}
			if (anyfree) {
				for (int i = 0; i < Prototypes.Length; i++) {
					if (Prototypes[i].States.Length != 0) {
						if (free[i]) {
							Vector3 p = Position;
							Transformation t = new Transformation(BaseTransformation, AuxTransformation);
							Vector3 s = t.X;
							Vector3 u = t.Y;
							Vector3 d = t.Z;
							p.X += Prototypes[i].States[0].Position.X * s.X + Prototypes[i].States[0].Position.Y * u.X + Prototypes[i].States[0].Position.Z * d.X;
							p.Y += Prototypes[i].States[0].Position.X * s.Y + Prototypes[i].States[0].Position.Y * u.Y + Prototypes[i].States[0].Position.Z * d.Y;
							p.Z += Prototypes[i].States[0].Position.X * s.Z + Prototypes[i].States[0].Position.Y * u.Z + Prototypes[i].States[0].Position.Z * d.Z;
							double zOffset = Prototypes[i].States[0].Position.Z;
							CreateStaticObject(Prototypes[i].States[0].Object, p, BaseTransformation, AuxTransformation, AccurateObjectDisposal, zOffset, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness);
						} else {
							CreateAnimatedWorldObject(Prototypes[i], Position, BaseTransformation, AuxTransformation, SectionIndex, TrackPosition, Brightness);
						}
					}
				}
			} else {
				for (int i = 0; i < Prototypes.Length; i++) {
					if (Prototypes[i].States.Length != 0) {
						CreateAnimatedWorldObject(Prototypes[i], Position, BaseTransformation, AuxTransformation, SectionIndex, TrackPosition, Brightness);
					}
				}
			}
		}
		internal static int CreateAnimatedWorldObject(AnimatedObject Prototype, Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, int SectionIndex, double TrackPosition, double Brightness) {
			int a = AnimatedWorldObjectsUsed;
			if (a >= AnimatedWorldObjects.Length) {
				Array.Resize<AnimatedWorldObject>(ref AnimatedWorldObjects, AnimatedWorldObjects.Length << 1);
			}
			Transformation FinalTransformation = new Transformation(AuxTransformation, BaseTransformation);
			AnimatedWorldObjects[a] = new AnimatedWorldObject();
			AnimatedWorldObjects[a].Position = Position;
			AnimatedWorldObjects[a].Direction = FinalTransformation.Z;
			AnimatedWorldObjects[a].Up = FinalTransformation.Y;
			AnimatedWorldObjects[a].Side = FinalTransformation.X;
			AnimatedWorldObjects[a].Object = Prototype.Clone();
			AnimatedWorldObjects[a].Object.ObjectIndex = CreateDynamicObject();
			AnimatedWorldObjects[a].SectionIndex = SectionIndex;
			AnimatedWorldObjects[a].TrackPosition = TrackPosition;
			for (int i = 0; i < AnimatedWorldObjects[a].Object.States.Length; i++) {
				if (AnimatedWorldObjects[a].Object.States[i].Object == null) {
					AnimatedWorldObjects[a].Object.States[i].Object = new StaticObject();
					AnimatedWorldObjects[a].Object.States[i].Object.RendererIndex = -1;
				}
			}
			double r = 0.0;
			for (int i = 0; i < AnimatedWorldObjects[a].Object.States.Length; i++) {
				for (int j = 0; j < AnimatedWorldObjects[a].Object.States[i].Object.Mesh.Materials.Length; j++) {
					AnimatedWorldObjects[a].Object.States[i].Object.Mesh.Materials[j].Color.R = (byte)Math.Round((double)Prototype.States[i].Object.Mesh.Materials[j].Color.R * Brightness);
					AnimatedWorldObjects[a].Object.States[i].Object.Mesh.Materials[j].Color.G = (byte)Math.Round((double)Prototype.States[i].Object.Mesh.Materials[j].Color.G * Brightness);
					AnimatedWorldObjects[a].Object.States[i].Object.Mesh.Materials[j].Color.B = (byte)Math.Round((double)Prototype.States[i].Object.Mesh.Materials[j].Color.B * Brightness);
				}
				for (int j = 0; j < AnimatedWorldObjects[a].Object.States[i].Object.Mesh.Vertices.Length; j++) {
					double x = Prototype.States[i].Object.Mesh.Vertices[j].Coordinates.X;
					double y = Prototype.States[i].Object.Mesh.Vertices[j].Coordinates.Y;
					double z = Prototype.States[i].Object.Mesh.Vertices[j].Coordinates.Z;
					double t = x * x + y * y + z * z;
					if (t > r) r = t;
				}
			}
			AnimatedWorldObjects[a].Radius = Math.Sqrt(r);
			AnimatedWorldObjects[a].Visible = false;
			InitializeAnimatedObject(ref AnimatedWorldObjects[a].Object, 0, false, false);
			AnimatedWorldObjectsUsed++;
			return a;
		}
		internal static void UpdateAnimatedWorldObjects(double TimeElapsed, bool ForceUpdate) {
			for (int i = 0; i < AnimatedWorldObjectsUsed; i++) {
				const double extraRadius = 10.0;
				double z = AnimatedWorldObjects[i].Object.TranslateZFunction == null ? 0.0 : AnimatedWorldObjects[i].Object.TranslateZFunction.LastResult;
				double pa = AnimatedWorldObjects[i].TrackPosition + z - AnimatedWorldObjects[i].Radius - extraRadius;
				double pb = AnimatedWorldObjects[i].TrackPosition + z + AnimatedWorldObjects[i].Radius + extraRadius;
				double ta = World.CameraTrackFollower.TrackPosition - World.BackgroundImageDistance - World.ExtraViewingDistance;
				double tb = World.CameraTrackFollower.TrackPosition + World.BackgroundImageDistance + World.ExtraViewingDistance;
				bool visible = pb >= ta & pa <= tb;
				if (visible | ForceUpdate) {
					if (AnimatedWorldObjects[i].Object.SecondsSinceLastUpdate >= AnimatedWorldObjects[i].Object.RefreshRate | ForceUpdate) {
						double timeDelta = AnimatedWorldObjects[i].Object.SecondsSinceLastUpdate + TimeElapsed;
						AnimatedWorldObjects[i].Object.SecondsSinceLastUpdate = 0.0;
						TrainManager.Train train = null;
						double trainDistance = double.MaxValue;
						for (int j = 0; j < TrainManager.Trains.Length; j++) {
							if (TrainManager.Trains[j].State == TrainManager.TrainState.Available) {
								double distance;
								if (TrainManager.Trains[j].Cars[0].FrontAxle.Follower.TrackPosition < AnimatedWorldObjects[i].TrackPosition) {
									distance = AnimatedWorldObjects[i].TrackPosition - TrainManager.Trains[j].Cars[0].FrontAxle.Follower.TrackPosition;
								} else if (TrainManager.Trains[j].Cars[TrainManager.Trains[j].Cars.Length - 1].RearAxle.Follower.TrackPosition > AnimatedWorldObjects[i].TrackPosition) {
									distance = TrainManager.Trains[j].Cars[TrainManager.Trains[j].Cars.Length - 1].RearAxle.Follower.TrackPosition - AnimatedWorldObjects[i].TrackPosition;
								} else {
									distance = 0;
								}
								if (distance < trainDistance) {
									train = TrainManager.Trains[j];
									trainDistance = distance;
								}
							}
						}
						UpdateAnimatedObject(ref AnimatedWorldObjects[i].Object, false, train, train == null ? 0 : train.DriverCar, AnimatedWorldObjects[i].SectionIndex, AnimatedWorldObjects[i].TrackPosition, AnimatedWorldObjects[i].Position, AnimatedWorldObjects[i].Direction, AnimatedWorldObjects[i].Up, AnimatedWorldObjects[i].Side, false, true, true, timeDelta);
					} else {
						AnimatedWorldObjects[i].Object.SecondsSinceLastUpdate += TimeElapsed;
					}
					if (!AnimatedWorldObjects[i].Visible) {
						Renderer.ShowObject(AnimatedWorldObjects[i].Object.ObjectIndex, ObjectType.Dynamic);
						AnimatedWorldObjects[i].Visible = true;
					}
				} else {
					AnimatedWorldObjects[i].Object.SecondsSinceLastUpdate += TimeElapsed;
					if (AnimatedWorldObjects[i].Visible) {
						Renderer.HideObject(AnimatedWorldObjects[i].Object.ObjectIndex);
						AnimatedWorldObjects[i].Visible = false;
					}
				}
			}
		}

		// load object
		internal static UnifiedObject LoadObject(string FileName, Encoding Encoding, bool PreserveVertices) {
			#if !DEBUG
			try {
				#endif
				if (!System.IO.Path.HasExtension(FileName)) {
					while (true) {
						string f;
						f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".x");
						if (System.IO.File.Exists(f)) {
							FileName = f;
							break;
						}
						f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".csv");
						if (System.IO.File.Exists(f)) {
							FileName = f;
							break;
						}
						f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".b3d");
						if (System.IO.File.Exists(f)) {
							FileName = f;
							break;
						}
						break;
					}
				}
				UnifiedObject Result;
				switch (System.IO.Path.GetExtension(FileName).ToLowerInvariant()) {
					case ".csv":
					case ".b3d":
						Result = CsvB3dObjectParser.ReadObject(FileName, Encoding);
						break;
					case ".x":
						Result = XObjectParser.ReadObject(FileName, Encoding);
						break;
					case ".animated":
						Result = AnimatedObjectParser.ReadObject(FileName, Encoding);
						break;
					case ".l3dobj":
						Result = Ls3DObjectParser.ReadObject(FileName, Encoding, new Vector3());
						break;
					case ".l3dgrp":
						Result = Ls3DGrpParser.ReadObject(FileName, Encoding, new Vector3());
						break;
					case ".obj":
						Result = WavefrontObjParser.ReadObject(FileName, Encoding);
						break;
				default:
						Interface.AddMessage(MessageType.Error, false, "The file extension is not supported: " + FileName);
						return null;
				}
				Result.OptimizeObject(PreserveVertices);
				return Result;
				#if !DEBUG
			} catch (Exception ex) {
				Interface.AddMessage(MessageType.Error, true, "An unexpected error occured (" + ex.Message + ") while attempting to load the file " + FileName);
				return null;
			}
			#endif
		}
		internal static StaticObject LoadStaticObject(string FileName, Encoding Encoding, bool PreserveVertices) {
			#if !DEBUG
			try {
				#endif
				if (!System.IO.Path.HasExtension(FileName)) {
					while (true) {
						string f;
						f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".x");
						if (System.IO.File.Exists(f)) {
							FileName = f;
							break;
						}
						f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".csv");
						if (System.IO.File.Exists(f)) {
							FileName = f;
							break;
						}
						f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".b3d");
						if (System.IO.File.Exists(f)) {
							FileName = f;
							break;
						}
						break;
					}
				}
				StaticObject Result;
				switch (System.IO.Path.GetExtension(FileName).ToLowerInvariant()) {
					case ".csv":
					case ".b3d":
						Result = CsvB3dObjectParser.ReadObject(FileName, Encoding);
						break;
					case ".x":
						Result = XObjectParser.ReadObject(FileName, Encoding);
						break;
					case ".animated":
						Interface.AddMessage(MessageType.Error, false, "Tried to load an animated object even though only static objects are allowed: " + FileName);
						return null;
					case ".obj":
						Result = WavefrontObjParser.ReadObject(FileName, Encoding);
						break;
				default:
						Interface.AddMessage(MessageType.Error, false, "The file extension is not supported: " + FileName);
						return null;
				}
				Result.OptimizeObject(PreserveVertices);
				return Result;
				#if !DEBUG
			} catch (Exception ex) {
				Interface.AddMessage(MessageType.Error, true, "An unexpected error occured (" + ex.Message + ") while attempting to load the file " + FileName);
				return null;
			}
			#endif
		}

		// create object
		internal static void CreateObject(UnifiedObject Prototype, Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition) {
			CreateObject(Prototype, Position, BaseTransformation, AuxTransformation, -1, AccurateObjectDisposal, StartingDistance, EndingDistance, BlockLength, TrackPosition, 1.0);
		}
		internal static void CreateObject(UnifiedObject Prototype, Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, int SectionIndex, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness) {
			if (Prototype is StaticObject) {
				StaticObject s = (StaticObject)Prototype;
				CreateStaticObject(s, Position, BaseTransformation, AuxTransformation, AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness);
			} else if (Prototype is AnimatedObjectCollection) {
				AnimatedObjectCollection a = (AnimatedObjectCollection)Prototype;
				CreateAnimatedWorldObjects(a.Objects, Position, BaseTransformation, AuxTransformation, SectionIndex, AccurateObjectDisposal, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness);
			}
		}

		// create static object
		internal static int CreateStaticObject(StaticObject Prototype, Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition) {
			return CreateStaticObject(Prototype, Position, BaseTransformation, AuxTransformation, AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, BlockLength, TrackPosition, 1.0);
		}
		internal static int CreateStaticObject(StaticObject Prototype, Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, bool AccurateObjectDisposal, double AccurateObjectDisposalZOffset, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness) {
			int a = ObjectsUsed;
			if (a >= Objects.Length) {
				Array.Resize<StaticObject>(ref Objects, Objects.Length << 1);
			}
			ApplyStaticObjectData(ref Objects[a], Prototype, Position, BaseTransformation, AuxTransformation, AccurateObjectDisposal, AccurateObjectDisposalZOffset, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness);
			for (int i = 0; i < Prototype.Mesh.Faces.Length; i++) {
				switch (Prototype.Mesh.Faces[i].Flags & MeshFace.FaceTypeMask) {
					case MeshFace.FaceTypeTriangles:
						Game.InfoTotalTriangles++;
						break;
					case MeshFace.FaceTypeTriangleStrip:
						Game.InfoTotalTriangleStrip++;
						break;
					case MeshFace.FaceTypeQuads:
						Game.InfoTotalQuads++;
						break;
					case MeshFace.FaceTypeQuadStrip:
						Game.InfoTotalQuadStrip++;
						break;
					case MeshFace.FaceTypePolygon:
						Game.InfoTotalPolygon++;
						break;
				}
			}
			ObjectsUsed++;
			return a;
		}
		internal static void ApplyStaticObjectData(ref StaticObject Object, StaticObject Prototype, Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, bool AccurateObjectDisposal, double AccurateObjectDisposalZOffset, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness) {
			Object = new StaticObject();
			Object.StartingDistance = float.MaxValue;
			Object.EndingDistance = float.MinValue;
			// bool brightnesschange = Brightness != 1.0;
			// vertices
			Object.Mesh.Vertices = new VertexTemplate[Prototype.Mesh.Vertices.Length];
			for (int j = 0; j < Prototype.Mesh.Vertices.Length; j++) {
				if (Prototype.Mesh.Vertices[j] is ColoredVertex)
				{
					Object.Mesh.Vertices[j] = new ColoredVertex((ColoredVertex)Prototype.Mesh.Vertices[j]);
				}
				else
				{
					Object.Mesh.Vertices[j] = new Vertex((Vertex)Prototype.Mesh.Vertices[j]);
				}
				if (AccurateObjectDisposal) {
					Object.Mesh.Vertices[j].Coordinates.Rotate(AuxTransformation);
					if (Object.Mesh.Vertices[j].Coordinates.Z < Object.StartingDistance) {
						Object.StartingDistance = (float)Object.Mesh.Vertices[j].Coordinates.Z;
					}
					if (Object.Mesh.Vertices[j].Coordinates.Z > Object.EndingDistance) {
						Object.EndingDistance = (float)Object.Mesh.Vertices[j].Coordinates.Z;
					}
					Object.Mesh.Vertices[j].Coordinates = Prototype.Mesh.Vertices[j].Coordinates;
				}
				Object.Mesh.Vertices[j].Coordinates.Rotate(AuxTransformation);
				Object.Mesh.Vertices[j].Coordinates.Rotate(BaseTransformation);
				Object.Mesh.Vertices[j].Coordinates.X += Position.X;
				Object.Mesh.Vertices[j].Coordinates.Y += Position.Y;
				Object.Mesh.Vertices[j].Coordinates.Z += Position.Z;
			}
			if (AccurateObjectDisposal) {
				Object.StartingDistance += (float)AccurateObjectDisposalZOffset;
				Object.EndingDistance += (float)AccurateObjectDisposalZOffset;
			}
			// faces
			Object.Mesh.Faces = new MeshFace[Prototype.Mesh.Faces.Length];
			for (int j = 0; j < Prototype.Mesh.Faces.Length; j++) {
				Object.Mesh.Faces[j].Flags = Prototype.Mesh.Faces[j].Flags;
				Object.Mesh.Faces[j].Material = Prototype.Mesh.Faces[j].Material;
				Object.Mesh.Faces[j].Vertices = new MeshFaceVertex[Prototype.Mesh.Faces[j].Vertices.Length];
				for (int k = 0; k < Prototype.Mesh.Faces[j].Vertices.Length; k++) {
					Object.Mesh.Faces[j].Vertices[k] = Prototype.Mesh.Faces[j].Vertices[k];
					double nx = Object.Mesh.Faces[j].Vertices[k].Normal.X;
					double ny = Object.Mesh.Faces[j].Vertices[k].Normal.Y;
					double nz = Object.Mesh.Faces[j].Vertices[k].Normal.Z;
					if (nx * nx + ny * ny + nz * nz != 0.0) {
						Object.Mesh.Faces[j].Vertices[k].Normal.Rotate(AuxTransformation);
						Object.Mesh.Faces[j].Vertices[k].Normal.Rotate(BaseTransformation);
					}
				}
			}
			// materials
			Object.Mesh.Materials = new MeshMaterial[Prototype.Mesh.Materials.Length];
			for (int j = 0; j < Prototype.Mesh.Materials.Length; j++) {
				Object.Mesh.Materials[j] = Prototype.Mesh.Materials[j];
				Object.Mesh.Materials[j].Color.R = (byte)Math.Round((double)Prototype.Mesh.Materials[j].Color.R * Brightness);
				Object.Mesh.Materials[j].Color.G = (byte)Math.Round((double)Prototype.Mesh.Materials[j].Color.G * Brightness);
				Object.Mesh.Materials[j].Color.B = (byte)Math.Round((double)Prototype.Mesh.Materials[j].Color.B * Brightness);
			}
			const double minBlockLength = 20.0;
			if (BlockLength < minBlockLength) {
				BlockLength = BlockLength * Math.Ceiling(minBlockLength / BlockLength);
			}
			if (AccurateObjectDisposal) {
				Object.StartingDistance += (float)TrackPosition;
				Object.EndingDistance += (float)TrackPosition;
				double z = BlockLength * Math.Floor(TrackPosition / BlockLength);
				StartingDistance = Math.Min(z - BlockLength, (double)Object.StartingDistance);
				EndingDistance = Math.Max(z + 2.0 * BlockLength, (double)Object.EndingDistance);
				Object.StartingDistance = (float)(BlockLength * Math.Floor(StartingDistance / BlockLength));
				Object.EndingDistance = (float)(BlockLength * Math.Ceiling(EndingDistance / BlockLength));
			} else {
				Object.StartingDistance = (float)StartingDistance;
				Object.EndingDistance = (float)EndingDistance;
			}
			if (BlockLength != 0.0) {
				checked {
					Object.GroupIndex = (short)Mod(Math.Floor(Object.StartingDistance / BlockLength), Math.Ceiling(World.BackgroundImageDistance / BlockLength));
				}
			}
		}
		
		private static double Mod(double a, double b) {
			return a - b * Math.Floor(a / b);
		}

		// create dynamic object
		internal static int CreateDynamicObject() {
			int a = ObjectsUsed;
			if (a >= Objects.Length) {
				Array.Resize<StaticObject>(ref Objects, Objects.Length << 1);
			}
			Objects[a] = new StaticObject();
			Objects[a].Dynamic = true;
			ObjectsUsed++;
			return a;
		}

		// clone object
		internal static StaticObject CloneObject(StaticObject Prototype, Texture DaytimeTexture, Texture NighttimeTexture) {
			if (Prototype == null) return null;
			StaticObject Result = new StaticObject();
			Result.StartingDistance = Prototype.StartingDistance;
			Result.EndingDistance = Prototype.EndingDistance;
			Result.Dynamic = Prototype.Dynamic;
			// vertices
			Result.Mesh.Vertices = new VertexTemplate[Prototype.Mesh.Vertices.Length];
			for (int j = 0; j < Prototype.Mesh.Vertices.Length; j++) {
				if (Result.Mesh.Vertices[j] is ColoredVertex)
				{
					Result.Mesh.Vertices[j] = new ColoredVertex((ColoredVertex)Prototype.Mesh.Vertices[j]);
				}
				else
				{
					Result.Mesh.Vertices[j] = new Vertex((Vertex)Prototype.Mesh.Vertices[j]);
				}

			}
			// faces
			Result.Mesh.Faces = new MeshFace[Prototype.Mesh.Faces.Length];
			for (int j = 0; j < Prototype.Mesh.Faces.Length; j++) {
				Result.Mesh.Faces[j].Flags = Prototype.Mesh.Faces[j].Flags;
				Result.Mesh.Faces[j].Material = Prototype.Mesh.Faces[j].Material;
				Result.Mesh.Faces[j].Vertices = new MeshFaceVertex[Prototype.Mesh.Faces[j].Vertices.Length];
				for (int k = 0; k < Prototype.Mesh.Faces[j].Vertices.Length; k++) {
					Result.Mesh.Faces[j].Vertices[k] = Prototype.Mesh.Faces[j].Vertices[k];
				}
			}
			// materials
			Result.Mesh.Materials = new MeshMaterial[Prototype.Mesh.Materials.Length];
			for (int j = 0; j < Prototype.Mesh.Materials.Length; j++) {
				Result.Mesh.Materials[j] = Prototype.Mesh.Materials[j];
				if (DaytimeTexture != null) {
					Result.Mesh.Materials[j].DaytimeTexture = DaytimeTexture;
				}
				if (NighttimeTexture != null) {
					Result.Mesh.Materials[j].NighttimeTexture = NighttimeTexture;
				}
			}
			return Result;
		}

		// finish creating objects
		internal static void FinishCreatingObjects() {
			Array.Resize<StaticObject>(ref Objects, ObjectsUsed);
			Array.Resize<AnimatedWorldObject>(ref AnimatedWorldObjects, AnimatedWorldObjectsUsed);
		}

		// initialize visibility
		internal static void InitializeVisibility() {
			// sort objects
			ObjectsSortedByStart = new int[ObjectsUsed];
			ObjectsSortedByEnd = new int[ObjectsUsed];
			double[] a = new double[ObjectsUsed];
			double[] b = new double[ObjectsUsed];
			int n = 0;
			for (int i = 0; i < ObjectsUsed; i++) {
				if (!Objects[i].Dynamic) {
					ObjectsSortedByStart[n] = i;
					ObjectsSortedByEnd[n] = i;
					a[n] = Objects[i].StartingDistance;
					b[n] = Objects[i].EndingDistance;
					n++;
				}
			}
			Array.Resize<int>(ref ObjectsSortedByStart, n);
			Array.Resize<int>(ref ObjectsSortedByEnd, n);
			Array.Resize<double>(ref a, n);
			Array.Resize<double>(ref b, n);
			Array.Sort<double, int>(a, ObjectsSortedByStart);
			Array.Sort<double, int>(b, ObjectsSortedByEnd);
			ObjectsSortedByStartPointer = 0;
			ObjectsSortedByEndPointer = 0;
			// initial visiblity
			double p = World.CameraTrackFollower.TrackPosition + World.CameraCurrentAlignment.Position.Z;
			for (int i = 0; i < ObjectsUsed; i++) {
				if (!Objects[i].Dynamic) {
					if (Objects[i].StartingDistance <= p + World.ForwardViewingDistance & Objects[i].EndingDistance >= p - World.BackwardViewingDistance) {
						Renderer.ShowObject(i, ObjectType.Static);
					}
				}
			}
		}

		// update visibility
		internal static void UpdateVisibility(double TrackPosition, bool ViewingDistanceChanged) {
			if (ViewingDistanceChanged) {
				UpdateVisibility(TrackPosition);
				UpdateVisibility(TrackPosition - 0.001);
				UpdateVisibility(TrackPosition + 0.001);
				UpdateVisibility(TrackPosition);
			} else {
				UpdateVisibility(TrackPosition);
			}
		}
		internal static void UpdateVisibility(double TrackPosition) {
			double d = TrackPosition - LastUpdatedTrackPosition;
			int n = ObjectsSortedByStart.Length;
			double p = World.CameraTrackFollower.TrackPosition + World.CameraCurrentAlignment.Position.Z;
			if (d < 0.0) {
				if (ObjectsSortedByStartPointer >= n) ObjectsSortedByStartPointer = n - 1;
				if (ObjectsSortedByEndPointer >= n) ObjectsSortedByEndPointer = n - 1;
				// dispose
				while (ObjectsSortedByStartPointer >= 0) {
					int o = ObjectsSortedByStart[ObjectsSortedByStartPointer];
					if (Objects[o].StartingDistance > p + World.ForwardViewingDistance) {
						Renderer.HideObject(o);
						ObjectsSortedByStartPointer--;
					} else {
						break;
					}
				}
				// introduce
				while (ObjectsSortedByEndPointer >= 0) {
					int o = ObjectsSortedByEnd[ObjectsSortedByEndPointer];
					if (Objects[o].EndingDistance >= p - World.BackwardViewingDistance) {
						if (Objects[o].StartingDistance <= p + World.ForwardViewingDistance) {
							Renderer.ShowObject(o, ObjectType.Static);
						}
						ObjectsSortedByEndPointer--;
					} else {
						break;
					}
				}
			} else if (d > 0.0) {
				if (ObjectsSortedByStartPointer < 0) ObjectsSortedByStartPointer = 0;
				if (ObjectsSortedByEndPointer < 0) ObjectsSortedByEndPointer = 0;
				// dispose
				while (ObjectsSortedByEndPointer < n) {
					int o = ObjectsSortedByEnd[ObjectsSortedByEndPointer];
					if (Objects[o].EndingDistance < p - World.BackwardViewingDistance) {
						Renderer.HideObject(o);
						ObjectsSortedByEndPointer++;
					} else {
						break;
					}
				}
				// introduce
				while (ObjectsSortedByStartPointer < n) {
					int o = ObjectsSortedByStart[ObjectsSortedByStartPointer];
					if (Objects[o].StartingDistance <= p + World.ForwardViewingDistance) {
						if (Objects[o].EndingDistance >= p - World.BackwardViewingDistance) {
							Renderer.ShowObject(o, ObjectType.Static);
						}
						ObjectsSortedByStartPointer++;
					} else {
						break;
					}
				}
			}
			LastUpdatedTrackPosition = TrackPosition;
		}

	}
}
