using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenBveApi.World
{
	public static class Unit
	{
		public enum Acceleration
		{
			KilometerPerHourPerSecond,
			MeterPerSecondSquared
		}
		public enum Jerk
		{
			MeterPerSecondCubed,
			CentimeterPerSecondCubed,
			FootPerSecondCubed
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

		public enum Time
		{
			Second,
			Millisecond
		}

		public enum Velocity
		{
			KilometerPerHour,
			MeterPerSecond
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
			defaultDictionary.Add(typeof(UnitOfArea), UnitOfArea.SquareMeter);
			AddReword(UnitOfArea.SquareMeter, "m²");
			AddReword(UnitOfArea.SquareCentimeter, "cm²");
			AddReword(UnitOfArea.SquareFoot, "ft²");

			defaultDictionary.Add(typeof(Acceleration), Acceleration.MeterPerSecondSquared);
			AddReword(Acceleration.KilometerPerHourPerSecond, "km/h/s");
			AddReword(Acceleration.MeterPerSecondSquared, "m/s²");

			defaultDictionary.Add(typeof(Jerk), Jerk.MeterPerSecondCubed);
			AddReword(Jerk.MeterPerSecondCubed, "m/s³");
			AddReword(Jerk.CentimeterPerSecondCubed, "cm/s³");
			AddReword(Jerk.FootPerSecondCubed, "ft/s³");

			defaultDictionary.Add(typeof(UnitOfLength), UnitOfLength.Meter);
			AddReword(UnitOfLength.Meter, "m");
			AddReword(UnitOfLength.Centimeter, "cm");
			AddReword(UnitOfLength.Millimeter, "mm");
			AddReword(UnitOfLength.Yard, "yd");
			AddReword(UnitOfLength.Feet, "ft");
			AddReword(UnitOfLength.Inches, "in");

			defaultDictionary.Add(typeof(UnitOfWeight), UnitOfWeight.Kilograms);
			AddReword(UnitOfWeight.Kilograms, "kg");
			AddReword(UnitOfWeight.Grams, "g");
			AddReword(UnitOfWeight.MetricTonnes, "t");
			AddReword(UnitOfWeight.Pounds, "lb");

			defaultDictionary.Add(typeof(Pressure), Pressure.Pascal);
			AddReword(Pressure.Kilopascal, "kPa");
			AddReword(Pressure.Pascal, "Pa");
			AddReword(Pressure.Bar, "bar");

			defaultDictionary.Add(typeof(PressureRate), PressureRate.PascalPerSecond);
			AddReword(PressureRate.KilopascalPerSecond, "kPa/s");
			AddReword(PressureRate.PascalPerSecond, "Pa/s");
			AddReword(PressureRate.BarPerSecond, "bar/s");

			defaultDictionary.Add(typeof(Time), Time.Second);
			AddReword(Time.Second, "s");
			AddReword(Time.Millisecond, "ms");

			defaultDictionary.Add(typeof(Velocity), Velocity.MeterPerSecond);
			AddReword(Velocity.KilometerPerHour, "km/h");
			AddReword(Velocity.MeterPerSecond, "m/s");
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


		public static string[] GetAllRewords<T>() where T : struct
		{
			Type type = typeof(T);
			return rewordsDictionary.Where(x => x.Key.Type == type).Select(x => x.Value.First()).ToArray();
		}
	}
}
