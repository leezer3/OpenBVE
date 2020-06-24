using OpenBveApi.Routes;

namespace Bve5RouteParser
{
	internal class Background
	{
		internal BackgroundHandle Handle;
		internal string Key;

		internal Background(string key, BackgroundHandle handle)
		{
			this.Key = key;
			this.Handle = handle;
		}
	}
}
