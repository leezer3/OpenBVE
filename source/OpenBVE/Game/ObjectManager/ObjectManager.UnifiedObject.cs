using OpenBveApi.Math;
using OpenBveApi.World;

namespace OpenBve
{
	/// <summary>The ObjectManager is the root class containing functions to load and manage objects within the simulation world</summary>
	public static partial class ObjectManager
	{
		/// <summary>A unified object is the abstract class encompassing all object types within the sim</summary>
		internal abstract class UnifiedObject
		{
			/// <summary>Creates the object within the world</summary>
			/// <param name="Position">The world position</param>
			/// <param name="BaseTransformation">The base transformation to be applied</param>
			/// <param name="AuxTransformation">The auxiliary transformation to be applied</param>
			/// <param name="AccurateObjectDisposal">Whether accurate object disposal is in use</param>
			/// <param name="StartingDistance">The track distance at which this is displayed by the renderer</param>
			/// <param name="EndingDistance">The track distance at which this hidden by the renderer</param>
			/// <param name="BlockLength">The current block length (Used when Options.ObjectVisibility is set to legacy mode)</param>
			/// <param name="TrackPosition">The absolute track position at which this object is placed</param>
			internal void CreateObject(Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition)
			{
				CreateObject(Position, BaseTransformation, AuxTransformation, -1, AccurateObjectDisposal, StartingDistance, EndingDistance, BlockLength, TrackPosition, 1.0, false);
			}

			/// <summary>Creates the object within the world</summary>
			/// <param name="Position">The world position</param>
			/// <param name="BaseTransformation">The base transformation to be applied</param>
			/// <param name="AuxTransformation">The auxiliary transformation to be applied</param>
			/// <param name="SectionIndex">The section index (If placed via Track.SigF)</param>
			/// <param name="AccurateObjectDisposal">Whether accurate object disposal is in use</param>
			/// <param name="StartingDistance">The track distance at which this is displayed by the renderer</param>
			/// <param name="EndingDistance">The track distance at which this hidden by the renderer</param>
			/// <param name="BlockLength">The current block length (Used when Options.ObjectVisibility is set to legacy mode)</param>
			/// <param name="TrackPosition">The absolute track position at which this object is placed</param>
			/// <param name="Brightness">The brightness value of this object</param>
			/// <param name="DuplicateMaterials">Whether the materials are to be duplicated (Not set when creating BVE4 signals)</param>
			internal abstract void CreateObject(Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, int SectionIndex, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness, bool DuplicateMaterials);

			/// <summary>Call this method to optimize the object</summary>
			/// <param name="PreserveVerticies">Whether duplicate verticies are to be preserved (Takes less time)</param>
			internal abstract void OptimizeObject(bool PreserveVerticies);
		}
	}
}
