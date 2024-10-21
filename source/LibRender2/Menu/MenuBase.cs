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

using OpenBveApi.Graphics;
using System;
using LibRender2.Text;
using OpenBveApi.Math;

namespace LibRender2.Menu
{
	/// <summary>The base class for a single menu within the menu stack</summary>
	public class MenuBase
	{
		/// <summary>The text alignment for the menu</summary>
		public TextAlignment Align;

		/// <summary>The list of items to be shown</summary>
		public MenuEntry[] Items = { };

		/// <summary>The smaller of the width of the largest item, and the absolute width</summary>
		public double ItemWidth = 0;

		/// <summary>The absolute width</summary>
		public double Width = 0;

		/// <summary>The absolute height</summary>
		public double Height = 0;

		/// <summary>The previous menu selection</summary>
		public int LastSelection = int.MaxValue;

		private int currentSelection;

		/// <summary>The currently displayed top item</summary>
		public int TopItem;

		/// <summary>The type of menu</summary>
		public readonly MenuType Type;

		/// <summary>Gets the currently selected menu item</summary>
		public virtual int Selection
		{
			get => currentSelection;
			set
			{
				LastSelection = currentSelection;
				currentSelection = value;
			}
		}

		public void ProcessScroll(int Scroll, int VisibleItems)
		{
			if (Math.Abs(Scroll) == Scroll)
			{
				//Negative
				if (TopItem > 0)
				{
					TopItem--;
				}
			}
			else
			{
				//Positive
				if (Items.Length - TopItem > VisibleItems)
				{
					TopItem++;
				}
			}
		}

		public MenuBase(MenuType type)
		{
			Type = type;
		}

		/// <summary>Computes the on-screen extent of the menu</summary>
		public void ComputeExtent(MenuType menuType, OpenGlFont menuFont, double MaxWidth)
		{
			for (int i = 0; i < Items.Length; i++)
			{
				if (Items[i] == null)
				{
					continue;
				}
				Vector2 size = menuFont.MeasureString(Items[i].Text);
				if (Items[i].Icon != null)
				{
					size.X += size.Y * 1.25;
				}
				if (size.X > Width)
				{
					Width = size.X;
				}

				if (MaxWidth != 0 && size.X > MaxWidth)
				{
					for (int j = Items[i].Text.Length - 1; j > 0; j--)
					{
						string trimmedText = Items[i].Text.Substring(0, j);
						size = menuFont.MeasureString(trimmedText);
						double mwi = MaxWidth;
						if (Items[i].Icon != null)
						{
							mwi -= size.Y * 1.25;
						}
						if (size.X < mwi)
						{
							Items[i].DisplayLength = trimmedText.Length;
							break;
						}
					}
					Width = MaxWidth;
				}
				if (!(Items[i] is MenuCaption && menuType != MenuType.RouteList && menuType != MenuType.GameStart && menuType != MenuType.Packages) && size.X > ItemWidth)
					ItemWidth = size.X;
			}
		}
	}
}
