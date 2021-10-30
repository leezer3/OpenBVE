using System;
using System.Collections.Generic;
using OpenBveApi.Interface;
using OpenBveApi.Objects;
using OpenBveApi.Sounds;
using SoundManager;

namespace Route.Bve5
{
	static partial class Bve5ScenarioParser
	{
		/// <summary>Defines a dictionary of objects</summary>
		private class ObjectDictionary : Dictionary<string, UnifiedObject>
		{
			internal ObjectDictionary() : base(StringComparer.InvariantCultureIgnoreCase)
			{
			}

			/// <summary>Adds a new Unified Object to the dictionary</summary>
			/// <param name="key">The object index</param>
			/// <param name="unifiedObject">The object</param>
			internal new void Add(string key, UnifiedObject unifiedObject)
			{
				if (ContainsKey(key))
				{
					base[key] = unifiedObject;
					Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "The structure " + key + " has been declared twice: The most recent declaration will be used.");
				}
				else
				{
					base.Add(key, unifiedObject);
				}
			}

			/// <summary>Adds a new Static Object to the dictionary</summary>
			/// <param name="key">The object index</param>
			/// <param name="staticObject">The object</param>
			internal void Add(string key, StaticObject staticObject)
			{
				if (ContainsKey(key))
				{
					base[key] = staticObject;
					Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "The structure " + key + " has been declared twice: The most recent declaration will be used.");
				}
				else
				{
					base.Add(key, staticObject);
				}
			}
		}

		private class SoundDictionary : Dictionary<string, SoundHandle>
		{
			internal SoundDictionary() : base(StringComparer.InvariantCultureIgnoreCase)
			{

			}

			internal new void Add(string key, SoundHandle soundBuffer)
			{
				if (ContainsKey(key))
				{
					base[key] = soundBuffer;
					Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "The sound " + key + " has been declared twice: The most recent declaration will be used.");
				}
				else
				{
					base.Add(key, soundBuffer);
				}
			}
		}
	}
}
