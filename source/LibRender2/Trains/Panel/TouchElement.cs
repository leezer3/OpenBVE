using OpenBveApi.Objects;

namespace LibRender2.Trains
{
	/// <summary>An animated element of a train panel which may be manipulated with the mouse</summary>
	public class TouchElement
	{
		/// <summary>The animated object</summary>
		public readonly AnimatedObject Element;
		/// <summary>The index of the screen to jump to (if applicable)</summary>
		public readonly int JumpScreenIndex;
		/// <summary>The sound indicies associated with the object</summary>
		public readonly int[] SoundIndices;
		/// <summary>The control indicies associated with the object</summary>
		public readonly int[] ControlIndices;
		/// <summary>The cursor texture</summary>
		public MouseCursor MouseCursor;

		public TouchElement(AnimatedObject element, int jumpScreenIndex, int[] soundIndices, int[] controlIndices)
		{
			Element = element;
			JumpScreenIndex = jumpScreenIndex;
			SoundIndices = soundIndices;
			ControlIndices = controlIndices;
		}
	}
}
