using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Dialogs;
using TrainEditor2.Models.Others;
using TrainEditor2.Systems;

namespace TrainEditor2.Models.Trains
{
	internal partial class Motor
	{
		private double XtoVelocity(double x)
		{
			double factorVelocity = GlControlWidth / (MaxVelocity - MinVelocity);
			return MinVelocity + x / factorVelocity;
		}

		private double YtoPitch(double y)
		{
			double factorPitch = -GlControlHeight / (MaxPitch - MinPitch);
			return MinPitch + (y - GlControlHeight) / factorPitch;
		}

		private double YtoVolume(double y)
		{
			double factorVolume = -GlControlHeight / (MaxVolume - MinVolume);
			return MinVolume + (y - GlControlHeight) / factorVolume;
		}

		private double VelocityToX(double v)
		{
			double factorVelocity = GlControlWidth / (MaxVelocity - MinVelocity);
			return (v - MinVelocity) * factorVelocity;
		}

		private double PitchToY(double p)
		{
			double factorPitch = -GlControlHeight / (MaxPitch - MinPitch);
			return GlControlHeight + (p - MinPitch) * factorPitch;
		}

		private double VolumeToY(double v)
		{
			double factorVolume = -GlControlHeight / (MaxVolume - MinVolume);
			return GlControlHeight + (v - MinVolume) * factorVolume;
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
			Utilities.Reset(0.5 * 40.0, ref minVelocity, ref maxVelocity);

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

		internal void Undo()
		{
			TrackState prev = PrevTrackStates.Last(x => x.Info == SelectedTrackInfo);
			NextTrackStates.Add(new TrackState(SelectedTrackInfo, SelectedTrack));
			SelectedTrack = (Track)prev.Track.Clone();
			PrevTrackStates.Remove(prev);

			IsRefreshGlControl = true;
		}

		internal void Redo()
		{
			TrackState next = NextTrackStates.Last(x => x.Info == SelectedTrackInfo);
			PrevTrackStates.Add(new TrackState(SelectedTrackInfo, SelectedTrack));
			SelectedTrack = (Track)next.Track.Clone();
			NextTrackStates.Remove(next);

			IsRefreshGlControl = true;
		}

		internal void TearingOff()
		{
			PrevTrackStates.Add(new TrackState(SelectedTrackInfo, (Track)SelectedTrack.Clone()));
			NextTrackStates.RemoveAll(x => x.Info == SelectedTrackInfo);
			Copy();
			SelectedTrack = new Track();

			IsRefreshGlControl = true;
		}

		internal void Copy()
		{
			CopyTrack = (Track)SelectedTrack.Clone();
		}

		internal void Paste()
		{
			PrevTrackStates.Add(new TrackState(SelectedTrackInfo, (Track)SelectedTrack.Clone()));
			NextTrackStates.RemoveAll(x => x.Info == SelectedTrackInfo);
			SelectedTrack = (Track)CopyTrack.Clone();

			IsRefreshGlControl = true;
		}

		internal void Cleanup()
		{
			Func<int, ObservableCollection<Line>, bool> condition = (i, ls) => ls.Any(l => l.LeftID == i || l.RightID == i);

			int[] pitchTargetIDs = new int[0];
			int[] volumeTargetIDs = new int[0];

			if (CurrentInputMode != InputMode.Volume)
			{
				pitchTargetIDs = SelectedTrack.PitchVertices.Keys.Where(i => !condition(i, SelectedTrack.PitchLines)).ToArray();
			}

			if (CurrentInputMode != InputMode.Pitch)
			{
				volumeTargetIDs = SelectedTrack.VolumeVertices.Keys.Where(i => !condition(i, SelectedTrack.VolumeLines)).ToArray();
			}

			if (!pitchTargetIDs.Any() && !volumeTargetIDs.Any())
			{
				return;
			}

			PrevTrackStates.Add(new TrackState(SelectedTrackInfo, (Track)SelectedTrack.Clone()));
			NextTrackStates.RemoveAll(s => s.Info == SelectedTrackInfo);

			foreach (int targetID in pitchTargetIDs)
			{
				Tracks[(int)SelectedTrackInfo].PitchVertices.Remove(targetID);
			}

			foreach (int targetID in volumeTargetIDs)
			{
				Tracks[(int)SelectedTrackInfo].VolumeVertices.Remove(targetID);
			}

			IsRefreshGlControl = true;
		}

		private void DeleteDotLine(VertexLibrary vertices, ObservableCollection<Line> lines)
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

		internal void Delete()
		{
			PrevTrackStates.Add(new TrackState(SelectedTrackInfo, (Track)SelectedTrack.Clone()));
			NextTrackStates.RemoveAll(s => s.Info == SelectedTrackInfo);

			switch (CurrentInputMode)
			{
				case InputMode.Pitch:
					DeleteDotLine(SelectedTrack.PitchVertices, SelectedTrack.PitchLines);
					break;
				case InputMode.Volume:
					DeleteDotLine(SelectedTrack.VolumeVertices, SelectedTrack.VolumeLines);
					break;
				case InputMode.SoundIndex:
					SelectedTrack.SoundIndices.Clear();
					break;
			}

			IsRefreshGlControl = true;
		}

		internal void DirectDot(double x, double y)
		{
			x = 0.2 * Math.Round(5.0 * x);
			y = 0.01 * Math.Round(100.0 * y);

			bool exist = false;

			switch (CurrentInputMode)
			{
				case InputMode.Pitch:
					exist = SelectedTrack.PitchVertices.Any(v => v.Value.X == x);
					break;
				case InputMode.Volume:
					exist = SelectedTrack.VolumeVertices.Any(v => v.Value.X == x);
					break;
			}

			if (exist)
			{
				MessageBox = new MessageBox
				{
					Title = @"Dot",
					Icon = BaseDialog.DialogIcon.Question,
					Button = BaseDialog.DialogButton.YesNo,
					Text = @"A point already exists at the same x coordinate, do you want to overwrite it?",
					IsOpen = true
				};

				if (MessageBox.DialogResult != true)
				{
					return;
				}
			}

			switch (CurrentInputMode)
			{
				case InputMode.Pitch:
					DrawDot(SelectedTrack.PitchVertices, x, y);
					break;
				case InputMode.Volume:
					DrawDot(SelectedTrack.VolumeVertices, x, y);
					break;
			}
		}

		internal void MouseDown(InputEventModel.EventArgs e)
		{
			if (e.LeftButton == InputEventModel.ButtonState.Pressed)
			{
				lastMousePosX = e.X;
				lastMousePosY = e.Y;

				if (CurrentSimState == SimulationState.Disable || CurrentSimState == SimulationState.Stopped)
				{
					if (CurrentInputMode != InputMode.SoundIndex)
					{
						switch (CurrentToolMode)
						{
							case ToolMode.Select:
								SelectDotLine(e);
								break;
							case ToolMode.Dot:
								DrawDot(e);
								break;
							case ToolMode.Line:
								DrawLine(e);
								break;
						}
					}
				}
			}
		}

		internal void MouseMove(InputEventModel.EventArgs e)
		{
			NowVelocity = 0.2 * Math.Round(5.0 * XtoVelocity(e.X));
			NowPitch = 0.01 * Math.Round(100.0 * YtoPitch(e.Y));
			NowVolume = 0.01 * Math.Round(100.0 * YtoVolume(e.Y));

			if (CurrentInputMode != InputMode.Volume)
			{
				ShowToolTipVertex(InputMode.Pitch, SelectedTrack.PitchVertices, ref hoveredVertexPitch, toolTipVertexPitch, NowVelocity, NowPitch);
			}

			if (CurrentInputMode != InputMode.Pitch)
			{
				ShowToolTipVertex(InputMode.Pitch, SelectedTrack.VolumeVertices, ref hoveredVertexVolume, toolTipVertexVolume, NowVelocity, NowVolume);
			}

			if (CurrentSimState == SimulationState.Disable || CurrentSimState == SimulationState.Stopped)
			{
				if (CurrentInputMode != InputMode.SoundIndex)
				{
					switch (CurrentToolMode)
					{
						case ToolMode.Select:
						case ToolMode.Line:
							switch (CurrentInputMode)
							{
								case InputMode.Pitch:
									ChangeCursor(SelectedTrack.PitchVertices, SelectedTrack.PitchLines, NowVelocity, NowPitch);
									break;
								case InputMode.Volume:
									ChangeCursor(SelectedTrack.VolumeVertices, SelectedTrack.VolumeLines, NowVelocity, NowVolume);
									break;
							}
							break;
						case ToolMode.Move:
							CurrentCursorType = InputEventModel.CursorType.ScrollAll;
							break;
						case ToolMode.Dot:
							CurrentCursorType = InputEventModel.CursorType.Cross;
							break;
					}
				}
				else
				{
					CurrentCursorType = InputEventModel.CursorType.Cross;
				}
			}
			else
			{
				CurrentCursorType = InputEventModel.CursorType.Arrow;
			}

			if (e.LeftButton == InputEventModel.ButtonState.Pressed)
			{
				if (CurrentSimState == SimulationState.Disable || CurrentSimState == SimulationState.Stopped)
				{
					double deltaX = e.X - lastMousePosX;
					double deltaY = e.Y - lastMousePosY;

					double factorVelocity = GlControlWidth / (maxVelocity - minVelocity);
					double factorPitch = -GlControlHeight / (maxPitch - minPitch);
					double factorVolume = -GlControlHeight / (maxVolume - minVolume);

					double deltaVelocity = 0.2 * Math.Round(5.0 * deltaX / factorVelocity);
					double deltaPitch = 0.01 * Math.Round(100.0 * deltaY / factorPitch);
					double deltaVolume = 0.01 * Math.Round(100.0 * deltaY / factorVolume);

					switch (CurrentInputMode)
					{
						case InputMode.Pitch:
							MouseDrag(SelectedTrack.PitchVertices, SelectedTrack.PitchLines, NowVelocity, NowPitch, deltaVelocity, deltaPitch);
							break;
						case InputMode.Volume:
							MouseDrag(SelectedTrack.VolumeVertices, SelectedTrack.VolumeLines, NowVelocity, NowVolume, deltaVelocity, deltaVolume);
							break;
						case InputMode.SoundIndex:
							if (deltaVelocity != 0.0)
							{
								previewArea = new Area(Math.Min(NowVelocity - deltaVelocity, NowVelocity), Math.Max(NowVelocity - deltaVelocity, NowVelocity), SelectedSoundIndex);
							}
							else
							{
								previewArea = null;
							}
							break;
					}

					if (CurrentInputMode != InputMode.SoundIndex && CurrentToolMode != ToolMode.Select)
					{
						lastMousePosX = e.X;
						lastMousePosY = e.Y;
					}

					IsRefreshGlControl = true;
				}
			}
		}

		private void ShowToolTipVertex(InputMode inputMode, VertexLibrary vertices, ref Vertex hoveredVertex, ToolTipModel toolTipVertex, double x, double y)
		{
			Func<Vertex, bool> conditionVertex = v => v.X - 0.2 < x && x < v.X + 0.2 && v.Y - 2.0 < y && y < v.Y + 2.0;

			Vertex newHoveredVertex = vertices.Values.FirstOrDefault(v => conditionVertex(v));

			if (newHoveredVertex != hoveredVertex)
			{
				if (newHoveredVertex != null)
				{
					Area area = SelectedTrack.SoundIndices.FirstOrDefault(a => a.LeftX <= newHoveredVertex.X && a.RightX >= newHoveredVertex.X);

					StringBuilder builder = new StringBuilder();
					builder.AppendLine($"{Utilities.GetInterfaceString("motor_sound_settings", "vertex_info", "velocity")}: {newHoveredVertex.X.ToString("0.00", culture)} km/h");

					switch (inputMode)
					{
						case InputMode.Pitch:
							builder.AppendLine($"{Utilities.GetInterfaceString("motor_sound_settings", "vertex_info", "pitch")}: {newHoveredVertex.Y.ToString("0.00", culture)}");
							break;
						case InputMode.Volume:
							builder.AppendLine($"{Utilities.GetInterfaceString("motor_sound_settings", "vertex_info", "volume")}: {newHoveredVertex.Y.ToString("0.00", culture)}");
							break;
					}

					builder.AppendLine($"{Utilities.GetInterfaceString("motor_sound_settings", "vertex_info", "sound_index")}: {area?.Index ?? -1}");

					toolTipVertex.Title = Utilities.GetInterfaceString("motor_sound_settings", "vertex_info", "name");
					toolTipVertex.Icon = ToolTipModel.ToolTipIcon.Information;
					toolTipVertex.Text = builder.ToString();
					toolTipVertex.X = VelocityToX(newHoveredVertex.X) + 10.0;

					switch (inputMode)
					{
						case InputMode.Pitch:
							toolTipVertex.Y = PitchToY(newHoveredVertex.Y) + 10.0;
							break;
						case InputMode.Volume:
							toolTipVertex.Y = VolumeToY(newHoveredVertex.Y) + 10.0;
							break;
					}

					toolTipVertex.IsOpen = true;
				}
				else
				{
					toolTipVertex.IsOpen = false;
				}

				hoveredVertex = newHoveredVertex;
			}
		}

		private void ChangeCursor(VertexLibrary vertices, ObservableCollection<Line> lines, double x, double y)
		{
			if (IsSelectDotLine(vertices, lines, x, y))
			{
				if (CurrentToolMode == ToolMode.Select || IsDrawLine(vertices, lines, x, y))
				{
					CurrentCursorType = InputEventModel.CursorType.Hand;
				}
				else
				{
					CurrentCursorType = InputEventModel.CursorType.No;
				}
			}
			else
			{
				CurrentCursorType = InputEventModel.CursorType.Arrow;
			}
		}

		private void MouseDrag(VertexLibrary vertices, ObservableCollection<Line> lines, double x, double y, double deltaX, double deltaY)
		{
			switch (CurrentToolMode)
			{
				case ToolMode.Select:
					{
						double leftX = Math.Min(x - deltaX, x);
						double rightX = Math.Max(x - deltaX, x);

						double topY = Math.Max(y - deltaY, y);
						double bottomY = Math.Min(y - deltaY, y);

						if (deltaX != 0.0 && deltaY != 0.0)
						{
							selectedRange = SelectedRange.CreateSelectedRange(vertices, lines, leftX, rightX, topY, bottomY);
						}
						else
						{
							selectedRange = null;
						}
					}
					break;
				case ToolMode.Move:
					MoveDot(vertices, deltaX, deltaY);
					break;
			}
		}

		internal void MouseUp()
		{
			if (CurrentInputMode != InputMode.SoundIndex)
			{
				isMoving = false;

				if (CurrentToolMode == ToolMode.Select)
				{
					if (selectedRange != null)
					{
						foreach (Vertex vertex in selectedRange.SelectedVertices)
						{
							vertex.Selected = !vertex.Selected;
						}

						foreach (Line line in selectedRange.SelectedLines)
						{
							line.Selected = !line.Selected;
						}

						selectedRange = null;
					}
				}
			}
			else
			{
				if (previewArea != null)
				{
					PrevTrackStates.Add(new TrackState(SelectedTrackInfo, (Track)SelectedTrack.Clone()));

					List<Area> addAreas = new List<Area>();

					foreach (Area area in SelectedTrack.SoundIndices)
					{
						if (area.RightX < previewArea.LeftX || area.LeftX > previewArea.RightX)
						{
							continue;
						}

						if (area.LeftX < previewArea.LeftX && area.RightX > previewArea.RightX)
						{
							if (area.Index != previewArea.Index)
							{
								addAreas.Add(new Area(area.LeftX, previewArea.LeftX - 0.2, area.Index));
								addAreas.Add(new Area(previewArea.RightX + 0.2, area.RightX, area.Index));
								area.TBD = true;
							}
							else
							{
								previewArea.TBD = true;
							}

							break;
						}

						if (area.LeftX < previewArea.LeftX)
						{
							if (area.Index != previewArea.Index)
							{
								area.RightX = previewArea.LeftX - 0.2;
							}
							else
							{
								previewArea.LeftX = area.LeftX;
								area.TBD = true;
							}
						}
						else if (area.RightX > previewArea.RightX)
						{
							if (area.Index != previewArea.Index)
							{
								area.LeftX = previewArea.RightX + 0.2;
							}
							else
							{
								previewArea.RightX = area.RightX;
								area.TBD = true;
							}
						}
						else
						{
							area.TBD = true;
						}
					}

					SelectedTrack.SoundIndices.Add(previewArea);
					SelectedTrack.SoundIndices.AddRange(addAreas);
					SelectedTrack.SoundIndices.RemoveAll(a => a.TBD);
					SelectedTrack.SoundIndices = new ObservableCollection<Area>(SelectedTrack.SoundIndices.OrderBy(a => a.LeftX));

					if (previewArea.TBD)
					{
						PrevTrackStates.Remove(PrevTrackStates.Last());
					}
					else
					{
						NextTrackStates.RemoveAll(s => s.Info == SelectedTrackInfo);
					}

					previewArea = null;
				}
			}

			IsRefreshGlControl = true;
		}

		internal void DirectMove(double x, double y)
		{
			x = 0.2 * Math.Round(5.0 * x);
			y = 0.01 * Math.Round(100.0 * y);

			switch (CurrentInputMode)
			{
				case InputMode.Pitch:
					MoveDot(SelectedTrack.PitchVertices, x, y);
					break;
				case InputMode.Volume:
					MoveDot(SelectedTrack.VolumeVertices, x, y);
					break;
			}
		}

		internal void ResetSelect()
		{
			if (CurrentInputMode != InputMode.Volume)
			{
				ResetSelect(SelectedTrack.PitchVertices, SelectedTrack.PitchLines);
			}

			if (CurrentInputMode != InputMode.Pitch)
			{
				ResetSelect(SelectedTrack.VolumeVertices, SelectedTrack.VolumeLines);
			}
		}

		private void ResetSelect(VertexLibrary vertices, ObservableCollection<Line> lines)
		{
			foreach (Vertex vertex in vertices.Values)
			{
				if (CurrentInputMode == InputMode.SoundIndex || CurrentToolMode != ToolMode.Select && CurrentToolMode != ToolMode.Move)
				{
					vertex.Selected = false;
				}

				if (CurrentInputMode == InputMode.SoundIndex || CurrentToolMode != ToolMode.Line)
				{
					vertex.IsOrigin = false;
				}
			}

			foreach (Line line in lines)
			{
				if (CurrentInputMode == InputMode.SoundIndex || CurrentToolMode != ToolMode.Select && CurrentToolMode != ToolMode.Move)
				{
					line.Selected = false;
				}
			}
		}

		private bool IsSelectDotLine(VertexLibrary vertices, ObservableCollection<Line> lines, double x, double y)
		{
			if (vertices.Any(v => v.Value.X - 0.2 < x && x < v.Value.X + 0.2 && v.Value.Y - 2.0 < y && y < v.Value.Y + 2.0))
			{
				return true;
			}

			if (lines.Any(l => vertices[l.LeftID].X + 0.2 < x && x < vertices[l.RightID].X - 0.2 && Math.Min(vertices[l.LeftID].Y, vertices[l.RightID].Y) - 2.0 < y && y < Math.Max(vertices[l.LeftID].Y, vertices[l.RightID].Y) + 2.0))
			{
				return true;
			}

			return false;
		}

		private void SelectDotLine(InputEventModel.EventArgs e)
		{
			double velocity = XtoVelocity(e.X);
			double pitch = YtoPitch(e.Y);
			double volume = YtoVolume(e.Y);

			switch (CurrentInputMode)
			{
				case InputMode.Pitch:
					SelectDotLine(SelectedTrack.PitchVertices, SelectedTrack.PitchLines, velocity, pitch);
					break;
				case InputMode.Volume:
					SelectDotLine(SelectedTrack.VolumeVertices, SelectedTrack.VolumeLines, velocity, volume);
					break;
			}
		}

		private void SelectDotLine(VertexLibrary vertices, ObservableCollection<Line> lines, double x, double y)
		{
			Func<Vertex, bool> conditionVertex = v => v.X - 0.2 < x && x < v.X + 0.2 && v.Y - 2.0 < y && y < v.Y + 2.0;

			if (vertices.Any(v => conditionVertex(v.Value)))
			{
				KeyValuePair<int, Vertex> selectVertex = vertices.First(v => conditionVertex(v.Value));

				if (!CurrentModifierKeys.HasFlag(InputEventModel.ModifierKeys.Control))
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
				if (!CurrentModifierKeys.HasFlag(InputEventModel.ModifierKeys.Control))
				{
					foreach (Vertex vertex in vertices.Values)
					{
						vertex.Selected = false;
					}
				}
			}

			Line selectLine = lines.FirstOrDefault(l => vertices[l.LeftID].X + 0.2 < x && x < vertices[l.RightID].X - 0.2 && Math.Min(vertices[l.LeftID].Y, vertices[l.RightID].Y) - 2.0 < y && y < Math.Max(vertices[l.LeftID].Y, vertices[l.RightID].Y) + 2.0);

			if (selectLine != null)
			{
				if (!CurrentModifierKeys.HasFlag(InputEventModel.ModifierKeys.Control))
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

			IsRefreshGlControl = true;
		}

		private void MoveDot(VertexLibrary vertices, double deltaX, double deltaY)
		{
			if (vertices.Values.Any(v => v.Selected))
			{
				if (!isMoving)
				{
					PrevTrackStates.Add(new TrackState(SelectedTrackInfo, (Track)SelectedTrack.Clone()));
					NextTrackStates.RemoveAll(s => s.Info == SelectedTrackInfo);
					isMoving = true;
				}

				foreach (Vertex select in vertices.Values.Where(v => v.Selected).OrderBy(v => v.X))
				{
					if (deltaX >= 0.0)
					{
						Vertex unselectLeft = vertices.Values.OrderBy(v => v.X).FirstOrDefault(v => v.X > select.X);

						if (unselectLeft != null)
						{
							if (select.X + deltaX + 0.2 >= unselectLeft.X)
							{
								deltaX = 0.0;
							}
						}
					}
					else
					{
						Vertex unselectRight = vertices.Values.OrderBy(v => v.X).LastOrDefault(v => v.X < select.X);

						if (unselectRight != null)
						{
							if (select.X + deltaX - 0.2 <= unselectRight.X)
							{
								deltaX = 0.0;
							}
						}

						if (select.X + deltaX < 0.0)
						{
							deltaX = 0.0;
						}
					}

					if (deltaY < 0.0)
					{
						if (select.Y + deltaY < 0.0)
						{
							deltaY = 0.0;
						}
					}
				}

				foreach (Vertex vertex in vertices.Values.Where(v => v.Selected))
				{
					vertex.X += deltaX;
					vertex.Y += deltaY;
				}

				IsRefreshGlControl = true;
			}
		}

		private void DrawDot(InputEventModel.EventArgs e)
		{
			double velocity = 0.2 * Math.Round(5.0 * XtoVelocity(e.X));
			double pitch = 0.01 * Math.Round(100.0 * YtoPitch(e.Y));
			double volume = 0.01 * Math.Round(100.0 * YtoVolume(e.Y));

			switch (CurrentInputMode)
			{
				case InputMode.Pitch:
					DrawDot(SelectedTrack.PitchVertices, velocity, pitch);
					break;
				case InputMode.Volume:
					DrawDot(SelectedTrack.VolumeVertices, velocity, volume);
					break;
			}
		}

		private void DrawDot(VertexLibrary vertices, double x, double y)
		{
			PrevTrackStates.Add(new TrackState(SelectedTrackInfo, (Track)SelectedTrack.Clone()));
			NextTrackStates.RemoveAll(s => s.Info == SelectedTrackInfo);
			vertices.Add(new Vertex(x, y));

			IsRefreshGlControl = true;
		}

		private bool IsDrawLine(VertexLibrary vertices, ObservableCollection<Line> lines, double x, double y)
		{
			Func<Vertex, bool> conditionVertex = v => v.X - 0.2 < x && x < v.X + 0.2 && v.Y - 2.0 < y && y < v.Y + 2.0;

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
					KeyValuePair<int, Vertex>[] selectVertices = new[] { origin, selectVertex }.OrderBy(v => v.Value.X).ToArray();

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

		private void DrawLine(InputEventModel.EventArgs e)
		{
			double velocity = XtoVelocity(e.X);
			double pitch = YtoPitch(e.Y);
			double volume = YtoVolume(e.Y);

			switch (CurrentInputMode)
			{
				case InputMode.Pitch:
					DrawLine(SelectedTrack.PitchVertices, SelectedTrack.PitchLines, velocity, pitch);
					break;
				case InputMode.Volume:
					DrawLine(SelectedTrack.VolumeVertices, SelectedTrack.VolumeLines, velocity, volume);
					break;
			}
		}

		private void DrawLine(VertexLibrary vertices, ObservableCollection<Line> lines, double x, double y)
		{
			Func<Vertex, bool> conditionVertex = v => v.X - 0.2 < x && x < v.X + 0.2 && v.Y - 2.0 < y && y < v.Y + 2.0;

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
					KeyValuePair<int, Vertex>[] selectVertices = new[] { origin, selectVertex }.OrderBy(v => v.Value.X).ToArray();

					Func<Line, bool> conditionLineLeft = l => vertices[l.LeftID].X <= selectVertices[0].Value.X && selectVertices[0].Value.X < vertices[l.RightID].X;
					Func<Line, bool> conditionLineRight = l => vertices[l.LeftID].X < selectVertices[1].Value.X && selectVertices[1].Value.X <= vertices[l.RightID].X;

					if (!lines.Any(l => conditionLineLeft(l)) && !lines.Any(l => conditionLineRight(l)))
					{
						PrevTrackStates.Add(new TrackState(SelectedTrackInfo, (Track)SelectedTrack.Clone()));
						NextTrackStates.RemoveAll(s => s.Info == SelectedTrackInfo);
						lines.Add(new Line(selectVertices[0].Key, selectVertices[1].Key));

						origin.Value.IsOrigin = false;
						selectVertex.Value.IsOrigin = true;
					}
				}
				else
				{
					selectVertex.Value.IsOrigin = true;
				}

				IsRefreshGlControl = true;
			}
		}

		private void DrawPolyLine(Matrix4d proj, Matrix4d look, Vector2d p1, Vector2d p2, double lineWidth, Color color)
		{
			Matrix4d inv = Matrix4d.Invert(look) * Matrix4d.Invert(proj);
			Vector2d line = new Vector2d((inv.M11 + inv.M12) * lineWidth / 2.0, (inv.M21 + inv.M22) * lineWidth / 2.0) / 100.0;

			double rad = Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);

			double p1x1 = p1.X + Math.Cos(rad + Math.PI / 2.0) * line.X;
			double p1y1 = p1.Y + Math.Sin(rad + Math.PI / 2.0) * line.Y;

			double p1x2 = p1.X + Math.Cos(rad - Math.PI / 2.0) * line.X;
			double p1y2 = p1.Y + Math.Sin(rad - Math.PI / 2.0) * line.Y;

			double p2x1 = p2.X + Math.Cos(rad + Math.PI / 2.0) * line.X;
			double p2y1 = p2.Y + Math.Sin(rad + Math.PI / 2.0) * line.Y;

			double p2x2 = p2.X + Math.Cos(rad - Math.PI / 2.0) * line.X;
			double p2y2 = p2.Y + Math.Sin(rad - Math.PI / 2.0) * line.Y;

			GL.Begin(PrimitiveType.TriangleStrip);
			GL.Color4(color);
			GL.Vertex2(p1x1, p1y1);
			GL.Vertex2(p1x2, p1y2);
			GL.Vertex2(p2x1, p2y1);
			GL.Vertex2(p2x2, p2y2);
			GL.End();
		}

		private void DrawPolyDashLine(Matrix4d proj, Matrix4d look, Box2d box, double lineWidth, double dashLength, Color color)
		{
			Matrix4d inv = Matrix4d.Invert(look) * Matrix4d.Invert(proj);
			Vector2d dash = new Vector2d((inv.M11 + inv.M12) * dashLength, (inv.M21 + inv.M22) * dashLength) / 100.0;

			for (double i = box.Left; i + dash.X < box.Right; i += dash.X * 2)
			{
				DrawPolyLine(proj, look, new Vector2d(i, box.Bottom), new Vector2d(i + dash.X, box.Bottom), lineWidth, color);
			}

			for (double i = box.Bottom; i + dash.Y < box.Top; i += dash.Y * 2)
			{
				DrawPolyLine(proj, look, new Vector2d(box.Right, i), new Vector2d(box.Right, i + dash.Y), lineWidth, color);
			}

			for (double i = box.Left; i + dash.X < box.Right; i += dash.X * 2)
			{
				DrawPolyLine(proj, look, new Vector2d(i, box.Top), new Vector2d(i + dash.X, box.Top), lineWidth, color);
			}

			for (double i = box.Bottom; i + dash.Y < box.Top; i += dash.Y * 2)
			{
				DrawPolyLine(proj, look, new Vector2d(box.Left, i), new Vector2d(box.Left, i + dash.Y), lineWidth, color);
			}
		}

		internal void DrawGlControl()
		{
			// prepare
			GL.Enable(EnableCap.PointSmooth);
			GL.Enable(EnableCap.PolygonSmooth);
			GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);
			GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Nicest);
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

			GL.Viewport(0, 0, GlControlWidth, GlControlHeight);
			GL.ClearColor(Color.Black);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			Matrix4d projPitch = Matrix4d.CreateOrthographic(MaxVelocity - MinVelocity, MaxPitch - MinPitch, float.Epsilon, 1.0);
			Matrix4d projVolume = Matrix4d.CreateOrthographic(MaxVelocity - MinVelocity, MaxVolume - MinVolume, float.Epsilon, 1.0);
			Matrix4d projString = Matrix4d.CreateOrthographicOffCenter(0.0, GlControlWidth, GlControlHeight, 0.0, -1.0, 1.0);
			Matrix4d lookPitch = Matrix4d.LookAt(new Vector3d((MinVelocity + MaxVelocity) / 2.0, (MinPitch + MaxPitch) / 2.0, float.Epsilon), new Vector3d((MinVelocity + MaxVelocity) / 2.0, (MinPitch + MaxPitch) / 2.0, 0.0), Vector3d.UnitY);
			Matrix4d lookVolume = Matrix4d.LookAt(new Vector3d((MinVelocity + MaxVelocity) / 2.0, (MinVolume + MaxVolume) / 2.0, float.Epsilon), new Vector3d((MinVelocity + MaxVelocity) / 2.0, (MinVolume + MaxVolume) / 2.0, 0.0), Vector3d.UnitY);

			// vertical grid
			{
				GL.MatrixMode(MatrixMode.Projection);
				GL.LoadMatrix(ref projPitch);

				GL.MatrixMode(MatrixMode.Modelview);
				GL.LoadMatrix(ref lookPitch);

				GL.Begin(PrimitiveType.Lines);

				for (double v = 0.0; v < MaxVelocity; v += 10.0)
				{
					GL.Color4(Color.DimGray);
					GL.Vertex2(v, 0.0);
					GL.Vertex2(v, float.MaxValue);
				}

				GL.End();

				Program.Renderer.CurrentProjectionMatrix = projString;
				Program.Renderer.CurrentViewMatrix = Matrix4d.Identity;

				for (double v = 0.0; v < MaxVelocity; v += 10.0)
				{
					Program.Renderer.OpenGlString.Draw(Fonts.VerySmallFont, v.ToString("0", culture), new Point((int)VelocityToX(v) + 1, 1), TextAlignment.TopLeft, new Color128(Color24.Grey));
				}

				GL.Disable(EnableCap.Texture2D);
			}

			// horizontal grid
			switch (CurrentInputMode)
			{
				case InputMode.Pitch:
					GL.MatrixMode(MatrixMode.Projection);
					GL.LoadMatrix(ref projPitch);

					GL.MatrixMode(MatrixMode.Modelview);
					GL.LoadMatrix(ref lookPitch);

					GL.Begin(PrimitiveType.Lines);

					for (double p = 0.0; p < MaxPitch; p += 100.0)
					{
						GL.Color4(Color.DimGray);
						GL.Vertex2(MinVelocity, p);
						GL.Vertex2(MaxVelocity, p);
					}

					GL.End();

					Program.Renderer.CurrentProjectionMatrix = projString;
					Program.Renderer.CurrentViewMatrix = Matrix4d.Identity;

					for (double p = 0.0; p < MaxPitch; p += 100.0)
					{
						Program.Renderer.OpenGlString.Draw(Fonts.VerySmallFont, p.ToString("0", culture), new Point(1, (int)PitchToY(p) + 1), TextAlignment.TopLeft, new Color128(Color24.Grey));
					}

					GL.Disable(EnableCap.Texture2D);
					break;
				case InputMode.Volume:
					GL.MatrixMode(MatrixMode.Projection);
					GL.LoadMatrix(ref projVolume);

					GL.MatrixMode(MatrixMode.Modelview);
					GL.LoadMatrix(ref lookVolume);

					GL.Begin(PrimitiveType.Lines);

					for (double v = 0.0; v < MaxVolume; v += 128.0)
					{
						GL.Color4(Color.DimGray);
						GL.Vertex2(MinVelocity, v);
						GL.Vertex2(MaxVelocity, v);
					}

					GL.End();

					Program.Renderer.CurrentProjectionMatrix = projString;
					Program.Renderer.CurrentViewMatrix = Matrix4d.Identity;

					for (double v = 0.0; v < MaxVolume; v += 128.0)
					{
						Program.Renderer.OpenGlString.Draw(Fonts.VerySmallFont, v.ToString("0", culture), new Point(1, (int)VolumeToY(v) + 1), TextAlignment.TopLeft, new Color128(Color24.Grey));
					}

					GL.Disable(EnableCap.Texture2D);
					break;
			}

			// dot
			if (CurrentInputMode != InputMode.Volume)
			{
				GL.MatrixMode(MatrixMode.Projection);
				GL.LoadMatrix(ref projPitch);

				GL.MatrixMode(MatrixMode.Modelview);
				GL.LoadMatrix(ref lookPitch);

				GL.PointSize(11.0f);
				GL.Begin(PrimitiveType.Points);

				foreach (Vertex vertex in SelectedTrack.PitchVertices.Values)
				{
					Area area = SelectedTrack.SoundIndices.FirstOrDefault(a => a.LeftX <= vertex.X && vertex.X <= a.RightX);
					Color c;

					if (area != null && area.Index >= 0)
					{
						double hue = Utilities.HueFactor * area.Index;
						hue -= Math.Floor(hue);
						c = Utilities.GetColor(hue, vertex.Selected || vertex.IsOrigin);
					}
					else
					{
						if (vertex.Selected || vertex.IsOrigin)
						{
							c = Color.Silver;
						}
						else
						{
							c = Color.FromArgb((int)Math.Round(Color.Silver.R * 0.6), (int)Math.Round(Color.Silver.G * 0.6), (int)Math.Round(Color.Silver.B * 0.6));
						}
					}

					GL.Color4(c);
					GL.Vertex2(vertex.X, vertex.Y);
				}

				GL.End();
			}

			if (CurrentInputMode != InputMode.Pitch)
			{
				GL.MatrixMode(MatrixMode.Projection);
				GL.LoadMatrix(ref projVolume);

				GL.MatrixMode(MatrixMode.Modelview);
				GL.LoadMatrix(ref lookVolume);

				GL.PointSize(9.0f);
				GL.Begin(PrimitiveType.Points);

				foreach (Vertex vertex in SelectedTrack.VolumeVertices.Values)
				{
					Area area = SelectedTrack.SoundIndices.FirstOrDefault(a => a.LeftX <= vertex.X && vertex.X <= a.RightX);
					Color c;

					if (area != null && area.Index >= 0)
					{
						double hue = Utilities.HueFactor * area.Index;
						hue -= Math.Floor(hue);
						c = Utilities.GetColor(hue, vertex.Selected || vertex.IsOrigin);
					}
					else
					{
						if (vertex.Selected || vertex.IsOrigin)
						{
							c = Color.Silver;
						}
						else
						{
							c = Color.FromArgb((int)Math.Round(Color.Silver.R * 0.6), (int)Math.Round(Color.Silver.G * 0.6), (int)Math.Round(Color.Silver.B * 0.6));
						}
					}

					GL.Color4(c);
					GL.Vertex2(vertex.X, vertex.Y);
				}

				GL.End();
			}

			// line
			if (CurrentInputMode != InputMode.Volume)
			{
				GL.MatrixMode(MatrixMode.Projection);
				GL.LoadMatrix(ref projPitch);

				GL.MatrixMode(MatrixMode.Modelview);
				GL.LoadMatrix(ref lookPitch);

				foreach (Line line in SelectedTrack.PitchLines)
				{
					Vertex left = SelectedTrack.PitchVertices[line.LeftID];
					Vertex right = SelectedTrack.PitchVertices[line.RightID];

					Func<double, double> f = x => left.Y + (right.Y - left.Y) / (right.X - left.X) * (x - left.X);

					{
						Color c;

						if (line.Selected)
						{
							c = Color.Silver;
						}
						else
						{
							c = Color.FromArgb((int)Math.Round(Color.Silver.R * 0.6), (int)Math.Round(Color.Silver.G * 0.6), (int)Math.Round(Color.Silver.B * 0.6));
						}

						DrawPolyLine(projPitch, lookPitch, new Vector2d(left.X, left.Y), new Vector2d(right.X, right.Y), 1.5, c);
					}

					foreach (Area area in SelectedTrack.SoundIndices)
					{
						if (right.X < area.LeftX || left.X > area.RightX || area.Index < 0)
						{
							continue;
						}

						double hue = Utilities.HueFactor * area.Index;
						hue -= Math.Floor(hue);

						Vector2d p1 = new Vector2d(left.X < area.LeftX ? area.LeftX : left.X, left.X < area.LeftX ? f(area.LeftX) : left.Y);
						Vector2d p2 = new Vector2d(right.X > area.RightX ? area.RightX : right.X, right.X > area.RightX ? f(area.RightX) : right.Y);

						DrawPolyLine(projPitch, lookPitch, p1, p2, 1.5, Utilities.GetColor(hue, line.Selected));
					}
				}
			}

			if (CurrentInputMode != InputMode.Pitch)
			{
				GL.MatrixMode(MatrixMode.Projection);
				GL.LoadMatrix(ref projVolume);

				GL.MatrixMode(MatrixMode.Modelview);
				GL.LoadMatrix(ref lookVolume);

				foreach (Line line in SelectedTrack.VolumeLines)
				{
					Vertex left = SelectedTrack.VolumeVertices[line.LeftID];
					Vertex right = SelectedTrack.VolumeVertices[line.RightID];

					Func<double, double> f = x => left.Y + (right.Y - left.Y) / (right.X - left.X) * (x - left.X);

					{
						Color c;

						if (line.Selected)
						{
							c = Color.Silver;
						}
						else
						{
							c = Color.FromArgb((int)Math.Round(Color.Silver.R * 0.6), (int)Math.Round(Color.Silver.G * 0.6), (int)Math.Round(Color.Silver.B * 0.6));
						}

						DrawPolyLine(projVolume, lookVolume, new Vector2d(left.X, left.Y), new Vector2d(right.X, right.Y), 1.0, c);
					}

					foreach (Area area in SelectedTrack.SoundIndices)
					{
						if (right.X < area.LeftX || left.X > area.RightX || area.Index < 0)
						{
							continue;
						}

						double hue = Utilities.HueFactor * area.Index;
						hue -= Math.Floor(hue);

						Vector2d p1 = new Vector2d(left.X < area.LeftX ? area.LeftX : left.X, left.X < area.LeftX ? f(area.LeftX) : left.Y);
						Vector2d p2 = new Vector2d(right.X > area.RightX ? area.RightX : right.X, right.X > area.RightX ? f(area.RightX) : right.Y);

						DrawPolyLine(projVolume, lookVolume, p1, p2, 1.0, Utilities.GetColor(hue, line.Selected));
					}
				}
			}

			// area
			if (CurrentInputMode == InputMode.SoundIndex)
			{
				IEnumerable<Area> areas;

				if (previewArea != null)
				{
					areas = SelectedTrack.SoundIndices.Concat(new[] { previewArea });
				}
				else
				{
					areas = SelectedTrack.SoundIndices;
				}

				GL.MatrixMode(MatrixMode.Projection);
				GL.LoadMatrix(ref projPitch);

				GL.MatrixMode(MatrixMode.Modelview);
				GL.LoadMatrix(ref lookPitch);

				foreach (Area area in areas)
				{
					Color c;

					if (area.Index >= 0)
					{
						double hue = Utilities.HueFactor * area.Index;
						hue -= Math.Floor(hue);
						c = Utilities.GetColor(hue, true);
					}
					else
					{
						c = Color.Silver;
					}

					GL.Begin(PrimitiveType.TriangleStrip);

					GL.Color4(Color.FromArgb(64, c));
					GL.Vertex2(area.LeftX, 0.0);
					GL.Vertex2(area.RightX, 0.0);
					GL.Vertex2(area.LeftX, float.MaxValue);
					GL.Vertex2(area.RightX, float.MaxValue);

					GL.End();
				}
			}

			// selected range
			if (selectedRange != null)
			{
				switch (CurrentInputMode)
				{
					case InputMode.Pitch:
						DrawPolyDashLine(projPitch, lookPitch, selectedRange.Range, 2.0, 4.0, Color.DimGray);
						break;
					case InputMode.Volume:
						DrawPolyDashLine(projVolume, lookVolume, selectedRange.Range, 2.0, 4.0, Color.DimGray);
						break;
				}
			}

			// simulation speed
			if (CurrentSimState == SimulationState.Started || CurrentSimState == SimulationState.Paused)
			{
				GL.MatrixMode(MatrixMode.Projection);
				GL.LoadMatrix(ref projPitch);

				GL.MatrixMode(MatrixMode.Modelview);
				GL.LoadMatrix(ref lookPitch);

				GL.LineWidth(3.0f);
				GL.Begin(PrimitiveType.Lines);

				GL.Color4(Color.White);
				GL.Vertex2(new Vector2((float)nowSpeed, 0.0f));
				GL.Vertex2(new Vector2((float)nowSpeed, float.MaxValue));

				GL.End();
			}

			IsRefreshGlControl = false;
		}
	}
}
