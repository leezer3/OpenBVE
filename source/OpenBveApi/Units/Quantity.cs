using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using OpenBveApi.Math;

namespace OpenBveApi.Units
{
	public static class Quantity
	{
		public struct Mass
		{
			public double Value
			{
				get;
			}

			public Unit.Mass UnitValue
			{
				get;
			}

			public Mass(double value) : this(value, Unit.Mass.Kilogram)
			{
			}

			public Mass(double value, Unit.Mass unitValue)
			{
				Value = value;
				UnitValue = unitValue;
			}

			public Mass ToNewUnit(Unit.Mass newUnitValue)
			{
				if (newUnitValue == UnitValue)
				{
					return this;
				}

				double factor = 1.0;

				// Convert to SI
				switch (UnitValue)
				{
					case Unit.Mass.Kilogram:
						// Ignore
						break;
					case Unit.Mass.Gram:
						factor *= 1.0e-3;
						break;
					case Unit.Mass.Kilotonne:
						factor *= 1.0e+3 * 1.0e+3;
						break;
					case Unit.Mass.Tonne:
						factor *= 1.0e+3;
						break;
					case Unit.Mass.Kilopound:
						factor *= 0.45359237 * 1.0e+3;
						break;
					case Unit.Mass.Pound:
						factor *= 0.45359237;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				// Convert to new unit
				switch (newUnitValue)
				{
					case Unit.Mass.Kilogram:
						// Ignore
						break;
					case Unit.Mass.Gram:
						factor /= 1.0e-3;
						break;
					case Unit.Mass.Kilotonne:
						factor /= 1.0e+3 * 1.0e+3;
						break;
					case Unit.Mass.Tonne:
						factor /= 1.0e+3;
						break;
					case Unit.Mass.Kilopound:
						factor /= 0.45359237 * 1.0e+3;
						break;
					case Unit.Mass.Pound:
						factor /= 0.45359237;
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(newUnitValue), newUnitValue, null);
				}

				return new Mass(Value * factor, newUnitValue);
			}

			public XElement ToXElement(string nodeName)
			{
				return new XElement(nodeName, new XAttribute("Unit", UnitValue), Value.ToString(CultureInfo.InvariantCulture));
			}

			public bool Equals(Mass obj, bool isAbsolute)
			{
				return isAbsolute ? Value.Equals(obj.ToNewUnit(UnitValue).Value) : Equals(obj);
			}

			public static bool TryParse(XElement element, out Mass quantity)
			{
				return TryParse(element, false, out quantity);
			}

			public static bool TryParse(XElement element, bool ignoreCase, out Mass quantity)
			{
				return TryParse(element, ignoreCase, Unit.Mass.Kilogram, out quantity);
			}

			public static bool TryParse(XElement element, bool ignoreCase, Unit.Mass defaultUnitValue, out Mass quantity)
			{
				double value;
				bool isSuccessValue = NumberFormats.TryParseDoubleVb6(element.Value, out value);

				Unit.Mass unitValue = defaultUnitValue;
				XAttribute unitAttribute = element.Attributes().FirstOrDefault(x => string.Equals(x.Name.LocalName, "Unit", ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture));
				bool isSuccessUnit = true;

				if (unitAttribute != null)
				{
					isSuccessUnit = Unit.TryParse(unitAttribute.Value, ignoreCase, out unitValue);
				}

				quantity = new Mass(value, unitValue);

				return isSuccessValue && isSuccessUnit;
			}

			public static Mass Parse(XElement element)
			{
				return Parse(element, false);
			}

			public static Mass Parse(XElement element, bool ignoreCase)
			{
				return Parse(element, ignoreCase, Unit.Mass.Kilogram);
			}

			public static Mass Parse(XElement element, bool ignoreCase, Unit.Mass defaultUnitValue)
			{
				Mass quantity;

				if (!TryParse(element, ignoreCase, defaultUnitValue, out quantity))
				{
					throw new FormatException();
				}

				return quantity;
			}

			public static bool operator <(Mass left, Mass right)
			{
				return left.Value < right.ToNewUnit(left.UnitValue).Value;
			}

			public static bool operator >(Mass left, Mass right)
			{
				return left.Value > right.ToNewUnit(left.UnitValue).Value;
			}

			public static bool operator <=(Mass left, Mass right)
			{
				return left.Value <= right.ToNewUnit(left.UnitValue).Value;
			}

			public static bool operator >=(Mass left, Mass right)
			{
				return left.Value >= right.ToNewUnit(left.UnitValue).Value;
			}
		}

		public struct Length
		{
			public double Value
			{
				get;
			}

			public Unit.Length UnitValue
			{
				get;
			}

			public Length(double value) : this(value, Unit.Length.Meter)
			{
			}

			public Length(double value, Unit.Length unitValue)
			{
				Value = value;
				UnitValue = unitValue;
			}

			public Length ToNewUnit(Unit.Length newUnitValue)
			{
				if (newUnitValue == UnitValue)
				{
					return this;
				}

				double factor = 1.0;

				// Convert to SI
				switch (UnitValue)
				{
					case Unit.Length.Meter:
						// Ignore
						break;
					case Unit.Length.Centimeter:
						factor *= 1.0e-2;
						break;
					case Unit.Length.Millimeter:
						factor *= 1.0e-3;
						break;
					case Unit.Length.Yard:
						factor *= 0.0254 * 12.0 * 3.0;
						break;
					case Unit.Length.Foot:
						factor *= 0.0254 * 12.0;
						break;
					case Unit.Length.Inch:
						factor *= 0.0254;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				// Convert to new unit
				switch (newUnitValue)
				{
					case Unit.Length.Meter:
						// Ignore
						break;
					case Unit.Length.Centimeter:
						factor /= 1.0e-2;
						break;
					case Unit.Length.Millimeter:
						factor /= 1.0e-3;
						break;
					case Unit.Length.Yard:
						factor /= 0.0254 * 12.0 * 3.0;
						break;
					case Unit.Length.Foot:
						factor /= 0.0254 * 12.0;
						break;
					case Unit.Length.Inch:
						factor /= 0.0254;
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(newUnitValue), newUnitValue, null);
				}

				return new Length(Value * factor, newUnitValue);
			}

			public XElement ToXElement(string nodeName)
			{
				return new XElement(nodeName, new XAttribute("Unit", UnitValue), Value.ToString(CultureInfo.InvariantCulture));
			}

			public bool Equals(Length obj, bool isAbsolute)
			{
				return isAbsolute ? Value.Equals(obj.ToNewUnit(UnitValue).Value) : Equals(obj);
			}

			public static bool TryParse(XElement element, out Length quantity)
			{
				return TryParse(element, false, out quantity);
			}

			public static bool TryParse(XElement element, bool ignoreCase, out Length quantity)
			{
				return TryParse(element, ignoreCase, Unit.Length.Meter, out quantity);
			}

			public static bool TryParse(XElement element, bool ignoreCase, Unit.Length defaultUnitValue, out Length quantity)
			{
				double value;
				bool isSuccessValue = NumberFormats.TryParseDoubleVb6(element.Value, out value);

				Unit.Length unitValue = defaultUnitValue;
				XAttribute unitAttribute = element.Attributes().FirstOrDefault(x => string.Equals(x.Name.LocalName, "Unit", ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture));
				bool isSuccessUnit = true;

				if (unitAttribute != null)
				{
					isSuccessUnit = Unit.TryParse(unitAttribute.Value, ignoreCase, out unitValue);
				}

				quantity = new Length(value, unitValue);

				return isSuccessValue && isSuccessUnit;
			}

			public static Length Parse(XElement element)
			{
				return Parse(element, false);
			}

			public static Length Parse(XElement element, bool ignoreCase)
			{
				return Parse(element, ignoreCase, Unit.Length.Meter);
			}

			public static Length Parse(XElement element, bool ignoreCase, Unit.Length defaultUnitValue)
			{
				Length quantity;

				if (!TryParse(element, ignoreCase, defaultUnitValue, out quantity))
				{
					throw new FormatException();
				}

				return quantity;
			}

			public static bool operator <(Length left, Length right)
			{
				return left.Value < right.ToNewUnit(left.UnitValue).Value;
			}

			public static bool operator >(Length left, Length right)
			{
				return left.Value > right.ToNewUnit(left.UnitValue).Value;
			}

			public static bool operator <=(Length left, Length right)
			{
				return left.Value <= right.ToNewUnit(left.UnitValue).Value;
			}

			public static bool operator >=(Length left, Length right)
			{
				return left.Value >= right.ToNewUnit(left.UnitValue).Value;
			}
		}

		public struct Pressure
		{
			public double Value
			{
				get;
			}

			public Unit.Pressure UnitValue
			{
				get;
			}

			public Pressure(double value) : this(value, Unit.Pressure.Pascal)
			{
			}

			public Pressure(double value, Unit.Pressure unitValue)
			{
				Value = value;
				UnitValue = unitValue;
			}

			public Pressure ToNewUnit(Unit.Pressure newUnitValue)
			{
				if (newUnitValue == UnitValue)
				{
					return this;
				}

				double factor = 1.0;

				// Convert to SI
				switch (UnitValue)
				{
					case Unit.Pressure.Kilopascal:
						factor *= 1.0e+3;
						break;
					case Unit.Pressure.Pascal:
						// Ignore
						break;
					case Unit.Pressure.Bar:
						factor *= 1.0e+5;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				// Convert to new unit
				switch (newUnitValue)
				{
					case Unit.Pressure.Kilopascal:
						factor /= 1.0e+3;
						break;
					case Unit.Pressure.Pascal:
						// Ignore
						break;
					case Unit.Pressure.Bar:
						factor /= 1.0e+5;
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(newUnitValue), newUnitValue, null);
				}

				return new Pressure(Value * factor, newUnitValue);
			}

			public XElement ToXElement(string nodeName)
			{
				return new XElement(nodeName, new XAttribute("Unit", UnitValue), Value.ToString(CultureInfo.InvariantCulture));
			}

			public bool Equals(Pressure obj, bool isAbsolute)
			{
				return isAbsolute ? Value.Equals(obj.ToNewUnit(UnitValue).Value) : Equals(obj);
			}

			public static bool TryParse(XElement element, out Pressure quantity)
			{
				return TryParse(element, false, out quantity);
			}

			public static bool TryParse(XElement element, bool ignoreCase, out Pressure quantity)
			{
				return TryParse(element, ignoreCase, Unit.Pressure.Pascal, out quantity);
			}

			public static bool TryParse(XElement element, bool ignoreCase, Unit.Pressure defaultUnitValue, out Pressure quantity)
			{
				double value;
				bool isSuccessValue = NumberFormats.TryParseDoubleVb6(element.Value, out value);

				Unit.Pressure unitValue = defaultUnitValue;
				XAttribute unitAttribute = element.Attributes().FirstOrDefault(x => string.Equals(x.Name.LocalName, "Unit", ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture));
				bool isSuccessUnit = true;

				if (unitAttribute != null)
				{
					isSuccessUnit = Unit.TryParse(unitAttribute.Value, ignoreCase, out unitValue);
				}

				quantity = new Pressure(value, unitValue);

				return isSuccessValue && isSuccessUnit;
			}

			public static Pressure Parse(XElement element)
			{
				return Parse(element, false);
			}

			public static Pressure Parse(XElement element, bool ignoreCase)
			{
				return Parse(element, ignoreCase, Unit.Pressure.Pascal);
			}

			public static Pressure Parse(XElement element, bool ignoreCase, Unit.Pressure defaultUnitValue)
			{
				Pressure quantity;

				if (!TryParse(element, ignoreCase, defaultUnitValue, out quantity))
				{
					throw new FormatException();
				}

				return quantity;
			}

			public static bool operator <(Pressure left, Pressure right)
			{
				return left.Value < right.ToNewUnit(left.UnitValue).Value;
			}

			public static bool operator >(Pressure left, Pressure right)
			{
				return left.Value > right.ToNewUnit(left.UnitValue).Value;
			}

			public static bool operator <=(Pressure left, Pressure right)
			{
				return left.Value <= right.ToNewUnit(left.UnitValue).Value;
			}

			public static bool operator >=(Pressure left, Pressure right)
			{
				return left.Value >= right.ToNewUnit(left.UnitValue).Value;
			}
		}

		public struct PressureRate
		{
			public double Value
			{
				get;
			}

			public Unit.PressureRate UnitValue
			{
				get;
			}

			public PressureRate(double value) : this(value, Unit.PressureRate.PascalPerSecond)
			{
			}

			public PressureRate(double value, Unit.PressureRate unitValue)
			{
				Value = value;
				UnitValue = unitValue;
			}

			public PressureRate ToNewUnit(Unit.PressureRate newUnitValue)
			{
				if (newUnitValue == UnitValue)
				{
					return this;
				}

				double factor = 1.0;

				// Convert to SI
				switch (UnitValue)
				{
					case Unit.PressureRate.KilopascalPerSecond:
						factor *= 1.0e+3;
						break;
					case Unit.PressureRate.PascalPerSecond:
						// Ignore
						break;
					case Unit.PressureRate.BarPerSecond:
						factor *= 1.0e+5;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				// Convert to new unit
				switch (newUnitValue)
				{
					case Unit.PressureRate.KilopascalPerSecond:
						factor /= 1.0e+3;
						break;
					case Unit.PressureRate.PascalPerSecond:
						// Ignore
						break;
					case Unit.PressureRate.BarPerSecond:
						factor /= 1.0e+5;
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(newUnitValue), newUnitValue, null);
				}

				return new PressureRate(Value * factor, newUnitValue);
			}

			public XElement ToXElement(string nodeName)
			{
				return new XElement(nodeName, new XAttribute("Unit", UnitValue), Value.ToString(CultureInfo.InvariantCulture));
			}

			public bool Equals(PressureRate obj, bool isAbsolute)
			{
				return isAbsolute ? Value.Equals(obj.ToNewUnit(UnitValue).Value) : Equals(obj);
			}

			public static bool TryParse(XElement element, out PressureRate quantity)
			{
				return TryParse(element, false, out quantity);
			}

			public static bool TryParse(XElement element, bool ignoreCase, out PressureRate quantity)
			{
				return TryParse(element, ignoreCase, Unit.PressureRate.PascalPerSecond, out quantity);
			}

			public static bool TryParse(XElement element, bool ignoreCase, Unit.PressureRate defaultUnitValue, out PressureRate quantity)
			{
				double value;
				bool isSuccessValue = NumberFormats.TryParseDoubleVb6(element.Value, out value);

				Unit.PressureRate unitValue = defaultUnitValue;
				XAttribute unitAttribute = element.Attributes().FirstOrDefault(x => string.Equals(x.Name.LocalName, "Unit", ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture));
				bool isSuccessUnit = true;

				if (unitAttribute != null)
				{
					isSuccessUnit = Unit.TryParse(unitAttribute.Value, ignoreCase, out unitValue);
				}

				quantity = new PressureRate(value, unitValue);

				return isSuccessValue && isSuccessUnit;
			}

			public static PressureRate Parse(XElement element)
			{
				return Parse(element, false);
			}

			public static PressureRate Parse(XElement element, bool ignoreCase)
			{
				return Parse(element, ignoreCase, Unit.PressureRate.PascalPerSecond);
			}

			public static PressureRate Parse(XElement element, bool ignoreCase, Unit.PressureRate defaultUnitValue)
			{
				PressureRate quantity;

				if (!TryParse(element, ignoreCase, defaultUnitValue, out quantity))
				{
					throw new FormatException();
				}

				return quantity;
			}

			public static bool operator <(PressureRate left, PressureRate right)
			{
				return left.Value < right.ToNewUnit(left.UnitValue).Value;
			}

			public static bool operator >(PressureRate left, PressureRate right)
			{
				return left.Value > right.ToNewUnit(left.UnitValue).Value;
			}

			public static bool operator <=(PressureRate left, PressureRate right)
			{
				return left.Value <= right.ToNewUnit(left.UnitValue).Value;
			}

			public static bool operator >=(PressureRate left, PressureRate right)
			{
				return left.Value >= right.ToNewUnit(left.UnitValue).Value;
			}
		}
	}
}
