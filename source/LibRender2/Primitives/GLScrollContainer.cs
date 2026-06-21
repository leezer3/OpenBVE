using OpenBveApi.Math;

namespace LibRender2.Primitives
{
	public class GLScrollContainer : GLContainer
	{
		public double ScrollOffset = 0.0;
		public double MaxScroll = 0.0;

		public GLScrollContainer(BaseRenderer renderer) : base(renderer)
		{
			IsVisible = true;
		}

		public override void Draw()
		{
			if (!IsVisible) return;
			double totalHeight = 0.0;
			for (int i = 0; i < Children.Count; i++)
			{
				if (!Children[i].IsVisible) continue;
				Vector2 originalLocation = Children[i].Location;
				Children[i].Location = new Vector2(originalLocation.X, originalLocation.Y - ScrollOffset);
				if (Children[i].Location.Y >= Location.Y && Children[i].Location.Y + Children[i].Size.Y <= Location.Y + Size.Y)
				{
					Children[i].Draw();
				}
				Children[i].Location = originalLocation;
				totalHeight += Children[i].Size.Y + 4.0;
			}
			MaxScroll = System.Math.Max(0.0, totalHeight - Size.Y);
		}

		public void Scroll(double delta)
		{
			ScrollOffset = System.Math.Max(0.0, System.Math.Min(MaxScroll, ScrollOffset + delta));
		}
	}
}
