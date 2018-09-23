using System;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Textures;
using OpenTK.Graphics.OpenGL;

namespace OpenBve
{
	internal static partial class Renderer
	{
		/// <summary>Renders the list of game (textual) messages</summary>
		/// <param name="Element">The HUD element these are to be rendered onto</param>
		/// <param name="TimeElapsed">The time elapsed</param>
		private static void RenderGameMessages(HUD.Element Element, double TimeElapsed)
		{
			//Calculate the size of the viewing plane
			int n = MessageManager.TextualMessages.Count;
			//Minimum initial width is 16px
			double totalwidth = 16.0f;
			for (int j = 0; j < n; j++)
			{
				//Update font size for the renderer
				System.Drawing.Size size = Renderer.MeasureString(Element.Font, (string)MessageManager.TextualMessages[j].MessageToDisplay);
				MessageManager.TextualMessages[j].Width = size.Width;
				MessageManager.TextualMessages[j].Height = size.Height;
				//Run through the list of current messages
				double a = MessageManager.TextualMessages[j].Width - j * (double)Element.Value1;
				//If our width is wider than the old, use this as the NEW viewing plane width
				if (a > totalwidth) totalwidth = a;
			}
			//Calculate the X-width of the viewing plane
			Game.MessagesRendererSize.X += 16.0 * TimeElapsed * ((double)totalwidth - Game.MessagesRendererSize.X);
			totalwidth = (float)Game.MessagesRendererSize.X;
			double lcrh, lw, rw;
			//Calculate final viewing plane size to pass to openGL
			CalculateViewingPlaneSize(Element, out lw, out rw, out lcrh);

			// start
			double w = totalwidth + lw + rw;
			double h = Element.Value2 * n;
			double x = Element.Alignment.X < 0 ? 0.0 : Element.Alignment.X > 0 ? Screen.Width - w : 0.5 * (Screen.Width - w);
			double y = Element.Alignment.Y < 0 ? 0.0 : Element.Alignment.Y > 0 ? Screen.Height - h : 0.5 * (Screen.Height - h);
			x += Element.Position.X;
			y += Element.Position.Y;
			int m = 0;
			for (int j = 0; j < n; j++)
			{
				var mm = MessageManager.TextualMessages[j];
				float br, bg, bb, ba;
				CreateBackColor(Element.BackgroundColor, mm.Color, out br, out bg, out bb, out ba);
				float tr, tg, tb, ta;
				CreateTextColor(Element.TextColor, mm.Color, out tr, out tg, out tb, out ta);
				float or, og, ob, oa;
				CreateBackColor(Element.OverlayColor, mm.Color, out or, out og, out ob, out oa);
				double tx, ty;
				bool preserve = false;
				if ((Element.Transition & HUD.Transition.Move) != 0)
				{
					if (Game.SecondsSinceMidnight < mm.Timeout)
					{
						if (mm.RendererAlpha == 0.0)
						{
							mm.RendererPosition.X = x + Element.TransitionVector.X;
							mm.RendererPosition.Y = y + Element.TransitionVector.Y;
							mm.RendererAlpha = 1.0;
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
					double dx = (speed * Math.Abs(tx - mm.RendererPosition.X) + 0.1) * TimeElapsed;
					double dy = (speed * Math.Abs(ty - mm.RendererPosition.Y) + 0.1) * TimeElapsed;
					if (Math.Abs(tx - mm.RendererPosition.X) < dx)
					{
						mm.RendererPosition.X = tx;
					}
					else
					{
						mm.RendererPosition.X += Math.Sign(tx - mm.RendererPosition.X) * dx;
					}
					if (Math.Abs(ty - mm.RendererPosition.Y) < dy)
					{
						mm.RendererPosition.Y = ty;
					}
					else
					{
						mm.RendererPosition.Y += Math.Sign(ty - mm.RendererPosition.Y) * dy;
					}
				}
				else
				{
					tx = x;
					ty = y + m * Element.Value2;
					mm.RendererPosition.X = 0.0;
					const double speed = 12.0;
					double dy = (speed * Math.Abs(ty - mm.RendererPosition.Y) + 0.1) * TimeElapsed;
					mm.RendererPosition.X = x;
					if (Math.Abs(ty - mm.RendererPosition.Y) < dy)
					{
						mm.RendererPosition.Y = ty;
					}
					else
					{
						mm.RendererPosition.Y += Math.Sign(ty - mm.RendererPosition.Y) * dy;
					}
				}
				if ((Element.Transition & HUD.Transition.Fade) != 0)
				{
					if (Game.SecondsSinceMidnight >= mm.Timeout)
					{
						mm.RendererAlpha -= TimeElapsed;
						if (mm.RendererAlpha < 0.0) mm.RendererAlpha = 0.0;
					}
					else
					{
						mm.RendererAlpha += TimeElapsed;
						if (mm.RendererAlpha > 1.0) mm.RendererAlpha = 1.0;
						preserve = true;
					}
				}
				else if (Game.SecondsSinceMidnight > mm.Timeout)
				{
					if (Math.Abs(mm.RendererPosition.X - tx) < 0.1 & Math.Abs(mm.RendererPosition.Y - ty) < 0.1)
					{
						mm.RendererAlpha = 0.0;
					}
				}
				if (preserve) m++;
				double px = mm.RendererPosition.X + (double)j * (double)Element.Value1;
				double py = mm.RendererPosition.Y;
				float alpha = (float)(mm.RendererAlpha * mm.RendererAlpha);
				// graphics
				HUD.Image Left = j == 0
					? Element.TopLeft
					: j < n - 1
						? Element.CenterLeft
						: Element.BottomLeft;
				HUD.Image Middle = j == 0
					? Element.TopMiddle
					: j < n - 1
						? Element.CenterMiddle
						: Element.BottomMiddle;
				HUD.Image Right = j == 0
					? Element.TopRight
					: j < n - 1
						? Element.CenterRight
						: Element.BottomRight;
				// left background
				if (Left.BackgroundTexture != null)
				{
					if (Textures.LoadTexture(Left.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
					{
						double u = (double)Left.BackgroundTexture.Width;
						double v = (double)Left.BackgroundTexture.Height;
						GL.Color4(br, bg, bb, ba * alpha);
						RenderOverlayTexture(Left.BackgroundTexture, px, py, px + u, py + v);
					}
				}
				// right background
				if (Right.BackgroundTexture != null)
				{
					if (Textures.LoadTexture(Right.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
					{
						double u = (double)Right.BackgroundTexture.Width;
						double v = (double)Right.BackgroundTexture.Height;
						GL.Color4(br, bg, bb, ba * alpha);
						RenderOverlayTexture(Right.BackgroundTexture, px + w - u, py, px + w, py + v);
					}
				}
				// middle background
				if (Middle.BackgroundTexture != null)
				{
					if (Textures.LoadTexture(Middle.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
					{
						double v = (double)Middle.BackgroundTexture.Height;
						GL.Color4(br, bg, bb, ba * alpha);
						RenderOverlayTexture(Middle.BackgroundTexture, px + lw, py, px + w - rw, py + v);
					}
				}
				{
					// text
					string t = (string)mm.MessageToDisplay;
					double u = mm.Width;
					double v = mm.Height;
					double p =
						Math.Round(
							(Element.TextAlignment.X < 0
								? px
								: Element.TextAlignment.X > 0
									? px + w - u
									: px + 0.5 * (w - u)) - j * Element.Value1);
					double q = Math.Round(Element.TextAlignment.Y < 0
						? py
						: Element.TextAlignment.Y > 0
							? py + lcrh - v
							: py + 0.5 * (lcrh - v));
					p += Element.TextPosition.X;
					q += Element.TextPosition.Y;
					DrawString(Element.Font, t, new System.Drawing.Point((int)p, (int)q),
						TextAlignment.TopLeft, new Color128(tr, tg, tb, ta * alpha), Element.TextShadow);
				}
				// left overlay
				if (Left.OverlayTexture != null)
				{
					if (Textures.LoadTexture(Left.OverlayTexture, OpenGlTextureWrapMode.ClampClamp))
					{
						double u = (double)Left.OverlayTexture.Width;
						double v = (double)Left.OverlayTexture.Height;
						GL.Color4(or, og, ob, oa * alpha);
						RenderOverlayTexture(Left.OverlayTexture, px, py, px + u, py + v);
					}
				}
				// right overlay
				if (Right.OverlayTexture != null)
				{
					if (Textures.LoadTexture(Right.OverlayTexture, OpenGlTextureWrapMode.ClampClamp))
					{
						double u = (double)Right.OverlayTexture.Width;
						double v = (double)Right.OverlayTexture.Height;
						GL.Color4(or, og, ob, oa * alpha);
						RenderOverlayTexture(Right.OverlayTexture, px + w - u, py, px + w, py + v);
					}
				}
				// middle overlay
				if (Middle.OverlayTexture != null)
				{
					if (Textures.LoadTexture(Middle.OverlayTexture, OpenGlTextureWrapMode.ClampClamp))
					{
						double v = (double)Middle.OverlayTexture.Height;
						GL.Color4(or, og, ob, oa * alpha);
						RenderOverlayTexture(Middle.OverlayTexture, px + lw, py, px + w - rw, py + v);
					}
				}

				if (Element.Font.FontSize >= 20.0)
				{
					//Add a little more line-padding to the large font sizes
					//Asian and some Cyrillic charsets otherwise overlap slightly
					y += 10;
				}
			}
		}
	}
}
