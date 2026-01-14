using System;
using System.Linq;
using OpenBveApi.Colors;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Graphics;
using OpenBveApi.Hosts;
using OpenBveApi.Math;
using OpenBveApi.Textures;
using OpenBveApi.Trains;
using OpenBveApi.World;

#pragma warning disable CS0659, CS0661 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
namespace OpenBveApi.Objects
{
	/// <summary>The base type for an animated object</summary>
	public class AnimatedObject
	{
		/// <summary>The filename</summary>
		private readonly string FileName;
		/// <summary>The array of states</summary>
		public ObjectState[] States;
		/// <summary>The function script controlling state changes</summary>
		public AnimationScript StateFunction;
		/// <summary>The index of the current state</summary>
		public int CurrentState;
		/// <summary>The function script controlling scaling in the X direction</summary>
		public AnimationScript ScaleXFunction;
		/// <summary>The function script controlling scaling in the Y direction</summary>
		public AnimationScript ScaleYFunction;
		/// <summary>The function script controlling scaling in the Z direction</summary>
		public AnimationScript ScaleZFunction;
		/// <summary>A 3D vector describing the direction taken when TranslateXFunction is called</summary>
		public Vector3 TranslateXDirection;
		/// <summary>A 3D vector describing the direction taken when TranslateXYFunction is called</summary>
		public Vector3 TranslateYDirection;
		/// <summary>A 3D vector describing the direction taken when TranslateZFunction is called</summary>
		public Vector3 TranslateZDirection;
		/// <summary>The function script controlling translation in the X direction</summary>
		/// <remarks>X is an arbitrary 3D direction, nominally 1,0,0</remarks>
		public AnimationScript TranslateXFunction;
		/// <summary>The function script controlling translation in the Y direction</summary>
		/// <remarks>Y is an arbitrary 3D direction, nominally 0,1,0</remarks>
		public AnimationScript TranslateYFunction;
		/// <summary>The function script controlling translation in the Z direction</summary>
		/// <remarks>Z is an arbitrary 3D direction, nominally 0,0,1</remarks>
		public AnimationScript TranslateZFunction;
		/// <summary>A 3D vector describing the rotation performed when RotateXFunction is called</summary>
		public Vector3 RotateXDirection;
		/// <summary>A 3D vector describing the rotation performed when RotateYFunction is called</summary>
		public Vector3 RotateYDirection;
		/// <summary>A 3D vector describing the rotation performed when RotateZFunction is called</summary>
		public Vector3 RotateZDirection;
		/// <summary>The function script controlling translation in the X direction</summary>
		/// <remarks>X is an arbitrary 3D direction, nominally 1,0,0</remarks>
		public AnimationScript RotateXFunction;
		/// <summary>The function script controlling translation in the Y direction</summary>
		/// <remarks>Y is an arbitrary 3D direction, nominally 0,1,0</remarks>
		public AnimationScript RotateYFunction;
		/// <summary>The function script controlling translation in the Z direction</summary>
		/// <remarks>Z is an arbitrary 3D direction, nominally 0,0,1</remarks>
		public AnimationScript RotateZFunction;
		/// <summary>The damping (if any) to perform when RotateXFunction is called</summary>
		public Damping RotateXDamping;
		/// <summary>The damping (if any) to perform when RotateYFunction is called</summary>
		public Damping RotateYDamping;
		/// <summary>The damping (if any) to perform when RotateZFunction is called</summary>
		public Damping RotateZDamping;
		/// <summary>A 2D vector describing the texture shifting performed when TextureShiftXFunction is called</summary>
		public Vector2 TextureShiftXDirection;
		/// <summary>A 2D vector describing the texture shifting performed when TextureShiftYFunction is called</summary>
		public Vector2 TextureShiftYDirection;
		/// <summary>The function script controlling texture shifting in the X direction</summary>
		/// <remarks>X is an arbitrary 2D direction, nominally 1,0</remarks>
		public AnimationScript TextureShiftXFunction;
		/// <summary>The function script controlling texture shifting in the Y direction</summary>
		/// <remarks>X is an arbitrary 2D direction, nominally 0,1</remarks>
		public AnimationScript TextureShiftYFunction;
		/// <summary>If the LED function is used, this controls whether the winding is clockwise or anti-clockwise</summary>
		public bool LEDClockwiseWinding;
		/// <summary>The initial angle of the LED function</summary>
		public double LEDInitialAngle;
		/// <summary>The final angle of the LED function</summary>
		public double LEDLastAngle;
		/// <summary>If LEDFunction is used, an array of five vectors representing the bottom-left, up-left, up-right, bottom-right and center coordinates of the LED square, or a null reference otherwise.</summary>
		public Vector3[] LEDVectors;
		/// <summary>The function script controlling the LED square</summary>
		public AnimationScript LEDFunction;
		/// <summary>The refresh rate in seconds</summary>
		public double RefreshRate;
		/// <summary>The time since the last update of this object</summary>
		public double SecondsSinceLastUpdate;
		
		/// <summary>The function script controlling movement along the track</summary>
		public AnimationScript TrackFollowerFunction;
		/// <summary>The front axle position if TrackFollowerFunction is used</summary>
		public double FrontAxlePosition = 1;
		/// <summary>The rear axle position if TrackFollowerFunction is used</summary>
		public double RearAxlePosition = -1;
		/// <summary>Holds a reference to the internal static object used for display</summary>
		/// <remarks>This is a fully transformed deep copy of the current state</remarks>
		public ObjectState internalObject;
		/// <summary>Holds a reference to the host interface of the current application</summary>
		private readonly HostInterface currentHost;
		/// <summary>Whether this object uses the Timetable texture</summary>
		public bool isTimeTableObject;
		/// <summary>Sets whether the openGL VAO should be updated by the renderer this frame</summary>
		public bool UpdateVAO;
		/// <summary>Whether the object forms part of a train</summary>
		public bool IsPartOfTrain;
		/// <summary>The signalling section the object refers to</summary>
		/// <remarks>This only applies to objects placed using the Track.Sig command</remarks>
		public int SectionIndex;
		/// <summary>Function used to change the colors of an object</summary>
		public AnimationScript ColorFunction;
		/// <summary>Array of colors for ColorFunction</summary>
		public Color24[] Colors;

		/// <summary>Creates a new animated object</summary>
		public AnimatedObject(HostInterface host, string fileName = "")
		{
			currentHost = host;
			States = new ObjectState[] { };
			FileName = fileName;
		}

		/// <summary>Creates a new animated object</summary>
		public AnimatedObject(HostInterface host, StaticObject staticObject)
		{
			currentHost = host;
			States = new [] { new ObjectState() };
			States[0].Prototype = staticObject;
			internalObject = new ObjectState(staticObject);
			CurrentState = 0;
		}

		/// <summary>Clones this object</summary>
		/// <returns>The new object</returns>
		public AnimatedObject Clone()
		{
			AnimatedObject Result = new AnimatedObject(currentHost, FileName)
			{
				States = States.Select(x => (ObjectState)x.Clone()).ToArray(),
				TrackFollowerFunction = TrackFollowerFunction?.Clone(),
				FrontAxlePosition = this.FrontAxlePosition,
				RearAxlePosition = this.RearAxlePosition,
				StateFunction = StateFunction?.Clone(),
				CurrentState = this.CurrentState,
				TranslateZDirection = this.TranslateZDirection,
				TranslateYDirection = this.TranslateYDirection,
				TranslateXDirection = this.TranslateXDirection,
				TranslateXFunction = TranslateXFunction?.Clone(),
				TranslateYFunction = TranslateYFunction?.Clone(),
				TranslateZFunction = TranslateZFunction?.Clone(),
				RotateXDirection = this.RotateXDirection,
				RotateYDirection = this.RotateYDirection,
				RotateZDirection = this.RotateZDirection,
				RotateXFunction = RotateXFunction?.Clone(),
				RotateXDamping = RotateXDamping?.Clone(),
				RotateYFunction = RotateYFunction?.Clone(),
				RotateYDamping = RotateYDamping?.Clone(),
				RotateZFunction = RotateZFunction?.Clone(),
				RotateZDamping = RotateZDamping?.Clone(),
				ScaleXFunction = ScaleXFunction?.Clone(),
				ScaleYFunction = ScaleYFunction?.Clone(),
				ScaleZFunction = ScaleZFunction?.Clone(),
				TextureShiftXDirection = this.TextureShiftXDirection,
				TextureShiftYDirection = this.TextureShiftYDirection,
				TextureShiftXFunction = TextureShiftXFunction?.Clone(),
				TextureShiftYFunction = TextureShiftYFunction?.Clone(),
				LEDClockwiseWinding = this.LEDClockwiseWinding,
				LEDInitialAngle = this.LEDInitialAngle,
				LEDLastAngle = this.LEDLastAngle,
				SectionIndex = this.SectionIndex,
				IsPartOfTrain = false // will be set by the CarSection load if appropriate
			};

			if (this.LEDVectors != null)
			{
				Result.LEDVectors = new Vector3[this.LEDVectors.Length];
				for (int i = 0; i < this.LEDVectors.Length; i++)
				{
					Result.LEDVectors[i] = this.LEDVectors[i];
				}
			}
			else
			{
				Result.LEDVectors = null;
			}

			Result.LEDFunction = LEDFunction?.Clone();
			Result.RefreshRate = this.RefreshRate;
			Result.SecondsSinceLastUpdate = 0.0;
			if (isTimeTableObject)
			{
				Result.isTimeTableObject = true;
				currentHost.AddObjectForCustomTimeTable(this);

			}
			return Result;
		}

		/// <summary>Checks whether this object contains any functions</summary>
		public bool IsFreeOfFunctions()
		{
			if (this.StateFunction != null) return false;
			if (this.TrackFollowerFunction != null) return false;
			if (this.ScaleXFunction != null | this.ScaleYFunction != null | this.ScaleZFunction != null) return false;
			if (this.TranslateXFunction != null | this.TranslateYFunction != null | this.TranslateZFunction != null) return false;
			if (this.RotateXFunction != null | this.RotateYFunction != null | this.RotateZFunction != null) return false;
			if (this.TextureShiftXFunction != null | this.TextureShiftYFunction != null) return false;
			if (this.LEDFunction != null) return false;
			if (this.TrackFollowerFunction != null) return false;
			return true;
		}

		/// <summary>Initialises the object</summary>
		/// <param name="StateIndex">The state to show</param>
		/// <param name="Type">The object type</param>
		/// <param name="Show">Whether the object should be shown immediately on initialisation</param>
		public void Initialize(int StateIndex, ObjectType Type, bool Show)
		{
			currentHost.HideObject(internalObject);
			int t = StateIndex;
			if (t >= 0)
			{
				internalObject.Prototype = States[t].Prototype;
			}
			
			CurrentState = StateIndex;
			if (Show && StateIndex != -1)
			{
				currentHost.ShowObject(internalObject, Type);
			}
		}

		/// <summary> Updates the position and state of the animated object</summary>
		/// <param name="Train">The train, or a null reference otherwise</param>
		/// <param name="CarIndex">If this object forms part of a train, the car index it refers to</param>
		/// <param name="TrackPosition"></param>
		/// <param name="Position"></param>
		/// <param name="Direction"></param>
		/// <param name="Up"></param>
		/// <param name="Side"></param>
		/// <param name="UpdateFunctions">Whether the functions associated with this object should be re-evaluated</param>
		/// <param name="Show"></param>
		/// <param name="TimeElapsed">The time elapsed since this object was last updated</param>
		/// <param name="EnableDamping">Whether damping is to be applied for this call</param>
		/// <param name="IsTouch">Whether Animated Object belonging to TouchElement class.</param>
		/// <param name="Camera"></param>
		public void Update(AbstractTrain Train, int CarIndex, double TrackPosition, Vector3 Position, Vector3 Direction, Vector3 Up, Vector3 Side, bool UpdateFunctions, bool Show, double TimeElapsed, bool EnableDamping, bool IsTouch = false, dynamic Camera = null)
		{
			// state change
			if (StateFunction != null & UpdateFunctions)
			{
				double sd = StateFunction.ExecuteScript(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
				int si = (int) System.Math.Round(sd);
				if (si < 0 | si >= States.Length) si = -1;
				if (CurrentState != si)
				{
					ObjectType type = ObjectType.Dynamic;
					if (Camera != null)
					{
						type = ObjectType.Overlay;
					}
					Initialize(si, type, Show);
					CurrentState = si;
				}
			}

			if (CurrentState == -1) return; //not visible state, so don't bother updating
			// translation
			if (TranslateXFunction != null)
			{
				double x = TranslateXFunction.LastResult;
				if (UpdateFunctions)
				{
					x = TranslateXFunction.ExecuteScript(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
				}

				Vector3 translationVector = new Vector3(TranslateXDirection); //Must clone
				translationVector.Rotate(Direction, Up, Side);
				translationVector *= x;
				Position += translationVector;
			}


			if (TranslateYFunction != null)
			{
				double y = TranslateYFunction.LastResult;
				if (UpdateFunctions)
				{
					y = TranslateYFunction.ExecuteScript(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
				}

				Vector3 translationVector = new Vector3(TranslateYDirection); //Must clone
				translationVector.Rotate(Direction, Up, Side);
				translationVector *= y;
				Position += translationVector;
			}

			if (TranslateZFunction != null)
			{
				double z = TranslateZFunction.LastResult;
				if (UpdateFunctions)
				{
					z = TranslateZFunction.ExecuteScript(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
				}
				
				Vector3 translationVector = new Vector3(TranslateZDirection); //Must clone
				translationVector.Rotate(Direction, Up, Side);
				translationVector *= z;
				Position += translationVector;
			}

			// rotation
			bool rotateX = RotateXFunction != null;
			bool rotateY = RotateYFunction != null;
			bool rotateZ = RotateZFunction != null;
			double radianX = 0.0;
			if (rotateX)
			{
				radianX = RotateXFunction.LastResult;
				if (UpdateFunctions)
				{
					radianX = RotateXFunction.ExecuteScript(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
				}

				RotateXDamping?.Update(TimeElapsed, ref radianX, EnableDamping);
			}

			double radianY = 0.0;
			if (rotateY)
			{
				radianY = RotateYFunction.LastResult;
				if (UpdateFunctions)
				{
					radianY = RotateYFunction.ExecuteScript(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
				}

				RotateYDamping?.Update(TimeElapsed, ref radianY, EnableDamping);
			}

			double radianZ = 0.0;
			if (rotateZ)
			{
				radianZ = RotateZFunction.LastResult;
				if (UpdateFunctions)
				{
					radianZ = RotateZFunction.ExecuteScript(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
				}

				RotateZDamping?.Update(TimeElapsed, ref radianZ, EnableDamping);
			}

			bool scaleX = ScaleXFunction != null;
			bool scaleY = ScaleYFunction != null;
			bool scaleZ = ScaleZFunction != null;
			Vector3 scale = Vector3.One;
			if (scaleX)
			{
				scale.X = ScaleXFunction.LastResult;
				if (UpdateFunctions)
				{
					scale.X = ScaleXFunction.ExecuteScript(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
				}
			}
			if (scaleY)
			{
				scale.Y = ScaleYFunction.LastResult;
				if (UpdateFunctions)
				{
					scale.Y = ScaleYFunction.ExecuteScript(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
				}
			}
			if (scaleZ)
			{
				scale.Z = ScaleZFunction.LastResult;
				if (UpdateFunctions)
				{
					scale.Z = ScaleZFunction.ExecuteScript(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
				}
			}
			// texture shift
			bool shiftx = TextureShiftXFunction != null;
			bool shifty = TextureShiftYFunction != null;
			internalObject.TextureTranslation = Matrix4D.Identity;

			if (shiftx | shifty)
			{
				if (shiftx)
				{
					double x = TextureShiftXFunction.LastResult;
					if (UpdateFunctions)
					{
						x = TextureShiftXFunction.ExecuteScript(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
					}
					
					x -= System.Math.Floor(x);
					internalObject.TextureTranslation *= Matrix4D.CreateTranslation(x * TextureShiftXDirection.X, x * TextureShiftXDirection.Y, 1.0);
				}

				if (shifty)
				{
					double y = TextureShiftYFunction.LastResult;
					if (UpdateFunctions)
					{
						y = TextureShiftYFunction.ExecuteScript(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
					}
					
					y -= System.Math.Floor(y);
					internalObject.TextureTranslation *= Matrix4D.CreateTranslation(y * TextureShiftYDirection.X, y * TextureShiftYDirection.Y, 1.0);
				}
			}

			// led
			bool led = LEDFunction != null;
			double ledangle = 0.0;
			if (led)
			{
				ledangle = LEDFunction.LastResult;
				if (UpdateFunctions)
				{
					ledangle = LEDFunction.ExecuteScript(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
				}
			}

			// null object
			if (States[CurrentState].Prototype == null)
			{
				return;
			}

			// led
			if (led)
			{
				/*
				 * Edges:         Vertices:
				 * 0 - bottom     0 - bottom-left
				 * 1 - left       1 - top-left
				 * 2 - top        2 - top-right
				 * 3 - right      3 - bottom-right
				 *                4 - center
				 * */
				int v = 1;
				if (LEDClockwiseWinding)
				{
					/* winding is clockwise*/
					if (ledangle < LEDInitialAngle)
					{
						ledangle = LEDInitialAngle;
					}

					if (ledangle < LEDLastAngle)
					{
						double currentEdgeFloat = System.Math.Floor(0.636619772367582 * (ledangle + 0.785398163397449));
						int currentEdge = ((int) currentEdgeFloat % 4 + 4) % 4;
						double lastEdgeFloat = System.Math.Floor(0.636619772367582 * (LEDLastAngle + 0.785398163397449));
						int lastEdge = ((int) lastEdgeFloat % 4 + 4) % 4;
						if (lastEdge < currentEdge | lastEdge == currentEdge & System.Math.Abs(currentEdgeFloat - lastEdgeFloat) > 2.0)
						{
							lastEdge += 4;
						}

						if (currentEdge == lastEdge)
						{
							/* current angle to last angle */
							{
								double t = 0.5 + 0.636619772367582 * ledangle - currentEdgeFloat;
								if (t < 0.0)
								{
									t = 0.0;
								}
								else if (t > 1.0)
								{
									t = 1.0;
								}

								t = 0.5 * (1.0 - System.Math.Tan(0.25 * (System.Math.PI - 2.0 * System.Math.PI * t)));
								double cx = (1.0 - t) * LEDVectors[(currentEdge + 3) % 4].X + t * LEDVectors[currentEdge].X;
								double cy = (1.0 - t) * LEDVectors[(currentEdge + 3) % 4].Y + t * LEDVectors[currentEdge].Y;
								double cz = (1.0 - t) * LEDVectors[(currentEdge + 3) % 4].Z + t * LEDVectors[currentEdge].Z;
								States[CurrentState].Prototype.Mesh.Vertices[v].Coordinates = new Vector3(cx, cy, cz);
								v++;
							}
							{
								double t = 0.5 + 0.636619772367582 * LEDLastAngle - lastEdgeFloat;
								if (t < 0.0)
								{
									t = 0.0;
								}
								else if (t > 1.0)
								{
									t = 1.0;
								}

								t = 0.5 * (1.0 - System.Math.Tan(0.25 * (System.Math.PI - 2.0 * System.Math.PI * t)));
								double lx = (1.0 - t) * LEDVectors[(lastEdge + 3) % 4].X + t * LEDVectors[lastEdge].X;
								double ly = (1.0 - t) * LEDVectors[(lastEdge + 3) % 4].Y + t * LEDVectors[lastEdge].Y;
								double lz = (1.0 - t) * LEDVectors[(lastEdge + 3) % 4].Z + t * LEDVectors[lastEdge].Z;
								States[CurrentState].Prototype.Mesh.Vertices[v].Coordinates = new Vector3(lx, ly, lz);
								v++;
							}
						}
						else
						{
							{
								/* current angle to square vertex */
								double t = 0.5 + 0.636619772367582 * ledangle - currentEdgeFloat;
								if (t < 0.0)
								{
									t = 0.0;
								}
								else if (t > 1.0)
								{
									t = 1.0;
								}

								t = 0.5 * (1.0 - System.Math.Tan(0.25 * (System.Math.PI - 2.0 * System.Math.PI * t)));
								double cx = (1.0 - t) * LEDVectors[(currentEdge + 3) % 4].X + t * LEDVectors[currentEdge].X;
								double cy = (1.0 - t) * LEDVectors[(currentEdge + 3) % 4].Y + t * LEDVectors[currentEdge].Y;
								double cz = (1.0 - t) * LEDVectors[(currentEdge + 3) % 4].Z + t * LEDVectors[currentEdge].Z;
								States[CurrentState].Prototype.Mesh.Vertices[v + 0].Coordinates = new Vector3(cx, cy, cz);
								States[CurrentState].Prototype.Mesh.Vertices[v + 1].Coordinates = LEDVectors[currentEdge];
								v += 2;
							}
							for (int j = currentEdge + 1; j < lastEdge; j++)
							{
								/* square-vertex to square-vertex */
								States[CurrentState].Prototype.Mesh.Vertices[v + 0].Coordinates = LEDVectors[(j + 3) % 4];
								States[CurrentState].Prototype.Mesh.Vertices[v + 1].Coordinates = LEDVectors[j % 4];
								v += 2;
							}

							{
								/* square vertex to last angle */
								double t = 0.5 + 0.636619772367582 * LEDLastAngle - lastEdgeFloat;
								if (t < 0.0)
								{
									t = 0.0;
								}
								else if (t > 1.0)
								{
									t = 1.0;
								}

								t = 0.5 * (1.0 - System.Math.Tan(0.25 * (System.Math.PI - 2.0 * System.Math.PI * t)));
								double lx = (1.0 - t) * LEDVectors[(lastEdge + 3) % 4].X + t * LEDVectors[lastEdge % 4].X;
								double ly = (1.0 - t) * LEDVectors[(lastEdge + 3) % 4].Y + t * LEDVectors[lastEdge % 4].Y;
								double lz = (1.0 - t) * LEDVectors[(lastEdge + 3) % 4].Z + t * LEDVectors[lastEdge % 4].Z;
								States[CurrentState].Prototype.Mesh.Vertices[v + 0].Coordinates = LEDVectors[(lastEdge + 3) % 4];
								States[CurrentState].Prototype.Mesh.Vertices[v + 1].Coordinates = new Vector3(lx, ly, lz);
								v += 2;
							}
						}
					}
				}
				else
				{
					/* winding is counter-clockwise*/
					if (ledangle > LEDInitialAngle)
					{
						ledangle = LEDInitialAngle;
					}

					if (ledangle > LEDLastAngle)
					{
						double currentEdgeFloat = System.Math.Floor(0.636619772367582 * (ledangle + 0.785398163397449));
						int currentEdge = ((int) currentEdgeFloat % 4 + 4) % 4;
						double lastEdgeFloat = System.Math.Floor(0.636619772367582 * (LEDLastAngle + 0.785398163397449));
						int lastEdge = ((int) lastEdgeFloat % 4 + 4) % 4;
						if (currentEdge < lastEdge | lastEdge == currentEdge & System.Math.Abs(currentEdgeFloat - lastEdgeFloat) > 2.0)
						{
							currentEdge += 4;
						}

						if (currentEdge == lastEdge)
						{
							/* current angle to last angle */
							{
								double t = 0.5 + 0.636619772367582 * LEDLastAngle - lastEdgeFloat;
								if (t < 0.0)
								{
									t = 0.0;
								}
								else if (t > 1.0)
								{
									t = 1.0;
								}

								t = 0.5 * (1.0 - System.Math.Tan(0.25 * (System.Math.PI - 2.0 * System.Math.PI * t)));
								double lx = (1.0 - t) * LEDVectors[(lastEdge + 3) % 4].X + t * LEDVectors[lastEdge].X;
								double ly = (1.0 - t) * LEDVectors[(lastEdge + 3) % 4].Y + t * LEDVectors[lastEdge].Y;
								double lz = (1.0 - t) * LEDVectors[(lastEdge + 3) % 4].Z + t * LEDVectors[lastEdge].Z;
								States[CurrentState].Prototype.Mesh.Vertices[v].Coordinates = new Vector3(lx, ly, lz);
								v++;
							}
							{
								double t = 0.5 + 0.636619772367582 * ledangle - currentEdgeFloat;
								if (t < 0.0)
								{
									t = 0.0;
								}
								else if (t > 1.0)
								{
									t = 1.0;
								}

								t -= System.Math.Floor(t);
								t = 0.5 * (1.0 - System.Math.Tan(0.25 * (System.Math.PI - 2.0 * System.Math.PI * t)));
								double cx = (1.0 - t) * LEDVectors[(currentEdge + 3) % 4].X + t * LEDVectors[currentEdge].X;
								double cy = (1.0 - t) * LEDVectors[(currentEdge + 3) % 4].Y + t * LEDVectors[currentEdge].Y;
								double cz = (1.0 - t) * LEDVectors[(currentEdge + 3) % 4].Z + t * LEDVectors[currentEdge].Z;
								States[CurrentState].Prototype.Mesh.Vertices[v].Coordinates = new Vector3(cx, cy, cz);
								v++;
							}
						}
						else
						{
							{
								/* current angle to square vertex */
								double t = 0.5 + 0.636619772367582 * ledangle - currentEdgeFloat;
								if (t < 0.0)
								{
									t = 0.0;
								}
								else if (t > 1.0)
								{
									t = 1.0;
								}

								t = 0.5 * (1.0 - System.Math.Tan(0.25 * (System.Math.PI - 2.0 * System.Math.PI * t)));
								double cx = (1.0 - t) * LEDVectors[(currentEdge + 3) % 4].X + t * LEDVectors[currentEdge % 4].X;
								double cy = (1.0 - t) * LEDVectors[(currentEdge + 3) % 4].Y + t * LEDVectors[currentEdge % 4].Y;
								double cz = (1.0 - t) * LEDVectors[(currentEdge + 3) % 4].Z + t * LEDVectors[currentEdge % 4].Z;
								States[CurrentState].Prototype.Mesh.Vertices[v + 0].Coordinates = LEDVectors[(currentEdge + 3) % 4];
								States[CurrentState].Prototype.Mesh.Vertices[v + 1].Coordinates = new Vector3(cx, cy, cz);
								v += 2;
							}
							for (int j = currentEdge - 1; j > lastEdge; j--)
							{
								/* square-vertex to square-vertex */
								States[CurrentState].Prototype.Mesh.Vertices[v + 0].Coordinates = LEDVectors[(j + 3) % 4];
								States[CurrentState].Prototype.Mesh.Vertices[v + 1].Coordinates = LEDVectors[j % 4];
								v += 2;
							}

							{
								/* square vertex to last angle */
								double t = 0.5 + 0.636619772367582 * LEDLastAngle - lastEdgeFloat;
								if (t < 0.0)
								{
									t = 0.0;
								}
								else if (t > 1.0)
								{
									t = 1.0;
								}

								t = 0.5 * (1.0 - System.Math.Tan(0.25 * (System.Math.PI - 2.0 * System.Math.PI * t)));
								double lx = (1.0 - t) * LEDVectors[(lastEdge + 3) % 4].X + t * LEDVectors[lastEdge].X;
								double ly = (1.0 - t) * LEDVectors[(lastEdge + 3) % 4].Y + t * LEDVectors[lastEdge].Y;
								double lz = (1.0 - t) * LEDVectors[(lastEdge + 3) % 4].Z + t * LEDVectors[lastEdge].Z;
								States[CurrentState].Prototype.Mesh.Vertices[v + 0].Coordinates = new Vector3(lx, ly, lz);
								States[CurrentState].Prototype.Mesh.Vertices[v + 1].Coordinates = LEDVectors[lastEdge % 4];
								v += 2;
							}
						}
					}
				}

				for (int j = v; v < 11; v++)
				{
					States[CurrentState].Prototype.Mesh.Vertices[j].Coordinates = LEDVectors[4];
				}
			}

			// update prototype
			internalObject.Prototype = States[CurrentState].Prototype;

			// update VAO for led if required
			UpdateVAO = led;

			// update state
			// rotate
			internalObject.Rotate = Matrix4D.Identity;

			if (rotateX)
			{
				internalObject.Rotate *= Matrix4D.CreateFromAxisAngle(new Vector3(RotateXDirection.X, RotateXDirection.Y, -RotateXDirection.Z), 2.0 * System.Math.PI - radianX);
			}

			if (rotateY)
			{
				internalObject.Rotate *= Matrix4D.CreateFromAxisAngle(new Vector3(RotateYDirection.X, RotateYDirection.Y, -RotateYDirection.Z), 2.0 * System.Math.PI - radianY);
			}

			if (rotateZ)
			{
				internalObject.Rotate *= Matrix4D.CreateFromAxisAngle(new Vector3(RotateZDirection.X, RotateZDirection.Y, -RotateZDirection.Z), 2.0 * System.Math.PI - radianZ);
			}

			// scale
			if (scaleX | scaleY | scaleZ)
			{
				Matrix4D scaleM = Matrix4D.Identity;
				scaleM.Row0 *= scale.X;
				scaleM.Row1 *= scale.Y;
				scaleM.Row2 *= scale.Z;
				internalObject.Scale = scaleM;
			}

			if (Camera != null && Camera.CurrentRestriction != CameraRestrictionMode.NotAvailable && Camera.CurrentRestriction != CameraRestrictionMode.Restricted3D)
			{
				internalObject.Rotate *= States[CurrentState].Translation * Matrix4D.CreateTranslation(-Position.X, -Position.Y, Position.Z);
				internalObject.Rotate *= (Matrix4D)new Transformation((Vector3)Camera.AbsoluteDirection, (Vector3)Camera.AbsoluteUp, (Vector3)Camera.AbsoluteSide);

				// translate
				double dx = -System.Math.Tan(Camera.Alignment.Yaw) - Camera.Alignment.Position.X;
				double dy = -System.Math.Tan(Camera.Alignment.Pitch) - Camera.Alignment.Position.Y;
				double dz = -Camera.Alignment.Position.Z;
				Vector3 add = Camera.AbsolutePosition + dx * Camera.AbsoluteSide + dy * Camera.AbsoluteUp + dz * Camera.AbsoluteDirection;
				internalObject.Translation = Matrix4D.CreateTranslation(add.X, add.Y, -add.Z);
			}
			else
			{
				internalObject.Rotate *= States[CurrentState].Translation;
				internalObject.Rotate *= (Matrix4D)new Transformation(Direction, Up, Side);

				// translate
				internalObject.Translation = Matrix4D.CreateTranslation(Position.X, Position.Y, -Position.Z);
			}

			if (ColorFunction != null && Colors != null)
			{
				int color = (int)ColorFunction.LastResult;
				if (UpdateFunctions)
				{
					color = (int)ColorFunction.ExecuteScript(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
				}
				internalObject.Prototype.Mesh.Materials[0].Color = Colors[color];
			}
			// visibility changed
			// TouchElement is handled by another function.
			if (!IsTouch)
			{
				if (Show)
				{
					currentHost.ShowObject(internalObject, Camera != null ? ObjectType.Overlay : ObjectType.Dynamic);
				}
				else
				{
					currentHost.HideObject(internalObject);
				}
			}
		}

		/// <summary>Creates the animated object within the game world</summary>
		/// <param name="Position">The absolute position</param>
		/// <param name="WorldTransformation">The world transformation to apply (e.g. ground, rail)</param>
		/// <param name="LocalTransformation">The local transformation to apply in order to rotate the model</param>
		/// <param name="sectionIndex">The index of the section if placed using a SigF command</param>
		/// <param name="TrackPosition">The absolute track position</param>
		public void CreateObject(Vector3 Position, Transformation WorldTransformation, Transformation LocalTransformation, int sectionIndex, double TrackPosition)
		{

			int a = currentHost.AnimatedWorldObjectsUsed;
			Transformation FinalTransformation = new Transformation(LocalTransformation, WorldTransformation);

			//Place track followers if required
			if (TrackFollowerFunction != null)
			{
				var o = this.Clone();
				currentHost.CreateDynamicObject(ref o.internalObject);
				TrackFollowingObject currentObject = new TrackFollowingObject(currentHost)
				{
					Position = Position,
					Direction = FinalTransformation.Z,
					Up = FinalTransformation.Y,
					Side = FinalTransformation.X,
					Object = o,
					TrackPosition = TrackPosition,
				};

				currentObject.FrontAxleFollower.TrackPosition = TrackPosition + FrontAxlePosition;
				currentObject.RearAxleFollower.TrackPosition = TrackPosition + RearAxlePosition;
				currentObject.FrontAxlePosition = FrontAxlePosition;
				currentObject.RearAxlePosition = RearAxlePosition;
				currentObject.FrontAxleFollower.UpdateWorldCoordinates(false);
				currentObject.RearAxleFollower.UpdateWorldCoordinates(false);
				for (int i = 0; i < currentObject.Object.States.Length; i++)
				{
					if (currentObject.Object.States[i].Prototype == null)
					{
						currentObject.Object.States[i].Prototype = new StaticObject(currentHost);
					}
				}


				double r = 0.0;
				for (int i = 0; i < currentObject.Object.States.Length; i++)
				{
					for (int j = 0; j < currentObject.Object.States[i].Prototype.Mesh.Vertices.Length; j++)
					{
						double t = States[i].Prototype.Mesh.Vertices[j].Coordinates.Norm();
						if (t > r) r = t;
					}
				}

				currentObject.Radius = System.Math.Sqrt(r);
				currentObject.Visible = false;
				currentObject.Object.Initialize(0, ObjectType.Dynamic, false);
				currentHost.AnimatedWorldObjects[a] = currentObject;
			}
			else
			{
				var o = this.Clone();
				currentHost.CreateDynamicObject(ref o.internalObject);
				o.SectionIndex = sectionIndex;
				AnimatedWorldObject currentObject = new AnimatedWorldObject(currentHost)
				{
					Position = Position,
					Direction = FinalTransformation.Z,
					Up = FinalTransformation.Y,
					Side = FinalTransformation.X,
					Object = o,
					SectionIndex = sectionIndex,
					TrackPosition = TrackPosition,
				};
				for (int i = 0; i < currentObject.Object.States.Length; i++)
				{
					if (currentObject.Object.States[i].Prototype == null)
					{
						currentObject.Object.States[i].Prototype = new StaticObject(currentHost);
					}
				}


				double r = 0.0;
				for (int i = 0; i < currentObject.Object.States.Length; i++)
				{
					for (int j = 0; j < currentObject.Object.States[i].Prototype.Mesh.Vertices.Length; j++)
					{
						double t = States[i].Prototype.Mesh.Vertices[j].Coordinates.Norm();
						if (t > r) r = t;
					}
				}

				currentObject.Radius = System.Math.Sqrt(r);
				currentObject.Visible = false;
				currentObject.Object.Initialize(0, ObjectType.Dynamic, false);
				currentHost.AnimatedWorldObjects[a] = currentObject;
			}

			currentHost.AnimatedWorldObjectsUsed++;
		}

		/// <summary>Reverses the object</summary>
		public void Reverse(bool interior = false)
		{
			foreach (ObjectState state in States)
			{
				if (state.Prototype == null)
				{
					continue;
				}
				state.Prototype = (StaticObject)state.Prototype.Clone();
				state.Prototype.ApplyScale(-1.0, 1.0, -1.0);
				Matrix4D t = state.Translation;
				t.Row3.X *= -1.0f;
				t.Row3.Z *= -1.0f;
				state.Translation = t;
			}
			TranslateXDirection.X *= -1.0;
			TranslateXDirection.Z *= -1.0;
			TranslateYDirection.X *= -1.0;
			TranslateYDirection.Z *= -1.0;
			TranslateZDirection.X *= -1.0;
			TranslateZDirection.Z *= -1.0;
			// If our object is an interior, we need to reverse the rotation of objects
			// This does not apply to exterior / general objects, as we're using a translation matrix
			if (interior)
			{
				RotateXDirection.X *= -1.0;
				RotateXDirection.Z *= -1.0;
				RotateYDirection.X *= -1.0;
				RotateYDirection.Z *= -1.0;
				RotateZDirection.X *= -1.0;
				RotateZDirection.Z *= -1.0;
			}
		}

		/// <summary>Corrects the car indices in any function scripts in use</summary>
		public void CorrectCarIndices(int offset)
		{
			if (RotateXFunction is FunctionScript rx)
			{
				rx.CorrectCarIndices(offset);
			}
			if (RotateYFunction is FunctionScript ry)
			{
				ry.CorrectCarIndices(offset);
			}
			if (RotateZFunction is FunctionScript rz)
			{
				rz.CorrectCarIndices(offset);
			}
			if (TranslateXFunction is FunctionScript tx)
			{
				tx.CorrectCarIndices(offset);
			}
			if (TranslateYFunction is FunctionScript ty)
			{
				ty.CorrectCarIndices(offset);
			}
			if (TranslateZFunction is FunctionScript tz)
			{
				tz.CorrectCarIndices(offset);
			}
			if (ScaleXFunction is FunctionScript sx)
			{
				sx.CorrectCarIndices(offset);
			}
			if (ScaleYFunction is FunctionScript sy)
			{
				sy.CorrectCarIndices(offset);
			}
			if (ScaleZFunction is FunctionScript sz)
			{
				sz.CorrectCarIndices(offset);
			}
			if (TextureShiftXFunction is FunctionScript tsx)
			{
				tsx.CorrectCarIndices(offset);
			}
			if (TextureShiftYFunction is FunctionScript tsy)
			{
				tsy.CorrectCarIndices(offset);
			}
			if (StateFunction is FunctionScript s)
			{
				s.CorrectCarIndices(offset);
			}
			if (ColorFunction is FunctionScript c)
			{
				c.CorrectCarIndices(offset);
			}
		}

		/// <summary>Loads all textures associated with this object</summary>
		public void LoadTextures()
		{
			for (int i = 0; i < States.Length; i++)
			{
				if (States[i].Prototype == null || States[i].Prototype.Mesh == null)
				{
					continue;
				}
				foreach (MeshFace face in States[i].Prototype.Mesh.Faces)
				{
					OpenGlTextureWrapMode wrap = OpenGlTextureWrapMode.ClampClamp;
					if (States[i].Prototype.Mesh.Materials[face.Material].DaytimeTexture != null || States[i].Prototype.Mesh.Materials[face.Material].NighttimeTexture != null)
					{
						if (States[i].Prototype.Mesh.Materials[face.Material].WrapMode == null)
						{
							/*
							 * If the object does not have a stored wrapping mode determine it now. However:
							 * https://github.com/leezer3/OpenBVE/issues/971
							 *
							 * Unfortunately, there appear to be X objects in the wild which expect a non-default wrapping mode
							 * which means the best fast exit we can do is to check for RepeatRepeat....
							 *
							 */
							foreach (VertexTemplate vertex in States[i].Prototype.Mesh.Vertices)
							{
								if (vertex.TextureCoordinates.X < 0.0f || vertex.TextureCoordinates.X > 1.0f)
								{
									wrap |= OpenGlTextureWrapMode.RepeatClamp;
								}

								if (vertex.TextureCoordinates.Y < 0.0f || vertex.TextureCoordinates.Y > 1.0f)
								{
									wrap |= OpenGlTextureWrapMode.ClampRepeat;
								}

								if (wrap == OpenGlTextureWrapMode.RepeatRepeat)
								{
									break;
								}
							}
						}
						else
						{
							wrap = (OpenGlTextureWrapMode)States[i].Prototype.Mesh.Materials[face.Material].WrapMode;
						}

						States[i].Prototype.Mesh.Materials[face.Material].WrapMode = wrap;
						if (States[i].Prototype.Mesh.Materials[face.Material].DaytimeTexture != null)
						{
							currentHost.LoadTexture(ref States[i].Prototype.Mesh.Materials[face.Material].DaytimeTexture, wrap);
							States[i].Prototype.Mesh.Materials[face.Material].DaytimeTexture.AvailableToUnload = false;
						}
						if (States[i].Prototype.Mesh.Materials[face.Material].NighttimeTexture != null)
						{
							currentHost.LoadTexture(ref States[i].Prototype.Mesh.Materials[face.Material].NighttimeTexture, wrap);
							States[i].Prototype.Mesh.Materials[face.Material].NighttimeTexture.AvailableToUnload = false;
						}
					}
				}
			}


		}

		/// <summary>Checks whether the two specified animated objects are equal</summary>
		public static bool operator ==(AnimatedObject a, AnimatedObject b)
		{
			if (a is null)
			{
				return b is null;
			}
			return a.Equals(b);
		}

		/// <summary>Checks whether the two specified animated objects are unequal</summary>
		public static bool operator !=(AnimatedObject a, AnimatedObject b)
		{
			if (a is null)
			{
				return !(b is null);
			}
			return !a.Equals(b);
		}

		/// <summary>Indicates whether this instance and a specified instance are equal.</summary>
		/// <param name="animatedObject">The instance to compare to.</param>
		/// <returns>True if the instances are equal; false otherwise.</returns>
		public bool Equals(AnimatedObject animatedObject)
		{
			if (animatedObject is null)
			{
				return false;
			}
			if (animatedObject.FileName != FileName) return false; // fast return hopefully
			if (animatedObject.States != States) return false;
			if (animatedObject.StateFunction != StateFunction) return false;
			if (animatedObject.TranslateXDirection != TranslateXDirection) return false;
			if (animatedObject.TranslateYDirection != TranslateYDirection) return false;
			if (animatedObject.TranslateZDirection != TranslateZDirection) return false;
			if (animatedObject.TranslateXFunction != TranslateXFunction) return false;
			if (animatedObject.TranslateYFunction != TranslateYFunction) return false;
			if (animatedObject.TranslateZFunction != TranslateZFunction) return false;
			if (animatedObject.RotateXDirection != RotateXDirection) return false;
			if (animatedObject.RotateXDamping != RotateXDamping) return false;
			if (animatedObject.RotateYDamping != RotateYDamping) return false;
			if (animatedObject.RotateZDamping != RotateZDamping) return false;
			if (animatedObject.RotateYDirection != RotateYDirection) return false;
			if (animatedObject.RotateZDirection != RotateZDirection) return false;
			if (animatedObject.RotateXFunction != RotateXFunction) return false;
			if (animatedObject.RotateYFunction != RotateYFunction) return false;
			if (animatedObject.RotateZFunction != RotateZFunction) return false;
			if (animatedObject.TextureShiftXDirection != TextureShiftXDirection) return false;
			if (animatedObject.TextureShiftYDirection != TextureShiftYDirection) return false;
			if (animatedObject.TextureShiftXFunction != TextureShiftXFunction) return false;
			if (animatedObject.TextureShiftYFunction != TextureShiftYFunction) return false;
			if (animatedObject.ScaleXFunction != ScaleXFunction) return false;
			if (animatedObject.ScaleYFunction != ScaleYFunction) return false;
			if (animatedObject.ScaleZFunction != ScaleZFunction) return false;
			if (animatedObject.LEDClockwiseWinding != LEDClockwiseWinding) return false;
			if (animatedObject.LEDInitialAngle != LEDInitialAngle) return false;
			if (animatedObject.LEDLastAngle != LEDLastAngle) return false;
			if (animatedObject.LEDVectors != LEDVectors) return false;
			if (animatedObject.LEDFunction != LEDFunction) return false;
			if (animatedObject.RefreshRate != RefreshRate) return false;
			if (animatedObject.TrackFollowerFunction != TrackFollowerFunction) return false;
			if (animatedObject.FrontAxlePosition != FrontAxlePosition) return false;
			if (animatedObject.RearAxlePosition != RearAxlePosition) return false;
			if (animatedObject.isTimeTableObject != isTimeTableObject) return false;
			// other fields should be instance related only
			return true;
		}

		/// <summary>Indicates whether this instance and a specified object are equal.</summary>
		/// <param name="obj">The object to compare to.</param>
		/// <returns>True if the instances are equal; false otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (!(obj is AnimatedObject animatedObject))
			{
				return false;
			}

			return Equals(animatedObject);
		}
	}
}
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
