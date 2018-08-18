namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		/// <summary>Defines the differing types of readhesion device which may be fitted to a car</summary>
		/// <remarks>See <see cref="http://openbve-project.net/documentation/HTML/train_train_dat.html"/> for further details</remarks>
		internal enum ReadhesionDeviceType
		{
			/// <summary>No readhesion device is fitted</summary>
			NotFitted = -1,
			/// <summary>Cuts off power instantly and rebuilds it up fast in steps.</summary>
			TypeA = 0,
			/// <summary>Updates not so often and adapts slowly. Wheel slip can persist longer and power is regained slower. The behavior is smoother.</summary>
			TypeB = 1,
			/// <summary>The behavior is somewhere in-between type B and type D.</summary>
			TypeC = 2,
			/// <summary>Updates fast and adapts fast. Wheel slip only occurs briefly and power is regained fast. The behavior is more abrupt.</summary>
			TypeD = 3,
		}
	}
}
