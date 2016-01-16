﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenBve
{
    internal static partial class Renderer
    {

        internal static IList<World.MeshMaterial> UnloadTextures = new List<World.MeshMaterial>();

        /// <summary>Re-adds all objects within the world, for example after a screen resolution change</summary>
        internal static void ReAddObjects()
        {
            Object[] list = new Object[ObjectCount];
            for (int i = 0; i < ObjectCount; i++)
            {
                list[i] = Objects[i];
            }
            for (int i = 0; i < list.Length; i++)
            {
                HideObject(list[i].ObjectIndex);
            }
            for (int i = 0; i < list.Length; i++)
            {
                ShowObject(list[i].ObjectIndex, list[i].Type);
            }
        }

        /// <summary>Makes an object visible within the world</summary>
        /// <param name="ObjectIndex">The object's index</param>
        /// <param name="Type">Whether this is a static or dynamic object</param>
        internal static void ShowObject(int ObjectIndex, ObjectType Type)
        {
            if (ObjectManager.Objects[ObjectIndex] == null)
            {
                return;
            }
            if (ObjectManager.Objects[ObjectIndex].RendererIndex == 0)
            {
                if (ObjectCount >= Objects.Length)
                {
                    Array.Resize<Object>(ref Objects, Objects.Length << 1);
                }
                Objects[ObjectCount].ObjectIndex = ObjectIndex;
                Objects[ObjectCount].Type = Type;
                int f = ObjectManager.Objects[ObjectIndex].Mesh.Faces.Length;
                Objects[ObjectCount].FaceListReferences = new ObjectListReference[f];
                for (int i = 0; i < f; i++)
                {
                    bool alpha = false;
                    int k = ObjectManager.Objects[ObjectIndex].Mesh.Faces[i].Material;
                    Textures.OpenGlTextureWrapMode wrap = Textures.OpenGlTextureWrapMode.ClampClamp;
                    if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].DaytimeTexture != null | ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].NighttimeTexture != null)
                    {

                        // HACK: Objects do not store information on the texture wrapping mode.
                        //       Let's determine the best wrapping mode now and then save it
                        //       so we can quickly access it in the rendering loop.

                        if (ObjectManager.Objects[ObjectIndex].Dynamic)
                        {
                            wrap = Textures.OpenGlTextureWrapMode.RepeatRepeat;
                        }
                        else
                        {
                            for (int v = 0; v < ObjectManager.Objects[ObjectIndex].Mesh.Vertices.Length; v++)
                            {
                                if (ObjectManager.Objects[ObjectIndex].Mesh.Vertices[v].TextureCoordinates.X < 0.0f | ObjectManager.Objects[ObjectIndex].Mesh.Vertices[v].TextureCoordinates.X > 1.0f)
                                {
                                    wrap |= Textures.OpenGlTextureWrapMode.RepeatClamp;
                                }
                                if (ObjectManager.Objects[ObjectIndex].Mesh.Vertices[v].TextureCoordinates.Y < 0.0f | ObjectManager.Objects[ObjectIndex].Mesh.Vertices[v].TextureCoordinates.Y > 1.0f)
                                {
                                    wrap |= Textures.OpenGlTextureWrapMode.ClampRepeat;
                                }
                            }
                        }

                        if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].DaytimeTexture != null)
                        {
                            if (Textures.LoadTexture(ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].DaytimeTexture, wrap))
                            {
                                OpenBveApi.Textures.TextureTransparencyType type = ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].DaytimeTexture.Transparency;
                                if (type == OpenBveApi.Textures.TextureTransparencyType.Alpha)
                                {
                                    alpha = true;
                                }
                                else if (type == OpenBveApi.Textures.TextureTransparencyType.Partial && Interface.CurrentOptions.TransparencyMode == TransparencyMode.Quality)
                                {
                                    alpha = true;
                                }
                            }
                        }
                        if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].NighttimeTexture != null)
                        {
                            if (Textures.LoadTexture(ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].NighttimeTexture, wrap))
                            {
                                OpenBveApi.Textures.TextureTransparencyType type = ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].NighttimeTexture.Transparency;
                                if (type == OpenBveApi.Textures.TextureTransparencyType.Alpha)
                                {
                                    alpha = true;
                                }
                                else if (type == OpenBveApi.Textures.TextureTransparencyType.Partial & Interface.CurrentOptions.TransparencyMode == TransparencyMode.Quality)
                                {
                                    alpha = true;
                                }
                            }
                        }
                    }
                    if (Type == ObjectType.Overlay & World.CameraRestriction != World.CameraRestrictionMode.NotAvailable)
                    {
                        alpha = true;
                    }
                    else if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].Color.A != 255)
                    {
                        alpha = true;
                    }
                    else if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].BlendMode == World.MeshMaterialBlendMode.Additive)
                    {
                        alpha = true;
                    }
                    else if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].GlowAttenuationData != 0)
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
                        int groupIndex = (int)ObjectManager.Objects[ObjectIndex].GroupIndex;
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
                            ObjectIndex = ObjectIndex,
                            FaceIndex = i,
                            Wrap = wrap
                        };

                        // HACK: Let's store the wrapping mode.

                        StaticOpaque[groupIndex].Update = true;
                        Objects[ObjectCount].FaceListReferences[i] = new ObjectListReference(listType, newIndex);
                        Game.InfoStaticOpaqueFaceCount++;

                        /*
                         * Check if the given object has a bounding box, and insert it to the end of the list of bounding boxes if required
                         */
                        if (ObjectManager.Objects[ObjectIndex].Mesh.BoundingBox != null)
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
                            list.BoundingBoxes[Index].Lower = ObjectManager.Objects[ObjectIndex].Mesh.BoundingBox[0];
                            list.BoundingBoxes[Index].Lower = ObjectManager.Objects[ObjectIndex].Mesh.BoundingBox[1];
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
                            ObjectIndex = ObjectIndex,
                            FaceIndex = i,
                            Wrap = wrap
                        };

                        // HACK: Let's store the wrapping mode.

                        Objects[ObjectCount].FaceListReferences[i] = new ObjectListReference(listType, list.FaceCount);
                        list.FaceCount++;
                    }

                }
                ObjectManager.Objects[ObjectIndex].RendererIndex = ObjectCount + 1;
                ObjectCount++;
            }
        }

        /// <summary>Hides an object within the world</summary>
        /// <param name="ObjectIndex">The object's index</param>
        internal static void HideObject(int ObjectIndex)
        {
            if (ObjectManager.Objects[ObjectIndex] == null)
            {
                return;
            }
            int k = ObjectManager.Objects[ObjectIndex].RendererIndex - 1;
            if (k >= 0)
            {
                if (Interface.CurrentOptions.UnloadUnusedTextures)
                {
                    for (int l = 0; l < ObjectManager.Objects[ObjectIndex].Mesh.Materials.Length; l++)
                    {
                        //Add all textures for the current object to the list
                        if (UnloadTextures.Count == 0)
                        {
                            UnloadTextures.Add(ObjectManager.Objects[ObjectIndex].Mesh.Materials[l]);
                        }
                        else
                        {
                            
                            for (int m = 0; m < UnloadTextures.Count; m++)
                            {
                                if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[l] == UnloadTextures[m])
                                {
                                    break;
                                }
                                if (m == UnloadTextures.Count - 1)
                                {
                                    UnloadTextures.Add(ObjectManager.Objects[ObjectIndex].Mesh.Materials[l]);
                                }
                            }
                            
                        }
                        
                    }
                }
                
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
                        int groupIndex = (int)ObjectManager.Objects[Objects[k].ObjectIndex].GroupIndex;
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
                        Game.InfoStaticOpaqueFaceCount--;
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
                        list.Faces[listIndex] = list.Faces[list.FaceCount - 1];
                        Objects[list.Faces[listIndex].ObjectListIndex].FaceListReferences[list.Faces[listIndex].FaceIndex].Index = listIndex;
                        list.FaceCount--;
                    }
                }
                // remove object
                if (k == ObjectCount - 1)
                {
                    ObjectCount--;
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
                                    int groupIndex = (int)ObjectManager.Objects[Objects[k].ObjectIndex].GroupIndex;
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
                            default:
                                throw new InvalidOperationException();
                        }
                        int listIndex = Objects[k].FaceListReferences[i].Index;
                        list.Faces[listIndex].ObjectListIndex = k;
                    }
                    ObjectManager.Objects[Objects[k].ObjectIndex].RendererIndex = k + 1;
                }
                ObjectManager.Objects[ObjectIndex].RendererIndex = 0;
            }
        }

        internal static void UnloadUnusedTextures()
        {
            for (int s = 0; s < ObjectManager.Objects.Length; s++)
            {
                if (UnloadTextures.Count == 0)
                {
                    //Break out of loop if no textures are left in the list
                    break;
                }
                if (ObjectManager.Objects[s].RendererIndex == 0)
                {
                    //Don't process not loaded objects
                    continue;
                }
                for (int r = 0; r < ObjectManager.Objects[s].Mesh.Materials.Length; r++)
                {
                    for (int q = 0; q < UnloadTextures.Count; q++)
                    {
                        try
                        {
                            if (UnloadTextures[q] == ObjectManager.Objects[s].Mesh.Materials[r])
                            {
                                UnloadTextures.RemoveAt(q);
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }

            if (UnloadTextures.Count != 0)
            {
                foreach (var Texture in UnloadTextures)
                {
                    Textures.UnloadTexture(Texture.DaytimeTexture);
                    Textures.UnloadTexture(Texture.NighttimeTexture);
                }

            }
        }
    }
}
