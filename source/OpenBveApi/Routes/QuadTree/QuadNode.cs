namespace OpenBveApi.Routes
{
	/// <summary>Represents a quad node.</summary>
	internal abstract class QuadNode
	{
		/// <summary>The parent of the quad node, or a null reference for the root node.</summary>
		internal QuadInternalNode Parent;

		/// <summary>The bounds of the quad node in world coordinates.</summary>
		internal QuadTreeBounds Rectangle;

		/// <summary>The smallest rectangle that encloses all child nodes and attached objects in world coordinates.</summary>
		internal QuadTreeBounds BoundingRectangle;
	}
}
