using System.Collections.Generic;
using System.Linq;
using LibRender2;
using LibRender2.Objects;
using LibRender2.Shaders;
using OpenBveApi.Interface;
using OpenBveApi.Objects;
using OpenBveApi.Runtime;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SoundManager;

namespace OpenBve.Graphics.Renderers
{
	internal class Touch
	{
		private readonly NewRenderer renderer;
		private readonly List<ObjectState> objectStates;
		private readonly List<FaceState> touchFaces;
		private readonly Shader pickingShader;
		private readonly FrameBufferObject fbo;
		private ObjectState prePickedObject;

		internal Touch(NewRenderer renderer)
		{
			this.renderer = renderer;

			objectStates = new List<ObjectState>();
			touchFaces = new List<FaceState>();
			try
			{
				pickingShader = new Shader("default", "picking", true);
				pickingShader.Activate();
				pickingShader.Deactivate();
			}
			catch
			{
				Interface.AddMessage(MessageType.Error, false, "Initialising the touch shader failed- Falling back to legacy openGL.");
				Interface.CurrentOptions.IsUseNewRenderer = false;
			}
			

			fbo = new FrameBufferObject();
			fbo.Bind();
			fbo.SetTextureBuffer(FrameBufferObject.TargetBuffer.Color, PixelInternalFormat.R32f, PixelFormat.Red, PixelType.Float, renderer.Screen.Width, renderer.Screen.Height);
			fbo.DrawBuffers(new[] { DrawBuffersEnum.ColorAttachment0 });
			fbo.UnBind();
		}

		internal void UpdateViewport()
		{
			fbo.Bind();
			fbo.SetTextureBuffer(FrameBufferObject.TargetBuffer.Color, PixelInternalFormat.R32f, PixelFormat.Red, PixelType.Float, renderer.Screen.Width, renderer.Screen.Height);
			fbo.DrawBuffers(new[] { DrawBuffersEnum.ColorAttachment0 });
			fbo.UnBind();
		}

		internal void RenderScene()
		{
			
			PreRender();
			if (touchFaces.Count == 0)
			{
				//Drop out early if the pre-render process reveals no available touch faces for a minor boost
				return;
			}
			renderer.ResetOpenGlState();

			fbo.Bind();

			GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			pickingShader.Activate();
			pickingShader.SetCurrentProjectionMatrix(renderer.CurrentProjectionMatrix);
			foreach (FaceState face in touchFaces)
			{
				pickingShader.SetObjectIndex(objectStates.IndexOf(face.Object) + 1);
				renderer.RenderFace(pickingShader, face);
			}
			//Must deactivate and unbind here
			pickingShader.Deactivate();
			fbo.UnBind();

			// for debug
			if (renderer.DebugTouchMode)
			{
				GL.DepthMask(false);
				GL.Disable(EnableCap.DepthTest);
				renderer.DefaultShader.Activate();
				renderer.ResetShader(renderer.DefaultShader);
				renderer.DefaultShader.SetCurrentProjectionMatrix(renderer.CurrentProjectionMatrix);
				foreach (FaceState face in touchFaces)
				{
					
					renderer.RenderFace(renderer.DefaultShader, face, true);
					
				}
				renderer.DefaultShader.Deactivate();
			}
		}

		private void PreRender()
		{
			objectStates.Clear();
			touchFaces.Clear();

			if (!Loading.SimulationSetup)
			{
				return;
			}

			if (renderer.Camera.CurrentMode != CameraViewMode.Interior && renderer.Camera.CurrentMode != CameraViewMode.InteriorLookAhead)
			{
				return;
			}

			TrainManager.Car Car = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar];
			int add = Car.CarSections[0].CurrentAdditionalGroup + 1;

			if (add >= Car.CarSections[0].Groups.Length)
			{
				return;
			}

			TrainManager.TouchElement[] TouchElements = Car.CarSections[0].Groups[add].TouchElements;

			if (TouchElements == null)
			{
				return;
			}

			foreach (TrainManager.TouchElement element in TouchElements)
			{
				ShowObject(element.Element.internalObject);
			}
		}

		private void ShowObject(ObjectState state)
		{
			objectStates.Add(state);

			if (state.Prototype.Mesh.VAO == null)
			{
				VAOExtensions.CreateVAO(ref state.Prototype.Mesh, state.Prototype.Dynamic, renderer.DefaultShader.VertexLayout);
			}

			foreach (MeshFace face in state.Prototype.Mesh.Faces)
			{
				touchFaces.Add(new FaceState(state, face));
			}
		}

		private ObjectState ParseFBO(Vector2 point, int deltaX, int deltaY)
		{
			Vector2 topLeft = point - new Vector2(deltaX, deltaY) / 2.0f;
			float[,] objectIndices = new float[deltaX, deltaY];

			fbo.Bind();
			GL.ReadBuffer(ReadBufferMode.ColorAttachment0);

			for (int i = 0; i < deltaX; i++)
			{
				for (int j = 0; j < deltaY; j++)
				{
					GL.ReadPixels((int)topLeft.X + i, renderer.Screen.Height - ((int)topLeft.Y - j), 1, 1, PixelFormat.Red, PixelType.Float, ref objectIndices[i, j]);
				}
			}

			GL.ReadBuffer(ReadBufferMode.None);
			fbo.UnBind();

			foreach (float objectIndex in objectIndices)
			{
				int index = (int)objectIndex - 1;

				if (index >= 0 && index < objectStates.Count)
				{
					return objectStates[index];
				}
			}

			return null;
		}

		internal bool MoveCheck(Vector2 Point, out Cursor.Status Status)
		{
			if (!Loading.SimulationSetup)
			{
				Status = Cursor.Status.Default;
				return false;
			}

			if (renderer.Camera.CurrentMode != CameraViewMode.Interior && renderer.Camera.CurrentMode != CameraViewMode.InteriorLookAhead)
			{
				Status = Cursor.Status.Default;
				return false;
			}

			Status = Cursor.Status.Default;

			TrainManager.Car Car = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar];
			int add = Car.CarSections[0].CurrentAdditionalGroup + 1;

			if (add >= Car.CarSections[0].Groups.Length)
			{
				return false;
			}

			TrainManager.TouchElement[] TouchElements = Car.CarSections[0].Groups[add].TouchElements;

			if (TouchElements == null)
			{
				return false;
			}

			ObjectState pickedObject = ParseFBO(Point, 5, 5);

			foreach (TrainManager.TouchElement TouchElement in TouchElements.Where(x => x.Element.internalObject == pickedObject))
			{
				foreach (int index in TouchElement.ControlIndices)
				{
					switch (Interface.CurrentControls[index].Command)
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

			return pickedObject != null;
		}

		internal void TouchCheck(Vector2 Point)
		{
			if (!Loading.SimulationSetup)
			{
				return;
			}

			if (renderer.Camera.CurrentMode != CameraViewMode.Interior && renderer.Camera.CurrentMode != CameraViewMode.InteriorLookAhead)
			{
				return;
			}

			TrainManager.Car Car = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar];
			int add = Car.CarSections[0].CurrentAdditionalGroup + 1;

			if (add >= Car.CarSections[0].Groups.Length)
			{
				return;
			}

			TrainManager.TouchElement[] TouchElements = Car.CarSections[0].Groups[add].TouchElements;

			if (TouchElements == null)
			{
				return;
			}

			ObjectState pickedObject = ParseFBO(Point, 5, 5);

			foreach (TrainManager.TouchElement TouchElement in TouchElements.Where(x => x.Element.internalObject == pickedObject))
			{
				foreach (int index in TouchElement.ControlIndices)
				{
					Interface.CurrentControls[index].AnalogState = 1.0;
					Interface.CurrentControls[index].DigitalState = Interface.DigitalControlState.Pressed;
					MainLoop.AddControlRepeat(index);
				}
			}

			prePickedObject = pickedObject;
		}

		internal void LeaveCheck(Vector2 Point)
		{
			if (!Loading.SimulationSetup)
			{
				return;
			}

			if (renderer.Camera.CurrentMode != CameraViewMode.Interior && renderer.Camera.CurrentMode != CameraViewMode.InteriorLookAhead)
			{
				return;
			}

			TrainManager.Car Car = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar];
			int add = Car.CarSections[0].CurrentAdditionalGroup + 1;
			if (add >= Car.CarSections[0].Groups.Length)
			{
				return;
			}

			TrainManager.TouchElement[] TouchElements = Car.CarSections[0].Groups[add].TouchElements;

			if (TouchElements == null)
			{
				return;
			}

			ObjectState pickedObject = ParseFBO(Point, 5, 5);

			foreach (TrainManager.TouchElement TouchElement in TouchElements)
			{
				if (TouchElement.Element.internalObject == pickedObject)
				{
					Car.CarSections[0].CurrentAdditionalGroup = TouchElement.JumpScreenIndex;
					Car.ChangeCarSection(TrainManager.CarSectionType.Interior);

					foreach (var index in TouchElement.SoundIndices.Where(x => x >= 0 && x < Car.Sounds.Touch.Length))
					{
						SoundBuffer Buffer = Car.Sounds.Touch[index].Buffer;
						OpenBveApi.Math.Vector3 Position = Car.Sounds.Touch[index].Position;
						Program.Sounds.PlaySound(Buffer, 1.0, 1.0, Position, TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar], false);
					}
				}

				// HACK: Normally terminate the command issued once.
				if (TouchElement.Element.internalObject == pickedObject || (pickedObject != prePickedObject && TouchElement.Element.internalObject == prePickedObject))
				{
					foreach (int index in TouchElement.ControlIndices)
					{
						Interface.CurrentControls[index].AnalogState = 0.0;
						Interface.CurrentControls[index].DigitalState = Interface.DigitalControlState.Released;
						MainLoop.RemoveControlRepeat(index);
					}
				}
			}
		}
	}
}
