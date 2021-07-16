using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ObjectViewer.Trains;

namespace OpenBve
{
	public partial class formTrain : Form
	{
		private static Task FormTrainTask;
		internal static formTrain Instance;

		private List<PluginState> PluginStates;

		internal static void ShowTrainSettings()
		{
			if (FormTrainTask == null || FormTrainTask.IsCompleted)
			{
				FormTrainTask = Task.Run(() =>
				{
					Instance = new formTrain();
					Instance.ShowDialog();
					Instance.Dispose();
				});
			}
			else
			{
				Instance?.ActivateUI_Async();
			}
		}

		/// <summary>
		/// Waits for the task running this form to finish
		/// </summary>
		/// <remarks>On Linux, you may get System.Threading.ThreadAbortException if you don't wait.</remarks>
		internal static void WaitTaskFinish()
		{
			if (FormTrainTask == null || FormTrainTask.IsCompleted)
			{
				return;
			}

			Console.WriteLine(@"Wait for Sub UI Thread to finish...");
			FormTrainTask.Wait();
			Console.WriteLine(@"Sub UI Thread finished.");
		}

		private formTrain()
		{
			InitializeComponent();
		}

		private void UpdatePluginListView()
		{
			listViewPlugin.Items.Clear();
			buttonRemove.Enabled = false;
			labelNumber.Enabled = false;
			numericUpDownNumber.Enabled = false;
			labelState.Enabled = false;
			numericUpDownState.Enabled = false;

			ListViewItem[] items = new ListViewItem[PluginStates.Count];
			for (int i = 0; i < items.Length; i++)
			{
				items[i] = new ListViewItem(new[] { "", "" });
				UpdatePluginListViewItem(items[i], i, false);
			}
			listViewPlugin.Items.AddRange(items);
			listViewPlugin.AutoResizeColumns(items.Length != 0 ? ColumnHeaderAutoResizeStyle.HeaderSize : ColumnHeaderAutoResizeStyle.None);
		}

		private void UpdatePluginListViewItem(ListViewItem item, int index, bool resizeColumns)
		{
			item.SubItems[0].Text = PluginStates[index].Index.ToString();
			item.SubItems[1].Text = PluginStates[index].Value.ToString();
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
			numericUpDownNumber.Value = state.Index;
			labelState.Enabled = true;
			numericUpDownState.Enabled = true;
			numericUpDownState.Value = state.Value;
		}

		private void UpdateTrain()
		{
			lock (NearestTrain.LockObj)
			{
				checkBoxEnableTrain.Checked = NearestTrain.EnableSimulation;
				tabControlSettings.Enabled = checkBoxEnableTrain.Checked;

				// Update Spec
				NearestTrainSpecs specs = NearestTrain.Specs;
				numericUpDownCars.Enabled = !NearestTrain.IsExtensionsCfg;
				numericUpDownCars.Value = specs.NumberOfCars;

				numericUpDownPowerNotches.Enabled = !NearestTrain.IsExtensionsCfg;
				numericUpDownPowerNotches.Value = specs.PowerNotches;

				checkBoxAirBrake.Enabled = !NearestTrain.IsExtensionsCfg;
				checkBoxAirBrake.Checked = specs.IsAirBrake;

				numericUpDownBrakeNotches.Enabled = !NearestTrain.IsExtensionsCfg && !specs.IsAirBrake;
				numericUpDownBrakeNotches.Value = specs.BrakeNotches;

				checkBoxHoldBrake.Enabled = !NearestTrain.IsExtensionsCfg && !specs.IsAirBrake;
				checkBoxHoldBrake.Checked = specs.HasHoldBrake;

				checkBoxConstSpeed.Enabled = !NearestTrain.IsExtensionsCfg;
				checkBoxConstSpeed.Checked = specs.HasConstSpeed;

				// Update Status
				NearestTrainStatus status = NearestTrain.Status;

				numericUpDownSpeed.Value = (decimal)status.Speed;
				numericUpDownAccel.Value = (decimal)status.Acceleration;

				numericUpDownMain.Enabled = !NearestTrain.IsExtensionsCfg;
				numericUpDownMain.Value = (decimal)status.MainReservoirPressure;

				numericUpDownEqualizing.Enabled = !NearestTrain.IsExtensionsCfg;
				numericUpDownEqualizing.Value = (decimal)status.EqualizingReservoirPressure;

				numericUpDownPipe.Enabled = !NearestTrain.IsExtensionsCfg;
				numericUpDownPipe.Value = (decimal)status.BrakePipePressure;

				numericUpDownCylinder.Enabled = !NearestTrain.IsExtensionsCfg;
				numericUpDownCylinder.Value = (decimal)status.BrakeCylinderPressure;

				numericUpDownAirPipe.Enabled = !NearestTrain.IsExtensionsCfg;
				numericUpDownAirPipe.Value = (decimal)status.StraightAirPipePressure;

				numericUpDownLeft.Value = (decimal)status.LeftDoorState;
				checkBoxLeftTarget.Checked = status.LeftDoorAnticipatedOpen;
				numericUpDownRight.Value = (decimal)status.RightDoorState;
				checkBoxRightTarget.Checked = status.RightDoorAnticipatedOpen;

				numericUpDownReverser.Value = status.Reverser;
				numericUpDownPowerNotch.Maximum = specs.PowerNotches;
				numericUpDownPowerNotch.Value = status.PowerNotch;
				numericUpDownBrakeNotch.Maximum = specs.BrakeNotches;
				numericUpDownBrakeNotch.Value = status.BrakeNotch;
				checkBoxSetHoldBrake.Enabled = checkBoxHoldBrake.Checked;
				checkBoxSetHoldBrake.Checked = status.HoldBrake;
				checkBoxSetEmergency.Checked = status.EmergencyBrake;
				checkBoxSetConstSpeed.Enabled = checkBoxConstSpeed.Checked;
				checkBoxSetConstSpeed.Checked = status.ConstSpeed;

				checkBoxEnablePlugin.Checked = NearestTrain.EnablePluginSimulation;
				panelPlugin.Enabled = checkBoxEnablePlugin.Checked;
				PluginStates = status.PluginStates.ToList();
			}

			UpdatePluginListView();
		}

		private void ResetTrain()
		{
			lock (NearestTrain.LockObj)
			{
				NearestTrain.EnableSimulation = checkBoxEnableTrain.Checked;

				if (!NearestTrain.IsExtensionsCfg)
				{
					NearestTrain.Specs = new NearestTrainSpecs();
				}

				NearestTrain.Status = new NearestTrainStatus();
				NearestTrain.EnablePluginSimulation = false;
				NearestTrain.RequiredApply = true;
			}

			UpdateTrain();
		}

		private void ApplyTrain()
		{
			lock (NearestTrain.LockObj)
			{
				NearestTrain.EnableSimulation = checkBoxEnableTrain.Checked;

				if (!NearestTrain.IsExtensionsCfg)
				{
					NearestTrainSpecs specs = NearestTrain.Specs;
					specs.NumberOfCars = (int)numericUpDownCars.Value;
					specs.PowerNotches = (int)numericUpDownPowerNotches.Value;
					specs.IsAirBrake = checkBoxAirBrake.Checked;
					specs.BrakeNotches = (int)numericUpDownBrakeNotches.Value;
					specs.HasHoldBrake = checkBoxHoldBrake.Checked;
					specs.HasConstSpeed = checkBoxConstSpeed.Checked;
				}

				NearestTrainStatus status = NearestTrain.Status;

				// Physics
				status.Speed = (int)numericUpDownSpeed.Value;
				status.Acceleration = (int)numericUpDownAccel.Value;

				// Brake system
				if (!NearestTrain.IsExtensionsCfg)
				{
					status.MainReservoirPressure = (int)numericUpDownMain.Value;
					status.EqualizingReservoirPressure = (int)numericUpDownEqualizing.Value;
					status.BrakePipePressure = (int)numericUpDownPipe.Value;
					status.BrakeCylinderPressure = (int)numericUpDownCylinder.Value;
					status.StraightAirPipePressure = (int)numericUpDownAirPipe.Value;
				}

				// Door
				status.LeftDoorState = (double)numericUpDownLeft.Value;
				status.LeftDoorAnticipatedOpen = checkBoxLeftTarget.Checked;
				status.RightDoorState = (double)numericUpDownRight.Value;
				status.RightDoorAnticipatedOpen = checkBoxRightTarget.Checked;

				// Handle
				status.Reverser = (int)numericUpDownReverser.Value;
				status.PowerNotch = (int)numericUpDownPowerNotch.Value;
				status.BrakeNotch = (int)numericUpDownBrakeNotch.Value;
				status.HoldBrake = checkBoxSetHoldBrake.Checked;
				status.EmergencyBrake = checkBoxSetEmergency.Checked;
				status.ConstSpeed = checkBoxSetConstSpeed.Checked;

				// Plugin
				NearestTrain.EnablePluginSimulation = checkBoxEnablePlugin.Checked;
				NearestTrain.Status.PluginStates = PluginStates.ToArray();

				NearestTrain.RequiredApply = true;
			}
		}

		internal void EnableUI()
		{
			if (IsDisposed)
			{
				return;
			}

			if (InvokeRequired)
			{
				Invoke((MethodInvoker)(() => Enabled = true));
			}
			else
			{
				Enabled = true;
			}
		}

		internal void DisableUI()
		{
			if (IsDisposed)
			{
				return;
			}

			if (InvokeRequired)
			{
				Invoke((MethodInvoker)(() => Enabled = false));
			}
			else
			{
				Enabled = false;
			}
		}

		internal void UpdateSpecsUI()
		{
			if (IsDisposed)
			{
				return;
			}

			if (InvokeRequired)
			{
				Invoke((MethodInvoker)UpdateTrain);
			}
			else
			{
				UpdateTrain();
			}
		}

		private void ActivateUI_Async()
		{
			if (IsDisposed)
			{
				return;
			}

			if (InvokeRequired)
			{
				BeginInvoke((MethodInvoker)Activate);
			}
			else
			{
				Activate();
			}
		}

		internal void CloseUI_Async()
		{
			if (IsDisposed)
			{
				return;
			}

			if (InvokeRequired)
			{
				BeginInvoke((MethodInvoker)Close);
			}
			else
			{
				Close();
			}
		}

		private void formTrain_Load(object sender, EventArgs e)
		{
			UpdateTrain();
		}

		private void checkBoxEnableTrain_Check(object sender, EventArgs e)
		{
			tabControlSettings.Enabled = checkBoxEnableTrain.Checked;
		}

		private void checkBoxAirBrake_Check(object sender, EventArgs e)
		{
			if (checkBoxAirBrake.Checked)
			{
				numericUpDownBrakeNotches.Value = 3;
				numericUpDownBrakeNotches.Enabled = false;
				checkBoxHoldBrake.Checked = false;
				checkBoxHoldBrake.Enabled = false;
				checkBoxSetHoldBrake.Checked = false;
				checkBoxSetHoldBrake.Enabled = false;
			}
			else
			{
				numericUpDownBrakeNotches.Enabled = true;
				checkBoxHoldBrake.Enabled = true;
			}
		}

		private void numericUpDownPowerNotches_ValueChanged(object sender, EventArgs e)
		{
			if (numericUpDownPowerNotch.Value > numericUpDownPowerNotches.Value)
			{
				numericUpDownPowerNotch.Value = numericUpDownPowerNotches.Value;
			}
			numericUpDownPowerNotch.Maximum = numericUpDownPowerNotches.Value;
		}

		private void numericUpDownBrakeNotches_ValueChanged(object sender, EventArgs e)
		{
			if (numericUpDownBrakeNotch.Value > numericUpDownBrakeNotches.Value)
			{
				numericUpDownBrakeNotch.Value = numericUpDownBrakeNotches.Value;
			}
			numericUpDownBrakeNotch.Maximum = numericUpDownBrakeNotches.Value;
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

		private void listViewPlugin_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (listViewPlugin.SelectedIndices.Count == 1)
			{
				int i = listViewPlugin.SelectedIndices[0];
				listViewPlugin.Tag = new object();
				UpdatePluginComponent(PluginStates[i]);
				listViewPlugin.Tag = null;
			}
			else
			{
				listViewPlugin.Tag = new object();
				buttonRemove.Enabled = false;
				labelNumber.Enabled = false;
				numericUpDownNumber.Enabled = false;
				labelState.Enabled = false;
				numericUpDownState.Enabled = false;
				listViewPlugin.Tag = null;
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
			ListViewItem item = new ListViewItem(new[] { "", "" });
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
			if (listViewPlugin.Tag == null && listViewPlugin.SelectedIndices.Count == 1)
			{
				int i = listViewPlugin.SelectedIndices[0];
				PluginStates[i] = new PluginState((int)numericUpDownNumber.Value, PluginStates[i].Value);
				UpdatePluginListViewItem(listViewPlugin.Items[i], i, true);
			}
		}

		private void numericUpDownState_ValueChanged(object sender, EventArgs e)
		{
			if (listViewPlugin.Tag == null && listViewPlugin.SelectedIndices.Count == 1)
			{
				int i = listViewPlugin.SelectedIndices[0];
				PluginStates[i] = new PluginState(PluginStates[i].Index, (int)numericUpDownState.Value);
				UpdatePluginListViewItem(listViewPlugin.Items[i], i, true);
			}
		}

		private void buttonReset_Click(object sender, EventArgs e)
		{
			ResetTrain();
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
