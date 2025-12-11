using Formats.OpenBve;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using TrainEditor2.Models.Panels;
using Path = OpenBveApi.Path;

namespace TrainEditor2.IO.Panels.Bve4
{
	internal static partial class PanelCfgBve4
	{
		internal static void Parse(string fileName, out Panel panel)
		{
			panel = new Panel();
			ConfigFile<Panel2Sections, Panel2Key> cfg = new ConfigFile<Panel2Sections, Panel2Key>(fileName, Program.CurrentHost);
			string trainFolder = Path.GetDirectoryName(fileName);

			while (cfg.RemainingSubBlocks > 0)
			{
				Block<Panel2Sections, Panel2Key> block = cfg.ReadNextBlock();
				switch (block.Key)
				{
					case Panel2Sections.This:
						block.GetPath(Panel2Key.DaytimeImage, trainFolder, out string daytimeImage);
						panel.This.DaytimeImage = daytimeImage;
						block.GetPath(Panel2Key.NighttimeImage, trainFolder, out string nighttimeImage);
						panel.This.NighttimeImage = nighttimeImage;
						block.GetColor24(Panel2Key.TransparentColor, out Color24 transparentColor);
						panel.This.TransparentColor = transparentColor;
						block.GetValue(Panel2Key.Resolution, out double resolution);
						panel.This.Resolution = resolution;
						block.GetValue(Panel2Key.Left, out double left);
						panel.This.Left = left;
						block.GetValue(Panel2Key.Right, out double right);
						panel.This.Right = right;
						block.GetValue(Panel2Key.Top, out double top);
						panel.This.Top = top;
						block.GetValue(Panel2Key.Bottom, out double bottom);
						panel.This.Bottom = bottom;
						block.GetVector2(Panel2Key.Center, ',', out Vector2 center);
						panel.This.Center = center;
						block.GetVector2(Panel2Key.Origin, ',', out Vector2 origin);
						panel.This.Origin = origin;
						break;
					case Panel2Sections.PilotLamp:
						PilotLampElement pilotLamp = new PilotLampElement();
						block.GetPath(Panel2Key.DaytimeImage, trainFolder, out daytimeImage);
						pilotLamp.DaytimeImage = daytimeImage;
						block.GetPath(Panel2Key.NighttimeImage, trainFolder, out nighttimeImage);
						pilotLamp.NighttimeImage = nighttimeImage;
						block.GetColor24(Panel2Key.TransparentColor, out transparentColor);
						pilotLamp.TransparentColor = transparentColor;
						block.GetVector2(Panel2Key.Location, ',', out Vector2 location);
						pilotLamp.Location = location;
						block.GetValue(Panel2Key.Layer, out int layer);
						pilotLamp.Layer = layer;
						block.GetValue(Panel2Key.Subject, out string subject);
						pilotLamp.Subject = Subject.StringToSubject(subject, Panel2Sections.PilotLamp);
						panel.PanelElements.Add(pilotLamp);
						break;
					case Panel2Sections.Needle:
						NeedleElement needle = new NeedleElement();
						block.GetPath(Panel2Key.DaytimeImage, trainFolder, out daytimeImage);
						needle.DaytimeImage = daytimeImage;
						block.GetPath(Panel2Key.NighttimeImage, trainFolder, out nighttimeImage);
						needle.NighttimeImage = nighttimeImage;
						block.GetColor24(Panel2Key.TransparentColor, out transparentColor);
						needle.TransparentColor = transparentColor;
						block.GetColor24(Panel2Key.Color, out Color24 color);
						needle.Color = color;
						block.GetVector2(Panel2Key.Location, ',', out location);
						needle.Location = location;
						if (block.GetVector2(Panel2Key.Origin, ',', out origin))
						{
							needle.Origin = origin;
							needle.DefinedOrigin = true;
						}
						if (block.GetValue(Panel2Key.Radius, out double radius))
						{
							needle.Radius = radius;
							needle.DefinedRadius = true;
						}
						if (block.GetValue(Panel2Key.InitialAngle, out double initialAngle))
						{
							needle.InitialAngle = initialAngle.ToRadians();
						}
						if (block.GetValue(Panel2Key.LastAngle, out double lastAngle))
						{
							needle.LastAngle = lastAngle.ToRadians();
						}
						if (block.GetValue(Panel2Key.Minimum, out double minimum))
						{
							needle.Minimum = minimum;
						}
						if (block.GetValue(Panel2Key.Maximum, out double maxiumum))
						{
							needle.Maximum = maxiumum;
						}
						double naturalFrequency = -1.0;
						if (block.TryGetValue(Panel2Key.NaturalFreq, ref naturalFrequency) && naturalFrequency < 0)
						{
							naturalFrequency = -naturalFrequency;
							Program.CurrentHost.AddMessage(MessageType.Error, false, "NaturalFrequency is expected to be non-negative in [Needle] in " + fileName);
						}
						needle.NaturalFreq = naturalFrequency;
						double dampingRatio = -1.0;
						if (block.TryGetValue(Panel2Key.DampingRatio, ref dampingRatio) && dampingRatio < 0)
						{
							dampingRatio = -dampingRatio;
							Program.CurrentHost.AddMessage(MessageType.Error, false, "DampingRatio is expected to be non-negative in [Needle] in " + fileName);
						}
						needle.DampingRatio = dampingRatio;
						block.GetValue(Panel2Key.Backstop, out bool backstop);
						needle.Backstop = backstop;
						block.GetValue(Panel2Key.Smoothed, out bool smoothed);
						needle.Smoothed = smoothed;
						block.GetValue(Panel2Key.Layer, out layer);
						needle.Layer = layer;
						block.GetValue(Panel2Key.Subject, out subject);
						needle.Subject = Subject.StringToSubject(subject, Panel2Sections.Needle);
						panel.PanelElements.Add(needle);
						break;
					case Panel2Sections.LinearGauge:
						LinearGaugeElement linearGauge = new LinearGaugeElement();
						block.GetPath(Panel2Key.DaytimeImage, trainFolder, out daytimeImage);
						linearGauge.DaytimeImage = daytimeImage;
						block.GetPath(Panel2Key.NighttimeImage, trainFolder, out nighttimeImage);
						linearGauge.NighttimeImage = nighttimeImage;
						block.GetColor24(Panel2Key.TransparentColor, out transparentColor);
						linearGauge.TransparentColor = transparentColor;
						block.GetVector2(Panel2Key.Location, ',', out location);
						linearGauge.Location = location;
						if (block.GetValue(Panel2Key.Minimum, out minimum))
						{
							linearGauge.Minimum = minimum;
						}
						if (block.GetValue(Panel2Key.Maximum, out maxiumum))
						{
							linearGauge.Maximum = maxiumum;
						}
						block.GetValue(Panel2Key.Width, out int width);
						linearGauge.Width = width;
						block.GetVector2(Panel2Key.Direction, ',', out Vector2 direction);
						linearGauge.Direction = direction;
						block.GetValue(Panel2Key.Layer, out layer);
						linearGauge.Layer = layer;
						block.GetValue(Panel2Key.Subject, out subject);
						linearGauge.Subject = Subject.StringToSubject(subject, Panel2Sections.LinearGauge);
						panel.PanelElements.Add(linearGauge);
						break;
					case Panel2Sections.DigitalNumber:
						DigitalNumberElement digitalNumber = new DigitalNumberElement();
						block.GetPath(Panel2Key.DaytimeImage, trainFolder, out daytimeImage);
						digitalNumber.DaytimeImage = daytimeImage;
						block.GetPath(Panel2Key.NighttimeImage, trainFolder, out nighttimeImage);
						digitalNumber.NighttimeImage = nighttimeImage;
						block.GetColor24(Panel2Key.TransparentColor, out transparentColor);
						digitalNumber.TransparentColor = transparentColor;
						block.GetVector2(Panel2Key.Location, ',', out location);
						digitalNumber.Location = location;
						block.GetValue(Panel2Key.Interval, out int interval);
						digitalNumber.Interval = interval;
						block.GetValue(Panel2Key.Layer, out layer);
						digitalNumber.Layer = layer;
						block.GetValue(Panel2Key.Subject, out subject);
						digitalNumber.Subject = Subject.StringToSubject(subject, Panel2Sections.DigitalNumber);
						panel.PanelElements.Add(digitalNumber);
						break;
					case Panel2Sections.DigitalGauge:
						DigitalGaugeElement digitalGauge = new DigitalGaugeElement();
						block.GetVector2(Panel2Key.Location, ',', out location);
						digitalGauge.Location = location;
						block.GetValue(Panel2Key.Radius, out radius);
						digitalGauge.Radius = radius;
						block.GetColor24(Panel2Key.Color, out color);
						digitalGauge.Color = color;
						if (block.GetValue(Panel2Key.InitialAngle, out initialAngle))
						{
							digitalGauge.InitialAngle = initialAngle.ToRadians();
						}
						if (block.GetValue(Panel2Key.LastAngle, out lastAngle))
						{
							digitalGauge.LastAngle = lastAngle.ToRadians();
						}
						if (block.GetValue(Panel2Key.Minimum, out minimum))
						{
							digitalGauge.Minimum = minimum;
						}
						if (block.GetValue(Panel2Key.Maximum, out maxiumum))
						{
							digitalGauge.Maximum = maxiumum;
						}

						block.GetValue(Panel2Key.Step, out double step);
						digitalGauge.Step = step;
						block.GetValue(Panel2Key.Layer, out layer);
						digitalGauge.Layer = layer;
						block.GetValue(Panel2Key.Subject, out subject);
						digitalGauge.Subject = Subject.StringToSubject(subject, Panel2Sections.DigitalGauge);
						panel.PanelElements.Add(digitalGauge);
						break;
					case Panel2Sections.Timetable:
						TimetableElement timetable = new TimetableElement();
						block.GetVector2(Panel2Key.Location, ',', out location);
						timetable.Location = location;
						block.GetValue(Panel2Key.Width, out width, NumberRange.Positive);
						timetable.Width = width;
						block.GetValue(Panel2Key.Height, out double height, NumberRange.Positive);
						timetable.Height = height;
						block.GetColor24(Panel2Key.TransparentColor, out transparentColor);
						timetable.TransparentColor = transparentColor;
						block.GetValue(Panel2Key.Layer, out layer);
						timetable.Layer = layer;
						panel.PanelElements.Add(timetable);
						break;
				}
			}
		}
	}
}
