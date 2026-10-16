using System.Collections.Generic;
using OpenBveApi.Math;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenTK.Input;

namespace LibRender2.Primitives
{
        public class GLTabContainer : GLContainer
        {
                public List<string> TabTitles = new List<string>();
                public List<GLControl> TabContents = new List<GLControl>();
                public int SelectedTab = 0;

                public float TabHeaderHeight = 30f;
                public Color128 SelectedTabColor = new Color128(0.0f, 0.47f, 0.83f, 1.0f);
                public Color128 InactiveTabColor = new Color128(0.15f, 0.15f, 0.15f, 1.0f);
                public Color128 HoverTabColor = new Color128(0.2f, 0.2f, 0.24f, 1.0f);
                public Color128 ContentBackgroundColor = new Color128(0.1f, 0.1f, 0.12f, 1.0f);
                /// <summary>Horizontal padding on each side of a tab title (tabs are content-sized, Blender-style)</summary>
                public float TabPadding = 12f;

                private int hoveredTab = -1;

                public GLTabContainer(BaseRenderer renderer) : base(renderer)
                {
                        IsVisible = true;
                        ClipChildren = true;
                }

                public void AddTab(string title, GLControl content)
                {
                        TabTitles.Add(title);
                        TabContents.Add(content);
                        Children.Add(content);
                }

                /// <summary>The content control for the currently selected tab, if any</summary>
                public GLControl SelectedContent => SelectedTab >= 0 && SelectedTab < TabContents.Count ? TabContents[SelectedTab] : null;

                private Vector2 ContentLocation => new Vector2(Location.X, Location.Y + TabHeaderHeight);
                private Vector2 ContentSize => new Vector2(Size.X, Size.Y - TabHeaderHeight);

                /// <summary>Measures each tab title for content-sized (Blender-style) header layout</summary>
                private float[] GetTabWidths()
                {
                        float[] widths = new float[TabTitles.Count];
                        float total = 0f;
                        for (int i = 0; i < TabTitles.Count; i++)
                        {
                                widths[i] = (float)Renderer.Fonts.NormalFont.MeasureString(TabTitles[i]).X + TabPadding * 2f;
                                total += widths[i];
                        }
                        // Shrink proportionally when the bar is narrower than the tabs (narrow sidebar)
                        if (total > Size.X && total > 0f)
                        {
                                float scale = (float)Size.X / total;
                                for (int i = 0; i < widths.Length; i++) widths[i] *= scale;
                        }
                        return widths;
                }

                /// <summary>Trims a tab title with an ellipsis so it fits the given tab width</summary>
                private string FitTitle(string title, float width)
                {
                        float avail = width - TabPadding * 2f;
                        if (avail <= 0f) return "";
                        if ((float)Renderer.Fonts.NormalFont.MeasureString(title).X <= avail) return title;
                        const string dots = "...";
                        if ((float)Renderer.Fonts.NormalFont.MeasureString(dots).X > avail) return "";
                        string trimmed = title;
                        while (trimmed.Length > 1 && (float)Renderer.Fonts.NormalFont.MeasureString(trimmed + dots).X > avail)
                        {
                                trimmed = trimmed.Substring(0, trimmed.Length - 1);
                        }
                        return trimmed + dots;
                }

                /// <summary>Returns the tab index under the given x co-ordinate, or -1</summary>
                private int TabAtX(float x)
                {
                        float[] widths = GetTabWidths();
                        float cx = (float)Location.X;
                        for (int i = 0; i < widths.Length; i++)
                        {
                                if (x >= cx && x <= cx + widths[i]) return i;
                                cx += widths[i];
                        }
                        return -1;
                }

                public override void PerformLayout()
                {
                        Vector2 contentLoc = ContentLocation;
                        Vector2 contentSize = ContentSize;
                        for (int i = 0; i < TabContents.Count; i++)
                        {
                                GLControl content = TabContents[i];
                                if (content == null) continue;
                                content.IsVisible = (i == SelectedTab);
                                if (content.IsVisible)
                                {
                                        content.Location = contentLoc;
                                        content.Size = contentSize;
                                        content.PerformLayout();
                                }
                        }
                }

                public override void Draw()
                {
                        if (!IsVisible || TabTitles.Count == 0) return;
                        PerformLayout();

                        // Blender-style headers: content-sized tabs aligned left; only the
                        // active tab gets a rounded background with an accent underline
                        float[] tabWidths = GetTabWidths();
                        float tabX = (float)Location.X;
                        for (int i = 0; i < TabTitles.Count; i++)
                        {
                                Vector2 headerLoc = new Vector2(tabX, Location.Y);
                                Vector2 headerSize = new Vector2(tabWidths[i], TabHeaderHeight);

                                bool isSelected = (i == SelectedTab);
                                bool isHovered = (i == hoveredTab);
                                if (isSelected)
                                {
                                        Color128 tabBg = new Color128(0.2f, 0.22f, 0.25f, 0.9f);
                                        Renderer.Rectangle.DrawRounded(null, headerLoc, headerSize, 6f, tabBg);
                                        // Square off the bottom corners so the tab merges into the content below
                                        Renderer.Rectangle.Draw(null, new Vector2(headerLoc.X, headerLoc.Y + TabHeaderHeight - 6f), new Vector2(headerSize.X, 6f), tabBg);
                                        // Draw active underline
                                        Renderer.Rectangle.Draw(null, new Vector2(headerLoc.X, headerLoc.Y + TabHeaderHeight - 3f), new Vector2(headerSize.X, 3f), SelectedTabColor);
                                }
                                else if (isHovered)
                                {
                                        Renderer.Rectangle.DrawRounded(null, headerLoc, headerSize, 6f, HoverTabColor);
                                }

                                // Draw text title left-aligned with padding (trimmed on narrow bars)
                                string title = FitTitle(TabTitles[i], tabWidths[i]);
                                Vector2 textSize = Renderer.Fonts.NormalFont.MeasureString(title);
                                Vector2 textPos = new Vector2(headerLoc.X + TabPadding, headerLoc.Y + (TabHeaderHeight - textSize.Y) / 2f);
                                Renderer.OpenGlString.Draw(Renderer.Fonts.NormalFont, title, textPos, TextAlignment.TopLeft, Color128.White);
                                tabX += tabWidths[i];
                        }

                        // Draw content area background
                        Renderer.Rectangle.DrawRounded(null, ContentLocation, ContentSize, 10f, ContentBackgroundColor);

                        // Children hold the tab contents; only the selected one is visible
                        base.Draw();
                }

                public void SelectTab(int index)
                {
                        if (index < 0 || index >= TabTitles.Count) return;
                        SelectedTab = index;
                        PerformLayout();
                }

                public override void MouseMove(int x, int y)
                {
                        if (!IsVisible || TabTitles.Count == 0) return;

                        if (x >= Location.X && x <= Location.X + Size.X && y >= Location.Y && y <= Location.Y + TabHeaderHeight)
                        {
                                hoveredTab = TabAtX(x);
                        }
                        else
                        {
                                hoveredTab = -1;
                        }

                        base.MouseMove(x, y);
                }

                public override void MouseDown(int x, int y)
                {
                        if (!IsVisible || TabTitles.Count == 0) return;

                        if (x >= Location.X && x <= Location.X + Size.X && y >= Location.Y && y <= Location.Y + TabHeaderHeight)
                        {
                                int clickedTab = TabAtX(x);
                                if (clickedTab >= 0 && clickedTab < TabTitles.Count)
                                {
                                        SelectTab(clickedTab);
                                        return;
                                }
                        }

                        base.MouseDown(x, y);
                }

                public override void MouseWheel(int delta)
                {
                        GLControl content = SelectedContent;
                        if (content != null)
                        {
                                content.MouseWheel(delta);
                        }
                }

                public override void KeyDown(Key key)
                {
                        if (key == Key.Left)
                        {
                                SelectTab(SelectedTab - 1);
                                return;
                        }
                        if (key == Key.Right)
                        {
                                SelectTab(SelectedTab + 1);
                                return;
                        }
                        GLControl content = SelectedContent;
                        if (content != null)
                        {
                                content.KeyDown(key);
                        }
                }
        }
}
