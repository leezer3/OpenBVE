using System;
using System.Windows.Forms;
using OpenBveApi.Math;

namespace CarXmlConvertor
{
	class ConvertTrainDat
	{
		internal static string FileName;

		internal static void Process()
		{
			if (!System.IO.File.Exists(FileName))
			{
				MessageBox.Show("The selected folder does not contain a valid train.dat \r\n Please retry.", "CarXML Convertor", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			string[] Lines = System.IO.File.ReadAllLines(FileName);
			for (int i = 0; i < Lines.Length; i++)
			{
				int n = 0;
				switch (Lines[i].ToLowerInvariant())
				{
					case "#cockpit":
					case "#cab":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.Ordinal))
						{
							double a; if (NumberFormats.TryParseDoubleVb6(Lines[i], out a))
							{
								switch (n)
								{
									case 0: ConvertSoundCfg.DriverPosition.X = 0.001 * a; break;
									case 1: ConvertSoundCfg.DriverPosition.Y = 0.001 * a; break;
									case 2: ConvertSoundCfg.DriverPosition.Z = 0.001 * a; break;
								}
							}
							i++; n++;
						}
						i--; break;
					case "#car":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.Ordinal))
						{
							double a; if (NumberFormats.TryParseDoubleVb6(Lines[i], out a))
							{
								switch (n)
								{
									case 4:
										if (a <= 0.0)
										{
											break;
										}
										else
										{
											ConvertSoundCfg.length = a;
										}
										break;
								}
							}
							i++; n++;
						}
						i--; break;

					default:
					{
						i++;
						while (i < Lines.Length && !Lines[i].StartsWith("#", StringComparison.Ordinal))
						{
							i++; n++;
						}
						i--;
					}
						break;
				}
			}
		}
	}
}
