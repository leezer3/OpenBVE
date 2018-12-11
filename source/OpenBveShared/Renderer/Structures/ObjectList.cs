namespace OpenBveShared
{
	public class ObjectList
	{
		public ObjectFace[] Faces;
		public int FaceCount;
		public BoundingBox[] BoundingBoxes;

		public ObjectList()
		{
			this.Faces = new ObjectFace[256];
			this.FaceCount = 0;
			this.BoundingBoxes = new BoundingBox[256];
		}
	}
}
