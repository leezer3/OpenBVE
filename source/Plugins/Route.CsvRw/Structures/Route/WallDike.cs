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

		internal WallDike(int type, Direction direction, bool exists = true)
		{
			Exists = exists;
			Type = type;
			Direction = direction;
		}

		internal WallDike Clone()
		{
			WallDike w = new WallDike(Type, Direction, Exists);
			return w;
		}
	}
}
