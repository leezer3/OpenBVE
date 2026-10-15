using OpenBveApi.Math;
using OpenBveApi.Colors;

namespace LibRender2.Primitives
{
        public enum BoxDirection
        {
                Horizontal,
                Vertical
        }

        public class GLBoxContainer : GLContainer
        {
                public BoxDirection Direction = BoxDirection.Vertical;
                public float Spacing = 4f;
                /// <summary>Padding applied to all sides of the container before laying out children</summary>
                public float Padding = 0f;

                /// <summary>Whether to draw a background (and hover/focus highlight) behind the children</summary>
                public bool DrawBackground = false;
                /// <summary>Highlight color applied when the pointer is over this container</summary>
                public Color128 HoverColor = new Color128(1.0f, 1.0f, 1.0f, 0.06f);
                /// <summary>Highlight color applied when this container holds keyboard focus</summary>
                public Color128 FocusColor = new Color128(0.0f, 0.47f, 0.83f, 0.18f);

                public GLBoxContainer(BaseRenderer renderer) : base(renderer)
                {
                        IsVisible = true;
                }

                /// <summary>Positions children along the container's layout direction and recurses into them</summary>
                public override void PerformLayout()
                {
                        double offset = Padding;
                        for (int i = 0; i < Children.Count; i++)
                        {
                                if (!Children[i].IsVisible) continue;
                                if (Direction == BoxDirection.Vertical)
                                {
                                        Children[i].Location = new Vector2(Location.X + Padding, Location.Y + offset);
                                        offset += Children[i].Size.Y + Spacing;
                                }
                                else
                                {
                                        Children[i].Location = new Vector2(Location.X + offset, Location.Y + Padding);
                                        offset += Children[i].Size.X + Spacing;
                                }
                                Children[i].PerformLayout();
                        }
                }

                public override void Draw()
                {
                        if (!IsVisible) return;
                        // Layout is deterministic and idempotent: run it each frame so input and draw agree
                        PerformLayout();
                        if (DrawBackground)
                        {
                                Color128 bg = KeyboardFocused ? FocusColor : (CurrentlySelected ? HoverColor : BackgroundColor);
                                if (bg.A > 0.001f)
                                {
                                        Renderer.Rectangle.Draw(null, Location, Size, bg);
                                }
                        }
                        base.Draw();
                }
        }

        public class GLHBoxContainer : GLBoxContainer
        {
                public GLHBoxContainer(BaseRenderer renderer) : base(renderer)
                {
                        Direction = BoxDirection.Horizontal;
                }
        }

        public class GLVBoxContainer : GLBoxContainer
        {
                public GLVBoxContainer(BaseRenderer renderer) : base(renderer)
                {
                        Direction = BoxDirection.Vertical;
                }
        }
}
