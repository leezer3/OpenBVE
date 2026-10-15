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

                        // Draw tab headers
                        float tabWidth = (float)(Size.X / TabTitles.Count);
                        for (int i = 0; i < TabTitles.Count; i++)
                        {
                                Vector2 headerLoc = new Vector2(Location.X + i * tabWidth, Location.Y);
                                Vector2 headerSize = new Vector2(tabWidth, TabHeaderHeight);

                                bool isSelected = (i == SelectedTab);
                                bool isHovered = (i == hoveredTab);
                                Color128 headerBg;
                                if (isSelected)
                                {
                                        headerBg = new Color128(0.2f, 0.22f, 0.25f, 0.9f);
                                }
                                else if (isHovered)
                                {
                                        headerBg = HoverTabColor;
                                }
                                else
                                {
                                        headerBg = InactiveTabColor;
                                }
                                Renderer.Rectangle.Draw(null, headerLoc, headerSize, headerBg);

                                // Draw active underline
                                if (isSelected)
                                {
                                        Renderer.Rectangle.Draw(null, new Vector2(headerLoc.X, headerLoc.Y + TabHeaderHeight - 3f), new Vector2(tabWidth, 3f), SelectedTabColor);
                                }

                                // Draw text title centered
                                Vector2 textSize = Renderer.Fonts.NormalFont.MeasureString(TabTitles[i]);
                                Vector2 textPos = new Vector2(headerLoc.X + (tabWidth - textSize.X) / 2f, headerLoc.Y + (TabHeaderHeight - textSize.Y) / 2f);
                                Renderer.OpenGlString.Draw(Renderer.Fonts.NormalFont, TabTitles[i], textPos, TextAlignment.TopLeft, Color128.White);
                        }

                        // Draw content area background
                        Renderer.Rectangle.Draw(null, ContentLocation, ContentSize, ContentBackgroundColor);

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
                                float tabWidth = (float)(Size.X / TabTitles.Count);
                                hoveredTab = (int)((x - Location.X) / tabWidth);
                                if (hoveredTab < 0 || hoveredTab >= TabTitles.Count) hoveredTab = -1;
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
                                float tabWidth = (float)(Size.X / TabTitles.Count);
                                int clickedTab = (int)((x - Location.X) / tabWidth);
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
