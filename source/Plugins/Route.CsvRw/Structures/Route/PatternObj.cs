using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.World;

namespace CsvRwRouteParser
{
	internal class PatternObj
	{
		/// <summary>The *last* placement position of the object</summary>
		internal double LastPlacement;
		/// <summary>The rail</summary>
		internal readonly int RailIndex;
		/// <summary>The placement interval</summary>
		internal double Interval;
		/// <summary>The last type of object placed</summary>
		internal int LastType;
		/// <summary>The routefile indices of the objects to be repeated</summary>
		internal int[] Types;
		/// <summary>The position of the object</summary>
		internal Vector2 Position;
		/// <summary>Whether the pattern ends this block</summary>
		internal bool Ends;

		internal PatternObj(int rail)
		{
			RailIndex = rail;
		}

		internal PatternObj Clone()
		{
			PatternObj p = new PatternObj(RailIndex)
			{
				Interval = Interval,
				Types = Types,
				Position = Position
			};
			return p;
		}

		internal bool CreateRailAligned(ObjectDictionary FreeObjects, Vector3 WorldPosition, Transformation RailTransformation, double StartingDistance, double EndingDistance)
		{
			if (Types.Length == 0)
			{
				return false;
			}
			if (LastType > Types.Length - 1)
			{
				LastType = 0;
			}
			LastPlacement += Interval;
			double dz = LastPlacement - StartingDistance;
			WorldPosition += Position.X * RailTransformation.X + Position.Y * RailTransformation.Y + dz * RailTransformation.Z;
			FreeObjects.TryGetValue(Types[LastType], out UnifiedObject obj);
			obj?.CreateObject(WorldPosition, RailTransformation, new Transformation(), StartingDistance, EndingDistance, LastPlacement);

			if (Types.Length > 1)
			{
				LastType++;
			}

			return true;
		}
	}
}
