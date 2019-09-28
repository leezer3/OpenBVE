using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using LibRender2.Cameras;
using OpenBveApi;
using OpenBveApi.Graphics;
using OpenBveApi.Hosts;
using OpenBveApi.Objects;
using OpenBveApi.Textures;
using OpenTK;

namespace LibRender2.Objects
{
	public class VisibleObjectLibrary
	{
		private readonly HostInterface currentHost;
		private readonly CameraProperties camera;
		private readonly BaseOptions currentOptions;

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

		internal VisibleObjectLibrary(HostInterface CurrentHost, CameraProperties Camera, BaseOptions CurrentOptions)
		{
			currentHost = CurrentHost;
			camera = Camera;
			currentOptions = CurrentOptions;

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

			if (State.Prototype.Mesh.VAO == null)
			{
				State.Prototype.Mesh.CreateVAO(State.Prototype.Dynamic);
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
						currentHost.LoadTexture(State.Prototype.Mesh.Materials[face.Material].DaytimeTexture, (OpenGlTextureWrapMode)State.Prototype.Mesh.Materials[face.Material].WrapMode);

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
						currentHost.LoadTexture(State.Prototype.Mesh.Materials[face.Material].NighttimeTexture, (OpenGlTextureWrapMode)State.Prototype.Mesh.Materials[face.Material].WrapMode);

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

				/*
				 * For all lists, insert the face at the end of the list.
				 * */
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

				list.Add(new FaceState(State, face));
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
					Vector4d v0 = new Vector4d((Vector3d)faces[i].Object.Prototype.Mesh.Vertices[faces[i].Face.Vertices[0].Index].Coordinates, 1.0);
					Vector4d v1 = new Vector4d((Vector3d)faces[i].Object.Prototype.Mesh.Vertices[faces[i].Face.Vertices[1].Index].Coordinates, 1.0);
					Vector4d v2 = new Vector4d((Vector3d)faces[i].Object.Prototype.Mesh.Vertices[faces[i].Face.Vertices[2].Index].Coordinates, 1.0);
					Vector4d w1 = v1 - v0;
					Vector4d w2 = v2 - v0;
					v0.Z *= -1.0;
					w1.Z *= -1.0;
					w2.Z *= -1.0;
					Matrix4d mat = faces[i].Object.Scale * faces[i].Object.Rotate * faces[i].Object.Translation;
					v0 = Vector4d.Transform(v0, mat);
					w1 = Vector4d.Transform(w1, mat);
					w2 = Vector4d.Transform(w2, mat);
					v0.Z *= -1.0;
					w1.Z *= -1.0;
					w2.Z *= -1.0;
					Vector3d d = Vector3d.Cross(w1.Xyz, w2.Xyz);
					double t = d.Length;

					if (t != 0.0)
					{
						d /= t;
						Vector3d w0 = v0.Xyz - (Vector3d)camera.AbsolutePosition;
						t = Vector3d.Dot(d, w0);
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
