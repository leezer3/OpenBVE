using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
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
using LibRender2.Text;
using LibRender2.Textures;
using LibRender2.Viewports;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Textures;
using OpenBveApi.World;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Path = OpenBveApi.Path;
using PixelFormat = OpenBveApi.Textures.PixelFormat;
using Vector2 = OpenBveApi.Math.Vector2;
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
		/// <summary>The host filesystem</summary>
		internal FileSystem fileSystem;

		/// <summary>Holds a reference to the current options</summary>
		internal BaseOptions currentOptions;

		public List<ObjectState> StaticObjectStates;
		public List<ObjectState> DynamicObjectStates;
		public VisibleObjectLibrary VisibleObjects;

		protected int[] ObjectsSortedByStart;
		protected int[] ObjectsSortedByEnd;
		protected int ObjectsSortedByStartPointer;
		protected int ObjectsSortedByEndPointer;
		protected internal double LastUpdatedTrackPosition;
		/// <summary>Whether ReShade is in use</summary>
		/// <remarks>Don't use OpenGL error checking with ReShade, as this breaks</remarks>
		public bool ReShadeInUse;
		/// <summary>A dummy VAO used when working with procedural data within the shader</summary>
		public VertexArrayObject dummyVao;

		public Screen Screen;

		/// <summary>The track follower for the main camera</summary>
		public TrackFollower CameraTrackFollower;

		public bool RenderThreadJobWaiting;

		/// <summary>Holds a reference to the current interface type of the game (Used by the renderer)</summary>
		public InterfaceType CurrentInterface
		{
			get => currentInterface;
			set
			{
				previousInterface = currentInterface;
				currentInterface = value;
			}
		}

		/// <summary>Gets the scale factor for the current display</summary>
		public static Vector2 ScaleFactor
		{
			get
			{
				if (_scaleFactor.X > 0)
				{
					return _scaleFactor;
				}
				// guard against the fact that some systems return a scale factor of zero
				_scaleFactor = new Vector2(Math.Max(DisplayDevice.Default.ScaleFactor.X, 1), Math.Max(DisplayDevice.Default.ScaleFactor.Y, 1));
				return _scaleFactor;
			}
		}

		private static Vector2 _scaleFactor = new Vector2(-1, -1);

		/// <summary>Holds a reference to the previous interface type of the game</summary>
		public InterfaceType PreviousInterface => previousInterface;

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
		public Particle Particle;
		public Loading Loading;
		public Keys Keys;
		public MotionBlur MotionBlur;
		public Fonts Fonts;

		public Matrix4D CurrentProjectionMatrix;
		public Matrix4D CurrentViewMatrix;

		public Vector3 TransformedLightPosition;

		protected List<Matrix4D> projectionMatrixList;
		protected List<Matrix4D> viewMatrixList;

		public List<int> usedTrackColors = new List<int>();
		public Dictionary<int, RailPath> trackColors = new Dictionary<int, RailPath>();

#pragma warning disable 0219, CS0169
		/// <summary>Holds the last openGL error</summary>
		/// <remarks>Is only used in debug builds, hence the pragma</remarks>
		private ErrorCode lastError;
#pragma warning restore 0219, CS0169

		/// <summary>The current shader in use</summary>
		protected internal Shader CurrentShader;

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

		protected internal Texture _programLogo;

		protected internal Texture whitePixel;

		private bool logoError;

		/// <summary>Gets the current program logo</summary>
		public Texture ProgramLogo
		{
			get
			{
				if (_programLogo != null || logoError)
				{
					return _programLogo;
				}
				try
				{
					if (Screen.Width > 1024)
					{
						currentHost.RegisterTexture(Path.CombineFile(fileSystem.GetDataFolder("In-game"), "logo_1024.png"), TextureParameters.NoChange, out _programLogo, true);
					}
					else if (Screen.Width > 512)
					{
						currentHost.RegisterTexture(Path.CombineFile(fileSystem.GetDataFolder("In-game"), "logo_512.png"), TextureParameters.NoChange, out _programLogo, true);
					}
					else
					{
						currentHost.RegisterTexture(Path.CombineFile(fileSystem.GetDataFolder("In-game"), "logo_256.png"), TextureParameters.NoChange, out _programLogo, true);
					}
				}
				catch
				{
					_programLogo = null;
					logoError = true;
				}
				return _programLogo;
			}
		}

		/// <summary>A joystick icon</summary>
		public Texture JoystickTexture;
		/// <summary>A keyboard icon</summary>
		public Texture KeyboardTexture;
		/// <summary>A generic gamepad icon</summary>
		public Texture GamepadTexture;
		/// <summary>An XInput gamepad icon</summary>
		public Texture XInputTexture;
		/// <summary>A Mascon 2-Handle controller icon</summary>
		public Texture MasconTeture;
		/// <summary>A raildriver icon</summary>
		public Texture RailDriverTexture;
		/// <summary>The game window</summary>
		public GameWindow GameWindow;
		/// <summary>The graphics mode in use</summary>
		public GraphicsMode GraphicsMode;
		public bool LoadLogo()
		{
			return currentHost.LoadTexture(ref _programLogo, OpenGlTextureWrapMode.ClampClamp);
		}

		/*
		 * List of VBO and IBO to delete on the next frame pass
		 * This needs to be done here as opposed to in the finalizer
		 */
		internal static readonly List<int> vaoToDelete = new List<int>();
		internal static readonly List<int> vboToDelete = new List<int>();
		internal static readonly List<int> iboToDelete = new List<int>();

		public bool AvailableNewRenderer => currentOptions != null && currentOptions.IsUseNewRenderer && !ForceLegacyOpenGL;

		protected BaseRenderer(HostInterface CurrentHost, BaseOptions CurrentOptions, FileSystem FileSystem)
		{
			currentHost = CurrentHost;
			currentOptions = CurrentOptions;
			fileSystem = FileSystem;
			if (CurrentHost.Application != HostApplication.TrainEditor)
			{
				/*
				 * TrainEditor2 uses a GLControl
				 * On the Linux SLD2 backend, this crashes when attempting to get the list of supported screen resolutions
				 * As we don't care about fullscreen here, just don't bother with this constructor
				 */
				Screen = new Screen();
			}
			Camera = new CameraProperties(this);
			Lighting = new Lighting(this);
			Marker = new Marker();

			projectionMatrixList = new List<Matrix4D>();
			viewMatrixList = new List<Matrix4D>();
			Fonts = new Fonts(currentHost, currentOptions.Font);
			VisibilityThread = new Thread(vt);
			VisibilityThread.Start();
			RenderThreadJobs = new ConcurrentQueue<ThreadStart>();
		}

		~BaseRenderer()
		{
			DeInitialize();
		}

		/// <summary>Call this once to initialise the renderer</summary>
		/// <remarks>A call to DeInitialize should be made when closing the progam to release resources</remarks>
		[HandleProcessCorruptedStateExceptions] //As some graphics cards crash really nastily if we request unsupported features
		public virtual void Initialize()
		{
			if (!ForceLegacyOpenGL && (currentOptions.IsUseNewRenderer || currentHost.Application != HostApplication.OpenBve)) // GL3 has already failed. Don't trigger unneccessary exceptions
			{
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
					dummyVao = new VertexArrayObject();
				}
				catch
				{
					currentHost.AddMessage(MessageType.Error, false, "Initializing the default shaders failed- Falling back to legacy openGL.");
					currentOptions.IsUseNewRenderer = false;
					ForceLegacyOpenGL = true;
					try
					{
						/*
						 * Nasty little edge case with some Intel graphics- They create the shader OK
						 * but it crashes on use, but remains active
						 * Deactivate it, otherwise we get a grey screen
						 */
						DefaultShader?.Deactivate();
					}
					catch 
					{ 
						// ignored
					}
					
				}

				if (DefaultShader == null)
				{
					// Shader failed to load, but no exception
					currentHost.AddMessage(MessageType.Error, false, "Initializing the default shaders failed- Falling back to legacy openGL.");
					currentOptions.IsUseNewRenderer = false;
					ForceLegacyOpenGL = true;
				}
			}

			Background = new Background(this);
			Fog = new Fog();
			OpenGlString = new OpenGlString(this); //text shader shares the rectangle fragment shader
			TextureManager = new TextureManager(currentHost, this);
			Cube = new Cube(this);
			Rectangle = new Rectangle(this);
			Particle = new Particle(this);
			Loading = new Loading(this);
			Keys = new Keys(this);
			MotionBlur = new MotionBlur(this);

			StaticObjectStates = new List<ObjectState>();
			DynamicObjectStates = new List<ObjectState>();
			VisibleObjects = new VisibleObjectLibrary(this);
			whitePixel = new Texture(new Texture(1, 1, PixelFormat.RGBAlpha, new byte[] {255, 255, 255, 255}, null));
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
			GL.Disable(EnableCap.Dither);
			GL.Disable(EnableCap.Lighting);
			GL.Disable(EnableCap.Fog);
			if (!AvailableNewRenderer)
			{
				GL.Disable(EnableCap.Texture2D);
				GL.Fog(FogParameter.FogMode, (int)FogMode.Linear);
			}

			// ReSharper disable once PossibleNullReferenceException
			string openGLdll = Path.CombineFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "opengl32.dll");

			if (File.Exists(openGLdll))
			{
				FileVersionInfo glVersionInfo = FileVersionInfo.GetVersionInfo(openGLdll);
				if (glVersionInfo.ProductName == @"ReShade")
				{
					ReShadeInUse = true;
				}
			}
			// icons for use in GL menus
			currentHost.RegisterTexture(Path.CombineFile(fileSystem.GetDataFolder("Menu"), "keyboard.png"), TextureParameters.NoChange, out KeyboardTexture);
			currentHost.RegisterTexture(Path.CombineFile(fileSystem.GetDataFolder("Menu"), "gamepad.png"), TextureParameters.NoChange, out GamepadTexture);
			currentHost.RegisterTexture(Path.CombineFile(fileSystem.GetDataFolder("Menu"), "xbox.png"), TextureParameters.NoChange, out XInputTexture);
			currentHost.RegisterTexture(Path.CombineFile(fileSystem.GetDataFolder("Menu"), "zuki.png"), TextureParameters.NoChange, out MasconTeture);
			currentHost.RegisterTexture(Path.CombineFile(fileSystem.GetDataFolder("Menu"), "joystick.png"), TextureParameters.NoChange, out JoystickTexture);
			currentHost.RegisterTexture(Path.CombineFile(fileSystem.GetDataFolder("Menu"), "raildriver.png"), TextureParameters.NoChange, out RailDriverTexture);
		}

		/// <summary>Deinitializes the renderer</summary>
		public void DeInitialize()
		{
			this.GameWindow?.Dispose();
			// terminate spinning thread
			visibilityThread = false;
		}

		/// <summary>Should be called when the OpenGL version is switched mid-game</summary>
		/// <remarks>We need to purge the current shader state and update lighting to avoid glitches</remarks>
		public void SwitchOpenGLVersion()
		{
			if (currentOptions.IsUseNewRenderer && AvailableNewRenderer)
			{
				DefaultShader.Activate();
				DefaultShader.Deactivate();
			}
			currentOptions.IsUseNewRenderer = !currentOptions.IsUseNewRenderer;
			Lighting.Initialize();
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
			if (!AvailableNewRenderer)
			{
				GL.Disable(EnableCap.Lighting);
				GL.Disable(EnableCap.Fog);
				GL.Disable(EnableCap.Texture2D);
			}
			
			SetBlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			UnsetBlendFunc();
			GL.Enable(EnableCap.DepthTest);
			GL.Disable(EnableCap.DepthClamp);
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
			List<ValueTuple<string, bool, DateTime>> keys = currentHost.StaticObjectCache.Keys.ToList();
			for (int i = 0; i < keys.Count; i++)
			{
				if (!File.Exists(keys[i].Item1) || File.GetLastWriteTime(keys[i].Item1) != keys[i].Item3)
				{
					currentHost.StaticObjectCache.Remove(keys[i]);
				}
			}
			TextureManager.UnloadAllTextures(true);
			VisibleObjects.Clear();
		}

		public int CreateStaticObject(StaticObject Prototype, Vector3 Position, Transformation WorldTransformation, Transformation LocalTransformation, ObjectDisposalMode AccurateObjectDisposal, double AccurateObjectDisposalZOffset, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness)
		{
			Matrix4D Translate = Matrix4D.CreateTranslation(Position.X, Position.Y, -Position.Z);
			Matrix4D Rotate = (Matrix4D)new Transformation(LocalTransformation, WorldTransformation);
			return CreateStaticObject(Position, Prototype, LocalTransformation, Rotate, Translate, AccurateObjectDisposal, AccurateObjectDisposalZOffset, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness);
		}

		public int CreateStaticObject(Vector3 Position, StaticObject Prototype, Transformation LocalTransformation, Matrix4D Rotate, Matrix4D Translate, ObjectDisposalMode AccurateObjectDisposal, double AccurateObjectDisposalZOffset, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness)
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

			if (AccurateObjectDisposal == ObjectDisposalMode.Accurate)
			{
				foreach (VertexTemplate vertex in Prototype.Mesh.Vertices)
				{
					Vector3 Coordinates = new Vector3(vertex.Coordinates);
					Coordinates.Rotate(LocalTransformation);

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

			switch (AccurateObjectDisposal)
			{
				case ObjectDisposalMode.Accurate:
					startingDistance += (float)TrackPosition;
					endingDistance += (float)TrackPosition;
					double z = BlockLength * Math.Floor(TrackPosition / BlockLength);
					StartingDistance = Math.Min(z - BlockLength, startingDistance);
					EndingDistance = Math.Max(z + 2.0 * BlockLength, endingDistance);
					startingDistance = (float)(BlockLength * Math.Floor(StartingDistance / BlockLength));
					endingDistance = (float)(BlockLength * Math.Ceiling(EndingDistance / BlockLength));
					break;
				case ObjectDisposalMode.Legacy:
					startingDistance = (float)StartingDistance;
					endingDistance = (float)EndingDistance;
					break;
				case ObjectDisposalMode.Mechanik:
					startingDistance = (float) StartingDistance;
					endingDistance = (float) EndingDistance + 1500;
					if (startingDistance < 0)
					{
						startingDistance = 0;
					}
					break;
			}
			StaticObjectStates.Add(new ObjectState
			{
				Prototype = Prototype,
				Translation = Translate,
				Rotate = Rotate,
				Brightness = Brightness,
				StartingDistance = startingDistance,
				EndingDistance = endingDistance,
				WorldPosition = Position
			});
			
			foreach (MeshFace face in Prototype.Mesh.Faces)
			{
				switch (face.Flags & FaceFlags.FaceTypeMask)
				{
					case FaceFlags.Triangles:
						InfoTotalTriangles++;
						break;
					case FaceFlags.TriangleStrip:
						InfoTotalTriangleStrip++;
						break;
					case FaceFlags.Quads:
						InfoTotalQuads++;
						break;
					case FaceFlags.QuadStrip:
						InfoTotalQuadStrip++;
						break;
					case FaceFlags.Polygon:
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
				internalObject = new ObjectState( new StaticObject(currentHost));
			}

			internalObject.Prototype.Dynamic = true;

			DynamicObjectStates.Add(internalObject);
		}

		/// <summary>Initializes the visibility of all objects within the game world</summary>
		/// <remarks>If the new renderer is enabled, this must be run in a thread processing an openGL context in order to successfully create
		/// the required VAO objects</remarks>
		public void InitializeVisibility()
		{
			if (!ForceLegacyOpenGL) // as we might want to switch renderer types
			{
				for (int i = 0; i < StaticObjectStates.Count; i++)
				{
					VAOExtensions.CreateVAO(StaticObjectStates[i].Prototype.Mesh, false, DefaultShader.VertexLayout, this);
					if (StaticObjectStates[i].Matricies != null)
					{
						GL.CreateBuffers(1, out StaticObjectStates[i].MatrixBufferIndex);
					}
				}
				for (int i = 0; i < DynamicObjectStates.Count; i++)
				{
					VAOExtensions.CreateVAO(DynamicObjectStates[i].Prototype.Mesh, false, DefaultShader.VertexLayout, this);
					if (DynamicObjectStates[i].Matricies != null)
					{
						GL.CreateBuffers(1, out DynamicObjectStates[i].MatrixBufferIndex);
					}
				}
			}
			ObjectsSortedByStart = StaticObjectStates.Select((x, i) => new { Index = i, Distance = x.StartingDistance }).OrderBy(x => x.Distance).Select(x => x.Index).ToArray();
			ObjectsSortedByEnd = StaticObjectStates.Select((x, i) => new { Index = i, Distance = x.EndingDistance }).OrderBy(x => x.Distance).Select(x => x.Index).ToArray();
			ObjectsSortedByStartPointer = 0;
			ObjectsSortedByEndPointer = 0;
			
			if (currentOptions.ObjectDisposalMode == ObjectDisposalMode.QuadTree)
			{
				foreach (ObjectState state in StaticObjectStates)
				{
					VisibleObjects.quadTree.Add(state, Orientation3.Default);
				}
				VisibleObjects.quadTree.Initialize(currentOptions.QuadTreeLeafSize);
				UpdateQuadTreeVisibility();
			}
			else
			{
				double p = CameraTrackFollower.TrackPosition + Camera.Alignment.Position.Z;
				foreach (ObjectState state in StaticObjectStates.Where(recipe => recipe.StartingDistance <= p + Camera.ForwardViewingDistance & recipe.EndingDistance >= p - Camera.BackwardViewingDistance))
				{
					VisibleObjects.ShowObject(state, ObjectType.Static);
				}
			}
		}

		private VisibilityUpdate updateVisibility;

		public bool PauseVisibilityUpdates;
		
		public bool visibilityThread = true;

		public Thread VisibilityThread;

		private void vt()
		{
			while (visibilityThread)
			{
				if (PauseVisibilityUpdates)
				{
					continue;
				}
				if (updateVisibility != VisibilityUpdate.None && CameraTrackFollower != null)
				{
					UpdateVisibility(CameraTrackFollower.TrackPosition + Camera.Alignment.Position.Z);
					updateVisibility = VisibilityUpdate.None;
				}
				else
				{
					Thread.Sleep(100);
				}
			}
		}

		public void UpdateVisibility(bool force)
		{
			updateVisibility = force ? VisibilityUpdate.Force : VisibilityUpdate.None;
		}

		private void UpdateVisibility(double TrackPosition)
		{
			if (currentOptions.ObjectDisposalMode == ObjectDisposalMode.QuadTree)
			{
				UpdateQuadTreeVisibility();
			}
			else
			{
				if (updateVisibility == VisibilityUpdate.Normal)
				{
					UpdateLegacyVisibility(TrackPosition);
				}
				else
				{
					/*
					 * The original visibility algorithm fails to handle correctly cases where the
					 * camera angle is rotated, but the track position does not change
					 *
					 * Horrible kludge...
					 */
					UpdateLegacyVisibility(TrackPosition + 0.01);
					UpdateLegacyVisibility(TrackPosition - 0.01);
				}
				
			}
		}

		private void UpdateQuadTreeVisibility()
		{
			if (VisibleObjects == null || VisibleObjects.quadTree == null)
			{
				Thread.Sleep(10);
				return;
			}
			Camera.UpdateQuadTreeLeaf();
		}

		private void UpdateLegacyVisibility(double TrackPosition)
		{
			if (ObjectsSortedByStart == null || ObjectsSortedByStart.Length == 0 || StaticObjectStates.Count == 0)
			{
				return;
			}
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
				n = ObjectsSortedByStart.Length;

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
			updateVisibility = VisibilityUpdate.Force;
		}

		/// <summary>Determines the maximum Anisotropic filtering level the system supports</summary>
		public void DetermineMaxAFLevel()
		{
			if (currentHost.Platform == HostPlatform.AppleOSX)
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
				ErrorCode error = GL.GetError();
				if (error == ErrorCode.InvalidEnum)
				{
					// Doing this on a forward compatible GL context fails with invalid enum
					currentOptions.AnisotropicFilteringMaximum = 16;
					return;
				}
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
			CurrentProjectionMatrix = Matrix4D.CreatePerspectiveFieldOfView(Camera.VerticalViewingAngle, Screen.AspectRatio, 0.2, currentOptions.ViewingDistance);
		}

		public void ResetShader(Shader shader)
		{
#if DEBUG
			if (!ReShadeInUse)
			{
				lastError = GL.GetError();

				if (lastError != ErrorCode.NoError)
				{
					throw new InvalidOperationException($"OpenGL Error: {lastError}");
				}
			}
#endif

			shader.SetCurrentProjectionMatrix(Matrix4D.Identity);
			shader.SetCurrentModelViewMatrix(Matrix4D.Identity);
			shader.SetCurrentTextureMatrix(Matrix4D.Identity);
			shader.SetIsLight(false);
			shader.SetLightPosition(Vector3.Zero);
			shader.SetLightAmbient(Color24.White);
			shader.SetLightDiffuse(Color24.White);
			shader.SetLightSpecular(Color24.White);
			shader.SetLightModel(Lighting.LightModel);
			shader.SetMaterialAmbient(Color24.White);
			shader.SetMaterialDiffuse(Color24.White);
			shader.SetMaterialSpecular(Color24.White);
			shader.SetMaterialEmission(Color24.White);
			lastColor = Color32.White;
			shader.SetMaterialShininess(1.0f);
			shader.SetIsFog(false);
			shader.DisableTexturing();
			shader.SetTexture(0);
			shader.SetBrightness(1.0f);
			shader.SetOpacity(1.0f);
			shader.SetObjectIndex(0);
			shader.SetAlphaTest(false);
		}

		public void SetBlendFunc()
		{
			SetBlendFunc(blendSrcFactor, blendDestFactor);
		}

		public void SetBlendFunc(BlendingFactor srcFactor, BlendingFactor destFactor)
		{
			blendEnabled = true;
			blendSrcFactor = srcFactor;
			blendDestFactor = destFactor;
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(srcFactor, destFactor);
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
		/// <param name="comparison">The comparison to use</param>
		/// <param name="value">The value to compare</param>
		public void SetAlphaFunc(AlphaFunction comparison, float value)
		{
			alphaTestEnabled = true;
			alphaFuncComparison = comparison;
			alphaFuncValue = value;
			if (AvailableNewRenderer)
			{
				CurrentShader.SetAlphaTest(true);
				CurrentShader.SetAlphaFunction(comparison, value);
			}
			else
			{
				GL.Enable(EnableCap.AlphaTest);
				GL.AlphaFunc(comparison, value);	
			}
			
		}

		/// <summary>Disables OpenGL alpha testing</summary>
		public void UnsetAlphaFunc()
		{
			alphaTestEnabled = false;
			if (AvailableNewRenderer)
			{
				CurrentShader.SetAlphaTest(false);
			}
			else
			{
				GL.Disable(EnableCap.AlphaTest);	
			}
			
		}

		/// <summary>Restores the OpenGL alpha function to it's previous state</summary>
		public void RestoreAlphaFunc()
		{
			if (alphaTestEnabled)
			{
				if (AvailableNewRenderer)
				{
					CurrentShader.SetAlphaTest(true);
					CurrentShader.SetAlphaFunction(alphaFuncComparison, alphaFuncValue);
				}
				else
				{
					GL.Enable(EnableCap.AlphaTest);
					GL.AlphaFunc(alphaFuncComparison, alphaFuncValue);
				}
				
			}
			else
			{
				if (AvailableNewRenderer)
				{
					CurrentShader.SetAlphaTest(false);
				}
				else
				{
					GL.Disable(EnableCap.AlphaTest);
				}
			}
		}


		// Cached object state and matricies for shader drawing
		protected internal ObjectState lastObjectState;
		private Matrix4D lastModelMatrix;
		private Matrix4D lastModelViewMatrix;
		private bool sendToShader;

		/// <summary>Draws a face using the current shader</summary>
		/// <param name="state">The FaceState to draw</param>
		/// <param name="isDebugTouchMode">Whether debug touch mode</param>
		public void RenderFace(FaceState state, bool isDebugTouchMode = false)
		{
			RenderFace(CurrentShader, state.Object, state.Face, isDebugTouchMode);
		}

		/// <summary>Draws a face using the specified shader and matricies</summary>
		/// <param name="shader">The shader to use</param>
		/// <param name="state">The ObjectState to draw</param>
		/// <param name="face">The Face within the ObjectState</param>
		/// <param name="modelMatrix">The model matrix to use</param>
		/// <param name="modelViewMatrix">The modelview matrix to use</param>
		public void RenderFace(Shader shader, ObjectState state, MeshFace face, Matrix4D modelMatrix, Matrix4D modelViewMatrix)
		{
			lastModelMatrix = modelMatrix;
			lastModelViewMatrix = modelViewMatrix;
			sendToShader = true;
			RenderFace(shader, state, face, false, true);
		}

		/// <summary>Draws a face using the specified shader</summary>
		/// <param name="shader">The shader to use</param>
		/// <param name="state">The ObjectState to draw</param>
		/// <param name="face">The Face within the ObjectState</param>
		/// <param name="debugTouchMode">Whether debug touch mode</param>
		/// <param name="screenSpace">Used when a forced matrix, for items which are in screen space not camera space</param>
		public void RenderFace(Shader shader, ObjectState state, MeshFace face, bool debugTouchMode = false, bool screenSpace = false)
		{
			if ((state != lastObjectState || state.Prototype.Dynamic) && !screenSpace)
			{
				lastModelMatrix = state.ModelMatrix * Camera.TranslationMatrix;
				lastModelViewMatrix = lastModelMatrix * CurrentViewMatrix;
				sendToShader = true;
			}

			if (state.Prototype.Mesh.Vertices.Length < 1)
			{
				return;
			}

			MeshMaterial material = state.Prototype.Mesh.Materials[face.Material];
			VertexArrayObject VAO = (VertexArrayObject)state.Prototype.Mesh.VAO;

			if (lastVAO != VAO.handle)
			{
				VAO.Bind();
				lastVAO = VAO.handle;
			}

			if (!OptionBackFaceCulling || (face.Flags & FaceFlags.Face2Mask) != 0)
			{
				GL.Disable(EnableCap.CullFace);
			}
			else if (OptionBackFaceCulling)
			{
				if ((face.Flags & FaceFlags.Face2Mask) == 0)
				{
					GL.Enable(EnableCap.CullFace);
				}
			}

			// model matricies
			if (state.Matricies != null && state.Matricies.Length > 0 && state != lastObjectState)
			{
				// n.b. if buffer has no data in it (matricies are of zero length), attempting to bind generates an InvalidValue
				shader.SetCurrentAnimationMatricies(state);
				GL.BindBufferBase(BufferTarget.ShaderStorageBuffer, 12, state.MatrixBufferIndex);
			}

			// matrix
			if (sendToShader)
			{
				shader.SetCurrentModelViewMatrix(lastModelViewMatrix);
				shader.SetCurrentTextureMatrix(state.TextureTranslation);
				sendToShader = false;
			}
			
			if (OptionWireFrame || debugTouchMode)
			{
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
			}
			
			// lighting
			shader.SetMaterialFlags(material.Flags);
			if (OptionLighting)
			{
				if (material.Color != lastColor)
				{
					shader.SetMaterialAmbient(material.Color);
					shader.SetMaterialDiffuse(material.Color);
					shader.SetMaterialSpecular(material.Color);
					//TODO: Ambient and specular colors are not set by any current parsers
				}
				if ((material.Flags & MaterialFlags.Emissive) != 0)
				{
					shader.SetMaterialEmission(material.EmissiveColor);
				}

				shader.SetMaterialShininess(1.0f);
			}
			else
			{
				if (material.Color != lastColor)
				{
					shader.SetMaterialAmbient(material.Color);
				}
			}

			lastColor = material.Color;
			PrimitiveType drawMode;

			switch (face.Flags & FaceFlags.FaceTypeMask)
			{
				case FaceFlags.Triangles:
					drawMode = PrimitiveType.Triangles;
					break;
				case FaceFlags.TriangleStrip:
					drawMode = PrimitiveType.TriangleStrip;
					break;
				case FaceFlags.Quads:
					drawMode = PrimitiveType.Quads;
					break;
				case FaceFlags.QuadStrip:
					drawMode = PrimitiveType.QuadStrip;
					break;
				default:
					drawMode = PrimitiveType.Polygon;
					break;
			}

			// blend factor
			float distanceFactor;
			if (material.GlowAttenuationData != 0)
			{
				distanceFactor = (float)Glow.GetDistanceFactor(lastModelMatrix, state.Prototype.Mesh.Vertices, ref face, material.GlowAttenuationData);
			}
			else
			{
				distanceFactor = 1.0f;
			}

			float blendFactor = inv255 * state.DaytimeNighttimeBlend + 1.0f - Lighting.OptionLightingResultingAmount;
			if (blendFactor > 1.0)
			{
				blendFactor = 1.0f;
			}

			// daytime polygon
			{
				// texture
				// ReSharper disable once PossibleInvalidOperationException
				if (material.DaytimeTexture != null && currentHost.LoadTexture(ref material.DaytimeTexture, (OpenGlTextureWrapMode)material.WrapMode))
				{
					if (LastBoundTexture != material.DaytimeTexture.OpenGlTextures[(int)material.WrapMode])
					{
						GL.BindTexture(TextureTarget.Texture2D,
							material.DaytimeTexture.OpenGlTextures[(int)material.WrapMode].Name);
						LastBoundTexture = material.DaytimeTexture.OpenGlTextures[(int)material.WrapMode];
					}
				}
				else
				{
					shader.DisableTexturing();
				}
				// Calculate the brightness of the poly to render
				float factor;
				if (material.BlendMode == MeshMaterialBlendMode.Additive)
				{
					//Additive blending- Full brightness
					factor = 1.0f;
					GL.Enable(EnableCap.Blend);
					GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
					shader.SetIsFog(false);
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
				shader.SetBrightness(factor);

				float alphaFactor = distanceFactor;
				if (material.NighttimeTexture != null && (material.Flags & MaterialFlags.CrossFadeTexture) != 0)
				{
					alphaFactor *= 1.0f - blendFactor;
				}

				shader.SetOpacity(inv255 * material.Color.A * alphaFactor);

				// render polygon
				VAO.Draw(drawMode, face.IboStartIndex, face.Vertices.Length);
			}

			// nighttime polygon
			if (blendFactor != 0 && material.NighttimeTexture != null && material.NighttimeTexture != material.DaytimeTexture && currentHost.LoadTexture(ref material.NighttimeTexture, (OpenGlTextureWrapMode)material.WrapMode))
			{
				// texture
				if (LastBoundTexture != material.NighttimeTexture.OpenGlTextures[(int)material.WrapMode])
				{
					GL.BindTexture(TextureTarget.Texture2D, material.NighttimeTexture.OpenGlTextures[(int)material.WrapMode].Name);
					LastBoundTexture = material.NighttimeTexture.OpenGlTextures[(int)material.WrapMode];
				}


				GL.Enable(EnableCap.Blend);

				// alpha test
				shader.SetAlphaTest(true);
				shader.SetAlphaFunction(AlphaFunction.Greater, 0.0f);
				
				// blend mode
				float alphaFactor = distanceFactor * blendFactor;

				shader.SetOpacity(inv255 * material.Color.A * alphaFactor);

				// render polygon
				VAO.Draw(drawMode, face.IboStartIndex, face.Vertices.Length);
				RestoreBlendFunc();
				RestoreAlphaFunc();
			}


			// normals
			if (OptionNormals)
			{
				shader.DisableTexturing();
				shader.SetBrightness(1.0f);
				shader.SetOpacity(1.0f);
				VertexArrayObject normalsVao = (VertexArrayObject)state.Prototype.Mesh.NormalsVAO;
				normalsVao.Bind();
				lastVAO = normalsVao.handle;
				normalsVao.Draw(PrimitiveType.Lines, face.NormalsIboStartIndex, face.Vertices.Length * 2);
			}

			// finalize
			if (material.BlendMode == MeshMaterialBlendMode.Additive)
			{
				RestoreBlendFunc();
				shader.SetIsFog(OptionFog);
			}
			if (OptionWireFrame || debugTouchMode)
			{
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
			}
			lastObjectState = state;
		}

		public void RenderFaceImmediateMode(FaceState state, bool debugTouchMode = false)
		{
			RenderFaceImmediateMode(state.Object, state.Face, debugTouchMode);
		}

		public void RenderFaceImmediateMode(ObjectState state, MeshFace face, bool debugTouchMode = false)
		{
			Matrix4D modelMatrix = state.ModelMatrix * Camera.TranslationMatrix;
			Matrix4D modelViewMatrix = modelMatrix * CurrentViewMatrix;
			RenderFaceImmediateMode(state, face, modelMatrix, modelViewMatrix, debugTouchMode);
		}

		public void RenderFaceImmediateMode(ObjectState state, MeshFace face, Matrix4D modelMatrix, Matrix4D modelViewMatrix, bool debugTouchMode = false)
		{
			if (state.Prototype.Mesh.Vertices.Length < 1)
			{
				return;
			}

			VertexTemplate[] vertices = state.Prototype.Mesh.Vertices;
			MeshMaterial material = state.Prototype.Mesh.Materials[face.Material];

			if (!OptionBackFaceCulling || (face.Flags & FaceFlags.Face2Mask) != 0)
			{
				GL.Disable(EnableCap.CullFace);
			}
			else if (OptionBackFaceCulling)
			{
				if ((face.Flags & FaceFlags.Face2Mask) == 0)
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
				fixed (double* matrixPointer = &state.TextureTranslation.Row0.X)
				{
					GL.LoadMatrix(matrixPointer);
				}

			}

			if (OptionWireFrame || debugTouchMode)
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

			GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, (material.Flags & MaterialFlags.Emissive) != 0 ? new Color4(material.EmissiveColor.R, material.EmissiveColor.G, material.EmissiveColor.B, 255) : Color4.Black);

			// fog
			if (OptionFog)
			{
				GL.Enable(EnableCap.Fog);
			}

			PrimitiveType drawMode;

			switch (face.Flags & FaceFlags.FaceTypeMask)
			{
				case FaceFlags.Triangles:
					drawMode = PrimitiveType.Triangles;
					break;
				case FaceFlags.TriangleStrip:
					drawMode = PrimitiveType.TriangleStrip;
					break;
				case FaceFlags.Quads:
					drawMode = PrimitiveType.Quads;
					break;
				case FaceFlags.QuadStrip:
					drawMode = PrimitiveType.QuadStrip;
					break;
				default:
					drawMode = PrimitiveType.Polygon;
					break;
			}

			// blend factor
			float distanceFactor;
			if (material.GlowAttenuationData != 0)
			{
				distanceFactor = (float)Glow.GetDistanceFactor(modelMatrix, vertices, ref face, material.GlowAttenuationData);
			}
			else
			{
				distanceFactor = 1.0f;
			}

			float blendFactor = inv255 * state.DaytimeNighttimeBlend + 1.0f - Lighting.OptionLightingResultingAmount;
			if (blendFactor > 1.0)
			{
				blendFactor = 1.0f;
			}

			// daytime polygon
			{
				// texture
				if (material.DaytimeTexture != null)
				{
					// ReSharper disable once PossibleInvalidOperationException
					if (currentHost.LoadTexture(ref material.DaytimeTexture, (OpenGlTextureWrapMode)material.WrapMode))
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

				GL.Begin(drawMode);

				if (OptionWireFrame)
				{
					GL.Color4(inv255 * material.Color.R * factor, inv255 * material.Color.G * factor, inv255 * material.Color.B * factor, 1.0f);
				}
				else
				{
					GL.Color4(inv255 * material.Color.R * factor, inv255 * material.Color.G * factor, inv255 * material.Color.B * factor, inv255 * material.Color.A * alphaFactor);
				}

				for (int i = 0; i < face.Vertices.Length; i++)
				{
					GL.Normal3(face.Vertices[i].Normal.X, face.Vertices[i].Normal.Y, -face.Vertices[i].Normal.Z);
					GL.TexCoord2(vertices[face.Vertices[i].Index].TextureCoordinates.X, vertices[face.Vertices[i].Index].TextureCoordinates.Y);

					if (vertices[face.Vertices[i].Index] is ColoredVertex v)
					{
						GL.Color4(v.Color.R, v.Color.G, v.Color.B, v.Color.A);
					}

					GL.Vertex3(vertices[face.Vertices[i].Index].Coordinates.X, vertices[face.Vertices[i].Index].Coordinates.Y, -vertices[face.Vertices[i].Index].Coordinates.Z);
				}

				GL.End();
			}

			// nighttime polygon
			if (blendFactor != 0 && material.NighttimeTexture != null && currentHost.LoadTexture(ref material.NighttimeTexture, (OpenGlTextureWrapMode)material.WrapMode))
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

				GL.Begin(drawMode);

				if (OptionWireFrame)
				{
					GL.Color4(inv255 * material.Color.R, inv255 * material.Color.G, inv255 * material.Color.B, 1.0f);
				}
				else
				{
					GL.Color4(inv255 * material.Color.R, inv255 * material.Color.G, inv255 * material.Color.B, inv255 * material.Color.A * alphaFactor);
				}

				for (int i = 0; i < face.Vertices.Length; i++)
				{
					GL.Normal3(face.Vertices[i].Normal.X, face.Vertices[i].Normal.Y, -face.Vertices[i].Normal.Z);
					GL.TexCoord2(vertices[face.Vertices[i].Index].TextureCoordinates.X, vertices[face.Vertices[i].Index].TextureCoordinates.Y);

					if (vertices[face.Vertices[i].Index] is ColoredVertex v)
					{
						GL.Color3(v.Color.R, v.Color.G, v.Color.B);
					}

					GL.Vertex3(vertices[face.Vertices[i].Index].Coordinates.X, vertices[face.Vertices[i].Index].Coordinates.Y, -vertices[face.Vertices[i].Index].Coordinates.Z);
				}

				GL.End();
				RestoreBlendFunc();
				RestoreAlphaFunc();
			}

			GL.Disable(EnableCap.Texture2D);

			// normals
			if (OptionNormals)
			{
				for (int i = 0; i < face.Vertices.Length; i++)
				{
					GL.Begin(PrimitiveType.Lines);
					GL.Color4(new Color4(material.Color.R, material.Color.G, material.Color.B, 255));
					GL.Vertex3(vertices[face.Vertices[i].Index].Coordinates.X, vertices[face.Vertices[i].Index].Coordinates.Y, -vertices[face.Vertices[i].Index].Coordinates.Z);
					GL.Vertex3(vertices[face.Vertices[i].Index].Coordinates.X + face.Vertices[i].Normal.X, vertices[face.Vertices[i].Index].Coordinates.Y + +face.Vertices[i].Normal.Y, -(vertices[face.Vertices[i].Index].Coordinates.Z + face.Vertices[i].Normal.Z));
					GL.End();
				}
			}

			// finalize
			if (OptionWireFrame || debugTouchMode)
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

		/// <summary>Sets the current MouseCursor</summary>
		/// <param name="newCursor">The new cursor</param>
		public void SetCursor(OpenTK.MouseCursor newCursor)
		{
			GameWindow.Cursor = newCursor;
		}

		/// <summary>Sets the window state</summary>
		/// <param name="windowState">The new window state</param>
		public void SetWindowState(WindowState windowState)
		{
			GameWindow.WindowState = windowState;
			if (windowState == WindowState.Fullscreen)
			{
				// move origin appropriately
				GameWindow.X = 0;
				GameWindow.Y = 0;
			}
		}

		/// <summary>Sets the size of the window</summary>
		/// <param name="width">The new width</param>
		/// <param name="height">The new height</param>
		public void SetWindowSize(int width, int height)
		{
			GameWindow.Width = width;
			GameWindow.Height = height;
			Screen.Width = width;
			Screen.Height = height;

			if (width == DisplayDevice.Default.Width && height == DisplayDevice.Default.Height)
			{
				SetWindowState(WindowState.Maximized);
			}
		}

		public ConcurrentQueue<ThreadStart> RenderThreadJobs;

		/// <summary>This method is used during loading to run commands requiring an OpenGL context in the main render loop</summary>
		/// <param name="job">The OpenGL command</param>
		/// <param name="timeout">The timeout</param>
		public void RunInRenderThread(ThreadStart job, int timeout)
		{
			RenderThreadJobs.Enqueue(job);
			//Don't set the job to available until after it's been loaded into the queue
			RenderThreadJobWaiting = true;
			//Failsafe: If our job has taken more than the timeout, stop waiting for it
			//A missing texture is probably better than an infinite loadscreen
			lock (job)
			{
				Monitor.Wait(job, timeout);
			}
		}
	}
}
