using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using OpenBveApi.Interface;
using Prism.Mvvm;
using SoundManager;
using TrainEditor2.Models.Others;
using TrainEditor2.Simulation.TrainManager;
using TrainEditor2.Systems;

namespace TrainEditor2.Models.Trains
{
	internal partial class Motor : BindableBase, ICloneable
	{
		private static CultureInfo Culture => CultureInfo.InvariantCulture;

		private TreeViewItemModel selectedTreeItem;

		private SimulationState currentSimState;
		private int runIndex;
		private bool isLoop;
		private bool isConstant;
		private double acceleration;
		private double startSpeed;
		private double endSpeed;

		private DateTime startTime;
		private double oldElapsedTime;

		internal ObservableCollection<TreeViewItemModel> TreeItems;

		internal ObservableCollection<Track> Tracks;

		internal TreeViewItemModel SelectedTreeItem
		{
			get
			{
				return selectedTreeItem;
			}
			set
			{
				SetProperty(ref selectedTreeItem, value);
			}
		}

		internal SimulationState CurrentSimState
		{
			get
			{
				return currentSimState;
			}
			set
			{
				SetProperty(ref currentSimState, value);
			}
		}

		internal int RunIndex
		{
			get
			{
				return runIndex;
			}
			set
			{
				SetProperty(ref runIndex, value);
			}
		}

		internal bool IsLoop
		{
			get
			{
				return isLoop;
			}
			set
			{
				SetProperty(ref isLoop, value);
			}
		}

		internal bool IsConstant
		{
			get
			{
				return isConstant;
			}
			set
			{
				SetProperty(ref isConstant, value);
			}
		}

		internal double Acceleration
		{
			get
			{
				return acceleration;
			}
			set
			{
				SetProperty(ref acceleration, value);
			}
		}

		internal double StartSpeed
		{
			get
			{
				return startSpeed;
			}
			set
			{
				SetProperty(ref startSpeed, value);
			}
		}

		internal double EndSpeed
		{
			get
			{
				return endSpeed;
			}
			set
			{
				SetProperty(ref endSpeed, value);
			}
		}

		internal Motor()
		{
			Tracks = new ObservableCollection<Track>();

			TreeItems = new ObservableCollection<TreeViewItemModel>();
			CreateTreeItem();

			RunIndex = -1;
			Acceleration = 2.6;
			StartSpeed = 0.0;
			EndSpeed = 160.0;
		}

		internal void CreateTreeItem()
		{
			TreeItems.Clear();
			TreeViewItemModel treeItem = new TreeViewItemModel(null) { Title = "Tracks" };
			treeItem.Children = new ObservableCollection<TreeViewItemModel>(Tracks.Select((x, i) => new TreeViewItemModel(treeItem) { Title = i.ToString(Culture), Tag = x }));
			TreeItems.Add(treeItem);
		}

		private void RenameTreeViewItem(ObservableCollection<TreeViewItemModel> items)
		{
			for (int i = 0; i < items.Count; i++)
			{
				items[i].Title = i.ToString(Culture);
			}
		}

		internal void UpTrack()
		{
			int index = Tracks.IndexOf((Track)SelectedTreeItem.Tag);
			Tracks.Move(index, index - 1);

			TreeItems[0].Children.Move(index, index - 1);
			RenameTreeViewItem(TreeItems[0].Children);
		}

		internal void DownTrack()
		{
			int index = Tracks.IndexOf((Track)SelectedTreeItem.Tag);
			Tracks.Move(index, index + 1);

			TreeItems[0].Children.Move(index, index + 1);
			RenameTreeViewItem(TreeItems[0].Children);
		}

		internal void AddTrack()
		{
			Tracks.Add(new Track(this));

			TreeItems[0].Children.Add(new TreeViewItemModel(TreeItems[0]) { Title = (Tracks.Count - 1).ToString(Culture), Tag = Tracks.Last() });
			SelectedTreeItem = TreeItems[0].Children.Last();
		}

		internal void RemoveTrack()
		{
			int index = Tracks.IndexOf((Track)SelectedTreeItem.Tag);
			Tracks.RemoveAt(index);

			TreeItems[0].Children.RemoveAt(index);
			RenameTreeViewItem(TreeItems[0].Children);

			SelectedTreeItem = null;
		}

		internal void CopyTrack()
		{
			Tracks.Add((Track)((Track)SelectedTreeItem.Tag).Clone());

			TreeItems[0].Children.Add(new TreeViewItemModel(TreeItems[0]) { Title = (Tracks.Count - 1).ToString(Culture), Tag = Tracks.Last() });
			SelectedTreeItem = TreeItems[0].Children.Last();
		}

		internal void StartSimulation()
		{
			try
			{
				CreateCar();
			}
			catch (Exception e)
			{
				Interface.AddMessage(MessageType.Error, false, $"{e.GetType().FullName}: {e.Message} at {e.StackTrace}");
				CurrentSimState = SimulationState.Disable;
				return;
			}

			if (TrainManager.PlayerTrain == null)
			{
				Interface.AddMessage(MessageType.Error, false, "Failed to create train.");
				CurrentSimState = SimulationState.Disable;
				return;
			}

			oldElapsedTime = 0;
			startTime = DateTime.Now;

			if (CurrentSimState != SimulationState.Paused)
			{
				Track.CurrentSimSpeed = StartSpeed;
			}

			CurrentSimState = SimulationState.Started;
		}

		internal void PauseSimulation()
		{
			DisposeCar();
			CurrentSimState = SimulationState.Paused;
		}

		internal void StopSimulation()
		{
			DisposeCar();
			CurrentSimState = SimulationState.Stopped;

			Track selectedTrack = SelectedTreeItem.Tag as Track;

			if (selectedTrack != null)
			{
				selectedTrack.IsRefreshGlControl = true;
			}
		}

		private void CreateCar()
		{
			DisposeCar();

			TrainManager.PlayerTrain = new TrainManager.Train();
			TrainManager.PlayerTrain.Car.Sounds.Motor.PowerTables = Tracks.Where(x => x.Type == TrackType.Power).Select(x => Track.TrackToMotorSoundTable(x, y => y / 3.6, y => 0.01 * y, y => Math.Pow(0.0078125 * y, 0.25))).ToArray();
			TrainManager.PlayerTrain.Car.Sounds.Motor.BrakeTables = Tracks.Where(x => x.Type == TrackType.Brake).Select(x => Track.TrackToMotorSoundTable(x, y => y / 3.6, y => 0.01 * y, y => Math.Pow(0.0078125 * y, 0.25))).ToArray();
			TrainManager.PlayerTrain.Car.ApplySounds();
		}

		internal void RunSimulation()
		{
			if (TrainManager.PlayerTrain == null)
			{
				return;
			}

			double nowElapsedTime = (DateTime.Now - startTime).TotalSeconds;

			if (oldElapsedTime == 0.0)
			{
				oldElapsedTime = nowElapsedTime;
			}

			double deltaTime = nowElapsedTime - oldElapsedTime;

			double outputAcceleration = Math.Sign(endSpeed - startSpeed) * Acceleration;

			Track.CurrentSimSpeed += outputAcceleration * deltaTime;
			double minSpeed = Math.Min(startSpeed, endSpeed);
			double maxSpeed = Math.Max(startSpeed, endSpeed);

			if (IsLoop)
			{
				if (Track.CurrentSimSpeed < minSpeed)
				{
					Track.CurrentSimSpeed = maxSpeed;
					outputAcceleration = 0.0;
				}

				if (Track.CurrentSimSpeed > maxSpeed)
				{
					Track.CurrentSimSpeed = minSpeed;
					outputAcceleration = 0.0;
				}

				if (IsConstant)
				{
					Track.CurrentSimSpeed = startSpeed;
					outputAcceleration = Math.Sign(endSpeed - startSpeed) * acceleration;
				}
			}
			else
			{
				if (Track.CurrentSimSpeed < minSpeed || Track.CurrentSimSpeed > maxSpeed)
				{
					StopSimulation();
					return;
				}
			}

			TrainManager.PlayerTrain.Car.Specs.CurrentSpeed = TrainManager.PlayerTrain.Car.Specs.CurrentPerceivedSpeed = Track.CurrentSimSpeed / 3.6;
			TrainManager.PlayerTrain.Car.Specs.CurrentAccelerationOutput = outputAcceleration / 3.6;

			TrainManager.PlayerTrain.Car.UpdateRunSounds(deltaTime, RunIndex);

			TrainManager.PlayerTrain.Car.UpdateMotorSounds(TreeItems[0].Children.Select(x => x.Checked).ToArray());

			Program.SoundApi.Update(deltaTime, SoundModels.Inverse);

			oldElapsedTime = nowElapsedTime;

			(SelectedTreeItem.Tag as Track)?.DrawSimulation(StartSpeed, EndSpeed);
		}

		private void DisposeCar()
		{
			if (TrainManager.PlayerTrain != null)
			{
				TrainManager.PlayerTrain.Dispose();
				TrainManager.PlayerTrain = null;
			}
		}

		public object Clone()
		{
			Motor motor = (Motor)MemberwiseClone();
			motor.Tracks = new ObservableCollection<Track>(Tracks.Select(x =>
			{
				Track track = (Track)x.Clone();
				track.BaseMotor = motor;
				return track;
			}));
			motor.CreateTreeItem();
			return motor;
		}
	}
}
