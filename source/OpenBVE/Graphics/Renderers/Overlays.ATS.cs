using System;
using System.Drawing;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Textures;

namespace OpenBve.Graphics.Renderers
{
	internal partial class Overlays
	{
		/// <summary>Renders the ATS lamp overlay</summary>
		/// <param name="Element">The HUD element these are to be rendererd onto</param>
		/// <param name="TimeElapsed">The time elapsed</param>
		private void RenderATSLamps(HUD.Element Element, double TimeElapsed)
		{
			if (TrainManager.PlayerTrain == null)
			{
				return;
			}
			// ats lamps
			if (CurrentLampCollection.Lamps == null)
			{
				InitializeLamps();
			}
			double lcrh, lw, rw;
			CalculateViewingPlaneSize(Element, out lw, out rw, out lcrh);
			// start

			// ReSharper disable once PossibleNullReferenceException
			int n = CurrentLampCollection.Lamps.Length;
			double w = CurrentLampCollection.Width + lw + rw;
			double h = Element.Value2 * n;
			double x = Element.Alignment.X < 0 ? 0.0 : Element.Alignment.X > 0 ? renderer.Screen.Width - w : 0.5 * (renderer.Screen.Width - w);
			double y = Element.Alignment.Y < 0 ? 0.0 : Element.Alignment.Y > 0 ? renderer.Screen.Height - h : 0.5 * (renderer.Screen.Height - h);
			x += Element.Position.X;
			y += Element.Position.Y;
			for (int j = 0; j < n; j++)
			{
				if (CurrentLampCollection.Lamps[j].Type != LampType.None)
				{
					int o;
					if (j == 0)
					{
						o = -1;
					}
					else if (CurrentLampCollection.Lamps[j - 1].Type == LampType.None)
					{
						o = -1;
					}
					else if (j < n - 1 && CurrentLampCollection.Lamps[j + 1].Type == LampType.None)
					{
						o = 1;
					}
					else if (j == n - 1)
					{
						o = 1;
					}
					else
					{
						o = 0;
					}
					HUD.Image Left = o < 0 ? Element.TopLeft : o == 0 ? Element.CenterLeft : Element.BottomLeft;
					HUD.Image Middle = o < 0 ? Element.TopMiddle : o == 0 ? Element.CenterMiddle : Element.BottomMiddle;
					HUD.Image Right = o < 0 ? Element.TopRight : o == 0 ? Element.CenterRight : Element.BottomRight;
					MessageColor sc = MessageColor.Gray;
					if (TrainManager.PlayerTrain.Plugin.Panel.Length >= 272)
					{
						switch (CurrentLampCollection.Lamps[j].Type)
						{
							case LampType.Ats:
								if (TrainManager.PlayerTrain.Plugin.Panel[256] != 0)
								{
									sc = MessageColor.Orange;
								}
								break;
							case LampType.AtsOperation:
								if (TrainManager.PlayerTrain.Plugin.Panel[258] != 0)
								{
									sc = MessageColor.Red;
								}
								break;
							case LampType.AtsPPower:
								if (TrainManager.PlayerTrain.Plugin.Panel[259] != 0)
								{
									sc = MessageColor.Green;
								}
								break;
							case LampType.AtsPPattern:
								if (TrainManager.PlayerTrain.Plugin.Panel[260] != 0)
								{
									sc = MessageColor.Orange;
								}
								break;
							case LampType.AtsPBrakeOverride:
								if (TrainManager.PlayerTrain.Plugin.Panel[261] != 0)
								{
									sc = MessageColor.Orange;
								}
								break;
							case LampType.AtsPBrakeOperation:
								if (TrainManager.PlayerTrain.Plugin.Panel[262] != 0)
								{
									sc = MessageColor.Orange;
								}
								break;
							case LampType.AtsP:
								if (TrainManager.PlayerTrain.Plugin.Panel[263] != 0)
								{
									sc = MessageColor.Green;
								}
								break;
							case LampType.AtsPFailure:
								if (TrainManager.PlayerTrain.Plugin.Panel[264] != 0)
								{
									sc = MessageColor.Red;
								}
								break;
							case LampType.Atc:
								if (TrainManager.PlayerTrain.Plugin.Panel[265] != 0)
								{
									sc = MessageColor.Orange;
								}
								break;
							case LampType.AtcPower:
								if (TrainManager.PlayerTrain.Plugin.Panel[266] != 0)
								{
									sc = MessageColor.Orange;
								}
								break;
							case LampType.AtcUse:
								if (TrainManager.PlayerTrain.Plugin.Panel[267] != 0)
								{
									sc = MessageColor.Orange;
								}
								break;
							case LampType.AtcEmergency:
								if (TrainManager.PlayerTrain.Plugin.Panel[268] != 0)
								{
									sc = MessageColor.Red;
								}
								break;
							case LampType.Eb:
								if (TrainManager.PlayerTrain.Plugin.Panel[270] != 0)
								{
									sc = MessageColor.Green;
								}
								break;
							case LampType.ConstSpeed:
								if (TrainManager.PlayerTrain.Plugin.Panel[269] != 0)
								{
									sc = MessageColor.Orange;
								}
								break;
						}
					}
					// colors
					Color128 bc = Element.BackgroundColor.CreateBackColor(sc, 1.0f);
					Color128 tc = Element.TextColor.CreateTextColor(sc, 1.0f);
					Color128 oc = Element.OverlayColor.CreateBackColor(sc, 1.0f);
					// left background
					if (Left.BackgroundTexture != null)
					{
						if (Program.CurrentHost.LoadTexture(Left.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
						{
							double u = Left.BackgroundTexture.Width;
							double v = Left.BackgroundTexture.Height;
							renderer.Rectangle.Draw(Left.BackgroundTexture, new PointF((float)x, (float)y), new SizeF((float)u, (float)v), new Color128(bc.R, bc.G, bc.B, bc.A));
						}
					}
					// right background
					if (Right.BackgroundTexture != null)
					{
						if (Program.CurrentHost.LoadTexture(Right.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
						{
							double u = Right.BackgroundTexture.Width;
							double v = Right.BackgroundTexture.Height;
							renderer.Rectangle.Draw(Right.BackgroundTexture, new PointF((float)(x + w - u), (float)y), new SizeF((float)u, (float)v), new Color128(bc.R, bc.G, bc.B, bc.A));
						}
					}
					// middle background
					if (Middle.BackgroundTexture != null)
					{
						if (Program.CurrentHost.LoadTexture(Middle.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
						{
							double v = Middle.BackgroundTexture.Height;
							renderer.Rectangle.Draw(Middle.BackgroundTexture, new PointF((float)(x + lw), (float)y), new SizeF((float)(w - lw - rw), (float)v), new Color128(bc.R, bc.G, bc.B, bc.A));
						}
					}
					{ // text
						string t = CurrentLampCollection.Lamps[j].Text;
						double u = CurrentLampCollection.Lamps[j].Width;
						double v = CurrentLampCollection.Lamps[j].Height;
						double p = Math.Round(Element.TextAlignment.X < 0 ? x : Element.TextAlignment.X > 0 ? x + w - u : x + 0.5 * (w - u));
						double q = Math.Round(Element.TextAlignment.Y < 0 ? y : Element.TextAlignment.Y > 0 ? y + lcrh - v : y + 0.5 * (lcrh - v));
						p += Element.TextPosition.X;
						q += Element.TextPosition.Y;
						renderer.OpenGlString.Draw(Element.Font, t, new System.Drawing.Point((int)p, (int)q), TextAlignment.TopLeft, tc, Element.TextShadow);
					}
					// left overlay
					if (Left.OverlayTexture != null)
					{
						if (Program.CurrentHost.LoadTexture(Left.OverlayTexture, OpenGlTextureWrapMode.ClampClamp))
						{
							double u = Left.OverlayTexture.Width;
							double v = Left.OverlayTexture.Height;
							renderer.Rectangle.Draw(Left.OverlayTexture, new PointF((float)x, (float)y), new SizeF((float)u, (float)v), new Color128(oc.R, oc.G, oc.B, oc.A));
						}
					}
					// right overlay
					if (Right.OverlayTexture != null)
					{
						if (Program.CurrentHost.LoadTexture(Right.OverlayTexture, OpenGlTextureWrapMode.ClampClamp))
						{
							double u = Right.OverlayTexture.Width;
							double v = Right.OverlayTexture.Height;
							renderer.Rectangle.Draw(Right.OverlayTexture, new PointF((float)(x + w - u), (float)y), new SizeF((float)u, (float)v), new Color128(oc.R, oc.G, oc.B, oc.A));
						}
					}
					// middle overlay
					if (Middle.OverlayTexture != null)
					{
						if (Program.CurrentHost.LoadTexture(Middle.OverlayTexture, OpenGlTextureWrapMode.ClampClamp))
						{
							double v = Middle.OverlayTexture.Height;
							renderer.Rectangle.Draw(Middle.OverlayTexture, new PointF((float)(x + lw), (float)y), new SizeF((float)(w - lw - rw), (float)v), new Color128(oc.R, oc.G, oc.B, oc.A));
						}
					}
				}
				y += Element.Value2;
			}
		}
	}
}
