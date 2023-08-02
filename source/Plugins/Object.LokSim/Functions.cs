using System;
using System.Text;
using OpenBveApi;

namespace Plugin
{
	internal static class Functions
	{
		/// <summary>Sanitizes a LokSim3D XML file so that the C# XML parser will read it correctly</summary>
		internal static string SanitizeXml(string s)
		{
			
			StringBuilder builder = new StringBuilder();
			builder.Append(s[0]);
			for (int i = 1; i < s.Length - 2; i++)
			{
				char next = s[i];
				char second = s[i + 1];
				char third = s[i + 2];
				char last = s[i - 1];
				if (next == '"' && second == '"' && last != '=')
				{
					/*
					 * LokSim allows the use of paired quotes within the FileInfo tag
					 * This is not valid XML, but some stuff (e.g. AndreasZ taurus) uses it
					 */
					if (third == '"')
					{
						// *stupid* variant- triple quotes at end of tag
						builder.Append('"');
					}
					i+= 2;
				}
				else
				{
					builder.Append(next);
				}
					
			}
			builder.Append(s[s.Length - 2]);
			builder.Append(s[s.Length - 1]);
			string sv = builder.ToString();
			return builder.ToString();

		}
	}
}
