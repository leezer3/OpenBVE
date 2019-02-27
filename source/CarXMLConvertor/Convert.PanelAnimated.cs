using System;
using System.IO;
using System.Windows.Forms;

namespace CarXmlConvertor
{
	class ConvertPanelAnimated
	{
		private static MainForm mainForm;
		internal static string FileName;

		internal static void Process(MainForm form)
		{
			if (!System.IO.File.Exists(FileName))
			{
				return;
			}

			mainForm = form;
			TabbedList newLines = new TabbedList();
			newLines.Add("<PanelAnimated>");
			newLines.Add("<Include>");
			newLines.Add("<FileName>panel.animated</FileName>");
			newLines.Add("</Include>");
			newLines.Add("</PanelAnimated>");
			newLines.Add("</openBVE>");
			string fileOut = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(FileName), "panel.xml");
			try
			{
                
				using (StreamWriter sw = new StreamWriter(fileOut))
				{
					foreach (String s in newLines.Lines)
						sw.WriteLine(s);
				}
			}
			catch
			{
				mainForm.updateLogBoxText += "Error writing file " + fileOut + Environment.NewLine;
				MessageBox.Show("An error occured whilst writing the new XML file. \r\n Please check for write permissions.", "CarXML Convertor", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
		}
	}
}
