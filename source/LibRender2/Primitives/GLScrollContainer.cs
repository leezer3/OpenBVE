using OpenBveApi.Math;
using OpenBveApi.Colors;

namespace LibRender2.Primitives
{
        public class GLScrollContainer : GLContainer
        {
                public double ScrollOffset = 0.0;
                public double MaxScroll = 0.0;
                public float ScrollbarWidth = 8f;
                public Color128 ScrollbarColor = new Color128(0.4f, 0.4f, 0.45f, 1f);
                public Color128 ScrollbarHandleColor = new Color128(0.6f, 0.6f, 0.65f, 1f);

                public GLScrollContainer(BaseRenderer renderer) : base(renderer)
                {
                        IsVisible = true;
                        ClipChildren = true;
                }

                public override void Draw()
                {
                        if (!IsVisible) return;
                        double totalHeight = 0.0;
                        for (int i = 0; i < Children.Count; i++)
                        {
                                if (!Children[i].IsVisible) continue;
                                totalHeight += Children[i].Size.Y + 4.0;
                        }
                        MaxScroll = System.Math.Max(0.0, totalHeight - Size.Y);

                        // Draw children with scroll offset and clipping
                        for (int i = 0; i < Children.Count; i++)
                        {
                                if (!Children[i].IsVisible) continue;
                                Vector2 originalLocation = Children[i].Location;
                                Children[i].Location = new Vector2(originalLocation.X, originalLocation.Y - ScrollOffset);
                                // Only draw if visible within container bounds
                                if (Children[i].Location.Y + Children[i].Size.Y > Location.Y &&
                                    Children[i].Location.Y < Location.Y + Size.Y)
                                {
                                        Children[i].Draw();
                                }
                                Children[i].Location = originalLocation;
                        }

                        // Draw scrollbar if content overflows
                        if (MaxScroll > 0)
                        {
                                float scrollbarX = (float)(Location.X + Size.X - ScrollbarWidth);
                                // Track
                                Renderer.Rectangle.Draw(null, new Vector2(scrollbarX, (float)Location.Y), new Vector2(ScrollbarWidth, (float)Size.Y), ScrollbarColor);
                                // Handle
                                float handleHeight = (float)(Size.Y / (totalHeight) * Size.Y);
                                float handleY = (float)(Location.Y + (ScrollOffset / MaxScroll) * (Size.Y - handleHeight));
                                Renderer.Rectangle.Draw(null, new Vector2(scrollbarX, handleY), new Vector2(ScrollbarWidth, handleHeight), ScrollbarHandleColor);
                        }
                }

                public override void MouseWheel(int delta)
                {
                        Scroll(delta);
                }

                public void Scroll(double delta)
                {
                        ScrollOffset = System.Math.Max(0.0, System.Math.Min(MaxScroll, ScrollOffset + delta));
                }
        }
}
