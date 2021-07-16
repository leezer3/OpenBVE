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

using OpenBveApi.Textures;

namespace MechanikRouteParser
{
	/// <summary>Holds cached properties of a Mechanik texture</summary>
	internal struct MechanikTexture
	{
		/// <summary>The absolute on-disk path of the texture file</summary>
		internal string Path;
		/// <summary>The texture itself</summary>
		internal Texture Texture;
		/// <summary>The calculated, unscaled world width of the texture (1px == 0.5cm)</summary>
		internal double Width;
		/// <summary>The calculated, unscaled world height of the texture (1px == 0.5cm)</summary>
		internal double Height;
		internal MechanikTexture(string p, string s)
		{
			Path = p;
			Plugin.CurrentHost.LoadTexture(p, new TextureParameters(null, null), out Texture);
			this.Width = Texture.Width / 200.0;
			this.Height = Texture.Height / 200.0;
		}
	}
}
