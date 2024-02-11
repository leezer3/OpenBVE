//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2023, Christopher Lees, The OpenBVE Project
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

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace OpenBveApi.Interface
{
	/// <summary>A group of translations</summary>
	public class TranslationGroup
	{
		/// <summary>The sub-groups within this group</summary>
		/// <remarks>A group may contain multiple layers of sub-groups</remarks>
		public Dictionary<string, TranslationGroup> SubGroups = new Dictionary<string, TranslationGroup>();
		/// <summary>The strings within this group</summary>
		public Dictionary<string, TranslationString> Strings = new Dictionary<string, TranslationString>();

		/// <summary>Creates a new TranslationGroup</summary>
		/// <param name="xmlns">The parent XML namespace</param>
		/// <param name="groupElement">The XElement containing the group</param>
		public TranslationGroup(XNamespace xmlns, XElement groupElement)
		{
			XElement[] groups = groupElement.Elements(xmlns + "group").ToArray();
			for (int i = 0; i < groups.Length; i++)
			{
				string subGroupID = (string)groups[i].Attribute("id");
				if (!SubGroups.ContainsKey(subGroupID))
				{
					SubGroups.Add(subGroupID, new TranslationGroup(xmlns, groups[i]));
				}
				else
				{
					// DUPLICATE GROUP!
					// NOTE: Requires a warning to add once this is complete
					SubGroups[subGroupID] = new TranslationGroup(xmlns, groups[i]);
				}
				
			}

			XElement[] strings = groupElement.Elements(xmlns + "trans-unit").ToArray();
			for (int i = 0; i < strings.Length; i++)
			{
				string stringKey = (string)strings[i].Attribute("id");
				if (!Strings.ContainsKey(stringKey))
				{
					Strings.Add(stringKey, new TranslationString(xmlns, strings[i]));
				}
				else
				{
					// WARNING: DUPLICATE STRING!
					TranslationString t = new TranslationString(xmlns, strings[i]);
					if (Strings[stringKey].Translation == null && t.Translation != null)
					{
						// only therefore replace if the *new* is translated and the *old* is not
						// NOTE: Requires a warning to add once this is complete
						Strings[stringKey] = t;
					}
					
				}
				
			}

		}
	}
}
