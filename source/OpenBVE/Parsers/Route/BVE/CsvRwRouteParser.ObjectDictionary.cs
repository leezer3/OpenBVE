using System.Collections.Generic;
using OpenBveApi.Interface;
using OpenBveApi.Objects;
using OpenBveApi.Routes;

namespace OpenBve
{
	internal partial class CsvRwRouteParser
	{
		/// <inheritdoc />
		/// <summary>Defines a dictionary of objects</summary>
		private class ObjectDictionary : Dictionary<int, UnifiedObject>
		{
			/// <summary>Adds a new Unified Object to the dictionary</summary>
			/// <param name="key">The object index</param>
			/// <param name="unifiedObject">The object</param>
			/// <param name="Type">The object type</param>
			internal void Add(int key, UnifiedObject unifiedObject, string Type)
			{
				if (this.ContainsKey(key))
				{
					this[key] = unifiedObject;
					Interface.AddMessage(MessageType.Warning, false, "The " + Type + " with an index of " + key + " has been declared twice: The most recent declaration will be used.");
				}
				else
				{
					base.Add(key, unifiedObject);
				}
			}

			/// <summary>Adds a new Static Object to the dictionary</summary>
			/// <param name="key">The object index</param>
			/// <param name="staticObject">The object</param>
			/// <param name="Type">The object type</param>
			internal void Add(int key, StaticObject staticObject, string Type)
			{
				if (this.ContainsKey(key))
				{
					this[key] = staticObject;
					Interface.AddMessage(MessageType.Warning, false, "The " + Type + " with an index of " + key + " has been declared twice: The most recent declaration will be used.");
				}
				else
				{
					base.Add(key, staticObject);
				}
			}

			internal new void Add(int key, UnifiedObject unifiedObject)
			{
				if (this.ContainsKey(key))
				{
					this[key] = unifiedObject;
					Interface.AddMessage(MessageType.Warning, false, "The object with an index of " + key + " has been declared twice: The most recent declaration will be used.");
				}
				else
				{
					base.Add(key, unifiedObject);
				}
			}

			internal void Add(int key, StaticObject staticObject)
			{
				if (this.ContainsKey(key))
				{
					this[key] = staticObject;
					Interface.AddMessage(MessageType.Warning, false, "The object with an index of " + key + " has been declared twice: The most recent declaration will be used.");
				}
				else
				{
					base.Add(key, staticObject);
				}
			}
		}

		/// <inheritdoc/>
		/// <summary>Defines a dictionary of signals</summary>
		private class SignalDictionary : Dictionary<int, SignalData>
		{
			/// <summary>Adds a new signal to the dictionary</summary>
			/// <param name="key">The signal index</param>
			/// <param name="signal">The signal object</param>
			internal new void Add(int key, SignalData signal)
			{
				if (this.ContainsKey(key))
				{
					this[key] = signal;
					Interface.AddMessage(MessageType.Warning, false, "The Signal with an index of " + key + " has been declared twice: The most recent declaration will be used.");
				}
				else
				{
					base.Add(key, signal);
				}
			}
		}

		/// <inheritdoc />
		/// <summary>Defines a dictionary of backgrounds</summary>
		private class BackgroundDictionary : Dictionary<int, BackgroundHandle>
		{
			/// <summary>Adds a new background to the dictionary</summary>
			/// <param name="key">The background index</param>
			/// <param name="handle">The background handle</param>
			internal new void Add(int key, BackgroundHandle handle)
			{
				if (this.ContainsKey(key))
				{
					this[key] = handle;
					Interface.AddMessage(MessageType.Warning, false, "The Background with an index of " + key + " has been declared twice: The most recent declaration will be used.");
				}
				else
				{
					base.Add(key, handle);
				}
			}
		}

		private class PoleDictionary : Dictionary<int, ObjectDictionary>
		{
			/// <summary>Adds a new set of poles to the dictionary</summary>
			/// <param name="key">The pole index</param>
			/// <param name="dict">The background handle</param>
			internal new void Add(int key, ObjectDictionary dict)
			{
				if (this.ContainsKey(key))
				{
					this[key] = dict;
					Interface.AddMessage(MessageType.Warning, false, "The Background with an index of " + key + " has been declared twice: The most recent declaration will be used.");
				}
				else
				{
					base.Add(key, dict);
				}
			}
		}
	}
}
