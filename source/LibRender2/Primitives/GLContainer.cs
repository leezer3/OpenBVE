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

                /// <summary>The last absolute screen position the pointer was seen at, used for wheel routing</summary>
                protected int LastPointerX = -1;
                protected int LastPointerY = -1;

                public override void Draw()
                {
                        if (!IsVisible) return;
                        for (int i = 0; i < Children.Count; i++)
                        {
                                if (!Children[i].IsVisible) continue;
                                if (Children[i].ClipToParent && ClipChildren)
                                {
                                        // Skip drawing children which lie entirely outside this container's bounds
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

                /// <summary>Runs a layout pass for this container and recursively for its children</summary>
                public override void PerformLayout()
                {
                        for (int i = 0; i < Children.Count; i++)
                        {
                                Children[i].PerformLayout();
                        }
                }

                /// <summary>Tests whether the given point lies within this container or any of its visible children</summary>
                public override bool HitTest(int x, int y)
                {
                        if (!IsVisible || !Enabled) return false;
                        if (x >= Location.X && x <= Location.X + Size.X && y >= Location.Y && y <= Location.Y + Size.Y) return true;
                        for (int i = 0; i < Children.Count; i++)
                        {
                                if (Children[i].HitTest(x, y)) return true;
                        }
                        return false;
                }

                /// <summary>Clears transient hover state from this container and its children</summary>
                public override void ClearHover()
                {
                        base.ClearHover();
                        for (int i = 0; i < Children.Count; i++)
                        {
                                Children[i].ClearHover();
                        }
                }

                public override void MouseMove(int x, int y)
                {
                        if (!IsVisible || !Enabled) return;
                        LastPointerX = x;
                        LastPointerY = y;
                        CurrentlySelected = true;
                        for (int i = 0; i < Children.Count; i++)
                        {
                                GLControl child = Children[i];
                                if (!child.IsVisible) continue;
                                // A control being dragged (e.g. a slider) keeps receiving moves even if the pointer leaves it
                                if (child.HitTest(x, y) || child.IsPressed)
                                {
                                        child.MouseMove(x, y);
                                }
                                else
                                {
                                        child.ClearHover();
                                }
                        }
                }

                public override void MouseDown(int x, int y)
                {
                        if (!IsVisible || !Enabled) return;
                        LastPointerX = x;
                        LastPointerY = y;
                        // An expanded dropdown captures input so clicks land on its overlay, not controls underneath
                        if (GLDropdown.ActiveDropdown != null)
                        {
                                GLDropdown dropdown = GLDropdown.ActiveDropdown;
                                if (dropdown.HitTest(x, y))
                                {
                                        dropdown.MouseDown(x, y);
                                        return;
                                }
                                // Clicking outside an expanded dropdown closes it
                                dropdown.Expanded = false;
                                GLDropdown.ActiveDropdown = null;
                        }
                        // Topmost child (last added) wins
                        for (int i = Children.Count - 1; i >= 0; i--)
                        {
                                GLControl child = Children[i];
                                if (!child.IsVisible) continue;
                                if (child.HitTest(x, y))
                                {
                                        child.MouseDown(x, y);
                                        return;
                                }
                        }
                }

                public override void MouseUp(int x, int y)
                {
                        if (!IsVisible || !Enabled) return;
                        LastPointerX = x;
                        LastPointerY = y;
                        for (int i = 0; i < Children.Count; i++)
                        {
                                GLControl child = Children[i];
                                if (child.IsVisible && (child.HitTest(x, y) || child.IsPressed))
                                {
                                        child.MouseUp(x, y);
                                }
                        }
                }

                public override void MouseWheel(int delta)
                {
                        if (!IsVisible || !Enabled) return;
                        if (LastPointerX < 0)
                        {
                                return;
                        }
                        for (int i = Children.Count - 1; i >= 0; i--)
                        {
                                GLControl child = Children[i];
                                if (child.IsVisible && child.HitTest(LastPointerX, LastPointerY))
                                {
                                        child.MouseWheel(delta);
                                        return;
                                }
                        }
                }
        }
}
