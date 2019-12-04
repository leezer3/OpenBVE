using OpenBveApi.Objects;

namespace LibRender2.Objects
{
	/// <summary>Represents a face state within the renderer</summary>
	public struct FaceState
	{
		/// <summary>The containing object</summary>
		public readonly ObjectState Object;
		/// <summary>The face to draw</summary>
		public readonly MeshFace Face;

		public FaceState(ObjectState _object, MeshFace face)
		{
			Object = _object;
			Face = face;
		}
	}
}
