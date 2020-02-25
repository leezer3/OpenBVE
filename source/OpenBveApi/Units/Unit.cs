using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenBveApi.Units
{
	public static class Unit
	{
		public enum Mass
		{
			Kilogram,
			Gram,
			Kilotonne,
			Tonne,
			Kilopound,
			Pound
		}

		public enum Length
		{
			Meter,
			Centimeter,
			Millimeter,
			Yard,
			Foot,
			Inch
		}

		public enum Pressure
		{
			Kilopascal,
			Pascal,
			Bar
		}

		public enum PressureRate
		{
			KilopascalPerSecond,
			PascalPerSecond,
			BarPerSecond
		}

		private struct TypeValuePair
		{
			internal Type Type
			{
				get;
			}

			internal Enum Value
			{
				get;
			}

			internal TypeValuePair(Type type, Enum value)
			{
				Type = type;
				Value = value;
			}
		}

		private static readonly Dictionary<Type, Enum> defaultDictionary = new Dictionary<Type, Enum>();
		private static readonly Dictionary<TypeValuePair, List<string>> rewordsDictionary = new Dictionary<TypeValuePair, List<string>>();

		static Unit()
		{
			defaultDictionary.Add(typeof(Mass), Mass.Kilogram);
			AddReword(Mass.Kilogram, "kg");
			AddReword(Mass.Gram, "g");
			AddReword(Mass.Kilotonne, "kt");
			AddReword(Mass.Tonne, "t");
			AddReword(Mass.Kilopound, "klb");
			AddReword(Mass.Pound, "lb");

			defaultDictionary.Add(typeof(Length), Length.Meter);
			AddReword(Length.Meter, "m");
			AddReword(Length.Centimeter, "cm");
			AddReword(Length.Millimeter, "mm");
			AddReword(Length.Yard, "yd");
			AddReword(Length.Foot, "ft");
			AddReword(Length.Inch, "in");

			defaultDictionary.Add(typeof(Pressure), Pressure.Pascal);
			AddReword(Pressure.Kilopascal, "kPa");
			AddReword(Pressure.Pascal, "Pa");
			AddReword(Pressure.Bar, "bar");

			defaultDictionary.Add(typeof(PressureRate), PressureRate.PascalPerSecond);
			AddReword(PressureRate.KilopascalPerSecond, "kPa/s");
			AddReword(PressureRate.PascalPerSecond, "Pa/s");
			AddReword(PressureRate.BarPerSecond, "bar/s");
		}

		private static void AddReword(Enum unitValue, params string[] rewords)
		{
			TypeValuePair pair = new TypeValuePair(unitValue.GetType(), unitValue);

			if (!rewordsDictionary.ContainsKey(pair))
			{
				rewordsDictionary.Add(pair, new List<string>());
			}

			rewordsDictionary[pair].AddRange(rewords);
		}

		public static bool TryParse<T>(string text, bool ignoreCase, out T unitValue) where T : struct
		{
			if (Enum.TryParse(text, ignoreCase, out unitValue))
			{
				return true;
			}

			Type unitType = typeof(T);

			Enum[] rewords = rewordsDictionary.Where(x => x.Key.Type == unitType && x.Value.Any(y => string.Equals(y, text, ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture))).Select(x => x.Key.Value).ToArray();

			if (rewords.Any())
			{
				unitValue = (T)(object)rewords.First();
				return true;
			}

			unitValue = defaultDictionary.ContainsKey(unitType) ? (T)(object)defaultDictionary[unitType] : default(T);
			return false;
		}

		public static T Parse<T>(string text) where T : struct
		{
			return Parse<T>(text, false);
		}

		public static T Parse<T>(string text, bool ignoreCase) where T : struct
		{
			T unitValue;

			if (!TryParse(text, ignoreCase, out unitValue))
			{
				throw new FormatException();
			}

			return unitValue;
		}

		public static string[] GetRewords(Enum unitValue)
		{
			return rewordsDictionary[new TypeValuePair(unitValue.GetType(), unitValue)].ToArray();
		}
	}
}
