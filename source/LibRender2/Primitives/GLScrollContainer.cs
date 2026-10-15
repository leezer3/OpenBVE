using OpenBveApi.Math;
using OpenBveApi.Colors;
using OpenTK.Input;

namespace LibRender2.Primitives
{
        /// <summary>A vertically scrollable container which paginates its children.</summary>
        /// <remarks>
        /// Children are laid out top-to-bottom (baked with the current scroll offset, so draw
        /// and hit-testing always agree). Only children which fit entirely within the viewport
        /// are made visible, which keeps content from spilling outside the container without
        /// needing GL scissor clipping.
        /// </remarks>
        public class GLScrollContainer : GLContainer
        {
                /// <summary>The current vertical scroll offset in pixels (0 = top)</summary>
                public double ScrollOffset = 0.0;
                /// <summary>The maximum valid scroll offset</summary>
                public double MaxScroll = 0.0;
                /// <summary>The measured total height of the content</summary>
                public double ContentHeight = 0.0;

                /// <summary>Padding applied inside the container before laying out children</summary>
                public float Padding = 0f;
                /// <summary>Vertical spacing between children</summary>
                public float Spacing = 4f;
                /// <summary>The width of the scrollbar</summary>
                public float ScrollbarWidth = 8f;
                /// <summary>How far one wheel notch / arrow key scrolls, in pixels</summary>
                public double Step = 48.0;

                /// <summary>Whether children are stretched to the container's inner width</summary>
                public bool StretchChildren = true;

                public Color128 ScrollbarTrackColor = new Color128(0.25f, 0.25f, 0.28f, 1f);
                public Color128 ScrollbarHandleColor = new Color128(0.55f, 0.55f, 0.6f, 1f);

                private bool draggingScrollbar = false;
                private double dragStartY = 0.0;
                private double dragStartOffset = 0.0;

                public GLScrollContainer(BaseRenderer renderer) : base(renderer)
                {
                        IsVisible = true;
                        ClipChildren = true;
                }

                /// <summary>The top of the scrollable viewport</summary>
                public double ViewportTop => Location.Y + Padding;
                /// <summary>The height of the scrollable viewport</summary>
                public double ViewportHeight => System.Math.Max(0.0, Size.Y - Padding * 2);
                /// <summary>The X co-ordinate of the scrollbar</summary>
                private double ScrollbarX => Location.X + Size.X - ScrollbarWidth;
                /// <summary>The height of the scrollbar handle</summary>
                private double HandleHeight => ContentHeight <= 0 ? ViewportHeight : System.Math.Max(24.0, ViewportHeight * (ViewportHeight / ContentHeight));

                public override void PerformLayout()
                {
                        double y = 0.0;
                        double innerWidth = System.Math.Max(0.0, Size.X - Padding * 2 - (StretchChildren ? ScrollbarWidth : 0));
                        for (int i = 0; i < Children.Count; i++)
                        {
                                GLControl child = Children[i];
                                if (child == null) continue;
                                if (StretchChildren)
                                {
                                        child.Size = new Vector2((float)innerWidth, child.Size.Y);
                                }
                                child.Location = new Vector2(Location.X + Padding, Location.Y + Padding + y - ScrollOffset);
                                y += child.Size.Y + Spacing;
                                child.PerformLayout();
                        }
                        ContentHeight = y > 0 ? y - Spacing : 0.0;
                        MaxScroll = System.Math.Max(0.0, ContentHeight - ViewportHeight);
                        ClampScroll();

                        // Only children which fit entirely within the viewport are visible and interactive
                        double top = ViewportTop;
                        double bottom = ViewportTop + ViewportHeight;
                        for (int i = 0; i < Children.Count; i++)
                        {
                                GLControl child = Children[i];
                                if (child == null) continue;
                                child.IsVisible = child.Location.Y >= top - 0.5 && child.Location.Y + child.Size.Y <= bottom + 0.5;
                        }
                }

                public override void Draw()
                {
                        if (!IsVisible) return;
                        PerformLayout();
                        base.Draw();
                        DrawScrollbar();
                }

                private void DrawScrollbar()
                {
                        if (MaxScroll <= 0) return;
                        float sx = (float)ScrollbarX;
                        Renderer.Rectangle.Draw(null, new Vector2(sx, (float)ViewportTop), new Vector2(ScrollbarWidth, (float)ViewportHeight), ScrollbarTrackColor);
                        float handleHeight = (float)HandleHeight;
                        float handleY = (float)(ViewportTop + (ScrollOffset / MaxScroll) * (ViewportHeight - HandleHeight));
                        Renderer.Rectangle.Draw(null, new Vector2(sx, handleY), new Vector2(ScrollbarWidth, handleHeight), ScrollbarHandleColor);
                }

                private void ClampScroll()
                {
                        if (ScrollOffset < 0.0) ScrollOffset = 0.0;
                        if (ScrollOffset > MaxScroll) ScrollOffset = MaxScroll;
                }

                /// <summary>Scrolls by the given number of pixels (positive scrolls down)</summary>
                public void ScrollBy(double delta)
                {
                        ScrollOffset += delta;
                        ClampScroll();
                        PerformLayout();
                }

                /// <summary>Scrolls the container so that the given child is fully visible</summary>
                public void EnsureVisible(GLControl child)
                {
                        if (child == null) return;
                        double offset = 0.0;
                        for (int i = 0; i < Children.Count; i++)
                        {
                                GLControl c = Children[i];
                                if (c == null) continue;
                                if (c == child) break;
                                offset += c.Size.Y + Spacing;
                        }
                        if (offset - ScrollOffset < 0.0)
                        {
                                ScrollOffset = offset;
                        }
                        else if (offset + child.Size.Y - ScrollOffset > ViewportHeight)
                        {
                                ScrollOffset = offset + child.Size.Y - ViewportHeight;
                        }
                        ClampScroll();
                        PerformLayout();
                }

                public override void MouseWheel(int delta)
                {
                        ScrollBy(-delta * Step);
                }

                public override void KeyDown(Key key)
                {
                        switch (key)
                        {
                                case Key.Up:
                                        ScrollBy(-Step);
                                        break;
                                case Key.Down:
                                        ScrollBy(Step);
                                        break;
                                case Key.PageUp:
                                        ScrollBy(-ViewportHeight);
                                        break;
                                case Key.PageDown:
                                        ScrollBy(ViewportHeight);
                                        break;
                                case Key.Home:
                                        ScrollOffset = 0.0;
                                        ClampScroll();
                                        PerformLayout();
                                        break;
                                case Key.End:
                                        ScrollOffset = MaxScroll;
                                        ClampScroll();
                                        PerformLayout();
                                        break;
                        }
                }

                private bool IsOnScrollbar(int x, int y)
                {
                        return MaxScroll > 0 && x >= ScrollbarX && x <= ScrollbarX + ScrollbarWidth && y >= ViewportTop && y <= ViewportTop + ViewportHeight;
                }

                public override void MouseDown(int x, int y)
                {
                        if (!IsVisible || !Enabled) return;
                        if (IsOnScrollbar(x, y))
                        {
                                draggingScrollbar = true;
                                dragStartY = y;
                                dragStartOffset = ScrollOffset;
                                return;
                        }
                        base.MouseDown(x, y);
                }

                public override void MouseMove(int x, int y)
                {
                        if (!IsVisible || !Enabled) return;
                        if (draggingScrollbar)
                        {
                                double range = ViewportHeight - HandleHeight;
                                if (range > 0.0)
                                {
                                        ScrollOffset = dragStartOffset + (y - dragStartY) * (MaxScroll / range);
                                        ClampScroll();
                                        PerformLayout();
                                }
                                return;
                        }
                        base.MouseMove(x, y);
                }

                public override void MouseUp(int x, int y)
                {
                        if (draggingScrollbar)
                        {
                                draggingScrollbar = false;
                                return;
                        }
                        base.MouseUp(x, y);
                }
        }
}
