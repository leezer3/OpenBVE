using System;
using LibRender2.Texts;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Textures;
using OpenTK.Graphics.OpenGL;

namespace LibRender2.Loadings
{
	public class Loading
	{
		private readonly BaseRenderer renderer;

		// the components of the screen background color
		private readonly Color128 bkg = new Color128(0.5f, 0.5f, 0.5f, 1.0f);

		// the openBVE yellow
		private static readonly Color128 ColourProgressBar = new Color128(1.00f, 0.69f, 0.00f, 1.00f);

		// the percentage to lower the logo centre from the screen top (currently set at the golden ratio)
		private const double logoCentreYFactor = 0.381966;
		private const int progrBorder = 1;
		private const int progrMargin = 24;
		private const int numOfLoadingBkgs = 7;

		private bool customLoadScreen;
		private bool showLogo;
		private bool showProgress;
		private Texture TextureLoadingBkg;
		private string ProgramVersion = "1.0";

		internal Loading(BaseRenderer renderer)
		{
			this.renderer = renderer;
		}

		/// <summary>Initializes the textures used for the loading screen</summary>
		public void InitLoading(string Path, string Version, bool ShowLogo = true, bool ShowProgress = true)
		{
			showLogo = ShowLogo;
			showProgress = ShowProgress;
			ProgramVersion = Version;
			customLoadScreen = false;

			if (TextureLoadingBkg == null)
			{
				int bkgNo = new Random().Next(numOfLoadingBkgs);
				string backgroundFile = string.Empty;
				try
				{
					backgroundFile = OpenBveApi.Path.CombineFile(Path, "loadingbkg_" + bkgNo + ".png");
				}
				catch
				{
					//ignored
				}

				if (System.IO.File.Exists(backgroundFile))
				{
					renderer.TextureManager.RegisterTexture(backgroundFile, out TextureLoadingBkg);
				}
			}
		}

		/// <summary>Sets the loading screen background to a custom image</summary>
		public void SetLoadingBkg(string fileName)
		{
			renderer.TextureManager.RegisterTexture(fileName, out TextureLoadingBkg);
			customLoadScreen = true;
		}

		public void SetLoadingBkg(Texture texture)
		{
			if (customLoadScreen || texture == null)
			{
				return;
			}
			TextureLoadingBkg = texture;
			customLoadScreen = true;
		}

		/// <summary>Draws on OpenGL canvas the route/train loading screen</summary>
		public void DrawLoadingScreen(OpenGlFont Font, double RouteProgress, double TrainProgress = double.MaxValue)
		{
			renderer.SetBlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha); //FIXME: Remove when text switches between two renderer types
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			renderer.Rectangle.Draw(null, Vector2.Null, new Vector2(renderer.Screen.Width, renderer.Screen.Height), bkg);

			// BACKGROUND IMAGE
			int fontHeight = (int)Font.FontSize;
			int logoBottom;
			//int versionTop;
			int halfWidth = renderer.Screen.Width / 2;

			if (TextureLoadingBkg != null && renderer.currentHost.LoadTexture(TextureLoadingBkg, OpenGlTextureWrapMode.ClampClamp))
			{
				int bkgHeight, bkgWidth;

				// stretch the background image to fit at least one screen dimension
				double ratio = TextureLoadingBkg.Width / (double)TextureLoadingBkg.Height;

				if (renderer.Screen.Width / ratio > renderer.Screen.Height) // if screen ratio is shorter than bkg...
				{
					bkgHeight = renderer.Screen.Height; // set height to screen height
					bkgWidth = (int)(renderer.Screen.Height * ratio); // and scale width proprtionally
				}
				else // if screen ratio is wider than bkg...
				{
					bkgWidth = renderer.Screen.Width; // set width to screen width
					bkgHeight = (int)(renderer.Screen.Width / ratio); // and scale height accordingly
				}

				// draw the background image down from the top screen edge
				renderer.Rectangle.Draw(TextureLoadingBkg, new Vector2((renderer.Screen.Width - bkgWidth) / 2.0, 0), new Vector2(bkgWidth, bkgHeight), Color128.White);
			}

			// if the route has no custom loading image, add the openBVE logo
			// (the route custom image is loaded in OldParsers/CsvRwRouteParser.cs)
			if (!customLoadScreen)
			{
				if (showLogo && renderer.ProgramLogo != null)
				{
					// place the centre of the logo at from the screen top
					int logoTop = (int)(renderer.Screen.Height * logoCentreYFactor - renderer.ProgramLogo.Height / 2.0);
					renderer.UnsetBlendFunc();
					renderer.SetAlphaFunc(AlphaFunction.Equal, 1.0f);
					GL.DepthMask(true);
					renderer.Rectangle.Draw(renderer.ProgramLogo, new Vector2((renderer.Screen.Width - renderer.ProgramLogo.Width) / 2.0, logoTop), new Vector2(renderer.ProgramLogo.Width, renderer.ProgramLogo.Height), Color128.White);
					renderer.SetBlendFunc();
					renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
					GL.DepthMask(false);
					renderer.Rectangle.Draw(renderer.ProgramLogo, new Vector2((renderer.Screen.Width - renderer.ProgramLogo.Width) / 2.0, logoTop), new Vector2(renderer.ProgramLogo.Width, renderer.ProgramLogo.Height), Color128.White);
					renderer.SetAlphaFunc(AlphaFunction.Equal, 1.0f);
				}
			}
			// ReSharper disable once RedundantIfElseBlock
			else
			{
				// if custom route image, no logo and leave a conventional black area below the potential logo
			}

			if (showProgress)
			{
				logoBottom = renderer.Screen.Height / 2;

				// take the height remaining below the logo and divide in 3 horiz. parts
				int blankHeight = (renderer.Screen.Height - logoBottom) / 3;

				// VERSION NUMBER
				// place the version above the first division
				int versionTop = logoBottom + blankHeight - fontHeight;
				renderer.OpenGlString.Draw(Font, "Version " + ProgramVersion, new Vector2(halfWidth, versionTop), TextAlignment.TopMiddle, Color128.White);
				// for the moment, do not show any URL; would go right below the first division
				//			DrawString(Fonts.SmallFont, "https://openbve-project.net",
				//				new Point(halfWidth, versionTop + fontHeight+2),
				//				TextAlignment.TopMiddle, Color128.White);
				// PROGRESS MESSAGE AND BAR
				// place progress bar right below the second division
				int progressTop = renderer.Screen.Height - blankHeight;
				int progressWidth = renderer.Screen.Width - progrMargin * 2;
				double routeProgress = Math.Max(0.0, Math.Min(1.0, RouteProgress));
				double trainProgress = Math.Max(0.0, Math.Min(1.0, TrainProgress));

				// draw progress message right above the second division
				string text = Translations.GetInterfaceString(routeProgress < 1.0 ? "loading_loading_route" : trainProgress < 1.0 ? "loading_loading_train" : "message_loading");
				renderer.OpenGlString.Draw(Font, text, new Vector2(halfWidth, progressTop - fontHeight - 6), TextAlignment.TopMiddle, Color128.White);

				// sum of route progress and train progress arrives up to 2.0:
				// => times 50.0 to convert to %
				double percent;
				if (TrainProgress != double.MaxValue)
				{
					percent = 50.0 * (routeProgress + trainProgress);
				}
				else
				{
					percent = 100.0 * routeProgress;
				}
				string percStr = percent.ToString("0") + "%";

				// progress frame
				renderer.Rectangle.Draw(null, new Vector2(progrMargin - progrBorder, progressTop - progrBorder), new Vector2(progressWidth + progrBorder * 2, fontHeight + 6), Color128.White);

				// progress bar
				renderer.Rectangle.Draw(null, new Vector2(progrMargin, progressTop), new Vector2(progressWidth * (int)percent / 100.0, fontHeight + 4), ColourProgressBar);

				// progress percent
				renderer.OpenGlString.Draw(Font, percStr, new Vector2(halfWidth, progressTop), TextAlignment.TopMiddle, Color128.Black);
			}
		}
	}
}
