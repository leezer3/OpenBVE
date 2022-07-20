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
	}
}
