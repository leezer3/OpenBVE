using System;
using System.Drawing;
using OpenBveApi.Colors;
using OpenTK.Graphics.OpenGL;

namespace OpenBve {
	internal static partial class Renderer {
		
		/* --------------------------------------------------------------
		 * This file contains the drawing routines for the loading screen
		 * -------------------------------------------------------------- */

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
			int size = Math.Min(Screen.Width, Screen.Height);
			//DrawRectangle(null, new Point(0, 0), Screen.Size, Color128.Black);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			GL.PushMatrix();
				DrawRectangle(TextureLogo, new Point((Screen.Width - size) / 2, (Screen.Height - size) / 2), new Size(size, size), Color128.White);
			DrawRectangle(null, 
				new Point((Screen.Width - size) / 2, Screen.Height - (int)Fonts.NormalFont.FontSize - 10),
				new Size(Screen.Width, (int)Fonts.NormalFont.FontSize + 10), 
				new Color128(0.0f, 0.0f, 0.0f, 0.5f));
			double routeProgress = Math.Max(0.0, Math.Min(1.0, Loading.RouteProgress));
			double trainProgress = Math.Max(0.0, Math.Min(1.0, Loading.TrainProgress));
			string text;
			if (routeProgress < 1.0) {
				//text = "Loading route... " + (100.0 * routeProgress).ToString("0") + "%";
				string percent = (100.0 * routeProgress).ToString("0.0");
				text = String.Format("{0} {1}%",Interface.GetInterfaceString("loading_loading_route"),percent);
			} else if (trainProgress < 1.0) {
				//text = "Loading train... " + (100.0 * trainProgress).ToString("0") + "%";
				string percent = (100.0 * trainProgress).ToString("0.0");
				text = String.Format("{0} {1}%",Interface.GetInterfaceString("loading_loading_train"),percent);
			} else {
				//text = "Loading textures and sounds...";
				text = Interface.GetInterfaceString("message_loading");
			}
			DrawString(Fonts.SmallFont, text, new Point((Screen.Width - size) / 2 + 5, Screen.Height - (int)(Fonts.NormalFont.FontSize / 2) - 5), TextAlignment.CenterLeft, Color128.White);
			GL.PopMatrix();
			// end HACK //
             
		}
		
	}
}