using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenBveApi.Trains;
using TrainManager;
using TrainManager.BrakeSystems;
using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.Power;
using TrainManager.Trains;

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

		private static Task FormTrainTask;
		internal static formTrain Instance;

		private readonly List<PluginState> PluginStates;

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
				Instance?.ActivateUI();
			}
		}

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

			checkBoxEnableTrain.Checked = Program.TrainManager.EnableSimulation;
			tabControlSettings.Enabled = checkBoxEnableTrain.Checked;

			labelEmergency.Enabled = false;
			numericUpDownEmergency.Enabled = false;
			checkBoxSetHoldBrake.Enabled = false;
			checkBoxSetConstSpeed.Enabled = false;

			if (Program.TrainManager.Trains.Length != 0)
			{
				TrainBase Train = Program.TrainManager.Trains[0];

				numericUpDownCars.Enabled = !Program.IsExtensionsCfg;
				numericUpDownCars.Value = Train.Cars.Length;
				numericUpDownSpeed.Value = (decimal)(Train.Cars[0].CurrentSpeed * 3.6);
				numericUpDownAccel.Value = (decimal)(Train.Cars[0].Specs.MotorAcceleration * 3.6);

				numericUpDownMain.Value = (decimal)(Train.Cars[0].CarBrake.mainReservoir.CurrentPressure / 1000.0);
				numericUpDownPipe.Value = (decimal)(Train.Cars[0].CarBrake.brakePipe.CurrentPressure / 1000.0);
				numericUpDownCylinder.Value = (decimal)(Train.Cars[0].CarBrake.brakeCylinder.CurrentPressure / 1000.0);
				numericUpDownAirPipe.Value = (decimal)(Train.Cars[0].CarBrake.straightAirPipe.CurrentPressure / 1000.0);

				numericUpDownLeft.Value = (decimal)Train.Cars[0].Doors[0].State;
				numericUpDownRight.Value = (decimal)Train.Cars[0].Doors[1].State;
				checkBoxLeftTarget.Checked = Train.Cars[0].Doors[0].AnticipatedOpen;
				checkBoxRightTarget.Checked = Train.Cars[0].Doors[1].AnticipatedOpen;

				numericUpDownReverser.Value = (decimal)Train.Handles.Reverser.Driver;
				numericUpDownPowerNotch.Value = Train.Handles.Power.Driver;
				numericUpDownPowerNotches.Enabled = !Program.IsExtensionsCfg;
				numericUpDownPowerNotches.Value = Train.Handles.Power.MaximumNotch;

				checkBoxAirBrake.Enabled = !Program.IsExtensionsCfg;
				checkBoxAirBrake.Checked = Train.Cars[0].CarBrake is AutomaticAirBrake;
				numericUpDownBrakeNotch.Value = Train.Handles.Brake.Driver;

				if (checkBoxAirBrake.Checked)
				{
					numericUpDownBrakeNotch.Maximum = 2;
					numericUpDownBrakeNotches.Enabled = false;
					numericUpDownBrakeNotches.Value = 2;
					checkBoxHoldBrake.Enabled = false;
				}
				else
				{
					numericUpDownBrakeNotch.Maximum = Train.Handles.Brake.MaximumNotch;
					numericUpDownBrakeNotches.Enabled = !Program.IsExtensionsCfg;
					numericUpDownBrakeNotches.Value = Train.Handles.Brake.MaximumNotch;
					checkBoxHoldBrake.Enabled = !Program.IsExtensionsCfg;
					checkBoxHoldBrake.Checked = Train.Handles.HasHoldBrake;
					if (checkBoxHoldBrake.Checked)
					{
						checkBoxSetHoldBrake.Enabled = true;
						checkBoxSetHoldBrake.Checked = Train.Handles.HoldBrake.Driver;
					}
				}
				checkBoxSetEmergency.Checked = Train.Handles.EmergencyBrake.Driver;
				checkBoxConstSpeed.Enabled = !Program.IsExtensionsCfg;
				checkBoxConstSpeed.Checked = Train.Specs.HasConstSpeed;
				if (checkBoxConstSpeed.Checked)
				{
					checkBoxSetConstSpeed.Enabled = true;
					checkBoxSetConstSpeed.Checked = Train.Specs.CurrentConstSpeed;
				}
			}
			checkBoxEnablePlugin.Checked = Program.TrainManager.EnablePluginSimulation;
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

		private void ActivateUI()
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

		internal void CloseUI()
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
				items[i] = new ListViewItem(new[] { "", "" });
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

		private TrainBase CreateDummyTrain()
		{
			TrainBase train = new TrainBase(TrainState.Available);

			train.Handles.Reverser = new ReverserHandle(train);
			train.Handles.Power = new PowerHandle((int)numericUpDownPowerNotches.Value, (int)numericUpDownPowerNotches.Value, new double[] { }, new double[] { }, train);
			if (checkBoxAirBrake.Checked)
			{
				train.Handles.Brake = new AirBrakeHandle(train);
			}
			else
			{
				train.Handles.Brake = new BrakeHandle((int)numericUpDownBrakeNotches.Value, (int)numericUpDownBrakeNotches.Value, null, new double[] { }, new double[] { }, train);
				train.Handles.HasHoldBrake = checkBoxHoldBrake.Checked;
			}
			train.Handles.EmergencyBrake = new EmergencyHandle(train);
			train.Handles.HoldBrake = new HoldBrakeHandle(train);
			train.Specs.HasConstSpeed = checkBoxConstSpeed.Checked;

			Array.Resize(ref train.Cars, (int)numericUpDownCars.Value);
			for (int i = 0; i < train.Cars.Length; i++)
			{
				train.Cars[i] = new CarBase(train, i);
				train.Cars[i].Specs = new CarPhysics();

				if (checkBoxAirBrake.Checked)
				{
					train.Cars[i].CarBrake = new AutomaticAirBrake(EletropneumaticBrakeType.None, train.Handles.EmergencyBrake, train.Handles.Reverser, true, 0.0, 0.0, new AccelerationCurve[] { });
				}
				else
				{
					train.Cars[i].CarBrake = new ElectromagneticStraightAirBrake(EletropneumaticBrakeType.None, train.Handles.EmergencyBrake, train.Handles.Reverser, true, 0.0, 0.0, new AccelerationCurve[] { });
				}
				//At the minute, Object Viewer uses dummy brake systems
				train.Cars[i].CarBrake.mainReservoir = new MainReservoir((int)numericUpDownMain.Value * 1000);
				train.Cars[i].CarBrake.brakePipe = new BrakePipe((int)numericUpDownPipe.Value * 1000);
				train.Cars[i].CarBrake.brakeCylinder = new BrakeCylinder((int)numericUpDownCylinder.Value * 1000);
				train.Cars[i].CarBrake.straightAirPipe = new StraightAirPipe((int)numericUpDownAirPipe.Value * 1000);

				train.Cars[i].Coupler = new Coupler(0.9 * 0.3, 1.1 * 0.3, train.Cars[i / 2], train.Cars.Length > 1 ? train.Cars[(i / 2) + 1] : null, train);

				train.Cars[i].Doors[0] = new Door(-1, 1000, 0);
				train.Cars[i].Doors[1] = new Door(1, 1000, 0);
			}

			return train;
		}

		private void ApplyTrain()
		{
			lock (Program.LockObj)
			{
				Program.TrainManager.EnableSimulation = checkBoxEnableTrain.Checked;

				if (Program.TrainManager.EnableSimulation)
				{
					TrainBase train;

					if (Program.IsExtensionsCfg)
					{
						train = Program.TrainManager.Trains[0];
					}
					else
					{
						train = CreateDummyTrain();
						Array.Resize(ref Program.TrainManager.Trains, 1);
						Program.TrainManager.Trains[0] = train;
						TrainManagerBase.PlayerTrain = train;
					}

					foreach (CarBase car in train.Cars)
					{
						car.CurrentSpeed = (int)numericUpDownSpeed.Value / 3.6;
						car.Specs.PerceivedSpeed = (int)numericUpDownSpeed.Value / 3.6;
						car.Specs.Acceleration = (int)numericUpDownAccel.Value / 3.6;

						car.CarBrake.mainReservoir.CurrentPressure = (int)numericUpDownMain.Value * 1000;
						car.CarBrake.brakePipe.CurrentPressure = (int)numericUpDownPipe.Value * 1000;
						car.CarBrake.brakeCylinder.CurrentPressure = (int)numericUpDownCylinder.Value * 1000;
						car.CarBrake.straightAirPipe.CurrentPressure = (int)numericUpDownAirPipe.Value * 1000;

						car.Doors[0].State = (double)numericUpDownLeft.Value;
						car.Doors[0].AnticipatedOpen = checkBoxLeftTarget.Checked;
						car.Doors[1].State = (double)numericUpDownRight.Value;
						car.Doors[1].AnticipatedOpen = checkBoxRightTarget.Checked;
					}

					train.Handles.Reverser.Driver = (ReverserPosition)numericUpDownReverser.Value;
					train.Handles.Reverser.Actual = (ReverserPosition)numericUpDownReverser.Value;
					train.Handles.Power.Driver = (int)numericUpDownPowerNotch.Value;
					train.Handles.Brake.Driver = (int)numericUpDownBrakeNotch.Value;
					if (train.Handles.HasHoldBrake)
					{
						train.Handles.HoldBrake.Driver = checkBoxSetHoldBrake.Checked;
					}
					train.Handles.EmergencyBrake.Driver = checkBoxSetEmergency.Checked;
					if (train.Specs.HasConstSpeed)
					{
						train.Specs.CurrentConstSpeed = checkBoxSetConstSpeed.Checked;
					}

					Program.TrainManager.EnablePluginSimulation = checkBoxEnablePlugin.Checked;
					if (checkBoxEnablePlugin.Checked && PluginStates.Count != 0)
					{
						PluginManager.CurrentPlugin.Panel = new int[PluginStates.Max(value => value.Number) + 1];
						foreach (PluginState state in PluginStates)
						{
							PluginManager.CurrentPlugin.Panel[state.Number] = state.State;
						}
					}
					else
					{
						PluginManager.CurrentPlugin.Panel = new int[] { };
					}
				}
				else
				{
					// Initialize
					if (Program.IsExtensionsCfg)
					{
						TrainBase train = Program.TrainManager.Trains[0];

						foreach (CarBase car in train.Cars)
						{
							car.CurrentSpeed = 0.0;
							car.Specs.PerceivedSpeed = 0.0;
							car.Specs.Acceleration = 0.0;

							car.Doors[0].State = 0.0;
							car.Doors[0].AnticipatedOpen = false;
							car.Doors[1].State = 0.0;
							car.Doors[1].AnticipatedOpen = false;
						}

						train.Handles.Reverser.Driver = ReverserPosition.Neutral;
						train.Handles.Reverser.Actual = ReverserPosition.Neutral;
						train.Handles.Power.Driver = 0;
						train.Handles.Brake.Driver = 0;
						if (train.Handles.HasHoldBrake)
						{
							train.Handles.HoldBrake.Driver = false;
						}
						train.Handles.EmergencyBrake.Driver = false;
						if (train.Specs.HasConstSpeed)
						{
							train.Specs.CurrentConstSpeed = false;
						}
					}
					else
					{
						Program.TrainManager.Trains = new TrainBase[] { };
					}

					Program.TrainManager.EnablePluginSimulation = false;
					PluginManager.CurrentPlugin.Panel = new int[] { };
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
