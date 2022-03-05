using System.IO;
using Path = OpenBveApi.Path;

namespace CarXmlConvertor
{
	class ConvertAts
	{
		internal static string DllPath(string trainFolder)
		{
			string config = Path.CombineFile(trainFolder, "ats.cfg");
			if (!File.Exists(config))
			{
				return string.Empty;
			}

			string Text = File.ReadAllText(config);
			Text = Text.Replace("\r", "").Replace("\n", "");
			if (Text.Length > 260)
			{
				/*
				 * String length is over max Windows path length, so
				 * comments or ATS plugin docs have been included in here
				 * e.g dlg70v40
				 */
				string[] fileLines = File.ReadAllLines(config);
				for (int i = 0; i < fileLines.Length; i++)
				{
					int commentStart = fileLines[i].IndexOf(';');
					if (commentStart != -1)
					{
						fileLines[i] = fileLines[i].Substring(0, commentStart);
					}

					fileLines[i] = fileLines[i].Trim();
					if (fileLines[i].Length != 0)
					{
						Text = fileLines[i];
						break;
					}
				}
			}

			string filePath = Path.CombineFile(trainFolder, Text);
			if (File.Exists(filePath))
			{
				return Text;
			}
			return string.Empty;
		}
		
	}
}
