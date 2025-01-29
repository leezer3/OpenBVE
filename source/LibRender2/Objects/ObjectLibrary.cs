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
		public readonly ReadOnlyCollection<ObjectState> Objects;
		public Dictionary<Guid, FaceState> Faces;

		public List<Guid> AlphaFaces;
		public List<Guid> OpaqueFaces;
		public List<Guid> OverlayOpaqueFaces;
		public List<Guid> OverlayAlphaFaces;
		public List<Guid> FacesQueuedForRemoval;

		public readonly object LockObject = new object();

		internal VisibleObjectLibrary(BaseRenderer Renderer)
		{
			renderer = Renderer;
			myObjects = new List<ObjectState>();
			Faces = new Dictionary<Guid, FaceState>();
			AlphaFaces = new List<Guid>();
			OpaqueFaces = new List<Guid>();
			OverlayAlphaFaces = new List<Guid>();
			OverlayOpaqueFaces = new List<Guid>();
			FacesQueuedForRemoval = new List<Guid>();
			Objects = myObjects.AsReadOnly();

			quadTree = new QuadTree(renderer.currentOptions.ViewingDistance);
		}

		private bool AddObject(ObjectState state)
		{
			if (!myObjects.Contains(state))
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
					OpaqueFaces.RemoveAll(x => Faces[x].Object == state);
					AlphaFaces.RemoveAll(x => Faces[x].Object == state);
					OverlayOpaqueFaces.RemoveAll(x => Faces[x].Object == state);
					OverlayAlphaFaces.RemoveAll(x => Faces[x].Object == state);
				}	
			}
			
		}

		public void Clear()
		{
			lock (LockObject)
			{
				myObjects.Clear();
				OpaqueFaces.Clear();
				AlphaFaces.Clear();
				OverlayAlphaFaces.Clear();
				OverlayOpaqueFaces.Clear();
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
				
				List<Guid> list;

				switch (Type)
				{
					case ObjectType.Static:
					case ObjectType.Dynamic:
						list = alpha ? AlphaFaces : OpaqueFaces;
						break;
					case ObjectType.Overlay:
						list = alpha ? OverlayAlphaFaces : OverlayOpaqueFaces;
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(Type), Type, null);
				}

				FaceState faceState = new FaceState(State, face, renderer);
				Faces.Add(faceState.Guid, faceState);
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
							list.Add(faceState.Guid);
						}
						else
						{
							for (int i = 0; i < list.Count; i++)
							{

								if (Faces[list[i]].Object.Prototype == State.Prototype)
								{
									list.Insert(i, faceState.Guid);
									break;
								}

								if (i == list.Count - 1)
								{
									list.Add(faceState.Guid);
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
						list.Add(faceState.Guid);
					}
				}
			}
		}

		public void HideObject(ObjectState State)
		{
			RemoveObject(State);
		}

		public List<Guid> GetSortedPolygons(bool overlay = false)
		{
			if (overlay)
			{
				return GetSortedPolygons(OverlayAlphaFaces);
			}
			return GetSortedPolygons(AlphaFaces);
		}

		private List<Guid> GetSortedPolygons(List<Guid> faces)
		{
			// calculate distance
			double[] distances = new double[faces.Count];

			Parallel.For(0, faces.Count, i =>
			{
				if (Faces.ContainsKey(faces[i]) && Faces[faces[i]].Face.Vertices.Length >= 3)
				{
					Vector4 v0 = new Vector4(Faces[faces[i]].Object.Prototype.Mesh.Vertices[Faces[faces[i]].Face.Vertices[0].Index].Coordinates, 1.0);
					Vector4 v1 = new Vector4(Faces[faces[i]].Object.Prototype.Mesh.Vertices[Faces[faces[i]].Face.Vertices[1].Index].Coordinates, 1.0);
					Vector4 v2 = new Vector4(Faces[faces[i]].Object.Prototype.Mesh.Vertices[Faces[faces[i]].Face.Vertices[2].Index].Coordinates, 1.0);
					Vector4 w1 = v1 - v0;
					Vector4 w2 = v2 - v0;
					v0.Z *= -1.0;
					w1.Z *= -1.0;
					w2.Z *= -1.0;
					v0 = Vector4.Transform(v0, Faces[faces[i]].Object.ModelMatrix);
					w1 = Vector4.Transform(w1, Faces[faces[i]].Object.ModelMatrix);
					w2 = Vector4.Transform(w2, Faces[faces[i]].Object.ModelMatrix);
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
