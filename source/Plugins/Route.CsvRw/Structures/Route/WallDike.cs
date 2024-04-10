using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.World;

namespace CsvRwRouteParser
{
	internal class WallDike
	{
		/// <summary>Whether the wall/ dike is shown for this block</summary>
		internal bool Exists;
		/// <summary>The routefile index of the object</summary>
		internal readonly int Type;
		/// <summary>The direction the object(s) are placed in: -1 for left, 0 for both, 1 for right</summary>
		internal readonly Direction Direction;
		/// <summary>Reference to the appropriate left-sided object array</summary>
		internal readonly ObjectDictionary leftObjects;
		/// <summary>Reference to the appropriate right-sided object array</summary>
		internal readonly ObjectDictionary rightObjects;

		internal WallDike(int type, Direction direction, ObjectDictionary LeftObjects, ObjectDictionary RightObjects, bool exists = true)
		{
			Exists = exists;
			Type = type;
			Direction = direction;
			leftObjects = LeftObjects;
			rightObjects = RightObjects;
		}

		internal WallDike Clone()
		{
			WallDike w = new WallDike(Type, Direction, leftObjects, rightObjects, Exists);
			return w;
		}

		internal void Create(Vector3 pos, Transformation RailTransformation, WorldProperties Properties)
		{
			if (!Exists)
			{
				return;
			}
			if (Direction <= 0)
			{
				if (leftObjects.ContainsKey(Type))
				{
					leftObjects[Type].CreateObject(pos, RailTransformation, Properties);	
				}
				
			}
			if (Direction >= 0)
			{
				if (rightObjects.ContainsKey(Type))
				{
					rightObjects[Type].CreateObject(pos, RailTransformation, Properties);
				}
				
			}
		}
	}
}
