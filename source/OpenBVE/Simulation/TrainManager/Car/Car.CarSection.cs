namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		/// <summary>An animated object attached to a car (Exterior, cab etc.)</summary>
		internal class CarSection
		{
			internal ObjectManager.AnimatedObject[] Elements;
			internal bool Overlay;

			internal void Initialize(bool CurrentlyVisible)
			{
				for (int j = 0; j < Elements.Length; j++)
				{
					for (int k = 0; k < Elements[j].States.Length; k++)
					{
						Elements[j].Initialize(k, Overlay, CurrentlyVisible);
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
