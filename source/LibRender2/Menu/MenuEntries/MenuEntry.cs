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
	/// <summary>The base abstract Menu Entry class</summary>
	public abstract class MenuEntry
	{
		/// <summary>Holds a reference to the containing base menu</summary>
		public readonly AbstractMenu BaseMenu;
		/// <summary>The base text of the menu entry</summary>
		public string Text;
		/// <summary>The display text of the menu entry</summary>
		public string DisplayText(double TimeElapsed)
		{
			if (DisplayLength == 0)
			{
				return Text;
			}
			timer += TimeElapsed;
			if (timer > 0.5)
			{
				if (pause)
				{
					pause = false;
					return _displayText;
				}
				timer = 0;
				scroll++;
				if (scroll == Text.Length)
				{
					scroll = 0;
					pause = true;
				}
				_displayText = Text.Substring(scroll);
				if (_displayText.Length > _displayLength)
				{
					_displayText = _displayText.Substring(0, _displayLength);
				}
			}
			return _displayText;
		}
		/// <summary>Backing property for display text</summary>
		private string _displayText;
		/// <summary>Backing property for display length</summary>
		private int _displayLength;
		/// <summary>The length to display</summary>
		public int DisplayLength
		{
			get => _displayLength;
			set
			{
				_displayLength = value;
				_displayText = Text.Substring(0, value);
				timer = 0;
			}
		}
		/// <summary>The icon to draw for this menu entry</summary>
		public Texture Icon;
		//Properties used for controlling the scrolling text if overlong
		private double timer;
		private int scroll;
		private bool pause;

		protected MenuEntry(AbstractMenu menu)
		{
			BaseMenu = menu;
		}

	}
}
