using OpenBveApi.Interface;
using OpenBveApi.Objects;

namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		internal class TouchElement
		{
			internal AnimatedObject Element;
			internal int JumpScreenIndex;
			internal int SoundIndex;
			internal Translations.Command Command;
			internal int CommandOption;
		}

		internal class ElementsGroup
		{
			internal AnimatedObject[] Elements;
			internal bool Overlay;
			internal TouchElement[] TouchElements;

			internal void Initialize(bool CurrentlyVisible)
			{
				for (int i = 0; i < Elements.Length; i++)
				{
					for (int j = 0; j < Elements[i].States.Length; j++)
					{
						Elements[i].Initialize(j, Overlay, CurrentlyVisible);
					}
				}
			}
		}

		/// <summary>An animated object attached to a car (Exterior, cab etc.)</summary>
		internal class CarSection
		{
			internal ElementsGroup[] Groups;
			internal int CurrentAdditionalGroup;

			internal void Initialize(bool CurrentlyVisible)
			{
				for (int i = 0; i < Groups.Length; i++)
				{
					Groups[i].Initialize(CurrentlyVisible);
				}
			}

			internal void Show()
			{
				if (Groups.Length > 0)
				{
					for (int i = 0; i < Groups[0].Elements.Length; i++)
					{
						if (Groups[0].Overlay)
						{
							Program.CurrentHost.ShowObject(Groups[0].Elements[i].internalObject, ObjectType.Overlay);
						}
						else
						{
							Program.CurrentHost.ShowObject(Groups[0].Elements[i].internalObject, ObjectType.Dynamic);
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
							Program.CurrentHost.ShowObject(Groups[add].Elements[i].internalObject, ObjectType.Overlay);
						}
						else
						{
							Program.CurrentHost.ShowObject(Groups[add].Elements[i].internalObject, ObjectType.Dynamic);
						}
					}
				}
			}
		}

		internal enum CarSectionType
		{
			NotVisible = -1,
			Interior = 0,
			Exterior = 1
		}

	}
}
