using System;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Math;
using OpenBveApi.Textures;

namespace OpenBve.Graphics.Renderers
{
	internal partial class Overlays
	{
		/// <summary>Renders the ATS lamp overlay</summary>
		/// <param name="Element">The HUD element these are to be rendered onto</param>
		private void RenderATSLamps(HUD.Element Element)
		{
			if (TrainManager.PlayerTrain == null)
			{
				return;
			}
			// ats lamps
			if (CurrentLampCollection == null)
			{
				CurrentLampCollection = new LampCollection(TrainManager.PlayerTrain);
			}
			Element.CalculateViewingPlaneSize(out double lw, out double rw, out double lcrh);
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
				if (CurrentLampCollection.Lamps[j].Type == LampType.None)
				{
					continue;
				}

				int o;
				if (j == 0 || CurrentLampCollection.Lamps[j - 1].Type == LampType.None)
				{
					o = -1;
				}
				else if (j < n - 1 && CurrentLampCollection.Lamps[j + 1].Type == LampType.None || j == n - 1)
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
				if (Program.CurrentHost.LoadTexture(ref Left.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					renderer.Rectangle.Draw(Left.BackgroundTexture, new Vector2(x, y), bc);
				}

				// right background
				if (Program.CurrentHost.LoadTexture(ref Right.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					renderer.Rectangle.Draw(Right.BackgroundTexture, new Vector2(x + w - Right.BackgroundTexture.Width, y), bc);
				}

				// middle background
				if (Program.CurrentHost.LoadTexture(ref Middle.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					renderer.Rectangle.Draw(Middle.BackgroundTexture, new Vector2(x + lw, y), new Vector2(w - lw - rw, Middle.BackgroundTexture.Height), bc);
				}

				// text
				string t = CurrentLampCollection.Lamps[j].Text;
				double u = CurrentLampCollection.Lamps[j].Width;
				double v = CurrentLampCollection.Lamps[j].Height;
				double p = Math.Round(Element.TextAlignment.X < 0 ? x : Element.TextAlignment.X > 0 ? x + w - u : x + 0.5 * (w - u));
				double q = Math.Round(Element.TextAlignment.Y < 0 ? y : Element.TextAlignment.Y > 0 ? y + lcrh - v : y + 0.5 * (lcrh - v));
				p += Element.TextPosition.X;
				q += Element.TextPosition.Y;
				renderer.OpenGlString.Draw(Element.Font, t, new Vector2(p, q), TextAlignment.TopLeft, tc, Element.TextShadow);
				// left overlay
				if (Program.CurrentHost.LoadTexture(ref Left.OverlayTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					renderer.Rectangle.Draw(Left.OverlayTexture, new Vector2(x, y), oc);
				}

				// right overlay
				if (Program.CurrentHost.LoadTexture(ref Right.OverlayTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					renderer.Rectangle.Draw(Right.OverlayTexture, new Vector2(x + w - Right.OverlayTexture.Width, y), oc);
				}

				// middle overlay
				if (Program.CurrentHost.LoadTexture(ref Middle.OverlayTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					renderer.Rectangle.Draw(Middle.OverlayTexture, new Vector2(x + lw, y), new Vector2(w - lw - rw, Middle.OverlayTexture.Height), oc);
				}

				y += Element.Value2;
			}
		}
	}
}
