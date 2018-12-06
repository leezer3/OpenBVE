#pragma warning disable 0660 // Defines == or != but does not override Object.Equals
#pragma warning disable 0661 // Defines == or != but does not override Object.GetHashCode

using System;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using Vector2 = OpenBveApi.Math.Vector2;
using OpenBveApi.Objects;
using OpenBveApi.Runtime;
using OpenBveApi.Textures;

namespace OpenBve {
	internal static partial class World {
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
				if (A.WrapMode != B.WrapMode) return false;
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
				if (A.WrapMode != B.WrapMode) return true;
				return false;
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
			internal Vector3[] BoundingBox;
			/// <summary>Creates a mesh consisting of one face, which is represented by individual vertices, and a color.</summary>
			/// <param name="Vertices">The vertices that make up one face.</param>
			/// <param name="Color">The color to be applied on the face.</param>
			internal Mesh(VertexTemplate[] Vertices, Color32 Color) {
				this.Vertices = Vertices;
				this.Materials = new MeshMaterial[1];
				this.Materials[0].Color = Color;
				this.Faces = new MeshFace[1];
				this.Faces[0].Material = 0;
				this.Faces[0].Vertices = new MeshFaceVertex[Vertices.Length];
				for (int i = 0; i < Vertices.Length; i++) {
					this.Faces[0].Vertices[i].Index = (ushort)i;
				}
				this.BoundingBox = new Vector3[2];
			}
			/// <summary>Creates a mesh consisting of the specified vertices, faces and color.</summary>
			/// <param name="Vertices">The vertices used.</param>
			/// <param name="FaceVertices">A list of faces represented by a list of references to vertices.</param>
			/// <param name="Color">The color to be applied on all of the faces.</param>
			internal Mesh(VertexTemplate[] Vertices, int[][] FaceVertices, Color32 Color) {
				this.Vertices = Vertices;
				this.Materials = new MeshMaterial[1];
				this.Materials[0].Color = Color;
				this.Faces = new MeshFace[FaceVertices.Length];
				for (int i = 0; i < FaceVertices.Length; i++) {
					this.Faces[i] = new MeshFace(FaceVertices[i]);
				}
				this.BoundingBox = new Vector3[2];
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
		/// <summary>The current horizontal viewing angle in radians</summary>
		internal static double HorizontalViewingAngle;
		/// <summary>The current vertical viewing angle in radians</summary>
		internal static double VerticalViewingAngle;
		/// <summary>The original vertical viewing angle in radians</summary>
		internal static double OriginalVerticalViewingAngle;
		/// <summary>The current aspect ratio</summary>
		internal static double AspectRatio;
		/// <summary>The current viewing distance in the forward direction.</summary>
		internal static double ForwardViewingDistance;
		/// <summary>The current viewing distance in the backward direction.</summary>
		internal static double BackwardViewingDistance;
		/// <summary>The extra viewing distance used for determining visibility of animated objects.</summary>
		internal static double ExtraViewingDistance;
		/// <summary>The user-selected viewing distance.</summary>
		internal static double BackgroundImageDistance;
		

		// driver body
		internal struct DriverBody {
			internal double SlowX;
			internal double FastX;
			internal double Roll;
			internal Damping RollDamping;
			internal double SlowY;
			internal double FastY;
			internal double Pitch;
			internal Damping PitchDamping;

			internal void Update(double TimeElapsed) {
			if (CameraRestriction == Camera.RestrictionMode.NotAvailable) {
				{
					// pitch
					double targetY = TrainManager.PlayerTrain.Specs.CurrentAverageAcceleration;
					const double accelerationSlow = 0.25;
					const double accelerationFast = 2.0;
					if (SlowY < targetY) {
						SlowY += accelerationSlow * TimeElapsed;
						if (SlowY > targetY) {
							SlowY = targetY;
						}
					} else if (SlowY > targetY) {
						SlowY -= accelerationSlow * TimeElapsed;
						if (SlowY < targetY) {
							SlowY = targetY;
						}
					}
					if (FastY < targetY) {
						FastY += accelerationFast * TimeElapsed;
						if (FastY > targetY) {
							FastY = targetY;
						}
					} else if (FastY > targetY) {
						FastY -= accelerationFast * TimeElapsed;
						if (FastY < targetY) {
							FastY = targetY;
						}
					}
					double diffY = FastY - SlowY;
					diffY = (double)Math.Sign(diffY) * diffY * diffY;
					Pitch = 0.5 * Math.Atan(0.1 * diffY);
					if (Pitch > 0.1) {
						Pitch = 0.1;
					}
					if (PitchDamping == null) {
						PitchDamping = new Damping(6.0, 0.3);
					}
					PitchDamping.Update(TimeElapsed, ref Pitch, true);
				}
				{
					// roll
					int c = TrainManager.PlayerTrain.DriverCar;
					double frontRadius = TrainManager.PlayerTrain.Cars[c].FrontAxle.Follower.CurveRadius;
					double rearRadius = TrainManager.PlayerTrain.Cars[c].RearAxle.Follower.CurveRadius;
					double radius;
					if (frontRadius != 0.0 & rearRadius != 0.0) {
						if (frontRadius != -rearRadius) {
							radius = 2.0 * frontRadius * rearRadius / (frontRadius + rearRadius);
						} else {
							radius = 0.0;
						}
					} else if (frontRadius != 0.0) {
						radius = 2.0 * frontRadius;
					} else if (rearRadius != 0.0) {
						radius = 2.0 * rearRadius;
					} else {
						radius = 0.0;
					}
					double targetX;
					if (radius != 0.0) {
						double speed = TrainManager.PlayerTrain.Cars[c].Specs.CurrentSpeed;
						targetX = speed * speed / radius;
					} else {
						targetX = 0.0;
					}
					const double accelerationSlow = 1.0;
					const double accelerationFast = 10.0;
					if (SlowX < targetX) {
						SlowX += accelerationSlow * TimeElapsed;
						if (SlowX > targetX) {
							SlowX = targetX;
						}
					} else if (SlowX > targetX) {
						SlowX -= accelerationSlow * TimeElapsed;
						if (SlowX < targetX) {
							SlowX = targetX;
						}
					}
					if (FastX < targetX) {
						FastX += accelerationFast * TimeElapsed;
						if (FastX > targetX) {
							FastX = targetX;
						}
					} else if (FastX > targetX) {
						FastX -= accelerationFast * TimeElapsed;
						if (FastX < targetX) {
							FastX = targetX;
						}
					}
					double diffX = SlowX - FastX;
					diffX = (double)Math.Sign(diffX) * diffX * diffX;
					Roll = 0.5 * Math.Atan(0.3 * diffX);
					if (RollDamping == null) {
						RollDamping = new Damping(6.0, 0.3);
					}
					RollDamping.Update(TimeElapsed, ref Roll, true);
				}
			}
		}
		}
		internal static DriverBody CurrentDriverBody;
		
		
		// mouse grab
		internal static bool MouseGrabEnabled = false;
		internal static bool MouseGrabIgnoreOnce = false;
		internal static Vector2 MouseGrabTarget = new Vector2(0.0, 0.0);
		internal static void UpdateMouseGrab(double TimeElapsed) {
			if (MouseGrabEnabled) {
				double factor;
				if (CameraMode == CameraViewMode.Interior | CameraMode == CameraViewMode.InteriorLookAhead) {
					factor = 1.0;
				} else {
					factor = 3.0;
				}
				CameraAlignmentDirection.Yaw += factor * MouseGrabTarget.X;
				CameraAlignmentDirection.Pitch -= factor * MouseGrabTarget.Y;
				MouseGrabTarget = new Vector2(0.0, 0.0);
			}
		}
		
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
		internal static bool CameraAtWorldEnd;
		internal static CameraAlignment CameraCurrentAlignment;
		internal static CameraAlignment CameraAlignmentDirection;
		internal static CameraAlignment CameraAlignmentSpeed;
		internal static double CameraSpeed;
		internal const double CameraInteriorTopSpeed = 5.0;
		internal const double CameraInteriorTopAngularSpeed = 5.0;
		internal const double CameraExteriorTopSpeed = 50.0;
		internal const double CameraExteriorTopAngularSpeed = 10.0;
		internal const double CameraZoomTopSpeed = 2.0;
		internal static CameraViewMode CameraMode;
		/// <summary>The index of the car which the camera is currently anchored to</summary>
		internal static int CameraCar;
		
		// camera memory
		internal static CameraAlignment CameraSavedExterior;
		internal static CameraAlignment CameraSavedTrack;

		// camera restriction
		internal static Vector3 CameraRestrictionBottomLeft = new Vector3(-1.0, -1.0, 1.0);
		internal static Vector3 CameraRestrictionTopRight = new Vector3(1.0, 1.0, 1.0);
		
		internal static Camera.RestrictionMode CameraRestriction = Camera.RestrictionMode.NotAvailable;

		// absolute camera
		internal static Vector3 AbsoluteCameraPosition;
		internal static Vector3 AbsoluteCameraDirection;
		internal static Vector3 AbsoluteCameraUp;
		internal static Vector3 AbsoluteCameraSide;

		// camera restriction
		internal static void InitializeCameraRestriction() {
			if ((CameraMode == CameraViewMode.Interior | CameraMode == CameraViewMode.InteriorLookAhead) & CameraRestriction == Camera.RestrictionMode.On) {
				CameraAlignmentSpeed = new CameraAlignment();
				UpdateAbsoluteCamera(0.0);
				if (!PerformCameraRestrictionTest()) {
					CameraCurrentAlignment = new CameraAlignment();
					VerticalViewingAngle = OriginalVerticalViewingAngle;
					Renderer.UpdateViewport(Renderer.ViewPortChangeMode.NoChange);
					UpdateAbsoluteCamera(0.0);
					UpdateViewingDistances();
					if (!PerformCameraRestrictionTest()) {
						CameraCurrentAlignment.Position.Z = 0.8;
						UpdateAbsoluteCamera(0.0);
						PerformProgressiveAdjustmentForCameraRestriction(ref CameraCurrentAlignment.Position.Z, 0.0, true);
						if (!PerformCameraRestrictionTest()) {
							CameraCurrentAlignment.Position.X = 0.5 * (CameraRestrictionBottomLeft.X + CameraRestrictionTopRight.X);
							CameraCurrentAlignment.Position.Y = 0.5 * (CameraRestrictionBottomLeft.Y + CameraRestrictionTopRight.Y);
							CameraCurrentAlignment.Position.Z = 0.0;
							UpdateAbsoluteCamera(0.0);
							if (PerformCameraRestrictionTest()) {
								PerformProgressiveAdjustmentForCameraRestriction(ref CameraCurrentAlignment.Position.X, 0.0, true);
								PerformProgressiveAdjustmentForCameraRestriction(ref CameraCurrentAlignment.Position.Y, 0.0, true);
							} else {
								CameraCurrentAlignment.Position.Z = 0.8;
								UpdateAbsoluteCamera(0.0);
								PerformProgressiveAdjustmentForCameraRestriction(ref CameraCurrentAlignment.Position.Z, 0.0, true);
								if (!PerformCameraRestrictionTest()) {
									CameraCurrentAlignment = new CameraAlignment();
								}
							}
						}
					}
					UpdateAbsoluteCamera(0.0);
				}
			}
		}
		internal static bool PerformProgressiveAdjustmentForCameraRestriction(ref double Source, double Target, bool Zoom) {
			if ((CameraMode != CameraViewMode.Interior & CameraMode != CameraViewMode.InteriorLookAhead) | CameraRestriction != Camera.RestrictionMode.On) {
				Source = Target;
				return true;
			}
			double best = Source;
			const int Precision = 8;
			double a = Source;
			double b = Target;
			Source = Target;
			if (Zoom) ApplyZoom();
			if (PerformCameraRestrictionTest()) {
				return true;
			}
			double x = 0.5 * (a + b);
			bool q = true;
			for (int i = 0; i < Precision; i++) {
				//Do not remove, this is updated via the ref & causes the panel zoom to bug out
				Source = x;
				if (Zoom) ApplyZoom();
				q = PerformCameraRestrictionTest();
				if (q) {
					a = x;
					best = x;
				} else {
					b = x;
				}
				x = 0.5 * (a + b);
			}
			Source = best;
			if (Zoom) ApplyZoom();
			return q;
		}
		/// <summary>Checks whether the camera can move in the selected direction, due to a bounding box.</summary>
		/// <returns>True if we are able to move the camera, false otherwise</returns>
		internal static bool PerformBoundingBoxTest(ref ObjectManager.StaticObject bounding, ref Vector3 cameraLocation)
		{
			if (cameraLocation.X < bounding.Mesh.BoundingBox[0].X || cameraLocation.X > bounding.Mesh.BoundingBox[1].X ||
				cameraLocation.Y < bounding.Mesh.BoundingBox[0].Y || cameraLocation.Y > bounding.Mesh.BoundingBox[1].Y ||
				cameraLocation.Z < bounding.Mesh.BoundingBox[0].Z || cameraLocation.Z > bounding.Mesh.BoundingBox[1].Z)
			{
				//Our bounding boxes do not intersect
				return true;
			}
			return false;
		}

		internal static bool PerformCameraRestrictionTest()
		{
			if (World.CameraRestriction == Camera.RestrictionMode.On) {
				Vector3[] p = new Vector3[] { CameraRestrictionBottomLeft, CameraRestrictionTopRight };
				Vector2[] r = new Vector2[2];
				for (int j = 0; j < 2; j++) {
					// determine relative world coordinates
					p[j].Rotate(World.AbsoluteCameraDirection, World.AbsoluteCameraUp, World.AbsoluteCameraSide);
					double rx = -Math.Tan(World.CameraCurrentAlignment.Yaw) - World.CameraCurrentAlignment.Position.X;
					double ry = -Math.Tan(World.CameraCurrentAlignment.Pitch) - World.CameraCurrentAlignment.Position.Y;
					double rz = -World.CameraCurrentAlignment.Position.Z;
					p[j].X += rx * World.AbsoluteCameraSide.X + ry * World.AbsoluteCameraUp.X + rz * World.AbsoluteCameraDirection.X;
					p[j].Y += rx * World.AbsoluteCameraSide.Y + ry * World.AbsoluteCameraUp.Y + rz * World.AbsoluteCameraDirection.Y;
					p[j].Z += rx * World.AbsoluteCameraSide.Z + ry * World.AbsoluteCameraUp.Z + rz * World.AbsoluteCameraDirection.Z;
					// determine screen coordinates
					double ez = AbsoluteCameraDirection.X * p[j].X + AbsoluteCameraDirection.Y * p[j].Y + AbsoluteCameraDirection.Z * p[j].Z;
					if (ez == 0.0) return false;
					double ex = AbsoluteCameraSide.X * p[j].X + AbsoluteCameraSide.Y * p[j].Y + AbsoluteCameraSide.Z * p[j].Z;
					double ey = AbsoluteCameraUp.X * p[j].X + AbsoluteCameraUp.Y * p[j].Y + AbsoluteCameraUp.Z * p[j].Z;
					r[j].X = ex / (ez * Math.Tan(0.5 * HorizontalViewingAngle));
					r[j].Y = ey / (ez * Math.Tan(0.5 * VerticalViewingAngle));
				}
				return r[0].X <= -1.0025 & r[1].X >= 1.0025 & r[0].Y <= -1.0025 & r[1].Y >= 1.0025;
			}
			return true;
		}

		// update absolute camera
		internal static void UpdateAbsoluteCamera(double TimeElapsed) {
			// zoom
			double zm = World.CameraCurrentAlignment.Zoom;
			AdjustAlignment(ref World.CameraCurrentAlignment.Zoom, World.CameraAlignmentDirection.Zoom, ref World.CameraAlignmentSpeed.Zoom, TimeElapsed, true);
			if (zm != World.CameraCurrentAlignment.Zoom) {
				ApplyZoom();
			}
			if (CameraMode == CameraViewMode.FlyBy | CameraMode == CameraViewMode.FlyByZooming) {
				// fly-by
				AdjustAlignment(ref World.CameraCurrentAlignment.Position.X, World.CameraAlignmentDirection.Position.X, ref World.CameraAlignmentSpeed.Position.X, TimeElapsed);
				AdjustAlignment(ref World.CameraCurrentAlignment.Position.Y, World.CameraAlignmentDirection.Position.Y, ref World.CameraAlignmentSpeed.Position.Y, TimeElapsed);
				double tr = World.CameraCurrentAlignment.TrackPosition;
				AdjustAlignment(ref World.CameraCurrentAlignment.TrackPosition, World.CameraAlignmentDirection.TrackPosition, ref World.CameraAlignmentSpeed.TrackPosition, TimeElapsed);
				if (tr != World.CameraCurrentAlignment.TrackPosition) {
					World.CameraTrackFollower.Update(World.CameraCurrentAlignment.TrackPosition, true, false);
					UpdateViewingDistances();
				}
				// camera
				double px = World.CameraTrackFollower.WorldPosition.X;
				double py = World.CameraTrackFollower.WorldPosition.Y;
				double pz = World.CameraTrackFollower.WorldPosition.Z;
				// position to focus on
				double tx, ty, tz;
				double zoomMultiplier;
				{
					const double heightFactor = 0.75;
					TrainManager.Train bestTrain = null;
					double bestDistanceSquared = double.MaxValue;
					TrainManager.Train secondBestTrain = null;
					double secondBestDistanceSquared = double.MaxValue;
					foreach (TrainManager.Train train in TrainManager.Trains) {
						if (train.State == TrainManager.TrainState.Available) {
							double x = 0.5 * (train.Cars[0].FrontAxle.Follower.WorldPosition.X + train.Cars[0].RearAxle.Follower.WorldPosition.X);
							double y = 0.5 * (train.Cars[0].FrontAxle.Follower.WorldPosition.Y + train.Cars[0].RearAxle.Follower.WorldPosition.Y) + heightFactor * train.Cars[0].Height;
							double z = 0.5 * (train.Cars[0].FrontAxle.Follower.WorldPosition.Z + train.Cars[0].RearAxle.Follower.WorldPosition.Z);
							double dx = x - px;
							double dy = y - py;
							double dz = z - pz;
							double d = dx * dx + dy * dy + dz * dz;
							if (d < bestDistanceSquared) {
								secondBestTrain = bestTrain;
								secondBestDistanceSquared = bestDistanceSquared;
								bestTrain = train;
								bestDistanceSquared = d;
							} else if (d < secondBestDistanceSquared) {
								secondBestTrain = train;
								secondBestDistanceSquared = d;
							}
						}
					}
					if (bestTrain != null) {
						const double maxDistance = 100.0;
						double bestDistance = Math.Sqrt(bestDistanceSquared);
						double secondBestDistance = Math.Sqrt(secondBestDistanceSquared);
						if (secondBestTrain != null && secondBestDistance - bestDistance <= maxDistance) {
							double x1 = 0.5 * (bestTrain.Cars[0].FrontAxle.Follower.WorldPosition.X + bestTrain.Cars[0].RearAxle.Follower.WorldPosition.X);
							double y1 = 0.5 * (bestTrain.Cars[0].FrontAxle.Follower.WorldPosition.Y + bestTrain.Cars[0].RearAxle.Follower.WorldPosition.Y) + heightFactor * bestTrain.Cars[0].Height;
							double z1 = 0.5 * (bestTrain.Cars[0].FrontAxle.Follower.WorldPosition.Z + bestTrain.Cars[0].RearAxle.Follower.WorldPosition.Z);
							double x2 = 0.5 * (secondBestTrain.Cars[0].FrontAxle.Follower.WorldPosition.X + secondBestTrain.Cars[0].RearAxle.Follower.WorldPosition.X);
							double y2 = 0.5 * (secondBestTrain.Cars[0].FrontAxle.Follower.WorldPosition.Y + secondBestTrain.Cars[0].RearAxle.Follower.WorldPosition.Y) + heightFactor * secondBestTrain.Cars[0].Height;
							double z2 = 0.5 * (secondBestTrain.Cars[0].FrontAxle.Follower.WorldPosition.Z + secondBestTrain.Cars[0].RearAxle.Follower.WorldPosition.Z);
							double t = 0.5 - (secondBestDistance - bestDistance) / (2.0 * maxDistance);
							if (t < 0.0) t = 0.0;
							t = 2.0 * t * t; /* in order to change the shape of the interpolation curve */
							tx = (1.0 - t) * x1 + t * x2;
							ty = (1.0 - t) * y1 + t * y2;
							tz = (1.0 - t) * z1 + t * z2;
							zoomMultiplier = 1.0 - 2.0 * t;
						} else {
							tx = 0.5 * (bestTrain.Cars[0].FrontAxle.Follower.WorldPosition.X + bestTrain.Cars[0].RearAxle.Follower.WorldPosition.X);
							ty = 0.5 * (bestTrain.Cars[0].FrontAxle.Follower.WorldPosition.Y + bestTrain.Cars[0].RearAxle.Follower.WorldPosition.Y) + heightFactor * bestTrain.Cars[0].Height;
							tz = 0.5 * (bestTrain.Cars[0].FrontAxle.Follower.WorldPosition.Z + bestTrain.Cars[0].RearAxle.Follower.WorldPosition.Z);
							zoomMultiplier = 1.0;
						}
					} else {
						tx = 0.0;
						ty = 0.0;
						tz = 0.0;
						zoomMultiplier = 1.0;
					}
				}
				// camera
				{
					double dx = World.CameraTrackFollower.WorldDirection.X;
					double dy = World.CameraTrackFollower.WorldDirection.Y;
					double dz = World.CameraTrackFollower.WorldDirection.Z;
					double ux = World.CameraTrackFollower.WorldUp.X;
					double uy = World.CameraTrackFollower.WorldUp.Y;
					double uz = World.CameraTrackFollower.WorldUp.Z;
					double sx = World.CameraTrackFollower.WorldSide.X;
					double sy = World.CameraTrackFollower.WorldSide.Y;
					double sz = World.CameraTrackFollower.WorldSide.Z;
					double ox = World.CameraCurrentAlignment.Position.X;
					double oy = World.CameraCurrentAlignment.Position.Y;
					double oz = World.CameraCurrentAlignment.Position.Z;
					double cx = px + sx * ox + ux * oy + dx * oz;
					double cy = py + sy * ox + uy * oy + dy * oz;
					double cz = pz + sz * ox + uz * oy + dz * oz;
					AbsoluteCameraPosition = new Vector3(cx, cy, cz);
					dx = tx - cx;
					dy = ty - cy;
					dz = tz - cz;
					double t = Math.Sqrt(dx * dx + dy * dy + dz * dz);
					double ti = 1.0 / t;
					dx *= ti;
					dy *= ti;
					dz *= ti;
					AbsoluteCameraDirection = new Vector3(dx, dy, dz);
					AbsoluteCameraSide = new Vector3(dz, 0.0, -dx);
					AbsoluteCameraSide.Normalize();
					AbsoluteCameraUp = Vector3.Cross(new Vector3(dx, dy, dz), AbsoluteCameraSide);
					UpdateViewingDistances();
					if (CameraMode == CameraViewMode.FlyByZooming) {
						// zoom
						const double fadeOutDistance = 600.0; /* the distance with the highest zoom factor is half the fade-out distance */
						const double maxZoomFactor = 7.0; /* the zoom factor at half the fade-out distance */
						const double factor = 256.0 / (fadeOutDistance * fadeOutDistance * fadeOutDistance * fadeOutDistance * fadeOutDistance * fadeOutDistance * fadeOutDistance * fadeOutDistance);
						double zoom;
						if (t < fadeOutDistance) {
							double tdist4 = fadeOutDistance - t; tdist4 *= tdist4; tdist4 *= tdist4;
							double t4 = t * t; t4 *= t4;
							zoom = 1.0 + factor * zoomMultiplier * (maxZoomFactor - 1.0) * tdist4 * t4;
						} else {
							zoom = 1.0;
						}
						World.VerticalViewingAngle = World.OriginalVerticalViewingAngle / zoom;
						Renderer.UpdateViewport(Renderer.ViewPortChangeMode.NoChange);
					}
				}
			} else {
				// non-fly-by
				{
					// current alignment
					AdjustAlignment(ref World.CameraCurrentAlignment.Position.X, World.CameraAlignmentDirection.Position.X, ref World.CameraAlignmentSpeed.Position.X, TimeElapsed);
					AdjustAlignment(ref World.CameraCurrentAlignment.Position.Y, World.CameraAlignmentDirection.Position.Y, ref World.CameraAlignmentSpeed.Position.Y, TimeElapsed);
					AdjustAlignment(ref World.CameraCurrentAlignment.Position.Z, World.CameraAlignmentDirection.Position.Z, ref World.CameraAlignmentSpeed.Position.Z, TimeElapsed);
					if ((CameraMode == CameraViewMode.Interior | World.CameraMode == CameraViewMode.InteriorLookAhead) & CameraRestriction == Camera.RestrictionMode.On) {
						if (CameraCurrentAlignment.Position.Z > 0.75) {
							CameraCurrentAlignment.Position.Z = 0.75;
						}
					}
					bool q = World.CameraAlignmentSpeed.Yaw != 0.0 | World.CameraAlignmentSpeed.Pitch != 0.0 | World.CameraAlignmentSpeed.Roll != 0.0;
					AdjustAlignment(ref World.CameraCurrentAlignment.Yaw, World.CameraAlignmentDirection.Yaw, ref World.CameraAlignmentSpeed.Yaw, TimeElapsed);
					AdjustAlignment(ref World.CameraCurrentAlignment.Pitch, World.CameraAlignmentDirection.Pitch, ref World.CameraAlignmentSpeed.Pitch, TimeElapsed);
					AdjustAlignment(ref World.CameraCurrentAlignment.Roll, World.CameraAlignmentDirection.Roll, ref World.CameraAlignmentSpeed.Roll, TimeElapsed);
					double tr = World.CameraCurrentAlignment.TrackPosition;
					AdjustAlignment(ref World.CameraCurrentAlignment.TrackPosition, World.CameraAlignmentDirection.TrackPosition, ref World.CameraAlignmentSpeed.TrackPosition, TimeElapsed);
					if (tr != World.CameraCurrentAlignment.TrackPosition) {
						World.CameraTrackFollower.Update(World.CameraCurrentAlignment.TrackPosition, true, false);
						q = true;
					}
					if (q) {
						UpdateViewingDistances();
					}
				}
				// camera
				Vector3 cF = new Vector3(CameraTrackFollower.WorldPosition);
				Vector3 dF = new Vector3(CameraTrackFollower.WorldDirection);
				Vector3 uF = new Vector3(CameraTrackFollower.WorldUp);
				Vector3 sF = new Vector3(CameraTrackFollower.WorldSide);
				double lookaheadYaw;
				double lookaheadPitch;
				if (CameraMode == CameraViewMode.InteriorLookAhead) {
					// look-ahead
					double d = 20.0;
					if (TrainManager.PlayerTrain.Specs.CurrentAverageSpeed > 0.0) {
						d += 3.0 * (Math.Sqrt(TrainManager.PlayerTrain.Specs.CurrentAverageSpeed * TrainManager.PlayerTrain.Specs.CurrentAverageSpeed + 1.0) - 1.0);
					}
					d -= TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].FrontAxle.Position;
					TrackManager.TrackFollower f = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].FrontAxle.Follower;
					f.TriggerType = TrackManager.EventTriggerType.None;
					f.Update(f.TrackPosition + d, true, false);
					double rx = f.WorldPosition.X - cF.X + World.CameraTrackFollower.WorldSide.X * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Driver.X + World.CameraTrackFollower.WorldUp.X * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Driver.Y + World.CameraTrackFollower.WorldDirection.X * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Driver.Z;
					double ry = f.WorldPosition.Y - cF.Y + World.CameraTrackFollower.WorldSide.Y * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Driver.X + World.CameraTrackFollower.WorldUp.Y * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Driver.Y + World.CameraTrackFollower.WorldDirection.Y * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Driver.Z;
					double rz = f.WorldPosition.Z - cF.Z + World.CameraTrackFollower.WorldSide.Z * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Driver.X + World.CameraTrackFollower.WorldUp.Z * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Driver.Y + World.CameraTrackFollower.WorldDirection.Z * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Driver.Z;
					World.Normalize(ref rx, ref ry, ref rz);
					double t = dF.Z * (sF.Y * uF.X - sF.X * uF.Y) + dF.Y * (-sF.Z * uF.X + sF.X * uF.Z) + dF.X * (sF.Z * uF.Y - sF.Y * uF.Z);
					if (t != 0.0) {
						t = 1.0 / t;

						double tx = (rz * (-dF.Y * uF.X + dF.X * uF.Y) + ry * (dF.Z * uF.X - dF.X * uF.Z) + rx * (-dF.Z * uF.Y + dF.Y * uF.Z)) * t;
						double ty = (rz * (dF.Y * sF.X - dF.X * sF.Y) + ry * (-dF.Z * sF.X + dF.X * sF.Z) + rx * (dF.Z * sF.Y - dF.Y * sF.Z)) * t;
						double tz = (rz * (sF.Y * uF.X - sF.X * uF.Y) + ry * (-sF.Z * uF.X + sF.X * uF.Z) + rx * (sF.Z * uF.Y - sF.Y * uF.Z)) * t;
						lookaheadYaw = tx * tz != 0.0 ? Math.Atan2(tx, tz) : 0.0;
						if (ty < -1.0) {
							lookaheadPitch = -0.5 * Math.PI;
						} else if (ty > 1.0) {
							lookaheadPitch = 0.5 * Math.PI;
						} else {
							lookaheadPitch = Math.Asin(ty);
						}
					} else {
						lookaheadYaw = 0.0;
						lookaheadPitch = 0.0;
					}
				} else {
					lookaheadYaw = 0.0;
					lookaheadPitch = 0.0;
				}
				{
					// cab pitch and yaw
					Vector3 d2 = new Vector3(dF);
					Vector3 u2 = new Vector3(uF);
					if ((World.CameraMode == CameraViewMode.Interior | World.CameraMode == CameraViewMode.InteriorLookAhead) & TrainManager.PlayerTrain != null) {
						int c = TrainManager.PlayerTrain.DriverCar;
						if (c >= 0) {
							if (TrainManager.PlayerTrain.Cars[c].CarSections.Length == 0 || !TrainManager.PlayerTrain.Cars[c].CarSections[0].Overlay) {
								double a = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].DriverPitch;
								double cosa = Math.Cos(-a);
								double sina = Math.Sin(-a);
								d2.Rotate(sF, cosa, sina);
								u2.Rotate(sF, cosa, sina);
							}
						}
					}
					cF.X += sF.X * CameraCurrentAlignment.Position.X + u2.X * CameraCurrentAlignment.Position.Y + d2.X * CameraCurrentAlignment.Position.Z;
					cF.Y += sF.Y * CameraCurrentAlignment.Position.X + u2.Y * CameraCurrentAlignment.Position.Y + d2.Y * CameraCurrentAlignment.Position.Z;
					cF.Z += sF.Z * CameraCurrentAlignment.Position.X + u2.Z * CameraCurrentAlignment.Position.Y + d2.Z * CameraCurrentAlignment.Position.Z;
				}
				// yaw, pitch, roll
				double headYaw = World.CameraCurrentAlignment.Yaw + lookaheadYaw;
				if ((World.CameraMode == CameraViewMode.Interior | World.CameraMode == CameraViewMode.InteriorLookAhead) & TrainManager.PlayerTrain != null) {
					if (TrainManager.PlayerTrain.DriverCar >= 0) {
						headYaw += TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].DriverYaw;
					}
				}
				double headPitch = World.CameraCurrentAlignment.Pitch + lookaheadPitch;
				if ((World.CameraMode == CameraViewMode.Interior | World.CameraMode == CameraViewMode.InteriorLookAhead) & TrainManager.PlayerTrain != null) {
					if (TrainManager.PlayerTrain.DriverCar >= 0) {
						headPitch += TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].DriverPitch;
					}
				}
				double bodyPitch = 0.0;
				double bodyRoll = 0.0;
				double headRoll = World.CameraCurrentAlignment.Roll;
				// rotation
				if (CameraRestriction == Camera.RestrictionMode.NotAvailable & (CameraMode == CameraViewMode.Interior | CameraMode == CameraViewMode.InteriorLookAhead)) {
					// with body and head
					bodyPitch += CurrentDriverBody.Pitch;
					headPitch -= 0.2 * CurrentDriverBody.Pitch;
					bodyRoll += CurrentDriverBody.Roll;
					headRoll += 0.2 * CurrentDriverBody.Roll;
					const double bodyHeight = 0.6;
					const double headHeight = 0.1;
					{
						// body pitch
						double ry = (Math.Cos(-bodyPitch) - 1.0) * bodyHeight;
						double rz = Math.Sin(-bodyPitch) * bodyHeight;
						cF.X += dF.X * rz + uF.X * ry;
						cF.Y += dF.Y * rz + uF.Y * ry;
						cF.Z += dF.Z * rz + uF.Z * ry;
						if (bodyPitch != 0.0) {
							double cosa = Math.Cos(-bodyPitch);
							double sina = Math.Sin(-bodyPitch);
							dF.Rotate(sF, cosa, sina);
							uF.Rotate(sF, cosa, sina);
						}
					}
					{
						// body roll
						double rx = Math.Sin(bodyRoll) * bodyHeight;
						double ry = (Math.Cos(bodyRoll) - 1.0) * bodyHeight;
						cF.X += sF.X * rx + uF.X * ry;
						cF.Y += sF.Y * rx + uF.Y * ry;
						cF.Z += sF.Z * rx + uF.Z * ry;
						if (bodyRoll != 0.0) {
							double cosa = Math.Cos(-bodyRoll);
							double sina = Math.Sin(-bodyRoll);
							uF.Rotate(dF, cosa, sina);
							sF.Rotate(dF, cosa, sina);
						}
					}
					{
						// head yaw
						double rx = Math.Sin(headYaw) * headHeight;
						double rz = (Math.Cos(headYaw) - 1.0) * headHeight;
						cF.X += sF.X * rx + dF.X * rz;
						cF.Y += sF.Y * rx + dF.Y * rz;
						cF.Z += sF.Z * rx + dF.Z * rz;
						if (headYaw != 0.0) {
							double cosa = Math.Cos(headYaw);
							double sina = Math.Sin(headYaw);
							dF.Rotate(uF, cosa, sina);
							sF.Rotate(uF, cosa, sina);
						}
					}
					{
						// head pitch
						double ry = (Math.Cos(-headPitch) - 1.0) * headHeight;
						double rz = Math.Sin(-headPitch) * headHeight;
						cF.X += dF.X * rz + uF.X * ry;
						cF.Y += dF.Y * rz + uF.Y * ry;
						cF.Z += dF.Z * rz + uF.Z * ry;
						if (headPitch != 0.0) {
							double cosa = Math.Cos(-headPitch);
							double sina = Math.Sin(-headPitch);
							dF.Rotate(sF, cosa, sina);
							uF.Rotate(sF, cosa, sina);
						}
					}
					{
						// head roll
						double rx = Math.Sin(headRoll) * headHeight;
						double ry = (Math.Cos(headRoll) - 1.0) * headHeight;
						cF.X += sF.X * rx + uF.X * ry;
						cF.Y += sF.Y * rx + uF.Y * ry;
						cF.Z += sF.Z * rx + uF.Z * ry;
						if (headRoll != 0.0) {
							double cosa = Math.Cos(-headRoll);
							double sina = Math.Sin(-headRoll);
							uF.Rotate(dF, cosa, sina);
							sF.Rotate(dF, cosa, sina);
						}
					}
				} else {
					// without body or head
					double totalYaw = headYaw;
					double totalPitch = headPitch + bodyPitch;
					double totalRoll = bodyRoll + headRoll;
					if (totalYaw != 0.0) {
						double cosa = Math.Cos(totalYaw);
						double sina = Math.Sin(totalYaw);
						dF.Rotate(uF, cosa, sina);
						sF.Rotate(uF, cosa, sina);
					}
					if (totalPitch != 0.0) {
						double cosa = Math.Cos(-totalPitch);
						double sina = Math.Sin(-totalPitch);
						dF.Rotate(sF, cosa, sina);
						uF.Rotate(sF, cosa, sina);
					}
					if (totalRoll != 0.0) {
						double cosa = Math.Cos(-totalRoll);
						double sina = Math.Sin(-totalRoll);
						uF.Rotate(dF, cosa, sina);
						sF.Rotate(dF, cosa, sina);
					}
				}
				// finish
				AbsoluteCameraPosition = cF;
				AbsoluteCameraDirection = dF;
				AbsoluteCameraUp = uF;
				AbsoluteCameraSide = sF;
			}
		}
		private static void AdjustAlignment(ref double Source, double Direction, ref double Speed, double TimeElapsed) {
			AdjustAlignment(ref Source, Direction, ref Speed, TimeElapsed, false);
		}
		private static void AdjustAlignment(ref double Source, double Direction, ref double Speed, double TimeElapsed, bool Zoom) {
			if (Direction != 0.0 | Speed != 0.0) {
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
					double x = Source + Speed * TimeElapsed;
					if (!PerformProgressiveAdjustmentForCameraRestriction(ref Source, x, Zoom)) {
						Speed = 0.0;
					}
				}
			}
		}
		private static void ApplyZoom() {
			World.VerticalViewingAngle = World.OriginalVerticalViewingAngle * Math.Exp(World.CameraCurrentAlignment.Zoom);
			if (World.VerticalViewingAngle < 0.001) World.VerticalViewingAngle = 0.001;
			if (World.VerticalViewingAngle > 1.5) World.VerticalViewingAngle = 1.5;
			Renderer.UpdateViewport(Renderer.ViewPortChangeMode.NoChange);
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
		
		

		// normalize
		internal static void Normalize(ref double x, ref double y) {
			double t = x * x + y * y;
			if (t != 0.0) {
				t = 1.0 / Math.Sqrt(t);
				x *= t;
				y *= t;
			}
		}
		internal static void Normalize(ref double x, ref double y, ref double z) {
			double t = x * x + y * y + z * z;
			if (t != 0.0) {
				t = 1.0 / Math.Sqrt(t);
				x *= t;
				y *= t;
				z *= t;
			}
		}
	}
}
