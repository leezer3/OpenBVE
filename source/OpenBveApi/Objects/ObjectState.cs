using System;
using OpenBveApi.Math;

namespace OpenBveApi.Objects
{
	/// <summary>A single internal state of an Object</summary>
	public class ObjectState : ICloneable
	{
		/// <summary>The prototype static object</summary>
		public StaticObject Prototype;
		/// <summary>The translation matrix to be applied</summary>
		public Matrix4D Translation;
		/// <summary>The scale matrix to be applied</summary>
		public Matrix4D Scale;
		/// <summary>The rotation matrix to be applied</summary>
		public Matrix4D Rotate;
		/// <summary>The texture translation matrix to be applied</summary>
		public Matrix4D TextureTranslation;
		/// <summary>The brightness value at this object's track position</summary>
		public double Brightness;

		/// <summary>The starting track position, for static objects only.</summary>
		public float StartingDistance;
		/// <summary>The ending track position, for static objects only.</summary>
		public float EndingDistance;

		/// <summary>Creates a new ObjectState</summary>
		public ObjectState()
		{
			Prototype = null;
			Translation = Matrix4D.Identity;
			Scale = Matrix4D.Identity;
			Rotate = Matrix4D.Identity;
			TextureTranslation = Matrix4D.Identity;
			Brightness = 0.0;
			StartingDistance = 0.0f;
			EndingDistance = 0.0f;
		}

		/// <summary>Clones this ObjectState</summary>
		public object Clone()
		{
			ObjectState os = (ObjectState) MemberwiseClone();
			os.Prototype = Prototype?.Clone() as StaticObject;
			return os;
		}
	}
}
