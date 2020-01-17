using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpenTK;
using Prism.Mvvm;
using TrainEditor2.Models.Dialogs;
using TrainEditor2.Models.Others;

namespace TrainEditor2.Models.Trains
{
	internal partial class Motor
	{
		internal class Vertex : ICloneable
		{
			internal double X;
			internal double Y;
			internal bool Selected;
			internal bool IsOrigin;

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

		internal class Line : ICloneable
		{
			internal int LeftID;
			internal int RightID;
			internal bool Selected;

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

		internal class Area : ICloneable
		{
			internal double LeftX;
			internal double RightX;
			internal int Index;
			internal bool TBD;

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

		internal class VertexLibrary : Dictionary<int, Vertex>, ICloneable
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

			internal static SelectedRange CreateSelectedRange(VertexLibrary vertices, ICollection<Line> lines, double leftX, double rightX, double topY, double bottomY)
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
			internal List<Line> PitchLines;

			internal VertexLibrary VolumeVertices;
			internal List<Line> VolumeLines;

			internal List<Area> SoundIndices;

			private TrackState()
			{
				PitchVertices = new VertexLibrary();
				PitchLines = new List<Line>();

				VolumeVertices = new VertexLibrary();
				VolumeLines = new List<Line>();

				SoundIndices = new List<Area>();
			}

			internal TrackState(Track track)
			{
				PitchVertices = (VertexLibrary)track.PitchVertices.Clone();
				PitchLines = new List<Line>(track.PitchLines.Select(l => (Line)l.Clone()));

				VolumeVertices = (VertexLibrary)track.VolumeVertices.Clone();
				VolumeLines = new List<Line>(track.VolumeLines.Select(l => (Line)l.Clone()));

				SoundIndices = new List<Area>(track.SoundIndices.Select(a => (Area)a.Clone()));
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
					PitchLines = new List<Line>(PitchLines.Select(l => (Line)l.Clone())),

					VolumeVertices = (VertexLibrary)VolumeVertices.Clone(),
					VolumeLines = new List<Line>(VolumeLines.Select(l => (Line)l.Clone())),

					SoundIndices = new List<Area>(SoundIndices.Select(a => (Area)a.Clone())),
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

			private static ToolMode currentToolMode;

			private double lastMousePosX;
			private double lastMousePosY;

			private bool isMoving;
			private Area previewArea;
			private SelectedRange selectedRange;
			private Vertex hoveredVertexPitch;
			private Vertex hoveredVertexVolume;

			private TrackType type;

			internal VertexLibrary PitchVertices;
			internal List<Line> PitchLines;

			internal VertexLibrary VolumeVertices;
			internal List<Line> VolumeLines;

			internal List<Area> SoundIndices;

			internal ObservableCollection<TrackState> PrevStates;
			internal ObservableCollection<TrackState> NextStates;

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

				PrevStates = new ObservableCollection<TrackState>();
				NextStates = new ObservableCollection<TrackState>();

				Type = TrackType.Power;

				PitchVertices = new VertexLibrary();
				PitchLines = new List<Line>();

				VolumeVertices = new VertexLibrary();
				VolumeLines = new List<Line>();

				SoundIndices = new List<Area>();
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
				track.PitchLines = new List<Line>(PitchLines.Select(l => (Line)l.Clone()));

				track.VolumeVertices = (VertexLibrary)VolumeVertices.Clone();
				track.VolumeLines = new List<Line>(VolumeLines.Select(l => (Line)l.Clone()));

				track.SoundIndices = new List<Area>(SoundIndices.Select(a => (Area)a.Clone()));

				return track;
			}
		}
	}
}
