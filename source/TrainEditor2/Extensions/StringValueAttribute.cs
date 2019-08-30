using System;
using System.Linq;
using System.Reflection;

namespace TrainEditor2.Extensions
{
	[AttributeUsage(AttributeTargets.Field)]
	internal sealed class StringValueAttribute : Attribute
	{
		internal readonly string[] Values;

		internal StringValueAttribute(params string[] values)
		{
			Values = values;
		}
	}

	internal static class StringValueAttributeExtensions
	{
		internal static string GetStringValue(this Enum value, int index = 0)
		{
			Type type = value.GetType();

			FieldInfo fieldInfo = type.GetField(value.ToString());

			if (fieldInfo == null)
			{
				return null;
			}

			StringValueAttribute[] attributes = fieldInfo.GetCustomAttributes(typeof(StringValueAttribute), false).OfType<StringValueAttribute>().ToArray();

			if (attributes.Any())
			{
				if (index < attributes[0].Values.Length)
				{
					return attributes[0].Values[index];
				}
			}

			return null;
		}
	}
}
