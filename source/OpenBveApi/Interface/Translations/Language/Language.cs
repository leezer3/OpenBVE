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
using OpenBveApi.Input;

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
		/*
		/// <summary>The time this language file was last updated</summary>
		internal DateTime LastUpdated;
		*/
		/// <summary>The translation groups contained within the language</summary>
		internal Dictionary<string, TranslationGroup> TranslationGroups = new Dictionary<string, TranslationGroup>();
		/// <summary>The quick references</summary>
		internal InterfaceQuickReference QuickReferences;
		/// <summary>The key translations</summary>
		internal Dictionary<Key, Translations.KeyInfo> KeyInfos;

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
							XElement[] strings = groups[i].Elements(xmlns + "trans-unit").ToArray();
							for (int j = 0; j < strings.Length; j++)
							{
								// n.b. we seem to have kept the source identical but translated the target for most languages
								// possibly ought to redesign things slightly- need to lookup the correct way to do this for XLF
								// properties which are language specific as I suspect this is not the intended way to do this.....
								//
								// To try and maintain full compatability with anything manually edited, use the target in preference to the source
								switch((string)strings[j].Attribute("id"))
								{
									case "name":
										Name = strings[j].Element(xmlns + "target") != null ? (string)strings[j].Element(xmlns + "target") : (string)strings[j].Element(xmlns + "source");
										break;
									case "flag":
										Flag = strings[j].Element(xmlns + "target") != null ? (string)strings[j].Element(xmlns + "target") : (string)strings[j].Element(xmlns + "source");
										break;
									case "contributors":
										Contributors = strings[j].Element(xmlns + "target") != null ? (string)strings[j].Element(xmlns + "target") : (string)strings[j].Element(xmlns + "source");
										break;
									case "last_updated":
										break;
								}
							}
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
			// NOTE: These should be loaded *after* the XML
			QuickReferences = new InterfaceQuickReference(this);
			KeyInfos = new Dictionary<Key, Translations.KeyInfo>();
			foreach (Key k in Enum.GetValues(typeof(Key)))
			{
				if (KeyInfos.ContainsKey(k) || k == Key.Unknown)
				{
					continue;
				}
				switch (k)
				{
					case Key.Keypad0:
						KeyInfos.Add(k, new Translations.KeyInfo(Key.Keypad0, GetInterfaceString(HostApplication.OpenBve, new []{ "keys", "kp0"}, out _)));
						break;
					case Key.Keypad1:
						KeyInfos.Add(k, new Translations.KeyInfo(Key.Keypad1, GetInterfaceString(HostApplication.OpenBve, new []{ "keys", "kp1"}, out _)));
						break;
					case Key.Keypad2:
						KeyInfos.Add(k, new Translations.KeyInfo(Key.Keypad0, GetInterfaceString(HostApplication.OpenBve, new []{ "keys", "kp2"}, out _)));
						break;
					case Key.Keypad3:
						KeyInfos.Add(k, new Translations.KeyInfo(Key.Keypad0, GetInterfaceString(HostApplication.OpenBve, new []{ "keys", "kp3"}, out _)));
						break;
					case Key.Keypad4:
						KeyInfos.Add(k, new Translations.KeyInfo(Key.Keypad0, GetInterfaceString(HostApplication.OpenBve, new []{ "keys", "kp4"}, out _)));
						break;
					case Key.Keypad5:
						KeyInfos.Add(k, new Translations.KeyInfo(Key.Keypad0, GetInterfaceString(HostApplication.OpenBve, new []{ "keys", "kp5"}, out _)));
						break;
					case Key.Keypad6:
						KeyInfos.Add(k, new Translations.KeyInfo(Key.Keypad0, GetInterfaceString(HostApplication.OpenBve, new []{ "keys", "kp6"}, out _)));
						break;
					case Key.Keypad7:
						KeyInfos.Add(k, new Translations.KeyInfo(Key.Keypad0, GetInterfaceString(HostApplication.OpenBve, new []{ "keys", "kp7"}, out _)));
						break;
					case Key.Keypad8:
						KeyInfos.Add(k, new Translations.KeyInfo(Key.Keypad0, GetInterfaceString(HostApplication.OpenBve, new []{ "keys", "kp8"}, out _)));
						break;
					case Key.Keypad9:
						KeyInfos.Add(k, new Translations.KeyInfo(Key.Keypad0, GetInterfaceString(HostApplication.OpenBve, new []{ "keys", "kp9"}, out _)));
						break;
					case Key.KeypadDivide:
						KeyInfos.Add(k, new Translations.KeyInfo(Key.Keypad0, GetInterfaceString(HostApplication.OpenBve, new []{ "keys", "kp_divide"}, out _)));
						break;
					case Key.KeypadEnter:
						KeyInfos.Add(k, new Translations.KeyInfo(Key.Keypad0, GetInterfaceString(HostApplication.OpenBve, new []{ "keys", "kp_enter"}, out _)));
						break;
					case Key.KeypadMultiply:
						KeyInfos.Add(k, new Translations.KeyInfo(Key.Keypad0, GetInterfaceString(HostApplication.OpenBve, new []{ "keys", "kp_multiply"}, out _)));
						break;
					case Key.KeypadDecimal:
						KeyInfos.Add(k, new Translations.KeyInfo(Key.Keypad0, GetInterfaceString(HostApplication.OpenBve, new []{ "keys", "kp_period"}, out _)));
						break;
					case Key.KeypadPlus:
						KeyInfos.Add(k, new Translations.KeyInfo(Key.Keypad0, GetInterfaceString(HostApplication.OpenBve, new []{ "keys", "kp_plus"}, out _)));
						break;
					case Key.KeypadMinus:
						KeyInfos.Add(k, new Translations.KeyInfo(Key.Keypad0, GetInterfaceString(HostApplication.OpenBve, new []{ "keys", "kp_minus"}, out _)));
						break;
					case Key.BracketLeft:
						KeyInfos.Add(k, new Translations.KeyInfo(Key.Keypad0, GetInterfaceString(HostApplication.OpenBve, new []{ "keys", "leftbracket"}, out _)));
						break;
					case Key.BracketRight:
						KeyInfos.Add(k, new Translations.KeyInfo(Key.Keypad0, GetInterfaceString(HostApplication.OpenBve, new []{ "keys", "rightbracket"}, out _)));
						break;
					case Key.ShiftLeft:
						KeyInfos.Add(k, new Translations.KeyInfo(Key.Keypad0, GetInterfaceString(HostApplication.OpenBve, new []{ "keys", "lshift"}, out _)));
						break;
					case Key.ShiftRight:
						KeyInfos.Add(k, new Translations.KeyInfo(Key.Keypad0, GetInterfaceString(HostApplication.OpenBve, new []{ "keys", "rshift"}, out _)));
						break;
					case Key.ControlLeft:
						KeyInfos.Add(k, new Translations.KeyInfo(Key.Keypad0, GetInterfaceString(HostApplication.OpenBve, new []{ "keys", "lctrl"}, out _)));
						break;
					case Key.ControlRight:
						KeyInfos.Add(k, new Translations.KeyInfo(Key.Keypad0, GetInterfaceString(HostApplication.OpenBve, new []{ "keys", "rctrl"}, out _)));
						break;
					case Key.LWin:
						KeyInfos.Add(k, new Translations.KeyInfo(Key.Keypad0, GetInterfaceString(HostApplication.OpenBve, new []{ "keys", "lsuper"}, out _)));
						break;
					case Key.RWin:
						KeyInfos.Add(k, new Translations.KeyInfo(Key.Keypad0, GetInterfaceString(HostApplication.OpenBve, new []{ "keys", "rsuper"}, out _)));
						break;
					case Key.PrintScreen:
						KeyInfos.Add(k, new Translations.KeyInfo(Key.Keypad0, GetInterfaceString(HostApplication.OpenBve, new []{ "keys", "print"}, out _)));
						break;
					default:
						string ks = k.ToString().Replace("Number", string.Empty);
						string keyName = GetInterfaceString(HostApplication.OpenBve, new[] { "keys", ks.ToLowerInvariant() }, out bool keyIsTranslated);
						KeyInfos.Add(k, keyIsTranslated ? new Translations.KeyInfo(k, keyName) : new Translations.KeyInfo(k, ks));
						break;
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
			string interfaceString = GetInterfaceString(hostApplication, parameters, out bool exists);

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
					else if(string.IsNullOrEmpty(interfaceString))
					{
						// requested string does not exist in the target language at all, but does untranslated in a fallback
						interfaceString = candidateString;
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

		/// <summary>Returns the name of the language</summary>
		public override string ToString()
		{
			return Name;
		}
	}
}
