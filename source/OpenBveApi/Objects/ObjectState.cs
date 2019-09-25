using System;
using OpenTK;

namespace OpenBveApi.Objects
{
	public class ObjectState : ICloneable
	{
		public StaticObject Prototype;

		public Matrix4 Translation;

		public Matrix4 Scale;

		public Matrix4 Rotate;

		public Matrix4 TextureTranslation;

		public double Brightness;

		/// <summary>The starting track position, for static objects only.</summary>
		public float StartingDistance;
		/// <summary>The ending track position, for static objects only.</summary>
		public float EndingDistance;

		public ObjectState()
		{
			Prototype = null;
			Translation = Matrix4.Identity;
			Scale = Matrix4.Identity;
			Rotate = Matrix4.Identity;
			TextureTranslation = Matrix4.Identity;
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
