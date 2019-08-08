using OpenBve.RouteManager;
using OpenBveApi.Routes;
using OpenBveApi.Trains;

namespace OpenBve
{
	internal static partial class TrackManager
	{
		/// <summary>Defines fixed transponder types used by the built-in safety systems</summary>
		internal static class SpecialTransponderTypes
		{
			/// <summary>Marks the status of ATC.</summary>
			internal const int AtcTrackStatus = -16777215;
			/// <summary>Sets up an ATC speed limit.</summary>
			internal const int AtcSpeedLimit = -16777214;
			/// <summary>Sets up an ATS-P temporary speed limit.</summary>
			internal const int AtsPTemporarySpeedLimit = -16777213;
			/// <summary>Sets up an ATS-P permanent speed limit.</summary>
			internal const int AtsPPermanentSpeedLimit = -16777212;
			/// <summary>For internal use inside the CSV/RW parser only.</summary>
			internal const int InternalAtsPTemporarySpeedLimit = -16777201;
		}

		
	}
}
