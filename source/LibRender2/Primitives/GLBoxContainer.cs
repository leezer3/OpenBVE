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

		public GLBoxContainer(BaseRenderer renderer) : base(renderer)
		{
			IsVisible = true;
		}

		public override void Draw()
		{
			if (!IsVisible) return;
			// Update child positions based on direction and spacing
			double offset = 0.0;
			for (int i = 0; i < Children.Count; i++)
			{
				if (!Children[i].IsVisible) continue;
				if (Direction == BoxDirection.Vertical)
				{
					Children[i].Location = new Vector2(Location.X, Location.Y + offset);
					offset += Children[i].Size.Y + Spacing;
				}
				else
				{
					Children[i].Location = new Vector2(Location.X + offset, Location.Y);
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
