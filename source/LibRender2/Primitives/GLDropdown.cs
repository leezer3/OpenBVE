using OpenBveApi.Math;
using OpenBveApi.Colors;
using System;
using System.Collections.Generic;
using OpenBveApi.Graphics;

namespace LibRender2.Primitives
{
	public class GLDropdown : GLControl
	{
		public readonly List<string> Items = new List<string>();
		public int SelectedIndex = 0;
		public bool Expanded = false;
		public EventHandler SelectionChanged;

		public GLDropdown(BaseRenderer renderer) : base(renderer)
		{
			IsVisible = true;
			Size = new Vector2(160, 24);
			BackgroundColor = new Color128(0.15f, 0.15f, 0.15f, 1f);
		}

		public override void Draw()
		{
			if (!IsVisible) return;
			Renderer.Rectangle.Draw(null, Location, Size, BackgroundColor);
			Renderer.OpenGlString.Draw(Renderer.Fonts.NormalFont, "v", new Vector2(Location.X + Size.X - 20, Location.Y + 2), TextAlignment.TopLeft, Color128.White);
			if (SelectedIndex >= 0 && SelectedIndex < Items.Count)
			{
				Renderer.OpenGlString.Draw(Renderer.Fonts.NormalFont, Items[SelectedIndex], new Vector2(Location.X + 8, Location.Y + 2), TextAlignment.TopLeft, Color128.White);
			}

			if (Expanded)
			{
				float itemHeight = 24f;
				float overlayHeight = Items.Count * itemHeight;
				Vector2 overlayLoc = new Vector2(Location.X, Location.Y + Size.Y);
				Renderer.Rectangle.Draw(null, overlayLoc, new Vector2(Size.X, overlayHeight), new Color128(0.1f, 0.1f, 0.1f, 0.95f));

				for (int i = 0; i < Items.Count; i++)
				{
					Vector2 itemLoc = new Vector2(Location.X, Location.Y + Size.Y + i * itemHeight);
					if (i == SelectedIndex)
					{
						Renderer.Rectangle.Draw(null, itemLoc, new Vector2(Size.X, itemHeight), Color128.Orange);
					}
					Renderer.OpenGlString.Draw(Renderer.Fonts.NormalFont, Items[i], new Vector2(itemLoc.X + 8, itemLoc.Y + 2), TextAlignment.TopLeft, Color128.White);
				}
			}
		}

		public override void MouseDown(int x, int y)
		{
			if (!IsVisible) return;
			if (Expanded)
			{
				float itemHeight = 24f;
				float overlayHeight = Items.Count * itemHeight;
				Vector2 overlayLoc = new Vector2(Location.X, Location.Y + Size.Y);

				if (x >= overlayLoc.X && x <= overlayLoc.X + Size.X && y >= overlayLoc.Y && y <= overlayLoc.Y + overlayHeight)
				{
					int item = (int)((y - overlayLoc.Y) / itemHeight);
					if (item >= 0 && item < Items.Count)
					{
						SelectedIndex = item;
						SelectionChanged?.Invoke(this, EventArgs.Empty);
						OnClick?.Invoke(this, EventArgs.Empty);
					}
					Expanded = false;
					return;
				}
			}

			if (x >= Location.X && x <= Location.X + Size.X && y >= Location.Y && y <= Location.Y + Size.Y)
			{
				Expanded = !Expanded;
			}
			else
			{
				Expanded = false;
			}
		}
	}
}
