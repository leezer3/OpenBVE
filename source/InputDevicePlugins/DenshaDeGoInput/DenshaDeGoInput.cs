//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020, Marc Riera, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Globalization;
using System.Windows.Forms;
using OpenBveApi.FileSystem;
using OpenBveApi.Interface;
using OpenBveApi.Runtime;

namespace DenshaDeGoInput
{
	/// <summary>
	/// Input Device Plugin class for controllers of the Densha de GO! series
	/// </summary>
	public class DenshaDeGoInput : IInputDevice
	{
		public event EventHandler<InputEventArgs> KeyDown;
		public event EventHandler<InputEventArgs> KeyUp;

		public InputControl[] Controls { get; private set; }

		private static FileSystem FileSystem;

		private Config configForm;

		internal bool loading = true;
		internal bool brakeHandleMoved;
		internal bool powerHandleMoved;

		internal VehicleSpecs vehicleSpecs = new VehicleSpecs(5, BrakeTypes.AutomaticAirBrake, 8, false, 1);

		public void Config(IWin32Window owner)
		{
			configForm.ShowDialog(owner);
		}

		public bool Load(FileSystem fileSystem)
		{
			configForm = new Config();
			FileSystem = fileSystem;

			Controls = new InputControl[16];
			for (int i = 0; i < 9; i++)
			{
				Controls[i].Command = Translations.Command.BrakeAnyNotch;
				Controls[i].Option = i;
			}
			Controls[9].Command = Translations.Command.BrakeEmergency;
			for (int i = 10; i < 16; i++)
			{
				Controls[i].Command = Translations.Command.PowerAnyNotch;
				Controls[i].Option = i - 10;
			}

			LoadConfig();
			return true;
		}

		public void OnUpdateFrame()
		{
			if (brakeHandleMoved)
			{
				KeyUp(this, new InputEventArgs(Controls[(int)InputTranslator.BrakeNotch]));
				brakeHandleMoved = false;
			}
			if (powerHandleMoved)
			{
				KeyUp(this, new InputEventArgs(Controls[(int)InputTranslator.PowerNotch + 10]));
				powerHandleMoved = false;
			}

			InputTranslator.Update();

			if (InputTranslator.IsControllerConnected)
			{
				if (InputTranslator.BrakeNotch != InputTranslator.PreviousBrakeNotch || loading)
				{
					KeyDown(this, new InputEventArgs(Controls[(int)InputTranslator.BrakeNotch]));
					brakeHandleMoved = true;
				}
				if (InputTranslator.PowerNotch != InputTranslator.PreviousPowerNotch || loading)
				{
					KeyDown(this, new InputEventArgs(Controls[(int)InputTranslator.PowerNotch + 10]));
					powerHandleMoved = true;
				}
			}

			loading = false;
		}

		public void SetElapseData(ElapseData data) {

			data.DebugMessage = InputTranslator.BrakeNotch.ToString();
}

		public void SetMaxNotch(int powerNotch, int brakeNotch) { }

		public void SetVehicleSpecs(VehicleSpecs specs)
		{
			vehicleSpecs = specs;
		}

		public void Unload() { }

		internal static void LoadConfig()
		{
			string optionsFolder = OpenBveApi.Path.CombineDirectory(FileSystem.SettingsFolder, "1.5.0");
			if (!System.IO.Directory.Exists(optionsFolder))
			{
				System.IO.Directory.CreateDirectory(optionsFolder);
			}
			string configFile = OpenBveApi.Path.CombineFile(optionsFolder, "options_denshadego.cfg");
			if (System.IO.File.Exists(configFile))
			{
				// load options
				string[] Lines = System.IO.File.ReadAllLines(configFile, new System.Text.UTF8Encoding());
				string Section = "";
				for (int i = 0; i < Lines.Length; i++)
				{
					Lines[i] = Lines[i].Trim(new char[] { });
					if (Lines[i].Length != 0 && !Lines[i].StartsWith(";", StringComparison.OrdinalIgnoreCase))
					{
						if (Lines[i].StartsWith("[", StringComparison.Ordinal) &
							Lines[i].EndsWith("]", StringComparison.Ordinal))
						{
							Section = Lines[i].Substring(1, Lines[i].Length - 2).Trim(new char[] { }).ToLowerInvariant();
						}
						else
						{
							int j = Lines[i].IndexOf("=", StringComparison.OrdinalIgnoreCase);
							string Key, Value;
							if (j >= 0)
							{
								Key = Lines[i].Substring(0, j).TrimEnd().ToLowerInvariant();
								Value = Lines[i].Substring(j + 1).TrimStart(new char[] { });
							}
							else
							{
								Key = "";
								Value = Lines[i];
							}
							switch (Section)
							{
								case "general":
									switch (Key)
									{
										case "controller":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													InputTranslator.activeControllerIndex = a;
												}
											}
											break;
									}
									break;
								case "playstation":
									switch (Key)
									{
										case "hat":
											PSController.UsesHat = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "hat_index":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													PSController.hatIndex = a;
												}
											}
											break;
										case "cross":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													PSController.ButtonIndex.Cross = a;
												}
											}
											break;
										case "square":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													PSController.ButtonIndex.Square = a;
												}
											}
											break;
										case "triangle":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													PSController.ButtonIndex.Triangle = a;
												}
											}
											break;
										case "circle":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													PSController.ButtonIndex.Circle = a;
												}
											}
											break;
										case "l1":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													PSController.ButtonIndex.L1 = a;
												}
											}
											break;
										case "l2":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													PSController.ButtonIndex.L2 = a;
												}
											}
											break;
										case "r1":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													PSController.ButtonIndex.R1 = a;
												}
											}
											break;
										case "r2":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													PSController.ButtonIndex.R2 = a;
												}
											}
											break;
										case "select":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													PSController.ButtonIndex.Select = a;
												}
											}
											break;
										case "start":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													PSController.ButtonIndex.Start = a;
												}
											}
											break;
										case "left":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													PSController.ButtonIndex.Left = a;
												}
											}
											break;
										case "right":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													PSController.ButtonIndex.Right = a;
												}
											}
											break;
									}
									break;
							}
						}
					}
				}
			}
		}

		internal static void SaveConfig()
		{
			try
			{
				CultureInfo Culture = CultureInfo.InvariantCulture;
				System.Text.StringBuilder Builder = new System.Text.StringBuilder();
				Builder.AppendLine("; Options");
				Builder.AppendLine("; =======");
				Builder.AppendLine("; This file was automatically generated. Please modify only if you know what you're doing.");
				Builder.AppendLine("; Specific options file for the Densha de GO! controller input plugin");
				Builder.AppendLine();
				Builder.AppendLine("[general]");
				Builder.AppendLine("controller = " + InputTranslator.activeControllerIndex.ToString(Culture));
				Builder.AppendLine();
				Builder.AppendLine("[playstation]");
				Builder.AppendLine("hat = " + PSController.UsesHat.ToString(Culture).ToLower());
				Builder.AppendLine("hat_index = " + PSController.hatIndex.ToString(Culture));
				Builder.AppendLine("cross = " + PSController.ButtonIndex.Cross.ToString(Culture));
				Builder.AppendLine("square = " + PSController.ButtonIndex.Square.ToString(Culture));
				Builder.AppendLine("triangle = " + PSController.ButtonIndex.Triangle.ToString(Culture));
				Builder.AppendLine("circle = " + PSController.ButtonIndex.Circle.ToString(Culture));
				Builder.AppendLine("l1 = " + PSController.ButtonIndex.L1.ToString(Culture));
				Builder.AppendLine("l2 = " + PSController.ButtonIndex.L2.ToString(Culture));
				Builder.AppendLine("r1 = " + PSController.ButtonIndex.R1.ToString(Culture));
				Builder.AppendLine("r2 = " + PSController.ButtonIndex.R2.ToString(Culture));
				Builder.AppendLine("select = " + PSController.ButtonIndex.Select.ToString(Culture));
				Builder.AppendLine("start = " + PSController.ButtonIndex.Start.ToString(Culture));
				Builder.AppendLine("left = " + PSController.ButtonIndex.Left.ToString(Culture));
				Builder.AppendLine("right = " + PSController.ButtonIndex.Right.ToString(Culture));
				string optionsFolder = OpenBveApi.Path.CombineDirectory(FileSystem.SettingsFolder, "1.5.0");
				string configFile = OpenBveApi.Path.CombineFile(optionsFolder, "options_denshadego.cfg");
				System.IO.File.WriteAllText(configFile, Builder.ToString(), new System.Text.UTF8Encoding(true));
			}
			catch
			{
				MessageBox.Show("An error occured whilst saving the options to disk." + System.Environment.NewLine +
								"Please check you have write permission.");
			}
		}
	}
}
