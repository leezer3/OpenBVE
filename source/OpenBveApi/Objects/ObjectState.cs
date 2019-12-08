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
		private Matrix4D _translation;
		/// <summary>The scale matrix to be applied</summary>
		private Matrix4D _scale;
		/// <summary>The rotation matrix to be applied</summary>
		private Matrix4D _rotate;

		public Vector3 WorldPosition;

		/// <summary>The translation matrix to be applied</summary>
		public Matrix4D Translation
		{
			get
			{
				return _translation;
			}
			set
			{
				_translation = value;
				updateModelMatrix = true;
			}
		}
		/// <summary>The scale matrix to be applied</summary>
		public Matrix4D Scale
		{
			get
			{
				return _scale;
			}
			set
			{
				_scale = value;
				updateModelMatrix = true;
			}
		}
		/// <summary>The rotation matrix to be applied</summary>
		public Matrix4D Rotate
		{
			get
			{
				return _rotate;
			}
			set
			{
				_rotate = value;
				updateModelMatrix = true;
			}
		}

		/// <summary>Backing property holding the cached model matrix</summary>
		private Matrix4D modelMatrix;

		/// <summary>Returns the final model matrix</summary>
		public Matrix4D ModelMatrix
		{
			get
			{
				if(updateModelMatrix == false)
				{
					return modelMatrix;
				}
				else
				{
					updateModelMatrix = false;
					modelMatrix = _scale * _rotate * _translation;
					return modelMatrix;
				}
			}
		}

		private bool updateModelMatrix;
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
			_translation = Matrix4D.Identity;
			_scale = Matrix4D.Identity;
			_rotate = Matrix4D.Identity;
			TextureTranslation = Matrix4D.Identity;
			Brightness = 0.0;
			StartingDistance = 0.0f;
			EndingDistance = 0.0f;
			updateModelMatrix = false;
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
