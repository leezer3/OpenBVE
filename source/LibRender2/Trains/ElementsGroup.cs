using OpenBveApi.Math;
using OpenBveApi.Objects;

namespace LibRender2.Trains
{
	/// <summary>A group of elements</summary>
	public class ElementsGroup
	{
		/// <summary>The animated objects</summary>
		public AnimatedObject[] Elements;

		/// <summary>The touch elements if applicable</summary>
		public TouchElement[] TouchElements;

		public KeyframeAnimatedObject Keyframes;

		public ElementsGroup()
		{
			Elements = new AnimatedObject[] {};
		}

		/// <summary>Initializes the ElementsGroup</summary>
		/// <param name="CurrentlyVisible">Whether visible at the time of the call</param>
		/// <param name="Type">The object type</param>
		public void Initialize(bool CurrentlyVisible, ObjectType Type)
		{
			for (int i = 0; i < Elements.Length; i++)
			{
				for (int j = 0; j < Elements[i].States.Length; j++)
				{
					Elements[i].Initialize(j, Type, CurrentlyVisible);
				}
			}
		}

		public void PreloadTextures()
		{
			for (int i = 0; i < Elements.Length; i++)
			{
				Elements[i].LoadTextures();
			}
		}

		/// <summary>Reverses the elements group</summary>
		/// <param name="pos">The drivers eye position</param>
		/// <param name="is3D">Whether this is a 3D or 2D elements group</param>
		public void Reverse(Vector3 pos, bool is3D)
		{
			if (is3D)
			{
				for (int i = 0; i < Elements.Length; i++)
				{
					Elements[i].Reverse(true);
				}

				if (TouchElements == null)
				{
					return;
				}
			
				for (int i = 0; i < TouchElements.Length; i++)
				{
					TouchElements[i].Element.Reverse(true);
				}
			}
			else
			{
				for (int i = 0; i < Elements.Length; i++)
				{
					for (int j = 0; j < Elements[i].States.Length; j++)
					{
						Elements[i].States[j].Reverse(pos);
					}
				}

				if (TouchElements == null)
				{
					return;
				}
			
				for (int i = 0; i < TouchElements.Length; i++)
				{
					for (int j = 0; j < TouchElements[i].Element.States.Length; j++)
					{
						TouchElements[i].Element.States[j].Reverse(pos);
					}
				}
			}
		}
	}
}
