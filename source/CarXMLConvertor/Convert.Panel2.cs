using System;
using System.IO;
using System.Windows.Forms;
using OpenBveApi;
using Path = System.IO.Path;

namespace CarXmlConvertor
{
	class ConvertPanel2
	{
		private static MainForm mainForm;
		internal static string FileName;
        internal static void Process(MainForm form)
        {
	        if (!File.Exists(FileName))
	        {
		        return;
	        }
	        mainForm = form;
			string[] Lines = File.ReadAllLines(FileName);
	        TabbedList newLines = new TabbedList();
            for (int i = 0; i < Lines.Length; i++)
            {
                int j = Lines[i].IndexOf(';');
                if (j >= 0)
                {
                    Lines[i] = Lines[i].Substring(0, j).Trim(new char[] { });
                }
                else
                {
                    Lines[i] = Lines[i].Trim(new char[] { });
                }
            }
            if (Lines.Length < 1 || string.Compare(Lines[0], "version 1.0", StringComparison.OrdinalIgnoreCase) != 0)
            {
	            mainForm.updateLogBoxText += "Invalid panel2 format string " + Lines[0] + Environment.NewLine;
                MessageBox.Show("Invalid panel2.cfg format declaration.");
                return;
            }
            newLines.Add("<Panel>");
            string currentSection = string.Empty;
            for (int i = 0; i < Lines.Length; i++)
            {
				int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
				if (Lines[i].Length == 0)
				{
					continue;
				}
	            if (Lines[i][0] == '[' && Lines[i][Lines[i].Length - 1] == ']')
	            {
		            if (currentSection != string.Empty)
		            {
						newLines.Add("</"+currentSection + ">");
		            }
					currentSection = Lines[i].Substring(1, Lines[i].Length - 2);
					newLines.Add("<"+currentSection + ">");
	            }
				else if (Lines[i].IndexOf('=') != -1)
	            {
		            string a = Lines[i].Substring(0, j).TrimEnd();
		            string b = Lines[i].Substring(j + 1).TrimStart();
					newLines.Add("<" + a + ">" + b.Escape() + "</"+ a + ">");
	            }
            }
            newLines.Add("</"+currentSection + ">");
            newLines.Add("</Panel>");
            newLines.Add("</openBVE>");
            // ReSharper disable once AssignNullToNotNullAttribute
            string fileOut = Path.Combine(Path.GetDirectoryName(FileName), "panel.xml");
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
