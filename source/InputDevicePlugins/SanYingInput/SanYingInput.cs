//The MIT License (MIT)
//
//Copyright (c) 2017 Rock_On, 2018 The openBVE Project
//
//Permission is hereby granted, free of charge, to any person obtaining a copy of
//this software and associated documentation files (the "Software"), to deal in
//the Software without restriction, including without limitation the rights to
//use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
//the Software, and to permit persons to whom the Software is furnished to do so,
//subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
//FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
//COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
//IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
//CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Windows.Forms;
using OpenBveApi.FileSystem;
using OpenBveApi.Interface;
using OpenBveApi.Runtime;

namespace SanYingInput
{
	/// <summary>
	/// Input Device Plugin class for OHC-PC01
	/// </summary>
	public class SanYingInput : IInputDevice
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

		private ConfigForm m_configForm;
		private bool m_pauseTick;
		private bool m_first;
		/// <summary>The current reverser position</summary>
		private int currentReverserPosition = 0;
		/// <summary>The reverser position at the previous update</summary>
		private int previousReversorPosition = -128;
		/// <summary>The current handle positions</summary>
		private int currentHandlePosition = 0;
		/// <summary>The handle positions at the previous update</summary>
		private int previousHandlePosition = -128;

		/// <summary>
		/// A function call when the plugin is loading
		/// </summary>
		/// <param name="fileSystem">The instance of FileSytem class</param>
		/// <returns>Check the plugin loading process is successfully</returns>
		public bool Load(FileSystem fileSystem)
		{
			m_first = true;
			JoystickApi.Init();

			m_configForm = new ConfigForm();
			string settingsPath = fileSystem.SettingsFolder + System.IO.Path.DirectorySeparatorChar + "1.5.0";
			m_configForm.LoadConfigurationFile(settingsPath);
			m_configForm.Hide();

			currentReverserPosition = 0;
			previousReversorPosition = -128;
			currentHandlePosition = 0;
			previousHandlePosition = -128;

			Controls = new InputControl[39];
			Controls[0].Command = Translations.Command.BrakeEmergency;
			for (int i = 1; i < 10; i++)
			{
				Controls[i].Command = Translations.Command.BrakeAnyNotch;
				Controls[i].Option = 9 - i;
			}
			for (int i = 10; i < 16; i++)
			{
				Controls[i].Command = Translations.Command.PowerAnyNotch;
				Controls[i].Option = i - 10;
			}
			for (int i = 16; i < 19; i++)
			{
				Controls[i].Command = Translations.Command.ReverserAnyPosition;
				Controls[i].Option = 17 - i;
			}
#pragma warning disable 618
			Controls[19].Command = Translations.Command.SecurityS;
			Controls[20].Command = Translations.Command.SecurityA1;
			Controls[21].Command = Translations.Command.SecurityA2;
			Controls[22].Command = Translations.Command.SecurityB1;
			Controls[23].Command = Translations.Command.SecurityB2;
			Controls[24].Command = Translations.Command.SecurityC1;
			Controls[25].Command = Translations.Command.SecurityC2;
			Controls[26].Command = Translations.Command.SecurityD;
			Controls[27].Command = Translations.Command.SecurityE;
			Controls[28].Command = Translations.Command.SecurityF;
			Controls[29].Command = Translations.Command.SecurityG;
			Controls[30].Command = Translations.Command.SecurityH;
			Controls[31].Command = Translations.Command.SecurityI;
			Controls[32].Command = Translations.Command.SecurityJ;
			Controls[33].Command = Translations.Command.SecurityK;
			Controls[34].Command = Translations.Command.SecurityL;
#pragma warning restore 618
			Controls[35].Command = Translations.Command.HornPrimary;
			Controls[36].Command = Translations.Command.HornSecondary;
			Controls[37].Command = Translations.Command.HornMusic;
			Controls[38].Command = Translations.Command.DeviceConstSpeed;
			return true;
		}

		/// <summary>
		/// A function call when the plugin is unload
		/// </summary>
		public void Unload()
		{
			m_configForm.Dispose();
		}

		/// <summary>
		/// A funciton call when the Config button pressed
		/// </summary>
		/// <param name="owner">The owner of the window</param>
		public void Config(IWin32Window owner)
		{
			m_first = false;
			m_pauseTick = true;
			m_configForm.ShowDialog(owner);
			m_pauseTick = false;
		}

		/// <summary>
		/// The function what the notify to the plugin that the train maximum notches
		/// </summary>
		/// <param name="powerNotch">Maximum power notch number</param>
		/// <param name="brakeNotch">Maximum brake notch number</param>
		public void SetMaxNotch(int powerNotch, int brakeNotch)
		{
			currentReverserPosition = 0;
			previousReversorPosition = -128;
			currentHandlePosition = 0;
			previousHandlePosition = -128;
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
			if (m_pauseTick)
			{
				return;
			}

			if (m_first)
			{
				m_configForm.EnumerateDevices();
				m_first = false;
			}

			JoystickApi.Update();

			SetReverserPosition();
			SetHandlePosition();
			SetSwitchState();
		}

		protected virtual void OnKeyDown(InputEventArgs e)
		{
			KeyDown?.Invoke(this, e);
		}

		protected virtual void OnKeyUp(InputEventArgs e)
		{
			KeyUp?.Invoke(this, e);
		}

		private void SetSwitchState()
		{
			if (JoystickApi.CurrentDevice == -1)
			{
				return;
			}

			var currentButtonState = JoystickApi.GetButtonsState();
			const int buttonNum = 6;

			if (currentButtonState.Count < buttonNum || JoystickApi.lastButtonState.Count < buttonNum)
			{
				// not SanYing compatible
				return;
			}

			for (int i = 0; i < buttonNum; i++)
			{
				if (currentButtonState[i] != JoystickApi.lastButtonState[i])
				{
					if (currentButtonState[i] == OpenTK.Input.ButtonState.Pressed)
					{
						int keyIdx = GetKeyIdx(i);
						if (keyIdx != -1)
						{
							OnKeyDown(new InputEventArgs(Controls[keyIdx]));
						}
					}
					else if (currentButtonState[i] == OpenTK.Input.ButtonState.Released)
					{
						int keyIdx = GetKeyIdx(i);
						if (keyIdx != -1)
						{
							OnKeyUp(new InputEventArgs(Controls[keyIdx]));
						}
					}
				}
			}
		}

		private int GetKeyIdx(int i)
		{
			int keyIdx = -1;
			ConfigForm.ConfigFormSaveData config = m_configForm.Configuration;

			if (config.switchS == i)
			{
				keyIdx = 19;
			}
			else if (config.switchA1 == i)
			{
				keyIdx = 20;
			}
			else if (config.switchA2 == i)
			{
				keyIdx = 21;
			}
			else if (config.switchB1 == i)
			{
				keyIdx = 22;
			}
			else if (config.switchB2 == i)
			{
				keyIdx = 23;
			}
			else if (config.switchC1 == i)
			{
				keyIdx = 24;
			}
			else if (config.switchC2 == i)
			{
				keyIdx = 25;
			}
			else if (config.switchD == i)
			{
				keyIdx = 26;
			}
			else if (config.switchE == i)
			{
				keyIdx = 27;
			}
			else if (config.switchF == i)
			{
				keyIdx = 28;
			}
			else if (config.switchG == i)
			{
				keyIdx = 29;
			}
			else if (config.switchH == i)
			{
				keyIdx = 30;
			}
			else if (config.switchI == i)
			{
				keyIdx = 31;
			}
			else if (config.switchJ == i)
			{
				keyIdx = 32;
			}
			else if (config.switchK == i)
			{
				keyIdx = 33;
			}
			else if (config.switchL == i)
			{
				keyIdx = 34;
			}
			else if (config.switchReverserFront == i)
			{
				keyIdx = 16;
			}
			else if (config.switchReverserNeutral == i)
			{
				keyIdx = 17;
			}
			else if (config.switchReverserBack == i)
			{
				keyIdx = 18;
			}
			else if (config.switchHorn1 == i)
			{
				keyIdx = 35;
			}
			else if (config.switchHorn2 == i)
			{
				keyIdx = 36;
			}
			else if (config.switchMusicHorn == i)
			{
				keyIdx = 37;
			}
			else if (config.switchConstSpeed == i)
			{
				keyIdx = 38;
			}

			return keyIdx;
		}

		private void SetHandlePosition()
		{
			var buttonsState = JoystickApi.GetButtonsState();
			if (InputTranslator.TranslateNotchPosition(buttonsState, out currentHandlePosition))
			{
				return;
			}

			var isNotchIntermediateEstimated = false;

			// Problem between P1 and P2
			// https://twitter.com/SanYingOfficial/status/1088429762698129408
			//
			// P5 may be output when the notch is between P1 and P2.
			// This is an unintended output and should be excluded.
			if (currentHandlePosition == 5)
			{
				if (previousHandlePosition == 1)
				{
					isNotchIntermediateEstimated = true;
				}
				else if (previousHandlePosition == 2)
				{
					isNotchIntermediateEstimated = true;
				}
			}

			if (!isNotchIntermediateEstimated)
			{
				for (int i = 0; i < 16; i++)
				{
					OnKeyUp(new InputEventArgs(Controls[i]));
				}

				if (currentHandlePosition != previousHandlePosition)
				{
					if (currentHandlePosition <= 0)
					{
						OnKeyDown(new InputEventArgs(Controls[currentHandlePosition + 9]));
					}
					if (currentHandlePosition >= 0)
					{
						OnKeyDown(new InputEventArgs(Controls[currentHandlePosition + 10]));
					}
				}
			}

			previousHandlePosition = currentHandlePosition;
		}

		private void SetReverserPosition()
		{
			var axises = JoystickApi.GetAxisStates();
			InputTranslator.TranslateReverserPosition(axises, out currentReverserPosition);

			for (int i = 16; i < 19; i++)
			{
				OnKeyUp(new InputEventArgs(Controls[i]));
			}

			if (currentReverserPosition != previousReversorPosition)
			{
				OnKeyDown(new InputEventArgs(Controls[17 - currentReverserPosition]));
			}

			previousReversorPosition = currentReverserPosition;
		}
	}
}
