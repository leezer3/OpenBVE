using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.World;
using OpenTK.Graphics.OpenGL;
using Prism.Mvvm;
using SoundManager;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Others;
using TrainEditor2.Simulation.TrainManager;
using TrainEditor2.Systems;

namespace TrainEditor2.Models.Trains
{
	internal partial class Motor : BindableBase, ICloneable
	{
		private static CultureInfo Culture => CultureInfo.InvariantCulture;

		private TreeViewItemModel selectedTreeItem;

		private int selectedSoundIndex;

		private Unit.Velocity velocityUnit;

		private Quantity.Velocity minVelocity;
		private Quantity.Velocity maxVelocity;
		private double minPitch;
		private double maxPitch;
		private double minVolume;
		private double maxVolume;
		private Quantity.Velocity nowVelocity;
		private double nowPitch;
		private double nowVolume;

		private InputMode currentInputMode;

		private static int glControlWidth;
		private static int glControlHeight;
		private bool isRefreshGlControl;

		private SimulationState currentSimState;
		private int runIndex;
		private bool isLoop;
		private bool isConstant;
		private Quantity.Acceleration acceleration;
		private Quantity.Velocity startSpeed;
		private Quantity.Velocity endSpeed;
		private Quantity.Velocity currentSimSpeed;

		private DateTime startTime;
		private double oldElapsedTime;

		private double FactorVelocity => GlControlWidth / (MaxVelocity - MinVelocity);
		private double FactorPitch => -GlControlHeight / (MaxPitch - MinPitch);
		private double FactorVolume => -GlControlHeight / (MaxVolume - MinVolume);

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

		internal int SelectedSoundIndex
		{
			get
			{
				return selectedSoundIndex;
			}
			set
			{
				SetProperty(ref selectedSoundIndex, value);
			}
		}

		internal Unit.Velocity VelocityUnit
		{
			get
			{
				return velocityUnit;
			}
			set
			{
				SetProperty(ref velocityUnit, value);

				minVelocity = minVelocity.ToNewUnit(value);
				OnPropertyChanged(new PropertyChangedEventArgs(nameof(MinVelocity)));
				maxVelocity = maxVelocity.ToNewUnit(value);
				OnPropertyChanged(new PropertyChangedEventArgs(nameof(MaxVelocity)));
				nowVelocity = nowVelocity.ToNewUnit(value);
				OnPropertyChanged(new PropertyChangedEventArgs(nameof(NowVelocity)));
				startSpeed = startSpeed.ToNewUnit(value);
				OnPropertyChanged(new PropertyChangedEventArgs(nameof(StartSpeed)));
				endSpeed = endSpeed.ToNewUnit(value);
				OnPropertyChanged(new PropertyChangedEventArgs(nameof(EndSpeed)));
			}
		}

		internal double MinVelocity
		{
			get
			{
				return minVelocity.Value;
			}
			set
			{
				SetProperty(ref minVelocity, new Quantity.Velocity(value, VelocityUnit));
			}
		}

		internal double MaxVelocity
		{
			get
			{
				return maxVelocity.Value;
			}
			set
			{
				SetProperty(ref maxVelocity, new Quantity.Velocity(value, VelocityUnit));
			}
		}

		internal double MinPitch
		{
			get
			{
				return minPitch;
			}
			set
			{
				SetProperty(ref minPitch, value);
			}
		}

		internal double MaxPitch
		{
			get
			{
				return maxPitch;
			}
			set
			{
				SetProperty(ref maxPitch, value);
			}
		}

		internal double MinVolume
		{
			get
			{
				return minVolume;
			}
			set
			{
				SetProperty(ref minVolume, value);
			}
		}

		internal double MaxVolume
		{
			get
			{
				return maxVolume;
			}
			set
			{
				SetProperty(ref maxVolume, value);
			}
		}

		internal double NowVelocity
		{
			get
			{
				return nowVelocity.Value;
			}
			set
			{
				SetProperty(ref nowVelocity, new Quantity.Velocity(value, VelocityUnit));
			}
		}

		internal double NowPitch
		{
			get
			{
				return nowPitch;
			}
			set
			{
				SetProperty(ref nowPitch, value);
			}
		}

		internal double NowVolume
		{
			get
			{
				return nowVolume;
			}
			set
			{
				SetProperty(ref nowVolume, value);
			}
		}

		internal InputMode CurrentInputMode
		{
			get
			{
				return currentInputMode;
			}
			set
			{
				SetProperty(ref currentInputMode, value);
			}
		}

		internal static int GlControlWidth
		{
			get
			{
				return glControlWidth;
			}
			set
			{
				if (value > 0)
				{
					glControlWidth = value;
				}
			}
		}

		internal static int GlControlHeight
		{
			get
			{
				return glControlHeight;
			}
			set
			{
				if (value > 0)
				{
					glControlHeight = value;
				}
			}
		}

		internal bool IsRefreshGlControl
		{
			get
			{
				return isRefreshGlControl;
			}
			set
			{
				SetProperty(ref isRefreshGlControl, value);
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

		internal Quantity.Acceleration Acceleration
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
				return startSpeed.Value;
			}
			set
			{
				SetProperty(ref startSpeed, new Quantity.Velocity(value, VelocityUnit));
			}
		}

		internal double EndSpeed
		{
			get
			{
				return endSpeed.Value;
			}
			set
			{
				SetProperty(ref endSpeed, new Quantity.Velocity(value, VelocityUnit));
			}
		}

		internal Motor()
		{
			Tracks = new ObservableCollection<Track>();

			TreeItems = new ObservableCollection<TreeViewItemModel>();
			CreateTreeItem();

			SelectedSoundIndex = -1;

			VelocityUnit = Unit.Velocity.KilometerPerHour;

			MinVelocity = 0.0;
			MaxVelocity = 40.0;

			MinPitch = 0.0;
			MaxPitch = 400.0;

			MinVolume = 0.0;
			MaxVolume = 256.0;

			CurrentSimState = SimulationState.Stopped;
			RunIndex = -1;
			Acceleration = new Quantity.Acceleration(2.6, Unit.Acceleration.KilometerPerHourPerSecond);
			StartSpeed = 0.0;
			EndSpeed = 160.0;
		}

		internal void CreateTreeItem()
		{
			TreeItems.Clear();
			TreeViewItemModel treeItem = new TreeViewItemModel(null) { Title = "Tracks" };
			treeItem.Children.Add(new TreeViewItemModel(treeItem) { Title = "Power" });
			treeItem.Children.Add(new TreeViewItemModel(treeItem) { Title = "Brake" });
			treeItem.Children[0].Children.AddRange(Tracks.Where(x => x.Type == TrackType.Power).Select((x, i) => new TreeViewItemModel(treeItem) { Title = i.ToString(Culture), Tag = x }));
			treeItem.Children[1].Children.AddRange(Tracks.Where(x => x.Type == TrackType.Brake).Select((x, i) => new TreeViewItemModel(treeItem) { Title = i.ToString(Culture), Tag = x }));
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
			TreeViewItemModel parentTreeItem = SelectedTreeItem.Parent;
			Track currentTrack = (Track)SelectedTreeItem.Tag;
			int currentIndex = Tracks.IndexOf(currentTrack);
			int currentTreeIndex = parentTreeItem.Children.IndexOf(SelectedTreeItem);

			Track prevTrack = Tracks.Take(currentIndex).Last(x => x.Type == currentTrack.Type);
			int prevIndex = Tracks.IndexOf(prevTrack);

			Tracks.Move(currentIndex, prevIndex);
			parentTreeItem.Children.Move(currentTreeIndex, currentTreeIndex - 1);
			RenameTreeViewItem(parentTreeItem.Children);
		}

		internal void DownTrack()
		{
			TreeViewItemModel parentTreeItem = SelectedTreeItem.Parent;
			Track currentTrack = (Track)SelectedTreeItem.Tag;
			int currentIndex = Tracks.IndexOf(currentTrack);
			int currentTreeIndex = parentTreeItem.Children.IndexOf(SelectedTreeItem);

			Track nextTrack = Tracks.Skip(currentIndex + 1).First(x => x.Type == currentTrack.Type);
			int nextIndex = Tracks.IndexOf(nextTrack);

			Tracks.Move(currentIndex, nextIndex);
			parentTreeItem.Children.Move(currentTreeIndex, currentTreeIndex + 1);
			RenameTreeViewItem(parentTreeItem.Children);
		}

		internal void AddTrack()
		{
			TreeViewItemModel parentTreeItem = SelectedTreeItem.Tag is Track ? SelectedTreeItem.Parent : SelectedTreeItem;
			Track track = new Track(this) { Type = TreeItems[0].Children.IndexOf(parentTreeItem) == 0 ? TrackType.Power : TrackType.Brake };

			Tracks.Add(track);
			parentTreeItem.Children.Add(new TreeViewItemModel(parentTreeItem) { Title = parentTreeItem.Children.Count.ToString(Culture), Tag = track });
			SelectedTreeItem = parentTreeItem.Children.Last();
		}

		internal void RemoveTrack()
		{
			TreeViewItemModel parentTreeItem = SelectedTreeItem.Parent;
			Track track = (Track)SelectedTreeItem.Tag;

			Tracks.Remove(track);

			parentTreeItem.Children.Remove(SelectedTreeItem);
			RenameTreeViewItem(parentTreeItem.Children);

			SelectedTreeItem = null;
		}

		internal void CopyTrack()
		{
			TreeViewItemModel parentTreeItem = SelectedTreeItem.Parent;
			Track track = (Track)((Track)SelectedTreeItem.Tag).Clone();
			Tracks.Add(track);

			parentTreeItem.Children.Add(new TreeViewItemModel(parentTreeItem) { Title = parentTreeItem.Children.Count.ToString(Culture), Tag = track });
			SelectedTreeItem = parentTreeItem.Children.Last();
		}

		internal void ApplyTrackType()
		{
			TreeViewItemModel currentTreeItem = SelectedTreeItem;
			TreeViewItemModel parentTreeItem = currentTreeItem.Parent;
			Track currentTrack = (Track)currentTreeItem.Tag;
			int currentIndex = Tracks.IndexOf(currentTrack);

			Tracks.Move(currentIndex, Tracks.Count - 1);
			parentTreeItem.Children.Remove(currentTreeItem);

			TreeViewItemModel newTreeItem = new TreeViewItemModel(TreeItems[0].Children[(int)currentTrack.Type]) { Tag = currentTrack };
			TreeItems[0].Children[(int)currentTrack.Type].Children.Add(newTreeItem);

			RenameTreeViewItem(TreeItems[0].Children[0].Children);
			RenameTreeViewItem(TreeItems[0].Children[1].Children);

			SelectedTreeItem = newTreeItem;
		}

		private Quantity.Velocity XtoVelocity(double x)
		{
			return new Quantity.Velocity(MinVelocity + x / FactorVelocity, VelocityUnit);
		}

		private double YtoPitch(double y)
		{
			return MinPitch + (y - GlControlHeight) / FactorPitch;
		}

		private double YtoVolume(double y)
		{
			return MinVolume + (y - GlControlHeight) / FactorVolume;
		}

		private double VelocityToX(Quantity.Velocity v)
		{
			return (v - minVelocity).ToNewUnit(VelocityUnit).Value * FactorVelocity;
		}

		private double PitchToY(double p)
		{
			return GlControlHeight + (p - MinPitch) * FactorPitch;
		}

		private double VolumeToY(double v)
		{
			return GlControlHeight + (v - MinVolume) * FactorVolume;
		}

		internal void ZoomIn()
		{
			Utilities.ZoomIn(ref minVelocity, ref maxVelocity);

			OnPropertyChanged(new PropertyChangedEventArgs(nameof(MinVelocity)));
			OnPropertyChanged(new PropertyChangedEventArgs(nameof(MaxVelocity)));

			switch (CurrentInputMode)
			{
				case InputMode.Pitch:
					Utilities.ZoomIn(ref minPitch, ref maxPitch);

					OnPropertyChanged(new PropertyChangedEventArgs(nameof(MinPitch)));
					OnPropertyChanged(new PropertyChangedEventArgs(nameof(MaxPitch)));
					break;
				case InputMode.Volume:
					Utilities.ZoomIn(ref minVolume, ref maxVolume);

					OnPropertyChanged(new PropertyChangedEventArgs(nameof(MinVolume)));
					OnPropertyChanged(new PropertyChangedEventArgs(nameof(MaxVolume)));
					break;
			}
		}

		internal void ZoomOut()
		{
			Utilities.ZoomOut(ref minVelocity, ref maxVelocity);

			OnPropertyChanged(new PropertyChangedEventArgs(nameof(MinVelocity)));
			OnPropertyChanged(new PropertyChangedEventArgs(nameof(MaxVelocity)));

			switch (CurrentInputMode)
			{
				case InputMode.Pitch:
					Utilities.ZoomOut(ref minPitch, ref maxPitch);

					OnPropertyChanged(new PropertyChangedEventArgs(nameof(MinPitch)));
					OnPropertyChanged(new PropertyChangedEventArgs(nameof(MaxPitch)));
					break;
				case InputMode.Volume:
					Utilities.ZoomOut(ref minVolume, ref maxVolume);

					OnPropertyChanged(new PropertyChangedEventArgs(nameof(MinVolume)));
					OnPropertyChanged(new PropertyChangedEventArgs(nameof(MaxVolume)));
					break;
			}
		}

		internal void Reset()
		{
			Utilities.Reset(new Quantity.Velocity(0.5 * 40.0, VelocityUnit), ref minVelocity, ref maxVelocity);

			OnPropertyChanged(new PropertyChangedEventArgs(nameof(MinVelocity)));
			OnPropertyChanged(new PropertyChangedEventArgs(nameof(MaxVelocity)));

			switch (CurrentInputMode)
			{
				case InputMode.Pitch:
					Utilities.Reset(0.5 * 400.0, ref minPitch, ref maxPitch);

					OnPropertyChanged(new PropertyChangedEventArgs(nameof(MinPitch)));
					OnPropertyChanged(new PropertyChangedEventArgs(nameof(MaxPitch)));
					break;
				case InputMode.Volume:
					Utilities.Reset(0.5 * 256.0, ref minVolume, ref maxVolume);

					OnPropertyChanged(new PropertyChangedEventArgs(nameof(MinVolume)));
					OnPropertyChanged(new PropertyChangedEventArgs(nameof(MaxVolume)));
					break;
			}
		}

		internal void MoveLeft()
		{
			Utilities.MoveNegative(ref minVelocity, ref maxVelocity);

			OnPropertyChanged(new PropertyChangedEventArgs(nameof(MinVelocity)));
			OnPropertyChanged(new PropertyChangedEventArgs(nameof(MaxVelocity)));
		}

		internal void MoveRight()
		{
			Utilities.MovePositive(ref minVelocity, ref maxVelocity);

			OnPropertyChanged(new PropertyChangedEventArgs(nameof(MinVelocity)));
			OnPropertyChanged(new PropertyChangedEventArgs(nameof(MaxVelocity)));
		}

		internal void MoveBottom()
		{
			switch (CurrentInputMode)
			{
				case InputMode.Pitch:
					Utilities.MoveNegative(ref minPitch, ref maxPitch);

					OnPropertyChanged(new PropertyChangedEventArgs(nameof(MinPitch)));
					OnPropertyChanged(new PropertyChangedEventArgs(nameof(MaxPitch)));
					break;
				case InputMode.Volume:
					Utilities.MoveNegative(ref minVolume, ref maxVolume);

					OnPropertyChanged(new PropertyChangedEventArgs(nameof(MinVolume)));
					OnPropertyChanged(new PropertyChangedEventArgs(nameof(MaxVolume)));
					break;
			}
		}

		internal void MoveTop()
		{
			switch (CurrentInputMode)
			{
				case InputMode.Pitch:
					Utilities.MovePositive(ref minPitch, ref maxPitch);

					OnPropertyChanged(new PropertyChangedEventArgs(nameof(MinPitch)));
					OnPropertyChanged(new PropertyChangedEventArgs(nameof(MaxPitch)));
					break;
				case InputMode.Volume:
					Utilities.MovePositive(ref minVolume, ref maxVolume);

					OnPropertyChanged(new PropertyChangedEventArgs(nameof(MinVolume)));
					OnPropertyChanged(new PropertyChangedEventArgs(nameof(MaxVolume)));
					break;
			}
		}

		internal void MouseMove(InputEventModel.EventArgs e)
		{
			NowVelocity = XtoVelocity(e.X).Value;
			NowPitch = YtoPitch(e.Y);
			NowVolume = YtoVolume(e.Y);

			(SelectedTreeItem.Tag as Track)?.MouseMove(e);
		}

		private static void UpdateViewport(RenderingMode renderingMode, Vector2 point, Vector2 delta)
		{
			GL.Viewport(0, 0, GlControlWidth, GlControlHeight);

			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();

			if (renderingMode == RenderingMode.Select)
			{
				if (delta.X <= 0.0 || delta.Y <= 0.0)
				{
					return;
				}

				GL.Translate((GlControlWidth - 2.0 * point.X) / delta.X, (GlControlHeight - 2.0 * point.Y) / delta.Y, 0);
				GL.Scale(GlControlWidth / delta.X, GlControlHeight / delta.Y, 1.0);
			}
		}

		private void CreateMatrix(out Matrix4D projPitch, out Matrix4D projVolume, out Matrix4D projString, out Matrix4D lookPitch, out Matrix4D lookVolume)
		{
			Matrix4D.CreateOrthographic(MaxVelocity - MinVelocity, MaxPitch - MinPitch, float.Epsilon, 1.0, out projPitch);
			Matrix4D.CreateOrthographic(MaxVelocity - MinVelocity, MaxVolume - MinVolume, float.Epsilon, 1.0, out projVolume);
			Matrix4D.CreateOrthographicOffCenter(0.0, GlControlWidth, GlControlHeight, 0.0, -1.0, 1.0, out projString);
			lookPitch = Matrix4D.LookAt(new Vector3((MinVelocity + MaxVelocity) / 2.0, (MinPitch + MaxPitch) / 2.0, float.Epsilon), new Vector3((MinVelocity + MaxVelocity) / 2.0, (MinPitch + MaxPitch) / 2.0, 0.0), new Vector3(0, 1, 0));
			lookVolume = Matrix4D.LookAt(new Vector3((MinVelocity + MaxVelocity) / 2.0, (MinVolume + MaxVolume) / 2.0, float.Epsilon), new Vector3((MinVelocity + MaxVelocity) / 2.0, (MinVolume + MaxVolume) / 2.0, 0.0), new Vector3(0, 1, 0));
		}

		internal void DrawGlControl()
		{
			UpdateViewport(RenderingMode.Render, Vector2.Null, Vector2.Null);

			GL.ClearColor(Color.Black);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			Matrix4D projPitch, projVolume, projString, lookPitch, lookVolume;
			CreateMatrix(out projPitch, out projVolume, out projString, out lookPitch, out lookVolume);

			// vertical grid
			{
				unsafe
				{
					GL.MatrixMode(MatrixMode.Projection);
					double* matrixPointer = &projPitch.Row0.X;
					GL.LoadMatrix(matrixPointer);
					GL.MatrixMode(MatrixMode.Modelview);
					matrixPointer = &lookPitch.Row0.X;
					GL.LoadMatrix(matrixPointer);
				}

				GL.Begin(PrimitiveType.Lines);

				for (double v = 0.0; v < MaxVelocity; v += 10.0)
				{
					GL.Color4(Color.DimGray);
					GL.Vertex2(v, 0.0);
					GL.Vertex2(v, float.MaxValue);
				}

				GL.End();

				Program.Renderer.CurrentProjectionMatrix = projString;
				Program.Renderer.CurrentViewMatrix = Matrix4D.Identity;

				for (double v = 0.0; v < MaxVelocity; v += 10.0)
				{
					Program.Renderer.OpenGlString.Draw(Fonts.VerySmallFont, v.ToString("0", Culture), new Vector2((int)VelocityToX(new Quantity.Velocity(v, VelocityUnit)) + 1, 1), TextAlignment.TopLeft, new Color128(Color24.Grey));
				}

				GL.Disable(EnableCap.Texture2D);
			}

			// horizontal grid
			{
				switch (CurrentInputMode)
				{
					case InputMode.Pitch:
						unsafe
						{
							GL.MatrixMode(MatrixMode.Projection);
							double* matrixPointer = &projPitch.Row0.X;
							GL.LoadMatrix(matrixPointer);
							GL.MatrixMode(MatrixMode.Modelview);
							matrixPointer = &lookPitch.Row0.X;
							GL.LoadMatrix(matrixPointer);
						}

						GL.Begin(PrimitiveType.Lines);

						for (double p = 0.0; p < MaxPitch; p += 100.0)
						{
							GL.Color4(Color.DimGray);
							GL.Vertex2(MinVelocity, p);
							GL.Vertex2(MaxVelocity, p);
						}

						GL.End();

						Program.Renderer.CurrentProjectionMatrix = projString;
						Program.Renderer.CurrentViewMatrix = Matrix4D.Identity;

						for (double p = 0.0; p < MaxPitch; p += 100.0)
						{
							Program.Renderer.OpenGlString.Draw(Fonts.VerySmallFont, p.ToString("0", Culture), new Vector2(1, (int)PitchToY(p) + 1), TextAlignment.TopLeft, new Color128(Color24.Grey));
						}

						GL.Disable(EnableCap.Texture2D);
						break;
					case InputMode.Volume:
						unsafe
						{
							GL.MatrixMode(MatrixMode.Projection);
							double* matrixPointer = &projVolume.Row0.X;
							GL.LoadMatrix(matrixPointer);
							GL.MatrixMode(MatrixMode.Modelview);
							matrixPointer = &lookVolume.Row0.X;
							GL.LoadMatrix(matrixPointer);
						}

						GL.Begin(PrimitiveType.Lines);

						for (double v = 0.0; v < MaxVolume; v += 128.0)
						{
							GL.Color4(Color.DimGray);
							GL.Vertex2(MinVelocity, v);
							GL.Vertex2(MaxVelocity, v);
						}

						GL.End();

						Program.Renderer.CurrentProjectionMatrix = projString;
						Program.Renderer.CurrentViewMatrix = Matrix4D.Identity;

						for (double v = 0.0; v < MaxVolume; v += 128.0)
						{
							Program.Renderer.OpenGlString.Draw(Fonts.VerySmallFont, v.ToString("0", Culture), new Vector2(1, (int)VolumeToY(v) + 1), TextAlignment.TopLeft, new Color128(Color24.Grey));
						}

						GL.Disable(EnableCap.Texture2D);
						break;
				}
			}

			if (SelectedTreeItem != null)
			{
				Track selectedTrack = SelectedTreeItem.Tag as Track;

				if (selectedTrack != null)
				{
					selectedTrack.DrawGlControl(projPitch, projVolume, lookPitch, lookVolume, false);
				}
				else
				{
					IEnumerable<Track> selectedTracks;

					if (SelectedTreeItem == TreeItems[0])
					{
						selectedTracks = SelectedTreeItem.Children.SelectMany(x => x.Children).Where(x => x.Checked).Select(x => x.Tag).Cast<Track>();
					}
					else
					{
						selectedTracks = SelectedTreeItem.Children.Where(x => x.Checked).Select(x => x.Tag).Cast<Track>();
					}

					foreach (Track track in selectedTracks)
					{
						track.DrawGlControl(projPitch, projVolume, lookPitch, lookVolume, true);
					}
				}
			}

			// simulation speed
			if (CurrentSimState == SimulationState.Started || CurrentSimState == SimulationState.Paused)
			{
				unsafe
				{
					GL.MatrixMode(MatrixMode.Projection);
					double* matrixPointer = &projPitch.Row0.X;
					GL.LoadMatrix(matrixPointer);
					GL.MatrixMode(MatrixMode.Modelview);
					matrixPointer = &lookPitch.Row0.X;
					GL.LoadMatrix(matrixPointer);
				}

				GL.LineWidth(3.0f);
				GL.Begin(PrimitiveType.Lines);

				GL.Color4(Color.White);
				GL.Vertex2(currentSimSpeed.Value, 0.0);
				GL.Vertex2(currentSimSpeed.Value, float.MaxValue);

				GL.End();
			}

			IsRefreshGlControl = false;
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

			if (TrainEditor.PlayerTrain == null)
			{
				Interface.AddMessage(MessageType.Error, false, "Failed to create train.");
				CurrentSimState = SimulationState.Disable;
				return;
			}

			oldElapsedTime = 0;
			startTime = DateTime.Now;

			if (CurrentSimState != SimulationState.Paused)
			{
				currentSimSpeed = startSpeed;
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
			IsRefreshGlControl = true;
		}

		private void CreateCar()
		{
			DisposeCar();

			TrainEditor.PlayerTrain = new TrainEditor.Train();
			TrainEditor.PlayerTrain.Car.Sounds.Motor.PowerTables = Tracks.Where(x => x.Type == TrackType.Power).Select(x => Track.TrackToMotorSoundTable(x, y => 0.01 * y, y => Math.Pow(0.0078125 * y, 0.25))).ToArray();
			TrainEditor.PlayerTrain.Car.Sounds.Motor.BrakeTables = Tracks.Where(x => x.Type == TrackType.Brake).Select(x => Track.TrackToMotorSoundTable(x, y => 0.01 * y, y => Math.Pow(0.0078125 * y, 0.25))).ToArray();
			TrainEditor.PlayerTrain.Car.ApplySounds();
		}

		internal void RunSimulation()
		{
			if (TrainEditor.PlayerTrain == null)
			{
				return;
			}

			double nowElapsedTime = (DateTime.Now - startTime).TotalSeconds;

			if (oldElapsedTime == 0.0)
			{
				oldElapsedTime = nowElapsedTime;
			}

			double deltaTime = nowElapsedTime - oldElapsedTime;

			Quantity.Acceleration outputAcceleration = Math.Sign((endSpeed - startSpeed).Value) * Acceleration;

			currentSimSpeed += new Quantity.Velocity(outputAcceleration.ToDefaultUnit().Value * deltaTime);
			Quantity.Velocity minSpeed = endSpeed >= startSpeed ? startSpeed : endSpeed;
			Quantity.Velocity maxSpeed = endSpeed >= startSpeed ? endSpeed : startSpeed;

			if (IsLoop)
			{
				if (currentSimSpeed < minSpeed)
				{
					currentSimSpeed = maxSpeed;
					outputAcceleration = new Quantity.Acceleration(0.0);
				}

				if (currentSimSpeed > maxSpeed)
				{
					currentSimSpeed = minSpeed;
					outputAcceleration = new Quantity.Acceleration(0.0);
				}

				if (IsConstant)
				{
					currentSimSpeed = startSpeed;
					outputAcceleration = Math.Sign((endSpeed - startSpeed).Value) * acceleration;
				}
			}
			else
			{
				if (currentSimSpeed < minSpeed || currentSimSpeed > maxSpeed)
				{
					StopSimulation();
					return;
				}
			}

			TrainEditor.PlayerTrain.Car.Specs.CurrentSpeed = TrainEditor.PlayerTrain.Car.Specs.CurrentPerceivedSpeed = currentSimSpeed.ToDefaultUnit().Value;
			TrainEditor.PlayerTrain.Car.Specs.CurrentAccelerationOutput = outputAcceleration.ToDefaultUnit().Value;

			TrainEditor.PlayerTrain.Car.UpdateRunSounds(deltaTime, RunIndex);

			TrainEditor.PlayerTrain.Car.UpdateMotorSounds(TreeItems[0].Children[0].Children.Select(x => x.Checked).ToArray(), TreeItems[0].Children[1].Children.Select(x => x.Checked).ToArray());

			Program.SoundApi.Update(deltaTime, SoundModels.Inverse);

			oldElapsedTime = nowElapsedTime;

			DrawSimulation();
		}

		private void DrawSimulation()
		{
			Quantity.Velocity rangeVelocity = maxVelocity - minVelocity;

			if (startSpeed <= endSpeed)
			{
				if (currentSimSpeed < minVelocity || currentSimSpeed > maxVelocity)
				{
					minVelocity = new Quantity.Velocity(10.0 * Math.Round(0.1 * currentSimSpeed.Value), VelocityUnit);

					if (minVelocity.Value < 0.0)
					{
						minVelocity = new Quantity.Velocity(0.0, VelocityUnit);
					}

					maxVelocity = minVelocity + rangeVelocity;

					OnPropertyChanged(new PropertyChangedEventArgs(nameof(MinVelocity)));
					OnPropertyChanged(new PropertyChangedEventArgs(nameof(MaxVelocity)));

					return;
				}
			}
			else
			{
				if (currentSimSpeed < minVelocity || currentSimSpeed > maxVelocity)
				{
					maxVelocity = new Quantity.Velocity(10.0 * Math.Round(0.1 * currentSimSpeed.Value), VelocityUnit);

					if (maxVelocity < rangeVelocity)
					{
						maxVelocity = rangeVelocity;
					}

					minVelocity = maxVelocity - rangeVelocity;

					OnPropertyChanged(new PropertyChangedEventArgs(nameof(MinVelocity)));
					OnPropertyChanged(new PropertyChangedEventArgs(nameof(MaxVelocity)));

					return;
				}
			}

			IsRefreshGlControl = true;
		}

		private void DisposeCar()
		{
			if (TrainEditor.PlayerTrain != null)
			{
				TrainEditor.PlayerTrain.Dispose();
				TrainEditor.PlayerTrain = null;
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
			motor.SelectedTreeItem = null;
			return motor;
		}
	}
}
