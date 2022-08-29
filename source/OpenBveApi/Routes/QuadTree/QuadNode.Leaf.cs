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
	}

	/// <summary>Represents a populated leaf node.</summary>
	public class QuadTreePopulatedLeafNode : QuadTreeLeafNode
	{
		/// <summary>A list of static objects attached to this quad node.</summary>
		public ObjectState[] Objects;

		/// <summary>The number of static objects attached to this quad node.</summary>
		internal int StaticObjectCount;

		/// <summary>A list of handles to transparent faces as obtained from the renderer.</summary>
		internal object[] TransparentFaces;

		/// <summary>The number of handles to transparent faces.</summary>
		public int TransparentFaceCount;

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

			TransparentFaces = new object[1];
			TransparentFaceCount = 0;
		}

		internal QuadTreePopulatedLeafNode(QuadTreeUnpopulatedLeafNode unpopulated)
		{
			Parent = unpopulated.Parent;
			Rectangle = unpopulated.Rectangle;
			BoundingRectangle = QuadTreeBounds.Uninitialized;
			VisibleLeafNodes = null;
			Objects = new ObjectState[] { null };
			StaticObjectCount = 0;
			TransparentFaces = new object[1];
			TransparentFaceCount = 0;
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

			if (Parent is QuadInternalNode)
			{
				QuadInternalNode intern = Parent;
				intern.UpdateBoundingRectangle();
			}
		}

		/// <summary>Ensures that all textures that are used by the static objects in this leaf node have been loaded.</summary>
		internal void LoadTextures()
		{
			for (int i = 0; i < StaticObjectCount; i++)
			{
				for (int j = 0; j < Objects[i].Prototype.Mesh.Faces.Length; j++)
				{
					int material = Objects[i].Prototype.Mesh.Faces[j].Material;
					Texture texture = Objects[i].Prototype.Mesh.Materials[i].DaytimeTexture;
					if (texture != null)
					{
						//Textures.LoadTexture(apiHandle.TextureIndex, true);
					}
				}
			}
		}
	}
}
