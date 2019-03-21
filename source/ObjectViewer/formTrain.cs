using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenBveApi.Trains;

namespace OpenBve
{
	public partial class formTrain : Form
	{
		private class PluginState
		{
			internal int Number;
			internal int State;

			internal PluginState(int number, int state)
			{
				Number = number;
				State = state;
			}
		}

		private List<PluginState> PluginStates;

		private static Task FormTrainTask;

		private formTrain()
		{
			InitializeComponent();

			checkBoxEnableTrain.Checked = TrainManager.Trains.Length != 0;
			tabControlSettings.Enabled = checkBoxEnableTrain.Checked;

			labelEmergency.Enabled = false;
			numericUpDownEmergency.Enabled = false;
			checkBoxSetHoldBrake.Enabled = false;
			checkBoxSetConstSpeed.Enabled = false;

			if (checkBoxEnableTrain.Checked)
			{
				numericUpDownCars.Value = TrainManager.Trains[0].Cars.Length;
				numericUpDownSpeed.Value = (decimal)(TrainManager.Trains[0].Cars[0].Specs.CurrentSpeed * 3.6);
				numericUpDownAccel.Value = (decimal)(TrainManager.Trains[0].Cars[0].Specs.CurrentAcceleration * 3.6);

				numericUpDownMain.Value = (decimal)(TrainManager.Trains[0].Cars[0].Specs.AirBrake.MainReservoirCurrentPressure / 1000.0);
				numericUpDownPipe.Value = (decimal)(TrainManager.Trains[0].Cars[0].Specs.AirBrake.BrakePipeCurrentPressure / 1000.0);
				numericUpDownCylinder.Value = (decimal)(TrainManager.Trains[0].Cars[0].Specs.AirBrake.BrakeCylinderCurrentPressure / 1000.0);
				numericUpDownAirPipe.Value = (decimal)(TrainManager.Trains[0].Cars[0].Specs.AirBrake.StraightAirPipeCurrentPressure / 1000.0);

				numericUpDownLeft.Value = (decimal)TrainManager.Trains[0].Cars[0].Specs.Doors[0].State;
				numericUpDownRight.Value = (decimal)TrainManager.Trains[0].Cars[0].Specs.Doors[1].State;
				checkBoxLeftTarget.Checked = TrainManager.Trains[0].Cars[0].Specs.AnticipatedLeftDoorsOpened;
				checkBoxRightTarget.Checked = TrainManager.Trains[0].Cars[0].Specs.AnticipatedRightDoorsOpened;

				numericUpDownReverser.Value = TrainManager.Trains[0].Specs.CurrentReverser.Driver;
				numericUpDownPowerNotch.Value = TrainManager.Trains[0].Specs.CurrentPowerNotch.Driver;
				numericUpDownPowerNotches.Value = TrainManager.Trains[0].Specs.MaximumPowerNotch;
				checkBoxAirBrake.Checked = TrainManager.Trains[0].Cars[0].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake;
				if (checkBoxAirBrake.Checked)
				{
					numericUpDownBrakeNotch.Value = (int)TrainManager.Trains[0].Specs.AirBrake.Handle.Driver;
					numericUpDownBrakeNotch.Maximum = 2;
					numericUpDownBrakeNotches.Value = 2;
					numericUpDownBrakeNotches.Enabled = false;
					checkBoxHoldBrake.Enabled = false;
				}
				else
				{
					numericUpDownBrakeNotch.Value = TrainManager.Trains[0].Specs.CurrentBrakeNotch.Driver;
					numericUpDownBrakeNotches.Value = TrainManager.Trains[0].Specs.MaximumBrakeNotch;
					checkBoxHoldBrake.Checked = TrainManager.Trains[0].Specs.HasHoldBrake;
					if (checkBoxHoldBrake.Checked)
					{
						checkBoxSetHoldBrake.Enabled = true;
						checkBoxSetHoldBrake.Checked = TrainManager.Trains[0].Specs.CurrentHoldBrake.Driver;
					}
				}
				checkBoxSetEmergency.Checked = TrainManager.Trains[0].Specs.CurrentEmergencyBrake.Driver;
				checkBoxConstSpeed.Checked = TrainManager.Trains[0].Specs.HasConstSpeed;
				if (checkBoxConstSpeed.Checked)
				{
					checkBoxSetConstSpeed.Enabled = true;
					checkBoxSetConstSpeed.Checked = TrainManager.Trains[0].Specs.CurrentConstSpeed;
				}

				checkBoxEnablePlugin.Checked = TrainManager.Trains[0].Specs.Safety.Mode == TrainManager.SafetySystem.Plugin;
			}
			panelPlugin.Enabled = checkBoxEnablePlugin.Checked;
			buttonRemove.Enabled = false;
			labelNumber.Enabled = false;
			numericUpDownNumber.Enabled = false;
			labelState.Enabled = false;
			numericUpDownState.Enabled = false;
			PluginStates = new List<PluginState>();
			for (int i = 0; i < PluginManager.CurrentPlugin.Panel.Length; i++)
			{
				if (PluginManager.CurrentPlugin.Panel[i] != 0)
				{
					PluginStates.Add(new PluginState(i, PluginManager.CurrentPlugin.Panel[i]));
				}
			}
			ListPluginStates();
		}

		internal static void ShowTrainSettings()
		{
			if (FormTrainTask == null || FormTrainTask.IsCompleted)
			{
				FormTrainTask = Task.Factory.StartNew(() =>
				{
					formTrain dialog = new formTrain();
					dialog.ShowDialog();
				});
			}
		}

		private void checkBoxEnableTrain_Check(object sender, EventArgs e)
		{
			tabControlSettings.Enabled = checkBoxEnableTrain.Checked;
		}

		private void checkBoxAirBrake_Check(object sender, EventArgs e)
		{
			if (checkBoxAirBrake.Checked)
			{
				if (numericUpDownBrakeNotch.Value > 2)
				{
					numericUpDownBrakeNotch.Value = 2;
				}
				numericUpDownBrakeNotch.Maximum = 2;
				numericUpDownBrakeNotches.Value = 2;
				numericUpDownBrakeNotches.Enabled = false;
				checkBoxHoldBrake.Checked = false;
				checkBoxHoldBrake.Enabled = false;
				checkBoxSetHoldBrake.Checked = false;
				checkBoxSetHoldBrake.Enabled = false;
			}
			else
			{
				numericUpDownBrakeNotch.Maximum = 100;
				numericUpDownBrakeNotches.Enabled = true;
				checkBoxHoldBrake.Enabled = true;
			}
		}

		private void checkBoxHoldBrake_Check(object sender, EventArgs e)
		{
			if (checkBoxHoldBrake.Checked)
			{
				checkBoxSetHoldBrake.Enabled = true;
			}
			else
			{
				checkBoxSetHoldBrake.Checked = false;
				checkBoxSetHoldBrake.Enabled = false;
			}
		}

		private void checkBoxConstSpeed_Check(object sender, EventArgs e)
		{
			if (checkBoxConstSpeed.Checked)
			{
				checkBoxSetConstSpeed.Enabled = true;
			}
			else
			{
				checkBoxSetConstSpeed.Checked = false;
				checkBoxSetConstSpeed.Enabled = false;
			}
		}

		private void checkBoxEnablePlugin_Check(object sender, EventArgs e)
		{
			panelPlugin.Enabled = checkBoxEnablePlugin.Checked;
		}

		private void ListPluginStates()
		{
			ListViewItem[] items = new ListViewItem[PluginStates.Count];
			for (int i = 0; i < items.Length; i++)
			{
				items[i] = new ListViewItem(new string[] { "", "" });
				UpdatePluginListViewItem(items[i], i, false);
			}
			listViewPlugin.Items.AddRange(items);
			if (items.Length != 0)
			{
				listViewPlugin.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
			}
			else
			{
				listViewPlugin.AutoResizeColumns(ColumnHeaderAutoResizeStyle.None);
			}
		}

		private void UpdatePluginListViewItem(ListViewItem item, int index, bool resizeColumns)
		{
			item.SubItems[0].Text = PluginStates[index].Number.ToString();
			item.SubItems[1].Text = PluginStates[index].State.ToString();
			if (resizeColumns)
			{
				listViewPlugin.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
			}
		}

		private void UpdatePluginComponent(PluginState state)
		{
			buttonRemove.Enabled = true;
			labelNumber.Enabled = true;
			numericUpDownNumber.Enabled = true;
			numericUpDownNumber.Value = state.Number;
			labelState.Enabled = true;
			numericUpDownState.Enabled = true;
			numericUpDownState.Value = state.State;
		}

		private void listViewPlugin_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (listViewPlugin.SelectedIndices.Count == 1)
			{
				int i = listViewPlugin.SelectedIndices[0];
				Tag = new object();
				UpdatePluginComponent(PluginStates[i]);
				Tag = null;
			}
			else
			{
				Tag = new object();
				buttonRemove.Enabled = false;
				labelNumber.Enabled = false;
				numericUpDownNumber.Enabled = false;
				labelState.Enabled = false;
				numericUpDownState.Enabled = false;
				Tag = null;
			}
		}

		private void buttonAdd_Click(object sender, EventArgs e)
		{
			int n = PluginStates.Count;
			for (int i = 0; i < n; i++)
			{
				listViewPlugin.Items[i].Selected = false;
			}
			PluginStates.Add(new PluginState(0, 0));
			ListViewItem item = new ListViewItem(new string[] { "", "" });
			UpdatePluginListViewItem(item, n, true);
			listViewPlugin.Items.Add(item);
			item.Selected = true;
		}

		private void buttonRemove_Click(object sender, EventArgs e)
		{
			if (listViewPlugin.SelectedIndices.Count == 1)
			{
				int i = listViewPlugin.SelectedIndices[0];
				PluginStates.RemoveAt(i);
				listViewPlugin.Items[i].Remove();
			}
		}

		private void numericUpDownNumber_ValueChanged(object sender, EventArgs e)
		{
			if (Tag == null && listViewPlugin.SelectedIndices.Count == 1)
			{
				int i = listViewPlugin.SelectedIndices[0];
				PluginStates[i].Number = (int)numericUpDownNumber.Value;
				UpdatePluginListViewItem(listViewPlugin.Items[i], i, true);
			}
		}

		private void numericUpDownState_ValueChanged(object sender, EventArgs e)
		{
			if (Tag == null && listViewPlugin.SelectedIndices.Count == 1)
			{
				int i = listViewPlugin.SelectedIndices[0];
				PluginStates[i].State = (int)numericUpDownState.Value;
				UpdatePluginListViewItem(listViewPlugin.Items[i], i, true);
			}
		}

		private void ApplyTrain()
		{
			lock (Program.LockObj)
			{
				TrainManager.Trains = new TrainManager.Train[] { };
				if (checkBoxEnableTrain.Checked)
				{
					Array.Resize(ref TrainManager.Trains, 1);
					TrainManager.Trains[0] = new TrainManager.Train();
					TrainManager.Trains[0].State = TrainState.Available;
					Array.Resize(ref TrainManager.Trains[0].Cars, (int)numericUpDownCars.Value);
					for (int i = 0; i < TrainManager.Trains[0].Cars.Length; i++)
					{
						TrainManager.Trains[0].Cars[i].Specs.CurrentSpeed = (int)numericUpDownSpeed.Value / 3.6;
						TrainManager.Trains[0].Cars[i].Specs.CurrentPerceivedSpeed = (int)numericUpDownSpeed.Value / 3.6;
						TrainManager.Trains[0].Cars[i].Specs.CurrentAcceleration = (int)numericUpDownAccel.Value / 3.6;

						TrainManager.Trains[0].Cars[i].Specs.AirBrake.MainReservoirCurrentPressure = (int)numericUpDownMain.Value * 1000;
						TrainManager.Trains[0].Cars[i].Specs.AirBrake.BrakePipeCurrentPressure = (int)numericUpDownPipe.Value * 1000;
						TrainManager.Trains[0].Cars[i].Specs.AirBrake.BrakeCylinderCurrentPressure = (int)numericUpDownCylinder.Value * 1000;
						TrainManager.Trains[0].Cars[i].Specs.AirBrake.StraightAirPipeCurrentPressure = (int)numericUpDownAirPipe.Value * 1000;

						TrainManager.Trains[0].Cars[i].Specs.Doors = new TrainManager.Door[] { new TrainManager.Door(), new TrainManager.Door() };
						TrainManager.Trains[0].Cars[i].Specs.Doors[0].Direction = -1;
						TrainManager.Trains[0].Cars[i].Specs.Doors[0].State = (double)numericUpDownLeft.Value;
						TrainManager.Trains[0].Cars[i].Specs.Doors[1].Direction = 1;
						TrainManager.Trains[0].Cars[i].Specs.Doors[1].State = (double)numericUpDownRight.Value;
						TrainManager.Trains[0].Cars[i].Specs.AnticipatedLeftDoorsOpened = checkBoxLeftTarget.Checked;
						TrainManager.Trains[0].Cars[i].Specs.AnticipatedRightDoorsOpened = checkBoxRightTarget.Checked;
					}

					TrainManager.Trains[0].Specs.CurrentReverser.Driver = (int)numericUpDownReverser.Value;
					TrainManager.Trains[0].Specs.CurrentReverser.Actual = (int)numericUpDownReverser.Value;
					TrainManager.Trains[0].Specs.CurrentPowerNotch.Driver = (int)numericUpDownPowerNotch.Value;
					TrainManager.Trains[0].Specs.MaximumPowerNotch = (int)numericUpDownPowerNotches.Value;
					if (checkBoxAirBrake.Checked)
					{
						TrainManager.Trains[0].Cars[0].Specs.BrakeType = TrainManager.CarBrakeType.AutomaticAirBrake;
						TrainManager.Trains[0].Specs.AirBrake.Handle.Driver = (TrainManager.AirBrakeHandleState)numericUpDownBrakeNotch.Value;
					}
					else
					{
						TrainManager.Trains[0].Specs.CurrentBrakeNotch.Driver = (int)numericUpDownBrakeNotch.Value;
						TrainManager.Trains[0].Specs.MaximumBrakeNotch = (int)numericUpDownBrakeNotches.Value;
						TrainManager.Trains[0].Specs.HasHoldBrake = checkBoxHoldBrake.Checked;
						if (checkBoxHoldBrake.Checked)
						{
							TrainManager.Trains[0].Specs.CurrentHoldBrake.Driver = checkBoxSetHoldBrake.Checked;
						}
					}
					TrainManager.Trains[0].Specs.CurrentEmergencyBrake.Driver = checkBoxSetEmergency.Checked;
					TrainManager.Trains[0].Specs.HasConstSpeed = checkBoxConstSpeed.Checked;
					if (checkBoxConstSpeed.Checked)
					{
						TrainManager.Trains[0].Specs.CurrentConstSpeed = checkBoxSetConstSpeed.Checked;
					}

					TrainManager.Trains[0].Specs.Safety.Mode = checkBoxEnablePlugin.Checked ? TrainManager.SafetySystem.Plugin : TrainManager.SafetySystem.None;
					if (checkBoxEnablePlugin.Checked && PluginStates.Count != 0)
					{
						PluginManager.CurrentPlugin.Panel = new int[PluginStates.Max(value => value.Number) + 1];
						foreach (var state in PluginStates)
						{
							PluginManager.CurrentPlugin.Panel[state.Number] = state.State;
						}
					}
					else
					{
						PluginManager.CurrentPlugin.Panel = new int[] { };
					}
				}
			}
		}

		private void buttonApply_Click(object sender, EventArgs e)
		{
			ApplyTrain();
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			ApplyTrain();
			Close();
		}
	}
}
