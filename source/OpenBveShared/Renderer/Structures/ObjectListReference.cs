namespace OpenBveShared
{
	public struct ObjectListReference
	{
		/// <summary>The type of list referenced.</summary>
		public readonly ObjectListType Type;
		/// <summary>The index in the specified list.</summary>
		public int Index;

		public ObjectListReference(ObjectListType type, int index)
		{
			this.Type = type;
			this.Index = index;
		}
	}
}
