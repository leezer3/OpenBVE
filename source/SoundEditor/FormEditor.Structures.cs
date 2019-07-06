using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace SoundEditor
{
	internal static class ObservableCollectionExtensions
	{
		internal static void RemoveAll<T>(this ObservableCollection<T> collection, Func<T, bool> match)
		{
			for (int i = collection.Count - 1; i >= 0; i--)
			{
				if (match(collection[i]))
				{
					collection.RemoveAt(i);
				}
			}
		}
	}

	internal class StringValueAttribute : Attribute
	{
		internal readonly string StringValue;

		internal StringValueAttribute(string stringValue)
		{
			StringValue = stringValue;
		}
	}

	internal static class StringValueAttributeExtensions
	{
		internal static string GetStringValue(this Enum value)
		{
			Type type = value.GetType();

			FieldInfo fieldInfo = type.GetField(value.ToString());

			if (fieldInfo == null)
			{
				return null;
			}

			StringValueAttribute[] attributes = fieldInfo.GetCustomAttributes(typeof(StringValueAttribute), false).OfType<StringValueAttribute>().ToArray();

			return attributes.Any() ? attributes[0].StringValue : null;
		}
	}

	public partial class FormEditor
	{
		private class Vertex
		{
			internal float X;
			internal float Y;
			internal bool Selected;
			internal bool IsOrigin;

			internal Vertex(float x, float y)
			{
				X = x;
				Y = y;
				Selected = false;
				IsOrigin = false;
			}

			internal Vertex Clone()
			{
				return (Vertex)MemberwiseClone();
			}
		}

		private class Line
		{
			internal readonly int LeftID;
			internal readonly int RightID;
			internal bool Selected;

			internal Line(int leftID, int rightID)
			{
				LeftID = leftID;
				RightID = rightID;
				Selected = false;
			}

			internal Line Clone()
			{
				return (Line)MemberwiseClone();
			}
		}

		private class SelectedRange
		{
			internal readonly RectangleF Range;
			internal readonly Vertex[] SelectedVertices;
			internal readonly Line[] SelectedLines;

			private SelectedRange(RectangleF range, Vertex[] selectedVertices, Line[] selectedLines)
			{
				Range = range;
				SelectedVertices = selectedVertices;
				SelectedLines = selectedLines;
			}

			internal static SelectedRange CreateSelectedRange(VertexLibrary vertices, List<Line> lines, float leftX, float rightX, float topY, float bottomY)
			{
				Func<Vertex, bool> conditionVertex = v => v.X >= leftX && v.X <= rightX && v.Y >= bottomY && v.Y <= topY;

				Vertex[] selectedVertices = vertices.Values.Where(v => conditionVertex(v)).ToArray();
				Line[] selectedLines = lines.Where(l => selectedVertices.Any(v => v.X == vertices[l.LeftID].X) && selectedVertices.Any(v => v.X == vertices[l.RightID].X)).ToArray();

				return new SelectedRange(new RectangleF(leftX, topY, rightX - leftX, topY - bottomY), selectedVertices, selectedLines);
			}
		}

		private class Area
		{
			internal float LeftX;
			internal float RightX;
			internal readonly int Index;
			internal bool TBD;

			internal Area(float leftX, float rightX, int index)
			{
				LeftX = leftX;
				RightX = rightX;
				Index = index;
				TBD = false;
			}

			internal Area Clone()
			{
				return (Area)MemberwiseClone();
			}
		}

		private class VertexLibrary : Dictionary<int, Vertex>
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

			internal VertexLibrary Clone()
			{
				VertexLibrary cloned = new VertexLibrary { lastID = lastID };

				foreach (KeyValuePair<int, Vertex> vertex in this)
				{
					cloned.Add(vertex.Key, vertex.Value.Clone());
				}

				return cloned;
			}
		}

		private class Track
		{
			internal VertexLibrary PitchVertices;
			internal List<Line> PitchLines;

			internal VertexLibrary VolumeVertices;
			internal List<Line> VolumeLines;

			internal List<Area> SoundIndices;

			internal Track()
			{
				PitchVertices = new VertexLibrary();
				PitchLines = new List<Line>();

				VolumeVertices = new VertexLibrary();
				VolumeLines = new List<Line>();

				SoundIndices = new List<Area>();
			}

			internal Track Clone()
			{
				Track cloned = (Track)MemberwiseClone();

				cloned.PitchVertices = PitchVertices.Clone();
				cloned.PitchLines = PitchLines.Select(l => l.Clone()).ToList();

				cloned.VolumeVertices = VolumeVertices.Clone();
				cloned.VolumeLines = VolumeLines.Select(l => l.Clone()).ToList();

				cloned.SoundIndices = SoundIndices.Select(a => a.Clone()).ToList();

				return cloned;
			}
		}

		private class EditState
		{
			internal enum ViewMode
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

			private readonly FormEditor form;
			private ViewMode currentViewMode;
			private InputMode currentInputMode;
			private ToolMode currentToolMode;
			private SimulationState currentSimuState;

			internal EditState(FormEditor form)
			{
				this.form = form;
			}

			internal ViewMode CurrentViewMode
			{
				get
				{
					return currentViewMode;
				}
				set
				{
					currentViewMode = value;

					form.toolStripMenuItemPowerTrack1.CheckState = value == ViewMode.Power1 ? CheckState.Checked : CheckState.Unchecked;
					form.toolStripMenuItemPowerTrack2.CheckState = value == ViewMode.Power2 ? CheckState.Checked : CheckState.Unchecked;
					form.toolStripMenuItemBrakeTrack1.CheckState = value == ViewMode.Brake1 ? CheckState.Checked : CheckState.Unchecked;
					form.toolStripMenuItemBrakeTrack2.CheckState = value == ViewMode.Brake2 ? CheckState.Checked : CheckState.Unchecked;

					form.toolStripMenuItemUndo.Enabled = form.toolStripButtonUndo.Enabled = form.prevTrackStates.Any(s => s.Mode == value);
					form.toolStripMenuItemRedo.Enabled = form.toolStripButtonRedo.Enabled = form.nextTrackStates.Any(s => s.Mode == value);

					string type = GetInterfaceString("motor_sound", "status", "type", "name");
					string power = GetInterfaceString("motor_sound", "status", "type", "power");
					string brake = GetInterfaceString("motor_sound", "status", "type", "brake");
					string track = GetInterfaceString("motor_sound", "status", "track", "name");

					form.toolStripStatusLabelType.Text = string.Format("{0} {1}", type, value == ViewMode.Power1 || value == ViewMode.Power2 ? power : brake);
					form.toolStripStatusLabelTrack.Text = string.Format("{0} {1}", track, value == ViewMode.Power1 || value == ViewMode.Brake1 ? 1 : 2);

					form.DrawPictureBoxDrawArea();
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
					currentInputMode = value;

					form.toolStripMenuItemPitch.CheckState = value == InputMode.Pitch ? CheckState.Checked : CheckState.Unchecked;
					form.toolStripMenuItemVolume.CheckState = value == InputMode.Volume ? CheckState.Checked : CheckState.Unchecked;
					form.toolStripMenuItemIndex.CheckState = value == InputMode.SoundIndex ? CheckState.Checked : CheckState.Unchecked;

					form.toolStripMenuItemTool.Enabled = value != InputMode.SoundIndex;
					form.toolStripMenuItemSelect.Enabled = form.toolStripButtonSelect.Enabled = value != InputMode.SoundIndex;
					form.toolStripMenuItemMove.Enabled = form.toolStripButtonMove.Enabled = value != InputMode.SoundIndex;
					form.toolStripMenuItemDot.Enabled = form.toolStripButtonDot.Enabled = value != InputMode.SoundIndex;
					form.toolStripMenuItemLine.Enabled = form.toolStripButtonLine.Enabled = value != InputMode.SoundIndex;

					string mode = GetInterfaceString("motor_sound", "status", "mode", "name");

					switch (value)
					{
						case InputMode.Pitch:
							form.toolStripStatusLabelMode.Text = string.Format("{0} {1}", mode, GetInterfaceString("motor_sound", "status", "mode", "pitch"));
							break;
						case InputMode.Volume:
							form.toolStripStatusLabelMode.Text = string.Format("{0} {1}", mode, GetInterfaceString("motor_sound", "status", "mode", "volume"));
							break;
						case InputMode.SoundIndex:
							form.toolStripStatusLabelMode.Text = string.Format("{0} {1}({2})", mode, GetInterfaceString("motor_sound", "status", "mode", "sound_index"), form.toolStripComboBoxIndex.SelectedIndex - 1);
							break;
					}

					form.toolStripStatusLabelTool.Enabled = value != InputMode.SoundIndex;

					if (form.toolStripStatusLabelY.Enabled)
					{
						form.toolStripStatusLabelY.Enabled = value != InputMode.SoundIndex;
					}

					form.labelMinPitch.Enabled = value == InputMode.Pitch;
					form.textBoxMinPitch.Enabled = value == InputMode.Pitch;

					form.labelMaxPitch.Enabled = value == InputMode.Pitch;
					form.textBoxMaxPitch.Enabled = value == InputMode.Pitch;

					form.labelMinVolume.Enabled = value == InputMode.Volume;
					form.textBoxMinVolume.Enabled = value == InputMode.Volume;

					form.labelMaxVolume.Enabled = value == InputMode.Volume;
					form.textBoxMaxVolume.Enabled = value == InputMode.Volume;

					form.groupBoxDirect.Enabled = value != InputMode.SoundIndex;

					if (value != InputMode.Pitch)
					{
						foreach (Vertex vertex in form.tracks[(int)currentViewMode].PitchVertices.Values)
						{
							vertex.Selected = false;
							vertex.IsOrigin = false;
						}
					}

					if (value != InputMode.Volume)
					{
						foreach (Vertex vertex in form.tracks[(int)currentViewMode].VolumeVertices.Values)
						{
							vertex.Selected = false;
							vertex.IsOrigin = false;
						}
					}

					form.selectedRange = null;
					form.hoveredVertexPitch = null;
					form.hoveredVertexVolume = null;

					form.toolStripMenuItemPitch.Invalidate();
					form.toolStripMenuItemVolume.Invalidate();
					form.DrawPictureBoxDrawArea();
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
					currentToolMode = value;

					form.toolStripMenuItemSelect.CheckState = form.toolStripButtonSelect.CheckState = value == ToolMode.Select ? CheckState.Checked : CheckState.Unchecked;
					form.toolStripMenuItemMove.CheckState = form.toolStripButtonMove.CheckState = value == ToolMode.Move ? CheckState.Checked : CheckState.Unchecked;
					form.toolStripMenuItemDot.CheckState = form.toolStripButtonDot.CheckState = value == ToolMode.Dot ? CheckState.Checked : CheckState.Unchecked;
					form.toolStripMenuItemLine.CheckState = form.toolStripButtonLine.CheckState = value == ToolMode.Line ? CheckState.Checked : CheckState.Unchecked;

					form.buttonDirectDot.Enabled = value == ToolMode.Dot;
					form.buttonDirectMove.Enabled = value == ToolMode.Move;

					string tool = GetInterfaceString("motor_sound", "status", "tool", "name");

					switch (value)
					{
						case ToolMode.Select:
							form.toolStripStatusLabelTool.Text = string.Format("{0} {1}", tool, GetInterfaceString("motor_sound", "status", "tool", "select"));
							break;
						case ToolMode.Move:
							form.toolStripStatusLabelTool.Text = string.Format("{0} {1}", tool, GetInterfaceString("motor_sound", "status", "tool", "move"));
							break;
						case ToolMode.Dot:
							form.toolStripStatusLabelTool.Text = string.Format("{0} {1}", tool, GetInterfaceString("motor_sound", "status", "tool", "dot"));
							break;
						case ToolMode.Line:
							form.toolStripStatusLabelTool.Text = string.Format("{0} {1}", tool, GetInterfaceString("motor_sound", "status", "tool", "line"));
							break;
					}

					foreach (Vertex vertex in form.tracks[(int)currentViewMode].PitchVertices.Values)
					{
						if (value != ToolMode.Select && value != ToolMode.Move)
						{
							vertex.Selected = false;
						}

						if (value != ToolMode.Line)
						{
							vertex.IsOrigin = false;
						}
					}

					form.DrawPictureBoxDrawArea();
				}
			}

			internal SimulationState CurrentSimuState
			{
				get
				{
					return currentSimuState;
				}
				set
				{
					currentSimuState = value;

					form.groupBoxPlay.Enabled = value != SimulationState.Disable;
					form.groupBoxArea.Enabled = value == SimulationState.Stopped;
					form.buttonPlay.Enabled = value == SimulationState.Stopped || value == SimulationState.Paused;
					form.buttonPause.Enabled = value == SimulationState.Started;
					form.buttonStop.Enabled = value == SimulationState.Started || value == SimulationState.Paused;
				}
			}

			internal void ChangeEditStatus(bool status)
			{
				form.toolStripMenuItemEdit.Enabled = status;
				form.toolStripMenuItemUndo.Enabled = form.toolStripButtonUndo.Enabled = status && form.prevTrackStates.Any(s => s.Mode == currentViewMode);
				form.toolStripMenuItemRedo.Enabled = form.toolStripButtonRedo.Enabled = status && form.nextTrackStates.Any(s => s.Mode == currentViewMode);
				form.toolStripMenuItemTearingOff.Enabled = form.toolStripButtonTearingOff.Enabled = status;
				form.toolStripMenuItemCopy.Enabled = form.toolStripButtonCopy.Enabled = status;
				form.toolStripMenuItemPaste.Enabled = form.toolStripButtonPaste.Enabled = status && form.copyTrack != null;
				form.toolStripMenuItemCleanup.Enabled = form.toolStripButtonCleanup.Enabled = status;
				form.toolStripMenuItemDelete.Enabled = form.toolStripButtonDelete.Enabled = status;
			}

			internal void ChangeViewStatus(bool status)
			{
				form.toolStripMenuItemView.Enabled = status;
				form.toolStripMenuItemPower.Enabled = status;
				form.toolStripMenuItemPowerTrack1.Enabled = form.toolStripMenuItemPowerTrack2.Enabled = status;
				form.toolStripMenuItemBrake.Enabled = status;
				form.toolStripMenuItemBrakeTrack1.Enabled = form.toolStripMenuItemBrakeTrack2.Enabled = status;
			}

			internal void ChangeInputStatus(bool status)
			{
				form.toolStripMenuItemInput.Enabled = status;
				form.toolStripMenuItemPitch.Enabled = status;
				form.toolStripMenuItemVolume.Enabled = status;
				form.toolStripMenuItemIndex.Enabled = form.toolStripComboBoxIndex.Enabled = status;
			}

			internal void ChangeToolsStatus(bool status)
			{
				form.toolStripMenuItemTool.Enabled = status && currentInputMode != InputMode.SoundIndex;
				form.toolStripMenuItemSelect.Enabled = form.toolStripButtonSelect.Enabled = status && currentInputMode != InputMode.SoundIndex;
				form.toolStripMenuItemMove.Enabled = form.toolStripButtonMove.Enabled = status && currentInputMode != InputMode.SoundIndex;
				form.toolStripMenuItemDot.Enabled = form.toolStripButtonDot.Enabled = status && currentInputMode != InputMode.SoundIndex;
				form.toolStripMenuItemLine.Enabled = form.toolStripButtonLine.Enabled = status && currentInputMode != InputMode.SoundIndex;

				form.groupBoxDirect.Enabled = status && currentInputMode != InputMode.SoundIndex;

				form.toolStripStatusLabelTool.Enabled = status && currentInputMode != InputMode.SoundIndex;
			}
		}

		private class TrackState
		{
			internal readonly EditState.ViewMode Mode;
			internal readonly Track State;
			internal bool IsSaved;

			public TrackState(EditState.ViewMode mode, Track state)
			{
				Mode = mode;
				State = state;
				IsSaved = false;
			}
		}

		internal enum SoundSection
		{
			[StringValue("Run")]
			Run,

			[StringValue("Flange")]
			Flange,

			[StringValue("Motor")]
			Motor,

			[StringValue("Front Switch")]
			FrontSwitch,

			[StringValue("Rear Switch")]
			RearSwitch,

			[StringValue("Brake")]
			Brake,

			[StringValue("Compressor")]
			Compressor,

			[StringValue("	Suspension")]
			Suspension,

			[StringValue("Horn")]
			Horn,

			[StringValue("Door")]
			Door,

			[StringValue("Ats")]
			Ats,

			[StringValue("Buzzer")]
			Buzzer,

			[StringValue("Pilot Lamp")]
			PilotLamp,

			[StringValue("Brake Handle")]
			BrakeHandle,

			[StringValue("Master Controller")]
			MasterController,

			[StringValue("Reverser")]
			Reverser,

			[StringValue("Breaker")]
			Breaker,

			[StringValue("Request Stop")]
			RequestStop,

			[StringValue("Touch")]
			Touch,

			[StringValue("Others")]
			Others
		}

		internal enum BrakeKey
		{
			[StringValue("BcReleaseHigh")]
			BcReleaseHigh,

			[StringValue("BcRelease")]
			BcRelease,

			[StringValue("BcReleaseFull")]
			BcReleaseFull,

			[StringValue("Emergency")]
			Emergency,

			[StringValue("BP Decomp")]
			BpDecomp
		}

		internal enum CompressorKey
		{
			[StringValue("Attack")]
			Attack,

			[StringValue("Loop")]
			Loop,

			[StringValue("Release")]
			Release
		}

		internal enum SuspensionKey
		{
			[StringValue("Left")]
			Left,

			[StringValue("Right")]
			Right
		}

		internal enum HornKey
		{
			[StringValue("PrimaryStart")]
			PrimaryStart,

			[StringValue("PrimaryLoop")]
			PrimaryLoop,

			[StringValue("PrimaryEnd")]
			PrimaryEnd,

			[StringValue("SecondaryStart")]
			SecondaryStart,

			[StringValue("SecondaryLoop")]
			SecondaryLoop,

			[StringValue("SecondaryEnd")]
			SecondaryEnd,

			[StringValue("MusicStart")]
			MusicStart,

			[StringValue("MusicLoop")]
			MusicLoop,

			[StringValue("MusicEnd")]
			MusicEnd
		}

		internal enum DoorKey
		{
			[StringValue("Open Left")]
			OpenLeft,

			[StringValue("Close Left")]
			CloseLeft,

			[StringValue("Open Right")]
			OpenRight,

			[StringValue("Close Right")]
			CloseRight
		}

		internal enum BuzzerKey
		{
			[StringValue("Correct")]
			Correct
		}

		internal enum PilotLampKey
		{
			[StringValue("On")]
			On,

			[StringValue("Off")]
			Off
		}

		internal enum BrakeHandleKey
		{
			[StringValue("Apply")]
			Apply,

			[StringValue("ApplyFast")]
			ApplyFast,

			[StringValue("Release")]
			Release,

			[StringValue("ReleaseFast")]
			ReleaseFast,

			[StringValue("Min")]
			Min,

			[StringValue("Max")]
			Max
		}

		internal enum MasterControllerKey
		{
			[StringValue("Up")]
			Up,

			[StringValue("UpFast")]
			UpFast,

			[StringValue("Down")]
			Down,

			[StringValue("DownFast")]
			DownFast,

			[StringValue("Min")]
			Min,

			[StringValue("Max")]
			Max
		}

		internal enum ReverserKey
		{
			[StringValue("On")]
			On,

			[StringValue("Off")]
			Off
		}

		internal enum BreakerKey
		{
			[StringValue("On")]
			On,

			[StringValue("Off")]
			Off
		}

		internal enum RequestStopKey
		{
			[StringValue("Stop")]
			Stop,

			[StringValue("Pass")]
			Pass,

			[StringValue("Ignored")]
			Ignored
		}

		internal enum OthersKey
		{
			[StringValue("Noise")]
			Noise,

			[StringValue("Shoe")]
			Shoe,

			[StringValue("Halt")]
			Halt
		}
	}
}
