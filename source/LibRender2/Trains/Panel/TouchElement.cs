using OpenBveApi.Objects;

namespace LibRender2.Trains
{
	/// <summary>An animated element of a train panel which may be manipulated with the mouse</summary>
	public class TouchElement
	{
		/// <summary>The animated object</summary>
		public AnimatedObject Element;
		/// <summary>The index of the screen to jump to (if applicable)</summary>
		public int JumpScreenIndex;
		/// <summary>The sound indicies associated with the object</summary>
		public int[] SoundIndices;
		/// <summary>The control indicies associated with the object</summary>
		public int[] ControlIndices;
		/// <summary>The cursor texture</summary>
		public MouseCursor MouseCursor;
	}
}
