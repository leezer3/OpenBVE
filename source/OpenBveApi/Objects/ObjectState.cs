using System;
using OpenBveApi.Math;

namespace OpenBveApi.Objects
{
	/// <summary>A single internal state of an Object</summary>
	public class ObjectState : ICloneable
	{
		/// <summary>A reference to the prototype static object</summary>
		public StaticObject Prototype;
		/// <summary>The translation matrix to be applied</summary>
		private Matrix4D _translation;
		/// <summary>The scale matrix to be applied</summary>
		private Matrix4D _scale;
		/// <summary>The rotation matrix to be applied</summary>
		private Matrix4D _rotate;
		/// <summary>The world position vector</summary>
		public Vector3 WorldPosition;
		/// <summary>A value between 0 (daytime) and 255 (nighttime).</summary>
		public byte DaytimeNighttimeBlend = 0;
		/// <summary>The matricies within the object</summary>
		public Matrix4D[] Matricies;
		/// <summary>The index of the storage buffer for the matricies</summary>
		public int MatrixBufferIndex;
		/// <summary>The translation matrix to be applied</summary>
		public Matrix4D Translation
		{
			get => _translation;
			set
			{
				_translation = value;
				updateModelMatrix = true;
			}
		}
		/// <summary>The scale matrix to be applied</summary>
		public Matrix4D Scale
		{
			get => _scale;
			set
			{
				_scale = value;
				updateModelMatrix = true;
			}
		}
		/// <summary>The rotation matrix to be applied</summary>
		public Matrix4D Rotate
		{
			get => _rotate;
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

				updateModelMatrix = false;
				modelMatrix = _scale * _rotate * _translation;
				return modelMatrix;
			}
		}

		private bool updateModelMatrix;
		/// <summary>The texture translation matrix to be applied</summary>
		public Matrix4D TextureTranslation;
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
			StartingDistance = 0.0f;
			EndingDistance = 0.0f;
			updateModelMatrix = false;
		}

		/// <summary>Creates a new ObjectState</summary>
		public ObjectState(StaticObject prototype) : this()
		{
			Prototype = prototype;
		}

		/// <summary>Clones this ObjectState</summary>
		public object Clone()
		{
			return MemberwiseClone();
		}

		/// <summary>Reverses this ObjectState</summary>
		/// <param name="pos">The viewer position</param>
		public void Reverse(Vector3 pos = new Vector3())
		{
			Vector3 translation = Translation.ExtractTranslation();
			translation.X -= pos.X * 2;
			translation.Z += pos.Z * 2;
			Translation = Matrix4D.CreateTranslation(translation);
		}
	}
}
