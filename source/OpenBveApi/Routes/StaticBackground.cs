using System.Collections.Generic;
using System.Linq;
using OpenBveApi.Graphics;
using OpenBveApi.Textures;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenBveApi.Routes
{
	/// <summary>Represents a static background, using the default viewing frustrum</summary>
	public class StaticBackground : BackgroundHandle
	{
		/// <summary>The background texture</summary>
		public Texture Texture;
		/// <summary>The number of times the texture is repeated around the viewing frustrum</summary>
		public double Repetition;
		/// <summary>Whether the texture's aspect ratio should be maintained</summary>
		public bool KeepAspectRatio;
		/// <summary>The time taken to transition to this background</summary>
		public double TransitionTime;
		/// <summary> The time at which this background should be displayed (Expressed as the number of seconds since midnight)</summary>
		public double Time;
		/// <summary>The OpenGL/OpenTK VAO for the background</summary>
		public VertexArrayObject VAO;

		/// <summary>Creates a new static background, using the default 0.8s fade-in time</summary>
		/// <param name="Texture">The texture to apply</param>
		/// <param name="Repetition">The number of times the texture should be repeated around the viewing frustrum</param>
		/// <param name="KeepAspectRatio">Whether the aspect ratio of the texture should be preseved</param>
		public StaticBackground(Texture Texture, double Repetition, bool KeepAspectRatio) : this(Texture, Repetition, KeepAspectRatio, 0.8, BackgroundTransitionMode.FadeIn)
		{
		}

		/// <summary>Creates a new static background</summary>
		/// <param name="Texture">The texture to apply</param>
		/// <param name="Repetition">The number of times the texture should be repeated around the viewing frustrum</param>
		/// <param name="KeepAspectRatio">Whether the aspect ratio of the texture should be preseved</param>
		/// <param name="transitionTime">The time taken in seconds for the fade-in transition to occur</param>
		/// <param name="Mode">The transition mode</param>
		public StaticBackground(Texture Texture, double Repetition, bool KeepAspectRatio, double transitionTime, BackgroundTransitionMode Mode) : this(Texture, Repetition, KeepAspectRatio, 0.8, BackgroundTransitionMode.FadeIn, -1.0)
		{
		}

		/// <summary>Creates a new static background</summary>
		/// <param name="Texture">The texture to apply</param>
		/// <param name="Repetition">The number of times the texture should be repeated around the viewing frustrum</param>
		/// <param name="KeepAspectRatio">Whether the aspect ratio of the texture should be preseved</param>
		/// <param name="transitionTime">The time taken in seconds for the fade-in transition to occur</param>
		/// <param name="Mode">The transition mode</param>
		/// <param name="Time">The time at which this background is to be displayed, expressed as the number of seconds since midnight</param>
		public StaticBackground(Texture Texture, double Repetition, bool KeepAspectRatio, double transitionTime, BackgroundTransitionMode Mode, double Time)
		{
			this.Texture = Texture;
			this.Repetition = Repetition;
			this.KeepAspectRatio = KeepAspectRatio;
			TransitionTime = transitionTime;
			this.Mode = Mode;
			this.Time = Time;
		}

		public void CreateVAO()
		{
			float y0, y1;

			if (KeepAspectRatio)
			{
				int tw = Texture.Width;
				int th = Texture.Height;
				double hh = System.Math.PI * BackgroundImageDistance * th / (tw * Repetition);
				y0 = (float)(-0.5 * hh);
				y1 = (float)(1.5 * hh);
			}
			else
			{
				y0 = (float)(-0.125 * BackgroundImageDistance);
				y1 = (float)(0.375 * BackgroundImageDistance);
			}

			const int n = 32;
			Vector3[] bottom = new Vector3[n];
			Vector3[] top = new Vector3[n];
			double angleValue = 2.61799387799149 - 3.14159265358979 / n;
			const double angleIncrement = 6.28318530717958 / n;

			/*
			 * To ensure that the whole background cylinder is rendered inside the viewing frustum,
			 * the background is rendered before the scene with z-buffer writes disabled. Then,
			 * the actual distance from the camera is irrelevant as long as it is inside the frustum.
			 * */
			for (int i = 0; i < n; i++)
			{
				float x = (float)(BackgroundImageDistance * System.Math.Cos(angleValue));
				float z = (float)(BackgroundImageDistance * System.Math.Sin(angleValue));
				bottom[i] = new Vector3(x, y0, -z);
				top[i] = new Vector3(x, y1, -z);
				angleValue += angleIncrement;
			}

			float textureStart = 0.5f * (float)Repetition / n;
			float textureIncrement = -(float)Repetition / n;
			float textureX = textureStart;

			List<LibRenderVertex> vertexData = new List<LibRenderVertex>();
			List<int> indexData = new List<int>();

			for (int i = 0; i < n; i++)
			{
				int j = (i + 1) % n;
				int indexOffset = vertexData.Count;

				// side wall
				vertexData.Add(new LibRenderVertex
				{
					Position = top[i],
					UV = new Vector2(textureX, 0.005f),
					Color = Color4.White
				});

				vertexData.Add(new LibRenderVertex
				{
					Position = bottom[i],
					UV = new Vector2(textureX, 0.995f),
					Color = Color4.White
				});

				vertexData.Add(new LibRenderVertex
				{
					Position = bottom[j],
					UV = new Vector2(textureX + textureIncrement, 0.995f),
					Color = Color4.White
				});

				vertexData.Add(new LibRenderVertex
				{
					Position = top[j],
					UV = new Vector2(textureX + textureIncrement, 0.005f),
					Color = Color4.White
				});

				indexData.AddRange(new[] { 0, 1, 2, 3 }.Select(x => x + indexOffset));

				// top cap
				vertexData.Add(new LibRenderVertex
				{
					Position = new Vector3(0.0f, top[i].Y, 0.0f),
					UV = new Vector2(textureX + 0.5f * textureIncrement, 0.1f),
					Color = Color4.White
				});

				indexData.AddRange(new[] { 0, 3, 4 }.Select(x => x + indexOffset));

				// bottom cap
				vertexData.Add(new LibRenderVertex
				{
					Position = new Vector3(0.0f, bottom[i].Y, 0.0f),
					UV = new Vector2(textureX + 0.5f * textureIncrement, 0.9f),
					Color = Color4.White
				});

				indexData.AddRange(new[] { 5, 2, 1 }.Select(x => x + indexOffset));

				// finish
				textureX += textureIncrement;
			}

			VAO?.UnBind();
			VAO?.Dispose();

			VAO = new VertexArrayObject();
			VAO.Bind();
			VAO.SetVBO(new VertexBufferObject(vertexData.ToArray(), BufferUsageHint.StaticDraw));
			VAO.SetIBO(new IndexBufferObject(indexData.ToArray(), BufferUsageHint.StaticDraw));
			VAO.UnBind();
		}

		public override void UpdateBackground(double SecondsSinceMidnight, double ElapsedTime, bool Target)
		{
			if (Target)
			{
				switch (Mode)
				{
					case BackgroundTransitionMode.None:
						CurrentAlpha = 1.0f;
						break;
					case BackgroundTransitionMode.FadeIn:
						CurrentAlpha = (float)(Countdown / TransitionTime);
						break;
					case BackgroundTransitionMode.FadeOut:
						CurrentAlpha = 1.0f - (float)(Countdown / TransitionTime);
						break;
				}
			}
			else
			{
				CurrentAlpha = 1.0f;
			}
		}
	}
}
