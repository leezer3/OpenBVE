using System;
using System.Collections.Generic;
using System.Linq;
using LibRender;
using OpenBveApi.Interface;
using OpenBveApi.Objects;
using OpenBveApi.Runtime;
using OpenBveApi.Textures;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SoundManager;
using static LibRender.CameraProperties;

namespace OpenBve
{
	internal static partial class Renderer
	{
		/// <summary>The list of touch element's faces to be rendered.</summary>
		private static ObjectList Touch = new ObjectList();

		private static StaticObject PrePickedObjectIndex = null;

		internal static bool DebugTouchMode = false;

		/// <summary>Makes an object visible within the world for selection</summary>
		/// <param name="ObjectToShow">The object's index</param>
		private static void ShowObjectSelection(StaticObject ObjectToShow)
		{
			if (ObjectToShow == null)
			{
				return;
			}
			if (ObjectToShow.RendererIndex == 0)
			{
				if (LibRender.Renderer.ObjectCount >= LibRender.Renderer.Objects.Length)
				{
					Array.Resize(ref LibRender.Renderer.Objects, LibRender.Renderer.Objects.Length << 1);
				}
				LibRender.Renderer.Objects[LibRender.Renderer.ObjectCount] = new RendererObject(ObjectToShow, ObjectType.Overlay);
				int f = ObjectToShow.Mesh.Faces.Length;
				LibRender.Renderer.Objects[LibRender.Renderer.ObjectCount].FaceListReferences = new ObjectListReference[f];
				for (int i = 0; i < f; i++)
				{
					OpenGlTextureWrapMode wrap = OpenGlTextureWrapMode.ClampClamp;
					if (Touch.FaceCount == Touch.Faces.Length)
					{
						Array.Resize(ref Touch.Faces, Touch.Faces.Length << 1);
					}
					Touch.Faces[Touch.FaceCount] = new ObjectFace
					{
						ObjectListIndex = LibRender.Renderer.ObjectCount,
						ObjectReference = ObjectToShow,
						FaceIndex = i,
						Wrap = wrap
					};

					// HACK: Let's store the wrapping mode.

					LibRender.Renderer.Objects[LibRender.Renderer.ObjectCount].FaceListReferences[i] = new ObjectListReference(ObjectListType.Touch, Touch.FaceCount);
					Touch.FaceCount++;
				}
				ObjectToShow.RendererIndex = LibRender.Renderer.ObjectCount + 1;
				LibRender.Renderer.ObjectCount++;
			}
		}

		/// <summary>Hides an object within the world for selection</summary>
		/// <param name="ObjectIndex">The object's index</param>
		private static void HideObjectSelection(ref StaticObject ObjectIndex)
		{
			if (ObjectIndex == null)
			{
				return;
			}
			int k = ObjectIndex.RendererIndex - 1;
			if (k >= 0)
			{
				// remove faces
				for (int i = 0; i < LibRender.Renderer.Objects[k].FaceListReferences.Length; i++)
				{
					ObjectListType listType = LibRender.Renderer.Objects[k].FaceListReferences[i].Type;
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
					int listIndex = LibRender.Renderer.Objects[k].FaceListReferences[i].Index;
					list.Faces[listIndex] = list.Faces[list.FaceCount - 1];
					LibRender.Renderer.Objects[list.Faces[listIndex].ObjectListIndex].FaceListReferences[list.Faces[listIndex].FaceIndex].Index = listIndex;
					if (list.FaceCount > 0)
					{
						list.FaceCount--;
					}
					
				}
				// remove object
				if (k == LibRender.Renderer.ObjectCount - 1)
				{
					LibRender.Renderer.ObjectCount--;
				}
				else
				{
					LibRender.Renderer.Objects[k] = LibRender.Renderer.Objects[LibRender.Renderer.ObjectCount - 1];
					LibRender.Renderer.ObjectCount--;
					for (int i = 0; i < LibRender.Renderer.Objects[k].FaceListReferences.Length; i++)
					{
						ObjectListType listType = LibRender.Renderer.Objects[k].FaceListReferences[i].Type;
						ObjectList list;
						switch (listType)
						{
							case ObjectListType.StaticOpaque:
								{
									int groupIndex = (int)LibRender.Renderer.Objects[k].InternalObject.GroupIndex;
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
						int listIndex = LibRender.Renderer.Objects[k].FaceListReferences[i].Index;
						list.Faces[listIndex].ObjectListIndex = k;
					}
					LibRender.Renderer.Objects[k].InternalObject.RendererIndex = k + 1;
				}
				ObjectIndex.RendererIndex = 0;
			}
		}

		/// <summary>Updates the openGL viewport for selection</summary>
		/// <param name="Point">Center of picking area at window coordinates</param>
		/// <param name="Delta">Width and height of picking area in window coordinates</param>
		private static void UpdateViewportSelection(Vector2 Point, Vector2 Delta)
		{
			LibRender.Renderer.CurrentViewPortMode = ViewPortMode.Cab;
			int[] Viewport = new int[] { 0, 0, LibRender.Screen.Width, LibRender.Screen.Height };
			GL.Viewport(Viewport[0], Viewport[1], Viewport[2], Viewport[3]);
			LibRender.Screen.AspectRatio = (double)LibRender.Screen.Width / (double)LibRender.Screen.Height;
			Camera.HorizontalViewingAngle = 2.0 * Math.Atan(Math.Tan(0.5 * Camera.VerticalViewingAngle) * LibRender.Screen.AspectRatio);
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			PickMatrix(new Vector2(Point.X, Viewport[3] - Point.Y), Delta, Viewport);
			Matrix4d perspective = Matrix4d.Perspective(Camera.VerticalViewingAngle, -LibRender.Screen.AspectRatio, 0.025, 50.0);
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
			double dx = Camera.AbsoluteDirection.X;
			double dy = Camera.AbsoluteDirection.Y;
			double dz = Camera.AbsoluteDirection.Z;
			double ux = Camera.AbsoluteUp.X;
			double uy = Camera.AbsoluteUp.Y;
			double uz = Camera.AbsoluteUp.Z;
			Matrix4d LookAt = Matrix4d.LookAt(0.0, 0.0, 0.0, dx, dy, dz, ux, uy, uz);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadMatrix(ref LookAt);

			if (LibRender.Renderer.LightingEnabled)
			{
				GL.Disable(EnableCap.Lighting);
				LibRender.Renderer.LightingEnabled = false; // TODO: was 'true' before
			}
			LibRender.Renderer.OptionLighting = false;
			if (!LibRender.Renderer.BlendEnabled)
			{
				GL.Enable(EnableCap.Blend);
				LibRender.Renderer.BlendEnabled = true;
			}
			GL.DepthMask(false);
			GL.Disable(EnableCap.DepthTest);
			LibRender.Renderer.UnsetAlphaFunc();
			Touch.SortPolygons();
			for (int i = 0; i < Touch.FaceCount; i++)
			{
				GL.LoadName(PartId);
				RenderFace(ref Touch.Faces[i], Camera.AbsolutePosition, IsDebugTouchMode);
				PartId++;
			}

			GL.PopName();
			LibRender.Renderer.OptionLighting = true;
		}

		/// <summary>Function to perform start processing of mouse picking.</summary>
		/// <param name="SelectBuffer">Selection buffer</param>
		/// <param name="Point">Coordinates of the mouse</param>
		/// <param name="Delta">Width and height of picking area in window coordinates</param>
		private static void PickPre(int[] SelectBuffer, Vector2 Point, Vector2 Delta)
		{
			GL.SelectBuffer(SelectBuffer.Length, SelectBuffer);
			GL.RenderMode(RenderingMode.Select);

			LibRender.Renderer.ResetOpenGlState();
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
		private static StaticObject PickPost(int[] SelectBuffer)
		{
			int Hits = GL.RenderMode(RenderingMode.Render);
			GL.PopMatrix();

			if (Hits <= 0)
			{
				return null;
			}

			List<PickedObject> PickedObjects = ParseSelectBuffer(SelectBuffer);

			if (PickedObjects.Any())
			{
				PickedObjects = PickedObjects.OrderBy(x => x.MinDepth).ToList();
				return Touch.Faces[PickedObjects[0].Names[0]].ObjectReference;
			}

			return null;
		}

		internal static void DebugTouchArea()
		{
			if (!Loading.SimulationSetup)
			{
				return;
			}
			
			if (Camera.CurrentMode != CameraViewMode.Interior && Camera.CurrentMode != CameraViewMode.InteriorLookAhead)
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
						ShowObjectSelection(TouchElement.Element.internalObject);
					}

					LibRender.Renderer.ResetOpenGlState();
					GL.PushMatrix();
					
					UpdateViewport(ViewPortChangeMode.ChangeToCab);

					RenderSceneSelection(true);

					GL.PopMatrix();
					
					foreach (var TouchElement in TouchElements)
					{
						HideObjectSelection(ref TouchElement.Element.internalObject);
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

			if (Camera.CurrentMode != CameraViewMode.Interior && Camera.CurrentMode != CameraViewMode.InteriorLookAhead)
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
						ShowObjectSelection(TouchElement.Element.internalObject);
					}

					int[] SelectBuffer = new int[2048];

					PickPre(SelectBuffer, Point, new Vector2(5));

					RenderSceneSelection();

					StaticObject PickedObjectIndex = PickPost(SelectBuffer);

					foreach (var TouchElement in TouchElements)
					{
						HideObjectSelection(ref TouchElement.Element.internalObject);

						if (TouchElement.Element.internalObject == PickedObjectIndex)
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
					
					if (PickedObjectIndex != null)
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
			
			if (Camera.CurrentMode != CameraViewMode.Interior && Camera.CurrentMode != CameraViewMode.InteriorLookAhead)
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
						ShowObjectSelection(TouchElement.Element.internalObject);
					}

					int[] SelectBuffer = new int[2048];

					PickPre(SelectBuffer, Point, new Vector2(5));

					RenderSceneSelection();

					StaticObject PickedObjectIndex = PickPost(SelectBuffer);

					foreach (var TouchElement in TouchElements)
					{
						HideObjectSelection(ref TouchElement.Element.internalObject);
						if (TouchElement.Element.internalObject == PickedObjectIndex)
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

			if (Camera.CurrentMode != CameraViewMode.Interior && Camera.CurrentMode != CameraViewMode.InteriorLookAhead)
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
						ShowObjectSelection(TouchElement.Element.internalObject);
					}

					int[] SelectBuffer = new int[2048];

					PickPre(SelectBuffer, Point, new Vector2(5));

					RenderSceneSelection();

					StaticObject PickedObjectIndex = PickPost(SelectBuffer);

					foreach (var TouchElement in TouchElements)
					{
						HideObjectSelection(ref TouchElement.Element.internalObject);
						if (TouchElement.Element.internalObject == PickedObjectIndex)
						{
							Car.CarSections[0].CurrentAdditionalGroup = TouchElement.JumpScreenIndex;
							Car.ChangeCarSection(TrainManager.CarSectionType.Interior);
							if (TouchElement.SoundIndex >= 0 && TouchElement.SoundIndex < Car.Sounds.Touch.Length)
							{
								SoundBuffer Buffer = Car.Sounds.Touch[TouchElement.SoundIndex].Buffer;
								OpenBveApi.Math.Vector3 Position = Car.Sounds.Touch[TouchElement.SoundIndex].Position;
								Program.Sounds.PlaySound(Buffer, 1.0, 1.0, Position, TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar], false);
							}
						}

						// HACK: Normally terminate the command issued once.
						if (TouchElement.Element.internalObject == PickedObjectIndex || (PickedObjectIndex != PrePickedObjectIndex && TouchElement.Element.internalObject == PrePickedObjectIndex))
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
