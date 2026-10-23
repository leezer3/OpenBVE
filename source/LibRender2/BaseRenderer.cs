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
using LibRender2.ShadowMapping;
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
		public Vector2 ScaleFactor
		{
			get
			{
				if (currentHost.Application == HostApplication.TrainEditor || currentHost.Application == HostApplication.TrainEditor2)
				{
					// accessing display device under SDL2 GLControl fails, scale not supported here anyways
					return Vector2.One;
				}
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
#if RELEASE
#pragma warning disable 0219, CS0169
#endif
		/// <summary>Holds the last openGL error</summary>
		/// <remarks>Is only used in debug builds, hence the pragma</remarks>
		private ErrorCode lastError;
		/// <summary>The last openGL error which was reported to the user / log</summary>
		private ErrorCode lastReportedError = ErrorCode.NoError;
#if RELEASE
#pragma warning restore 0219, CS0169
#endif

		/// <summary>The current shader in use</summary>
		public AbstractShader CurrentShader;

		public Shader DefaultShader;
		
		/// <summary>Manages the Cascaded Shadow Mapping (CSM) system.</summary>
		public Shadows Shadows;

		/// <summary>Whether shadows are enabled.</summary>
		public bool ShadowsEnabled => Shadows?.Enabled ?? false;

		/// <summary>Shadow strength: 0=invisible, 1=full darkness.</summary>
		public float ShadowStrength => Shadows?.Strength ?? 0.7f;

		/// <summary>Whether lighting is enabled in the debug options</summary>
		public bool OptionLighting = true;

		/// <summary>Whether normals rendering is enabled in the debug options</summary>
		private bool optionNormals = false;

		/// <summary>Whether normals rendering is enabled in the debug options</summary>
		/// <remarks>Disposing the normals VAO when disabled frees the duplicate vertex buffer held in RAM.</remarks>
		public bool OptionNormals
		{
			get => optionNormals;
			set
			{
				if (value == optionNormals)
				{
					return;
				}

				optionNormals = value;
				if (!value)
				{
					DisposeNormalsVAOs();
					// Force the GC so the freed normals buffers are gone from RAM immediately.
					GC.Collect();
					GC.WaitForPendingFinalizers();
				}
			}
		}

		private void DisposeNormalsVAOs()
		{
			if (StaticObjectStates != null)
			{
				foreach (var state in StaticObjectStates)
				{
					DisposeNormalsVAO(state);
				}
			}

			if (DynamicObjectStates != null)
			{
				foreach (var state in DynamicObjectStates)
				{
					DisposeNormalsVAO(state);
				}
			}
		}

		private static void DisposeNormalsVAO(ObjectState state)
		{
			if (state?.Prototype?.Mesh?.NormalsVAO is VertexArrayObject normalsVao)
			{
				normalsVao.UnBind();
				normalsVao.Dispose();
				state.Prototype.Mesh.NormalsVAO = null;
			}
		}

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

		internal Color32 lastColor;

		/// <summary>Holds the handle of the last VAO bound by openGL</summary>
		public int lastVAO;

		protected internal Texture _programLogo;

		protected internal Texture whitePixel;
		/// <summary>A dummy 1x1 depth texture with comparison enabled, used when shadows are disabled to satisfy driver requirements.</summary>
		internal int nullDepthMap; 

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
		public Texture MasconTexture;
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

		public Dictionary<Texture, HashSet<Vector3>> CubesToDraw = new Dictionary<Texture, HashSet<Vector3>>();
		
		protected BaseRenderer(HostInterface CurrentHost, BaseOptions CurrentOptions, FileSystem FileSystem)
		{
			currentHost = CurrentHost;
			currentOptions = CurrentOptions;
			fileSystem = FileSystem;
			Screen = new Screen(this);
			Camera = new CameraProperties(this);
			Lighting = new Lighting(this);
			Marker = new Marker(this);
			Shadows = new Shadows(this);

			projectionMatrixList = new List<Matrix4D>();
			viewMatrixList = new List<Matrix4D>();
			Fonts = new Fonts(currentHost, this, CurrentOptions.Font);
			VisibilityThread = new Thread(RunVisibiliityThread);
			VisibilityThread.Start();
			RenderThreadJobs = new ConcurrentQueue<ThreadStart>();
		}


		/// <summary>Call this once to initialise the renderer</summary>
		/// <remarks>A call to DeInitialize should be made when closing the program to release resources</remarks>
		[HandleProcessCorruptedStateExceptions] //As some graphics cards crash really nastily if we request unsupported features
		public virtual void Initialize()
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
				currentHost.AddMessage(MessageType.Error, false, "Initializing the default shaders failed.");
				GL.GetError();
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
					GL.GetError();
				}

			}

			if (DefaultShader == null)
			{
				// Shader failed to load, but no exception
				currentHost.AddMessage(MessageType.Error, false, "Initializing the default shaders failed.");
			}

            Background = new Background(this);
			Fog = new Fog(this);
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
			whitePixel = new Texture(new Texture(1, 1, PixelFormat.RGBAlpha, new byte[] {255, 255, 255, 255}, (OpenBveApi.Colors.Color24[])null));
			nullDepthMap = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, nullDepthMap);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent16, 1, 1, 0, OpenTK.Graphics.OpenGL.PixelFormat.DepthComponent, PixelType.UnsignedShort, IntPtr.Zero);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.CompareRefToTexture);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
			GL.BindTexture(TextureTarget.Texture2D, 0);
			GL.ClearColor(currentOptions.ClearColor.R * inv255, currentOptions.ClearColor.G * inv255, currentOptions.ClearColor.B * inv255, 1.0f);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Lequal);
			SetBlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			if (currentOptions.ForceForwardsCompatibleContext == false)
			{
				// not valid with a forwards compatible context, so don't generate spurious error
				GL.Hint(HintTarget.FogHint, HintMode.Fastest);
				GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Fastest);
				GL.Hint(HintTarget.GenerateMipmapHint, HintMode.Nicest);
				GL.Disable(EnableCap.Lighting);
				GL.Disable(EnableCap.Fog);
				GL.Hint(HintTarget.LineSmoothHint, HintMode.Fastest);
				GL.Hint(HintTarget.PointSmoothHint, HintMode.Fastest);
				GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Fastest);
			}

			GL.Enable(EnableCap.CullFace);
			GL.CullFace(CullFaceMode.Front);
			GL.Disable(EnableCap.Dither);
			
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
			currentHost.RegisterTexture(Path.CombineFile(fileSystem.GetDataFolder("Menu"), "zuki.png"), TextureParameters.NoChange, out MasconTexture);
			currentHost.RegisterTexture(Path.CombineFile(fileSystem.GetDataFolder("Menu"), "joystick.png"), TextureParameters.NoChange, out JoystickTexture);
			currentHost.RegisterTexture(Path.CombineFile(fileSystem.GetDataFolder("Menu"), "raildriver.png"), TextureParameters.NoChange, out RailDriverTexture);

			Lighting.Initialize();
			Shadows.Initialize();
        }

		/// <summary>Initializes (or reinitializes) shadow mapping from current options.</summary>
		public void InitializeShadows() => Shadows.Initialize();

		/// <summary>Disposes all shadow GPU resources.</summary>
		public void DisposeShadows() => Shadows.Dispose();

		/// <summary>
		/// Call this when the user changes shadow settings at runtime
		/// (e.g. from an in-game options menu). This will recreate
		/// GPU resources to match the new settings.
		/// </summary>
		public void ReloadShadowSettings()
		{
			fileSystem.AppendToLogFile("[CSM] Reloading shadow settings from options...");
			InitializeShadows();
		}

		/// <summary>Deinitializes the renderer</summary>
		public void DeInitialize()
		{
			// Flush all in-memory caches on shutdown (close window / process exit).
			// Must run before GameWindow.Dispose() while the GL context is still alive.
			try
			{
				TextureManager?.UnloadAllTextures(false);
			}
			catch
			{
				// Ignored - best effort cleanup during shutdown
			}
			try
			{
				currentHost?.ClearObjectCaches();
				currentHost?.ClearErrors();
			}
			catch
			{
				// Ignored - best effort cleanup during shutdown
			}
			if (nullDepthMap != 0)
			{
				GL.DeleteTexture(nullDepthMap);
				nullDepthMap = 0;
			}
			GameWindow?.Dispose();
			// terminate spinning thread
			VisibilityThreadShouldRun = false;
		}
		
		/// <summary>Performs the CSM shadow depth rendering pass for all geometry.</summary>
		protected void PerformCSMShadowPass() => Shadows.RenderPass();

		/// <summary>Binds cascading shadow data to the default shader.</summary>
		protected void BindCSMToDefaultShader() => Shadows.Bind(DefaultShader);

		internal PrimitiveType GetPrimitiveType(FaceFlags flags)
		{
			switch (flags & FaceFlags.FaceTypeMask)
			{
				case FaceFlags.Triangles: return PrimitiveType.Triangles;
				case FaceFlags.TriangleStrip: return PrimitiveType.TriangleStrip;
				case FaceFlags.Quads: return PrimitiveType.Quads;
				case FaceFlags.QuadStrip: return PrimitiveType.QuadStrip;
				default: return PrimitiveType.Polygon;
			}
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
			GL.CullFace(CullFaceMode.Front);
			SetBlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			UnsetBlendFunc();
			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Lequal);
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
			currentHost.ClearAnimatedObjectCache();
			currentHost.PruneStaleStaticObjects();
			TextureManager.UnloadAllTextures(true);
			VisibleObjects.Clear();
		}

		public int CreateStaticObject(StaticObject Prototype, Vector3 Position, Transformation WorldTransformation, Transformation LocalTransformation, ObjectDisposalMode AccurateObjectDisposal, ObjectCreationParameters Parameters, double BlockLength)
		{
			Matrix4D Rotate = (Matrix4D)new Transformation(LocalTransformation ?? Transformation.NullTransformation, WorldTransformation);
			return CreateStaticObject(Position, Prototype, LocalTransformation, Rotate, AccurateObjectDisposal, Parameters, BlockLength);
		}

		public int CreateStaticObject(Vector3 Position, StaticObject Prototype, Transformation LocalTransformation, Matrix4D Rotate, ObjectDisposalMode AccurateObjectDisposal, ObjectCreationParameters Parameters, double BlockLength)
		{
			if (Prototype == null)
			{
				return -1;
			}

			Matrix4D Translate = Matrix4D.CreateTranslation(Position.X, Position.Y, -Position.Z);

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
					Coordinates.Rotate(LocalTransformation ?? Transformation.NullTransformation);

					if (Coordinates.Z < startingDistance)
					{
						startingDistance = (float)Coordinates.Z;
					}

					if (Coordinates.Z > endingDistance)
					{
						endingDistance = (float)Coordinates.Z;
					}
				}

				startingDistance += (float)Parameters.AccurateObjectDisposalZOffset;
				endingDistance += (float)Parameters.AccurateObjectDisposalZOffset;
			}

			const double minBlockLength = 20.0;

			if (BlockLength < minBlockLength)
			{
				BlockLength *= Math.Ceiling(minBlockLength / BlockLength);
			}

			switch (AccurateObjectDisposal)
			{
				case ObjectDisposalMode.Accurate:
					startingDistance += (float)Parameters.TrackPosition;
					endingDistance += (float)Parameters.TrackPosition;
					double z = BlockLength * Math.Floor(Parameters.TrackPosition / BlockLength);
					Parameters.StartingDistance = Math.Min(z - BlockLength, startingDistance);
					Parameters.EndingDistance = Math.Max(z + 2.0 * BlockLength, endingDistance);
					startingDistance = (float)(BlockLength * Math.Floor(Parameters.StartingDistance / BlockLength));
					endingDistance = (float)(BlockLength * Math.Ceiling(Parameters.EndingDistance / BlockLength));
					break;
				case ObjectDisposalMode.Legacy:
					startingDistance = (float)Parameters.StartingDistance;
					endingDistance = (float)Parameters.EndingDistance;
					break;
				case ObjectDisposalMode.Mechanik:
					startingDistance = (float)Parameters.StartingDistance;
					endingDistance = (float)Parameters.EndingDistance + 1500;
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
				StartingDistance = startingDistance,
				EndingDistance = endingDistance,
				WorldPosition = Position,
				DisableShadowCasting = Parameters.DisableShadowCasting
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
			for (int i = 0; i < StaticObjectStates.Count; i++)
			{
				VAOExtensions.CreateOrUpdateVAO(StaticObjectStates[i].Prototype.Mesh, false, DefaultShader.VertexLayout, this);
				/*
				 * n.b.
				 * Only create the actual matrix buffer at first frame render time
				 * I can't find why at the minute, but Object Viewer otherwise doesn't show them, and attempting
				 * to retrieve previously set matricies from the shader shows all zeros
				 *
				 * Probably a timing issue, but it works doing it that way :/
				 */
			}
			for (int i = 0; i < DynamicObjectStates.Count; i++)
			{
				VAOExtensions.CreateOrUpdateVAO(DynamicObjectStates[i].Prototype.Mesh, false, DefaultShader.VertexLayout, this);
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
		/// <summary>The lock to be held whilst visibility updates or loading operations are in progress</summary>
		public object VisibilityUpdateLock = new object();
		
		public bool VisibilityThreadShouldRun = true;

		public Thread VisibilityThread;

		private void RunVisibiliityThread()
		{
			while (VisibilityThreadShouldRun)
			{
				lock (VisibilityUpdateLock)
				{
					if (updateVisibility != VisibilityUpdate.None && CameraTrackFollower != null)
					{
						UpdateVisibility(CameraTrackFollower.TrackPosition + Camera.Alignment.Position.Z);
					}
				}

				if (updateVisibility == VisibilityUpdate.None)
				{
					Thread.Sleep(100);
				}
			}
		}

		public void UpdateVisibility(bool force)
		{
			updateVisibility = force ? VisibilityUpdate.Force : VisibilityUpdate.Normal;
		}

		private void UpdateVisibility(double trackPosition)
		{
			if (currentOptions.ObjectDisposalMode == ObjectDisposalMode.QuadTree)
			{
				UpdateQuadTreeVisibility();
			}
			else
			{
				if (updateVisibility == VisibilityUpdate.Normal)
				{
					UpdateLegacyVisibility(trackPosition);
				}
				else
				{
					/*
					 * The original visibility algorithm fails to handle correctly cases where the
					 * camera angle is rotated, but the track position does not change
					 *
					 * Horrible kludge...
					 */
					UpdateLegacyVisibility(trackPosition + 0.01);
					UpdateLegacyVisibility(trackPosition - 0.01);
				}
				
			}

			updateVisibility = VisibilityUpdate.None;
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

		private void UpdateLegacyVisibility(double trackPosition)
		{
			if (ObjectsSortedByStart == null || ObjectsSortedByStart.Length == 0 || StaticObjectStates.Count == 0)
			{
				return;
			}
			double d = trackPosition - LastUpdatedTrackPosition;
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

			LastUpdatedTrackPosition = trackPosition;
		}

		public void UpdateViewingDistances(double backgroundImageDistance)
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

			double d = backgroundImageDistance + Camera.ExtraViewingDistance;
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
				if (string.Compare(extension, "GL_EXT_texture_filter_anisotropic", StringComparison.OrdinalIgnoreCase) == 0)
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
		
		/// <summary>Updates the openGL viewport</summary>
		/// <param name="mode">The viewport change mode</param>
		public void UpdateViewport(ViewportChangeMode mode)
		{
			switch (mode)
			{
				case ViewportChangeMode.ChangeToScenery:
					CurrentViewportMode = ViewportMode.Scenery;
					break;
				case ViewportChangeMode.ChangeToCab:
					CurrentViewportMode = ViewportMode.Cab;
					break;
			}

			UpdateViewport(Screen.Width, Screen.Height);
		}

		protected virtual void UpdateViewport(int Width, int Height)
		{
			Screen.Width = Width;
			Screen.Height = Height;
			GL.Viewport(0, 0, Screen.Width, Screen.Height);

			Screen.AspectRatio = Screen.Width / (double)Screen.Height;
			Camera.HorizontalViewingAngle = 2.0 * Math.Atan(Math.Tan(0.5 * Camera.VerticalViewingAngle) * Screen.AspectRatio);
			double nearClip = Math.Max(0.01, currentOptions.NearClipBase);
			CurrentProjectionMatrix = Matrix4D.CreatePerspectiveFieldOfView(Camera.VerticalViewingAngle, Screen.AspectRatio, nearClip, currentOptions.ViewingDistance);
		}

		public void ResetShader(Shader shader)
		{
#if DEBUG
			if (!ReShadeInUse)
			{
				lastError = GL.GetError();

				if (lastError != ErrorCode.NoError && lastError != lastReportedError)
				{
					// Report the error once per distinct code instead of throwing:
					// throwing here killed the process with nothing at all written to
					// disk, which made GL state bugs impossible to diagnose from a
					// user report (see RouteViewer event markers hanging / vanishing)
					lastReportedError = lastError;
					const string message = "OpenGL error detected whilst resetting shader state: ";
					try
					{
						fileSystem?.AppendToLogFile(message + lastError);
					}
					catch
					{
						// ignored- logging must never be able to throw
					}
					currentHost.AddMessage(MessageType.Warning, false, message + lastError);
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
			shader.SetFog(false);
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
			CurrentShader.SetAlphaTest(true);
			CurrentShader.SetAlphaFunction(comparison, value);
        }

		/// <summary>Disables OpenGL alpha testing</summary>
		public void UnsetAlphaFunc()
		{
			alphaTestEnabled = false;
			CurrentShader.SetAlphaTest(false);
        }

		/// <summary>Restores the OpenGL alpha function to it's previous state</summary>
		public void RestoreAlphaFunc()
		{
			if (alphaTestEnabled)
			{
				CurrentShader.SetAlphaTest(true);
				CurrentShader.SetAlphaFunction(alphaFuncComparison, alphaFuncValue);
            }
			else
			{
				CurrentShader.SetAlphaTest(false);
            }
		}


		// Cached object state and matrices for shader drawing
		protected internal ObjectState lastObjectState;
		private Matrix4D lastModelMatrix;
		private Matrix4D lastModelViewMatrix;
		private bool sendToShader;

		/// <summary>Whether opaque faces should be drawn using instanced rendering where possible (one draw call per mesh/face instead of one per placed instance)</summary>
		public bool OptionInstancing = true;
		/// <summary>
		/// Controls the batching of alpha (blended) faces:
		/// 0 - every alpha face is drawn individually,
		/// 1 - only faces whose blending cannot depend upon the draw order are batched,
		/// 2 - every batchable face is batched, at the cost of an approximate back to front order.
		/// </summary>
		public int OptionAlphaInstancing = 1;
		/// <summary>The number of primitive batches (draw calls) issued for opaque faces by the last call to RenderOpaqueFaces</summary>
		public int OpaqueDrawCalls;
		/// <summary>The number of primitive batches (draw calls) issued for alpha faces by the alpha pass of the current frame</summary>
		public int AlphaDrawCalls;
		/// <summary>The number of unique meshes contributing to the visible opaque faces</summary>
		public int OpaqueUniqueMeshes;
		/// <summary>The maximum number of instances drawn in a single instanced batch (must match the uInstanceMatricies array size in default.vert)</summary>
		private const int MaxInstancesPerBatch = 256;
		/// <summary>The OpenGL uniform buffer used to pass per-instance modelview matrices to the shader</summary>
		private int instanceMatrixBuffer;
		/// <summary>Scratch buffer holding the packed per-instance modelview matrices</summary>
		private readonly float[] instanceMatrixData = new float[MaxInstancesPerBatch * 16];

		/// <summary>Draws a face using the current shader</summary>
		/// <param name="state">The FaceState to draw</param>
		/// <param name="isDebugTouchMode">Whether debug touch mode</param>
		public void RenderFace(FaceState state, bool isDebugTouchMode = false)
		{
			RenderFace(CurrentShader as Shader, state.Object, state.Face, isDebugTouchMode);
		}

		/// <summary>Draws a face using the specified shader and matrices</summary>
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
#pragma warning disable CS0618
				GL.BindBufferBase(BufferTarget.UniformBuffer, 0, state.MatrixBufferIndex);
#pragma warning restore CS0618
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
					shader.SetMaterialSpecular((material.Flags & MaterialFlags.Specular) != 0 ? material.SpecularColor : material.Color);
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
					shader.SetFog(false);
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
				Mesh normalsMesh = state.Prototype.Mesh;
				if (normalsMesh.NormalsVAO == null)
				{
					// Build the normals VAO lazily on first use to avoid holding a duplicate vertex buffer in RAM
					VAOExtensions.CreateNormalsVAO(normalsMesh, state.Prototype.Dynamic, DefaultShader.VertexLayout, this);
				}
				VertexArrayObject normalsVao = (VertexArrayObject)normalsMesh.NormalsVAO;
				if (normalsVao != null && face.IboStartIndex == 0)
				{
					// Draw all normals for this mesh in a single call (the normals VAO is a contiguous list of line pairs).
					// Solid purple overlay: disable lighting and force a white, emissive material so the shader
					// outputs the baked per-vertex purple colour unchanged (finalColor *= oLightResult == white).
					shader.SetIsLight(false);
					shader.SetMaterialAmbient(Color32.White);
					shader.SetMaterialFlags(MaterialFlags.Emissive);
					normalsVao.Bind();
					lastVAO = normalsVao.handle;
					normalsVao.DrawArrays(PrimitiveType.Lines, 0, normalsMesh.Vertices.Length > 0 ? normalsMesh.Faces.Sum(f => f.Vertices.Length) * 2 : 0);
					shader.SetIsLight(OptionLighting);
					shader.SetMaterialFlags(material.Flags);
					shader.SetMaterialAmbient(material.Color);
				}
			}

			// finalize
			if (material.BlendMode == MeshMaterialBlendMode.Additive)
			{
				RestoreBlendFunc();
				shader.SetFog(Fog.Enabled);
			}
			if (OptionWireFrame || debugTouchMode)
			{
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
			}
			lastObjectState = state;
		}

		/// <summary>
		/// Draws the supplied opaque faces, batching all instances which share a mesh into a single instanced
		/// draw call per (mesh, face) where this is provably safe.
		/// </summary>
		/// <remarks>
		/// BVE5/BVE6 routes place thousands of copies of the same structure (rails, sleepers, ballast, catenary)
		/// along the track. Optimisation collapses each prototype to one face per material, but the renderer still
		/// issues one draw call per placed instance. This method collapses those instances into instanced draws.
		/// Faces which cannot be batched safely- glow attenuation, animation matrices, per-instance texture
		/// translation, mixed day/night blend, single instances or debug overlays- fall back to the original
		/// one-draw-per-face path, so output is identical when batching is not possible.
		/// </remarks>
		public void RenderOpaqueFaces(List<FaceState> opaqueFaces)
		{
			OpaqueDrawCalls = 0;

			if (opaqueFaces == null || opaqueFaces.Count == 0)
			{
				return;
			}

			if (!OptionInstancing || OptionWireFrame || OptionNormals || opaqueFaces.Count < 2)
			{
				foreach (FaceState face in opaqueFaces)
				{
					OpaqueDrawCalls++;
					face.Draw();
				}

				return;
			}

			// Group the visible instances (one ObjectState per placed object) by the mesh they share.
			// Derived from the supplied opaque faces so that it stays consistent with the caller's snapshot.
			Dictionary<Mesh, List<ObjectState>> instancesByMesh = new Dictionary<Mesh, List<ObjectState>>();
			Dictionary<Mesh, HashSet<ObjectState>> seenByMesh = new Dictionary<Mesh, HashSet<ObjectState>>();
			Dictionary<Mesh, byte> blendByMesh = new Dictionary<Mesh, byte>();
			HashSet<Mesh> ineligibleMeshes = new HashSet<Mesh>();

			foreach (FaceState face in opaqueFaces)
			{
				ObjectState state = face.Object;

				if (state?.Prototype?.Mesh == null || state.Prototype.Mesh.VAO == null)
				{
					continue;
				}

				Mesh mesh = state.Prototype.Mesh;

				if ((state.Matricies != null && state.Matricies.Length > 0) || !IsIdentityMatrix(state.TextureTranslation))
				{
					// The per-instance state which is not carried by the model matrix differs from the rest of the batch
					ineligibleMeshes.Add(mesh);
					continue;
				}

				if (!instancesByMesh.TryGetValue(mesh, out List<ObjectState> list))
				{
					list = new List<ObjectState>();
					instancesByMesh.Add(mesh, list);
					seenByMesh.Add(mesh, new HashSet<ObjectState>());
					blendByMesh.Add(mesh, state.DaytimeNighttimeBlend);
				}
				else if (blendByMesh[mesh] != state.DaytimeNighttimeBlend)
				{
					// The day/night blend is a uniform, so a differing value cannot share a batch
					ineligibleMeshes.Add(mesh);
				}

				if (seenByMesh[mesh].Add(state))
				{
					list.Add(state);
				}
			}

			// Partition the visible opaque faces into batches (sharing a mesh) and the remainder.
			// Every placed instance contributes the same set of faces, so each face must only be
			// collected once per mesh- IboStartIndex uniquely identifies a face within its mesh.
			Dictionary<Mesh, List<MeshFace>> batchedFaces = new Dictionary<Mesh, List<MeshFace>>();
			Dictionary<Mesh, HashSet<int>> batchedFaceIndices = new Dictionary<Mesh, HashSet<int>>();
			List<FaceState> fallbackFaces = new List<FaceState>();
			HashSet<Mesh> fallbackMeshes = new HashSet<Mesh>();

			foreach (FaceState face in opaqueFaces)
			{
				Mesh mesh = face.Object?.Prototype?.Mesh;

				if (mesh == null || ineligibleMeshes.Contains(mesh) || !instancesByMesh.TryGetValue(mesh, out List<ObjectState> instances) || instances.Count < 2 || !CanBatchFace(mesh, face.Face))
				{
					fallbackFaces.Add(face);

					if (mesh != null)
					{
						fallbackMeshes.Add(mesh);
					}

					continue;
				}

				if (!batchedFaces.TryGetValue(mesh, out List<MeshFace> faces))
				{
					faces = new List<MeshFace>();
					batchedFaces.Add(mesh, faces);
					batchedFaceIndices.Add(mesh, new HashSet<int>());
				}

				if (batchedFaceIndices[mesh].Add(face.Face.IboStartIndex))
				{
					faces.Add(face.Face);
				}
			}

			foreach (FaceState face in fallbackFaces)
			{
				OpaqueDrawCalls++;
				face.Draw();
			}

			OpaqueUniqueMeshes = batchedFaces.Count + fallbackMeshes.Count;

			if (batchedFaces.Count == 0)
			{
				return;
			}

			EnsureInstanceMatrixBuffer();

			Shader shader = DefaultShader;
			shader.SetInstanced(true);
			// Batched faces only share a batch when their texture translation is identity
			shader.SetCurrentTextureMatrix(Matrix4D.Identity);
			// The uniform modelview matrices are unused while instancing, so ensure they are recomputed afterwards
			lastObjectState = null;

			try
			{
				foreach (KeyValuePair<Mesh, List<MeshFace>> batch in batchedFaces)
				{
					List<ObjectState> instances = instancesByMesh[batch.Key];
					byte daytimeNighttimeBlend = blendByMesh[batch.Key];

					for (int start = 0; start < instances.Count; start += MaxInstancesPerBatch)
					{
						int count = Math.Min(MaxInstancesPerBatch, instances.Count - start);
						UploadInstanceMatrices(instances, start, count);

						foreach (MeshFace face in batch.Value)
						{
							OpaqueDrawCalls += DrawFaceInstanced(shader, instances[start], face, count, daytimeNighttimeBlend);
						}
					}
				}
			}
			finally
			{
				shader.SetInstanced(false);
				lastObjectState = null;
			}
		}

		/// <summary>
		/// Renders the supplied faces into the shadow map, batching instances which share a mesh into
		/// instanced draw calls where this is provably safe. Visually equivalent to the per-face path.
		/// </summary>
		public void RenderShadowFaces(ShadowDepthShader shader, IEnumerable<FaceState> faces, double maxDistanceSquared, ref int lastVAO)
		{
			IList<FaceState> faceList = faces is IList<FaceState> list ? list : new List<FaceState>(faces);

			if (!OptionInstancing || OptionWireFrame || OptionNormals || faceList.Count < 2)
			{
				RenderShadowFacesFallback(shader, faceList, maxDistanceSquared, ref lastVAO);
				return;
			}

			Vector3 cameraPos = Camera.AbsolutePosition;

			Dictionary<Mesh, List<ObjectState>> instancesByMesh = new Dictionary<Mesh, List<ObjectState>>();
			Dictionary<Mesh, HashSet<ObjectState>> seenByMesh = new Dictionary<Mesh, HashSet<ObjectState>>();
			HashSet<Mesh> ineligibleMeshes = new HashSet<Mesh>();
			HashSet<ObjectState> seenInstances = new HashSet<ObjectState>();

			// First pass: collect the instances which can be batched, applying the per-cascade distance cull.
			foreach (FaceState face in faceList)
			{
				ObjectState state = face?.Object;
				if (state?.Prototype?.Mesh == null || state.Prototype.Mesh.VAO == null || state.DisableShadowCasting)
				{
					continue;
				}

				if (maxDistanceSquared < double.MaxValue)
				{
					double dx = state.WorldPosition.X - cameraPos.X;
					double dy = state.WorldPosition.Y - cameraPos.Y;
					double dz = state.WorldPosition.Z - cameraPos.Z;
					if (dx * dx + dy * dy + dz * dz > maxDistanceSquared)
					{
						continue;
					}
				}

				Mesh mesh = state.Prototype.Mesh;

				if (seenInstances.Add(state))
				{
					if ((state.Matricies != null && state.Matricies.Length > 0) || !IsIdentityMatrix(state.TextureTranslation))
					{
						ineligibleMeshes.Add(mesh);
					}
					else
					{
						if (!instancesByMesh.TryGetValue(mesh, out List<ObjectState> list2))
						{
							list2 = new List<ObjectState>();
							instancesByMesh.Add(mesh, list2);
							seenByMesh.Add(mesh, new HashSet<ObjectState>());
						}

						if (seenByMesh[mesh].Add(state))
						{
							list2.Add(state);
						}
					}
				}
			}

			// Partition the faces into batches (sharing a mesh) and the remainder.
			Dictionary<Mesh, List<MeshFace>> batchedFaces = new Dictionary<Mesh, List<MeshFace>>();
			Dictionary<Mesh, HashSet<int>> batchedFaceIndices = new Dictionary<Mesh, HashSet<int>>();
			List<FaceState> fallbackFaces = new List<FaceState>();

			foreach (FaceState face in faceList)
			{
				ObjectState state = face?.Object;
				Mesh mesh = state?.Prototype?.Mesh;

				if (mesh == null || ineligibleMeshes.Contains(mesh) || !instancesByMesh.TryGetValue(mesh, out List<ObjectState> instances) || instances.Count < 2)
				{
					fallbackFaces.Add(face);
					continue;
				}

				if (face.Face.Material < 0 || face.Face.Material >= mesh.Materials.Length)
				{
					fallbackFaces.Add(face);
					continue;
				}

				MeshMaterial material = mesh.Materials[face.Face.Material];
				if ((material.Flags & MaterialFlags.NoShadow) != 0 || material.BlendMode == MeshMaterialBlendMode.Additive)
				{
					fallbackFaces.Add(face);
					continue;
				}

				if (face.Face.Vertices.Length == 0)
				{
					fallbackFaces.Add(face);
					continue;
				}

				if (!batchedFaces.TryGetValue(mesh, out List<MeshFace> faces2))
				{
					faces2 = new List<MeshFace>();
					batchedFaces.Add(mesh, faces2);
					batchedFaceIndices.Add(mesh, new HashSet<int>());
				}

				if (batchedFaceIndices[mesh].Add(face.Face.IboStartIndex))
				{
					faces2.Add(face.Face);
				}
			}

			// Draw the faces which cannot be batched individually, exactly as the original path.
			RenderShadowFacesFallback(shader, fallbackFaces, maxDistanceSquared, ref lastVAO);

			if (batchedFaces.Count == 0)
			{
				return;
			}

			EnsureInstanceMatrixBuffer();

			shader.SetInstanced(true);
			shader.SetTextureMatrix(Matrix4D.Identity);
			lastObjectState = null;

			try
			{
				foreach (KeyValuePair<Mesh, List<MeshFace>> batch in batchedFaces)
				{
					List<ObjectState> instances = instancesByMesh[batch.Key];

					for (int start = 0; start < instances.Count; start += MaxInstancesPerBatch)
					{
						int count = Math.Min(MaxInstancesPerBatch, instances.Count - start);
						UploadShadowInstanceMatrices(instances, start, count);

						foreach (MeshFace face in batch.Value)
						{
							Mesh mesh = batch.Key;
							MeshMaterial material = mesh.Materials[face.Material];
							VertexArrayObject VAO = (VertexArrayObject)mesh.VAO;

							if (lastVAO != VAO.handle)
							{
								VAO.Bind();
								lastVAO = VAO.handle;
							}

							if (OptionBackFaceCulling)
							{
								if ((face.Flags & FaceFlags.Face2Mask) != 0)
								{
									GL.Disable(EnableCap.CullFace);
								}
								else
								{
									GL.Enable(EnableCap.CullFace);
								}
							}

							if (material.DaytimeTexture != null && currentHost.LoadTexture(ref material.DaytimeTexture, (OpenGlTextureWrapMode)(material.WrapMode ?? OpenGlTextureWrapMode.ClampClamp)))
							{
								GL.ActiveTexture(TextureUnit.Texture0);
								GL.BindTexture(TextureTarget.Texture2D, material.DaytimeTexture.OpenGlTextures[(int)(material.WrapMode ?? OpenGlTextureWrapMode.ClampClamp)].Name);
								shader.SetHasTexture(true);
							}
							else
							{
								shader.SetHasTexture(false);
							}

							shader.SetAlphaCutoff(0.5f);
							shader.SetMaterialAlpha(material.Color.A / 255.0f);
							shader.SetMaterialFlags(material.Flags);

							PrimitiveType drawMode = GetPrimitiveType(face.Flags);
							VAO.DrawInstanced(drawMode, face.IboStartIndex, face.Vertices.Length, count);
						}
					}
				}
			}
			finally
			{
				shader.SetInstanced(false);
				lastObjectState = null;
			}
		}

		/// <summary>Per-face fallback used by <see cref="RenderShadowFaces"/> when instancing is unavailable or unsafe.</summary>
		private void RenderShadowFacesFallback(ShadowDepthShader shader, IEnumerable<FaceState> faces, double maxDistanceSquared, ref int lastVAO)
		{
			Vector3 cameraPos = Camera.AbsolutePosition;

			foreach (FaceState faceState in faces)
			{
				FaceState face = faceState;
				if (face?.Object?.Prototype?.Mesh?.VAO == null || face.Object.DisableShadowCasting)
				{
					continue;
				}

				if (maxDistanceSquared < double.MaxValue)
				{
					ObjectState state = face.Object;
					double dx = state.WorldPosition.X - cameraPos.X;
					double dy = state.WorldPosition.Y - cameraPos.Y;
					double dz = state.WorldPosition.Z - cameraPos.Z;
					if (dx * dx + dy * dy + dz * dz > maxDistanceSquared)
					{
						continue;
					}
				}

				ObjectState st = face.Object;
				Mesh mesh = st.Prototype.Mesh;
				MeshMaterial material = mesh.Materials[face.Face.Material];
				if ((material.Flags & MaterialFlags.NoShadow) != 0 || material.BlendMode == MeshMaterialBlendMode.Additive)
				{
					continue;
				}

				shader.SetModelMatrix(st.ModelMatrix * Camera.TranslationMatrix);
				shader.SetTextureMatrix(st.TextureTranslation);

				if (material.DaytimeTexture != null && currentHost.LoadTexture(ref material.DaytimeTexture, (OpenGlTextureWrapMode)(material.WrapMode ?? OpenGlTextureWrapMode.ClampClamp)))
				{
					GL.ActiveTexture(TextureUnit.Texture0);
					GL.BindTexture(TextureTarget.Texture2D, material.DaytimeTexture.OpenGlTextures[(int)(material.WrapMode ?? OpenGlTextureWrapMode.ClampClamp)].Name);
					shader.SetHasTexture(true);
				}
				else
				{
					shader.SetHasTexture(false);
				}

				shader.SetAlphaCutoff(0.5f);
				shader.SetMaterialAlpha(material.Color.A / 255.0f);
				shader.SetMaterialFlags(material.Flags);

				if (st.Matricies != null && st.Matricies.Length > 0)
				{
					shader.SetCurrentAnimationMatricies(st);
					GL.BindBufferBase(BufferTarget.UniformBuffer, 0, st.MatrixBufferIndex);
				}

				VertexArrayObject vao = (VertexArrayObject)mesh.VAO;
				if (vao.handle != lastVAO)
				{
					vao.Bind();
					lastVAO = vao.handle;
				}

				if (OptionBackFaceCulling)
				{
					if ((face.Face.Flags & FaceFlags.Face2Mask) != 0)
					{
						GL.Disable(EnableCap.CullFace);
					}
					else
					{
						GL.Enable(EnableCap.CullFace);
					}
				}

				PrimitiveType drawMode = GetPrimitiveType(face.Face.Flags);
				vao.Draw(drawMode, face.Face.IboStartIndex, face.Face.Vertices.Length);
			}
		}

		/// <summary>Packs and uploads world-space matrices for a range of shadow instances to the instance matrix buffer</summary>
		private void UploadShadowInstanceMatrices(IList<ObjectState> instances, int start, int count)
		{
			for (int i = 0; i < count; i++)
			{
				ObjectState state = instances[start + i];
				Matrix4D world = state.ModelMatrix * Camera.TranslationMatrix;
				int offset = i * 16;
				instanceMatrixData[offset] = (float)world.Row0.X;
				instanceMatrixData[offset + 1] = (float)world.Row0.Y;
				instanceMatrixData[offset + 2] = (float)world.Row0.Z;
				instanceMatrixData[offset + 3] = (float)world.Row0.W;
				instanceMatrixData[offset + 4] = (float)world.Row1.X;
				instanceMatrixData[offset + 5] = (float)world.Row1.Y;
				instanceMatrixData[offset + 6] = (float)world.Row1.Z;
				instanceMatrixData[offset + 7] = (float)world.Row1.W;
				instanceMatrixData[offset + 8] = (float)world.Row2.X;
				instanceMatrixData[offset + 9] = (float)world.Row2.Y;
				instanceMatrixData[offset + 10] = (float)world.Row2.Z;
				instanceMatrixData[offset + 11] = (float)world.Row2.W;
				instanceMatrixData[offset + 12] = (float)world.Row3.X;
				instanceMatrixData[offset + 13] = (float)world.Row3.Y;
				instanceMatrixData[offset + 14] = (float)world.Row3.Z;
				instanceMatrixData[offset + 15] = (float)world.Row3.W;
			}

			GL.BindBuffer(BufferTarget.UniformBuffer, instanceMatrixBuffer);
			GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, new IntPtr(count * 16 * sizeof(float)), instanceMatrixData);
			GL.BindBufferBase(BufferTarget.UniformBuffer, 1, instanceMatrixBuffer);
		}

		/// <summary>Checks whether a matrix is exactly the identity matrix</summary>
		private static bool IsIdentityMatrix(Matrix4D matrix)
		{
			return matrix.Row0.X == 1.0 && matrix.Row0.Y == 0.0 && matrix.Row0.Z == 0.0 && matrix.Row0.W == 0.0 &&
			       matrix.Row1.X == 0.0 && matrix.Row1.Y == 1.0 && matrix.Row1.Z == 0.0 && matrix.Row1.W == 0.0 &&
			       matrix.Row2.X == 0.0 && matrix.Row2.Y == 0.0 && matrix.Row2.Z == 1.0 && matrix.Row2.W == 0.0 &&
			       matrix.Row3.X == 0.0 && matrix.Row3.Y == 0.0 && matrix.Row3.Z == 0.0 && matrix.Row3.W == 1.0;
		}

		/// <summary>Checks whether a face can be drawn as part of an instanced batch</summary>
		private static bool CanBatchFace(Mesh mesh, MeshFace face)
		{
			if (face.Vertices.Length == 0 || face.Material < 0 || face.Material >= mesh.Materials.Length)
			{
				return false;
			}

			MeshMaterial material = mesh.Materials[face.Material];
			// Glow attenuation is derived per instance from the model matrix, so it cannot share a single batch
			return material.GlowAttenuationData == 0 && material.BlendMode != MeshMaterialBlendMode.Additive;
		}

		/// <summary>Creates the uniform buffer used to upload per-instance matrices, if it does not yet exist</summary>
		private void EnsureInstanceMatrixBuffer()
		{
			if (instanceMatrixBuffer != 0)
			{
				return;
			}

			instanceMatrixBuffer = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.UniformBuffer, instanceMatrixBuffer);
			GL.BufferData(BufferTarget.UniformBuffer, new IntPtr(MaxInstancesPerBatch * 16 * sizeof(float)), IntPtr.Zero, BufferUsageHint.DynamicDraw);
			GL.BindBuffer(BufferTarget.UniformBuffer, 0);
		}

		/// <summary>Packs and uploads the modelview matrices for a range of instances to the instance matrix buffer</summary>
		private void UploadInstanceMatrices(IList<ObjectState> instances, int start, int count)
		{
			for (int i = 0; i < count; i++)
			{
				ObjectState state = instances[start + i];
				Matrix4D modelView = state.ModelMatrix * Camera.TranslationMatrix * CurrentViewMatrix;
				int offset = i * 16;
				instanceMatrixData[offset] = (float)modelView.Row0.X;
				instanceMatrixData[offset + 1] = (float)modelView.Row0.Y;
				instanceMatrixData[offset + 2] = (float)modelView.Row0.Z;
				instanceMatrixData[offset + 3] = (float)modelView.Row0.W;
				instanceMatrixData[offset + 4] = (float)modelView.Row1.X;
				instanceMatrixData[offset + 5] = (float)modelView.Row1.Y;
				instanceMatrixData[offset + 6] = (float)modelView.Row1.Z;
				instanceMatrixData[offset + 7] = (float)modelView.Row1.W;
				instanceMatrixData[offset + 8] = (float)modelView.Row2.X;
				instanceMatrixData[offset + 9] = (float)modelView.Row2.Y;
				instanceMatrixData[offset + 10] = (float)modelView.Row2.Z;
				instanceMatrixData[offset + 11] = (float)modelView.Row2.W;
				instanceMatrixData[offset + 12] = (float)modelView.Row3.X;
				instanceMatrixData[offset + 13] = (float)modelView.Row3.Y;
				instanceMatrixData[offset + 14] = (float)modelView.Row3.Z;
				instanceMatrixData[offset + 15] = (float)modelView.Row3.W;
			}

			UploadInstanceMatrices(count);
		}

		/// <summary>Uploads the instance matrices currently packed in the staging array to the instance matrix buffer</summary>
		/// <param name="count">The number of matrices to upload</param>
		private void UploadInstanceMatrices(int count)
		{
			GL.BindBuffer(BufferTarget.UniformBuffer, instanceMatrixBuffer);
			GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, new IntPtr(count * 16 * sizeof(float)), instanceMatrixData);
#pragma warning disable CS0618
			GL.BindBufferBase(BufferTarget.UniformBuffer, 1, instanceMatrixBuffer);
#pragma warning restore CS0618
		}

		/// <summary>Draws a batch of marker cubes sharing a texture and an orientation in as few instanced calls as possible</summary>
		/// <param name="positions">The world-space positions of the markers</param>
		/// <param name="direction">The direction vector used for every marker</param>
		/// <param name="up">The up vector used for every marker</param>
		/// <param name="side">The side vector used for every marker</param>
		/// <param name="size">The size of every marker in metres</param>
		/// <param name="camera">The camera position</param>
		/// <param name="texture">The texture applied to every marker</param>
		/// <returns>The number of draw calls issued</returns>
		/// <remarks>Visually equivalent to calling Cube.Draw for every position (the positions are drawn in the same order), but the shader is
		/// reset and the texture loaded once per batch and the markers are drawn MaxInstancesPerBatch at a time.</remarks>
		public int DrawMarkerCubes(IList<Vector3> positions, Vector3 direction, Vector3 up, Vector3 side, double size, Vector3 camera, Texture texture)
		{
			if (positions == null || positions.Count == 0)
			{
				return 0;
			}

			DefaultShader.Activate();
			ResetShader(DefaultShader);
			DefaultShader.SetCurrentProjectionMatrix(CurrentProjectionMatrix);

			if (texture != null && currentHost.LoadTexture(ref texture, OpenGlTextureWrapMode.ClampClamp))
			{
				GL.Enable(EnableCap.Texture2D);
				GL.BindTexture(TextureTarget.Texture2D, texture.OpenGlTextures[(int)OpenGlTextureWrapMode.ClampClamp].Name);
			}
			else
			{
				GL.Disable(EnableCap.Texture2D);
			}

			EnsureInstanceMatrixBuffer();

			Shader shader = DefaultShader;
			Matrix4D orientation = Matrix4D.Scale(size) * (Matrix4D)new Transformation(direction, up, side);
			int drawCalls = 0;
			shader.SetInstanced(true);
			// The uniform modelview matrices are unused whilst instancing, so ensure they are recomputed afterwards
			lastObjectState = null;

			try
			{
				for (int start = 0; start < positions.Count; start += MaxInstancesPerBatch)
				{
					int count = Math.Min(MaxInstancesPerBatch, positions.Count - start);

					for (int i = 0; i < count; i++)
					{
						Vector3 p = positions[start + i];
						Matrix4D modelView = orientation * Matrix4D.CreateTranslation(p.X - camera.X, p.Y - camera.Y, -p.Z + camera.Z) * CurrentViewMatrix;
						int offset = i * 16;
						instanceMatrixData[offset] = (float)modelView.Row0.X;
						instanceMatrixData[offset + 1] = (float)modelView.Row0.Y;
						instanceMatrixData[offset + 2] = (float)modelView.Row0.Z;
						instanceMatrixData[offset + 3] = (float)modelView.Row0.W;
						instanceMatrixData[offset + 4] = (float)modelView.Row1.X;
						instanceMatrixData[offset + 5] = (float)modelView.Row1.Y;
						instanceMatrixData[offset + 6] = (float)modelView.Row1.Z;
						instanceMatrixData[offset + 7] = (float)modelView.Row1.W;
						instanceMatrixData[offset + 8] = (float)modelView.Row2.X;
						instanceMatrixData[offset + 9] = (float)modelView.Row2.Y;
						instanceMatrixData[offset + 10] = (float)modelView.Row2.Z;
						instanceMatrixData[offset + 11] = (float)modelView.Row2.W;
						instanceMatrixData[offset + 12] = (float)modelView.Row3.X;
						instanceMatrixData[offset + 13] = (float)modelView.Row3.Y;
						instanceMatrixData[offset + 14] = (float)modelView.Row3.Z;
						instanceMatrixData[offset + 15] = (float)modelView.Row3.W;
					}

					UploadInstanceMatrices(count);
					Cube.DrawInstanced(count);
					drawCalls++;
				}
			}
			finally
			{
				shader.SetInstanced(false);
				lastObjectState = null;
			}

			GL.Disable(EnableCap.Texture2D);

			return drawCalls;
		}

		/// <summary>Draws a single face for every instance in the currently uploaded batch</summary>
		/// <returns>The number of draw calls issued</returns>
		/// <remarks>Mirrors the per-face portion of RenderFace, minus the model matrix handling (supplied per instance) and glow/normals debug output (batched faces never use them).</remarks>
		private int DrawFaceInstanced(Shader shader, ObjectState representative, MeshFace face, int instanceCount, byte daytimeNighttimeBlend)
		{
			int drawCalls = 0;
			Mesh mesh = representative.Prototype.Mesh;
			MeshMaterial material = mesh.Materials[face.Material];
			VertexArrayObject VAO = (VertexArrayObject)mesh.VAO;

			if (lastVAO != VAO.handle)
			{
				VAO.Bind();
				lastVAO = VAO.handle;
			}

			if (!OptionBackFaceCulling || (face.Flags & FaceFlags.Face2Mask) != 0)
			{
				GL.Disable(EnableCap.CullFace);
			}
			else if ((face.Flags & FaceFlags.Face2Mask) == 0)
			{
				GL.Enable(EnableCap.CullFace);
			}

			// lighting
			shader.SetMaterialFlags(material.Flags);

			if (OptionLighting)
			{
				if (material.Color != lastColor)
				{
					shader.SetMaterialAmbient(material.Color);
					shader.SetMaterialDiffuse(material.Color);
					shader.SetMaterialSpecular((material.Flags & MaterialFlags.Specular) != 0 ? material.SpecularColor : material.Color);
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

			float blendFactor = inv255 * daytimeNighttimeBlend + 1.0f - Lighting.OptionLightingResultingAmount;

			if (blendFactor > 1.0)
			{
				blendFactor = 1.0f;
			}

			// daytime polygon
			{
				if (material.DaytimeTexture != null && currentHost.LoadTexture(ref material.DaytimeTexture, (OpenGlTextureWrapMode)material.WrapMode))
				{
					if (LastBoundTexture != material.DaytimeTexture.OpenGlTextures[(int)material.WrapMode])
					{
						GL.BindTexture(TextureTarget.Texture2D, material.DaytimeTexture.OpenGlTextures[(int)material.WrapMode].Name);
						LastBoundTexture = material.DaytimeTexture.OpenGlTextures[(int)material.WrapMode];
					}
				}
				else
				{
					shader.DisableTexturing();
				}

				float factor;

				if (material.BlendMode == MeshMaterialBlendMode.Additive)
				{
					factor = 1.0f;
					GL.Enable(EnableCap.Blend);
					GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
					shader.SetFog(false);
				}
				else if (material.NighttimeTexture == null || material.NighttimeTexture == material.DaytimeTexture)
				{
					factor = 1.0f - 0.7f * blendFactor;
				}
				else
				{
					factor = 1.0f;
				}

				shader.SetBrightness(factor);
				float alphaFactor = 1.0f;
				if (material.NighttimeTexture != null && (material.Flags & MaterialFlags.CrossFadeTexture) != 0)
				{
					alphaFactor = 1.0f - blendFactor;
				}
				shader.SetOpacity(inv255 * material.Color.A * alphaFactor);

				drawCalls++;
				VAO.DrawInstanced(drawMode, face.IboStartIndex, face.Vertices.Length, instanceCount);
			}

			// nighttime polygon
			if (blendFactor != 0 && material.NighttimeTexture != null && material.NighttimeTexture != material.DaytimeTexture && currentHost.LoadTexture(ref material.NighttimeTexture, (OpenGlTextureWrapMode)material.WrapMode))
			{
				if (LastBoundTexture != material.NighttimeTexture.OpenGlTextures[(int)material.WrapMode])
				{
					GL.BindTexture(TextureTarget.Texture2D, material.NighttimeTexture.OpenGlTextures[(int)material.WrapMode].Name);
					LastBoundTexture = material.NighttimeTexture.OpenGlTextures[(int)material.WrapMode];
				}

				GL.Enable(EnableCap.Blend);
				shader.SetAlphaTest(true);
				shader.SetAlphaFunction(AlphaFunction.Greater, 0.0f);
				shader.SetOpacity(inv255 * material.Color.A * blendFactor);

				drawCalls++;
				VAO.DrawInstanced(drawMode, face.IboStartIndex, face.Vertices.Length, instanceCount);
				RestoreBlendFunc();
				RestoreAlphaFunc();
			}

			if (material.BlendMode == MeshMaterialBlendMode.Additive)
			{
				RestoreBlendFunc();
				shader.SetFog(Fog.Enabled);
			}

			return drawCalls;
		}

		/// <summary>Holds the state required to draw every instance of a mesh's faces within a single batched alpha pass</summary>
		private sealed class AlphaBatch
		{
			/// <summary>The mesh shared by every face and instance in the batch</summary>
			public Mesh Mesh;
			/// <summary>The distinct instances (placed objects) to draw, ordered from the farthest to the nearest</summary>
			public ObjectState[] Instances;
			/// <summary>The distinct faces of the mesh which are to be drawn</summary>
			public List<MeshFace> Faces;
			/// <summary>The distance of the nearest instance from the camera, used to order the batches themselves</summary>
			public double NearestDistance;
		}

		/// <summary>
		/// Draws the supplied alpha (blended) faces, batching every instance which shares a mesh into a single
		/// instanced draw call per (mesh, face).
		/// </summary>
		/// <param name="alphaFaces">The alpha faces to draw</param>
		/// <param name="opaqueCutoutPass">
		/// True when drawing the first pass of the intermediate / quality transparency modes, in which only fully
		/// opaque pixels are written and the draw order is therefore irrelevant.
		/// </param>
		/// <param name="qualityBlendPass">
		/// True when drawing the second pass of the intermediate / quality transparency modes, in which the alpha
		/// test is disabled for additively blended faces and the fully opaque pixels of every face are discarded.
		/// </param>
		/// <remarks>
		/// By default only faces whose blending cannot depend upon the draw order are batched (see
		/// <see cref="FaceState.AdditiveBlend"/> and <see cref="FaceState.BinaryAlpha"/>), so that the image is
		/// identical to the individual draw path. Everything else falls back to individual sorted draws.
		/// Batches are drawn back to front by their nearest instance, and the instances within a batch are
		/// themselves ordered back to front.
		/// </remarks>
		public void RenderAlphaFaces(List<FaceState> alphaFaces, bool opaqueCutoutPass = false, bool qualityBlendPass = false)
		{
			if (alphaFaces == null || alphaFaces.Count == 0)
			{
				return;
			}

			if (OptionAlphaInstancing == 0 || OptionWireFrame || OptionNormals || alphaFaces.Count < 2)
			{
				foreach (FaceState face in alphaFaces)
				{
					if (opaqueCutoutPass && !IsAlphaCutoutFace(face.Object?.Prototype?.Mesh, face.Face))
					{
						// Not selected by this pass at all
						continue;
					}

					AlphaDrawCalls++;
					face.Draw();
				}

				return;
			}

			// Group the visible instances (one ObjectState per placed object) by the mesh they share
			Dictionary<Mesh, List<ObjectState>> instancesByMesh = new Dictionary<Mesh, List<ObjectState>>();
			Dictionary<Mesh, HashSet<ObjectState>> seenByMesh = new Dictionary<Mesh, HashSet<ObjectState>>();
			Dictionary<Mesh, byte> blendByMesh = new Dictionary<Mesh, byte>();
			HashSet<Mesh> ineligibleMeshes = new HashSet<Mesh>();

			foreach (FaceState face in alphaFaces)
			{
				ObjectState state = face.Object;

				if (state?.Prototype?.Mesh == null || state.Prototype.Mesh.VAO == null)
				{
					continue;
				}

				Mesh mesh = state.Prototype.Mesh;

				if ((state.Matricies != null && state.Matricies.Length > 0) || !IsIdentityMatrix(state.TextureTranslation))
				{
					ineligibleMeshes.Add(mesh);
					continue;
				}

				if (!instancesByMesh.TryGetValue(mesh, out List<ObjectState> list))
				{
					list = new List<ObjectState>();
					instancesByMesh.Add(mesh, list);
					seenByMesh.Add(mesh, new HashSet<ObjectState>());
					blendByMesh.Add(mesh, state.DaytimeNighttimeBlend);
				}
				else if (blendByMesh[mesh] != state.DaytimeNighttimeBlend)
				{
					ineligibleMeshes.Add(mesh);
				}

				if (seenByMesh[mesh].Add(state))
				{
					list.Add(state);
				}
			}

			// Partition the alpha faces into batches (sharing a mesh) and the remainder
			double cameraX = Camera.AbsolutePosition.X;
			double cameraY = Camera.AbsolutePosition.Y;
			double cameraZ = Camera.AbsolutePosition.Z;
			Dictionary<Mesh, AlphaBatch> batchesByMesh = new Dictionary<Mesh, AlphaBatch>();
			Dictionary<Mesh, HashSet<int>> batchedFaceIndices = new Dictionary<Mesh, HashSet<int>>();
			List<FaceState> fallbackFaces = new List<FaceState>();

			foreach (FaceState face in alphaFaces)
			{
				Mesh mesh = face.Object?.Prototype?.Mesh;

				// The first pass of the intermediate / quality modes only draws the fully opaque pixels of the
				// selected faces, so faces which it does not select are skipped entirely
				if (mesh == null || (opaqueCutoutPass && !IsAlphaCutoutFace(mesh, face.Face)))
				{
					continue;
				}

				if (!opaqueCutoutPass && !IsAlphaBatchingSafe(face, qualityBlendPass) && OptionAlphaInstancing < 2)
				{
					// Blended faces whose result depends upon the draw order must be drawn individually
					fallbackFaces.Add(face);
					continue;
				}

				if (ineligibleMeshes.Contains(mesh) || !instancesByMesh.TryGetValue(mesh, out List<ObjectState> instances) || instances.Count < 2 || !CanBatchAlphaFace(mesh, face.Face))
				{
					fallbackFaces.Add(face);
					continue;
				}

				if (!batchesByMesh.TryGetValue(mesh, out AlphaBatch batch))
				{
					// Build the ordered instance list for this mesh- instance order within a single draw call is
					// the order in which the GPU blends the instances, so the farthest must come first
					ObjectState[] batchInstances = instances.ToArray();
					double[] distances = new double[batchInstances.Length];
					double nearest = double.MaxValue;

					for (int i = 0; i < batchInstances.Length; i++)
					{
						double dx = batchInstances[i].WorldPosition.X - cameraX;
						double dy = batchInstances[i].WorldPosition.Y - cameraY;
						double dz = batchInstances[i].WorldPosition.Z - cameraZ;
						double d = dx * dx + dy * dy + dz * dz;
						distances[i] = d;

						if (d < nearest)
						{
							nearest = d;
						}
					}

					Array.Sort(distances, batchInstances);
					Array.Reverse(batchInstances);

					batch = new AlphaBatch
					{
						Mesh = mesh,
						Instances = batchInstances,
						Faces = new List<MeshFace>(),
						NearestDistance = nearest
					};

					batchesByMesh.Add(mesh, batch);
					batchedFaceIndices.Add(mesh, new HashSet<int>());
				}

				if (batchedFaceIndices[mesh].Add(face.Face.IboStartIndex))
				{
					batch.Faces.Add(face.Face);
				}
			}

			// Faces which cannot be batched (glow sprites, animated instances, meshes with a single instance)
			// are drawn individually, as before
			foreach (FaceState face in fallbackFaces)
			{
				if (qualityBlendPass)
				{
					// Mirrors the per-face handling of additively blended faces, which are drawn
					// without an alpha test. This also covers faces which could not be batched.
					Mesh mesh = face.Object?.Prototype?.Mesh;

					if (mesh != null && face.Face.Material >= 0 && face.Face.Material < mesh.Materials.Length && mesh.Materials[face.Face.Material].BlendMode == MeshMaterialBlendMode.Additive)
					{
						UnsetAlphaFunc();
					}
					else
					{
						SetAlphaFunc();
					}
				}

				AlphaDrawCalls++;
				face.Draw();
			}

			if (batchesByMesh.Count == 0)
			{
				return;
			}

			// Draw the batches themselves from the farthest to the nearest
			List<AlphaBatch> sortedBatches = new List<AlphaBatch>(batchesByMesh.Values);
			sortedBatches.Sort((a, b) => b.NearestDistance.CompareTo(a.NearestDistance));

			EnsureInstanceMatrixBuffer();

			Shader shader = DefaultShader;
			shader.SetInstanced(true);
			shader.SetCurrentTextureMatrix(Matrix4D.Identity);
			lastObjectState = null;

			try
			{
				for (int i = 0; i < sortedBatches.Count; i++)
				{
					AlphaBatch batch = sortedBatches[i];
					byte daytimeNighttimeBlend = blendByMesh[batch.Mesh];

					for (int start = 0; start < batch.Instances.Length; start += MaxInstancesPerBatch)
					{
						int count = Math.Min(MaxInstancesPerBatch, batch.Instances.Length - start);
						UploadInstanceMatrices(batch.Instances, start, count);

						foreach (MeshFace face in batch.Faces)
						{
							if (qualityBlendPass)
							{
								// Mirrors the caller's per-face handling of additively blended faces, which are
								// drawn without an alpha test. Set unconditionally, as drawing a nighttime polygon
								// restores the alpha function itself.
								if (batch.Mesh.Materials[face.Material].BlendMode == MeshMaterialBlendMode.Additive)
								{
									UnsetAlphaFunc();
								}
								else
								{
									SetAlphaFunc();
								}
							}

							AlphaDrawCalls += DrawFaceInstanced(shader, batch.Instances[start], face, count, daytimeNighttimeBlend);
						}
					}
				}
			}
			finally
			{
				shader.SetInstanced(false);
				lastObjectState = null;
			}
		}

		/// <summary>Checks whether batching an alpha face can be proven not to change the resulting image</summary>
		/// <param name="face">The face to check</param>
		/// <param name="qualityBlendPass">Whether the second pass of the intermediate / quality transparency modes is being drawn</param>
		private static bool IsAlphaBatchingSafe(FaceState face, bool qualityBlendPass)
		{
			if (face.AdditiveBlend)
			{
				// Additive blending is commutative, so the order never matters
				return true;
			}

			// A binary alpha contributes only fully opaque pixels, which the second pass of the intermediate /
			// quality modes discards, so the face cannot affect the image at all in that pass
			return qualityBlendPass && face.BinaryAlpha;
		}

		/// <summary>Checks whether an alpha face can be drawn as part of an instanced batch</summary>
		private static bool CanBatchAlphaFace(Mesh mesh, MeshFace face)
		{
			if (face.Vertices.Length == 0 || face.Material < 0 || face.Material >= mesh.Materials.Length)
			{
				return false;
			}

			// Glow attenuation is derived per instance from the model matrix, so it cannot share a batch
			return mesh.Materials[face.Material].GlowAttenuationData == 0;
		}

		/// <summary>Checks whether a face is drawn by the first pass of the intermediate / quality transparency modes</summary>
		/// <remarks>This must match the predicate used by the caller to select the faces for that pass</remarks>
		private static bool IsAlphaCutoutFace(Mesh mesh, MeshFace face)
		{
			if (mesh == null || face.Vertices.Length == 0 || face.Material < 0 || face.Material >= mesh.Materials.Length)
			{
				return false;
			}

			MeshMaterial material = mesh.Materials[face.Material];
			return material.BlendMode == MeshMaterialBlendMode.Normal && material.GlowAttenuationData == 0 && material.Color.A == 255;
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
