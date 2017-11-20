using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace OpenBve
{
	partial class Bve5TrainParser
	{
		private static void ParseAmmeter(ref TrainManager.Train Train)
		{
			Train.Ammeter = new Ammeter(Train);
			string[] Lines;
			//Parse the power data first
			if(!string.IsNullOrEmpty(TrainData.PowerData.NoLoadCurrentFile))
			{
				string fileFormat = File.ReadLines(TrainData.PowerData.NoLoadCurrentFile).First();
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
				Lines = File.ReadAllLines(TrainData.PowerData.NoLoadCurrentFile, e);
				List<AmmeterTable> currentTable = new List<AmmeterTable>();
				for (int i = 1; i < Lines.Length; i++)
				{
					string line = Lines[i];
					int hash = line.IndexOf('#');
					if (hash > 0)
					{
						line = line.Substring(0, hash).Trim();
					}
					else if (hash == 0)
					{
						continue;
					}
					else
					{
						line = line.Trim();
						if (line.Length == 0)
						{
							continue;
						}
					}
					string[] splitLine = line.Split(',');
					double speed;
					if (!double.TryParse(splitLine[0], out speed))
					{
						//May well actually be limited to integers, but using a double here won't hurt.....
						Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid speed encountered in BVE5 Power NoCurrent file at Line " + i);
						continue;
					}
					double[] currentValues = new double[splitLine.Length - 1];
					for (int j = 1; j < splitLine.Length; j++)
					{
						if(!double.TryParse(splitLine[j].Trim(), out currentValues[j - 1]))
						{
							currentValues[j - 1] = 0;
							Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid ammeter current value encountered in BVE5 Power NoCurrent file at Line " + i);
						}
					}
					AmmeterTable Table = new AmmeterTable(speed, currentValues);
					currentTable.Add(Table);
				}
				Train.Ammeter.noCurrentTable = currentTable.ToArray();
			}
			else
			{
				AmmeterTable[] Table = new AmmeterTable[] { new AmmeterTable(0, new double[] { 0.0} ) };
				Train.Ammeter.noCurrentTable = Table;
			}

			if (!string.IsNullOrEmpty(TrainData.PowerData.CurrentFile))
			{
				string fileFormat = File.ReadLines(TrainData.PowerData.CurrentFile).First();
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
				Lines = File.ReadAllLines(TrainData.PowerData.CurrentFile, e);
				List<AmmeterTable> currentTable = new List<AmmeterTable>();
				for (int i = 1; i < Lines.Length; i++)
				{
					string line = Lines[i];
					int hash = line.IndexOf('#');
					if (hash > 0)
					{
						line = line.Substring(0, hash).Trim();
					}
					else if (hash == 0)
					{
						continue;
					}
					else
					{
						line = line.Trim();
						if (line.Length == 0)
						{
							continue;
						}
					}
					string[] splitLine = line.Split(',');
					double speed;
					if (!double.TryParse(splitLine[0], out speed))
					{
						//May well actually be limited to integers, but using a double here won't hurt.....
						Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid speed encountered in BVE5 Power Current file at Line " + i);
						continue;
					}
					double[] currentValues = new double[splitLine.Length - 1];
					for (int j = 1; j < splitLine.Length; j++)
					{
						if (!double.TryParse(splitLine[j].Trim(), out currentValues[j - 1]))
						{
							currentValues[j - 1] = 0;
							Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid ammeter current value encountered in BVE5 Power Current file at Line " + i);
						}
					}
					AmmeterTable Table = new AmmeterTable(speed, currentValues);
					currentTable.Add(Table);
				}
				Train.Ammeter.noLoadTable = currentTable.ToArray();
			}
			else
			{
				AmmeterTable[] Table = new AmmeterTable[] { new AmmeterTable(0, new double[] { 0.0 }) };
				Train.Ammeter.noLoadTable = Table;
			}

			if (!string.IsNullOrEmpty(TrainData.PowerData.MaxCurrentFile))
			{
				string fileFormat = File.ReadLines(TrainData.PowerData.MaxCurrentFile).First();
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
				Lines = File.ReadAllLines(TrainData.PowerData.MaxCurrentFile, e);
				List<AmmeterTable> currentTable = new List<AmmeterTable>();
				for (int i = 1; i < Lines.Length; i++)
				{
					string line = Lines[i];
					int hash = line.IndexOf('#');
					if (hash > 0)
					{
						line = line.Substring(0, hash).Trim();
					}
					else if (hash == 0)
					{
						continue;
					}
					else
					{
						line = line.Trim();
						if (line.Length == 0)
						{
							continue;
						}
					}
					string[] splitLine = line.Split(',');
					double speed;
					if (!double.TryParse(splitLine[0], out speed))
					{
						//May well actually be limited to integers, but using a double here won't hurt.....
						Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid speed encountered in BVE5 Power MaxCurrent file at Line " + i);
						continue;
					}
					double[] currentValues = new double[splitLine.Length - 1];
					for (int j = 1; j < splitLine.Length; j++)
					{
						if (!double.TryParse(splitLine[j].Trim(), out currentValues[j - 1]))
						{
							currentValues[j - 1] = 0;
							Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid ammeter current value encountered in BVE5 Power MaxCurrent file at Line " + i);
						}
					}
					AmmeterTable Table = new AmmeterTable(speed, currentValues);
					currentTable.Add(Table);
				}
				Train.Ammeter.maxLoadTable = currentTable.ToArray();
			}
			else
			{
				AmmeterTable[] Table = new AmmeterTable[] { new AmmeterTable(0, new double[] { 0.0 }) };
				Train.Ammeter.maxLoadTable = Table;
			}
		}
	}
}
