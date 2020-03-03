using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using OpenBveApi.Math;

namespace OpenBveApi.Units
{
	public static class Quantity
	{
		public struct Acceleration
		{
			public double Value
			{
				get;
			}

			public Unit.Acceleration UnitValue
			{
				get;
			}

			public Acceleration(double value) : this(value, Unit.Acceleration.MeterPerSecondSquared)
			{
			}

			public Acceleration(double value, Unit.Acceleration unitValue)
			{
				Value = value;
				UnitValue = unitValue;
			}

			public Acceleration ToDefaultUnit()
			{
				return ToNewUnit(Unit.Acceleration.MeterPerSecondSquared);
			}

			public Acceleration ToNewUnit(Unit.Acceleration newUnitValue)
			{
				if (newUnitValue == UnitValue)
				{
					return this;
				}

				double factor = 1.0;

				// Convert to SI
				switch (UnitValue)
				{
					case Unit.Acceleration.KilometerPerHourPerSecond:
						factor /= 3.6;
						break;
					case Unit.Acceleration.MeterPerSecondSquared:
						// Ignore
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				// Convert to new unit
				switch (newUnitValue)
				{
					case Unit.Acceleration.KilometerPerHourPerSecond:
						factor *= 3.6;
						break;
					case Unit.Acceleration.MeterPerSecondSquared:
						// Ignore
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(newUnitValue), newUnitValue, null);
				}

				return new Acceleration(Value * factor, newUnitValue);
			}

			public XElement ToXElement(string nodeName)
			{
				return new XElement(nodeName, new XAttribute("Unit", UnitValue), Value.ToString(CultureInfo.InvariantCulture));
			}

			public bool Equals(Acceleration obj, bool isAbsolute)
			{
				return isAbsolute ? Value.Equals(obj.ToNewUnit(UnitValue).Value) : Equals(obj);
			}

			public static bool TryParse(XElement element, out Acceleration quantity)
			{
				return TryParse(element, false, out quantity);
			}

			public static bool TryParse(XElement element, bool ignoreCase, out Acceleration quantity)
			{
				return TryParse(element, ignoreCase, Unit.Acceleration.MeterPerSecondSquared, out quantity);
			}

			public static bool TryParse(XElement element, bool ignoreCase, Unit.Acceleration defaultUnitValue, out Acceleration quantity)
			{
				double value;
				bool isSuccessValue = NumberFormats.TryParseDoubleVb6(element.Value, out value);

				Unit.Acceleration unitValue = defaultUnitValue;
				XAttribute unitAttribute = element.Attributes().FirstOrDefault(x => string.Equals(x.Name.LocalName, "Unit", ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture));
				bool isSuccessUnit = true;

				if (unitAttribute != null)
				{
					isSuccessUnit = Unit.TryParse(unitAttribute.Value, ignoreCase, out unitValue);
				}

				quantity = new Acceleration(value, unitValue);

				return isSuccessValue && isSuccessUnit;
			}

			public static Acceleration Parse(XElement element)
			{
				return Parse(element, false);
			}

			public static Acceleration Parse(XElement element, bool ignoreCase)
			{
				return Parse(element, ignoreCase, Unit.Acceleration.MeterPerSecondSquared);
			}

			public static Acceleration Parse(XElement element, bool ignoreCase, Unit.Acceleration defaultUnitValue)
			{
				Acceleration quantity;

				if (!TryParse(element, ignoreCase, defaultUnitValue, out quantity))
				{
					throw new FormatException();
				}

				return quantity;
			}

			#region operators

			public static Acceleration operator +(Acceleration left, Acceleration right)
			{
				return new Acceleration(left.Value + right.ToNewUnit(left.UnitValue).Value, left.UnitValue);
			}

			public static Acceleration operator -(Acceleration left, Acceleration right)
			{
				return new Acceleration(left.Value - right.ToNewUnit(left.UnitValue).Value, left.UnitValue);
			}

			public static Acceleration operator *(Acceleration left, Acceleration right)
			{
				return new Acceleration(left.Value * right.ToNewUnit(left.UnitValue).Value, left.UnitValue);
			}

			public static double operator /(Acceleration left, Acceleration right)
			{
				return left.Value / right.ToNewUnit(left.UnitValue).Value;
			}

			public static Acceleration operator *(Acceleration left, double right)
			{
				return new Acceleration(left.Value * right, left.UnitValue);
			}

			public static Acceleration operator /(Acceleration left, double right)
			{
				return new Acceleration(left.Value / right, left.UnitValue);
			}

			public static Acceleration operator *(double left, Acceleration right)
			{
				return new Acceleration(left * right.Value, right.UnitValue);
			}

			public static bool operator <(Acceleration left, Acceleration right)
			{
				return left.Value < right.ToNewUnit(left.UnitValue).Value;
			}

			public static bool operator >(Acceleration left, Acceleration right)
			{
				return left.Value > right.ToNewUnit(left.UnitValue).Value;
			}

			public static bool operator <=(Acceleration left, Acceleration right)
			{
				return left.Value <= right.ToNewUnit(left.UnitValue).Value;
			}

			public static bool operator >=(Acceleration left, Acceleration right)
			{
				return left.Value >= right.ToNewUnit(left.UnitValue).Value;
			}

			#endregion
		}

		public struct Area
		{
			public double Value
			{
				get;
			}

			public Unit.Area UnitValue
			{
				get;
			}

			public Area(double value) : this(value, Unit.Area.SquareMeter)
			{
			}

			public Area(double value, Unit.Area unitValue)
			{
				Value = value;
				UnitValue = unitValue;
			}

			public Area ToDefaultUnit()
			{
				return ToNewUnit(Unit.Area.SquareMeter);
			}

			public Area ToNewUnit(Unit.Area newUnitValue)
			{
				if (newUnitValue == UnitValue)
				{
					return this;
				}

				double factor = 1.0;

				// Convert to SI
				switch (UnitValue)
				{
					case Unit.Area.SquareMeter:
						// Ignore
						break;
					case Unit.Area.SquareCentimeter:
						factor *= System.Math.Pow(1.0e-2, 2.0);
						break;
					case Unit.Area.SquareFoot:
						factor *= System.Math.Pow(0.0254 * 12.0, 2.0);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				// Convert to new unit
				switch (newUnitValue)
				{
					case Unit.Area.SquareMeter:
						// Ignore
						break;
					case Unit.Area.SquareCentimeter:
						factor /= System.Math.Pow(1.0e-2, 2.0);
						break;
					case Unit.Area.SquareFoot:
						factor /= System.Math.Pow(0.0254 * 12.0, 2.0);
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(newUnitValue), newUnitValue, null);
				}

				return new Area(Value * factor, newUnitValue);
			}

			public XElement ToXElement(string nodeName)
			{
				return new XElement(nodeName, new XAttribute("Unit", UnitValue), Value.ToString(CultureInfo.InvariantCulture));
			}

			public bool Equals(Area obj, bool isAbsolute)
			{
				return isAbsolute ? Value.Equals(obj.ToNewUnit(UnitValue).Value) : Equals(obj);
			}

			public static bool TryParse(XElement element, out Area quantity)
			{
				return TryParse(element, false, out quantity);
			}

			public static bool TryParse(XElement element, bool ignoreCase, out Area quantity)
			{
				return TryParse(element, ignoreCase, Unit.Area.SquareMeter, out quantity);
			}

			public static bool TryParse(XElement element, bool ignoreCase, Unit.Area defaultUnitValue, out Area quantity)
			{
				double value;
				bool isSuccessValue = NumberFormats.TryParseDoubleVb6(element.Value, out value);

				Unit.Area unitValue = defaultUnitValue;
				XAttribute unitAttribute = element.Attributes().FirstOrDefault(x => string.Equals(x.Name.LocalName, "Unit", ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture));
				bool isSuccessUnit = true;

				if (unitAttribute != null)
				{
					isSuccessUnit = Unit.TryParse(unitAttribute.Value, ignoreCase, out unitValue);
				}

				quantity = new Area(value, unitValue);

				return isSuccessValue && isSuccessUnit;
			}

			public static Area Parse(XElement element)
			{
				return Parse(element, false);
			}

			public static Area Parse(XElement element, bool ignoreCase)
			{
				return Parse(element, ignoreCase, Unit.Area.SquareMeter);
			}

			public static Area Parse(XElement element, bool ignoreCase, Unit.Area defaultUnitValue)
			{
				Area quantity;

				if (!TryParse(element, ignoreCase, defaultUnitValue, out quantity))
				{
					throw new FormatException();
				}

				return quantity;
			}

			#region operators

			public static Area operator +(Area left, Area right)
			{
				return new Area(left.Value + right.ToNewUnit(left.UnitValue).Value, left.UnitValue);
			}

			public static Area operator -(Area left, Area right)
			{
				return new Area(left.Value - right.ToNewUnit(left.UnitValue).Value, left.UnitValue);
			}

			public static Area operator *(Area left, Area right)
			{
				return new Area(left.Value * right.ToNewUnit(left.UnitValue).Value, left.UnitValue);
			}

			public static double operator /(Area left, Area right)
			{
				return left.Value / right.ToNewUnit(left.UnitValue).Value;
			}

			public static Area operator *(Area left, double right)
			{
				return new Area(left.Value * right, left.UnitValue);
			}

			public static Area operator /(Area left, double right)
			{
				return new Area(left.Value / right, left.UnitValue);
			}

			public static Area operator *(double left, Area right)
			{
				return new Area(left * right.Value, right.UnitValue);
			}

			public static bool operator <(Area left, Area right)
			{
				return left.Value < right.ToNewUnit(left.UnitValue).Value;
			}

			public static bool operator >(Area left, Area right)
			{
				return left.Value > right.ToNewUnit(left.UnitValue).Value;
			}

			public static bool operator <=(Area left, Area right)
			{
				return left.Value <= right.ToNewUnit(left.UnitValue).Value;
			}

			public static bool operator >=(Area left, Area right)
			{
				return left.Value >= right.ToNewUnit(left.UnitValue).Value;
			}

			#endregion
		}

		public struct Jerk
		{
			public double Value
			{
				get;
			}

			public Unit.Jerk UnitValue
			{
				get;
			}

			public Jerk(double value) : this(value, Unit.Jerk.MeterPerSecondCubed)
			{
			}

			public Jerk(double value, Unit.Jerk unitValue)
			{
				Value = value;
				UnitValue = unitValue;
			}

			public Jerk ToDefaultUnit()
			{
				return ToNewUnit(Unit.Jerk.MeterPerSecondCubed);
			}

			public Jerk ToNewUnit(Unit.Jerk newUnitValue)
			{
				if (newUnitValue == UnitValue)
				{
					return this;
				}

				double factor = 1.0;

				// Convert to SI
				switch (UnitValue)
				{
					case Unit.Jerk.MeterPerSecondCubed:
						// Ignore
						break;
					case Unit.Jerk.CentimeterPerSecondCubed:
						factor *= 1.0e-2;
						break;
					case Unit.Jerk.FootPerSecondCubed:
						factor *= 0.0254 * 12.0;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				// Convert to new unit
				switch (newUnitValue)
				{
					case Unit.Jerk.MeterPerSecondCubed:
						// Ignore
						break;
					case Unit.Jerk.CentimeterPerSecondCubed:
						factor /= 1.0e-2;
						break;
					case Unit.Jerk.FootPerSecondCubed:
						factor /= 0.0254 * 12.0;
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(newUnitValue), newUnitValue, null);
				}

				return new Jerk(Value * factor, newUnitValue);
			}

			public XElement ToXElement(string nodeName)
			{
				return new XElement(nodeName, new XAttribute("Unit", UnitValue), Value.ToString(CultureInfo.InvariantCulture));
			}

			public bool Equals(Jerk obj, bool isAbsolute)
			{
				return isAbsolute ? Value.Equals(obj.ToNewUnit(UnitValue).Value) : Equals(obj);
			}

			public static bool TryParse(XElement element, out Jerk quantity)
			{
				return TryParse(element, false, out quantity);
			}

			public static bool TryParse(XElement element, bool ignoreCase, out Jerk quantity)
			{
				return TryParse(element, ignoreCase, Unit.Jerk.MeterPerSecondCubed, out quantity);
			}

			public static bool TryParse(XElement element, bool ignoreCase, Unit.Jerk defaultUnitValue, out Jerk quantity)
			{
				double value;
				bool isSuccessValue = NumberFormats.TryParseDoubleVb6(element.Value, out value);

				Unit.Jerk unitValue = defaultUnitValue;
				XAttribute unitAttribute = element.Attributes().FirstOrDefault(x => string.Equals(x.Name.LocalName, "Unit", ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture));
				bool isSuccessUnit = true;

				if (unitAttribute != null)
				{
					isSuccessUnit = Unit.TryParse(unitAttribute.Value, ignoreCase, out unitValue);
				}

				quantity = new Jerk(value, unitValue);

				return isSuccessValue && isSuccessUnit;
			}

			public static Jerk Parse(XElement element)
			{
				return Parse(element, false);
			}

			public static Jerk Parse(XElement element, bool ignoreCase)
			{
				return Parse(element, ignoreCase, Unit.Jerk.MeterPerSecondCubed);
			}

			public static Jerk Parse(XElement element, bool ignoreCase, Unit.Jerk defaultUnitValue)
			{
				Jerk quantity;

				if (!TryParse(element, ignoreCase, defaultUnitValue, out quantity))
				{
					throw new FormatException();
				}

				return quantity;
			}

			#region operators

			public static Jerk operator +(Jerk left, Jerk right)
			{
				return new Jerk(left.Value + right.ToNewUnit(left.UnitValue).Value, left.UnitValue);
			}

			public static Jerk operator -(Jerk left, Jerk right)
			{
				return new Jerk(left.Value - right.ToNewUnit(left.UnitValue).Value, left.UnitValue);
			}

			public static Jerk operator *(Jerk left, Jerk right)
			{
				return new Jerk(left.Value * right.ToNewUnit(left.UnitValue).Value, left.UnitValue);
			}

			public static double operator /(Jerk left, Jerk right)
			{
				return left.Value / right.ToNewUnit(left.UnitValue).Value;
			}

			public static Jerk operator *(Jerk left, double right)
			{
				return new Jerk(left.Value * right, left.UnitValue);
			}

			public static Jerk operator /(Jerk left, double right)
			{
				return new Jerk(left.Value / right, left.UnitValue);
			}

			public static Jerk operator *(double left, Jerk right)
			{
				return new Jerk(left * right.Value, right.UnitValue);
			}

			public static bool operator <(Jerk left, Jerk right)
			{
				return left.Value < right.ToNewUnit(left.UnitValue).Value;
			}

			public static bool operator >(Jerk left, Jerk right)
			{
				return left.Value > right.ToNewUnit(left.UnitValue).Value;
			}

			public static bool operator <=(Jerk left, Jerk right)
			{
				return left.Value <= right.ToNewUnit(left.UnitValue).Value;
			}

			public static bool operator >=(Jerk left, Jerk right)
			{
				return left.Value >= right.ToNewUnit(left.UnitValue).Value;
			}

			#endregion
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

			public Length ToDefaultUnit()
			{
				return ToNewUnit(Unit.Length.Meter);
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

			#region operators

			public static Length operator +(Length left, Length right)
			{
				return new Length(left.Value + right.ToNewUnit(left.UnitValue).Value, left.UnitValue);
			}

			public static Length operator -(Length left, Length right)
			{
				return new Length(left.Value - right.ToNewUnit(left.UnitValue).Value, left.UnitValue);
			}

			public static Length operator *(Length left, Length right)
			{
				return new Length(left.Value * right.ToNewUnit(left.UnitValue).Value, left.UnitValue);
			}

			public static double operator /(Length left, Length right)
			{
				return left.Value / right.ToNewUnit(left.UnitValue).Value;
			}

			public static Length operator *(Length left, double right)
			{
				return new Length(left.Value * right, left.UnitValue);
			}

			public static Length operator /(Length left, double right)
			{
				return new Length(left.Value / right, left.UnitValue);
			}

			public static Length operator *(double left, Length right)
			{
				return new Length(left * right.Value, right.UnitValue);
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

			#endregion
		}

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

			public Mass ToDefaultUnit()
			{
				return ToNewUnit(Unit.Mass.Kilogram);
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

			#region operators

			public static Mass operator +(Mass left, Mass right)
			{
				return new Mass(left.Value + right.ToNewUnit(left.UnitValue).Value, left.UnitValue);
			}

			public static Mass operator -(Mass left, Mass right)
			{
				return new Mass(left.Value - right.ToNewUnit(left.UnitValue).Value, left.UnitValue);
			}

			public static Mass operator *(Mass left, Mass right)
			{
				return new Mass(left.Value * right.ToNewUnit(left.UnitValue).Value, left.UnitValue);
			}

			public static double operator /(Mass left, Mass right)
			{
				return left.Value / right.ToNewUnit(left.UnitValue).Value;
			}

			public static Mass operator *(Mass left, double right)
			{
				return new Mass(left.Value * right, left.UnitValue);
			}

			public static Mass operator /(Mass left, double right)
			{
				return new Mass(left.Value / right, left.UnitValue);
			}

			public static Mass operator *(double left, Mass right)
			{
				return new Mass(left * right.Value, right.UnitValue);
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

			#endregion
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

			public Pressure ToDefaultUnit()
			{
				return ToNewUnit(Unit.Pressure.Pascal);
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

			#region operators

			public static Pressure operator +(Pressure left, Pressure right)
			{
				return new Pressure(left.Value + right.ToNewUnit(left.UnitValue).Value, left.UnitValue);
			}

			public static Pressure operator -(Pressure left, Pressure right)
			{
				return new Pressure(left.Value - right.ToNewUnit(left.UnitValue).Value, left.UnitValue);
			}

			public static Pressure operator *(Pressure left, Pressure right)
			{
				return new Pressure(left.Value * right.ToNewUnit(left.UnitValue).Value, left.UnitValue);
			}

			public static double operator /(Pressure left, Pressure right)
			{
				return left.Value / right.ToNewUnit(left.UnitValue).Value;
			}

			public static Pressure operator *(Pressure left, double right)
			{
				return new Pressure(left.Value * right, left.UnitValue);
			}

			public static Pressure operator /(Pressure left, double right)
			{
				return new Pressure(left.Value / right, left.UnitValue);
			}

			public static Pressure operator *(double left, Pressure right)
			{
				return new Pressure(left * right.Value, right.UnitValue);
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

			#endregion
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

			public PressureRate ToDefaultUnit()
			{
				return ToNewUnit(Unit.PressureRate.PascalPerSecond);
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

			#region operators

			public static PressureRate operator +(PressureRate left, PressureRate right)
			{
				return new PressureRate(left.Value + right.ToNewUnit(left.UnitValue).Value, left.UnitValue);
			}

			public static PressureRate operator -(PressureRate left, PressureRate right)
			{
				return new PressureRate(left.Value - right.ToNewUnit(left.UnitValue).Value, left.UnitValue);
			}

			public static PressureRate operator *(PressureRate left, PressureRate right)
			{
				return new PressureRate(left.Value * right.ToNewUnit(left.UnitValue).Value, left.UnitValue);
			}

			public static double operator /(PressureRate left, PressureRate right)
			{
				return left.Value / right.ToNewUnit(left.UnitValue).Value;
			}

			public static PressureRate operator *(PressureRate left, double right)
			{
				return new PressureRate(left.Value * right, left.UnitValue);
			}

			public static PressureRate operator /(PressureRate left, double right)
			{
				return new PressureRate(left.Value / right, left.UnitValue);
			}

			public static PressureRate operator *(double left, PressureRate right)
			{
				return new PressureRate(left * right.Value, right.UnitValue);
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

			#endregion
		}

		public struct Time
		{
			public double Value
			{
				get;
			}

			public Unit.Time UnitValue
			{
				get;
			}

			public Time(double value) : this(value, Unit.Time.Second)
			{
			}

			public Time(double value, Unit.Time unitValue)
			{
				Value = value;
				UnitValue = unitValue;
			}

			public Time ToDefaultUnit()
			{
				return ToNewUnit(Unit.Time.Second);
			}

			public Time ToNewUnit(Unit.Time newUnitValue)
			{
				if (newUnitValue == UnitValue)
				{
					return this;
				}

				double factor = 1.0;

				// Convert to SI
				switch (UnitValue)
				{
					case Unit.Time.Second:
						// Ignore
						break;
					case Unit.Time.Millisecond:
						factor *= 1.0e-3;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				// Convert to new unit
				switch (newUnitValue)
				{
					case Unit.Time.Second:
						// Ignore
						break;
					case Unit.Time.Millisecond:
						factor /= 1.0e-3;
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(newUnitValue), newUnitValue, null);
				}

				return new Time(Value * factor, newUnitValue);
			}

			public XElement ToXElement(string nodeName)
			{
				return new XElement(nodeName, new XAttribute("Unit", UnitValue), Value.ToString(CultureInfo.InvariantCulture));
			}

			public bool Equals(Time obj, bool isAbsolute)
			{
				return isAbsolute ? Value.Equals(obj.ToNewUnit(UnitValue).Value) : Equals(obj);
			}

			public static bool TryParse(XElement element, out Time quantity)
			{
				return TryParse(element, false, out quantity);
			}

			public static bool TryParse(XElement element, bool ignoreCase, out Time quantity)
			{
				return TryParse(element, ignoreCase, Unit.Time.Second, out quantity);
			}

			public static bool TryParse(XElement element, bool ignoreCase, Unit.Time defaultUnitValue, out Time quantity)
			{
				double value;
				bool isSuccessValue = NumberFormats.TryParseDoubleVb6(element.Value, out value);

				Unit.Time unitValue = defaultUnitValue;
				XAttribute unitAttribute = element.Attributes().FirstOrDefault(x => string.Equals(x.Name.LocalName, "Unit", ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture));
				bool isSuccessUnit = true;

				if (unitAttribute != null)
				{
					isSuccessUnit = Unit.TryParse(unitAttribute.Value, ignoreCase, out unitValue);
				}

				quantity = new Time(value, unitValue);

				return isSuccessValue && isSuccessUnit;
			}

			public static Time Parse(XElement element)
			{
				return Parse(element, false);
			}

			public static Time Parse(XElement element, bool ignoreCase)
			{
				return Parse(element, ignoreCase, Unit.Time.Second);
			}

			public static Time Parse(XElement element, bool ignoreCase, Unit.Time defaultUnitValue)
			{
				Time quantity;

				if (!TryParse(element, ignoreCase, defaultUnitValue, out quantity))
				{
					throw new FormatException();
				}

				return quantity;
			}

			#region operators

			public static Time operator +(Time left, Time right)
			{
				return new Time(left.Value + right.ToNewUnit(left.UnitValue).Value, left.UnitValue);
			}

			public static Time operator -(Time left, Time right)
			{
				return new Time(left.Value - right.ToNewUnit(left.UnitValue).Value, left.UnitValue);
			}

			public static Time operator *(Time left, Time right)
			{
				return new Time(left.Value * right.ToNewUnit(left.UnitValue).Value, left.UnitValue);
			}

			public static double operator /(Time left, Time right)
			{
				return left.Value / right.ToNewUnit(left.UnitValue).Value;
			}

			public static Time operator *(Time left, double right)
			{
				return new Time(left.Value * right, left.UnitValue);
			}

			public static Time operator /(Time left, double right)
			{
				return new Time(left.Value / right, left.UnitValue);
			}

			public static Time operator *(double left, Time right)
			{
				return new Time(left * right.Value, right.UnitValue);
			}

			public static bool operator <(Time left, Time right)
			{
				return left.Value < right.ToNewUnit(left.UnitValue).Value;
			}

			public static bool operator >(Time left, Time right)
			{
				return left.Value > right.ToNewUnit(left.UnitValue).Value;
			}

			public static bool operator <=(Time left, Time right)
			{
				return left.Value <= right.ToNewUnit(left.UnitValue).Value;
			}

			public static bool operator >=(Time left, Time right)
			{
				return left.Value >= right.ToNewUnit(left.UnitValue).Value;
			}

			#endregion
		}

		public struct Velocity
		{
			public double Value
			{
				get;
			}

			public Unit.Velocity UnitValue
			{
				get;
			}

			public Velocity(double value) : this(value, Unit.Velocity.MeterPerSecond)
			{
			}

			public Velocity(double value, Unit.Velocity unitValue)
			{
				Value = value;
				UnitValue = unitValue;
			}

			public Velocity ToDefaultUnit()
			{
				return ToNewUnit(Unit.Velocity.MeterPerSecond);
			}

			public Velocity ToNewUnit(Unit.Velocity newUnitValue)
			{
				if (newUnitValue == UnitValue)
				{
					return this;
				}

				double factor = 1.0;

				// Convert to SI
				switch (UnitValue)
				{
					case Unit.Velocity.KilometerPerHour:
						factor /= 3.6;
						break;
					case Unit.Velocity.MeterPerSecond:
						// Ignore
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				// Convert to new unit
				switch (newUnitValue)
				{
					case Unit.Velocity.KilometerPerHour:
						factor *= 3.6;
						break;
					case Unit.Velocity.MeterPerSecond:
						// Ignore
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(newUnitValue), newUnitValue, null);
				}

				return new Velocity(Value * factor, newUnitValue);
			}

			public XElement ToXElement(string nodeName)
			{
				return new XElement(nodeName, new XAttribute("Unit", UnitValue), Value.ToString(CultureInfo.InvariantCulture));
			}

			public bool Equals(Velocity obj, bool isAbsolute)
			{
				return isAbsolute ? Value.Equals(obj.ToNewUnit(UnitValue).Value) : Equals(obj);
			}

			public static bool TryParse(XElement element, out Velocity quantity)
			{
				return TryParse(element, false, out quantity);
			}

			public static bool TryParse(XElement element, bool ignoreCase, out Velocity quantity)
			{
				return TryParse(element, ignoreCase, Unit.Velocity.MeterPerSecond, out quantity);
			}

			public static bool TryParse(XElement element, bool ignoreCase, Unit.Velocity defaultUnitValue, out Velocity quantity)
			{
				double value;
				bool isSuccessValue = NumberFormats.TryParseDoubleVb6(element.Value, out value);

				Unit.Velocity unitValue = defaultUnitValue;
				XAttribute unitAttribute = element.Attributes().FirstOrDefault(x => string.Equals(x.Name.LocalName, "Unit", ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture));
				bool isSuccessUnit = true;

				if (unitAttribute != null)
				{
					isSuccessUnit = Unit.TryParse(unitAttribute.Value, ignoreCase, out unitValue);
				}

				quantity = new Velocity(value, unitValue);

				return isSuccessValue && isSuccessUnit;
			}

			public static Velocity Parse(XElement element)
			{
				return Parse(element, false);
			}

			public static Velocity Parse(XElement element, bool ignoreCase)
			{
				return Parse(element, ignoreCase, Unit.Velocity.MeterPerSecond);
			}

			public static Velocity Parse(XElement element, bool ignoreCase, Unit.Velocity defaultUnitValue)
			{
				Velocity quantity;

				if (!TryParse(element, ignoreCase, defaultUnitValue, out quantity))
				{
					throw new FormatException();
				}

				return quantity;
			}

			#region operators

			public static Velocity operator +(Velocity left, Velocity right)
			{
				return new Velocity(left.Value + right.ToNewUnit(left.UnitValue).Value, left.UnitValue);
			}

			public static Velocity operator -(Velocity left, Velocity right)
			{
				return new Velocity(left.Value - right.ToNewUnit(left.UnitValue).Value, left.UnitValue);
			}

			public static Velocity operator *(Velocity left, Velocity right)
			{
				return new Velocity(left.Value * right.ToNewUnit(left.UnitValue).Value, left.UnitValue);
			}

			public static double operator /(Velocity left, Velocity right)
			{
				return left.Value / right.ToNewUnit(left.UnitValue).Value;
			}

			public static Velocity operator *(Velocity left, double right)
			{
				return new Velocity(left.Value * right, left.UnitValue);
			}

			public static Velocity operator /(Velocity left, double right)
			{
				return new Velocity(left.Value / right, left.UnitValue);
			}

			public static Velocity operator *(double left, Velocity right)
			{
				return new Velocity(left * right.Value, right.UnitValue);
			}

			public static bool operator <(Velocity left, Velocity right)
			{
				return left.Value < right.ToNewUnit(left.UnitValue).Value;
			}

			public static bool operator >(Velocity left, Velocity right)
			{
				return left.Value > right.ToNewUnit(left.UnitValue).Value;
			}

			public static bool operator <=(Velocity left, Velocity right)
			{
				return left.Value <= right.ToNewUnit(left.UnitValue).Value;
			}

			public static bool operator >=(Velocity left, Velocity right)
			{
				return left.Value >= right.ToNewUnit(left.UnitValue).Value;
			}

			public static explicit operator Velocity(VelocityF velocity)
			{
				return new Velocity(velocity.Value, velocity.UnitValue);
			}

			#endregion
		}

		public struct VelocityF
		{
			public float Value
			{
				get;
			}

			public Unit.Velocity UnitValue
			{
				get;
			}

			public VelocityF(float value) : this(value, Unit.Velocity.MeterPerSecond)
			{
			}

			public VelocityF(float value, Unit.Velocity unitValue)
			{
				Value = value;
				UnitValue = unitValue;
			}

			public VelocityF ToDefaultUnit()
			{
				return ToNewUnit(Unit.Velocity.MeterPerSecond);
			}

			public VelocityF ToNewUnit(Unit.Velocity newUnitValue)
			{
				if (newUnitValue == UnitValue)
				{
					return this;
				}

				float factor = 1.0f;

				// Convert to SI
				switch (UnitValue)
				{
					case Unit.Velocity.KilometerPerHour:
						factor /= 3.6f;
						break;
					case Unit.Velocity.MeterPerSecond:
						// Ignore
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				// Convert to new unit
				switch (newUnitValue)
				{
					case Unit.Velocity.KilometerPerHour:
						factor *= 3.6f;
						break;
					case Unit.Velocity.MeterPerSecond:
						// Ignore
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(newUnitValue), newUnitValue, null);
				}

				return new VelocityF(Value * factor, newUnitValue);
			}

			public XElement ToXElement(string nodeName)
			{
				return new XElement(nodeName, new XAttribute("Unit", UnitValue), Value.ToString(CultureInfo.InvariantCulture));
			}

			public bool Equals(VelocityF obj, bool isAbsolute)
			{
				return isAbsolute ? Value.Equals(obj.ToNewUnit(UnitValue).Value) : Equals(obj);
			}

			public static bool TryParse(XElement element, out VelocityF quantity)
			{
				return TryParse(element, false, out quantity);
			}

			public static bool TryParse(XElement element, bool ignoreCase, out VelocityF quantity)
			{
				return TryParse(element, ignoreCase, Unit.Velocity.MeterPerSecond, out quantity);
			}

			public static bool TryParse(XElement element, bool ignoreCase, Unit.Velocity defaultUnitValue, out VelocityF quantity)
			{
				float value;
				bool isSuccessValue = NumberFormats.TryParseFloatVb6(element.Value, out value);

				Unit.Velocity unitValue = defaultUnitValue;
				XAttribute unitAttribute = element.Attributes().FirstOrDefault(x => string.Equals(x.Name.LocalName, "Unit", ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture));
				bool isSuccessUnit = true;

				if (unitAttribute != null)
				{
					isSuccessUnit = Unit.TryParse(unitAttribute.Value, ignoreCase, out unitValue);
				}

				quantity = new VelocityF(value, unitValue);

				return isSuccessValue && isSuccessUnit;
			}

			public static VelocityF Parse(XElement element)
			{
				return Parse(element, false);
			}

			public static VelocityF Parse(XElement element, bool ignoreCase)
			{
				return Parse(element, ignoreCase, Unit.Velocity.MeterPerSecond);
			}

			public static VelocityF Parse(XElement element, bool ignoreCase, Unit.Velocity defaultUnitValue)
			{
				VelocityF quantity;

				if (!TryParse(element, ignoreCase, defaultUnitValue, out quantity))
				{
					throw new FormatException();
				}

				return quantity;
			}

			#region operators

			public static VelocityF operator +(VelocityF left, VelocityF right)
			{
				return new VelocityF(left.Value + right.ToNewUnit(left.UnitValue).Value, left.UnitValue);
			}

			public static VelocityF operator -(VelocityF left, VelocityF right)
			{
				return new VelocityF(left.Value - right.ToNewUnit(left.UnitValue).Value, left.UnitValue);
			}

			public static VelocityF operator *(VelocityF left, VelocityF right)
			{
				return new VelocityF(left.Value * right.ToNewUnit(left.UnitValue).Value, left.UnitValue);
			}

			public static float operator /(VelocityF left, VelocityF right)
			{
				return left.Value / right.ToNewUnit(left.UnitValue).Value;
			}

			public static VelocityF operator *(VelocityF left, float right)
			{
				return new VelocityF(left.Value * right, left.UnitValue);
			}

			public static VelocityF operator /(VelocityF left, float right)
			{
				return new VelocityF(left.Value / right, left.UnitValue);
			}

			public static VelocityF operator *(float left, VelocityF right)
			{
				return new VelocityF(left * right.Value, right.UnitValue);
			}

			public static bool operator <(VelocityF left, VelocityF right)
			{
				return left.Value < right.ToNewUnit(left.UnitValue).Value;
			}

			public static bool operator >(VelocityF left, VelocityF right)
			{
				return left.Value > right.ToNewUnit(left.UnitValue).Value;
			}

			public static bool operator <=(VelocityF left, VelocityF right)
			{
				return left.Value <= right.ToNewUnit(left.UnitValue).Value;
			}

			public static bool operator >=(VelocityF left, VelocityF right)
			{
				return left.Value >= right.ToNewUnit(left.UnitValue).Value;
			}

			public static explicit operator VelocityF(Velocity velocity)
			{
				return new VelocityF((float)velocity.Value, velocity.UnitValue);
			}

			#endregion
		}
	}
}
