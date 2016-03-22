﻿#pragma warning disable 0660 // Defines == or != but does not override Object.Equals
#pragma warning disable 0661 // Defines == or != but does not override Object.GetHashCode

using System;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using Vector2 = OpenBveApi.Math.Vector2;

namespace OpenBve {
	internal static class World {
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
		}

		// mesh material
		/// <summary>Represents material properties.</summary>
		internal struct MeshMaterial {
			/// <summary>A bit mask combining constants of the MeshMaterial structure.</summary>
			internal byte Flags;
			internal Color32 Color;
			internal Color24 TransparentColor;
			internal Color24 EmissiveColor;
			internal Textures.Texture DaytimeTexture;
			internal Textures.Texture NighttimeTexture;
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
		    internal Vector3[] BoundingBox;
			/// <summary>Creates a mesh consisting of one face, which is represented by individual vertices, and a color.</summary>
			/// <param name="Vertices">The vertices that make up one face.</param>
			/// <param name="Color">The color to be applied on the face.</param>
			internal Mesh(Vertex[] Vertices, Color32 Color) {
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
			internal Mesh(Vertex[] Vertices, int[][] FaceVertices, Color32 Color) {
				this.Vertices = Vertices;
				this.Materials = new MeshMaterial[1];
				this.Materials[0].Color = Color;
				this.Faces = new MeshFace[FaceVertices.Length];
				for (int i = 0; i < FaceVertices.Length; i++) {
					this.Faces[i] = new MeshFace(FaceVertices[i]);
				}
                this.BoundingBox = new Vector3[2];
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
		internal struct Background {
			internal Textures.Texture Texture;
			internal int Repetition;
			internal bool KeepAspectRatio;
			internal Background(Textures.Texture Texture, int Repetition, bool KeepAspectRatio) {
				this.Texture = Texture;
				this.Repetition = Repetition;
				this.KeepAspectRatio = KeepAspectRatio;
			}
		}
		internal static Background CurrentBackground = new Background(null, 6, false);
		internal static Background TargetBackground = new Background(null, 6, false);
		internal const double TargetBackgroundDefaultCountdown = 0.8;
		internal static double TargetBackgroundCountdown;

		// driver body
		internal struct DriverBody {
			internal double SlowX;
			internal double FastX;
			internal double Roll;
			internal ObjectManager.Damping RollDamping;
			internal double SlowY;
			internal double FastY;
			internal double Pitch;
			internal ObjectManager.Damping PitchDamping;
		}
		internal static DriverBody CurrentDriverBody;
		internal static void UpdateDriverBody(double TimeElapsed) {
			if (CameraRestriction == CameraRestrictionMode.NotAvailable) {
				{
					// pitch
					double targetY = TrainManager.PlayerTrain.Specs.CurrentAverageAcceleration;
					const double accelerationSlow = 0.25;
					const double accelerationFast = 2.0;
					if (CurrentDriverBody.SlowY < targetY) {
						CurrentDriverBody.SlowY += accelerationSlow * TimeElapsed;
						if (CurrentDriverBody.SlowY > targetY) {
							CurrentDriverBody.SlowY = targetY;
						}
					} else if (CurrentDriverBody.SlowY > targetY) {
						CurrentDriverBody.SlowY -= accelerationSlow * TimeElapsed;
						if (CurrentDriverBody.SlowY < targetY) {
							CurrentDriverBody.SlowY = targetY;
						}
					}
					if (CurrentDriverBody.FastY < targetY) {
						CurrentDriverBody.FastY += accelerationFast * TimeElapsed;
						if (CurrentDriverBody.FastY > targetY) {
							CurrentDriverBody.FastY = targetY;
						}
					} else if (CurrentDriverBody.FastY > targetY) {
						CurrentDriverBody.FastY -= accelerationFast * TimeElapsed;
						if (CurrentDriverBody.FastY < targetY) {
							CurrentDriverBody.FastY = targetY;
						}
					}
					double diffY = CurrentDriverBody.FastY - CurrentDriverBody.SlowY;
					diffY = (double)Math.Sign(diffY) * diffY * diffY;
					CurrentDriverBody.Pitch = 0.5 * Math.Atan(0.1 * diffY);
					if (CurrentDriverBody.Pitch > 0.1) {
						CurrentDriverBody.Pitch = 0.1;
					}
					if (CurrentDriverBody.PitchDamping == null) {
						CurrentDriverBody.PitchDamping = new ObjectManager.Damping(6.0, 0.3);
					}
					ObjectManager.UpdateDamping(ref CurrentDriverBody.PitchDamping, TimeElapsed, ref CurrentDriverBody.Pitch);
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
					if (CurrentDriverBody.SlowX < targetX) {
						CurrentDriverBody.SlowX += accelerationSlow * TimeElapsed;
						if (CurrentDriverBody.SlowX > targetX) {
							CurrentDriverBody.SlowX = targetX;
						}
					} else if (CurrentDriverBody.SlowX > targetX) {
						CurrentDriverBody.SlowX -= accelerationSlow * TimeElapsed;
						if (CurrentDriverBody.SlowX < targetX) {
							CurrentDriverBody.SlowX = targetX;
						}
					}
					if (CurrentDriverBody.FastX < targetX) {
						CurrentDriverBody.FastX += accelerationFast * TimeElapsed;
						if (CurrentDriverBody.FastX > targetX) {
							CurrentDriverBody.FastX = targetX;
						}
					} else if (CurrentDriverBody.FastX > targetX) {
						CurrentDriverBody.FastX -= accelerationFast * TimeElapsed;
						if (CurrentDriverBody.FastX < targetX) {
							CurrentDriverBody.FastX = targetX;
						}
					}
					double diffX = CurrentDriverBody.SlowX - CurrentDriverBody.FastX;
					diffX = (double)Math.Sign(diffX) * diffX * diffX;
					CurrentDriverBody.Roll = 0.5 * Math.Atan(0.3 * diffX);
					if (CurrentDriverBody.RollDamping == null) {
						CurrentDriverBody.RollDamping = new ObjectManager.Damping(6.0, 0.3);
					}
					ObjectManager.UpdateDamping(ref CurrentDriverBody.RollDamping, TimeElapsed, ref CurrentDriverBody.Roll);
				}
			}
		}
		
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
		internal static CameraAlignment CameraCurrentAlignment;
		internal static CameraAlignment CameraAlignmentDirection;
		internal static CameraAlignment CameraAlignmentSpeed;
		internal static double CameraSpeed;
		internal const double CameraInteriorTopSpeed = 5.0;
		internal const double CameraInteriorTopAngularSpeed = 5.0;
		internal const double CameraExteriorTopSpeed = 50.0;
		internal const double CameraExteriorTopAngularSpeed = 10.0;
		internal const double CameraZoomTopSpeed = 2.0;
		internal enum CameraViewMode { Interior, InteriorLookAhead, Exterior, Track, FlyBy, FlyByZooming }
		internal static CameraViewMode CameraMode;
		/// <summary>The current car the camera is anchored to</summary>
		internal static int CameraCar;
		
		// camera memory
		internal static CameraAlignment CameraSavedInterior;
		internal static CameraAlignment CameraSavedExterior;
		internal static CameraAlignment CameraSavedTrack;

		// camera restriction
		internal static Vector3 CameraRestrictionBottomLeft = new Vector3(-1.0, -1.0, 1.0);
		internal static Vector3 CameraRestrictionTopRight = new Vector3(1.0, 1.0, 1.0);
		internal enum CameraRestrictionMode {
			/// <summary>Represents a 3D cab.</summary>
			NotAvailable = -1,
			/// <summary>Represents a 2D cab with camera restriction disabled.</summary>
			Off = 0,
			/// <summary>Represents a 2D cab with camera restriction enabled.</summary>
			On = 1
		}
		internal static CameraRestrictionMode CameraRestriction = CameraRestrictionMode.NotAvailable;

		// absolute camera
		internal static Vector3 AbsoluteCameraPosition;
		internal static Vector3 AbsoluteCameraDirection;
		internal static Vector3 AbsoluteCameraUp;
		internal static Vector3 AbsoluteCameraSide;

		// camera restriction
		internal static void InitializeCameraRestriction() {
			if ((CameraMode == CameraViewMode.Interior | CameraMode == CameraViewMode.InteriorLookAhead) & CameraRestriction == CameraRestrictionMode.On) {
				CameraAlignmentSpeed = new CameraAlignment();
				UpdateAbsoluteCamera(0.0);
				if (!PerformCameraRestrictionTest()) {
					CameraCurrentAlignment = new CameraAlignment();
					VerticalViewingAngle = OriginalVerticalViewingAngle;
					MainLoop.UpdateViewport(MainLoop.ViewPortChangeMode.NoChange);
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
			if ((CameraMode != CameraViewMode.Interior & CameraMode != CameraViewMode.InteriorLookAhead) | CameraRestriction != CameraRestrictionMode.On) {
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
		    if (World.CameraRestriction == CameraRestrictionMode.On) {
				Vector3[] p = new Vector3[] { CameraRestrictionBottomLeft, CameraRestrictionTopRight };
				Vector2[] r = new Vector2[2];
				for (int j = 0; j < 2; j++) {
					// determine relative world coordinates
					World.Rotate(ref p[j].X, ref p[j].Y, ref p[j].Z, World.AbsoluteCameraDirection.X, World.AbsoluteCameraDirection.Y, World.AbsoluteCameraDirection.Z, World.AbsoluteCameraUp.X, World.AbsoluteCameraUp.Y, World.AbsoluteCameraUp.Z, World.AbsoluteCameraSide.X, World.AbsoluteCameraSide.Y, World.AbsoluteCameraSide.Z);
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
					TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, World.CameraCurrentAlignment.TrackPosition, true, false);
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
							
                            /*
                             * d1 is never used, so don't calculate it
                            double d1;
							{
								double dx = x1 - px;
								double dy = y1 - py;
								double dz = z1 - pz;
								d1 = dx * dx + dy * dy + dz * dz;
							}
                             */
							double x2 = 0.5 * (secondBestTrain.Cars[0].FrontAxle.Follower.WorldPosition.X + secondBestTrain.Cars[0].RearAxle.Follower.WorldPosition.X);
							double y2 = 0.5 * (secondBestTrain.Cars[0].FrontAxle.Follower.WorldPosition.Y + secondBestTrain.Cars[0].RearAxle.Follower.WorldPosition.Y) + heightFactor * secondBestTrain.Cars[0].Height;
							double z2 = 0.5 * (secondBestTrain.Cars[0].FrontAxle.Follower.WorldPosition.Z + secondBestTrain.Cars[0].RearAxle.Follower.WorldPosition.Z);

                            /*
                             * d2 is never used, so don't calculate it
							double d2;
							{
								double dx = x2 - px;
								double dy = y2 - py;
								double dz = z2 - pz;
								d2 = dx * dx + dy * dy + dz * dz;
							}
                             */
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
					Normalize(ref AbsoluteCameraSide.X, ref AbsoluteCameraSide.Y, ref AbsoluteCameraSide.Z);
					World.Cross(dx, dy, dz, AbsoluteCameraSide.X, AbsoluteCameraSide.Y, AbsoluteCameraSide.Z, out AbsoluteCameraUp.X, out AbsoluteCameraUp.Y, out AbsoluteCameraUp.Z);
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
						MainLoop.UpdateViewport(MainLoop.ViewPortChangeMode.NoChange);
					}
				}
			} else {
				// non-fly-by
				{
					// current alignment
					AdjustAlignment(ref World.CameraCurrentAlignment.Position.X, World.CameraAlignmentDirection.Position.X, ref World.CameraAlignmentSpeed.Position.X, TimeElapsed);
					AdjustAlignment(ref World.CameraCurrentAlignment.Position.Y, World.CameraAlignmentDirection.Position.Y, ref World.CameraAlignmentSpeed.Position.Y, TimeElapsed);
					AdjustAlignment(ref World.CameraCurrentAlignment.Position.Z, World.CameraAlignmentDirection.Position.Z, ref World.CameraAlignmentSpeed.Position.Z, TimeElapsed);
					if ((CameraMode == CameraViewMode.Interior | World.CameraMode == World.CameraViewMode.InteriorLookAhead) & CameraRestriction == CameraRestrictionMode.On) {
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
						TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, World.CameraCurrentAlignment.TrackPosition, true, false);
						q = true;
					}
					if (q) {
						UpdateViewingDistances();
					}
				}
				// camera
				double cx = World.CameraTrackFollower.WorldPosition.X;
				double cy = World.CameraTrackFollower.WorldPosition.Y;
				double cz = World.CameraTrackFollower.WorldPosition.Z;
				double dx = World.CameraTrackFollower.WorldDirection.X;
				double dy = World.CameraTrackFollower.WorldDirection.Y;
				double dz = World.CameraTrackFollower.WorldDirection.Z;
				double ux = World.CameraTrackFollower.WorldUp.X;
				double uy = World.CameraTrackFollower.WorldUp.Y;
				double uz = World.CameraTrackFollower.WorldUp.Z;
				double sx = World.CameraTrackFollower.WorldSide.X;
				double sy = World.CameraTrackFollower.WorldSide.Y;
				double sz = World.CameraTrackFollower.WorldSide.Z;
				double lookaheadYaw;
				double lookaheadPitch;
				if (CameraMode == CameraViewMode.InteriorLookAhead) {
					// look-ahead
					double d = 20.0;
					if (TrainManager.PlayerTrain.Specs.CurrentAverageSpeed > 0.0) {
						d += 3.0 * (Math.Sqrt(TrainManager.PlayerTrain.Specs.CurrentAverageSpeed * TrainManager.PlayerTrain.Specs.CurrentAverageSpeed + 1.0) - 1.0);
					}
					d -= TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].FrontAxlePosition;
					TrackManager.TrackFollower f = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].FrontAxle.Follower;
					f.TriggerType = TrackManager.EventTriggerType.None;
					TrackManager.UpdateTrackFollower(ref f, f.TrackPosition + d, true, false);
					double rx = f.WorldPosition.X - cx + World.CameraTrackFollower.WorldSide.X * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].DriverX + World.CameraTrackFollower.WorldUp.X * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].DriverY + World.CameraTrackFollower.WorldDirection.X * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].DriverZ;
					double ry = f.WorldPosition.Y - cy + World.CameraTrackFollower.WorldSide.Y * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].DriverX + World.CameraTrackFollower.WorldUp.Y * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].DriverY + World.CameraTrackFollower.WorldDirection.Y * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].DriverZ;
					double rz = f.WorldPosition.Z - cz + World.CameraTrackFollower.WorldSide.Z * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].DriverX + World.CameraTrackFollower.WorldUp.Z * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].DriverY + World.CameraTrackFollower.WorldDirection.Z * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].DriverZ;
					World.Normalize(ref rx, ref ry, ref rz);
					double t = dz * (sy * ux - sx * uy) + dy * (-sz * ux + sx * uz) + dx * (sz * uy - sy * uz);
					if (t != 0.0) {
						t = 1.0 / t;

						double tx = (rz * (-dy * ux + dx * uy) + ry * (dz * ux - dx * uz) + rx * (-dz * uy + dy * uz)) * t;
						double ty = (rz * (dy * sx - dx * sy) + ry * (-dz * sx + dx * sz) + rx * (dz * sy - dy * sz)) * t;
						double tz = (rz * (sy * ux - sx * uy) + ry * (-sz * ux + sx * uz) + rx * (sz * uy - sy * uz)) * t;
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
					double tx = World.CameraCurrentAlignment.Position.X;
					double ty = World.CameraCurrentAlignment.Position.Y;
					double tz = World.CameraCurrentAlignment.Position.Z;
					double dx2 = dx, dy2 = dy, dz2 = dz;
					double ux2 = ux, uy2 = uy, uz2 = uz;
					if ((World.CameraMode == CameraViewMode.Interior | World.CameraMode == World.CameraViewMode.InteriorLookAhead) & TrainManager.PlayerTrain != null) {
						int c = TrainManager.PlayerTrain.DriverCar;
						if (c >= 0) {
							if (TrainManager.PlayerTrain.Cars[c].CarSections.Length == 0 || !TrainManager.PlayerTrain.Cars[c].CarSections[0].Overlay) {
								double a = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].DriverPitch;
								double cosa = Math.Cos(-a);
								double sina = Math.Sin(-a);
								World.Rotate(ref dx2, ref dy2, ref dz2, sx, sy, sz, cosa, sina);
								World.Rotate(ref ux2, ref uy2, ref uz2, sx, sy, sz, cosa, sina);
							}
						}
					}
					cx += sx * tx + ux2 * ty + dx2 * tz;
					cy += sy * tx + uy2 * ty + dy2 * tz;
					cz += sz * tx + uz2 * ty + dz2 * tz;
				}
				// yaw, pitch, roll
				double headYaw = World.CameraCurrentAlignment.Yaw + lookaheadYaw;
				if ((World.CameraMode == CameraViewMode.Interior | World.CameraMode == World.CameraViewMode.InteriorLookAhead) & TrainManager.PlayerTrain != null) {
					if (TrainManager.PlayerTrain.DriverCar >= 0) {
						headYaw += TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].DriverYaw;
					}
				}
				double headPitch = World.CameraCurrentAlignment.Pitch + lookaheadPitch;
				if ((World.CameraMode == CameraViewMode.Interior | World.CameraMode == World.CameraViewMode.InteriorLookAhead) & TrainManager.PlayerTrain != null) {
					if (TrainManager.PlayerTrain.DriverCar >= 0) {
						headPitch += TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].DriverPitch;
					}
				}
				double bodyPitch = 0.0;
				double bodyRoll = 0.0;
				double headRoll = World.CameraCurrentAlignment.Roll;
				// rotation
				if (CameraRestriction == CameraRestrictionMode.NotAvailable & (CameraMode == CameraViewMode.Interior | CameraMode == CameraViewMode.InteriorLookAhead)) {
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
						cx += dx * rz + ux * ry;
						cy += dy * rz + uy * ry;
						cz += dz * rz + uz * ry;
						if (bodyPitch != 0.0) {
							double cosa = Math.Cos(-bodyPitch);
							double sina = Math.Sin(-bodyPitch);
							World.Rotate(ref dx, ref dy, ref dz, sx, sy, sz, cosa, sina);
							World.Rotate(ref ux, ref uy, ref uz, sx, sy, sz, cosa, sina);
						}
					}
					{
						// body roll
						double rx = Math.Sin(bodyRoll) * bodyHeight;
						double ry = (Math.Cos(bodyRoll) - 1.0) * bodyHeight;
						cx += sx * rx + ux * ry;
						cy += sy * rx + uy * ry;
						cz += sz * rx + uz * ry;
						if (bodyRoll != 0.0) {
							double cosa = Math.Cos(-bodyRoll);
							double sina = Math.Sin(-bodyRoll);
							World.Rotate(ref ux, ref uy, ref uz, dx, dy, dz, cosa, sina);
							World.Rotate(ref sx, ref sy, ref sz, dx, dy, dz, cosa, sina);
						}
					}
					{
						// head yaw
						double rx = Math.Sin(headYaw) * headHeight;
						double rz = (Math.Cos(headYaw) - 1.0) * headHeight;
						cx += sx * rx + dx * rz;
						cy += sy * rx + dy * rz;
						cz += sz * rx + dz * rz;
						if (headYaw != 0.0) {
							double cosa = Math.Cos(headYaw);
							double sina = Math.Sin(headYaw);
							World.Rotate(ref dx, ref dy, ref dz, ux, uy, uz, cosa, sina);
							World.Rotate(ref sx, ref sy, ref sz, ux, uy, uz, cosa, sina);
						}
					}
					{
						// head pitch
						double ry = (Math.Cos(-headPitch) - 1.0) * headHeight;
						double rz = Math.Sin(-headPitch) * headHeight;
						cx += dx * rz + ux * ry;
						cy += dy * rz + uy * ry;
						cz += dz * rz + uz * ry;
						if (headPitch != 0.0) {
							double cosa = Math.Cos(-headPitch);
							double sina = Math.Sin(-headPitch);
							World.Rotate(ref dx, ref dy, ref dz, sx, sy, sz, cosa, sina);
							World.Rotate(ref ux, ref uy, ref uz, sx, sy, sz, cosa, sina);
						}
					}
					{
						// head roll
						double rx = Math.Sin(headRoll) * headHeight;
						double ry = (Math.Cos(headRoll) - 1.0) * headHeight;
						cx += sx * rx + ux * ry;
						cy += sy * rx + uy * ry;
						cz += sz * rx + uz * ry;
						if (headRoll != 0.0) {
							double cosa = Math.Cos(-headRoll);
							double sina = Math.Sin(-headRoll);
							World.Rotate(ref ux, ref uy, ref uz, dx, dy, dz, cosa, sina);
							World.Rotate(ref sx, ref sy, ref sz, dx, dy, dz, cosa, sina);
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
						World.Rotate(ref dx, ref dy, ref dz, ux, uy, uz, cosa, sina);
						World.Rotate(ref sx, ref sy, ref sz, ux, uy, uz, cosa, sina);
					}
					if (totalPitch != 0.0) {
						double cosa = Math.Cos(-totalPitch);
						double sina = Math.Sin(-totalPitch);
						World.Rotate(ref dx, ref dy, ref dz, sx, sy, sz, cosa, sina);
						World.Rotate(ref ux, ref uy, ref uz, sx, sy, sz, cosa, sina);
					}
					if (totalRoll != 0.0) {
						double cosa = Math.Cos(-totalRoll);
						double sina = Math.Sin(-totalRoll);
						World.Rotate(ref ux, ref uy, ref uz, dx, dy, dz, cosa, sina);
						World.Rotate(ref sx, ref sy, ref sz, dx, dy, dz, cosa, sina);
					}
				}
				// finish
				AbsoluteCameraPosition = new Vector3(cx, cy, cz);
				AbsoluteCameraDirection = new Vector3(dx, dy, dz);
				AbsoluteCameraUp = new Vector3(ux, uy, uz);
				AbsoluteCameraSide = new Vector3(sx, sy, sz);
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
			MainLoop.UpdateViewport(MainLoop.ViewPortChangeMode.NoChange);
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

            //With any luck, this removes six multiplications from the Rotation calculation....
		    double Opt1 = oc*dx*dy;
		    double Opt2 = sina*dz;
		    double Opt3 = oc*dy*dz;
		    double Opt4 = sina*dx;
		    double Opt5 = sina*dy;
		    double Opt6 = oc*dx*dz;
            double x = (cosa + oc * dx * dx) * px + (Opt1 - Opt2) * py + (Opt6 + Opt5) * pz;
            double y = (cosa + oc * dy * dy) * py + (Opt1 + Opt2) * px + (Opt3 - Opt4) * pz;
            double z = (cosa + oc * dz * dz) * pz + (Opt6 - Opt5) * px + (Opt3 + Opt4) * py;

            /*
             * Original Function Unmodified
			double x = (cosa + oc * dx * dx) * px + (oc * dx * dy - sina * dz) * py + (oc * dx * dz + sina * dy) * pz;
			double y = (cosa + oc * dy * dy) * py + (oc * dx * dy + sina * dz) * px + (oc * dy * dz - sina * dx) * pz;
			double z = (cosa + oc * dz * dz) * pz + (oc * dx * dz - sina * dy) * px + (oc * dy * dz + sina * dx) * py;
             */
			px = x; py = y; pz = z;
		}
		internal static void Rotate(ref float px, ref float py, ref float pz, double dx, double dy, double dz, double cosa, double sina) {
			double t = 1.0 / Math.Sqrt(dx * dx + dy * dy + dz * dz);
			dx *= t; dy *= t; dz *= t;
			double oc = 1.0 - cosa;

            

            //With any luck, this removes six multiplications from the Rotation calculation....
            double Opt1 = oc * dx * dy;
            double Opt2 = sina * dz;
            double Opt3 = oc * dy * dz;
            double Opt4 = sina * dx;
            double Opt5 = sina * dy;
            double Opt6 = oc * dx * dz;
            double x = (cosa + oc * dx * dx) * (double)px + (Opt1 - Opt2) * (double)py + (Opt6 + Opt5) * (double)pz;
            double y = (cosa + oc * dy * dy) * (double)py + (Opt1 + Opt2) * (double)px + (Opt3 - Opt4) * (double)pz;
            double z = (cosa + oc * dz * dz) * (double)pz + (Opt6 - Opt5) * (double)px + (Opt3 + Opt4) * (double)py;

            /*
             * Original Function Unmodified
			double x = (cosa + oc * dx * dx) * (double)px + (oc * dx * dy - sina * dz) * (double)py + (oc * dx * dz + sina * dy) * (double)pz;
			double y = (cosa + oc * dy * dy) * (double)py + (oc * dx * dy + sina * dz) * (double)px + (oc * dy * dz - sina * dx) * (double)pz;
			double z = (cosa + oc * dz * dz) * (double)pz + (oc * dx * dz - sina * dy) * (double)px + (oc * dy * dz + sina * dx) * (double)py;
             */

			px = (float)x; py = (float)y; pz = (float)z;
		}
		internal static void Rotate(ref Vector2 Vector, double cosa, double sina) {
			double u = Vector.X * cosa - Vector.Y * sina;
			double v = Vector.X * sina + Vector.Y * cosa;
			Vector.X = u;
			Vector.Y = v;
		}
		internal static void Rotate(ref float px, ref float py, ref float pz, double dx, double dy, double dz, double ux, double uy, double uz, double sx, double sy, double sz) {
		    var x = sx * (double)px + ux * (double)py + dx * (double)pz;
			var y = sy * (double)px + uy * (double)py + dy * (double)pz;
			var z = sz * (double)px + uz * (double)py + dz * (double)pz;
			px = (float)x; py = (float)y; pz = (float)z;
		}
		internal static void Rotate(ref double px, ref double py, ref double pz, double dx, double dy, double dz, double ux, double uy, double uz, double sx, double sy, double sz) {
			var x = sx * px + ux * py + dx * pz;
			var y = sy * px + uy * py + dy * pz;
			var z = sz * px + uz * py + dz * pz;
			px = x; py = y; pz = z;
		}
		internal static void Rotate(ref float px, ref float py, ref float pz, Transformation t) {
			var x = t.X.X * (double)px + t.Y.X * (double)py + t.Z.X * (double)pz;
			var y = t.X.Y * (double)px + t.Y.Y * (double)py + t.Z.Y * (double)pz;
			var z = t.X.Z * (double)px + t.Y.Z * (double)py + t.Z.Z * (double)pz;
			px = (float)x; py = (float)y; pz = (float)z;
		}
		internal static void Rotate(ref double px, ref double py, ref double pz, Transformation t) {
			var x = t.X.X * px + t.Y.X * py + t.Z.X * pz;
			var y = t.X.Y * px + t.Y.Y * py + t.Z.Y * pz;
			var z = t.X.Z * px + t.Y.Z * py + t.Z.Z * pz;
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

		/// <summary>Generates the default lighting normals for a mesh</summary>
		/// <param name="Mesh">The mesh for which to generate normals</param>
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
                        if (Vector3.IsZero(Mesh.Faces[FaceIndex].Vertices[j].Normal))
                        {
							Mesh.Faces[FaceIndex].Vertices[j].Normal = new Vector3(mx, my, mz);
						}
					}
				} else {
					for (int j = 0; j < Mesh.Faces[FaceIndex].Vertices.Length; j++) {
                        if (Vector3.IsZero(Mesh.Faces[FaceIndex].Vertices[j].Normal))
                        {
							Mesh.Faces[FaceIndex].Vertices[j].Normal = new Vector3(0.0f, 1.0f, 0.0f);
						}
					}
				}
			}
		}

	}
}
