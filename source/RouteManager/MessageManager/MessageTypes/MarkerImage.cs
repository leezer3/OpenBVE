using OpenBveApi.Textures;

namespace OpenBve.RouteManager
{
	/// <summary>Defines a legacy marker image, displayed between two points on a route</summary>
	public class MarkerImage : AbstractMessage
	{
		/// <summary>The texture to be displayed</summary>
		private readonly Texture Texture;

		public MarkerImage(Texture texture)
		{
			this.Texture = texture;
		}

		public override void AddMessage(double currentTime)
		{
			QueueForRemoval = false;
			LibRender.Renderer.AddMarker(this.Texture);
		}

		public override void Update()
		{
			if (QueueForRemoval == true)
			{
				LibRender.Renderer.RemoveMarker(this.Texture);
			}
		}
	}
}
