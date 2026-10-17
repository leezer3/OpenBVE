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
		/// <summary>
		/// Whether the face is additively blended, so that its contribution never depends upon the order in
		/// which it is drawn.
		/// </summary>
		public readonly bool AdditiveBlend;
		/// <summary>
		/// Whether the alpha of the face is binary, i.e. it contains only fully transparent or fully opaque
		/// pixels, and the face has no distinct nighttime texture.
		/// </summary>
		/// <remarks>
		/// A binary face contributes nothing to a pass which discards its fully opaque pixels (the second pass
		/// of the quality transparency mode), but its fully opaque pixels are blended into the destination
		/// without a depth write in the performance transparency mode, where the draw order does still matter.
		/// </remarks>
		public readonly bool BinaryAlpha;

		public FaceState(ObjectState _object, MeshFace face, BaseRenderer renderer, bool additiveBlend = false, bool binaryAlpha = false)
		{
			Object = _object;
			Face = face;
			Renderer = renderer;
			AdditiveBlend = additiveBlend;
			BinaryAlpha = binaryAlpha;
			if (Object.Prototype.Mesh.VAO == null)
			{
				VAOExtensions.CreateOrUpdateVAO(Object.Prototype.Mesh, Object.Prototype.Dynamic, Renderer.DefaultShader.VertexLayout, Renderer);
            }
			
        }

		public void Draw()
		{
			Renderer.RenderFace(this);
		}
	}
}
