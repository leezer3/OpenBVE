using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace OpenBveApi.Interface
{
	public static partial class Translations
	{
		private class Language
		{
			/// <summary>The interface strings for this language</summary>
			internal readonly InterfaceString[] InterfaceStrings;
			/// <summary>The command information strings for this language</summary>
			internal readonly CommandInfo[] myCommandInfos;
			/// <summary>The key information strings for this language</summary>
			internal readonly KeyInfo[] KeyInfos;
			/// <summary>Returns the number of translated strings contained in the language</summary>
			internal int InterfaceStringCount => InterfaceStrings.Length;

			/// <summary>The language name</summary>
			private readonly string Name;
			/// <summary>The language flag</summary>
			internal readonly string Flag;
			/// <summary>The language code</summary>
			internal readonly string LanguageCode;
			/// <summary>The language codes on which to fall-back if a string is not found in this language(In order from best to worst)</summary>
			/// en-US should always be present in this list
			internal readonly List<string> FallbackCodes;

			private class XliffFile
			{
				internal class Unit
				{
					internal readonly string Id;
					internal readonly string Value;

					internal Unit(XNamespace xmlns, XElement unit, string languageCode)
					{
						XElement source = unit.Element(xmlns + "source");
						XElement target = unit.Element(xmlns + "target");

						Id = (string)unit.Attribute("id");

						if (target == null && languageCode != "en-US")
						{
							Value = ((string)source).Replace("\\r\\n", Environment.NewLine).Replace("\\x20", " ");
							return;
						}
						
						Value = (languageCode != "en-US" ? (string)target : (string)source).Replace("\\r\\n", Environment.NewLine).Replace("\\x20", " ");
						if (string.IsNullOrEmpty(Value))
						{
							//if target is empty / null, let's use the untranslated value https://github.com/leezer3/OpenBVE/issues/663
							Value = ((string)source).Replace("\\r\\n", Environment.NewLine).Replace("\\x20", " ");
						}
					}
				}

				internal class Group
				{
					internal readonly string Id;
					internal readonly Group[] Groups;
					internal readonly Unit[] Units;

					internal Group(XNamespace xmlns, XElement group, string languageCode)
					{
						Id = (string)group.Attribute("id");
						Groups = group.Elements(xmlns + "group").Select(g => new Group(xmlns, g, languageCode)).ToArray();
						Units = group.Elements(xmlns + "trans-unit").Select(t => new Unit(xmlns, t, languageCode)).Where(t => !string.IsNullOrEmpty(t.Value)).ToArray();
					}
				}

				internal readonly Group[] Groups;
				internal readonly Unit[] Units;

				internal XliffFile(TextReader reader, string languageCode)
				{
					// ReSharper disable PossibleNullReferenceException
					// Exceptions will propagate up the load chain anyway
					XDocument xml = XDocument.Load(reader);
					XNamespace xmlns = xml.Root.Name.Namespace;
					XElement body = xml.Root.Element(xmlns + "file").Element(xmlns + "body");
					Groups = body.Elements(xmlns + "group").Select(g => new Group(xmlns, g, languageCode)).ToArray();
					Units = body.Elements(xmlns + "trans-unit").Select(t => new Unit(xmlns, t, languageCode)).Where(t => !string.IsNullOrEmpty(t.Value)).ToArray();
					// ReSharper restore PossibleNullReferenceException
				}
			}

			/// <summary>Creates a new language from a file stream</summary>
			/// <param name="languageReader">The file stream</param>
			/// <param name="languageCode">The language code</param>
			internal Language(TextReader languageReader, string languageCode)
			{
				Name = "Unknown";
				LanguageCode = languageCode;
				FallbackCodes = new List<string> { "en-US" };
				myCommandInfos = new CommandInfo[CommandInfos.Length];
				KeyInfos = new KeyInfo[TranslatedKeys.Length];
				Array.Copy(CommandInfos, myCommandInfos, myCommandInfos.Length);
				Array.Copy(TranslatedKeys, KeyInfos, TranslatedKeys.Length);

				string prefix = string.Empty;
				XliffFile file = new XliffFile(languageReader, languageCode);
				List<InterfaceString> strings = new List<InterfaceString>();

				ExportUnits(prefix, file.Units, strings);

				foreach (XliffFile.Group group in file.Groups)
				{
					ExportGroup(prefix, group, strings);
				}

				InterfaceString[] groupLanguage = strings.Where(s => s.Name.StartsWith("language_")).ToArray();

				foreach (var interfaceString in groupLanguage)
				{
					string key = interfaceString.Name.Split('_')[1];

					switch (key)
					{
						case "name":
							Name = interfaceString.Text;
							strings.Remove(interfaceString);
							break;
						case "flag":
							Flag = interfaceString.Text;
							strings.Remove(interfaceString);
							break;
					}
				}

				InterfaceString[] groupOpenBve = strings.Where(s => s.Name.StartsWith("openbve_")).ToArray();

				foreach (var interfaceString in groupOpenBve)
				{
					string section = interfaceString.Name.Split('_')[1];
					string key = string.Join("_", interfaceString.Name.Split('_').Skip(2));

					switch (section)
					{
						case "commands":
							for (int k = 0; k < myCommandInfos.Length; k++)
							{
								if (string.Compare(myCommandInfos[k].Name, key, StringComparison.OrdinalIgnoreCase) == 0)
								{
									myCommandInfos[k].Description = interfaceString.Text;
									strings.Remove(interfaceString);
									break;
								}
							}
							break;
						case "keys":
							for (int k = 0; k < KeyInfos.Length; k++)
							{
								if (string.Compare(KeyInfos[k].Name, key, StringComparison.OrdinalIgnoreCase) == 0)
								{
									KeyInfos[k].Description = interfaceString.Text;
									strings.Remove(interfaceString);
									break;
								}
							}
							break;
						case "fallback":
							switch (key)
							{
								case "language":
									FallbackCodes.Add(interfaceString.Text);
									strings.Remove(interfaceString);
									break;
							}
							break;
					}
				}

				InterfaceStrings = strings.ToArray();

				for (int i = 0; i < InterfaceStrings.Length; i++)
				{
					if (InterfaceStrings[i].Name.StartsWith("openbve_"))
					{
						InterfaceStrings[i].Name = InterfaceStrings[i].Name.Replace("openbve_", string.Empty);
					}
				}

				for (int i = 0; i < myCommandInfos.Length; i++)
				{
					// try to set any untranslated commandinfo descriptions to something other than N/A
					if (myCommandInfos[i].Command != Command.None && myCommandInfos[i].Description == "N/A")
					{
						for (int j = 0; j < Translations.AvailableLanguages.Count; j++)
						{
							if (Translations.AvailableLanguages[j].LanguageCode == "en-US")
							{
								for (int k = 0; k < Translations.AvailableLanguages[j].myCommandInfos.Length; k++)
								{
									if (Translations.AvailableLanguages[j].myCommandInfos[k].Command == myCommandInfos[i].Command)
									{
										myCommandInfos[i].Description = Translations.AvailableLanguages[j].myCommandInfos[k].Description;
										break;
									}
								}
								
							}
						}
					}

				}
			}

			private void ExportUnits(string prefix, XliffFile.Unit[] units, List<InterfaceString> strings)
			{
				strings.AddRange(units.Select(u => new InterfaceString
				{
					Name = prefix + u.Id,
					Text = u.Value
				}));
			}

			private void ExportGroup(string prefix, XliffFile.Group group, List<InterfaceString> strings)
			{
				prefix += group.Id + "_";

				ExportUnits(prefix, group.Units, strings);

				foreach (XliffFile.Group childGroup in group.Groups)
				{
					ExportGroup(prefix, childGroup, strings);
				}
			}


			/// <summary>Always returns the textual name of the language</summary>
			public override string ToString()
			{
				return Name;
			}
		}
	}
}
