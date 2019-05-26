using OpenBveApi.Textures;

namespace LibRender
{
	/// <summary>An object face to be renderered</summary>
	public class ObjectFace
	{
		/// <summary>The openGL display list index</summary>
		public int ObjectListIndex;
		/// <summary>The object index in the ObjectManager</summary>
		public int ObjectIndex;
		/// <summary>The face index within the object</summary>
		public int FaceIndex;
		/// <summary>The distance</summary>
		public double Distance;
		/// <summary>
		/// The openGL texture wrapping mode to be applied
		/// </summary>
		public OpenGlTextureWrapMode Wrap;
	}
}
