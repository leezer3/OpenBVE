using OpenBveApi.Math;

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

			Parent?.UpdateBoundingRectangle();
		}

		public override void FinalizeBoundingRectangles()
		{
			for (int i = 0; i < Children.Length; i++)
			{
				Children[i].FinalizeBoundingRectangles();
			}
		}

		public override bool GetLeafNode(Vector3 position, out QuadTreeLeafNode leaf)
		{
			if (position.X >= Rectangle.Left &
			    position.X <= Rectangle.Right &
			    position.Z >= Rectangle.Near &
			    position.Z <= Rectangle.Far)
			{
				for (int i = 0; i < Children.Length; i++)
				{
					if (Children[i].GetLeafNode(position, out leaf))
					{
						return true;
					}
				}
			}
			leaf = null;
			return false;
		}

		public override void CreateVisibilityLists(QuadNode rootNode, double viewingDistance)
		{
			for (int i = 0; i < Children.Length; i++)
			{
				Children[i].CreateVisibilityLists(rootNode, viewingDistance);
			}
		}
	}
}
