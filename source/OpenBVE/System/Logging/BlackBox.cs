using System;
using System.Globalization;
using System.IO;
using OpenBveApi;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;

namespace OpenBve
{
	internal static partial class Interface
	{
		/// <summary>Magic bytes identifying this as an OpenBVE blackbox file</summary>
		/// <remarks>openBVELOGS in UTF-8</remarks>
		private static readonly byte[] identifier = { 111, 112, 101, 110, 66, 86, 69, 95, 76, 79, 71, 83 };
		/// <summary>Magic bytes identifying the EOF</summary>
		/// <remarks>_fileEND in UTF-8</remarks>
		private static readonly byte[] footer = { 95, 102, 105, 108, 101, 69, 78, 68 };

		/// <summary>Loads the black-box logs from the previous simulation run</summary>
		internal static void LoadLogs()
		{
			string blackBoxFile = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "logs.bin");
			if (File.Exists(blackBoxFile))
			{
				try
				{
					using (FileStream stream = new FileStream(blackBoxFile, FileMode.Open, FileAccess.Read))
					{
						using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.UTF8))
						{
							const short version = 1;
							byte[] data = reader.ReadBytes(identifier.Length);
							for (int i = 0; i < identifier.Length; i++)
							{
								if (identifier[i] != data[i]) throw new InvalidDataException();
							}

							short number = reader.ReadInt16();
							if (version != number) throw new InvalidDataException();
							Game.LogRouteName = reader.ReadString();
							Game.LogTrainName = reader.ReadString();
							Game.LogDateTime = DateTime.FromBinary(reader.ReadInt64());
							CurrentOptions.PreviousGameMode = (GameMode) reader.ReadInt16();
							Game.BlackBoxEntryCount = reader.ReadInt32();
							Game.BlackBoxEntries = new Game.BlackBoxEntry[Game.BlackBoxEntryCount];
							for (int i = 0; i < Game.BlackBoxEntryCount; i++)
							{
								Game.BlackBoxEntries[i].Time = reader.ReadDouble();
								Game.BlackBoxEntries[i].Position = reader.ReadDouble();
								Game.BlackBoxEntries[i].Speed = reader.ReadSingle();
								Game.BlackBoxEntries[i].Acceleration = reader.ReadSingle();
								Game.BlackBoxEntries[i].ReverserDriver = reader.ReadInt16();
								Game.BlackBoxEntries[i].ReverserSafety = reader.ReadInt16();
								Game.BlackBoxEntries[i].PowerDriver = (Game.BlackBoxPower) reader.ReadInt16();
								Game.BlackBoxEntries[i].PowerSafety = (Game.BlackBoxPower) reader.ReadInt16();
								Game.BlackBoxEntries[i].BrakeDriver = (Game.BlackBoxBrake) reader.ReadInt16();
								Game.BlackBoxEntries[i].BrakeSafety = (Game.BlackBoxBrake) reader.ReadInt16();
								Game.BlackBoxEntries[i].EventToken = (Game.BlackBoxEventToken) reader.ReadInt16();
							}

							Game.ScoreLogCount = reader.ReadInt32();
							Game.ScoreLogs = new Game.ScoreLog[Game.ScoreLogCount];
							Game.CurrentScore.CurrentValue = 0;
							for (int i = 0; i < Game.ScoreLogCount; i++)
							{
								Game.ScoreLogs[i].Time = reader.ReadDouble();
								Game.ScoreLogs[i].Position = reader.ReadDouble();
								Game.ScoreLogs[i].Value = reader.ReadInt32();
								Game.ScoreLogs[i].TextToken = (Game.ScoreTextToken) reader.ReadInt16();
								Game.CurrentScore.CurrentValue += Game.ScoreLogs[i].Value;
							}

							Game.CurrentScore.Maximum = reader.ReadInt32();
							data = reader.ReadBytes(footer.Length);
							for (int i = 0; i < footer.Length; i++)
							{
								if (footer[i] != data[i]) throw new InvalidDataException();
							}

							reader.Close();
						}

						stream.Close();
					}
					return;
				}
				catch
				{
					//Broken black box data so just discard....
				}
			}
			Game.LogRouteName = "";
			Game.LogTrainName = "";
			Game.LogDateTime = DateTime.Now;
			Game.BlackBoxEntries = new Game.BlackBoxEntry[256];
			Game.BlackBoxEntryCount = 0;
			Game.ScoreLogs = new Game.ScoreLog[64];
			Game.ScoreLogCount = 0;
		}


		private static double lastLogSaveTime = 0;

		/// <summary>Saves the current in-game black box log</summary>
		internal static void SaveLogs(bool forceSave = false)
		{
			if (CurrentOptions.BlackBox == false)
			{
				return;
			}

			if (Program.CurrentRoute.SecondsSinceMidnight - lastLogSaveTime < 30 && !forceSave)
			{
				//TODO: This now only recreates the black-box log every ~30s
				//Still shitty code, but still....
				return;
			}
			lastLogSaveTime = Program.CurrentRoute.SecondsSinceMidnight;
			string blackBoxFile = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "logs.bin");
			try
			{
				using (FileStream stream = new FileStream(blackBoxFile, FileMode.Create, FileAccess.Write))
				{
					using (BinaryWriter writer = new BinaryWriter(stream, System.Text.Encoding.UTF8))
					{
						const short version = 1;
						writer.Write(identifier);
						writer.Write(version);
						writer.Write(Game.LogRouteName);
						writer.Write(Game.LogTrainName);
						writer.Write(Game.LogDateTime.ToBinary());
						writer.Write((short) Interface.CurrentOptions.GameMode);
						writer.Write(Game.BlackBoxEntryCount);
						for (int i = 0; i < Game.BlackBoxEntryCount; i++)
						{
							writer.Write(Game.BlackBoxEntries[i].Time);
							writer.Write(Game.BlackBoxEntries[i].Position);
							writer.Write(Game.BlackBoxEntries[i].Speed);
							writer.Write(Game.BlackBoxEntries[i].Acceleration);
							writer.Write(Game.BlackBoxEntries[i].ReverserDriver);
							writer.Write(Game.BlackBoxEntries[i].ReverserSafety);
							writer.Write((short) Game.BlackBoxEntries[i].PowerDriver);
							writer.Write((short) Game.BlackBoxEntries[i].PowerSafety);
							writer.Write((short) Game.BlackBoxEntries[i].BrakeDriver);
							writer.Write((short) Game.BlackBoxEntries[i].BrakeSafety);
							writer.Write((short) Game.BlackBoxEntries[i].EventToken);
						}

						writer.Write(Game.ScoreLogCount);
						for (int i = 0; i < Game.ScoreLogCount; i++)
						{
							writer.Write(Game.ScoreLogs[i].Time);
							writer.Write(Game.ScoreLogs[i].Position);
							writer.Write(Game.ScoreLogs[i].Value);
							writer.Write((short) Game.ScoreLogs[i].TextToken);
						}

						writer.Write(Game.CurrentScore.Maximum);
						writer.Write(footer);
						writer.Close();
					}

					stream.Close();
				}
			}
			catch
			{
				Interface.CurrentOptions.BlackBox = false;
				Interface.AddMessage(MessageType.Error, false, "An unexpected error occurred whilst attempting to write to the black box log- Black box has been disabled.");
			}
		}

		/// <summary>The available formats for exporting black box data</summary>
		internal enum BlackBoxFormat
		{
			CommaSeparatedValue = 0,
			FormattedText = 1
		}
		/// <summary>Gets the formatted output text for a black box event token</summary>
		/// <param name="eventToken">The event token for which to get the text</param>
		internal static string GetBlackBoxText(Game.BlackBoxEventToken eventToken)
		{
			//TODO: Only returns a blank string, what was intended here???
			switch (eventToken)
			{
				default: return "";
			}
		}

		/// <summary>Exports the current black box data to a file</summary>
		/// <param name="file">The file to write</param>
		/// <param name="format">The format in which to write the data</param>
		internal static void ExportBlackBox(string file, BlackBoxFormat format)
		{
			switch (format)
			{
				// comma separated value
				case BlackBoxFormat.CommaSeparatedValue:
					{
						CultureInfo culture = CultureInfo.InvariantCulture;
						System.Text.StringBuilder builder = new System.Text.StringBuilder();
						for (int i = 0; i < Game.BlackBoxEntryCount; i++)
						{
							builder.Append(Game.BlackBoxEntries[i].Time.ToString(culture) + ",");
							builder.Append(Game.BlackBoxEntries[i].Position.ToString(culture) + ",");
							builder.Append(Game.BlackBoxEntries[i].Speed.ToString(culture) + ",");
							builder.Append(Game.BlackBoxEntries[i].Acceleration.ToString(culture) + ",");
							builder.Append((Game.BlackBoxEntries[i].ReverserDriver).ToString(culture) + ",");
							builder.Append((Game.BlackBoxEntries[i].ReverserSafety).ToString(culture) + ",");
							builder.Append(((short)Game.BlackBoxEntries[i].PowerDriver).ToString(culture) + ",");
							builder.Append(((short)Game.BlackBoxEntries[i].PowerSafety).ToString(culture) + ",");
							builder.Append(((short)Game.BlackBoxEntries[i].BrakeDriver).ToString(culture) + ",");
							builder.Append(((short)Game.BlackBoxEntries[i].BrakeSafety).ToString(culture) + ",");
							builder.Append(((short)Game.BlackBoxEntries[i].EventToken).ToString(culture));
							builder.Append("\r\n");
						}
						File.WriteAllText(file, builder.ToString(), new System.Text.UTF8Encoding(true));
					} break;
				// formatted text
				case BlackBoxFormat.FormattedText:
					{
						CultureInfo culture = CultureInfo.InvariantCulture;
						System.Text.StringBuilder builder = new System.Text.StringBuilder();
						string[][] lines = new string[Game.BlackBoxEntryCount + 1][];
						lines[0] = new[] {
							Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","time"}),
							Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","position"}),
							Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","speed"}),
							Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","acceleration"}),
							Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","reverser"}),
							Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","power"}),
							Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","brake"}),
							Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","event"}),
						};
						int columns = lines[0].Length;
						for (int i = 0; i < Game.BlackBoxEntryCount; i++)
						{
							int j = i + 1;
							lines[j] = new string[columns];
							{
								double x = Game.BlackBoxEntries[i].Time;
								int h = (int)Math.Floor(x / 3600.0);
								x -= h * 3600.0;
								int m = (int)Math.Floor(x / 60.0);
								x -= m * 60.0;
								int s = (int)Math.Floor(x);
								x -= s;
								int n = (int)Math.Floor(1000.0 * x);
								lines[j][0] = h.ToString("00", culture) + ":" + m.ToString("00", culture) + ":" + s.ToString("00", culture) + ":" + n.ToString("000", culture);
							}
							lines[j][1] = Game.BlackBoxEntries[i].Position.ToString("0.000", culture);
							lines[j][2] = Game.BlackBoxEntries[i].Speed.ToString("0.0000", culture);
							lines[j][3] = Game.BlackBoxEntries[i].Acceleration.ToString("0.0000", culture);
							{
								string[] reverser = new string[2];
								for (int k = 0; k < 2; k++)
								{
									short r = k == 0 ? Game.BlackBoxEntries[i].ReverserDriver : Game.BlackBoxEntries[i].ReverserSafety;
									switch (r)
									{
										case -1:
											reverser[k] = Translations.QuickReferences.HandleBackward;
											break;
										case 0:
											reverser[k] = Translations.QuickReferences.HandleNeutral;
											break;
										case 1:
											reverser[k] = Translations.QuickReferences.HandleForward;
											break;
										default:
											reverser[k] = r.ToString(culture);
											break;
									}
								}
								lines[j][4] = reverser[0] + " → " + reverser[1];
							}
							{
								string[] power = new string[2];
								for (int k = 0; k < 2; k++)
								{
									Game.BlackBoxPower p = k == 0 ? Game.BlackBoxEntries[i].PowerDriver : Game.BlackBoxEntries[i].PowerSafety;
									switch (p)
									{
										case Game.BlackBoxPower.PowerNull:
											power[k] = Translations.QuickReferences.HandlePowerNull;
											break;
										default:
											power[k] = Translations.QuickReferences.HandlePower + ((short)p).ToString(culture);
											break;
									}
								}
								lines[j][5] = power[0] + " → " + power[1];
							}
							{
								string[] brake = new string[2];
								for (int k = 0; k < 2; k++)
								{
									Game.BlackBoxBrake b = k == 0 ? Game.BlackBoxEntries[i].BrakeDriver : Game.BlackBoxEntries[i].BrakeSafety;
									switch (b)
									{
										case Game.BlackBoxBrake.BrakeNull:
											brake[k] = Translations.QuickReferences.HandleBrakeNull;
											break;
										case Game.BlackBoxBrake.Emergency:
											brake[k] = Translations.QuickReferences.HandleEmergency;
											break;
										case Game.BlackBoxBrake.HoldBrake:
											brake[k] = Translations.QuickReferences.HandleHoldBrake;
											break;
										case Game.BlackBoxBrake.Release:
											brake[k] = Translations.QuickReferences.HandleRelease;
											break;
										case Game.BlackBoxBrake.Lap:
											brake[k] = Translations.QuickReferences.HandleLap;
											break;
										case Game.BlackBoxBrake.Service:
											brake[k] = Translations.QuickReferences.HandleService;
											break;
										default:
											brake[k] = Translations.QuickReferences.HandleBrake + ((short)b).ToString(culture);
											break;
									}
								}
								lines[j][6] = brake[0] + " → " + brake[1];
							}
							lines[j][7] = GetBlackBoxText(Game.BlackBoxEntries[i].EventToken);
						}
						int[] widths = new int[columns];
						for (int i = 0; i < lines.Length; i++)
						{
							for (int j = 0; j < columns; j++)
							{
								if (lines[i][j].Length > widths[j])
								{
									widths[j] = lines[i][j].Length;
								}
							}
						}
						{ // header rows
							int totalWidth = 0;
							for (int j = 0; j < columns; j++)
							{
								totalWidth += widths[j] + 2;
							}
							totalWidth += columns - 1;
							builder.Append('╔');
							builder.Append('═', totalWidth);
							builder.Append("╗\r\n");
							{
								builder.Append('║');
								builder.Append((" " + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","route"}) + " " + Game.LogRouteName).PadRight(totalWidth, ' '));
								builder.Append("║\r\n║");
								builder.Append((" " + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","train"}) + " " + Game.LogTrainName).PadRight(totalWidth, ' '));
								builder.Append("║\r\n║");
								builder.Append((" " + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","date"}) + " " + Game.LogDateTime.ToString("yyyy-MM-dd HH:mm:ss", culture)).PadRight(totalWidth, ' '));
								builder.Append("║\r\n");
							}
						}
						{ // top border row
							builder.Append('╠');
							for (int j = 0; j < columns; j++)
							{
								if (j != 0)
								{
									builder.Append('╤');
								} builder.Append('═', widths[j] + 2);
							} builder.Append("╣\r\n");
						}
						for (int i = 0; i < lines.Length; i++)
						{
							// center border row
							if (i != 0)
							{
								builder.Append('╟');
								for (int j = 0; j < columns; j++)
								{
									if (j != 0)
									{
										builder.Append('┼');
									} builder.Append('─', widths[j] + 2);
								} builder.Append("╢\r\n");
							}
							// cell content
							builder.Append('║');
							for (int j = 0; j < columns; j++)
							{
								if (j != 0) builder.Append('│');
								builder.Append(' ');
								if (i != 0 & j <= 3)
								{
									builder.Append(lines[i][j].PadLeft(widths[j], ' '));
								}
								else
								{
									builder.Append(lines[i][j].PadRight(widths[j], ' '));
								}
								builder.Append(' ');
							} builder.Append("║\r\n");
						}
						{ // bottom border row
							builder.Append('╚');
							for (int j = 0; j < columns; j++)
							{
								if (j != 0)
								{
									builder.Append('╧');
								} builder.Append('═', widths[j] + 2);
							} builder.Append('╝');
						}
						File.WriteAllText(file, builder.ToString(), new System.Text.UTF8Encoding(true));
					} break;
			}
		}

	}
}
