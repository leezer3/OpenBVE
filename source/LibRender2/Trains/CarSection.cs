using OpenBveApi.Objects;

namespace LibRender2.Trains
{
	/// <summary>An animated object attached to a car (Exterior, cab etc.)</summary>
	public class CarSection
	{
		/// <summary>Holds a reference to the base renderer</summary>
		private readonly BaseRenderer renderer;
		/// <summary>The groups of animated objects</summary>
		public ElementsGroup[] Groups;
		/// <summary>The current additional group (touch etc.)</summary>
		public int CurrentAdditionalGroup;
		/// <summary>Whether this is visible from internal views</summary>
		public bool VisibleFromInterior;

		public CarSection(BaseRenderer Renderer, bool Overlay)
		{
			renderer = Renderer;
			Groups = new ElementsGroup[1];
			Groups[0] = new ElementsGroup(Overlay);
		}

		/// <summary>Initalizes the CarSection</summary>
		/// <param name="CurrentlyVisible">Whether visible at the time of this call</param>
		public void Initialize(bool CurrentlyVisible)
		{
			for (int i = 0; i < Groups.Length; i++)
			{
				Groups[i].Initialize(CurrentlyVisible);
			}
		}

		/// <summary>Shows all objects associated with the car section</summary>
		public void Show()
		{
			if (Groups.Length > 0)
			{
				for (int i = 0; i < Groups[0].Elements.Length; i++)
				{
					if (Groups[0].Overlay)
					{
						renderer.VisibleObjects.ShowObject(Groups[0].Elements[i].internalObject, ObjectType.Overlay);
					}
					else
					{
						renderer.VisibleObjects.ShowObject(Groups[0].Elements[i].internalObject, ObjectType.Dynamic);
					}
				}
			}

			int add = CurrentAdditionalGroup + 1;
			if (add < Groups.Length)
			{
				for (int i = 0; i < Groups[add].Elements.Length; i++)
				{
					if (Groups[add].Overlay)
					{
						renderer.VisibleObjects.ShowObject(Groups[add].Elements[i].internalObject, ObjectType.Overlay);
					}
					else
					{
						renderer.VisibleObjects.ShowObject(Groups[add].Elements[i].internalObject, ObjectType.Dynamic);
					}
				}
			}
		}
	}
}
