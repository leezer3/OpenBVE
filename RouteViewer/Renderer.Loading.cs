﻿using System;
using System.Drawing;
using System.Reflection;			// for AssemblyVersion
using OpenBveApi.Colors;
using OpenTK;
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
		private static int TextureLoadingBkg = -1;
		private static int TextureLogo = -1;
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
			TextureLoadingBkg = TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Path, "loadingbkg_" + bkgNo + ".png"), TextureManager.TextureWrapMode.ClampToEdge, TextureManager.TextureWrapMode.ClampToEdge, false);
			TextureManager.UseTexture(TextureLoadingBkg, TextureManager.UseMode.LoadImmediately);
			// choose logo size according to screen width
			string fName;
			if (Renderer.ScreenWidth > 2048) fName = LogoFileName[2];
			else if (Renderer.ScreenWidth > 1024) fName = LogoFileName[1];
			else fName = LogoFileName[0];
			TextureLogo = TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Path, fName), TextureManager.TextureWrapMode.ClampToEdge, TextureManager.TextureWrapMode.ClampToEdge, false);
			TextureManager.UseTexture(TextureLogo, TextureManager.UseMode.LoadImmediately);
			
		}

		//
		// SET CUSTOM LOADING SCREEN BACKGROUND
		//
		/// <summary>Sets the loading screen background to a custom image</summary>
		internal static void SetLoadingBkg(string fileName)
		{
			TextureLoadingBkg = TextureManager.RegisterTexture(fileName, TextureManager.TextureWrapMode.ClampToEdge, TextureManager.TextureWrapMode.ClampToEdge, false);
			TextureManager.UseTexture(TextureLoadingBkg, TextureManager.UseMode.LoadImmediately);
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
				GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
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
			DrawRectangle(-1, new System.Drawing.Point((int)0, (int)0), new System.Drawing.Size((int)Renderer.ScreenWidth, (int)Renderer.ScreenHeight), null);
			GL.Color4(1.0f, 1.0f, 1.0f, 1.0f);
			// BACKGROUND IMAGE
			int bkgHeight, bkgWidth;
			int fontHeight = (int)Fonts.SmallFontSize;
			int logoBottom;
			//			int		versionTop;
			int halfWidth = Renderer.ScreenWidth / 2;
			bool bkgLoaded = TextureLoadingBkg != -1;
			// stretch the background image to fit at least one screen dimension
			double ratio = bkgLoaded ? (double)TextureManager.Textures[TextureLoadingBkg].Width / (double)TextureManager.Textures[TextureLoadingBkg].Height : 1.0;
			if (Renderer.ScreenWidth / ratio > Renderer.ScreenHeight)		// if screen ratio is shorter than bkg...
			{
				bkgHeight = Renderer.ScreenHeight;				// set height to screen height
				bkgWidth = (int)(Renderer.ScreenHeight * ratio);	// and scale width proprtionally
			}
			else											// if screen ratio is wider than bkg...
			{
				bkgWidth = Renderer.ScreenWidth;					// set width to screen width
				bkgHeight = (int)(Renderer.ScreenWidth / ratio);	// and scale height accordingly
			}
			// draw the background image down from the top screen edge
			DrawRectangle(TextureLoadingBkg, new Point((Renderer.ScreenWidth - bkgWidth) / 2, 0),new Size(bkgWidth, bkgHeight), Color128.White);
			// if the route has no custom loading image, add the openBVE logo
			// (the route custom image is loaded in OldParsers/CsvRwRouteParser.cs)
			if (!customLoadScreen)
			{
				// place the centre of the logo at from the screen top
				int logoTop = (int)(Renderer.ScreenHeight * logoCentreYFactor - TextureManager.Textures[TextureLogo].Height / 2.0);
				DrawRectangle(TextureLogo,new Point((Renderer.ScreenWidth - TextureManager.Textures[TextureLogo].Width) / 2, logoTop),new Size(TextureManager.Textures[TextureLogo].Width, TextureManager.Textures[TextureLogo].Height), Color128.White);
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
			int versionTop = logoBottom + blankHeight - fontHeight;
			//RenderString(Fonts.SmallFontSize, "Version " + typeof(Renderer).Assembly.GetName().Version,new Point(halfWidth, versionTop), TextAlignment.TopMiddle, Color128.White);
			// for the moment, do not show any URL; would go right below the first division
			//			DrawString(Fonts.SmallFont, "https://sites.google.com/site/openbvesim/home",
			//				new Point(halfWidth, versionTop + fontHeight+2),
			//				TextAlignment.TopMiddle, Color128.White);

			// PROGRESS MESSAGE AND BAR
			// place progress bar right below the second division
			int progressTop = Renderer.ScreenHeight - blankHeight;
			int progressWidth = Renderer.ScreenWidth - progrMargin * 2;
			double routeProgress = Math.Max(0.0, Math.Min(1.0, Loading.RouteProgress));
			double trainProgress = Math.Max(0.0, Math.Min(1.0, Loading.TrainProgress));
			// draw progress message right above the second division

			double percent = 50.0 * (routeProgress + trainProgress);
			string percStr = percent.ToString("0") + "%";
			// progress frame
			DrawRectangle(-1, new Point(progrMargin - progrBorder, progressTop - progrBorder), new Size(progressWidth + progrBorder * 2, fontHeight + 6), Color128.White);
			// progress bar
			DrawRectangle(-1, new Point(progrMargin, progressTop), new Size(progressWidth * (int)percent / 100, fontHeight + 4), ColourProgressBar);
			

			string text = "Loading route, please wait.....";

			RenderString(halfWidth - 60, progressTop - fontHeight - 6, Fonts.FontType.Small, text, -1, 1.0f, 1.0f, 1.0f, false);
			RenderString(halfWidth - 5, progressTop + 2, Fonts.FontType.Small, percStr, -1,0.0f, 0.0f,0.0f, false);

			GL.PopMatrix();
		}

	}
}