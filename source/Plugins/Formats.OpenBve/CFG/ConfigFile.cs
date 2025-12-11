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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using OpenBveApi;
using Path = OpenBveApi.Path;
using OpenBveApi.Objects;

namespace Formats.OpenBve
{
	/// <summary>Root block for a .CFG type file</summary>
	public class ConfigFile<T1, T2> : Block<T1, T2> where T1 : struct, Enum where T2 : struct, Enum
	{
		public ConfigFile(string fileName, HostInterface currentHost, string expectedHeader = null) : this(File.ReadAllLines(fileName, TextEncoding.GetSystemEncodingFromFile(fileName)), currentHost, expectedHeader)
		{
		}

		public ConfigFile(string[] lines, HostInterface currentHost, string expectedHeader = null) : base(-1, default, currentHost)
		{
			List<string> blockLines = new List<string>();
			bool addToBlock = false;
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
						subBlocks.Add(new ConfigSection<T1, T2>(previousIdx, startingLine + 1, previousSection, blockLines.ToArray(), currentHost));
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
				subBlocks.Add(new ConfigSection<T1, T2>(idx, startingLine + 1, previousSection, blockLines.ToArray(), currentHost));
			}
		}
	}

	public class ConfigSection<T1, T2> : Block<T1, T2> where T1 : struct, Enum where T2 : struct, Enum
	{
		private readonly ConcurrentDictionary<int, KeyValuePair<int, string>> indexedValues;

		private readonly Queue<KeyValuePair<int, string>> rawValues;
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

		public override List<Block<T1, T2>> ReadBlocks(T1[] blocks)
		{
			currentHost.AddMessage(MessageType.Error, false, "A section in a CFG file cannot contain sub-blocks.");
			return null;
		}

		public override int RemainingDataValues => keyValuePairs.Count + indexedValues.Count + rawValues.Count;

		internal ConfigSection(int myIndex, int startingLine, T1 myKey, string[] myLines, HostInterface currentHost) : base(myIndex, myKey, currentHost)
		{
			indexedValues = new ConcurrentDictionary<int, KeyValuePair<int, string>>();
			rawValues = new Queue<KeyValuePair<int, string>>();
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

		public override bool GetVector2(T2 key, char separator, out Vector2 value)
		{
			value = Vector2.Null;
			if (keyValuePairs.TryRemove(key, out var rawValue))
			{
				string[] splitStrings = rawValue.Value.ConsistantSplit(separator, 2);

				if (!NumberFormats.TryParseDoubleVb6(splitStrings[0], out value.X))
				{
					currentHost.AddMessage(MessageType.Warning, false, "X was invalid in " + key + " in Section " + Key + " at line " + rawValue.Key);
				}
				if (!NumberFormats.TryParseDoubleVb6(splitStrings[1], out value.Y))
				{
					currentHost.AddMessage(MessageType.Warning, false, "Y was invalid in " + key + " in Section " + Key + " at line " + rawValue.Key);
				}
				return true;
			}
			return false;
		}

		public override bool TryGetVector2(T2 key, char separator, ref Vector2 value)
		{
			if (keyValuePairs.TryRemove(key, out var rawValue))
			{
				string[] splitStrings = rawValue.Value.ConsistantSplit(separator, 2);
				bool error = false;

				if (!NumberFormats.TryParseDoubleVb6(splitStrings[0], out double X))
				{
					currentHost.AddMessage(MessageType.Warning, false, "X was invalid in " + key + " in Section " + Key + " at line " + rawValue.Key);
					error = true;
				}
				else
				{
					value.X = X;
				}
				if (!NumberFormats.TryParseDoubleVb6(splitStrings[1], out double Y))
				{
					currentHost.AddMessage(MessageType.Warning, false, "Y was invalid in " + key + " in Section " + Key + " at line " + rawValue.Key);
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
			if (keyValuePairs.TryRemove(key, out var rawValue))
			{
				string[] splitStrings = rawValue.Value.ConsistantSplit(separator, 3);
				
				if (!NumberFormats.TryParseDoubleVb6(splitStrings[0], out value.X))
				{
					currentHost.AddMessage(MessageType.Warning, false, "X was invalid in " + key + " in Section " + Key + " at line " + rawValue.Key);
				}
				if (!NumberFormats.TryParseDoubleVb6(splitStrings[1], out value.Y))
				{
					currentHost.AddMessage(MessageType.Warning, false, "Y was invalid in " + key + " in Section " + Key + " at line " + rawValue.Key);
				}
				if (!NumberFormats.TryParseDoubleVb6(splitStrings[2], out value.Z))
				{
					currentHost.AddMessage(MessageType.Warning, false, "Z was invalid in " + key + " in Section " + Key + " at line " + rawValue.Key);
				}
				return true;
			}
			return false;
		}

		public override bool TryGetVector3(T2 key, char separator, ref Vector3 value)
		{
			if (keyValuePairs.TryRemove(key, out var rawValue))
			{
				string[] splitStrings = rawValue.Value.ConsistantSplit(separator, 3);
				
				if (!NumberFormats.TryParseDoubleVb6(splitStrings[0], out value.X))
				{
					currentHost.AddMessage(MessageType.Warning, false, "X was invalid in " + key + " in Section " + Key + " at line " + rawValue.Key);
				}
				if (!NumberFormats.TryParseDoubleVb6(splitStrings[1], out value.Y))
				{
					currentHost.AddMessage(MessageType.Warning, false, "Y was invalid in " + key + " in Section " + Key + " at line " + rawValue.Key);
				}
				if (!NumberFormats.TryParseDoubleVb6(splitStrings[2], out value.Z))
				{
					currentHost.AddMessage(MessageType.Warning, false, "Z was invalid in " + key + " in Section " + Key + " at line " + rawValue.Key);
				}
				return true;
			}
			return false;
		}

		public override bool TryGetStringArray(T2 key, char separator, ref string[] values)
		{
			if (keyValuePairs.TryRemove(key, out var value))
			{
				values = value.Value.Split(separator);
				return true;
			}
			currentHost.AddMessage(MessageType.Warning, false, "Key " + key + " was not found in Section " + Key + " at line " + value.Key);
			return false;
		}

		public override bool GetPathArray(T2 key, char separator, string absolutePath, ref string[] values)
		{
			if (!TryGetPathArray(key, separator, absolutePath, ref values))
			{
				currentHost.AddMessage(MessageType.Warning, false, "Key " + key + " was not found in Section " + Key);
				return false;
			}
			return true;
		}

		public override bool TryGetPathArray(T2 key, char separator, string absolutePath, ref string[] values)
		{
			if (keyValuePairs.TryRemove(key, out var value))
			{
				string[] splitValues = value.Value.Split(separator);
				if (splitValues.Length > 0)
				{
					values = new string[splitValues.Length];
					for (int i = 0; i < splitValues.Length; i++)
					{
						try
						{
							values[i] = Path.CombineFile(absolutePath, splitValues[i].Trim());
						}
						catch
						{
							if (!string.IsNullOrEmpty(splitValues[i]) && !splitValues[i].Equals("null", StringComparison.InvariantCultureIgnoreCase))
							{
								// allow empty states etc.
								currentHost.AddMessage(MessageType.Warning, false, "The path for state " + i + " was invalid in " + key + " in Section " + Key + " at line " + value.Key);
							}
						}
					}
					return true;
				}
				currentHost.AddMessage(MessageType.Warning, false, "An empty path list was provided for " + key + " in Section " + Key + " at line " + value.Key);
			}
			return false;
		}

		public override bool TryGetDoubleArray(T2 key, char separator, ref double[] values)
		{
			if (keyValuePairs.TryRemove(key, out var value))
			{
				string[] strings = value.Value.Split(separator);
				values = new double[strings.Length];
				for (int i = 0; i < strings.Length; i++)
				{
					if (!NumberFormats.TryParseDoubleVb6(strings[i], out values[i]))
					{
						currentHost.AddMessage(MessageType.Warning, false, "Value " + i + " in array " + key + " was not a valid double in Section " + Key + " at line " + value.Key);
					}
				}
				return true;
			}
			return false;
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

		public override bool GetIndexedPath(string absolutePath, out int index, out string finalPath)
		{
			if (indexedValues.Count > 0)
			{
				index = indexedValues.ElementAt(0).Key;
				indexedValues.TryRemove(index, out var value);

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
			if (keyValuePairs.TryRemove(key, out var value))
			{
				stringValue = value.Value;
				return true;
			}
			return false;
		}

		public override bool TryGetValue(T2 key, ref bool boolValue)
		{
			if (keyValuePairs.TryRemove(key, out var s))
			{
				var ss = s.Value.ToLowerInvariant().Trim();
				if (ss == "1" || ss == "true")
				{
					boolValue = true;
					return true;
				}
				boolValue = false;
				return true;
			}
			return false;
		}

		public override bool GetValue(T2 key, out string stringValue)
		{
			if (keyValuePairs.TryRemove(key, out var value))
			{
				stringValue = value.Value;
				return true;
			}
			stringValue = string.Empty;
			return false;
		}

		public override bool TryGetValue(T2 key, ref double value, NumberRange range = NumberRange.Any)
		{
			if (keyValuePairs.TryRemove(key, out var s))
			{
				if (NumberFormats.TryParseDoubleVb6(s.Value, out double newValue))
				{
					switch (range)
					{
						case NumberRange.Any:
							value = newValue;
							return true;
						case NumberRange.Positive:
							if (newValue > 0)
							{
								value = newValue;
								return true;
							}
							currentHost.AddMessage(MessageType.Warning, false, "Value " + s + " is not a positive double in Key " + key + " in Section " + Key + " at line " + s.Key);
							return false;
						case NumberRange.NonNegative:
							if (newValue >= 0)
							{
								value = newValue;
								return true;
							}
							currentHost.AddMessage(MessageType.Warning, false, "Value " + s + " is not a non-negative double in Key " + key + " in Section " + Key + " at line " + s.Key);
							return false;
						case NumberRange.NonZero:
							if (newValue != 0)
							{
								value = newValue;
								return true;
							}
							currentHost.AddMessage(MessageType.Warning, false, "Value " + s + " is not a non-zero double in Key " + key + " in Section " + Key + " at line " + s.Key);
							return false;
					}
				}
				currentHost.AddMessage(MessageType.Warning, false, "Value " + s + " is not a valid double in Key " + key + " in Section " + Key + " at line " + s.Key);
			}
			return false;
		}
		
		public override bool TryGetValue(T2 key, ref int value, NumberRange range = NumberRange.Any)
		{
			if (keyValuePairs.TryRemove(key, out var s))
			{
				if (NumberFormats.TryParseIntVb6(s.Value, out int newValue))
				{
					if (!int.TryParse(s.Value, out _))
					{
						currentHost.AddMessage(MessageType.Warning, false, "Value " + s.Value + " is a double, not an integer and precision will be lost in Key " + key + " in Section " + Key + " at line " + s.Key);
					}

					switch (range)
					{
						case NumberRange.Any:
							value = newValue;
							return true;
						case NumberRange.Positive:
							if (newValue > 0)
							{
								value = newValue;
								return true;
							}
							currentHost.AddMessage(MessageType.Warning, false, "Value " + s + " is not a positive integer in Key " + key + " in Section " + Key + " at line " + s.Key);
							return false;
						case NumberRange.NonNegative:
							if (newValue >= 0)
							{
								value = newValue;
								return true;
							}
							currentHost.AddMessage(MessageType.Warning, false, "Value " + s + " is not a non-negative integer in Key " + key + " in Section " + Key + " at line " + s.Key);
							return false;
						case NumberRange.NonZero:
							if (newValue != 0)
							{
								value = newValue;
								return true;
							}
							currentHost.AddMessage(MessageType.Warning, false, "Value " + s + " is not a non-zero integer in Key " + key + " in Section " + Key + " at line " + s.Key);
							return false;
					}
				}
				currentHost.AddMessage(MessageType.Warning, false, "Value " + s.Value + " is not a valid integer in Key " + key + " in Section " + Key + " at line " + s.Key);
			}
			return false;
		}

		public override bool GetValue(T2 key, out bool value)
		{
			if (keyValuePairs.TryRemove(key, out var s))
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
			if (keyValuePairs.TryRemove(key, out var color))
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
			if (keyValuePairs.TryRemove(key, out var color))
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

		public override bool TryGetColor32(T2 key, ref Color32 value)
		{
			if (keyValuePairs.TryRemove(key, out var color))
			{
				if (Color32.TryParseHexColor(color.Value, out Color32 newValue))
				{
					value = newValue;
					return true;
				}

				if (Color32.TryParseColor(color.Value, ',', out value))
				{
					return true;
				}
				currentHost.AddMessage(MessageType.Error, false, "Color is invalid in " + key + " in " + Key + " at line " + color.Key);
			}

			return false;
		}

		public override bool GetEnumValue<T3>(T2 key, out T3 enumValue)
		{
			if (keyValuePairs.TryRemove(key, out var value))
			{
				if (Enum.TryParse(value.Value, true, out enumValue))
				{
					return true;
				}
				
				currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + key + " in " + Key + " at line " + value.Key);
			}

			enumValue = default;
			return false;
		}

		public override bool GetEnumValue<T3>(T2 key, out T3 enumValue, out int index, out string suffix)
		{
			index = -1;
			suffix = string.Empty;
			if (keyValuePairs.TryRemove(key, out var value))
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
								suffix = " floor 10 mod";
							}
							else
							{
								string t0 = Math.Pow(10.0, n).ToString(CultureInfo.InvariantCulture);
								string t1 = Math.Pow(10.0, -n).ToString(CultureInfo.InvariantCulture);
								suffix = " ~ " + t0 + " >= <> " + t1 + " * floor 10 mod 10 ?";
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
				else if (s.StartsWith("doorl"))
				{
					num = s.Substring(5);
					s = s.Substring(0, 5);
				}
				else if (s.StartsWith("doorr"))
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

		public override bool GetEnumValue<T3>(T2 key, out T3 enumValue, out Color32 color)
		{
			color = Color32.Black;
			if (keyValuePairs.TryRemove(key, out var value))
			{
				int colonIndex = value.Value.IndexOf(':');
				string colorValue = value.Value.Substring(colonIndex + 1);
				string s = value.Value.Substring(0, colonIndex);
				
				if (Enum.TryParse(s, true, out enumValue))
				{
					if (Color32.TryParseColor(colorValue.Split(','), out var newColor))
					{
						color = newColor;
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
				indexedValues.TryRemove(encodingIdx, out var value);
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
				s = rawValues.Dequeue().Value;
				return true;
			}

			s = string.Empty;
			return false;
		}

		public override bool GetNextPath(string absolutePath, out string finalPath)
		{
			if (rawValues.Count > 0)
			{
				var fileName = rawValues.Dequeue();
				if (!Path.ContainsInvalidChars(fileName.Value))
				{
					try
					{
						finalPath = Path.CombineFile(absolutePath, fileName.Value);
					}
					catch
					{
						finalPath = string.Empty;
					}

					if (File.Exists(finalPath))
					{
						return true;
					}
					
					currentHost.AddMessage(MessageType.Warning, false, "File " + fileName + " was not found at line " + fileName.Key + " in Section " + Key);
					finalPath = string.Empty;
					return false;
				}

				currentHost.AddMessage(MessageType.Warning, false, "Path contains invalid characters for " + fileName + " at line " + fileName.Key + " in Section " + Key);
			}
			finalPath = string.Empty;
			return false;
		}

		public override bool GetDamping(T2 key, char separator, out Damping damping)
		{
			damping = null;
			if (keyValuePairs.TryRemove(key, out var value))
			{
				string[] s = value.Value.Split(separator);
				if (s.Length == 2)
				{
					if (!double.TryParse(s[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double nf))
					{
						currentHost.AddMessage(MessageType.Error, false, "NaturalFrequency is invalid in " + key + " at line " + value.Key + " in the Section " + Key);
						return false;
					}
					if (!double.TryParse(s[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double dr))
					{
						currentHost.AddMessage(MessageType.Error, false, "DampingRatio is invalid in " + key + " at line " + value.Key + " in the Section " + Key);
						return false;
					}
					if (nf <= 0.0)
					{
						currentHost.AddMessage(MessageType.Error, false, "NaturalFrequency is expected to be positive in " + key + " at line " + value.Key + " in the Section " + Key);
						return false;
					}
					if (dr <= 0.0)
					{
						currentHost.AddMessage(MessageType.Error, false, "DampingRatio is expected to be positive in " + key + " at line " + value.Key + " in the Section " + Key);
						return false;
					}

					damping = new Damping(nf, dr);
					return true;
				}
				currentHost.AddMessage(MessageType.Error, false, "Exactly 2 arguments are expected in " + key + " at line " + value.Key + " in the Section " + Key);
			}
			return false;
		}

		public override void ReportErrors()
		{
			for (int i = 0; i < keyValuePairs.Count; i++)
			{
				T2 key = keyValuePairs.ElementAt(i).Key;
				currentHost.AddMessage(MessageType.Error, false, key + " is not valid in an " + Key + " section at line " + keyValuePairs[key].Key);
			}

			for (int i = 0; i < rawValues.Count; i++)
			{
				KeyValuePair<int, string> errorValue = rawValues.Dequeue();
				currentHost.AddMessage(MessageType.Error, false, "Unexpected non key-value-pair encountered in " + Key + " section at line " + errorValue.Key);
			}
		}
	}
}
