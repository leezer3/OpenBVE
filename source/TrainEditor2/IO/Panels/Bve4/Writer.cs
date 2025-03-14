using System.IO;
using System.Text;
using TrainEditor2.Models.Panels;

namespace TrainEditor2.IO.Panels.Bve4
{
	internal static partial class PanelCfgBve4
	{
		internal static void Write(string fileName, Panel panel)
		{
			StringBuilder builder = new StringBuilder();

			panel.This.WriteCfg(fileName, builder);

			foreach (PanelElement element in panel.PanelElements)
			{
				element.WriteCfg(fileName, builder);
			}

			File.WriteAllText(fileName, builder.ToString(), new UTF8Encoding(true));
		}
	}
}
