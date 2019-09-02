using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using OpenBveApi.Colors;
using OpenBveApi.Interface;

namespace TrainEditor2.Extensions
{
	internal enum NumberRange
	{
		Any,
		Positive,
		NonNegative,
		NonZero
	}

	internal static class Utilities
	{
		internal const double HueFactor = 0.785398163397448;

		internal static string GetInterfaceString(params string[] ids)
		{
			return Translations.GetInterfaceString($"train_editor2_{string.Join("_", ids)}");
		}

		internal static bool TryParse(string text, NumberRange range, out double result)
		{
			bool error;

			if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
			{
				switch (range)
				{
					case NumberRange.Positive:
						error = result <= 0.0;
						break;
					case NumberRange.NonNegative:
						error = result < 0.0;
						break;
					case NumberRange.NonZero:
						error = result == 0.0;
						break;
					default:
						error = false;
						break;
				}
			}
			else
			{
				error = true;
			}

			return !error;
		}

		internal static bool TryParse(string text, NumberRange range, out double result, out string message)
		{
			if (!TryParse(text, range, out result))
			{
				string prefix;

				switch (range)
				{
					case NumberRange.Positive:
						prefix = $"{GetInterfaceString("message", "positive")} ";
						break;
					case NumberRange.NonNegative:
						prefix = $"{GetInterfaceString("message", "non_negative")} ";
						break;
					case NumberRange.NonZero:
						prefix = $"{GetInterfaceString("message", "non_zero")} ";
						break;
					default:
						prefix = string.Empty;
						break;
				}

				message = string.Format(GetInterfaceString("message", "invalid_float"), prefix);
				return false;
			}

			message = null;
			return true;
		}

		internal static bool TryParse(string text, out Color24 result, out string message)
		{
			if (!Color24.TryParseHexColor(text, out result))
			{
				message = string.Format(GetInterfaceString("message", "invalid_color"), "This");
				return false;
			}

			message = null;
			return true;
		}

		internal static double ToDegrees(this double radians)
		{
			return radians * (180.0 / Math.PI);
		}

		internal static void ZoomIn(ref double min, ref double max)
		{
			double range = max - min;
			double center = 0.5 * (min + max);
			double radius = 0.5 * range;

			min = center - 0.8 * radius;
			max = center + 0.8 * radius;
		}

		internal static void ZoomOut(ref double min, ref double max)
		{
			double range = max - min;
			double center = 0.5 * (min + max);
			double radius = 0.5 * range;

			min = center - 1.25 * radius;

			if (min < 0.0)
			{
				min = 0.0;
				max = 2.25 * radius;
			}
			else
			{
				max = center + 1.25 * radius;
			}
		}

		internal static void Reset(double radius, ref double min, ref double max)
		{
			double center = 0.5 * (min + max);

			min = center - radius;

			if (min < 0.0)
			{
				min = 0.0;
			}

			max = min + 2.0 * radius;
		}

		internal static void MoveNegative(ref double min, ref double max)
		{
			double range = max - min;

			min -= 0.1 * range;

			if (min < 0.0)
			{
				min = 0.0;
			}

			max = min + range;
		}

		internal static void MovePositive(ref double min, ref double max)
		{
			double range = max - min;

			min += 0.1 * range;
			max = min + range;
		}

		internal static Color GetColor(double Hue, bool Selected)
		{
			double r, g, b;

			if (Hue < 0.0)
			{
				r = 0.0; g = 0.0; b = 0.0;
			}
			else if (Hue <= 0.166666666666667)
			{
				double x = 6.0 * Hue;
				r = 1.0; g = x; b = 0.0;
			}
			else if (Hue <= 0.333333333333333)
			{
				double x = 6.0 * Hue - 1.0;
				r = 1.0 - x; g = 1.0; b = 0.0;
			}
			else if (Hue <= 0.5)
			{
				double x = 6.0 * Hue - 2.0;
				r = 0.0; g = 1.0; b = x;
			}
			else if (Hue <= 0.666666666666667)
			{
				double x = 6.0 * Hue - 3.0;
				r = 0.0; g = 1.0 - x; b = 1.0;
			}
			else if (Hue <= 0.833333333333333)
			{
				double x = 6.0 * Hue - 4.0;
				r = x; g = 0.0; b = 1.0;
			}
			else if (Hue <= 1.0)
			{
				double x = 6.0 * Hue - 5.0;
				r = 1.0; g = 0.0; b = 1.0 - x;
			}
			else
			{
				r = 1.0; g = 1.0; b = 1.0;
			}

			if (r < 0.0)
			{
				r = 0.0;
			}
			else if (r > 1.0)
			{
				r = 1.0;
			}

			if (g < 0.0)
			{
				g = 0.0;
			}
			else if (g > 1.0)
			{
				g = 1.0;
			}

			if (b < 0.0)
			{
				b = 0.0;
			}
			else if (b > 1.0)
			{
				b = 1.0;
			}

			if (!Selected)
			{
				r *= 0.6;
				g *= 0.6;
				b *= 0.6;
			}

			return Color.FromArgb((int)Math.Round(255.0 * r), (int)Math.Round(255.0 * g), (int)Math.Round(255.0 * b));
		}

		internal static void RemoveAll<T>(this ICollection<T> collection, Func<T, bool> match)
		{
			foreach (T item in collection.Where(match).ToArray())
			{
				collection.Remove(item);
			}
		}

		internal static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> addCollection)
		{
			foreach (T add in addCollection)
			{
				collection.Add(add);
			}
		}

		internal static string MakeRelativePath(string baseFile, string targetFile)
		{
			if (string.IsNullOrEmpty(baseFile) || string.IsNullOrEmpty(targetFile))
			{
				return string.Empty;
			}

			Uri basePathUri = new Uri($@"{Path.GetDirectoryName(baseFile)}\".Replace("%", "%25"));
			Uri targetPathUri = new Uri(targetFile.Replace("%", "%25"));

			string relativePath = basePathUri.MakeRelativeUri(targetPathUri).ToString();

			return Uri.UnescapeDataString(relativePath).Replace("%25", "%");
		}
	}
}
