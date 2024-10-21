//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2023, Christopher Lees, The OpenBVE Project
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
using LibRender2.Text;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;

namespace LibRender2.Primitives
{
	public class Button : GLControl
	{
		/// <summary>The text displayed on the button</summary>
		public readonly string Text;
		/// <summary>The highlight color of the button</summary>
		public Color128 HighlightColor;
		/// <summary>The color of the text on the button</summary>
		public Color128 TextColor;
		/// <summary>The font for the button</summary>
		public OpenGlFont Font;

		public Button(BaseRenderer renderer, string text) : base(renderer)
		{
			Text = text;
			Font = Renderer.Fonts.LargeFont;
			Size = Font.MeasureString(Text) * 1.5;
			// default colors to match GLMenu
			BackgroundColor = Color128.Black;
			HighlightColor = Color128.Orange;
			TextColor = Color128.White;
		}

		public override void Draw()
		{
			if (!IsVisible)
			{
				return;
			}
			Renderer.Rectangle.Draw(Texture, Location, Size, BackgroundColor);
			if (CurrentlySelected)
			{
				Renderer.Rectangle.Draw(Texture, Location + Size * 0.1, Size - (Size * 0.2), HighlightColor);
			}
			Renderer.OpenGlString.Draw(Font, Text, Location + (Size * 0.15), TextAlignment.TopLeft, TextColor);
		}

		public override void MouseMove(int x, int y)
		{
			CurrentlySelected = x > Location.X && x < Location.X + Size.X && y > Location.Y && y < Location.Y + Size.Y;
		}

		public override void MouseDown(int x, int y)
		{
			MouseMove(x, y);
			if (CurrentlySelected)
			{
				OnClick?.Invoke(this, EventArgs.Empty);
			}
		}
	}
}
