using OpenBveApi.Routes;

namespace Bve5RouteParser
{
	internal class Background
	{
		internal readonly BackgroundHandle Handle;
		internal readonly string Key;

		internal Background(string key, BackgroundHandle handle)
		{
			this.Key = key;
			this.Handle = handle;
		}
	}
}
