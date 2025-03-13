using OpenBveApi.Objects;

namespace LibRender2.Objects
{
	/// <summary>Represents a face state within the renderer</summary>
	public class FaceState
	{
		/// <summary>The containing object</summary>
		public readonly ObjectState Object;
		/// <summary>The face to draw</summary>
		public readonly MeshFace Face;
		/// <summary>Holds the reference to the base renderer</summary>
		public readonly BaseRenderer Renderer;

		public FaceState(ObjectState _object, MeshFace face, BaseRenderer renderer)
		{
			Object = _object;
			Face = face;
			Renderer = renderer;
			if (Object.Prototype.Mesh.VAO == null && !Renderer.ForceLegacyOpenGL)
			{
				VAOExtensions.CreateVAO(Object.Prototype.Mesh, Object.Prototype.Dynamic, Renderer.DefaultShader.VertexLayout, Renderer);
			}
		}

		public void Draw()
		{
			if (Renderer.AvailableNewRenderer)
			{
				Renderer.RenderFace(this);
			}
			else
			{
				Renderer.RenderFaceImmediateMode(this);
			}
		}
	}
}
