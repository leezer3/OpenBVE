using System;
using OpenTK;

namespace OpenBveApi.Objects
{
	public class ObjectState : ICloneable
	{
		public StaticObject Prototype;

		public Matrix4d Translation;

		public Matrix4d Scale;

		public Matrix4d Rotate;

		public Matrix4d TextureTranslation;

		public double Brightness;

		/// <summary>The starting track position, for static objects only.</summary>
		public float StartingDistance;
		/// <summary>The ending track position, for static objects only.</summary>
		public float EndingDistance;

		public ObjectState()
		{
			Prototype = null;
			Translation = Matrix4d.Identity;
			Scale = Matrix4d.Identity;
			Rotate = Matrix4d.Identity;
			TextureTranslation = Matrix4d.Identity;
			Brightness = 0.0;
			StartingDistance = 0.0f;
			EndingDistance = 0.0f;
		}

		public object Clone()
		{
			ObjectState os = (ObjectState)MemberwiseClone();
			os.Prototype = Prototype?.Clone() as StaticObject;
			return os;
		}
	}

	public struct FaceState
	{
		public readonly ObjectState Object;
		public readonly MeshFace Face;

		public FaceState(ObjectState _object, MeshFace face)
		{
			Object = _object;
			Face = face;
		}
	}
}
