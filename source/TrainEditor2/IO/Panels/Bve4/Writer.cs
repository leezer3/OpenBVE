using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Panels;

namespace TrainEditor2.IO.Panels.Bve4
{
	internal static partial class PanelCfgBve4
	{
		internal static void Write(string fileName, Panel panel)
		{
			StringBuilder builder = new StringBuilder();

			WriteThisNode(fileName, builder, panel.This);

			foreach (PanelElement element in panel.PanelElements)
			{
				if (element is PilotLampElement)
				{
					WritePilotLampNode(fileName, builder, (PilotLampElement)element);
				}

				if (element is NeedleElement)
				{
					WriteNeedleNode(fileName, builder, (NeedleElement)element);
				}

				if (element is DigitalNumberElement)
				{
					WriteDigitalNumberNode(fileName, builder, (DigitalNumberElement)element);
				}

				if (element is DigitalGaugeElement)
				{
					WriteDigitalGaugeNode(builder, (DigitalGaugeElement)element);
				}

				if (element is LinearGaugeElement)
				{
					WriteLinearGaugeNode(fileName, builder, (LinearGaugeElement)element);
				}

				if (element is TimetableElement)
				{
					WriteTimetableNode(builder, (TimetableElement)element);
				}
			}

			File.WriteAllText(fileName, builder.ToString(), new UTF8Encoding(true));
		}

		private static void WriteThisNode(string fileName, StringBuilder builder, This element)
		{
			builder.AppendLine("[This]");
			WriteKey(builder, "Resolution", element.Resolution);
			WriteKey(builder, "Left", element.Left);
			WriteKey(builder, "Right", element.Right);
			WriteKey(builder, "Top", element.Top);
			WriteKey(builder, "Bottom", element.Bottom);
			WriteKey(builder, "DaytimeImage", Utilities.MakeRelativePath(fileName, element.DaytimeImage));
			WriteKey(builder, "NighttimeImage", Utilities.MakeRelativePath(fileName, element.NighttimeImage));
			WriteKey(builder, "TransparentColor", element.TransparentColor.ToString());
			WriteKey(builder, "Center", element.CenterX, element.CenterY);
			WriteKey(builder, "Origin", element.OriginX, element.CenterY);
		}

		private static void WritePilotLampNode(string fileName, StringBuilder builder, PilotLampElement element)
		{
			builder.AppendLine("[PilotLamp]");
			WriteKey(builder, "Subject", element.Subject.ToString());
			WriteKey(builder, "Location", element.LocationX, element.LocationY);
			WriteKey(builder, "DaytimeImage", Utilities.MakeRelativePath(fileName, element.DaytimeImage));
			WriteKey(builder, "NighttimeImage", Utilities.MakeRelativePath(fileName, element.NighttimeImage));
			WriteKey(builder, "TransparentColor", element.TransparentColor.ToString());
			WriteKey(builder, "Layer", element.Layer);
		}

		private static void WriteNeedleNode(string fileName, StringBuilder builder, NeedleElement element)
		{
			builder.AppendLine("[Needle]");
			WriteKey(builder, "Subject", element.Subject.ToString());
			WriteKey(builder, "Location", element.LocationX, element.LocationY);

			if (element.DefinedRadius)
			{
				WriteKey(builder, "Radius", element.Radius);
			}

			WriteKey(builder, "DaytimeImage", Utilities.MakeRelativePath(fileName, element.DaytimeImage));
			WriteKey(builder, "NighttimeImage", Utilities.MakeRelativePath(fileName, element.NighttimeImage));
			WriteKey(builder, "Color", element.Color.ToString());
			WriteKey(builder, "TransparentColor", element.TransparentColor.ToString());

			if (element.DefinedOrigin)
			{
				WriteKey(builder, "Origin", element.OriginX, element.OriginY);
			}

			WriteKey(builder, "InitialAngle", element.InitialAngle.ToDegrees());
			WriteKey(builder, "LastAngle", element.LastAngle.ToDegrees());
			WriteKey(builder, "Minimum", element.Minimum);
			WriteKey(builder, "Maximum", element.Maximum);

			if (element.DefinedNaturalFreq)
			{
				WriteKey(builder, "NaturalFreq", element.NaturalFreq);
			}

			if (element.DefinedDampingRatio)
			{
				WriteKey(builder, "DampingRatio", element.DampingRatio);
			}

			WriteKey(builder, "Backstop", element.Backstop.ToString());
			WriteKey(builder, "Smoothed", element.Smoothed.ToString());
			WriteKey(builder, "Layer", element.Layer);
		}

		private static void WriteDigitalNumberNode(string fileName, StringBuilder builder, DigitalNumberElement element)
		{
			builder.AppendLine("[DigitalNumber]");
			WriteKey(builder, "Subject", element.Subject.ToString());
			WriteKey(builder, "Location", element.LocationX, element.LocationY);
			WriteKey(builder, "DaytimeImage", Utilities.MakeRelativePath(fileName, element.DaytimeImage));
			WriteKey(builder, "NighttimeImage", Utilities.MakeRelativePath(fileName, element.NighttimeImage));
			WriteKey(builder, "TransparentColor", element.TransparentColor.ToString());
			WriteKey(builder, "Interval", element.Interval);
			WriteKey(builder, "Layer", element.Layer);
		}

		private static void WriteDigitalGaugeNode(StringBuilder builder, DigitalGaugeElement element)
		{
			builder.AppendLine("[DigitalGauge]");
			WriteKey(builder, "Subject", element.Subject.ToString());
			WriteKey(builder, "Location", element.LocationX, element.LocationY);
			WriteKey(builder, "Radius", element.Radius);
			WriteKey(builder, "Color", element.Color.ToString());
			WriteKey(builder, "InitialAngle", element.InitialAngle.ToDegrees());
			WriteKey(builder, "LastAngle", element.LastAngle.ToDegrees());
			WriteKey(builder, "Minimum", element.Minimum);
			WriteKey(builder, "Maximum", element.Maximum);
			WriteKey(builder, "Step", element.Step);
			WriteKey(builder, "Layer", element.Layer);
		}

		private static void WriteLinearGaugeNode(string fileName, StringBuilder builder, LinearGaugeElement element)
		{
			builder.AppendLine("[LinearGauge]");
			WriteKey(builder, "Subject", element.Subject.ToString());
			WriteKey(builder, "Location", element.LocationX, element.LocationY);
			WriteKey(builder, "Minimum", element.Minimum);
			WriteKey(builder, "Maximum", element.Maximum);
			WriteKey(builder, "Location", element.LocationX, element.LocationY);
			WriteKey(builder, "DaytimeImage", Utilities.MakeRelativePath(fileName, element.DaytimeImage));
			WriteKey(builder, "NighttimeImage", Utilities.MakeRelativePath(fileName, element.NighttimeImage));
			WriteKey(builder, "Width", element.Width);
			WriteKey(builder, "Layer", element.Layer);
		}

		private static void WriteTimetableNode(StringBuilder builder, TimetableElement element)
		{
			builder.AppendLine("[Timetable]");
			WriteKey(builder, "Location", element.LocationX, element.LocationY);
			WriteKey(builder, "Width", element.Width);
			WriteKey(builder, "Height", element.Height);
			WriteKey(builder, "TransparentColor", element.TransparentColor.ToString());
			WriteKey(builder, "Layer", element.Layer);
		}

		private static void WriteKey(StringBuilder builder, string key, params string[] values)
		{
			if (values.All(string.IsNullOrEmpty))
			{
				return;
			}

			builder.AppendLine($"{key} = {string.Join(", ", values)}");
		}

		private static void WriteKey(StringBuilder builder, string key, params int[] values)
		{
			WriteKey(builder, key, values.Select(v => v.ToString(CultureInfo.InvariantCulture)).ToArray());
		}

		private static void WriteKey(StringBuilder builder, string key, params double[] values)
		{
			WriteKey(builder, key, values.Select(v => v.ToString(CultureInfo.InvariantCulture)).ToArray());
		}
	}
}
