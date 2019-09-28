namespace RouteManager2.Events
{
	public enum TransponderTypes
	{
		/// <summary>Marks the status of ATC.</summary>
		AtcTrackStatus = -16777215,
		
		/// <summary>Sets up an ATC speed limit.</summary>
		AtcSpeedLimit = -16777214,
		
		/// <summary>Sets up an ATS-P temporary speed limit.</summary>
		AtsPTemporarySpeedLimit = -16777213,
		
		/// <summary>Sets up an ATS-P permanent speed limit.</summary>
		AtsPPermanentSpeedLimit = -16777212,
		
		/// <summary>For internal use inside the CSV/RW parser only.</summary>
		InternalAtsPTemporarySpeedLimit = -16777201
	}
}
