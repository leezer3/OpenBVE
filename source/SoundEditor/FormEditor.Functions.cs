using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Windows.Forms;
using OpenBveApi.Interface;
using SoundEditor.Parsers.Sound;
using SoundEditor.Parsers.Train;
using SoundEditor.Systems;

namespace SoundEditor
{
	public partial class FormEditor
	{
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
			switch (MessageBox.Show(GetInterfaceString("message", "new"), GetInterfaceString("menu", "file", "new"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
			{
				case DialogResult.Cancel:
					return;
				case DialogResult.Yes:
					SaveFile();
					break;
			}

			fileName = null;
			train = null;
			soundCfg = new SoundCfg.Sounds();

			InitializeListViewSound();

			radioButtonSaveXml.Checked = isSoundXml = false;

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
			switch (MessageBox.Show(GetInterfaceString("message", "open"), GetInterfaceString("menu", "file", "open"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
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
					soundCfg = SoundCfg.ParseSoundCfg(Path.GetDirectoryName(fileName), Encoding.UTF8, radioButtonOpenXml.Checked, out isSoundXml);

					InitializeListViewSound();

					radioButtonSaveXml.Checked = isSoundXml;

					LoadData();

					toolStripMenuItemSave.Enabled = true;

					editState.CurrentSimuState = EditState.SimulationState.Stopped;
					buttonPause.Enabled = false;
					buttonStop.Enabled = false;
				}
				catch (Exception exception)
				{
					MessageBox.Show(exception.Message, GetInterfaceString("menu", "file", "open"), MessageBoxButtons.OK, MessageBoxIcon.Hand);

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
				SoundCfg.SaveSoundCfg(Path.GetDirectoryName(fileName), soundCfg, radioButtonSaveXml.Checked);

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
				MessageBox.Show(exception.Message, GetInterfaceString("menu", "file", "save"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
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
					SoundCfg.SaveSoundCfg(Path.GetDirectoryName(fileName), soundCfg, radioButtonSaveXml.Checked);

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
					MessageBox.Show(exception.Message, GetInterfaceString("menu", "file", "save_as"), MessageBoxButtons.OK, MessageBoxIcon.Hand);

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

		private static string GetInterfaceString(params string[] ids)
		{
			return Translations.GetInterfaceString(string.Format("sound_editor_{0}", string.Join("_", ids)));
		}

		private void ApplyLanguage()
		{
			toolStripMenuItemFile.Text = string.Format("{0}(&F)", GetInterfaceString("menu", "file", "name"));
			toolStripMenuItemNew.Text = string.Format("{0}(&N)", GetInterfaceString("menu", "file", "new"));
			toolStripMenuItemOpen.Text = string.Format("{0}(&O)", GetInterfaceString("menu", "file", "open"));
			toolStripMenuItemSave.Text = string.Format("{0}(&S)", GetInterfaceString("menu", "file", "save"));
			toolStripMenuItemSaveAs.Text = string.Format("{0}(&A)", GetInterfaceString("menu", "file", "save_as"));
			toolStripMenuItemExit.Text = string.Format("{0}(&X)", GetInterfaceString("menu", "file", "exit"));

			toolStripButtonNew.Text = string.Format("{0} (Ctrl+N)", GetInterfaceString("menu", "file", "new"));
			toolStripButtonOpen.Text = string.Format("{0} (Ctrl+O)", GetInterfaceString("menu", "file", "open"));
			toolStripButtonSave.Text = string.Format("{0} (Ctrl+S)", GetInterfaceString("menu", "file", "save"));

			toolStripMenuItemEdit.Text = string.Format("{0}(&E)", GetInterfaceString("menu", "edit", "name"));
			toolStripMenuItemUndo.Text = string.Format("{0}(&U)", GetInterfaceString("menu", "edit", "undo"));
			toolStripMenuItemRedo.Text = string.Format("{0}(&R)", GetInterfaceString("menu", "edit", "redo"));
			toolStripMenuItemTearingOff.Text = string.Format("{0}(&T)", GetInterfaceString("menu", "edit", "cut"));
			toolStripMenuItemCopy.Text = string.Format("{0}(&C)", GetInterfaceString("menu", "edit", "copy"));
			toolStripMenuItemPaste.Text = string.Format("{0}(&P)", GetInterfaceString("menu", "edit", "paste"));
			toolStripMenuItemCleanup.Text = string.Format("{0}", GetInterfaceString("menu", "edit", "cleanup"));
			toolStripMenuItemDelete.Text = string.Format("{0}(&D)", GetInterfaceString("menu", "edit", "delete"));

			toolStripButtonUndo.Text = string.Format("{0} (Ctrl+Z)", GetInterfaceString("menu", "edit", "undo"));
			toolStripButtonRedo.Text = string.Format("{0} (Ctrl+Y)", GetInterfaceString("menu", "edit", "redo"));
			toolStripButtonTearingOff.Text = string.Format("{0} (Ctrl+X)", GetInterfaceString("menu", "edit", "cut"));
			toolStripButtonCopy.Text = string.Format("{0} (Ctrl+C)", GetInterfaceString("menu", "edit", "copy"));
			toolStripButtonPaste.Text = string.Format("{0} (Ctrl+V)", GetInterfaceString("menu", "edit", "paste"));
			toolStripButtonCleanup.Text = string.Format("{0}", GetInterfaceString("menu", "edit", "cleanup"));
			toolStripButtonDelete.Text = string.Format("{0} (Del)", GetInterfaceString("menu", "edit", "delete"));

			toolStripMenuItemView.Text = string.Format("{0}(&V)", GetInterfaceString("menu", "view", "name"));
			toolStripMenuItemPower.Text = string.Format("{0}(&P)", GetInterfaceString("menu", "view", "power"));
			toolStripMenuItemBrake.Text = string.Format("{0}(&B)", GetInterfaceString("menu", "view", "brake"));
			toolStripMenuItemPowerTrack1.Text = toolStripMenuItemBrakeTrack1.Text = string.Format("{0}1(&1)", GetInterfaceString("menu", "view", "track"));
			toolStripMenuItemPowerTrack2.Text = toolStripMenuItemBrakeTrack2.Text = string.Format("{0}2(&2)", GetInterfaceString("menu", "view", "track"));

			toolStripMenuItemInput.Text = string.Format("{0}(&I)", GetInterfaceString("menu", "input", "name"));
			toolStripMenuItemPitch.Text = string.Format("{0}(&P)", GetInterfaceString("menu", "input", "pitch"));
			toolStripMenuItemVolume.Text = string.Format("{0}(&V)", GetInterfaceString("menu", "input", "volume"));
			toolStripMenuItemIndex.Text = string.Format("{0}(&I)", GetInterfaceString("menu", "input", "sound_index"));
			toolStripComboBoxIndex.Items[0] = string.Format("{0}", GetInterfaceString("menu", "input", "sound_index_none"));

			toolStripMenuItemTool.Text = string.Format("{0}(&T)", GetInterfaceString("menu", "tool", "name"));
			toolStripMenuItemSelect.Text = string.Format("{0}(&S)", GetInterfaceString("menu", "tool", "select"));
			toolStripMenuItemMove.Text = string.Format("{0}(&M)", GetInterfaceString("menu", "tool", "move"));
			toolStripMenuItemDot.Text = string.Format("{0}(&D)", GetInterfaceString("menu", "tool", "dot"));
			toolStripMenuItemLine.Text = string.Format("{0}(&L)", GetInterfaceString("menu", "tool", "line"));

			toolStripButtonSelect.Text = string.Format("{0} (Alt+A)", GetInterfaceString("menu", "tool", "select"));
			toolStripButtonMove.Text = string.Format("{0} (Alt+S)", GetInterfaceString("menu", "tool", "move"));
			toolStripButtonDot.Text = string.Format("{0} (Alt+D)", GetInterfaceString("menu", "tool", "dot"));
			toolStripButtonLine.Text = string.Format("{0} (Alt+F)", GetInterfaceString("menu", "tool", "line"));

			toolStripStatusLabelLanguage.Text = string.Format("{0}", GetInterfaceString("menu", "language"));

			tabPageSoundSetting.Text = string.Format("{0}", GetInterfaceString("sound_cfg", "name"));

			columnHeaderKey.Text = string.Format("{0}", GetInterfaceString("sound_cfg", "key"));
			columnHeaderFileName.Text = string.Format("{0}", GetInterfaceString("sound_cfg", "filename"));
			columnHeaderPosition.Text = string.Format("{0}", GetInterfaceString("sound_cfg", "position"));
			columnHeaderRadius.Text = string.Format("{0}", GetInterfaceString("sound_cfg", "radius"));

			toolStripMenuItemError.Text = string.Format("0 {0}", GetInterfaceString("sound_cfg", "status", "error"));
			toolStripMenuItemWarning.Text = string.Format("0 {0}", GetInterfaceString("sound_cfg", "status", "warning"));
			toolStripMenuItemInfo.Text = string.Format("0 {0}", GetInterfaceString("sound_cfg", "status", "info"));
			toolStripMenuItemClear.Text = string.Format("{0}", GetInterfaceString("sound_cfg", "status", "clear"));

			columnHeaderLevel.Text = string.Format("{0}", GetInterfaceString("sound_cfg", "status", "level"));
			columnHeaderText.Text = string.Format("{0}", GetInterfaceString("sound_cfg", "status", "description"));

			groupBoxOpen.Text = string.Format("{0}", GetInterfaceString("sound_cfg", "open_setting", "name"));

			groupBoxOpenFormat.Text = string.Format("{0}", GetInterfaceString("sound_cfg", "open_setting", "priority_format", "name"));
			radioButtonOpenCfg.Text = string.Format("{0}", GetInterfaceString("sound_cfg", "open_setting", "priority_format", "cfg"));
			radioButtonOpenXml.Text = string.Format("{0}", GetInterfaceString("sound_cfg", "open_setting", "priority_format", "xml"));

			groupBoxSave.Text = string.Format("{0}", GetInterfaceString("sound_cfg", "save_setting", "name"));

			groupBoxSaveFormat.Text = string.Format("{0}", GetInterfaceString("sound_cfg", "save_setting", "format", "name"));
			radioButtonSaveCfg.Text = string.Format("{0}", GetInterfaceString("sound_cfg", "save_setting", "format", "cfg"));
			radioButtonSaveXml.Text = string.Format("{0}", GetInterfaceString("sound_cfg", "save_setting", "format", "xml"));

			groupBoxEntryEdit.Text = string.Format("{0}", GetInterfaceString("sound_cfg", "entry_edit", "name"));

			groupBoxSection.Text = string.Format("{0}", GetInterfaceString("sound_cfg", "entry_edit", "section", "name"));

			groupBoxKey.Text = string.Format("{0}", GetInterfaceString("sound_cfg", "entry_edit", "key", "name"));

			groupBoxValue.Text = string.Format("{0}", GetInterfaceString("sound_cfg", "entry_edit", "value", "name"));
			labelFileName.Text = string.Format("{0}", GetInterfaceString("sound_cfg", "entry_edit", "value", "filename"));
			buttonOpen.Text = string.Format("{0}", GetInterfaceString("sound_cfg", "entry_edit", "value", "open"));
			checkBoxRadius.Text = string.Format("{0}", GetInterfaceString("sound_cfg", "entry_edit", "value", "radius"));
			checkBoxPosition.Text = string.Format("{0}", GetInterfaceString("sound_cfg", "entry_edit", "value", "position"));

			groupBoxPosition.Text = string.Format("{0}", GetInterfaceString("sound_cfg", "entry_edit", "value", "position", "name"));
			labelPositionX.Text = string.Format("{0}", GetInterfaceString("sound_cfg", "entry_edit", "value", "position", "x"));
			labelPositionY.Text = string.Format("{0}", GetInterfaceString("sound_cfg", "entry_edit", "value", "position", "y"));
			labelPositionZ.Text = string.Format("{0}", GetInterfaceString("sound_cfg", "entry_edit", "value", "position", "z"));

			buttonAdd.Text = string.Format("{0}", GetInterfaceString("sound_cfg", "entry_edit", "add"));
			buttonRemove.Text = string.Format("{0}", GetInterfaceString("sound_cfg", "entry_edit", "remove"));
			buttonApply.Text = string.Format("{0}", GetInterfaceString("sound_cfg", "entry_edit", "apply"));

			tabPageMotorEditor.Text = string.Format("{0}", GetInterfaceString("motor_sound", "name"));

			groupBoxView.Text = string.Format("{0}", GetInterfaceString("motor_sound", "view_setting", "name"));
			labelMinVelocity.Text = string.Format("{0}", GetInterfaceString("motor_sound", "view_setting", "min_velocity"));
			labelMaxVelocity.Text = string.Format("{0}", GetInterfaceString("motor_sound", "view_setting", "max_velocity"));
			labelMinPitch.Text = string.Format("{0}", GetInterfaceString("motor_sound", "view_setting", "min_pitch"));
			labelMaxPitch.Text = string.Format("{0}", GetInterfaceString("motor_sound", "view_setting", "max_pitch"));
			labelMinVolume.Text = string.Format("{0}", GetInterfaceString("motor_sound", "view_setting", "min_volume"));
			labelMaxVolume.Text = string.Format("{0}", GetInterfaceString("motor_sound", "view_setting", "max_volume"));
			toolTipName.SetToolTip(buttonZoomIn, GetInterfaceString("motor_sound", "view_setting", "zoom_in"));
			toolTipName.SetToolTip(buttonZoomOut, GetInterfaceString("motor_sound", "view_setting", "zoom_out"));
			toolTipName.SetToolTip(buttonReset, GetInterfaceString("motor_sound", "view_setting", "reset"));

			groupBoxDirect.Text = string.Format("{0}", GetInterfaceString("motor_sound", "direct_input", "name"));
			labelDirectX.Text = string.Format("{0}", GetInterfaceString("motor_sound", "direct_input", "x"));
			labelDirectY.Text = string.Format("{0}", GetInterfaceString("motor_sound", "direct_input", "y"));
			toolTipName.SetToolTip(buttonDirectDot, GetInterfaceString("motor_sound", "direct_input", "dot"));
			toolTipName.SetToolTip(buttonDirectMove, GetInterfaceString("motor_sound", "direct_input", "move"));

			groupBoxPlay.Text = string.Format("{0}", GetInterfaceString("motor_sound", "play_setting", "name"));

			groupBoxSource.Text = string.Format("{0}", GetInterfaceString("motor_sound", "play_setting", "source", "name"));
			labelRun.Text = string.Format("{0}", GetInterfaceString("motor_sound", "play_setting", "source", "run"));
			checkBoxTrack1.Text = string.Format("{0}1", GetInterfaceString("motor_sound", "play_setting", "source", "track"));
			checkBoxTrack2.Text = string.Format("{0}2", GetInterfaceString("motor_sound", "play_setting", "source", "track"));

			groupBoxArea.Text = string.Format("{0}", GetInterfaceString("motor_sound", "play_setting", "area", "name"));
			checkBoxLoop.Text = string.Format("{0}", GetInterfaceString("motor_sound", "play_setting", "area", "loop"));
			checkBoxConstant.Text = string.Format("{0}", GetInterfaceString("motor_sound", "play_setting", "area", "constant"));
			labelAccel.Text = string.Format("{0}", GetInterfaceString("motor_sound", "play_setting", "area", "acceleration"));
			toolTipName.SetToolTip(buttonSwap, GetInterfaceString("motor_sound", "play_setting", "area", "swap"));

			toolTipName.SetToolTip(buttonPlay, GetInterfaceString("motor_sound", "play_setting", "play"));
			toolTipName.SetToolTip(buttonPause, GetInterfaceString("motor_sound", "play_setting", "pause"));
			toolTipName.SetToolTip(buttonStop, GetInterfaceString("motor_sound", "play_setting", "stop"));

			toolStripStatusLabelX.Text = string.Format("{0} 0.00 km/h", GetInterfaceString("motor_sound", "status", "xy", "velocity"));
			toolStripStatusLabelY.Text = string.Format("{0} 0.00", GetInterfaceString("motor_sound", "status", "xy", "pitch"));

			// Display update
			editState.CurrentViewMode = editState.CurrentViewMode;
			editState.CurrentInputMode = editState.CurrentInputMode;
			editState.CurrentToolMode = editState.CurrentToolMode;

			switch (tabControlEditor.SelectedIndex)
			{
				case 0:
					editState.ChangeEditStatus(false);
					editState.ChangeViewStatus(false);
					editState.ChangeInputStatus(false);
					editState.ChangeToolsStatus(false);
					break;
				case 1:
					editState.ChangeEditStatus(true);
					editState.ChangeViewStatus(true);
					editState.ChangeInputStatus(true);
					editState.ChangeToolsStatus(true);
					break;
			}
		}

		private void InitializeListViewSound()
		{
			listViewSound.Groups.Clear();
			listViewSound.Items.Clear();

			ListViewGroup groupRun = new ListViewGroup
			{
				Tag = SoundSection.Run,
				Header = SoundSection.Run.GetStringValue(),
				Name = SoundSection.Run.GetStringValue()
			};
			ListViewGroup groupFlange = new ListViewGroup
			{
				Tag = SoundSection.Flange,
				Header = SoundSection.Flange.GetStringValue(),
				Name = SoundSection.Flange.GetStringValue()
			};
			ListViewGroup groupMotor = new ListViewGroup
			{
				Tag = SoundSection.Motor,
				Header = SoundSection.Motor.GetStringValue(),
				Name = SoundSection.Motor.GetStringValue()
			};
			ListViewGroup groupFrontSwitch = new ListViewGroup
			{
				Tag = SoundSection.FrontSwitch,
				Header = SoundSection.FrontSwitch.GetStringValue(),
				Name = SoundSection.FrontSwitch.GetStringValue()
			};
			ListViewGroup groupRearSwitch = new ListViewGroup
			{
				Tag = SoundSection.RearSwitch,
				Header = SoundSection.RearSwitch.GetStringValue(),
				Name = SoundSection.RearSwitch.GetStringValue()
			};
			ListViewGroup groupBrake = new ListViewGroup
			{
				Tag = SoundSection.Brake,
				Header = SoundSection.Brake.GetStringValue(),
				Name = SoundSection.Brake.GetStringValue()
			};
			ListViewGroup groupCompressor = new ListViewGroup
			{
				Tag = SoundSection.Compressor,
				Header = SoundSection.Compressor.GetStringValue(),
				Name = SoundSection.Compressor.GetStringValue()
			};
			ListViewGroup groupSuspension = new ListViewGroup
			{
				Tag = SoundSection.Suspension,
				Header = SoundSection.Suspension.GetStringValue(),
				Name = SoundSection.Suspension.GetStringValue()
			};
			ListViewGroup groupHorn = new ListViewGroup
			{
				Tag = SoundSection.Horn,
				Header = SoundSection.Horn.GetStringValue(),
				Name = SoundSection.Horn.GetStringValue()
			};
			ListViewGroup groupDoor = new ListViewGroup
			{
				Tag = SoundSection.Door,
				Header = SoundSection.Door.GetStringValue(),
				Name = SoundSection.Door.GetStringValue()
			};
			ListViewGroup groupAts = new ListViewGroup
			{
				Tag = SoundSection.Ats,
				Header = SoundSection.Ats.GetStringValue(),
				Name = SoundSection.Ats.GetStringValue()
			};
			ListViewGroup groupBuzzer = new ListViewGroup
			{
				Tag = SoundSection.Buzzer,
				Header = SoundSection.Buzzer.GetStringValue(),
				Name = SoundSection.Buzzer.GetStringValue()
			};
			ListViewGroup groupPilotLamp = new ListViewGroup
			{
				Tag = SoundSection.PilotLamp,
				Header = SoundSection.PilotLamp.GetStringValue(),
				Name = SoundSection.PilotLamp.GetStringValue()
			};
			ListViewGroup groupBrakeHandle = new ListViewGroup
			{
				Tag = SoundSection.BrakeHandle,
				Header = SoundSection.BrakeHandle.GetStringValue(),
				Name = SoundSection.BrakeHandle.GetStringValue()
			};
			ListViewGroup groupMasterController = new ListViewGroup
			{
				Tag = SoundSection.MasterController,
				Header = SoundSection.MasterController.GetStringValue(),
				Name = SoundSection.MasterController.GetStringValue()
			};
			ListViewGroup groupReverser = new ListViewGroup
			{
				Tag = SoundSection.Reverser,
				Header = SoundSection.Reverser.GetStringValue(),
				Name = SoundSection.Reverser.GetStringValue()
			};
			ListViewGroup groupBreaker = new ListViewGroup
			{
				Tag = SoundSection.Breaker,
				Header = SoundSection.Breaker.GetStringValue(),
				Name = SoundSection.Breaker.GetStringValue()
			};
			ListViewGroup groupRequestStop = new ListViewGroup
			{
				Tag = SoundSection.RequestStop,
				Header = SoundSection.RequestStop.GetStringValue(),
				Name = SoundSection.RequestStop.GetStringValue()
			};
			ListViewGroup groupTouch = new ListViewGroup
			{
				Tag = SoundSection.Touch,
				Header = SoundSection.Touch.GetStringValue(),
				Name = SoundSection.Touch.GetStringValue()
			};
			ListViewGroup groupOthers = new ListViewGroup
			{
				Tag = SoundSection.Others,
				Header = SoundSection.Others.GetStringValue(),
				Name = SoundSection.Others.GetStringValue()
			};

			listViewSound.Groups.AddRange(new ListViewGroup[]
			{
				groupRun,
				groupFlange,
				groupMotor,
				groupFrontSwitch,
				groupRearSwitch,
				groupBrake,
				groupCompressor,
				groupSuspension,
				groupHorn,
				groupDoor,
				groupAts,
				groupBuzzer,
				groupPilotLamp,
				groupBrakeHandle,
				groupMasterController,
				groupReverser,
				groupBreaker,
				groupRequestStop,
				groupTouch,
				groupOthers
			});

			AddListViewSoundItems(groupRun, soundCfg.Run);
			AddListViewSoundItems(groupFlange, soundCfg.Flange);
			AddListViewSoundItems(groupMotor, soundCfg.Motor);
			AddListViewSoundItems(groupFrontSwitch, soundCfg.FrontSwitch);
			AddListViewSoundItems(groupRearSwitch, soundCfg.RearSwitch);

			AddListViewSoundItem(groupBrake, BrakeKey.BcReleaseHigh, soundCfg.Brake.BcReleaseHigh);
			AddListViewSoundItem(groupBrake, BrakeKey.BcRelease, soundCfg.Brake.BcRelease);
			AddListViewSoundItem(groupBrake, BrakeKey.BcReleaseFull, soundCfg.Brake.BcReleaseFull);
			AddListViewSoundItem(groupBrake, BrakeKey.Emergency, soundCfg.Brake.Emergency);
			AddListViewSoundItem(groupBrake, BrakeKey.BpDecomp, soundCfg.Brake.BpDecomp);

			AddListViewSoundItem(groupCompressor, CompressorKey.Attack, soundCfg.Compressor.Attack);
			AddListViewSoundItem(groupCompressor, CompressorKey.Loop, soundCfg.Compressor.Loop);
			AddListViewSoundItem(groupCompressor, CompressorKey.Release, soundCfg.Compressor.Release);

			AddListViewSoundItem(groupSuspension, SuspensionKey.Left, soundCfg.Suspension.Left);
			AddListViewSoundItem(groupSuspension, SuspensionKey.Right, soundCfg.Suspension.Right);

			AddListViewSoundItem(groupHorn, HornKey.PrimaryStart, soundCfg.PrimaryHorn.Start);
			AddListViewSoundItem(groupHorn, HornKey.PrimaryLoop, soundCfg.PrimaryHorn.Loop);
			AddListViewSoundItem(groupHorn, HornKey.PrimaryEnd, soundCfg.PrimaryHorn.End);

			AddListViewSoundItem(groupHorn, HornKey.SecondaryStart, soundCfg.SecondaryHorn.Start);
			AddListViewSoundItem(groupHorn, HornKey.SecondaryLoop, soundCfg.SecondaryHorn.Loop);
			AddListViewSoundItem(groupHorn, HornKey.SecondaryEnd, soundCfg.SecondaryHorn.End);

			AddListViewSoundItem(groupHorn, HornKey.MusicStart, soundCfg.MusicHorn.Start);
			AddListViewSoundItem(groupHorn, HornKey.MusicLoop, soundCfg.MusicHorn.Loop);
			AddListViewSoundItem(groupHorn, HornKey.MusicEnd, soundCfg.MusicHorn.End);

			AddListViewSoundItem(groupDoor, DoorKey.OpenLeft, soundCfg.Door.OpenLeft);
			AddListViewSoundItem(groupDoor, DoorKey.CloseLeft, soundCfg.Door.CloseLeft);

			AddListViewSoundItem(groupDoor, DoorKey.OpenRight, soundCfg.Door.OpenRight);
			AddListViewSoundItem(groupDoor, DoorKey.CloseRight, soundCfg.Door.CloseRight);

			AddListViewSoundItems(groupAts, soundCfg.Ats);
			AddListViewSoundItem(groupBuzzer, BuzzerKey.Correct, soundCfg.Buzzer.Correct);

			AddListViewSoundItem(groupPilotLamp, PilotLampKey.On, soundCfg.PilotLamp.On);
			AddListViewSoundItem(groupPilotLamp, PilotLampKey.Off, soundCfg.PilotLamp.Off);

			AddListViewSoundItem(groupBrakeHandle, BrakeHandleKey.Apply, soundCfg.BrakeHandle.Apply);
			AddListViewSoundItem(groupBrakeHandle, BrakeHandleKey.ApplyFast, soundCfg.BrakeHandle.ApplyFast);
			AddListViewSoundItem(groupBrakeHandle, BrakeHandleKey.Release, soundCfg.BrakeHandle.Release);
			AddListViewSoundItem(groupBrakeHandle, BrakeHandleKey.ReleaseFast, soundCfg.BrakeHandle.ReleaseFast);
			AddListViewSoundItem(groupBrakeHandle, BrakeHandleKey.Min, soundCfg.BrakeHandle.Min);
			AddListViewSoundItem(groupBrakeHandle, BrakeHandleKey.Max, soundCfg.BrakeHandle.Max);

			AddListViewSoundItem(groupMasterController, MasterControllerKey.Up, soundCfg.MasterController.Up);
			AddListViewSoundItem(groupMasterController, MasterControllerKey.UpFast, soundCfg.MasterController.UpFast);
			AddListViewSoundItem(groupMasterController, MasterControllerKey.Down, soundCfg.MasterController.Down);
			AddListViewSoundItem(groupMasterController, MasterControllerKey.DownFast, soundCfg.MasterController.DownFast);
			AddListViewSoundItem(groupMasterController, MasterControllerKey.Min, soundCfg.MasterController.Min);
			AddListViewSoundItem(groupMasterController, MasterControllerKey.Max, soundCfg.MasterController.Max);

			AddListViewSoundItem(groupReverser, ReverserKey.On, soundCfg.Reverser.On);
			AddListViewSoundItem(groupReverser, ReverserKey.Off, soundCfg.Reverser.Off);

			AddListViewSoundItem(groupBreaker, BreakerKey.On, soundCfg.Breaker.On);
			AddListViewSoundItem(groupBreaker, BreakerKey.Off, soundCfg.Breaker.Off);

			AddListViewSoundItem(groupRequestStop, RequestStopKey.Stop, soundCfg.RequestStop.Stop);
			AddListViewSoundItem(groupRequestStop, RequestStopKey.Pass, soundCfg.RequestStop.Pass);
			AddListViewSoundItem(groupRequestStop, RequestStopKey.Ignored, soundCfg.RequestStop.Ignored);

			AddListViewSoundItems(groupTouch, soundCfg.Touch);

			AddListViewSoundItem(groupOthers, OthersKey.Noise, soundCfg.Others.Noise);
			AddListViewSoundItem(groupOthers, OthersKey.Shoe, soundCfg.Others.Shoe);
			AddListViewSoundItem(groupOthers, OthersKey.Halt, soundCfg.Others.Halt);

			if (listViewSound.Items.Count != 0)
			{
				listViewSound.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
			}
			else
			{
				listViewSound.AutoResizeColumns(ColumnHeaderAutoResizeStyle.None);
			}
		}

		private void AddListViewSoundItem(ListViewGroup group, object key, SoundCfg.Sound sound)
		{
			if (string.IsNullOrEmpty(sound.FileName))
			{
				return;
			}

			ListViewItem item = new ListViewItem(new string[4])
			{
				Group = group
			};
			UpdateListViewSoundItem(item, key, sound, false);
			listViewSound.Items.Add(item);
		}

		private void AddListViewSoundItems(ListViewGroup group, SoundCfg.ListedSound sounds)
		{
			foreach (SoundCfg.IndexedSound sound in sounds)
			{
				AddListViewSoundItem(group, sound.Index, sound);
			}
		}

		private void UpdateListViewSoundItem(ListViewItem item, object key, SoundCfg.Sound sound, bool resizeColumns)
		{
			if (item.Tag != null && item.Tag != sound)
			{
				((SoundCfg.Sound)item.Tag).Initialize();
			}

			item.Tag = sound;
			item.SubItems[0].Tag = key;
			item.SubItems[0].Text = key is Enum ? ((Enum)key).GetStringValue() : key.ToString();
			item.SubItems[1].Text = sound.FileName;

			if (sound.IsPositionDefined)
			{
				item.SubItems[2].Text = string.Format("{0}, {1}, {2}", sound.Position.X, sound.Position.Y, sound.Position.Z);
			}
			else
			{
				item.SubItems[2].Text = string.Empty;
			}

			if (sound.IsRadiusDefined)
			{
				item.SubItems[3].Text = sound.Radius.ToString(CultureInfo.InvariantCulture);
			}
			else
			{
				item.SubItems[3].Text = string.Empty;
			}

			if (resizeColumns)
			{
				listViewSound.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
			}
		}

		private void AddOrApplySound(bool isAdditional, bool isOverride, SoundCfg.Sound srcSound, SoundSection distSection, object distKey, SoundCfg.Sound distSound)
		{
			if (isAdditional || isOverride)
			{
				if (!string.IsNullOrEmpty(distSound.FileName))
				{
					ToolTip error = new ToolTip
					{
						ToolTipIcon = ToolTipIcon.Error,
						ToolTipTitle = GetInterfaceString("sound_cfg", "status", "error"),
						IsBalloon = true
					};

					Control control = distKey is Enum ? comboBoxKey : (Control)numericUpDownKeyIndex;


					// This code is to work around ToolTip bugs. Please refer to the link below for details.
					// https://stackoverflow.com/questions/8716917/how-to-show-a-net-balloon-tooltip
					error.Show(string.Empty, control);

					error.Show(GetInterfaceString("message", "key_exist"), control, 5000);
					control.Focus();

					return;
				}
			}

			distSound.Apply(srcSound);

			ListViewItem item = isAdditional ? new ListViewItem(new string[4]) : listViewSound.FocusedItem;
			item.Group = listViewSound.Groups[distSection.GetStringValue()];

			if (isAdditional)
			{
				listViewSound.Items.Add(item);
			}

			UpdateListViewSoundItem(item, distKey, distSound, true);

			item.Selected = item.Focused = true;
		}

		private void AddOrApplySound(bool isAdditional, bool isOverride, SoundCfg.IndexedSound srcSound, SoundSection distSection, SoundCfg.ListedSound distSounds)
		{
			SoundCfg.IndexedSound distSound = distSounds.FirstOrDefault(s => s.Index == srcSound.Index);

			if (distSound == null)
			{
				distSound = new SoundCfg.IndexedSound
				{
					Index = srcSound.Index
				};

				distSounds.Add(distSound);
			}

			AddOrApplySound(isAdditional, isOverride, srcSound, distSection, srcSound.Index, distSound);
		}

		private void RemoveSound()
		{
			if (listViewSound.FocusedItem != null)
			{
				ListViewItem item = listViewSound.FocusedItem;
				SoundCfg.Sound sound = item.Tag as SoundCfg.Sound;

				if (sound != null)
				{
					sound.Initialize();
				}

				item.Selected = item.Focused = false;
				listViewSound.Items.Remove(item);
			}
		}

		private void AddOrApplySound(bool isAdditional)
		{
			if (string.IsNullOrEmpty(textBoxFileName.Text))
			{
				ToolTip error = new ToolTip
				{
					ToolTipIcon = ToolTipIcon.Error,
					ToolTipTitle = GetInterfaceString("sound_cfg", "status", "error"),
					IsBalloon = true
				};

				// This code is to work around ToolTip bugs. Please refer to the link below for details.
				// https://stackoverflow.com/questions/8716917/how-to-show-a-net-balloon-tooltip
				error.Show(string.Empty, textBoxFileName);

				error.Show(GetInterfaceString("message", "empty_filename"), textBoxFileName, 5000);
				textBoxFileName.Focus();

				return;
			}

			SoundCfg.Sound srcSound = new SoundCfg.Sound
			{
				FileName = textBoxFileName.Text,
				IsPositionDefined = checkBoxPosition.Checked,
				IsRadiusDefined = checkBoxRadius.Checked
			};

			if (srcSound.IsPositionDefined)
			{
				if (!double.TryParse(textBoxPositionX.Text, out srcSound.Position.X))
				{
					textBoxPositionX.Text = 0.0.ToString(CultureInfo.InvariantCulture);
				}

				if (!double.TryParse(textBoxPositionY.Text, out srcSound.Position.Y))
				{
					textBoxPositionY.Text = 0.0.ToString(CultureInfo.InvariantCulture);
				}

				if (!double.TryParse(textBoxPositionZ.Text, out srcSound.Position.Z))
				{
					textBoxPositionZ.Text = 0.0.ToString(CultureInfo.InvariantCulture);
				}
			}

			if (srcSound.IsRadiusDefined)
			{
				if (!double.TryParse(textBoxRadius.Text, out srcSound.Radius))
				{
					textBoxRadius.Text = 0.0.ToString(CultureInfo.InvariantCulture);
				}
			}

			bool isOverride = false;
			SoundSection newSection = (SoundSection)comboBoxSection.SelectedValue;

			if (listViewSound.FocusedItem != null)
			{
				isOverride = newSection != (SoundSection)listViewSound.FocusedItem.Group.Tag;
			}

			if (comboBoxKey.Enabled)
			{
				Enum newKey = (Enum)comboBoxKey.SelectedValue;

				if (listViewSound.FocusedItem != null)
				{
					if (listViewSound.FocusedItem.SubItems[0].Tag is Enum)
					{
						Enum oldKey = (Enum)listViewSound.FocusedItem.SubItems[0].Tag;

						if (newKey.GetType() == oldKey.GetType())
						{
							isOverride = (dynamic)newKey != (dynamic)oldKey;
						}
						else
						{
							isOverride = true;
						}
					}
					else
					{
						isOverride = true;
					}
				}

				SoundCfg.Sound distSound = null;

				switch (newSection)
				{
					case SoundSection.Brake:
						switch ((BrakeKey)newKey)
						{
							case BrakeKey.BcReleaseHigh:
								distSound = soundCfg.Brake.BcReleaseHigh;
								break;
							case BrakeKey.BcRelease:
								distSound = soundCfg.Brake.BcRelease;
								break;
							case BrakeKey.BcReleaseFull:
								distSound = soundCfg.Brake.BcReleaseFull;
								break;
							case BrakeKey.Emergency:
								distSound = soundCfg.Brake.Emergency;
								break;
							case BrakeKey.BpDecomp:
								distSound = soundCfg.Brake.BpDecomp;
								break;
						}
						break;
					case SoundSection.Compressor:
						switch ((CompressorKey)newKey)
						{
							case CompressorKey.Attack:
								distSound = soundCfg.Compressor.Attack;
								break;
							case CompressorKey.Loop:
								distSound = soundCfg.Compressor.Loop;
								break;
							case CompressorKey.Release:
								distSound = soundCfg.Compressor.Release;
								break;
						}
						break;
					case SoundSection.Suspension:
						switch ((SuspensionKey)newKey)
						{
							case SuspensionKey.Left:
								distSound = soundCfg.Suspension.Left;
								break;
							case SuspensionKey.Right:
								distSound = soundCfg.Suspension.Right;
								break;
						}
						break;
					case SoundSection.Horn:
						switch ((HornKey)newKey)
						{
							case HornKey.PrimaryStart:
								distSound = soundCfg.PrimaryHorn.Start;
								break;
							case HornKey.PrimaryLoop:
								distSound = soundCfg.PrimaryHorn.Loop;
								break;
							case HornKey.PrimaryEnd:
								distSound = soundCfg.PrimaryHorn.End;
								break;
							case HornKey.SecondaryStart:
								distSound = soundCfg.SecondaryHorn.Start;
								break;
							case HornKey.SecondaryLoop:
								distSound = soundCfg.SecondaryHorn.Loop;
								break;
							case HornKey.SecondaryEnd:
								distSound = soundCfg.SecondaryHorn.End;
								break;
							case HornKey.MusicStart:
								distSound = soundCfg.MusicHorn.Start;
								break;
							case HornKey.MusicLoop:
								distSound = soundCfg.MusicHorn.Loop;
								break;
							case HornKey.MusicEnd:
								distSound = soundCfg.MusicHorn.End;
								break;
						}
						break;
					case SoundSection.Door:
						switch ((DoorKey)newKey)
						{
							case DoorKey.OpenLeft:
								distSound = soundCfg.Door.OpenLeft;
								break;
							case DoorKey.CloseLeft:
								distSound = soundCfg.Door.CloseLeft;
								break;
							case DoorKey.OpenRight:
								distSound = soundCfg.Door.OpenRight;
								break;
							case DoorKey.CloseRight:
								distSound = soundCfg.Door.CloseRight;
								break;
						}
						break;
					case SoundSection.Buzzer:
						switch ((BuzzerKey)newKey)
						{
							case BuzzerKey.Correct:
								distSound = soundCfg.Buzzer.Correct;
								break;
						}
						break;
					case SoundSection.PilotLamp:
						switch ((PilotLampKey)newKey)
						{
							case PilotLampKey.On:
								distSound = soundCfg.PilotLamp.On;
								break;
							case PilotLampKey.Off:
								distSound = soundCfg.PilotLamp.Off;
								break;
						}
						break;
					case SoundSection.BrakeHandle:
						switch ((BrakeHandleKey)newKey)
						{
							case BrakeHandleKey.Apply:
								distSound = soundCfg.BrakeHandle.Apply;
								break;
							case BrakeHandleKey.ApplyFast:
								distSound = soundCfg.BrakeHandle.ApplyFast;
								break;
							case BrakeHandleKey.Release:
								distSound = soundCfg.BrakeHandle.Release;
								break;
							case BrakeHandleKey.ReleaseFast:
								distSound = soundCfg.BrakeHandle.ReleaseFast;
								break;
							case BrakeHandleKey.Min:
								distSound = soundCfg.BrakeHandle.Min;
								break;
							case BrakeHandleKey.Max:
								distSound = soundCfg.BrakeHandle.Max;
								break;
						}
						break;
					case SoundSection.MasterController:
						switch ((MasterControllerKey)newKey)
						{
							case MasterControllerKey.Up:
								distSound = soundCfg.MasterController.Up;
								break;
							case MasterControllerKey.UpFast:
								distSound = soundCfg.MasterController.UpFast;
								break;
							case MasterControllerKey.Down:
								distSound = soundCfg.MasterController.Down;
								break;
							case MasterControllerKey.DownFast:
								distSound = soundCfg.MasterController.DownFast;
								break;
							case MasterControllerKey.Min:
								distSound = soundCfg.MasterController.Min;
								break;
							case MasterControllerKey.Max:
								distSound = soundCfg.MasterController.Max;
								break;
						}
						break;
					case SoundSection.Reverser:
						switch ((ReverserKey)newKey)
						{
							case ReverserKey.On:
								distSound = soundCfg.Reverser.On;
								break;
							case ReverserKey.Off:
								distSound = soundCfg.Reverser.Off;
								break;
						}
						break;
					case SoundSection.Breaker:
						switch ((BreakerKey)newKey)
						{
							case BreakerKey.On:
								distSound = soundCfg.Breaker.On;
								break;
							case BreakerKey.Off:
								distSound = soundCfg.Breaker.Off;
								break;
						}
						break;
					case SoundSection.RequestStop:
						switch ((RequestStopKey)newKey)
						{
							case RequestStopKey.Stop:
								distSound = soundCfg.RequestStop.Stop;
								break;
							case RequestStopKey.Pass:
								distSound = soundCfg.RequestStop.Pass;
								break;
							case RequestStopKey.Ignored:
								distSound = soundCfg.RequestStop.Ignored;
								break;
						}
						break;
					case SoundSection.Others:
						switch ((OthersKey)newKey)
						{
							case OthersKey.Noise:
								distSound = soundCfg.Others.Noise;
								break;
							case OthersKey.Shoe:
								distSound = soundCfg.Others.Shoe;
								break;
							case OthersKey.Halt:
								distSound = soundCfg.Others.Halt;
								break;
						}
						break;
				}

				AddOrApplySound(isAdditional, isOverride, srcSound, newSection, newKey, distSound);
			}
			else if (numericUpDownKeyIndex.Enabled)
			{
				int newKeyIndex = (int)numericUpDownKeyIndex.Value;

				if (listViewSound.FocusedItem != null)
				{
					if (listViewSound.FocusedItem.SubItems[0].Tag is int)
					{
						isOverride = newKeyIndex != (int)listViewSound.FocusedItem.SubItems[0].Tag;
					}
					else
					{
						isOverride = true;
					}
				}

				SoundCfg.ListedSound distSounds = null;

				switch (newSection)
				{
					case SoundSection.Run:
						distSounds = soundCfg.Run;
						break;
					case SoundSection.Flange:
						distSounds = soundCfg.Flange;
						break;
					case SoundSection.Motor:
						distSounds = soundCfg.Motor;
						break;
					case SoundSection.FrontSwitch:
						distSounds = soundCfg.FrontSwitch;
						break;
					case SoundSection.RearSwitch:
						distSounds = soundCfg.RearSwitch;
						break;
					case SoundSection.Ats:
						distSounds = soundCfg.Ats;
						break;
					case SoundSection.Touch:
						distSounds = soundCfg.Touch;
						break;
				}

				AddOrApplySound(isAdditional, isOverride, new SoundCfg.IndexedSound(newKeyIndex, srcSound), newSection, distSounds);
			}
		}

		private void LogMessagesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (Interface.LogMessages.Any())
			{
				if (e.NewItems != null)
				{
					foreach (LogMessage message in e.NewItems)
					{
						switch (message.Type)
						{
							case MessageType.Information:
								if (!toolStripMenuItemInfo.Checked)
								{
									continue;
								}
								break;
							case MessageType.Warning:
								if (!toolStripMenuItemWarning.Checked)
								{
									continue;
								}
								break;
							case MessageType.Error:
							case MessageType.Critical:
								if (!toolStripMenuItemError.Checked)
								{
									continue;
								}
								break;
						}

						ListViewItem item = new ListViewItem(new string[2])
						{
							Tag = message,
							ImageIndex = (int)message.Type
						};

						item.SubItems[0].Text = message.Type.ToString();
						item.SubItems[1].Text = message.Text;

						listViewStatus.Items.Add(item);
					}
				}

				if (e.OldItems != null)
				{
					foreach (LogMessage message in e.OldItems)
					{
						ListViewItem[] items = listViewStatus.Items.OfType<ListViewItem>().Where(i => i.Tag == message).ToArray();

						foreach (ListViewItem item in items)
						{
							listViewStatus.Items.Remove(item);
						}
					}
				}
			}
			else
			{
				listViewStatus.Items.Clear();
			}

			toolStripMenuItemError.Text = string.Format("{0} {1}", listViewStatus.Items.OfType<ListViewItem>().Select(i => i.Tag).OfType<LogMessage>().Count(m => m.Type == MessageType.Error || m.Type == MessageType.Critical), GetInterfaceString("sound_cfg", "status", "error"));
			toolStripMenuItemWarning.Text = string.Format("{0} {1}", listViewStatus.Items.OfType<ListViewItem>().Select(i => i.Tag).OfType<LogMessage>().Count(m => m.Type == MessageType.Warning), GetInterfaceString("sound_cfg", "status", "warning"));
			toolStripMenuItemInfo.Text = string.Format("{0} {1}", listViewStatus.Items.OfType<ListViewItem>().Select(i => i.Tag).OfType<LogMessage>().Count(m => m.Type == MessageType.Information), GetInterfaceString("sound_cfg", "status", "info"));

			if (listViewStatus.Items.Count != 0)
			{
				listViewStatus.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
			}
			else
			{
				listViewStatus.AutoResizeColumns(ColumnHeaderAutoResizeStyle.None);
			}
		}

		private void ChangeLogMessagesVisible(MessageType type, bool isVisible)
		{
			if (isVisible)
			{
				LogMessage[] messages = Interface.LogMessages.Where(m => m.Type == type).ToArray();

				foreach (LogMessage message in messages)
				{
					ListViewItem item = new ListViewItem(new string[2])
					{
						Tag = message,
						ImageIndex = (int)message.Type
					};

					item.SubItems[0].Text = message.Type.ToString();
					item.SubItems[1].Text = message.Text;

					listViewStatus.Items.Add(item);
				}
			}
			else
			{
				ListViewItem[] items = listViewStatus.Items.OfType<ListViewItem>().Where(i => ((LogMessage)i.Tag).Type == type).ToArray();

				foreach (ListViewItem item in items)
				{
					listViewStatus.Items.Remove(item);
				}
			}
		}
	}
}
