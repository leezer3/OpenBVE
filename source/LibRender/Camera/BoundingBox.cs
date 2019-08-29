using OpenBveApi.Math;

namespace LibRender
{
	/// <summary>A 3D region of space which the camera may not enter</summary>
	public class BoundingBox
	{
		/// <summary>The upper left bound</summary>
		public Vector3 Upper;
		/// <summary>The lower right bound</summary>
		public Vector3 Lower;
	}
}
