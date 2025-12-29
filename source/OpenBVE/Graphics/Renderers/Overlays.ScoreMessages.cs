using System;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Math;
using OpenBveApi.Textures;

namespace OpenBve.Graphics.Renderers
{
	internal partial class Overlays
	{
		/// <summary>Renders the list of score messages</summary>
		/// <param name="element">The HUD element these are to be rendered onto</param>
		/// <param name="timeElapsed">The time elapsed</param>
		private void RenderScoreMessages(HUD.Element element, double timeElapsed)
		{
			// score messages
			int n = Game.ScoreMessages.Count;
			double totalwidth = 16.0f;
			double[] widths = new double[n];
			double[] heights = new double[n];
			for (int j = 0; j < n; j++)
			{
				Vector2 size = element.Font.MeasureString(Game.ScoreMessages[j].Text);
				widths[j] = size.X;
				heights[j] = size.Y;
				double a = widths[j] - j * element.Value1;
				if (a > totalwidth)
				{
					totalwidth = a;
				}
			}
			Game.ScoreMessagesRendererSize.X += 16.0 * timeElapsed * (totalwidth - Game.ScoreMessagesRendererSize.X);
			totalwidth = (float)Game.ScoreMessagesRendererSize.X;
			element.CalculateViewingPlaneSize(out double lw, out double rw, out double lcrh);
			// start
			double w = element.Alignment.X == 0 ? lw + rw + 128 : totalwidth + lw + rw;
			double h = element.Value2 * n;
			double x = element.Alignment.X < 0 ? 0.0 : element.Alignment.X > 0 ? renderer.Screen.Width - w : 0.5 * (renderer.Screen.Width - w);
			double y = element.Alignment.Y < 0 ? 0.0 : element.Alignment.Y > 0 ? renderer.Screen.Height - h : 0.5 * (renderer.Screen.Height - h);
			x += element.Position.X;
			y += element.Position.Y;
			int m = 0;
			for (int j = 0; j < n; j++)
			{
				Color128 bc = element.BackgroundColor.CreateBackColor(Game.ScoreMessages[j].Color, 1.0f);
				Color128 tc = element.TextColor.CreateTextColor(Game.ScoreMessages[j].Color, 1.0f);
				Color128 oc = element.OverlayColor.CreateTextColor(Game.ScoreMessages[j].Color, 1.0f);
				double tx, ty;
				bool preserve = false;
				if ((element.Transition & HUD.Transition.Move) != 0)
				{
					if (Game.ScoreMessages[j].Timeout >= 0)
					{
						if (Game.ScoreMessages[j].RendererAlpha == 0.0)
						{
							Game.ScoreMessages[j].RendererPosition.X = x + element.TransitionVector.X;
							Game.ScoreMessages[j].RendererPosition.Y = y + element.TransitionVector.Y;
							Game.ScoreMessages[j].RendererAlpha = 1.0;
						}
						tx = x;
						ty = y + m * element.Value2;
						preserve = true;
					}
					else if (element.Transition == HUD.Transition.MoveAndFade)
					{
						tx = x;
						ty = y + m * element.Value2;
					}
					else
					{
						tx = x + element.TransitionVector.X;
						ty = y + (j + 1) * element.TransitionVector.Y;
					}
					const double speed = 2.0;
					double dx = (speed * Math.Abs(tx - Game.ScoreMessages[j].RendererPosition.X) + 0.1) * timeElapsed;
					double dy = (speed * Math.Abs(ty - Game.ScoreMessages[j].RendererPosition.Y) + 0.1) * timeElapsed;
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
					ty = y + m * element.Value2;
					Game.ScoreMessages[j].RendererPosition.X = 0.0;
					const double speed = 12.0;
					double dy = (speed * Math.Abs(ty - Game.ScoreMessages[j].RendererPosition.Y) + 0.1) * timeElapsed;
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
				if ((element.Transition & HUD.Transition.Fade) != 0)
				{
					if (Game.ScoreMessages[j].Timeout <= 0)
					{
						Game.ScoreMessages[j].RendererAlpha -= timeElapsed;
						if (Game.ScoreMessages[j].RendererAlpha < 0.0)
						{
							Game.ScoreMessages[j].RendererAlpha = 0.0;
						}
					}
					else
					{
						Game.ScoreMessages[j].RendererAlpha += timeElapsed;
						if (Game.ScoreMessages[j].RendererAlpha > 1.0)
						{
							Game.ScoreMessages[j].RendererAlpha = 1.0;
						}

						preserve = true;
					}
				}
				else if (Game.ScoreMessages[j].Timeout < 0)
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

				double px = Game.ScoreMessages[j].RendererPosition.X + j * element.Value1;
				double py = Game.ScoreMessages[j].RendererPosition.Y;
				float alpha = (float)(Game.ScoreMessages[j].RendererAlpha * Game.ScoreMessages[j].RendererAlpha);
				// graphics
				HUD.Image Left = j == 0 ? element.TopLeft : j < n - 1 ? element.CenterLeft : element.BottomLeft;
				HUD.Image Middle = j == 0 ? element.TopMiddle : j < n - 1 ? element.CenterMiddle : element.BottomMiddle;
				HUD.Image Right = j == 0 ? element.TopRight : j < n - 1 ? element.CenterRight : element.BottomRight;
				// left background
				if (Program.CurrentHost.LoadTexture(ref Left.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					renderer.Rectangle.Draw(Left.BackgroundTexture, new Vector2(px, py), Left.BackgroundTexture.Size, new Color128(bc.R, bc.G, bc.B, bc.A * alpha));
				}
				// right background
				if (Program.CurrentHost.LoadTexture(ref Right.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					renderer.Rectangle.Draw(Right.BackgroundTexture, new Vector2(px + w - Right.BackgroundTexture.Width, py), Right.BackgroundTexture.Size, new Color128(bc.R, bc.G, bc.B, bc.A * alpha));
				}
				// middle background
				if (Program.CurrentHost.LoadTexture(ref Middle.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					renderer.Rectangle.Draw(Middle.BackgroundTexture, new Vector2(px + lw, py), new Vector2(w - lw - rw, Middle.BackgroundTexture.Height), new Color128(bc.R, bc.G, bc.B, bc.A * alpha));
				}
				// text
				double p = Math.Round((element.TextAlignment.X < 0 ? px : element.TextAlignment.X > 0 ? px + w - widths[j] : px + 0.5 * (w - widths[j])) - j * element.Value1);
				double q = Math.Round(element.TextAlignment.Y < 0 ? py : element.TextAlignment.Y > 0 ? py + lcrh - heights[j] : py + 0.5 * (lcrh - heights[j]));
				p += element.TextPosition.X;
				q += element.TextPosition.Y;
				renderer.OpenGlString.Draw(element.Font, Game.ScoreMessages[j].Text, new Vector2(p, q), TextAlignment.TopLeft, new Color128(tc.R, tc.G, tc.B, tc.A * alpha), element.TextShadow);
				// left overlay
				if (Program.CurrentHost.LoadTexture(ref Left.OverlayTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					renderer.Rectangle.Draw(Left.OverlayTexture, new Vector2(px, py), new Vector2(Left.OverlayTexture.Width, Left.OverlayTexture.Height), new Color128(oc.R, oc.G, oc.B, oc.A * alpha));
				}
				// right overlay
				if (Program.CurrentHost.LoadTexture(ref Right.OverlayTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					renderer.Rectangle.Draw(Right.OverlayTexture, new Vector2(px + w - Right.OverlayTexture.Width, py), Right.OverlayTexture.Size, new Color128(oc.R, oc.G, oc.B, oc.A * alpha));
				}
				// middle overlay
				if (Program.CurrentHost.LoadTexture(ref Middle.OverlayTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					renderer.Rectangle.Draw(Middle.OverlayTexture, new Vector2(px + lw, py), new Vector2(w - lw - rw, Middle.OverlayTexture.Height), new Color128(oc.R, oc.G, oc.B, oc.A * alpha));
				}
			}
		}
	}
}
