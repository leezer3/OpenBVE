// ReSharper disable UnusedMember.Global
namespace OpenBveApi.Routes
{
	/// <summary>The default direction of travel on a track</summary>
	public enum TrackDirection
	{
		/// <summary>The track position counts downwards towards zero</summary>
		Reverse = -1,
		/// <summary>Unknown</summary>
		Unknown = 0,
		/// <summary>The track position counts up from zero</summary>
		Forwards = 1
	}
}
