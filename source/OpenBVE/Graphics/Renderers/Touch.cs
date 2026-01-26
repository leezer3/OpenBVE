using System;
using System.Collections.Generic;
using System.Linq;
using LibRender2;
using LibRender2.Trains;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Runtime;
using OpenTK.Graphics.OpenGL;
using TrainManager.Car;
using Vector2 = OpenTK.Vector2;

namespace OpenBve.Graphics.Renderers
{
	internal class Touch
	{
		private struct PickedObject
		{
			internal int NameDepth;
			internal int[] Names;
			internal double MinDepth;
			internal double MaxDepth;
		}

		private readonly NewRenderer renderer;
		private readonly List<ObjectState> touchableObject;
		private readonly FrameBufferObject fbo;
		
		/// <summary>Stores whether the mouse is currently down</summary>
		private bool mouseCurrentlyDown;
		/// <summary>The object picked on the last touch down event</summary>
		private ObjectState previouslyPickedObject;

		internal Touch(NewRenderer renderer)
		{
			this.renderer = renderer;
			touchableObject = new List<ObjectState>();

			if (renderer.ForceLegacyOpenGL)
			{
				// touch not supported when GL4 is not available
				return;
			}
			fbo = new FrameBufferObject();
			fbo.Bind();
			fbo.SetTextureBuffer(FrameBufferObject.TargetBuffer.Color, PixelInternalFormat.R32f, PixelFormat.Red, PixelType.Float, renderer.Screen.Width, renderer.Screen.Height);
			fbo.DrawBuffers(new[] { DrawBuffersEnum.ColorAttachment0 });
			fbo.UnBind();
		}

		internal void UpdateViewport()
		{
			if (!renderer.AvailableNewRenderer)
			{
				return;
			}
			fbo.Bind();
			fbo.SetTextureBuffer(FrameBufferObject.TargetBuffer.Color, PixelInternalFormat.R32f, PixelFormat.Red, PixelType.Float, renderer.Screen.Width, renderer.Screen.Height);
			fbo.DrawBuffers(new[] { DrawBuffersEnum.ColorAttachment0 });
			fbo.UnBind();
		}

		private void ShowObject(ObjectState state)
		{
			touchableObject.Add(state);

			if (renderer.AvailableNewRenderer && state.Prototype.Mesh.VAO == null)
			{
				VAOExtensions.CreateVAO(state.Prototype.Mesh, state.Prototype.Dynamic, renderer.pickingShader.VertexLayout, renderer);
			}
		}

		private void PreRender()
		{
			touchableObject.Clear();

			if (!Loading.SimulationSetup)
			{
				return;
			}

			CarBase Car = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar];
			if (renderer.Camera.CurrentMode > CameraViewMode.InteriorLookAhead || !Car.CarSections.TryGetValue(CarSectionType.Interior, out CarSection interiorSection))
			{
				return;
			}

			int add = interiorSection.CurrentAdditionalGroup + 1;

			if (add >= interiorSection.Groups.Length)
			{
				return;
			}

			TouchElement[] TouchElements = interiorSection.Groups[add].TouchElements;

			if (TouchElements == null)
			{
				return;
			}

			foreach (TouchElement element in TouchElements)
			{
				ShowObject(element.Element.internalObject);
			}
		}

		internal void RenderScene()
		{
			PreRender();

			if (!touchableObject.Any())
			{
				//Drop out early if the pre-render process reveals no available touch faces for a minor boost
				return;
			}

			renderer.ResetOpenGlState();

			if (renderer.AvailableNewRenderer)
			{
				fbo.Bind();
				GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
				GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
				renderer.pickingShader.Activate();
				renderer.pickingShader.SetCurrentProjectionMatrix(renderer.CurrentProjectionMatrix);

				for (int i = 0; i < touchableObject.Count; i++)
				{
					renderer.pickingShader.SetObjectIndex(i + 1);

					foreach (MeshFace face in touchableObject[i].Prototype.Mesh.Faces)
					{
						renderer.RenderFace(renderer.pickingShader, touchableObject[i], face);
					}
				}

				//Must deactivate and unbind here
				renderer.pickingShader.Deactivate();
				fbo.UnBind();
			}

			// for debug
			if (renderer.DebugTouchMode)
			{
				GL.DepthMask(false);
				GL.Disable(EnableCap.DepthTest);

				if (renderer.AvailableNewRenderer)
				{
					renderer.DefaultShader.Activate();
					renderer.ResetShader(renderer.DefaultShader);
					renderer.DefaultShader.SetCurrentProjectionMatrix(renderer.CurrentProjectionMatrix);

					foreach (ObjectState objectState in touchableObject)
					{
						foreach (MeshFace face in objectState.Prototype.Mesh.Faces)
						{
							renderer.RenderFace(renderer.DefaultShader, objectState, face, true);
						}
					}

					renderer.DefaultShader.Deactivate();
				}
				else
				{
					foreach (ObjectState objectState in touchableObject)
					{
						foreach (MeshFace face in objectState.Prototype.Mesh.Faces)
						{
							renderer.RenderFaceImmediateMode(objectState, face, true);
						}
					}
				}
			}
		}

		private ObjectState ParseFBO(Vector2 point, int deltaX, int deltaY)
		{
			Vector2 topLeft = point - new Vector2(deltaX, deltaY) / 2.0f;
			float[,] objectIndices = new float[deltaX, deltaY];

			fbo.Bind();
			GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
			GL.ReadPixels((int)topLeft.X, renderer.Screen.Height - (int)topLeft.Y, deltaX, deltaY, PixelFormat.Red, PixelType.Float, objectIndices);
			GL.ReadBuffer(ReadBufferMode.None);
			fbo.UnBind();

			foreach (float objectIndex in objectIndices)
			{
				int index = (int)objectIndex - 1;

				if (index >= 0 && index < touchableObject.Count)
				{
					return touchableObject[index];
				}
			}

			return null;
		}

		/// <summary>Make a projection matrix that can be used to limit drawing to small areas of the viewport.</summary>
		/// <param name="point">Center of picking area at window coordinates</param>
		/// <param name="delta">Width and height of picking area in window coordinates</param>
		private Matrix4D CreatePickMatrix(Vector2 point, Vector2 delta)
		{
			if (delta.X <= 0 || delta.Y <= 0)
			{
				return Matrix4D.Identity;
			}

			Matrix4D translateMatrix = Matrix4D.CreateTranslation((renderer.Screen.Width - 2 * point.X) / delta.X, (2 * point.Y - renderer.Screen.Height) / delta.Y, 0);
			Matrix4D scaleMatrix = Matrix4D.Scale(renderer.Screen.Width / delta.X, renderer.Screen.Height / delta.Y, 1.0);

			return renderer.CurrentProjectionMatrix * scaleMatrix * translateMatrix;
		}

		private static List<PickedObject> ParseSelectBuffer(int[] selectBuffer)
		{
			List<PickedObject> pickedObjects = new List<PickedObject>();
			int position = 0;

			try
			{
				while (position < selectBuffer.Length)
				{
					if (selectBuffer[position] == 0)
					{
						break;
					}

					PickedObject pickedObject = new PickedObject
					{
						NameDepth = selectBuffer[position++],
						MinDepth = (double)selectBuffer[position++] / int.MaxValue,
						MaxDepth = (double)selectBuffer[position++] / int.MaxValue
					};
					pickedObject.Names = new int[pickedObject.NameDepth];

					for (int i = 0; i < pickedObject.NameDepth; i++)
					{
						pickedObject.Names[i] = selectBuffer[position++];
					}

					pickedObjects.Add(pickedObject);
				}

				return pickedObjects;
			}
			catch (IndexOutOfRangeException)
			{
				if (position >= selectBuffer.Length)
				{
					return pickedObjects;
				}

				throw;
			}
		}

		private ObjectState RenderSceneSelection(Vector2 point, Vector2 delta)
		{
			// Pre
			PreRender();
			renderer.ResetOpenGlState();
			int[] selectBuffer = new int[2048];
			GL.SelectBuffer(selectBuffer.Length, selectBuffer);
			GL.RenderMode(RenderingMode.Select);
			renderer.PushMatrix(MatrixMode.Projection);
			renderer.CurrentProjectionMatrix = CreatePickMatrix(point, delta);
			int partID = 0;
			GL.InitNames();
			GL.PushName(0);

			// Rendering
			foreach (ObjectState objectState in touchableObject)
			{
				GL.LoadName(partID);

				foreach (MeshFace face in objectState.Prototype.Mesh.Faces)
				{
					renderer.RenderFaceImmediateMode(objectState, face);
				}

				partID++;
			}

			// Post
			GL.PopName();
			renderer.PopMatrix(MatrixMode.Projection);
			int hits = GL.RenderMode(RenderingMode.Render);

			if (hits <= 0)
			{
				return null;
			}

			List<PickedObject> pickedObjects = ParseSelectBuffer(selectBuffer);
			return pickedObjects.Any() ? touchableObject[pickedObjects.OrderBy(x => x.MinDepth).First().Names[0]] : null;
		}

		internal bool MoveCheck(Vector2 Point, out MouseCursor.Status Status, out MouseCursor NewCursor)
		{
			NewCursor = null;
			
			if (!Loading.SimulationSetup || renderer.Camera.CurrentMode != CameraViewMode.Interior && renderer.Camera.CurrentMode != CameraViewMode.InteriorLookAhead)
			{
				Status = MouseCursor.Status.Default;
				return false;
			}

			CarBase Car = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar];

			if (!Car.CarSections.TryGetValue(CarSectionType.Interior, out CarSection interCarSection) || renderer.Camera.CurrentMode != CameraViewMode.Interior && renderer.Camera.CurrentMode != CameraViewMode.InteriorLookAhead)
			{
				Status = MouseCursor.Status.Default;
				return false;
			}

			Status = MouseCursor.Status.Default;

			int add = Car.CarSections[CarSectionType.Interior].CurrentAdditionalGroup + 1;
			
			if (add >= interCarSection.Groups.Length)
			{
				return false;
			}
			
			TouchElement[] TouchElements = interCarSection.Groups[add].TouchElements;

			if (TouchElements == null)
			{
				return false;
			}

			ObjectState pickedObject = renderer.AvailableNewRenderer ? ParseFBO(Point, 5, 5) : RenderSceneSelection(Point, new Vector2(5.0f));

			if (mouseCurrentlyDown && pickedObject != previouslyPickedObject)
			{
				// picked object has changed on move, so issue mouse up event (to clear old), then mouse down (for new)
				LeaveCheck(Point);
				TouchCheck(Point);
			}

			foreach (TouchElement TouchElement in TouchElements.Where(x => x.Element.internalObject == pickedObject))
			{
				if (TouchElement.MouseCursor != null)
				{
					Status = MouseCursor.Status.Default;
					NewCursor = TouchElement.MouseCursor;
					return true;
				}
				foreach (int index in TouchElement.ControlIndices)
				{
					switch (Interface.CurrentControls[index].Command)
					{
						case Translations.Command.PowerIncrease:
						case Translations.Command.BrakeIncrease:
						case Translations.Command.ReverserForward:
							Status = MouseCursor.Status.Plus;
							break;
						case Translations.Command.PowerDecrease:
						case Translations.Command.BrakeDecrease:
						case Translations.Command.ReverserBackward:
							Status = MouseCursor.Status.Minus;
							break;
					}
				}
			}
			return pickedObject != null;
		}

		internal void TouchCheck(Vector2 Point)
		{
			if (!Loading.SimulationSetup)
			{
				return;
			}

			mouseCurrentlyDown = true;

			if (renderer.Camera.CurrentMode != CameraViewMode.Interior && renderer.Camera.CurrentMode != CameraViewMode.InteriorLookAhead)
			{
				return;
			}

			CarBase Car = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar];
			int add = Car.CarSections[CarSectionType.Interior].CurrentAdditionalGroup + 1;

			if (add >= Car.CarSections[CarSectionType.Interior].Groups.Length)
			{
				return;
			}

			TouchElement[] TouchElements = Car.CarSections[CarSectionType.Interior].Groups[add].TouchElements;

			if (TouchElements == null)
			{
				return;
			}

			ObjectState pickedObject = renderer.AvailableNewRenderer ? ParseFBO(Point, 5, 5) : RenderSceneSelection(Point, new Vector2(5.0f));

			foreach (TouchElement TouchElement in TouchElements.Where(x => x.Element.internalObject == pickedObject))
			{
				foreach (int index in TouchElement.ControlIndices)
				{
					if (Interface.CurrentControls[index].DigitalState != DigitalControlState.Pressed && TrainManager.PlayerTrain.Plugin != null)
					{
						TrainManager.PlayerTrain.Plugin.TouchEvent(add, index);
						TrainManager.PlayerTrain.Plugin.TouchEvent(add, Interface.CurrentControls[index].Command);
					}
					Interface.CurrentControls[index].AnalogState = 1.0;
					Interface.CurrentControls[index].DigitalState = DigitalControlState.Pressed;
					MainLoop.AddControlRepeat(index);
				}
			}
			previouslyPickedObject = pickedObject;
		}

		internal void LeaveCheck(Vector2 Point)
		{
			if (!Loading.SimulationSetup)
			{
				return;
			}

			mouseCurrentlyDown = false;

			if (renderer.Camera.CurrentMode != CameraViewMode.Interior && renderer.Camera.CurrentMode != CameraViewMode.InteriorLookAhead)
			{
				return;
			}

			CarBase Car = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar];
			int add = Car.CarSections[CarSectionType.Interior].CurrentAdditionalGroup + 1;
			if (add >= Car.CarSections[CarSectionType.Interior].Groups.Length)
			{
				return;
			}

			TouchElement[] TouchElements = Car.CarSections[CarSectionType.Interior].Groups[add].TouchElements;

			if (TouchElements == null)
			{
				return;
			}

			ObjectState pickedObject = renderer.AvailableNewRenderer ? ParseFBO(Point, 5, 5) : RenderSceneSelection(Point, new Vector2(5.0f));

			foreach (TouchElement TouchElement in TouchElements)
			{
				if (TouchElement.Element.internalObject == pickedObject)
				{
					Car.CarSections[CarSectionType.Interior].CurrentAdditionalGroup = TouchElement.JumpScreenIndex;
					// Force a show / hide of the car sections to ensure that the touch stack is correctly updated
					Car.ChangeCarSection(CarSectionType.Interior, false, true);

					foreach (var index in TouchElement.SoundIndices)
					{
						if (Car.Sounds.Touch.ContainsKey(index))
						{
							Car.Sounds.Touch[index].Play(TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar], false);
						}		
					}
				}

				// HACK: Normally terminate the command issued once.
				if (TouchElement.Element.internalObject == pickedObject || (pickedObject != previouslyPickedObject && TouchElement.Element.internalObject == previouslyPickedObject))
				{
					foreach (int index in TouchElement.ControlIndices)
					{
						Interface.CurrentControls[index].AnalogState = 0.0;
						Interface.CurrentControls[index].DigitalState = DigitalControlState.Released;
						MainLoop.RemoveControlRepeat(index);
					}
				}
			}
		}
	}
}
