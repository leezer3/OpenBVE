using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using OpenBveApi;
using OpenBveApi.Interface;
using TrainEditor2.Models.Sounds;
using TrainEditor2.Systems;
using Path = OpenBveApi.Path;

namespace TrainEditor2.IO.Sounds.Bve4
{
	internal static partial class SoundCfgBve4
	{
		internal static void Parse(string fileName, out Sound sound)
		{
			sound = new Sound();

			CultureInfo culture = CultureInfo.InvariantCulture;
			List<string> lines = File.ReadAllLines(fileName, TextEncoding.GetSystemEncodingFromFile(fileName)).ToList();
			string basePath = Path.GetDirectoryName(fileName);

			for (int i = lines.Count - 1; i >= 0; i--)
			{
				/*
				 * Strip comments and remove empty resulting lines etc.
				 *
				 * This fixes an error with some NYCTA content, which has
				 * a copyright notice instead of the file header specified....
				 */
				int j = lines[i].IndexOf(';');

				if (j >= 0)
				{
					lines[i] = lines[i].Substring(0, j).Trim();
				}
				else
				{
					lines[i] = lines[i].Trim();
				}

				if (string.IsNullOrEmpty(lines[i]))
				{
					lines.RemoveAt(i);
				}
			}

			if (!lines.Any())
			{
				Interface.AddMessage(MessageType.Error, false, $"Empty sound.cfg encountered in {fileName}.");
			}

			if (string.Compare(lines[0], "version 1.0", StringComparison.OrdinalIgnoreCase) != 0)
			{
				Interface.AddMessage(MessageType.Error, false, $"Invalid file format encountered in {fileName}. The first line is expected to be \"Version 1.0\".");
			}

			for (int i = 0; i < lines.Count; i++)
			{
				switch (lines[i].ToLowerInvariant())
				{
					case "[run]":
						i++;

						while (i < lines.Count && !lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = lines[i].Substring(0, j).TrimEnd();
								string b = lines[i].Substring(j + 1).TrimStart();
								int k;

								if (!int.TryParse(a, NumberStyles.Integer, culture, out k))
								{
									Interface.AddMessage(MessageType.Error, false, $"Invalid index appeared at line {(i + 1).ToString(culture)} in file {fileName}");
								}
								else if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters or is empty at line {(i + 1).ToString(culture)} in file {fileName}");
								}
								else
								{
									if (k >= 0)
									{
										sound.SoundElements.Add(new RunElement { Key = k, FilePath = Path.CombineFile(basePath, b) });
									}
									else
									{
										Interface.AddMessage(MessageType.Error, false, $"Index must be greater or equal to zero at line {(i + 1).ToString(culture)} in file {fileName}");
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[flange]":
						i++;

						while (i < lines.Count && !lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = lines[i].Substring(0, j).TrimEnd();
								string b = lines[i].Substring(j + 1).TrimStart();
								int k;

								if (!int.TryParse(a, NumberStyles.Integer, culture, out k))
								{
									Interface.AddMessage(MessageType.Error, false, $"Invalid index appeared at line {(i + 1).ToString(culture)} in file {fileName}");
								}
								else if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters or is empty at line {(i + 1).ToString(culture)} in file {fileName}");
								}
								else
								{
									if (k >= 0)
									{
										sound.SoundElements.Add(new FlangeElement { Key = k, FilePath = Path.CombineFile(basePath, b) });
									}
									else
									{
										Interface.AddMessage(MessageType.Error, false, $"Index must be greater or equal to zero at line {(i + 1).ToString(culture)} in file {fileName}");
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[motor]":
						i++;

						while (i < lines.Count && !lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = lines[i].Substring(0, j).TrimEnd();
								string b = lines[i].Substring(j + 1).TrimStart();
								int k;

								if (!int.TryParse(a, NumberStyles.Integer, culture, out k))
								{
									Interface.AddMessage(MessageType.Error, false, $"Invalid index appeared at line {(i + 1).ToString(culture)} in file {fileName}");
								}
								else if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters or is empty at line {(i + 1).ToString(culture)} in file {fileName}");
								}
								else
								{
									if (k >= 0)
									{
										sound.SoundElements.Add(new MotorElement { Key = k, FilePath = Path.CombineFile(basePath, b) });
									}
									else
									{
										Interface.AddMessage(MessageType.Error, false, $"Index is invalid at line {(i + 1).ToString(culture)} in file {fileName}");
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[switch]":
						i++;

						while (i < lines.Count && !lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = lines[i].Substring(0, j).TrimEnd();
								string b = lines[i].Substring(j + 1).TrimStart();
								int k;

								if (!int.TryParse(a, NumberStyles.Integer, culture, out k))
								{
									Interface.AddMessage(MessageType.Error, false, $"Invalid index appeared at line {(i + 1).ToString(culture)} in file {fileName}");
								}
								else if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters or is empty at line {(i + 1).ToString(culture)} in file {fileName}");
								}
								else
								{
									if (k >= 0)
									{
										sound.SoundElements.Add(new FrontSwitchElement { Key = k, FilePath = Path.CombineFile(basePath, b) });
									}
									else
									{
										Interface.AddMessage(MessageType.Error, false, $"Index is invalid at line {(i + 1).ToString(culture)} in file {fileName}");
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[brake]":
						i++;

						while (i < lines.Count && !lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = lines[i].Substring(0, j).TrimEnd();
								string b = lines[i].Substring(j + 1).TrimStart();

								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters or is empty at line {(i + 1).ToString(culture)} in file {fileName}");
								}
								else
								{
									SoundKey.Brake result;

									if (SoundKey.TryParse(a, true, out result))
									{
										sound.SoundElements.Add(new BrakeElement { Key = result, FilePath = Path.CombineFile(basePath, b) });
									}
									else
									{
										Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {a} encountered at line {(i + 1).ToString(culture)} in file {fileName}");
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[compressor]":
						i++;

						while (i < lines.Count && !lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = lines[i].Substring(0, j).TrimEnd();
								string b = lines[i].Substring(j + 1).TrimStart();

								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters or is empty at line {(i + 1).ToString(culture)} in file {fileName}");
								}
								else
								{
									SoundKey.Compressor result;

									if (SoundKey.TryParse(a, true, out result))
									{
										sound.SoundElements.Add(new CompressorElement { Key = result, FilePath = Path.CombineFile(basePath, b) });
									}
									else
									{
										Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {a} encountered at line {(i + 1).ToString(culture)} in file {fileName}");
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[suspension]":
						i++;

						while (i < lines.Count && !lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = lines[i].Substring(0, j).TrimEnd();
								string b = lines[i].Substring(j + 1).TrimStart();

								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters or is empty at line {(i + 1).ToString(culture)} in file {fileName}");
								}
								else
								{
									SoundKey.Suspension result;

									if (SoundKey.TryParse(a, true, out result))
									{
										sound.SoundElements.Add(new SuspensionElement { Key = result, FilePath = Path.CombineFile(basePath, b) });
									}
									else
									{
										Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {a} encountered at line {(i + 1).ToString(culture)} in file {fileName}");
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[horn]":
						i++;

						while (i < lines.Count && !lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = lines[i].Substring(0, j).TrimEnd();
								string b = lines[i].Substring(j + 1).TrimStart();

								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters or is empty at line {(i + 1).ToString(culture)} in file {fileName}");
								}
								else
								{
									if (string.Equals(a, "Primary", StringComparison.InvariantCultureIgnoreCase))
									{
										SoundKey.Horn result;

										if (SoundKey.TryParse(Regex.Replace(a, "Primary", string.Empty, RegexOptions.IgnoreCase), true, out result))
										{
											sound.SoundElements.Add(new PrimaryHornElement { Key = result, FilePath = Path.CombineFile(basePath, b) });

										}
										else
										{
											sound.SoundElements.Add(new PrimaryHornElement { Key = SoundKey.Horn.Loop, FilePath = Path.CombineFile(basePath, b) });
										}
									}
									else if (string.Equals(a, "Secondary", StringComparison.InvariantCultureIgnoreCase))
									{
										SoundKey.Horn result;

										if (SoundKey.TryParse(Regex.Replace(a, "Secondary", string.Empty, RegexOptions.IgnoreCase), true, out result))
										{
											sound.SoundElements.Add(new SecondaryHornElement { Key = SoundKey.Horn.Loop, FilePath = Path.CombineFile(basePath, b) });

										}
										else
										{
											sound.SoundElements.Add(new SecondaryHornElement { Key = SoundKey.Horn.Loop, FilePath = Path.CombineFile(basePath, b) });
										}
									}
									else if (string.Equals(a, "Music", StringComparison.InvariantCultureIgnoreCase))
									{
										SoundKey.Horn result;

										if (SoundKey.TryParse(Regex.Replace(a, "Music", string.Empty, RegexOptions.IgnoreCase), true, out result))
										{
											sound.SoundElements.Add(new MusicHornElement { Key = result, FilePath = Path.CombineFile(basePath, b) });

										}
										else
										{
											sound.SoundElements.Add(new MusicHornElement { Key = SoundKey.Horn.Loop, FilePath = Path.CombineFile(basePath, b) });
										}
									}
									else
									{
										Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {a} encountered at line {(i + 1).ToString(culture)} in file {fileName}");
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[door]":
						i++;

						while (i < lines.Count && !lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = lines[i].Substring(0, j).TrimEnd();
								string b = lines[i].Substring(j + 1).TrimStart();

								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters or is empty at line {(i + 1).ToString(culture)} in file {fileName}");
								}
								else
								{
									SoundKey.Door result;

									if (SoundKey.TryParse(a, true, out result))
									{
										sound.SoundElements.Add(new DoorElement { Key = result, FilePath = Path.CombineFile(basePath, b) });
									}
									else
									{
										Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {a} encountered at line {(i + 1).ToString(culture)} in file {fileName}");
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[ats]":
						i++;

						while (i < lines.Count && !lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = lines[i].Substring(0, j).TrimEnd();
								string b = lines[i].Substring(j + 1).TrimStart();

								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters or is empty at line {(i + 1).ToString(culture)} in file {fileName}");
								}
								else
								{
									int k;

									if (!int.TryParse(a, NumberStyles.Integer, culture, out k))
									{
										Interface.AddMessage(MessageType.Error, false, $"Invalid index appeared at line {(i + 1).ToString(culture)} in file {fileName}");
									}
									else
									{
										if (k >= 0)
										{
											sound.SoundElements.Add(new AtsElement { Key = k, FilePath = Path.CombineFile(basePath, b) });
										}
										else
										{
											Interface.AddMessage(MessageType.Warning, false, $"Index must be greater or equal to zero at line {(i + 1).ToString(culture)} in file {fileName}");
										}
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[buzzer]":
						i++;

						while (i < lines.Count && !lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = lines[i].Substring(0, j).TrimEnd();
								string b = lines[i].Substring(j + 1).TrimStart();

								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters or is empty at line {(i + 1).ToString(culture)} in file {fileName}");
								}
								else
								{
									SoundKey.Buzzer result;

									if (SoundKey.TryParse(a, true, out result))
									{
										sound.SoundElements.Add(new BuzzerElement { Key = result, FilePath = Path.CombineFile(basePath, b) });
									}
									else
									{
										Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {a} encountered at line {(i + 1).ToString(culture)} in file {fileName}");
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[pilot lamp]":
						i++;

						while (i < lines.Count && !lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = lines[i].Substring(0, j).TrimEnd();
								string b = lines[i].Substring(j + 1).TrimStart();

								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters or is empty at line {(i + 1).ToString(culture)} in file {fileName}");
								}
								else
								{
									SoundKey.PilotLamp result;

									if (SoundKey.TryParse(a, true, out result))
									{
										sound.SoundElements.Add(new PilotLampElement { Key = result, FilePath = Path.CombineFile(basePath, b) });
									}
									else
									{
										Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {a} encountered at line {(i + 1).ToString(culture)} in file {fileName}");
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[brake handle]":
						i++;

						while (i < lines.Count && !lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = lines[i].Substring(0, j).TrimEnd();
								string b = lines[i].Substring(j + 1).TrimStart();

								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters or is empty at line {(i + 1).ToString(culture)} in file {fileName}");
								}
								else
								{
									SoundKey.BrakeHandle result;

									if (SoundKey.TryParse(a, true, out result))
									{
										sound.SoundElements.Add(new BrakeHandleElement { Key = result, FilePath = Path.CombineFile(basePath, b) });
									}
									else
									{
										Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {a} encountered at line {(i + 1).ToString(culture)} in file {fileName}");
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[master controller]":
						i++;

						while (i < lines.Count && !lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = lines[i].Substring(0, j).TrimEnd();
								string b = lines[i].Substring(j + 1).TrimStart();

								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters or is empty at line {(i + 1).ToString(culture)} in file {fileName}");
								}
								else
								{
									SoundKey.MasterController result;

									if (SoundKey.TryParse(a, true, out result))
									{
										sound.SoundElements.Add(new MasterControllerElement { Key = result, FilePath = Path.CombineFile(basePath, b) });
									}
									else
									{
										Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {a} encountered at line {(i + 1).ToString(culture)} in file {fileName}");
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[reverser]":
						i++;

						while (i < lines.Count && !lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = lines[i].Substring(0, j).TrimEnd();
								string b = lines[i].Substring(j + 1).TrimStart();

								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters or is empty at line {(i + 1).ToString(culture)} in file {fileName}");
								}
								else
								{
									SoundKey.Reverser result;

									if (SoundKey.TryParse(a, true, out result))
									{
										sound.SoundElements.Add(new ReverserElement { Key = result, FilePath = Path.CombineFile(basePath, b) });
									}
									else
									{
										Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {a} encountered at line {(i + 1).ToString(culture)} in file {fileName}");
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[breaker]":
						i++;

						while (i < lines.Count && !lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = lines[i].Substring(0, j).TrimEnd();
								string b = lines[i].Substring(j + 1).TrimStart();

								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters or is empty at line {(i + 1).ToString(culture)} in file {fileName}");
								}
								else
								{
									SoundKey.Breaker result;

									if (SoundKey.TryParse(a, true, out result))
									{
										sound.SoundElements.Add(new BreakerElement { Key = result, FilePath = Path.CombineFile(basePath, b) });
									}
									else
									{
										Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {a} encountered at line {(i + 1).ToString(culture)} in file {fileName}");
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[others]":
						i++;

						while (i < lines.Count && !lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = lines[i].Substring(0, j).TrimEnd();
								string b = lines[i].Substring(j + 1).TrimStart();

								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters or is empty at line {(i + 1).ToString(culture)} in file {fileName}");
								}
								else
								{
									SoundKey.Others result;

									if (SoundKey.TryParse(a, true, out result))
									{
										sound.SoundElements.Add(new OthersElement { Key = result, FilePath = Path.CombineFile(basePath, b) });
									}
									else
									{
										Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {a} encountered at line {(i + 1).ToString(culture)} in file {fileName}");
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[requeststop]":
						i++;

						while (i < lines.Count && !lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = lines[i].Substring(0, j).TrimEnd();
								string b = lines[i].Substring(j + 1).TrimStart();

								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters or is empty at line {(i + 1).ToString(culture)} in file {fileName}");
								}
								else
								{
									SoundKey.RequestStop result;

									if (SoundKey.TryParse(a, true, out result))
									{
										sound.SoundElements.Add(new RequestStopElement { Key = result, FilePath = Path.CombineFile(basePath, b) });
									}
									else
									{
										Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {a} encountered at line {(i + 1).ToString(culture)} in file {fileName}");
									}
								}
							}

							i++;
						}

						i--;
						break;
					case "[touch]":
						i++;

						while (i < lines.Count && !lines[i].StartsWith("[", StringComparison.Ordinal))
						{
							int j = lines[i].IndexOf("=", StringComparison.Ordinal);

							if (j >= 0)
							{
								string a = lines[i].Substring(0, j).TrimEnd();
								string b = lines[i].Substring(j + 1).TrimStart();

								if (b.Length == 0 || Path.ContainsInvalidChars(b))
								{
									Interface.AddMessage(MessageType.Error, false, $"FileName contains illegal characters or is empty at line {(i + 1).ToString(culture)} in file {fileName}");
								}
								else
								{
									int k;

									if (!int.TryParse(a, NumberStyles.Integer, culture, out k))
									{
										Interface.AddMessage(MessageType.Error, false, $"Invalid index appeared at line {(i + 1).ToString(culture)} in file {fileName}");
									}
									else
									{
										if (k >= 0)
										{
											sound.SoundElements.Add(new TouchElement { Key = k, FilePath = Path.CombineFile(basePath, b) });
										}
										else
										{
											Interface.AddMessage(MessageType.Warning, false, $"Index must be greater or equal to zero at line {(i + 1).ToString(culture)} in file {fileName}");
										}
									}
								}
							}

							i++;
						}

						i--;
						break;
				}
			}

			sound.SoundElements = new ObservableCollection<SoundElement>(sound.SoundElements.GroupBy(x => new { Type = x.GetType(), x.Key }).Select(x => x.First()));
		}
	}
}
