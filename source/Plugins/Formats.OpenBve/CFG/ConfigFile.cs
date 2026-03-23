//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020, Christopher Lees, The OpenBVE Project
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

using OpenBveApi.FunctionScripting;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenBveApi;
using Path = OpenBveApi.Path;

namespace Formats.OpenBve
{
	/// <summary>Root block for a .CFG type file</summary>
	public class ConfigFile<T1, T2> : Block<T1, T2> where T1 : struct, Enum where T2 : struct, Enum
	{
		/// <summary>The version of the file (if set)</summary>
		public readonly double Version;
		public ConfigFile(string fileName, HostInterface currentHost, string expectedHeader = null, double minVersion = 0, double maxVersion = 0, bool defaultFirstBlock = false) 
			: this(File.ReadAllLines(fileName, TextEncoding.GetSystemEncodingFromFile(fileName)), fileName, currentHost, expectedHeader, minVersion, maxVersion, defaultFirstBlock)
		{
		}

		public ConfigFile(string[] lines, string fileName, HostInterface currentHost, string expectedHeader = null, double minVersion = 0, double maxVersion = 0, bool defaultFirstBlock = false) : base(-1, default, fileName, currentHost)
		{
			List<string> blockLines = new List<string>();
			bool addToBlock = defaultFirstBlock;
			int idx = -1;
			int previousIdx = -1;
			T1 previousSection = default(T1);

			// ReSharper disable once InconsistentNaming
			bool headerOK = string.IsNullOrEmpty(expectedHeader);

			//string 

			int startingLine = 0;

			for (int i = 0; i < lines.Length; i++)
			{
				int j = lines[i].IndexOf(';');
				if (j >= 0)
				{
					lines[i] = lines[i].Substring(0, j).Trim();
				}
				else
				{
					lines[i] = lines[i].Trim();
				}
				if (headerOK == false)
				{
					int vi = lines[i].Length - 1;
					if (minVersion > 0 || maxVersion > 0)
					{
						while (char.IsDigit(lines[i][vi]) || lines[i][vi] == '.')
						{
							vi--;
						}

						Version = double.Parse(lines[i].Substring(vi));
						lines[i] = lines[i].Substring(0, vi);
						if (Version < minVersion || Version > maxVersion)
						{
							currentHost.AddMessage(MessageType.Error, false, "Expected a version between " + minVersion + " and " + maxVersion + " , found " + Version);
                        }
					}
					
					if (!string.IsNullOrEmpty(lines[i]) && string.Compare(lines[i], expectedHeader, StringComparison.OrdinalIgnoreCase) == 0)
					{
						headerOK = true;
					}
				}

				if (lines[i].StartsWith("[") && lines[i].EndsWith("]"))
				{
					if (!headerOK)
					{
						currentHost.AddMessage(MessageType.Error, false, "The expected header " + expectedHeader + " was not found.");
						headerOK = true;
					}
					// n.b. remove spaces to allow parsing to an enum
					string sct = lines[i].Trim().Trim('[', ']').Replace(" ", "");

					if (char.IsDigit(sct[sct.Length - 1]))
					{
						int c = sct.Length - 1;
						while (char.IsDigit(sct[c]) && c > 0)
						{
							c--;
						}

						c++;


						if (!int.TryParse(sct.Substring(c, sct.Length - c), out idx) || idx < 0)
						{
							currentHost.AddMessage(MessageType.Error, false, "Invalid index encountered in Section " + sct + " at line " + i);
							idx = -1;
						}
						sct = sct.Substring(0, c);

					}
					if (!Enum.TryParse(sct, true, out T1 currentSection))
					{
						addToBlock = false;
						currentHost.AddMessage(MessageType.Error, false, "Unknown Section " + sct + " encountered at line " + i);
					}
					else
					{
						addToBlock = true;
					}

					if (blockLines.Count > 0)
					{
						subBlocks.Add(new ConfigSection<T1, T2>(previousIdx, startingLine + 1, previousSection, blockLines.ToArray(), fileName, currentHost));
						blockLines.Clear();
					}
					startingLine = i;
					previousSection = currentSection;
					previousIdx = idx;
				}
				else
				{
					if (addToBlock)
					{
						blockLines.Add(lines[i]);
					}
				}
			}
			// final block
			if (blockLines.Count > 0)
			{
				subBlocks.Add(new ConfigSection<T1, T2>(idx, startingLine + 1, previousSection, blockLines.ToArray(), fileName, currentHost));
			}
		}
	}

	public class ConfigSection<T1, T2> : Block<T1, T2> where T1 : struct, Enum where T2 : struct, Enum
	{
		public readonly T1 Token;

		
		public override Block<T1, T2> ReadNextBlock()
		{
			currentHost.AddMessage(MessageType.Error, false, "A section in a CFG file cannot contain sub-blocks.");
			return null;
		}

		public override bool ReadBlock(T1 blockToRead, out Block<T1, T2> block)
		{
			currentHost.AddMessage(MessageType.Error, false, "A section in a CFG file cannot contain sub-blocks.");
			block = null;
			return false;
		}

		public override bool ReadBlock(T1[] validBlocks, out Block<T1, T2> block)
		{
			currentHost.AddMessage(MessageType.Error, false, "A section in a CFG file cannot contain sub-blocks.");
			block = null;
			return false;
		}

		public override List<Block<T1, T2>> ReadBlocks(T1[] blocks)
		{
			currentHost.AddMessage(MessageType.Error, false, "A section in a CFG file cannot contain sub-blocks.");
			return null;
		}
		
		internal ConfigSection(int myIndex, int startingLine, T1 myKey, string[] myLines, string fileName, HostInterface currentHost) : base(myIndex, myKey, fileName, currentHost)
		{
			for (int i = 0; i < myLines.Length; i++)
			{
				int j = myLines[i].IndexOf("=", StringComparison.Ordinal);
				if (j > 0)
				{
					string a = myLines[i].Substring(0, j).TrimEnd();
					string b = myLines[i].Substring(j + 1).TrimStart();
					if (int.TryParse(a, out var idx))
					{
						if (idx >= 0)
						{
							if (indexedValues.ContainsKey(idx))
							{
								currentHost.AddMessage(MessageType.Warning, false, "Duplicate index " + idx + " encountered in Section " + myKey + " at line " + i + startingLine);
								indexedValues[idx] = new KeyValuePair<int, string>(i + startingLine, b);
							}
							else
							{
								indexedValues.TryAdd(idx, new KeyValuePair<int, string>(i + startingLine, b));
							}

						}
						else
						{
							currentHost.AddMessage(MessageType.Error, false, "Invalid index " + idx + " encountered in Section " + myKey + " at line " + i + startingLine);
						}

					}
					else if (Enum.TryParse(a.Replace(" ", ""), true, out T2 key))
					{
						keyValuePairs.TryAdd(key, new KeyValuePair<int, string>(i + startingLine, b));
					}
					else
					{
						currentHost.AddMessage(MessageType.Error, false, "Unknown Key " + a + " encountered in Section " + myKey + " at line " + i + startingLine);
					}
				}
				else
				{
					if (!string.IsNullOrEmpty(myLines[i]))
					{
						rawValues.Enqueue(new KeyValuePair<int, string>(i + startingLine, myLines[i]));
					}
				}
			}
		}

		public override bool GetFunctionScript(T2 key, out AnimationScript function)
		{
			if (keyValuePairs.TryRemove(key, out var script))
			{
				try
				{
					bool isInfix = key.ToString().IndexOf("RPN", StringComparison.Ordinal) == -1;
					function = new FunctionScript(currentHost, script.Value, isInfix);
					return true;
				}
				catch
				{
					currentHost.AddMessage(MessageType.Warning, false, "Function Script " + script + " was invalid in Key " + key + " in Section " + Key + " at line " + script.Key);
					function = null;
					return false;
				}
			}
			currentHost.AddMessage(MessageType.Warning, false, "Key " + key + " was not found in Section " + Key);
			function = null;
			return false;
		}

		public override bool GetFunctionScript(T2[] keys, string absolutePath, out AnimationScript function)
		{

			foreach (T2 key in keys)
			{
				if (keyValuePairs.TryRemove(key, out var script))
				{
					if (key.ToString().IndexOf("script", StringComparison.InvariantCultureIgnoreCase) != -1)
					{
						try
						{
							string scriptFile = Path.CombineFile(absolutePath, script.Value.Split('?').First());
							if (File.Exists(scriptFile))
							{
								function = new CSAnimationScript(currentHost, Path.CombineDirectory(absolutePath, script.Value, true));
								return true;
							}
							currentHost.AddMessage(MessageType.Warning, false, "Function Script " + script + " was not found in Key " + key + " in Section " + Key + " at line " + script.Key);
							function = null;
							return false;
						}
						catch
						{
							currentHost.AddMessage(MessageType.Warning, false, "An error occured whilst attempting to load Function Script " + script + " in Key " + key + " in Section " + Key + " at line " + script.Key);
						}
					}
					else
					{
						try
						{
							bool isInfix = key.ToString().IndexOf("RPN", StringComparison.Ordinal) == -1;
							function = new FunctionScript(currentHost, script.Value, isInfix);
							return true;
						}
						catch
						{
							currentHost.AddMessage(MessageType.Warning, false, "Function Script " + script + " was invalid in Key " + key + " in Section " + Key + " at line " + script.Key);
							function = null;
							return false;
						}
					}
				}
			}
			function = null;
			return false;
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
					if (!Path.HasExtension(relativePath))
					{
						// HACK: BVE allows bmp without extension
						relativePath += ".bmp";
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
