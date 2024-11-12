using OpenBveApi;
using OpenBveApi.Math;

namespace OpenBve
{
	/// <summary>Contains methods used to parse command-line arguments</summary>
	class CommandLine
	{
		/// <summary>Parses any command-line arguments passed to the main program</summary>
		/// <param name="arguments">A string array of arguments</param>
		internal static LaunchParameters ParseArguments(string[] arguments)
		{
			LaunchParameters result = new LaunchParameters();
			if (arguments.Length == 0)
			{
				return result;
			}
			for (int i = 0; i < arguments.Length; i++)
			{
				int equals = arguments[i].IndexOf('=');
				if (equals >= 0)
				{
					string key = arguments[i].Substring(0, equals).Trim(new char[] { }).ToLowerInvariant();
					string value = arguments[i].Substring(equals + 1).Trim(new char[] { });
					switch (key)
					{
						case "/route":
							result.RouteFile = value;
							result.RouteEncoding = TextEncoding.GetSystemEncodingFromFile(result.RouteFile);
							break;
						case "/train":
							result.TrainFolder = value;
							result.TrainEncoding = TextEncoding.GetSystemEncodingFromFile(result.TrainFolder, "train.txt");
							break;
						case "/station":
							result.InitialStation = value;
							break;
						case "/time":
							Interface.TryParseTime(value, out result.StartTime);
							break;
						case "/ai":
							if (value.ToLowerInvariant() == "true" || value.ToLowerInvariant() == "1")
							{
								result.AIDriver = true;
							}
							break;
						case "/fullscreen":
							if (value.ToLowerInvariant() == "true" || value.ToLowerInvariant() == "1")
							{
								result.FullScreen = true;
							}
							break;
						case "/width":
							NumberFormats.TryParseIntVb6(value, out result.Width);
							break;
						case "/height":
							NumberFormats.TryParseIntVb6(value, out result.Height);
							break;
						case "/glmenu":
							if (value.ToLowerInvariant() == "true" || value.ToLowerInvariant() == "1")
							{
								result.ExperimentalGLMenu = true;
							}
							break;
					}
				}
			}
			return result;
		}
	}
}
