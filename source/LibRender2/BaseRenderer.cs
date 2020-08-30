using System;
using System.Collections.Generic;
using System.Linq;
using LibRender2.Backgrounds;
using LibRender2.Cameras;
using LibRender2.Fogs;
using LibRender2.Lightings;
using LibRender2.Loadings;
using LibRender2.MotionBlurs;
using LibRender2.Objects;
using LibRender2.Overlays;
using LibRender2.Primitives;
using LibRender2.Screens;
using LibRender2.Shaders;
using LibRender2.Texts;
using LibRender2.Textures;
using LibRender2.Viewports;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Textures;
using OpenBveApi.World;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Vector3 = OpenBveApi.Math.Vector3;

namespace LibRender2
{
	public abstract class BaseRenderer
	{
		// constants
		protected const float inv255 = 1.0f / 255.0f;

		/// <summary>Holds the lock for GDI Plus functions</summary>
		public static readonly object GdiPlusLock = new object();

		/// <summary>The callback to the host application</summary>
		internal HostInterface currentHost;

		/// <summary>Holds a reference to the current options</summary>
		internal BaseOptions currentOptions;

		public List<ObjectState> StaticObjectStates;
		public List<ObjectState> DynamicObjectStates;
		public VisibleObjectLibrary VisibleObjects;

		protected int[] ObjectsSortedByStart;
		protected int[] ObjectsSortedByEnd;
		protected int ObjectsSortedByStartPointer;
		protected int ObjectsSortedByEndPointer;
		protected double LastUpdatedTrackPosition;

		public Screen Screen;

		/// <summary>The track follower for the main camera</summary>
		public TrackFollower CameraTrackFollower;

		/// <summary>Holds a reference to the current interface type of the game (Used by the renderer)</summary>
		public InterfaceType CurrentInterface
		{
			get
			{
				return currentInterface;
			}
			set
			{
				previousInterface = currentInterface;
				currentInterface = value;
			}
		}

		/// <summary>Holds a reference to the previous interface type of the game</summary>
		public InterfaceType PreviousInterface
		{
			get
			{
				return previousInterface;
			}
		}
		//Backing properties for the interface values
		private InterfaceType currentInterface = InterfaceType.Normal;
		private InterfaceType previousInterface = InterfaceType.Normal;

		public CameraProperties Camera;
		public Lighting Lighting;
		public Background Background;
		public Fog Fog;
		public Marker Marker;
		public OpenGlString OpenGlString;
		public TextureManager TextureManager;
		public Cube Cube;
		public Rectangle Rectangle;
		public Loading Loading;
		public Keys Keys;
		public MotionBlur MotionBlur;

		public Matrix4D CurrentProjectionMatrix;
		public Matrix4D CurrentViewMatrix;

		public Vector3 TransformedLightPosition;

		protected List<Matrix4D> projectionMatrixList;
		protected List<Matrix4D> viewMatrixList;

		private ErrorCode lastError;

		/// <summary>The current shader in use</summary>
		internal Shader CurrentShader;

		public Shader DefaultShader;

		/// <summary>Whether fog is enabled in the debug options</summary>
		public bool OptionFog = true;

		/// <summary>Whether lighting is enabled in the debug options</summary>
		public bool OptionLighting = true;

		/// <summary>Whether normals rendering is enabled in the debug options</summary>
		public bool OptionNormals = false;

		/// <summary>Whether back face culling is enabled</summary>
		public bool OptionBackFaceCulling = true;

		/// <summary>Whether WireFrame rendering is enabled in the debug options</summary>
		public bool OptionWireFrame = false;

		/// <summary>The current viewport mode</summary>
		protected ViewportMode CurrentViewportMode = ViewportMode.Scenery;

		/// <summary>The current debug output mode</summary>
		public OutputMode CurrentOutputMode = OutputMode.Default;

		/// <summary>The previous debug output mode</summary>
		public OutputMode PreviousOutputMode = OutputMode.Default;

		/// <summary>The currently displayed timetable</summary>
		public DisplayedTimetable CurrentTimetable = DisplayedTimetable.None;

		/// <summary>The total number of OpenGL triangles in the current frame</summary>
		public int InfoTotalTriangles;

		/// <summary>The total number of OpenGL triangle strips in the current frame</summary>
		public int InfoTotalTriangleStrip;

		/// <summary>The total number of OpenGL quad strips in the current frame</summary>
		public int InfoTotalQuadStrip;

		/// <summary>The total number of OpenGL quads in the current frame</summary>
		public int InfoTotalQuads;

		/// <summary>The total number of OpenGL polygons in the current frame</summary>
		public int InfoTotalPolygon;

		/// <summary>The game's current framerate</summary>
		public double FrameRate = 1.0;

		/// <summary>Whether Blend is enabled in openGL</summary>
		private bool blendEnabled;

		private BlendingFactor blendSrcFactor;

		private BlendingFactor blendDestFactor;

		/// <summary>Whether Alpha Testing is enabled in openGL</summary>
		private bool alphaTestEnabled;

		/// <summary>The current AlphaFunc comparison</summary>
		private AlphaFunction alphaFuncComparison;

		/// <summary>The current AlphaFunc comparison value</summary>
		private float alphaFuncValue;

		/// <summary>Stores the most recently bound texture</summary>
		public OpenGlTexture LastBoundTexture;

		private Color32 lastColor;

		/// <summary>Holds the handle of the last VAO bound by openGL</summary>
		public int lastVAO;

		public bool ForceLegacyOpenGL
		{
			get;
			set;
		}

		/*
		 * List of VBO and IBO to delete on the next frame pass
		 * This needs to be done here as opposed to in the finalizer
		 */
		internal static readonly List<int> vaoToDelete = new List<int>();
		internal static readonly List<int> vboToDelete = new List<int>();
		internal static readonly List<int> iboToDelete = new List<int>();

		public bool AvailableNewRenderer => currentOptions != null && currentOptions.IsUseNewRenderer && !ForceLegacyOpenGL;

		protected BaseRenderer()
		{
			Screen = new Screen();
			Camera = new CameraProperties(this);
			Lighting = new Lighting(this);
			Marker = new Marker();

			projectionMatrixList = new List<Matrix4D>();
			viewMatrixList = new List<Matrix4D>();
		}

		/// <summary>
		/// Call this once to initialise the renderer
		/// </summary>
		public virtual void Initialize(HostInterface CurrentHost, BaseOptions CurrentOptions)
		{
			currentHost = CurrentHost;
			currentOptions = CurrentOptions;

			try
			{
				if (DefaultShader == null)
				{
					DefaultShader = new Shader(this, "default", "default", true);
				}
				DefaultShader.Activate();
				DefaultShader.SetMaterialAmbient(Color32.White);
				DefaultShader.SetMaterialDiffuse(Color32.White);
				DefaultShader.SetMaterialSpecular(Color32.White);
				lastColor = Color32.White;
				DefaultShader.Deactivate();
			}
			catch
			{
				CurrentHost.AddMessage(MessageType.Error, false, "Initializing the default shaders failed- Falling back to legacy openGL.");
				CurrentOptions.IsUseNewRenderer = false;
				ForceLegacyOpenGL = true;
			}

			Background = new Background(this);
			Fog = new Fog();
			OpenGlString = new OpenGlString(this);
			TextureManager = new TextureManager(currentHost, this);
			Cube = new Cube(this);
			Rectangle = new Rectangle(this);
			Loading = new Loading(this);
			Keys = new Keys(this);
			MotionBlur = new MotionBlur(this);

			StaticObjectStates = new List<ObjectState>();
			DynamicObjectStates = new List<ObjectState>();
			VisibleObjects = new VisibleObjectLibrary(currentHost, Camera, currentOptions, this);

			GL.ClearColor(0.67f, 0.67f, 0.67f, 1.0f);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Lequal);
			SetBlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			GL.Hint(HintTarget.FogHint, HintMode.Fastest);
			GL.Hint(HintTarget.LineSmoothHint, HintMode.Fastest);
			GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Fastest);
			GL.Hint(HintTarget.PointSmoothHint, HintMode.Fastest);
			GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Fastest);
			GL.Hint(HintTarget.GenerateMipmapHint, HintMode.Nicest);
			GL.Enable(EnableCap.CullFace);
			GL.CullFace(CullFaceMode.Front);
			GL.Disable(EnableCap.Texture2D);
			GL.Disable(EnableCap.Dither);
			GL.Disable(EnableCap.Lighting);
			GL.Disable(EnableCap.Fog);
			GL.Fog(FogParameter.FogMode, (int)FogMode.Linear);
		}

		/// <summary>Performs cleanup of disposed resources</summary>
		public void ReleaseResources()
		{
			//Must remember to lock on the lists as the destructor is in a different thread
			lock (vaoToDelete)
			{
				foreach (int VAO in vaoToDelete)
				{
					GL.DeleteVertexArray(VAO);
				}
				vaoToDelete.Clear();
			}

			lock (vboToDelete)
			{
				foreach (int VBO in vboToDelete)
				{
					GL.DeleteBuffer(VBO);
				}
				vboToDelete.Clear();
			}

			lock (iboToDelete)
			{
				foreach (int IBO in iboToDelete)
				{
					GL.DeleteBuffer(IBO);
				}
				iboToDelete.Clear();
			}
		}

		/// <summary>
		/// Performs a reset of OpenGL to the default state
		/// </summary>
		public virtual void ResetOpenGlState()
		{
			GL.Enable(EnableCap.CullFace);
			GL.Disable(EnableCap.Lighting);
			GL.Disable(EnableCap.Fog);
			GL.Disable(EnableCap.Texture2D);
			SetBlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			UnsetBlendFunc();
			GL.Enable(EnableCap.DepthTest);
			GL.DepthMask(true);
			SetAlphaFunc(AlphaFunction.Greater, 0.9f);
		}

		public void PushMatrix(MatrixMode Mode)
		{
			switch (Mode)
			{
				case MatrixMode.Modelview:
					viewMatrixList.Add(CurrentViewMatrix);
					break;
				case MatrixMode.Projection:
					projectionMatrixList.Add(CurrentProjectionMatrix);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(Mode), Mode, null);
			}
		}

		public void PopMatrix(MatrixMode Mode)
		{
			switch (Mode)
			{
				case MatrixMode.Modelview:
					CurrentViewMatrix = viewMatrixList.Last();
					viewMatrixList.RemoveAt(viewMatrixList.Count - 1);
					break;
				case MatrixMode.Projection:
					CurrentProjectionMatrix = projectionMatrixList.Last();
					projectionMatrixList.RemoveAt(projectionMatrixList.Count - 1);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(Mode), Mode, null);
			}
		}

		public void Reset()
		{
			currentHost.AnimatedObjectCollectionCache.Clear();
			currentHost.StaticObjectCache.Clear();
			TextureManager.UnloadAllTextures();

			Initialize(currentHost, currentOptions);
		}

		public int CreateStaticObject(StaticObject Prototype, Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, bool AccurateObjectDisposal, double AccurateObjectDisposalZOffset, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness)
		{
			Matrix4D Translate = Matrix4D.CreateTranslation(Position.X, Position.Y, -Position.Z);
			//FIXME: This seems to need to be the 'wrong' way around. Need to standardise on Matrices throughout?
			Matrix4D Rotate = (Matrix4D)new Transformation(AuxTransformation, BaseTransformation);
			return CreateStaticObject(Prototype, AuxTransformation, Rotate, Translate, AccurateObjectDisposal, AccurateObjectDisposalZOffset, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness);
		}

		public int CreateStaticObject(StaticObject Prototype, Transformation AuxTransformation, Matrix4D Rotate, Matrix4D Translate, bool AccurateObjectDisposal, double AccurateObjectDisposalZOffset, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness)
		{
			if (Prototype == null)
			{
				return -1;
			}

			if (Prototype.Mesh.Faces.Length == 0)
			{
				//Null object- Waste of time trying to calculate anything for these
				return -1;
			}

			float startingDistance = float.MaxValue;
			float endingDistance = float.MinValue;

			if (AccurateObjectDisposal)
			{
				foreach (VertexTemplate vertex in Prototype.Mesh.Vertices)
				{
					Vector3 Coordinates = new Vector3(vertex.Coordinates);
					Coordinates.Rotate(AuxTransformation);

					if (Coordinates.Z < startingDistance)
					{
						startingDistance = (float)Coordinates.Z;
					}

					if (Coordinates.Z > endingDistance)
					{
						endingDistance = (float)Coordinates.Z;
					}
				}

				startingDistance += (float)AccurateObjectDisposalZOffset;
				endingDistance += (float)AccurateObjectDisposalZOffset;
			}

			const double minBlockLength = 20.0;

			if (BlockLength < minBlockLength)
			{
				BlockLength *= Math.Ceiling(minBlockLength / BlockLength);
			}

			if (AccurateObjectDisposal)
			{
				startingDistance += (float)TrackPosition;
				endingDistance += (float)TrackPosition;
				double z = BlockLength * Math.Floor(TrackPosition / BlockLength);
				StartingDistance = Math.Min(z - BlockLength, startingDistance);
				EndingDistance = Math.Max(z + 2.0 * BlockLength, endingDistance);
				startingDistance = (float)(BlockLength * Math.Floor(StartingDistance / BlockLength));
				endingDistance = (float)(BlockLength * Math.Ceiling(EndingDistance / BlockLength));
			}
			else
			{
				startingDistance = (float)StartingDistance;
				endingDistance = (float)EndingDistance;
			}

			StaticObjectStates.Add(new ObjectState
			{
				Prototype = Prototype,
				Translation = Translate,
				Rotate = Rotate,
				Brightness = Brightness,
				StartingDistance = startingDistance,
				EndingDistance = endingDistance
			});

			foreach (MeshFace face in Prototype.Mesh.Faces)
			{
				switch (face.Flags & MeshFace.FaceTypeMask)
				{
					case MeshFace.FaceTypeTriangles:
						InfoTotalTriangles++;
						break;
					case MeshFace.FaceTypeTriangleStrip:
						InfoTotalTriangleStrip++;
						break;
					case MeshFace.FaceTypeQuads:
						InfoTotalQuads++;
						break;
					case MeshFace.FaceTypeQuadStrip:
						InfoTotalQuadStrip++;
						break;
					case MeshFace.FaceTypePolygon:
						InfoTotalPolygon++;
						break;
				}
			}

			return StaticObjectStates.Count - 1;
		}

		public void CreateDynamicObject(ref ObjectState internalObject)
		{
			if (internalObject == null)
			{
				internalObject = new ObjectState { Prototype = new StaticObject(currentHost) };
			}

			internalObject.Prototype.Dynamic = true;

			DynamicObjectStates.Add(internalObject);
		}

		/// <summary>Initializes the visibility of all objects within the game world</summary>
		/// <remarks>If the new renderer is enabled, this must be run in a thread processing an openGL context in order to successfully create
		/// the required VAO objects</remarks>
		public void InitializeVisibility()
		{
			ObjectsSortedByStart = StaticObjectStates.Select((x, i) => new { Index = i, Distance = x.StartingDistance }).OrderBy(x => x.Distance).Select(x => x.Index).ToArray();
			ObjectsSortedByEnd = StaticObjectStates.Select((x, i) => new { Index = i, Distance = x.EndingDistance }).OrderBy(x => x.Distance).Select(x => x.Index).ToArray();
			ObjectsSortedByStartPointer = 0;
			ObjectsSortedByEndPointer = 0;

			double p = CameraTrackFollower.TrackPosition + Camera.Alignment.Position.Z;

			foreach (ObjectState state in StaticObjectStates.Where(recipe => recipe.StartingDistance <= p + Camera.ForwardViewingDistance & recipe.EndingDistance >= p - Camera.BackwardViewingDistance))
			{
				VisibleObjects.ShowObject(state, ObjectType.Static);
			}
		}

		public void UpdateVisibility(double TrackPosition)
		{
			double d = TrackPosition - LastUpdatedTrackPosition;
			int n = ObjectsSortedByStart.Length;
			double p = CameraTrackFollower.TrackPosition + Camera.Alignment.Position.Z;

			if (d < 0.0)
			{
				if (ObjectsSortedByStartPointer >= n)
				{
					ObjectsSortedByStartPointer = n - 1;
				}

				if (ObjectsSortedByEndPointer >= n)
				{
					ObjectsSortedByEndPointer = n - 1;
				}

				// dispose
				while (ObjectsSortedByStartPointer >= 0)
				{
					int o = ObjectsSortedByStart[ObjectsSortedByStartPointer];

					if (StaticObjectStates[o].StartingDistance > p + Camera.ForwardViewingDistance)
					{
						VisibleObjects.HideObject(StaticObjectStates[o]);
						ObjectsSortedByStartPointer--;
					}
					else
					{
						break;
					}
				}

				// introduce
				while (ObjectsSortedByEndPointer >= 0)
				{
					int o = ObjectsSortedByEnd[ObjectsSortedByEndPointer];

					if (StaticObjectStates[o].EndingDistance >= p - Camera.BackwardViewingDistance)
					{
						if (StaticObjectStates[o].StartingDistance <= p + Camera.ForwardViewingDistance)
						{
							VisibleObjects.ShowObject(StaticObjectStates[o], ObjectType.Static);
						}

						ObjectsSortedByEndPointer--;
					}
					else
					{
						break;
					}
				}
			}
			else if (d > 0.0)
			{
				if (ObjectsSortedByStartPointer < 0)
				{
					ObjectsSortedByStartPointer = 0;
				}

				if (ObjectsSortedByEndPointer < 0)
				{
					ObjectsSortedByEndPointer = 0;
				}

				// dispose
				while (ObjectsSortedByEndPointer < n)
				{
					int o = ObjectsSortedByEnd[ObjectsSortedByEndPointer];

					if (StaticObjectStates[o].EndingDistance < p - Camera.BackwardViewingDistance)
					{
						VisibleObjects.HideObject(StaticObjectStates[o]);
						ObjectsSortedByEndPointer++;
					}
					else
					{
						break;
					}
				}

				// introduce
				while (ObjectsSortedByStartPointer < n)
				{
					int o = ObjectsSortedByStart[ObjectsSortedByStartPointer];

					if (StaticObjectStates[o].StartingDistance <= p + Camera.ForwardViewingDistance)
					{
						if (StaticObjectStates[o].EndingDistance >= p - Camera.BackwardViewingDistance)
						{
							VisibleObjects.ShowObject(StaticObjectStates[o], ObjectType.Static);
						}

						ObjectsSortedByStartPointer++;
					}
					else
					{
						break;
					}
				}
			}

			LastUpdatedTrackPosition = TrackPosition;
		}

		public void UpdateViewingDistances(double BackgroundImageDistance)
		{
			double f = Math.Atan2(CameraTrackFollower.WorldDirection.Z, CameraTrackFollower.WorldDirection.X);
			double c = Math.Atan2(Camera.AbsoluteDirection.Z, Camera.AbsoluteDirection.X) - f;
			if (c < -Math.PI)
			{
				c += 2.0 * Math.PI;
			}
			else if (c > Math.PI)
			{
				c -= 2.0 * Math.PI;
			}

			double a0 = c - 0.5 * Camera.HorizontalViewingAngle;
			double a1 = c + 0.5 * Camera.HorizontalViewingAngle;
			double max;
			if (a0 <= 0.0 & a1 >= 0.0)
			{
				max = 1.0;
			}
			else
			{
				double c0 = Math.Cos(a0);
				double c1 = Math.Cos(a1);
				max = c0 > c1 ? c0 : c1;
				if (max < 0.0) max = 0.0;
			}

			double min;
			if (a0 <= -Math.PI | a1 >= Math.PI)
			{
				min = -1.0;
			}
			else
			{
				double c0 = Math.Cos(a0);
				double c1 = Math.Cos(a1);
				min = c0 < c1 ? c0 : c1;
				if (min > 0.0) min = 0.0;
			}

			double d = BackgroundImageDistance + Camera.ExtraViewingDistance;
			Camera.ForwardViewingDistance = d * max;
			Camera.BackwardViewingDistance = -d * min;
			UpdateVisibility(CameraTrackFollower.TrackPosition + Camera.Alignment.Position.Z, true);
		}

		public void UpdateVisibility(double TrackPosition, bool ViewingDistanceChanged)
		{
			if (ViewingDistanceChanged)
			{
				UpdateVisibility(TrackPosition);
				UpdateVisibility(TrackPosition - 0.001);
				UpdateVisibility(TrackPosition + 0.001);
				UpdateVisibility(TrackPosition);
			}
			else
			{
				UpdateVisibility(TrackPosition);
			}
		}

		/// <summary>Determines the maximum Anisotropic filtering level the system supports</summary>
		public void DetermineMaxAFLevel(bool MacOS)
		{
			if (MacOS)
			{
				// Calling GL.GetString in this manner seems to be crashing the OS-X driver (No idea, but probably OpenTK....)
				// As we only support newer Intel Macs, 16x AF is safe
				currentOptions.AnisotropicFilteringMaximum = 16;
				return;
			}
			string[] Extensions;
			try
			{
				Extensions = GL.GetString(StringName.Extensions).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			}
			catch
			{
				currentOptions.AnisotropicFilteringMaximum = 0;
				currentOptions.AnisotropicFilteringLevel = 0;
				return;
			}
			currentOptions.AnisotropicFilteringMaximum = 0;

			foreach (string extension in Extensions)
			{
				if (String.Compare(extension, "GL_EXT_texture_filter_anisotropic", StringComparison.OrdinalIgnoreCase) == 0)
				{
					float n = GL.GetFloat((GetPName)ExtTextureFilterAnisotropic.MaxTextureMaxAnisotropyExt);
					int MaxAF = (int)Math.Round(n);

					if (MaxAF != currentOptions.AnisotropicFilteringMaximum)
					{
						currentOptions.AnisotropicFilteringMaximum = (int)Math.Round(n);
					}
					break;
				}
			}

			if (currentOptions.AnisotropicFilteringMaximum <= 0)
			{
				currentOptions.AnisotropicFilteringMaximum = 0;
				currentOptions.AnisotropicFilteringLevel = 0;
			}
			else if (currentOptions.AnisotropicFilteringLevel == 0 & currentOptions.AnisotropicFilteringMaximum > 0)
			{
				currentOptions.AnisotropicFilteringLevel = currentOptions.AnisotropicFilteringMaximum;
			}
			else if (currentOptions.AnisotropicFilteringLevel > currentOptions.AnisotropicFilteringMaximum)
			{
				currentOptions.AnisotropicFilteringLevel = currentOptions.AnisotropicFilteringMaximum;
			}
		}

		public void UpdateViewport()
		{
			UpdateViewport(Screen.Width, Screen.Height);
		}

		/// <summary>Updates the openGL viewport</summary>
		/// <param name="Mode">The viewport change mode</param>
		public void UpdateViewport(ViewportChangeMode Mode)
		{
			switch (Mode)
			{
				case ViewportChangeMode.ChangeToScenery:
					CurrentViewportMode = ViewportMode.Scenery;
					break;
				case ViewportChangeMode.ChangeToCab:
					CurrentViewportMode = ViewportMode.Cab;
					break;
			}

			UpdateViewport();
		}

		public virtual void UpdateViewport(int Width, int Height)
		{
			Screen.Width = Width;
			Screen.Height = Height;
			GL.Viewport(0, 0, Screen.Width, Screen.Height);

			Screen.AspectRatio = Screen.Width / (double)Screen.Height;
			Camera.HorizontalViewingAngle = 2.0 * Math.Atan(Math.Tan(0.5 * Camera.VerticalViewingAngle) * Screen.AspectRatio);
			CurrentProjectionMatrix = Matrix4D.CreatePerspectiveFieldOfView(Camera.VerticalViewingAngle, Screen.AspectRatio, 0.2, 1000.0);
		}

		public void ResetShader(Shader Shader)
		{
#if DEBUG
			lastError = GL.GetError();

			if (lastError != ErrorCode.NoError)
			{
				throw new InvalidOperationException($"OpenGL Error: {lastError}");
			}
#endif

			Shader.SetCurrentProjectionMatrix(Matrix4D.Identity);
			Shader.SetCurrentModelViewMatrix(Matrix4D.Identity);
			Shader.SetCurrentTextureMatrix(Matrix4D.Identity);
			Shader.SetIsLight(false);
			Shader.SetLightPosition(Vector3.Zero);
			Shader.SetLightAmbient(Color24.White);
			Shader.SetLightDiffuse(Color24.White);
			Shader.SetLightSpecular(Color24.White);
			Shader.SetLightModel(Lighting.LightModel);
			Shader.SetMaterialAmbient(Color24.White);
			Shader.SetMaterialDiffuse(Color24.White);
			Shader.SetMaterialSpecular(Color24.White);
			Shader.SetMaterialEmission(Color24.White);
			lastColor = Color32.White;
			Shader.SetMaterialShininess(1.0f);
			Shader.SetIsFog(false);
			Shader.SetIsTexture(false);
			Shader.SetTexture(0);
			Shader.SetBrightness(1.0f);
			Shader.SetOpacity(1.0f);
			Shader.SetObjectIndex(0);
		}

		public void SetBlendFunc()
		{
			SetBlendFunc(blendSrcFactor, blendDestFactor);
		}

		public void SetBlendFunc(BlendingFactor SrcFactor, BlendingFactor DestFactor)
		{
			blendEnabled = true;
			blendSrcFactor = SrcFactor;
			blendDestFactor = DestFactor;
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(SrcFactor, DestFactor);
		}

		public void UnsetBlendFunc()
		{
			blendEnabled = false;
			GL.Disable(EnableCap.Blend);
		}

		public void RestoreBlendFunc()
		{
			if (blendEnabled)
			{
				GL.Enable(EnableCap.Blend);
				GL.BlendFunc(blendSrcFactor, blendDestFactor);
			}
			else
			{
				GL.Disable(EnableCap.Blend);
			}
		}

		/// <summary>Specifies the OpenGL alpha function to perform</summary>
		public void SetAlphaFunc()
		{
			SetAlphaFunc(alphaFuncComparison, alphaFuncValue);
		}

		/// <summary>Specifies the OpenGL alpha function to perform</summary>
		/// <param name="Comparison">The comparison to use</param>
		/// <param name="Value">The value to compare</param>
		public void SetAlphaFunc(AlphaFunction Comparison, float Value)
		{
			alphaTestEnabled = true;
			alphaFuncComparison = Comparison;
			alphaFuncValue = Value;
			GL.Enable(EnableCap.AlphaTest);
			GL.AlphaFunc(Comparison, Value);
		}

		/// <summary>Disables OpenGL alpha testing</summary>
		public void UnsetAlphaFunc()
		{
			alphaTestEnabled = false;
			GL.Disable(EnableCap.AlphaTest);
		}

		/// <summary>Restores the OpenGL alpha function to it's previous state</summary>
		public void RestoreAlphaFunc()
		{
			if (alphaTestEnabled)
			{
				GL.Enable(EnableCap.AlphaTest);
				GL.AlphaFunc(alphaFuncComparison, alphaFuncValue);
			}
			else
			{
				GL.Disable(EnableCap.AlphaTest);
			}
		}

		/// <summary>Draws a face using the current shader</summary>
		/// <param name="State">The FaceState to draw</param>
		/// <param name="IsDebugTouchMode">Whether debug touch mode</param>
		public void RenderFace(FaceState State, bool IsDebugTouchMode = false)
		{
			RenderFace(CurrentShader, State.Object, State.Face, IsDebugTouchMode);
		}

		/// <summary>Draws a face using the specified shader</summary>
		/// <param name="Shader">The shader to use</param>
		/// <param name="State">The FaceState to draw</param>
		/// <param name="IsDebugTouchMode">Whether debug touch mode</param>
		public void RenderFace(Shader Shader, ObjectState State, MeshFace Face, bool IsDebugTouchMode = false)
		{
			Matrix4D modelMatrix = State.ModelMatrix * Camera.TranslationMatrix;
			Matrix4D modelViewMatrix = modelMatrix * CurrentViewMatrix;
			RenderFace(Shader, State, Face, modelMatrix, modelViewMatrix, IsDebugTouchMode);
		}

		public void RenderFace(Shader Shader, ObjectState State, MeshFace Face, Matrix4D modelMatrix, Matrix4D modelViewMatrix, bool IsDebugTouchMode = false)
		{
			GL.Disable(EnableCap.Lighting);
			if (State.Prototype.Mesh.Vertices.Length < 1)
			{
				return;
			}

			MeshMaterial material = State.Prototype.Mesh.Materials[Face.Material];
			VertexArrayObject VAO = (VertexArrayObject)State.Prototype.Mesh.VAO;

			if (lastVAO != VAO.handle)
			{
				VAO.Bind();
				lastVAO = VAO.handle;
			}

			if (!OptionBackFaceCulling || (Face.Flags & MeshFace.Face2Mask) != 0)
			{
				GL.Disable(EnableCap.CullFace);
			}
			else if (OptionBackFaceCulling)
			{
				if ((Face.Flags & MeshFace.Face2Mask) == 0)
				{
					GL.Enable(EnableCap.CullFace);
				}
			}

			// matrix
			Shader.SetCurrentModelViewMatrix(modelViewMatrix);
			Shader.SetCurrentTextureMatrix(State.TextureTranslation);

			if (OptionWireFrame || IsDebugTouchMode)
			{
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
			}

			// lighting
			Shader.SetMaterialFlags(material.Flags);
			if (OptionLighting)
			{
				if (material.Color != lastColor)
				{
					Shader.SetMaterialAmbient(material.Color);
					Shader.SetMaterialDiffuse(material.Color);
					Shader.SetMaterialSpecular(material.Color);
					//TODO: Ambient and specular colors are not set by any current parsers
				}
				if ((material.Flags & MaterialFlags.Emissive) != 0)
				{
					Shader.SetMaterialEmission(material.EmissiveColor);
				}

				Shader.SetMaterialShininess(1.0f);
			}
			else
			{
				if (material.Color != lastColor)
				{
					Shader.SetMaterialAmbient(material.Color);
				}
			}

			lastColor = material.Color;
			PrimitiveType DrawMode;

			switch (Face.Flags & MeshFace.FaceTypeMask)
			{
				case MeshFace.FaceTypeTriangles:
					DrawMode = PrimitiveType.Triangles;
					break;
				case MeshFace.FaceTypeTriangleStrip:
					DrawMode = PrimitiveType.TriangleStrip;
					break;
				case MeshFace.FaceTypeQuads:
					DrawMode = PrimitiveType.Quads;
					break;
				case MeshFace.FaceTypeQuadStrip:
					DrawMode = PrimitiveType.QuadStrip;
					break;
				default:
					DrawMode = PrimitiveType.Polygon;
					break;
			}

			// blend factor
			float distanceFactor;
			if (material.GlowAttenuationData != 0)
			{
				distanceFactor = (float)Glow.GetDistanceFactor(modelMatrix, State.Prototype.Mesh.Vertices, ref Face, material.GlowAttenuationData);
			}
			else
			{
				distanceFactor = 1.0f;
			}

			float blendFactor = inv255 * material.DaytimeNighttimeBlend + 1.0f - Lighting.OptionLightingResultingAmount;
			if (blendFactor > 1.0)
			{
				blendFactor = 1.0f;
			}

			// daytime polygon
			{
				// texture
				if (material.DaytimeTexture != null && currentHost.LoadTexture(material.DaytimeTexture, (OpenGlTextureWrapMode)material.WrapMode))
				{
					Shader.SetIsTexture(true);
					if (LastBoundTexture != material.DaytimeTexture.OpenGlTextures[(int)material.WrapMode])
					{
						GL.BindTexture(TextureTarget.Texture2D,
							material.DaytimeTexture.OpenGlTextures[(int)material.WrapMode].Name);
						LastBoundTexture = material.DaytimeTexture.OpenGlTextures[(int)material.WrapMode];
					}
				}
				else
				{
					Shader.SetIsTexture(false);
				}
				// Calculate the brightness of the poly to render
				float factor;
				if (material.BlendMode == MeshMaterialBlendMode.Additive)
				{
					//Additive blending- Full brightness
					factor = 1.0f;
					GL.Enable(EnableCap.Blend);
					GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
					Shader.SetIsFog(false);
				}
				else if (material.NighttimeTexture == null || material.NighttimeTexture == material.DaytimeTexture)
				{
					//No nighttime texture or both are identical- Darken the polygon to match the light conditions
					factor = 1.0f - 0.7f * blendFactor;
				}
				else
				{
					//Valid nighttime texture- Blend the two textures by DNB at max brightness
					factor = 1.0f;
				}
				Shader.SetBrightness(factor);

				float alphaFactor = distanceFactor;
				if (material.NighttimeTexture != null && (material.Flags & MaterialFlags.CrossFadeTexture) != 0)
				{
					alphaFactor *= 1.0f - blendFactor;
				}

				Shader.SetOpacity(inv255 * material.Color.A * alphaFactor);

				// render polygon
				VAO.Draw(DrawMode, Face.IboStartIndex, Face.Vertices.Length);
			}

			// nighttime polygon
			if (material.NighttimeTexture != null && material.NighttimeTexture != material.DaytimeTexture && currentHost.LoadTexture(material.NighttimeTexture, (OpenGlTextureWrapMode)material.WrapMode))
			{
				// texture
				Shader.SetIsTexture(true);
				if (LastBoundTexture != material.NighttimeTexture.OpenGlTextures[(int)material.WrapMode])
				{
					GL.BindTexture(TextureTarget.Texture2D, material.NighttimeTexture.OpenGlTextures[(int)material.WrapMode].Name);
					LastBoundTexture = material.NighttimeTexture.OpenGlTextures[(int)material.WrapMode];
				}


				GL.Enable(EnableCap.Blend);

				// alpha test
				GL.Enable(EnableCap.AlphaTest);
				GL.AlphaFunc(AlphaFunction.Greater, 0.0f);

				// blend mode
				float alphaFactor = distanceFactor * blendFactor;

				Shader.SetOpacity(inv255 * material.Color.A * alphaFactor);

				// render polygon
				VAO.Draw(DrawMode, Face.IboStartIndex, Face.Vertices.Length);
				RestoreBlendFunc();
				RestoreAlphaFunc();
			}


			// normals
			if (OptionNormals)
			{
				Shader.SetIsTexture(false);
				Shader.SetBrightness(1.0f);
				Shader.SetOpacity(1.0f);
				VertexArrayObject NormalsVAO = (VertexArrayObject)State.Prototype.Mesh.NormalsVAO;
				NormalsVAO.Bind();
				lastVAO = NormalsVAO.handle;
				NormalsVAO.Draw(PrimitiveType.Lines, Face.NormalsIboStartIndex, Face.Vertices.Length * 2);
			}

			// finalize
			if (material.BlendMode == MeshMaterialBlendMode.Additive)
			{
				RestoreBlendFunc();
				Shader.SetIsFog(OptionFog);
			}
			if (OptionWireFrame || IsDebugTouchMode)
			{
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
			}
		}

		public void RenderFaceImmediateMode(FaceState State, bool IsDebugTouchMode = false)
		{
			RenderFaceImmediateMode(State.Object, State.Face, IsDebugTouchMode);
		}

		public void RenderFaceImmediateMode(ObjectState State, MeshFace Face, bool IsDebugTouchMode = false)
		{
			Matrix4D modelMatrix = State.ModelMatrix * Camera.TranslationMatrix;
			Matrix4D modelViewMatrix = modelMatrix * CurrentViewMatrix;
			RenderFaceImmediateMode(State, Face, modelMatrix, modelViewMatrix, IsDebugTouchMode);
		}

		public void RenderFaceImmediateMode(ObjectState State, MeshFace Face, Matrix4D modelMatrix, Matrix4D modelViewMatrix, bool IsDebugTouchMode = false)
		{
			if (State.Prototype.Mesh.Vertices.Length < 1)
			{
				return;
			}

			VertexTemplate[] vertices = State.Prototype.Mesh.Vertices;
			MeshMaterial material = State.Prototype.Mesh.Materials[Face.Material];

			if (!OptionBackFaceCulling || (Face.Flags & MeshFace.Face2Mask) != 0)
			{
				GL.Disable(EnableCap.CullFace);
			}
			else if (OptionBackFaceCulling)
			{
				if ((Face.Flags & MeshFace.Face2Mask) == 0)
				{
					GL.Enable(EnableCap.CullFace);
				}
			}

			// matrix
			unsafe
			{
				GL.MatrixMode(MatrixMode.Projection);
				GL.PushMatrix();
				fixed (double* matrixPointer = &CurrentProjectionMatrix.Row0.X)
				{
					GL.LoadMatrix(matrixPointer);
				}
				GL.MatrixMode(MatrixMode.Modelview);
				GL.PushMatrix();
				fixed (double* matrixPointer = &CurrentViewMatrix.Row0.X)
				{
					GL.LoadMatrix(matrixPointer);
				}

				double* matrixPointer2 = &modelMatrix.Row0.X;
				{
					GL.MultMatrix(matrixPointer2);
				}

				GL.MatrixMode(MatrixMode.Texture);
				GL.PushMatrix();
				fixed (double* matrixPointer = &State.TextureTranslation.Row0.X)
				{
					GL.LoadMatrix(matrixPointer);
				}

			}

			if (OptionWireFrame || IsDebugTouchMode)
			{
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
			}

			// lighting
			if (material.NighttimeTexture == null)
			{
				if (OptionLighting)
				{
					GL.Enable(EnableCap.Lighting);
				}
			}

			if ((material.Flags & MaterialFlags.Emissive) != 0)
			{
				GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, new Color4(material.EmissiveColor.R, material.EmissiveColor.G, material.EmissiveColor.B, 255));
			}
			else
			{
				GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, new Color4(0.0f, 0.0f, 0.0f, 1.0f));
			}

			// fog
			if (OptionFog)
			{
				GL.Enable(EnableCap.Fog);
			}

			PrimitiveType DrawMode;

			switch (Face.Flags & MeshFace.FaceTypeMask)
			{
				case MeshFace.FaceTypeTriangles:
					DrawMode = PrimitiveType.Triangles;
					break;
				case MeshFace.FaceTypeTriangleStrip:
					DrawMode = PrimitiveType.TriangleStrip;
					break;
				case MeshFace.FaceTypeQuads:
					DrawMode = PrimitiveType.Quads;
					break;
				case MeshFace.FaceTypeQuadStrip:
					DrawMode = PrimitiveType.QuadStrip;
					break;
				default:
					DrawMode = PrimitiveType.Polygon;
					break;
			}

			// blend factor
			float distanceFactor;
			if (material.GlowAttenuationData != 0)
			{
				distanceFactor = (float)Glow.GetDistanceFactor(modelMatrix, vertices, ref Face, material.GlowAttenuationData);
			}
			else
			{
				distanceFactor = 1.0f;
			}

			float blendFactor = inv255 * material.DaytimeNighttimeBlend + 1.0f - Lighting.OptionLightingResultingAmount;
			if (blendFactor > 1.0)
			{
				blendFactor = 1.0f;
			}

			// daytime polygon
			{
				// texture
				if (material.DaytimeTexture != null)
				{
					if (currentHost.LoadTexture(material.DaytimeTexture, (OpenGlTextureWrapMode)material.WrapMode))
					{
						GL.Enable(EnableCap.Texture2D);
						if (LastBoundTexture != material.DaytimeTexture.OpenGlTextures[(int)material.WrapMode])
						{
							GL.BindTexture(TextureTarget.Texture2D, material.DaytimeTexture.OpenGlTextures[(int)material.WrapMode].Name);
							LastBoundTexture = material.DaytimeTexture.OpenGlTextures[(int)material.WrapMode];
						}
					}
				}

				// blend mode
				float factor;
				if (material.BlendMode == MeshMaterialBlendMode.Additive)
				{
					factor = 1.0f;
					GL.Enable(EnableCap.Blend);
					GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
					GL.Disable(EnableCap.Fog);
				}
				else if (material.NighttimeTexture == null)
				{
					factor = 1.0f - 0.7f * blendFactor;
				}
				else
				{
					factor = 1.0f;
				}

				if ((material.Flags & MaterialFlags.DisableLighting) != 0)
				{
					GL.Disable(EnableCap.Lighting);
				}

				float alphaFactor = distanceFactor;
				if (material.NighttimeTexture != null && (material.Flags & MaterialFlags.CrossFadeTexture) != 0)
				{
					alphaFactor *= 1.0f - blendFactor;
				}

				GL.Begin(DrawMode);

				if (OptionWireFrame)
				{
					GL.Color4(inv255 * material.Color.R * factor, inv255 * material.Color.G * factor, inv255 * material.Color.B * factor, 1.0f);
				}
				else
				{
					GL.Color4(inv255 * material.Color.R * factor, inv255 * material.Color.G * factor, inv255 * material.Color.B * factor, inv255 * material.Color.A * alphaFactor);
				}

				for (int i = 0; i < Face.Vertices.Length; i++)
				{
					GL.Normal3(Face.Vertices[i].Normal.X, Face.Vertices[i].Normal.Y, -Face.Vertices[i].Normal.Z);
					GL.TexCoord2(vertices[Face.Vertices[i].Index].TextureCoordinates.X, vertices[Face.Vertices[i].Index].TextureCoordinates.Y);

					if (vertices[Face.Vertices[i].Index] is ColoredVertex)
					{
						ColoredVertex v = (ColoredVertex)vertices[Face.Vertices[i].Index];
						GL.Color3(v.Color.R, v.Color.G, v.Color.B);
					}

					GL.Vertex3(vertices[Face.Vertices[i].Index].Coordinates.X, vertices[Face.Vertices[i].Index].Coordinates.Y, -vertices[Face.Vertices[i].Index].Coordinates.Z);
				}

				GL.End();
			}

			// nighttime polygon
			if (material.NighttimeTexture != null && currentHost.LoadTexture(material.NighttimeTexture, (OpenGlTextureWrapMode)material.WrapMode))
			{
				// texture
				GL.Enable(EnableCap.Texture2D);
				if (LastBoundTexture != material.NighttimeTexture.OpenGlTextures[(int)material.WrapMode])
				{
					GL.BindTexture(TextureTarget.Texture2D, material.NighttimeTexture.OpenGlTextures[(int)material.WrapMode].Name);
					LastBoundTexture = material.NighttimeTexture.OpenGlTextures[(int)material.WrapMode];
				}

				GL.Enable(EnableCap.Blend);

				// alpha test
				GL.Enable(EnableCap.AlphaTest);
				GL.AlphaFunc(AlphaFunction.Greater, 0.0f);

				// blend mode
				float alphaFactor = distanceFactor * blendFactor;

				GL.Begin(DrawMode);

				if (OptionWireFrame)
				{
					GL.Color4(inv255 * material.Color.R, inv255 * material.Color.G, inv255 * material.Color.B, 1.0f);
				}
				else
				{
					GL.Color4(inv255 * material.Color.R, inv255 * material.Color.G, inv255 * material.Color.B, inv255 * material.Color.A * alphaFactor);
				}

				for (int i = 0; i < Face.Vertices.Length; i++)
				{
					GL.Normal3(Face.Vertices[i].Normal.X, Face.Vertices[i].Normal.Y, -Face.Vertices[i].Normal.Z);
					GL.TexCoord2(vertices[Face.Vertices[i].Index].TextureCoordinates.X, vertices[Face.Vertices[i].Index].TextureCoordinates.Y);

					if (vertices[Face.Vertices[i].Index] is ColoredVertex)
					{
						ColoredVertex v = (ColoredVertex)vertices[Face.Vertices[i].Index];
						GL.Color3(v.Color.R, v.Color.G, v.Color.B);
					}

					GL.Vertex3(vertices[Face.Vertices[i].Index].Coordinates.X, vertices[Face.Vertices[i].Index].Coordinates.Y, -vertices[Face.Vertices[i].Index].Coordinates.Z);
				}

				GL.End();
				RestoreBlendFunc();
				RestoreAlphaFunc();
			}

			GL.Disable(EnableCap.Texture2D);

			// normals
			if (OptionNormals)
			{
				for (int i = 0; i < Face.Vertices.Length; i++)
				{
					GL.Begin(PrimitiveType.Lines);
					GL.Color4(new Color4(material.Color.R, material.Color.G, material.Color.B, 255));
					GL.Vertex3(vertices[Face.Vertices[i].Index].Coordinates.X, vertices[Face.Vertices[i].Index].Coordinates.Y, -vertices[Face.Vertices[i].Index].Coordinates.Z);
					GL.Vertex3(vertices[Face.Vertices[i].Index].Coordinates.X + Face.Vertices[i].Normal.X, vertices[Face.Vertices[i].Index].Coordinates.Y + +Face.Vertices[i].Normal.Y, -(vertices[Face.Vertices[i].Index].Coordinates.Z + Face.Vertices[i].Normal.Z));
					GL.End();
				}
			}

			// finalize
			if (OptionWireFrame || IsDebugTouchMode)
			{
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
			}

			if (material.BlendMode == MeshMaterialBlendMode.Additive)
			{
				RestoreBlendFunc();
			}

			GL.PopMatrix();

			GL.MatrixMode(MatrixMode.Modelview);
			GL.PopMatrix();

			GL.MatrixMode(MatrixMode.Projection);
			GL.PopMatrix();
		}
	}
}
