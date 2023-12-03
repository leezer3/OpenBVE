using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Plugin
{
	internal static class Functions
	{
		/// <summary>Sanitizes and loads a LokSim3D XML file</summary>
		/// <param name="currentXML">The XmlDocument to load into</param>
		/// <param name="fileName">The filename</param>
		internal static void SanitizeAndLoadXml(this XmlDocument currentXML, string fileName)
		{
			string s = File.ReadAllText(fileName);
			string sanitized = SanitizeXml(s);
			currentXML.LoadXml(sanitized);
			XmlDeclaration declaration = currentXML.ChildNodes.OfType<XmlDeclaration>().FirstOrDefault();
			if (declaration != null)
			{
				/*
				 * Yuck: Handle stuff not encoded in UTF-8 by re-reading with correct encoding
				 *		 Unfortunately, some stuff uses umlauts in filenames....
				 *       Non-standard XML means that 
				 */
				Encoding e = Encoding.GetEncoding(declaration.Encoding);
				if (!e.Equals(Encoding.UTF8))
				{
					s = File.ReadAllText(fileName, e);
					sanitized = SanitizeXml(s);
					currentXML.LoadXml(sanitized);
				}
			}
		}

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
			return builder.ToString();
		}
	}
}
