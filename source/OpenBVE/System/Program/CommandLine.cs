using System.Text;
using OpenBveApi;
using OpenBveApi.Math;

namespace OpenBve
{
	/// <summary>Contains methods used to parse command-line arguments</summary>
	class CommandLine
	{
		/// <summary>Parses any command-line arguments passed to the main program</summary>
		/// <param name="Arguments">A string array of arguments</param>
		/// <param name="Result">The main dialog result (Used to launch)</param>
		internal static void ParseArguments(string[] Arguments, ref formMain.MainDialogResult Result)
		{
			if (Arguments.Length == 0)
			{
				return;
			}
			for (int i = 0; i < Arguments.Length; i++)
			{
				int equals = Arguments[i].IndexOf('=');
				if (equals >= 0)
				{
					string key = Arguments[i].Substring(0, equals).Trim(new char[] { }).ToLowerInvariant();
					string value = Arguments[i].Substring(equals + 1).Trim(new char[] { });
					switch (key)
					{
						case "/route":
							Result.RouteFile = value;
							Result.RouteEncoding = TextEncoding.GetSystemEncodingFromFile(Result.RouteFile);
							break;
						case "/train":
							Result.TrainFolder = value;
							Result.TrainEncoding = TextEncoding.GetSystemEncodingFromFile(Result.TrainFolder, "train.txt");
							break;
						case "/station":
							Result.InitialStation = value;
							break;
						case "/time":
							Interface.TryParseTime(value, out Result.StartTime);
							break;
						case "/ai":
							if (value.ToLowerInvariant() == "true" || value.ToLowerInvariant() == "1")
							{
								Result.AIDriver = true;
							}
							break;
						case "/fullscreen":
							if (value.ToLowerInvariant() == "true" || value.ToLowerInvariant() == "1")
							{
								Result.FullScreen = true;
							}
							break;
						case "/width":
							NumberFormats.TryParseIntVb6(value, out Result.Width);
							break;
						case "/height":
							NumberFormats.TryParseIntVb6(value, out Result.Height);
							break;
						case "/glmenu":
							if (value.ToLowerInvariant() == "true" || value.ToLowerInvariant() == "1")
							{
								Result.ExperimentalGLMenu = true;
							}
							break;
					}
				}
			}
		}
	}
}
