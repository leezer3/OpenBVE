using System;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Math;
using OpenBveApi.Textures;

namespace OpenBve.Graphics.Renderers
{
	internal partial class Overlays
	{
		/// <summary>The current size of the plane upon which messages are rendered</summary>
		private static Vector2 MessagesRendererSize = new Vector2(16.0, 16.0);
		/// <summary>Renders the list of game (textual) messages</summary>
		/// <param name="Element">The HUD element these are to be rendered onto</param>
		/// <param name="TimeElapsed">The time elapsed</param>
		private void RenderGameMessages(HUD.Element Element, double TimeElapsed)
		{
			//Calculate the size of the viewing plane
			int n = MessageManager.TextualMessages.Count;
			//Minimum initial width is 16px
			double totalwidth = 16.0f;
			for (int j = 0; j < n; j++)
			{
				//Update font size for the renderer
				Vector2 size = Element.Font.MeasureString((string)MessageManager.TextualMessages[j].MessageToDisplay);
				MessageManager.TextualMessages[j].Width = size.X;
				MessageManager.TextualMessages[j].Height = size.Y;
				//Run through the list of current messages
				double a = MessageManager.TextualMessages[j].Width - j * Element.Value1;
				//If our width is wider than the old, use this as the NEW viewing plane width
				if (a > totalwidth)
				{
					totalwidth = a;
				}
			}
			//Calculate the X-width of the viewing plane
			MessagesRendererSize.X += 16.0 * TimeElapsed * (totalwidth -MessagesRendererSize.X);
			totalwidth = (float)MessagesRendererSize.X;
			//Calculate final viewing plane size to pass to openGL
			Element.CalculateViewingPlaneSize(out double lw, out double rw, out double lcrh);

			// start
			double w = totalwidth + lw + rw;
			double h = Element.Value2 * n;
			double x = Element.Alignment.X < 0 ? 0.0 : Element.Alignment.X > 0 ? renderer.Screen.Width - w : 0.5 * (renderer.Screen.Width - w);
			double y = Element.Alignment.Y < 0 ? 0.0 : Element.Alignment.Y > 0 ? renderer.Screen.Height - h : 0.5 * (renderer.Screen.Height - h);
			x += Element.Position.X;
			y += Element.Position.Y;
			int m = 0;
			for (int j = 0; j < n; j++)
			{
				RouteManager2.MessageManager.AbstractMessage mm = MessageManager.TextualMessages[j];
				Color128 bc = Element.BackgroundColor.CreateBackColor(mm.Color, 1.0f);
				Color128 tc = Element.TextColor.CreateTextColor(mm.Color, 1.0f);
				Color128 oc = Element.OverlayColor.CreateBackColor(mm.Color, 1.0f);
				double tx, ty;
				bool preserve = false;
				if ((Element.Transition & HUD.Transition.Move) != 0)
				{
					if (mm.Timeout > 0)
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
					if (mm.Timeout <= 0)
					{
						mm.RendererAlpha -= TimeElapsed;
						if (mm.RendererAlpha < 0.0)
						{
							mm.RendererAlpha = 0.0;
						}
					}
					else
					{
						mm.RendererAlpha += TimeElapsed;
						if (mm.RendererAlpha > 1.0)
						{
							mm.RendererAlpha = 1.0;
						}

						preserve = true;
					}
				}
				else if (mm.Timeout < 0)
				{
					if (Math.Abs(mm.RendererPosition.X - tx) < 0.1 & Math.Abs(mm.RendererPosition.Y - ty) < 0.1)
					{
						mm.RendererAlpha = 0.0;
					}
				}
				if (preserve)
				{
					m++;
				}

				double px = mm.RendererPosition.X + j * Element.Value1;
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
				if (Program.CurrentHost.LoadTexture(ref Left.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					renderer.Rectangle.Draw(Left.BackgroundTexture, new Vector2(px, py), new Color128(bc.R, bc.G, bc.B, bc.A * alpha));
				}
				// right background
				if (Program.CurrentHost.LoadTexture(ref Right.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					renderer.Rectangle.Draw(Right.BackgroundTexture, new Vector2(px + w - Right.BackgroundTexture.Width, py), new Color128(bc.R, bc.G, bc.B, bc.A * alpha));
				}
				// middle background
				if (Program.CurrentHost.LoadTexture(ref Middle.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					double v = Middle.BackgroundTexture.Height;
					renderer.Rectangle.Draw(Middle.BackgroundTexture, new Vector2(px + lw, py), new Vector2(w - lw - rw, v), new Color128(bc.R, bc.G, bc.B, bc.A * alpha));
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
					renderer.OpenGlString.Draw(Element.Font, t, new Vector2(p, q),
						TextAlignment.TopLeft, new Color128(tc.R, tc.G, tc.B, tc.A * alpha), Element.TextShadow);
				}
				// left overlay
				if (Program.CurrentHost.LoadTexture(ref Left.OverlayTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					renderer.Rectangle.Draw(Left.OverlayTexture, new Vector2(px, py), new Color128(oc.R, oc.G, oc.B, oc.A * alpha));
				}
				// right overlay
				if (Program.CurrentHost.LoadTexture(ref Right.OverlayTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					renderer.Rectangle.Draw(Right.OverlayTexture, new Vector2(px + w - Right.OverlayTexture.Width, py), new Color128(oc.R, oc.G, oc.B, oc.A * alpha));
				}
				// middle overlay
				if (Program.CurrentHost.LoadTexture(ref Middle.OverlayTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					double v = Middle.OverlayTexture.Height;
					renderer.Rectangle.Draw(Middle.OverlayTexture, new Vector2(px + lw, py), new Vector2(w - lw - rw, v), new Color128(oc.R, oc.G, oc.B, oc.A * alpha));
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
