//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2025, Christopher Lees, The OpenBVE Project
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

using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using OpenBveApi.Math;
using Path = OpenBveApi.Path;

namespace Formats.OpenBve.XML
{
	/// <summary>Root block for a .XML type file</summary>
	public class XMLFile<T1, T2> : Block<T1, T2> where T1 : struct, Enum where T2 : struct, Enum
	{
		public XMLFile(string fileName, string rootPath, HostInterface currentHost) : this(XDocument.Load(fileName, LoadOptions.SetLineInfo), fileName, rootPath, currentHost)
		{
		}
		
		public XMLFile(XDocument currentXML, string fileName, string rootPath, HostInterface currentHost) : base(-1, default, fileName, currentHost)
		{
			string path = Path.GetDirectoryName(fileName);
			IEnumerable<XElement> DocumentNodes = currentXML.XPathSelectElements(rootPath);
			if (DocumentNodes.Any())
			{
				foreach (XElement rootNode in DocumentNodes)
				{
					foreach (XElement element in rootNode.Elements())
					{
						if (Enum.TryParse(element.Name.LocalName, true, out T1 key))
						{
							if (element.HasElements)
							{
								subBlocks.Add(new XMLSection<T1, T2>(fileName, element, key, currentHost));
								continue;
							}

							string childFile;
							try
							{
								childFile = Path.CombineFile(path, element.Value);
							}
							catch
							{
								// ignored at the minute, ?? possibly invalid filename error ??
								continue;
							}

							if (File.Exists(childFile) &&
							    !childFile.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
							{
								XMLFile<T1, T2> childXML;
								try
								{
									childXML = new XMLFile<T1, T2>(childFile, "/openBVE", currentHost);
								}
								catch
								{
									// ignored
									continue;
								}

								if (childXML.ReadBlock(key, out Block<T1, T2> childFileBlock))
								{
									subBlocks.Add(childFileBlock);
								}
								else
								{
									currentHost.AddMessage(MessageType.Warning, false,
										"Child XML File " + element.Value + " does not contain the expected node " +
										element.Name.LocalName);
									continue;
								}
							}
							// HACK: If file does not exist, add it as a value instead (allows for using the same thing as both a value and section key)
						}

						if (Enum.TryParse(element.Name.LocalName, true, out T2 valueKey))
						{
							keyValuePairs.TryAdd(valueKey, new KeyValuePair<int, string>(((IXmlLineInfo)element).LineNumber, element.Value));
						}
						else
						{
							if (element.HasElements)
							{
								currentHost.AddMessage(MessageType.Warning, false,
									"Unexpected node " + element.Name.LocalName + " encountered in XML file " + FileName);
							}
							else
							{
								if (element.NodeType != XmlNodeType.Comment)
								{
									currentHost.AddMessage(MessageType.Warning, false,
										"Unexpected value " + element.Name.LocalName + " encountered in XML file " + FileName);
								}
							}
						}
					}
				}
			}

		}

		public override Block<T1, T2> ReadNextBlock()
		{
			Block<T1, T2> block = subBlocks[0];
			subBlocks.RemoveAt(0);
			return block;
		}
	}

	public class XMLSection<T1, T2> : Block<T1, T2> where T1 : struct, Enum where T2 : struct, Enum
	{
		public XMLSection(string fileName, XElement element, T1 myKey, HostInterface currentHost) : base(-1, myKey, fileName, currentHost)
		{
			string path = Path.GetDirectoryName(fileName);
			if (element.HasElements)
			{
				foreach (XElement childElement in element.Elements())
				{
					if (Enum.TryParse(childElement.Name.LocalName, true, out T1 key))
					{
						if (childElement.HasElements)
						{
							subBlocks.Add(new XMLSection<T1, T2>(path, childElement, key, currentHost));
							continue;
						}
						string childFile;
						try
						{
							childFile = Path.CombineFile(path, childElement.Value);
						}
						catch
						{
							// ignored at the minute, ?? possibly invalid filename error ??
							continue;
						}

						if (File.Exists(childFile) && !childFile.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
						{
							XMLFile<T1, T2> childXML;
							try
							{
								childXML = new XMLFile<T1, T2>(childFile, "/openBVE", currentHost);
							}
							catch
							{
								// ignored
								continue;
							}

							if (childXML.ReadBlock(key, out Block<T1, T2> childFileBlock))
							{
								subBlocks.Add(childFileBlock);
								continue;
							}

							currentHost.AddMessage(MessageType.Warning, false, "Child XML File " + childElement.Value + " does not contain the expected node " + childElement.Name.LocalName);
							continue;
						}
						// HACK: If file does not exist, add it as a value instead (allows for using the same thing as both a value and section key)
					}
					
					if (Enum.TryParse(childElement.Name.LocalName, true, out T2 valueKey))
					{
						XAttribute numberAttribute = childElement.Attribute("Number");
						if (numberAttribute != null)
						{
							if (int.TryParse(numberAttribute.Value, out int num))
							{
								indexedValues.TryAdd(num, new KeyValuePair<int, string>(((IXmlLineInfo)childElement).LineNumber, childElement.Value));
							}
						}
						else
						{
							keyValuePairs.TryAdd(valueKey, new KeyValuePair<int, string>(((IXmlLineInfo)childElement).LineNumber, childElement.Value));
						}
					}
					else
					{
						if (childElement.HasElements)
						{
							currentHost.AddMessage(MessageType.Warning, false, "Unexpected node " + childElement.Name.LocalName + " encountered in XML file " + fileName);
						}
						else
						{
							if (childElement.NodeType != XmlNodeType.Comment)
							{
								currentHost.AddMessage(MessageType.Warning, false, "Unexpected value " + childElement.Name.LocalName + " encountered in XML file " + fileName);
							}
						}
					}
				}
			}
		}

		public override bool GetPath(T2 key, string absolutePath, out string finalPath)
		{
			if (keyValuePairs.TryRemove(key, out var value))
			{
				if (!Path.ContainsInvalidChars(value.Value))
				{

					string relativePath = value.Value;
					try
					{
						finalPath = Path.CombineFile(absolutePath, relativePath);
					}
					catch
					{
						finalPath = string.Empty;
					}

					if (File.Exists(finalPath))
					{
						return true;
					}

					try
					{
						finalPath = Path.CombineFile(absolutePath, relativePath);
					}
					catch
					{
						finalPath = string.Empty;
						return false;
					}

					if (File.Exists(finalPath))
					{
						return true;
					}

					currentHost.AddMessage(MessageType.Warning, false, "File " + value.Value + " was not found in Key " + key + " in Section " + Key + " at line " + value.Key);
					finalPath = string.Empty;
					return false;

				}

				currentHost.AddMessage(MessageType.Warning, false, "Path contains invalid characters for " + key + " in Section " + Key + " at line " + value.Key);
			}
			finalPath = string.Empty;
			return false;
		}
	}
}
