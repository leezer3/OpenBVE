using OpenBveApi.Textures;

namespace OpenBveShared
{
	/// <summary>Represents an object face to be shown by the renderer</summary>
	public class ObjectFace
	{
		/// <summary>The object list index</summary>
		public int ObjectListIndex;
		/// <summary>The overall object index</summary>
		public int ObjectIndex;
		/// <summary>The face index within the object</summary>
		public int FaceIndex;
		/// <summary>The visibility distance for this face</summary>
		public double Distance;
		/// <summary>The texture wrap mode to be applied</summary>
		public OpenGlTextureWrapMode Wrap;
	}
}
