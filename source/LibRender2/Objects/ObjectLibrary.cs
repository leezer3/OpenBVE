using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

		public readonly ConcurrentDictionary<ObjectState, byte> Objects;
		private readonly List<FaceState> myOpaqueFaces;
		private readonly List<FaceState> myAlphaFaces;
		private readonly List<FaceState> myOverlayOpaqueFaces;
		private List<FaceState> myOverlayAlphaFaces;
		public readonly ReadOnlyCollection<FaceState> OpaqueFaces;  // StaticOpaque and DynamicOpaque
		public readonly ReadOnlyCollection<FaceState> OverlayOpaqueFaces;
		public readonly ReadOnlyCollection<FaceState> AlphaFaces;  // DynamicAlpha
		public ReadOnlyCollection<FaceState> OverlayAlphaFaces;

		public readonly object LockObject = new object();

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
			return state != null && state.Prototype != null && Objects.TryAdd(state, 0);
		}

		private void RemoveObject(ObjectState state)
		{
			if (state == null)
			{
				return;
			}
			lock (LockObject)
			{
				if (Objects.TryRemove(state, out _))
				{
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
				Objects.Clear();
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

			// Per-material cache: avoid repeated textureCache lookups for faces sharing the same material
			Dictionary<int, AlphaFaceClassification> materialCache = new Dictionary<int, AlphaFaceClassification>();

			foreach (MeshFace face in State.Prototype.Mesh.Faces)
			{
				if (face.Vertices.Length == 0)
				{
					// defensive, may occur if ObjectOptimisation is disabled or object is too big to optimise
					continue;
				}

				if (!materialCache.TryGetValue(face.Material, out AlphaFaceClassification classification))
				{
					classification = ClassifyFace(State, face, Type);
					materialCache[face.Material] = classification;
				}

				bool alpha = classification.Alpha;
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
							list.Add(new FaceState(State, face, renderer, classification.AdditiveBlend, classification.BinaryAlpha));
						}
						else
						{
							for (int i = 0; i < list.Count; i++)
							{

								if (list[i].Object.Prototype == State.Prototype)
								{
									list.Insert(i, new FaceState(State, face, renderer, classification.AdditiveBlend, classification.BinaryAlpha));
									break;
								}

								if (i == list.Count - 1)
								{
									list.Add(new FaceState(State, face, renderer, classification.AdditiveBlend, classification.BinaryAlpha));
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
						list.Add(new FaceState(State, face, renderer, classification.AdditiveBlend, classification.BinaryAlpha));
					}
				}
			}
		}

		/// <summary>Describes how a single face of an object is drawn</summary>
		private struct AlphaFaceClassification
		{
			/// <summary>Whether the face is drawn in the blended alpha pass</summary>
			public bool Alpha;
			/// <summary>Whether the face is additively blended, and so independent of the draw order in every pass</summary>
			public bool AdditiveBlend;
			/// <summary>Whether the face's alpha is binary and it has no distinct nighttime texture</summary>
			public bool BinaryAlpha;
		}

		/// <summary>Determines how a single face of an object is drawn</summary>
		private AlphaFaceClassification ClassifyFace(ObjectState State, MeshFace face, ObjectType Type)
		{
			AlphaFaceClassification classification = new AlphaFaceClassification();
			MeshMaterial material = State.Prototype.Mesh.Materials[face.Material];

			if (Type == ObjectType.Overlay && renderer.Camera.CurrentRestriction != CameraRestrictionMode.NotAvailable)
			{
				classification.Alpha = true;
			}
			else if (material.Color.A != 255)
			{
				classification.Alpha = true;
			}
			else if (material.BlendMode == MeshMaterialBlendMode.Additive)
			{
				classification.Alpha = true;
				// Additive blending is commutative, so the draw order never matters
				classification.AdditiveBlend = true;
			}
			else if (material.GlowAttenuationData != 0)
			{
				classification.Alpha = true;
			}
			else
			{
				TextureTransparencyType transparencyType = GetTextureTransparencyType(material.DaytimeTexture);

				if (transparencyType == TextureTransparencyType.Alpha)
				{
					classification.Alpha = true;
					// A binary alpha contains only fully transparent or fully opaque pixels. A distinct nighttime
					// texture is drawn as a second blended polygon which may contribute partially transparent
					// pixels however, so such a face does not qualify.
					classification.BinaryAlpha = material.NighttimeTexture == null || material.NighttimeTexture == material.DaytimeTexture;
				}
				else if (transparencyType == TextureTransparencyType.Partial && renderer.currentOptions.TransparencyMode == TransparencyMode.Quality)
				{
					classification.Alpha = true;
				}
			}

			if (!classification.Alpha)
			{
				TextureTransparencyType transparencyType = GetTextureTransparencyType(material.NighttimeTexture);

				if (transparencyType == TextureTransparencyType.Alpha)
				{
					classification.Alpha = true;
				}
				else if (transparencyType == TextureTransparencyType.Partial && renderer.currentOptions.TransparencyMode == TransparencyMode.Quality)
				{
					classification.Alpha = true;
				}
			}

			return classification;
		}

		/// <summary>Gets the transparency type of a material texture, defaulting to opaque if it cannot be decoded</summary>
		private TextureTransparencyType GetTextureTransparencyType(Texture handle)
		{
			Texture texture = GetTransparencySourceTexture(handle);

			// as loading the cached texture may have failed, e.g. corrupt file etc
			return texture == null ? TextureTransparencyType.Opaque : texture.GetTransparencyType();
		}

		public void HideObject(ObjectState State)
		{
			RemoveObject(State);
		}

		/// <summary>Gets a decoded texture for transparency classification without forcing a full re-decode.</summary>
		/// <param name="handle">The material texture handle.</param>
		/// <returns>A decoded texture, or null if unavailable.</returns>
		/// <remarks>Unload drops textureCache entries, but the register-time decode survives on the
		/// handle (DecodedTexture) and is reused while the source file is unchanged.</remarks>
		private Texture GetTransparencySourceTexture(Texture handle)
		{
			if (handle == null || handle.Origin == null) return null;
			if (TextureManager.TryGetCachedTexture(handle.Origin, out Texture cached) && cached != null) return cached;
			Texture decoded = handle.DecodedTexture;
			if (decoded != null && TextureManager.TextureFileUnchanged(handle.Origin))
			{
				TextureManager.StoreCachedTexture(handle.Origin, decoded);
				return decoded;
			}
			Stopwatch sw = Stopwatch.StartNew();
			bool ok = handle.Origin.GetTexture(out Texture fresh);
			sw.Stop();
			TextureManager.TextureDecodeTime += sw.ElapsedMilliseconds;
			if (!ok || fresh == null)
			{
				// Do not cache failures: a null value here caused NRE in TextureManager.LoadTextureInternal
				return null;
			}
			TextureManager.StoreCachedTexture(handle.Origin, fresh);
			return fresh;
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
					Vector4 v0 = new Vector4(faces[i].Object.Prototype.Mesh.Vertices[faces[i].Face.Vertices[0]].Coordinates, 1.0);
					Vector4 v1 = new Vector4(faces[i].Object.Prototype.Mesh.Vertices[faces[i].Face.Vertices[1]].Coordinates, 1.0);
					Vector4 v2 = new Vector4(faces[i].Object.Prototype.Mesh.Vertices[faces[i].Face.Vertices[2]].Coordinates, 1.0);
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
