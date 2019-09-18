namespace LibRender
{
	public struct ObjectListReference
	{
		/// <summary>The type of list.</summary>
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
