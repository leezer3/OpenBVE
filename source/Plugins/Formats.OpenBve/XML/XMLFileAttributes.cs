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
using System.Xml.Linq;
using System.Xml.XPath;
using Path = OpenBveApi.Path;


namespace Formats.OpenBve
{
	/// <summary>Root block for an XML file using attributes to contain values</summary>
	public class AttributedXMLFile<T1, T2> : Block<T1, T2> where T1 : struct, Enum where T2 : struct, Enum
	{
		public AttributedXMLFile(string fileName, string rootPath, HostInterface currentHost) : this(XDocument.Load(fileName, LoadOptions.SetLineInfo), fileName, rootPath, currentHost)
		{
		}

		public AttributedXMLFile(string text, string fileName, string rootPath, HostInterface currentHost) : this(XDocument.Parse(text), fileName, rootPath, currentHost)
		{
		}

		public AttributedXMLFile(XDocument currentXML, string fileName, string rootPath, HostInterface currentHost) :
			base(-1, default, fileName, currentHost)
		{
			IEnumerable<XElement> DocumentNodes = currentXML.XPathSelectElements(rootPath).ToList();
			if (DocumentNodes.Any())
			{
				foreach (XElement rootNode in DocumentNodes)
				{
					foreach (XElement element in rootNode.Elements())
					{
						if (element.Name.LocalName.Equals("Props", StringComparison.InvariantCultureIgnoreCase))
						{
							foreach (XAttribute attribute in element.Attributes())
							{
								if (Enum.TryParse(attribute.Name.LocalName, true, out T2 at))
								{
									keyValuePairs.TryAdd(at, new KeyValuePair<int, string>(-1, attribute.Value));
								}
							}
							continue;
						}
						if (Enum.TryParse(element.Name.LocalName, true, out T1 key))
						{
							subBlocks.Add(new AttributedXMLSection<T1, T2>(fileName, element, key, currentHost));
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
					if (string.IsNullOrEmpty(relativePath))
					{
						finalPath = string.Empty;
						return false;
					}

					if (Path.IsAbsolutePath(relativePath))
					{
						relativePath = relativePath.TrimStart('/', '\\');
					}

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

	public class AttributedXMLSection<T1, T2> : Block<T1, T2> where T1 : struct, Enum where T2 : struct, Enum
	{
		public AttributedXMLSection(string fileName, XElement element, T1 myKey, HostInterface host) : base(-1, myKey, fileName, host)
		{
			if (element.HasAttributes)
			{
				foreach (XAttribute attribute in element.Attributes())
				{
					if (Enum.TryParse(attribute.Name.LocalName, true, out T2 at))
					{
						keyValuePairs.TryAdd(at, new KeyValuePair<int, string>(-1, attribute.Value));
					}
				}
			}
			else if (element.HasElements)
			{
				foreach (XElement childElement in element.Elements())
				{
					if (childElement.Name.LocalName.Equals("Props", StringComparison.InvariantCultureIgnoreCase))
					{
						foreach (XAttribute attribute in childElement.Attributes())
						{
							if (Enum.TryParse(attribute.Name.LocalName, true, out T2 at))
							{
								keyValuePairs.TryAdd(at, new KeyValuePair<int, string>(-1, attribute.Value));
							}
						}
						continue;
					}
					if (Enum.TryParse(childElement.Name.LocalName, true, out T1 key))
					{
						subBlocks.Add(new AttributedXMLSection<T1, T2>(fileName, childElement, key, host));
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
					if (string.IsNullOrEmpty(relativePath))
					{
						finalPath = string.Empty;
						return false;
					}

					if (Path.IsAbsolutePath(relativePath))
					{
						relativePath = relativePath.TrimStart('/', '\\');
					}

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
