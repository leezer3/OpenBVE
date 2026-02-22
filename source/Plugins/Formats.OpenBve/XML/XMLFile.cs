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

using OpenBveApi;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Path = OpenBveApi.Path;

namespace Formats.OpenBve.XML
{
	/// <summary>Root block for a .XML type file</summary>
	public class XMLFile<T1, T2> : Block<T1, T2> where T1 : struct, Enum where T2 : struct, Enum
	{
		public XMLFile(string fileName, string rootPath, HostInterface currentHost) : this(File.ReadAllText(fileName, TextEncoding.GetSystemEncodingFromFile(fileName)), Path.GetDirectoryName(fileName), rootPath, currentHost)
		{
		}

		public XMLFile(string text, string path, string rootPath, HostInterface currentHost) : base(-1, default, currentHost)
		{
			XmlDocument currentXML = new XmlDocument();
			currentXML.LoadXml(text);
			if (currentXML.DocumentElement != null)
			{
				XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes(rootPath);
				if (DocumentNodes != null)
				{
					foreach (XmlNode rootNode in DocumentNodes)
					{
						foreach (XmlNode node in rootNode.ChildNodes)
						{
							if (Enum.TryParse(node.LocalName, true, out T1 key))
							{
								if (node.ChildNodes.OfType<XmlNode>().Any(x => x.NodeType != XmlNodeType.Text))
								{
									subBlocks.Add(new XMLSection<T1, T2>(node, key, currentHost));
									continue;
								}

								string childFile;
								try
								{
									childFile = Path.CombineFile(path, node.InnerText);
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
									}
									else
									{
										currentHost.AddMessage(MessageType.Warning, false, "Child XML File " + node.InnerText + " does not contain the expected node " + node.LocalName);
										continue;
									}
								}
								// HACK: If file does not exist, add it as a value instead (allows for using the same thing as both a value and section key)
							}

							if (Enum.TryParse(node.LocalName, true, out T2 valueKey))
							{
								keyValuePairs.TryAdd(valueKey, new KeyValuePair<int, string>(-1, node.InnerText));
							}
							else
							{
								if (node.HasChildNodes)
								{
									currentHost.AddMessage(MessageType.Warning, false, "Unexpected node " + node.LocalName + " encountered in XML file ");
								}
								else
								{
									if (!(node is XmlComment))
									{
										currentHost.AddMessage(MessageType.Warning, false, "Unexpected value " + node.LocalName + " encountered in XML file ");
									}
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
		public XMLSection(XmlNode node, T1 myKey, HostInterface currentHost) : base(-1, myKey, currentHost)
		{
			if (node.HasChildNodes)
			{
				foreach (XmlNode childNode in node.ChildNodes)
				{
					if (Enum.TryParse(childNode.LocalName, true, out T1 key))
					{
						if (childNode.HasChildNodes)
						{
							subBlocks.Add(new XMLSection<T1, T2>(childNode, key, currentHost));
						}
						else
						{
							// reference to child XML file
						}
					}
					else if (Enum.TryParse(childNode.LocalName, true, out T2 valueKey))
					{
						keyValuePairs.TryAdd(valueKey, new KeyValuePair<int, string>(-1, childNode.InnerText));
					}
					else
					{
						if (childNode.HasChildNodes)
						{
							currentHost.AddMessage(MessageType.Warning, false, "Unexpected node " + childNode.LocalName + " encountered in XML file ");
						}
						else
						{
							if (!(childNode is XmlComment))
							{
								currentHost.AddMessage(MessageType.Warning, false, "Unexpected value " + childNode.LocalName + " encountered in XML file ");
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
