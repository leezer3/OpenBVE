using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using LibRender2.Cameras;
using OpenBveApi.Graphics;
using OpenBveApi.Hosts;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Textures;

namespace LibRender2.Objects
{
	public class VisibleObjectLibrary
	{
		private readonly HostInterface currentHost;
		private readonly CameraProperties camera;
		private readonly BaseRenderer renderer;

		private readonly List<ObjectState> myObjects;
		private readonly List<FaceState> myOpaqueFaces;
		private List<FaceState> myPartialFaces;
		private List<FaceState> myAlphaFaces;
		private readonly List<FaceState> myOverlayOpaqueFaces;
		private List<FaceState> myOverlayPartialFaces;
		private List<FaceState> myOverlayAlphaFaces;
		public readonly ReadOnlyCollection<ObjectState> Objects;
		public readonly ReadOnlyCollection<FaceState> OpaqueFaces;  // StaticOpaque and DynamicOpaque
		public ReadOnlyCollection<FaceState> PartialFaces;
		public ReadOnlyCollection<FaceState> AlphaFaces;            // DynamicAlpha
		public readonly ReadOnlyCollection<FaceState> OverlayOpaqueFaces;
		public ReadOnlyCollection<FaceState> OverlayPartialFaces;
		public ReadOnlyCollection<FaceState> OverlayAlphaFaces;

		internal VisibleObjectLibrary(HostInterface CurrentHost, CameraProperties Camera, BaseRenderer Renderer)
		{
			currentHost = CurrentHost;
			camera = Camera;
			renderer = Renderer;

			myObjects = new List<ObjectState>();
			myOpaqueFaces = new List<FaceState>();
			myPartialFaces = new List<FaceState>();
			myAlphaFaces = new List<FaceState>();
			myOverlayOpaqueFaces = new List<FaceState>();
			myOverlayPartialFaces = new List<FaceState>();
			myOverlayAlphaFaces = new List<FaceState>();

			Objects = myObjects.AsReadOnly();
			OpaqueFaces = myOpaqueFaces.AsReadOnly();
			PartialFaces = myPartialFaces.AsReadOnly();
			AlphaFaces = myAlphaFaces.AsReadOnly();
			OverlayOpaqueFaces = myOverlayOpaqueFaces.AsReadOnly();
			OverlayPartialFaces = myOverlayPartialFaces.AsReadOnly();
			OverlayAlphaFaces = myOverlayAlphaFaces.AsReadOnly();
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
			if (myObjects.Contains(state))
			{
				myObjects.Remove(state);
				myOpaqueFaces.RemoveAll(x => x.Object == state);
				myPartialFaces.RemoveAll(x => x.Object == state);
				myAlphaFaces.RemoveAll(x => x.Object == state);
				myOverlayOpaqueFaces.RemoveAll(x => x.Object == state);
				myOverlayPartialFaces.RemoveAll(x => x.Object == state);
				myOverlayAlphaFaces.RemoveAll(x => x.Object == state);
			}
		}

		public void Clear()
		{
			myObjects.Clear();
			myOpaqueFaces.Clear();
			myPartialFaces.Clear();
			myAlphaFaces.Clear();
			myOverlayOpaqueFaces.Clear();
			myOverlayPartialFaces.Clear();
			myOverlayAlphaFaces.Clear();
		}

		public void ShowObject(ObjectState State, ObjectType Type)
		{
			bool result = AddObject(State);

			if (!renderer.ForceLegacyOpenGL && State.Prototype.Mesh.VAO == null)
			{
				VAOExtensions.CreateVAO(ref State.Prototype.Mesh, State.Prototype.Dynamic, renderer.DefaultShader.VertexLayout, renderer);
			}

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
						// If the object does not have a stored wrapping mode, determine it now
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
						}

						State.Prototype.Mesh.Materials[face.Material].WrapMode = wrap;
					}
				}

				FaceTransparencyType faceType = FaceTransparencyType.Opaque;

				if (Type == ObjectType.Overlay && camera.CurrentRestriction != CameraRestrictionMode.NotAvailable)
				{
					faceType = FaceTransparencyType.Alpha;
				}
				else if (State.Prototype.Mesh.Materials[face.Material].Color.A != 255)
				{
					faceType = FaceTransparencyType.Alpha;
				}
				else if (State.Prototype.Mesh.Materials[face.Material].BlendMode == MeshMaterialBlendMode.Additive)
				{
					faceType = FaceTransparencyType.Alpha;
				}
				else if (State.Prototype.Mesh.Materials[face.Material].GlowAttenuationData != 0)
				{
					faceType = FaceTransparencyType.Alpha;
				}
				else
				{
					if (State.Prototype.Mesh.Materials[face.Material].DaytimeTexture != null)
					{
						currentHost.LoadTexture(State.Prototype.Mesh.Materials[face.Material].DaytimeTexture, (OpenGlTextureWrapMode)State.Prototype.Mesh.Materials[face.Material].WrapMode);

						switch (State.Prototype.Mesh.Materials[face.Material].DaytimeTexture.Transparency)
						{
							case TextureTransparencyType.Alpha:
								faceType = FaceTransparencyType.Alpha;
								break;
							case TextureTransparencyType.Partial:
								faceType = FaceTransparencyType.Partial;
								break;
						}
					}

					if (State.Prototype.Mesh.Materials[face.Material].NighttimeTexture != null)
					{
						currentHost.LoadTexture(State.Prototype.Mesh.Materials[face.Material].NighttimeTexture, (OpenGlTextureWrapMode)State.Prototype.Mesh.Materials[face.Material].WrapMode);

						switch (State.Prototype.Mesh.Materials[face.Material].NighttimeTexture.Transparency)
						{
							case TextureTransparencyType.Alpha:
								faceType = FaceTransparencyType.Alpha;
								break;
							case TextureTransparencyType.Partial:
								faceType = FaceTransparencyType.Partial;
								break;
						}
					}
				}

				List<FaceState> list;

				switch (Type)
				{
					case ObjectType.Static:
					case ObjectType.Dynamic:
						switch (faceType)
						{
							case FaceTransparencyType.Opaque:
								list = myOpaqueFaces;
								break;
							case FaceTransparencyType.Partial:
								list = myPartialFaces;
								break;
							case FaceTransparencyType.Alpha:
								list = myAlphaFaces;
								break;
							default:
								throw new ArgumentOutOfRangeException();
						}
						break;
					case ObjectType.Overlay:
						switch (faceType)
						{
							case FaceTransparencyType.Opaque:
								list = myOverlayOpaqueFaces;
								break;
							case FaceTransparencyType.Partial:
								list = myOverlayPartialFaces;
								break;
							case FaceTransparencyType.Alpha:
								list = myOverlayAlphaFaces;
								break;
							default:
								throw new ArgumentOutOfRangeException();
						}
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(Type), Type, null);
				}

				if (faceType != FaceTransparencyType.Opaque)
				{
					/*
					 * Alpha faces should be inserted at the end of the list- We're going to sort it anyway so it makes no odds
					 */
					list.Add(new FaceState(State, face, renderer));
					continue;
				}

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
		}

		public void HideObject(ObjectState State)
		{
			RemoveObject(State);
		}

		private List<FaceState> SortPolygons(List<FaceState> faces)
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
						Vector3 w0 = v0.Xyz - camera.AbsolutePosition;
						t = Vector3.Dot(d, w0);
						distances[i] = -t * t;
					}
				}
			});

			// sort
			return faces.Select((face, index) => new { Face = face, Distance = distances[index] }).OrderBy(list => list.Distance).Select(list => list.Face).ToList();
		}

		private void SortPolygons(ref List<FaceState> myFaces, out ReadOnlyCollection<FaceState> faces)
		{
			myFaces = SortPolygons(myFaces);
			faces = myFaces.AsReadOnly();
		}

		public void SortPolygons(bool IsOverlay, FaceTransparencyType FaceType)
		{
			if (IsOverlay)
			{
				switch (FaceType)
				{
					case FaceTransparencyType.Opaque:
						throw new InvalidOperationException("There is no need to sort Opaque faces.");
					case FaceTransparencyType.Partial:
						SortPolygons(ref myPartialFaces, out PartialFaces);
						break;
					case FaceTransparencyType.Alpha:
						SortPolygons(ref myAlphaFaces, out AlphaFaces);
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(FaceType), FaceType, null);
				}
			}
			else
			{
				switch (FaceType)
				{
					case FaceTransparencyType.Opaque:
						throw new InvalidOperationException("There is no need to sort Opaque faces.");
					case FaceTransparencyType.Partial:
						SortPolygons(ref myOverlayPartialFaces, out OverlayPartialFaces);
						break;
					case FaceTransparencyType.Alpha:
						SortPolygons(ref myOverlayAlphaFaces, out OverlayAlphaFaces);
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(FaceType), FaceType, null);
				}
			}
		}

		public ReadOnlyCollection<FaceState> GetFaces(bool IsOverlay, FaceTransparencyType FaceType)
		{
			switch (FaceType)
			{
				case FaceTransparencyType.Opaque:
					return IsOverlay ? OverlayOpaqueFaces : OpaqueFaces;
				case FaceTransparencyType.Partial:
					return IsOverlay ? OverlayPartialFaces : PartialFaces;
				case FaceTransparencyType.Alpha:
					return IsOverlay ? OverlayAlphaFaces : AlphaFaces;
				default:
					throw new ArgumentOutOfRangeException(nameof(FaceType), FaceType, null);
			}
		}
	}
}
