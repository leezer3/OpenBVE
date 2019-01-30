using System;
using System.Drawing;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Textures;
using OpenTK.Graphics.OpenGL;

namespace OpenBve
{
	internal static partial class Renderer
	{

		/* --------------------------------------------------------------
		 * This file contains the drawing routines for the loading screen
		 * -------------------------------------------------------------- */

		// the components of the screen background colour
		private const float bkgR = 0.5f;
		private const float bkgG = 0.5f;
		private const float bkgB = 0.5f;
		private const float bkgA = 1.00f;
		// the openBVE yellow
		private static Color128 ColourProgressBar = new Color128(1.00f, 0.69f, 0.00f, 1.00f);
		// the percentage to lower the logo centre from the screen top (currently set at the golden ratio)
		private const double logoCentreYFactor = 0.381966;
		private const int progrBorder = 1;
		private const int progrMargin = 24;
		private const int numOfLoadingBkgs = 7;

		private static bool customLoadScreen = false;
		internal static Texture TextureLoadingBkg = null;
		private static Texture TextureLogo = null;
		private static string[] LogoFileName = { "logo_256.png", "logo_512.png", "logo_1024.png" };

		//
		// INIT LOADING RESOURCES
		//
		/// <summary>Initializes the textures used for the loading screen</summary>
		internal static void InitLoading()
		{
			customLoadScreen = false;
			string Path = Program.FileSystem.GetDataFolder("In-game");
			int bkgNo = Game.Generator.Next(numOfLoadingBkgs);
			if(TextureLoadingBkg == null)
			{
				string file = OpenBveApi.Path.CombineFile(Path, "loadingbkg_" + bkgNo + ".png");
				if (System.IO.File.Exists(file))
				{
					Textures.RegisterTexture(file, out TextureLoadingBkg);
				}
			}
			
			// choose logo size according to screen width
			string fName;
			if (Renderer.ScreenWidth > 2048) fName = LogoFileName[2];
			else if (Renderer.ScreenWidth > 1024) fName = LogoFileName[1];
			else fName = LogoFileName[0];
			fName = OpenBveApi.Path.CombineFile(Path, fName);
			if (System.IO.File.Exists(fName))
			{
				if (System.IO.File.Exists(fName))
				{
					Textures.RegisterTexture(fName, out TextureLogo);
				}
			}

		}

		//
		// SET CUSTOM LOADING SCREEN BACKGROUND
		//
		/// <summary>Sets the loading screen background to a custom image</summary>
		/// <summary>Sets the loading screen background to a custom image</summary>
		internal static void SetLoadingBkg(string fileName)
		{
			Textures.RegisterTexture(fileName, out TextureLoadingBkg);
			customLoadScreen = true;
		}

		//
		// DRAW LOADING SCREEN
		//
		/// <summary>Draws on OpenGL canvas the route/train loading screen</summary>
		internal static void DrawLoadingScreen()
		{
			
			// begin HACK //
			if (!BlendEnabled)
			{
				GL.Enable(EnableCap.Blend);
				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
				BlendEnabled = true;
			}
			if (LightingEnabled)
			{
				GL.Disable(EnableCap.Lighting);
				LightingEnabled = false;
			}
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			GL.PushMatrix();

			// fill the screen with background colour
			GL.Color4(bkgR, bkgG, bkgB, bkgA);
			DrawRectangle(null, new System.Drawing.Point((int)0, (int)0), new System.Drawing.Size((int)Renderer.ScreenWidth, (int)Renderer.ScreenHeight), null);
			GL.Color4(1.0f, 1.0f, 1.0f, 1.0f);
			// BACKGROUND IMAGE
			int bkgHeight, bkgWidth;
			int fontHeight = (int) Fonts.SmallFont.FontSize;
			int logoBottom;
			//			int		versionTop;
			int halfWidth = Renderer.ScreenWidth / 2;
			bool bkgLoaded = TextureLoadingBkg != null;
			if (TextureLoadingBkg != null)
			{
				// stretch the background image to fit at least one screen dimension
				double ratio = (double) TextureLoadingBkg.Width/ (double) TextureLoadingBkg.Height;
				if ((double) Renderer.ScreenWidth/ratio > Renderer.ScreenHeight) // if screen ratio is shorter than bkg...
				{
					bkgHeight = Renderer.ScreenHeight; // set height to screen height
					bkgWidth = (int) (Renderer.ScreenWidth*ratio); // and scale width proprtionally
				}
				else // if screen ratio is wider than bkg...
				{
					bkgWidth = Renderer.ScreenWidth; // set width to screen width
					bkgHeight = (int) (Renderer.ScreenHeight/ratio); // and scale height accordingly
				}
				// draw the background image down from the top screen edge
				DrawRectangle(TextureLoadingBkg, new Point((Renderer.ScreenWidth - bkgWidth)/2, 0), new Size(bkgWidth, bkgHeight), Color128.White);
			}
			// if the route has no custom loading image, add the openBVE logo
			// (the route custom image is loaded in OldParsers/CsvRwRouteParser.cs)
			if (!customLoadScreen && Interface.CurrentOptions.LoadingLogo && TextureLogo != null)
			{
				// place the centre of the logo at from the screen top
				int logoTop = (int)(Renderer.ScreenHeight * logoCentreYFactor - TextureLogo.Height / 2.0);
				DrawRectangle(TextureLogo,new Point((Renderer.ScreenWidth - TextureLogo.Width) / 2, logoTop),new Size(TextureLogo.Width, TextureLogo.Height), Color128.White);
			}
			else
			{
				// if custom route image, no logo and leave a conventional black area below the potential logo
			}
			logoBottom = Renderer.ScreenHeight / 2;
			if (!bkgLoaded)				// if the background texture not yet loaded, do nothing else
				return;
			// take the height remaining below the logo and divide in 3 horiz. parts
			int blankHeight = (Renderer.ScreenHeight - logoBottom) / 3;

			// VERSION NUMBER
			// place the version above the first division
			// int versionTop = logoBottom + blankHeight - fontHeight;
			//RenderString(Fonts.SmallFontSize, "Version " + typeof(Renderer).Assembly.GetName().Version,new Point(halfWidth, versionTop), TextAlignment.TopMiddle, Color128.White);
			// for the moment, do not show any URL; would go right below the first division
			//			DrawString(Fonts.SmallFont, "https://sites.google.com/site/openbvesim/home",
			//				new Point(halfWidth, versionTop + fontHeight+2),
			//				TextAlignment.TopMiddle, Color128.White);

			// place progress bar right below the second division
			int progressTop = Renderer.ScreenHeight - blankHeight;
			int progressWidth = Renderer.ScreenWidth - progrMargin * 2;
			double routeProgress = Math.Max(0.0, Math.Min(1.0, Loading.RouteProgress));
			if (Interface.CurrentOptions.LoadingProgressBar)
			{
				// PROGRESS MESSAGE AND BAR
				double percent = 100.0 * routeProgress;
				string percStr = percent.ToString("0") + "%";
				// progress frame
				DrawRectangle(null, new Point(progrMargin - progrBorder, progressTop - progrBorder), new Size(progressWidth + progrBorder * 2, fontHeight + 6), Color128.White);
				// progress bar
				DrawRectangle(null, new Point(progrMargin, progressTop), new Size(progressWidth * (int)percent / 100, fontHeight + 4), ColourProgressBar);

				DrawString(Fonts.SmallFont, percStr, new Point(halfWidth, progressTop + 2), TextAlignment.TopMiddle, Color128.Black);
			}

			string text = "Loading route, please wait.....";
			DrawString(Fonts.SmallFont, text, new Point(halfWidth, progressTop - fontHeight - 6), TextAlignment.TopMiddle, Color128.White);
			GL.PopMatrix();
		}

	}
}
