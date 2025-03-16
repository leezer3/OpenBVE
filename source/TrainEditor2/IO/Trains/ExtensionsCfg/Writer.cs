using System.IO;
using System.Text;
using TrainEditor2.Models.Trains;

namespace TrainEditor2.IO.Trains.ExtensionsCfg
{
	internal static partial class ExtensionsCfg
	{
		internal static void Write(string fileName, Train train)
		{
			StringBuilder builder = new StringBuilder();

			for (int i = 0; i < train.Cars.Count; i++)
			{
				train.Cars[i].WriteExtensionsCfg(fileName, builder, i);
			}

			for (int i = 0; i < train.Couplers.Count; i++)
			{
				train.Couplers[i].WriteExtensionsCfg(fileName, builder, i);
			}

			File.WriteAllText(fileName, builder.ToString(), new UTF8Encoding(true));
		}
	}
}
