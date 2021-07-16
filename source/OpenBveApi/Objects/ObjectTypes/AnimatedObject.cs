using System.IO;
using System.Linq;
using CSScriptLibrary;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Graphics;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Trains;
using OpenBveApi.World;

namespace OpenBveApi.Objects
{
	/// <summary>The base type for an animated object</summary>
	public class AnimatedObject
	{
		/// <summary>The array of states</summary>
		public ObjectState[] States;
		/// <summary>The function script controlling state changes</summary>
		public FunctionScript StateFunction;
		/// <summary>The index of the current state</summary>
		public int CurrentState;
		/// <summary>A 3D vector describing the direction taken when TranslateXFunction is called</summary>
		public Vector3 TranslateXDirection;
		/// <summary>A 3D vector describing the direction taken when TranslateXYFunction is called</summary>
		public Vector3 TranslateYDirection;
		/// <summary>A 3D vector describing the direction taken when TranslateZFunction is called</summary>
		public Vector3 TranslateZDirection;
		/// <summary>The function script controlling translation in the X direction</summary>
		/// <remarks>X is an arbitrary 3D direction, nominally 1,0,0</remarks>
		public FunctionScript TranslateXFunction;
		/// <summary>The function script controlling translation in the Y direction</summary>
		/// <remarks>Y is an arbitrary 3D direction, nominally 0,1,0</remarks>
		public FunctionScript TranslateYFunction;
		/// <summary>The function script controlling translation in the Z direction</summary>
		/// <remarks>Z is an arbitrary 3D direction, nominally 0,0,1</remarks>
		public FunctionScript TranslateZFunction;
		/// <summary>A 3D vector describing the rotation performed when RotateXFunction is called</summary>
		public Vector3 RotateXDirection;
		/// <summary>A 3D vector describing the rotation performed when RotateYFunction is called</summary>
		public Vector3 RotateYDirection;
		/// <summary>A 3D vector describing the rotation performed when RotateZFunction is called</summary>
		public Vector3 RotateZDirection;
		/// <summary>The function script controlling translation in the X direction</summary>
		/// <remarks>X is an arbitrary 3D direction, nominally 1,0,0</remarks>
		public FunctionScript RotateXFunction;
		/// <summary>The function script controlling translation in the Y direction</summary>
		/// <remarks>Y is an arbitrary 3D direction, nominally 0,1,0</remarks>
		public FunctionScript RotateYFunction;
		/// <summary>The function script controlling translation in the Z direction</summary>
		/// <remarks>Z is an arbitrary 3D direction, nominally 0,0,1</remarks>
		public FunctionScript RotateZFunction;
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
		public FunctionScript TextureShiftXFunction;
		/// <summary>The function script controlling texture shifting in the Y direction</summary>
		/// <remarks>X is an arbitrary 2D direction, nominally 0,1</remarks>
		public FunctionScript TextureShiftYFunction;
		/// <summary>If the LED function is used, this controls whether the winding is clockwise or anti-clockwise</summary>
		public bool LEDClockwiseWinding;
		/// <summary>The initial angle of the LED function</summary>
		public double LEDInitialAngle;
		/// <summary>The final angle of the LED function</summary>
		public double LEDLastAngle;
		/// <summary>If LEDFunction is used, an array of five vectors representing the bottom-left, up-left, up-right, bottom-right and center coordinates of the LED square, or a null reference otherwise.</summary>
		public Vector3[] LEDVectors;
		/// <summary>The function script controlling the LED square</summary>
		public FunctionScript LEDFunction;
		/// <summary>The refresh rate in seconds</summary>
		public double RefreshRate;
		/// <summary>The time since the last update of this object</summary>
		public double SecondsSinceLastUpdate;

		//This section holds script files executed by CS-Script
		/// <summary>The absolute path to the script file to be evaluated when TranslateXScript is called</summary>
		public string TranslateXScriptFile;
		/// <summary>The actual script interface</summary>
		public AnimationScript TranslateXAnimationScript;
		/// <summary>The absolute path to the script file to be evaluated when TranslateYScript is called</summary>
		public AnimationScript TranslateYAnimationScript;
		/// <summary>The actual script interface</summary>
		public string TranslateYScriptFile;
		/// <summary>The absolute path to the script file to be evaluated when TranslateZScript is called</summary>
		public AnimationScript TranslateZAnimationScript;
		/// <summary>The actual script interface</summary>
		public string TranslateZScriptFile;
		/// <summary>The function script controlling movement along the track</summary>
		public FunctionScript TrackFollowerFunction;
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
		
		/// <summary>Creates a new animated object</summary>
		public AnimatedObject(HostInterface host)
		{
			currentHost = host;
			States = new ObjectState[] { };
		}

		/// <summary>Clones this object</summary>
		/// <returns>The new object</returns>
		public AnimatedObject Clone()
		{
			AnimatedObject Result = new AnimatedObject(currentHost) { States = States.Select(x => (ObjectState)x.Clone()).ToArray() };

			Result.TrackFollowerFunction = TrackFollowerFunction?.Clone();
			Result.FrontAxlePosition = this.FrontAxlePosition;
			Result.RearAxlePosition = this.RearAxlePosition;
			Result.TranslateXScriptFile = this.TranslateXScriptFile;
			Result.StateFunction = StateFunction?.Clone();
			Result.CurrentState = this.CurrentState;
			Result.TranslateZDirection = this.TranslateZDirection;
			Result.TranslateYDirection = this.TranslateYDirection;
			Result.TranslateXDirection = this.TranslateXDirection;
			Result.TranslateXFunction = TranslateXFunction?.Clone();
			Result.TranslateYFunction = TranslateYFunction?.Clone();
			Result.TranslateZFunction = TranslateZFunction?.Clone();
			Result.RotateXDirection = this.RotateXDirection;
			Result.RotateYDirection = this.RotateYDirection;
			Result.RotateZDirection = this.RotateZDirection;
			Result.RotateXFunction = RotateXFunction?.Clone();
			Result.RotateXDamping = RotateXDamping?.Clone();
			Result.RotateYFunction = RotateYFunction?.Clone();
			Result.RotateYDamping = RotateYDamping?.Clone();
			Result.RotateZFunction = RotateZFunction?.Clone();
			Result.RotateZDamping = RotateZDamping?.Clone();
			Result.TextureShiftXDirection = this.TextureShiftXDirection;
			Result.TextureShiftYDirection = this.TextureShiftYDirection;
			Result.TextureShiftXFunction = TextureShiftXFunction?.Clone();
			Result.TextureShiftYFunction = TextureShiftYFunction?.Clone();
			Result.LEDClockwiseWinding = this.LEDClockwiseWinding;
			Result.LEDInitialAngle = this.LEDInitialAngle;
			Result.LEDLastAngle = this.LEDLastAngle;
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
			if (this.TranslateXFunction != null | this.TranslateYFunction != null | this.TranslateZFunction != null) return false;
			if (this.RotateXFunction != null | this.RotateYFunction != null | this.RotateZFunction != null) return false;
			if (this.TextureShiftXFunction != null | this.TextureShiftYFunction != null) return false;
			if (this.LEDFunction != null) return false;
			if (this.TranslateXScriptFile != null | this.TranslateYScriptFile != null | this.TranslateZScriptFile != null) return false;
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
			if (t >= 0 && States[t].Prototype != null)
			{
				internalObject.Prototype = States[t].Prototype;
			}
			else
			{
				/*
				 * Must internally reset the object, not create a new one.
				 * This allows the reference to keep pointing to the same place
				 */
				internalObject.Prototype = new StaticObject(currentHost);
			}

			CurrentState = StateIndex;
			if (Show)
			{
				currentHost.ShowObject(internalObject, Type);
			}
		}

		/// <summary> Updates the position and state of the animated object</summary>
		/// <param name="IsPartOfTrain">Whether this object forms part of a train</param>
		/// <param name="Train">The train, or a null reference otherwise</param>
		/// <param name="CarIndex">If this object forms part of a train, the car index it refers to</param>
		/// <param name="SectionIndex">If this object has been placed via Track.Sig, the index of the section it is attached to</param>
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
		public void Update(bool IsPartOfTrain, AbstractTrain Train, int CarIndex, int SectionIndex, double TrackPosition, Vector3 Position, Vector3 Direction, Vector3 Up, Vector3 Side, bool UpdateFunctions, bool Show, double TimeElapsed, bool EnableDamping, bool IsTouch = false, dynamic Camera = null)
		{
			// state change
			if (StateFunction != null & UpdateFunctions)
			{
				double sd = StateFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
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
					x = TranslateXFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
				}

				Vector3 translationVector = new Vector3(TranslateXDirection); //Must clone
				translationVector.Rotate(Direction, Up, Side);
				translationVector *= x;
				Position += translationVector;
			}
			else if (TranslateXScriptFile != null)
			{
				//Translate X Script
				if (TranslateXAnimationScript == null)
				{
					//Load the script if required
					try
					{
						CSScript.GlobalSettings.TargetFramework = "v4.0";
						TranslateXAnimationScript = CSScript.LoadCode(File.ReadAllText(TranslateXScriptFile))
							.CreateObject("OpenBVEScript")
							.AlignToInterface<AnimationScript>(true);
					}
					catch
					{
						currentHost.AddMessage(MessageType.Error, false,
							"An error occcured whilst parsing script " + TranslateXScriptFile);
						TranslateXScriptFile = null;
						return;
					}
				}

				double x = TranslateXAnimationScript.ExecuteScript(Train, Position, TrackPosition, SectionIndex,
					IsPartOfTrain, TimeElapsed);
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
					y = TranslateYFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
				}

				Vector3 translationVector = new Vector3(TranslateYDirection); //Must clone
				translationVector.Rotate(Direction, Up, Side);
				translationVector *= y;
				Position += translationVector;
			}
			else if (TranslateYScriptFile != null)
			{
				//Translate X Script
				if (TranslateYAnimationScript == null)
				{
					//Load the script if required
					try
					{
						CSScript.GlobalSettings.TargetFramework = "v4.0";
						TranslateYAnimationScript = CSScript.LoadCode(File.ReadAllText(TranslateYScriptFile))
							.CreateObject("OpenBVEScript")
							.AlignToInterface<AnimationScript>(true);
					}
					catch
					{
						currentHost.AddMessage(MessageType.Error, false,
							"An error occcured whilst parsing script " + TranslateYScriptFile);
						TranslateYScriptFile = null;
						return;
					}
				}

				double y = TranslateYAnimationScript.ExecuteScript(Train, Position, TrackPosition, SectionIndex,
					IsPartOfTrain, TimeElapsed);
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
					z = TranslateZFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
				}
				
				Vector3 translationVector = new Vector3(TranslateZDirection); //Must clone
				translationVector.Rotate(Direction, Up, Side);
				translationVector *= z;
				Position += translationVector;
			}
			else if (TranslateZScriptFile != null)
			{
				//Translate X Script
				if (TranslateZAnimationScript == null)
				{
					//Load the script if required
					try
					{
						CSScript.GlobalSettings.TargetFramework = "v4.0";
						TranslateZAnimationScript = CSScript.LoadCode(File.ReadAllText(TranslateZScriptFile))
							.CreateObject("OpenBVEScript")
							.AlignToInterface<AnimationScript>(true);
					}
					catch
					{
						currentHost.AddMessage(MessageType.Error, false,
							"An error occcured whilst parsing script " + TranslateZScriptFile);
						TranslateZScriptFile = null;
						return;
					}
				}

				double z = TranslateZAnimationScript.ExecuteScript(Train, Position, TrackPosition, SectionIndex,
					IsPartOfTrain, TimeElapsed);
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
					radianX = RotateXFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
				}
				
				if (RotateXDamping != null)
				{
					RotateXDamping.Update(TimeElapsed, ref radianX, EnableDamping);
				}
			}

			double radianY = 0.0;
			if (rotateY)
			{
				radianY = RotateYFunction.LastResult;
				if (UpdateFunctions)
				{
					radianY = RotateYFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
				}
				
				if (RotateYDamping != null)
				{
					RotateYDamping.Update(TimeElapsed, ref radianY, EnableDamping);
				}
			}

			double radianZ = 0.0;
			if (rotateZ)
			{
				radianZ = RotateZFunction.LastResult;
				if (UpdateFunctions)
				{
					radianZ = RotateZFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
				}
				
				if (RotateZDamping != null)
				{
					RotateZDamping.Update(TimeElapsed, ref radianZ, EnableDamping);
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
						x = TextureShiftXFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
					}
					
					x -= System.Math.Floor(x);
					internalObject.TextureTranslation *= Matrix4D.CreateTranslation(x * TextureShiftXDirection.X, x * TextureShiftXDirection.Y, 1.0);
				}

				if (shifty)
				{
					double y = TextureShiftYFunction.LastResult;
					if (UpdateFunctions)
					{
						y = TextureShiftYFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
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
					ledangle = LEDFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
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

								t = t - System.Math.Floor(t);
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

			// visibility changed
			// TouchElement is handled by another function.
			if (!IsTouch)
			{
				if (Show)
				{
					if (Camera != null)
					{
						currentHost.ShowObject(internalObject, ObjectType.Overlay);
					}
					else
					{
						currentHost.ShowObject(internalObject, ObjectType.Dynamic);
					}
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
		/// <param name="SectionIndex">The index of the section if placed using a SigF command</param>
		/// <param name="TrackPosition">The absolute track position</param>
		/// <param name="Brightness">The brightness value at the track position</param>
		public void CreateObject(Vector3 Position, Transformation WorldTransformation, Transformation LocalTransformation, int SectionIndex, double TrackPosition, double Brightness)
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
					SectionIndex = SectionIndex,
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

				currentObject.Object.internalObject.Brightness = Brightness;

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
				AnimatedWorldObject currentObject = new AnimatedWorldObject(currentHost)
				{
					Position = Position,
					Direction = FinalTransformation.Z,
					Up = FinalTransformation.Y,
					Side = FinalTransformation.X,
					Object = o,
					SectionIndex = SectionIndex,
					TrackPosition = TrackPosition,
				};
				for (int i = 0; i < currentObject.Object.States.Length; i++)
				{
					if (currentObject.Object.States[i].Prototype == null)
					{
						currentObject.Object.States[i].Prototype = new StaticObject(currentHost);
					}
				}

				currentObject.Object.internalObject.Brightness = Brightness;

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
		public void Reverse()
		{
			foreach (ObjectState state in States)
			{
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
			//As we are using a rotation matrix, we only need to reverse the translation and not the rotation
		}
	}
}
