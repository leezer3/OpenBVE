using System;
using LibRender2.Overlays;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Math;
using OpenBveApi.Textures;
using TrainManager.Car;
using static TrainManager.TrainManagerBase;

namespace OpenBve.Graphics.Renderers
{
	internal partial class Overlays
	{
		/// <summary>Renders a HUD gauge element</summary>
		/// <param name="Element">The HUD element to render</param>
		/// <param name="x">The anchor x-coordinate in pixels</param>
		/// <param name="y">The anchor y-coordinate in pixels</param>
		private void RenderHUDGauge(HUD.Element Element, double x, double y)
		{
			if (PlayerTrain == null)
			{
				return;
			}
			double t = GetHudGaugePosition(Element);
			switch (Element.GaugeType)
			{
				case HUD.GaugeType.Needle:
					{
						Texture texture = Element.GaugeTexture;
						if (texture == null || !Program.CurrentHost.LoadTexture(ref texture, OpenGlTextureWrapMode.ClampClamp))
						{
							return;
						}
						double ox = Element.Origin.X < 0 ? 0.5 * texture.Width : Element.Origin.X;
						double oy = Element.Origin.Y < 0 ? 0.5 * texture.Height : Element.Origin.Y;
						double scale = Element.Radius > 0.0 && oy > 0.0 ? Element.Radius / oy : 1.0;
						double w = texture.Width * scale;
						double h = texture.Height * scale;
						double angle = (Element.InitialAngle + (Element.LastAngle - Element.InitialAngle) * t) * Math.PI / 180.0;
						Vector2 anchor = new Vector2(x, y);
						Vector2 topLeft = new Vector2(x - ox * scale, y - oy * scale);
						Color128 c = Element.GaugeColor;
						renderer.Rectangle.Draw(texture, topLeft, new Vector2(w, h), c, anchor, angle);
					}
					break;
				case HUD.GaugeType.LinearGauge:
					{
						Texture texture = Element.GaugeTexture;
						if (texture == null || !Program.CurrentHost.LoadTexture(ref texture, OpenGlTextureWrapMode.ClampClamp))
						{
							return;
						}
						double dx = Element.Direction.X;
						double dy = Element.Direction.Y;
						double length = Math.Sqrt(dx * dx + dy * dy);
						if (length == 0.0)
						{
							dx = 1.0;
							dy = 0.0;
						}
						else
						{
							dx /= length;
							dy /= length;
						}
						double w = texture.Width;
						double h = texture.Height;
						Vector2 topLeft = new Vector2(x + dx * t * Element.Width - 0.5 * w, y + dy * t * Element.Width - 0.5 * h);
						Color128 c = Element.GaugeColor;
						renderer.Rectangle.Draw(texture, topLeft, new Vector2(w, h), c);
					}
					break;
				case HUD.GaugeType.DigitalGauge:
					{
						double angle = (Element.InitialAngle + (Element.LastAngle - Element.InitialAngle) * t) * Math.PI / 180.0;
						double px = x + Element.Radius * Math.Sin(angle);
						double py = y - Element.Radius * Math.Cos(angle);
						string text = FormatHudGaugeValue(Element);
						Vector2 size = Element.Font.MeasureString(text);
						Vector2 location = new Vector2(px - 0.5 * size.X, py - 0.5 * size.Y);
						Program.Renderer.OpenGlString.Draw(Element.Font, text, location, TextAlignment.TopLeft, Element.TextColor, Element.TextShadow);
					}
					break;
			}
		}

		/// <summary>Returns the normalised position of a gauge between it's minimum and maximum</summary>
		private double GetHudGaugePosition(HUD.Element Element)
		{
			double value = GetHudGaugeValue(Element.Subject);
			double range = Element.Maximum - Element.Minimum;
			if (range == 0.0)
			{
				return 0.0;
			}
			double t = (value - Element.Minimum) / range;
			if (t < 0.0) t = 0.0;
			if (t > 1.0) t = 1.0;
			return t;
		}

		/// <summary>Returns the current value for a HUD subject</summary>
		private double GetHudGaugeValue(HUDSubject Subject)
		{
			switch (Subject)
			{
				case HUDSubject.Speed:
					return renderer.OptionSpeed == SpeedDisplayMode.Kmph
						? Math.Abs(PlayerTrain.CurrentSpeed) * 3.6
						: Math.Abs(PlayerTrain.CurrentSpeed) * 2.2369362920544;
				case HUDSubject.Power:
				case HUDSubject.Single:
					return PlayerTrain.Handles.Power.Actual;
				case HUDSubject.Brake:
					return PlayerTrain.Handles.Brake.Actual;
				case HUDSubject.LocoBrake:
					return PlayerTrain.Handles.HasLocoBrake ? PlayerTrain.Handles.LocoBrake.Actual : 0.0;
				case HUDSubject.Reverser:
					return (double)(int)PlayerTrain.Handles.Reverser.Actual;
				case HUDSubject.DoorsLeft:
					return (PlayerTrain.GetDoorsState(true, false) & TrainDoorState.AllClosed) != 0 ? 0.0 : 1.0;
				case HUDSubject.DoorsRight:
					return (PlayerTrain.GetDoorsState(false, true) & TrainDoorState.AllClosed) != 0 ? 0.0 : 1.0;
				case HUDSubject.Gradient:
					return Program.Renderer.CameraTrackFollower.Pitch;
				case HUDSubject.FPS:
					return Program.Renderer.FrameRate;
				default:
					return 0.0;
			}
		}

		/// <summary>Formats the value of a digital gauge</summary>
		private string FormatHudGaugeValue(HUD.Element Element)
		{
			double value = GetHudGaugeValue(Element.Subject);
			if (Element.Step > 0.0)
			{
				value = Math.Round(value / Element.Step) * Element.Step;
			}
			int decimals = 0;
			double step = Element.Step;
			while (step > 0.0 && step < 1.0)
			{
				step *= 10.0;
				decimals++;
			}
			return value.ToString("F" + decimals, System.Globalization.CultureInfo.InvariantCulture);
		}
	}
}
