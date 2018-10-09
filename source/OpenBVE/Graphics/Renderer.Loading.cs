using System;
using System.Drawing;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Textures;
using OpenBveApi.Interface;
using OpenTK.Graphics.OpenGL;

namespace OpenBve {
	internal static partial class Renderer {
		
		/* --------------------------------------------------------------
		 * This file contains the drawing routines for the loading screen
		 * -------------------------------------------------------------- */

		// the components of the screen background colour
		private const float		bkgR				= 0.5f;
		private const float		bkgG				= 0.5f;
		private const float		bkgB				= 0.5f;
		private const float		bkgA				= 1.00f;
		// the openBVE yellow
		private static readonly Color128 ColourProgressBar = new Color128(1.00f, 0.69f, 0.00f, 1.00f);
		// the percentage to lower the logo centre from the screen top (currently set at the golden ratio)
		private const double	logoCentreYFactor	= 0.381966;
		private const int		progrBorder			= 1;
		private const int		progrMargin			= 24;
		private const int		numOfLoadingBkgs	= 7;

		private static bool				customLoadScreen	= false;
		private static Texture	TextureLoadingBkg	= null;
		private static Texture	TextureLogo			= null;
		private static string[]			LogoFileName		= {"logo_256.png", "logo_512.png", "logo_1024.png"};

		//
		// INIT LOADING RESOURCES
		//
		/// <summary>Initializes the textures used for the loading screen</summary>
		internal static void InitLoading()
		{
			customLoadScreen	= false;
			string Path = Program.FileSystem.GetDataFolder("In-game");
			if (TextureLoadingBkg == null)
			{
				int bkgNo = Program.RandomNumberGenerator.Next (numOfLoadingBkgs);
				string backgroundFile = OpenBveApi.Path.CombineFile(Path, "loadingbkg_" + bkgNo + ".png");
				if (System.IO.File.Exists(backgroundFile))
				{
					Textures.RegisterTexture(backgroundFile, out TextureLoadingBkg);
				}
			}
			if (Renderer.TextureLogo == null)
			{
				// choose logo size according to screen width
				string	fName;
				if (Screen.Width > 2048)		fName	= LogoFileName[2];
				else if (Screen.Width > 1024)	fName	= LogoFileName[1];
				else							fName	= LogoFileName[0];
				string logoFile = OpenBveApi.Path.CombineFile(Path, fName);
				if (System.IO.File.Exists(logoFile))
				{
					Textures.RegisterTexture(logoFile, out TextureLogo);
				}
			}
		}

		//
		// SET CUSTOM LOADING SCREEN BACKGROUND
		//
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
			if (!BlendEnabled) {
				GL.Enable(EnableCap.Blend);
				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
				BlendEnabled = true;
			}
			if (LightingEnabled) {
				GL.Disable(EnableCap.Lighting);
				LightingEnabled = false;
			}
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			GL.PushMatrix();
			// fill the screen with background colour
			GL.Color4(bkgR, bkgG, bkgB, bkgA);
			Renderer.RenderOverlaySolid(0.0, 0.0, (double)Screen.Width, (double)Screen.Height);
			GL.Color4(1.0f, 1.0f, 1.0f, 1.0f);

			// BACKGROUND IMAGE
			int		fontHeight	= (int)Fonts.SmallFont.FontSize;
			int		logoBottom;
//			int		versionTop;
			int		halfWidth	= Screen.Width/2;
			if (TextureLoadingBkg != null)
			{
				int bkgHeight, bkgWidth;
				// stretch the background image to fit at least one screen dimension
				double ratio = (double) TextureLoadingBkg.Width/(double) TextureLoadingBkg.Height;
				if (Screen.Width/ratio > Screen.Height) // if screen ratio is shorter than bkg...
				{
					bkgHeight = Screen.Height; // set height to screen height
					bkgWidth = (int) (Screen.Height*ratio); // and scale width proprtionally
				}
				else // if screen ratio is wider than bkg...
				{
					bkgWidth = Screen.Width; // set width to screen width
					bkgHeight = (int) (Screen.Width/ratio); // and scale height accordingly
				}
				// draw the background image down from the top screen edge
				DrawRectangle(TextureLoadingBkg, new Point((Screen.Width - bkgWidth)/2, 0), new Size(bkgWidth, bkgHeight),Color128.White);
			}
			// if the route has no custom loading image, add the openBVE logo
			// (the route custom image is loaded in OldParsers/CsvRwRouteParser.cs)
			if (!customLoadScreen)
			{
				if (TextureLogo != null)
				{
					// place the centre of the logo at from the screen top
					int logoTop = (int) (Screen.Height*logoCentreYFactor - TextureLogo.Height/2.0);
					logoBottom = logoTop + TextureLogo.Height;
					DrawRectangle(TextureLogo, new Point((Screen.Width - TextureLogo.Width)/2, logoTop),new Size(TextureLogo.Width, TextureLogo.Height), Color128.White);
				}
			}
			else
			{
					// if custom route image, no logo and leave a conventional black area below the potential logo
			}
				logoBottom	= Screen.Height / 2;
			// take the height remaining below the logo and divide in 3 horiz. parts
			int	blankHeight	= (Screen.Height - logoBottom) / 3;

			// VERSION NUMBER
			// place the version above the first division
			int	versionTop	= logoBottom + blankHeight - fontHeight;
			DrawString(Fonts.SmallFont, "Version " + typeof(Renderer).Assembly.GetName().Version,
				new Point(halfWidth, versionTop), TextAlignment.TopMiddle, Color128.White);
			// for the moment, do not show any URL; would go right below the first division
//			DrawString(Fonts.SmallFont, "https://sites.google.com/site/openbvesim/home",
//				new Point(halfWidth, versionTop + fontHeight+2),
//				TextAlignment.TopMiddle, Color128.White);

			// PROGRESS MESSAGE AND BAR
			// place progress bar right below the second division
			int		progressTop		= Screen.Height - blankHeight;
			int		progressWidth	= Screen.Width - progrMargin * 2;
			double	routeProgress	= Math.Max(0.0, Math.Min(1.0, Loading.RouteProgress));
			double	trainProgress	= Math.Max(0.0, Math.Min(1.0, Loading.TrainProgress));
			// draw progress message right above the second division
			string	text			= Translations.GetInterfaceString(
				routeProgress < 1.0 ? "loading_loading_route" :
				(trainProgress < 1.0 ? "loading_loading_train" : "message_loading") );
			DrawString(Fonts.SmallFont, text, new Point(halfWidth, progressTop - fontHeight - 6),
				TextAlignment.TopMiddle, Color128.White);
			// sum of route progress and train progress arrives up to 2.0:
			// => times 50.0 to convert to %
			double	percent	= 50.0 * (routeProgress + trainProgress);
			string	percStr	= percent.ToString("0") + "%";
			// progress frame
			DrawRectangle(null, new Point(progrMargin-progrBorder, progressTop-progrBorder),
				new Size(progressWidth+progrBorder*2, fontHeight+6), Color128.White);
			// progress bar
			DrawRectangle(null, new Point(progrMargin, progressTop),
				new Size(progressWidth * (int)percent / 100, fontHeight+4), ColourProgressBar);
			// progress percent
			DrawString(Fonts.SmallFont, percStr, new Point(halfWidth, progressTop),
				TextAlignment.TopMiddle, Color128.Black);
			GL.PopMatrix();
		}

	}
}
