//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2024, Maurizo M. Gavioli, The OpenBVE Project
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

namespace LibRender2.Menu
{
	/// <summary>A menu entry which performs a command when selected</summary>
	public class MenuCommand : MenuEntry
	{
		/// <summary>The command tag describing the function of the menu entry</summary>
		public readonly MenuTag Tag;
		/// <summary>The optional data to be passed with the command</summary>
		public readonly object Data;

		public MenuCommand(AbstractMenu menu, string Text, MenuTag Tag, object Data) : base(menu)
		{
			this.Text = Text;
			this.Tag = Tag;
			this.Data = Data;
			this.Icon = null;
		}

		public MenuCommand(AbstractMenu menu, string Text, MenuTag Tag, object Data, Texture Icon) : base(menu)
		{
			this.Text = Text;
			this.Tag = Tag;
			this.Data = Data;
			this.Icon = Icon;
		}
	}
}
