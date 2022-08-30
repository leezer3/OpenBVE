using System;
using System.Collections.Generic;
using OpenBveApi.Math;
using OpenBveApi.Objects;

namespace OpenBveApi.Routes
{
	/// <summary>Represents a quad tree.</summary>
	public class QuadTree
	{
		/// <summary>The root node that encapsulates all other quad nodes.</summary>
		internal QuadNode Root;

		/// <summary>The side length of a leaf node.</summary>
		internal double SideLength;

		internal List<ObjectState> Objects = new List<ObjectState>();

		/// <summary>Creates an empty quad with a null root node.</summary>
		/// <param name="sideLength">The side length of a leaf node.</param>
		public QuadTree(double sideLength)
		{
			Root = null;
			SideLength = sideLength;
		}

		/// <summary>Adds a new instance of a static object to the quad.</summary>
		/// <param name="objectState">The reference to an object state.</param>
		/// <param name="orientation">The absolute world orientation of the object.</param>
		public void Add(ObjectState objectState, Orientation3 orientation)
		{
			if (Objects.Contains(objectState))
			{
				// object state is already in the quad tree
				return;
			}
			Objects.Add(objectState);
			if (Root == null)
			{
				// the root node does not exist yet
				Vector3 quadPosition = new Vector3(0.0, objectState.WorldPosition.Y, 0.0);
				QuadTreePopulatedLeafNode leaf = new QuadTreePopulatedLeafNode(
					null,
					new QuadTreeBounds(
						objectState.WorldPosition.X - 0.5 * SideLength,
						objectState.WorldPosition.X + 0.5 * SideLength,
						objectState.WorldPosition.Z - 0.5 * SideLength,
						objectState.WorldPosition.Z + 0.5 * SideLength
					),
					objectState
				);
				leaf.UpdateBoundingRectangle(objectState, quadPosition, orientation);
				Root = leaf;
			}
			else
			{
				// the root node exists
				while (true)
				{
					if (objectState.WorldPosition != Vector3.Zero)
					{
						int b = 0;
						b++;
					}
					if (objectState.WorldPosition.X >= Root.Rectangle.Left & objectState.WorldPosition.X <= Root.Rectangle.Right & objectState.WorldPosition.Z >= Root.Rectangle.Near & objectState.WorldPosition.Z <= Root.Rectangle.Far)
					{
						// the position is within the bounds of the root node
						QuadNode node = Root;
						double left = Root.Rectangle.Left;
						double right = Root.Rectangle.Right;
						double near = Root.Rectangle.Near;
						double far = Root.Rectangle.Far;
						while (true)
						{
							if (node is QuadTreePopulatedLeafNode)
							{
								// populated leaf node
								Vector3 quadPosition = new Vector3(
									objectState.WorldPosition.X - 0.5 * (left + right),
									objectState.WorldPosition.Y,
									objectState.WorldPosition.Z - 0.5 * (near + far)
								);
								QuadTreePopulatedLeafNode leaf = (QuadTreePopulatedLeafNode)node;
								if (leaf.StaticObjectCount == leaf.Objects.Length)
								{
									Array.Resize(ref leaf.Objects, leaf.Objects.Length << 1);
								}

								leaf.VisibleLeafNodes = null;
								leaf.Objects[leaf.StaticObjectCount] = objectState;
								leaf.StaticObjectCount++;
								leaf.UpdateBoundingRectangle(objectState, quadPosition, orientation);
								break;
							}

							if (node is QuadInternalNode)
							{
								// internal node
								QuadInternalNode intern = (QuadInternalNode)node;
								int index;
								double centerX = 0.5 * (left + right);
								double centerZ = 0.5 * (near + far);
								if (objectState.WorldPosition.Z <= centerZ)
								{
									if (objectState.WorldPosition.X <= centerX)
									{
										index = 0;
										right = centerX;
										far = centerZ;
									}
									else
									{
										index = 1;
										left = centerX;
										far = centerZ;
									}
								}
								else
								{
									if (objectState.WorldPosition.X <= centerX)
									{
										index = 2;
										right = centerX;
										near = centerZ;
									}
									else
									{
										index = 3;
										left = centerX;
										near = centerZ;
									}
								}

								if (intern.Children[index] is QuadTreeUnpopulatedLeafNode)
								{
									double sideLength = 0.5 * (right - left + far - near);
									const double toleranceFactor = 1.01;
									if (sideLength < toleranceFactor * SideLength)
									{
										// create populated leaf child
										QuadTreePopulatedLeafNode child = new QuadTreePopulatedLeafNode(
											intern,
											new QuadTreeBounds(left, right, near, far),
											null
										);
										child.BoundingRectangle = QuadTreeBounds.Uninitialized;
										intern.Children[index] = child;
										node = child;
									}
									else
									{
										// create internal child
										QuadInternalNode child = new QuadInternalNode(
											intern,
											new QuadTreeBounds(left, right, near, far),
											new QuadNode[] { null, null, null, null }
										);
										child.Children[0] = new QuadTreeUnpopulatedLeafNode(child, new QuadTreeBounds(left, 0.5 * (left + right), near, 0.5 * (near + far)));
										child.Children[1] = new QuadTreeUnpopulatedLeafNode(child, new QuadTreeBounds(0.5 * (left + right), right, near, 0.5 * (near + far)));
										child.Children[2] = new QuadTreeUnpopulatedLeafNode(child, new QuadTreeBounds(left, 0.5 * (left + right), 0.5 * (near + far), far));
										child.Children[3] = new QuadTreeUnpopulatedLeafNode(child, new QuadTreeBounds(0.5 * (left + right), right, 0.5 * (near + far), far));
										intern.Children[index] = child;
										node = child;
									}
								}
								else
								{
									// go to child
									node = intern.Children[index];
								}
							}
							else
							{
								throw new InvalidOperationException();
							}
						}

						break;
					}

					// the position is outside the bounds of the root node
					if (objectState.WorldPosition.Z <= 0.5 * (Root.Rectangle.Near + Root.Rectangle.Far))
					{
						if (objectState.WorldPosition.X <= 0.5 * (Root.Rectangle.Left + Root.Rectangle.Right))
						{
							// expand toward near-left
							QuadInternalNode intern = new QuadInternalNode(
								null,
								new QuadTreeBounds(
									2.0 * Root.Rectangle.Left - Root.Rectangle.Right,
									Root.Rectangle.Right,
									2.0 * Root.Rectangle.Near - Root.Rectangle.Far,
									Root.Rectangle.Far
								),
								new[] { null, null, null, Root }
							);
							Root.Parent = intern;
							intern.Children[0] = new QuadTreeUnpopulatedLeafNode(intern, new QuadTreeBounds(intern.Rectangle.Left, 0.5 * (intern.Rectangle.Left + intern.Rectangle.Right), intern.Rectangle.Near, 0.5 * (intern.Rectangle.Near + intern.Rectangle.Far)));
							intern.Children[1] = new QuadTreeUnpopulatedLeafNode(intern, new QuadTreeBounds(0.5 * (intern.Rectangle.Left + intern.Rectangle.Right), intern.Rectangle.Right, intern.Rectangle.Near, 0.5 * (intern.Rectangle.Near + intern.Rectangle.Far)));
							intern.Children[2] = new QuadTreeUnpopulatedLeafNode(intern, new QuadTreeBounds(intern.Rectangle.Left, 0.5 * (intern.Rectangle.Left + intern.Rectangle.Right), 0.5 * (intern.Rectangle.Near + intern.Rectangle.Far), intern.Rectangle.Far));
							intern.UpdateBoundingRectangle();
							Root = intern;
						}
						else
						{
							// expand toward near-right
							QuadInternalNode intern = new QuadInternalNode(
								null,
								new QuadTreeBounds(
									Root.Rectangle.Left,
									2.0 * Root.Rectangle.Right - Root.Rectangle.Left,
									2.0 * Root.Rectangle.Near - Root.Rectangle.Far,
									Root.Rectangle.Far
								),
								new[] { null, null, Root, null }
							);
							Root.Parent = intern;
							intern.Children[0] = new QuadTreeUnpopulatedLeafNode(intern, new QuadTreeBounds(intern.Rectangle.Left, 0.5 * (intern.Rectangle.Left + intern.Rectangle.Right), intern.Rectangle.Near, 0.5 * (intern.Rectangle.Near + intern.Rectangle.Far)));
							intern.Children[1] = new QuadTreeUnpopulatedLeafNode(intern, new QuadTreeBounds(0.5 * (intern.Rectangle.Left + intern.Rectangle.Right), intern.Rectangle.Right, intern.Rectangle.Near, 0.5 * (intern.Rectangle.Near + intern.Rectangle.Far)));
							intern.Children[3] = new QuadTreeUnpopulatedLeafNode(intern, new QuadTreeBounds(0.5 * (intern.Rectangle.Left + intern.Rectangle.Right), intern.Rectangle.Right, 0.5 * (intern.Rectangle.Near + intern.Rectangle.Far), intern.Rectangle.Far));
							intern.UpdateBoundingRectangle();
							Root = intern;
						}
					}
					else
					{
						if (objectState.WorldPosition.X <= 0.5 * (Root.Rectangle.Left + Root.Rectangle.Right))
						{
							// expand toward far-left
							QuadInternalNode intern = new QuadInternalNode(
								null,
								new QuadTreeBounds(
									2.0 * Root.Rectangle.Left - Root.Rectangle.Right,
									Root.Rectangle.Right,
									Root.Rectangle.Near,
									2.0 * Root.Rectangle.Far - Root.Rectangle.Near
								),
								new[] { null, Root, null, null }
							);
							Root.Parent = intern;
							intern.Children[0] = new QuadTreeUnpopulatedLeafNode(intern, new QuadTreeBounds(intern.Rectangle.Left, 0.5 * (intern.Rectangle.Left + intern.Rectangle.Right), intern.Rectangle.Near, 0.5 * (intern.Rectangle.Near + intern.Rectangle.Far)));
							intern.Children[2] = new QuadTreeUnpopulatedLeafNode(intern, new QuadTreeBounds(intern.Rectangle.Left, 0.5 * (intern.Rectangle.Left + intern.Rectangle.Right), 0.5 * (intern.Rectangle.Near + intern.Rectangle.Far), intern.Rectangle.Far));
							intern.Children[3] = new QuadTreeUnpopulatedLeafNode(intern, new QuadTreeBounds(0.5 * (intern.Rectangle.Left + intern.Rectangle.Right), intern.Rectangle.Right, 0.5 * (intern.Rectangle.Near + intern.Rectangle.Far), intern.Rectangle.Far));
							intern.UpdateBoundingRectangle();
							Root = intern;
						}
						else
						{
							// expand toward far-right
							QuadInternalNode intern = new QuadInternalNode(
								null,
								new QuadTreeBounds(
									Root.Rectangle.Left,
									2.0 * Root.Rectangle.Right - Root.Rectangle.Left,
									Root.Rectangle.Near,
									2.0 * Root.Rectangle.Far - Root.Rectangle.Near
								),
								new[] { Root, null, null, null }
							);
							Root.Parent = intern;
							intern.Children[1] = new QuadTreeUnpopulatedLeafNode(intern, new QuadTreeBounds(0.5 * (intern.Rectangle.Left + intern.Rectangle.Right), intern.Rectangle.Right, intern.Rectangle.Near, 0.5 * (intern.Rectangle.Near + intern.Rectangle.Far)));
							intern.Children[2] = new QuadTreeUnpopulatedLeafNode(intern, new QuadTreeBounds(intern.Rectangle.Left, 0.5 * (intern.Rectangle.Left + intern.Rectangle.Right), 0.5 * (intern.Rectangle.Near + intern.Rectangle.Far), intern.Rectangle.Far));
							intern.Children[3] = new QuadTreeUnpopulatedLeafNode(intern, new QuadTreeBounds(0.5 * (intern.Rectangle.Left + intern.Rectangle.Right), intern.Rectangle.Right, 0.5 * (intern.Rectangle.Near + intern.Rectangle.Far), intern.Rectangle.Far));
							intern.UpdateBoundingRectangle();
							Root = intern;
						}
					}
				}
			}
		}

		/// <summary>Creates the visibility lists for all nodes in this quad collection.</summary>
		/// <param name="viewingDistance">The viewing distance.</param>
		/// <remarks>Call this function whenever the viewing distance changes.</remarks>
		public void CreateVisibilityLists(double viewingDistance)
		{
			FinalizeBoundingRectangles(Root);
			CreateVisibilityLists(Root, viewingDistance);
		}

		/// <summary>Finalizes the bounding rectangles for the specified node and all its child nodes.</summary>
		/// <param name="node">The node for which to finalize visiblity lists.</param>
		/// <remarks>This function is used to get rid of uninitialized bounding rectangles.</remarks>
		private void FinalizeBoundingRectangles(QuadNode node)
		{
			if (node is QuadInternalNode)
			{
				QuadInternalNode intern = (QuadInternalNode)node;
				for (int i = 0; i < intern.Children.Length; i++)
				{
					FinalizeBoundingRectangles(intern.Children[i]);
				}
			}
			else if (node is QuadTreeLeafNode)
			{
				QuadTreeLeafNode leaf = (QuadTreeLeafNode)node;
				if (leaf.BoundingRectangle.Left > leaf.BoundingRectangle.Right | leaf.BoundingRectangle.Near > leaf.BoundingRectangle.Far)
				{
					if (leaf is QuadTreePopulatedLeafNode)
					{
						QuadTreeUnpopulatedLeafNode unpopulated = new QuadTreeUnpopulatedLeafNode(leaf.Parent, leaf.Rectangle);
						unpopulated.BoundingRectangle = unpopulated.Rectangle;
						for (int i = 0; i < leaf.Parent.Children.Length; i++)
						{
							if (unpopulated.Parent.Children[i] == leaf)
							{
								unpopulated.Parent.Children[i] = unpopulated;
							}
						}

						unpopulated.Parent.UpdateBoundingRectangle();
					}
					else
					{
						leaf.BoundingRectangle = leaf.Rectangle;
						leaf.Parent.UpdateBoundingRectangle();
					}
				}
			}
		}

		/// <summary>Creates the visibility lists for the specified node and all its child nodes.</summary>
		/// <param name="node">The node for which to create visiblity lists.</param>
		/// <param name="viewingDistance">The viewing distance.</param>
		private void CreateVisibilityLists(QuadNode node, double viewingDistance)
		{
			if (node is QuadInternalNode)
			{
				QuadInternalNode intern = (QuadInternalNode)node;
				for (int i = 0; i < intern.Children.Length; i++)
				{
					CreateVisibilityLists(intern.Children[i], viewingDistance);
				}
			}
			else if (node is QuadTreeLeafNode)
			{
				QuadTreeLeafNode leaf = (QuadTreeLeafNode)node;
				CreateVisibilityList(leaf, viewingDistance);
			}
		}

		/// <summary>Creates the visibility list for the specified leaf node.</summary>
		/// <param name="leaf">The leaf node for which to create its visibility list.</param>
		/// <param name="viewingDistance">The viewing distance.</param>
		private void CreateVisibilityList(QuadTreeLeafNode leaf, double viewingDistance)
		{
			List<QuadTreePopulatedLeafNode> nodes = new List<QuadTreePopulatedLeafNode>();
			CreateVisibilityList(leaf, Root, nodes, viewingDistance);
			leaf.VisibleLeafNodes = nodes.ToArray();
		}

		/// <summary>Creates the visibility list for the specified leaf node.</summary>
		/// <param name="leaf">The leaf node for which to create its visibility list.</param>
		/// <param name="node">The node to potentially include in the visiblity list if visible.</param>
		/// <param name="nodes">The list of visible leaf nodes.</param>
		/// <param name="viewingDistance">The viewing distance.</param>
		private void CreateVisibilityList(QuadTreeLeafNode leaf, QuadNode node, List<QuadTreePopulatedLeafNode> nodes, double viewingDistance)
		{
			if (node != null)
			{
				bool visible;
				if (
					leaf.BoundingRectangle.Left <= node.BoundingRectangle.Right &
					leaf.BoundingRectangle.Right >= node.BoundingRectangle.Left &
					leaf.BoundingRectangle.Near <= node.BoundingRectangle.Far &
					leaf.BoundingRectangle.Far >= node.BoundingRectangle.Near
				)
				{
					/*
					 * If the bounding rectangles intersect directly, the node is
					 * definately visible from at least some point inside the leaf.
					 * */
					visible = true;
				}
				else if (
					leaf.BoundingRectangle.Left - viewingDistance <= node.BoundingRectangle.Right &
					leaf.BoundingRectangle.Right + viewingDistance >= node.BoundingRectangle.Left &
					leaf.BoundingRectangle.Near - viewingDistance <= node.BoundingRectangle.Far &
					leaf.BoundingRectangle.Far + viewingDistance >= node.BoundingRectangle.Near
				)
				{
					/*
					 * If the leaf bounding rectangle extended by the viewing distance
					 * in all directions intersects with the node bounding rectangle,
					 * visibility is at least a possibility.
					 * */
					if (
						leaf.BoundingRectangle.Left <= node.BoundingRectangle.Right &
						leaf.BoundingRectangle.Right >= node.BoundingRectangle.Left |
						leaf.BoundingRectangle.Near <= node.BoundingRectangle.Far &
						leaf.BoundingRectangle.Far >= node.BoundingRectangle.Near
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
						if (leaf.BoundingRectangle.Right <= node.BoundingRectangle.Left)
						{
							if (leaf.BoundingRectangle.Far <= node.BoundingRectangle.Near)
							{
								double x = leaf.BoundingRectangle.Right - node.BoundingRectangle.Left;
								double y = leaf.BoundingRectangle.Far - node.BoundingRectangle.Near;
								visible = x * x + y * y <= viewingDistance * viewingDistance;
							}
							else
							{
								double x = leaf.BoundingRectangle.Right - node.BoundingRectangle.Left;
								double y = leaf.BoundingRectangle.Near - node.BoundingRectangle.Far;
								visible = x * x + y * y <= viewingDistance * viewingDistance;
							}
						}
						else
						{
							if (leaf.BoundingRectangle.Far <= node.BoundingRectangle.Near)
							{
								double x = leaf.BoundingRectangle.Left - node.BoundingRectangle.Right;
								double y = leaf.BoundingRectangle.Far - node.BoundingRectangle.Near;
								visible = x * x + y * y <= viewingDistance * viewingDistance;
							}
							else
							{
								double x = leaf.BoundingRectangle.Left - node.BoundingRectangle.Right;
								double y = leaf.BoundingRectangle.Near - node.BoundingRectangle.Far;
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
					if (node is QuadInternalNode)
					{
						QuadInternalNode intern = (QuadInternalNode)node;
						for (int i = 0; i < intern.Children.Length; i++)
						{
							CreateVisibilityList(leaf, intern.Children[i], nodes, viewingDistance);
						}
					}
					else if (node is QuadTreePopulatedLeafNode)
					{
						nodes.Add((QuadTreePopulatedLeafNode)node);
					}
				}
			}
		}

		/// <summary>Gets the leaf node for a specified position.</summary>
		/// <param name="position">The position.</param>
		/// <param name="leaf">Receives the leaf node on success.</param>
		/// <returns>The success of the operation.</returns>
		public bool GetLeafNode(Vector3 position, out QuadTreeLeafNode leaf)
		{
			return GetLeafNode(position, Root, out leaf);
		}

		private bool GetLeafNode(Vector3 position, QuadNode node, out QuadTreeLeafNode leaf)
		{
			if (node != null)
			{
				if (
					position.X >= node.Rectangle.Left &
					position.X <= node.Rectangle.Right &
					position.Z >= node.Rectangle.Near &
					position.Z <= node.Rectangle.Far
				)
				{
					if (node is QuadInternalNode)
					{
						QuadInternalNode intern = (QuadInternalNode)node;
						for (int i = 0; i < intern.Children.Length; i++)
						{
							if (GetLeafNode(position, intern.Children[i], out leaf))
							{
								return true;
							}
						}

						leaf = null;
						return false;
					}

					if (node is QuadTreeLeafNode)
					{
						leaf = (QuadTreeLeafNode)node;
						return true;
					}

					throw new InvalidOperationException();
				}

				leaf = null;
				return false;
			}

			leaf = null;
			return false;
		}
	}
}
