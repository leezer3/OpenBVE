using OpenBveApi.Math;

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

                public GLBoxContainer(BaseRenderer renderer) : base(renderer)
                {
                        IsVisible = true;
                }

                public override void Draw()
                {
                        if (!IsVisible) return;
                        // Update child positions based on direction, spacing and padding
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
