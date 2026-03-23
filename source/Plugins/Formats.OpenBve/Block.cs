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

using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Path = OpenBveApi.Path;

namespace Formats.OpenBve
{
	public abstract class Block <T1, T2> where T1 : struct, Enum where T2 : struct, Enum
	{
		internal readonly List<Block<T1, T2>> subBlocks;

		internal readonly ConcurrentDictionary<T2, KeyValuePair<int, string>> keyValuePairs;

		internal readonly ConcurrentDictionary<int, KeyValuePair<int, string>> indexedValues;

		internal readonly Queue<KeyValuePair<int, string>> rawValues;

		public virtual Block<T1, T2> ReadNextBlock()
		{
			Block<T1, T2> b = subBlocks.First();
			subBlocks.RemoveAt(0);
			return b;
		}

		public virtual bool ReadBlock(T1 blockToRead, out Block<T1, T2> block)
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

		public virtual bool ReadBlock(T1[] validBlocks, out Block<T1, T2> block)
		{
			for (int i = 0; i < subBlocks.Count; i++)
			{
				for (int j = 0; j < validBlocks.Length; j++)
				{
					if (EqualityComparer<T1>.Default.Equals(subBlocks[i].Key, validBlocks[j]))
					{
						block = subBlocks[i];
						subBlocks.RemoveAt(i);
						return true;
					}
				}
				
			}

			block = null;
			return false;
		}

		public virtual List<Block<T1, T2>> ReadBlocks(T1[] blocks)
		{
			List<Block<T1, T2>> returnedBlocks = new List<Block<T1, T2>>();
			for (int i = subBlocks.Count - 1; i >= 0; i--)
			{
				for (int j = 0; j < blocks.Length; j++)
				{
					if (EqualityComparer<T1>.Default.Equals(subBlocks[i].Key, blocks[j]))
					{
						returnedBlocks.Insert(0, subBlocks[i]);
						subBlocks.RemoveAt(i);
						break;
					}
				}
			}

			return returnedBlocks;
		}

		public int RemainingSubBlocks => subBlocks.Count;

	    public int RemainingDataValues => keyValuePairs.Count + indexedValues.Count + rawValues.Count;

	    public int RemainingIndexedValues => indexedValues.Count;

		public readonly T1 Key;

	    public readonly int Index;

	    public readonly string FileName;
		
	    internal readonly HostInterface currentHost;

        /// <summary>Unconditionally reads the specified string from the block</summary>
	    public bool GetValue(T2 key, out string stringValue)
	    {
			if (keyValuePairs.TryRemove(key, out KeyValuePair<int, string> value))
			{
				stringValue = value.Value;
				return true;
			}
			stringValue = string.Empty;
			return false;
		}

        /// <summary>Unconditionally reads the specified boolean from the block</summary>
        public bool GetValue(T2 key, out bool value)
	    {
			if (keyValuePairs.TryRemove(key, out KeyValuePair<int, string> s))
			{
				string ss = s.Value.ToLowerInvariant().Trim();
				if (ss == "1" || ss == "true")
				{
					value = true;
					return true;
				}
			}
			value = false;
			return false;
		}

        /// <summary>Unconditionally reads the specified double from the block</summary>
	    public bool GetValue(T2 key, out double value, NumberRange range = NumberRange.Any)
	    {
			value = 0;
			return TryGetValue(key, ref value, range);
	    }

        /// <summary>Reads the specified double from the block if it exists, preserving the prior value if not present</summary>
	    public bool TryGetValue(T2 key, ref double value, NumberRange range = NumberRange.Any)
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

	    /// <summary>Unconditionally reads the specified integer from the block</summary>
		public bool GetValue(T2 key, out int value, NumberRange range = NumberRange.Any)
	    {
		    value = 0;
		    return TryGetValue(key, ref value, range);
	    }

	    /// <summary>Reads the specified integer from the block if it exists, preserving the prior value if not present</summary>
	    public virtual bool TryGetValue(T2 key, ref int value, NumberRange range = NumberRange.Any)
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

	    /// <summary>Unconditionally reads the specified path from the block</summary>
		public virtual bool GetPath(T2 key, string absolutePath, out string finalPath)
	    {
		    finalPath = string.Empty;
		    return false;
	    }

	    /// <summary>Unconditionally reads the next indexed value from the block</summary>
	    public virtual bool GetIndexedValue(out int index, out string value)
	    {
		    if (indexedValues.Count > 0)
		    {
			    index = indexedValues.ElementAt(0).Key;
			    indexedValues.TryRemove(index, out var kvp);

			    value = kvp.Value;
			    return true;
		    }
		    index = -1;
		    value = string.Empty;
		    return false;
	    }

		/// <summary>Unconditionally reads the next indexed path from the block</summary>
		public virtual bool GetIndexedPath(string absolutePath, out int index, out string finalPath)
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

				currentHost.AddMessage(MessageType.Warning, false, "File " + value.Value + " was not found for Index " + value.Key + " in Section " + Key + " in file " + FileName);
				return true;
			}
			index = -1;
			finalPath = string.Empty;
			return false;
		}

	    /// <summary>Unconditionally reads the next indexed encoding from the block</summary>
	    public virtual bool GetIndexedEncoding(out TextEncoding.Encoding e, out string path)
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

		/// <summary>Reads the specified string from the block, preserving the prior value if not present</summary>
		public virtual bool TryGetValue(T2 key, ref string stringValue)
	    {
		    if (GetValue(key, out string value))
		    {
				stringValue = value;
				return true;
		    }
			return false;
	    }

		/// <summary>Reads the specified bool from the block, preserving the prior value if not present</summary>
		public virtual bool TryGetValue(T2 key, ref bool boolValue)
		{
			if (GetValue(key, out bool value))
			{
				boolValue = value;
				return true;
			}
			return false;
		}

		/// <summary>Unconditionally reads the specified Vector2 from the block</summary>
		public bool GetVector2(T2 key, char separator, out Vector2 value)
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

	    /// <summary>Reads the specified Vector2 from the block, preserving the prior value if not present</summary>
	    public bool TryGetVector2(T2 key, char separator, ref Vector2 value)
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

	    /// <summary>Unconditionally reads the specified Vector3 from the block</summary>
		public virtual bool GetVector3(T2 key, char separator, out Vector3 value)
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

	    /// <summary>Reads the specified Vector3 from the block, preserving the prior value if not present</summary>
	    public virtual bool TryGetVector3(T2 key, char separator, ref Vector3 value)
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

		/// <summary>Unconditionally reads the specified Color24 from the block</summary>
		public virtual bool GetColor24(T2 key, out Color24 value)
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

	    /// <summary>Reads the specified Color24 from the block, preserving the prior value if not present</summary>
	    public virtual bool TryGetColor24(T2 key, ref Color24 value)
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

		/// <summary>Reads the specified Color24 from the block, preserving the prior value if not present</summary>
		public virtual bool TryGetColor32(T2 key, ref Color32 value)
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

		/// <summary>Reads the specified string array from the block, preserving the prior value if not present</summary>
		public virtual bool TryGetStringArray(T2 key, char separator, ref string[] values)
		{
			return TryGetStringArray(key, new[] { separator }, ref values);
		}

		/// <summary>Reads the specified string array from the block, preserving the prior value if not present</summary>
		public virtual bool TryGetStringArray(T2 key, char[] separators, ref string[] values)
		{
			if (keyValuePairs.TryRemove(key, out var value))
			{
				values = value.Value.Split(separators);
				return true;
			}
			currentHost.AddMessage(MessageType.Warning, false, "Key " + key + " was not found in Section " + Key + " at line " + value.Key + " in file " + FileName);
			return false;
		}

		/// <summary>Reads the specified path array from the block</summary>
		public virtual bool TryGetPathArray(T2 key, char separator, string absolutePath, ref string[] values)
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
								currentHost.AddMessage(MessageType.Warning, false, "The path for state " + i + " was invalid in " + key + " in Section " + Key + " at line " + value.Key + " in file " + FileName);
							}
						}
					}
					return true;
				}
				currentHost.AddMessage(MessageType.Warning, false, "An empty path list was provided for " + key + " in Section " + Key + " at line " + value.Key + " in file " + FileName);
			}
			return false;
		}

		/// <summary>Reads the specified path array from the block</summary>
		public virtual bool GetPathArray(T2 key, char separator, string absolutePath, ref string[] values)
	    {
			if (!TryGetPathArray(key, separator, absolutePath, ref values))
			{
				currentHost.AddMessage(MessageType.Warning, false, "Key " + key + " was not found in Section " + Key);
				return false;
			}
			return true;
		}

		/// <summary>Reads the specified double array from the block</summary>
		public virtual bool TryGetDoubleArray(T2 key, char separator, ref double[] values)
		{
			if (keyValuePairs.TryRemove(key, out var value))
			{
				string[] strings = value.Value.Split(separator);
				values = new double[strings.Length];
				for (int i = 0; i < strings.Length; i++)
				{
					if (!NumberFormats.TryParseDoubleVb6(strings[i], out values[i]))
					{
						currentHost.AddMessage(MessageType.Warning, false, "Value " + i + " in array " + key + " was not a valid double in Section " + Key + " at line " + value.Key + " in file " + FileName);
					}
				}
				return true;
			}
			return false;
		}

		/// <summary>Reads the specified integer array from the block</summary>
		public virtual bool TryGetIntArray(T2 key, char separator, ref int[] values)
		{
			if (keyValuePairs.TryRemove(key, out var value))
			{
				string[] strings = value.Value.Split(separator);
				values = new int[strings.Length];
				for (int i = 0; i < strings.Length; i++)
				{
					if (!NumberFormats.TryParseIntVb6(strings[i], out values[i]))
					{
						currentHost.AddMessage(MessageType.Warning, false, "Value " + i + " in array " + key + " was not a valid double in Section " + Key + " at line " + value.Key + " in file " + FileName);
					}
				}
				return true;
			}
			return false;
		}

		/// <summary>Reads the specified FunctionScript from the block, preserving the prior value if not present</summary>
		public virtual bool GetFunctionScript(T2 key, out AnimationScript function)
	    {
		    function = null;
		    return false;
	    }

	    /// <summary>Reads the specified FunctionScript from the block, preserving the prior value if not present</summary>
	    public virtual bool GetFunctionScript(T2[] keys, string absolutePath, out AnimationScript function)
	    {
		    function = null;
		    return false;
	    }

		/// <summary>Reads the specified Enum value from the block</summary>
		public virtual bool GetEnumValue<T3>(T2 key, out T3 enumValue) where T3 : struct, Enum
	    {
			if (keyValuePairs.TryRemove(key, out var value))
			{
				if (Enum.TryParse(value.Value, true, out enumValue))
				{
					return true;
				}

				currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + key + " in " + Key + " at line " + value.Key + " in file " + FileName);
			}

			enumValue = default;
			return false;
		}

		/// <summary>Reads the specified Enum value from the block</summary>
		public virtual bool TryGetEnumValue<T3>(T2 key, ref T3 enumValue) where T3 : struct, Enum
		{
			if (keyValuePairs.TryRemove(key, out var value))
			{
				if (Enum.TryParse(value.Value, true, out enumValue))
				{
					return true;
				}
				currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + key + " in " + Key + " at line " + value.Key + " in file " + FileName);
			}
			return false;
		}

		/// <summary>Reads the specified Enum value from the block</summary>
		public virtual bool GetEnumValue<T3>(T2 key, out T3 enumValue, out int index, out string suffix) where T3 : struct, Enum
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

				currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + key + " in " + Key + " at line " + value.Key + " in file " + FileName);
			}

			enumValue = default;
			return false;
		}

		/// <summary>Reads the specified Enum value from the block</summary>
		public virtual bool GetEnumValue<T3>(T2 key, out T3 enumValue, out Color32 color) where T3 : struct, Enum
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
						currentHost.AddMessage(MessageType.Error, false, "Color is invalid in " + key + " in " + Key + " at line " + value.Key + " in file " + FileName);
					}
					return true;
				}

				currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + key + " in " + Key + " at line " + value.Key + " in file " + FileName);
			}

			enumValue = default;
			return false;
		}

		public virtual bool GetNextRawValue(out string s)
		{
			if (rawValues.Count > 0)
			{
				s = rawValues.Dequeue().Value;
				return true;
			}

			s = string.Empty;
			return false;
		}

		public virtual bool GetNextPath(string absolutePath, out string finalPath)
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

					currentHost.AddMessage(MessageType.Warning, false, "File " + fileName + " was not found at line " + fileName.Key + " in Section " + Key + " in file " + FileName);
					finalPath = string.Empty;
					return false;
				}

				currentHost.AddMessage(MessageType.Warning, false, "Path contains invalid characters for " + fileName + " at line " + fileName.Key + " in Section " + Key + " in file " + FileName);
			}
			finalPath = string.Empty;
			return false;
		}

		public virtual bool GetDamping(T2 key, char separator, out Damping damping)
		{
			damping = null;
			if (keyValuePairs.TryRemove(key, out var value))
			{
				string[] s = value.Value.Split(separator);
				if (s.Length == 2)
				{
					if (!double.TryParse(s[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double nf))
					{
						currentHost.AddMessage(MessageType.Error, false, "NaturalFrequency is invalid in " + key + " at line " + value.Key + " in the Section " + Key + " in file " + FileName);
						return false;
					}
					if (!double.TryParse(s[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double dr))
					{
						currentHost.AddMessage(MessageType.Error, false, "DampingRatio is invalid in " + key + " at line " + value.Key + " in the Section " + Key + " in file " + FileName);
						return false;
					}
					if (nf <= 0.0)
					{
						currentHost.AddMessage(MessageType.Error, false, "NaturalFrequency is expected to be positive in " + key + " at line " + value.Key + " in the Section " + Key + " in file " + FileName);
						return false;
					}
					if (dr <= 0.0)
					{
						currentHost.AddMessage(MessageType.Error, false, "DampingRatio is expected to be positive in " + key + " at line " + value.Key + " in the Section " + Key + " in file " + FileName);
						return false;
					}

					damping = new Damping(nf, dr);
					return true;
				}
				currentHost.AddMessage(MessageType.Error, false, "Exactly 2 arguments are expected in " + key + " at line " + value.Key + " in the Section " + Key + " in file " + FileName);
			}
			return false;
		}

		public virtual bool TryGetTime(T2 key, ref double Value)
		{
			if (GetTime(key, out double timeValue))
			{
				Value = timeValue;
				return true;
			}

			return false;
		}

		public virtual bool GetTime(T2 key, out double Value)
		{
			if (keyValuePairs.TryRemove(key, out var value))
			{
				string Expression = value.Value.TrimInside();
				if (Expression.Length != 0)
				{
					CultureInfo Culture = CultureInfo.InvariantCulture;
					int i = Expression.IndexOf('.');
					if (i == -1)
					{
						i = Expression.IndexOf(':');
					}

					if (i >= 1)
					{
						if (int.TryParse(Expression.Substring(0, i), NumberStyles.Integer, Culture, out int h))
						{
							int n = Expression.Length - i - 1;
							if (n == 1 | n == 2)
							{
								if (uint.TryParse(Expression.Substring(i + 1, n), NumberStyles.None, Culture,
									    out uint m))
								{
									Value = 3600.0 * h + 60.0 * m;
									return true;
								}
							}
							else if (n >= 3)
							{
								if (n > 4)
								{
									currentHost.AddMessage(MessageType.Warning, false,
										"A maximum of 4 digits of precision are supported in TIME values");
									n = 4;
								}

								if (uint.TryParse(Expression.Substring(i + 1, 2), NumberStyles.None, Culture,
									    out uint m))
								{
									string ss = Expression.Substring(i + 3, n - 2);
									/*
									 * Handles values in the following format:
									 * HH.MM.SS
									 */
									if (ss.StartsWith("."))
									{
										ss = ss.Substring(1, ss.Length - 1);
									}

									if (uint.TryParse(ss, NumberStyles.None, Culture, out uint s))
									{
										Value = 3600.0 * h + 60.0 * m + s;
										return true;
									}
								}
							}
						}
					}
					else if (i == -1)
					{
						if (int.TryParse(Expression, NumberStyles.Integer, Culture, out int h))
						{
							Value = 3600.0 * h;
							return true;
						}
					}
				}
			}
			Value = 0.0;
			return false;
		}

		protected Block(int myIndex, T1 myKey, string myFile, HostInterface currentHost)
		{
			Index = myIndex;
		    Key = myKey;
			FileName = myFile;
		    this.currentHost = currentHost;
		    subBlocks = new List<Block<T1, T2>>();
		    keyValuePairs = new ConcurrentDictionary<T2, KeyValuePair<int, string>>();
		    indexedValues = new ConcurrentDictionary<int, KeyValuePair<int, string>>();
		    rawValues = new Queue<KeyValuePair<int, string>>();
		}

		public virtual void ReportErrors()
		{
			for (int i = 0; i < keyValuePairs.Count; i++)
			{
				T2 key = keyValuePairs.ElementAt(i).Key;
				currentHost.AddMessage(MessageType.Error, false, key + " is not valid in an " + Key + " section at line " + keyValuePairs[key].Key + " in file " + FileName);
			}

			for (int i = 0; i < rawValues.Count; i++)
			{
				KeyValuePair<int, string> errorValue = rawValues.Dequeue();
				currentHost.AddMessage(MessageType.Error, false, "Unexpected non key-value-pair encountered in " + Key + " section at line " + errorValue.Key + " in file " + FileName);
			}
		}
    }
}
