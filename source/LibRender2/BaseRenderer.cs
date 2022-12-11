using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
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
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Path = OpenBveApi.Path;
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
		protected double LastUpdatedTrackPosition;
		/// <summary>Whether ReShade is in use</summary>
		/// <remarks>Don't use OpenGL error checking with ReShade, as this breaks</remarks>
		public bool ReShadeInUse;
		/// <summary>A dummy VAO used when working with procedural data within the shader</summary>
		public VertexArrayObject dummyVao;

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
						currentHost.RegisterTexture(Path.CombineFile(fileSystem.GetDataFolder("In-game"), "logo_1024.png"), new TextureParameters(null, null), out _programLogo, true);
					}
					else if (Screen.Width > 512)
					{
						currentHost.RegisterTexture(Path.CombineFile(fileSystem.GetDataFolder("In-game"), "logo_512.png"), new TextureParameters(null, null), out _programLogo, true);
					}
					else
					{
						currentHost.RegisterTexture(Path.CombineFile(fileSystem.GetDataFolder("In-game"), "logo_256.png"), new TextureParameters(null, null), out _programLogo, true);
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
			Fonts = new Fonts(currentHost, fileSystem, currentOptions.Font);
		}

		/// <summary>
		/// Call this once to initialise the renderer
		/// </summary>
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
						if (DefaultShader != null)
						{
							DefaultShader.Deactivate();
						}
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
			Loading = new Loading(this);
			Keys = new Keys(this);
			MotionBlur = new MotionBlur(this);

			StaticObjectStates = new List<ObjectState>();
			DynamicObjectStates = new List<ObjectState>();
			VisibleObjects = new VisibleObjectLibrary(currentHost, Camera, currentOptions, this);
			whitePixel = new Texture(new Texture(1, 1, 32, new byte[] {255, 255, 255, 255}, null));
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
			string openGLdll = Path.CombineFile(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "opengl32.dll");

			if (File.Exists(openGLdll))
			{
				FileVersionInfo glVersionInfo = FileVersionInfo.GetVersionInfo(openGLdll);
				if (glVersionInfo.ProductName == @"ReShade")
				{
					ReShadeInUse = true;
				}
			}
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

			Initialize();
		}

		public int CreateStaticObject(StaticObject Prototype, Vector3 Position, Transformation WorldTransformation, Transformation LocalTransformation, ObjectDisposalMode AccurateObjectDisposal, double AccurateObjectDisposalZOffset, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness)
		{
			Matrix4D Translate = Matrix4D.CreateTranslation(Position.X, Position.Y, -Position.Z);
			Matrix4D Rotate = (Matrix4D)new Transformation(LocalTransformation, WorldTransformation);
			return CreateStaticObject(Prototype, LocalTransformation, Rotate, Translate, AccurateObjectDisposal, AccurateObjectDisposalZOffset, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness);
		}

		public int CreateStaticObject(StaticObject Prototype, Transformation LocalTransformation, Matrix4D Rotate, Matrix4D Translate, ObjectDisposalMode AccurateObjectDisposal, double AccurateObjectDisposalZOffset, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness)
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
				EndingDistance = endingDistance
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
			if (ObjectsSortedByStart == null || ObjectsSortedByStart.Length == 0)
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
			CurrentProjectionMatrix = Matrix4D.CreatePerspectiveFieldOfView(Camera.VerticalViewingAngle, Screen.AspectRatio, 0.2, 1000.0);
		}

		public void ResetShader(Shader Shader)
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
			Shader.DisableTexturing();
			Shader.SetTexture(0);
			Shader.SetBrightness(1.0f);
			Shader.SetOpacity(1.0f);
			Shader.SetObjectIndex(0);
			Shader.SetAlphaTest(false);
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
			if (AvailableNewRenderer)
			{
				CurrentShader.SetAlphaTest(true);
				CurrentShader.SetAlphaFunction(Comparison, Value);
			}
			else
			{
				GL.Enable(EnableCap.AlphaTest);
				GL.AlphaFunc(Comparison, Value);	
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
		/// <param name="State">The FaceState to draw</param>
		/// <param name="IsDebugTouchMode">Whether debug touch mode</param>
		public void RenderFace(FaceState State, bool IsDebugTouchMode = false)
		{
			RenderFace(CurrentShader, State.Object, State.Face, IsDebugTouchMode);
		}

		/// <summary>Draws a face using the specified shader and matricies</summary>
		/// <param name="Shader">The shader to use</param>
		/// <param name="State">The ObjectState to draw</param>
		/// <param name="Face">The Face within the ObjectState</param>
		/// <param name="ModelMatrix">The model matrix to use</param>
		/// <param name="ModelViewMatrix">The modelview matrix to use</param>
		public void RenderFace(Shader Shader, ObjectState State, MeshFace Face, Matrix4D ModelMatrix, Matrix4D ModelViewMatrix)
		{
			lastModelMatrix = ModelMatrix;
			lastModelViewMatrix = ModelViewMatrix;
			sendToShader = true;
			RenderFace(Shader, State, Face, false, true);
		}

		/// <summary>Draws a face using the specified shader</summary>
		/// <param name="Shader">The shader to use</param>
		/// <param name="State">The ObjectState to draw</param>
		/// <param name="Face">The Face within the ObjectState</param>
		/// <param name="IsDebugTouchMode">Whether debug touch mode</param>
		/// <param name="screenSpace">Used when a forced matrix, for items which are in screen space not camera space</param>
		public void RenderFace(Shader Shader, ObjectState State, MeshFace Face, bool IsDebugTouchMode = false, bool screenSpace = false)
		{
			if ((State != lastObjectState || State.Prototype.Dynamic) && !screenSpace)
			{
				lastModelMatrix = State.ModelMatrix * Camera.TranslationMatrix;
				lastModelViewMatrix = lastModelMatrix * CurrentViewMatrix;
				sendToShader = true;
			}

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

			if (!OptionBackFaceCulling || (Face.Flags & FaceFlags.Face2Mask) != 0)
			{
				GL.Disable(EnableCap.CullFace);
			}
			else if (OptionBackFaceCulling)
			{
				if ((Face.Flags & FaceFlags.Face2Mask) == 0)
				{
					GL.Enable(EnableCap.CullFace);
				}
			}

			// matrix
			if (sendToShader)
			{
				Shader.SetCurrentModelViewMatrix(lastModelViewMatrix);
				Shader.SetCurrentTextureMatrix(State.TextureTranslation);
				sendToShader = false;
			}
			
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

			switch (Face.Flags & FaceFlags.FaceTypeMask)
			{
				case FaceFlags.Triangles:
					DrawMode = PrimitiveType.Triangles;
					break;
				case FaceFlags.TriangleStrip:
					DrawMode = PrimitiveType.TriangleStrip;
					break;
				case FaceFlags.Quads:
					DrawMode = PrimitiveType.Quads;
					break;
				case FaceFlags.QuadStrip:
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
				distanceFactor = (float)Glow.GetDistanceFactor(lastModelMatrix, State.Prototype.Mesh.Vertices, ref Face, material.GlowAttenuationData);
			}
			else
			{
				distanceFactor = 1.0f;
			}

			float blendFactor = inv255 * State.DaytimeNighttimeBlend + 1.0f - Lighting.OptionLightingResultingAmount;
			if (blendFactor > 1.0)
			{
				blendFactor = 1.0f;
			}

			// daytime polygon
			{
				// texture
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
					Shader.DisableTexturing();
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
				Shader.SetAlphaTest(true);
				Shader.SetAlphaFunction(AlphaFunction.Greater, 0.0f);
				
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
				Shader.DisableTexturing();
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
			lastObjectState = State;
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

			if (!OptionBackFaceCulling || (Face.Flags & FaceFlags.Face2Mask) != 0)
			{
				GL.Disable(EnableCap.CullFace);
			}
			else if (OptionBackFaceCulling)
			{
				if ((Face.Flags & FaceFlags.Face2Mask) == 0)
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

			switch (Face.Flags & FaceFlags.FaceTypeMask)
			{
				case FaceFlags.Triangles:
					DrawMode = PrimitiveType.Triangles;
					break;
				case FaceFlags.TriangleStrip:
					DrawMode = PrimitiveType.TriangleStrip;
					break;
				case FaceFlags.Quads:
					DrawMode = PrimitiveType.Quads;
					break;
				case FaceFlags.QuadStrip:
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

			float blendFactor = inv255 * State.DaytimeNighttimeBlend + 1.0f - Lighting.OptionLightingResultingAmount;
			if (blendFactor > 1.0)
			{
				blendFactor = 1.0f;
			}

			// daytime polygon
			{
				// texture
				if (material.DaytimeTexture != null)
				{
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
