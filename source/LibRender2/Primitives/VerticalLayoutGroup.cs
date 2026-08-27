using System.Collections.Generic;
using OpenBveApi.Graphics;
using OpenBveApi.Math;

namespace LibRender2.Primitives
{
	/// <summary>A container control that automatically positions its children vertically</summary>
	public class VerticalLayoutGroup : GLControl
	{
		/// <summary>The child controls of this layout group</summary>
		public readonly List<GLControl> Children = new List<GLControl>();

		/// <summary>The vertical spacing between child controls</summary>
		public double Spacing = 10.0;

		/// <summary>The padding inside the layout group borders</summary>
		public Vector2 Padding = new Vector2(10.0, 10.0);

		/// <summary>Creates a new VerticalLayoutGroup</summary>
		public VerticalLayoutGroup(IGLRenderer renderer) : base(renderer)
		{
		}

		/// <summary>Draws the layout group and positions/draws all visible children</summary>
		public override void Draw()
		{
			if (!IsVisible)
			{
				return;
			}

			// Draw background if background color is set and size is non-zero
			if (BackgroundColor.A > 0.0f && Size.X > 0f && Size.Y > 0f)
			{
				Renderer.DrawRectangle(Texture, Location, Size, BackgroundColor, null, (float)CornerRadius);
			}

			double currentY = Location.Y + Padding.Y;
			foreach (var child in Children)
			{
				if (!child.IsVisible)
				{
					continue;
				}

				child.Location.X = Location.X + Padding.X;
				child.Location.Y = currentY;
				child.Size.X = Size.X - (Padding.X * 2);

				child.Draw();

				currentY += child.Size.Y + Spacing;
			}
		}

		/// <summary>Passes mouse down events to children</summary>
		public override void MouseDown(int x, int y)
		{
			if (!IsVisible)
			{
				return;
			}

			foreach (var child in Children)
			{
				if (child.IsVisible)
				{
					child.MouseDown(x, y);
				}
			}
		}

		/// <summary>Passes mouse move events to children</summary>
		public override void MouseMove(int x, int y)
		{
			if (!IsVisible)
			{
				return;
			}

			foreach (var child in Children)
			{
				if (child.IsVisible)
				{
					child.MouseMove(x, y);
				}
			}
		}
	}
}
