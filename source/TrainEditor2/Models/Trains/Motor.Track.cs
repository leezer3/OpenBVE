using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpenTK;
using Prism.Mvvm;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Dialogs;
using TrainEditor2.Models.Others;

namespace TrainEditor2.Models.Trains
{
	internal partial class Motor
	{
		internal class Vertex : BindableBase, ICloneable
		{
			private double x;
			private double y;
			private bool selected;
			private bool isOrigin;

			internal double X
			{
				get
				{
					return x;
				}
				set
				{
					SetProperty(ref x, value);
				}
			}

			internal double Y
			{
				get
				{
					return y;
				}
				set
				{
					SetProperty(ref y, value);
				}
			}

			internal bool Selected
			{
				get
				{
					return selected;
				}
				set
				{
					SetProperty(ref selected, value);
				}
			}

			internal bool IsOrigin
			{
				get
				{
					return isOrigin;
				}
				set
				{
					SetProperty(ref isOrigin, value);
				}
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
				get
				{
					return leftID;
				}
				private set
				{
					SetProperty(ref leftID, value);
				}
			}

			internal int RightID
			{
				get
				{
					return rightID;
				}
				private set
				{
					SetProperty(ref rightID, value);
				}
			}

			internal bool Selected
			{
				get
				{
					return selected;
				}
				set
				{
					SetProperty(ref selected, value);
				}
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
				get
				{
					return leftX;
				}
				set
				{
					SetProperty(ref leftX, value);
				}
			}

			internal double RightX
			{
				get
				{
					return rightX;
				}
				set
				{
					SetProperty(ref rightX, value);
				}
			}

			internal int Index
			{
				get
				{
					return index;
				}
				private set
				{
					SetProperty(ref index, value);
				}
			}

			internal bool TBD
			{
				get
				{
					return tbd;
				}
				set
				{
					SetProperty(ref tbd, value);
				}
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

		internal enum TrackType
		{
			Power,
			Brake
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

		internal class SelectedRange
		{
			internal Box2d Range
			{
				get;
				// ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
				private set;
			}

			internal Vertex[] SelectedVertices
			{
				get;
				// ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
				private set;
			}

			internal Line[] SelectedLines
			{
				get;
				// ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
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

		internal class TrackState : ICloneable
		{
			internal VertexLibrary PitchVertices;
			internal ObservableCollection<Line> PitchLines;

			internal VertexLibrary VolumeVertices;
			internal ObservableCollection<Line> VolumeLines;

			internal ObservableCollection<Area> SoundIndices;

			private TrackState()
			{
				PitchVertices = new VertexLibrary();
				PitchLines = new ObservableCollection<Line>();

				VolumeVertices = new VertexLibrary();
				VolumeLines = new ObservableCollection<Line>();

				SoundIndices = new ObservableCollection<Area>();
			}

			internal TrackState(Track track)
			{
				PitchVertices = (VertexLibrary)track.PitchVertices.Clone();
				PitchLines = new ObservableCollection<Line>(track.PitchLines.Select(l => (Line)l.Clone()));

				VolumeVertices = (VertexLibrary)track.VolumeVertices.Clone();
				VolumeLines = new ObservableCollection<Line>(track.VolumeLines.Select(l => (Line)l.Clone()));

				SoundIndices = new ObservableCollection<Area>(track.SoundIndices.Select(a => (Area)a.Clone()));
			}

			internal void Apply(Track track)
			{
				track.PitchVertices = PitchVertices;
				track.PitchLines = PitchLines;

				track.VolumeVertices = VolumeVertices;
				track.VolumeLines = VolumeLines;

				track.SoundIndices = SoundIndices;
			}

			public object Clone()
			{
				return new TrackState
				{
					PitchVertices = (VertexLibrary)PitchVertices.Clone(),
					PitchLines = new ObservableCollection<Line>(PitchLines.Select(l => (Line)l.Clone())),

					VolumeVertices = (VertexLibrary)VolumeVertices.Clone(),
					VolumeLines = new ObservableCollection<Line>(VolumeLines.Select(l => (Line)l.Clone())),

					SoundIndices = new ObservableCollection<Area>(SoundIndices.Select(a => (Area)a.Clone())),
				};
			}
		}

		internal partial class Track : BindableBase, ICloneable
		{
			internal Motor BaseMotor;

			private MessageBox messageBox;
			private ToolTipModel toolTipVertexPitch;
			private ToolTipModel toolTipVertexVolume;
			private InputEventModel.ModifierKeys currentModifierKeys;
			private InputEventModel.CursorType currentCursorType;

			private int selectedSoundIndex;
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

			private double lastMousePosX;
			private double lastMousePosY;

			private bool isMoving;
			private Area previewArea;
			private SelectedRange selectedRange;
			private Vertex hoveredVertexPitch;
			private Vertex hoveredVertexVolume;

			private bool isRefreshGlControl;

			private TrackType type;

			internal VertexLibrary PitchVertices;
			internal ObservableCollection<Line> PitchLines;

			internal VertexLibrary VolumeVertices;
			internal ObservableCollection<Line> VolumeLines;

			internal ObservableCollection<Area> SoundIndices;

			internal ObservableCollection<TrackState> PrevStates;
			internal ObservableCollection<TrackState> NextStates;

			internal static double CurrentSimSpeed;

			private static int glControlWidth;
			private static int glControlHeight;

			internal MessageBox MessageBox
			{
				get
				{
					return messageBox;
				}
				set
				{
					SetProperty(ref messageBox, value);
				}
			}

			internal ToolTipModel ToolTipVertexPitch
			{
				get
				{
					return toolTipVertexPitch;
				}
				set
				{
					SetProperty(ref toolTipVertexPitch, value);
				}
			}

			internal ToolTipModel ToolTipVertexVolume
			{
				get
				{
					return toolTipVertexVolume;
				}
				set
				{
					SetProperty(ref toolTipVertexVolume, value);
				}
			}

			internal InputEventModel.ModifierKeys CurrentModifierKeys
			{
				get
				{
					return currentModifierKeys;
				}
				set
				{
					SetProperty(ref currentModifierKeys, value);
				}
			}

			internal InputEventModel.CursorType CurrentCursorType
			{
				get
				{
					return currentCursorType;
				}
				set
				{
					SetProperty(ref currentCursorType, value);
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

			internal double MinVelocity
			{
				get
				{
					return minVelocity;
				}
				set
				{
					SetProperty(ref minVelocity, value);
				}
			}

			internal double MaxVelocity
			{
				get
				{
					return maxVelocity;
				}
				set
				{
					SetProperty(ref maxVelocity, value);
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
					return nowVelocity;
				}
				set
				{
					SetProperty(ref nowVelocity, value);
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

			internal ToolMode CurrentToolMode
			{
				get
				{
					return currentToolMode;
				}
				set
				{
					SetProperty(ref currentToolMode, value);
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

			internal TrackType Type
			{
				get
				{
					return type;
				}
				set
				{
					SetProperty(ref type, value);
				}
			}

			internal Track(Motor baseMotor)
			{
				BaseMotor = baseMotor;

				MessageBox = new MessageBox();
				ToolTipVertexPitch = new ToolTipModel();
				ToolTipVertexVolume = new ToolTipModel();
				CurrentCursorType = InputEventModel.CursorType.Arrow;

				SelectedSoundIndex = -1;

				PrevStates = new ObservableCollection<TrackState>();
				NextStates = new ObservableCollection<TrackState>();

				MinVelocity = 0.0;
				MaxVelocity = 40.0;

				MinPitch = 0.0;
				MaxPitch = 400.0;

				MinVolume = 0.0;
				MaxVolume = 256.0;

				Type = TrackType.Power;

				PitchVertices = new VertexLibrary();
				PitchLines = new ObservableCollection<Line>();

				VolumeVertices = new VertexLibrary();
				VolumeLines = new ObservableCollection<Line>();

				SoundIndices = new ObservableCollection<Area>();
			}

			public object Clone()
			{
				Track track = (Track)MemberwiseClone();

				track.MessageBox = new MessageBox();
				track.ToolTipVertexPitch = new ToolTipModel();
				track.ToolTipVertexVolume = new ToolTipModel();
				track.previewArea = null;
				track.selectedRange = null;
				track.PrevStates = new ObservableCollection<TrackState>(PrevStates.Select(x => (TrackState)x.Clone()));
				track.NextStates = new ObservableCollection<TrackState>(NextStates.Select(x => (TrackState)x.Clone()));

				track.PitchVertices = (VertexLibrary)PitchVertices.Clone();
				track.PitchLines = new ObservableCollection<Line>(PitchLines.Select(l => (Line)l.Clone()));

				track.VolumeVertices = (VertexLibrary)VolumeVertices.Clone();
				track.VolumeLines = new ObservableCollection<Line>(VolumeLines.Select(l => (Line)l.Clone()));

				track.SoundIndices = new ObservableCollection<Area>(SoundIndices.Select(a => (Area)a.Clone()));

				return track;
			}
		}
	}
}
