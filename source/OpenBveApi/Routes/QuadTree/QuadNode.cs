using OpenBveApi.Math;

namespace OpenBveApi.Routes
{
	/// <summary>Represents a quad node.</summary>
	public abstract class QuadNode
	{
		/// <summary>The parent of the quad node, or a null reference for the root node.</summary>
		internal QuadInternalNode Parent;

		/// <summary>The bounds of the quad node in world coordinates.</summary>
		public QuadTreeBounds Rectangle;

		/// <summary>The smallest rectangle that encloses all child nodes and attached objects in world coordinates.</summary>
		internal QuadTreeBounds BoundingRectangle;

		/// <summary>Finalizes the bounding rectangles for this node and all its child nodes.</summary>
		/// <remarks>This function is used to get rid of uninitialized bounding rectangles.</remarks>
		public abstract void FinalizeBoundingRectangles();

		/// <summary>Gets the leaf node at the specified position</summary>
		/// <param name="position">The position</param>
		/// <param name="leaf">The leaf node</param>
		/// <returns>True if a valid leaf node is found, false otherwise</returns>
		public virtual bool GetLeafNode(Vector3 position, out QuadTreeLeafNode leaf)
		{
			leaf = null;
			return false;
		}

		public virtual void CreateVisibilityLists(QuadNode rootNode, double viewingDistance)
		{

		}
	}
}
