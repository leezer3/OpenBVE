using System.Collections.Generic;
using OpenBveApi.Interface;

namespace OpenBve
{
	internal partial class CsvRwRouteParser
	{
		/// <inheritdoc />
		/// <summary>Defines a dictionary of objects</summary>
		private class ObjectDictionary : Dictionary<int, ObjectManager.UnifiedObject>
		{
			/// <summary>Adds a new Unified Object to the dictionary</summary>
			/// <param name="key">The object index</param>
			/// <param name="unifiedObject">The object</param>
			/// <param name="Type">The object type</param>
			internal void Add(int key, ObjectManager.UnifiedObject unifiedObject, string Type)
			{
				if (this.ContainsKey(key))
				{
					this[key] = unifiedObject;
					Interface.AddMessage(MessageType.Warning, false, "The " + Type + " with an index of " + key + " has been declared twice: The most recent declaration will be used.");
				}
				else
				{
					this.Add(key, unifiedObject);
				}
			}

			/// <summary>Adds a new Static Object to the dictionary</summary>
			/// <param name="key">The object index</param>
			/// <param name="staticObject">The object</param>
			/// <param name="Type">The object type</param>
			internal void Add(int key, ObjectManager.StaticObject staticObject, string Type)
			{
				if (this.ContainsKey(key))
				{
					this[key] = staticObject;
					Interface.AddMessage(MessageType.Warning, false, "The " + Type + " with an index of " + key + " has been declared twice: The most recent declaration will be used.");
				}
				else
				{
					this.Add(key, staticObject);
				}
			}
		}
	}
}
