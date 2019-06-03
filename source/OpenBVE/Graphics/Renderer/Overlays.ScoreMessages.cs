using System;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Textures;
using OpenTK.Graphics.OpenGL;

namespace OpenBve
{
	internal static partial class Renderer
	{
		/// <summary>Renders the list of score messages</summary>
		/// <param name="Element">The HUD element these are to be rendererd onto</param>
		/// <param name="TimeElapsed">The time elapsed</param>
		private static void RenderScoreMessages(HUD.Element Element, double TimeElapsed)
		{
			// score messages
			int n = Game.ScoreMessages.Length;
			float totalwidth = 16.0f;
			float[] widths = new float[n];
			float[] heights = new float[n];
			for (int j = 0; j < n; j++)
			{
				System.Drawing.Size size = Element.Font.MeasureString(Game.ScoreMessages[j].Text);
				widths[j] = size.Width;
				heights[j] = size.Height;
				float a = widths[j] - j * Element.Value1;
				if (a > totalwidth) totalwidth = a;
			}
			Game.ScoreMessagesRendererSize.X += 16.0 * TimeElapsed * ((double)totalwidth - Game.ScoreMessagesRendererSize.X);
			totalwidth = (float)Game.ScoreMessagesRendererSize.X;
			double lcrh, lw, rw;
			CalculateViewingPlaneSize(Element, out lw, out rw, out lcrh);
			// start
			double w = Element.Alignment.X == 0 ? lw + rw + 128 : totalwidth + lw + rw;
			double h = Element.Value2 * n;
			double x = Element.Alignment.X < 0 ? 0.0 : Element.Alignment.X > 0 ? LibRender.Screen.Width - w : 0.5 * (LibRender.Screen.Width - w);
			double y = Element.Alignment.Y < 0 ? 0.0 : Element.Alignment.Y > 0 ? LibRender.Screen.Height - h : 0.5 * (LibRender.Screen.Height - h);
			x += Element.Position.X;
			y += Element.Position.Y;
			int m = 0;
			for (int j = 0; j < n; j++)
			{
				Color128 bc = Element.BackgroundColor.CreateBackColor(Game.ScoreMessages[j].Color, 1.0f);
				Color128 tc = Element.TextColor.CreateTextColor(Game.ScoreMessages[j].Color, 1.0f);
				Color128 oc = Element.OverlayColor.CreateTextColor(Game.ScoreMessages[j].Color, 1.0f);
				double tx, ty;
				bool preserve = false;
				if ((Element.Transition & HUD.Transition.Move) != 0)
				{
					if (Game.SecondsSinceMidnight < Game.ScoreMessages[j].Timeout)
					{
						if (Game.ScoreMessages[j].RendererAlpha == 0.0)
						{
							Game.ScoreMessages[j].RendererPosition.X = x + Element.TransitionVector.X;
							Game.ScoreMessages[j].RendererPosition.Y = y + Element.TransitionVector.Y;
							Game.ScoreMessages[j].RendererAlpha = 1.0;
						}
						tx = x;
						ty = y + m * Element.Value2;
						preserve = true;
					}
					else if (Element.Transition == HUD.Transition.MoveAndFade)
					{
						tx = x;
						ty = y + m * Element.Value2;
					}
					else
					{
						tx = x + Element.TransitionVector.X;
						ty = y + (j + 1) * Element.TransitionVector.Y;
					}
					const double speed = 2.0;
					double dx = (speed * Math.Abs(tx - Game.ScoreMessages[j].RendererPosition.X) + 0.1) * TimeElapsed;
					double dy = (speed * Math.Abs(ty - Game.ScoreMessages[j].RendererPosition.Y) + 0.1) * TimeElapsed;
					if (Math.Abs(tx - Game.ScoreMessages[j].RendererPosition.X) < dx)
					{
						Game.ScoreMessages[j].RendererPosition.X = tx;
					}
					else
					{
						Game.ScoreMessages[j].RendererPosition.X += Math.Sign(tx - Game.ScoreMessages[j].RendererPosition.X) * dx;
					}
					if (Math.Abs(ty - Game.ScoreMessages[j].RendererPosition.Y) < dy)
					{
						Game.ScoreMessages[j].RendererPosition.Y = ty;
					}
					else
					{
						Game.ScoreMessages[j].RendererPosition.Y += Math.Sign(ty - Game.ScoreMessages[j].RendererPosition.Y) * dy;
					}
				}
				else
				{
					tx = x;
					ty = y + m * Element.Value2;
					Game.ScoreMessages[j].RendererPosition.X = 0.0;
					const double speed = 12.0;
					double dy = (speed * Math.Abs(ty - Game.ScoreMessages[j].RendererPosition.Y) + 0.1) * TimeElapsed;
					Game.ScoreMessages[j].RendererPosition.X = x;
					if (Math.Abs(ty - Game.ScoreMessages[j].RendererPosition.Y) < dy)
					{
						Game.ScoreMessages[j].RendererPosition.Y = ty;
					}
					else
					{
						Game.ScoreMessages[j].RendererPosition.Y += Math.Sign(ty - Game.ScoreMessages[j].RendererPosition.Y) * dy;
					}
				}
				if ((Element.Transition & HUD.Transition.Fade) != 0)
				{
					if (Game.SecondsSinceMidnight >= Game.ScoreMessages[j].Timeout)
					{
						Game.ScoreMessages[j].RendererAlpha -= TimeElapsed;
						if (Game.ScoreMessages[j].RendererAlpha < 0.0) Game.ScoreMessages[j].RendererAlpha = 0.0;
					}
					else
					{
						Game.ScoreMessages[j].RendererAlpha += TimeElapsed;
						if (Game.ScoreMessages[j].RendererAlpha > 1.0) Game.ScoreMessages[j].RendererAlpha = 1.0;
						preserve = true;
					}
				}
				else if (Game.SecondsSinceMidnight > Game.ScoreMessages[j].Timeout)
				{
					if (Math.Abs(Game.ScoreMessages[j].RendererPosition.X - tx) < 0.1 & Math.Abs(Game.ScoreMessages[j].RendererPosition.Y - ty) < 0.1)
					{
						Game.ScoreMessages[j].RendererAlpha = 0.0;
					}
				}
				if (preserve) m++;
				double px = Game.ScoreMessages[j].RendererPosition.X + (double)j * (double)Element.Value1;
				double py = Game.ScoreMessages[j].RendererPosition.Y;
				float alpha = (float)(Game.ScoreMessages[j].RendererAlpha * Game.ScoreMessages[j].RendererAlpha);
				// graphics
				HUD.Image Left = j == 0 ? Element.TopLeft : j < n - 1 ? Element.CenterLeft : Element.BottomLeft;
				HUD.Image Middle = j == 0 ? Element.TopMiddle : j < n - 1 ? Element.CenterMiddle : Element.BottomMiddle;
				HUD.Image Right = j == 0 ? Element.TopRight : j < n - 1 ? Element.CenterRight : Element.BottomRight;
				// left background
				if (Left.BackgroundTexture != null)
				{
					if (Program.CurrentHost.LoadTexture(Left.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
					{
						double u = (double)Left.BackgroundTexture.Width;
						double v = (double)Left.BackgroundTexture.Height;
						GL.Color4(bc.R, bc.G, bc.B, bc.A * alpha);
						LibRender.Renderer.RenderOverlayTexture(Left.BackgroundTexture, px, py, px + u, py + v);
					}
				}
				// right background
				if (Right.BackgroundTexture != null)
				{
					if (Program.CurrentHost.LoadTexture(Right.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
					{
						double u = (double)Right.BackgroundTexture.Width;
						double v = (double)Right.BackgroundTexture.Height;
						GL.Color4(bc.R, bc.G, bc.B, bc.A * alpha);
						LibRender.Renderer.RenderOverlayTexture(Right.BackgroundTexture, px + w - u, py, px + w, py + v);
					}
				}
				// middle background
				if (Middle.BackgroundTexture != null)
				{
					if (Program.CurrentHost.LoadTexture(Middle.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
					{
						double v = (double)Middle.BackgroundTexture.Height;
						GL.Color4(bc.R, bc.G, bc.B, bc.A * alpha);
						LibRender.Renderer.RenderOverlayTexture(Middle.BackgroundTexture, px + lw, py, px + w - rw, py + v);
					}
				}
				{ // text
					string t = Game.ScoreMessages[j].Text;
					double u = widths[j];
					double v = heights[j];
					double p = Math.Round((Element.TextAlignment.X < 0 ? px : Element.TextAlignment.X > 0 ? px + w - u : px + 0.5 * (w - u)) - j * Element.Value1);
					double q = Math.Round(Element.TextAlignment.Y < 0 ? py : Element.TextAlignment.Y > 0 ? py + lcrh - v : py + 0.5 * (lcrh - v));
					p += Element.TextPosition.X;
					q += Element.TextPosition.Y;
					LibRender.Renderer.DrawString(Element.Font, t, new System.Drawing.Point((int)p, (int)q), TextAlignment.TopLeft, new Color128(tc.R, tc.G, tc.B, tc.A * alpha), Element.TextShadow);
				}
				// left overlay
				if (Left.OverlayTexture != null)
				{
					if (Program.CurrentHost.LoadTexture(Left.OverlayTexture, OpenGlTextureWrapMode.ClampClamp))
					{
						double u = (double)Left.OverlayTexture.Width;
						double v = (double)Left.OverlayTexture.Height;
						GL.Color4(oc.R, oc.G, oc.B, oc.A * alpha);
						LibRender.Renderer.RenderOverlayTexture(Left.OverlayTexture, px, py, px + u, py + v);
					}
				}
				// right overlay
				if (Right.OverlayTexture != null)
				{
					if (Program.CurrentHost.LoadTexture(Right.OverlayTexture, OpenGlTextureWrapMode.ClampClamp))
					{
						double u = (double)Right.OverlayTexture.Width;
						double v = (double)Right.OverlayTexture.Height;
						GL.Color4(oc.R, oc.G, oc.B, oc.A * alpha);
						LibRender.Renderer.RenderOverlayTexture(Right.OverlayTexture, px + w - u, py, px + w, py + v);
					}
				}
				// middle overlay
				if (Middle.OverlayTexture != null)
				{
					if (Program.CurrentHost.LoadTexture(Middle.OverlayTexture, OpenGlTextureWrapMode.ClampClamp))
					{
						double v = (double)Middle.OverlayTexture.Height;
						GL.Color4(oc.R, oc.G, oc.B, oc.A * alpha);
						LibRender.Renderer.RenderOverlayTexture(Middle.OverlayTexture, px + lw, py, px + w - rw, py + v);
					}
				}
			}
		}
	}
}
