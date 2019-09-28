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
			/// <summary>The quick-reference strings for this language</summary>
			internal readonly InterfaceQuickReference myQuickReferences;
			/// <summary>Returns the number of translated strings contained in the language</summary>
			internal int InterfaceStringCount
			{
				get
				{
					return InterfaceStrings.Length;
				}
			}
			/// <summary>The language name</summary>
			internal readonly string Name;
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

						if (target == null && languageCode != "en-US")
						{
							return;
						}

						Id = (string)unit.Attribute("id");
						Value = (languageCode != "en-US" ? (string)target : (string)source).Replace("\\r\\n", Environment.NewLine).Replace("\\x20", " ");
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

				internal XliffFile(Stream stream, string languageCode)
				{
					XDocument xml = XDocument.Load(stream);
					XNamespace xmlns = xml.Root.Name.Namespace;
					XElement body = xml.Root.Element(xmlns + "file").Element(xmlns + "body");

					Groups = body.Elements(xmlns + "group").Select(g => new Group(xmlns, g, languageCode)).ToArray();
					Units = body.Elements(xmlns + "trans-unit").Select(t => new Unit(xmlns, t, languageCode)).Where(t => !string.IsNullOrEmpty(t.Value)).ToArray();
				}
			}

			/// <summary>Creates a new language from a file stream</summary>
			/// <param name="languageStream">The file stream</param>
			/// <param name="languageCode">The language code</param>
			internal Language(Stream languageStream, string languageCode)
			{
				Name = "Unknown";
				LanguageCode = languageCode;
				FallbackCodes = new List<string> { "en-US" };
				myCommandInfos = new CommandInfo[CommandInfos.Length];
				KeyInfos = new KeyInfo[TranslatedKeys.Length];
				myQuickReferences = new InterfaceQuickReference();
				Array.Copy(CommandInfos, myCommandInfos, myCommandInfos.Length);
				Array.Copy(TranslatedKeys, KeyInfos, TranslatedKeys.Length);

				string prefix = string.Empty;
				XliffFile file = new XliffFile(languageStream, languageCode);
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
						case "handles":
							switch (key)
							{
								case "forward":
									myQuickReferences.HandleForward = interfaceString.Text;
									strings.Remove(interfaceString);
									break;
								case "neutral":
									myQuickReferences.HandleNeutral = interfaceString.Text;
									strings.Remove(interfaceString);
									break;
								case "backward":
									myQuickReferences.HandleBackward = interfaceString.Text;
									strings.Remove(interfaceString);
									break;
								case "power":
									myQuickReferences.HandlePower = interfaceString.Text;
									strings.Remove(interfaceString);
									break;
								case "powernull":
									myQuickReferences.HandlePowerNull = interfaceString.Text;
									strings.Remove(interfaceString);
									break;
								case "brake":
									myQuickReferences.HandleBrake = interfaceString.Text;
									strings.Remove(interfaceString);
									break;
								case "locobrake":
									myQuickReferences.HandleLocoBrake = interfaceString.Text;
									strings.Remove(interfaceString);
									break;
								case "brakenull":
									myQuickReferences.HandleBrakeNull = interfaceString.Text;
									strings.Remove(interfaceString);
									break;
								case "release":
									myQuickReferences.HandleRelease = interfaceString.Text;
									strings.Remove(interfaceString);
									break;
								case "lap":
									myQuickReferences.HandleLap = interfaceString.Text;
									strings.Remove(interfaceString);
									break;
								case "service":
									myQuickReferences.HandleService = interfaceString.Text;
									strings.Remove(interfaceString);
									break;
								case "emergency":
									myQuickReferences.HandleEmergency = interfaceString.Text;
									strings.Remove(interfaceString);
									break;
								case "holdbrake":
									myQuickReferences.HandleHoldBrake = interfaceString.Text;
									strings.Remove(interfaceString);
									break;
							}
							break;
						case "doors":
							switch (key)
							{
								case "left":
									myQuickReferences.DoorsLeft = interfaceString.Text;
									strings.Remove(interfaceString);
									break;
								case "right":
									myQuickReferences.DoorsRight = interfaceString.Text;
									strings.Remove(interfaceString);
									break;
							}
							break;
						case "misc":
							switch (key)
							{
								case "score":
									myQuickReferences.Score = interfaceString.Text;
									strings.Remove(interfaceString);
									break;
							}
							break;
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
