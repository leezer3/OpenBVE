// ╔═════════════════════════════════════════════════════════════╗
// ║ World.cs for Object Viewer and Route Viewer                 ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using System;
using OpenBveApi.Colors;
using OpenBveApi.Math;

namespace OpenBve {
	public static class World {
		// vertices
		/// <summary>Represents a vertex consisting of 3D coordinates and 2D texture coordinates.</summary>
		internal struct Vertex {
			internal Vector3 Coordinates;
			internal Vector2 TextureCoordinates;
			internal Vertex(double X, double Y, double Z) {
				this.Coordinates = new Vector3(X, Y, Z);
				this.TextureCoordinates = new Vector2(0.0f, 0.0f);
			}
			internal Vertex(Vector3 Coordinates, Vector2 TextureCoordinates) {
				this.Coordinates = Coordinates;
				this.TextureCoordinates = TextureCoordinates;
			}
			internal static bool Equals(Vertex A, Vertex B) {
				if (A.Coordinates.X != B.Coordinates.X | A.Coordinates.Y != B.Coordinates.Y | A.Coordinates.Z != B.Coordinates.Z) return false;
				if (A.TextureCoordinates.X != B.TextureCoordinates.X | A.TextureCoordinates.Y != B.TextureCoordinates.Y) return false;
				return true;
			}
			// operators
			public static bool operator ==(Vertex A, Vertex B) {
				if (A.Coordinates.X != B.Coordinates.X | A.Coordinates.Y != B.Coordinates.Y | A.Coordinates.Z != B.Coordinates.Z) return false;
				if (A.TextureCoordinates.X != B.TextureCoordinates.X | A.TextureCoordinates.Y != B.TextureCoordinates.Y) return false;
				return true;
			}
			public static bool operator !=(Vertex A, Vertex B) {
				if (A.Coordinates.X != B.Coordinates.X | A.Coordinates.Y != B.Coordinates.Y | A.Coordinates.Z != B.Coordinates.Z) return true;
				if (A.TextureCoordinates.X != B.TextureCoordinates.X | A.TextureCoordinates.Y != B.TextureCoordinates.Y) return true;
				return false;
			}
			public override int GetHashCode() {
				return base.GetHashCode();
			}
			public override bool Equals(object obj) {
				return base.Equals(obj);
			}
		}

		// mesh material
		/// <summary>Represents material properties.</summary>
		internal struct MeshMaterial {
			/// <summary>A bit mask combining constants of the MeshMaterial structure.</summary>
			internal byte Flags;
			internal Color32 Color;
			internal Color24 TransparentColor;
			internal Color24 EmissiveColor;
			internal int DaytimeTextureIndex;
			internal int NighttimeTextureIndex;
			/// <summary>A value between 0 (daytime) and 255 (nighttime).</summary>
			internal byte DaytimeNighttimeBlend;
			internal MeshMaterialBlendMode BlendMode;
			/// <summary>A bit mask specifying the glow properties. Use GetGlowAttenuationData to create valid data for this field.</summary>
			internal ushort GlowAttenuationData;
			internal const int EmissiveColorMask = 1;
			internal const int TransparentColorMask = 2;
			// operators
			public static bool operator ==(MeshMaterial A, MeshMaterial B) {
				if (A.Flags != B.Flags) return false;
				if (A.Color.R != B.Color.R | A.Color.G != B.Color.G | A.Color.B != B.Color.B | A.Color.A != B.Color.A) return false;
				if (A.TransparentColor.R != B.TransparentColor.R | A.TransparentColor.G != B.TransparentColor.G | A.TransparentColor.B != B.TransparentColor.B) return false;
				if (A.EmissiveColor.R != B.EmissiveColor.R | A.EmissiveColor.G != B.EmissiveColor.G | A.EmissiveColor.B != B.EmissiveColor.B) return false;
				if (A.DaytimeTextureIndex != B.DaytimeTextureIndex) return false;
				if (A.NighttimeTextureIndex != B.NighttimeTextureIndex) return false;
				if (A.BlendMode != B.BlendMode) return false;
				if (A.GlowAttenuationData != B.GlowAttenuationData) return false;
				return true;
			}
			public static bool operator !=(MeshMaterial A, MeshMaterial B) {
				if (A.Flags != B.Flags) return true;
				if (A.Color.R != B.Color.R | A.Color.G != B.Color.G | A.Color.B != B.Color.B | A.Color.A != B.Color.A) return true;
				if (A.TransparentColor.R != B.TransparentColor.R | A.TransparentColor.G != B.TransparentColor.G | A.TransparentColor.B != B.TransparentColor.B) return true;
				if (A.EmissiveColor.R != B.EmissiveColor.R | A.EmissiveColor.G != B.EmissiveColor.G | A.EmissiveColor.B != B.EmissiveColor.B) return true;
				if (A.DaytimeTextureIndex != B.DaytimeTextureIndex) return true;
				if (A.NighttimeTextureIndex != B.NighttimeTextureIndex) return true;
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
			internal Vertex[] Vertices;
			internal MeshMaterial[] Materials;
			internal MeshFace[] Faces;
			/// <summary>Creates a mesh consisting of one face, which is represented by individual vertices, and a color.</summary>
			/// <param name="Vertices">The vertices that make up one face.</param>
			/// <param name="Color">The color to be applied on the face.</param>
			internal Mesh(Vertex[] Vertices, Color32 Color) {
				this.Vertices = Vertices;
				this.Materials = new MeshMaterial[1];
				this.Materials[0].Color = Color;
				this.Materials[0].DaytimeTextureIndex = -1;
				this.Materials[0].NighttimeTextureIndex = -1;
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
			internal Mesh(Vertex[] Vertices, int[][] FaceVertices, Color32 Color) {
				this.Vertices = Vertices;
				this.Materials = new MeshMaterial[1];
				this.Materials[0].Color = Color;
				this.Materials[0].DaytimeTextureIndex = -1;
				this.Materials[0].NighttimeTextureIndex = -1;
				this.Faces = new MeshFace[FaceVertices.Length];
				for (int i = 0; i < FaceVertices.Length; i++) {
					this.Faces[i] = new MeshFace(FaceVertices[i]);
				}
			}
		}

		// glow
		internal enum GlowAttenuationMode {
			None = 0,
			DivisionExponent2 = 1,
			DivisionExponent4 = 2,
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
		internal static double ForwardViewingDistance;
		internal static double BackwardViewingDistance;
		internal static double ExtraViewingDistance;
		internal static double BackgroundImageDistance;
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
		internal static double TargetBackgroundCountdown;

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
		internal static TrackManager.TrackFollower CameraTrackFollower;
		internal static CameraAlignment CameraCurrentAlignment;
		internal static CameraAlignment CameraAlignmentDirection;
		internal static CameraAlignment CameraAlignmentSpeed;
		internal const double CameraSpeed = 0.0;
		internal const double CameraInteriorTopSpeed = 1.0;
		internal const double CameraInteriorTopAngularSpeed = 2.0;
		internal const double CameraExteriorTopSpeed = 50.0;
		internal const double CameraExteriorTopAngularSpeed = 5.0;
		internal const double CameraZoomTopSpeed = 2.0;
		internal enum CameraViewMode { Interior, InteriorLookAhead, Exterior, Track, FlyBy, FlyByZooming }
		internal static CameraViewMode CameraMode;

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
			double dx = World.CameraTrackFollower.WorldDirection.X;
			double dy = World.CameraTrackFollower.WorldDirection.Y;
			double dz = World.CameraTrackFollower.WorldDirection.Z;
			double ux = World.CameraTrackFollower.WorldUp.X;
			double uy = World.CameraTrackFollower.WorldUp.Y;
			double uz = World.CameraTrackFollower.WorldUp.Z;
			double sx = World.CameraTrackFollower.WorldSide.X;
			double sy = World.CameraTrackFollower.WorldSide.Y;
			double sz = World.CameraTrackFollower.WorldSide.Z;
			double tx = World.CameraCurrentAlignment.Position.X;
			double ty = World.CameraCurrentAlignment.Position.Y;
			double tz = World.CameraCurrentAlignment.Position.Z;
			double dx2 = dx, dy2 = dy, dz2 = dz;
			double ux2 = ux, uy2 = uy, uz2 = uz;
			double cx = World.CameraTrackFollower.WorldPosition.X + sx * tx + ux2 * ty + dx2 * tz;
			double cy = World.CameraTrackFollower.WorldPosition.Y + sy * tx + uy2 * ty + dy2 * tz;
			double cz = World.CameraTrackFollower.WorldPosition.Z + sz * tx + uz2 * ty + dz2 * tz;
			if (World.CameraCurrentAlignment.Yaw != 0.0) {
				double cosa = Math.Cos(World.CameraCurrentAlignment.Yaw);
				double sina = Math.Sin(World.CameraCurrentAlignment.Yaw);
				World.Rotate(ref dx, ref dy, ref dz, ux, uy, uz, cosa, sina);
				World.Rotate(ref sx, ref sy, ref sz, ux, uy, uz, cosa, sina);
			}
			double p = World.CameraCurrentAlignment.Pitch;
			if (p != 0.0) {
				double cosa = Math.Cos(-p);
				double sina = Math.Sin(-p);
				World.Rotate(ref dx, ref dy, ref dz, sx, sy, sz, cosa, sina);
				World.Rotate(ref ux, ref uy, ref uz, sx, sy, sz, cosa, sina);
			}
			if (World.CameraCurrentAlignment.Roll != 0.0) {
				double cosa = Math.Cos(-World.CameraCurrentAlignment.Roll);
				double sina = Math.Sin(-World.CameraCurrentAlignment.Roll);
				World.Rotate(ref ux, ref uy, ref uz, dx, dy, dz, cosa, sina);
				World.Rotate(ref sx, ref sy, ref sz, dx, dy, dz, cosa, sina);
			}
			AbsoluteCameraPosition = new Vector3(cx, cy, cz);
			AbsoluteCameraDirection = new Vector3(dx, dy, dz);
			AbsoluteCameraUp = new Vector3(ux, uy, uz);
			AbsoluteCameraSide = new Vector3(sx, sy, sz);
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

		// ================================

		// cross
		internal static void Cross(double ax, double ay, double az, double bx, double by, double bz, out double cx, out double cy, out double cz) {
			cx = ay * bz - az * by;
			cy = az * bx - ax * bz;
			cz = ax * by - ay * bx;
		}

		// transformation
		internal struct Transformation {
			internal Vector3 X;
			internal Vector3 Y;
			internal Vector3 Z;
			internal Transformation(double Yaw, double Pitch, double Roll) {
				if (Yaw == 0.0 & Pitch == 0.0 & Roll == 0.0) {
					this.X = new Vector3(1.0, 0.0, 0.0);
					this.Y = new Vector3(0.0, 1.0, 0.0);
					this.Z = new Vector3(0.0, 0.0, 1.0);
				} else if (Pitch == 0.0 & Roll == 0.0) {
					double cosYaw = Math.Cos(Yaw);
					double sinYaw = Math.Sin(Yaw);
					this.X = new Vector3(cosYaw, 0.0, -sinYaw);
					this.Y = new Vector3(0.0, 1.0, 0.0);
					this.Z = new Vector3(sinYaw, 0.0, cosYaw);
				} else {
					double sx = 1.0, sy = 0.0, sz = 0.0;
					double ux = 0.0, uy = 1.0, uz = 0.0;
					double dx = 0.0, dy = 0.0, dz = 1.0;
					double cosYaw = Math.Cos(Yaw);
					double sinYaw = Math.Sin(Yaw);
					double cosPitch = Math.Cos(-Pitch);
					double sinPitch = Math.Sin(-Pitch);
					double cosRoll = Math.Cos(-Roll);
					double sinRoll = Math.Sin(-Roll);
					Rotate(ref sx, ref sy, ref sz, ux, uy, uz, cosYaw, sinYaw);
					Rotate(ref dx, ref dy, ref dz, ux, uy, uz, cosYaw, sinYaw);
					Rotate(ref ux, ref uy, ref uz, sx, sy, sz, cosPitch, sinPitch);
					Rotate(ref dx, ref dy, ref dz, sx, sy, sz, cosPitch, sinPitch);
					Rotate(ref sx, ref sy, ref sz, dx, dy, dz, cosRoll, sinRoll);
					Rotate(ref ux, ref uy, ref uz, dx, dy, dz, cosRoll, sinRoll);
					this.X = new Vector3(sx, sy, sz);
					this.Y = new Vector3(ux, uy, uz);
					this.Z = new Vector3(dx, dy, dz);
				}
			}
			internal Transformation(Transformation Transformation, double Yaw, double Pitch, double Roll) {
				double sx = Transformation.X.X, sy = Transformation.X.Y, sz = Transformation.X.Z;
				double ux = Transformation.Y.X, uy = Transformation.Y.Y, uz = Transformation.Y.Z;
				double dx = Transformation.Z.X, dy = Transformation.Z.Y, dz = Transformation.Z.Z;
				double cosYaw = Math.Cos(Yaw);
				double sinYaw = Math.Sin(Yaw);
				double cosPitch = Math.Cos(-Pitch);
				double sinPitch = Math.Sin(-Pitch);
				double cosRoll = Math.Cos(Roll);
				double sinRoll = Math.Sin(Roll);
				Rotate(ref sx, ref sy, ref sz, ux, uy, uz, cosYaw, sinYaw);
				Rotate(ref dx, ref dy, ref dz, ux, uy, uz, cosYaw, sinYaw);
				Rotate(ref ux, ref uy, ref uz, sx, sy, sz, cosPitch, sinPitch);
				Rotate(ref dx, ref dy, ref dz, sx, sy, sz, cosPitch, sinPitch);
				Rotate(ref sx, ref sy, ref sz, dx, dy, dz, cosRoll, sinRoll);
				Rotate(ref ux, ref uy, ref uz, dx, dy, dz, cosRoll, sinRoll);
				this.X = new Vector3(sx, sy, sz);
				this.Y = new Vector3(ux, uy, uz);
				this.Z = new Vector3(dx, dy, dz);
			}
			internal Transformation(Transformation BaseTransformation, Transformation AuxTransformation) {
				Vector3 x = BaseTransformation.X;
				Vector3 y = BaseTransformation.Y;
				Vector3 z = BaseTransformation.Z;
				Vector3 s = AuxTransformation.X;
				Vector3 u = AuxTransformation.Y;
				Vector3 d = AuxTransformation.Z;
				Rotate(ref x.X, ref x.Y, ref x.Z, d.X, d.Y, d.Z, u.X, u.Y, u.Z, s.X, s.Y, s.Z);
				Rotate(ref y.X, ref y.Y, ref y.Z, d.X, d.Y, d.Z, u.X, u.Y, u.Z, s.X, s.Y, s.Z);
				Rotate(ref z.X, ref z.Y, ref z.Z, d.X, d.Y, d.Z, u.X, u.Y, u.Z, s.X, s.Y, s.Z);
				this.X = x;
				this.Y = y;
				this.Z = z;
			}
		}

		// rotate
		internal static void Rotate(ref double px, ref double py, ref double pz, double dx, double dy, double dz, double cosa, double sina) {
			double t = 1.0 / Math.Sqrt(dx * dx + dy * dy + dz * dz);
			dx *= t; dy *= t; dz *= t;
			double oc = 1.0 - cosa;
			double x = (cosa + oc * dx * dx) * px + (oc * dx * dy - sina * dz) * py + (oc * dx * dz + sina * dy) * pz;
			double y = (cosa + oc * dy * dy) * py + (oc * dx * dy + sina * dz) * px + (oc * dy * dz - sina * dx) * pz;
			double z = (cosa + oc * dz * dz) * pz + (oc * dx * dz - sina * dy) * px + (oc * dy * dz + sina * dx) * py;
			px = x; py = y; pz = z;
		}
		internal static void Rotate(ref float px, ref float py, ref float pz, double dx, double dy, double dz, double cosa, double sina) {
			double t = 1.0 / Math.Sqrt(dx * dx + dy * dy + dz * dz);
			dx *= t; dy *= t; dz *= t;
			double oc = 1.0 - cosa;
			double x = (cosa + oc * dx * dx) * (double)px + (oc * dx * dy - sina * dz) * (double)py + (oc * dx * dz + sina * dy) * (double)pz;
			double y = (cosa + oc * dy * dy) * (double)py + (oc * dx * dy + sina * dz) * (double)px + (oc * dy * dz - sina * dx) * (double)pz;
			double z = (cosa + oc * dz * dz) * (double)pz + (oc * dx * dz - sina * dy) * (double)px + (oc * dy * dz + sina * dx) * (double)py;
			px = (float)x; py = (float)y; pz = (float)z;
		}
		internal static void Rotate(ref Vector2 Vector, double cosa, double sina) {
			double u = Vector.X * cosa - Vector.Y * sina;
			double v = Vector.X * sina + Vector.Y * cosa;
			Vector.X = u;
			Vector.Y = v;
		}
		internal static void Rotate(ref float px, ref float py, ref float pz, double dx, double dy, double dz, double ux, double uy, double uz, double sx, double sy, double sz) {
			double x, y, z;
			x = sx * (double)px + ux * (double)py + dx * (double)pz;
			y = sy * (double)px + uy * (double)py + dy * (double)pz;
			z = sz * (double)px + uz * (double)py + dz * (double)pz;
			px = (float)x; py = (float)y; pz = (float)z;
		}
		internal static void Rotate(ref double px, ref double py, ref double pz, double dx, double dy, double dz, double ux, double uy, double uz, double sx, double sy, double sz) {
			double x, y, z;
			x = sx * px + ux * py + dx * pz;
			y = sy * px + uy * py + dy * pz;
			z = sz * px + uz * py + dz * pz;
			px = x; py = y; pz = z;
		}
		internal static void Rotate(ref float px, ref float py, ref float pz, Transformation t) {
			double x, y, z;
			x = t.X.X * (double)px + t.Y.X * (double)py + t.Z.X * (double)pz;
			y = t.X.Y * (double)px + t.Y.Y * (double)py + t.Z.Y * (double)pz;
			z = t.X.Z * (double)px + t.Y.Z * (double)py + t.Z.Z * (double)pz;
			px = (float)x; py = (float)y; pz = (float)z;
		}
		internal static void Rotate(ref double px, ref double py, ref double pz, Transformation t) {
			double x, y, z;
			x = t.X.X * px + t.Y.X * py + t.Z.X * pz;
			y = t.X.Y * px + t.Y.Y * py + t.Z.Y * pz;
			z = t.X.Z * px + t.Y.Z * py + t.Z.Z * pz;
			px = x; py = y; pz = z;
		}
		internal static void RotatePlane(ref Vector3 Vector, double cosa, double sina) {
			double u = Vector.X * cosa - Vector.Z * sina;
			double v = Vector.X * sina + Vector.Z * cosa;
			Vector.X = u;
			Vector.Z = v;
		}

		internal static void RotateUpDown(ref Vector3 Vector, Vector2 Direction, double cosa, double sina) {
			double dx = Direction.X, dy = Direction.Y;
			double x = Vector.X, y = Vector.Y, z = Vector.Z;
			double u = dy * x - dx * z;
			double v = dx * x + dy * z;
			Vector.X = dy * u + dx * v * cosa - dx * y * sina;
			Vector.Y = y * cosa + v * sina;
			Vector.Z = -dx * u + dy * v * cosa - dy * y * sina;
		}
		internal static void RotateUpDown(ref Vector3 Vector, double dx, double dy, double cosa, double sina) {
			double x = Vector.X, y = Vector.Y, z = Vector.Z;
			double u = dy * x - dx * z;
			double v = dx * x + dy * z;
			Vector.X = dy * u + dx * v * cosa - dx * y * sina;
			Vector.Y = y * cosa + v * sina;
			Vector.Z = -dx * u + dy * v * cosa - dy * y * sina;
		}

		internal static void RotateUpDown(ref double px, ref double py, ref double pz, double dx, double dz, double cosa, double sina) {
			double x = px, y = py, z = pz;
			double u = dz * x - dx * z;
			double v = dx * x + dz * z;
			px = dz * u + dx * v * cosa - dx * y * sina;
			py = y * cosa + v * sina;
			pz = -dx * u + dz * v * cosa - dz * y * sina;
		}

		// normalize
		internal static void Normalize(ref double x, ref double y) {
			double t = x * x + y * y;
			if (t != 0.0) {
				t = 1.0 / Math.Sqrt(t);
				x *= t; y *= t;
			}
		}
		internal static void Normalize(ref double x, ref double y, ref double z) {
			double t = x * x + y * y + z * z;
			if (t != 0.0) {
				t = 1.0 / Math.Sqrt(t);
				x *= t; y *= t; z *= t;
			}
		}

		// create normals
		internal static void CreateNormals(ref Mesh Mesh) {
			for (int i = 0; i < Mesh.Faces.Length; i++) {
				CreateNormals(ref Mesh, i);
			}
		}
		internal static void CreateNormals(ref Mesh Mesh, int FaceIndex) {
			if (Mesh.Faces[FaceIndex].Vertices.Length >= 3) {
				int i0 = (int)Mesh.Faces[FaceIndex].Vertices[0].Index;
				int i1 = (int)Mesh.Faces[FaceIndex].Vertices[1].Index;
				int i2 = (int)Mesh.Faces[FaceIndex].Vertices[2].Index;
				double ax = Mesh.Vertices[i1].Coordinates.X - Mesh.Vertices[i0].Coordinates.X;
				double ay = Mesh.Vertices[i1].Coordinates.Y - Mesh.Vertices[i0].Coordinates.Y;
				double az = Mesh.Vertices[i1].Coordinates.Z - Mesh.Vertices[i0].Coordinates.Z;
				double bx = Mesh.Vertices[i2].Coordinates.X - Mesh.Vertices[i0].Coordinates.X;
				double by = Mesh.Vertices[i2].Coordinates.Y - Mesh.Vertices[i0].Coordinates.Y;
				double bz = Mesh.Vertices[i2].Coordinates.Z - Mesh.Vertices[i0].Coordinates.Z;
				double nx = ay * bz - az * by;
				double ny = az * bx - ax * bz;
				double nz = ax * by - ay * bx;
				double t = nx * nx + ny * ny + nz * nz;
				if (t != 0.0) {
					t = 1.0 / Math.Sqrt(t);
					float mx = (float)(nx * t);
					float my = (float)(ny * t);
					float mz = (float)(nz * t);
					for (int j = 0; j < Mesh.Faces[FaceIndex].Vertices.Length; j++) {
						if (Vector3.IsZero(Mesh.Faces[FaceIndex].Vertices[j].Normal)) {
							Mesh.Faces[FaceIndex].Vertices[j].Normal = new Vector3(mx, my, mz);
						}
					}
				} else {
					for (int j = 0; j < Mesh.Faces[FaceIndex].Vertices.Length; j++) {
						if (Vector3.IsZero(Mesh.Faces[FaceIndex].Vertices[j].Normal)) {
							Mesh.Faces[FaceIndex].Vertices[j].Normal = new Vector3(0.0f, 1.0f, 0.0f);
						}
					}
				}
			}
		}

	}
}
