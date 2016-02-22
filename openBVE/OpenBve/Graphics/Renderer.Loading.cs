using System;
using System.Drawing;
using System.Reflection;			// for AssemblyVersion
using OpenBveApi.Colors;
using OpenTK.Graphics.OpenGL;

namespace OpenBve {
	internal static partial class Renderer {
		
		/* --------------------------------------------------------------
		 * This file contains the drawing routines for the loading screen
		 * -------------------------------------------------------------- */

		private static Color128	ColourBackground	= new Color128 (0.39f, 0.39f, 0.39f, 1.0f);
		private const int		progrBorder			= 1;
		private const int		progrMargin			= 24;

		internal static void DrawLoadingScreen() {
            
			// begin HACK //
			if (!BlendEnabled) {
				GL.Enable(EnableCap.Blend);
				GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
				BlendEnabled = true;
			}
			if (LightingEnabled) {
				GL.Disable(EnableCap.Lighting);
				LightingEnabled = false;
			}
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			GL.PushMatrix();
			// place the centre of the logo at the golden ratio of the screen height
			int logoTop		= (int)(Screen.Height * 0.381966 - TextureLogo.Height / 2);
			int	logoBottom	= logoTop + TextureLogo.Height;
			// draw a dark gray background and the (unscaled) logo bitmap centred in the screen
			DrawRectangle(null, new Point(0, 0), new Size(Screen.Width, Screen.Height), ColourBackground);
			DrawRectangle(TextureLogo,
				new Point((Screen.Width - TextureLogo.Width) / 2, logoTop),
				new Size(TextureLogo.Width, TextureLogo.Height), Color128.White);
			if (logoTop == logoBottom)			// if logo texture not yet loaded, do nothing else
				return;
			// take the height remaining below the logo and divide in 3 horiz. parts
			int	fontHeight	= (int)Fonts.SmallFont.FontSize;
			int	blankHeight	= (Screen.Height - logoBottom) / 3;
			int	versionTop	= logoBottom + blankHeight - fontHeight;
			int	halfWidth	= Screen.Width/2;
			// draw version number and web site URL
			DrawString(Fonts.SmallFont, "Version " + typeof(Renderer).Assembly.GetName().Version,
				new Point(halfWidth, versionTop), TextAlignment.TopMiddle, Color128.White);
			// for the moment, do not show any URL
//			DrawString(Fonts.SmallFont, "https://sites.google.com/site/openbvesim/home",
//				new Point(halfWidth, versionTop + fontHeight+2),
//				TextAlignment.TopMiddle, Color128.White);
			// draw progress message and bar
			int		progressTop		= Screen.Height - blankHeight;
			int		progressWidth	= Screen.Width - progrMargin * 2;
			double	routeProgress	= Math.Max(0.0, Math.Min(1.0, Loading.RouteProgress));
			double	trainProgress	= Math.Max(0.0, Math.Min(1.0, Loading.TrainProgress));
			string	text			= Interface.GetInterfaceString(
				routeProgress < 1.0 ? "loading_loading_route" :
				(trainProgress < 1.0 ? "loading_loading_train" : "message_loading") );
			DrawString(Fonts.SmallFont, text, new Point(halfWidth, progressTop - fontHeight - 6),
				TextAlignment.TopMiddle, Color128.White);
			// sum of route progress and train progress arrives up to 2.0:
			// => 50.0 * to convert to %
			double	percent	= 50.0 * (routeProgress + trainProgress);
			string	percStr	= percent.ToString("0") + "%";
			// progress frame
			DrawRectangle(null, new Point(progrMargin-progrBorder, progressTop-progrBorder),
				new Size(progressWidth+progrBorder*2, fontHeight+6), Color128.White);
			// progress bar
			DrawRectangle(null, new Point(progrMargin, progressTop),
				new Size(progressWidth * (int)percent / 100, fontHeight+4), Color128.Cyan);
			// progress percent
			DrawString(Fonts.SmallFont, percStr, new Point(halfWidth, progressTop),
				TextAlignment.TopMiddle, Color128.Black);
			GL.PopMatrix();             
		}
		
	}
}