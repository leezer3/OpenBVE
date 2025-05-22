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
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Hosts;
using OpenBveApi.Math;
using OpenBveApi.Objects;

namespace Formats.OpenBve
{
	public abstract class Block <T1, T2> where T1 : struct, Enum where T2 : struct, Enum
	{
		public abstract Block<T1, T2> ReadNextBlock();

		public abstract bool ReadBlock(T1 blockToRead, out Block<T1, T2> block);

	    public virtual int RemainingSubBlocks => 0;

	    public virtual int RemainingDataValues => 0;

	    public readonly T1 Key;

	    public readonly int Index;
		
	    internal readonly HostInterface currentHost;

        /// <summary>Unconditionally reads the specified string from the block</summary>
	    public virtual bool GetValue(T2 key, out string value)
	    {
		    value = string.Empty;
		    return false;
	    }

        /// <summary>Unconditionally reads the specified boolean from the block</summary>
        public virtual bool GetValue(T2 key, out bool value)
	    {
		    value = false;
		    return false;
	    }

        /// <summary>Unconditionally reads the specified double from the block</summary>
	    public virtual bool GetValue(T2 key, out double value)
	    {
		    value = 0;
		    return false;
	    }

        /// <summary>Reads the specified double from the block if it exists, preserving the prior value if not present</summary>
	    public virtual bool TryGetValue(T2 key, ref double value)
	    {
		    return false;
	    }

        /// <summary>Unconditionally reads the specified integer from the block</summary>
		public virtual bool GetValue(T2 key, out int value)
	    {
		    value = 0;
		    return false;
	    }

	    /// <summary>Reads the specified integer from the block if it exists, preserving the prior value if not present</summary>
	    public virtual bool TryGetValue(T2 key, ref int value)
	    {
		    return false;
	    }

	    /// <summary>Unconditionally reads the specified path from the block</summary>
		public virtual bool GetPath(T2 key, string absolutePath, out string finalPath)
	    {
		    finalPath = string.Empty;
		    return false;
	    }

	    /// <summary>Uncondtionally reads the next indexed path from the block</summary>
	    public virtual bool GetIndexedPath(string absolutePath, out int index, out string finalPath)
	    {
		    index = -1;
		    finalPath = string.Empty;
		    return false;
	    }

	    /// <summary>Uncondtionally reads the next indexed encoding from the block</summary>
	    public virtual bool GetIndexedEncoding(out TextEncoding.Encoding e, out string path)
	    {
		    e = TextEncoding.Encoding.Unknown;
		    path = string.Empty;
			return false;
	    }

		/// <summary>Reads the specified string from the block, preserving the prior value if not present</summary>
		public virtual bool TryGetValue(T2 key, ref string value)
	    {
		    return false;
	    }

		/// <summary>Reads the specified bool from the block, preserving the prior value if not present</summary>
		public virtual bool TryGetValue(T2 key, ref bool boolValue)
		{
			return false;
		}

		/// <summary>Unconditionally reads the specified Vector2 from the block</summary>
		public virtual bool GetVector2(T2 key, char separator, out Vector2 value)
	    {
			value = Vector2.Null;
			return false;
	    }

	    /// <summary>Reads the specified Vector2 from the block, preserving the prior value if not present</summary>
	    public virtual bool TryGetVector2(T2 key, char separator, ref Vector2 value)
	    {
		    return false;
	    }

	    /// <summary>Unconditionally reads the specified Vector3 from the block</summary>
		public virtual bool GetVector3(T2 key, char separator, out Vector3 value)
	    {
		    value = Vector3.Zero;
		    return false;
	    }

	    /// <summary>Reads the specified Vector3 from the block, preserving the prior value if not present</summary>
	    public virtual bool TryGetVector3(T2 key, char separator, ref Vector3 value)
	    {
		    return false;
	    }

		/// <summary>Unconditionally reads the specified Color24 from the block</summary>
		public virtual bool GetColor24(T2 key, out Color24 value)
	    {
			value = Color24.White;
			return false;
	    }

	    /// <summary>Reads the specified Color24 from the block, preserving the prior value if not present</summary>
	    public virtual bool TryGetColor24(T2 key, ref Color24 value)
	    {
		    return false;
	    }

		/// <summary>Reads the specified Color24 from the block, preserving the prior value if not present</summary>
		public virtual bool TryGetColor32(T2 key, ref Color32 value)
		{
			return false;
		}

		/// <summary>Reads the specified string array from the block, preserving the prior value if not present</summary>
		public virtual bool TryGetStringArray(T2 key, char separator, ref string[] values)
	    {
		    values = new string[0];
		    return false;
	    }

	    /// <summary>Reads the specified path array from the block</summary>
	    public virtual bool GetPathArray(T2 key, char separator, string absolutePath, ref string[] values)
	    {
		    values = new string[0];
		    return false;
	    }

		/// <summary>Reads the specified double array from the block</summary>
		public virtual bool GetDoubleArray(T2 key, char separator, ref double[] values)
		{
			values = new double[0];
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
		    enumValue = default;
		    return false;
	    }

		/// <summary>Reads the specified Enum value from the block</summary>
		public virtual bool GetEnumValue<T3>(T2 key, out T3 enumValue, out int index, out string Suffix) where T3 : struct, Enum
		{
		    enumValue = default;
		    index = 0;
		    Suffix = string.Empty;
		    return false;
	    }

		/// <summary>Reads the specified Enum value from the block</summary>
		public virtual bool GetEnumValue<T3>(T2 key, out T3 enumValue, out Color32 Color) where T3 : struct, Enum
		{
			enumValue = default;
			Color = Color32.Black;
			return false;
		}

		public virtual bool GetNextRawValue(out string s)
		{
			s = string.Empty;
			return false;
		}

		public virtual bool GetNextPath(string absolutePath, out string finalPath)
		{
			finalPath = string.Empty;
			return false;
		}

		public virtual bool GetDamping(T2 key, char separator, out Damping damping)
		{
			damping = null;
			return false;
		}

		protected Block(int myIndex, T1 myKey, HostInterface currentHost)
		{
			Index = myIndex;
		    Key = myKey;
		    this.currentHost = currentHost;
	    }
    }
}
