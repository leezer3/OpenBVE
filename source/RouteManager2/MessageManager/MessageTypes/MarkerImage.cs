﻿using OpenBveApi.Hosts;
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

		public MarkerImage(HostInterface host, Texture texture)
		{
			currentHost = host;
			this.texture = texture;
		}

		public override void AddMessage(double currentTime)
		{
			QueueForRemoval = false;

			currentHost.AddMarker(texture, Vector2.Null);
		}

		public override void Update()
		{
			if (QueueForRemoval)
			{
				currentHost.RemoveMarker(texture);
			}
		}
	}
}
