//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2023, Christopher Lees, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using OpenBveApi.Textures;

namespace LibRender2.Primitives
{
        /// <summary>An abstract OpenGL based control</summary>
        public abstract class GLControl
        {
                /// <summary>Holds a reference to the base renderer</summary>
                internal readonly BaseRenderer Renderer;
                /// <summary>The background color for the control</summary>
                public Color128 BackgroundColor;
                /// <summary>The texture for the control</summary>
                public Texture Texture;
                /// <summary>The stored location for the control</summary>
                public Vector2 Location;
                /// <summary>The stored size for the control</summary>
                public Vector2 Size;
                /// <summary>Whether the control is currently selected by the mouse</summary>
                public bool CurrentlySelected;
                /// <summary>The event handler for the OnClick event</summary>
                public EventHandler OnClick;
                /// <summary>Whether the control is currently visible</summary>
                public bool IsVisible;
                /// <summary>Whether the control is currently being pressed</summary>
                public bool IsPressed;
                /// <summary>Whether the control supports clipping to its parent bounds</summary>
                public bool ClipToParent = true;
                /// <summary>The currently globally focused control for keyboard input</summary>
                public static GLControl FocusedControl;

                /// <summary>Whether the control is currently enabled for mouse and keyboard interaction</summary>
                public bool Enabled = true;

                /// <summary>Whether the control currently holds keyboard focus (drawn as a highlight ring)</summary>
                public bool KeyboardFocused = false;

                protected GLControl(BaseRenderer renderer)
                {
                        Renderer = renderer;
                }

                /// <summary>Draws the control</summary>
                public abstract void Draw();

                /// <summary>Passes a keyboard key down event to the control</summary>
                public virtual void KeyDown(OpenTK.Input.Key key)
                {
                }

                /// <summary>Passes a mouse move event to the control</summary>
                /// <param name="x">The absolute screen X co-ordinate</param>
                /// <param name="y">The absolute screen Y co-ordinate</param>
                public virtual void MouseMove(int x, int y)
                {

                }

                /// <summary>Passes a mouse down event to the control</summary>
                /// <param name="x">The absolute screen X co-ordinate</param>
                /// <param name="y">The absolute screen Y co-ordinate</param>
                public virtual void MouseDown(int x, int y)
                {
                        MouseMove(x, y);
                }

                /// <summary>Passes a mouse up event to the control</summary>
                /// <param name="x">The absolute screen X co-ordinate</param>
                /// <param name="y">The absolute screen Y co-ordinate</param>
                public virtual void MouseUp(int x, int y)
                {
                        IsPressed = false;
                }

                /// <summary>Passes a mouse wheel event to the control</summary>
                /// <param name="delta">The scroll wheel delta</param>
                public virtual void MouseWheel(int delta)
                {

                }

                /// <summary>Tests whether a given screen point lies within this control's interactive bounds</summary>
                public virtual bool HitTest(int x, int y)
                {
                        return IsVisible && Enabled && x >= Location.X && x <= Location.X + Size.X && y >= Location.Y && y <= Location.Y + Size.Y;
                }

                /// <summary>Clears transient hover (CurrentlySelected) state from this control</summary>
                public virtual void ClearHover()
                {
                        CurrentlySelected = false;
                }

                /// <summary>Runs a layout pass for this control (and any children)</summary>
                public virtual void PerformLayout()
                {
                }

                /// <summary>Invoked when this control is activated via keyboard navigation</summary>
                public virtual void OnActivate()
                {
                }

                /// <summary>Draws a highlight ring around the control when it holds keyboard focus</summary>
                protected void DrawFocusRing()
                {
                        if (!KeyboardFocused) return;
                        Color128 c = new Color128(0.0f, 0.47f, 0.83f, 1f);
                        Renderer.Rectangle.Draw(null, Location, new Vector2(Size.X, 1f), c);
                        Renderer.Rectangle.Draw(null, new Vector2(Location.X, Location.Y + Size.Y - 1f), new Vector2(Size.X, 1f), c);
                        Renderer.Rectangle.Draw(null, Location, new Vector2(1f, Size.Y), c);
                        Renderer.Rectangle.Draw(null, new Vector2(Location.X + Size.X - 1f, Location.Y), new Vector2(1f, Size.Y), c);
                }

                /// <summary>Invoked when this control is adjusted via the keyboard; direction is -1 (decrease) or +1 (increase)</summary>
                /// <returns>True if the control handled the adjustment</returns>
                public virtual bool OnAdjust(int direction)
                {
                        return false;
                }
        }
}
