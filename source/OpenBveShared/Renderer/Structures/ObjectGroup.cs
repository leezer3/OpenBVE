using OpenBveApi.Math;

namespace OpenBveShared
{
	public class ObjectGroup
	{
		public readonly ObjectList List;
		public int OpenGlDisplayList;
		public bool OpenGlDisplayListAvailable;
		public Vector3 WorldPosition;
		public bool Update;
		public ObjectGroup()
		{
			this.List = new ObjectList();
			this.OpenGlDisplayList = 0;
			this.OpenGlDisplayListAvailable = false;
			this.WorldPosition = new Vector3(0.0, 0.0, 0.0);
			this.Update = true;
		}
	}
}
