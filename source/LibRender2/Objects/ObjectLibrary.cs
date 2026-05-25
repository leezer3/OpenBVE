using System;
using System.Collections.Concurrent;
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
	public class FaceComparer : IComparer<FaceState>
	{
		/// <summary>Compares two faces for sorting, primarily by distance from camera.</summary>
		public int Compare(FaceState a, FaceState b)
		{
			if (ReferenceEquals(a, b)) return 0;
			if (a == null) return -1;
			if (b == null) return 1;

			// Sort by distance from camera (Back-to-Front for alpha blending)
			int result = b.SortDistance.CompareTo(a.SortDistance);

			// Tie-breakers to ensure stable sort order and prevent flickering
			if (result == 0) result = a.InternalIndex.CompareTo(b.InternalIndex);

			return result;
		}
	}

	public class VisibleObjectLibrary
	{
		private readonly BaseRenderer renderer;

		public readonly QuadTree quadTree;

		public readonly ConcurrentDictionary<ObjectState, byte> Objects;
		private readonly List<FaceState> myOpaqueFaces;
		private readonly List<FaceState> myAlphaFaces;
		private readonly List<FaceState> myOverlayOpaqueFaces;
		private List<FaceState> myOverlayAlphaFaces;
		private readonly FaceComparer faceComparer = new FaceComparer();
		public readonly ReadOnlyCollection<FaceState> OpaqueFaces;  // StaticOpaque and DynamicOpaque
		public readonly ReadOnlyCollection<FaceState> OverlayOpaqueFaces;
		public readonly ReadOnlyCollection<FaceState> AlphaFaces;  // DynamicAlpha
		public ReadOnlyCollection<FaceState> OverlayAlphaFaces;

		public readonly object LockObject = new object();
		private bool needsSort;
		private int faceCount;

		internal VisibleObjectLibrary(BaseRenderer Renderer)
		{
			renderer = Renderer;
			// Note: .Net has no Concurrent HashSet, so use a dictionary with a byte value instead
			// previous approach used a List and Contains()
			Objects = new ConcurrentDictionary<ObjectState, byte>();
			myOpaqueFaces = new List<FaceState>();
			myAlphaFaces = new List<FaceState>();
			myOverlayOpaqueFaces = new List<FaceState>();
			myOverlayAlphaFaces = new List<FaceState>();

			OpaqueFaces = myOpaqueFaces.AsReadOnly();
			AlphaFaces = myAlphaFaces.AsReadOnly();
			OverlayOpaqueFaces = myOverlayOpaqueFaces.AsReadOnly();
			OverlayAlphaFaces = myOverlayAlphaFaces.AsReadOnly();
			quadTree = new QuadTree(renderer.currentOptions.ViewingDistance);
		}

		private bool AddObject(ObjectState state)
		{
			return state.Prototype != null && Objects.TryAdd(state, 0);
		}

		private void RemoveObject(ObjectState state)
		{
			lock (LockObject)
			{
				if (Objects.TryRemove(state, out _))
				{
					myOpaqueFaces.RemoveAll(x => x.Object == state);
					myAlphaFaces.RemoveAll(x => x.Object == state);
					myOverlayOpaqueFaces.RemoveAll(x => x.Object == state);
					myOverlayAlphaFaces.RemoveAll(x => x.Object == state);
					needsSort = true;
				}	
			}
			
		}

		public void Clear()
		{
			lock (LockObject)
			{
				Objects.Clear();
				myOpaqueFaces.Clear();
				myAlphaFaces.Clear();
				myOverlayOpaqueFaces.Clear();
				myOverlayAlphaFaces.Clear();
				renderer.Scene.StaticObjectStates = new List<ObjectState>();
				renderer.Scene.DynamicObjectStates = new List<ObjectState>();
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

				if (Type == ObjectType.Overlay && (renderer.Camera.CurrentRestriction == CameraRestrictionMode.On || renderer.Camera.CurrentRestriction == CameraRestrictionMode.Off))
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
						list.Add(new FaceState(State, face, renderer, faceCount++));
						needsSort = true;
					}
					else
					{
						/*
						 * Alpha faces should be inserted at the end of the list- We're going to sort it anyway so it makes no odds
						 */
						list.Add(new FaceState(State, face, renderer, faceCount++));
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
			lock (LockObject)
			{
				if (overlay)
				{
					SortPolygons(myOverlayAlphaFaces, true);
					return new List<FaceState>(myOverlayAlphaFaces);
				}

				if (needsSort)
				{
					/*
					 * Sort opaque faces by Prototype (VAO) to minimize binds
					 * When the new renderer is in use, this prevents re-binding the VBO as it is simply re-drawn with
					 * a different translation matrix
					 * NOTE: The shader isn't currently smart enough to do depth discards, so if this changes may need to
					 * be revisited (e.g. Front-to-Back sorting for Early-Z)
					 */
					myOpaqueFaces.Sort((x, y) =>
					{
						if (x.Object.Prototype == y.Object.Prototype) return 0;
						return x.Object.Prototype.GetHashCode().CompareTo(y.Object.Prototype.GetHashCode());
					});
					needsSort = false;
				}

				SortPolygons(myAlphaFaces);
				return new List<FaceState>(myAlphaFaces);
			}
		}

		private void SortPolygons(List<FaceState> faces, bool overlay = false)
		{
			// calculate distance from camera forward vector
			Vector3 forward = overlay
				? new Vector3(renderer.Camera.AbsoluteDirection.X, renderer.Camera.AbsoluteDirection.Y, -renderer.Camera.AbsoluteDirection.Z)
				: renderer.Camera.AbsoluteDirection;

			Vector3 cameraPos = overlay ? Vector3.Zero : renderer.Camera.AbsolutePosition;

			foreach (var face in faces)
			{
				face.SortDistance = Vector3.Dot(face.Object.WorldPosition - cameraPos, forward);
			}
			// sort
			faces.Sort(faceComparer);
		}

	}
}
