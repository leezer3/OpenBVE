using System;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenBve
{
	partial class Bve5TrainParser
	{
		private static void ParseNotchParameters(ref TrainManager.Train Train, bool Power)
		{
			string FileName;
			if (Power)
			{
				FileName = TrainData.PowerData.ParametersFile;
			}
			else
			{
				FileName = TrainData.BrakeData.ParametersFile;
			}
			string fileFormat = File.ReadLines(FileName).First();
			string[] splitFormat = fileFormat.Split(':');
			if (!string.Equals(splitFormat[0], "BveTs Vehicle Performance Table 2.00", StringComparison.InvariantCultureIgnoreCase))
			{
				throw new Exception("Invalid BVE5 vehicle performance table format: " + splitFormat[0]);
			}
			System.Text.Encoding e = Encoding.UTF8;
			if (splitFormat.Length >= 2)
			{
				/*
				 * Pull out the text encoding of our file
				 */
				e = TextEncoding.ParseEncoding(splitFormat[1]);
			}
			string[] Lines = File.ReadAllLines(FileName, e);

			double[] JerkUp = new[] { 10.0 };
			double[] JerkDown = new[] { 10.0 };
			double[] DelayUp = new[] { 10.0 };
			double[] DelayDown = new[] { 10.0 };

			for (int i = 1; i < Lines.Length; i++)
			{
				string line = Lines[i];

				int hash = line.IndexOf('#');
				if (hash >= 0)
				{
					line = line.Substring(0, hash).Trim();
				}
				else
				{
					line = line.Trim();
				}
				string[] splitLine = line.Split(',');
				switch (splitLine[0].ToLowerInvariant())
				{
					case "breakerdelayon":
						break;
					case "breakerdelayoff":
						break;
					case "currentreducingtime":
						break;
					/* 
					 * Convertor produces a straight copy of the jerk values (1/100 m/s³), although docs describe them as in A (amps??)
					 * Probably reasonable to just use them as is for the minute
					 * 
					 */
					case "jerkregulation_up":
						JerkUp = new double[splitLine.Length - 1];
						for (int j = 1; j < splitLine.Length; j++)
						{
							if(!Double.TryParse(splitLine[j], out JerkUp[j -1]))
							{
								Interface.AddMessage(Interface.MessageType.Error, false, "The JerkUp value for Notch " + j + " of " + splitLine[j] + " is invalid.");
								JerkUp[j - 1] = 10.0;
							}
							else
							{
								JerkUp[j - 1] *= 0.1;
							}
						}
						break;
					case "jerkregulation_down":
						JerkDown = new double[splitLine.Length - 1];
						for (int j = 1; j < splitLine.Length; j++)
						{
							if (!Double.TryParse(splitLine[j], out JerkDown[j - 1]))
							{
								Interface.AddMessage(Interface.MessageType.Error, false, "The JerkDown value for Notch " + j + " of " + splitLine[j] + " is invalid.");
								JerkDown[j - 1] = 10.0;
							}
							else
							{
								JerkDown[j - 1] *= 0.1;
							}
						}
						break;
					case "requirednotch_up":
						break;
					case "requirednotch_down":
						break;
					case "stepdelay_up":
						DelayUp = new double[splitLine.Length - 1];
						for (int j = 1; j < splitLine.Length; j++)
						{
							if (!Double.TryParse(splitLine[j], out DelayUp[j - 1]))
							{
								Interface.AddMessage(Interface.MessageType.Error, false, "The DelayUp value for Notch " + j + " of " + splitLine[j] + " is invalid.");
								DelayUp[j - 1] = 0.0;
							}
						}
						break;
					case "stepdelay_down":
						DelayDown = new double[splitLine.Length - 1];
						for (int j = 1; j < splitLine.Length; j++)
						{
							if (!Double.TryParse(splitLine[j], out DelayDown[j - 1]))
							{
								Interface.AddMessage(Interface.MessageType.Error, false, "The DelayDown value for Notch " + j + " of " + splitLine[j] + " is invalid.");
								DelayDown[j - 1] = 0.0;
							}
						}
						break;
					case "currentlimitingvalue_empty":
						break;
					case "currentlimitingvalue_full":
						break;
					case "resettime":
						break;
				}
			}
			if (Power)
			{
				Train.Specs.DelayPowerUp = DelayUp;
				Train.Specs.DelayPowerDown = DelayDown;
				for (int i = 0; i < Train.Cars.Length; i++)
				{
					Train.Cars[i].Specs.JerkPowerUp = JerkUp;
					Train.Cars[i].Specs.JerkPowerDown = JerkDown;
				}
			}
			else
			{
				Train.Specs.DelayBrakeUp = DelayUp;
				Train.Specs.DelayBrakeDown = DelayDown;
				for (int i = 0; i < Train.Cars.Length; i++)
				{
					Train.Cars[i].Specs.JerkBrakeUp = JerkUp;
					Train.Cars[i].Specs.JerkBrakeDown = JerkDown;
				}
			}
		}
	}
}
