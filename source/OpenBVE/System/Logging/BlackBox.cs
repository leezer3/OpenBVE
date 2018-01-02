using System;
using System.Globalization;

namespace OpenBve
{
	internal static partial class Interface
	{
		/// <summary>Loads the black-box logs from the previous simulation run</summary>
		internal static void LoadLogs()
		{
			string BlackBoxFile = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "logs.bin");
			try
			{
				using (System.IO.FileStream Stream = new System.IO.FileStream(BlackBoxFile, System.IO.FileMode.Open, System.IO.FileAccess.Read))
				{
					using (System.IO.BinaryReader Reader = new System.IO.BinaryReader(Stream, System.Text.Encoding.UTF8))
					{
						byte[] Identifier = new byte[] { 111, 112, 101, 110, 66, 86, 69, 95, 76, 79, 71, 83 };
						const short Version = 1;
						byte[] Data = Reader.ReadBytes(Identifier.Length);
						for (int i = 0; i < Identifier.Length; i++)
						{
							if (Identifier[i] != Data[i]) throw new System.IO.InvalidDataException();
						}
						short Number = Reader.ReadInt16();
						if (Version != Number) throw new System.IO.InvalidDataException();
						Game.LogRouteName = Reader.ReadString();
						Game.LogTrainName = Reader.ReadString();
						Game.LogDateTime = DateTime.FromBinary(Reader.ReadInt64());
						Interface.CurrentOptions.GameMode = (Interface.GameMode)Reader.ReadInt16();
						Game.BlackBoxEntryCount = Reader.ReadInt32();
						Game.BlackBoxEntries = new Game.BlackBoxEntry[Game.BlackBoxEntryCount];
						for (int i = 0; i < Game.BlackBoxEntryCount; i++)
						{
							Game.BlackBoxEntries[i].Time = Reader.ReadDouble();
							Game.BlackBoxEntries[i].Position = Reader.ReadDouble();
							Game.BlackBoxEntries[i].Speed = Reader.ReadSingle();
							Game.BlackBoxEntries[i].Acceleration = Reader.ReadSingle();
							Game.BlackBoxEntries[i].ReverserDriver = Reader.ReadInt16();
							Game.BlackBoxEntries[i].ReverserSafety = Reader.ReadInt16();
							Game.BlackBoxEntries[i].PowerDriver = (Game.BlackBoxPower)Reader.ReadInt16();
							Game.BlackBoxEntries[i].PowerSafety = (Game.BlackBoxPower)Reader.ReadInt16();
							Game.BlackBoxEntries[i].BrakeDriver = (Game.BlackBoxBrake)Reader.ReadInt16();
							Game.BlackBoxEntries[i].BrakeSafety = (Game.BlackBoxBrake)Reader.ReadInt16();
							Game.BlackBoxEntries[i].EventToken = (Game.BlackBoxEventToken)Reader.ReadInt16();
						}
						Game.ScoreLogCount = Reader.ReadInt32();
						Game.ScoreLogs = new Game.ScoreLog[Game.ScoreLogCount];
						Game.CurrentScore.CurrentValue = 0;
						for (int i = 0; i < Game.ScoreLogCount; i++)
						{
							Game.ScoreLogs[i].Time = Reader.ReadDouble();
							Game.ScoreLogs[i].Position = Reader.ReadDouble();
							Game.ScoreLogs[i].Value = Reader.ReadInt32();
							Game.ScoreLogs[i].TextToken = (Game.ScoreTextToken)Reader.ReadInt16();
							Game.CurrentScore.CurrentValue += Game.ScoreLogs[i].Value;
						}
						Game.CurrentScore.Maximum = Reader.ReadInt32();
						Identifier = new byte[] { 95, 102, 105, 108, 101, 69, 78, 68 };
						Data = Reader.ReadBytes(Identifier.Length);
						for (int i = 0; i < Identifier.Length; i++)
						{
							if (Identifier[i] != Data[i]) throw new System.IO.InvalidDataException();
						}
						Reader.Close();
					} Stream.Close();
				}
			}
			catch
			{
				Game.LogRouteName = "";
				Game.LogTrainName = "";
				Game.LogDateTime = DateTime.Now;
				Game.BlackBoxEntries = new Game.BlackBoxEntry[256];
				Game.BlackBoxEntryCount = 0;
				Game.ScoreLogs = new Game.ScoreLog[64];
				Game.ScoreLogCount = 0;
			}
		}

		/// <summary>Saves the current in-game black box log</summary>
		internal static void SaveLogs()
		{
			if (Interface.CurrentOptions.BlackBox == false)
			{
				return;
			}
			string BlackBoxFile = OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "logs.bin");
			using (System.IO.FileStream Stream = new System.IO.FileStream(BlackBoxFile, System.IO.FileMode.Create, System.IO.FileAccess.Write))
			{
				//TODO: This code recreates the file every frame.....
				//It should be possible to spin up a stream in a separate thread which then continously appends
				using (System.IO.BinaryWriter Writer = new System.IO.BinaryWriter(Stream, System.Text.Encoding.UTF8))
				{
					byte[] Identifier = new byte[] { 111, 112, 101, 110, 66, 86, 69, 95, 76, 79, 71, 83 };
					const short Version = 1;
					Writer.Write(Identifier);
					Writer.Write(Version);
					Writer.Write(Game.LogRouteName);
					Writer.Write(Game.LogTrainName);
					Writer.Write(Game.LogDateTime.ToBinary());
					Writer.Write((short)Interface.CurrentOptions.GameMode);
					Writer.Write(Game.BlackBoxEntryCount);
					for (int i = 0; i < Game.BlackBoxEntryCount; i++)
					{
						Writer.Write(Game.BlackBoxEntries[i].Time);
						Writer.Write(Game.BlackBoxEntries[i].Position);
						Writer.Write(Game.BlackBoxEntries[i].Speed);
						Writer.Write(Game.BlackBoxEntries[i].Acceleration);
						Writer.Write(Game.BlackBoxEntries[i].ReverserDriver);
						Writer.Write(Game.BlackBoxEntries[i].ReverserSafety);
						Writer.Write((short)Game.BlackBoxEntries[i].PowerDriver);
						Writer.Write((short)Game.BlackBoxEntries[i].PowerSafety);
						Writer.Write((short)Game.BlackBoxEntries[i].BrakeDriver);
						Writer.Write((short)Game.BlackBoxEntries[i].BrakeSafety);
						Writer.Write((short)Game.BlackBoxEntries[i].EventToken);
					}
					Writer.Write(Game.ScoreLogCount);
					for (int i = 0; i < Game.ScoreLogCount; i++)
					{
						Writer.Write(Game.ScoreLogs[i].Time);
						Writer.Write(Game.ScoreLogs[i].Position);
						Writer.Write(Game.ScoreLogs[i].Value);
						Writer.Write((short)Game.ScoreLogs[i].TextToken);
					}
					Writer.Write(Game.CurrentScore.Maximum);
					Identifier = new byte[] { 95, 102, 105, 108, 101, 69, 78, 68 };
					Writer.Write(Identifier);
					Writer.Close();
				} Stream.Close();
			}
		}

		/// <summary>The available formats for exporting black box data</summary>
		internal enum BlackBoxFormat
		{
			CommaSeparatedValue = 0,
			FormattedText = 1
		}
		/// <summary>Gets the formatted output text for a black box event token</summary>
		/// <param name="EventToken">The event token for which to get the text</param>
		internal static string GetBlackBoxText(Game.BlackBoxEventToken EventToken)
		{
			//TODO: Only returns a blank string, what was intended here???
			switch (EventToken)
			{
				default: return "";
			}
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
						for (int i = 0; i < Game.BlackBoxEntryCount; i++)
						{
							Builder.Append(Game.BlackBoxEntries[i].Time.ToString(Culture) + ",");
							Builder.Append(Game.BlackBoxEntries[i].Position.ToString(Culture) + ",");
							Builder.Append(Game.BlackBoxEntries[i].Speed.ToString(Culture) + ",");
							Builder.Append(Game.BlackBoxEntries[i].Acceleration.ToString(Culture) + ",");
							Builder.Append(((short)Game.BlackBoxEntries[i].ReverserDriver).ToString(Culture) + ",");
							Builder.Append(((short)Game.BlackBoxEntries[i].ReverserSafety).ToString(Culture) + ",");
							Builder.Append(((short)Game.BlackBoxEntries[i].PowerDriver).ToString(Culture) + ",");
							Builder.Append(((short)Game.BlackBoxEntries[i].PowerSafety).ToString(Culture) + ",");
							Builder.Append(((short)Game.BlackBoxEntries[i].BrakeDriver).ToString(Culture) + ",");
							Builder.Append(((short)Game.BlackBoxEntries[i].BrakeSafety).ToString(Culture) + ",");
							Builder.Append(((short)Game.BlackBoxEntries[i].EventToken).ToString(Culture));
							Builder.Append("\r\n");
						}
						System.IO.File.WriteAllText(File, Builder.ToString(), new System.Text.UTF8Encoding(true));
					} break;
				// formatted text
				case BlackBoxFormat.FormattedText:
					{
						CultureInfo Culture = CultureInfo.InvariantCulture;
						System.Text.StringBuilder Builder = new System.Text.StringBuilder();
						string[][] Lines = new string[Game.BlackBoxEntryCount + 1][];
						Lines[0] = new string[] {
							GetInterfaceString("log_time"),
							GetInterfaceString("log_position"),
							GetInterfaceString("log_speed"),
							GetInterfaceString("log_acceleration"),
							GetInterfaceString("log_reverser"),
							GetInterfaceString("log_power"),
							GetInterfaceString("log_brake"),
							GetInterfaceString("log_event"),
						};
						int Columns = Lines[0].Length;
						for (int i = 0; i < Game.BlackBoxEntryCount; i++)
						{
							int j = i + 1;
							Lines[j] = new string[Columns];
							{
								double x = Game.BlackBoxEntries[i].Time;
								int h = (int)Math.Floor(x / 3600.0);
								x -= (double)h * 3600.0;
								int m = (int)Math.Floor(x / 60.0);
								x -= (double)m * 60.0;
								int s = (int)Math.Floor(x);
								x -= (double)s;
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
											reverser[k] = QuickReferences.HandleBackward;
											break;
										case 0:
											reverser[k] = QuickReferences.HandleNeutral;
											break;
										case 1:
											reverser[k] = QuickReferences.HandleForward;
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
									Game.BlackBoxPower p = k == 0 ? Game.BlackBoxEntries[i].PowerDriver : Game.BlackBoxEntries[i].PowerSafety;
									switch (p)
									{
										case Game.BlackBoxPower.PowerNull:
											power[k] = GetInterfaceString(QuickReferences.HandlePowerNull);
											break;
										default:
											power[k] = GetInterfaceString(QuickReferences.HandlePower) + ((short)p).ToString(Culture);
											break;
									}
								}
								Lines[j][5] = power[0] + " → " + power[1];
							}
							{
								string[] brake = new string[2];
								for (int k = 0; k < 2; k++)
								{
									Game.BlackBoxBrake b = k == 0 ? Game.BlackBoxEntries[i].BrakeDriver : Game.BlackBoxEntries[i].BrakeSafety;
									switch (b)
									{
										case Game.BlackBoxBrake.BrakeNull:
											brake[k] = GetInterfaceString(QuickReferences.HandleBrakeNull);
											break;
										case Game.BlackBoxBrake.Emergency:
											brake[k] = GetInterfaceString(QuickReferences.HandleEmergency);
											break;
										case Game.BlackBoxBrake.HoldBrake:
											brake[k] = GetInterfaceString(QuickReferences.HandleHoldBrake);
											break;
										case Game.BlackBoxBrake.Release:
											brake[k] = GetInterfaceString(QuickReferences.HandleRelease);
											break;
										case Game.BlackBoxBrake.Lap:
											brake[k] = GetInterfaceString(QuickReferences.HandleLap);
											break;
										case Game.BlackBoxBrake.Service:
											brake[k] = GetInterfaceString(QuickReferences.HandleService);
											break;
										default:
											brake[k] = GetInterfaceString(QuickReferences.HandleBrake) + ((short)b).ToString(Culture);
											break;
									}
								}
								Lines[j][6] = brake[0] + " → " + brake[1];
							}
							Lines[j][7] = GetBlackBoxText(Game.BlackBoxEntries[i].EventToken);
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
								Builder.Append((" " + GetInterfaceString("log_route") + " " + Game.LogRouteName).PadRight(TotalWidth, ' '));
								Builder.Append("║\r\n║");
								Builder.Append((" " + GetInterfaceString("log_train") + " " + Game.LogTrainName).PadRight(TotalWidth, ' '));
								Builder.Append("║\r\n║");
								Builder.Append((" " + GetInterfaceString("log_date") + " " + Game.LogDateTime.ToString("yyyy-MM-dd HH:mm:ss", Culture)).PadRight(TotalWidth, ' '));
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
