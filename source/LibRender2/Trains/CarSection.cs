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
		public readonly bool VisibleFromInterior;

		public CarSection(HostInterface Host, ObjectType Type, bool visibleFromInterior, UnifiedObject Object = null)
		{
			currentHost = Host;
			Groups = new ElementsGroup[1];
			Groups[0] = new ElementsGroup(Type);
			VisibleFromInterior = visibleFromInterior;
			if (Object is StaticObject)
			{
				StaticObject s = (StaticObject) Object;
				Groups[0].Elements = new AnimatedObject[1];
				Groups[0].Elements[0] = new AnimatedObject(Host)
				{
					States = new[] {new ObjectState(s)},
					CurrentState = 0
				};
				Groups[0].Elements[0].IsPartOfTrain = true;
				currentHost.CreateDynamicObject(ref Groups[0].Elements[0].internalObject);
			}
			else if (Object is AnimatedObjectCollection)
			{
				AnimatedObjectCollection a = (AnimatedObjectCollection)Object;
				Groups[0].Elements = new AnimatedObject[a.Objects.Length];
				for (int h = 0; h < a.Objects.Length; h++)
				{
					Groups[0].Elements[h] = a.Objects[h].Clone() as AnimatedObject;
					Groups[0].Elements[h].IsPartOfTrain = true;
					currentHost.CreateDynamicObject(ref Groups[0].Elements[h].internalObject);
				}
			}
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
