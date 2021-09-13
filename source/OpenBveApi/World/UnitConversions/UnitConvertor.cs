using System;
using System.Collections.Concurrent;

namespace OpenBveApi.World
{
	/// <summary>
	/// Generic conversion class for converting between values of different units.
	/// </summary>
	/// <typeparam name="TUnitType">The type representing the unit type (eg. enum)</typeparam>
	/// <typeparam name="TValueType">The type of value for this unit (float, decimal, int, etc.)</typeparam>
	public abstract class UnitConverter<TUnitType, TValueType>
	{
		/// <summary>
		/// The base unit, which all calculations will be expressed in terms of.
		/// </summary>
		protected static TUnitType BaseUnit;

		/// <summary>
		/// Dictionary of functions to convert from the base unit type into a specific type.
		/// </summary>
		static ConcurrentDictionary<TUnitType, Func<TValueType, TValueType>> ConversionsTo = new ConcurrentDictionary<TUnitType, Func<TValueType, TValueType>>();

		/// <summary>
		/// Dictionary of functions to convert from the specified type into the base unit type.
		/// </summary>
		static ConcurrentDictionary<TUnitType, Func<TValueType, TValueType>> ConversionsFrom = new ConcurrentDictionary<TUnitType, Func<TValueType, TValueType>>();

		/// <summary>
		/// Converts a value from one unit type to another.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <param name="from">The unit type the provided value is in.</param>
		/// <param name="to">The unit type to convert the value to.</param>
		/// <returns>The converted value.</returns>
		public TValueType Convert(TValueType value, TUnitType from, TUnitType to)
		{
			// If both From/To are the same, don't do any work.
			if (from.Equals(to))
				return value;

			// Convert into the base unit, if required.
			var valueInBaseUnit = from.Equals(BaseUnit)
				? value
				: ConversionsFrom[from](value);

			// Convert from the base unit into the requested unit, if required
			var valueInRequiredUnit = to.Equals(BaseUnit)
				? valueInBaseUnit
				: ConversionsTo[to](valueInBaseUnit);

			return valueInRequiredUnit;
		}

		/// <summary>
		/// Registers functions for converting to/from a unit.
		/// </summary>
		/// <param name="convertToUnit">The type of unit to convert to/from, from the base unit.</param>
		/// <param name="conversionTo">A function to convert from the base unit.</param>
		/// <param name="conversionFrom">A function to convert to the base unit.</param>
		protected static void RegisterConversion(TUnitType convertToUnit, Func<TValueType, TValueType> conversionTo, Func<TValueType, TValueType> conversionFrom)
		{
			if (!ConversionsTo.TryAdd(convertToUnit, conversionTo))
				throw new ArgumentException("Already exists", "convertToUnit");
			if (!ConversionsFrom.TryAdd(convertToUnit, conversionFrom))
				throw new ArgumentException("Already exists", "convertToUnit");
		}
	}
}
