using System;

namespace OpenBve {
	internal static class ObjectManager {

		// unified objects
		internal abstract class UnifiedObject { }

		// static objects
		internal class StaticObject : UnifiedObject {
			internal World.Mesh Mesh;
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
		}
		internal static StaticObject[] Objects = new StaticObject[16];
		internal static int ObjectsUsed;
		internal static int[] ObjectsSortedByStart = new int[] { };
		internal static int[] ObjectsSortedByEnd = new int[] { };
		internal static int ObjectsSortedByStartPointer = 0;
		internal static int ObjectsSortedByEndPointer = 0;
		internal static double LastUpdatedTrackPosition = 0.0;

		// animated objects
		internal class Damping {
			internal double NaturalFrequency;
			internal double NaturalTime;
			internal double DampingRatio;
			internal double NaturalDampingFrequency;
			internal double OriginalAngle;
			internal double OriginalDerivative;
			internal double TargetAngle;
			internal double CurrentAngle;
			internal double CurrentValue;
			internal double CurrentTimeDelta;
			internal Damping(double NaturalFrequency, double DampingRatio) {
				if (NaturalFrequency < 0.0) {
					throw new ArgumentException("NaturalFrequency must be non-negative in the constructor of the Damping class.");
				} else if (DampingRatio < 0.0) {
					throw new ArgumentException("DampingRatio must be non-negative in the constructor of the Damping class.");
				} else {
					this.NaturalFrequency = NaturalFrequency;
					this.NaturalTime = NaturalFrequency != 0.0 ? 1.0 / NaturalFrequency : 0.0;
					this.DampingRatio = DampingRatio;
					if (DampingRatio < 1.0) {
						this.NaturalDampingFrequency = NaturalFrequency * Math.Sqrt(1.0 - DampingRatio * DampingRatio);
					} else if (DampingRatio == 1.0) {
						this.NaturalDampingFrequency = NaturalFrequency;
					} else {
						this.NaturalDampingFrequency = NaturalFrequency * Math.Sqrt(DampingRatio * DampingRatio - 1.0);
					}
					this.OriginalAngle = 0.0;
					this.OriginalDerivative = 0.0;
					this.TargetAngle = 0.0;
					this.CurrentAngle = 0.0;
					this.CurrentValue = 1.0;
					this.CurrentTimeDelta = 0.0;
				}
			}
			internal Damping Clone() {
				return (Damping)this.MemberwiseClone();
			}
		}
		internal struct AnimatedObjectState {
			internal World.Vector3D Position;
			internal ObjectManager.StaticObject Object;
		}
		internal class AnimatedObject {
			// states
			internal AnimatedObjectState[] States;
			internal FunctionScripts.FunctionScript StateFunction;
			internal int CurrentState;
			internal World.Vector3D TranslateXDirection;
			internal World.Vector3D TranslateYDirection;
			internal World.Vector3D TranslateZDirection;
			internal FunctionScripts.FunctionScript TranslateXFunction;
			internal FunctionScripts.FunctionScript TranslateYFunction;
			internal FunctionScripts.FunctionScript TranslateZFunction;
			internal World.Vector3D RotateXDirection;
			internal World.Vector3D RotateYDirection;
			internal World.Vector3D RotateZDirection;
			internal FunctionScripts.FunctionScript RotateXFunction;
			internal FunctionScripts.FunctionScript RotateYFunction;
			internal FunctionScripts.FunctionScript RotateZFunction;
			internal Damping RotateXDamping;
			internal Damping RotateYDamping;
			internal Damping RotateZDamping;
			internal World.Vector2D TextureShiftXDirection;
			internal World.Vector2D TextureShiftYDirection;
			internal FunctionScripts.FunctionScript TextureShiftXFunction;
			internal FunctionScripts.FunctionScript TextureShiftYFunction;
			internal bool LEDClockwiseWinding;
			internal double LEDInitialAngle;
			internal double LEDLastAngle;
			/// <summary>If LEDFunction is used, an array of five vectors representing the bottom-left, up-left, up-right, bottom-right and center coordinates of the LED square, or a null reference otherwise.</summary>
			internal World.Vector3D[] LEDVectors;
			internal FunctionScripts.FunctionScript LEDFunction;
			internal double RefreshRate;
			internal double CurrentTrackZOffset;
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
					Result.States[i].Object = CloneObject(this.States[i].Object);
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
					Result.LEDVectors = new World.Vector3D[this.LEDVectors.Length];
					for (int i = 0; i < this.LEDVectors.Length; i++) {
						Result.LEDVectors[i] = this.LEDVectors[i];
					}
				} else {
					Result.LEDVectors = null;
				}
				Result.LEDFunction = this.LEDFunction == null ? null : this.LEDFunction.Clone();
				Result.RefreshRate = this.RefreshRate;
				Result.CurrentTrackZOffset = 0.0;
				Result.SecondsSinceLastUpdate = 0.0;
				Result.ObjectIndex = -1;
				return Result;
			}
		}
		internal class AnimatedObjectCollection : UnifiedObject {
			internal AnimatedObject[] Objects;
		}
		internal static void InitializeAnimatedObject(ref AnimatedObject Object, int StateIndex, bool Overlay, bool Show) {
			int i = Object.ObjectIndex;
			Renderer.HideObject(i);
			int t = StateIndex;
			if (t >= 0 && Object.States[t].Object != null) {
				int m = Object.States[t].Object.Mesh.Vertices.Length;
				ObjectManager.Objects[i].Mesh.Vertices = new World.Vertex[m];
				for (int k = 0; k < m; k++) {
					ObjectManager.Objects[i].Mesh.Vertices[k] = Object.States[t].Object.Mesh.Vertices[k];
				}
				m = Object.States[t].Object.Mesh.Faces.Length;
				ObjectManager.Objects[i].Mesh.Faces = new World.MeshFace[m];
				for (int k = 0; k < m; k++) {
					ObjectManager.Objects[i].Mesh.Faces[k].Flags = Object.States[t].Object.Mesh.Faces[k].Flags;
					ObjectManager.Objects[i].Mesh.Faces[k].Material = Object.States[t].Object.Mesh.Faces[k].Material;
					int o = Object.States[t].Object.Mesh.Faces[k].Vertices.Length;
					ObjectManager.Objects[i].Mesh.Faces[k].Vertices = new World.MeshFaceVertex[o];
					for (int h = 0; h < o; h++) {
						ObjectManager.Objects[i].Mesh.Faces[k].Vertices[h] = Object.States[t].Object.Mesh.Faces[k].Vertices[h];
					}
				}
				ObjectManager.Objects[i].Mesh.Materials = Object.States[t].Object.Mesh.Materials;
			} else {
				ObjectManager.Objects[i] = null;
				ObjectManager.Objects[i] = new StaticObject();
				ObjectManager.Objects[i].Mesh.Faces = new World.MeshFace[] { };
				ObjectManager.Objects[i].Mesh.Materials = new World.MeshMaterial[] { };
				ObjectManager.Objects[i].Mesh.Vertices = new World.Vertex[] { };
			}
			Object.CurrentState = StateIndex;
			if (Show) {
				if (Overlay) {
					Renderer.ShowObject(i, Renderer.ObjectType.Overlay);
				} else {
					Renderer.ShowObject(i, Renderer.ObjectType.Dynamic);
				}
			}
		}

		internal static void UpdateAnimatedObject(ref AnimatedObject Object, bool IsPartOfTrain, TrainManager.Train Train, int CarIndex, int SectionIndex, double TrackPosition, World.Vector3D Position, World.Vector3D Direction, World.Vector3D Up, World.Vector3D Side, bool Overlay, bool UpdateFunctions, bool Show, double TimeElapsed) {
			int s = Object.CurrentState;
			int i = Object.ObjectIndex;
			// state change
			if (Object.StateFunction != null & UpdateFunctions) {
				double sd = Object.StateFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed);
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
					x = Object.TranslateXFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed);
				} else  {
					x = Object.TranslateXFunction.LastResult;
				}
				double rx = Object.TranslateXDirection.X, ry = Object.TranslateXDirection.Y, rz = Object.TranslateXDirection.Z;
				World.Rotate(ref rx, ref ry, ref rz, Direction.X, Direction.Y, Direction.Z, Up.X, Up.Y, Up.Z, Side.X, Side.Y, Side.Z);
				Position.X += x * rx;
				Position.Y += x * ry;
				Position.Z += x * rz;
			}
			if (Object.TranslateYFunction != null) {
				double y;
				if (UpdateFunctions) {
					y = Object.TranslateYFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed);
				} else {
					y = Object.TranslateYFunction.LastResult;
				}
				double rx = Object.TranslateYDirection.X, ry = Object.TranslateYDirection.Y, rz = Object.TranslateYDirection.Z;
				World.Rotate(ref rx, ref ry, ref rz, Direction.X, Direction.Y, Direction.Z, Up.X, Up.Y, Up.Z, Side.X, Side.Y, Side.Z);
				Position.X += y * rx;
				Position.Y += y * ry;
				Position.Z += y * rz;
			}
			if (Object.TranslateZFunction != null) {
				double z;
				if (UpdateFunctions) {
					z = Object.TranslateZFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed);
				} else {
					z = Object.TranslateZFunction.LastResult;
				}
				double rx = Object.TranslateZDirection.X, ry = Object.TranslateZDirection.Y, rz = Object.TranslateZDirection.Z;
				World.Rotate(ref rx, ref ry, ref rz, Direction.X, Direction.Y, Direction.Z, Up.X, Up.Y, Up.Z, Side.X, Side.Y, Side.Z);
				Position.X += z * rx;
				Position.Y += z * ry;
				Position.Z += z * rz;
			}
			// rotation
			bool rotateX = Object.RotateXFunction != null;
			bool rotateY = Object.RotateYFunction != null;
			bool rotateZ = Object.RotateZFunction != null;
			double cosX, sinX;
			if (rotateX) {
				double a;
				if (UpdateFunctions) {
					a = Object.RotateXFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed);
				} else {
					a = Object.RotateXFunction.LastResult;
				}
				ObjectManager.UpdateDamping(ref Object.RotateXDamping, TimeElapsed, ref a);
				cosX = Math.Cos(a);
				sinX = Math.Sin(a);
			} else {
				cosX = 0.0; sinX = 0.0;
			}
			double cosY, sinY;
			if (rotateY) {
				double a;
				if (UpdateFunctions) {
					a = Object.RotateYFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed);
				} else {
					a = Object.RotateYFunction.LastResult;
				}
				ObjectManager.UpdateDamping(ref Object.RotateYDamping, TimeElapsed, ref a);
				cosY = Math.Cos(a);
				sinY = Math.Sin(a);
			} else {
				cosY = 0.0; sinY = 0.0;
			}
			double cosZ, sinZ;
			if (rotateZ) {
				double a;
				if (UpdateFunctions) {
					a = Object.RotateZFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed);
				} else {
					a = Object.RotateZFunction.LastResult;
				}
				ObjectManager.UpdateDamping(ref Object.RotateZDamping, TimeElapsed, ref a);
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
					double x = Object.TextureShiftXFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed);
					x -= Math.Floor(x);
					for (int k = 0; k < ObjectManager.Objects[i].Mesh.Vertices.Length; k++) {
						ObjectManager.Objects[i].Mesh.Vertices[k].TextureCoordinates.X += (float)(x * Object.TextureShiftXDirection.X);
						ObjectManager.Objects[i].Mesh.Vertices[k].TextureCoordinates.Y += (float)(x * Object.TextureShiftXDirection.Y);
					}
				}
				if (shifty) {
					double y = Object.TextureShiftYFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed);
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
					double lastangle = Object.LEDFunction.LastResult;
					ledangle = Object.LEDFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed);
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
								Object.States[s].Object.Mesh.Vertices[v].Coordinates = new World.Vector3D(cx, cy, cz);
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
								Object.States[s].Object.Mesh.Vertices[v].Coordinates = new World.Vector3D(lx, ly, lz);
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
								Object.States[s].Object.Mesh.Vertices[v + 0].Coordinates = new World.Vector3D(cx, cy, cz);
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
								Object.States[s].Object.Mesh.Vertices[v + 1].Coordinates = new World.Vector3D(lx, ly, lz);
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
								Object.States[s].Object.Mesh.Vertices[v].Coordinates = new World.Vector3D(lx, ly, lz);
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
								Object.States[s].Object.Mesh.Vertices[v].Coordinates = new World.Vector3D(cx, cy, cz);
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
								Object.States[s].Object.Mesh.Vertices[v + 1].Coordinates = new World.Vector3D(cx, cy, cz);
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
								Object.States[s].Object.Mesh.Vertices[v + 0].Coordinates = new World.Vector3D(lx, ly, lz);
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
					World.Rotate(ref ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.X, ref ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Y, ref ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Z, Object.RotateXDirection.X, Object.RotateXDirection.Y, Object.RotateXDirection.Z, cosX, sinX);
				}
				if (rotateY) {
					World.Rotate(ref ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.X, ref ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Y, ref ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Z, Object.RotateYDirection.X, Object.RotateYDirection.Y, Object.RotateYDirection.Z, cosY, sinY);
				}
				if (rotateZ) {
					World.Rotate(ref ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.X, ref ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Y, ref ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Z, Object.RotateZDirection.X, Object.RotateZDirection.Y, Object.RotateZDirection.Z, cosZ, sinZ);
				}
				// translate
				if (Overlay & World.CameraRestriction != World.CameraRestrictionMode.NotAvailable) {
					ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.X += Object.States[s].Position.X - Position.X;
					ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Y += Object.States[s].Position.Y - Position.Y;
					ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Z += Object.States[s].Position.Z - Position.Z;
					World.Rotate(ref ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.X, ref ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Y, ref ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Z, World.AbsoluteCameraDirection.X, World.AbsoluteCameraDirection.Y, World.AbsoluteCameraDirection.Z, World.AbsoluteCameraUp.X, World.AbsoluteCameraUp.Y, World.AbsoluteCameraUp.Z, World.AbsoluteCameraSide.X, World.AbsoluteCameraSide.Y, World.AbsoluteCameraSide.Z);
					double dx = -Math.Tan(World.CameraCurrentAlignment.Yaw) - World.CameraCurrentAlignment.Position.X;
					double dy = -Math.Tan(World.CameraCurrentAlignment.Pitch) - World.CameraCurrentAlignment.Position.Y;
					double dz = -World.CameraCurrentAlignment.Position.Z;
					ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.X += World.AbsoluteCameraPosition.X + dx * World.AbsoluteCameraSide.X + dy * World.AbsoluteCameraUp.X + dz * World.AbsoluteCameraDirection.X;
					ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Y += World.AbsoluteCameraPosition.Y + dx * World.AbsoluteCameraSide.Y + dy * World.AbsoluteCameraUp.Y + dz * World.AbsoluteCameraDirection.Y;
					ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Z += World.AbsoluteCameraPosition.Z + dx * World.AbsoluteCameraSide.Z + dy * World.AbsoluteCameraUp.Z + dz * World.AbsoluteCameraDirection.Z;
				} else {
					ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.X += Object.States[s].Position.X;
					ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Y += Object.States[s].Position.Y;
					ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Z += Object.States[s].Position.Z;
					World.Rotate(ref ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.X, ref ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Y, ref ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Z, Direction.X, Direction.Y, Direction.Z, Up.X, Up.Y, Up.Z, Side.X, Side.Y, Side.Z);
					ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.X += Position.X;
					ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Y += Position.Y;
					ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Z += Position.Z;
				}
			}
			// update normals
			for (int k = 0; k < Object.States[s].Object.Mesh.Faces.Length; k++) {
				for (int h = 0; h < Object.States[s].Object.Mesh.Faces[k].Vertices.Length; h++) {
					ObjectManager.Objects[i].Mesh.Faces[k].Vertices[h].Normal = Object.States[s].Object.Mesh.Faces[k].Vertices[h].Normal;
				}
				for (int h = 0; h < Object.States[s].Object.Mesh.Faces[k].Vertices.Length; h++) {
					if (!Object.States[s].Object.Mesh.Faces[k].Vertices[h].Normal.IsZero()) {
						if (rotateX) {
							World.Rotate(ref ObjectManager.Objects[i].Mesh.Faces[k].Vertices[h].Normal.X, ref ObjectManager.Objects[i].Mesh.Faces[k].Vertices[h].Normal.Y, ref ObjectManager.Objects[i].Mesh.Faces[k].Vertices[h].Normal.Z, Object.RotateXDirection.X, Object.RotateXDirection.Y, Object.RotateXDirection.Z, cosX, sinX);
						}
						if (rotateY) {
							World.Rotate(ref ObjectManager.Objects[i].Mesh.Faces[k].Vertices[h].Normal.X, ref ObjectManager.Objects[i].Mesh.Faces[k].Vertices[h].Normal.Y, ref ObjectManager.Objects[i].Mesh.Faces[k].Vertices[h].Normal.Z, Object.RotateYDirection.X, Object.RotateYDirection.Y, Object.RotateYDirection.Z, cosY, sinY);
						}
						if (rotateZ) {
							World.Rotate(ref ObjectManager.Objects[i].Mesh.Faces[k].Vertices[h].Normal.X, ref ObjectManager.Objects[i].Mesh.Faces[k].Vertices[h].Normal.Y, ref ObjectManager.Objects[i].Mesh.Faces[k].Vertices[h].Normal.Z, Object.RotateZDirection.X, Object.RotateZDirection.Y, Object.RotateZDirection.Z, cosZ, sinZ);
						}
						World.Rotate(ref ObjectManager.Objects[i].Mesh.Faces[k].Vertices[h].Normal.X, ref ObjectManager.Objects[i].Mesh.Faces[k].Vertices[h].Normal.Y, ref ObjectManager.Objects[i].Mesh.Faces[k].Vertices[h].Normal.Z, Direction.X, Direction.Y, Direction.Z, Up.X, Up.Y, Up.Z, Side.X, Side.Y, Side.Z);
					}
				}
				// visibility changed
				if (Show) {
					if (Overlay) {
						Renderer.ShowObject(i, Renderer.ObjectType.Overlay);
					} else {
						Renderer.ShowObject(i, Renderer.ObjectType.Dynamic);
					}
				} else {
					Renderer.HideObject(i);
				}
			}
		}

		// update damping
		internal static void UpdateDamping(ref Damping Damping, double TimeElapsed, ref double Angle) {
			if (TimeElapsed < 0.0) {
				TimeElapsed = 0.0;
			} else if (TimeElapsed > 1.0) {
				TimeElapsed = 1.0;
			}
			if (Damping != null) {
				if (Damping.CurrentTimeDelta > Damping.NaturalTime) {
					// update
					double newDerivative;
					if (Damping.NaturalFrequency == 0.0) {
						newDerivative = 0.0;
					} else if (Damping.DampingRatio == 0.0) {
						newDerivative = Damping.OriginalDerivative * Math.Cos(Damping.NaturalFrequency * Damping.CurrentTimeDelta) - Damping.NaturalFrequency * Math.Sin(Damping.NaturalFrequency * Damping.CurrentTimeDelta);
					} else if (Damping.DampingRatio < 1.0) {
						newDerivative = Math.Exp(-Damping.DampingRatio * Damping.NaturalFrequency * Damping.CurrentTimeDelta) * (Damping.NaturalDampingFrequency * Damping.OriginalDerivative * Math.Cos(Damping.NaturalDampingFrequency * Damping.CurrentTimeDelta) - (Damping.NaturalDampingFrequency * Damping.NaturalDampingFrequency + Damping.DampingRatio * Damping.NaturalFrequency * (Damping.DampingRatio * Damping.NaturalFrequency + Damping.OriginalDerivative)) * Math.Sin(Damping.NaturalDampingFrequency * Damping.CurrentTimeDelta)) / Damping.NaturalDampingFrequency;
					} else if (Damping.DampingRatio == 1.0) {
						newDerivative = Math.Exp(-Damping.NaturalFrequency * Damping.CurrentTimeDelta) * (Damping.OriginalDerivative - Damping.NaturalFrequency * (Damping.NaturalFrequency + Damping.OriginalDerivative) * Damping.CurrentTimeDelta);
					} else {
						newDerivative = Math.Exp(-Damping.DampingRatio * Damping.NaturalFrequency * Damping.CurrentTimeDelta) * (Damping.NaturalDampingFrequency * Damping.OriginalDerivative * Math.Cosh(Damping.NaturalDampingFrequency * Damping.CurrentTimeDelta) + (Damping.NaturalDampingFrequency * Damping.NaturalDampingFrequency - Damping.DampingRatio * Damping.NaturalFrequency * (Damping.DampingRatio * Damping.NaturalFrequency + Damping.OriginalDerivative)) * Math.Sinh(Damping.NaturalDampingFrequency * Damping.CurrentTimeDelta)) / Damping.NaturalDampingFrequency;
					}
					double a = Damping.TargetAngle - Damping.OriginalAngle;
					Damping.OriginalAngle = Damping.CurrentAngle;
					Damping.TargetAngle = Angle;
					double b = Damping.TargetAngle - Damping.OriginalAngle;
					double r = b == 0.0 ? 1.0 : a / b;
					Damping.OriginalDerivative = newDerivative * r;
					if (Damping.NaturalTime > 0.0) {
						Damping.CurrentTimeDelta = Damping.CurrentTimeDelta % Damping.NaturalTime;
					}
				}
				{
					// perform
					double newValue;
					if (Damping.NaturalFrequency == 0.0) {
						newValue = 1.0;
					} else if (Damping.DampingRatio == 0.0) {
						newValue = Math.Cos(Damping.NaturalFrequency * Damping.CurrentTimeDelta) + Damping.OriginalDerivative * Math.Sin(Damping.NaturalFrequency * Damping.CurrentTimeDelta) / Damping.NaturalFrequency;
					} else if (Damping.DampingRatio < 1.0) {
						double n = (Damping.OriginalDerivative + Damping.NaturalFrequency * Damping.DampingRatio) / Damping.NaturalDampingFrequency;
						newValue = Math.Exp(-Damping.DampingRatio * Damping.NaturalFrequency * Damping.CurrentTimeDelta) * (Math.Cos(Damping.NaturalDampingFrequency * Damping.CurrentTimeDelta) + n * Math.Sin(Damping.NaturalDampingFrequency * Damping.CurrentTimeDelta));
					} else if (Damping.DampingRatio == 1.0) {
						newValue = Math.Exp(-Damping.NaturalFrequency * Damping.CurrentTimeDelta) * (1.0 + (Damping.OriginalDerivative + Damping.NaturalFrequency) * Damping.CurrentTimeDelta);
					} else {
						double n = (Damping.OriginalDerivative + Damping.NaturalFrequency * Damping.DampingRatio) / Damping.NaturalDampingFrequency;
						newValue = Math.Exp(-Damping.DampingRatio * Damping.NaturalFrequency * Damping.CurrentTimeDelta) * (Math.Cosh(Damping.NaturalDampingFrequency * Damping.CurrentTimeDelta) + n * Math.Sinh(Damping.NaturalDampingFrequency * Damping.CurrentTimeDelta));
					}
					Damping.CurrentValue = newValue;
					Damping.CurrentAngle = Damping.TargetAngle * (1.0 - newValue) + Damping.OriginalAngle * newValue;
					Damping.CurrentTimeDelta += TimeElapsed;
					Angle = Damping.CurrentAngle;
				}
			}
		}

		// animated world object
		internal class AnimatedWorldObject {
			internal World.Vector3D Position;
			internal double TrackPosition;
			internal World.Vector3D Direction;
			internal World.Vector3D Up;
			internal World.Vector3D Side;
			internal AnimatedObject Object;
			internal int SectionIndex;
			internal double Radius;
			internal bool Visible;
		}
		internal static AnimatedWorldObject[] AnimatedWorldObjects = new AnimatedWorldObject[4];
		internal static int AnimatedWorldObjectsUsed = 0;
		internal static void CreateAnimatedWorldObjects(AnimatedObject[] Prototypes, World.Vector3D Position, World.Transformation BaseTransformation, World.Transformation AuxTransformation, int SectionIndex, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness, bool DuplicateMaterials) {
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
							World.Vector3D p = Position;
							World.Transformation t = new OpenBve.World.Transformation(BaseTransformation, AuxTransformation);
							World.Vector3D s = t.X;
							World.Vector3D u = t.Y;
							World.Vector3D d = t.Z;
							p.X += Prototypes[i].States[0].Position.X * s.X + Prototypes[i].States[0].Position.Y * u.X + Prototypes[i].States[0].Position.Z * d.X;
							p.Y += Prototypes[i].States[0].Position.X * s.Y + Prototypes[i].States[0].Position.Y * u.Y + Prototypes[i].States[0].Position.Z * d.Y;
							p.Z += Prototypes[i].States[0].Position.X * s.Z + Prototypes[i].States[0].Position.Y * u.Z + Prototypes[i].States[0].Position.Z * d.Z;
							double zOffset = Prototypes[i].States[0].Position.Z;
							CreateStaticObject(Prototypes[i].States[0].Object, p, BaseTransformation, AuxTransformation, AccurateObjectDisposal, zOffset, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness, DuplicateMaterials);
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
		internal static int CreateAnimatedWorldObject(AnimatedObject Prototype, World.Vector3D Position, World.Transformation BaseTransformation, World.Transformation AuxTransformation, int SectionIndex, double TrackPosition, double Brightness) {
			int a = AnimatedWorldObjectsUsed;
			if (a >= AnimatedWorldObjects.Length) {
				Array.Resize<AnimatedWorldObject>(ref AnimatedWorldObjects, AnimatedWorldObjects.Length << 1);
			}
			World.Transformation FinalTransformation = new World.Transformation(BaseTransformation, AuxTransformation);
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
					AnimatedWorldObjects[a].Object.States[i].Object.Mesh.Faces = new World.MeshFace[] { };
					AnimatedWorldObjects[a].Object.States[i].Object.Mesh.Materials = new World.MeshMaterial[] { };
					AnimatedWorldObjects[a].Object.States[i].Object.Mesh.Vertices = new World.Vertex[] { };
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
						Renderer.ShowObject(AnimatedWorldObjects[i].Object.ObjectIndex, Renderer.ObjectType.Dynamic);
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
		internal enum ObjectLoadMode { Normal, DontAllowUnloadOfTextures }
		internal static UnifiedObject LoadObject(string FileName, System.Text.Encoding Encoding, ObjectLoadMode LoadMode, bool PreserveVertices, bool ForceTextureRepeatX, bool ForceTextureRepeatY) {
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
						Result = CsvB3dObjectParser.ReadObject(FileName, Encoding, LoadMode, ForceTextureRepeatX, ForceTextureRepeatY);
						break;
					case ".x":
						Result = XObjectParser.ReadObject(FileName, Encoding, LoadMode, ForceTextureRepeatX, ForceTextureRepeatY);
						break;
					case ".animated":
						Result = AnimatedObjectParser.ReadObject(FileName, Encoding, LoadMode);
						break;
					default:
						Interface.AddMessage(Interface.MessageType.Error, false, "The file extension is not supported: " + FileName);
						return null;
				}
				OptimizeObject(Result, PreserveVertices);
				return Result;
				#if !DEBUG
			} catch (Exception ex) {
				Interface.AddMessage(Interface.MessageType.Error, true, "An unexpected error occured (" + ex.Message + ") while attempting to load the file " + FileName);
				return null;
			}
			#endif
		}
		internal static StaticObject LoadStaticObject(string FileName, System.Text.Encoding Encoding, ObjectLoadMode LoadMode, bool PreserveVertices, bool ForceTextureRepeatX, bool ForceTextureRepeatY) {
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
						Result = CsvB3dObjectParser.ReadObject(FileName, Encoding, LoadMode, ForceTextureRepeatX, ForceTextureRepeatY);
						break;
					case ".x":
						Result = XObjectParser.ReadObject(FileName, Encoding, LoadMode, ForceTextureRepeatX, ForceTextureRepeatY);
						break;
					case ".animated":
						Interface.AddMessage(Interface.MessageType.Error, false, "Tried to load an animated object even though only static objects are allowed: " + FileName);
						return null;
					default:
						Interface.AddMessage(Interface.MessageType.Error, false, "The file extension is not supported: " + FileName);
						return null;
				}
				OptimizeObject(Result, PreserveVertices);
				return Result;
				#if !DEBUG
			} catch (Exception ex) {
				Interface.AddMessage(Interface.MessageType.Error, true, "An unexpected error occured (" + ex.Message + ") while attempting to load the file " + FileName);
				return null;
			}
			#endif
		}

		// optimize object
		internal static void OptimizeObject(UnifiedObject Prototype, bool PreserveVertices) {
			if (Prototype is StaticObject) {
				StaticObject s = (StaticObject)Prototype;
				OptimizeObject(s, PreserveVertices);
			} else if (Prototype is AnimatedObjectCollection) {
				AnimatedObjectCollection a = (AnimatedObjectCollection)Prototype;
				for (int i = 0; i < a.Objects.Length; i++) {
					for (int j = 0; j < a.Objects[i].States.Length; j++) {
						OptimizeObject(a.Objects[i].States[j].Object, PreserveVertices);
					}
				}
			}
		}
		internal static void OptimizeObject(StaticObject Prototype, bool PreserveVertices) {
			if (Prototype == null) return;
			int v = Prototype.Mesh.Vertices.Length;
			int m = Prototype.Mesh.Materials.Length;
			int f = Prototype.Mesh.Faces.Length;
			if (f >= Interface.CurrentOptions.ObjectOptimizationBasicThreshold) return;
			// eliminate invalid faces and reduce incomplete faces
			for (int i = 0; i < f; i++) {
				int type = Prototype.Mesh.Faces[i].Flags & World.MeshFace.FaceTypeMask;
				bool keep;
				if (type == World.MeshFace.FaceTypeTriangles) {
					keep = Prototype.Mesh.Faces[i].Vertices.Length >= 3;
					if (keep) {
						int n = (Prototype.Mesh.Faces[i].Vertices.Length / 3) * 3;
						if (Prototype.Mesh.Faces[i].Vertices.Length != n) {
							Array.Resize<World.MeshFaceVertex>(ref Prototype.Mesh.Faces[i].Vertices, n);
						}
					}
				} else if (type == World.MeshFace.FaceTypeQuads) {
					keep = Prototype.Mesh.Faces[i].Vertices.Length >= 4;
					if (keep) {
						int n = Prototype.Mesh.Faces[i].Vertices.Length & ~3;
						if (Prototype.Mesh.Faces[i].Vertices.Length != n) {
							Array.Resize<World.MeshFaceVertex>(ref Prototype.Mesh.Faces[i].Vertices, n);
						}
					}
				} else if (type == World.MeshFace.FaceTypeQuadStrip) {
					keep = Prototype.Mesh.Faces[i].Vertices.Length >= 4;
					if (keep) {
						int n = Prototype.Mesh.Faces[i].Vertices.Length & ~1;
						if (Prototype.Mesh.Faces[i].Vertices.Length != n) {
							Array.Resize<World.MeshFaceVertex>(ref Prototype.Mesh.Faces[i].Vertices, n);
						}
					}
				} else {
					keep = Prototype.Mesh.Faces[i].Vertices.Length >= 3;
				}
				if (!keep) {
					for (int j = i; j < f - 1; j++) {
						Prototype.Mesh.Faces[j] = Prototype.Mesh.Faces[j + 1];
					}
					f--;
					i--;
				}
			}
			// eliminate unused vertices
			if (!PreserveVertices) {
				for (int i = 0; i < v; i++) {
					bool keep = false;
					for (int j = 0; j < f; j++) {
						for (int k = 0; k < Prototype.Mesh.Faces[j].Vertices.Length; k++) {
							if (Prototype.Mesh.Faces[j].Vertices[k].Index == i) {
								keep = true;
								break;
							}
						}
						if (keep) {
							break;
						}
					}
					if (!keep) {
						for (int j = 0; j < f; j++) {
							for (int k = 0; k < Prototype.Mesh.Faces[j].Vertices.Length; k++) {
								if (Prototype.Mesh.Faces[j].Vertices[k].Index > i) {
									Prototype.Mesh.Faces[j].Vertices[k].Index--;
								}
							}
						}
						for (int j = i; j < v - 1; j++) {
							Prototype.Mesh.Vertices[j] = Prototype.Mesh.Vertices[j + 1];
						}
						v--;
						i--;
					}
				}
			}
			// eliminate duplicate vertices
			if (!PreserveVertices) {
				for (int i = 0; i < v - 1; i++) {
					for (int j = i + 1; j < v; j++) {
						if (Prototype.Mesh.Vertices[i] == Prototype.Mesh.Vertices[j]) {
							for (int k = 0; k < f; k++) {
								for (int h = 0; h < Prototype.Mesh.Faces[k].Vertices.Length; h++) {
									if (Prototype.Mesh.Faces[k].Vertices[h].Index == j) {
										Prototype.Mesh.Faces[k].Vertices[h].Index = (ushort)i;
									} else if (Prototype.Mesh.Faces[k].Vertices[h].Index > j) {
										Prototype.Mesh.Faces[k].Vertices[h].Index--;
									}
								}
							}
							for (int k = j; k < v - 1; k++) {
								Prototype.Mesh.Vertices[k] = Prototype.Mesh.Vertices[k + 1];
							}
							v--;
							j--;
						}
					}
				}
			}
			// eliminate unused materials
			bool[] materialUsed = new bool[m];
			for (int i = 0; i < f; i++) {
				materialUsed[Prototype.Mesh.Faces[i].Material] = true;
			}
			for (int i = 0; i < m; i++) {
				if (!materialUsed[i]) {
					for (int j = 0; j < f; j++) {
						if (Prototype.Mesh.Faces[j].Material > i) {
							Prototype.Mesh.Faces[j].Material--;
						}
					}
					for (int j = i; j < m - 1; j++) {
						Prototype.Mesh.Materials[j] = Prototype.Mesh.Materials[j + 1];
						materialUsed[j] = materialUsed[j + 1];
					}
					m--;
					i--;
				}
			}
			// eliminate duplicate materials
			for (int i = 0; i < m - 1; i++) {
				for (int j = i + 1; j < m; j++) {
					if (Prototype.Mesh.Materials[i] == Prototype.Mesh.Materials[j]) {
						for (int k = 0; k < f; k++) {
							if (Prototype.Mesh.Faces[k].Material == j) {
								Prototype.Mesh.Faces[k].Material = (ushort)i;
							} else if (Prototype.Mesh.Faces[k].Material > j) {
								Prototype.Mesh.Faces[k].Material--;
							}
						}
						for (int k = j; k < m - 1; k++) {
							Prototype.Mesh.Materials[k] = Prototype.Mesh.Materials[k + 1];
						}
						m--;
						j--;
					}
				}
			}
			// structure optimization
			if (!PreserveVertices & f < Interface.CurrentOptions.ObjectOptimizationFullThreshold) {
				// create TRIANGLES and QUADS from POLYGON
				for (int i = 0; i < f; i++) {
					int type = Prototype.Mesh.Faces[i].Flags & World.MeshFace.FaceTypeMask;
					if (type == World.MeshFace.FaceTypePolygon) {
						if (Prototype.Mesh.Faces[i].Vertices.Length == 3) {
							unchecked {
								Prototype.Mesh.Faces[i].Flags &= (byte)~World.MeshFace.FaceTypeMask;
								Prototype.Mesh.Faces[i].Flags |= World.MeshFace.FaceTypeTriangles;
							}
						} else if (Prototype.Mesh.Faces[i].Vertices.Length == 4) {
							unchecked {
								Prototype.Mesh.Faces[i].Flags &= (byte)~World.MeshFace.FaceTypeMask;
								Prototype.Mesh.Faces[i].Flags |= World.MeshFace.FaceTypeQuads;
							}
						}
					}
				}
				// decomposit TRIANGLES and QUADS
				for (int i = 0; i < f; i++) {
					int type = Prototype.Mesh.Faces[i].Flags & World.MeshFace.FaceTypeMask;
					if (type == World.MeshFace.FaceTypeTriangles) {
						if (Prototype.Mesh.Faces[i].Vertices.Length > 3) {
							int n = (Prototype.Mesh.Faces[i].Vertices.Length - 3) / 3;
							while (f + n > Prototype.Mesh.Faces.Length) {
								Array.Resize<World.MeshFace>(ref Prototype.Mesh.Faces, Prototype.Mesh.Faces.Length << 1);
							}
							for (int j = 0; j < n; j++) {
								Prototype.Mesh.Faces[f + j].Vertices = new World.MeshFaceVertex[3];
								for (int k = 0; k < 3; k++) {
									Prototype.Mesh.Faces[f + j].Vertices[k] = Prototype.Mesh.Faces[i].Vertices[3 + 3 * j + k];
								}
								Prototype.Mesh.Faces[f + j].Material = Prototype.Mesh.Faces[i].Material;
								Prototype.Mesh.Faces[f + j].Flags = Prototype.Mesh.Faces[i].Flags;
								unchecked {
									Prototype.Mesh.Faces[i].Flags &= (byte)~World.MeshFace.FaceTypeMask;
									Prototype.Mesh.Faces[i].Flags |= World.MeshFace.FaceTypeTriangles;
								}
							}
							Array.Resize<World.MeshFaceVertex>(ref Prototype.Mesh.Faces[i].Vertices, 3);
							f += n;
						}
					} else if (type == World.MeshFace.FaceTypeQuads) {
						if (Prototype.Mesh.Faces[i].Vertices.Length > 4) {
							int n = (Prototype.Mesh.Faces[i].Vertices.Length - 4) >> 2;
							while (f + n > Prototype.Mesh.Faces.Length) {
								Array.Resize<World.MeshFace>(ref Prototype.Mesh.Faces, Prototype.Mesh.Faces.Length << 1);
							}
							for (int j = 0; j < n; j++) {
								Prototype.Mesh.Faces[f + j].Vertices = new World.MeshFaceVertex[4];
								for (int k = 0; k < 4; k++) {
									Prototype.Mesh.Faces[f + j].Vertices[k] = Prototype.Mesh.Faces[i].Vertices[4 + 4 * j + k];
								}
								Prototype.Mesh.Faces[f + j].Material = Prototype.Mesh.Faces[i].Material;
								Prototype.Mesh.Faces[f + j].Flags = Prototype.Mesh.Faces[i].Flags;
								unchecked {
									Prototype.Mesh.Faces[i].Flags &= (byte)~World.MeshFace.FaceTypeMask;
									Prototype.Mesh.Faces[i].Flags |= World.MeshFace.FaceTypeQuads;
								}
							}
							Array.Resize<World.MeshFaceVertex>(ref Prototype.Mesh.Faces[i].Vertices, 4);
							f += n;
						}
					}
				}
				// optimize for TRIANGLE_STRIP
				int index = -1;
				while (true) {
					// add TRIANGLES to TRIANGLE_STRIP
					for (int i = 0; i < f; i++) {
						if (index == i | index == -1) {
							int type = Prototype.Mesh.Faces[i].Flags & World.MeshFace.FaceTypeMask;
							if (type == World.MeshFace.FaceTypeTriangleStrip) {
								int face = Prototype.Mesh.Faces[i].Flags & World.MeshFace.Face2Mask;
								for (int j = 0; j < f; j++) {
									int type2 = Prototype.Mesh.Faces[j].Flags & World.MeshFace.FaceTypeMask;
									int face2 = Prototype.Mesh.Faces[j].Flags & World.MeshFace.Face2Mask;
									if (type2 == World.MeshFace.FaceTypeTriangles & face == face2) {
										if (Prototype.Mesh.Faces[i].Material == Prototype.Mesh.Faces[j].Material) {
											bool keep = true;
											for (int k = 0; k < 3; k++) {
												int l = (k + 1) % 3;
												int n = Prototype.Mesh.Faces[i].Vertices.Length;
												if (Prototype.Mesh.Faces[i].Vertices[0] == Prototype.Mesh.Faces[j].Vertices[k] & Prototype.Mesh.Faces[i].Vertices[1] == Prototype.Mesh.Faces[j].Vertices[l]) {
													Array.Resize<World.MeshFaceVertex>(ref Prototype.Mesh.Faces[i].Vertices, n + 1);
													for (int h = n; h >= 1; h--) {
														Prototype.Mesh.Faces[i].Vertices[h] = Prototype.Mesh.Faces[i].Vertices[h - 1];
													}
													Prototype.Mesh.Faces[i].Vertices[0] = Prototype.Mesh.Faces[j].Vertices[(k + 2) % 3];
													keep = false;
												} else if (Prototype.Mesh.Faces[i].Vertices[n - 1] == Prototype.Mesh.Faces[j].Vertices[l] & Prototype.Mesh.Faces[i].Vertices[n - 2] == Prototype.Mesh.Faces[j].Vertices[k]) {
													Array.Resize<World.MeshFaceVertex>(ref Prototype.Mesh.Faces[i].Vertices, n + 1);
													Prototype.Mesh.Faces[i].Vertices[n] = Prototype.Mesh.Faces[j].Vertices[(k + 2) % 3];
													keep = false;
												}
												if (!keep) {
													break;
												}
											}
											if (!keep) {
												for (int k = j; k < f - 1; k++) {
													Prototype.Mesh.Faces[k] = Prototype.Mesh.Faces[k + 1];
												}
												if (j < i) {
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
					for (int i = 0; i < f - 1; i++) {
						int type = Prototype.Mesh.Faces[i].Flags & World.MeshFace.FaceTypeMask;
						if (type == World.MeshFace.FaceTypeTriangles) {
							int face = Prototype.Mesh.Faces[i].Flags & World.MeshFace.Face2Mask;
							for (int j = i + 1; j < f; j++) {
								int type2 = Prototype.Mesh.Faces[j].Flags & World.MeshFace.FaceTypeMask;
								int face2 = Prototype.Mesh.Faces[j].Flags & World.MeshFace.Face2Mask;
								if (type2 == World.MeshFace.FaceTypeTriangles & face == face2) {
									if (Prototype.Mesh.Faces[i].Material == Prototype.Mesh.Faces[j].Material) {
										for (int ik = 0; ik < 3; ik++) {
											int il = (ik + 1) % 3;
											for (int jk = 0; jk < 3; jk++) {
												int jl = (jk + 1) % 3;
												if (Prototype.Mesh.Faces[i].Vertices[ik] == Prototype.Mesh.Faces[j].Vertices[jl] & Prototype.Mesh.Faces[i].Vertices[il] == Prototype.Mesh.Faces[j].Vertices[jk]) {
													unchecked {
														Prototype.Mesh.Faces[i].Flags &= (byte)~World.MeshFace.FaceTypeMask;
														Prototype.Mesh.Faces[i].Flags |= World.MeshFace.FaceTypeTriangleStrip;
													}
													Prototype.Mesh.Faces[i].Vertices = new World.MeshFaceVertex[] {
														Prototype.Mesh.Faces[i].Vertices[(ik + 2) % 3],
														Prototype.Mesh.Faces[i].Vertices[ik],
														Prototype.Mesh.Faces[i].Vertices[il],
														Prototype.Mesh.Faces[j].Vertices[(jk + 2) % 3]
													};
													for (int k = j; k < f - 1; k++) {
														Prototype.Mesh.Faces[k] = Prototype.Mesh.Faces[k + 1];
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
				while (true) {
					// add QUADS to QUAD_STRIP
					for (int i = 0; i < f; i++) {
						if (index == i | index == -1) {
							int type = Prototype.Mesh.Faces[i].Flags & World.MeshFace.FaceTypeMask;
							if (type == World.MeshFace.FaceTypeQuadStrip) {
								int face = Prototype.Mesh.Faces[i].Flags & World.MeshFace.Face2Mask;
								for (int j = 0; j < f; j++) {
									int type2 = Prototype.Mesh.Faces[j].Flags & World.MeshFace.FaceTypeMask;
									int face2 = Prototype.Mesh.Faces[j].Flags & World.MeshFace.Face2Mask;
									if (type2 == World.MeshFace.FaceTypeQuads & face == face2) {
										if (Prototype.Mesh.Faces[i].Material == Prototype.Mesh.Faces[j].Material) {
											bool keep = true;
											for (int k = 0; k < 4; k++) {
												int l = (k + 1) & 3;
												int n = Prototype.Mesh.Faces[i].Vertices.Length;
												if (Prototype.Mesh.Faces[i].Vertices[0] == Prototype.Mesh.Faces[j].Vertices[l] & Prototype.Mesh.Faces[i].Vertices[1] == Prototype.Mesh.Faces[j].Vertices[k]) {
													Array.Resize<World.MeshFaceVertex>(ref Prototype.Mesh.Faces[i].Vertices, n + 2);
													for (int h = n + 1; h >= 2; h--) {
														Prototype.Mesh.Faces[i].Vertices[h] = Prototype.Mesh.Faces[i].Vertices[h - 2];
													}
													Prototype.Mesh.Faces[i].Vertices[0] = Prototype.Mesh.Faces[j].Vertices[(k + 2) & 3];
													Prototype.Mesh.Faces[i].Vertices[1] = Prototype.Mesh.Faces[j].Vertices[(k + 3) & 3];
													keep = false;
												} else if (Prototype.Mesh.Faces[i].Vertices[n - 1] == Prototype.Mesh.Faces[j].Vertices[l] & Prototype.Mesh.Faces[i].Vertices[n - 2] == Prototype.Mesh.Faces[j].Vertices[k]) {
													Array.Resize<World.MeshFaceVertex>(ref Prototype.Mesh.Faces[i].Vertices, n + 2);
													Prototype.Mesh.Faces[i].Vertices[n] = Prototype.Mesh.Faces[j].Vertices[(k + 3) & 3];
													Prototype.Mesh.Faces[i].Vertices[n + 1] = Prototype.Mesh.Faces[j].Vertices[(k + 2) & 3];
													keep = false;
												}
												if (!keep) {
													break;
												}
											}
											if (!keep) {
												for (int k = j; k < f - 1; k++) {
													Prototype.Mesh.Faces[k] = Prototype.Mesh.Faces[k + 1];
												}
												if (j < i) {
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
					for (int i = 0; i < f - 1; i++) {
						int type = Prototype.Mesh.Faces[i].Flags & World.MeshFace.FaceTypeMask;
						if (type == World.MeshFace.FaceTypeQuads) {
							int face = Prototype.Mesh.Faces[i].Flags & World.MeshFace.Face2Mask;
							for (int j = i + 1; j < f; j++) {
								int type2 = Prototype.Mesh.Faces[j].Flags & World.MeshFace.FaceTypeMask;
								int face2 = Prototype.Mesh.Faces[j].Flags & World.MeshFace.Face2Mask;
								if (type2 == World.MeshFace.FaceTypeQuads & face == face2) {
									if (Prototype.Mesh.Faces[i].Material == Prototype.Mesh.Faces[j].Material) {
										for (int ik = 0; ik < 4; ik++) {
											int il = (ik + 1) & 3;
											for (int jk = 0; jk < 4; jk++) {
												int jl = (jk + 1) & 3;
												if (Prototype.Mesh.Faces[i].Vertices[ik] == Prototype.Mesh.Faces[j].Vertices[jl] & Prototype.Mesh.Faces[i].Vertices[il] == Prototype.Mesh.Faces[j].Vertices[jk]) {
													unchecked {
														Prototype.Mesh.Faces[i].Flags &= (byte)~World.MeshFace.FaceTypeMask;
														Prototype.Mesh.Faces[i].Flags |= World.MeshFace.FaceTypeQuadStrip;
													}
													Prototype.Mesh.Faces[i].Vertices = new World.MeshFaceVertex[] {
														Prototype.Mesh.Faces[i].Vertices[(ik + 2) & 3],
														Prototype.Mesh.Faces[i].Vertices[(ik + 3) & 3],
														Prototype.Mesh.Faces[i].Vertices[il],
														Prototype.Mesh.Faces[i].Vertices[ik],
														Prototype.Mesh.Faces[j].Vertices[(jk + 3) & 3],
														Prototype.Mesh.Faces[j].Vertices[(jk + 2) & 3]
													};
													for (int k = j; k < f - 1; k++) {
														Prototype.Mesh.Faces[k] = Prototype.Mesh.Faces[k + 1];
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
				for (int i = 0; i < f - 1; i++) {
					int type = Prototype.Mesh.Faces[i].Flags & World.MeshFace.FaceTypeMask;
					if (type == World.MeshFace.FaceTypeTriangles | type == World.MeshFace.FaceTypeQuads) {
						int face = Prototype.Mesh.Faces[i].Flags & World.MeshFace.Face2Mask;
						for (int j = i + 1; j < f; j++) {
							int type2 = Prototype.Mesh.Faces[j].Flags & World.MeshFace.FaceTypeMask;
							int face2 = Prototype.Mesh.Faces[j].Flags & World.MeshFace.Face2Mask;
							if (type == type2 & face == face2) {
								if (Prototype.Mesh.Faces[i].Material == Prototype.Mesh.Faces[j].Material) {
									int n = Prototype.Mesh.Faces[i].Vertices.Length;
									Array.Resize<World.MeshFaceVertex>(ref Prototype.Mesh.Faces[i].Vertices, n + Prototype.Mesh.Faces[j].Vertices.Length);
									for (int k = 0; k < Prototype.Mesh.Faces[j].Vertices.Length; k++) {
										Prototype.Mesh.Faces[i].Vertices[n + k] = Prototype.Mesh.Faces[j].Vertices[k];
									}
									for (int k = j; k < f - 1; k++) {
										Prototype.Mesh.Faces[k] = Prototype.Mesh.Faces[k + 1];
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
			if (v != Prototype.Mesh.Vertices.Length) {
				Array.Resize<World.Vertex>(ref Prototype.Mesh.Vertices, v);
			}
			if (m != Prototype.Mesh.Materials.Length) {
				Array.Resize<World.MeshMaterial>(ref Prototype.Mesh.Materials, m);
			}
			if (f != Prototype.Mesh.Faces.Length) {
				Array.Resize<World.MeshFace>(ref Prototype.Mesh.Faces, f);
			}
		}

		// join objects
		internal static void JoinObjects(ref StaticObject Base, StaticObject Add) {
			if (Base == null & Add == null) {
				return;
			} else if (Base == null) {
				Base = CloneObject(Add);
			} else if (Add != null) {
				int mf = Base.Mesh.Faces.Length;
				int mm = Base.Mesh.Materials.Length;
				int mv = Base.Mesh.Vertices.Length;
				Array.Resize<World.MeshFace>(ref Base.Mesh.Faces, mf + Add.Mesh.Faces.Length);
				Array.Resize<World.MeshMaterial>(ref Base.Mesh.Materials, mm + Add.Mesh.Materials.Length);
				Array.Resize<World.Vertex>(ref Base.Mesh.Vertices, mv + Add.Mesh.Vertices.Length);
				for (int i = 0; i < Add.Mesh.Faces.Length; i++) {
					Base.Mesh.Faces[mf + i] = Add.Mesh.Faces[i];
					for (int j = 0; j < Base.Mesh.Faces[mf + i].Vertices.Length; j++) {
						Base.Mesh.Faces[mf + i].Vertices[j].Index += (ushort)mv;
					}
					Base.Mesh.Faces[mf + i].Material += (ushort)mm;
				}
				for (int i = 0; i < Add.Mesh.Materials.Length; i++) {
					Base.Mesh.Materials[mm + i] = Add.Mesh.Materials[i];
				}
				for (int i = 0; i < Add.Mesh.Vertices.Length; i++) {
					Base.Mesh.Vertices[mv + i] = Add.Mesh.Vertices[i];
				}
			}
		}

		// create object
		internal static void CreateObject(UnifiedObject Prototype, World.Vector3D Position, World.Transformation BaseTransformation, World.Transformation AuxTransformation, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition) {
			CreateObject(Prototype, Position, BaseTransformation, AuxTransformation, -1, AccurateObjectDisposal, StartingDistance, EndingDistance, BlockLength, TrackPosition, 1.0, false);
		}
		internal static void CreateObject(UnifiedObject Prototype, World.Vector3D Position, World.Transformation BaseTransformation, World.Transformation AuxTransformation, int SectionIndex, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness, bool DuplicateMaterials) {
			if (Prototype is StaticObject) {
				StaticObject s = (StaticObject)Prototype;
				CreateStaticObject(s, Position, BaseTransformation, AuxTransformation, AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness, DuplicateMaterials);
			} else if (Prototype is AnimatedObjectCollection) {
				AnimatedObjectCollection a = (AnimatedObjectCollection)Prototype;
				CreateAnimatedWorldObjects(a.Objects, Position, BaseTransformation, AuxTransformation, SectionIndex, AccurateObjectDisposal, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness, DuplicateMaterials);
			}
		}

		// create static object
		internal static int CreateStaticObject(StaticObject Prototype, World.Vector3D Position, World.Transformation BaseTransformation, World.Transformation AuxTransformation, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition) {
			return CreateStaticObject(Prototype, Position, BaseTransformation, AuxTransformation, AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, BlockLength, TrackPosition, 1.0, false);
		}
		internal static int CreateStaticObject(StaticObject Prototype, World.Vector3D Position, World.Transformation BaseTransformation, World.Transformation AuxTransformation, bool AccurateObjectDisposal, double AccurateObjectDisposalZOffset, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness, bool DuplicateMaterials) {
			int a = ObjectsUsed;
			if (a >= Objects.Length) {
				Array.Resize<StaticObject>(ref Objects, Objects.Length << 1);
			}
			ApplyStaticObjectData(ref Objects[a], Prototype, Position, BaseTransformation, AuxTransformation, AccurateObjectDisposal, AccurateObjectDisposalZOffset, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness, DuplicateMaterials);
			for (int i = 0; i < Prototype.Mesh.Faces.Length; i++) {
				switch (Prototype.Mesh.Faces[i].Flags & World.MeshFace.FaceTypeMask) {
					case World.MeshFace.FaceTypeTriangles:
						Game.InfoTotalTriangles++;
						break;
					case World.MeshFace.FaceTypeTriangleStrip:
						Game.InfoTotalTriangleStrip++;
						break;
					case World.MeshFace.FaceTypeQuads:
						Game.InfoTotalQuads++;
						break;
					case World.MeshFace.FaceTypeQuadStrip:
						Game.InfoTotalQuadStrip++;
						break;
					case World.MeshFace.FaceTypePolygon:
						Game.InfoTotalPolygon++;
						break;
				}
			}
			ObjectsUsed++;
			return a;
		}
		internal static void ApplyStaticObjectData(ref StaticObject Object, StaticObject Prototype, World.Vector3D Position, World.Transformation BaseTransformation, World.Transformation AuxTransformation, bool AccurateObjectDisposal, double AccurateObjectDisposalZOffset, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness, bool DuplicateMaterials) {
			Object = new StaticObject();
			Object.StartingDistance = float.MaxValue;
			Object.EndingDistance = float.MinValue;
			bool brightnesschange = Brightness != 1.0;
			// vertices
			Object.Mesh.Vertices = new World.Vertex[Prototype.Mesh.Vertices.Length];
			for (int j = 0; j < Prototype.Mesh.Vertices.Length; j++) {
				Object.Mesh.Vertices[j] = Prototype.Mesh.Vertices[j];
				if (AccurateObjectDisposal) {
					World.Rotate(ref Object.Mesh.Vertices[j].Coordinates.X, ref Object.Mesh.Vertices[j].Coordinates.Y, ref Object.Mesh.Vertices[j].Coordinates.Z, AuxTransformation);
					if (Object.Mesh.Vertices[j].Coordinates.Z < Object.StartingDistance) {
						Object.StartingDistance = (float)Object.Mesh.Vertices[j].Coordinates.Z;
					}
					if (Object.Mesh.Vertices[j].Coordinates.Z > Object.EndingDistance) {
						Object.EndingDistance = (float)Object.Mesh.Vertices[j].Coordinates.Z;
					}
					Object.Mesh.Vertices[j].Coordinates = Prototype.Mesh.Vertices[j].Coordinates;
				}
				World.Rotate(ref Object.Mesh.Vertices[j].Coordinates.X, ref Object.Mesh.Vertices[j].Coordinates.Y, ref Object.Mesh.Vertices[j].Coordinates.Z, AuxTransformation);
				World.Rotate(ref Object.Mesh.Vertices[j].Coordinates.X, ref Object.Mesh.Vertices[j].Coordinates.Y, ref Object.Mesh.Vertices[j].Coordinates.Z, BaseTransformation);
				Object.Mesh.Vertices[j].Coordinates.X += Position.X;
				Object.Mesh.Vertices[j].Coordinates.Y += Position.Y;
				Object.Mesh.Vertices[j].Coordinates.Z += Position.Z;
			}
			if (AccurateObjectDisposal) {
				Object.StartingDistance += (float)AccurateObjectDisposalZOffset;
				Object.EndingDistance += (float)AccurateObjectDisposalZOffset;
			}
			// faces
			Object.Mesh.Faces = new World.MeshFace[Prototype.Mesh.Faces.Length];
			for (int j = 0; j < Prototype.Mesh.Faces.Length; j++) {
				Object.Mesh.Faces[j].Flags = Prototype.Mesh.Faces[j].Flags;
				Object.Mesh.Faces[j].Material = Prototype.Mesh.Faces[j].Material;
				Object.Mesh.Faces[j].Vertices = new World.MeshFaceVertex[Prototype.Mesh.Faces[j].Vertices.Length];
				for (int k = 0; k < Prototype.Mesh.Faces[j].Vertices.Length; k++) {
					Object.Mesh.Faces[j].Vertices[k] = Prototype.Mesh.Faces[j].Vertices[k];
					double nx = Object.Mesh.Faces[j].Vertices[k].Normal.X;
					double ny = Object.Mesh.Faces[j].Vertices[k].Normal.Y;
					double nz = Object.Mesh.Faces[j].Vertices[k].Normal.Z;
					if (nx * nx + ny * ny + nz * nz != 0.0) {
						World.Rotate(ref Object.Mesh.Faces[j].Vertices[k].Normal.X, ref Object.Mesh.Faces[j].Vertices[k].Normal.Y, ref Object.Mesh.Faces[j].Vertices[k].Normal.Z, AuxTransformation);
						World.Rotate(ref Object.Mesh.Faces[j].Vertices[k].Normal.X, ref Object.Mesh.Faces[j].Vertices[k].Normal.Y, ref Object.Mesh.Faces[j].Vertices[k].Normal.Z, BaseTransformation);
					}
				}
			}
			// materials
			Object.Mesh.Materials = new World.MeshMaterial[Prototype.Mesh.Materials.Length];
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
			Objects[a].Mesh.Faces = new World.MeshFace[] { };
			Objects[a].Mesh.Materials = new World.MeshMaterial[] { };
			Objects[a].Mesh.Vertices = new World.Vertex[] { };
			Objects[a].Dynamic = true;
			ObjectsUsed++;
			return a;
		}

		// clone object
		internal static StaticObject CloneObject(StaticObject Prototype) {
			if (Prototype == null) return null;
			return CloneObject(Prototype, -1, -1);
		}
		internal static StaticObject CloneObject(StaticObject Prototype, int DaytimeTextureIndex, int NighttimeTextureIndex) {
			if (Prototype == null) return null;
			StaticObject Result = new StaticObject();
			Result.StartingDistance = Prototype.StartingDistance;
			Result.EndingDistance = Prototype.EndingDistance;
			Result.Dynamic = Prototype.Dynamic;
			// vertices
			Result.Mesh.Vertices = new World.Vertex[Prototype.Mesh.Vertices.Length];
			for (int j = 0; j < Prototype.Mesh.Vertices.Length; j++) {
				Result.Mesh.Vertices[j] = Prototype.Mesh.Vertices[j];
			}
			// faces
			Result.Mesh.Faces = new World.MeshFace[Prototype.Mesh.Faces.Length];
			for (int j = 0; j < Prototype.Mesh.Faces.Length; j++) {
				Result.Mesh.Faces[j].Flags = Prototype.Mesh.Faces[j].Flags;
				Result.Mesh.Faces[j].Material = Prototype.Mesh.Faces[j].Material;
				Result.Mesh.Faces[j].Vertices = new World.MeshFaceVertex[Prototype.Mesh.Faces[j].Vertices.Length];
				for (int k = 0; k < Prototype.Mesh.Faces[j].Vertices.Length; k++) {
					Result.Mesh.Faces[j].Vertices[k] = Prototype.Mesh.Faces[j].Vertices[k];
				}
			}
			// materials
			Result.Mesh.Materials = new World.MeshMaterial[Prototype.Mesh.Materials.Length];
			for (int j = 0; j < Prototype.Mesh.Materials.Length; j++) {
				Result.Mesh.Materials[j] = Prototype.Mesh.Materials[j];
				if (DaytimeTextureIndex >= 0) {
					Result.Mesh.Materials[j].DaytimeTextureIndex = DaytimeTextureIndex;
				}
				if (NighttimeTextureIndex >= 0) {
					Result.Mesh.Materials[j].NighttimeTextureIndex = NighttimeTextureIndex;
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
						Renderer.ShowObject(i, Renderer.ObjectType.Static);
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
			int m = ObjectsSortedByEnd.Length;
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
							Renderer.ShowObject(o, Renderer.ObjectType.Static);
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
							Renderer.ShowObject(o, Renderer.ObjectType.Static);
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