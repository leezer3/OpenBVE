namespace Bve5RouteParser
{
	internal class RepeaterType
	{
		internal readonly int Type;
		internal readonly int ObjectArrayIndex;

		internal RepeaterType(int type, int objectArrayIndex)
		{
			Type = type;
			ObjectArrayIndex = objectArrayIndex;
		}

		public RepeaterType(RepeaterType typeToClone)
		{
			Type = typeToClone.Type;
			ObjectArrayIndex = typeToClone.ObjectArrayIndex;
		}
	}
}
