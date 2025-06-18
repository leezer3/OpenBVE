using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using OpenTK;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Dialogs;
using TrainEditor2.Models.Others;
using TrainManager.Motor;

namespace TrainEditor2.Models.Trains
{
	internal partial class Motor : BindableBase, ICloneable
	{
		internal class Vertex : BindableBase, ICloneable
		{
			private OpenBveApi.Math.Vector2 position;
			private bool selected;
			private bool isOrigin;

			internal double X
			{
				get => position.X;
				set => SetProperty(ref position.X, value);
			}

			internal double Y
			{
				get => position.Y;
				set => SetProperty(ref position.Y, value);
			}

			internal bool Selected
			{
				get => selected;
				set => SetProperty(ref selected, value);
			}

			internal bool IsOrigin
			{
				get => isOrigin;
				set => SetProperty(ref isOrigin, value);
			}

			internal Vertex(double x, double y)
			{
				X = x;
				Y = y;
				Selected = false;
				IsOrigin = false;
			}

			public object Clone()
			{
				return MemberwiseClone();
			}
		}

		internal class Line : BindableBase, ICloneable
		{
			private int leftID;
			private int rightID;
			private bool selected;

			internal int LeftID
			{
				get => leftID;
				private set => SetProperty(ref leftID, value);
			}

			internal int RightID
			{
				get => rightID;
				private set => SetProperty(ref rightID, value);
			}

			internal bool Selected
			{
				get => selected;
				set => SetProperty(ref selected, value);
			}

			internal Line(int leftID, int rightID)
			{
				LeftID = leftID;
				RightID = rightID;
				Selected = false;
			}

			public object Clone()
			{
				return MemberwiseClone();
			}
		}

		internal class Area : BindableBase, ICloneable
		{
			private double leftX;
			private double rightX;
			private int index;
			private bool tbd;

			internal double LeftX
			{
				get => leftX;
				set => SetProperty(ref leftX, value);
			}

			internal double RightX
			{
				get => rightX;
				set => SetProperty(ref rightX, value);
			}

			internal int Index
			{
				get => index;
				private set => SetProperty(ref index, value);
			}

			internal bool TBD
			{
				get => tbd;
				set => SetProperty(ref tbd, value);
			}

			internal Area(double leftX, double rightX, int index)
			{
				LeftX = leftX;
				RightX = rightX;
				Index = index;
				TBD = false;
			}

			public object Clone()
			{
				return MemberwiseClone();
			}
		}

		internal class VertexLibrary : ObservableDictionary<int, Vertex>, ICloneable
		{
			private int lastID;

			internal VertexLibrary()
			{
				lastID = -1;
			}

			internal void Add(Vertex vertex)
			{
				if (this.Any(v => v.Value.X == vertex.X))
				{
					int id = this.First(v => v.Value.X == vertex.X).Key;
					base[id] = vertex;
				}
				else
				{
					Add(++lastID, vertex);
				}
			}

			public object Clone()
			{
				VertexLibrary vertices = new VertexLibrary
				{
					lastID = lastID
				};

				foreach (KeyValuePair<int, Vertex> vertex in this)
				{
					vertices.Add(vertex.Key, (Vertex)vertex.Value.Clone());
				}

				return vertices;
			}
		}

		internal class Track : ICloneable
		{
			internal VertexLibrary PitchVertices;
			internal ObservableCollection<Line> PitchLines;

			internal VertexLibrary VolumeVertices;
			internal ObservableCollection<Line> VolumeLines;

			internal ObservableCollection<Area> SoundIndices;

			internal Track()
			{
				PitchVertices = new VertexLibrary();
				PitchLines = new ObservableCollection<Line>();

				VolumeVertices = new VertexLibrary();
				VolumeLines = new ObservableCollection<Line>();

				SoundIndices = new ObservableCollection<Area>();
			}

			public object Clone()
			{
				Track track = (Track)MemberwiseClone();

				track.PitchVertices = (VertexLibrary)PitchVertices.Clone();
				track.PitchLines = new ObservableCollection<Line>(PitchLines.Select(l => (Line)l.Clone()));

				track.VolumeVertices = (VertexLibrary)VolumeVertices.Clone();
				track.VolumeLines = new ObservableCollection<Line>(VolumeLines.Select(l => (Line)l.Clone()));

				track.SoundIndices = new ObservableCollection<Area>(SoundIndices.Select(a => (Area)a.Clone()));

				return track;
			}

			internal static Track EntriesToTrack(BVEMotorSoundTableEntry[] entries)
			{
				Track track = new Track();

				for (int i = 0; i < entries.Length; i++)
				{
					double velocity = 0.2 * i;

					if (track.PitchVertices.Count >= 2)
					{
						KeyValuePair<int, Vertex>[] leftVertices = { track.PitchVertices.ElementAt(track.PitchVertices.Count - 2), track.PitchVertices.Last() };
						Func<double, double> f = x => leftVertices[0].Value.Y + (leftVertices[1].Value.Y - leftVertices[0].Value.Y) / (leftVertices[1].Value.X - leftVertices[0].Value.X) * (x - leftVertices[0].Value.X);

						if (f(velocity) == entries[i].Pitch)
						{
							track.PitchVertices.Remove(leftVertices[1].Key);
						}
					}

					track.PitchVertices.Add(new Vertex(velocity, 0.01 * Math.Round(100.0 * entries[i].Pitch)));

					if (track.VolumeVertices.Count >= 2)
					{
						KeyValuePair<int, Vertex>[] leftVertices = { track.VolumeVertices.ElementAt(track.VolumeVertices.Count - 2), track.VolumeVertices.Last() };
						Func<double, double> f = x => leftVertices[0].Value.Y + (leftVertices[1].Value.Y - leftVertices[0].Value.Y) / (leftVertices[1].Value.X - leftVertices[0].Value.X) * (x - leftVertices[0].Value.X);

						if (f(velocity) == entries[i].Gain)
						{
							track.VolumeVertices.Remove(leftVertices[1].Key);
						}
					}

					track.VolumeVertices.Add(new Vertex(velocity, 0.01 * Math.Round(100.0 * entries[i].Gain)));

					if (track.SoundIndices.Any())
					{
						Area leftArea = track.SoundIndices.Last();

						if (entries[i].SoundIndex != leftArea.Index)
						{
							leftArea.RightX = velocity - 0.2;
							track.SoundIndices.Add(new Area(velocity, velocity, entries[i].SoundIndex));
						}
						else
						{
							leftArea.RightX = velocity;
						}
					}
					else
					{
						track.SoundIndices.Add(new Area(velocity, velocity, entries[i].SoundIndex));
					}
				}

				for (int j = 0; j < track.PitchVertices.Count - 1; j++)
				{
					track.PitchLines.Add(new Line(track.PitchVertices.ElementAt(j).Key, track.PitchVertices.ElementAt(j + 1).Key));
				}

				for (int j = 0; j < track.VolumeVertices.Count - 1; j++)
				{
					track.VolumeLines.Add(new Line(track.VolumeVertices.ElementAt(j).Key, track.VolumeVertices.ElementAt(j + 1).Key));
				}

				if (track.SoundIndices.Any())
				{
					Area lastArea = track.SoundIndices.Last();

					if (lastArea.LeftX == lastArea.RightX)
					{
						lastArea.RightX += 0.2;
					}
				}

				return track;
			}

			internal static BVEMotorSoundTableEntry[] TrackToEntries(Track track)
			{
				int n = 0;

				if (track.PitchVertices.Any())
				{
					n = Math.Max(n, (int)Math.Round(5.0 * track.PitchVertices.Last().Value.X));
				}

				if (track.VolumeVertices.Any())
				{
					n = Math.Max(n, (int)Math.Round(5.0 * track.VolumeVertices.Last().Value.X));
				}

				BVEMotorSoundTableEntry[] entries = Enumerable.Repeat(new BVEMotorSoundTableEntry { SoundIndex = -1, Pitch = 100.0f, Gain = 128.0f }, n + 1).ToArray();

				for (int i = 0; i < entries.Length; i++)
				{
					double velocity = 0.2 * i;

					Line pitchLine = track.PitchLines.FirstOrDefault(l => track.PitchVertices[l.LeftID].X <= velocity && track.PitchVertices[l.RightID].X >= velocity);

					if (pitchLine != null)
					{
						Vertex left = track.PitchVertices[pitchLine.LeftID];
						Vertex right = track.PitchVertices[pitchLine.RightID];

						Func<double, double> f = x => left.Y + (right.Y - left.Y) / (right.X - left.X) * (x - left.X);

						entries[i].Pitch = (float)(0.01 * Math.Round(100.0 * Math.Max(f(velocity), 0.0)));
					}

					Line volumeLine = track.VolumeLines.FirstOrDefault(l => track.VolumeVertices[l.LeftID].X <= velocity && track.VolumeVertices[l.RightID].X >= velocity);

					if (volumeLine != null)
					{
						Vertex left = track.VolumeVertices[volumeLine.LeftID];
						Vertex right = track.VolumeVertices[volumeLine.RightID];

						Func<double, double> f = x => left.Y + (right.Y - left.Y) / (right.X - left.X) * (x - left.X);

						entries[i].Gain = (float)(0.01 * Math.Round(100.0 * Math.Max(f(velocity), 0.0)));
					}

					Area area = track.SoundIndices.FirstOrDefault(a => a.LeftX <= velocity && a.RightX >= velocity);

					if (area != null)
					{
						entries[i].SoundIndex = area.Index;
					}
				}

				return entries;
			}

			internal static BVEMotorSoundTable EntriesToMotorSoundTable(BVEMotorSoundTableEntry[] entries)
			{
				BVEMotorSoundTable table = new BVEMotorSoundTable
				{
					Entries = new BVEMotorSoundTableEntry[entries.Length]
				};

				for (int i = 0; i < entries.Length; i++)
				{
					table.Entries[i].Pitch = (float)(0.01 * entries[i].Pitch);
					table.Entries[i].Gain = (float)Math.Pow(0.0078125 * entries[i].Gain, 0.25);
					table.Entries[i].SoundIndex = entries[i].SoundIndex;
				}

				return table;
			}
		}

		internal enum TrackInfo
		{
			Power1,
			Power2,
			Brake1,
			Brake2
		}

		internal enum InputMode
		{
			Pitch,
			Volume,
			SoundIndex
		}

		internal enum ToolMode
		{
			Select,
			Move,
			Dot,
			Line
		}

		internal enum SimulationState
		{
			Disable,
			Stopped,
			Paused,
			Started
		}

		internal class TrackState : ICloneable
		{
			internal TrackInfo Info
			{
				get;
				private set;
			}

			internal Track Track
			{
				get;
				private set;
			}

			internal TrackState(TrackInfo info, Track track)
			{
				Info = info;
				Track = track;
			}

			public object Clone()
			{
				TrackState state = (TrackState)MemberwiseClone();
				state.Track = (Track)Track.Clone();
				return state;
			}
		}

		internal class SelectedRange
		{
			internal Box2d Range
			{
				get;
				private set;
			}

			internal Vertex[] SelectedVertices
			{
				get;
				private set;
			}

			internal Line[] SelectedLines
			{
				get;
				private set;
			}

			private SelectedRange(Box2d range, Vertex[] selectedVertices, Line[] selectedLines)
			{
				Range = range;
				SelectedVertices = selectedVertices;
				SelectedLines = selectedLines;
			}

			internal static SelectedRange CreateSelectedRange(VertexLibrary vertices, ObservableCollection<Line> lines, double leftX, double rightX, double topY, double bottomY)
			{
				Func<Vertex, bool> conditionVertex = v => v.X >= leftX && v.X <= rightX && v.Y >= bottomY && v.Y <= topY;

				Vertex[] selectedVertices = vertices.Values.Where(v => conditionVertex(v)).ToArray();
				Line[] selectedLines = lines.Where(l => selectedVertices.Any(v => v.X == vertices[l.LeftID].X) && selectedVertices.Any(v => v.X == vertices[l.RightID].X)).ToArray();

				return new SelectedRange(new Box2d(leftX, topY, rightX, bottomY), selectedVertices, selectedLines);
			}
		}

		private readonly CultureInfo culture;

		private MessageBox messageBox;
		private ToolTipModel toolTipVertexPitch;
		private ToolTipModel toolTipVertexVolume;
		private InputEventModel.ModifierKeys currentModifierKeys;
		private InputEventModel.CursorType currentCursorType;

		private TrackInfo selectedTrackInfo;
		private int selectedSoundIndex;
		private Track copyTrack;
		private double minVelocity;
		private double maxVelocity;
		private double minPitch;
		private double maxPitch;
		private double minVolume;
		private double maxVolume;
		private double nowVelocity;
		private double nowPitch;
		private double nowVolume;

		private InputMode currentInputMode;
		private ToolMode currentToolMode;
		private SimulationState currentSimState;

		private double lastMousePosX;
		private double lastMousePosY;

		private bool isMoving;
		private Area previewArea;
		private SelectedRange selectedRange;
		private Vertex hoveredVertexPitch;
		private Vertex hoveredVertexVolume;

		private int runIndex;
		private bool isPlayTrack1;
		private bool isPlayTrack2;
		private bool isLoop;
		private bool isConstant;
		private double acceleration;
		private double startSpeed;
		private double endSpeed;

		private DateTime startTime;
		private double oldElapsedTime;
		private double nowSpeed;

		private int glControlWidth;
		private int glControlHeight;
		private bool isRefreshGlControl;

		internal ObservableCollection<Track> Tracks;
		internal ObservableCollection<TrackState> PrevTrackStates;
		internal ObservableCollection<TrackState> NextTrackStates;

		internal MessageBox MessageBox
		{
			get => messageBox;
			set => SetProperty(ref messageBox, value);
		}

		internal ToolTipModel ToolTipVertexPitch
		{
			get => toolTipVertexPitch;
			set => SetProperty(ref toolTipVertexPitch, value);
		}

		internal ToolTipModel ToolTipVertexVolume
		{
			get => toolTipVertexVolume;
			set => SetProperty(ref toolTipVertexVolume, value);
		}

		internal InputEventModel.ModifierKeys CurrentModifierKeys
		{
			get => currentModifierKeys;
			set => SetProperty(ref currentModifierKeys, value);
		}

		internal InputEventModel.CursorType CurrentCursorType
		{
			get => currentCursorType;
			set => SetProperty(ref currentCursorType, value);
		}

		internal TrackInfo SelectedTrackInfo
		{
			get => selectedTrackInfo;
			set => SetProperty(ref selectedTrackInfo, value);
		}

		internal Track SelectedTrack
		{
			get => Tracks[(int)SelectedTrackInfo];
			set => Tracks[(int)SelectedTrackInfo] = value;
		}

		internal int SelectedSoundIndex
		{
			get => selectedSoundIndex;
			set => SetProperty(ref selectedSoundIndex, value);
		}

		internal Track CopyTrack
		{
			get => copyTrack;
			set => SetProperty(ref copyTrack, value);
		}

		internal double MinVelocity
		{
			get => minVelocity;
			set => SetProperty(ref minVelocity, value);
		}

		internal double MaxVelocity
		{
			get => maxVelocity;
			set => SetProperty(ref maxVelocity, value);
		}

		internal double MinPitch
		{
			get => minPitch;
			set => SetProperty(ref minPitch, value);
		}

		internal double MaxPitch
		{
			get => maxPitch;
			set => SetProperty(ref maxPitch, value);
		}

		internal double MinVolume
		{
			get => minVolume;
			set => SetProperty(ref minVolume, value);
		}

		internal double MaxVolume
		{
			get => maxVolume;
			set => SetProperty(ref maxVolume, value);
		}

		internal double NowVelocity
		{
			get => nowVelocity;
			set => SetProperty(ref nowVelocity, value);
		}

		internal double NowPitch
		{
			get => nowPitch;
			set => SetProperty(ref nowPitch, value);
		}

		internal double NowVolume
		{
			get => nowVolume;
			set => SetProperty(ref nowVolume, value);
		}

		internal InputMode CurrentInputMode
		{
			get => currentInputMode;
			set => SetProperty(ref currentInputMode, value);
		}

		internal ToolMode CurrentToolMode
		{
			get => currentToolMode;
			set => SetProperty(ref currentToolMode, value);
		}

		internal SimulationState CurrentSimState
		{
			get => currentSimState;
			set => SetProperty(ref currentSimState, value);
		}

		internal int RunIndex
		{
			get => runIndex;
			set => SetProperty(ref runIndex, value);
		}

		internal bool IsPlayTrack1
		{
			get => isPlayTrack1;
			set => SetProperty(ref isPlayTrack1, value);
		}

		internal bool IsPlayTrack2
		{
			get => isPlayTrack2;
			set => SetProperty(ref isPlayTrack2, value);
		}

		internal bool IsLoop
		{
			get => isLoop;
			set => SetProperty(ref isLoop, value);
		}

		internal bool IsConstant
		{
			get => isConstant;
			set => SetProperty(ref isConstant, value);
		}

		internal double Acceleration
		{
			get => acceleration;
			set => SetProperty(ref acceleration, value);
		}

		internal double StartSpeed
		{
			get => startSpeed;
			set => SetProperty(ref startSpeed, value);
		}

		internal double EndSpeed
		{
			get => endSpeed;
			set => SetProperty(ref endSpeed, value);
		}

		internal int GlControlWidth
		{
			get => glControlWidth;
			set => SetProperty(ref glControlWidth, value);
		}

		internal int GlControlHeight
		{
			get => glControlHeight;
			set => SetProperty(ref glControlHeight, value);
		}

		internal bool IsRefreshGlControl
		{
			get => isRefreshGlControl;
			set => SetProperty(ref isRefreshGlControl, value);
		}

		internal Motor()
		{
			culture = CultureInfo.InvariantCulture;

			MessageBox = new MessageBox();
			ToolTipVertexPitch = new ToolTipModel();
			ToolTipVertexVolume = new ToolTipModel();
			CurrentCursorType = InputEventModel.CursorType.Arrow;

			Tracks = new ObservableCollection<Track>();

			for (int i = 0; i < 4; i++)
			{
				Tracks.Add(new Track());
			}

			SelectedTrackInfo = TrackInfo.Power1;
			SelectedSoundIndex = -1;

			PrevTrackStates = new ObservableCollection<TrackState>();
			NextTrackStates = new ObservableCollection<TrackState>();

			MinVelocity = 0.0;
			MaxVelocity = 40.0;

			MinPitch = 0.0;
			MaxPitch = 400.0;

			MinVolume = 0.0;
			MaxVolume = 256.0;

			CurrentSimState = SimulationState.Stopped;
			RunIndex = -1;
			IsPlayTrack1 = IsPlayTrack2 = true;
			Acceleration = 2.6;
			StartSpeed = 0.0;
			EndSpeed = 160.0;

			GlControlWidth = 568;
			GlControlHeight = 593;
		}

		public object Clone()
		{
			Motor motor = (Motor)MemberwiseClone();
			motor.MessageBox = new MessageBox();
			ToolTipVertexPitch = new ToolTipModel();
			ToolTipVertexVolume = new ToolTipModel();
			motor.previewArea = null;
			motor.selectedRange = null;
			motor.Tracks = new ObservableCollection<Track>(Tracks.Select(x => (Track)x.Clone()));
			motor.PrevTrackStates = new ObservableCollection<TrackState>(PrevTrackStates.Select(x => (TrackState)x.Clone()));
			motor.NextTrackStates = new ObservableCollection<TrackState>(NextTrackStates.Select(x => (TrackState)x.Clone()));
			return motor;
		}
	}
}
