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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using OpenBveApi.Hosts;

namespace OpenBveApi.Interface
{
	/// <summary>A language file</summary>
	public class NewLanguage
	{
		/// <summary>The display name of the language</summary>
		internal string Name;
		/// <summary>The ISO-639 language code</summary>
		internal string Code;
		/// <summary>A list of fallback language codes to try if a string does not exist</summary>
		internal List<string> FallbackCodes;
		/// <summary>The path to the flag for this language code</summary>
		internal string Flag;
		/// <summary>The list of contributors</summary>
		internal string Contributors;
		/// <summary>The time this language file was last updated</summary>
		internal DateTime LastUpdated;
		/// <summary>The translation groups contained within the language</summary>
		internal Dictionary<string, TranslationGroup> TranslationGroups = new Dictionary<string, TranslationGroup>();

		/// <summary>Loads a language from a language file</summary>
		internal NewLanguage(string languageFile) : this(new FileStream(languageFile, FileMode.Open, FileAccess.Read), languageFile)
		{

		}

		/// <summary>Loads a language from an embedded stream</summary>
		internal NewLanguage(Stream languageStream, string fileName)
		{
			Code = Path.GetFileNameWithoutExtension(fileName); // n.b. filename must be language code
			FallbackCodes = new List<string> { "en-US" };
			using (StreamReader reader = new StreamReader(languageStream))
			{
				try
				{
					// ReSharper disable PossibleNullReferenceException
					// Exceptions will propagate up the load chain anyway
					XDocument currentXML = XDocument.Load(reader);
					XNamespace xmlns = currentXML.Root.Name.Namespace;
					XElement body = currentXML.Root.Element(xmlns + "file").Element(xmlns + "body");
					XElement[] groups = body.Elements(xmlns + "group").ToArray();
					for (int i = 0; i < groups.Length; i++)
					{
						string groupID = (string)groups[i].Attribute("id");
						if (groupID == "language")
						{
							// contains language properties
						}
						else
						{
							TranslationGroups.Add(groupID, new TranslationGroup(xmlns, groups[i]));	
						}
						
					}
				}
				catch
				{
					// ignore
				}
			}
		}

		/// <summary>Gets an interface string unconditionally</summary>
		/// <param name="hostApplication">The host application</param>
		/// <param name="parameters">The string to retrieve</param>
		/// <returns>The translated string</returns>
		/// <remarks>If the translated string does not exist in the current language, this may return a result from a fallback language file</remarks>
		public string GetInterfaceString(HostApplication hostApplication, string[] parameters)
		{
			bool exists;
			string interfaceString = GetInterfaceString(hostApplication, parameters, out exists);

			if (exists)
			{
				return interfaceString;
			}

			for (int i = 0; i < FallbackCodes.Count; i++)
			{
					
				if (Translations.AvailableNewLanguages.ContainsKey(FallbackCodes[i]))
				{
					// don't overwrite the original string from the file in case someone has just translated the source.....
					string candidateString = Translations.AvailableNewLanguages[FallbackCodes[i]].GetInterfaceString(hostApplication, parameters, out exists);
					if (exists)
					{
						return candidateString;
					}
				}
			}

			return interfaceString;
		}

		private string GetInterfaceString(HostApplication hostApplication, string[] parameters, out bool translatedStringExists)
		{
			translatedStringExists = false;
			TranslationGroup translationGroup;
			switch (hostApplication)
			{
				case HostApplication.OpenBve:
					translationGroup = TranslationGroups["openbve"];
					break;
				case HostApplication.TrainEditor:
					translationGroup = TranslationGroups["train_editor"];
					break;
				case HostApplication.TrainEditor2:
					translationGroup = TranslationGroups["train_editor2"];
					break;
				default:
					throw new Exception("Untranslated program");
			}
			for (int i = 0; i < parameters.Length - 1; i++)
			{
				if(translationGroup.SubGroups.ContainsKey(parameters[i]))
				{
					// continue to traverse down the sub-group chain
					translationGroup = translationGroup.SubGroups[parameters[i]];
				}
				else
				{
					// sub group does not exist in our translation
					return string.Empty;
				}
				
			}

			string stringKey = parameters[parameters.Length - 1];
			if (translationGroup.Strings.ContainsKey(stringKey))
			{
				if (translationGroup.Strings[stringKey].Translation == null)
				{
					// Exists in the translation file, but no translated string
					// This will be used if no better option is found
					return translationGroup.Strings[stringKey].Source;
				}
				// A translated copy exists in the translation file
				translatedStringExists = true;
				return translationGroup.Strings[stringKey].Translation;
			}
			return string.Empty;
		}
	}
}
