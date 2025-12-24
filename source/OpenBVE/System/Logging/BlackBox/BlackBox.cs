using System;
using System.Collections.Generic;
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
		private static readonly byte[] Identifier = { 111, 112, 101, 110, 66, 86, 69, 95, 76, 79, 71, 83 };
		/// <summary>Magic bytes identifying the EOF</summary>
		/// <remarks>_fileEND in UTF-8</remarks>
		private static readonly byte[] Footer = { 95, 102, 105, 108, 101, 69, 78, 68 };

		/// <summary>Loads the black-box logs from the previous simulation run</summary>
		internal static void LoadLogs()
		{
			string BlackBoxFile = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "logs.bin");
			if (File.Exists(BlackBoxFile))
			{
				try
				{
					using (FileStream Stream = new FileStream(BlackBoxFile, FileMode.Open, FileAccess.Read))
					{
						using (BinaryReader Reader = new BinaryReader(Stream, System.Text.Encoding.UTF8))
						{
							const short Version = 1;
							byte[] Data = Reader.ReadBytes(Identifier.Length);
							for (int i = 0; i < Identifier.Length; i++)
							{
								if (Identifier[i] != Data[i]) throw new InvalidDataException();
							}

							short Number = Reader.ReadInt16();
							if (Version != Number) throw new InvalidDataException();
							Game.LogRouteName = Reader.ReadString();
							Game.LogTrainName = Reader.ReadString();
							Game.LogDateTime = DateTime.FromBinary(Reader.ReadInt64());
							CurrentOptions.PreviousGameMode = (GameMode) Reader.ReadInt16();
							int entryCount = Reader.ReadInt32();
							Game.BlackBoxEntries = new List<BlackBoxEntry>();
							for (int i = 0; i < entryCount; i++)
							{
								Game.BlackBoxEntries.Add(new BlackBoxEntry(Reader));
							}

							Game.ScoreLogCount = Reader.ReadInt32();
							Game.ScoreLogs = new Game.ScoreLog[Game.ScoreLogCount];
							Game.CurrentScore.CurrentValue = 0;
							for (int i = 0; i < Game.ScoreLogCount; i++)
							{
								Game.ScoreLogs[i].Time = Reader.ReadDouble();
								Game.ScoreLogs[i].Position = Reader.ReadDouble();
								Game.ScoreLogs[i].Value = Reader.ReadInt32();
								Game.ScoreLogs[i].TextToken = (Game.ScoreTextToken) Reader.ReadInt16();
								Game.CurrentScore.CurrentValue += Game.ScoreLogs[i].Value;
							}

							Game.CurrentScore.Maximum = Reader.ReadInt32();
							Data = Reader.ReadBytes(Footer.Length);
							for (int i = 0; i < Footer.Length; i++)
							{
								if (Footer[i] != Data[i]) throw new InvalidDataException();
							}

							Reader.Close();
						}

						Stream.Close();
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
			Game.BlackBoxEntries = new List<BlackBoxEntry>();
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
			string BlackBoxFile = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "logs.bin");
			try
			{
				using (FileStream Stream = new FileStream(BlackBoxFile, FileMode.Create, FileAccess.Write))
				{
					using (BinaryWriter Writer = new BinaryWriter(Stream, System.Text.Encoding.UTF8))
					{
						const short Version = 1;
						Writer.Write(Identifier);
						Writer.Write(Version);
						Writer.Write(Game.LogRouteName);
						Writer.Write(Game.LogTrainName);
						Writer.Write(Game.LogDateTime.ToBinary());
						Writer.Write((short) CurrentOptions.GameMode);
						Writer.Write(Game.BlackBoxEntries.Count);
						for (int i = 0; i < Game.BlackBoxEntries.Count; i++)
						{
							Writer.Write(Game.BlackBoxEntries[i].Time);
							Writer.Write(Game.BlackBoxEntries[i].Position);
							Writer.Write(Game.BlackBoxEntries[i].Speed);
							Writer.Write(Game.BlackBoxEntries[i].Acceleration);
							Writer.Write(Game.BlackBoxEntries[i].ReverserDriver);
							Writer.Write(Game.BlackBoxEntries[i].ReverserSafety);
							Writer.Write((short) Game.BlackBoxEntries[i].PowerDriver);
							Writer.Write((short) Game.BlackBoxEntries[i].PowerSafety);
							Writer.Write((short) Game.BlackBoxEntries[i].BrakeDriver);
							Writer.Write((short) Game.BlackBoxEntries[i].BrakeSafety);
							Writer.Write((short)0);
						}

						Writer.Write(Game.ScoreLogCount);
						for (int i = 0; i < Game.ScoreLogCount; i++)
						{
							Writer.Write(Game.ScoreLogs[i].Time);
							Writer.Write(Game.ScoreLogs[i].Position);
							Writer.Write(Game.ScoreLogs[i].Value);
							Writer.Write((short) Game.ScoreLogs[i].TextToken);
						}

						Writer.Write(Game.CurrentScore.Maximum);
						Writer.Write(Footer);
						Writer.Close();
					}

					Stream.Close();
				}
			}
			catch
			{
				CurrentOptions.BlackBox = false;
				AddMessage(MessageType.Error, false, "An unexpected error occurred whilst attempting to write to the black box log- Black box has been disabled.");
			}
		}

		/// <summary>The available formats for exporting black box data</summary>
		internal enum BlackBoxFormat
		{
			CommaSeparatedValue = 0,
			FormattedText = 1
		}
		
		/// <summary>Exports the current black box data to a file</summary>
		/// <param name="File">The file to write</param>
		/// <param name="Format">The format in which to write the data</param>
		internal static void ExportBlackBox(string File, BlackBoxFormat Format)
		{
			switch (Format)
			{
				// comma separated value
				case BlackBoxFormat.CommaSeparatedValue:
					{
						CultureInfo Culture = CultureInfo.InvariantCulture;
						System.Text.StringBuilder Builder = new System.Text.StringBuilder();
						for (int i = 0; i < Game.BlackBoxEntries.Count; i++)
						{
							Builder.Append(Game.BlackBoxEntries[i].Time.ToString(Culture) + ",");
							Builder.Append(Game.BlackBoxEntries[i].Position.ToString(Culture) + ",");
							Builder.Append(Game.BlackBoxEntries[i].Speed.ToString(Culture) + ",");
							Builder.Append(Game.BlackBoxEntries[i].Acceleration.ToString(Culture) + ",");
							Builder.Append(Game.BlackBoxEntries[i].ReverserDriver.ToString(Culture) + ",");
							Builder.Append(Game.BlackBoxEntries[i].ReverserSafety.ToString(Culture) + ",");
							Builder.Append(((short)Game.BlackBoxEntries[i].PowerDriver).ToString(Culture) + ",");
							Builder.Append(((short)Game.BlackBoxEntries[i].PowerSafety).ToString(Culture) + ",");
							Builder.Append(((short)Game.BlackBoxEntries[i].BrakeDriver).ToString(Culture) + ",");
							Builder.Append(((short)Game.BlackBoxEntries[i].BrakeSafety).ToString(Culture) + ",");
							Builder.Append("\r\n");
						}
						System.IO.File.WriteAllText(File, Builder.ToString(), new System.Text.UTF8Encoding(true));
					} break;
				// formatted text
				case BlackBoxFormat.FormattedText:
					{
						CultureInfo Culture = CultureInfo.InvariantCulture;
						System.Text.StringBuilder Builder = new System.Text.StringBuilder();
						string[][] Lines = new string[Game.BlackBoxEntries.Count + 1][];
						Lines[0] = new[] {
							Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","time"}),
							Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","position"}),
							Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","speed"}),
							Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","acceleration"}),
							Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","reverser"}),
							Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","power"}),
							Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","brake"}),
							Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","event"}),
						};
						int Columns = Lines[0].Length;
						for (int i = 0; i < Game.BlackBoxEntries.Count; i++)
						{
							int j = i + 1;
							Lines[j] = new string[Columns];
							{
								double x = Game.BlackBoxEntries[i].Time;
								int h = (int)Math.Floor(x / 3600.0);
								x -= h * 3600.0;
								int m = (int)Math.Floor(x / 60.0);
								x -= m * 60.0;
								int s = (int)Math.Floor(x);
								x -= s;
								int n = (int)Math.Floor(1000.0 * x);
								Lines[j][0] = h.ToString("00", Culture) + ":" + m.ToString("00", Culture) + ":" + s.ToString("00", Culture) + ":" + n.ToString("000", Culture);
							}
							Lines[j][1] = Game.BlackBoxEntries[i].Position.ToString("0.000", Culture);
							Lines[j][2] = Game.BlackBoxEntries[i].Speed.ToString("0.0000", Culture);
							Lines[j][3] = Game.BlackBoxEntries[i].Acceleration.ToString("0.0000", Culture);
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
											reverser[k] = r.ToString(Culture);
											break;
									}
								}
								Lines[j][4] = reverser[0] + " → " + reverser[1];
							}
							{
								string[] power = new string[2];
								for (int k = 0; k < 2; k++)
								{
									BlackBoxPower p = k == 0 ? Game.BlackBoxEntries[i].PowerDriver : Game.BlackBoxEntries[i].PowerSafety;
									switch (p)
									{
										case BlackBoxPower.PowerNull:
											power[k] = Translations.QuickReferences.HandlePowerNull;
											break;
										default:
											power[k] = Translations.QuickReferences.HandlePower + ((short)p).ToString(Culture);
											break;
									}
								}
								Lines[j][5] = power[0] + " → " + power[1];
							}
							{
								string[] brake = new string[2];
								for (int k = 0; k < 2; k++)
								{
									BlackBoxBrake b = k == 0 ? Game.BlackBoxEntries[i].BrakeDriver : Game.BlackBoxEntries[i].BrakeSafety;
									switch (b)
									{
										case BlackBoxBrake.BrakeNull:
											brake[k] = Translations.QuickReferences.HandleBrakeNull;
											break;
										case BlackBoxBrake.Emergency:
											brake[k] = Translations.QuickReferences.HandleEmergency;
											break;
										case BlackBoxBrake.HoldBrake:
											brake[k] = Translations.QuickReferences.HandleHoldBrake;
											break;
										case BlackBoxBrake.Release:
											brake[k] = Translations.QuickReferences.HandleRelease;
											break;
										case BlackBoxBrake.Lap:
											brake[k] = Translations.QuickReferences.HandleLap;
											break;
										case BlackBoxBrake.Service:
											brake[k] = Translations.QuickReferences.HandleService;
											break;
										default:
											brake[k] = Translations.QuickReferences.HandleBrake + ((short)b).ToString(Culture);
											break;
									}
								}
								Lines[j][6] = brake[0] + " → " + brake[1];
							}
							Lines[j][7] = string.Empty;
						}
						int[] Widths = new int[Columns];
						for (int i = 0; i < Lines.Length; i++)
						{
							for (int j = 0; j < Columns; j++)
							{
								if (Lines[i][j].Length > Widths[j])
								{
									Widths[j] = Lines[i][j].Length;
								}
							}
						}
						{ // header rows
							int TotalWidth = 0;
							for (int j = 0; j < Columns; j++)
							{
								TotalWidth += Widths[j] + 2;
							}
							TotalWidth += Columns - 1;
							Builder.Append('╔');
							Builder.Append('═', TotalWidth);
							Builder.Append("╗\r\n");
							{
								Builder.Append('║');
								Builder.Append((" " + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","route"}) + " " + Game.LogRouteName).PadRight(TotalWidth, ' '));
								Builder.Append("║\r\n║");
								Builder.Append((" " + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","train"}) + " " + Game.LogTrainName).PadRight(TotalWidth, ' '));
								Builder.Append("║\r\n║");
								Builder.Append((" " + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"log","date"}) + " " + Game.LogDateTime.ToString("yyyy-MM-dd HH:mm:ss", Culture)).PadRight(TotalWidth, ' '));
								Builder.Append("║\r\n");
							}
						}
						{ // top border row
							Builder.Append('╠');
							for (int j = 0; j < Columns; j++)
							{
								if (j != 0)
								{
									Builder.Append('╤');
								} Builder.Append('═', Widths[j] + 2);
							} Builder.Append("╣\r\n");
						}
						for (int i = 0; i < Lines.Length; i++)
						{
							// center border row
							if (i != 0)
							{
								Builder.Append('╟');
								for (int j = 0; j < Columns; j++)
								{
									if (j != 0)
									{
										Builder.Append('┼');
									} Builder.Append('─', Widths[j] + 2);
								} Builder.Append("╢\r\n");
							}
							// cell content
							Builder.Append('║');
							for (int j = 0; j < Columns; j++)
							{
								if (j != 0) Builder.Append('│');
								Builder.Append(' ');
								if (i != 0 & j <= 3)
								{
									Builder.Append(Lines[i][j].PadLeft(Widths[j], ' '));
								}
								else
								{
									Builder.Append(Lines[i][j].PadRight(Widths[j], ' '));
								}
								Builder.Append(' ');
							} Builder.Append("║\r\n");
						}
						{ // bottom border row
							Builder.Append('╚');
							for (int j = 0; j < Columns; j++)
							{
								if (j != 0)
								{
									Builder.Append('╧');
								} Builder.Append('═', Widths[j] + 2);
							} Builder.Append('╝');
						}
						System.IO.File.WriteAllText(File, Builder.ToString(), new System.Text.UTF8Encoding(true));
					} break;
			}
		}

	}
}
