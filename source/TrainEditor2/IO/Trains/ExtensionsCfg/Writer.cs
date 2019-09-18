using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TrainEditor2.Extensions;
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
				WriteCarNode(fileName, builder, i, train.Cars[i]);
			}

			for (int i = 0; i < train.Couplers.Count; i++)
			{
				WriteCouplerNode(fileName, builder, i, train.Couplers[i]);
			}

			File.WriteAllText(fileName, builder.ToString(), new UTF8Encoding(true));
		}

		private static void WriteCarNode(string fileName, StringBuilder builder, int carIndex, Car car)
		{
			builder.AppendLine($"[Car{carIndex.ToString(CultureInfo.InvariantCulture)}]");
			WriteKey(builder, "Object", Utilities.MakeRelativePath(fileName, car.Object));
			WriteKey(builder, "Length", car.Length);

			if (car.DefinedAxles)
			{
				WriteKey(builder, "Axles", car.RearAxle, car.FrontAxle);
			}

			WriteKey(builder, "Reversed", car.Reversed.ToString());
			WriteKey(builder, "LoadingSway", car.LoadingSway.ToString());
			WriteBogieNode(fileName, builder, carIndex * 2, car.FrontBogie);
			WriteBogieNode(fileName, builder, carIndex * 2 + 1, car.RearBogie);
		}

		private static void WriteBogieNode(string fileName, StringBuilder builder, int bogieIndex, Car.Bogie bogie)
		{
			builder.AppendLine($"[Bogie{bogieIndex.ToString(CultureInfo.InvariantCulture)}]");
			WriteKey(builder, "Object", Utilities.MakeRelativePath(fileName, bogie.Object));

			if (bogie.DefinedAxles)
			{
				WriteKey(builder, "Axles", bogie.RearAxle, bogie.FrontAxle);
			}

			WriteKey(builder, "Reversed", bogie.Reversed.ToString());
		}

		private static void WriteCouplerNode(string fileName, StringBuilder builder, int couplerIndex, Coupler coupler)
		{
			builder.AppendLine($"[Coupler{couplerIndex.ToString(CultureInfo.InvariantCulture)}]");
			WriteKey(builder, "Distances", coupler.Min, coupler.Max);
			WriteKey(builder, "Object", Utilities.MakeRelativePath(fileName, coupler.Object));
		}

		private static void WriteKey(StringBuilder builder, string key, params string[] values)
		{
			if (values.All(string.IsNullOrEmpty))
			{
				return;
			}

			builder.AppendLine($"{key} = {string.Join(", ", values)}");
		}

		private static void WriteKey(StringBuilder builder, string key, params double[] values)
		{
			WriteKey(builder, key, values.Select(v => v.ToString(CultureInfo.InvariantCulture)).ToArray());
		}
	}
}
