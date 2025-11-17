using OpenBveApi.Math;
using OpenBveApi.Routes;
using System.Collections.Generic;

namespace CsvRwRouteParser
{
	/// <summary>Describes the positioning of a rail within a block</summary>
	internal class Rail
	{
		/// <summary>Whether the rail is currently active</summary>
		internal bool RailStarted;
		/// <summary>Whether a .Rail or .RailStart command has been issued in the current block</summary>
		internal bool RailStartRefreshed;
		/// <summary>The position relative to the block of the .RailStart command</summary>
		internal Vector2 RailStart;
		/// <summary>Whether a .RailEnd command has been issued in the current block</summary>
		internal bool RailEnded;
		/// <summary>The position of the .RailEnd command relative to the block</summary>
		internal Vector2 RailEnd;
		/// <summary>The cant value</summary>
		internal double CurveCant;
		/// <summary>Whether the rail is driveable by the player</summary>
		internal bool IsDriveable;
		/// <summary>The accuracy level of the rail (affects cab sway) </summary>
		internal double Accuracy;
		/// <summary>The adhesion multiplier applying to the rail</summary>
		internal double AdhesionMultiplier;
		/// <summary>The starting track position</summary>
		internal double StartingTrackPosition;
		/// <summary>The power supplies available</summary>
		internal Dictionary<PowerSupplyTypes, PowerSupply> PowerSupplies;

		/// <summary>Gets the mid point of the rail</summary>
		internal Vector2 MidPoint => new Vector2(RailEnd - RailStart);

		internal Rail(double accuracy, double adhesionMultiplier)
		{
			Accuracy = accuracy;
			AdhesionMultiplier = adhesionMultiplier;
			PowerSupplies = new Dictionary<PowerSupplyTypes, PowerSupply>();
		}
	}
}
