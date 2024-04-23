namespace Bve5RouteParser
{
	/// <summary>Defines the different types of base transform which may be applied to an object</summary>
	/// <remarks>Rotation is applied separately to the result of the base transform</remarks>
	internal enum RailTransformationTypes
	{
		/// <summary>Flat within the world</summary>
		Flat = 0,
		/// <summary>Follows the pitch of it's rail</summary>
		FollowsPitch = 1,
		/// <summary>Follows the cant of it's rail</summary>
		FollowsCant = 2,
		/// <summary>Follows both the pitch and cant of it's rail</summary>
		FollowsBoth = 3,
		/// <summary>Follows the pitch and cant of rail 0</summary>
		FollowsFirstRail = 4

	}
}
