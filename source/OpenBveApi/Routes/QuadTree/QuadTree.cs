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
		internal readonly double SideLength;

		internal readonly List<ObjectState> Objects;

		/// <summary>Creates an empty quad with a null root node.</summary>
		/// <param name="sideLength">The side length of a leaf node.</param>
		public QuadTree(double sideLength)
		{
			Root = null;
			SideLength = sideLength;
			Objects = new List<ObjectState>();
		}

		/// <summary>Clears the quad tree</summary>
		public void Clear()
		{
			Objects.Clear();
			Root = null;
		}

		/// <summary>Adds a new instance of a static object to the quad.</summary>
		/// <param name="objectState">The reference to an object state.</param>
		/// <param name="orientation">The absolute world orientation of the object.</param>
		public void Add(ObjectState objectState, Orientation3 orientation)
		{
			if (Objects.Contains(objectState) || !Vector3.IsFinite(objectState.WorldPosition))
			{
				// object state is already in the quad tree, or has an invalid world position
				return;
			}
			Objects.Add(objectState);
			if (Root == null)
			{
				// the root node does not exist yet
				// as our object must be in world-space, it's world position is a good starting point for the tree, as
				// routes may start at any arbritary track position
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
							if (node is QuadTreePopulatedLeafNode leaf)
							{
								// populated leaf node
								Vector3 quadPosition = new Vector3(
									objectState.WorldPosition.X - 0.5 * (left + right),
									objectState.WorldPosition.Y,
									objectState.WorldPosition.Z - 0.5 * (near + far)
								);
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

							if (node is QuadInternalNode intern)
							{
								// internal node
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

		/// <summary>Initializes the quad tree</summary>
		/// <param name="viewingDistance">The viewing distance.</param>
		/// <remarks>Call this function whenever the viewing distance changes.</remarks>
		public void Initialize(double viewingDistance)
		{
			if (Root == null)
			{
				return;
			}
			if (viewingDistance <= 0)
			{
				throw new InvalidOperationException("Invalid viewing distance.");
			}
			Root.FinalizeBoundingRectangles();
			Root.CreateVisibilityLists(Root, viewingDistance);
		}
		
		/// <summary>Gets the leaf node for a specified position.</summary>
		/// <param name="position">The position.</param>
		/// <param name="leaf">Receives the leaf node on success.</param>
		/// <returns>The success of the operation.</returns>
		public bool GetLeafNode(Vector3 position, out QuadTreeLeafNode leaf)
		{
			if (Root != null)
			{
				return Root.GetLeafNode(position, out leaf);
			}

			leaf = null;
			return false;

		}
	}
}
