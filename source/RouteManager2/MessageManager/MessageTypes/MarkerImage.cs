using LibRender2;
using OpenBveApi.Textures;

namespace RouteManager2.MessageManager.MessageTypes
{
	/// <summary>Defines a legacy marker image, displayed between two points on a route</summary>
	public class MarkerImage : AbstractMessage
	{
		private readonly BaseRenderer renderer;

		/// <summary>The texture to be displayed</summary>
		private readonly Texture texture;

		public MarkerImage(BaseRenderer Renderer, Texture Texture)
		{
			renderer = Renderer;
			texture = Texture;
		}

		public override void AddMessage(double currentTime)
		{
			QueueForRemoval = false;
			renderer.Marker.AddMarker(texture);
		}

		public override void Update()
		{
			if (QueueForRemoval)
			{
				renderer.Marker.RemoveMarker(texture);
			}
		}
	}
}
