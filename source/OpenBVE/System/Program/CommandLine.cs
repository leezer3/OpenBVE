using System.Text;
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
					string key = Arguments[i].Substring(0, equals).Trim().ToLowerInvariant();
					string value = Arguments[i].Substring(equals + 1).Trim();
					switch (key)
					{
						case "/route":
							Result.RouteFile = value;
							switch (TextEncoding.GetEncodingFromFile(Result.RouteFile))
							{
								case TextEncoding.Encoding.Utf7:
									Result.RouteEncoding = System.Text.Encoding.UTF7;
									break;
								case TextEncoding.Encoding.Utf8:
									Result.RouteEncoding = System.Text.Encoding.UTF8;
									break;
								case TextEncoding.Encoding.Utf16Le:
									Result.RouteEncoding = System.Text.Encoding.Unicode;
									break;
								case TextEncoding.Encoding.Utf16Be:
									Result.RouteEncoding = System.Text.Encoding.BigEndianUnicode;
									break;
								case TextEncoding.Encoding.Utf32Le:
									Result.RouteEncoding = System.Text.Encoding.UTF32;
									break;
								case TextEncoding.Encoding.Utf32Be:
									Result.RouteEncoding = System.Text.Encoding.GetEncoding(12001);
									break;
								case TextEncoding.Encoding.Shift_JIS:
									Result.RouteEncoding = System.Text.Encoding.GetEncoding(932);
									break;
								case TextEncoding.Encoding.Windows1252:
									Result.RouteEncoding = System.Text.Encoding.GetEncoding(1252);
									break;
								case TextEncoding.Encoding.Big5:
									Result.RouteEncoding = System.Text.Encoding.GetEncoding(950);
									break;
								case TextEncoding.Encoding.EUC_KR:
									Result.RouteEncoding = System.Text.Encoding.GetEncoding(949);
									break;
								default:
									Result.RouteEncoding = Encoding.Default;
									break;
							}
							break;
						case "/train":
							Result.TrainFolder = value;
							switch (TextEncoding.GetEncodingFromFile(Result.TrainFolder, "train.txt"))
							{
								case TextEncoding.Encoding.Utf8:
									Result.TrainEncoding = System.Text.Encoding.UTF8;
									break;
								case TextEncoding.Encoding.Utf16Le:
									Result.TrainEncoding = System.Text.Encoding.Unicode;
									break;
								case TextEncoding.Encoding.Utf16Be:
									Result.TrainEncoding = System.Text.Encoding.BigEndianUnicode;
									break;
								case TextEncoding.Encoding.Utf32Le:
									Result.TrainEncoding = System.Text.Encoding.UTF32;
									break;
								case TextEncoding.Encoding.Utf32Be:
									Result.TrainEncoding = System.Text.Encoding.GetEncoding(12001);
									break;
								case TextEncoding.Encoding.Shift_JIS:
									Result.TrainEncoding = System.Text.Encoding.GetEncoding(932);
									break;
								case TextEncoding.Encoding.Windows1252:
									Result.TrainEncoding = System.Text.Encoding.GetEncoding(1252);
									break;
								case TextEncoding.Encoding.Big5:
									Result.TrainEncoding = System.Text.Encoding.GetEncoding(950);
									break;
								case TextEncoding.Encoding.EUC_KR:
									Result.TrainEncoding = System.Text.Encoding.GetEncoding(949);
									break;
								default:
									Result.TrainEncoding = Encoding.Default;
									break;
							}
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
					}
				}
			}
		}
	}
}