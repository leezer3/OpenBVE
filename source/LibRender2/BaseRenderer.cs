﻿using System;
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
using OpenBveApi.Graphics;
using OpenBveApi.Hosts;
using OpenBveApi.Objects;
using OpenBveApi.Textures;
using OpenBveApi.World;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

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
		private BaseOptions currentOptions;

		public List<ObjectState> StaticObjectStates;
		public List<ObjectState> DynamicObjectStates;
		public VisibleObjectLibrary VisibleObjects;

		protected int[] ObjectsSortedByStart;
		protected int[] ObjectsSortedByEnd;
		protected int ObjectsSortedByStartPointer;
		protected int ObjectsSortedByEndPointer;
		protected double LastUpdatedTrackPosition;

		public Screen Screen;
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

		public Matrix4d CurrentProjectionMatrix;
		public Matrix4d CurrentViewMatrix;

		protected List<Matrix4d> projectionMatrixList;
		protected List<Matrix4d> viewMatrixList;

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

		public BaseRenderer()
		{
			Screen = new Screen();
			Camera = new CameraProperties();
			Lighting = new Lighting();
			Marker = new Marker();

			projectionMatrixList = new List<Matrix4d>();
			viewMatrixList = new List<Matrix4d>();
		}

		/// <summary>
		/// Call this once to initialise the renderer
		/// </summary>
		public virtual void Initialize(HostInterface CurrentHost, BaseOptions CurrentOptions)
		{
			currentHost = CurrentHost;
			currentOptions = CurrentOptions;

			Background = new Background(this);
			Fog = new Fog();
			OpenGlString = new OpenGlString(this);
			TextureManager = new TextureManager(currentHost);
			Cube = new Cube(this);
			Rectangle = new Rectangle(this);
			Loading = new Loading(this);
			Keys = new Keys(this);
			MotionBlur = new MotionBlur(this);

			StaticObjectStates = new List<ObjectState>();
			DynamicObjectStates = new List<ObjectState>();
			VisibleObjects = new VisibleObjectLibrary(currentHost, Camera, currentOptions);

			DefaultShader = new Shader("default", "default", true);
			DefaultShader.Use();
			DefaultShader.NonUse();

			GL.ClearColor(0.67f, 0.67f, 0.67f, 1.0f);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Lequal);
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
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
		}

		public void Finalization()
		{
			foreach (FrameBufferObject fbo in FrameBufferObject.Disposable)
			{
				fbo.Dispose();
			}

			foreach (VertexArrayObject vao in VertexArrayObject.Disposable)
			{
				vao.Dispose();
			}

			foreach (Shader shader in Shader.Disposable)
			{
				shader.Dispose();
			}
		}

		/// <summary>
		/// Performs a reset of OpenGL to the default state
		/// </summary>
		public virtual void ResetOpenGlState()
		{
			GL.Enable(EnableCap.CullFace);
			GL.Disable(EnableCap.Texture2D);
			GL.Disable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			GL.Enable(EnableCap.DepthTest);
			GL.DepthMask(true);
			DefaultShader.Use();
			ResetShader(DefaultShader);
			DefaultShader.SetIsAlphaTest(true);
			DefaultShader.SetAlphaFuncType(AlphaFuncType.Greater);
			DefaultShader.SetAlphaFuncValue(0.9f);
			DefaultShader.NonUse();
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
			Initialize(currentHost, currentOptions);
		}

		public int CreateStaticObject(StaticObject Prototype, OpenBveApi.Math.Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, bool AccurateObjectDisposal, double AccurateObjectDisposalZOffset, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness)
		{
			if (Prototype == null)
			{
				return -1;
			}

			float startingDistance = float.MaxValue;
			float endingDistance = float.MinValue;

			if (AccurateObjectDisposal)
			{
				foreach (VertexTemplate vertex in Prototype.Mesh.Vertices)
				{
					OpenBveApi.Math.Vector3 Coordinates = vertex.Coordinates;
					Coordinates.Rotate(AuxTransformation);

					if (Coordinates.Z < StartingDistance)
					{
						startingDistance = (float)Coordinates.Z;
					}

					if (Coordinates.Z > EndingDistance)
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
				Translation = Matrix4d.CreateTranslation(Position.X, Position.Y, -Position.Z),
				Rotate = (Matrix4d)new Transformation(BaseTransformation, AuxTransformation),
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

		public virtual void InitializeVisibility()
		{
			ObjectsSortedByStart = StaticObjectStates.Select((x, i) => new { Index = i, Distance = x.StartingDistance }).OrderBy(x => x.Distance).Select(x => x.Index).ToArray();
			ObjectsSortedByEnd = StaticObjectStates.Select((x, i) => new { Index = i, Distance = x.EndingDistance }).OrderBy(x => x.Distance).Select(x => x.Index).ToArray();
			ObjectsSortedByStartPointer = 0;
			ObjectsSortedByEndPointer = 0;

			double p = Camera.Alignment.Position.Z;

			foreach (ObjectState state in StaticObjectStates.Where(recipe => recipe.StartingDistance <= p + Camera.ForwardViewingDistance & recipe.EndingDistance >= p - Camera.BackwardViewingDistance))
			{
				VisibleObjects.ShowObject(state, ObjectType.Static);
			}
		}

		public virtual void UpdateVisibility(double TrackPosition)
		{
			double d = TrackPosition - LastUpdatedTrackPosition;
			int n = ObjectsSortedByStart.Length;
			double p = Camera.Alignment.Position.Z;

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
			string[] Extensions = GL.GetString(StringName.Extensions).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
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

		public void UpdateViewport()
		{
			UpdateViewport(Screen.Width, Screen.Height);
		}

		public virtual void UpdateViewport(int Width, int Height)
		{
			Screen.Width = Width;
			Screen.Height = Height;
			GL.Viewport(0, 0, Screen.Width, Screen.Height);

			Screen.AspectRatio = Screen.Width / (double)Screen.Height;
			Camera.HorizontalViewingAngle = 2.0 * Math.Atan(Math.Tan(0.5 * Camera.VerticalViewingAngle) * Screen.AspectRatio);
			CurrentProjectionMatrix = Matrix4d.CreatePerspectiveFieldOfView(Camera.VerticalViewingAngle, Screen.AspectRatio, 0.2, 1000.0);
		}

		public void ResetShader(Shader Shader)
		{
			ErrorCode message = GL.GetError();

			if (message != ErrorCode.NoError)
			{
				throw new InvalidOperationException($"OpenGL Error: {message.ToString()}");
			}

			Shader.SetCurrentTranslateMatrix(Matrix4d.Identity);
			Shader.SetCurrentScaleMatrix(Matrix4d.Identity);
			Shader.SetCurrentRotateMatrix(Matrix4d.Identity);
			Shader.SetCurrentTextureTranslateMatrix(Matrix4d.Identity);
			Shader.SetCurrentProjectionMatrix(Matrix4d.Identity);
			Shader.SetCurrentViewMatrix(Matrix4d.Identity);
			Shader.SetEyePosition(Vector3.Zero);
			Shader.SetIsLight(false);
			Shader.SetLightPosition(Vector3.Zero);
			Shader.SetLightAmbient(Color4.White);
			Shader.SetLightDiffuse(Color4.White);
			Shader.SetLightSpecular(Color4.White);
			Shader.SetMaterialAmbient(Color4.White);
			Shader.SetMaterialDiffuse(Color4.White);
			Shader.SetMaterialSpecular(Color4.White);
			Shader.SetMaterialEmission(Color4.White);
			Shader.SetMaterialShininess(1.0f);
			Shader.SetIsFog(false);
			Shader.SetFogStart(0.0f);
			Shader.SetFogEnd(0.0f);
			Shader.SetFogColor(Color4.White);
			Shader.SetIsTexture(false);
			Shader.SetTexture(0);
			Shader.SetBrightness(1.0f);
			Shader.SetOpacity(1.0f);
			Shader.SetIsAlphaTest(false);
			Shader.SetAlphaFuncType(AlphaFuncType.Always);
			Shader.SetAlphaFuncValue(1.0f);
			Shader.SetObjectIndex(0);
			GL.GetError();
		}

		public void RenderFace(Shader Shader, FaceState State, bool IsDebugTouchMode = false)
		{
			RenderFace(Shader, State.Object, State.Face, IsDebugTouchMode);
		}

		public void RenderFace(Shader Shader, FaceState State, Vector3d EyePosition, bool IsDebugTouchMode = false)
		{
			RenderFace(Shader, State.Object, State.Face, EyePosition, IsDebugTouchMode);
		}

		public void RenderFace(Shader Shader, ObjectState State, MeshFace Face, bool IsDebugTouchMode = false)
		{
			RenderFace(Shader, State, Face, new Vector3d(Camera.AbsolutePosition.X, Camera.AbsolutePosition.Y, -Camera.AbsolutePosition.Z), IsDebugTouchMode);
		}

		public void RenderFace(Shader Shader, ObjectState State, MeshFace Face, Vector3d EyePosition, bool IsDebugTouchMode = false)
		{
			if (State.Prototype.Mesh.Vertices.Length < 1)
			{
				return;
			}

			VertexTemplate[] vertices = State.Prototype.Mesh.Vertices;
			MeshMaterial material = State.Prototype.Mesh.Materials[Face.Material];
			VertexArrayObject VAO = State.Prototype.Mesh.VAO;
			VertexArrayObject NormalsVAO = State.Prototype.Mesh.NormalsVAO;

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
			Shader.SetCurrentTranslateMatrix(State.Translation * Matrix4d.CreateTranslation(-EyePosition));
			Shader.SetCurrentScaleMatrix(State.Scale);
			Shader.SetCurrentRotateMatrix(State.Rotate);
			Shader.SetCurrentTextureTranslateMatrix(State.TextureTranslation);
			Shader.SetCurrentProjectionMatrix(CurrentProjectionMatrix);
			Shader.SetCurrentViewMatrix(CurrentViewMatrix);

			if (OptionWireFrame || IsDebugTouchMode)
			{
				VAO.Bind();
				VAO.Draw(Shader.VertexLayout, PrimitiveType.LineLoop, Face.IboStartIndex, Face.Vertices.Length);
				VAO.UnBind();
				return;
			}

			// lighting
			if (material.NighttimeTexture == null)
			{
				if (OptionLighting)
				{
					Shader.SetEyePosition(new Vector3((float)EyePosition.X, (float)EyePosition.Y, (float)EyePosition.Z));
					Shader.SetIsLight(true);
					Shader.SetLightPosition(new Vector3((float)Lighting.OptionLightPosition.X, (float)Lighting.OptionLightPosition.Y, -(float)Lighting.OptionLightPosition.Z));
					Shader.SetLightAmbient(new Color4(Lighting.OptionAmbientColor.R, Lighting.OptionAmbientColor.G, Lighting.OptionAmbientColor.B, 255));
					Shader.SetLightDiffuse(new Color4(Lighting.OptionDiffuseColor.R, Lighting.OptionDiffuseColor.G, Lighting.OptionDiffuseColor.B, 255));
					Shader.SetLightSpecular(new Color4(Lighting.OptionSpecularColor.R, Lighting.OptionSpecularColor.G, Lighting.OptionSpecularColor.B, 255));
					Shader.SetMaterialAmbient(new Color4(material.Color.R, material.Color.G, material.Color.B, material.Color.A));  // TODO
					Shader.SetMaterialDiffuse(new Color4(material.Color.R, material.Color.G, material.Color.B, material.Color.A));
					Shader.SetMaterialSpecular(new Color4(material.Color.R, material.Color.G, material.Color.B, material.Color.A));  // TODO

					if ((material.Flags & MeshMaterial.EmissiveColorMask) != 0)
					{
						Shader.SetMaterialEmission(new Color4(material.EmissiveColor.R, material.EmissiveColor.G, material.EmissiveColor.B, 255));
					}
					else
					{
						Shader.SetMaterialEmission(new Color4(0.0f, 0.0f, 0.0f, 0.0f));
					}

					Shader.SetMaterialShininess(1.0f);

					Lighting.OptionLightingResultingAmount = (Lighting.OptionAmbientColor.R + Lighting.OptionAmbientColor.G + Lighting.OptionAmbientColor.B) / 480.0f;

					if (Lighting.OptionLightingResultingAmount > 1.0f)
					{
						Lighting.OptionLightingResultingAmount = 1.0f;
					}
				}
			}

			// fog
			if (OptionFog)
			{
				Shader.SetIsFog(true);
				Shader.SetFogStart(Fog.Start);
				Shader.SetFogEnd(Fog.End);
				Shader.SetFogColor(new Color4(Fog.Color.R, Fog.Color.G, Fog.Color.B, 255));
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

			// daytime polygon
			{
				// texture
				if (material.DaytimeTexture != null)
				{
					if (currentHost.LoadTexture(material.DaytimeTexture, (OpenGlTextureWrapMode)material.WrapMode))
					{
						if (!GL.IsEnabled(EnableCap.Texture2D))
						{
							GL.Enable(EnableCap.Texture2D);
						}

						Shader.SetIsTexture(true);
						Shader.SetTexture(0);
						GL.BindTexture(TextureTarget.Texture2D, material.DaytimeTexture.OpenGlTextures[(int)material.WrapMode].Name);
					}
				}

				// blend mode
				float factor;
				if (material.BlendMode == MeshMaterialBlendMode.Additive)
				{
					factor = 1.0f;
					GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
				}
				else if (material.NighttimeTexture == null)
				{
					float blend = inv255 * material.DaytimeNighttimeBlend + 1.0f - Lighting.OptionLightingResultingAmount;

					if (blend > 1.0f)
					{
						blend = 1.0f;
					}

					factor = 1.0f - 0.7f * blend;
				}
				else
				{
					factor = 1.0f;
				}
				Shader.SetBrightness(factor);

				float alphaFactor;
				if (material.GlowAttenuationData != 0)
				{
					alphaFactor = (float)Glow.GetDistanceFactor(vertices, ref Face, material.GlowAttenuationData, Camera.AbsolutePosition);
				}
				else
				{
					alphaFactor = 1.0f;
				}
				Shader.SetOpacity(alphaFactor);

				// render polygon
				VAO.Bind();
				VAO.Draw(Shader.VertexLayout, DrawMode, Face.IboStartIndex, Face.Vertices.Length);
				VAO.UnBind();

				GL.BindTexture(TextureTarget.Texture2D, 0);
			}

			// nighttime polygon
			if (material.NighttimeTexture != null && currentHost.LoadTexture(material.NighttimeTexture, (OpenGlTextureWrapMode)material.WrapMode))
			{
				if (!GL.IsEnabled(EnableCap.Texture2D))
				{
					GL.Enable(EnableCap.Texture2D);
				}

				// texture
				Shader.SetIsTexture(true);
				Shader.SetTexture(0);
				GL.BindTexture(TextureTarget.Texture2D, material.NighttimeTexture.OpenGlTextures[(int)material.WrapMode].Name);

				// alpha test
				Shader.SetIsAlphaTest(true);
				Shader.SetAlphaFuncType(AlphaFuncType.Greater);
				Shader.SetAlphaFuncValue(0.0f);

				// blend mode
				float alphaFactor;
				if (material.GlowAttenuationData != 0)
				{
					alphaFactor = (float)Glow.GetDistanceFactor(vertices, ref Face, material.GlowAttenuationData, Camera.AbsolutePosition);
					float blend = inv255 * material.DaytimeNighttimeBlend + 1.0f - Lighting.OptionLightingResultingAmount;
					if (blend > 1.0f)
					{
						blend = 1.0f;
					}

					alphaFactor *= blend;
				}
				else
				{
					alphaFactor = inv255 * material.DaytimeNighttimeBlend + 1.0f - Lighting.OptionLightingResultingAmount;
					if (alphaFactor > 1.0f)
					{
						alphaFactor = 1.0f;
					}
				}

				Shader.SetOpacity(alphaFactor);

				// render polygon
				VAO.Bind();
				VAO.Draw(Shader.VertexLayout, DrawMode, Face.IboStartIndex, Face.Vertices.Length);
				VAO.UnBind();

				GL.BindTexture(TextureTarget.Texture2D, 0);
			}

			GL.Disable(EnableCap.Texture2D);

			// normals
			if (OptionNormals)
			{
				Shader.SetIsTexture(false);
				Shader.SetBrightness(1.0f);
				Shader.SetOpacity(1.0f);
				Shader.SetIsAlphaTest(false);

				NormalsVAO.Bind();
				NormalsVAO.Draw(Shader.VertexLayout, PrimitiveType.Lines, Face.NormalsIboStartIndex, Face.Vertices.Length * 2);
				NormalsVAO.UnBind();
			}
		}
	}
}