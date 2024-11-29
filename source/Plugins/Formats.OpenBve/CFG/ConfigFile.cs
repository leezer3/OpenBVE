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

using OpenBveApi.Colors;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using OpenBveApi;
using Path = OpenBveApi.Path;

namespace Formats.OpenBve
{
	/// <summary>Root block for a .CFG type file</summary>
	public class ConfigFile<T1, T2> : Block<T1, T2> where T1 : struct, Enum where T2 : struct, Enum
	{
		private readonly List<Block<T1, T2>> subBlocks;

		public ConfigFile(string[] Lines, HostInterface Host, string expectedHeader = null) : base(-1, default, Host)
		{
			subBlocks = new List<Block<T1, T2>>();
			List<string> blockLines = new List<string>();
			bool addToBlock = false;
			int idx = -1;
			int previousIdx = -1;
			T1 previousSection = default(T1);

			bool headerOK = string.IsNullOrEmpty(expectedHeader);

			//string 

			int startingLine = 0;

			for (int i = 0; i < Lines.Length; i++)
			{
				int j = Lines[i].IndexOf(';');
				if (j >= 0)
				{
					Lines[i] = Lines[i].Substring(0, j).Trim();
				}
				else
				{
					Lines[i] = Lines[i].Trim();
				}
				if (headerOK == false)
				{
					if (!string.IsNullOrEmpty(Lines[i]) && string.Compare(Lines[i], expectedHeader, StringComparison.OrdinalIgnoreCase) == 0)
					{
						headerOK = true;
					}
				}
				if (Lines[i].StartsWith("[") && Lines[i].EndsWith("]"))
				{
					startingLine = i;
					if (!headerOK)
					{
						currentHost.AddMessage(MessageType.Error, false, "The expected header " + expectedHeader + " was not found.");
						headerOK = true;
					}
					// n.b. remove spaces to allow parsing to an enum
					string sct = Lines[i].Trim().Trim('[', ']').Replace(" ", "");

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
							currentHost.AddMessage(MessageType.Error, false, "Invalid index encountered in Section " + sct + " at Line " + i);
							idx = -1;
						}
						sct = sct.Substring(0, c);

					}
					if (!Enum.TryParse(sct, true, out T1 currentSection))
					{
						addToBlock = false;
						currentHost.AddMessage(MessageType.Error, false, "Unknown Section " + sct + " encountered at Line " + i);
					}
					else
					{
						addToBlock = true;
					}

					if (blockLines.Count > 0)
					{
						subBlocks.Add(new ConfigSection<T1, T2>(previousIdx, startingLine, previousSection, blockLines.ToArray(), currentHost));
						blockLines.Clear();
					}
					previousSection = currentSection;
					previousIdx = idx;
				}
				else
				{
					if (addToBlock)
					{
						blockLines.Add(Lines[i]);
					}
				}
			}
			// final block
			if (blockLines.Count > 0)
			{
				subBlocks.Add(new ConfigSection<T1, T2>(idx, startingLine, previousSection, blockLines.ToArray(), currentHost));
			}
		}

		public override Block<T1, T2> ReadNextBlock()
		{
			Block<T1, T2> b = subBlocks.First();
			subBlocks.RemoveAt(0);
			return b;
		}

		public override bool ReadBlock(T1 blockToRead, out Block<T1, T2> block)
		{
			for (int i = 0; i < subBlocks.Count; i++)
			{
				if (EqualityComparer<T1>.Default.Equals(subBlocks[i].Key, blockToRead))
				{
					block = subBlocks[i];
					subBlocks.RemoveAt(i);
					return true;
				}
			}

			block = null;
			return false;
		}

		public override int RemainingSubBlocks => subBlocks.Count;
	}

	public class ConfigSection<T1, T2> : Block<T1, T2> where T1 : struct, Enum where T2 : struct, Enum
	{
		private readonly Dictionary<T2, KeyValuePair<int, string>> keyValuePairs;

		private readonly Dictionary<int, KeyValuePair<int, string>> indexedValues;

		private readonly Queue<string> rawValues;
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

		public override int RemainingDataValues => keyValuePairs.Count + indexedValues.Count + rawValues.Count;

		internal ConfigSection(int myIndex, int startingLine, T1 myKey, string[] myLines, HostInterface Host) : base(myIndex, myKey, Host)
		{
			keyValuePairs = new Dictionary<T2, KeyValuePair<int, string>>();
			indexedValues = new Dictionary<int, KeyValuePair<int, string>>();
			rawValues = new Queue<string>();
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
								currentHost.AddMessage(MessageType.Warning, false, "Duplicate index " + idx + " encountered in Section " + myKey + " at Line " + i + startingLine);
								indexedValues[idx] = new KeyValuePair<int, string>(i + startingLine, b);
							}
							else
							{
								indexedValues.Add(idx, new KeyValuePair<int, string>(i + startingLine, b));
							}

						}
						else
						{
							currentHost.AddMessage(MessageType.Error, false, "Invalid index " + idx + " encountered in Section " + myKey + " at Line " + i + startingLine);
						}

					}
					else if (Enum.TryParse(a.Replace(" ", ""), true, out T2 key))
					{
						keyValuePairs.Add(key, new KeyValuePair<int, string>(i + startingLine, b));
					}
					else
					{
						currentHost.AddMessage(MessageType.Error, false, "Unknown Key " + a + " encountered in Section " + myKey + " at Line " + i + startingLine);
					}
				}
				else
				{
					if (!string.IsNullOrEmpty(myLines[i]))
					{
						rawValues.Enqueue(myLines[i]);
					}
				}
			}
		}

		public override bool GetVector2(T2 key, char separator, out Vector2 value)
		{
			value = Vector2.Null;
			if (keyValuePairs.TryGetValue(key, out var rawValue))
			{
				string[] splitStrings = rawValue.Value.Split(separator);
				if (splitStrings.Length > 2)
				{
					currentHost.AddMessage(MessageType.Warning, false, "Unexpected extra " + (splitStrings.Length - 2) + " paramaters " + key + " encountered in " + key + " in Section " + Key + " at Line " + rawValue.Key);
				}

				if (!NumberFormats.TryParseDoubleVb6(splitStrings[0], out value.X))
				{
					currentHost.AddMessage(MessageType.Warning, false, "X was invalid in " + key + " in Section " + Key + " at Line " + rawValue.Key);
				}
				if (!NumberFormats.TryParseDoubleVb6(splitStrings[1], out value.Y))
				{
					currentHost.AddMessage(MessageType.Warning, false, "Y was invalid in " + key + " in Section " + Key + " at Line " + rawValue.Key);
				}
				return true;
			}
			return false;
		}

		public override bool TryGetVector2(T2 key, char separator, ref Vector2 value)
		{
			if (keyValuePairs.TryGetValue(key, out var rawValue))
			{
				string[] splitStrings = rawValue.Value.Split(separator);
				if (splitStrings.Length > 2)
				{
					currentHost.AddMessage(MessageType.Warning, false, "Unexpected extra " + (splitStrings.Length - 2) + " paramaters " + key + " encountered in " + key + " in Section " + Key + " at Line " + rawValue.Key);
				}

				bool error = false;

				if (!NumberFormats.TryParseDoubleVb6(splitStrings[0], out double X))
				{
					currentHost.AddMessage(MessageType.Warning, false, "X was invalid in " + key + " in Section " + Key + " at Line " + rawValue.Key);
					error = true;
				}
				else
				{
					value.X = X;
				}
				if (!NumberFormats.TryParseDoubleVb6(splitStrings[1], out double Y))
				{
					currentHost.AddMessage(MessageType.Warning, false, "Y was invalid in " + key + " in Section " + Key + " at Line " + rawValue.Key);
					error = true;
				}
				else
				{
					value.Y = Y;
				}
				return !error;
			}
			return false;
		}

		public override bool GetVector3(T2 key, char separator, out Vector3 value)
		{
			value = Vector3.Zero;
			if (keyValuePairs.TryGetValue(key, out var rawValue))
			{
				string[] splitStrings = rawValue.Value.Split(separator);
				if (splitStrings.Length > 3)
				{
					currentHost.AddMessage(MessageType.Warning, false, "Unexpected extra " + (splitStrings.Length - 2) + " paramaters " + key + " encountered in " + key + " in Section " + Key + " at Line " + rawValue.Key);
				}

				if (!NumberFormats.TryParseDoubleVb6(splitStrings[0], out value.X))
				{
					currentHost.AddMessage(MessageType.Warning, false, "X was invalid in " + key + " in Section " + Key + " at Line " + rawValue.Key);
				}
				if (!NumberFormats.TryParseDoubleVb6(splitStrings[1], out value.Y))
				{
					currentHost.AddMessage(MessageType.Warning, false, "Y was invalid in " + key + " in Section " + Key + " at Line " + rawValue.Key);
				}
				if (!NumberFormats.TryParseDoubleVb6(splitStrings[2], out value.Z))
				{
					currentHost.AddMessage(MessageType.Warning, false, "Z was invalid in " + key + " in Section " + Key + " at Line " + rawValue.Key);
				}
				return true;
			}
			return false;
		}

		public override bool TryGetStringArray(T2 key, char separator, ref string[] values)
		{
			if (keyValuePairs.TryGetValue(key, out var value))
			{
				values = value.Value.Split(separator);
				return true;
			}
			currentHost.AddMessage(MessageType.Warning, false, "Key " + key + " was not found in Section " + Key + " at Line " + value.Key);
			return false;
		}

		public override bool GetFunctionScript(T2 key, out FunctionScript function)
		{

			if (keyValuePairs.TryGetValue(key, out var script))
			{
				try
				{
					bool isInfix = key.ToString().IndexOf("RPN", StringComparison.Ordinal) != -1;
					function = new FunctionScript(currentHost, script.Value, isInfix);
					return true;
				}
				catch
				{
					currentHost.AddMessage(MessageType.Warning, false, "Function Script " + script + " was invalid in Key " + key + " in Section " + Key + " at Line " + script.Key);
					function = null;
					return false;
				}
			}
			currentHost.AddMessage(MessageType.Warning, false, "Key " + key + " was not found in Section " + Key);
			function = null;
			return false;
		}

		public override bool GetPath(T2 key, string absolutePath, out string finalPath)
		{
			if (keyValuePairs.TryGetValue(key, out var value))
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
					if (!System.IO.Path.HasExtension(relativePath))
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

					currentHost.AddMessage(MessageType.Warning, false, "File " + value.Value + " was not found in Key " + key + " in Section " + Key + " at Line " + value.Key);
					finalPath = string.Empty;
					return false;

				}

				currentHost.AddMessage(MessageType.Warning, false, "Path contains invalid characters for " + key + " in Section " + Key + " at Line " + value.Key);
				finalPath = string.Empty;
				return false;

			}
			finalPath = string.Empty;
			return false;
		}

		public override bool GetIndexedPath(string absolutePath, out int index, out string finalPath)
		{
			if (indexedValues.Count > 0)
			{
				index = indexedValues.ElementAt(0).Key;
				KeyValuePair<int, string> value = indexedValues.ElementAt(0).Value;
				indexedValues.Remove(index);

				try
				{
					finalPath = Path.CombineFile(absolutePath, value.Value);
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

				currentHost.AddMessage(MessageType.Warning, false, "File " + value.Value + " was not found for Index " + value.Key + " in Section " + Key);
				return true;
			}
			index = -1;
			finalPath = string.Empty;
			return false;
		}

		public override bool TryGetValue(T2 key, ref string stringValue)
		{
			if (keyValuePairs.TryGetValue(key, out var value))
			{
				stringValue = value.Value;
				return true;
			}
			return false;
		}

		public override bool GetValue(T2 key, out string stringValue)
		{
			if (keyValuePairs.TryGetValue(key, out var value))
			{
				stringValue = value.Value;
				return true;
			}
			stringValue = string.Empty;
			return false;
		}

		public override bool GetValue(T2 key, out double value)
		{
			if (keyValuePairs.TryGetValue(key, out var s))
			{
				if (double.TryParse(s.Value, out value))
				{
					return true;
				}
				currentHost.AddMessage(MessageType.Warning, false, "Value " + s + " is not a valid double in Key " + Key + " in Section " + Key + " at Line " + s.Key);
				return false;

			}
			value = 0;
			return false;
		}

		public override bool TryGetValue(T2 key, ref double value)
		{
			if (keyValuePairs.TryGetValue(key, out var s))
			{
				if (double.TryParse(s.Value, out double newValue))
				{
					value = newValue;
					return true;
				}
				currentHost.AddMessage(MessageType.Warning, false, "Value " + s + " is not a valid double in Key " + Key + " in Section " + Key + " at Line " + s.Key);
				return false;

			}
			return false;
		}

		public override bool GetValue(T2 key, out int value)
		{
			if (keyValuePairs.TryGetValue(key, out var s))
			{
				if (int.TryParse(s.Value, out value))
				{
					return true;
				}
				currentHost.AddMessage(MessageType.Warning, false, "Value " + s + " is not a valid integer in Key " + Key + " in Section " + Key + " at Line " + s.Key);
				return false;

			}
			value = 0;
			return false;
		}

		public override bool TryGetValue(T2 key, ref int value)
		{
			if (keyValuePairs.TryGetValue(key, out var s))
			{
				if (int.TryParse(s.Value, out int newValue))
				{
					value = newValue;
					return true;
				}
				currentHost.AddMessage(MessageType.Warning, false, "Value " + s + " is not a valid integer in Key " + Key + " in Section " + Key + " at Line " + s.Key);
				return false;

			}
			return false;
		}

		public override bool GetValue(T2 key, out bool value)
		{
			if (keyValuePairs.TryGetValue(key, out var s))
			{
				var ss = s.Value.ToLowerInvariant().Trim();
				if (ss == "1" || ss == "true")
				{
					value = true;
					return true;
				}
			}
			value = false;
			return false;
		}

		public override bool GetColor24(T2 key, out Color24 value)
		{
			if (keyValuePairs.TryGetValue(key, out var color))
			{
				if (Color24.TryParseHexColor(color.Value, out value))
				{
					return true;
				}

				if (Color24.TryParseColor(color.Value, ',', out value))
				{
					return true;
				}
				currentHost.AddMessage(MessageType.Error, false, "Color is invalid in " + key + " in " + Key + " at line " + color.Key);
			}

			value = Color24.White;
			return false;
		}

		public override bool TryGetColor24(T2 key, ref Color24 value)
		{
			if (keyValuePairs.TryGetValue(key, out var color))
			{
				if (Color24.TryParseHexColor(color.Value, out Color24 newValue))
				{
					value = newValue;
					return true;
				}

				if (Color24.TryParseColor(color.Value, ',', out value))
				{
					return true;
				}
				currentHost.AddMessage(MessageType.Error, false, "Color is invalid in " + key + " in " + Key + " at line " + color.Key);
			}

			return false;
		}

		public override bool GetEnumValue<T3>(T2 key, out T3 enumValue)
		{
			if (keyValuePairs.TryGetValue(key, out var value))
			{
				if (Enum.TryParse(value.Value, out enumValue))
				{
					return true;
				}

				currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + key + " in " + Key + " at line " + value.Key);
			}

			enumValue = default;
			return false;
		}

		public override bool GetEnumValue<T3>(T2 key, out T3 enumValue, out int index, out string Suffix)
		{
			index = -1;
			Suffix = string.Empty;
			if (keyValuePairs.TryGetValue(key, out var value))
			{
				string s = value.Value.ToLowerInvariant();

				// detect d# suffix
				int i;
				for (i = s.Length - 1; i >= 0; i--)
				{
					int a = char.ConvertToUtf32(s, i);
					if (a < 48 | a > 57) break;
				}
				if (i >= 0 & i < s.Length - 1)
				{
					if (s[i] == 'd')
					{
						if (int.TryParse(s.Substring(i + 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n))
						{
							if (n == 0)
							{
								Suffix = " floor 10 mod";
							}
							else
							{
								string t0 = Math.Pow(10.0, n).ToString(CultureInfo.InvariantCulture);
								string t1 = Math.Pow(10.0, -n).ToString(CultureInfo.InvariantCulture);
								Suffix = " ~ " + t0 + " >= <> " + t1 + " * floor 10 mod 10 ?";
							}
							s = s.Substring(0, i);
						}
					}
				}

				string num = string.Empty;
				if (s.StartsWith("ats"))
				{
					num = s.Substring(3);
					s = s.Substring(0, 3);
				}
				else if (s.StartsWith("doorsl"))
				{
					num = s.Substring(5);
					s = s.Substring(0, 5);
				}
				else if (s.StartsWith("doorsr"))
				{
					num = s.Substring(5);
					s = s.Substring(0, 5);
				}

				if (Enum.TryParse(s, true, out enumValue))
				{
					int.TryParse(num, out index);
					return true;
				}
				
				currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + key + " in " + Key + " at line " + value.Key);
			}

			enumValue = default;
			return false;
		}

		public override bool GetEnumValue<T3>(T2 key, out T3 enumValue, out Color32 Color)
		{
			Color = Color32.Black;
			if (keyValuePairs.TryGetValue(key, out var value))
			{
				int colonIndex = value.Value.IndexOf(':');
				string colorValue = value.Value.Substring(colonIndex + 1);
				string s = value.Value.Substring(0, colonIndex);
				
				if (Enum.TryParse(s, true, out enumValue))
				{
					if (Color32.TryParseColor(colorValue.Split(','), out var newColor))
					{
						Color = newColor;
					}
					else
					{
						currentHost.AddMessage(MessageType.Error, false, "Color is invalid in " + key + " in " + Key + " at line " + value.Key);
					}
					return true;
				}

				currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + key + " in " + Key + " at line " + value.Key);
			}
			
			enumValue = default;
			return false;
		}

		public override bool GetIndexedEncoding(out TextEncoding.Encoding e, out string path)
		{
			if (indexedValues.Count > 0)
			{
				int encodingIdx = indexedValues.ElementAt(0).Key;
				KeyValuePair<int, string> value = indexedValues.ElementAt(0).Value;
				indexedValues.Remove(encodingIdx);
				
				if (File.Exists(value.Value))
				{
					path = value.Value;
					e = (TextEncoding.Encoding)encodingIdx;
					return true;
				}
			}

			e = TextEncoding.Encoding.Unknown;
			path = string.Empty;
			return false;
		}

		public override bool GetNextRawValue(out string s)
		{
			if (rawValues.Count > 0)
			{
				s = rawValues.Dequeue();
				return true;
			}

			s = string.Empty;
			return false;
		}
	}
}
