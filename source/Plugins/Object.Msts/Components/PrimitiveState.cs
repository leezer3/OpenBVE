//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2026, Christopher Lees, The OpenBVE Project
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

namespace Plugin
{
	/// <summary>Describes a PrimitiveState</summary>
	internal class PrimitiveState
	{
		/// <summary>The textual name of the PrimitiveState</summary>
		internal readonly string Name;
		/// <summary>The shader name to be applied when drawing this primitive</summary>
		/// <remarks>Controls alpha blending etc.</remarks>
		internal int Shader;
		/// <summary>The texture indicies used by this primitive</summary>
		internal int[] Textures;
		/*
		 * Unlikely to be able to support these at present
		 * However, read and see if we can hack common ones
		 */
		internal uint Flags;
		internal float ZBias;
		internal int vertexStates;
		internal int alphaTestMode;
		internal int lightCfgIdx;
		internal int zBufferMode;

		internal PrimitiveState(string name)
		{
			Name = name;
		}
	}
}
