namespace Bve5RouteParser
{
	internal struct RepeaterType
	{
		internal int Type;
		internal int ObjectArrayIndex;

		public RepeaterType(RepeaterType typeToClone)
		{
			Type = typeToClone.Type;
			ObjectArrayIndex = typeToClone.ObjectArrayIndex;
		}
	}
}
