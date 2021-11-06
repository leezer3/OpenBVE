namespace OpenBveApi.Textures
{
	/// <summary>Differing methods of fitting an image to a surface size</summary>
	public enum ImageSizeMode
	{
		/// <summary>The texture is placed at the top left corner, and is clipped if larger than the surface</summary>
		Normal = 0,
		/// <summary>The texture is centered on the surface</summary>
		Center = 1,
		/// <summary>The texture is stretched or shrunk to fit the surface</summary>
		Stretch = 2,
		/// <summary>The texture is stretched or shrunk to fit the surface, maintaining it's aspect ratio</summary>
		Zoom = 3
	}
}
