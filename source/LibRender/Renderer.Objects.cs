using System;
using OpenBveApi.Graphics;
using OpenBveApi.Objects;
using OpenBveApi.Textures;

namespace LibRender
{
	public static partial class Renderer
	{
		/// <summary>Re-adds all objects within the world, for example after a screen resolution change</summary>
		public static void ReAddObjects()
		{
			RendererObject[] list = new RendererObject[ObjectCount];
			for (int i = 0; i < ObjectCount; i++)
			{
				list[i] = Objects[i];
			}
			for (int i = 0; i < list.Length; i++)
			{
				HideObject(ref list[i].InternalObject);
			}
			for (int i = 0; i < list.Length; i++)
			{
				ShowObject(list[i].InternalObject, list[i].Type);
			}
		}

		/// <summary>Makes an object visible within the world</summary>
		/// <param name="objectToShow">The object to show</param>
		/// <param name="Type">Whether this is a static or dynamic object</param>
		public static void ShowObject(StaticObject objectToShow, ObjectType Type)
		{
			if (objectToShow == null)
			{
				return;
			}
			if (objectToShow.RendererIndex == 0)
			{
				if (ObjectCount >= Objects.Length)
				{
					Array.Resize<RendererObject>(ref Objects, Objects.Length << 1);
				}

				Objects[ObjectCount] = new RendererObject(objectToShow, Type);
				int f = objectToShow.Mesh.Faces.Length;
				Objects[ObjectCount].FaceListReferences = new ObjectListReference[f];
				for (int i = 0; i < f; i++)
				{
					bool alpha = false;
					int k = objectToShow.Mesh.Faces[i].Material;
					OpenGlTextureWrapMode wrap = OpenGlTextureWrapMode.ClampClamp;
					if (objectToShow.Mesh.Materials[k].DaytimeTexture != null | objectToShow.Mesh.Materials[k].NighttimeTexture != null)
					{
						if (objectToShow.Mesh.Materials[k].WrapMode == null)
						{
							// If the object does not have a stored wrapping mode, determine it now
							for (int v = 0; v < objectToShow.Mesh.Vertices.Length; v++)
							{
								if (objectToShow.Mesh.Vertices[v].TextureCoordinates.X < 0.0f |
								    objectToShow.Mesh.Vertices[v].TextureCoordinates.X > 1.0f)
								{
									wrap |= OpenGlTextureWrapMode.RepeatClamp;
								}
								if (objectToShow.Mesh.Vertices[v].TextureCoordinates.Y < 0.0f |
								    objectToShow.Mesh.Vertices[v].TextureCoordinates.Y > 1.0f)
								{
									wrap |= OpenGlTextureWrapMode.ClampRepeat;
								}
							}							
						}
						else
						{
							//Yuck cast, but we need the null, as otherwise requires rewriting the texture indexer
							wrap = (OpenGlTextureWrapMode)objectToShow.Mesh.Materials[k].WrapMode;
						}
						if (objectToShow.Mesh.Materials[k].DaytimeTexture != null)
						{
							if (currentHost.LoadTexture(objectToShow.Mesh.Materials[k].DaytimeTexture, wrap))
							{
								TextureTransparencyType type =
									objectToShow.Mesh.Materials[k].DaytimeTexture.Transparency;
								if (type == TextureTransparencyType.Alpha)
								{
									alpha = true;
								}
								else if (type == TextureTransparencyType.Partial && currentOptions.TransparencyMode == TransparencyMode.Quality)
								{
									alpha = true;
								}
							}
						}
						if (objectToShow.Mesh.Materials[k].NighttimeTexture != null)
						{
							if (currentHost.LoadTexture(objectToShow.Mesh.Materials[k].NighttimeTexture, wrap))
							{
								TextureTransparencyType type =
									objectToShow.Mesh.Materials[k].NighttimeTexture.Transparency;
								if (type == TextureTransparencyType.Alpha)
								{
									alpha = true;
								}
								else if (type == TextureTransparencyType.Partial & currentOptions.TransparencyMode == TransparencyMode.Quality)
								{
									alpha = true;
								}
							}
						}
					}
					if (Type == ObjectType.Overlay & CameraProperties.Camera.CurrentRestriction != CameraRestrictionMode.NotAvailable)
					{
						alpha = true;
					}
					else if (objectToShow.Mesh.Materials[k].Color.A != 255)
					{
						alpha = true;
					}
					else if (objectToShow.Mesh.Materials[k].BlendMode == MeshMaterialBlendMode.Additive)
					{
						alpha = true;
					}
					else if (objectToShow.Mesh.Materials[k].GlowAttenuationData != 0)
					{
						alpha = true;
					}
					ObjectListType listType;
					switch (Type)
					{
						case ObjectType.Static:
							listType = alpha ? ObjectListType.DynamicAlpha : ObjectListType.StaticOpaque;
							break;
						case ObjectType.Dynamic:
							listType = alpha ? ObjectListType.DynamicAlpha : ObjectListType.DynamicOpaque;
							break;
						case ObjectType.Overlay:
							listType = alpha ? ObjectListType.OverlayAlpha : ObjectListType.OverlayOpaque;
							break;
						default:
							throw new InvalidOperationException();
					}
					if (listType == ObjectListType.StaticOpaque)
					{
						/*
						 * For the static opaque list, insert the face into
						 * the first vacant position in the matching group's list.
						 * */
						int groupIndex = (int)objectToShow.GroupIndex;
						if (groupIndex >= StaticOpaque.Length)
						{
							if (StaticOpaque.Length == 0)
							{
								StaticOpaque = new ObjectGroup[16];
							}
							while (groupIndex >= StaticOpaque.Length)
							{
								Array.Resize<ObjectGroup>(ref StaticOpaque, StaticOpaque.Length << 1);
							}
						}
						if (StaticOpaque[groupIndex] == null)
						{
							StaticOpaque[groupIndex] = new ObjectGroup();
						}
						ObjectList list = StaticOpaque[groupIndex].List;
						int newIndex = list.FaceCount;
						for (int j = 0; j < list.FaceCount; j++)
						{
							if (list.Faces[j] == null)
							{
								newIndex = j;
								break;
							}
						}
						if (newIndex == list.FaceCount)
						{
							if (list.FaceCount == list.Faces.Length)
							{
								Array.Resize<ObjectFace>(ref list.Faces, list.Faces.Length << 1);
							}
							list.FaceCount++;
						}
						list.Faces[newIndex] = new ObjectFace
						{
							ObjectListIndex = ObjectCount,
							ObjectReference = objectToShow,
							FaceIndex = i,
							Wrap = wrap
						};

						// HACK: Let's store the wrapping mode.

						StaticOpaque[groupIndex].Update = true;
						Objects[ObjectCount].FaceListReferences[i] = new ObjectListReference(listType, newIndex);
						InfoStaticOpaqueFaceCount++;

						/*
						 * Check if the given object has a bounding box, and insert it to the end of the list of bounding boxes if required
						 */
						if (objectToShow.Mesh.BoundingBox != null)
						{
							int Index = list.BoundingBoxes.Length;
							for (int j = 0; j < list.BoundingBoxes.Length; j++)
							{
								if (list.Faces[j] == null)
								{
									Index = j;
									break;
								}
							}
							if (Index == list.BoundingBoxes.Length)
							{
								Array.Resize<BoundingBox>(ref list.BoundingBoxes, list.BoundingBoxes.Length << 1);
							}
							list.BoundingBoxes[Index].Upper = objectToShow.Mesh.BoundingBox[0];
							list.BoundingBoxes[Index].Lower = objectToShow.Mesh.BoundingBox[1];
						}
					}
					else
					{
						/*
						 * For all other lists, insert the face at the end of the list.
						 * */
						ObjectList list;
						switch (listType)
						{
							case ObjectListType.DynamicOpaque:
								list = DynamicOpaque;
								break;
							case ObjectListType.DynamicAlpha:
								list = DynamicAlpha;
								break;
							case ObjectListType.OverlayOpaque:
								list = OverlayOpaque;
								break;
							case ObjectListType.OverlayAlpha:
								list = OverlayAlpha;
								break;
							default:
								throw new InvalidOperationException();
						}
						if (list.FaceCount == list.Faces.Length)
						{
							Array.Resize<ObjectFace>(ref list.Faces, list.Faces.Length << 1);
						}
						list.Faces[list.FaceCount] = new ObjectFace
						{
							ObjectListIndex = ObjectCount,
							ObjectReference = objectToShow,
							FaceIndex = i,
							Wrap = wrap
						};

						// HACK: Let's store the wrapping mode.

						Objects[ObjectCount].FaceListReferences[i] = new ObjectListReference(listType, list.FaceCount);
						list.FaceCount++;
					}

				}
				objectToShow.RendererIndex = ObjectCount + 1;
				ObjectCount++;
			}
		}

		/// <summary>Hides an object within the world</summary>
		/// <param name="Object">The object to hide</param>
		public static void HideObject(ref StaticObject Object)
		{
			if (Object == null)
			{
				return;
			}
			int k = Object.RendererIndex - 1;
			if (k >= 0)
			{		
				// remove faces
				for (int i = 0; i < Objects[k].FaceListReferences.Length; i++)
				{
					ObjectListType listType = Objects[k].FaceListReferences[i].Type;
					if (listType == ObjectListType.StaticOpaque)
					{
						/*
						 * For static opaque faces, set the face to be removed
						 * to a null reference. If there are null entries at
						 * the end of the list, update the number of faces used
						 * accordingly.
						 * */
						int groupIndex = (int)Objects[k].InternalObject.GroupIndex;
						ObjectList list = StaticOpaque[groupIndex].List;
						int listIndex = Objects[k].FaceListReferences[i].Index;
						list.Faces[listIndex] = null;
						if (listIndex == list.FaceCount - 1)
						{
							int count = 0;
							for (int j = list.FaceCount - 2; j >= 0; j--)
							{
								if (list.Faces[j] != null)
								{
									count = j + 1;
									break;
								}
							}
							list.FaceCount = count;
						}

						StaticOpaque[groupIndex].Update = true;
						InfoStaticOpaqueFaceCount--;
					}
					else
					{
						/*
						 * For all other kinds of faces, move the last face into place
						 * of the face to be removed and decrement the face counter.
						 * */
						ObjectList list;
						switch (listType)
						{
							case ObjectListType.DynamicOpaque:
								list = DynamicOpaque;
								break;
							case ObjectListType.DynamicAlpha:
								list = DynamicAlpha;
								break;
							case ObjectListType.OverlayOpaque:
								list = OverlayOpaque;
								break;
							case ObjectListType.OverlayAlpha:
								list = OverlayAlpha;
								break;
							default:
								throw new InvalidOperationException();
						}
						int listIndex = Objects[k].FaceListReferences[i].Index;
						if (list.FaceCount > 0)
						{
							list.Faces[listIndex] = list.Faces[list.FaceCount - 1];
						}
						Objects[list.Faces[listIndex].ObjectListIndex].FaceListReferences[list.Faces[listIndex].FaceIndex].Index = listIndex;
						if (list.FaceCount > 0)
						{
							list.FaceCount--;
						}
					}
				}
				// remove object
				if (k == ObjectCount - 1)
				{
					ObjectCount--;
				}
				else if (ObjectCount == 0)
				{
					return; //Outside the world?
				}
				else
				{
					Objects[k] = Objects[ObjectCount - 1];
					ObjectCount--;
					for (int i = 0; i < Objects[k].FaceListReferences.Length; i++)
					{
						ObjectListType listType = Objects[k].FaceListReferences[i].Type;
						ObjectList list;
						switch (listType)
						{
							case ObjectListType.StaticOpaque:
								{
									int groupIndex = (int)Objects[k].InternalObject.GroupIndex;
									list = StaticOpaque[groupIndex].List;
								}
								break;
							case ObjectListType.DynamicOpaque:
								list = DynamicOpaque;
								break;
							case ObjectListType.DynamicAlpha:
								list = DynamicAlpha;
								break;
							case ObjectListType.OverlayOpaque:
								list = OverlayOpaque;
								break;
							case ObjectListType.OverlayAlpha:
								list = OverlayAlpha;
								break;
							case ObjectListType.Touch:
								list = Touch;
								break;
							default:
								throw new InvalidOperationException();
						}
						int listIndex = Objects[k].FaceListReferences[i].Index;
						if (list.Faces[listIndex] == null)
						{
							continue;
						}
						list.Faces[listIndex].ObjectListIndex = k;
					}
					Objects[k].InternalObject.RendererIndex = k + 1;
				}
				Object.RendererIndex = 0;
			}
		}
	}
}
