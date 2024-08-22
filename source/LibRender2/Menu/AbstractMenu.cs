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

using LibRender2.Text;
using OpenBveApi.Colors;
using OpenBveApi.Math;

namespace LibRender2.Menu
{
	/// <summary>Provides the abstract class for creating OpenGL based menus</summary>
	public abstract class AbstractMenu
	{
		/// <summary>The color used for the overlay</summary>
		public readonly Color128 overlayColor = new Color128(0.0f, 0.0f, 0.0f, 0.2f);

		/// <summary>The color used for the menu background</summary>
		public readonly Color128 backgroundColor = Color128.Black;

		/// <summary>The color used to draw the highlight over a selected item</summary>
		public readonly Color128 highlightColor = Color128.Orange;

		/// <summary> The color used to draw the highlight over a selected folder</summary>
		public readonly Color128 folderHighlightColor = new Color128(0.0f, 0.69f, 1.0f, 1.0f);

		/// <summary>The color used to draw the highlight over a selected routefile</summary>
		public readonly Color128 routeHighlightColor = new Color128(0.0f, 1.0f, 0.69f, 1.0f);

		/// <summary>The color used for caption text</summary>
		public static readonly Color128 ColourCaption = new Color128(0.750f, 0.750f, 0.875f, 1.0f);

		/// <summary>The color used to draw dimmed text</summary>
		public static readonly Color128 ColourDimmed = new Color128(1.000f, 1.000f, 1.000f, 0.5f);

		/// <summary>The color used to draw highlighted text</summary>
		public static readonly Color128 ColourHighlight = Color128.Black;

		/// <summary>The color used to draw normal text</summary>
		public static readonly Color128 ColourNormal = Color128.White;

		/// <summary> Holds the stack of menus</summary>
		public MenuBase[] Menus = { };

		/// <summary>The font currently used for drawing menu items</summary>
		public OpenGlFont menuFont = null;

		/// <summary>The number of visible items in the menu</summary>
		public int visibleItems;

		/// <summary>The screen co-ordinates of the top-left of the menu</summary>
		public Vector2 menuMin;

		/// <summary>The screen co-ordinates of the bottom-right of the menu</summary>
		public Vector2 menuMax;
	}
}
