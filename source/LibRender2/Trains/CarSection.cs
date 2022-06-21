using OpenBveApi.Hosts;
using OpenBveApi.Objects;

namespace LibRender2.Trains
{
	/// <summary>An animated object attached to a car (Exterior, cab etc.)</summary>
	public class CarSection
	{
		
		/// <summary>The groups of animated objects</summary>
		public ElementsGroup[] Groups;
		/// <summary>The current additional group (touch etc.)</summary>
		public int CurrentAdditionalGroup;
		/// <summary>Whether this is visible from internal views</summary>
		public readonly bool VisibleFromInterior;
		/// <summary>Whether this is to be shown in overlay mode (e.g. panel)</summary>
		public readonly ObjectType Type;

		public CarSection(HostInterface Host, BaseRenderer Renderer, ObjectType ObjectType, bool visibleFromInterior, UnifiedObject Object = null)
		{
			Groups = new ElementsGroup[1];
			VisibleFromInterior = visibleFromInterior;
			if (Object is StaticObject)
			{
				Groups[0] = new AnimatedElementsGroup(Host, Renderer, Object as StaticObject);
			}
			else if (Object is AnimatedObjectCollection)
			{
				Groups[0] = new AnimatedElementsGroup(Host, Renderer, Object as AnimatedObjectCollection);
			}
			Type = ObjectType;
		}

		/// <summary>Initalizes the CarSection</summary>
		/// <param name="CurrentlyVisible">Whether visible at the time of this call</param>
		public void Initialize(bool CurrentlyVisible)
		{
			for (int i = 0; i < Groups.Length; i++)
			{
				Groups[i].Initialize(CurrentlyVisible, Type);
			}
		}

		/// <summary>Shows all objects associated with the car section</summary>
		public void Show()
		{
			if (Groups.Length > 0)
			{
				Groups[0].Show(Type);
			}

			int add = CurrentAdditionalGroup + 1;
			if (add < Groups.Length)
			{
				Groups[add].Show(Type);
			}
		}
	}
}
