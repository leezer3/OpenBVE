using System.Collections.Generic;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Textures;

namespace OpenBveApi.Routes
{
	/// <summary>Represents an abstract leaf node.</summary>
	public abstract class QuadTreeLeafNode : QuadNode
	{
		/// <summary>The list of all leaf nodes in the quad collection that are visible from within the bounding rectangle of this filler node.</summary>
		public QuadTreePopulatedLeafNode[] VisibleLeafNodes;

		/// <inheritdoc />
		public override bool GetLeafNode(Vector3 position, out QuadTreeLeafNode leaf)
		{
			if (position.X >= Rectangle.Left &
			    position.X <= Rectangle.Right &
			    position.Z >= Rectangle.Near &
			    position.Z <= Rectangle.Far)
			{
				leaf = this;
				return true;
			}
			leaf = null;
			return false;
		}

		/// <inheritdoc />
		public override void CreateVisibilityLists(QuadNode rootNode, double viewingDistance)
		{
			List<QuadTreePopulatedLeafNode> nodes = new List<QuadTreePopulatedLeafNode>();
			CreateVisibilityList(rootNode, nodes, viewingDistance);
			VisibleLeafNodes = nodes.ToArray();
		}

		/// <summary>Creates the visibility list for the specified leaf node.</summary>
		/// <param name="node">The node to potentially include in the visiblity list if visible.</param>
		/// <param name="nodes">The list of visible leaf nodes.</param>
		/// <param name="viewingDistance">The viewing distance.</param>
		private void CreateVisibilityList(QuadNode node, List<QuadTreePopulatedLeafNode> nodes, double viewingDistance)
		{
			if (node != null && viewingDistance > 0)
			{
				bool visible;
				if (
					BoundingRectangle.Left <= node.BoundingRectangle.Right &
					BoundingRectangle.Right >= node.BoundingRectangle.Left &
					BoundingRectangle.Near <= node.BoundingRectangle.Far &
					BoundingRectangle.Far >= node.BoundingRectangle.Near
				)
				{
					/*
					 * If the bounding rectangles intersect directly, the node is
					 * definately visible from at least some point inside the leaf.
					 * */
					visible = true;
				}
				else if (
					BoundingRectangle.Left - viewingDistance <= node.BoundingRectangle.Right &
					BoundingRectangle.Right + viewingDistance >= node.BoundingRectangle.Left &
					BoundingRectangle.Near - viewingDistance <= node.BoundingRectangle.Far &
					BoundingRectangle.Far + viewingDistance >= node.BoundingRectangle.Near
				)
				{
					/*
					 * If the leaf bounding rectangle extended by the viewing distance
					 * in all directions intersects with the node bounding rectangle,
					 * visibility is at least a possibility.
					 * */
					if (
						BoundingRectangle.Left <= node.BoundingRectangle.Right &
						BoundingRectangle.Right >= node.BoundingRectangle.Left |
						BoundingRectangle.Near <= node.BoundingRectangle.Far &
						BoundingRectangle.Far >= node.BoundingRectangle.Near
					)
					{
						/*
						 * The bounding rectangles intersect, but either only on
						 * the x-axis, or on the y-axis. This case is always visible
						 * given that the above constraint (extension by viewing
						 * distance) is also met.
						 * */
						visible = true;
					}
					else
					{
						/*
						 * The bounding rectangles don't intersect on either axis.
						 * Visibility is given if the smallest vertex-to-vertex
						 * distance is smaller than the viewing distance.
						 * */
						if (BoundingRectangle.Right <= node.BoundingRectangle.Left)
						{
							if (BoundingRectangle.Far <= node.BoundingRectangle.Near)
							{
								double x = BoundingRectangle.Right - node.BoundingRectangle.Left;
								double y = BoundingRectangle.Far - node.BoundingRectangle.Near;
								visible = x * x + y * y <= viewingDistance * viewingDistance;
							}
							else
							{
								double x = BoundingRectangle.Right - node.BoundingRectangle.Left;
								double y = BoundingRectangle.Near - node.BoundingRectangle.Far;
								visible = x * x + y * y <= viewingDistance * viewingDistance;
							}
						}
						else
						{
							if (BoundingRectangle.Far <= node.BoundingRectangle.Near)
							{
								double x = BoundingRectangle.Left - node.BoundingRectangle.Right;
								double y = BoundingRectangle.Far - node.BoundingRectangle.Near;
								visible = x * x + y * y <= viewingDistance * viewingDistance;
							}
							else
							{
								double x = BoundingRectangle.Left - node.BoundingRectangle.Right;
								double y = BoundingRectangle.Near - node.BoundingRectangle.Far;
								visible = x * x + y * y <= viewingDistance * viewingDistance;
							}
						}
					}
				}
				else
				{
					/*
					 * If the leaf bounding rectangle extended by the viewing distance
					 * in all directions doesn't intersect with the node bounding rectangle,
					 * visibility is not possible.
					 * */
					visible = false;
				}

				if (visible)
				{
					/*
					 * The node is visible and is either added to the list of visible nodes if
					 * a leaf node, or recursively processed for all children if an internal node.
					 * */
					if (node is QuadInternalNode intern)
					{
						for (int i = 0; i < intern.Children.Length; i++)
						{
							CreateVisibilityList(intern.Children[i], nodes, viewingDistance);
						}
					}
					else if (node is QuadTreePopulatedLeafNode leafNode)
					{
						nodes.Add(leafNode);
					}
				}
			}
		}
	}

	/// <summary>Represents an unpopulated leaf node.</summary>
	internal class QuadTreeUnpopulatedLeafNode : QuadTreeLeafNode
	{
		internal QuadTreeUnpopulatedLeafNode(QuadInternalNode parent, QuadTreeBounds rectangle)
		{
			Parent = parent;
			Rectangle = rectangle;
			BoundingRectangle = QuadTreeBounds.Uninitialized;
			VisibleLeafNodes = null;
		}

		public override void FinalizeBoundingRectangles()
		{
			if (BoundingRectangle.Left > BoundingRectangle.Right | BoundingRectangle.Near > BoundingRectangle.Far)
			{
				BoundingRectangle = Rectangle;
				Parent.UpdateBoundingRectangle();
			}
		}
	}

	/// <summary>Represents a populated leaf node.</summary>
	public class QuadTreePopulatedLeafNode : QuadTreeLeafNode
	{
		/// <summary>A list of static objects attached to this quad node.</summary>
		public ObjectState[] Objects;

		/// <summary>The number of static objects attached to this quad node.</summary>
		internal int StaticObjectCount;
		

		internal QuadTreePopulatedLeafNode(QuadInternalNode parent, QuadTreeBounds rectangle, ObjectState initialState)
		{
			Parent = parent;
			Rectangle = rectangle;
			BoundingRectangle = QuadTreeBounds.Uninitialized;
			VisibleLeafNodes = null;
			if (initialState != null)
			{
				Objects = new[] { initialState };
				StaticObjectCount = 1;
			}
			else
			{
				Objects = new ObjectState[] { null };
				StaticObjectCount = 0;
			}
		}

		internal QuadTreePopulatedLeafNode(QuadTreeUnpopulatedLeafNode unpopulated)
		{
			Parent = unpopulated.Parent;
			Rectangle = unpopulated.Rectangle;
			BoundingRectangle = QuadTreeBounds.Uninitialized;
			VisibleLeafNodes = null;
			Objects = new ObjectState[] { null };
			StaticObjectCount = 0;
		}

		/// <summary>Takes an object, its position and orientation on the quad node, and then updates the bounding rectangle accordingly.</summary>
		/// <param name="objectState">A reference to the object state.</param>
		/// <param name="quadPosition">The position of the object relative to the center of the contained quad node.</param>
		/// <param name="quadOrientation">The orientation of the object.</param>
		internal void UpdateBoundingRectangle(ObjectState objectState, Vector3 quadPosition, Orientation3 quadOrientation)
		{
			Vector3 quadCenter = new Vector3(
				0.5 * (Rectangle.Left + Rectangle.Right),
				0.0,
				0.5 * (Rectangle.Near + Rectangle.Far)
			);
			Vector3 absolutePosition = quadCenter + quadPosition;
			for (int i = 0; i < objectState.Prototype.Mesh.Vertices.Length; i++)
			{
				Vector3 vector = absolutePosition + Vector3.Rotate(objectState.Prototype.Mesh.Vertices[i].Coordinates, quadOrientation);
				if (vector.X < BoundingRectangle.Left)
				{
					BoundingRectangle.Left = vector.X;
				}

				if (vector.X > BoundingRectangle.Right)
				{
					BoundingRectangle.Right = vector.X;
				}

				if (vector.Z < BoundingRectangle.Near)
				{
					BoundingRectangle.Near = vector.Z;
				}

				if (vector.Z > BoundingRectangle.Far)
				{
					BoundingRectangle.Far = vector.Z;
				}
			}

			Parent?.UpdateBoundingRectangle();
		}

		/// <summary>Ensures that all textures that are used by the static objects in this leaf node have been loaded.</summary>
		internal void LoadTextures()
		{
			for (int i = 0; i < StaticObjectCount; i++)
			{
				for (int j = 0; j < Objects[i].Prototype.Mesh.Faces.Length; j++)
				{
					Texture texture = Objects[i].Prototype.Mesh.Materials[i].DaytimeTexture;
					if (texture != null)
					{
						//Textures.LoadTexture(apiHandle.TextureIndex, true);
					}
				}
			}
		}

		/// <inheritdoc />
		public override void FinalizeBoundingRectangles()
		{
			if (BoundingRectangle.Left > BoundingRectangle.Right | BoundingRectangle.Near > BoundingRectangle.Far)
			{
				QuadTreeUnpopulatedLeafNode unpopulated = new QuadTreeUnpopulatedLeafNode(Parent, Rectangle);
				unpopulated.BoundingRectangle = unpopulated.Rectangle;
				for (int i = 0; i < Parent.Children.Length; i++)
				{
					if (unpopulated.Parent.Children[i] == this)
					{
						unpopulated.Parent.Children[i] = unpopulated;
					}
				}

				unpopulated.Parent.UpdateBoundingRectangle();
			}
		}
	}
}
