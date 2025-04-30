using OpenBveApi.Math;

namespace Train.MsTs
{
	/// <summary>Describes the exhaust animation for a MSTS model</summary>
	internal struct Exhaust
	{
		/// <summary>The offset from the center of the model</summary>
		internal Vector3 Offset;
		/// <summary>The direction of the exhaust emissions</summary>
		internal Vector3 Direction;
		/// <summary>The size of the exhaust outlet (controls initial particle size)</summary>
		internal double Size;
		/// <summary>The maximum expanded size of a smoke particle</summary>
		internal double SmokeMaxMagnitude;
		/// <summary>The rate of particle emissions at idle</summary>
		internal double SmokeInitialRate;
		/// <summary>The rate of particle emissions at maximum power</summary>
		internal double SmokeMaxRate;
	}
}
