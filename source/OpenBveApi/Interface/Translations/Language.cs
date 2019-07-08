using System;
using System.Collections.Generic;
using System.IO;
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

			/// <summary>Creates a new language from a file stream</summary>
			/// <param name="languageStream">The file stream</param>
			/// <param name="languageCode">The language code</param>
			internal Language(Stream languageStream, string languageCode)
			{
				Name = "Unknown";
				LanguageCode = languageCode;
				FallbackCodes = new List<string> { "en-US" };
				InterfaceStrings = new InterfaceString[16];
				myCommandInfos = new CommandInfo[CommandInfos.Length];
				KeyInfos = new KeyInfo[TranslatedKeys.Length];
				myQuickReferences = new InterfaceQuickReference();
				Array.Copy(CommandInfos, myCommandInfos, myCommandInfos.Length);
				Array.Copy(TranslatedKeys, KeyInfos, TranslatedKeys.Length);

				XDocument xml = XDocument.Load(languageStream);
				XNamespace xmlns = xml.Root.Name.Namespace;
				XElement body = xml.Root.Element(xmlns + "file").Element(xmlns + "body");

				int LoadedStringCount = 0;

				foreach (XElement groupProduct in body.Elements(xmlns + "group"))
				{
					switch (groupProduct.Attribute("id").Value)
					{
						case "language":
							foreach (XElement trans_unit in groupProduct.Elements(xmlns + "trans-unit"))
							{
								XElement source = trans_unit.Element(xmlns + "source");
								XElement target = trans_unit.Element(xmlns + "target");

								if (target == null && LanguageCode != "en-US")
								{
									continue;
								}

								string key = trans_unit.Attribute("id").Value;
								string text = LanguageCode != "en-US" ? target.Value : source.Value;

								switch (key)
								{
									case "name":
										Name = text;
										break;
									case "flag":
										Flag = text;
										break;
								}
							}
							break;
						default:
							{
								string productName = groupProduct.Attribute("id").Value;

								if (productName == "openbve")
								{
									foreach (XElement group in groupProduct.Elements(xmlns + "group"))
									{
										foreach (XElement trans_unit in group.Elements(xmlns + "trans-unit"))
										{
											XElement source = trans_unit.Element(xmlns + "source");
											XElement target = trans_unit.Element(xmlns + "target");

											if (target == null && LanguageCode != "en-US")
											{
												continue;
											}

											string section = group.Attribute("id").Value;
											string key = trans_unit.Attribute("id").Value;
											string text = LanguageCode != "en-US" ? target.Value : source.Value;
											text = text.Replace("\\n", Environment.NewLine);
											switch (section)
											{
												case "handles":
													switch (key)
													{
														case "forward":
															myQuickReferences.HandleForward = text;
															break;
														case "neutral":
															myQuickReferences.HandleNeutral = text;
															break;
														case "backward":
															myQuickReferences.HandleBackward = text;
															break;
														case "power":
															myQuickReferences.HandlePower = text;
															break;
														case "powernull":
															myQuickReferences.HandlePowerNull = text;
															break;
														case "brake":
															myQuickReferences.HandleBrake = text;
															break;
														case "locobrake":
															myQuickReferences.HandleLocoBrake = text;
															break;
														case "brakenull":
															myQuickReferences.HandleBrakeNull = text;
															break;
														case "release":
															myQuickReferences.HandleRelease = text;
															break;
														case "lap":
															myQuickReferences.HandleLap = text;
															break;
														case "service":
															myQuickReferences.HandleService = text;
															break;
														case "emergency":
															myQuickReferences.HandleEmergency = text;
															break;
														case "holdbrake":
															myQuickReferences.HandleHoldBrake = text;
															break;
													}
													break;
												case "doors":
													switch (key)
													{
														case "left":
															myQuickReferences.DoorsLeft = text;
															break;
														case "right":
															myQuickReferences.DoorsRight = text;
															break;
													}
													break;
												case "misc":
													switch (key)
													{
														case "score":
															myQuickReferences.Score = text;
															break;
													}
													break;
												case "commands":
													for (int k = 0; k < myCommandInfos.Length; k++)
													{
														if (string.Compare(myCommandInfos[k].Name, key, StringComparison.OrdinalIgnoreCase) == 0)
														{
															myCommandInfos[k].Description = text;
															break;
														}
													}
													break;
												case "keys":
													for (int k = 0; k < KeyInfos.Length; k++)
													{
														if (string.Compare(KeyInfos[k].Name, key, StringComparison.OrdinalIgnoreCase) == 0)
														{
															KeyInfos[k].Description = text;
															break;
														}
													}
													break;
												case "fallback":
													switch (key)
													{
														case "language":
															FallbackCodes.Add(text);
															break;
													}
													break;
												default:
													if (LoadedStringCount >= InterfaceStrings.Length)
													{
														Array.Resize(ref InterfaceStrings, InterfaceStrings.Length << 1);
													}

													InterfaceStrings[LoadedStringCount].Name = string.Format("{0}_{1}", section, key);
													InterfaceStrings[LoadedStringCount].Text = text;
													LoadedStringCount++;
													break;
											}
										}
									}
								}
								else
								{
									foreach (XElement group in groupProduct.Elements(xmlns + "group"))
									{
										foreach (XElement trans_unit in group.Elements(xmlns + "trans-unit"))
										{
											XElement source = trans_unit.Element(xmlns + "source");
											XElement target = trans_unit.Element(xmlns + "target");

											if (target == null && LanguageCode != "en-US")
											{
												continue;
											}

											string section = group.Attribute("id").Value;
											string key = trans_unit.Attribute("id").Value;
											string text = LanguageCode != "en-US" ? target.Value : source.Value;
											text = text.Replace("\\n", Environment.NewLine);
											if (LoadedStringCount >= InterfaceStrings.Length)
											{
												Array.Resize(ref InterfaceStrings, InterfaceStrings.Length << 1);
											}

											InterfaceStrings[LoadedStringCount].Name = string.Format("{0}_{1}_{2}", productName, section, key);
											InterfaceStrings[LoadedStringCount].Text = text;
											LoadedStringCount++;
										}
									}
								}
							}
							break;
					}
				}

				Array.Resize(ref InterfaceStrings, LoadedStringCount);
			}

			/// <summary>Always returns the textual name of the language</summary>
			public override string ToString()
			{
				return Name;
			}
		}
	}
}
