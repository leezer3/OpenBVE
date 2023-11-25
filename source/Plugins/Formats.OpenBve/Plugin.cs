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

using System;
using System.Collections.Generic;
using System.Linq;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Math;

namespace Formats.OpenBve
{
	public abstract class Block <T, TT> where T : struct, Enum where TT : struct, Enum
    {
		public abstract Block<T, TT> ReadNextBlock();

	    public virtual int RemainingSubBlocks => 0;

	    public virtual int RemainingDataValues => 0;

	    public readonly T Key;

	    public readonly int Index;
		
	    internal readonly HostInterface currentHost;

	    public virtual bool GetValue(TT key, out bool value)
	    {
		    value = false;
		    return false;
	    }

	    public virtual bool GetValue(TT key, out double value)
	    {
		    value = 0;
		    return false;
	    }

	    public virtual bool GetValue(TT key, out string value)
	    {
		    value = string.Empty;
		    return false;
	    }

	    public virtual bool GetIndexedValue(out int index, out string value)
	    {
		    index = -1;
		    value = string.Empty;
		    return false;
	    }

	    public virtual bool GetVector2(TT key, char separator, out Vector2 value)
	    {
			value = Vector2.Null;
			return false;
	    }

	    public virtual bool GetVector3(TT key, char separator, out Vector3 value)
	    {
		    value = Vector3.Zero;
		    return false;
	    }

	    public virtual bool GetStringArray(TT key, char separator, out string[] values)
	    {
		    values = new string[0];
		    return false;
	    }

	    public virtual bool GetFunctionScript(TT key, out FunctionScript function)
	    {
		    function = null;
		    return false;
	    }

		protected Block(int myIndex, T myKey, HostInterface host)
		{
			Index = myIndex;
		    Key = myKey;
		    currentHost = host;
	    }
    }

	/// <summary>Root block for a .CFG type file</summary>
    public class ConfigFile <T, TT> : Block<T, TT> where T : struct, Enum where TT : struct, Enum
	{
		private readonly string[] myLines;

		private readonly List<Block<T, TT>> subBlocks;

		public ConfigFile(string[] Lines, HostInterface Host, string expectedHeader = null) : base(-1, default, Host)
		{
			myLines = Lines;
			subBlocks = new List<Block<T, TT>>();
			List<string> blockLines = new List<string>();
			bool addToBlock = false;
			int idx = -1;
			int previousIdx = -1;
			T previousSection = default(T);

			bool headerOK = string.IsNullOrEmpty(expectedHeader);

			//string 

			for (int i = 0; i < myLines.Length; i++)
			{
				if (headerOK == false)
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

					if (!string.IsNullOrEmpty(Lines[i]) && string.Compare(Lines[i], expectedHeader, StringComparison.OrdinalIgnoreCase) == 0)
					{
						headerOK = true;
					}
				}
				if (myLines[i].StartsWith("[") && myLines[i].EndsWith("]"))
				{
					if (!headerOK)
					{
						currentHost.AddMessage(MessageType.Error, false, "The expected header " + expectedHeader + " was not found.");
						headerOK = true;
					}
					// n.b. remove spaces to allow parsing to an enum
					string sct = myLines[i].Trim().Trim('[', ']').Replace(" ", "");

					T currentSection;
					if (char.IsDigit(sct[sct.Length - 1]))
					{
						int c = sct.Length - 1;
						while (!char.IsDigit(sct[c]) && c > 0)
						{
							c--;
						}

						
						if (!int.TryParse(sct.Substring(c, sct.Length - c), out idx) || idx < 0)
						{
							currentHost.AddMessage(MessageType.Error, false, "Invalid index encountered in Section " + sct + " at Line " + i);
							idx = -1;
						}
						sct = sct.Substring(0, c);

					}
					if (!Enum.TryParse(sct, true, out currentSection))
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
						subBlocks.Add(new ConfigSection<T, TT>(previousIdx, previousSection, blockLines.ToArray(), currentHost));
						blockLines.Clear();
					}
					previousSection = currentSection;
					previousIdx = idx;
				}
				else
				{
					if (addToBlock)
					{
						blockLines.Add(myLines[i]);
					}
				}
			}
			// final block
			if (blockLines.Count > 0)
			{
				subBlocks.Add(new ConfigSection<T, TT>(idx, previousSection, blockLines.ToArray(), currentHost));
			}
		}

		public override Block<T, TT> ReadNextBlock()
		{
			Block<T, TT> b = subBlocks.First();
			subBlocks.RemoveAt(0);
			return b;
		}

		public override int RemainingSubBlocks => subBlocks.Count;
	}

	public class ConfigSection <T, TT> : Block<T, TT> where T : struct, Enum where TT : struct, Enum
	{
		private readonly Dictionary<TT, string> keyValuePairs;

		private readonly Dictionary<int, string> indexedValues;
		public override Block<T, TT> ReadNextBlock()
		{
			currentHost.AddMessage(MessageType.Error, false, "A section in a CFG file cannot contain sub-blocks.");
			return null;
		}

		public override int RemainingDataValues => keyValuePairs.Count + indexedValues.Count;
		
		internal ConfigSection(int myIndex, T myKey, string[] myLines, HostInterface Host) : base(myIndex, myKey, Host)
		{
			keyValuePairs = new Dictionary<TT, string>();
			indexedValues = new Dictionary<int, string>();
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
								currentHost.AddMessage(MessageType.Warning, false, "Duplicate index " + idx + " encountered in Section " + myKey + " at Line " + i);
								indexedValues[idx] = b;
							}
							else
							{
								indexedValues.Add(idx, b);
							}
							
						}
						else
						{
							currentHost.AddMessage(MessageType.Error, false, "Invalid index " + idx + " encountered in Section " + myKey + " at Line " + i);	
						}
						
					}
					else if (Enum.TryParse(a, true, out TT key))
					{
						keyValuePairs.Add(key, b);
					}
					else
					{
						currentHost.AddMessage(MessageType.Error, false, "Unknown Key " + key + " encountered in Section " + myKey + " at Line " + i);
					}
				}
			}
		}

		public override bool GetVector2(TT key, char separator, out Vector2 value)
		{
			value = Vector2.Null;
			if (keyValuePairs.TryGetValue(key, out var rawValue))
			{
				string[] splitStrings = rawValue.Split(separator);
				if (splitStrings.Length > 2)
				{
					currentHost.AddMessage(MessageType.Warning, false, "Unexpected extra " + (splitStrings.Length - 2) + " paramaters " + key + " encountered in " + key + " in Section " + Key);
				}
				
				if (!NumberFormats.TryParseDoubleVb6(splitStrings[0], out value.X))
				{
					currentHost.AddMessage(MessageType.Warning, false, "X was invalid in " + key + " in Section " + Key);
				}
				if (!NumberFormats.TryParseDoubleVb6(splitStrings[1], out value.Y))
				{
					currentHost.AddMessage(MessageType.Warning, false, "Y was invalid in " + key + " in Section " + Key);
				}
				return true;
			}
			return false;
		}

		public override bool GetIndexedValue(out int index, out string value)
		{
			if (indexedValues.Count > 0)
			{
				index = indexedValues.ElementAt(0).Key;
				value = indexedValues.ElementAt(0).Value;
				indexedValues.Remove(index);
				return true;
			}
			index = -1;
			value = string.Empty;
			return false;
		}

		public override bool GetStringArray(TT key, char separator, out string[] values)
		{
			if (keyValuePairs.TryGetValue(key, out var value))
			{
				values = value.Split(separator);
				return true;
			}
			currentHost.AddMessage(MessageType.Warning, false, "Key " + key + " was not found in Section " + Key);
			values = new string[0];
			return false;
		}

		public override bool GetFunctionScript(TT key, out FunctionScript function)
		{

			if (keyValuePairs.TryGetValue(key, out var script))
			{
				try
				{
					bool isInfix = key.ToString().IndexOf("RPN", StringComparison.Ordinal) != -1;
					function = new FunctionScript(currentHost, script, isInfix);
					return true;
				}
				catch
				{
					currentHost.AddMessage(MessageType.Warning, false, "Function Script " + script + " was invalid in Key "+ Key + " in Section " + Key);
					function = null;
					return false;
				}
			}
			currentHost.AddMessage(MessageType.Warning, false, "Key " + key + " was not found in Section " + Key);
			function = null;
			return false;
		}

		public override bool GetValue(TT key, out string value)
		{
			if (keyValuePairs.TryGetValue(key, out value))
			{
				return true;
			}
			value = string.Empty;
			return false;
		}

		public override bool GetValue(TT key, out double value)
		{
			if (keyValuePairs.TryGetValue(key, out var s))
			{
				if (double.TryParse(s, out value))
				{
					return true;
				}
				currentHost.AddMessage(MessageType.Warning, false, "Value " + s + " is not a valid double in Key "+ Key + " in Section " + Key);
				return false;

			}
			value = 0;
			return false;
		}

		public override bool GetValue(TT key, out bool value)
		{
			if (keyValuePairs.TryGetValue(key, out var s))
			{
				s = s.ToLowerInvariant().Trim();
				if (s == "1" || s == "true")
				{
					value = true;
					return true;
				}
			}
			value = false;
			return false;
		}
	}
}
