using OpenBveApi.Hosts;
using OpenBveApi.Math;
using OpenBveApi.Textures;

namespace RouteManager2.MessageManager.MessageTypes
{
	/// <summary>Defines a legacy marker image, displayed between two points on a route</summary>
	public class MarkerImage : AbstractMessage
	{
		private readonly HostInterface currentHost;

		/// <summary>The texture to be displayed</summary>
		private readonly Texture texture;

		public MarkerImage(HostInterface Host, Texture Texture)
		{
			currentHost = Host;
			texture = Texture;
		}

		public override void AddMessage(double currentTime)
		{
			QueueForRemoval = false;

			currentHost.AddMarker(texture, Vector2.Null);
		}

		public override void Update(double timeElapsed)
		{
			Timeout -= timeElapsed;
			if (QueueForRemoval)
			{
				currentHost.RemoveMarker(texture);
			}
		}
	}
}
