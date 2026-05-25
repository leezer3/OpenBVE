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
using LibRender2.Managers;
using LibRender2.Pipeline;
using LibRender2.Primitives;
using LibRender2.Screens;
using LibRender2.Shaders;
using LibRender2.Shadows;
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
		protected internal BaseOptions currentOptions;

		/// <summary>The scene manager.</summary>
		public SceneManager Scene;

		protected internal double LastUpdatedTrackPosition;
		/// <summary>Whether ReShade is in use</summary>
		/// <remarks>Don't use OpenGL error checking with ReShade, as this breaks</remarks>
		public bool ReShadeInUse;
		/// <summary>A dummy VAO used when working with procedural data within the shader</summary>
		public VertexArrayObject dummyVao;

		/// <summary>The graphics device state manager.</summary>
		public GraphicsDevice Device;

		/// <summary>The render pipeline.</summary>
		public RenderPipeline Pipeline;

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

		/// <summary>The GPU resource manager.</summary>
		public LibRender2.Managers.GpuResourceManager ResourceManager;

		public VisibleObjectLibrary VisibleObjects => Scene.VisibleObjects;
		public object VisibilityUpdateLock => Scene.VisibilityUpdateLock;
		public bool VisibilityThreadShouldRun
		{
			get => Scene.VisibilityThreadShouldRun;
			set => Scene.VisibilityThreadShouldRun = value;
		}

		public void BindCSMToDefaultShader()
		{
			if (CSMShadowMaps == null || DefaultShader == null)
			{
				return;
			}

			Shader shader = DefaultShader as Shader;
			if (shader == null) return;

			shader.SetShadowEnabled(true);
			shader.SetCascadeCount(CSMCaster.CascadeCount);

			for (int i = 0; i < CSMCaster.CascadeCount; i++)
			{
				// Shadows use texture units 4, 5, 6, 7
				SetActiveTexture(TextureUnit.Texture4 + i);
				GL.BindTexture(TextureTarget.Texture2D, CSMShadowMaps.DepthTextures[i]);
				shader.SetCascadeShadowMapUnit(i, 4 + i);
				shader.SetCascadeLightSpaceMatrix(i, CSMCaster.LightSpaceMatrices[i]);
				shader.SetCascadeFarDistance(i, (float)CSMCaster.CascadeFarDistances[i]);
				shader.SetCascadeBias(i, CSMCaster.CascadeBiases[i] + (float)currentOptions.ShadowBias);
				shader.SetNormalBias(i, (float)currentOptions.ShadowNormalBias);
			}

			shader.SetShadowStrength(Lighting.ShadowStrength);
			SetActiveTexture(TextureUnit.Texture0);
		}

		public ConcurrentQueue<ThreadStart> RenderThreadJobs => Scene.RenderThreadJobs;

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
		protected internal AbstractShader CurrentShader;

		public Shader DefaultShader;
		
		/// <summary>Cascaded shadow map FBOs + depth textures.</summary>
		public CascadedShadowMap CSMShadowMaps;
		
		/// <summary>Computes per-cascade light-space matrices.</summary>
		public CascadedShadowCaster CSMCaster;
		
		/// <summary>The shadow depth shader for the depth-only pass.</summary>
		public ShadowDepthShader ShadowDepthShaderProgram;
		
		/// <summary>Whether shadows are enabled.</summary>
		public bool ShadowsEnabled = true;

		/// <summary>Shadow strength: 0=invisible, 1=full darkness.</summary>
		public float ShadowStrength = 0.7f;

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
		private struct BlendState
		{
			public bool Enabled;
			public BlendingFactor SrcFactor;
			public BlendingFactor DestFactor;
		}

		private readonly Stack<BlendState> blendStack = new Stack<BlendState>();

		public void PushBlendFunc()
		{
			blendStack.Push(new BlendState
			{
				Enabled = blendEnabled,
				SrcFactor = blendSrcFactor,
				DestFactor = blendDestFactor
			});
		}

		public void PopBlendFunc()
		{
			if (blendStack.Count > 0)
			{
				BlendState state = blendStack.Pop();
				blendEnabled = state.Enabled;
				blendSrcFactor = state.SrcFactor;
				blendDestFactor = state.DestFactor;
				RestoreBlendFunc();
			}
		}

		public int InfoTotalPolygon;

		/// <summary>The game's current framerate</summary>
		public double FrameRate = 1.0;

		/// <summary>Whether Blend is enabled in openGL</summary>
		internal bool blendEnabled;

		internal BlendingFactor blendSrcFactor = BlendingFactor.SrcAlpha;

		internal BlendingFactor blendDestFactor = BlendingFactor.OneMinusSrcAlpha;

		/// <summary>Whether Alpha Testing is enabled in openGL</summary>
		private bool alphaTestEnabled;
		private bool cullFaceEnabled;
		private CullFaceMode cullFaceMode;
		private bool depthTestEnabled;
		private DepthFunction depthTestFunction;
		private bool depthMaskEnabled = true;

		private AlphaFunction alphaTestFunction;
		private float alphaTestComparison;

		private TextureUnit lastActiveTextureUnit = TextureUnit.Texture0;

		/// <summary>The current AlphaFunc comparison</summary>
		private AlphaFunction alphaFuncComparison;

		/// <summary>The current AlphaFunc comparison value</summary>
		private float alphaFuncValue;

		/// <summary>Stores the most recently bound texture</summary>
		public OpenGlTexture LastBoundTexture;
		private OpenGlTexture lastBoundNightTexture;

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
		/// <summary>A dummy 1x1 depth texture with comparison enabled, used when shadows are disabled to satisfy driver requirements.</summary>
		private int nullDepthMap; 

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

		public Dictionary<Texture, HashSet<Vector3>> CubesToDraw = new Dictionary<Texture, HashSet<Vector3>>();

		public bool AvailableNewRenderer => currentOptions != null && currentOptions.IsUseNewRenderer && !ForceLegacyOpenGL;

		protected BaseRenderer(HostInterface CurrentHost, BaseOptions CurrentOptions, FileSystem FileSystem)
		{
			currentHost = CurrentHost;
			currentOptions = CurrentOptions;
			fileSystem = FileSystem;
			if (CurrentHost.Application != HostApplication.TrainEditor && CurrentHost.Application != HostApplication.TrainEditor2)
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
			Marker = new Marker(this);

			projectionMatrixList = new List<Matrix4D>();
			viewMatrixList = new List<Matrix4D>();
			Device = new GraphicsDevice();
			Pipeline = new RenderPipeline();
			Scene = new SceneManager(this);
			ResourceManager = new LibRender2.Managers.GpuResourceManager();
			Fonts = new Fonts(currentHost, this, currentOptions.Font);
		}


		/// <summary>Call this once to initialise the renderer</summary>
		/// <remarks>A call to DeInitialize should be made when closing the program to release resources</remarks>
		[HandleProcessCorruptedStateExceptions] //As some graphics cards crash really nastily if we request unsupported features
		public virtual void Initialize()
		{
			if (!ForceLegacyOpenGL && (currentOptions.IsUseNewRenderer || currentHost.Application != HostApplication.OpenBve)) // GL3 has already failed. Don't trigger unnecessary exceptions
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
					DefaultShader.SetNightTexture(1);
					lastColor = Color32.White;
					DefaultShader.Deactivate();
					dummyVao = new VertexArrayObject();
				}
				catch
				{
					currentHost.AddMessage(MessageType.Error, false, "Initializing the default shaders failed- Falling back to legacy openGL.");
					currentOptions.IsUseNewRenderer = false;
					ForceLegacyOpenGL = true;
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
					currentHost.AddMessage(MessageType.Error, false, "Initializing the default shaders failed- Falling back to legacy openGL.");
					currentOptions.IsUseNewRenderer = false;
					ForceLegacyOpenGL = true;
				}
			}
			else
			{
				ForceLegacyOpenGL = true;
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

			Scene.StaticObjectStates = new List<ObjectState>();
			Scene.DynamicObjectStates = new List<ObjectState>();
			Scene.VisibleObjects = new VisibleObjectLibrary(this);
			whitePixel = new Texture(new Texture(1, 1, PixelFormat.RGBAlpha, new byte[] {255, 255, 255, 255}, null));
			if (AvailableNewRenderer)
			{
				nullDepthMap = GL.GenTexture();
				GL.BindTexture(TextureTarget.Texture2D, nullDepthMap);
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent16, 1, 1, 0, OpenTK.Graphics.OpenGL.PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.CompareRefToTexture);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
				GL.BindTexture(TextureTarget.Texture2D, 0);
			}
			GL.ClearColor(0.67f, 0.67f, 0.67f, 1.0f);
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
			currentHost.RegisterTexture(Path.CombineFile(fileSystem.GetDataFolder("Menu"), "zuki.png"), TextureParameters.NoChange, out MasconTexture);
			currentHost.RegisterTexture(Path.CombineFile(fileSystem.GetDataFolder("Menu"), "joystick.png"), TextureParameters.NoChange, out JoystickTexture);
			currentHost.RegisterTexture(Path.CombineFile(fileSystem.GetDataFolder("Menu"), "raildriver.png"), TextureParameters.NoChange, out RailDriverTexture);

			if (AvailableNewRenderer)
			{
				InitializeShadows();
			}
		}

		/// <summary>
		/// Initializes (or reinitializes) shadow mapping from current options.
		/// Call after GL context creation and whenever shadow settings change.
		/// </summary>
		public void InitializeShadows()
		{
			// Read options
			var opts = currentOptions;

			// If shadows are off, dispose and disable
			if (opts.ShadowResolution == ShadowMapResolution.Off)
			{
				DisposeShadows();
				ShadowsEnabled = false;
				fileSystem.AppendToLogFile("[CSM] Shadows disabled by user setting.");
				return;
			}

			int resolution = Math.Max(1, (int)opts.ShadowResolution);
			int cascadeCount = (int)opts.ShadowCascades;
			double shadowDistance = opts.ShadowDrawDistance == ShadowDistance.ViewingDistance ? opts.ViewingDistance : (double)(int)opts.ShadowDrawDistance;
			shadowDistance = Math.Max(1.0, shadowDistance);
			float shadowStrength = (float)opts.ShadowStrength;

			try
			{
				if (CSMShadowMaps == null)
				{
					// First-time creation
					CSMShadowMaps = new CascadedShadowMap(cascadeCount, resolution);
				}
				else
				{
					// Hot-reload: resize existing maps
					CSMShadowMaps.Resize(cascadeCount, resolution);
				}

				if (CSMCaster == null || cascadeCount != CSMCaster.CascadeCount)
				{
					// Create, or update if cascade count changed
					CSMCaster = new CascadedShadowCaster(cascadeCount);
				}

				CSMCaster.ShadowDistance = shadowDistance;
				CSMCaster.Resolution = resolution;
				CSMCaster.SplitLambda = 0.75;
				CSMCaster.DepthMargin = 150.0;

				ShadowStrength = shadowStrength;
				Lighting.ShadowStrength = shadowStrength;

				if (ShadowDepthShaderProgram == null)
				{
					ShadowDepthShaderProgram = new ShadowDepthShader(this, "shadow_depth", "shadow_depth", true);
				}

				ShadowsEnabled = true;

				fileSystem.AppendToLogFile($"[CSM] Initialized: {cascadeCount} cascades, " + $"{resolution}×{resolution}, distance={shadowDistance}m, " + $"strength={shadowStrength:P0}");
			}
			catch (Exception ex)
			{
				fileSystem.AppendToLogFile($"[CSM] Init failed: {ex.Message}");
				ShadowsEnabled = false;
				// Purge lingering OpenGL error state if any, to avoid crashing later in ResetShader
				GL.GetError();
			}
		}

		/// <summary>Disposes all shadow GPU resources.</summary>
		public void DisposeShadows()
		{
			CSMShadowMaps?.Dispose();
			CSMShadowMaps = null;
			ShadowDepthShaderProgram?.Dispose();
			ShadowDepthShaderProgram = null;
			CSMCaster = null;
			ShadowsEnabled = false;
		}

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
			if (nullDepthMap != 0)
			{
				GL.DeleteTexture(nullDepthMap);
				nullDepthMap = 0;
			}
			GameWindow?.Dispose();
			Scene?.DeInitialize();
		}

		/// <summary>Should be called when the OpenGL version is switched mid-game</summary>
		/// <remarks>We need to purge the current shader state and update lighting to avoid glitches</remarks>
		public void SwitchOpenGLVersion()
		{
			GL.UseProgram(0);
			currentOptions.IsUseNewRenderer = !currentOptions.IsUseNewRenderer;
			ResetOpenGlState();
			Lighting.Initialize();
			if (currentOptions.IsUseNewRenderer && AvailableNewRenderer)
			{
				ReloadShadowSettings();
			}
			// Drain errors to ensure the shader reset in the next frame doesn't pick up legacy errors
			while (GL.GetError() != ErrorCode.NoError) { }
		}

		/// <summary>
		/// Executes all registered render passes in the pipeline.
		/// </summary>
		/// <param name="context">The current rendering context.</param>
		public void ExecutePipeline(RenderContext context)
		{
			Pipeline.Execute(context);
		}


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

		/// <summary>Should be called at the start of each frame to release any GPU resources queued for deletion</summary>
		public void ReleaseResources()
		{
			ResourceManager.ReleaseResources();
		}

		/// <summary>
		/// Performs a reset of OpenGL to the default state
		/// </summary>
		public virtual void ResetOpenGlState()
		{
			GL.Enable(EnableCap.CullFace);
			GL.CullFace(CullFaceMode.Front);
			cullFaceEnabled = true;
			cullFaceMode = CullFaceMode.Front;

			if (!AvailableNewRenderer)
			{
				GL.Disable(EnableCap.Lighting);
				GL.Disable(EnableCap.Fog);
				GL.Disable(EnableCap.Texture2D);
			}
			
			SetBlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			UnsetBlendFunc();
			
			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Lequal);
			depthTestEnabled = true;
			depthTestFunction = DepthFunction.Lequal;

			GL.Disable(EnableCap.DepthClamp);
			
			GL.DepthMask(true);
			depthMaskEnabled = true;

			SetActiveTexture(TextureUnit.Texture0);
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
			Scene.Reset();
		}

		public int CreateStaticObject(StaticObject Prototype, Vector3 Position, Transformation WorldTransformation, Transformation LocalTransformation, ObjectDisposalMode AccurateObjectDisposal, ObjectCreationParameters Parameters, double BlockLength)
		{
			Matrix4D Translate = Matrix4D.CreateTranslation(Position.X, Position.Y, -Position.Z);
			Matrix4D Rotate = (Matrix4D)new Transformation(LocalTransformation ?? Transformation.NullTransformation, WorldTransformation);
			return CreateStaticObject(Position, Prototype, LocalTransformation, Rotate, Translate, AccurateObjectDisposal, Parameters, BlockLength);
		}

		public int CreateStaticObject(Vector3 Position, StaticObject Prototype, Transformation LocalTransformation, Matrix4D Rotate, Matrix4D Translate, ObjectDisposalMode AccurateObjectDisposal, ObjectCreationParameters Parameters, double BlockLength)
		{
			return Scene.CreateStaticObject(Position, Prototype, LocalTransformation, Rotate, Translate, AccurateObjectDisposal, Parameters, BlockLength);
		}

		public void CreateDynamicObject(ref ObjectState internalObject)
		{
			Scene.CreateDynamicObject(ref internalObject);
		}

		/// <summary>Initializes the visibility of all objects within the game world</summary>
		/// <remarks>If the new renderer is enabled, this must be run in a thread processing an openGL context in order to successfully create
		/// the required VAO objects</remarks>
		public void InitializeVisibility()
		{
			Scene.InitializeVisibility();
		}

		public void UpdateVisibility(bool force)
		{
			Scene.UpdateVisibility(force);
		}

		public void UpdateViewingDistances(double backgroundImageDistance)
		{
			Scene.UpdateViewingDistances(backgroundImageDistance);
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
			blendEnabled = true;
			SetBlendFunc(blendSrcFactor, blendDestFactor);
		}

		public void SetBlendFunc(BlendingFactor srcFactor, BlendingFactor destFactor)
		{
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

		public void SetColorMask(bool red, bool green, bool blue, bool alpha)
		{
			GL.ColorMask(red, green, blue, alpha);
		}

		public void SetDepthMask(bool enabled)
		{
			if (depthMaskEnabled != enabled)
			{
				GL.DepthMask(enabled);
				depthMaskEnabled = enabled;
			}
		}

		public void SetCullFace(bool enabled, CullFaceMode mode = CullFaceMode.Front)
		{
			if (enabled)
			{
				if (!cullFaceEnabled)
				{
					GL.Enable(EnableCap.CullFace);
					cullFaceEnabled = true;
				}

				if (cullFaceMode != mode)
				{
					GL.CullFace(mode);
					cullFaceMode = mode;
				}
			}
			else
			{
				if (cullFaceEnabled)
				{
					GL.Disable(EnableCap.CullFace);
					cullFaceEnabled = false;
				}
			}
		}

		public void SetDepthTest(bool enabled, DepthFunction function = DepthFunction.Lequal)
		{
			if (enabled)
			{
				if (!depthTestEnabled)
				{
					GL.Enable(EnableCap.DepthTest);
					depthTestEnabled = true;
				}

				if (depthTestFunction != function)
				{
					GL.DepthFunc(function);
					depthTestFunction = function;
				}
			}
			else
			{
				if (depthTestEnabled)
				{
					GL.Disable(EnableCap.DepthTest);
					depthTestEnabled = false;
				}
			}
		}

		public void SetActiveTexture(TextureUnit unit)
		{
			if (lastActiveTextureUnit != unit)
			{
				GL.ActiveTexture(unit);
				lastActiveTextureUnit = unit;
			}
		}

		public void SetViewport(int x, int y, int width, int height)
		{
			GL.Viewport(x, y, width, height);
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
			RenderFace(CurrentShader as Shader, state.Object, state.Face, isDebugTouchMode);
		}

		public void RenderFace(AbstractShader shader, ObjectState state, MeshFace face, bool debugTouchMode = false, bool screenSpace = false, int vertexCount = -1)
		{
			if (shader is Shader defaultShader)
			{
				RenderFace(defaultShader, state, face, debugTouchMode, screenSpace, vertexCount);
				return;
			}

			if (shader is ShadowDepthShader shadowShader)
			{
				RenderFaceShadow(shadowShader, state, face, vertexCount);
				return;
			}
		}

		private void RenderFaceShadow(ShadowDepthShader shader, ObjectState state, MeshFace face, int vertexCount)
		{
			if (state.Prototype == null || state.Prototype.Mesh == null || state.Prototype.Mesh.VAO == null)
			{
				return;
			}

			if (vertexCount == -1)
			{
				vertexCount = face.Vertices.Length;
			}

			if (state != lastObjectState || state.Prototype.Dynamic)
			{
				lastModelMatrix = state.ModelMatrix * Camera.TranslationMatrix;
				shader.SetModelMatrix(lastModelMatrix);

				if (state.Matricies != null && state.Matricies.Length > 0)
				{
					shader.SetCurrentAnimationMatricies(state);
#pragma warning disable CS0618
					GL.BindBufferBase(BufferTarget.UniformBuffer, 0, state.MatrixBufferIndex);
#pragma warning restore CS0618
				}
			}

			MeshMaterial material = state.Prototype.Mesh.Materials[face.Material];
			VertexArrayObject VAO = (VertexArrayObject)state.Prototype.Mesh.VAO;

			if (lastVAO != VAO.handle)
			{
				VAO.Bind();
				lastVAO = VAO.handle;
			}

			bool needsTexture = material.DaytimeTexture != null &&
								((material.Flags & MaterialFlags.TransparentColor) != 0 ||
								 material.Color.A < 255);

			if (needsTexture && currentHost.LoadTexture(ref material.DaytimeTexture, (OpenGlTextureWrapMode)(material.WrapMode ?? OpenGlTextureWrapMode.ClampClamp)))
			{
				SetActiveTexture(TextureUnit.Texture0);
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
			VAO.Draw(drawMode, face.IboStartIndex, vertexCount);

			lastObjectState = state;
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
		public void RenderFace(Shader shader, ObjectState state, MeshFace face, bool debugTouchMode = false, bool screenSpace = false, int vertexCount = -1)
		{
			if (vertexCount == -1)
			{
				vertexCount = face.Vertices.Length;
			}
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
				SetCullFace(false);
			}
			else if (OptionBackFaceCulling)
			{
				if ((face.Flags & FaceFlags.Face2Mask) == 0)
				{
					SetCullFace(true);
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
					SetBlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
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

				if (blendFactor != 0 && material.NighttimeTexture != null && material.NighttimeTexture != material.DaytimeTexture && currentHost.LoadTexture(ref material.NighttimeTexture, (OpenGlTextureWrapMode)material.WrapMode))
				{
					OpenGlTexture nightTexture = material.NighttimeTexture.OpenGlTextures[(int)material.WrapMode];
					if (lastBoundNightTexture != nightTexture)
					{
						SetActiveTexture(TextureUnit.Texture1);
						GL.BindTexture(TextureTarget.Texture2D, nightTexture.Name);
						lastBoundNightTexture = nightTexture;
						SetActiveTexture(TextureUnit.Texture0);
					}
					shader.SetIsNightTexture(true);
					shader.SetNightBlendFactor(blendFactor);
				}
				else
				{
					shader.SetIsNightTexture(false);
				}

				float alphaFactor = distanceFactor;
				if (material.NighttimeTexture != null && (material.Flags & MaterialFlags.CrossFadeTexture) != 0)
				{
					alphaFactor *= 1.0f - blendFactor;
				}

				shader.SetOpacity(inv255 * material.Color.A * alphaFactor);

				// render polygon
				VAO.Draw(drawMode, face.IboStartIndex, vertexCount);
				if (material.BlendMode == MeshMaterialBlendMode.Additive)
				{
					RestoreBlendFunc();
					shader.SetFog(true);
				}
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
				normalsVao.Draw(PrimitiveType.Lines, face.NormalsIboStartIndex, vertexCount * 2);
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

			GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, (material.Flags & MaterialFlags.Emissive) != 0 ? new Color4(material.EmissiveColor.R, material.EmissiveColor.G, material.EmissiveColor.B, material.EmissiveColor.A) : Color4.Black);

			// fog
			if (Fog.Enabled)
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
					SetBlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
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

				Device.SetBlend(true);

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

		public void ShowObject(ObjectState Object, ObjectType Type)
		{
			Scene.ShowObject(Object, Type);
		}

		public void HideObject(ObjectState Object)
		{
			Scene.HideObject(Object);
		}

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
