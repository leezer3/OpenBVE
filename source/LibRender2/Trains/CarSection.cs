using OpenBveApi.Hosts;
using OpenBveApi.Objects;

namespace LibRender2.Trains
{
	/// <summary>An animated object attached to a car (Exterior, cab etc.)</summary>
	public class CarSection
	{
		/// <summary>Holds a reference to the current host</summary>
		private readonly HostInterface currentHost;
		/// <summary>The groups of animated objects</summary>
		public ElementsGroup[] Groups;
		/// <summary>The current additional group (touch etc.)</summary>
		public int CurrentAdditionalGroup;
		/// <summary>Whether this is visible from internal views</summary>
		public bool VisibleFromInterior;

		public CarSection(HostInterface Host, ObjectType Type)
		{
			currentHost = Host;
			Groups = new ElementsGroup[1];
			Groups[0] = new ElementsGroup(Type);
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
					currentHost.ShowObject(Groups[0].Elements[i].internalObject, Groups[0].Type);
				}
			}

			int add = CurrentAdditionalGroup + 1;
			if (add < Groups.Length)
			{
				for (int i = 0; i < Groups[add].Elements.Length; i++)
				{
					currentHost.ShowObject(Groups[add].Elements[i].internalObject, Groups[add].Type);
					
				}
			}
		}
	}
}
