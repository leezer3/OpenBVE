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
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace SanYingInput
{

	public partial class ConfigForm : Form
	{

		public struct ConfigFormSaveData
		{
			public Guid guid;

			public int switchS;
			public int switchA1;
			public int switchA2;
			public int switchB1;
			public int switchB2;
			public int switchC1;
			public int switchC2;
			public int switchD;
			public int switchE;
			public int switchF;
			public int switchG;
			public int switchH;
			public int switchI;
			public int switchJ;
			public int switchK;
			public int switchL;
			public int switchReverserFront;
			public int switchReverserNeutral;
			public int switchReverserBack;
			public int switchHorn1;
			public int switchHorn2;
			public int switchMusicHorn;
			public int switchConstSpeed;
		};

		public enum AxisType
		{
			axisNothing = 0,
			axisX = 1,
			axisY,
			axisZ,
			axisRx,
			axisRy,
			axisRz,
		}

		private ConfigFormSaveData m_saveData;
		private const string FileName = "SanYingInput.xml";

		private string m_configFilePath;

		private int _notchPosition = 0;
		private int _reverserPosition = 0;

		public ConfigFormSaveData Configuration
		{
			get
			{
				return m_saveData;
			}
		}

		public void loadConfigurationFile(string path)
		{
			m_configFilePath = path;

			try
			{
				XmlSerializer serializer = new XmlSerializer(typeof(ConfigFormSaveData));
				FileStream fs = new FileStream(Path.Combine(path, FileName), FileMode.Open);
				m_saveData = (ConfigFormSaveData)serializer.Deserialize(fs);
				fs.Close();
			}
			catch
			{
			}
		}

		public void saveConfigurationFile(string path)
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			path = Path.Combine(path, FileName);

			XmlSerializer serializer = new XmlSerializer(typeof(ConfigFormSaveData));
			FileStream fs = new FileStream(path, FileMode.Create);

			serializer.Serialize(fs, m_saveData);
			fs.Close();
		}

		public ConfigForm()
		{
			InitializeComponent();

			List<string> axisArray = new List<string>();

			axisArray.Add("OFF");
			axisArray.Add("X");
			axisArray.Add("Y");
			axisArray.Add("Z");
			axisArray.Add("Rx");
			axisArray.Add("Ry");
			axisArray.Add("Rz");

			m_saveData = new ConfigFormSaveData();

			m_saveData.switchS = fromSwitchString("OFF");
			m_saveData.switchA1 = fromSwitchString("OFF");
			m_saveData.switchA2 = fromSwitchString("OFF");
			m_saveData.switchB1 = fromSwitchString("OFF");
			m_saveData.switchB2 = fromSwitchString("OFF");
			m_saveData.switchC1 = fromSwitchString("OFF");
			m_saveData.switchC2 = fromSwitchString("OFF");
			m_saveData.switchD = fromSwitchString("OFF");
			m_saveData.switchE = fromSwitchString("OFF");
			m_saveData.switchF = fromSwitchString("OFF");
			m_saveData.switchG = fromSwitchString("OFF");
			m_saveData.switchH = fromSwitchString("OFF");
			m_saveData.switchI = fromSwitchString("OFF");
			m_saveData.switchJ = fromSwitchString("OFF");
			m_saveData.switchK = fromSwitchString("OFF");
			m_saveData.switchL = fromSwitchString("OFF");
			m_saveData.switchReverserFront = fromSwitchString("OFF");
			m_saveData.switchReverserNeutral = fromSwitchString("OFF");
			m_saveData.switchReverserBack = fromSwitchString("OFF");
			m_saveData.switchHorn1 = fromSwitchString("OFF");
			m_saveData.switchHorn2 = fromSwitchString("OFF");
			m_saveData.switchMusicHorn = fromSwitchString("OFF");
			m_saveData.switchConstSpeed = fromSwitchString("OFF");
		}

		private string toSwitchString(int n)
		{
			switch (n)
			{
				case -1:
					return "OFF";
				default:
					return toString(n);
			}
		}

		private int fromSwitchString(string s)
		{
			switch (s)
			{
				case "OFF":
					return -1;
				default:
					return fromString(s);
			}
		}

		private void restoreConfiguration(ConfigFormSaveData saveData)
		{
			txtSwS.Text = toSwitchString(saveData.switchS);
			txtSwA1.Text = toSwitchString(saveData.switchA1);
			txtSwA2.Text = toSwitchString(saveData.switchA2);
			txtSwB1.Text = toSwitchString(saveData.switchB1);
			txtSwB2.Text = toSwitchString(saveData.switchB2);
			txtSwC1.Text = toSwitchString(saveData.switchC1);
			txtSwC2.Text = toSwitchString(saveData.switchC2);
			txtSwD.Text = toSwitchString(saveData.switchD);
			txtSwE.Text = toSwitchString(saveData.switchE);
			txtSwF.Text = toSwitchString(saveData.switchF);
			txtSwG.Text = toSwitchString(saveData.switchG);
			txtSwH.Text = toSwitchString(saveData.switchH);
			txtSwI.Text = toSwitchString(saveData.switchI);
			txtSwJ.Text = toSwitchString(saveData.switchJ);
			txtSwK.Text = toSwitchString(saveData.switchK);
			txtSwL.Text = toSwitchString(saveData.switchL);
			txtSwReverserFront.Text = toSwitchString(saveData.switchReverserFront);
			txtSwReverserNeutral.Text = toSwitchString(saveData.switchReverserNeutral);
			txtSwReverserBack.Text = toSwitchString(saveData.switchReverserBack);
			txtSwHorn1.Text = toSwitchString(saveData.switchHorn1);
			txtSwHorn2.Text = toSwitchString(saveData.switchHorn2);
			txtSwMusicHorn.Text = toSwitchString(saveData.switchMusicHorn);
			txtSwConstSpeed.Text = toSwitchString(saveData.switchConstSpeed);
		}

		private ConfigFormSaveData saveConfiguration()
		{
			ConfigFormSaveData saveData = new ConfigFormSaveData();

			if (JoystickApi.currentDevice != -1)
			{
				saveData.guid = JoystickApi.GetGuid();
			}

			saveData.switchS = fromSwitchString(txtSwS.Text);
			saveData.switchA1 = fromSwitchString(txtSwA1.Text);
			saveData.switchA2 = fromSwitchString(txtSwA2.Text);
			saveData.switchB1 = fromSwitchString(txtSwB1.Text);
			saveData.switchB2 = fromSwitchString(txtSwB2.Text);
			saveData.switchC1 = fromSwitchString(txtSwC1.Text);
			saveData.switchC2 = fromSwitchString(txtSwC2.Text);
			saveData.switchD = fromSwitchString(txtSwD.Text);
			saveData.switchE = fromSwitchString(txtSwE.Text);
			saveData.switchF = fromSwitchString(txtSwF.Text);
			saveData.switchG = fromSwitchString(txtSwG.Text);
			saveData.switchH = fromSwitchString(txtSwH.Text);
			saveData.switchI = fromSwitchString(txtSwI.Text);
			saveData.switchJ = fromSwitchString(txtSwJ.Text);
			saveData.switchK = fromSwitchString(txtSwK.Text);
			saveData.switchL = fromSwitchString(txtSwL.Text);
			saveData.switchReverserFront = fromSwitchString(txtSwReverserFront.Text);
			saveData.switchReverserNeutral = fromSwitchString(txtSwReverserNeutral.Text);
			saveData.switchReverserBack = fromSwitchString(txtSwReverserBack.Text);
			saveData.switchHorn1 = fromSwitchString(txtSwHorn1.Text);
			saveData.switchHorn2 = fromSwitchString(txtSwHorn2.Text);
			saveData.switchMusicHorn = fromSwitchString(txtSwMusicHorn.Text);
			saveData.switchConstSpeed = fromSwitchString(txtSwConstSpeed.Text);

			return saveData;
		}

		public void enumerateDevices()
		{
			JoystickApi.EnumerateJoystick();
			int joyNum = JoystickManager.AttachedJoysticks.Length;

			cmbJoySelect.Items.Clear();

			if (joyNum == 0) return;

			cmbJoySelect.MaxDropDownItems = joyNum;

			for (int i = 0; i < joyNum; ++i)
			{
				cmbJoySelect.Items.Add(JoystickManager.AttachedJoysticks[i].Name);

				if (m_saveData.guid == JoystickManager.AttachedJoysticks[i].Guid)
				{
					JoystickApi.SelectJoystick(i);
					cmbJoySelect.SelectedIndex = i;
				}
			}

			if (cmbJoySelect.SelectedIndex == -1)
			{
				cmbJoySelect.SelectedIndex = joyNum - 1;
			}
		}

		private string toString(int n)
		{
			return (n + 1).ToString();
		}

		private int fromString(string s)
		{
			int n = 0;

			try
			{
				n = int.Parse(s);
			}
			catch
			{
				n = 0;
			}

			return n - 1;
		}

		private void ConfigForm_Load(object sender, EventArgs e)
		{
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			m_saveData = saveConfiguration();
			saveConfigurationFile(m_configFilePath);
			this.Close();
		}

		private void btnClose_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void configurateSwitch()
		{
			int buttonNum = 6;

			var currentButtonState = JoystickApi.GetButtonsState();

			if (currentButtonState.Count < buttonNum)
				return;

			if (JoystickApi.lastButtonState.Count < buttonNum)
				return;

			for (int i = 0; i < buttonNum; ++i)
			{
				if (currentButtonState[i] != JoystickApi.lastButtonState[i])
				{
					if (txtSwS.Focused)
					{
						txtSwS.Text = toSwitchString(i);
					}
					else if (txtSwA1.Focused)
					{
						txtSwA1.Text = toSwitchString(i);
					}
					else if (txtSwA2.Focused)
					{
						txtSwA2.Text = toSwitchString(i);
					}
					else if (txtSwB1.Focused)
					{
						txtSwB1.Text = toSwitchString(i);
					}
					else if (txtSwB2.Focused)
					{
						txtSwB2.Text = toSwitchString(i);
					}
					else if (txtSwC1.Focused)
					{
						txtSwC1.Text = toSwitchString(i);
					}
					else if (txtSwC2.Focused)
					{
						txtSwC2.Text = toSwitchString(i);
					}
					else if (txtSwD.Focused)
					{
						txtSwD.Text = toSwitchString(i);
					}
					else if (txtSwE.Focused)
					{
						txtSwE.Text = toSwitchString(i);
					}
					else if (txtSwF.Focused)
					{
						txtSwF.Text = toSwitchString(i);
					}
					else if (txtSwG.Focused)
					{
						txtSwG.Text = toSwitchString(i);
					}
					else if (txtSwH.Focused)
					{
						txtSwH.Text = toSwitchString(i);
					}
					else if (txtSwI.Focused)
					{
						txtSwI.Text = toSwitchString(i);
					}
					else if (txtSwJ.Focused)
					{
						txtSwJ.Text = toSwitchString(i);
					}
					else if (txtSwK.Focused)
					{
						txtSwK.Text = toSwitchString(i);
					}
					else if (txtSwL.Focused)
					{
						txtSwL.Text = toSwitchString(i);
					}
					else if (txtSwReverserFront.Focused)
					{
						txtSwReverserFront.Text = toSwitchString(i);
					}
					else if (txtSwReverserNeutral.Focused)
					{
						txtSwReverserNeutral.Text = toSwitchString(i);
					}
					else if (txtSwReverserBack.Focused)
					{
						txtSwReverserBack.Text = toSwitchString(i);
					}
					else if (txtSwHorn1.Focused)
					{
						txtSwHorn1.Text = toSwitchString(i);
					}
					else if (txtSwHorn2.Focused)
					{
						txtSwHorn2.Text = toSwitchString(i);
					}
					else if (txtSwMusicHorn.Focused)
					{
						txtSwMusicHorn.Text = toSwitchString(i);
					}
					else if (txtSwConstSpeed.Focused)
					{
						txtSwConstSpeed.Text = toSwitchString(i);
					}

					break;
				}
			}
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			JoystickApi.Update();

			if (JoystickApi.currentDevice != -1)
			{
				var buttonsState = JoystickApi.GetButtonsState();
				var axises = JoystickApi.GetAxises();

				int lastNotchPosition = _notchPosition;
				if (InputTranslator.TranslateNotchPosition(buttonsState, out _notchPosition))
				{
					_notchPosition = lastNotchPosition;
				}
				InputTranslator.TranslateReverserPosition(axises, out _reverserPosition);

				{
					uint notchButtonsState;

					InputTranslator.MakeBitFromNotchButtons(buttonsState, out notchButtonsState);

					txtInfoBt7.Text = ((notchButtonsState & (uint)InputTranslator.BT7_10_Pressed.BT7) != 0) ? "1" : "0";
					txtInfoBt8.Text = ((notchButtonsState & (uint)InputTranslator.BT7_10_Pressed.BT8) != 0) ? "1" : "0";
					txtInfoBt9.Text = ((notchButtonsState & (uint)InputTranslator.BT7_10_Pressed.BT9) != 0) ? "1" : "0";
					txtInfoBt10.Text = ((notchButtonsState & (uint)InputTranslator.BT7_10_Pressed.BT10) != 0) ? "1" : "0";
				}

				{
					if (_reverserPosition > 0)
					{
						txtInfoUp.Text = "1";
						txtInfoDown.Text = "0";
					}
					else if (_reverserPosition < 0)
					{
						txtInfoUp.Text = "0";
						txtInfoDown.Text = "1";
					}
					else
					{
						txtInfoUp.Text = "0";
						txtInfoDown.Text = "0";
					}
				}

				var notchPositionString = string.Empty;
				var reverserPositionString = string.Empty;

				if (_notchPosition > 0)
				{
					notchPositionString = string.Format("P{0}", _notchPosition);
				}
				else if (_notchPosition < 0)
				{
					if (_notchPosition > -9)
					{
						notchPositionString = string.Format("B{0}", _notchPosition);
					}
					else
					{
						notchPositionString = "EB";
					}
				}
				else
				{
					notchPositionString = "N";
				}

				if (_reverserPosition > 0)
				{
					reverserPositionString = "F";
				}
				else if (_reverserPosition < 0)
				{
					reverserPositionString = "B";
				}
				else
				{
					reverserPositionString = "N";
				}

				labelTch.Text = notchPositionString;
				labelReverser.Text = reverserPositionString;

				configurateSwitch();
			}
			else
			{
				enumerateDevices();
			}
		}

		private void ConfigForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			timer1.Enabled = false;
		}

		private void ConfigForm_Shown(object sender, EventArgs e)
		{
			timer1.Enabled = true;

			enumerateDevices();
			restoreConfiguration(m_saveData);
		}

		private void cmbJoySelect_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (cmbJoySelect.SelectedIndex != -1)
			{
				JoystickApi.SelectJoystick(cmbJoySelect.SelectedIndex);
			}
		}

		private void deconfigurateSwitch(object sender, KeyEventArgs e)
		{
			TextBox me = (TextBox)sender;

			if (e.KeyCode == Keys.Delete)
			{
				me.Text = "OFF";
			}
		}
	}
}
