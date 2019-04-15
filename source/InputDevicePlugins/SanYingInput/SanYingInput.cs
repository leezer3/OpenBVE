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

		private int _reverserPos = 0;
		private int _lastReverserPos = -128;
		private int _handlePos = 0;
		private int _lastHandlePos = -128;

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
			m_configForm.loadConfigurationFile(settingsPath);
			m_configForm.Hide();

			_reverserPos = 0;
			_lastReverserPos = -128;
			_handlePos = 0;
			_lastHandlePos = -128;

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
				Controls[i].Command = Translations.Command.ReverserAnyPostion;
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
			_reverserPos = 0;
			_lastReverserPos = -128;
			_handlePos = 0;
			_lastHandlePos = -128;
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
				m_configForm.enumerateDevices();
				m_first = false;
			}

			JoystickApi.Update();

			setReverserPos();
			setHandlePos();
			setSwitchState();
		}

		protected virtual void OnKeyDown(InputEventArgs e)
		{
			if (KeyDown != null)
			{
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

		private void setSwitchState()
		{
			if (JoystickApi.currentDevice == -1)
			{
				return;
			}

			var currentButtonState = JoystickApi.GetButtonsState();
			int buttonNum = 6;

			if (currentButtonState.Count < buttonNum || JoystickApi.lastButtonState.Count < buttonNum)
			{
				return;
			}

			for (int i = 0; i < buttonNum; ++i)
			{
				if (currentButtonState[i] != JoystickApi.lastButtonState[i])
				{
					if (currentButtonState[i] == OpenTK.Input.ButtonState.Pressed)
					{
						int keyIdx = getKeyIdx(i);
						if (keyIdx != -1)
						{
							OnKeyDown(new InputEventArgs(Controls[keyIdx]));
						}
					}
					else if (currentButtonState[i] == OpenTK.Input.ButtonState.Released)
					{
						int keyIdx = getKeyIdx(i);
						if (keyIdx != -1)
						{
							OnKeyUp(new InputEventArgs(Controls[keyIdx]));
						}
					}
				}
			}
		}

		private int getKeyIdx(int i)
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

		private void setHandlePos()
		{
			var buttonsState = JoystickApi.GetButtonsState();
			if (InputTranslator.TranslateNotchPosition(buttonsState, out _handlePos))
			{
				return;
			}

			var isNotchIntermediateEstimated = false;

			// Problem between P1 and P2
			// https://twitter.com/SanYingOfficial/status/1088429762698129408
			//
			// P5 may be output when the notch is between P1 and P2.
			// This is an unintended output and should be excluded.
			if (_handlePos == 5)
			{
				if (_lastHandlePos == 1)
				{
					isNotchIntermediateEstimated = true;
				}
				else if (_lastHandlePos == 2)
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

				if (_handlePos != _lastHandlePos)
				{
					if (_handlePos <= 0)
					{
						OnKeyDown(new InputEventArgs(Controls[_handlePos + 9]));
					}
					if (_handlePos >= 0)
					{
						OnKeyDown(new InputEventArgs(Controls[_handlePos + 10]));
					}
				}
			}

			_lastHandlePos = _handlePos;
		}

		private void setReverserPos()
		{
			var axises = JoystickApi.GetAxises();
			InputTranslator.TranslateReverserPosition(axises, out _reverserPos);

			for (int i = 16; i < 19; i++)
			{
				OnKeyUp(new InputEventArgs(Controls[i]));
			}

			if (_reverserPos != _lastReverserPos)
			{
				OnKeyDown(new InputEventArgs(Controls[17 - _reverserPos]));
			}

			_lastReverserPos = _reverserPos;
		}
	}
}
