using System;
using OpenBveShared;
using OpenBveApi;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Trains;
using OpenBveApi.World;

namespace OpenBve {
	internal static class ObjectManager {
		internal class AnimatedObject : AbstractAnimatedObject
        {
            internal AnimatedObject Clone()
            {
                AnimatedObject Result = new AnimatedObject();
                Result.States = new AnimatedObjectState[this.States.Length];
                for (int i = 0; i < this.States.Length; i++)
                {
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
                Result.ObjectIndex = -1;
                return Result;
            }

			internal void Initialize(int StateIndex, bool Overlay, bool Show)
			{
				int i = ObjectIndex;
				OpenBveShared.Renderer.HideObject(i);
				int t = StateIndex;
				if (t >= 0 && States[t].Object != null)
				{
					int m = States[t].Object.Mesh.Vertices.Length;
					GameObjectManager.Objects[i].Mesh.Vertices = new VertexTemplate[m];
					for (int k = 0; k < m; k++)
					{
						if (States[t].Object.Mesh.Vertices[k] is ColoredVertex)
						{
							GameObjectManager.Objects[i].Mesh.Vertices[k] = new ColoredVertex((ColoredVertex)States[t].Object.Mesh.Vertices[k]);
						}
						else
						{
							GameObjectManager.Objects[i].Mesh.Vertices[k] = new Vertex((Vertex)States[t].Object.Mesh.Vertices[k]);
						}
						
					}
					m = States[t].Object.Mesh.Faces.Length;
					GameObjectManager.Objects[i].Mesh.Faces = new MeshFace[m];
					for (int k = 0; k < m; k++)
					{
						GameObjectManager.Objects[i].Mesh.Faces[k].Flags = States[t].Object.Mesh.Faces[k].Flags;
						GameObjectManager.Objects[i].Mesh.Faces[k].Material = States[t].Object.Mesh.Faces[k].Material;
						int o = States[t].Object.Mesh.Faces[k].Vertices.Length;
						GameObjectManager.Objects[i].Mesh.Faces[k].Vertices = new MeshFaceVertex[o];
						for (int h = 0; h < o; h++)
						{
							GameObjectManager.Objects[i].Mesh.Faces[k].Vertices[h] = States[t].Object.Mesh.Faces[k].Vertices[h];
						}
					}
					GameObjectManager.Objects[i].Mesh.Materials = States[t].Object.Mesh.Materials;
				}
				else
				{
					GameObjectManager.Objects[i] = null;
					GameObjectManager.Objects[i] = new StaticObject(Program.CurrentHost)
					{
						Mesh =
						{
							Faces = new MeshFace[] {},
							Materials = new MeshMaterial[] {},
							Vertices = new VertexTemplate[] {}
						}
					};
				}
				CurrentState = StateIndex;
				if (Show)
				{
					if (Overlay)
					{
						OpenBveShared.Renderer.ShowObject(i, ObjectType.Overlay, Interface.CurrentOptions.TransparencyMode);
					}
					else
					{
						OpenBveShared.Renderer.ShowObject(i, ObjectType.Dynamic, Interface.CurrentOptions.TransparencyMode);
					}
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
			/// <param name="Overlay">Whether this object should be overlaid over the other objects on-screen (Forms part of the cab etc.)</param>
			/// <param name="UpdateFunctions">Whether the functions associated with this object should be re-evaluated</param>
			/// <param name="Show"></param>
			/// <param name="TimeElapsed">The time elapsed since this object was last updated</param>
			/// <param name="EnableDamping">Whether damping is to be applied for this call</param>
			internal void Update(bool IsPartOfTrain, AbstractTrain Train, int CarIndex, int SectionIndex, double TrackPosition, Vector3 Position, Vector3 Direction, Vector3 Up, Vector3 Side, bool Overlay, bool UpdateFunctions, bool Show, double TimeElapsed, bool EnableDamping)
			{
				int s = CurrentState;
				int i = ObjectIndex;
				// state change
				if (StateFunction != null & UpdateFunctions)
				{
					double sd = StateFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
					int si = (int)Math.Round(sd);
					int sn = States.Length;
					if (si < 0 | si >= sn) si = -1;
					if (s != si)
					{
						Initialize(si, Overlay, Show);
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
					for (int k = 0; k < GameObjectManager.Objects[i].Mesh.Vertices.Length; k++)
					{
						GameObjectManager.Objects[i].Mesh.Vertices[k].TextureCoordinates = States[s].Object.Mesh.Vertices[k].TextureCoordinates;
					}
					if (shiftx)
					{
						double x = TextureShiftXFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
						x -= Math.Floor(x);
						for (int k = 0; k < GameObjectManager.Objects[i].Mesh.Vertices.Length; k++)
						{
							GameObjectManager.Objects[i].Mesh.Vertices[k].TextureCoordinates.X += (float)(x * TextureShiftXDirection.X);
							GameObjectManager.Objects[i].Mesh.Vertices[k].TextureCoordinates.Y += (float)(x * TextureShiftXDirection.Y);
						}
					}
					if (shifty)
					{
						double y = TextureShiftYFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
						y -= Math.Floor(y);
						for (int k = 0; k < GameObjectManager.Objects[i].Mesh.Vertices.Length; k++)
						{
							GameObjectManager.Objects[i].Mesh.Vertices[k].TextureCoordinates.X += (float)(y * TextureShiftYDirection.X);
							GameObjectManager.Objects[i].Mesh.Vertices[k].TextureCoordinates.Y += (float)(y * TextureShiftYDirection.Y);
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
					GameObjectManager.Objects[i].Mesh.Vertices[k].Coordinates = States[s].Object.Mesh.Vertices[k].Coordinates;
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
						GameObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Rotate(RotateXDirection, cosX, sinX);
					}
					if (rotateY)
					{
						GameObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Rotate(RotateYDirection, cosY, sinY);
					}
					if (rotateZ)
					{
						GameObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Rotate(RotateZDirection, cosZ, sinZ);
					}
					// translate
					if (Overlay & Camera.CameraRestriction != CameraRestrictionMode.NotAvailable)
					{
						GameObjectManager.Objects[i].Mesh.Vertices[k].Coordinates += States[s].Position - Position;
						GameObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Rotate(Camera.AbsoluteCameraDirection, Camera.AbsoluteCameraUp, Camera.AbsoluteCameraSide);
						double dx = -Math.Tan(Camera.CameraCurrentAlignment.Yaw) - Camera.CameraCurrentAlignment.Position.X;
						double dy = -Math.Tan(Camera.CameraCurrentAlignment.Pitch) -Camera.CameraCurrentAlignment.Position.Y;
						double dz = -Camera.CameraCurrentAlignment.Position.Z;
						GameObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.X += Camera.AbsoluteCameraPosition.X + dx * Camera.AbsoluteCameraSide.X + dy * Camera.AbsoluteCameraUp.X + dz * Camera.AbsoluteCameraDirection.X;
						GameObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Y += Camera.AbsoluteCameraPosition.Y + dx * Camera.AbsoluteCameraSide.Y + dy * Camera.AbsoluteCameraUp.Y + dz * Camera.AbsoluteCameraDirection.Y;
						GameObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Z += Camera.AbsoluteCameraPosition.Z + dx * Camera.AbsoluteCameraSide.Z + dy * Camera.AbsoluteCameraUp.Z + dz * Camera.AbsoluteCameraDirection.Z;
					}
					else
					{
						GameObjectManager.Objects[i].Mesh.Vertices[k].Coordinates += States[s].Position;
						GameObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Rotate(Direction, Up, Side);
						GameObjectManager.Objects[i].Mesh.Vertices[k].Coordinates += Position;
					}
				}
				// update normals
				for (int k = 0; k < States[s].Object.Mesh.Faces.Length; k++)
				{
					for (int h = 0; h < States[s].Object.Mesh.Faces[k].Vertices.Length; h++)
					{
						GameObjectManager.Objects[i].Mesh.Faces[k].Vertices[h].Normal = States[s].Object.Mesh.Faces[k].Vertices[h].Normal;
						if (!Vector3.IsZero(States[s].Object.Mesh.Faces[k].Vertices[h].Normal))
						{
							if (rotateX)
							{
								GameObjectManager.Objects[i].Mesh.Faces[k].Vertices[h].Normal.Rotate(RotateXDirection, cosX, sinX);
							}
							if (rotateY)
							{
								GameObjectManager.Objects[i].Mesh.Faces[k].Vertices[h].Normal.Rotate(RotateYDirection, cosY, sinY);
							}
							if (rotateZ)
							{
								GameObjectManager.Objects[i].Mesh.Faces[k].Vertices[h].Normal.Rotate(RotateZDirection, cosZ, sinZ);
							}
							GameObjectManager.Objects[i].Mesh.Faces[k].Vertices[h].Normal.Rotate(Direction, Up, Side);
						}
					}
					// visibility changed
					if (Show)
					{
						if (Overlay)
						{
							OpenBveShared.Renderer.ShowObject(i, ObjectType.Overlay, Interface.CurrentOptions.TransparencyMode);
						}
						else
						{
							OpenBveShared.Renderer.ShowObject(i, ObjectType.Dynamic, Interface.CurrentOptions.TransparencyMode);
						}
					}
					else
					{
						OpenBveShared.Renderer.HideObject(i);
					}
				}
			}
        }
        internal class AnimatedObjectCollection : UnifiedObject
        {
            internal AnimatedObject[] Objects;
	        public override void CreateObject(Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, int SectionIndex, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness, bool DuplicateMaterials)
	        {
		        CreateAnimatedWorldObjects(Objects, Position, BaseTransformation, AuxTransformation, SectionIndex, AccurateObjectDisposal, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness, DuplicateMaterials);
	        }

	        public override void OptimizeObject(bool PreserveVerticies)
	        {
		        for (int i = 0; i < Objects.Length; i++)
		        {
			        if (Objects[i] == null)
			        {
				        continue;
			        }
			        for (int j = 0; j < Objects[i].States.Length; j++)
			        {
				        Objects[i].States[j].Object.OptimizeObject(PreserveVerticies);
			        }
		        }
			}
        }
        
        // animated world object
        internal class AnimatedWorldObject : WorldObject
        {
            internal AnimatedObject Object;
            internal int SectionIndex;
            internal double Radius;
	        public override void Update(double TimeElapsed, bool ForceUpdate)
	        {
		        const double extraRadius = 10.0;
				double z = Object.TranslateZFunction == null ? 0.0 : Object.TranslateZFunction.LastResult;
				double pa = TrackPosition + z - Radius - extraRadius;
				double pb = TrackPosition + z + Radius + extraRadius;
				double ta = World.CameraTrackFollower.TrackPosition + OpenBveShared.Camera.CameraCurrentAlignment.Position.Z - OpenBveShared.Renderer.BackgroundImageDistance - OpenBveShared.World.ExtraViewingDistance;
				double tb = World.CameraTrackFollower.TrackPosition + OpenBveShared.Camera.CameraCurrentAlignment.Position.Z + OpenBveShared.Renderer.BackgroundImageDistance + OpenBveShared.World.ExtraViewingDistance;
				bool visible = pb >= ta & pa <= tb;
				if (visible | ForceUpdate)
				{
					if (Object.SecondsSinceLastUpdate >= Object.RefreshRate | ForceUpdate)
					{
						double timeDelta = Object.SecondsSinceLastUpdate + TimeElapsed;
						Object.SecondsSinceLastUpdate = 0.0;
						TrainManager.Train train = null;
						double trainDistance = double.MaxValue;
						for (int j = 0; j < TrainManager.Trains.Length; j++)
						{
							if (TrainManager.Trains[j].State == TrainState.Available)
							{
								double distance;
								if (TrainManager.Trains[j].Cars[0].FrontAxle.Follower.TrackPosition < TrackPosition)
								{
									distance = TrackPosition - TrainManager.Trains[j].Cars[0].FrontAxle.Follower.TrackPosition;
								}
								else if (TrainManager.Trains[j].Cars[TrainManager.Trains[j].Cars.Length - 1].RearAxle.Follower.TrackPosition > TrackPosition)
								{
									distance = TrainManager.Trains[j].Cars[TrainManager.Trains[j].Cars.Length - 1].RearAxle.Follower.TrackPosition - TrackPosition;
								}
								else
								{
									distance = 0;
								}
								if (distance < trainDistance)
								{
									train = TrainManager.Trains[j];
									trainDistance = distance;
								}
							}
						}
						Object.Update(false, train, train == null ? 0 : train.DriverCar, SectionIndex, TrackPosition, Position, Direction, Up, Side, false, true, true, timeDelta, true);
					}
					else
					{
						Object.SecondsSinceLastUpdate += TimeElapsed;
					}
					if (!Visible)
					{
						OpenBveShared.Renderer.ShowObject(Object.ObjectIndex, ObjectType.Dynamic, Interface.CurrentOptions.TransparencyMode);
						Visible = true;
					}
				}
				else
				{
					Object.SecondsSinceLastUpdate += TimeElapsed;
					if (Visible)
					{
						OpenBveShared.Renderer.HideObject(Object.ObjectIndex);
						Visible = false;
					}
				}
	        }
        }

        internal static void CreateAnimatedWorldObjects(AnimatedObject[] Prototypes, Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, int SectionIndex, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness, bool DuplicateMaterials)
        {
            bool[] free = new bool[Prototypes.Length];
            bool anyfree = false;
            for (int i = 0; i < Prototypes.Length; i++)
            {
				if (Prototypes[i] == null)
				{
					free[i] = true;
					continue;
				}
                free[i] = Prototypes[i].IsFreeOfFunctions();
                if (free[i]) anyfree = true;
            }
            if (anyfree)
            {
                for (int i = 0; i < Prototypes.Length; i++)
                {
					if (Prototypes[i] == null)
					{
						continue;
					}
                    if (Prototypes[i].States.Length != 0)
                    {
                        if (free[i])
                        {
                            Vector3 p = Position;
                            Transformation t = new Transformation(BaseTransformation, AuxTransformation);
                            Vector3 s = t.X;
                            Vector3 u = t.Y;
                            Vector3 d = t.Z;
                            p.X += Prototypes[i].States[0].Position.X * s.X + Prototypes[i].States[0].Position.Y * u.X + Prototypes[i].States[0].Position.Z * d.X;
                            p.Y += Prototypes[i].States[0].Position.X * s.Y + Prototypes[i].States[0].Position.Y * u.Y + Prototypes[i].States[0].Position.Z * d.Y;
                            p.Z += Prototypes[i].States[0].Position.X * s.Z + Prototypes[i].States[0].Position.Y * u.Z + Prototypes[i].States[0].Position.Z * d.Z;
                            double zOffset = Prototypes[i].States[0].Position.Z;
                            GameObjectManager.CreateStaticObject(Prototypes[i].States[0].Object, p, BaseTransformation, AuxTransformation, AccurateObjectDisposal, zOffset, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness, DuplicateMaterials, Program.CurrentHost, 600);
                        }
                        else
                        {
                            CreateAnimatedWorldObject(Prototypes[i], Position, BaseTransformation, AuxTransformation, SectionIndex, TrackPosition, Brightness);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < Prototypes.Length; i++)
                {
                    if (Prototypes[i].States.Length != 0)
                    {
                        CreateAnimatedWorldObject(Prototypes[i], Position, BaseTransformation, AuxTransformation, SectionIndex, TrackPosition, Brightness);
                    }
                }
            }
        }
        internal static int CreateAnimatedWorldObject(AnimatedObject Prototype, Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, int SectionIndex, double TrackPosition, double Brightness)
        {
            int a = GameObjectManager.AnimatedWorldObjectsUsed;
            if (a >= GameObjectManager.AnimatedWorldObjects.Length)
            {
                Array.Resize<WorldObject>(ref GameObjectManager.AnimatedWorldObjects, GameObjectManager.AnimatedWorldObjects.Length << 1);
            }
            Transformation FinalTransformation = new Transformation(BaseTransformation, AuxTransformation);
	        var o = new AnimatedWorldObject
	        {
		        Position = Position,
		        Direction = FinalTransformation.Z,
		        Up = FinalTransformation.Y,
		        Side = FinalTransformation.X,
		        Object = Prototype.Clone()
	        };
	        o.Object.ObjectIndex = CreateDynamicObject();
            o.SectionIndex = SectionIndex;
            o.TrackPosition = TrackPosition;
            for (int i = 0; i < o.Object.States.Length; i++)
            {
                if (o.Object.States[i].Object == null)
                {
                    o.Object.States[i].Object = new StaticObject(Program.CurrentHost);
                    o.Object.States[i].Object.Mesh.Faces = new MeshFace[] { };
                    o.Object.States[i].Object.Mesh.Materials = new MeshMaterial[] { };
                    o.Object.States[i].Object.Mesh.Vertices = new VertexTemplate[] { };
                    o.Object.States[i].Object.RendererIndex = -1;
                }
            }
            double r = 0.0;
            for (int i = 0; i < o.Object.States.Length; i++)
            {
                for (int j = 0; j < o.Object.States[i].Object.Mesh.Materials.Length; j++)
                {
                    o.Object.States[i].Object.Mesh.Materials[j].Color.R = (byte)Math.Round((double)Prototype.States[i].Object.Mesh.Materials[j].Color.R * Brightness);
                    o.Object.States[i].Object.Mesh.Materials[j].Color.G = (byte)Math.Round((double)Prototype.States[i].Object.Mesh.Materials[j].Color.G * Brightness);
                    o.Object.States[i].Object.Mesh.Materials[j].Color.B = (byte)Math.Round((double)Prototype.States[i].Object.Mesh.Materials[j].Color.B * Brightness);
                }
                for (int j = 0; j < o.Object.States[i].Object.Mesh.Vertices.Length; j++)
                {
                    double x = Prototype.States[i].Object.Mesh.Vertices[j].Coordinates.X;
                    double y = Prototype.States[i].Object.Mesh.Vertices[j].Coordinates.Y;
                    double z = Prototype.States[i].Object.Mesh.Vertices[j].Coordinates.Z;
                    double t = x * x + y * y + z * z;
                    if (t > r) r = t;
                }
            }
            o.Radius = Math.Sqrt(r);
            o.Visible = false;
			o.Object.Initialize(0, false, false);
	        GameObjectManager.AnimatedWorldObjects[a] = o;
            GameObjectManager.AnimatedWorldObjectsUsed++;
            return a;
        }

		// load object
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
					case ".l3dobj":
						Result = Ls3DObjectParser.ReadObject(FileName, LoadMode, new Vector3());
						break;
					case ".l3dgrp":
						Result = Ls3DGrpParser.ReadObject(FileName, Encoding, LoadMode);
						break;
					case ".obj":
						Result = WavefrontObjParser.ReadObject(FileName, Encoding, LoadMode, ForceTextureRepeatX, ForceTextureRepeatY);
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
						Interface.AddMessage(MessageType.Error, false, "Tried to load an animated object even though only static objects are allowed: " + FileName);
						return null;
					case ".obj":
						Result = WavefrontObjParser.ReadObject(FileName, Encoding, LoadMode, ForceTextureRepeatX, ForceTextureRepeatY);
						break;
				default:
						Interface.AddMessage(MessageType.Error, false, "The file extension is not supported: " + FileName);
						return null;
				}

			if (Result != null)
			{
				Result.OptimizeObject(PreserveVertices);
			}
				
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
			CreateObject(Prototype, Position, BaseTransformation, AuxTransformation, -1, AccurateObjectDisposal, StartingDistance, EndingDistance, BlockLength, TrackPosition, 1.0, false);
		}
		internal static void CreateObject(UnifiedObject Prototype, Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, int SectionIndex, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness, bool DuplicateMaterials) {
			if (Prototype is StaticObject) {
				StaticObject s = (StaticObject)Prototype;
				GameObjectManager.CreateStaticObject(s, Position, BaseTransformation, AuxTransformation, AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness, DuplicateMaterials, Program.CurrentHost, 600);
			} else if (Prototype is AnimatedObjectCollection) {
				AnimatedObjectCollection a = (AnimatedObjectCollection)Prototype;
				CreateAnimatedWorldObjects(a.Objects, Position, BaseTransformation, AuxTransformation, SectionIndex, AccurateObjectDisposal, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness, DuplicateMaterials);
			}
		}

		internal static void ApplyStaticObjectData(ref StaticObject Object, StaticObject Prototype, Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, bool AccurateObjectDisposal, double AccurateObjectDisposalZOffset, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness, bool DuplicateMaterials) {
			Object = new StaticObject(Program.CurrentHost);
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
					Object.GroupIndex = (short)Mod(Math.Floor(Object.StartingDistance / BlockLength), Math.Ceiling(OpenBveShared.Renderer.BackgroundImageDistance / BlockLength));
				}
			}
		}
		
		private static double Mod(double a, double b) {
			return a - b * Math.Floor(a / b);
		}

		// create dynamic object
		internal static int CreateDynamicObject() {
			int a = GameObjectManager.ObjectsUsed;
			if (a >= GameObjectManager.Objects.Length) {
				Array.Resize<StaticObject>(ref GameObjectManager.Objects, GameObjectManager.Objects.Length << 1);
			}
			GameObjectManager.Objects[a] = new StaticObject(Program.CurrentHost);
			GameObjectManager.Objects[a].Mesh.Faces = new MeshFace[] { };
			GameObjectManager.Objects[a].Mesh.Materials = new MeshMaterial[] { };
			GameObjectManager.Objects[a].Mesh.Vertices = new VertexTemplate[] { };
			GameObjectManager.Objects[a].Dynamic = true;
			GameObjectManager.ObjectsUsed++;
			return a;
		}
		
		// finish creating objects
		internal static void FinishCreatingObjects() {
			Array.Resize<StaticObject>(ref GameObjectManager.Objects, GameObjectManager.ObjectsUsed);
			Array.Resize<WorldObject>(ref GameObjectManager.AnimatedWorldObjects, GameObjectManager.AnimatedWorldObjectsUsed);
		}

		// initialize visibility
		internal static void InitializeVisibility() {
			// sort objects
			GameObjectManager.ObjectsSortedByStart = new int[GameObjectManager.ObjectsUsed];
			GameObjectManager.ObjectsSortedByEnd = new int[GameObjectManager.ObjectsUsed];
			double[] a = new double[GameObjectManager.ObjectsUsed];
			double[] b = new double[GameObjectManager.ObjectsUsed];
			int n = 0;
			for (int i = 0; i < GameObjectManager.ObjectsUsed; i++) {
				if (!GameObjectManager.Objects[i].Dynamic) {
					GameObjectManager.ObjectsSortedByStart[n] = i;
					GameObjectManager.ObjectsSortedByEnd[n] = i;
					a[n] = GameObjectManager.Objects[i].StartingDistance;
					b[n] = GameObjectManager.Objects[i].EndingDistance;
					n++;
				}
			}
			Array.Resize<int>(ref GameObjectManager.ObjectsSortedByStart, n);
			Array.Resize<int>(ref GameObjectManager.ObjectsSortedByEnd, n);
			Array.Resize<double>(ref a, n);
			Array.Resize<double>(ref b, n);
			Array.Sort<double, int>(a, GameObjectManager.ObjectsSortedByStart);
			Array.Sort<double, int>(b, GameObjectManager.ObjectsSortedByEnd);
			GameObjectManager.ObjectsSortedByStartPointer = 0;
			GameObjectManager.ObjectsSortedByEndPointer = 0;
			// initial visiblity
			double p = World.CameraTrackFollower.TrackPosition + OpenBveShared.Camera.CameraCurrentAlignment.Position.Z;
			for (int i = 0; i < GameObjectManager.ObjectsUsed; i++) {
				if (!GameObjectManager.Objects[i].Dynamic) {
					if (GameObjectManager.Objects[i].StartingDistance <= p + OpenBveShared.World.ForwardViewingDistance & GameObjectManager.Objects[i].EndingDistance >= p - OpenBveShared.World.BackwardViewingDistance) {
						OpenBveShared.Renderer.ShowObject(i, ObjectType.Static, Interface.CurrentOptions.TransparencyMode);
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
		/// <summary>Is called to update the visibility of the camera</summary>
		/// <param name="TrackPosition">The camera's track position</param>
		internal static void UpdateVisibility(double TrackPosition)
		{
			double d = TrackPosition - GameObjectManager.LastUpdatedTrackPosition;
			int n = GameObjectManager.ObjectsSortedByStart.Length;
			double p = World.CameraTrackFollower.TrackPosition + OpenBveShared.Camera.CameraCurrentAlignment.Position.Z;
			if (d < 0.0)
			{
				if (GameObjectManager.ObjectsSortedByStartPointer >= n) GameObjectManager.ObjectsSortedByStartPointer = n - 1;
				if (GameObjectManager.ObjectsSortedByEndPointer >= n) GameObjectManager.ObjectsSortedByEndPointer = n - 1;
				// dispose
				while (GameObjectManager.ObjectsSortedByStartPointer >= 0)
				{
					int o = GameObjectManager.ObjectsSortedByStart[GameObjectManager.ObjectsSortedByStartPointer];
					if (GameObjectManager.Objects[o].StartingDistance > p + OpenBveShared.World.ForwardViewingDistance)
					{
						OpenBveShared.Renderer.HideObject(o);
						GameObjectManager.ObjectsSortedByStartPointer--;
					}
					else
					{
						break;
					}
				}
				// introduce
				while (GameObjectManager.ObjectsSortedByEndPointer >= 0)
				{
					int o = GameObjectManager.ObjectsSortedByEnd[GameObjectManager.ObjectsSortedByEndPointer];
					if (GameObjectManager.Objects[o].EndingDistance >= p - OpenBveShared.World.BackwardViewingDistance)
					{
						if (GameObjectManager.Objects[o].StartingDistance <= p + OpenBveShared.World.ForwardViewingDistance)
						{
							OpenBveShared.Renderer.ShowObject(o, ObjectType.Static, Interface.CurrentOptions.TransparencyMode);
						}
						GameObjectManager.ObjectsSortedByEndPointer--;
					}
					else
					{
						break;
					}
				}
			}
			else if (d > 0.0)
			{
				if (GameObjectManager.ObjectsSortedByStartPointer < 0) GameObjectManager.ObjectsSortedByStartPointer = 0;
				if (GameObjectManager.ObjectsSortedByEndPointer < 0) GameObjectManager.ObjectsSortedByEndPointer = 0;
				// dispose
				while (GameObjectManager.ObjectsSortedByEndPointer < n)
				{
					int o = GameObjectManager.ObjectsSortedByEnd[GameObjectManager.ObjectsSortedByEndPointer];
					if (GameObjectManager.Objects[o].EndingDistance < p - OpenBveShared.World.BackwardViewingDistance)
					{
						OpenBveShared.Renderer.HideObject(o);
						GameObjectManager.ObjectsSortedByEndPointer++;
					}
					else
					{
						break;
					}
				}
				// introduce
				while (GameObjectManager.ObjectsSortedByStartPointer < n)
				{
					int o = GameObjectManager.ObjectsSortedByStart[GameObjectManager.ObjectsSortedByStartPointer];
					if (GameObjectManager.Objects[o].StartingDistance <= p + OpenBveShared.World.ForwardViewingDistance)
					{
						if (GameObjectManager.Objects[o].EndingDistance >= p - OpenBveShared.World.BackwardViewingDistance)
						{
							OpenBveShared.Renderer.ShowObject(o, ObjectType.Static, Interface.CurrentOptions.TransparencyMode);
						}
						GameObjectManager.ObjectsSortedByStartPointer++;
					}
					else
					{
						break;
					}
				}
			}
			GameObjectManager.LastUpdatedTrackPosition = TrackPosition;
		}

	}
}
