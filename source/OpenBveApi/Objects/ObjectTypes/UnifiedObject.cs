﻿using OpenBveApi.Math;
using OpenBveApi.World;

namespace OpenBveApi.Objects
{
	/// <summary>A unified object is the abstract class encompassing all object types within the sim</summary>
	public abstract class UnifiedObject
	{
		/// <summary>Creates the object within the worldspace without using track based transforms</summary>
		/// <param name="Position">The world position</param>
		/// <param name="Properties">The world properties of the object</param>
		public void CreateObject(Vector3 Position, WorldProperties Properties)
		{
			CreateObject(Position, Transformation.NullTransformation, Transformation.NullTransformation, Properties);
		}

		/// <summary>Creates the object within the worldspace using a single track based transforms</summary>
		/// <param name="Position">The world position</param>
		/// <param name="WorldTransformation">The world transformation to apply (e.g. ground, rail)</param>
		/// <param name="Properties">The world properties of the object</param>
		public void CreateObject(Vector3 Position, Transformation WorldTransformation, WorldProperties Properties)
		{
			CreateObject(Position, WorldTransformation, Transformation.NullTransformation, Properties);
		}

		/// <summary>Creates the object within the world</summary>
		/// <param name="Position">The world position</param>
		/// <param name="WorldTransformation">The world transformation to apply (e.g. ground, rail)</param>
		/// <param name="LocalTransformation">The local transformation to apply in order to rotate the model</param>
		/// <param name="Properties">The world properties of the object</param>
		/// <param name="DuplicateMaterials">Whether the materials are to be duplicated (Not set when creating BVE4 signals)</param>
		public abstract void CreateObject(Vector3 Position, Transformation WorldTransformation, Transformation LocalTransformation, WorldProperties Properties, bool DuplicateMaterials = false);

		/// <summary>Call this method to optimize the object</summary>
		/// <param name="PreserveVerticies">Whether duplicate verticies are to be preserved (Takes less time)</param>
		/// <param name="Threshold">The face size threshold for optimization</param>
		/// <param name="VertexCulling">Whether vertex culling is performed</param>
		public abstract void OptimizeObject(bool PreserveVerticies, int Threshold, bool VertexCulling);

		/// <summary>Creates a clone of this object</summary>
		/// <returns>The cloned object</returns>
		public abstract UnifiedObject Clone();

		/// <summary>Creates a mirrored clone of this object</summary>
		/// <returns>The mirrored clone</returns>
		public abstract UnifiedObject Mirror();

		/// <summary>Creates a transformed clone of this object</summary>
		/// <param name="NearDistance">The object's width at the start of the block</param>
		/// <param name="FarDistance">The object's width at the end of the block</param>
		/// <returns>The transformed clone</returns>
		public abstract UnifiedObject Transform(double NearDistance, double FarDistance);
	}
}
