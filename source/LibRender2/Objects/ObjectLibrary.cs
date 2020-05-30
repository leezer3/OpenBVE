using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using LibRender2.Cameras;
using OpenBveApi;
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
		private readonly BaseOptions currentOptions;
		private readonly BaseRenderer renderer;

		private readonly List<ObjectState> myObjects;
		private readonly List<FaceState> myOpaqueFaces;
		private List<FaceState> myAlphaFaces;
		private readonly List<FaceState> myOverlayOpaqueFaces;
		private List<FaceState> myOverlayAlphaFaces;
		public readonly ReadOnlyCollection<ObjectState> Objects;
		public readonly ReadOnlyCollection<FaceState> OpaqueFaces;  // StaticOpaque and DynamicOpaque
		public ReadOnlyCollection<FaceState> AlphaFaces;  // DynamicAlpha
		public readonly ReadOnlyCollection<FaceState> OverlayOpaqueFaces;
		public ReadOnlyCollection<FaceState> OverlayAlphaFaces;

		internal VisibleObjectLibrary(HostInterface CurrentHost, CameraProperties Camera, BaseOptions CurrentOptions, BaseRenderer Renderer)
		{
			currentHost = CurrentHost;
			camera = Camera;
			currentOptions = CurrentOptions;
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
				myAlphaFaces.RemoveAll(x => x.Object == state);
				myOverlayOpaqueFaces.RemoveAll(x => x.Object == state);
				myOverlayAlphaFaces.RemoveAll(x => x.Object == state);
			}
		}

		public void Clear()
		{
			myObjects.Clear();
			myOpaqueFaces.Clear();
			myAlphaFaces.Clear();
			myOverlayOpaqueFaces.Clear();
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

				bool alpha = false;

				if (Type == ObjectType.Overlay && camera.CurrentRestriction != CameraRestrictionMode.NotAvailable)
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
						currentHost.LoadTexture(ref State.Prototype.Mesh.Materials[face.Material].DaytimeTexture, (OpenGlTextureWrapMode)State.Prototype.Mesh.Materials[face.Material].WrapMode);

						if (State.Prototype.Mesh.Materials[face.Material].DaytimeTexture.Transparency == TextureTransparencyType.Alpha)
						{
							alpha = true;
						}
						else if (State.Prototype.Mesh.Materials[face.Material].DaytimeTexture.Transparency == TextureTransparencyType.Partial && currentOptions.TransparencyMode == TransparencyMode.Quality)
						{
							alpha = true;
						}
					}

					if (State.Prototype.Mesh.Materials[face.Material].NighttimeTexture != null)
					{
						currentHost.LoadTexture(ref State.Prototype.Mesh.Materials[face.Material].NighttimeTexture, (OpenGlTextureWrapMode)State.Prototype.Mesh.Materials[face.Material].WrapMode);

						if (State.Prototype.Mesh.Materials[face.Material].NighttimeTexture.Transparency == TextureTransparencyType.Alpha)
						{
							alpha = true;
						}
						else if (State.Prototype.Mesh.Materials[face.Material].NighttimeTexture.Transparency == TextureTransparencyType.Partial && currentOptions.TransparencyMode == TransparencyMode.Quality)
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

		public void SortPolygonsInAlphaFaces()
		{
			myAlphaFaces = SortPolygons(myAlphaFaces);
			AlphaFaces = myAlphaFaces.AsReadOnly();
		}

		public void SortPolygonsInOverlayAlphaFaces()
		{
			myOverlayAlphaFaces = SortPolygons(myOverlayAlphaFaces);
			OverlayAlphaFaces = myOverlayAlphaFaces.AsReadOnly();
		}
	}
}
