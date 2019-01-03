// ╔═════════════════════════════════════════════════════════════╗
// ║ World.cs for Object Viewer and Route Viewer                 ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using System;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Runtime;
using OpenBveApi.Textures;
using OpenBveApi.World;

namespace OpenBve {
	public static class World {
		// mesh material
		/// <summary>Represents material properties.</summary>
		internal struct MeshMaterial {
			/// <summary>A bit mask combining constants of the MeshMaterial structure.</summary>
			internal byte Flags;
			internal Color32 Color;
			internal Color24 TransparentColor;
			internal Color24 EmissiveColor;
			internal Texture DaytimeTexture;
			internal Texture NighttimeTexture;
			/// <summary>A value between 0 (daytime) and 255 (nighttime).</summary>
			internal byte DaytimeNighttimeBlend;
			internal MeshMaterialBlendMode BlendMode;
			/// <summary>A bit mask specifying the glow properties. Use GetGlowAttenuationData to create valid data for this field.</summary>
			internal ushort GlowAttenuationData;
			internal const int EmissiveColorMask = 1;
			internal const int TransparentColorMask = 2;
			internal OpenGlTextureWrapMode? WrapMode;
			// operators
			public static bool operator ==(MeshMaterial A, MeshMaterial B) {
				if (A.Flags != B.Flags) return false;
				if (A.Color.R != B.Color.R | A.Color.G != B.Color.G | A.Color.B != B.Color.B | A.Color.A != B.Color.A) return false;
				if (A.TransparentColor.R != B.TransparentColor.R | A.TransparentColor.G != B.TransparentColor.G | A.TransparentColor.B != B.TransparentColor.B) return false;
				if (A.EmissiveColor.R != B.EmissiveColor.R | A.EmissiveColor.G != B.EmissiveColor.G | A.EmissiveColor.B != B.EmissiveColor.B) return false;
				if (A.DaytimeTexture != B.DaytimeTexture) return false;
				if (A.NighttimeTexture != B.NighttimeTexture) return false;
				if (A.BlendMode != B.BlendMode) return false;
				if (A.GlowAttenuationData != B.GlowAttenuationData) return false;
				return true;
			}
			public static bool operator !=(MeshMaterial A, MeshMaterial B) {
				if (A.Flags != B.Flags) return true;
				if (A.Color.R != B.Color.R | A.Color.G != B.Color.G | A.Color.B != B.Color.B | A.Color.A != B.Color.A) return true;
				if (A.TransparentColor.R != B.TransparentColor.R | A.TransparentColor.G != B.TransparentColor.G | A.TransparentColor.B != B.TransparentColor.B) return true;
				if (A.EmissiveColor.R != B.EmissiveColor.R | A.EmissiveColor.G != B.EmissiveColor.G | A.EmissiveColor.B != B.EmissiveColor.B) return true;
				if (A.DaytimeTexture != B.DaytimeTexture) return true;
				if (A.NighttimeTexture != B.NighttimeTexture) return true;
				if (A.BlendMode != B.BlendMode) return true;
				if (A.GlowAttenuationData != B.GlowAttenuationData) return true;
				return false;
			}
			public override int GetHashCode() {
				return base.GetHashCode();
			}
			public override bool Equals(object obj) {
				return base.Equals(obj);
			}
		}
		internal enum MeshMaterialBlendMode : byte {
			Normal = 0,
			Additive = 1
		}
		
		// mesh face vertex
		/// <summary>Represents a reference to a vertex and the normal to be used for that vertex.</summary>
		internal struct MeshFaceVertex {
			/// <summary>A reference to an element in the Vertex array of the contained Mesh structure.</summary>
			internal ushort Index;
			/// <summary>The normal to be used at the vertex.</summary>
			internal Vector3 Normal;
			internal MeshFaceVertex(int Index) {
				this.Index = (ushort)Index;
				this.Normal = new Vector3(0.0f, 0.0f, 0.0f);
			}
			internal MeshFaceVertex(int Index, Vector3 Normal) {
				this.Index = (ushort)Index;
				this.Normal = Normal;
			}
			// operators
			public static bool operator ==(MeshFaceVertex A, MeshFaceVertex B) {
				if (A.Index != B.Index) return false;
				if (A.Normal.X != B.Normal.X) return false;
				if (A.Normal.Y != B.Normal.Y) return false;
				if (A.Normal.Z != B.Normal.Z) return false;
				return true;
			}
			public static bool operator !=(MeshFaceVertex A, MeshFaceVertex B) {
				if (A.Index != B.Index) return true;
				if (A.Normal.X != B.Normal.X) return true;
				if (A.Normal.Y != B.Normal.Y) return true;
				if (A.Normal.Z != B.Normal.Z) return true;
				return false;
			}
			public override int GetHashCode() {
				return base.GetHashCode();
			}
			public override bool Equals(object obj) {
				return base.Equals(obj);
			}
		}
		
		// mesh face
		/// <summary>Represents a face consisting of vertices and material attributes.</summary>
		internal struct MeshFace {
			internal MeshFaceVertex[] Vertices;
			/// <summary>A reference to an element in the Material array of the containing Mesh structure.</summary>
			internal ushort Material;
			/// <summary>A bit mask combining constants of the MeshFace structure.</summary>
			internal byte Flags;
			internal MeshFace(int[] Vertices) {
				this.Vertices = new MeshFaceVertex[Vertices.Length];
				for (int i = 0; i < Vertices.Length; i++) {
					this.Vertices[i] = new MeshFaceVertex(Vertices[i]);
				}
				this.Material = 0;
				this.Flags = 0;
			}
			internal MeshFace(MeshFaceVertex[] verticies, ushort material)
			{
				this.Vertices = verticies;
				this.Material = material;
				this.Flags = 0;
			}
			internal void Flip() {
				if ((this.Flags & FaceTypeMask) == FaceTypeQuadStrip) {
					for (int i = 0; i < this.Vertices.Length; i += 2) {
						MeshFaceVertex x = this.Vertices[i];
						this.Vertices[i] = this.Vertices[i + 1];
						this.Vertices[i + 1] = x;
					}
				} else {
					int n = this.Vertices.Length;
					for (int i = 0; i < (n >> 1); i++) {
						MeshFaceVertex x = this.Vertices[i];
						this.Vertices[i] = this.Vertices[n - i - 1];
						this.Vertices[n - i - 1] = x;
					}
				}
			}
			internal const int FaceTypeMask = 7;
			internal const int FaceTypePolygon = 0;
			internal const int FaceTypeTriangles = 1;
			internal const int FaceTypeTriangleStrip = 2;
			internal const int FaceTypeQuads = 3;
			internal const int FaceTypeQuadStrip = 4;
			internal const int Face2Mask = 8;
		}
		
		// mesh
		/// <summary>Represents a mesh consisting of a series of vertices, faces and material properties.</summary>
		internal struct Mesh {
			internal VertexTemplate[] Vertices;
			internal MeshMaterial[] Materials;
			internal MeshFace[] Faces;
			/// <summary>Creates a mesh consisting of one face, which is represented by individual vertices, and a color.</summary>
			/// <param name="Vertices">The vertices that make up one face.</param>
			/// <param name="Color">The color to be applied on the face.</param>
			internal Mesh(VertexTemplate[] Vertices, Color32 Color) {
				this.Vertices = Vertices;
				this.Materials = new MeshMaterial[1];
				this.Materials[0].Color = Color;
				this.Materials[0].DaytimeTexture = null;
				this.Materials[0].NighttimeTexture = null;
				this.Faces = new MeshFace[1];
				this.Faces[0].Material = 0;
				this.Faces[0].Vertices = new MeshFaceVertex[Vertices.Length];
				for (int i = 0; i < Vertices.Length; i++) {
					this.Faces[0].Vertices[i].Index = (ushort)i;
				}
			}
			/// <summary>Creates a mesh consisting of the specified vertices, faces and color.</summary>
			/// <param name="Vertices">The vertices used.</param>
			/// <param name="FaceVertices">A list of faces represented by a list of references to vertices.</param>
			/// <param name="Color">The color to be applied on all of the faces.</param>
			internal Mesh(VertexTemplate[] Vertices, int[][] FaceVertices, Color32 Color) {
				this.Vertices = Vertices;
				this.Materials = new MeshMaterial[1];
				this.Materials[0].Color = Color;
				this.Materials[0].DaytimeTexture = null;
				this.Materials[0].NighttimeTexture = null;
				this.Faces = new MeshFace[FaceVertices.Length];
				for (int i = 0; i < FaceVertices.Length; i++) {
					this.Faces[i] = new MeshFace(FaceVertices[i]);
				}
			}

			/// <summary>Creates the normals for all faces within this mesh</summary>
			internal void CreateNormals()
			{
				for (int i = 0; i < Faces.Length; i++)
				{
					CreateNormals(i);
				}
			}

			/// <summary>Creates the normals for the specified face index</summary>
			private void CreateNormals(int FaceIndex)
			{
				if (Faces[FaceIndex].Vertices.Length >= 3)
				{
					int i0 = (int)Faces[FaceIndex].Vertices[0].Index;
					int i1 = (int)Faces[FaceIndex].Vertices[1].Index;
					int i2 = (int)Faces[FaceIndex].Vertices[2].Index;
					double ax = Vertices[i1].Coordinates.X - Vertices[i0].Coordinates.X;
					double ay = Vertices[i1].Coordinates.Y - Vertices[i0].Coordinates.Y;
					double az = Vertices[i1].Coordinates.Z - Vertices[i0].Coordinates.Z;
					double bx = Vertices[i2].Coordinates.X - Vertices[i0].Coordinates.X;
					double by = Vertices[i2].Coordinates.Y - Vertices[i0].Coordinates.Y;
					double bz = Vertices[i2].Coordinates.Z - Vertices[i0].Coordinates.Z;
					double nx = ay * bz - az * by;
					double ny = az * bx - ax * bz;
					double nz = ax * by - ay * bx;
					double t = nx * nx + ny * ny + nz * nz;
					if (t != 0.0)
					{
						t = 1.0 / Math.Sqrt(t);
						float mx = (float)(nx * t);
						float my = (float)(ny * t);
						float mz = (float)(nz * t);
						for (int j = 0; j < Faces[FaceIndex].Vertices.Length; j++)
						{
							if (Vector3.IsZero(Faces[FaceIndex].Vertices[j].Normal))
							{
								Faces[FaceIndex].Vertices[j].Normal = new Vector3(mx, my, mz);
							}
						}
					}
					else
					{
						for (int j = 0; j < Faces[FaceIndex].Vertices.Length; j++)
						{
							if (Vector3.IsZero(Faces[FaceIndex].Vertices[j].Normal))
							{
								Faces[FaceIndex].Vertices[j].Normal = new Vector3(0.0f, 1.0f, 0.0f);
							}
						}
					}
				}
			}
		}

		/// <summary>Creates glow attenuation data from a half distance and a mode. The resulting value can be later passed to SplitGlowAttenuationData in order to reconstruct the parameters.</summary>
		/// <param name="HalfDistance">The distance at which the glow is at 50% of its full intensity. The value is clamped to the integer range from 1 to 4096. Values less than or equal to 0 disable glow attenuation.</param>
		/// <param name="Mode">The glow attenuation mode.</param>
		/// <returns>A System.UInt16 packed with the information about the half distance and glow attenuation mode.</returns>
		internal static ushort GetGlowAttenuationData(double HalfDistance, GlowAttenuationMode Mode) {
			if (HalfDistance <= 0.0 | Mode == GlowAttenuationMode.None) return 0;
			if (HalfDistance < 1.0) {
				HalfDistance = 1.0;
			} else if (HalfDistance > 4095.0) {
				HalfDistance = 4095.0;
			}
			return (ushort)((int)Math.Round(HalfDistance) | ((int)Mode << 12));
		}
		/// <summary>Recreates the half distance and the glow attenuation mode from a packed System.UInt16 that was created by GetGlowAttenuationData.</summary>
		/// <param name="Data">The data returned by GetGlowAttenuationData.</param>
		/// <param name="Mode">The mode of glow attenuation.</param>
		/// <param name="HalfDistance">The half distance of glow attenuation.</param>
		internal static void SplitGlowAttenuationData(ushort Data, out GlowAttenuationMode Mode, out double HalfDistance) {
			Mode = (GlowAttenuationMode)(Data >> 12);
			HalfDistance = (double)(Data & 4095);
		}

		// display
		internal static double HorizontalViewingAngle;
		internal static double VerticalViewingAngle;
		internal static double OriginalVerticalViewingAngle;
		internal static double AspectRatio;
		internal static double ForwardViewingDistance = 100000.0;
		internal static double BackwardViewingDistance = 100000.0;
		internal static double ExtraViewingDistance = 1000.0;
		internal static double BackgroundImageDistance = 100000.0;
		internal struct Background {
			internal int Texture;
			internal int Repetition;
			internal bool KeepAspectRatio;
			internal Background(int Texture, int Repetition, bool KeepAspectRatio) {
				this.Texture = Texture;
				this.Repetition = Repetition;
				this.KeepAspectRatio = KeepAspectRatio;
			}
		}
		internal static Background CurrentBackground = new Background(-1, 6, false);
		internal static Background TargetBackground = new Background(-1, 6, false);
		internal const double TargetBackgroundDefaultCountdown = 0.8;
		internal const double TargetBackgroundCountdown = 0.0; // static

		// relative camera
		internal struct CameraAlignment {
			internal Vector3 Position;
			internal double Yaw;
			internal double Pitch;
			internal double Roll;
			internal double TrackPosition;
			internal double Zoom;
			internal CameraAlignment(Vector3 Position, double Yaw, double Pitch, double Roll, double TrackPosition, double Zoom) {
				this.Position = Position;
				this.Yaw = Yaw;
				this.Pitch = Pitch;
				this.Roll = Roll;
				this.TrackPosition = TrackPosition;
				this.Zoom = Zoom;
			}
		}
#pragma warning disable 0649
		internal static TrackManager.TrackFollower CameraTrackFollower;
		internal static CameraAlignment CameraCurrentAlignment;
		internal static CameraAlignment CameraAlignmentDirection;
		internal static CameraAlignment CameraAlignmentSpeed;
		internal static double CameraSpeed;
		internal const double CameraInteriorTopSpeed = 1.0;
		internal const double CameraInteriorTopAngularSpeed = 2.0;
		internal const double CameraExteriorTopSpeed = 50.0;
		internal const double CameraExteriorTopAngularSpeed = 5.0;
		internal const double CameraZoomTopSpeed = 2.0;
		internal static CameraViewMode CameraMode;
#pragma warning restore 0649

		// camera restriction
		internal enum CameraRestrictionMode {
			NotAvailable = -1,
			Off = 0,
			On = 1
		}
		internal static CameraRestrictionMode CameraRestriction = CameraRestrictionMode.NotAvailable;
		
		// absolute camera
		internal static Vector3 AbsoluteCameraPosition;
		internal static Vector3 AbsoluteCameraDirection;
		internal static Vector3 AbsoluteCameraUp;
		internal static Vector3 AbsoluteCameraSide;

		// update absolute camera
		internal static void UpdateAbsoluteCamera(double TimeElapsed) {
			// zoom
			double zm = World.CameraCurrentAlignment.Zoom;
			AdjustAlignment(ref World.CameraCurrentAlignment.Zoom, World.CameraAlignmentDirection.Zoom, ref World.CameraAlignmentSpeed.Zoom, TimeElapsed, World.CameraAlignmentSpeed.Zoom != 0.0);
			if (zm != World.CameraCurrentAlignment.Zoom) {
				ApplyZoom();
			}
			// current alignment
			AdjustAlignment(ref World.CameraCurrentAlignment.Position.X, World.CameraAlignmentDirection.Position.X, ref World.CameraAlignmentSpeed.Position.X, TimeElapsed);
			AdjustAlignment(ref World.CameraCurrentAlignment.Position.Y, World.CameraAlignmentDirection.Position.Y, ref World.CameraAlignmentSpeed.Position.Y, TimeElapsed);
			bool q = World.CameraAlignmentSpeed.Yaw != 0.0 | World.CameraAlignmentSpeed.Pitch != 0.0 | World.CameraAlignmentSpeed.Roll != 0.0;
			AdjustAlignment(ref World.CameraCurrentAlignment.Yaw, World.CameraAlignmentDirection.Yaw, ref World.CameraAlignmentSpeed.Yaw, TimeElapsed);
			AdjustAlignment(ref World.CameraCurrentAlignment.Pitch, World.CameraAlignmentDirection.Pitch, ref World.CameraAlignmentSpeed.Pitch, TimeElapsed);
			AdjustAlignment(ref World.CameraCurrentAlignment.Roll, World.CameraAlignmentDirection.Roll, ref World.CameraAlignmentSpeed.Roll, TimeElapsed);
			double tr = World.CameraCurrentAlignment.TrackPosition;
			AdjustAlignment(ref World.CameraCurrentAlignment.TrackPosition, World.CameraAlignmentDirection.TrackPosition, ref World.CameraAlignmentSpeed.TrackPosition, TimeElapsed);
			if (tr != World.CameraCurrentAlignment.TrackPosition) {
				TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, World.CameraCurrentAlignment.TrackPosition, true, false);
				q = true;
			}
			if (q) {
				UpdateViewingDistances();
			}

			Vector3 dF = new Vector3(CameraTrackFollower.WorldDirection);
			Vector3 uF = new Vector3(CameraTrackFollower.WorldUp);
			Vector3 sF = new Vector3(CameraTrackFollower.WorldSide);
			Vector3 pF = new Vector3(CameraCurrentAlignment.Position);
			Vector3 dx2 = new Vector3(dF);
			Vector3 ux2 = new Vector3(uF);
			if (World.CameraCurrentAlignment.Yaw != 0.0) {
				double cosa = Math.Cos(World.CameraCurrentAlignment.Yaw);
				double sina = Math.Sin(World.CameraCurrentAlignment.Yaw);
				dF.Rotate(uF, cosa, sina);
				sF.Rotate(uF, cosa, sina);
			}
			double p = World.CameraCurrentAlignment.Pitch;
			if (p != 0.0) {
				double cosa = Math.Cos(-p);
				double sina = Math.Sin(-p);
				dF.Rotate(sF, cosa, sina);
				uF.Rotate(sF, cosa, sina);
				uF.Rotate(sF, cosa, sina);
			}
			if (World.CameraCurrentAlignment.Roll != 0.0) {
				double cosa = Math.Cos(-World.CameraCurrentAlignment.Roll);
				double sina = Math.Sin(-World.CameraCurrentAlignment.Roll);
				uF.Rotate(dF, cosa, sina);
				sF.Rotate(dF, cosa, sina);
			}
			AbsoluteCameraPosition = new Vector3(CameraTrackFollower.WorldPosition.X + sF.X * pF.X + ux2.X * pF.Y + dx2.X * pF.Z, CameraTrackFollower.WorldPosition.Y + sF.Y * pF.X + ux2.Y * pF.Y + dx2.Y * pF.Z, CameraTrackFollower.WorldPosition.Z + sF.Z * pF.X + ux2.Z * pF.Y + dx2.Z * pF.Z);
			AbsoluteCameraDirection = dF;
			AbsoluteCameraUp = uF;
			AbsoluteCameraSide = sF;
		}
		private static void AdjustAlignment(ref double Source, double Direction, ref double Speed, double TimeElapsed) {
			AdjustAlignment(ref Source, Direction, ref Speed, TimeElapsed, false);
		}
		private static void AdjustAlignment(ref double Source, double Direction, ref double Speed, double TimeElapsed, bool Zoom) {
			if (TimeElapsed > 0.0) {
				if (Direction == 0.0) {
					double d = (0.025 + 5.0 * Math.Abs(Speed)) * TimeElapsed;
					if (Speed >= -d & Speed <= d) {
						Speed = 0.0;
					} else {
						Speed -= (double)Math.Sign(Speed) * d;
					}
				} else {
					double t = Math.Abs(Direction);
					double d = ((1.15 - 1.0 / (1.0 + 0.025 * Math.Abs(Speed)))) * TimeElapsed;
					Speed += Direction * d;
					if (Speed < -t) {
						Speed = -t;
					} else if (Speed > t) {
						Speed = t;
					}
				}
				Source += Speed * TimeElapsed;
			}
		}
		private static void ApplyZoom() {
			World.VerticalViewingAngle = World.OriginalVerticalViewingAngle * Math.Exp(World.CameraCurrentAlignment.Zoom);
			if (World.VerticalViewingAngle < 0.001) World.VerticalViewingAngle = 0.001;
			if (World.VerticalViewingAngle > 1.5) World.VerticalViewingAngle = 1.5;
			Program.UpdateViewport();
		}

		// update viewing distance
		internal static void UpdateViewingDistances() {
			double f = Math.Atan2(World.CameraTrackFollower.WorldDirection.Z, World.CameraTrackFollower.WorldDirection.X);
			double c = Math.Atan2(World.AbsoluteCameraDirection.Z, World.AbsoluteCameraDirection.X) - f;
			if (c < -Math.PI) {
				c += 2.0 * Math.PI;
			} else if (c > Math.PI) {
				c -= 2.0 * Math.PI;
			}
			double a0 = c - 0.5 * World.HorizontalViewingAngle;
			double a1 = c + 0.5 * World.HorizontalViewingAngle;
			double max;
			if (a0 <= 0.0 & a1 >= 0.0) {
				max = 1.0;
			} else {
				double c0 = Math.Cos(a0);
				double c1 = Math.Cos(a1);
				max = c0 > c1 ? c0 : c1;
				if (max < 0.0) max = 0.0;
			}
			double min;
			if (a0 <= -Math.PI | a1 >= Math.PI) {
				min = -1.0;
			} else {
				double c0 = Math.Cos(a0);
				double c1 = Math.Cos(a1);
				min = c0 < c1 ? c0 : c1;
				if (min > 0.0) min = 0.0;
			}
			double d = World.BackgroundImageDistance + World.ExtraViewingDistance;
			World.ForwardViewingDistance = d * max;
			World.BackwardViewingDistance = -d * min;
			ObjectManager.UpdateVisibility(World.CameraTrackFollower.TrackPosition + World.CameraCurrentAlignment.Position.Z, true);
		}
	}
}
