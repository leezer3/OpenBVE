namespace OpenBveApi.Routes
{
	/// <summary>Represents an internal grid node.</summary>
	internal class QuadInternalNode : QuadNode
	{
		/// <summary>A list of four child nodes or null references.</summary>
		/// <remarks>The child nodes are stored in the order: near-left, near-right, far-left and far-right. Individual members may be null references.</remarks>
		internal QuadNode[] Children;

		internal QuadInternalNode(QuadInternalNode parent, QuadTreeBounds rectangle, QuadNode[] children)
		{
			Parent = parent;
			Rectangle = rectangle;
			BoundingRectangle = QuadTreeBounds.Uninitialized;
			Children = children;
		}

		internal void UpdateBoundingRectangle()
		{
			BoundingRectangle = new QuadTreeBounds();
			for (int i = 0; i < Children.Length; i++)
			{
				if (Children[i] != null)
				{
					if (Children[i].BoundingRectangle.Left < BoundingRectangle.Left)
					{
						BoundingRectangle.Left = Children[i].BoundingRectangle.Left;
					}

					if (Children[i].BoundingRectangle.Right > BoundingRectangle.Right)
					{
						BoundingRectangle.Right = Children[i].BoundingRectangle.Right;
					}

					if (Children[i].BoundingRectangle.Near < BoundingRectangle.Near)
					{
						BoundingRectangle.Near = Children[i].BoundingRectangle.Near;
					}

					if (Children[i].BoundingRectangle.Far > BoundingRectangle.Far)
					{
						BoundingRectangle.Far = Children[i].BoundingRectangle.Far;
					}
				}
			}

			if (Parent is QuadInternalNode)
			{
				QuadInternalNode intern = Parent;
				intern.UpdateBoundingRectangle();
			}
		}
	}
}
