﻿using System;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Math;
using OpenBveApi.Textures;

namespace OpenBve.Graphics.Renderers
{
	internal partial class Overlays
	{
		/// <summary>Renders the list of score messages</summary>
		/// <param name="Element">The HUD element these are to be rendererd onto</param>
		/// <param name="TimeElapsed">The time elapsed</param>
		private void RenderScoreMessages(HUD.Element Element, double TimeElapsed)
		{
			// score messages
			int n = Game.ScoreMessages.Length;
			double totalwidth = 16.0f;
			double[] widths = new double[n];
			double[] heights = new double[n];
			for (int j = 0; j < n; j++)
			{
				Vector2 size = Element.Font.MeasureString(Game.ScoreMessages[j].Text);
				widths[j] = size.X;
				heights[j] = size.Y;
				double a = widths[j] - j * Element.Value1;
				if (a > totalwidth)
				{
					totalwidth = a;
				}
			}
			Game.ScoreMessagesRendererSize.X += 16.0 * TimeElapsed * (totalwidth - Game.ScoreMessagesRendererSize.X);
			totalwidth = (float)Game.ScoreMessagesRendererSize.X;
			Element.CalculateViewingPlaneSize(out double lw, out double rw, out double lcrh);
			// start
			double w = Element.Alignment.X == 0 ? lw + rw + 128 : totalwidth + lw + rw;
			double h = Element.Value2 * n;
			double x = Element.Alignment.X < 0 ? 0.0 : Element.Alignment.X > 0 ? renderer.Screen.Width - w : 0.5 * (renderer.Screen.Width - w);
			double y = Element.Alignment.Y < 0 ? 0.0 : Element.Alignment.Y > 0 ? renderer.Screen.Height - h : 0.5 * (renderer.Screen.Height - h);
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
					if (Program.CurrentRoute.SecondsSinceMidnight < Game.ScoreMessages[j].Timeout)
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
					if (Program.CurrentRoute.SecondsSinceMidnight >= Game.ScoreMessages[j].Timeout)
					{
						Game.ScoreMessages[j].RendererAlpha -= TimeElapsed;
						if (Game.ScoreMessages[j].RendererAlpha < 0.0)
						{
							Game.ScoreMessages[j].RendererAlpha = 0.0;
						}
					}
					else
					{
						Game.ScoreMessages[j].RendererAlpha += TimeElapsed;
						if (Game.ScoreMessages[j].RendererAlpha > 1.0)
						{
							Game.ScoreMessages[j].RendererAlpha = 1.0;
						}

						preserve = true;
					}
				}
				else if (Program.CurrentRoute.SecondsSinceMidnight > Game.ScoreMessages[j].Timeout)
				{
					if (Math.Abs(Game.ScoreMessages[j].RendererPosition.X - tx) < 0.1 & Math.Abs(Game.ScoreMessages[j].RendererPosition.Y - ty) < 0.1)
					{
						Game.ScoreMessages[j].RendererAlpha = 0.0;
					}
				}
				if (preserve)
				{
					m++;
				}

				double px = Game.ScoreMessages[j].RendererPosition.X + j * (double)Element.Value1;
				double py = Game.ScoreMessages[j].RendererPosition.Y;
				float alpha = (float)(Game.ScoreMessages[j].RendererAlpha * Game.ScoreMessages[j].RendererAlpha);
				// graphics
				HUD.Image Left = j == 0 ? Element.TopLeft : j < n - 1 ? Element.CenterLeft : Element.BottomLeft;
				HUD.Image Middle = j == 0 ? Element.TopMiddle : j < n - 1 ? Element.CenterMiddle : Element.BottomMiddle;
				HUD.Image Right = j == 0 ? Element.TopRight : j < n - 1 ? Element.CenterRight : Element.BottomRight;
				// left background
				if (Program.CurrentHost.LoadTexture(ref Left.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					double u = Left.BackgroundTexture.Width;
					double v = Left.BackgroundTexture.Height;
					renderer.Rectangle.Draw(Left.BackgroundTexture, new Vector2(px, py), new Vector2(u, v), new Color128(bc.R, bc.G, bc.B, bc.A * alpha));
				}
				// right background
				if (Program.CurrentHost.LoadTexture(ref Right.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					double u = Right.BackgroundTexture.Width;
					double v = Right.BackgroundTexture.Height;
					renderer.Rectangle.Draw(Right.BackgroundTexture, new Vector2(px + w - u, py), new Vector2(u, v), new Color128(bc.R, bc.G, bc.B, bc.A * alpha));
				}
				// middle background
				if (Program.CurrentHost.LoadTexture(ref Middle.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					double v = Middle.BackgroundTexture.Height;
					renderer.Rectangle.Draw(Middle.BackgroundTexture, new Vector2(px + lw, py), new Vector2(w - lw - rw, v), new Color128(bc.R, bc.G, bc.B, bc.A * alpha));
				}
				{ // text
					double u = widths[j];
					double v = heights[j];
					double p = Math.Round((Element.TextAlignment.X < 0 ? px : Element.TextAlignment.X > 0 ? px + w - u : px + 0.5 * (w - u)) - j * Element.Value1);
					double q = Math.Round(Element.TextAlignment.Y < 0 ? py : Element.TextAlignment.Y > 0 ? py + lcrh - v : py + 0.5 * (lcrh - v));
					p += Element.TextPosition.X;
					q += Element.TextPosition.Y;
					renderer.OpenGlString.Draw(Element.Font, Game.ScoreMessages[j].Text, new Vector2(p, q), TextAlignment.TopLeft, new Color128(tc.R, tc.G, tc.B, tc.A * alpha), Element.TextShadow);
				}
				// left overlay
				if (Program.CurrentHost.LoadTexture(ref Left.OverlayTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					renderer.Rectangle.Draw(Left.OverlayTexture, new Vector2(px, py), new Vector2(Left.OverlayTexture.Width, Left.OverlayTexture.Height), new Color128(oc.R, oc.G, oc.B, oc.A * alpha));
				}
				// right overlay
				if (Program.CurrentHost.LoadTexture(ref Right.OverlayTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					double u = Right.OverlayTexture.Width;
					double v = Right.OverlayTexture.Height;
					renderer.Rectangle.Draw(Right.OverlayTexture, new Vector2(px + w - u, py), new Vector2(u, v), new Color128(oc.R, oc.G, oc.B, oc.A * alpha));
				}
				// middle overlay
				if (Program.CurrentHost.LoadTexture(ref Middle.OverlayTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					double v = Middle.OverlayTexture.Height;
					renderer.Rectangle.Draw(Middle.OverlayTexture, new Vector2(px + lw, py), new Vector2(w - lw - rw, v), new Color128(oc.R, oc.G, oc.B, oc.A * alpha));
				}
			}
		}
	}
}
