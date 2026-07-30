using System.Collections.Generic;
using OpenBveApi.Math;

namespace LibRender2.Primitives
{
        public abstract class GLContainer : GLControl
        {
                public readonly List<GLControl> Children = new List<GLControl>();

                protected GLContainer(BaseRenderer renderer) : base(renderer)
                {
                }

                /// <summary>Whether this container should clip child rendering to its own bounds</summary>
                public bool ClipChildren = false;

                public override void Draw()
                {
                        if (!IsVisible) return;
                        for (int i = 0; i < Children.Count; i++)
                        {
                                if (Children[i].IsVisible && Children[i].ClipToParent && ClipChildren)
                                {
                                        // Simple clipping check: skip drawing if child is entirely outside container bounds
                                        if (Children[i].Location.X + Children[i].Size.X < Location.X ||
                                            Children[i].Location.X > Location.X + Size.X ||
                                            Children[i].Location.Y + Children[i].Size.Y < Location.Y ||
                                            Children[i].Location.Y > Location.Y + Size.Y)
                                        {
                                                continue;
                                        }
                                }
                                Children[i].Draw();
                        }
                }

                public override void MouseMove(int x, int y)
                {
                        if (!IsVisible) return;
                        for (int i = 0; i < Children.Count; i++)
                        {
                                if (Children[i].IsVisible)
                                {
                                        Children[i].MouseMove(x, y);
                                }
                        }
                }

                public override void MouseDown(int x, int y)
                {
                        if (!IsVisible) return;
                        for (int i = 0; i < Children.Count; i++)
                        {
                                if (Children[i].IsVisible)
                                {
                                        Children[i].MouseDown(x, y);
                                }
                        }
                }

                public override void MouseUp(int x, int y)
                {
                        if (!IsVisible) return;
                        for (int i = 0; i < Children.Count; i++)
                        {
                                if (Children[i].IsVisible)
                                {
                                        Children[i].MouseUp(x, y);
                                }
                        }
                }

                public override void MouseWheel(int delta)
                {
                        if (!IsVisible) return;
                        for (int i = 0; i < Children.Count; i++)
                        {
                                if (Children[i].IsVisible)
                                {
                                        Children[i].MouseWheel(delta);
                                }
                        }
                }
        }
}
