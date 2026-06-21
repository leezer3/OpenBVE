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
                private int hoveredIndex = -1;

                public GLDropdown(BaseRenderer renderer) : base(renderer)
                {
                        IsVisible = true;
                        Size = new Vector2(160, 24);
                        BackgroundColor = new Color128(0.15f, 0.15f, 0.15f, 1f);
                }

                /// <summary>Checks whether the dropdown should expand upward to avoid overflowing the screen</summary>
                private bool ShouldExpandUpward()
                {
                        float itemHeight = 24f;
                        float overlayHeight = Items.Count * itemHeight;
                        return (Location.Y + Size.Y + overlayHeight) > Renderer.Screen.Height;
                }

                public override void Draw()
                {
                        if (!IsVisible) return;
                        // Draw main box
                        Color128 boxBg = Expanded ? new Color128(0.22f, 0.22f, 0.26f, 1f) : BackgroundColor;
                        Renderer.Rectangle.Draw(null, Location, Size, boxBg);
                        // Draw border
                        Renderer.Rectangle.Draw(null, Location, new Vector2(Size.X, 1.0), new Color128(0.0f, 0.47f, 0.83f, 1f));
                        // Draw arrow indicator
                        float arrowX = (float)(Location.X + Size.X - 18);
                        float arrowY = (float)(Location.Y + (Size.Y - 5) / 2);
                        if (Expanded)
                        {
                                Renderer.Rectangle.Draw(null, new Vector2(arrowX + 4, arrowY), new Vector2(1, 1), Color128.White);
                                Renderer.Rectangle.Draw(null, new Vector2(arrowX + 3, arrowY + 1), new Vector2(3, 1), Color128.White);
                                Renderer.Rectangle.Draw(null, new Vector2(arrowX + 2, arrowY + 2), new Vector2(5, 1), Color128.White);
                                Renderer.Rectangle.Draw(null, new Vector2(arrowX + 1, arrowY + 3), new Vector2(7, 1), Color128.White);
                                Renderer.Rectangle.Draw(null, new Vector2(arrowX, arrowY + 4), new Vector2(9, 1), Color128.White);
                        }
                        else
                        {
                                Renderer.Rectangle.Draw(null, new Vector2(arrowX, arrowY), new Vector2(9, 1), Color128.White);
                                Renderer.Rectangle.Draw(null, new Vector2(arrowX + 1, arrowY + 1), new Vector2(7, 1), Color128.White);
                                Renderer.Rectangle.Draw(null, new Vector2(arrowX + 2, arrowY + 2), new Vector2(5, 1), Color128.White);
                                Renderer.Rectangle.Draw(null, new Vector2(arrowX + 3, arrowY + 3), new Vector2(3, 1), Color128.White);
                                Renderer.Rectangle.Draw(null, new Vector2(arrowX + 4, arrowY + 4), new Vector2(1, 1), Color128.White);
                        }

                        if (SelectedIndex >= 0 && SelectedIndex < Items.Count)
                        {
                                Renderer.OpenGlString.Draw(Renderer.Fonts.NormalFont, Items[SelectedIndex], new Vector2(Location.X + 8, Location.Y + 2), TextAlignment.TopLeft, Color128.White);
                        }
                }

                public void DrawOverlay()
                {
                        if (!IsVisible || !Expanded || Items.Count == 0) return;

                        float itemHeight = 24f;
                        float overlayHeight = Items.Count * itemHeight;
                        bool expandUp = ShouldExpandUpward();

                        Vector2 overlayLoc = expandUp
                                ? new Vector2(Location.X, Location.Y - overlayHeight)
                                : new Vector2(Location.X, Location.Y + Size.Y);

                        // Draw overlay background
                        Renderer.Rectangle.Draw(null, overlayLoc, new Vector2(Size.X, overlayHeight), new Color128(0.1f, 0.1f, 0.1f, 0.95f));
                        // Draw overlay border
                        Renderer.Rectangle.Draw(null, overlayLoc, new Vector2(Size.X, overlayHeight), new Color128(0.0f, 0.35f, 0.65f, 1f));

                        for (int i = 0; i < Items.Count; i++)
                        {
                                Vector2 itemLoc = expandUp
                                        ? new Vector2(Location.X, Location.Y - overlayHeight + i * itemHeight)
                                        : new Vector2(Location.X, Location.Y + Size.Y + i * itemHeight);
                                if (i == SelectedIndex)
                                {
                                        Renderer.Rectangle.Draw(null, itemLoc, new Vector2(Size.X, itemHeight), Color128.Orange);
                                }
                                else if (i == hoveredIndex)
                                {
                                        Renderer.Rectangle.Draw(null, itemLoc, new Vector2(Size.X, itemHeight), new Color128(0.2f, 0.25f, 0.35f, 1f));
                                }
                                Renderer.OpenGlString.Draw(Renderer.Fonts.NormalFont, Items[i], new Vector2(itemLoc.X + 8, itemLoc.Y + 2), TextAlignment.TopLeft, Color128.White);
                        }
                }

                public override void MouseMove(int x, int y)
                {
                        if (!IsVisible) return;
                        CurrentlySelected = (x >= Location.X && x <= Location.X + Size.X && y >= Location.Y && y <= Location.Y + Size.Y);

                        if (Expanded)
                        {
                                float itemHeight = 24f;
                                float overlayHeight = Items.Count * itemHeight;
                                bool expandUp = ShouldExpandUpward();
                                Vector2 overlayLoc = expandUp
                                        ? new Vector2(Location.X, Location.Y - overlayHeight)
                                        : new Vector2(Location.X, Location.Y + Size.Y);

                                if (x >= overlayLoc.X && x <= overlayLoc.X + Size.X && y >= overlayLoc.Y && y <= overlayLoc.Y + overlayHeight)
                                {
                                        hoveredIndex = (int)((y - overlayLoc.Y) / itemHeight);
                                        if (hoveredIndex < 0 || hoveredIndex >= Items.Count) hoveredIndex = -1;
                                }
                                else
                                {
                                        hoveredIndex = -1;
                                }
                        }
                        else
                        {
                                hoveredIndex = -1;
                        }
                }

                public override void MouseDown(int x, int y)
                {
                        if (!IsVisible) return;
                        if (Expanded)
                        {
                                float itemHeight = 24f;
                                float overlayHeight = Items.Count * itemHeight;
                                bool expandUp = ShouldExpandUpward();
                                Vector2 overlayLoc = expandUp
                                        ? new Vector2(Location.X, Location.Y - overlayHeight)
                                        : new Vector2(Location.X, Location.Y + Size.Y);

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
                                        if (ActiveDropdown == this) ActiveDropdown = null;
                                        hoveredIndex = -1;
                                        return;
                                }
                        }

                        if (x >= Location.X && x <= Location.X + Size.X && y >= Location.Y && y <= Location.Y + Size.Y)
                        {
                                Expanded = !Expanded;
                                if (Expanded)
                                {
                                        if (ActiveDropdown != null && ActiveDropdown != this)
                                        {
                                                ActiveDropdown.Expanded = false;
                                        }
                                        ActiveDropdown = this;
                                }
                                else
                                {
                                        if (ActiveDropdown == this) ActiveDropdown = null;
                                }
                        }
                        else
                        {
                                Expanded = false;
                                if (ActiveDropdown == this) ActiveDropdown = null;
                                hoveredIndex = -1;
                        }
                }

                public static GLDropdown ActiveDropdown;
        }
}
