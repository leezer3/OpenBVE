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

		protected GLControl(BaseRenderer renderer)
		{
			Renderer = renderer;
		}

		/// <summary>Draws the control</summary>
		public abstract void Draw();

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
	}
}
