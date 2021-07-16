using System;
using System.Globalization;
using System.Windows.Forms;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Runtime;

namespace DefaultDisplayPlugin
{
	/// <summary>
	/// Informations Default Displaying Plugin (for Testing)
	/// </summary>
	public class DefaultDisplayPlugin : IInputDevice
	{
		/// <summary>
		/// Define KeyDown event
		/// </summary>
		public event EventHandler<InputEventArgs> KeyDown;

		/// <summary>
		/// Define KeyUp event
		/// </summary>
		public event EventHandler<InputEventArgs> KeyUp;

		/// <summary>
		/// The control list that is using for plugin
		/// </summary>
		public InputControl[] Controls { get; private set; }

		private static FileSystem FileSystem = null;
		private static int FrameCount = 0;
		internal static bool IsDisplayClock = false;
		internal static bool IsDisplaySpeed = false;
		internal static int SpeedDisplayMode = 0;
		internal static bool IsDisplayGradient = false;
		internal static int GradientDisplayMode = 0;
		internal static bool IsDisplayDistNextStation = false;
		internal static int DistNextStationDisplayMode = 0;
		internal static bool IsDisplayFps = false;

		/// <summary>
		/// A function call when the plugin is loading
		/// </summary>
		/// <param name="fileSystem">The instance of FileSytem class</param>
		/// <returns>Check the plugin loading process is successfully</returns>
		public bool Load(FileSystem fileSystem)
		{
			FrameCount = 0;
			FileSystem = fileSystem;
			Controls = new InputControl[5];
			Controls[0].Command = Translations.Command.MiscClock;
			Controls[1].Command = Translations.Command.MiscSpeed;
			Controls[2].Command = Translations.Command.MiscGradient;
			Controls[3].Command = Translations.Command.MiscDistanceToNextStation;
			Controls[4].Command = Translations.Command.MiscFps;
			LoadConfig();
			return true;
		}

		/// <summary>
		/// A function call when the plugin is unload
		/// </summary>
		public void Unload()
		{
			FrameCount = 0;
			FileSystem = null;
		}

		/// <summary>
		/// A funciton call when the Config button pressed
		/// </summary>
		/// <param name="owner">The owner of the window</param>
		public void Config(IWin32Window owner)
		{
			using (var form = new Config()) {
				form.ShowDialog(owner);
			}
		}

		/// <summary>
		/// The function what the notify to the plugin that the train maximum notches
		/// </summary>
		/// <param name="powerNotch">Maximum power notch number</param>
		/// <param name="brakeNotch">Maximum brake notch number</param>
		public void SetMaxNotch(int powerNotch, int brakeNotch)
		{
		}

		/// <summary>
		/// The function what notify to the plugin that the train existing status
		/// </summary>
		/// <param name="data">Data</param>
		public void SetElapseData(ElapseData data)
		{
		}

		/// <summary>
		/// A function that calls each frame
		/// </summary>
		public void OnUpdateFrame()
		{
			if (IsDisplayClock)
			{
				if (FrameCount < 1)
				{
					OnKeyDown(new InputEventArgs(Controls[0]));
				}
				else if (FrameCount == 1)
				{
					OnKeyUp(new InputEventArgs(Controls[0]));
				}
			}
			if (IsDisplaySpeed)
			{
				if (FrameCount < SpeedDisplayMode + 1)
				{
					OnKeyDown(new InputEventArgs(Controls[1]));
				}
				else if (FrameCount == SpeedDisplayMode + 1)
				{
					OnKeyUp(new InputEventArgs(Controls[1]));
				}
			}
			if (IsDisplayGradient)
			{
				if (FrameCount < GradientDisplayMode + 1)
				{
					OnKeyDown(new InputEventArgs(Controls[2]));
				}
				else if (FrameCount == GradientDisplayMode + 1)
				{
					OnKeyUp(new InputEventArgs(Controls[2]));
				}
			}
			if (IsDisplayDistNextStation)
			{
				if (FrameCount < DistNextStationDisplayMode + 1)
				{
					OnKeyDown(new InputEventArgs(Controls[3]));
				}
				else if (FrameCount == DistNextStationDisplayMode + 1)
				{
					OnKeyUp(new InputEventArgs(Controls[3]));
				}
			}
			if (IsDisplayFps)
			{
				if (FrameCount < 1)
				{
					OnKeyDown(new InputEventArgs(Controls[4]));
				}
				else if (FrameCount == 1)
				{
					OnKeyUp(new InputEventArgs(Controls[4]));
				}
			}
			if (FrameCount < 5)
			{
				FrameCount++;
			}
		}

		protected virtual void OnKeyDown(InputEventArgs e)
		{
			if (KeyDown != null) {
				KeyDown(this, e);
			}
		}

		protected virtual void OnKeyUp(InputEventArgs e)
		{
			if (KeyUp != null)
			{
				KeyUp(this, e);
			}
		}

		private void LoadConfig()
		{
			string optionsFolder = OpenBveApi.Path.CombineDirectory(FileSystem.SettingsFolder, "1.5.0");
			if (!System.IO.Directory.Exists(optionsFolder))
			{
				System.IO.Directory.CreateDirectory(optionsFolder);
			}
			string configFile = OpenBveApi.Path.CombineFile(optionsFolder, "options_ddp.cfg");
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
							Lines[i].EndsWith("]", StringComparison.Ordinal)) {
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
								case "clock":
									switch (Key)
									{
										case "is_display":
											IsDisplayClock = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
									}
									break;
								case "speed":
									switch (Key)
									{
										case "is_display":
											IsDisplaySpeed = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "display_mode":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													SpeedDisplayMode = a;
												}
											}
											break;
									}
									break;
								case "gradient":
									switch (Key)
									{
										case "is_display":
											IsDisplayGradient = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "display_mode":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													GradientDisplayMode = a;
												}
											}
											break;
									}
									break;
								case "dist_next_station":
									switch (Key)
									{
										case "is_display":
											IsDisplayDistNextStation = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
										case "display_mode":
											{
												int a;
												if (int.TryParse(Value, out a))
												{
													DistNextStationDisplayMode = a;
												}
											}
											break;
									}
									break;
								case "fps":
									switch (Key)
									{
										case "is_display":
											IsDisplayFps = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
											break;
									}
									break;
							}
						}
					}
				}
			}
		}

		internal static void SaveConfig() {
			try {
				CultureInfo Culture = CultureInfo.InvariantCulture;
				System.Text.StringBuilder Builder = new System.Text.StringBuilder();
				Builder.AppendLine("; Options");
				Builder.AppendLine("; =======");
				Builder.AppendLine("; This file was automatically generated. Please modify only if you know what you're doing.");
				Builder.AppendLine("; Test Plugin specific options file");
				Builder.AppendLine();
				Builder.AppendLine("[clock]");
				Builder.AppendLine("is_display = " + IsDisplayClock.ToString(Culture));
				Builder.AppendLine();
				Builder.AppendLine("[speed]");
				Builder.AppendLine("is_display = " + IsDisplaySpeed.ToString(Culture));
				Builder.AppendLine("display_mode = " + SpeedDisplayMode.ToString(Culture));
				Builder.AppendLine();
				Builder.AppendLine("[gradient]");
				Builder.AppendLine("is_display = " + IsDisplayGradient.ToString(Culture));
				Builder.AppendLine("display_mode = " + GradientDisplayMode.ToString(Culture));
				Builder.AppendLine();
				Builder.AppendLine("[dist_next_station]");
				Builder.AppendLine("is_display = " + IsDisplayDistNextStation.ToString(Culture));
				Builder.AppendLine("display_mode = " + DistNextStationDisplayMode.ToString(Culture));
				Builder.AppendLine();
				Builder.AppendLine("[fps]");
				Builder.AppendLine("is_display = " + IsDisplayFps.ToString(Culture));
				string configFile = OpenBveApi.Path.CombineFile(FileSystem.SettingsFolder, "1.5.0/options_ddp.cfg");
				System.IO.File.WriteAllText(configFile, Builder.ToString(), new System.Text.UTF8Encoding(true));
			} catch {
				MessageBox.Show("An error occured whilst saving the options to disk." + System.Environment.NewLine +
								"Please check you have write permission.");
			}
		}
	}
}
