using System;
using System.Collections.Generic;
using System.Linq;
using OpenBveApi.Interface;
using OpenBveApi.Objects;
using OpenBveApi.Runtime;
using OpenBveApi.Textures;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OpenBve
{
	internal static partial class Renderer
	{
		/// <summary>The list of touch element's faces to be rendered.</summary>
		private static ObjectList Touch = new ObjectList();

		private static int PrePickedObjectIndex = -1;

		internal static bool DebugTouchMode = false;

		/// <summary>Makes an object visible within the world for selection</summary>
		/// <param name="ObjectIndex">The object's index</param>
		private static void ShowObjectSelection(int ObjectIndex)
		{
			if (ObjectManager.Objects[ObjectIndex] == null)
			{
				return;
			}
			if (ObjectManager.Objects[ObjectIndex].RendererIndex == 0)
			{
				if (ObjectCount >= Objects.Length)
				{
					Array.Resize(ref Objects, Objects.Length << 1);
				}
				Objects[ObjectCount].ObjectIndex = ObjectIndex;
				Objects[ObjectCount].Type = ObjectType.Overlay;
				int f = ObjectManager.Objects[ObjectIndex].Mesh.Faces.Length;
				Objects[ObjectCount].FaceListReferences = new ObjectListReference[f];
				for (int i = 0; i < f; i++)
				{
					int k = ObjectManager.Objects[ObjectIndex].Mesh.Faces[i].Material;
					OpenGlTextureWrapMode wrap = OpenGlTextureWrapMode.ClampClamp;
					if (Touch.FaceCount == Touch.Faces.Length)
					{
						Array.Resize(ref Touch.Faces, Touch.Faces.Length << 1);
					}
					Touch.Faces[Touch.FaceCount] = new ObjectFace
					{
						ObjectListIndex = ObjectCount,
						ObjectIndex = ObjectIndex,
						FaceIndex = i,
						Wrap = wrap
					};

					// HACK: Let's store the wrapping mode.

					Objects[ObjectCount].FaceListReferences[i] = new ObjectListReference(ObjectListType.Touch, Touch.FaceCount);
					Touch.FaceCount++;
				}
				ObjectManager.Objects[ObjectIndex].RendererIndex = ObjectCount + 1;
				ObjectCount++;
			}
		}

		/// <summary>Hides an object within the world for selection</summary>
		/// <param name="ObjectIndex">The object's index</param>
		private static void HideObjectSelection(int ObjectIndex)
		{
			if (ObjectManager.Objects[ObjectIndex] == null)
			{
				return;
			}
			int k = ObjectManager.Objects[ObjectIndex].RendererIndex - 1;
			if (k >= 0)
			{
				// remove faces
				for (int i = 0; i < Objects[k].FaceListReferences.Length; i++)
				{
					ObjectListType listType = Objects[k].FaceListReferences[i].Type;
					/*
					 * For all other kinds of faces, move the last face into place
					 * of the face to be removed and decrement the face counter.
					 * */
					ObjectList list;
					switch (listType)
					{
						case ObjectListType.Touch:
							list = Touch;
							break;
						default:
							throw new InvalidOperationException();
					}
					int listIndex = Objects[k].FaceListReferences[i].Index;
					list.Faces[listIndex] = list.Faces[list.FaceCount - 1];
					Objects[list.Faces[listIndex].ObjectListIndex].FaceListReferences[list.Faces[listIndex].FaceIndex].Index = listIndex;
					list.FaceCount--;
				}
				// remove object
				if (k == ObjectCount - 1)
				{
					ObjectCount--;
				}
				else
				{
					Objects[k] = Objects[ObjectCount - 1];
					ObjectCount--;
					for (int i = 0; i < Objects[k].FaceListReferences.Length; i++)
					{
						ObjectListType listType = Objects[k].FaceListReferences[i].Type;
						ObjectList list;
						switch (listType)
						{
							case ObjectListType.StaticOpaque:
								{
									int groupIndex = (int)ObjectManager.Objects[Objects[k].ObjectIndex].GroupIndex;
									list = StaticOpaque[groupIndex].List;
								}
								break;
							case ObjectListType.DynamicOpaque:
								list = DynamicOpaque;
								break;
							case ObjectListType.DynamicAlpha:
								list = DynamicAlpha;
								break;
							case ObjectListType.OverlayOpaque:
								list = OverlayOpaque;
								break;
							case ObjectListType.OverlayAlpha:
								list = OverlayAlpha;
								break;
							case ObjectListType.Touch:
								list = Touch;
								break;
							default:
								throw new InvalidOperationException();
						}
						int listIndex = Objects[k].FaceListReferences[i].Index;
						list.Faces[listIndex].ObjectListIndex = k;
					}
					ObjectManager.Objects[Objects[k].ObjectIndex].RendererIndex = k + 1;
				}
				ObjectManager.Objects[ObjectIndex].RendererIndex = 0;
			}
		}

		/// <summary>Updates the openGL viewport for selection</summary>
		/// <param name="Point">Center of picking area at window coordinates</param>
		/// <param name="Delta">Width and height of picking area in window coordinates</param>
		private static void UpdateViewportSelection(Vector2 Point, Vector2 Delta)
		{
			CurrentViewPortMode = ViewPortMode.Cab;
			int[] Viewport = new int[] { 0, 0, Screen.Width, Screen.Height };
			GL.Viewport(Viewport[0], Viewport[1], Viewport[2], Viewport[3]);
			World.AspectRatio = (double)Screen.Width / (double)Screen.Height;
			World.HorizontalViewingAngle = 2.0 * Math.Atan(Math.Tan(0.5 * World.VerticalViewingAngle) * World.AspectRatio);
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			PickMatrix(new Vector2(Point.X, Viewport[3] - Point.Y), Delta, Viewport);
			Matrix4d perspective = Matrix4d.Perspective(World.VerticalViewingAngle, -World.AspectRatio, 0.025, 50.0);
			GL.MultMatrix(ref perspective);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadIdentity();
		}

		/// <summary>Make a projection matrix that can be used to limit drawing to small areas of the viewport.</summary>
		/// <param name="Point">Center of picking area at window coordinates</param>
		/// <param name="Delta">Width and height of picking area in window coordinates</param>
		/// <param name="Viewport">Current viewport</param>
		private static void PickMatrix(Vector2 Point, Vector2 Delta, int[] Viewport)
		{
			if (Delta.X <= 0 || Delta.Y <= 0)
			{
				return;
			}

			GL.Translate((Viewport[2] - 2 * (Point.X - Viewport[0])) / Delta.X, (Viewport[3] - 2 * (Point.Y - Viewport[1])) / Delta.Y, 0);
			GL.Scale(Viewport[2] / Delta.X, Viewport[3] / Delta.Y, 1.0);
		}

		/// <summary>Render scene for selection</summary>
		private static void RenderSceneSelection(bool IsDebugTouchMode = false)
		{
			// initialize
			GL.InitNames();
			GL.PushName(0);
			int PartId = 0;

			// set up camera
			double dx = World.AbsoluteCameraDirection.X;
			double dy = World.AbsoluteCameraDirection.Y;
			double dz = World.AbsoluteCameraDirection.Z;
			double ux = World.AbsoluteCameraUp.X;
			double uy = World.AbsoluteCameraUp.Y;
			double uz = World.AbsoluteCameraUp.Z;
			Matrix4d LookAt = Matrix4d.LookAt(0.0, 0.0, 0.0, dx, dy, dz, ux, uy, uz);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadMatrix(ref LookAt);

			if (LightingEnabled)
			{
				GL.Disable(EnableCap.Lighting);
				LightingEnabled = false; // TODO: was 'true' before
			}
			OptionLighting = false;
			if (!BlendEnabled)
			{
				GL.Enable(EnableCap.Blend);
				BlendEnabled = true;
			}
			GL.DepthMask(false);
			GL.Disable(EnableCap.DepthTest);
			UnsetAlphaFunc();
			Touch.SortPolygons();
			for (int i = 0; i < Touch.FaceCount; i++)
			{
				GL.LoadName(PartId);
				RenderFace(ref Touch.Faces[i], World.AbsoluteCameraPosition, IsDebugTouchMode);
				PartId++;
			}

			GL.PopName();
			OptionLighting = true;
		}

		/// <summary>Function to perform start processing of mouse picking.</summary>
		/// <param name="SelectBuffer">Selection buffer</param>
		/// <param name="Point">Coordinates of the mouse</param>
		/// <param name="Delta">Width and height of picking area in window coordinates</param>
		private static void PickPre(int[] SelectBuffer, Vector2 Point, Vector2 Delta)
		{
			GL.SelectBuffer(SelectBuffer.Length, SelectBuffer);
			GL.RenderMode(RenderingMode.Select);

			ResetOpenGlState();
			GL.PushMatrix();

			UpdateViewportSelection(Point, Delta);
		}

		private struct PickedObject
		{
			internal int NameDepth;
			internal int[] Names;
			internal double MinDepth;
			internal double MaxDepth;
		}

		private static List<PickedObject> ParseSelectBuffer(int[] SelectBuffer)
		{
			List<PickedObject> PickedObjects = new List<PickedObject>();
			int Position = 0;

			try
			{
				while (Position < SelectBuffer.Length)
				{
					if (SelectBuffer[Position] == 0)
					{
						break;
					}
					PickedObject Object = new PickedObject();
					Object.NameDepth = SelectBuffer[Position++];
					Object.MinDepth = (double)SelectBuffer[Position++] / int.MaxValue;
					Object.MaxDepth = (double)SelectBuffer[Position++] / int.MaxValue;
					Object.Names = new int[Object.NameDepth];
					for (int i = 0; i < Object.NameDepth; i++)
					{
						Object.Names[i] = SelectBuffer[Position++];
					}
					PickedObjects.Add(Object);
				}
				return PickedObjects;
			}
			catch (IndexOutOfRangeException)
			{
				if (Position >= SelectBuffer.Length)
				{
					return PickedObjects;
				}
				throw;
			}

		}

		/// <summary>Function to perform the end processing of mouse picking.</summary>
		/// <param name="SelectBuffer">Selection buffer</param>
		/// <returns>The object index of the object hit in the shallowest place.</returns>
		private static int PickPost(int[] SelectBuffer)
		{
			int Hits = GL.RenderMode(RenderingMode.Render);
			GL.PopMatrix();

			if (Hits <= 0)
			{
				return -1;
			}

			List<PickedObject> PickedObjects = ParseSelectBuffer(SelectBuffer);

			if (PickedObjects.Any())
			{
				PickedObjects = PickedObjects.OrderBy(x => x.MinDepth).ToList();
				return Touch.Faces[PickedObjects[0].Names[0]].ObjectIndex;
			}

			return -1;
		}

		internal static void DebugTouchArea()
		{
			if (!Loading.SimulationSetup)
			{
				return;
			}
			
			if (World.CameraMode != CameraViewMode.Interior && World.CameraMode != CameraViewMode.InteriorLookAhead)
			{
				return;
			}

			TrainManager.Car Car = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar];
			int add = Car.CarSections[0].CurrentAdditionalGroup + 1;
			if (add < Car.CarSections[0].Groups.Length)
			{
				TrainManager.TouchElement[] TouchElements = Car.CarSections[0].Groups[add].TouchElements;

				if (TouchElements != null)
				{
					foreach (var TouchElement in TouchElements)
					{
						int o = TouchElement.Element.ObjectIndex;
						ShowObjectSelection(o);
					}

					ResetOpenGlState();
					GL.PushMatrix();
					
					UpdateViewport(ViewPortChangeMode.ChangeToCab);

					RenderSceneSelection(true);

					GL.PopMatrix();
					
					foreach (var TouchElement in TouchElements)
					{
						int o = TouchElement.Element.ObjectIndex;
						HideObjectSelection(o);
					}
				}
			}
		}

		internal static bool MoveCheck(Vector2 Point, out Cursor.Status Status)
		{
			if (!Loading.SimulationSetup)
			{
				Status = Cursor.Status.Default;
				return false;
			}

			if (World.CameraMode != CameraViewMode.Interior && World.CameraMode != CameraViewMode.InteriorLookAhead)
			{
				Status = Cursor.Status.Default;
				return false;
			}

			Status = Cursor.Status.Default;

			TrainManager.Car Car = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar];
			int add = Car.CarSections[0].CurrentAdditionalGroup + 1;
			if (add < Car.CarSections[0].Groups.Length)
			{
				TrainManager.TouchElement[] TouchElements = Car.CarSections[0].Groups[add].TouchElements;

				if (TouchElements != null)
				{
					foreach (var TouchElement in TouchElements)
					{
						int o = TouchElement.Element.ObjectIndex;
						ShowObjectSelection(o);
					}

					int[] SelectBuffer = new int[2048];

					PickPre(SelectBuffer, Point, new Vector2(5));

					RenderSceneSelection();

					int PickedObjectIndex = PickPost(SelectBuffer);

					foreach (var TouchElement in TouchElements)
					{
						int o = TouchElement.Element.ObjectIndex;
						HideObjectSelection(o);

						if (o == PickedObjectIndex)
						{
							switch (TouchElement.Command)
							{
								case Translations.Command.PowerIncrease:
								case Translations.Command.BrakeIncrease:
								case Translations.Command.ReverserForward:
									Status = Cursor.Status.Plus;
									break;
								case Translations.Command.PowerDecrease:
								case Translations.Command.BrakeDecrease:
								case Translations.Command.ReverserBackward:
									Status = Cursor.Status.Minus;
									break;
							}
						}
					}
					
					if (PickedObjectIndex >= 0)
					{
						return true;
					}
				}
			}
			return false;
		}

		internal static void TouchCheck(Vector2 Point)
		{
			if (!Loading.SimulationSetup)
			{
				return;
			}
			
			if (World.CameraMode != CameraViewMode.Interior && World.CameraMode != CameraViewMode.InteriorLookAhead)
			{
				return;
			}

			TrainManager.Car Car = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar];
			int add = Car.CarSections[0].CurrentAdditionalGroup + 1;
			if (add < Car.CarSections[0].Groups.Length)
			{
				TrainManager.TouchElement[] TouchElements = Car.CarSections[0].Groups[add].TouchElements;

				if (TouchElements != null)
				{
					foreach (var TouchElement in TouchElements)
					{
						int o = TouchElement.Element.ObjectIndex;
						ShowObjectSelection(o);
					}

					int[] SelectBuffer = new int[2048];

					PickPre(SelectBuffer, Point, new Vector2(5));

					RenderSceneSelection();

					int PickedObjectIndex = PickPost(SelectBuffer);

					foreach (var TouchElement in TouchElements)
					{
						int o = TouchElement.Element.ObjectIndex;
						HideObjectSelection(o);
						if (o == PickedObjectIndex)
						{
							for (int i = 0; i < Interface.CurrentControls.Length; i++)
							{
								if (Interface.CurrentControls[i].Method != Interface.ControlMethod.Touch)
								{
									continue;
								}
								bool EnableOption = false;
								for (int j = 0; j < Translations.CommandInfos.Length; j++)
								{
									if (Interface.CurrentControls[i].Command == Translations.CommandInfos[j].Command)
									{
										EnableOption = Translations.CommandInfos[j].EnableOption;
										break;
									}
								}
								if (TouchElement.Command == Interface.CurrentControls[i].Command)
								{
									if (EnableOption && TouchElement.CommandOption != Interface.CurrentControls[i].Option)
									{
										continue;
									}
									Interface.CurrentControls[i].AnalogState = 1.0;
									Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.Pressed;
									MainLoop.AddControlRepeat(i);
								}
							}
						}
					}

					PrePickedObjectIndex = PickedObjectIndex;
				}
			}
		}

		internal static void LeaveCheck(Vector2 Point)
		{
			if (!Loading.SimulationSetup)
			{
				return;
			}

			if (World.CameraMode != CameraViewMode.Interior && World.CameraMode != CameraViewMode.InteriorLookAhead)
			{
				return;
			}

			TrainManager.Car Car = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar];
			int add = Car.CarSections[0].CurrentAdditionalGroup + 1;
			if (add < Car.CarSections[0].Groups.Length)
			{
				TrainManager.TouchElement[] TouchElements = Car.CarSections[0].Groups[add].TouchElements;

				if (TouchElements != null)
				{
					foreach (var TouchElement in TouchElements)
					{
						int o = TouchElement.Element.ObjectIndex;
						ShowObjectSelection(o);
					}

					int[] SelectBuffer = new int[2048];

					PickPre(SelectBuffer, Point, new Vector2(5));

					RenderSceneSelection();

					int PickedObjectIndex = PickPost(SelectBuffer);

					foreach (var TouchElement in TouchElements)
					{
						int o = TouchElement.Element.ObjectIndex;
						HideObjectSelection(o);
						if (o == PickedObjectIndex)
						{
							Car.CarSections[0].CurrentAdditionalGroup = TouchElement.JumpScreenIndex;
							Car.ChangeCarSection(TrainManager.CarSectionType.Interior);
							if (TouchElement.SoundIndex >= 0 && TouchElement.SoundIndex < Car.Sounds.Touch.Length)
							{
								Sounds.SoundBuffer Buffer = Car.Sounds.Touch[TouchElement.SoundIndex].Buffer;
								OpenBveApi.Math.Vector3 Position = Car.Sounds.Touch[TouchElement.SoundIndex].Position;
								Sounds.PlaySound(Buffer, 1.0, 1.0, Position, TrainManager.PlayerTrain, TrainManager.PlayerTrain.DriverCar, false);
							}
						}

						// HACK: Normally terminate the command issued once.
						if (o == PickedObjectIndex || (PickedObjectIndex != PrePickedObjectIndex && o == PrePickedObjectIndex))
						{
							for (int i = 0; i < Interface.CurrentControls.Length; i++)
							{
								if (Interface.CurrentControls[i].Method != Interface.ControlMethod.Touch)
								{
									continue;
								}
								bool EnableOption = false;
								for (int j = 0; j < Translations.CommandInfos.Length; j++)
								{
									if (Interface.CurrentControls[i].Command == Translations.CommandInfos[j].Command)
									{
										EnableOption = Translations.CommandInfos[j].EnableOption;
										break;
									}
								}
								if (TouchElement.Command == Interface.CurrentControls[i].Command)
								{
									if (EnableOption && TouchElement.CommandOption != Interface.CurrentControls[i].Option)
									{
										continue;
									}
									Interface.CurrentControls[i].AnalogState = 0.0;
									Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.Released;
									MainLoop.RemoveControlRepeat(i);
								}
							}
						}
					}
				}
			}
		}
	}
}
