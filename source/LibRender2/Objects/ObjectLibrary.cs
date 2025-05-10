using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using LibRender2.Textures;
using OpenBveApi.Graphics;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Textures;

namespace LibRender2.Objects
{
	public class VisibleObjectLibrary
	{
		private readonly BaseRenderer renderer;

		public readonly QuadTree quadTree;

		private readonly List<ObjectState> myObjects;
		private readonly List<FaceState> myOpaqueFaces;
		private readonly List<FaceState> myAlphaFaces;
		private readonly List<FaceState> myOverlayOpaqueFaces;
		private List<FaceState> myOverlayAlphaFaces;
		public readonly ReadOnlyCollection<ObjectState> Objects;
		public readonly ReadOnlyCollection<FaceState> OpaqueFaces;  // StaticOpaque and DynamicOpaque
		public readonly ReadOnlyCollection<FaceState> OverlayOpaqueFaces;
		public readonly ReadOnlyCollection<FaceState> AlphaFaces;  // DynamicAlpha
		public ReadOnlyCollection<FaceState> OverlayAlphaFaces;

		public readonly object LockObject = new object();

		internal VisibleObjectLibrary(BaseRenderer Renderer)
		{
			renderer = Renderer;
			myObjects = new List<ObjectState>();
			myOpaqueFaces = new List<FaceState>();
			myAlphaFaces = new List<FaceState>();
			myOverlayOpaqueFaces = new List<FaceState>();
			myOverlayAlphaFaces = new List<FaceState>();

			Objects = myObjects.AsReadOnly();
			OpaqueFaces = myOpaqueFaces.AsReadOnly();
			AlphaFaces = myAlphaFaces.AsReadOnly();
			OverlayOpaqueFaces = myOverlayOpaqueFaces.AsReadOnly();
			OverlayAlphaFaces = myOverlayAlphaFaces.AsReadOnly();
			quadTree = new QuadTree(renderer.currentOptions.ViewingDistance);
		}

		private bool AddObject(ObjectState state)
		{
			if (state.Prototype != null &&!myObjects.Contains(state))
			{
				myObjects.Add(state);
				return true;
			}

			return false;
		}

		private void RemoveObject(ObjectState state)
		{
			lock (LockObject)
			{
				if (myObjects.Contains(state))
				{
					myObjects.Remove(state);
					myOpaqueFaces.RemoveAll(x => x.Object == state);
					myAlphaFaces.RemoveAll(x => x.Object == state);
					myOverlayOpaqueFaces.RemoveAll(x => x.Object == state);
					myOverlayAlphaFaces.RemoveAll(x => x.Object == state);
				}	
			}
			
		}

		public void Clear()
		{
			lock (LockObject)
			{
				myObjects.Clear();
				myOpaqueFaces.Clear();
				myAlphaFaces.Clear();
				myOverlayOpaqueFaces.Clear();
				myOverlayAlphaFaces.Clear();
				renderer.StaticObjectStates.Clear();
				renderer.DynamicObjectStates.Clear();
			}
		}

		public void ShowObject(ObjectState State, ObjectType Type)
		{
			bool result = AddObject(State);
			
			if (!result)
			{
				return;
			}

			foreach (MeshFace face in State.Prototype.Mesh.Faces)
			{
				OpenGlTextureWrapMode wrap = OpenGlTextureWrapMode.ClampClamp;

				if (State.Prototype.Mesh.Materials[face.Material].DaytimeTexture != null || State.Prototype.Mesh.Materials[face.Material].NighttimeTexture != null)
				{
					if (State.Prototype.Mesh.Materials[face.Material].WrapMode == null)
					{
						/*
						 * If the object does not have a stored wrapping mode determine it now. However:
						 * https://github.com/leezer3/OpenBVE/issues/971
						 *
						 * Unfortunately, there appear to be X objects in the wild which expect a non-default wrapping mode
						 * which means the best fast exit we can do is to check for RepeatRepeat....
						 *
						 */ 
						foreach (VertexTemplate vertex in State.Prototype.Mesh.Vertices)
						{
							if (vertex.TextureCoordinates.X < 0.0f || vertex.TextureCoordinates.X > 1.0f)
							{
								wrap |= OpenGlTextureWrapMode.RepeatClamp;
							}

							if (vertex.TextureCoordinates.Y < 0.0f || vertex.TextureCoordinates.Y > 1.0f)
							{
								wrap |= OpenGlTextureWrapMode.ClampRepeat;
							}

							if (wrap == OpenGlTextureWrapMode.RepeatRepeat)
							{
								break;
							}
						}
						State.Prototype.Mesh.Materials[face.Material].WrapMode = wrap;
					}
				}

				bool alpha = false;

				if (Type == ObjectType.Overlay && renderer.Camera.CurrentRestriction != CameraRestrictionMode.NotAvailable)
				{
					alpha = true;
				}
				else if (State.Prototype.Mesh.Materials[face.Material].Color.A != 255)
				{
					alpha = true;
				}
				else if (State.Prototype.Mesh.Materials[face.Material].BlendMode == MeshMaterialBlendMode.Additive)
				{
					alpha = true;
				}
				else if (State.Prototype.Mesh.Materials[face.Material].GlowAttenuationData != 0)
				{
					alpha = true;
				}
				else
				{
					if (State.Prototype.Mesh.Materials[face.Material].DaytimeTexture != null)
					{
						// Have to load the texture bytes in order to determine transparency type
						Texture daytimeTexture; 
						if (TextureManager.textureCache.ContainsKey(State.Prototype.Mesh.Materials[face.Material].DaytimeTexture.Origin))
						{
							daytimeTexture = TextureManager.textureCache[State.Prototype.Mesh.Materials[face.Material].DaytimeTexture.Origin];
						}
						else
						{
							State.Prototype.Mesh.Materials[face.Material].DaytimeTexture.Origin.GetTexture(out daytimeTexture);
							if (!TextureManager.textureCache.ContainsKey(State.Prototype.Mesh.Materials[face.Material].DaytimeTexture.Origin)) // because getting the Origin may change the ref
							{
								TextureManager.textureCache.Add(State.Prototype.Mesh.Materials[face.Material].DaytimeTexture.Origin, daytimeTexture);
							}
							
						}

						TextureTransparencyType transparencyType = TextureTransparencyType.Opaque;
						if (daytimeTexture != null)
						{
							// as loading the cached texture may have failed, e.g. corrupt file etc
							transparencyType = daytimeTexture.GetTransparencyType();
						}
						
						if (transparencyType == TextureTransparencyType.Alpha)
						{
							alpha = true;
						}
						else if (transparencyType == TextureTransparencyType.Partial && renderer.currentOptions.TransparencyMode == TransparencyMode.Quality)
						{
							alpha = true;
						}
					}

					if (State.Prototype.Mesh.Materials[face.Material].NighttimeTexture != null)
					{
						Texture nighttimeTexture; 
						if (TextureManager.textureCache.ContainsKey(State.Prototype.Mesh.Materials[face.Material].NighttimeTexture.Origin))
						{
							nighttimeTexture = TextureManager.textureCache[State.Prototype.Mesh.Materials[face.Material].NighttimeTexture.Origin];
						}
						else
						{
							State.Prototype.Mesh.Materials[face.Material].NighttimeTexture.Origin.GetTexture(out nighttimeTexture);
							TextureManager.textureCache.Add(State.Prototype.Mesh.Materials[face.Material].NighttimeTexture.Origin, nighttimeTexture);
						}
						TextureTransparencyType transparencyType = TextureTransparencyType.Opaque;
						if (nighttimeTexture != null)
						{
							// as loading the cached texture may have failed, e.g. corrupt file etc
							transparencyType = nighttimeTexture.GetTransparencyType();
						}
						if (transparencyType == TextureTransparencyType.Alpha)
						{
							alpha = true;
						}
						else if (transparencyType == TextureTransparencyType.Partial && renderer.currentOptions.TransparencyMode == TransparencyMode.Quality)
						{
							alpha = true;
						}
					}
				}
				
				List<FaceState> list;

				switch (Type)
				{
					case ObjectType.Static:
					case ObjectType.Dynamic:
						list = alpha ? myAlphaFaces : myOpaqueFaces;
						break;
					case ObjectType.Overlay:
						list = alpha ? myOverlayAlphaFaces : myOverlayOpaqueFaces;
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(Type), Type, null);
				}

				lock (LockObject)
				{
					if (!alpha)
					{
						/*
						 * If an opaque face, itinerate through the list to see if the prototype is present in the list
						 * When the new renderer is in use, this prevents re-binding the VBO as it is simply re-drawn with
						 * a different translation matrix
						 * NOTE: The shader isn't currently smart enough to do depth discards, so if this changes may need to
						 * be revisited
						 */
						if (list.Count == 0)
						{
							list.Add(new FaceState(State, face, renderer));
						}
						else
						{
							for (int i = 0; i < list.Count; i++)
							{

								if (list[i].Object.Prototype == State.Prototype)
								{
									list.Insert(i, new FaceState(State, face, renderer));
									break;
								}

								if (i == list.Count - 1)
								{
									list.Add(new FaceState(State, face, renderer));
									break;
								}
							}
						}
					}
					else
					{
						/*
						 * Alpha faces should be inserted at the end of the list- We're going to sort it anyway so it makes no odds
						 */
						list.Add(new FaceState(State, face, renderer));
					}
				}
			}
		}

		public void HideObject(ObjectState State)
		{
			RemoveObject(State);
		}

		public List<FaceState> GetSortedPolygons(bool overlay = false)
		{
			if (overlay)
			{
				myOverlayAlphaFaces = GetSortedPolygons(myOverlayAlphaFaces.AsReadOnly());
				OverlayAlphaFaces = myOverlayAlphaFaces.AsReadOnly();
				return OverlayAlphaFaces.ToList();
			}
			return GetSortedPolygons(AlphaFaces);
		}

		private List<FaceState> GetSortedPolygons(ReadOnlyCollection<FaceState> faces)
		{
			// calculate distance
			double[] distances = new double[faces.Count];

			Parallel.For(0, faces.Count, i =>
			{
				if (faces[i].Face.Vertices.Length >= 3)
				{
					Vector4 v0 = new Vector4(faces[i].Object.Prototype.Mesh.Vertices[faces[i].Face.Vertices[0].Index].Coordinates, 1.0);
					Vector4 v1 = new Vector4(faces[i].Object.Prototype.Mesh.Vertices[faces[i].Face.Vertices[1].Index].Coordinates, 1.0);
					Vector4 v2 = new Vector4(faces[i].Object.Prototype.Mesh.Vertices[faces[i].Face.Vertices[2].Index].Coordinates, 1.0);
					Vector4 w1 = v1 - v0;
					Vector4 w2 = v2 - v0;
					v0.Z *= -1.0;
					w1.Z *= -1.0;
					w2.Z *= -1.0;
					v0 = Vector4.Transform(v0, faces[i].Object.ModelMatrix);
					w1 = Vector4.Transform(w1, faces[i].Object.ModelMatrix);
					w2 = Vector4.Transform(w2, faces[i].Object.ModelMatrix);
					v0.Z *= -1.0;
					w1.Z *= -1.0;
					w2.Z *= -1.0;
					Vector3 d = Vector3.Cross(w1.Xyz, w2.Xyz);
					double t = d.Norm();

					if (t != 0.0)
					{
						d /= t;
						Vector3 w0 = v0.Xyz - renderer.Camera.AbsolutePosition;
						t = Vector3.Dot(d, w0);
						distances[i] = -t * t;
					}
				}
			});
			// sort
			return faces.Select((face, index) => new { Face = face, Distance = distances[index] }).OrderBy(list => list.Distance).Select(list => list.Face).ToList();
		}
	}
}
