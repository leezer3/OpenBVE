//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020, S520, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using OpenBveApi.Interface;
using OpenBveApi.Objects;
using OpenBveApi.Sounds;

namespace Route.Bve5
{
	static partial class Bve5ScenarioParser
	{
		/// <summary>Defines a dictionary of objects</summary>
		internal class ObjectDictionary : Dictionary<string, UnifiedObject>
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
		}

		internal class SoundDictionary : Dictionary<string, SoundHandle>
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
