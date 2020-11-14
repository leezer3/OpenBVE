using OpenBveApi.Textures;

namespace MechanikRouteParser
{
	internal struct MechanikTexture
	{
		internal string Path;
		internal int Index;
		internal Texture Texture;
		internal double Width;
		internal double Height;
		internal MechanikTexture(string p, string s, int i)
		{
			Path = p;
			Index = i;
			Plugin.CurrentHost.LoadTexture(p, new TextureParameters(null, null), out Texture);
			this.Width = Texture.Width / 200.0;
			this.Height = Texture.Height / 200.0;
		}
	}
}
