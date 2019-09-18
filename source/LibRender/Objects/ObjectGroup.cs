using OpenBveApi.Math;

namespace LibRender
{
	/// <summary>Represents a group of linked objects to be drawn by the renderer</summary>
	/// <remarks>Must contain no transparent faces</remarks>
	public class ObjectGroup
	{
		/// <summary>The object list for this group</summary>
		public readonly ObjectList List;
		/// <summary>The openGL display list ID</summary>
		public int OpenGlDisplayList;
		/// <summary>Whether the openGL display list is currently valid for calling</summary>
		public bool OpenGlDisplayListAvailable;
		/// <summary>The absolute world position of this group</summary>
		public Vector3 WorldPosition;
		/// <summary>Whether this group should be updated and the display list refreshed</summary>
		public bool Update;

		/// <summary>Creates a new ObjectGroup</summary>
		public ObjectGroup()
		{
			this.List = new ObjectList();
			this.OpenGlDisplayList = 0;
			this.OpenGlDisplayListAvailable = false;
			this.WorldPosition = Vector3.Zero;
			this.Update = true;
		}
	}
}
