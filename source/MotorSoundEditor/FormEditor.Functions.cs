using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows.Forms;
using MotorSoundEditor.Parsers.Train;
using OpenBveApi.Interface;

namespace MotorSoundEditor
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

					string type = GetInterfaceString("status_type", "status_type");
					string power = GetInterfaceString("status_type", "power");
					string brake = GetInterfaceString("status_type", "brake");
					string track = GetInterfaceString("status_track", "status_track");

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

					string mode = GetInterfaceString("status_mode", "status_mode");

					switch (value)
					{
						case InputMode.Pitch:
							form.toolStripStatusLabelMode.Text = string.Format("{0} {1}", mode, GetInterfaceString("status_mode", "pitch"));
							break;
						case InputMode.Volume:
							form.toolStripStatusLabelMode.Text = string.Format("{0} {1}", mode, GetInterfaceString("status_mode", "volume"));
							break;
						case InputMode.SoundIndex:
							form.toolStripStatusLabelMode.Text = string.Format("{0} {1}({2})", mode, GetInterfaceString("status_mode", "sound_index"), form.toolStripComboBoxIndex.SelectedIndex - 1);
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

					string tool = GetInterfaceString("status_tool", "status_tool");

					switch (value)
					{
						case ToolMode.Select:
							form.toolStripStatusLabelTool.Text = string.Format("{0} {1}", tool, GetInterfaceString("status_tool", "select"));
							break;
						case ToolMode.Move:
							form.toolStripStatusLabelTool.Text = string.Format("{0} {1}", tool, GetInterfaceString("status_tool", "move"));
							break;
						case ToolMode.Dot:
							form.toolStripStatusLabelTool.Text = string.Format("{0} {1}", tool, GetInterfaceString("status_tool", "dot"));
							break;
						case ToolMode.Line:
							form.toolStripStatusLabelTool.Text = string.Format("{0} {1}", tool, GetInterfaceString("status_tool", "line"));
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

			internal void ChangeEditStatus(bool status)
			{
				form.toolStripMenuItemEdit.Enabled = status;
				form.toolStripMenuItemUndo.Enabled = form.toolStripButtonUndo.Enabled = status && form.prevTrackStates.Any(s => s.Mode == currentViewMode);
				form.toolStripMenuItemRedo.Enabled = form.toolStripButtonRedo.Enabled = status && form.nextTrackStates.Any(s => s.Mode == currentViewMode);
				form.toolStripMenuItemTearingOff.Enabled = form.toolStripButtonTearingOff.Enabled = status;
				form.toolStripMenuItemCopy.Enabled = form.toolStripButtonCopy.Enabled = status;
				form.toolStripMenuItemPaste.Enabled = form.toolStripButtonPaste.Enabled = status && form.copyTrack != null;
				form.toolStripMenuItemDelete.Enabled = form.toolStripButtonDelete.Enabled = status;
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

		private float XtoVelocity(float x)
		{
			int width = pictureBoxDrawArea.ClientRectangle.Width;
			float factorVelocity = width / (maxVelocity - minVelocity);
			return minVelocity + x / factorVelocity;
		}

		private float YtoPitch(float y)
		{
			int height = pictureBoxDrawArea.ClientRectangle.Height;
			float factorPitch = -height / (maxPitch - minPitch);
			return minPitch + (y - height) / factorPitch;
		}

		private float YtoVolume(float y)
		{
			int height = pictureBoxDrawArea.ClientRectangle.Height;
			float factorVolume = -height / (maxVolume - minVolume);
			return minVolume + (y - height) / factorVolume;
		}

		private float VelocityToX(float v)
		{
			int width = pictureBoxDrawArea.ClientRectangle.Width;
			float factorVelocity = width / (maxVelocity - minVelocity);
			return (v - minVelocity) * factorVelocity;
		}

		private float PitchToY(float p)
		{
			int height = pictureBoxDrawArea.ClientRectangle.Height;
			float factorPitch = -height / (maxPitch - minPitch);
			return height + (p - minPitch) * factorPitch;
		}

		private float VolumeToY(float v)
		{
			int height = pictureBoxDrawArea.ClientRectangle.Height;
			float factorVolume = -height / (maxVolume - minVolume);
			return height + (v - minVolume) * factorVolume;
		}

		private void DrawDot(MouseEventArgs e)
		{
			float velocity = 0.2f * (float)Math.Round(5.0 * XtoVelocity(e.X));
			float pitch = 0.01f * (float)Math.Round(100.0 * YtoPitch(e.Y));
			float volume = 0.01f * (float)Math.Round(100.0 * YtoVolume(e.Y));

			DrawDot(velocity, pitch, volume);
		}

		private void DrawDot(float velocity, float pitch, float volume)
		{
			switch (editState.CurrentInputMode)
			{
				case EditState.InputMode.Pitch:
					prevTrackStates.Add(new TrackState(editState.CurrentViewMode, tracks[(int)editState.CurrentViewMode].Clone()));
					nextTrackStates.RemoveAll(s => s.Mode == editState.CurrentViewMode);
					tracks[(int)editState.CurrentViewMode].PitchVertices.Add(new Vertex(velocity, pitch));
					break;
				case EditState.InputMode.Volume:
					prevTrackStates.Add(new TrackState(editState.CurrentViewMode, tracks[(int)editState.CurrentViewMode].Clone()));
					nextTrackStates.RemoveAll(s => s.Mode == editState.CurrentViewMode);
					tracks[(int)editState.CurrentViewMode].VolumeVertices.Add(new Vertex(velocity, volume));
					break;
			}

			DrawPictureBoxDrawArea();
		}

		private bool IsSelectDotLine(VertexLibrary vertices, List<Line> lines, float x, float y)
		{
			if (vertices.Any(v => v.Value.X - 0.2f < x && x < v.Value.X + 0.2f && v.Value.Y - 2.0f < y && y < v.Value.Y + 2.0f))
			{
				return true;
			}

			if (lines.Any(l => vertices[l.LeftID].X + 0.2f < x && x < vertices[l.RightID].X - 0.2f && Math.Min(vertices[l.LeftID].Y, vertices[l.RightID].Y) - 2.0 < y && y < Math.Max(vertices[l.LeftID].Y, vertices[l.RightID].Y) + 2.0))
			{
				return true;
			}

			return false;
		}

		private void SelectDotLine(MouseEventArgs e)
		{
			float velocity = XtoVelocity(e.X);
			float pitch = YtoPitch(e.Y);
			float volume = YtoVolume(e.Y);

			switch (editState.CurrentInputMode)
			{
				case EditState.InputMode.Pitch:
					SelectDotLine(tracks[(int)editState.CurrentViewMode].PitchVertices, tracks[(int)editState.CurrentViewMode].PitchLines, velocity, pitch);
					break;
				case EditState.InputMode.Volume:
					SelectDotLine(tracks[(int)editState.CurrentViewMode].VolumeVertices, tracks[(int)editState.CurrentViewMode].VolumeLines, velocity, volume);
					break;
			}

			DrawPictureBoxDrawArea();
		}

		private void SelectDotLine(VertexLibrary vertices, List<Line> lines, float x, float y)
		{
			Func<Vertex, bool> conditionVertex = v => v.X - 0.2f < x && x < v.X + 0.2f && v.Y - 2.0f < y && y < v.Y + 2.0f;

			if (vertices.Any(v => conditionVertex(v.Value)))
			{
				KeyValuePair<int, Vertex> selectVertex = vertices.First(v => conditionVertex(v.Value));

				if ((ModifierKeys & Keys.Control) != Keys.Control)
				{
					foreach (Vertex vertex in vertices.Values.Where(v => v != selectVertex.Value))
					{
						vertex.Selected = false;
					}
				}

				selectVertex.Value.Selected = !selectVertex.Value.Selected;
			}
			else
			{
				if ((ModifierKeys & Keys.Control) != Keys.Control)
				{
					foreach (Vertex vertex in vertices.Values)
					{
						vertex.Selected = false;
					}
				}
			}

			Line selectLine = lines.FirstOrDefault(l => vertices[l.LeftID].X + 0.2f < x && x < vertices[l.RightID].X - 0.2f && Math.Min(vertices[l.LeftID].Y, vertices[l.RightID].Y) - 2.0 < y && y < Math.Max(vertices[l.LeftID].Y, vertices[l.RightID].Y) + 2.0);

			if (selectLine != null)
			{
				if ((ModifierKeys & Keys.Control) != Keys.Control)
				{
					foreach (Line line in lines.Where(l => l != selectLine))
					{
						line.Selected = false;
					}
				}

				selectLine.Selected = !selectLine.Selected;
			}
			else
			{
				foreach (Line line in lines)
				{
					line.Selected = false;
				}
			}
		}

		private bool IsDrawLine(VertexLibrary vertices, List<Line> lines, float x, float y)
		{
			Func<Vertex, bool> conditionVertex = v => v.X - 0.2f < x && x < v.X + 0.2f && v.Y - 2.0f < y && y < v.Y + 2.0f;

			if (vertices.Any(v => conditionVertex(v.Value)))
			{
				KeyValuePair<int, Vertex> selectVertex = vertices.First(v => conditionVertex(v.Value));

				if (selectVertex.Value.IsOrigin)
				{
					return true;
				}

				if (vertices.Any(v => v.Value.IsOrigin))
				{
					KeyValuePair<int, Vertex> origin = vertices.First(v => v.Value.IsOrigin);
					KeyValuePair<int, Vertex>[] selectVertices = new KeyValuePair<int, Vertex>[] { origin, selectVertex }.OrderBy(v => v.Value.X).ToArray();

					Func<Line, bool> conditionLineLeft = l => vertices[l.LeftID].X <= selectVertices[0].Value.X && selectVertices[0].Value.X < vertices[l.RightID].X;
					Func<Line, bool> conditionLineRight = l => vertices[l.LeftID].X < selectVertices[1].Value.X && selectVertices[1].Value.X <= vertices[l.RightID].X;

					if (!lines.Any(l => conditionLineLeft(l)) && !lines.Any(l => conditionLineRight(l)))
					{
						return true;
					}

					return false;
				}

				return true;
			}

			return false;
		}

		private void DrawLine(MouseEventArgs e)
		{
			float velocity = XtoVelocity(e.X);
			float pitch = YtoPitch(e.Y);
			float volume = YtoVolume(e.Y);

			switch (editState.CurrentInputMode)
			{
				case EditState.InputMode.Pitch:
					DrawLine(tracks[(int)editState.CurrentViewMode].PitchVertices, tracks[(int)editState.CurrentViewMode].PitchLines, velocity, pitch);
					break;
				case EditState.InputMode.Volume:
					DrawLine(tracks[(int)editState.CurrentViewMode].VolumeVertices, tracks[(int)editState.CurrentViewMode].VolumeLines, velocity, volume);
					break;
			}

			DrawPictureBoxDrawArea();
		}

		private void DrawLine(VertexLibrary vertices, List<Line> lines, float x, float y)
		{
			Func<Vertex, bool> conditionVertex = v => v.X - 0.2f < x && x < v.X + 0.2f && v.Y - 2.0f < y && y < v.Y + 2.0f;

			if (vertices.Any(v => conditionVertex(v.Value)))
			{
				KeyValuePair<int, Vertex> selectVertex = vertices.First(v => conditionVertex(v.Value));

				if (selectVertex.Value.IsOrigin)
				{
					selectVertex.Value.IsOrigin = false;
				}
				else if (vertices.Any(v => v.Value.IsOrigin))
				{
					KeyValuePair<int, Vertex> origin = vertices.First(v => v.Value.IsOrigin);
					KeyValuePair<int, Vertex>[] selectVertices = new KeyValuePair<int, Vertex>[] { origin, selectVertex }.OrderBy(v => v.Value.X).ToArray();

					Func<Line, bool> conditionLineLeft = l => vertices[l.LeftID].X <= selectVertices[0].Value.X && selectVertices[0].Value.X < vertices[l.RightID].X;
					Func<Line, bool> conditionLineRight = l => vertices[l.LeftID].X < selectVertices[1].Value.X && selectVertices[1].Value.X <= vertices[l.RightID].X;

					if (!lines.Any(l => conditionLineLeft(l)) && !lines.Any(l => conditionLineRight(l)))
					{
						prevTrackStates.Add(new TrackState(editState.CurrentViewMode, tracks[(int)editState.CurrentViewMode].Clone()));
						nextTrackStates.RemoveAll(s => s.Mode == editState.CurrentViewMode);
						lines.Add(new Line(selectVertices[0].Key, selectVertices[1].Key));

						origin.Value.IsOrigin = false;
						selectVertex.Value.IsOrigin = true;
					}
				}
				else
				{
					selectVertex.Value.IsOrigin = true;
				}
			}
		}

		private void MoveDot(VertexLibrary vertices, float deltaX, float deltaY)
		{
			if (vertices.Values.Any(v => v.Selected))
			{
				if (!isMoving)
				{
					prevTrackStates.Add(new TrackState(editState.CurrentViewMode, tracks[(int)editState.CurrentViewMode].Clone()));
					nextTrackStates.RemoveAll(s => s.Mode == editState.CurrentViewMode);
					isMoving = true;
				}

				foreach (Vertex select in vertices.Values.Where(v => v.Selected).OrderBy(v => v.X))
				{
					if (deltaX >= 0.0f)
					{
						Vertex unselectLeft = vertices.Values.OrderBy(v => v.X).FirstOrDefault(v => v.X > select.X);

						if (unselectLeft != null)
						{
							if (select.X + deltaX + 0.2f >= unselectLeft.X)
							{
								deltaX = 0.0f;
							}
						}
					}
					else
					{
						Vertex unselectRight = vertices.Values.OrderBy(v => v.X).LastOrDefault(v => v.X < select.X);

						if (unselectRight != null)
						{
							if (select.X + deltaX - 0.2f <= unselectRight.X)
							{
								deltaX = 0.0f;
							}
						}

						if (select.X + deltaX < 0.0f)
						{
							deltaX = 0.0f;
						}
					}

					if (deltaY < 0.0f)
					{
						if (select.Y + deltaY < 0.0f)
						{
							deltaY = 0.0f;
						}
					}
				}

				foreach (Vertex vertex in vertices.Values.Where(v => v.Selected))
				{
					vertex.X += deltaX;
					vertex.Y += deltaY;
				}
			}
		}

		private void DeleteDotLine(VertexLibrary vertices, List<Line> lines)
		{
			if (vertices.Any(v => v.Value.Selected) || lines.Any(l => l.Selected))
			{
				KeyValuePair<int, Vertex>[] selectVertices = vertices.Where(v => v.Value.Selected).ToArray();

				foreach (KeyValuePair<int, Vertex> vertex in selectVertices)
				{
					lines.RemoveAll(l => l.LeftID == vertex.Key || l.RightID == vertex.Key);
					vertices.Remove(vertex.Key);
				}

				lines.RemoveAll(l => l.Selected);
			}
			else
			{
				vertices.Clear();
				lines.Clear();
			}
		}

		private void PrevTrackStatesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			toolStripMenuItemUndo.Enabled = toolStripButtonUndo.Enabled = prevTrackStates.Any(s => s.Mode == editState.CurrentViewMode);

			if (string.IsNullOrEmpty(fileName))
			{
				if (prevTrackStates.Any() && !prevTrackStates.Last().IsSaved)
				{
					Text = string.Format("NewFile * - {0}", Application.ProductName);
				}
				else
				{
					Text = string.Format("NewFile - {0}", Application.ProductName);
				}
			}
			else
			{
				if (prevTrackStates.Any() && !prevTrackStates.Last().IsSaved)
				{
					Text = string.Format("{0} * - {1}", Path.GetFileName(Path.GetDirectoryName(fileName)), Application.ProductName);
				}
				else
				{
					Text = string.Format("{0} - {1}", Path.GetFileName(Path.GetDirectoryName(fileName)), Application.ProductName);
				}
			}
		}

		private void NextTrackStatesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			toolStripMenuItemRedo.Enabled = toolStripButtonRedo.Enabled = nextTrackStates.Any(s => s.Mode == editState.CurrentViewMode);
		}

		private void Undo()
		{
			if (prevTrackStates.Any(s => s.Mode == editState.CurrentViewMode))
			{
				TrackState prev = prevTrackStates.Last(s => s.Mode == editState.CurrentViewMode);
				nextTrackStates.Add(new TrackState(editState.CurrentViewMode, tracks[(int)editState.CurrentViewMode]));
				tracks[(int)editState.CurrentViewMode] = prev.State.Clone();
				prevTrackStates.Remove(prev);
			}

			DrawPictureBoxDrawArea();
		}

		private void Redo()
		{
			if (nextTrackStates.Any(s => s.Mode == editState.CurrentViewMode))
			{
				TrackState next = nextTrackStates.Last(s => s.Mode == editState.CurrentViewMode);
				prevTrackStates.Add(new TrackState(editState.CurrentViewMode, tracks[(int)editState.CurrentViewMode]));
				tracks[(int)editState.CurrentViewMode] = next.State.Clone();
				nextTrackStates.Remove(next);
			}

			DrawPictureBoxDrawArea();
		}

		private void TearingOff()
		{
			prevTrackStates.Add(new TrackState(editState.CurrentViewMode, tracks[(int)editState.CurrentViewMode].Clone()));
			nextTrackStates.RemoveAll(s => s.Mode == editState.CurrentViewMode);
			copyTrack = tracks[(int)editState.CurrentViewMode].Clone();
			tracks[(int)editState.CurrentViewMode] = new Track();

			DrawPictureBoxDrawArea();
		}

		private void Copy()
		{
			copyTrack = tracks[(int)editState.CurrentViewMode].Clone();
			toolStripMenuItemPaste.Enabled = true;
			toolStripButtonPaste.Enabled = true;
		}

		private void Paste()
		{
			if (copyTrack != null)
			{
				prevTrackStates.Add(new TrackState(editState.CurrentViewMode, tracks[(int)editState.CurrentViewMode].Clone()));
				nextTrackStates.RemoveAll(s => s.Mode == editState.CurrentViewMode);
				tracks[(int)editState.CurrentViewMode] = copyTrack.Clone();
			}

			DrawPictureBoxDrawArea();
		}

		private void Delete()
		{
			prevTrackStates.Add(new TrackState(editState.CurrentViewMode, tracks[(int)editState.CurrentViewMode].Clone()));
			nextTrackStates.RemoveAll(s => s.Mode == editState.CurrentViewMode);

			switch (editState.CurrentInputMode)
			{
				case EditState.InputMode.Pitch:
					DeleteDotLine(tracks[(int)editState.CurrentViewMode].PitchVertices, tracks[(int)editState.CurrentViewMode].PitchLines);
					break;
				case EditState.InputMode.Volume:
					DeleteDotLine(tracks[(int)editState.CurrentViewMode].VolumeVertices, tracks[(int)editState.CurrentViewMode].VolumeLines);
					break;
				case EditState.InputMode.SoundIndex:
					tracks[(int)editState.CurrentViewMode].SoundIndices.Clear();
					break;
			}

			DrawPictureBoxDrawArea();
		}

		private void Cleanup()
		{
			Func<int, List<Line>, bool> condition = (i, ls) => ls.Any(l => l.LeftID == i || l.RightID == i);

			int[] pitchTargetIDs = new int[0];
			int[] volumeTargetIDs = new int[0];

			if (editState.CurrentInputMode != EditState.InputMode.Volume)
			{
				pitchTargetIDs = tracks[(int)editState.CurrentViewMode].PitchVertices.Keys.Where(i => !condition(i, tracks[(int)editState.CurrentViewMode].PitchLines)).ToArray();
			}

			if (editState.CurrentInputMode != EditState.InputMode.Pitch)
			{
				volumeTargetIDs = tracks[(int)editState.CurrentViewMode].VolumeVertices.Keys.Where(i => !condition(i, tracks[(int)editState.CurrentViewMode].VolumeLines)).ToArray();
			}

			if (!pitchTargetIDs.Any() && !volumeTargetIDs.Any())
			{
				return;
			}

			prevTrackStates.Add(new TrackState(editState.CurrentViewMode, tracks[(int)editState.CurrentViewMode].Clone()));
			nextTrackStates.RemoveAll(s => s.Mode == editState.CurrentViewMode);

			foreach (int targetID in pitchTargetIDs)
			{
				tracks[(int)editState.CurrentViewMode].PitchVertices.Remove(targetID);
			}

			foreach (int targetID in volumeTargetIDs)
			{
				tracks[(int)editState.CurrentViewMode].VolumeVertices.Remove(targetID);
			}

			DrawPictureBoxDrawArea();
		}

		private Track MotorToTrack(TrainDat.Motor motor)
		{
			Track track = new Track();

			for (int i = 0; i < motor.Entries.Length; i++)
			{
				float velocity = 0.2f * i;

				if (track.PitchVertices.Count >= 2)
				{
					KeyValuePair<int, Vertex>[] leftVertices = new KeyValuePair<int, Vertex>[] { track.PitchVertices.ElementAt(track.PitchVertices.Count - 2), track.PitchVertices.Last() };
					Func<float, float> f = x => leftVertices[0].Value.Y + (leftVertices[1].Value.Y - leftVertices[0].Value.Y) / (leftVertices[1].Value.X - leftVertices[0].Value.X) * (x - leftVertices[0].Value.X);

					if (f(velocity) == (float)motor.Entries[i].Pitch)
					{
						track.PitchVertices.Remove(leftVertices[1].Key);
					}
				}

				track.PitchVertices.Add(new Vertex(velocity, 0.01f * (float)Math.Round(100.0 * motor.Entries[i].Pitch)));

				if (track.VolumeVertices.Count >= 2)
				{
					KeyValuePair<int, Vertex>[] leftVertices = new KeyValuePair<int, Vertex>[] { track.VolumeVertices.ElementAt(track.VolumeVertices.Count - 2), track.VolumeVertices.Last() };
					Func<float, float> f = x => leftVertices[0].Value.Y + (leftVertices[1].Value.Y - leftVertices[0].Value.Y) / (leftVertices[1].Value.X - leftVertices[0].Value.X) * (x - leftVertices[0].Value.X);

					if (f(velocity) == (float)motor.Entries[i].Volume)
					{
						track.VolumeVertices.Remove(leftVertices[1].Key);
					}
				}

				track.VolumeVertices.Add(new Vertex(velocity, 0.01f * (float)Math.Round(100.0 * motor.Entries[i].Volume)));

				if (track.SoundIndices.Any())
				{
					Area leftArea = track.SoundIndices.Last();

					if (motor.Entries[i].SoundIndex != leftArea.Index)
					{
						leftArea.RightX = velocity - 0.2f;
						track.SoundIndices.Add(new Area(velocity, velocity, motor.Entries[i].SoundIndex));
					}
					else
					{
						leftArea.RightX = velocity;
					}
				}
				else
				{
					track.SoundIndices.Add(new Area(velocity, velocity, motor.Entries[i].SoundIndex));
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
					lastArea.RightX += 0.2f;
				}
			}

			return track;
		}

		private TrainDat.Motor TrackToMotor(Track track)
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

			TrainDat.Motor motor = new TrainDat.Motor
			{
				Entries = Enumerable.Repeat(new TrainDat.Motor.Entry { SoundIndex = -1, Pitch = 100.0, Volume = 128.0 }, n + 1).ToArray()
			};

			for (int i = 0; i < motor.Entries.Length; i++)
			{
				float velocity = 0.2f * i;

				int pitchLineIndex = track.PitchLines.FindIndex(l => track.PitchVertices[l.LeftID].X <= velocity && track.PitchVertices[l.RightID].X >= velocity);

				if (pitchLineIndex >= 0)
				{
					Vertex left = track.PitchVertices[track.PitchLines[pitchLineIndex].LeftID];
					Vertex right = track.PitchVertices[track.PitchLines[pitchLineIndex].RightID];

					Func<float, float> f = x => left.Y + (right.Y - left.Y) / (right.X - left.X) * (x - left.X);

					motor.Entries[i].Pitch = 0.01 * Math.Round(100.0 * Math.Max(f(velocity), 0.0));
				}

				int volumeLineIndex = track.VolumeLines.FindIndex(l => track.VolumeVertices[l.LeftID].X <= velocity && track.VolumeVertices[l.RightID].X >= velocity);

				if (volumeLineIndex >= 0)
				{
					Vertex left = track.VolumeVertices[track.VolumeLines[volumeLineIndex].LeftID];
					Vertex right = track.VolumeVertices[track.VolumeLines[volumeLineIndex].RightID];

					Func<float, float> f = x => left.Y + (right.Y - left.Y) / (right.X - left.X) * (x - left.X);

					motor.Entries[i].Volume = 0.01 * Math.Round(100.0 * Math.Max(f(velocity), 0.0));
				}

				int areaIndex = track.SoundIndices.FindIndex(a => a.LeftX <= velocity && a.RightX >= velocity);

				if (areaIndex >= 0)
				{
					motor.Entries[i].SoundIndex = track.SoundIndices[areaIndex].Index;
				}
			}

			return motor;
		}

		private void LoadData()
		{
			tracks[(int)EditState.ViewMode.Power1] = MotorToTrack(train.MotorP1);
			tracks[(int)EditState.ViewMode.Power2] = MotorToTrack(train.MotorP2);
			tracks[(int)EditState.ViewMode.Brake1] = MotorToTrack(train.MotorB1);
			tracks[(int)EditState.ViewMode.Brake2] = MotorToTrack(train.MotorB2);
		}

		private void SaveData()
		{
			if (train == null)
			{
				train = new TrainDat.Train();
			}

			train.MotorP1 = TrackToMotor(tracks[(int)EditState.ViewMode.Power1]);
			train.MotorP2 = TrackToMotor(tracks[(int)EditState.ViewMode.Power2]);
			train.MotorB1 = TrackToMotor(tracks[(int)EditState.ViewMode.Brake1]);
			train.MotorB2 = TrackToMotor(tracks[(int)EditState.ViewMode.Brake2]);
		}

		private void NewFile()
		{
			switch (MessageBox.Show(GetInterfaceString("message", "new"), GetInterfaceString("menu_file", "new"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
			{
				case DialogResult.Cancel:
					return;
				case DialogResult.Yes:
					SaveFile();
					break;
			}

			fileName = null;
			train = null;

			for (int i = 0; i < tracks.Length; i++)
			{
				tracks[i] = new Track();
			}

			toolStripMenuItemSave.Enabled = false;

			editState.CurrentSimuState = EditState.SimulationState.Disable;

			prevTrackStates.Clear();
			nextTrackStates.Clear();

			DrawPictureBoxDrawArea();
		}

		private void OpenFile()
		{
			switch (MessageBox.Show(GetInterfaceString("message", "open"), GetInterfaceString("menu_file", "open"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
			{
				case DialogResult.Cancel:
					return;
				case DialogResult.Yes:
					SaveFile();
					break;
			}

			using (OpenFileDialog dialog = new OpenFileDialog())
			{
				dialog.Filter = @"train.dat files|train.dat|All files|*";
				dialog.CheckFileExists = true;

				if (dialog.ShowDialog() != DialogResult.OK)
				{
					return;
				}

				try
				{
					fileName = dialog.FileName;
					train = TrainDat.Load(fileName);

					LoadData();

					toolStripMenuItemSave.Enabled = true;

					editState.CurrentSimuState = EditState.SimulationState.Stopped;
					buttonPause.Enabled = false;
					buttonStop.Enabled = false;
				}
				catch (Exception exception)
				{
					MessageBox.Show(exception.Message, GetInterfaceString("menu_file", "open"), MessageBoxButtons.OK, MessageBoxIcon.Hand);

					fileName = null;
					train = null;

					for (int i = 0; i < tracks.Length; i++)
					{
						tracks[i] = new Track();
					}

					toolStripMenuItemSave.Enabled = false;

					editState.CurrentSimuState = EditState.SimulationState.Disable;
				}
			}

			prevTrackStates.Clear();
			nextTrackStates.Clear();

			DrawPictureBoxDrawArea();
		}

		private void SaveFile()
		{
			if (fileName == null)
			{
				SaveAsFile();
				return;
			}

			try
			{
				SaveData();

				TrainDat.Save(fileName, train);

				foreach (TrackState state in prevTrackStates)
				{
					state.IsSaved = false;
				}

				foreach (TrackState state in nextTrackStates)
				{
					state.IsSaved = false;
				}

				if (prevTrackStates.Any())
				{
					prevTrackStates.Last().IsSaved = true;
				}

				Text = string.Format("{0} - {1}", Path.GetFileName(Path.GetDirectoryName(fileName)), Application.ProductName);

				editState.CurrentSimuState = EditState.SimulationState.Stopped;

				SystemSounds.Asterisk.Play();
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message, GetInterfaceString("menu_file", "save"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}

		private void SaveAsFile()
		{
			using (SaveFileDialog dialog = new SaveFileDialog())
			{
				dialog.Filter = @"train.dat files|train.dat|All files|*";
				dialog.OverwritePrompt = true;

				if (dialog.ShowDialog() != DialogResult.OK)
				{
					return;
				}

				try
				{
					SaveData();

					fileName = dialog.FileName;
					TrainDat.Save(fileName, train);

					foreach (TrackState state in prevTrackStates)
					{
						state.IsSaved = false;
					}

					foreach (TrackState state in nextTrackStates)
					{
						state.IsSaved = false;
					}

					if (prevTrackStates.Any())
					{
						prevTrackStates.Last().IsSaved = true;
					}

					Text = string.Format("{0} - {1}", Path.GetFileName(Path.GetDirectoryName(fileName)), Application.ProductName);

					toolStripMenuItemSave.Enabled = true;

					editState.CurrentSimuState = EditState.SimulationState.Stopped;
				}
				catch (Exception exception)
				{
					MessageBox.Show(exception.Message, GetInterfaceString("menu_file", "save_as"), MessageBoxButtons.OK, MessageBoxIcon.Hand);

					fileName = null;
					train = null;

					Text = Application.ProductName;

					toolStripMenuItemSave.Enabled = false;

					editState.CurrentSimuState = EditState.SimulationState.Disable;
				}
			}
		}

		private Color GetColor(double Hue, bool Selected)
		{
			double r, g, b;

			if (Hue < 0.0)
			{
				r = 0.0; g = 0.0; b = 0.0;
			}
			else if (Hue <= 0.166666666666667)
			{
				double x = 6.0 * Hue;
				r = 1.0; g = x; b = 0.0;
			}
			else if (Hue <= 0.333333333333333)
			{
				double x = 6.0 * Hue - 1.0;
				r = 1.0 - x; g = 1.0; b = 0.0;
			}
			else if (Hue <= 0.5)
			{
				double x = 6.0 * Hue - 2.0;
				r = 0.0; g = 1.0; b = x;
			}
			else if (Hue <= 0.666666666666667)
			{
				double x = 6.0 * Hue - 3.0;
				r = 0.0; g = 1.0 - x; b = 1.0;
			}
			else if (Hue <= 0.833333333333333)
			{
				double x = 6.0 * Hue - 4.0;
				r = x; g = 0.0; b = 1.0;
			}
			else if (Hue <= 1.0)
			{
				double x = 6.0 * Hue - 5.0;
				r = 1.0; g = 0.0; b = 1.0 - x;
			}
			else
			{
				r = 1.0; g = 1.0; b = 1.0;
			}

			if (r < 0.0)
			{
				r = 0.0;
			}
			else if (r > 1.0)
			{
				r = 1.0;
			}

			if (g < 0.0)
			{
				g = 0.0;
			}
			else if (g > 1.0)
			{
				g = 1.0;
			}

			if (b < 0.0)
			{
				b = 0.0;
			}
			else if (b > 1.0)
			{
				b = 1.0;
			}

			if (!Selected)
			{
				r *= 0.6;
				g *= 0.6;
				b *= 0.6;
			}

			return Color.FromArgb((int)Math.Round(255.0 * r), (int)Math.Round(255.0 * g), (int)Math.Round(255.0 * b));
		}

		private Bitmap GetImage(string path)
		{
			string folder = Program.FileSystem.GetDataFolder("MotorSoundEditor");
			Bitmap image = new Bitmap(OpenBveApi.Path.CombineFile(folder, path));
			image.MakeTransparent();
			return image;
		}

		private static string GetInterfaceString(string section, string key)
		{
			return Translations.GetInterfaceString(string.Format("motor_sound_editor_{0}_{1}", section, key));
		}

		private void ApplyLanguage()
		{
			toolStripMenuItemFile.Text = string.Format("{0}(&F)", GetInterfaceString("menu_file", "file"));
			toolStripMenuItemNew.Text = string.Format("{0}(&N)", GetInterfaceString("menu_file", "new"));
			toolStripMenuItemOpen.Text = string.Format("{0}(&O)", GetInterfaceString("menu_file", "open"));
			toolStripMenuItemSave.Text = string.Format("{0}(&S)", GetInterfaceString("menu_file", "save"));
			toolStripMenuItemSaveAs.Text = string.Format("{0}(&A)", GetInterfaceString("menu_file", "save_as"));
			toolStripMenuItemExit.Text = string.Format("{0}(&X)", GetInterfaceString("menu_file", "exit"));

			toolStripButtonNew.Text = string.Format("{0} (Ctrl+N)", GetInterfaceString("menu_file", "new"));
			toolStripButtonOpen.Text = string.Format("{0} (Ctrl+O)", GetInterfaceString("menu_file", "open"));
			toolStripButtonSave.Text = string.Format("{0} (Ctrl+S)", GetInterfaceString("menu_file", "save"));

			toolStripMenuItemEdit.Text = string.Format("{0}(&E)", GetInterfaceString("menu_edit", "edit"));
			toolStripMenuItemUndo.Text = string.Format("{0}(&U)", GetInterfaceString("menu_edit", "undo"));
			toolStripMenuItemRedo.Text = string.Format("{0}(&R)", GetInterfaceString("menu_edit", "redo"));
			toolStripMenuItemTearingOff.Text = string.Format("{0}(&T)", GetInterfaceString("menu_edit", "cut"));
			toolStripMenuItemCopy.Text = string.Format("{0}(&C)", GetInterfaceString("menu_edit", "copy"));
			toolStripMenuItemPaste.Text = string.Format("{0}(&P)", GetInterfaceString("menu_edit", "paste"));
			toolStripMenuItemCleanup.Text = string.Format("{0}", GetInterfaceString("menu_edit", "cleanup"));
			toolStripMenuItemDelete.Text = string.Format("{0}(&D)", GetInterfaceString("menu_edit", "delete"));

			toolStripButtonUndo.Text = string.Format("{0} (Ctrl+Z)", GetInterfaceString("menu_edit", "undo"));
			toolStripButtonRedo.Text = string.Format("{0} (Ctrl+Y)", GetInterfaceString("menu_edit", "redo"));
			toolStripButtonTearingOff.Text = string.Format("{0} (Ctrl+X)", GetInterfaceString("menu_edit", "cut"));
			toolStripButtonCopy.Text = string.Format("{0} (Ctrl+C)", GetInterfaceString("menu_edit", "copy"));
			toolStripButtonPaste.Text = string.Format("{0} (Ctrl+V)", GetInterfaceString("menu_edit", "paste"));
			toolStripButtonCleanup.Text = string.Format("{0}", GetInterfaceString("menu_edit", "cleanup"));
			toolStripButtonDelete.Text = string.Format("{0} (Del)", GetInterfaceString("menu_edit", "delete"));

			toolStripMenuItemView.Text = string.Format("{0}(&V)", GetInterfaceString("menu_view", "view"));
			toolStripMenuItemPower.Text = string.Format("{0}(&P)", GetInterfaceString("menu_view", "power"));
			toolStripMenuItemBrake.Text = string.Format("{0}(&B)", GetInterfaceString("menu_view", "brake"));
			toolStripMenuItemPowerTrack1.Text = toolStripMenuItemBrakeTrack1.Text = string.Format("{0}1(&1)", GetInterfaceString("menu_view", "track"));
			toolStripMenuItemPowerTrack2.Text = toolStripMenuItemBrakeTrack2.Text = string.Format("{0}2(&2)", GetInterfaceString("menu_view", "track"));

			toolStripMenuItemInput.Text = string.Format("{0}(&I)", GetInterfaceString("menu_input", "input"));
			toolStripMenuItemPitch.Text = string.Format("{0}(&P)", GetInterfaceString("menu_input", "pitch"));
			toolStripMenuItemVolume.Text = string.Format("{0}(&V)", GetInterfaceString("menu_input", "volume"));
			toolStripMenuItemIndex.Text = string.Format("{0}(&I)", GetInterfaceString("menu_input", "sound_index"));
			toolStripComboBoxIndex.Items[0] = string.Format("{0}", GetInterfaceString("menu_input", "sound_index_none"));

			toolStripMenuItemTool.Text = string.Format("{0}(&T)", GetInterfaceString("menu_tool", "tool"));
			toolStripMenuItemSelect.Text = string.Format("{0}(&S)", GetInterfaceString("menu_tool", "select"));
			toolStripMenuItemMove.Text = string.Format("{0}(&M)", GetInterfaceString("menu_tool", "move"));
			toolStripMenuItemDot.Text = string.Format("{0}(&D)", GetInterfaceString("menu_tool", "dot"));
			toolStripMenuItemLine.Text = string.Format("{0}(&L)", GetInterfaceString("menu_tool", "line"));

			toolStripButtonSelect.Text = string.Format("{0}", GetInterfaceString("menu_tool", "select"));
			toolStripButtonMove.Text = string.Format("{0}", GetInterfaceString("menu_tool", "move"));
			toolStripButtonDot.Text = string.Format("{0}", GetInterfaceString("menu_tool", "dot"));
			toolStripButtonLine.Text = string.Format("{0}", GetInterfaceString("menu_tool", "line"));

			groupBoxView.Text = string.Format("{0}", GetInterfaceString("view_setting", "view_setting"));
			labelMinVelocity.Text = string.Format("{0}", GetInterfaceString("view_setting", "min_velocity"));
			labelMaxVelocity.Text = string.Format("{0}", GetInterfaceString("view_setting", "max_velocity"));
			labelMinPitch.Text = string.Format("{0}", GetInterfaceString("view_setting", "min_pitch"));
			labelMaxPitch.Text = string.Format("{0}", GetInterfaceString("view_setting", "max_pitch"));
			labelMinVolume.Text = string.Format("{0}", GetInterfaceString("view_setting", "min_volume"));
			labelMaxVolume.Text = string.Format("{0}", GetInterfaceString("view_setting", "max_volume"));
			toolTipName.SetToolTip(buttonZoomIn, GetInterfaceString("view_setting", "zoom_in"));
			toolTipName.SetToolTip(buttonZoomOut, GetInterfaceString("view_setting", "zoom_out"));
			toolTipName.SetToolTip(buttonReset, GetInterfaceString("view_setting", "reset"));

			groupBoxDirect.Text = string.Format("{0}", GetInterfaceString("direct_input", "direct_input"));
			labelDirectX.Text = string.Format("{0}", GetInterfaceString("direct_input", "x_coordinate"));
			labelDirectY.Text = string.Format("{0}", GetInterfaceString("direct_input", "y_coordinate"));
			toolTipName.SetToolTip(buttonDirectDot, GetInterfaceString("direct_input", "dot"));
			toolTipName.SetToolTip(buttonDirectMove, GetInterfaceString("direct_input", "move"));

			groupBoxPlay.Text = string.Format("{0}", GetInterfaceString("play_setting", "play_setting"));

			groupBoxSource.Text = string.Format("{0}", GetInterfaceString("play_setting", "source"));
			labelRun.Text = string.Format("{0}", GetInterfaceString("play_setting", "source_run"));
			checkBoxTrack1.Text = string.Format("{0}1", GetInterfaceString("play_setting", "source_track"));
			checkBoxTrack2.Text = string.Format("{0}2", GetInterfaceString("play_setting", "source_track"));

			groupBoxArea.Text = string.Format("{0}", GetInterfaceString("play_setting", "area"));
			checkBoxLoop.Text = string.Format("{0}", GetInterfaceString("play_setting", "area_loop"));
			checkBoxConstant.Text = string.Format("{0}", GetInterfaceString("play_setting", "area_constant"));
			labelAccel.Text = string.Format("{0}", GetInterfaceString("play_setting", "area_acceleration"));
			toolTipName.SetToolTip(buttonSwap, GetInterfaceString("play_setting", "area_swap"));

			toolTipName.SetToolTip(buttonPlay, GetInterfaceString("play_setting", "play"));
			toolTipName.SetToolTip(buttonPause, GetInterfaceString("play_setting", "pause"));
			toolTipName.SetToolTip(buttonStop, GetInterfaceString("play_setting", "stop"));

			toolStripStatusLabelX.Text = string.Format("{0} 0.00 km/h", GetInterfaceString("status_xy", "velocity"));
			toolStripStatusLabelY.Text = string.Format("{0} 0.00", GetInterfaceString("status_xy", "pitch"));

			// Display update
			editState.CurrentViewMode = editState.CurrentViewMode;
			editState.CurrentInputMode = editState.CurrentInputMode;
			editState.CurrentToolMode = editState.CurrentToolMode;
		}
	}
}
