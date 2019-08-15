using System;
using System.IO;
using CSScriptLibrary;
using LibRender;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Trains;
using OpenBveApi.World;

namespace OpenBve
{
	/// <summary>The ObjectManager is the root class containing functions to load and manage objects within the simulation world</summary>
	public static partial class ObjectManager
	{
		
		internal class AnimatedObject : AnimatedObjectBase
		{
			internal AnimatedObject(HostInterface host)
			{
				currentHost = host;
			}

			/// <summary>Clones this object</summary>
			/// <returns>The new object</returns>
			internal AnimatedObject Clone()
			{
				AnimatedObject Result = new AnimatedObject(currentHost) { States = new AnimatedObjectState[this.States.Length] };
				for (int i = 0; i < this.States.Length; i++)
				{
					Result.States[i].Position = this.States[i].Position;
					if (this.States[i].Object != null)
					{
						Result.States[i].Object = (StaticObject)this.States[i].Object.Clone();
					}
				}
				Result.TrackFollowerFunction = this.TrackFollowerFunction == null ? null : this.TrackFollowerFunction.Clone();
				Result.FrontAxlePosition = this.FrontAxlePosition;
				Result.RearAxlePosition = this.RearAxlePosition;
				Result.TranslateXScriptFile = this.TranslateXScriptFile;
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
				Result.LEDFunction = this.LEDFunction == null ? null : this.LEDFunction.Clone();
				Result.RefreshRate = this.RefreshRate;
				Result.SecondsSinceLastUpdate = 0.0;
				for (int i = 0; i < Timetable.CustomObjectsUsed; i++)
				{
					if (Timetable.CustomObjects[i] == this)
					{
						Timetable.AddObjectForCustomTimetable(Result);
					}
				}
				return Result;
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
			internal void Update(bool IsPartOfTrain, AbstractTrain Train, int CarIndex, int SectionIndex, double TrackPosition, Vector3 Position, Vector3 Direction, Vector3 Up, Vector3 Side, bool UpdateFunctions, bool Show, double TimeElapsed, bool EnableDamping, bool IsTouch = false, dynamic Camera = null)
			{
				int s = CurrentState;
				// state change
				if (StateFunction != null & UpdateFunctions)
				{
					double sd = StateFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
					int si = (int)Math.Round(sd);
					int sn = States.Length;
					if (si < 0 | si >= sn) si = -1;
					if (s != si)
					{
						Initialize(si, Camera != null, Show);
						s = si;
					}
				}
				if (s == -1) return;
				// translation
				if (TranslateXFunction != null)
				{
					double x;
					if (UpdateFunctions)
					{
						x = TranslateXFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
					}
					else
					{
						x = TranslateXFunction.LastResult;
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
					double y;
					if (UpdateFunctions)
					{
						y = TranslateYFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
					}
					else
					{
						y = TranslateYFunction.LastResult;
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
					double z;
					if (UpdateFunctions)
					{
						z = TranslateZFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
					}
					else
					{
						z = TranslateZFunction.LastResult;
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
				double cosX, sinX;
				if (rotateX)
				{
					double a;
					if (UpdateFunctions)
					{
						a = RotateXFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
					}
					else
					{
						a = RotateXFunction.LastResult;
					}
					if (RotateXDamping != null)
					{
						RotateXDamping.Update(TimeElapsed, ref a, EnableDamping);
					}
					cosX = Math.Cos(a);
					sinX = Math.Sin(a);
				}
				else
				{
					cosX = 0.0; sinX = 0.0;
				}
				double cosY, sinY;
				if (rotateY)
				{
					double a;
					if (UpdateFunctions)
					{
						a = RotateYFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
					}
					else
					{
						a = RotateYFunction.LastResult;
					}
					if (RotateYDamping != null)
					{
						RotateYDamping.Update(TimeElapsed, ref a, EnableDamping);
					}
					cosY = Math.Cos(a);
					sinY = Math.Sin(a);
				}
				else
				{
					cosY = 0.0; sinY = 0.0;
				}
				double cosZ, sinZ;
				if (rotateZ)
				{
					double a;
					if (UpdateFunctions)
					{
						a = RotateZFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
					}
					else
					{
						a = RotateZFunction.LastResult;
					}
					if (RotateZDamping != null)
					{
						RotateZDamping.Update(TimeElapsed, ref a, EnableDamping);
					}
					cosZ = Math.Cos(a);
					sinZ = Math.Sin(a);
				}
				else
				{
					cosZ = 0.0; sinZ = 0.0;
				}
				// texture shift
				bool shiftx = TextureShiftXFunction != null;
				bool shifty = TextureShiftYFunction != null;
				if ((shiftx | shifty) & UpdateFunctions)
				{
					for (int k = 0; k < internalObject.Mesh.Vertices.Length; k++)
					{
						internalObject.Mesh.Vertices[k].TextureCoordinates = States[s].Object.Mesh.Vertices[k].TextureCoordinates;
					}
					if (shiftx)
					{
						double x = TextureShiftXFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
						x -= Math.Floor(x);
						for (int k = 0; k < internalObject.Mesh.Vertices.Length; k++)
						{
							internalObject.Mesh.Vertices[k].TextureCoordinates.X += (float)(x * TextureShiftXDirection.X);
							internalObject.Mesh.Vertices[k].TextureCoordinates.Y += (float)(x * TextureShiftXDirection.Y);
						}
					}
					if (shifty)
					{
						double y = TextureShiftYFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
						y -= Math.Floor(y);
						for (int k = 0; k < internalObject.Mesh.Vertices.Length; k++)
						{
							internalObject.Mesh.Vertices[k].TextureCoordinates.X += (float)(y * TextureShiftYDirection.X);
							internalObject.Mesh.Vertices[k].TextureCoordinates.Y += (float)(y * TextureShiftYDirection.Y);
						}
					}
				}
				// led
				bool led = LEDFunction != null;
				double ledangle;
				if (led)
				{
					if (UpdateFunctions)
					{
						ledangle = LEDFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
					}
					else
					{
						ledangle = LEDFunction.LastResult;
					}
				}
				else
				{
					ledangle = 0.0;
				}
				// null object
				if (States[s].Object == null)
				{
					return;
				}
				// initialize vertices
				for (int k = 0; k < States[s].Object.Mesh.Vertices.Length; k++)
				{
					internalObject.Mesh.Vertices[k].Coordinates = States[s].Object.Mesh.Vertices[k].Coordinates;
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
							double currentEdgeFloat = Math.Floor(0.636619772367582 * (ledangle + 0.785398163397449));
							int currentEdge = ((int)currentEdgeFloat % 4 + 4) % 4;
							double lastEdgeFloat = Math.Floor(0.636619772367582 * (LEDLastAngle + 0.785398163397449));
							int lastEdge = ((int)lastEdgeFloat % 4 + 4) % 4;
							if (lastEdge < currentEdge | lastEdge == currentEdge & Math.Abs(currentEdgeFloat - lastEdgeFloat) > 2.0)
							{
								lastEdge += 4;
							}
							if (currentEdge == lastEdge)
							{
								/* current angle to last angle */
								{
									double t = 0.5 + (0.636619772367582 * ledangle) - currentEdgeFloat;
									if (t < 0.0)
									{
										t = 0.0;
									}
									else if (t > 1.0)
									{
										t = 1.0;
									}
									t = 0.5 * (1.0 - Math.Tan(0.25 * (Math.PI - 2.0 * Math.PI * t)));
									double cx = (1.0 - t) * LEDVectors[(currentEdge + 3) % 4].X + t * LEDVectors[currentEdge].X;
									double cy = (1.0 - t) * LEDVectors[(currentEdge + 3) % 4].Y + t * LEDVectors[currentEdge].Y;
									double cz = (1.0 - t) * LEDVectors[(currentEdge + 3) % 4].Z + t * LEDVectors[currentEdge].Z;
									States[s].Object.Mesh.Vertices[v].Coordinates = new Vector3(cx, cy, cz);
									v++;
								}
								{
									double t = 0.5 + (0.636619772367582 * LEDLastAngle) - lastEdgeFloat;
									if (t < 0.0)
									{
										t = 0.0;
									}
									else if (t > 1.0)
									{
										t = 1.0;
									}
									t = 0.5 * (1.0 - Math.Tan(0.25 * (Math.PI - 2.0 * Math.PI * t)));
									double lx = (1.0 - t) * LEDVectors[(lastEdge + 3) % 4].X + t * LEDVectors[lastEdge].X;
									double ly = (1.0 - t) * LEDVectors[(lastEdge + 3) % 4].Y + t * LEDVectors[lastEdge].Y;
									double lz = (1.0 - t) * LEDVectors[(lastEdge + 3) % 4].Z + t * LEDVectors[lastEdge].Z;
									States[s].Object.Mesh.Vertices[v].Coordinates = new Vector3(lx, ly, lz);
									v++;
								}
							}
							else
							{
								{
									/* current angle to square vertex */
									double t = 0.5 + (0.636619772367582 * ledangle) - currentEdgeFloat;
									if (t < 0.0)
									{
										t = 0.0;
									}
									else if (t > 1.0)
									{
										t = 1.0;
									}
									t = 0.5 * (1.0 - Math.Tan(0.25 * (Math.PI - 2.0 * Math.PI * t)));
									double cx = (1.0 - t) * LEDVectors[(currentEdge + 3) % 4].X + t * LEDVectors[currentEdge].X;
									double cy = (1.0 - t) * LEDVectors[(currentEdge + 3) % 4].Y + t * LEDVectors[currentEdge].Y;
									double cz = (1.0 - t) * LEDVectors[(currentEdge + 3) % 4].Z + t * LEDVectors[currentEdge].Z;
									States[s].Object.Mesh.Vertices[v + 0].Coordinates = new Vector3(cx, cy, cz);
									States[s].Object.Mesh.Vertices[v + 1].Coordinates = LEDVectors[currentEdge];
									v += 2;
								}
								for (int j = currentEdge + 1; j < lastEdge; j++)
								{
									/* square-vertex to square-vertex */
									States[s].Object.Mesh.Vertices[v + 0].Coordinates = LEDVectors[(j + 3) % 4];
									States[s].Object.Mesh.Vertices[v + 1].Coordinates = LEDVectors[j % 4];
									v += 2;
								}
								{
									/* square vertex to last angle */
									double t = 0.5 + (0.636619772367582 * LEDLastAngle) - lastEdgeFloat;
									if (t < 0.0)
									{
										t = 0.0;
									}
									else if (t > 1.0)
									{
										t = 1.0;
									}
									t = 0.5 * (1.0 - Math.Tan(0.25 * (Math.PI - 2.0 * Math.PI * t)));
									double lx = (1.0 - t) * LEDVectors[(lastEdge + 3) % 4].X + t * LEDVectors[lastEdge % 4].X;
									double ly = (1.0 - t) * LEDVectors[(lastEdge + 3) % 4].Y + t * LEDVectors[lastEdge % 4].Y;
									double lz = (1.0 - t) * LEDVectors[(lastEdge + 3) % 4].Z + t * LEDVectors[lastEdge % 4].Z;
									States[s].Object.Mesh.Vertices[v + 0].Coordinates = LEDVectors[(lastEdge + 3) % 4];
									States[s].Object.Mesh.Vertices[v + 1].Coordinates = new Vector3(lx, ly, lz);
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
							double currentEdgeFloat = Math.Floor(0.636619772367582 * (ledangle + 0.785398163397449));
							int currentEdge = ((int)currentEdgeFloat % 4 + 4) % 4;
							double lastEdgeFloat = Math.Floor(0.636619772367582 * (LEDLastAngle + 0.785398163397449));
							int lastEdge = ((int)lastEdgeFloat % 4 + 4) % 4;
							if (currentEdge < lastEdge | lastEdge == currentEdge & Math.Abs(currentEdgeFloat - lastEdgeFloat) > 2.0)
							{
								currentEdge += 4;
							}
							if (currentEdge == lastEdge)
							{
								/* current angle to last angle */
								{
									double t = 0.5 + (0.636619772367582 * LEDLastAngle) - lastEdgeFloat;
									if (t < 0.0)
									{
										t = 0.0;
									}
									else if (t > 1.0)
									{
										t = 1.0;
									}
									t = 0.5 * (1.0 - Math.Tan(0.25 * (Math.PI - 2.0 * Math.PI * t)));
									double lx = (1.0 - t) * LEDVectors[(lastEdge + 3) % 4].X + t * LEDVectors[lastEdge].X;
									double ly = (1.0 - t) * LEDVectors[(lastEdge + 3) % 4].Y + t * LEDVectors[lastEdge].Y;
									double lz = (1.0 - t) * LEDVectors[(lastEdge + 3) % 4].Z + t * LEDVectors[lastEdge].Z;
									States[s].Object.Mesh.Vertices[v].Coordinates = new Vector3(lx, ly, lz);
									v++;
								}
								{
									double t = 0.5 + (0.636619772367582 * ledangle) - currentEdgeFloat;
									if (t < 0.0)
									{
										t = 0.0;
									}
									else if (t > 1.0)
									{
										t = 1.0;
									}
									t = t - Math.Floor(t);
									t = 0.5 * (1.0 - Math.Tan(0.25 * (Math.PI - 2.0 * Math.PI * t)));
									double cx = (1.0 - t) * LEDVectors[(currentEdge + 3) % 4].X + t * LEDVectors[currentEdge].X;
									double cy = (1.0 - t) * LEDVectors[(currentEdge + 3) % 4].Y + t * LEDVectors[currentEdge].Y;
									double cz = (1.0 - t) * LEDVectors[(currentEdge + 3) % 4].Z + t * LEDVectors[currentEdge].Z;
									States[s].Object.Mesh.Vertices[v].Coordinates = new Vector3(cx, cy, cz);
									v++;
								}
							}
							else
							{
								{
									/* current angle to square vertex */
									double t = 0.5 + (0.636619772367582 * ledangle) - currentEdgeFloat;
									if (t < 0.0)
									{
										t = 0.0;
									}
									else if (t > 1.0)
									{
										t = 1.0;
									}
									t = 0.5 * (1.0 - Math.Tan(0.25 * (Math.PI - 2.0 * Math.PI * t)));
									double cx = (1.0 - t) * LEDVectors[(currentEdge + 3) % 4].X + t * LEDVectors[currentEdge % 4].X;
									double cy = (1.0 - t) * LEDVectors[(currentEdge + 3) % 4].Y + t * LEDVectors[currentEdge % 4].Y;
									double cz = (1.0 - t) * LEDVectors[(currentEdge + 3) % 4].Z + t * LEDVectors[currentEdge % 4].Z;
									States[s].Object.Mesh.Vertices[v + 0].Coordinates = LEDVectors[(currentEdge + 3) % 4];
									States[s].Object.Mesh.Vertices[v + 1].Coordinates = new Vector3(cx, cy, cz);
									v += 2;
								}
								for (int j = currentEdge - 1; j > lastEdge; j--)
								{
									/* square-vertex to square-vertex */
									States[s].Object.Mesh.Vertices[v + 0].Coordinates = LEDVectors[(j + 3) % 4];
									States[s].Object.Mesh.Vertices[v + 1].Coordinates = LEDVectors[j % 4];
									v += 2;
								}
								{
									/* square vertex to last angle */
									double t = 0.5 + (0.636619772367582 * LEDLastAngle) - lastEdgeFloat;
									if (t < 0.0)
									{
										t = 0.0;
									}
									else if (t > 1.0)
									{
										t = 1.0;
									}
									t = 0.5 * (1.0 - Math.Tan(0.25 * (Math.PI - 2.0 * Math.PI * t)));
									double lx = (1.0 - t) * LEDVectors[(lastEdge + 3) % 4].X + t * LEDVectors[lastEdge].X;
									double ly = (1.0 - t) * LEDVectors[(lastEdge + 3) % 4].Y + t * LEDVectors[lastEdge].Y;
									double lz = (1.0 - t) * LEDVectors[(lastEdge + 3) % 4].Z + t * LEDVectors[lastEdge].Z;
									States[s].Object.Mesh.Vertices[v + 0].Coordinates = new Vector3(lx, ly, lz);
									States[s].Object.Mesh.Vertices[v + 1].Coordinates = LEDVectors[lastEdge % 4];
									v += 2;
								}
							}
						}
					}
					for (int j = v; v < 11; v++)
					{
						States[s].Object.Mesh.Vertices[j].Coordinates = LEDVectors[4];
					}
				}
				// update vertices
				for (int k = 0; k < States[s].Object.Mesh.Vertices.Length; k++)
				{
					// rotate
					if (rotateX)
					{
						internalObject.Mesh.Vertices[k].Coordinates.Rotate(RotateXDirection, cosX, sinX);
					}
					if (rotateY)
					{
						internalObject.Mesh.Vertices[k].Coordinates.Rotate(RotateYDirection, cosY, sinY);
					}
					if (rotateZ)
					{
						internalObject.Mesh.Vertices[k].Coordinates.Rotate(RotateZDirection, cosZ, sinZ);
					}
					// translate
					if (Camera != null && Camera.CurrentRestriction != CameraRestrictionMode.NotAvailable)
					{

						internalObject.Mesh.Vertices[k].Coordinates += States[s].Position - Position;
						/*
						 * HACK: No idea why, but when using dynamic here, we MUST cast the parameters to Vector3 as otherwise it breaks....
						 */
						internalObject.Mesh.Vertices[k].Coordinates.Rotate((Vector3) Camera.AbsoluteDirection, (Vector3) Camera.AbsoluteUp, (Vector3) Camera.AbsoluteSide);
						double dx = -Math.Tan(Camera.Alignment.Yaw) - Camera.Alignment.Position.X;
						double dy = -Math.Tan(Camera.Alignment.Pitch) - Camera.Alignment.Position.Y;
						double dz = -Camera.Alignment.Position.Z;
						internalObject.Mesh.Vertices[k].Coordinates.X += Camera.AbsolutePosition.X + dx * Camera.AbsoluteSide.X + dy * Camera.AbsoluteUp.X + dz * Camera.AbsoluteDirection.X;
						internalObject.Mesh.Vertices[k].Coordinates.Y += Camera.AbsolutePosition.Y + dx * Camera.AbsoluteSide.Y + dy * Camera.AbsoluteUp.Y + dz * Camera.AbsoluteDirection.Y;
						internalObject.Mesh.Vertices[k].Coordinates.Z += Camera.AbsolutePosition.Z + dx * Camera.AbsoluteSide.Z + dy * Camera.AbsoluteUp.Z + dz * Camera.AbsoluteDirection.Z;
					}
					else
					{
						internalObject.Mesh.Vertices[k].Coordinates += States[s].Position;
						internalObject.Mesh.Vertices[k].Coordinates.Rotate(Direction, Up, Side);
						internalObject.Mesh.Vertices[k].Coordinates += Position;
					}
				}
				// update normals
				for (int k = 0; k < States[s].Object.Mesh.Faces.Length; k++)
				{
					for (int h = 0; h < States[s].Object.Mesh.Faces[k].Vertices.Length; h++)
					{
						internalObject.Mesh.Faces[k].Vertices[h].Normal = States[s].Object.Mesh.Faces[k].Vertices[h].Normal;
						if (!Vector3.IsZero(States[s].Object.Mesh.Faces[k].Vertices[h].Normal))
						{
							if (rotateX)
							{
								internalObject.Mesh.Faces[k].Vertices[h].Normal.Rotate(RotateXDirection, cosX, sinX);
							}
							if (rotateY)
							{
								internalObject.Mesh.Faces[k].Vertices[h].Normal.Rotate(RotateYDirection, cosY, sinY);
							}
							if (rotateZ)
							{
								internalObject.Mesh.Faces[k].Vertices[h].Normal.Rotate(RotateZDirection, cosZ, sinZ);
							}
							internalObject.Mesh.Faces[k].Vertices[h].Normal.Rotate(Direction, Up, Side);
						}
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
							currentHost.HideObject(ref internalObject);
						}
					}
				}
			}

			internal void CreateObject(Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, int SectionIndex, double TrackPosition, double Brightness)
			{
				int a = AnimatedWorldObjectsUsed;
				if (a >= AnimatedWorldObjects.Length)
				{
					Array.Resize<WorldObject>(ref AnimatedWorldObjects, AnimatedWorldObjects.Length << 1);
				}
				Transformation FinalTransformation = new Transformation(AuxTransformation, BaseTransformation);
				
				//Place track followers if required
				if (TrackFollowerFunction != null)
				{
					var o = this.Clone();
					currentHost.CreateDynamicObject(ref internalObject);
					TrackFollowingObject currentObject = new TrackFollowingObject
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
						if (currentObject.Object.States[i].Object == null)
						{
							currentObject.Object.States[i].Object = new StaticObject(currentHost) { RendererIndex =  -1 };
						}
					}
					double r = 0.0;
					for (int i = 0; i < currentObject.Object.States.Length; i++)
					{
						for (int j = 0; j < currentObject.Object.States[i].Object.Mesh.Materials.Length; j++)
						{
							currentObject.Object.States[i].Object.Mesh.Materials[j].Color *= Brightness;
						}
						for (int j = 0; j < currentObject.Object.States[i].Object.Mesh.Vertices.Length; j++)
						{
							double t = States[i].Object.Mesh.Vertices[j].Coordinates.Norm();
							if (t > r) r = t;
						}
					}
					currentObject.Radius = Math.Sqrt(r);
					currentObject.Visible = false;
					currentObject.Object.Initialize(0, false, false);
					AnimatedWorldObjects[a] = currentObject;
				}
				else
				{
					var o = this.Clone();
					currentHost.CreateDynamicObject(ref o.internalObject);
					AnimatedWorldObject currentObject = new AnimatedWorldObject
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
						if (currentObject.Object.States[i].Object == null)
						{
							currentObject.Object.States[i].Object = new StaticObject(currentHost) { RendererIndex =  -1 };
						}
					}
					double r = 0.0;
					for (int i = 0; i < currentObject.Object.States.Length; i++)
					{
						for (int j = 0; j < currentObject.Object.States[i].Object.Mesh.Materials.Length; j++)
						{
							currentObject.Object.States[i].Object.Mesh.Materials[j].Color *= Brightness;
						}
						for (int j = 0; j < currentObject.Object.States[i].Object.Mesh.Vertices.Length; j++)
						{
							double t = States[i].Object.Mesh.Vertices[j].Coordinates.Norm();
							if (t > r) r = t;
						}
					}
					currentObject.Radius = Math.Sqrt(r);
					currentObject.Visible = false;
					currentObject.Object.Initialize(0, false, false);
					AnimatedWorldObjects[a] = currentObject;
				}
				AnimatedWorldObjectsUsed++;
			}
		}

		
	}
}
