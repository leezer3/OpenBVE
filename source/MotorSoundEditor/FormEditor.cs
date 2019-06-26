using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MotorSoundEditor.Parsers.Train;
using MotorSoundEditor.Systems;
using OpenBveApi.Interface;
using Timer = System.Timers.Timer;

namespace MotorSoundEditor
{
	public partial class FormEditor : Form
	{
		private string fileName;
		private TrainDat.Train train;
		private readonly Track[] tracks;
		private readonly EditState editState;
		private float minVelocity;
		private float maxVelocity;
		private float minPitch;
		private float maxPitch;
		private float minVolume;
		private float maxVolume;
		private bool isMoving;
		private Point lastMousePos;
		private SelectedRange selectedRange;
		private readonly ToolTip toolTipVertexPitch;
		private readonly ToolTip toolTipVertexVolume;
		private Vertex hoveredVertexPitch;
		private Vertex hoveredVertexVolume;
		private Area previewArea;
		private readonly ObservableCollection<TrackState> prevTrackStates;
		private readonly ObservableCollection<TrackState> nextTrackStates;
		private Track copyTrack;
		private const double hueFactor = 0.785398163397448;

		private string currentLanguageCode;

		private readonly Timer timer;
		private double oldElapsedTime;
		private int runIndex;
		private bool isPlayTrack1;
		private bool isPlayTrack2;
		private bool isLooping;
		private bool isConstant;
		private double acceleration;
		private double startSpeed;
		private double endSpeed;
		private double nowSpeed;

		public FormEditor()
		{
			tracks = new Track[4];

			for (int i = 0; i < tracks.Length; i++)
			{
				tracks[i] = new Track();
			}

			editState = new EditState(this);

			prevTrackStates = new ObservableCollection<TrackState>();
			prevTrackStates.CollectionChanged += PrevTrackStatesChanged;

			nextTrackStates = new ObservableCollection<TrackState>();
			nextTrackStates.CollectionChanged += NextTrackStatesChanged;

			InitializeComponent();

			pictureBoxDrawArea.Image = new Bitmap(pictureBoxDrawArea.Width, pictureBoxDrawArea.Height);

			toolTipVertexPitch = new ToolTip
			{
				ToolTipIcon = ToolTipIcon.Info
			};

			toolTipVertexVolume = new ToolTip
			{
				ToolTipIcon = ToolTipIcon.Info
			};

			currentLanguageCode = "en-US";

			timer = new Timer
			{
				Interval = 1000.0 / 60.0,

				// Note: Sound can not be stopped unless it is executed in the same thread as the UI thread.
				SynchronizingObject = this
			};
			timer.Elapsed += RunSimulation;
			timer.Elapsed += DrawSimulation;
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			Text = string.Format("NewFile - {0}", Application.ProductName);

			MinimumSize = Size;

			ComboBox comboBox = toolStripComboBoxIndex.ComboBox;

			if (comboBox != null)
			{
				comboBox.DrawMode = DrawMode.OwnerDrawFixed;
				comboBox.DrawItem += ToolStripComboBoxIndex_DrawItem;
				comboBox.SelectedIndexChanged += ToolStripComboBoxIndex_SelectedIndexChanged;
			}

			toolStripMenuItemSave.Enabled = false;
			toolStripMenuItemPaste.Enabled = false;
			toolStripMenuItemUndo.Enabled = false;
			toolStripMenuItemRedo.Enabled = false;

			editState.CurrentViewMode = EditState.ViewMode.Power1;

			editState.CurrentInputMode = EditState.InputMode.Pitch;

			toolStripStatusLabelX.Enabled = false;
			toolStripStatusLabelY.Enabled = false;

			toolStripComboBoxIndex.SelectedIndex = 0;

			editState.CurrentToolMode = EditState.ToolMode.Select;

			toolStripButtonPaste.Enabled = false;
			toolStripButtonUndo.Enabled = false;
			toolStripButtonRedo.Enabled = false;

			toolStripButtonNew.Image = GetImage("new.png");
			toolStripButtonOpen.Image = GetImage("open.png");
			toolStripButtonSave.Image = GetImage("save.png");
			toolStripButtonUndo.Image = GetImage("undo.png");
			toolStripButtonRedo.Image = GetImage("redo.png");
			toolStripButtonTearingOff.Image = GetImage("cut.png");
			toolStripButtonCopy.Image = GetImage("copy.png");
			toolStripButtonPaste.Image = GetImage("paste.png");
			toolStripButtonDelete.Image = GetImage("delete.png");
			toolStripButtonCleanup.Image = GetImage("cleanup.png");
			toolStripButtonSelect.Image = GetImage("select.png");
			toolStripButtonMove.Image = GetImage("move.png");
			toolStripButtonDot.Image = GetImage("draw.png");
			toolStripButtonLine.Image = GetImage("ruler.png");

			minVelocity = 0.0f;
			maxVelocity = 40.0f;

			minPitch = 0.0f;
			maxPitch = 400.0f;

			minVolume = 0.0f;
			maxVolume = 256.0f;

			textBoxMinVelocity.Text = minVelocity.ToString(CultureInfo.InvariantCulture);
			textBoxMaxVelocity.Text = maxVelocity.ToString(CultureInfo.InvariantCulture);

			textBoxMinPitch.Text = minPitch.ToString(CultureInfo.InvariantCulture);
			textBoxMaxPitch.Text = maxPitch.ToString(CultureInfo.InvariantCulture);

			textBoxMinVolume.Text = minVolume.ToString(CultureInfo.InvariantCulture);
			textBoxMaxVolume.Text = maxVolume.ToString(CultureInfo.InvariantCulture);

			buttonZoomIn.Image = GetImage("zoomin.png");
			buttonZoomOut.Image = GetImage("zoomout.png");
			buttonReset.Image = GetImage("reset.png");

			textBoxDirectX.Text = textBoxDirectY.Text = 0.0f.ToString(CultureInfo.InvariantCulture);

			buttonDirectDot.Image = GetImage("draw.png");
			buttonDirectMove.Image = GetImage("move.png");

			runIndex = -1;
			isPlayTrack1 = isPlayTrack2 = true;
			acceleration = 2.6;
			startSpeed = 0.0;
			endSpeed = 160.0;

			editState.CurrentSimuState = EditState.SimulationState.Disable;
			checkBoxTrack1.CheckState = checkBoxTrack2.CheckState = CheckState.Checked;
			checkBoxConstant.Enabled = false;
			textBoxRunIndex.Text = runIndex.ToString(CultureInfo.InvariantCulture);
			textBoxAccel.Text = acceleration.ToString(CultureInfo.InvariantCulture);
			textBoxAreaLeft.Text = startSpeed.ToString(CultureInfo.InvariantCulture);
			textBoxAreaRight.Text = endSpeed.ToString(CultureInfo.InvariantCulture);

			buttonSwap.Image = GetImage("change.png");
			buttonPlay.Image = GetImage("play.png");
			buttonPause.Image = GetImage("pause.png");
			buttonStop.Image = GetImage("stop.png");

			ActiveControl = pictureBoxDrawArea;

			Translations.CurrentLanguageCode = Interface.CurrentOptions.LanguageCode;
			string folder = Program.FileSystem.GetDataFolder("Languages");
			Translations.LoadLanguageFiles(folder);
			Translations.ListLanguages(toolStripComboBoxLanguage.ComboBox);
			ApplyLanguage();
		}

		private void Form1_Resize(object sender, EventArgs e)
		{
			if (WindowState != FormWindowState.Minimized && pictureBoxDrawArea.Width != 0 && pictureBoxDrawArea.Height != 0)
			{
				pictureBoxDrawArea.Image = new Bitmap(pictureBoxDrawArea.Width, pictureBoxDrawArea.Height);
			}

			DrawPictureBoxDrawArea();
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			switch (MessageBox.Show(GetInterfaceString("message", "exit"), Application.ProductName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
			{
				case DialogResult.Cancel:
					e.Cancel = true;
					return;
				case DialogResult.Yes:
					SaveFile();
					break;
			}

			editState.CurrentSimuState = EditState.SimulationState.Stopped;

			Interface.CurrentOptions.LanguageCode = currentLanguageCode;
		}

		private void ToolStripMenuItemNew_Click(object sender, EventArgs e)
		{
			NewFile();
		}

		private void ToolStripMenuItemOpen_Click(object sender, EventArgs e)
		{
			OpenFile();
		}

		private void ToolStripMenuItemSave_Click(object sender, EventArgs e)
		{
			SaveFile();
		}

		private void ToolStripMenuItemSaveAs_Click(object sender, EventArgs e)
		{
			SaveAsFile();
		}

		private void ToolStripMenuItemExit_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void ToolStripMenuItemUndo_Click(object sender, EventArgs e)
		{
			Undo();
		}

		private void ToolStripMenuItemRedo_Click(object sender, EventArgs e)
		{
			Redo();
		}

		private void ToolStripMenuItemTearingOff_Click(object sender, EventArgs e)
		{
			TearingOff();
		}

		private void ToolStripMenuItemCopy_Click(object sender, EventArgs e)
		{
			Copy();
		}

		private void ToolStripMenuItemPaste_Click(object sender, EventArgs e)
		{
			Paste();
		}

		private void ToolStripMenuItemCleanup_Click(object sender, EventArgs e)
		{
			Cleanup();
		}

		private void ToolStripMenuItemDelete_Click(object sender, EventArgs e)
		{
			Delete();
		}

		private void ToolStripMenuItemPowerTrack1_Click(object sender, EventArgs e)
		{
			editState.CurrentViewMode = EditState.ViewMode.Power1;
		}

		private void ToolStripMenuItemPowerTrack2_Click(object sender, EventArgs e)
		{
			editState.CurrentViewMode = EditState.ViewMode.Power2;
		}

		private void ToolStripMenuItemBrakeTrack1_Click(object sender, EventArgs e)
		{
			editState.CurrentViewMode = EditState.ViewMode.Brake1;
		}

		private void ToolStripMenuItemBrakeTrack2_Click(object sender, EventArgs e)
		{
			editState.CurrentViewMode = EditState.ViewMode.Brake2;
		}

		private void ToolStripMenuItemPitch_Click(object sender, EventArgs e)
		{
			editState.CurrentInputMode = EditState.InputMode.Pitch;
		}

		private void ToolStripMenuItemVolume_Click(object sender, EventArgs e)
		{
			editState.CurrentInputMode = EditState.InputMode.Volume;
		}

		private void ToolStripMenuItemIndex_Click(object sender, EventArgs e)
		{
			editState.CurrentInputMode = EditState.InputMode.SoundIndex;
		}

		private void ToolStripComboBoxIndex_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (editState.CurrentInputMode == EditState.InputMode.SoundIndex)
			{
				// Display update
				editState.CurrentInputMode = editState.CurrentInputMode;
			}
		}

		private void ToolStripComboBoxIndex_DrawItem(object sender, DrawItemEventArgs e)
		{
			e.DrawBackground();

			if (e.Index == -1)
			{
				return;
			}

			Color c;

			if (e.Index - 1 >= 0)
			{
				double hue = hueFactor * (e.Index - 1);
				hue -= Math.Floor(hue);
				c = GetColor(hue, false);
			}
			else
			{
				c = Color.FromArgb((int)Math.Round(Color.Silver.R * 0.6), (int)Math.Round(Color.Silver.G * 0.6), (int)Math.Round(Color.Silver.B * 0.6));
			}

			e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
			e.Graphics.FillRectangle(new SolidBrush(c), e.Bounds.X, e.Bounds.Y, e.Bounds.Height, e.Bounds.Height);
			e.Graphics.DrawString(toolStripComboBoxIndex.Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds.X + e.Bounds.Height + 10, e.Bounds.Y);
		}

		private void ToolStripMenuItemSelect_Click(object sender, EventArgs e)
		{
			editState.CurrentToolMode = EditState.ToolMode.Select;
		}

		private void ToolStripMenuItemMove_Click(object sender, EventArgs e)
		{
			editState.CurrentToolMode = EditState.ToolMode.Move;
		}

		private void ToolStripMenuItemDot_Click(object sender, EventArgs e)
		{
			editState.CurrentToolMode = EditState.ToolMode.Dot;
		}

		private void ToolStripMenuItemLine_Click(object sender, EventArgs e)
		{
			editState.CurrentToolMode = EditState.ToolMode.Line;
		}

		private void ToolStripComboBoxLanguage_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (Translations.SelectedLanguage(ref currentLanguageCode, toolStripComboBoxLanguage.ComboBox))
			{
				ApplyLanguage();
			}
		}

		private void ToolStripButtonNew_Click(object sender, EventArgs e)
		{
			NewFile();
		}

		private void ToolStripButtonOpen_Click(object sender, EventArgs e)
		{
			OpenFile();
		}

		private void ToolStripButtonSave_Click(object sender, EventArgs e)
		{
			SaveFile();
		}

		private void ToolStripButtonUndo_Click(object sender, EventArgs e)
		{
			Undo();
		}

		private void ToolStripButtonRedo_Click(object sender, EventArgs e)
		{
			Redo();
		}

		private void ToolStripButtonTearingOff_Click(object sender, EventArgs e)
		{
			TearingOff();
		}

		private void ToolStripButtonCopy_Click(object sender, EventArgs e)
		{
			Copy();
		}

		private void ToolStripButtonPaste_Click(object sender, EventArgs e)
		{
			Paste();
		}

		private void ToolStripButtonCleanup_Click(object sender, EventArgs e)
		{
			Cleanup();
		}

		private void ToolStripButtonDelete_Click(object sender, EventArgs e)
		{
			Delete();
		}

		private void ToolStripButtonSelect_Click(object sender, EventArgs e)
		{
			editState.CurrentToolMode = EditState.ToolMode.Select;
		}

		private void ToolStripButtonMove_Click(object sender, EventArgs e)
		{
			editState.CurrentToolMode = EditState.ToolMode.Move;
		}

		private void ToolStripButtonDot_Click(object sender, EventArgs e)
		{
			editState.CurrentToolMode = EditState.ToolMode.Dot;
		}

		private void ToolStripButtonLine_Click(object sender, EventArgs e)
		{
			editState.CurrentToolMode = EditState.ToolMode.Line;
		}

		private void TextBoxMinVelocity_TextChanged(object sender, EventArgs e)
		{
			float result;

			if (float.TryParse(textBoxMinVelocity.Text, out result))
			{
				if (result >= 0.0f && result < maxVelocity)
				{
					minVelocity = result;
					DrawPictureBoxDrawArea();
				}
			}
		}

		private void TextBoxMaxVelocity_TextChanged(object sender, EventArgs e)
		{
			float result;

			if (float.TryParse(textBoxMaxVelocity.Text, out result))
			{
				if (result > minVelocity)
				{
					maxVelocity = result;
					DrawPictureBoxDrawArea();
				}
			}
		}

		private void TextBoxMinPitch_TextChanged(object sender, EventArgs e)
		{
			float result;

			if (float.TryParse(textBoxMinPitch.Text, out result))
			{
				if (result >= 0.0f && result < maxPitch)
				{
					minPitch = result;
					DrawPictureBoxDrawArea();
				}
			}
		}

		private void TextBoxMaxPitch_TextChanged(object sender, EventArgs e)
		{
			float result;

			if (float.TryParse(textBoxMaxPitch.Text, out result))
			{
				if (result > minPitch)
				{
					maxPitch = result;
					DrawPictureBoxDrawArea();
				}
			}
		}

		private void TextBoxMinVolume_TextChanged(object sender, EventArgs e)
		{
			float result;

			if (float.TryParse(textBoxMinVolume.Text, out result))
			{
				if (result >= 0.0f && result < maxVolume)
				{
					minVolume = result;
					DrawPictureBoxDrawArea();
				}
			}
		}

		private void TextBoxMaxVolume_TextChanged(object sender, EventArgs e)
		{
			float result;

			if (float.TryParse(textBoxMaxVolume.Text, out result))
			{
				if (result > minVolume)
				{
					maxVolume = result;
					DrawPictureBoxDrawArea();
				}
			}
		}

		private void ButtonZoomIn_Click(object sender, EventArgs e)
		{
			float rangeVelocity = maxVelocity - minVelocity;
			float rangePitch = maxPitch - minPitch;
			float rangeVolume = maxVolume - minVolume;

			float centerVelocity = 0.5f * (minVelocity + maxVelocity);
			float centerPitch = 0.5f * (minPitch + maxPitch);
			float centerVolume = 0.5f * (minVolume + maxVolume);

			float radiusVelocity = 0.5f * rangeVelocity;
			float radiusPitch = 0.5f * rangePitch;
			float radiusVolume = 0.5f * rangeVolume;

			minVelocity = centerVelocity - 0.8f * radiusVelocity;
			maxVelocity = centerVelocity + 0.8f * radiusVelocity;

			switch (editState.CurrentInputMode)
			{
				case EditState.InputMode.Pitch:
					minPitch = centerPitch - 0.8f * radiusPitch;
					maxPitch = centerPitch + 0.8f * radiusPitch;
					break;
				case EditState.InputMode.Volume:
					minVolume = centerVolume - 0.8f * radiusVolume;
					maxVolume = centerVolume + 0.8f * radiusVolume;
					break;
			}

			textBoxMinVelocity.Text = minVelocity.ToString(CultureInfo.InvariantCulture);
			textBoxMaxVelocity.Text = maxVelocity.ToString(CultureInfo.InvariantCulture);

			textBoxMinPitch.Text = minPitch.ToString(CultureInfo.InvariantCulture);
			textBoxMaxPitch.Text = maxPitch.ToString(CultureInfo.InvariantCulture);

			textBoxMinVolume.Text = minVolume.ToString(CultureInfo.InvariantCulture);
			textBoxMaxVolume.Text = maxVolume.ToString(CultureInfo.InvariantCulture);

			DrawPictureBoxDrawArea();
		}

		private void ButtonZoomOut_Click(object sender, EventArgs e)
		{
			float rangeVelocity = maxVelocity - minVelocity;
			float rangePitch = maxPitch - minPitch;
			float rangeVolume = maxVolume - minVolume;

			float centerVelocity = 0.5f * (minVelocity + maxVelocity);
			float centerPitch = 0.5f * (minPitch + maxPitch);
			float centerVolume = 0.5f * (minVolume + maxVolume);

			float radiusVelocity = 0.5f * rangeVelocity;
			float radiusPitch = 0.5f * rangePitch;
			float radiusVolume = 0.5f * rangeVolume;

			minVelocity = centerVelocity - 1.25f * radiusVelocity;

			if (minVelocity < 0.0f)
			{
				minVelocity = 0.0f;
				maxVelocity = 2.25f * radiusVelocity;
			}
			else
			{
				maxVelocity = centerVelocity + 1.25f * radiusVelocity;
			}

			switch (editState.CurrentInputMode)
			{
				case EditState.InputMode.Pitch:
					minPitch = centerPitch - 1.25f * radiusPitch;

					if (minPitch < 0.0f)
					{
						minPitch = 0.0f;
						maxPitch = 2.25f * radiusPitch;
					}
					else
					{
						maxPitch = centerPitch + 1.25f * radiusPitch;
					}
					break;
				case EditState.InputMode.Volume:
					minVolume = centerVolume - 1.25f * radiusVolume;

					if (minVolume < 0.0f)
					{
						minVolume = 0.0f;
						maxVolume = 2.25f * radiusVolume;
					}
					else
					{
						maxVolume = centerVolume + 1.25f * radiusVolume;
					}
					break;
			}

			textBoxMinVelocity.Text = minVelocity.ToString(CultureInfo.InvariantCulture);
			textBoxMaxVelocity.Text = maxVelocity.ToString(CultureInfo.InvariantCulture);

			textBoxMinPitch.Text = minPitch.ToString(CultureInfo.InvariantCulture);
			textBoxMaxPitch.Text = maxPitch.ToString(CultureInfo.InvariantCulture);

			textBoxMinVolume.Text = minVolume.ToString(CultureInfo.InvariantCulture);
			textBoxMaxVolume.Text = maxVolume.ToString(CultureInfo.InvariantCulture);

			DrawPictureBoxDrawArea();
		}

		private void ButtonReset_Click(object sender, EventArgs e)
		{
			float centerVelocity = 0.5f * (minVelocity + maxVelocity);
			float centerPitch = 0.5f * (minPitch + maxPitch);
			float centerVolume = 0.5f * (minVolume + maxVolume);

			float radiusVelocity = 0.5f * 40.0f;
			float radiusPitch = 0.5f * 400.0f;
			float radiusVolume = 0.5f * 256.0f;

			minVelocity = centerVelocity - radiusVelocity;

			if (minVelocity < 0.0f)
			{
				minVelocity = 0.0f;
			}

			maxVelocity = minVelocity + 2.0f * radiusVelocity;

			switch (editState.CurrentInputMode)
			{
				case EditState.InputMode.Pitch:
					minPitch = centerPitch - radiusPitch;

					if (minPitch < 0.0f)
					{
						minPitch = 0.0f;
					}

					maxPitch = minPitch + 2.0f * radiusPitch;
					break;
				case EditState.InputMode.Volume:
					minVolume = centerVolume - radiusVolume;

					if (minVolume < 0.0f)
					{
						minVolume = 0.0f;
					}

					maxVolume = minVolume + 2.0f * radiusVolume;
					break;
			}

			textBoxMinVelocity.Text = minVelocity.ToString(CultureInfo.InvariantCulture);
			textBoxMaxVelocity.Text = maxVelocity.ToString(CultureInfo.InvariantCulture);

			textBoxMinPitch.Text = minPitch.ToString(CultureInfo.InvariantCulture);
			textBoxMaxPitch.Text = maxPitch.ToString(CultureInfo.InvariantCulture);

			textBoxMinVolume.Text = minVolume.ToString(CultureInfo.InvariantCulture);
			textBoxMaxVolume.Text = maxVolume.ToString(CultureInfo.InvariantCulture);

			DrawPictureBoxDrawArea();
		}

		private void ButtonDirectDot_Click(object sender, EventArgs e)
		{
			float x, y;

			if (float.TryParse(textBoxDirectX.Text, out x) && float.TryParse(textBoxDirectY.Text, out y))
			{
				x = Math.Max(0.2f * (float)Math.Round(5.0 * x), 0.0f);
				y = Math.Max(0.01f * (float)Math.Round(100.0 * y), 0.0f);

				bool exist = false;

				switch (editState.CurrentInputMode)
				{
					case EditState.InputMode.Pitch:
						exist = tracks[(int)editState.CurrentViewMode].PitchVertices.Any(v => v.Value.X == x);
						break;
					case EditState.InputMode.Volume:
						exist = tracks[(int)editState.CurrentViewMode].VolumeVertices.Any(v => v.Value.X == x);
						break;
				}

				if (exist)
				{
					switch (MessageBox.Show(GetInterfaceString("message", "vertex_exist"), GetInterfaceString("direct_input", "dot"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
					{
						case DialogResult.Yes:
							break;
						default:
							return;
					}
				}

				DrawDot(x, y, y);
			}
			else
			{
				x = y = 0.0f;
			}

			textBoxDirectX.Text = x.ToString(CultureInfo.InvariantCulture);
			textBoxDirectY.Text = y.ToString(CultureInfo.InvariantCulture);
		}

		private void ButtonDirectMove_Click(object sender, EventArgs e)
		{
			float x, y;

			if (float.TryParse(textBoxDirectX.Text, out x) && float.TryParse(textBoxDirectY.Text, out y))
			{
				x = 0.2f * (float)Math.Round(5.0 * x);
				y = 0.01f * (float)Math.Round(100.0 * y);

				switch (editState.CurrentInputMode)
				{
					case EditState.InputMode.Pitch:
						MoveDot(tracks[(int)editState.CurrentViewMode].PitchVertices, x, y);
						break;
					case EditState.InputMode.Volume:
						MoveDot(tracks[(int)editState.CurrentViewMode].VolumeVertices, x, y);
						break;
				}

				DrawPictureBoxDrawArea();
			}
			else
			{
				x = y = 0.0f;
			}

			textBoxDirectX.Text = x.ToString(CultureInfo.InvariantCulture);
			textBoxDirectY.Text = y.ToString(CultureInfo.InvariantCulture);
		}

		private void TextBoxRunIndex_TextChanged(object sender, EventArgs e)
		{
			int result;

			if (int.TryParse(textBoxRunIndex.Text, out result))
			{
				if (result >= -1)
				{
					runIndex = result;
				}
			}
		}

		private void CheckBoxTrack1_CheckedChanged(object sender, EventArgs e)
		{
			isPlayTrack1 = checkBoxTrack1.Checked;
		}

		private void CheckBoxTrack2_CheckedChanged(object sender, EventArgs e)
		{
			isPlayTrack2 = checkBoxTrack2.Checked;
		}

		private void CheckBoxLoop_CheckedChanged(object sender, EventArgs e)
		{
			isLooping = checkBoxLoop.Checked;

			checkBoxConstant.Enabled = isLooping;
		}

		private void CheckBoxConstant_CheckedChanged(object sender, EventArgs e)
		{
			isConstant = checkBoxConstant.Checked;
		}

		private void TextBoxAccel_TextChanged(object sender, EventArgs e)
		{
			double result;

			if (double.TryParse(textBoxAccel.Text, out result))
			{
				if (result >= 0.0)
				{
					acceleration = result;
				}
			}
		}

		private void TextBoxAreaLeft_TextChanged(object sender, EventArgs e)
		{
			double result;

			if (double.TryParse(textBoxAreaLeft.Text, out result))
			{
				if (result >= 0.0)
				{
					startSpeed = result;
				}
			}
		}

		private void TextBoxAreaRight_TextChanged(object sender, EventArgs e)
		{
			double result;

			if (double.TryParse(textBoxAreaRight.Text, out result))
			{
				if (result >= 0.0)
				{
					endSpeed = result;
				}
			}
		}

		private void ButtonSwap_Click(object sender, EventArgs e)
		{
			string tmp = textBoxAreaLeft.Text;
			textBoxAreaLeft.Text = textBoxAreaRight.Text;
			textBoxAreaRight.Text = tmp;
		}

		private void ButtonPlay_Click(object sender, EventArgs e)
		{
			try
			{
				CreateCar(fileName, TrackToMotor(tracks[(int)EditState.ViewMode.Power1]), TrackToMotor(tracks[(int)EditState.ViewMode.Power2]), TrackToMotor(tracks[(int)EditState.ViewMode.Brake1]), TrackToMotor(tracks[(int)EditState.ViewMode.Brake2]));
			}
			catch (Exception exception)
			{
				editState.CurrentSimuState = EditState.SimulationState.Disable;
				return;
			}

			editState.ChangeEditStatus(false);

			editState.ChangeToolsStatus(false);

			bool isPaused = editState.CurrentSimuState == EditState.SimulationState.Paused;

			editState.CurrentSimuState = EditState.SimulationState.Started;

			textBoxAccel.Text = acceleration.ToString(CultureInfo.InvariantCulture);
			textBoxAreaLeft.Text = startSpeed.ToString(CultureInfo.InvariantCulture);
			textBoxAreaRight.Text = endSpeed.ToString(CultureInfo.InvariantCulture);

			StartSimulation(isPaused);
		}

		private void ButtonPause_Click(object sender, EventArgs e)
		{
			StopSimulation();
			DisposeCar();

			editState.CurrentSimuState = EditState.SimulationState.Paused;
		}

		private void ButtonStop_Click(object sender, EventArgs e)
		{
			StopSimulation();
			DisposeCar();

			editState.CurrentSimuState = EditState.SimulationState.Stopped;

			editState.ChangeEditStatus(true);

			editState.ChangeToolsStatus(true);

			DrawPictureBoxDrawArea();
		}

		private void PictureBoxDrawArea_MouseEnter(object sender, EventArgs e)
		{
			pictureBoxDrawArea.Focus();
			toolStripStatusLabelX.Enabled = true;

			if (editState.CurrentInputMode != EditState.InputMode.SoundIndex)
			{
				toolStripStatusLabelY.Enabled = true;
			}
		}

		private void PictureBoxDrawArea_MouseLeave(object sender, EventArgs e)
		{
			toolStripStatusLabelX.Enabled = false;

			if (editState.CurrentInputMode != EditState.InputMode.SoundIndex)
			{
				toolStripStatusLabelY.Enabled = false;
			}
		}

		private void PictureBoxDrawArea_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			float rangeVelocity = maxVelocity - minVelocity;
			float rangePitch = maxPitch - minPitch;
			float rangeVolume = maxVolume - minVolume;

			switch (e.KeyData)
			{
				case Keys.Left:
					minVelocity -= 0.1f * rangeVelocity;

					if (minVelocity < 0.0f)
					{
						minVelocity = 0.0f;
					}

					e.IsInputKey = true;
					break;
				case Keys.Right:
					minVelocity += 0.1f * rangeVelocity;
					e.IsInputKey = true;
					break;
				case Keys.Up:
					switch (editState.CurrentInputMode)
					{
						case EditState.InputMode.Pitch:
							minPitch += 0.1f * rangePitch;
							break;
						case EditState.InputMode.Volume:
							minVolume += 0.1f * rangeVolume;
							break;
					}

					e.IsInputKey = true;
					break;
				case Keys.Down:
					switch (editState.CurrentInputMode)
					{
						case EditState.InputMode.Pitch:
							minPitch -= 0.1f * rangePitch;

							if (minPitch < 0.0f)
							{
								minPitch = 0.0f;
							}

							break;
						case EditState.InputMode.Volume:
							minVolume -= 0.1f * rangeVolume;

							if (minVolume < 0.0f)
							{
								minVolume = 0.0f;
							}

							break;
					}

					e.IsInputKey = true;
					break;
			}

			maxVelocity = minVelocity + rangeVelocity;
			maxPitch = minPitch + rangePitch;
			maxVolume = minVolume + rangeVolume;

			textBoxMinVelocity.Text = minVelocity.ToString(CultureInfo.InvariantCulture);
			textBoxMaxVelocity.Text = maxVelocity.ToString(CultureInfo.InvariantCulture);

			textBoxMinPitch.Text = minPitch.ToString(CultureInfo.InvariantCulture);
			textBoxMaxPitch.Text = maxPitch.ToString(CultureInfo.InvariantCulture);

			textBoxMinVolume.Text = minVolume.ToString(CultureInfo.InvariantCulture);
			textBoxMaxVolume.Text = maxVolume.ToString(CultureInfo.InvariantCulture);

			DrawPictureBoxDrawArea();
		}

		private void PictureBoxDrawArea_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				lastMousePos = e.Location;

				if (editState.CurrentInputMode != EditState.InputMode.SoundIndex)
				{
					switch (editState.CurrentToolMode)
					{
						case EditState.ToolMode.Select:
							SelectDotLine(e);
							break;
						case EditState.ToolMode.Dot:
							DrawDot(e);
							break;
						case EditState.ToolMode.Line:
							DrawLine(e);
							break;
					}
				}
			}
		}

		private void PictureBoxDrawArea_MouseMove(object sender, MouseEventArgs e)
		{
			{
				float velocity = 0.2f * (float)Math.Round(5.0 * XtoVelocity(e.X));
				float pitch = 0.01f * (float)Math.Round(100.0 * YtoPitch(e.Y));
				float volume = 0.01f * (float)Math.Round(100.0 * YtoVolume(e.Y));

				toolStripStatusLabelX.Text = string.Format("{0} {1} km/h", GetInterfaceString("status_xy", "velocity"), velocity.ToString("0.00", CultureInfo.InvariantCulture));

				switch (editState.CurrentInputMode)
				{
					case EditState.InputMode.Pitch:
						toolStripStatusLabelY.Text = string.Format("{0} {1} ", GetInterfaceString("status_xy", "pitch"), pitch.ToString("0.00", CultureInfo.InvariantCulture));
						break;
					case EditState.InputMode.Volume:
						toolStripStatusLabelY.Text = string.Format("{0} {1} ", GetInterfaceString("status_xy", "volume"), volume.ToString("0.00", CultureInfo.InvariantCulture));
						break;
				}

				Func<float, float, Vertex, bool> conditionVertex = (x, y, v) => v.X - 0.2f < x && x < v.X + 0.2f && v.Y - 2.0f < y && y < v.Y + 2.0f;

				if (editState.CurrentInputMode != EditState.InputMode.Volume)
				{
					Vertex newHoveredVertex = tracks[(int)editState.CurrentViewMode].PitchVertices.Values.FirstOrDefault(v => conditionVertex(velocity, pitch, v));

					if (newHoveredVertex != hoveredVertexPitch)
					{
						if (newHoveredVertex != null)
						{
							Area area = tracks[(int)editState.CurrentViewMode].SoundIndices.Find(a => a.LeftX <= newHoveredVertex.X && a.RightX >= newHoveredVertex.X);

							StringBuilder builder = new StringBuilder();
							builder.AppendFormat("{0} {1} km/h", GetInterfaceString("vertex_info", "velocity"), newHoveredVertex.X.ToString("0.00", CultureInfo.InvariantCulture));
							builder.AppendLine();
							builder.AppendFormat("{0} {1}", GetInterfaceString("vertex_info", "pitch"), newHoveredVertex.Y.ToString("0.00", CultureInfo.InvariantCulture));
							builder.AppendLine();
							builder.AppendFormat("{0} {1}", GetInterfaceString("vertex_info", "sound_index"), area != null ? area.Index : -1);

							toolTipVertexPitch.ToolTipTitle = GetInterfaceString("vertex_info", "vertex_info");
							toolTipVertexPitch.Show(builder.ToString(), pictureBoxDrawArea, (int)VelocityToX(newHoveredVertex.X) + 10, (int)PitchToY(newHoveredVertex.Y) + 10);
						}
						else
						{
							toolTipVertexPitch.Hide(pictureBoxDrawArea);
						}

						hoveredVertexPitch = newHoveredVertex;
					}
				}

				if (editState.CurrentInputMode != EditState.InputMode.Pitch)
				{
					Vertex newHoveredVertex = tracks[(int)editState.CurrentViewMode].VolumeVertices.Values.FirstOrDefault(v => conditionVertex(velocity, volume, v));

					if (newHoveredVertex != hoveredVertexVolume)
					{
						if (newHoveredVertex != null)
						{
							Area area = tracks[(int)editState.CurrentViewMode].SoundIndices.Find(a => a.LeftX <= newHoveredVertex.X && a.RightX >= newHoveredVertex.X);

							StringBuilder builder = new StringBuilder();
							builder.AppendFormat("{0} {1} km/h", GetInterfaceString("vertex_info", "velocity"), newHoveredVertex.X.ToString("0.00", CultureInfo.InvariantCulture));
							builder.AppendLine();
							builder.AppendFormat("{0} {1}", GetInterfaceString("vertex_info", "volume"), newHoveredVertex.Y.ToString("0.00", CultureInfo.InvariantCulture));
							builder.AppendLine();
							builder.AppendFormat("{0} {1}", GetInterfaceString("vertex_info", "sound_index"), area != null ? area.Index : -1);

							toolTipVertexVolume.ToolTipTitle = GetInterfaceString("vertex_info", "vertex_info");
							toolTipVertexVolume.Show(builder.ToString(), pictureBoxDrawArea, (int)VelocityToX(newHoveredVertex.X) + 10, (int)VolumeToY(newHoveredVertex.Y) + 10);
						}
						else
						{
							toolTipVertexVolume.Hide(pictureBoxDrawArea);
						}

						hoveredVertexVolume = newHoveredVertex;
					}
				}


				if (editState.CurrentInputMode != EditState.InputMode.SoundIndex)
				{
					switch (editState.CurrentToolMode)
					{
						case EditState.ToolMode.Select:
						case EditState.ToolMode.Line:
							switch (editState.CurrentInputMode)
							{
								case EditState.InputMode.Pitch:
									if (IsSelectDotLine(tracks[(int)editState.CurrentViewMode].PitchVertices, tracks[(int)editState.CurrentViewMode].PitchLines, velocity, pitch))
									{
										if (editState.CurrentToolMode == EditState.ToolMode.Select || IsDrawLine(tracks[(int)editState.CurrentViewMode].PitchVertices, tracks[(int)editState.CurrentViewMode].PitchLines, velocity, pitch))
										{
											pictureBoxDrawArea.Cursor = Cursors.Hand;
										}
										else
										{
											pictureBoxDrawArea.Cursor = Cursors.No;
										}
									}
									else
									{
										pictureBoxDrawArea.Cursor = Cursors.Arrow;
									}
									break;
								case EditState.InputMode.Volume:
									if (IsSelectDotLine(tracks[(int)editState.CurrentViewMode].VolumeVertices, tracks[(int)editState.CurrentViewMode].VolumeLines, velocity, volume))
									{
										if (editState.CurrentToolMode == EditState.ToolMode.Select || IsSelectDotLine(tracks[(int)editState.CurrentViewMode].VolumeVertices, tracks[(int)editState.CurrentViewMode].VolumeLines, velocity, volume))
										{
											pictureBoxDrawArea.Cursor = Cursors.Hand;
										}
										else
										{
											pictureBoxDrawArea.Cursor = Cursors.No;
										}
									}
									else
									{
										pictureBoxDrawArea.Cursor = Cursors.Arrow;
									}
									break;
							}
							break;
						case EditState.ToolMode.Move:
							pictureBoxDrawArea.Cursor = Cursors.NoMove2D;
							break;
						case EditState.ToolMode.Dot:
							pictureBoxDrawArea.Cursor = Cursors.Cross;
							break;
					}
				}
				else
				{
					pictureBoxDrawArea.Cursor = Cursors.Cross;
				}
			}

			if (e.Button == MouseButtons.Left)
			{
				Point delta = new Point(e.Location.X - lastMousePos.X, e.Location.Y - lastMousePos.Y);

				int width = pictureBoxDrawArea.ClientRectangle.Width;
				int height = pictureBoxDrawArea.ClientRectangle.Height;

				float factorVelocity = width / (maxVelocity - minVelocity);
				float factorPitch = -height / (maxPitch - minPitch);
				float factorVolume = -height / (maxVolume - minVolume);

				float deltaVelocity = 0.2f * (float)Math.Round(5.0 * delta.X / factorVelocity);
				float deltaPitch = 0.01f * (float)Math.Round(100.0 * delta.Y / factorPitch);
				float deltaVolume = 0.01f * (float)Math.Round(100.0 * delta.Y / factorVolume);

				switch (editState.CurrentInputMode)
				{
					case EditState.InputMode.Pitch:
						switch (editState.CurrentToolMode)
						{
							case EditState.ToolMode.Select:
								{
									float lastVelocity = 0.2f * (float)Math.Round(5.0 * XtoVelocity(lastMousePos.X));
									float velocity = 0.2f * (float)Math.Round(5.0 * XtoVelocity(e.Location.X));

									float lastPitch = 0.01f * (float)Math.Round(100.0 * YtoPitch(lastMousePos.Y));
									float pitch = 0.01f * (float)Math.Round(100.0 * YtoPitch(e.Location.Y));

									float leftX = Math.Min(lastVelocity, velocity);
									float rightX = Math.Max(lastVelocity, velocity);

									float topY = Math.Max(lastPitch, pitch);
									float bottomY = Math.Min(lastPitch, pitch);

									if (velocity != lastVelocity && pitch != lastPitch)
									{
										selectedRange = SelectedRange.CreateSelectedRange(tracks[(int)editState.CurrentViewMode].PitchVertices, tracks[(int)editState.CurrentViewMode].PitchLines, leftX, rightX, topY, bottomY);
									}
									else
									{
										selectedRange = null;
									}
								}
								break;
							case EditState.ToolMode.Move:
								MoveDot(tracks[(int)editState.CurrentViewMode].PitchVertices, deltaVelocity, deltaPitch);
								break;
						}
						break;
					case EditState.InputMode.Volume:
						switch (editState.CurrentToolMode)
						{
							case EditState.ToolMode.Select:
								{
									float lastVelocity = 0.2f * (float)Math.Round(5.0 * XtoVelocity(lastMousePos.X));
									float velocity = 0.2f * (float)Math.Round(5.0 * XtoVelocity(e.Location.X));

									float lastVolume = 0.01f * (float)Math.Round(100.0 * YtoVolume(lastMousePos.Y));
									float volume = 0.01f * (float)Math.Round(100.0 * YtoVolume(e.Location.Y));

									float leftX = Math.Min(lastVelocity, velocity);
									float rightX = Math.Max(lastVelocity, velocity);

									float topY = Math.Max(lastVolume, volume);
									float bottomY = Math.Min(lastVolume, volume);

									if (velocity != lastVelocity && volume != lastVolume)
									{
										selectedRange = SelectedRange.CreateSelectedRange(tracks[(int)editState.CurrentViewMode].VolumeVertices, tracks[(int)editState.CurrentViewMode].VolumeLines, leftX, rightX, topY, bottomY);
									}
									else
									{
										selectedRange = null;
									}
								}
								break;
							case EditState.ToolMode.Move:
								MoveDot(tracks[(int)editState.CurrentViewMode].VolumeVertices, deltaVelocity, deltaVolume);
								break;
						}
						break;
					case EditState.InputMode.SoundIndex:
						{
							float lastVelocity = 0.2f * (float)Math.Round(5.0 * XtoVelocity(lastMousePos.X));
							float velocity = 0.2f * (float)Math.Round(5.0 * XtoVelocity(e.Location.X));

							if (velocity != lastVelocity)
							{
								previewArea = new Area(Math.Min(lastVelocity, velocity), Math.Max(lastVelocity, velocity), toolStripComboBoxIndex.SelectedIndex - 1);
							}
							else
							{
								previewArea = null;
							}
						}
						break;
				}

				if (editState.CurrentInputMode != EditState.InputMode.SoundIndex && editState.CurrentToolMode != EditState.ToolMode.Select)
				{
					lastMousePos = e.Location;
				}
			}

			DrawPictureBoxDrawArea();
		}

		private void PictureBoxDrawArea_MouseUp(object sender, MouseEventArgs e)
		{
			if (editState.CurrentInputMode != EditState.InputMode.SoundIndex)
			{
				isMoving = false;

				if (editState.CurrentToolMode == EditState.ToolMode.Select)
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
					prevTrackStates.Add(new TrackState(editState.CurrentViewMode, tracks[(int)editState.CurrentViewMode].Clone()));

					List<Area> addAreas = new List<Area>();

					foreach (Area area in tracks[(int)editState.CurrentViewMode].SoundIndices)
					{
						if (area.RightX < previewArea.LeftX || area.LeftX > previewArea.RightX)
						{
							continue;
						}

						if (area.LeftX < previewArea.LeftX && area.RightX > previewArea.RightX)
						{
							if (area.Index != previewArea.Index)
							{
								addAreas.Add(new Area(area.LeftX, previewArea.LeftX - 0.2f, area.Index));
								addAreas.Add(new Area(previewArea.RightX + 0.2f, area.RightX, area.Index));
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
								area.RightX = previewArea.LeftX - 0.2f;
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
								area.LeftX = previewArea.RightX + 0.2f;
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

					tracks[(int)editState.CurrentViewMode].SoundIndices.Add(previewArea);
					tracks[(int)editState.CurrentViewMode].SoundIndices.AddRange(addAreas);
					tracks[(int)editState.CurrentViewMode].SoundIndices.RemoveAll(a => a.TBD);
					tracks[(int)editState.CurrentViewMode].SoundIndices = tracks[(int)editState.CurrentViewMode].SoundIndices.OrderBy(a => a.LeftX).ToList();

					if (previewArea.TBD)
					{
						prevTrackStates.Remove(prevTrackStates.Last());
					}
					else
					{
						nextTrackStates.RemoveAll(s => s.Mode == editState.CurrentViewMode);
					}

					previewArea = null;
				}
			}

			DrawPictureBoxDrawArea();
		}

		private void DrawPictureBoxDrawArea()
		{
			Graphics g = Graphics.FromImage(pictureBoxDrawArea.Image);

			// prepare
			int width = pictureBoxDrawArea.ClientRectangle.Width;
			int height = pictureBoxDrawArea.ClientRectangle.Height;

			g.CompositingQuality = CompositingQuality.HighQuality;
			g.InterpolationMode = InterpolationMode.High;
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
			g.Clear(Color.Black);

			Font font = new Font(Font.FontFamily, 7.0f);
			Pen grayPen = new Pen(Color.DimGray);
			Brush grayBrush = Brushes.DimGray;

			CultureInfo culture = CultureInfo.InvariantCulture;

			// vertical grid
			for (float v = 0.0f; v < maxVelocity; v += 10.0f)
			{
				float a = VelocityToX(v);
				g.DrawLine(grayPen, new PointF(a, 0.0f), new PointF(a, height));
				g.DrawString(v.ToString("0", culture), font, grayBrush, new PointF(a, 1.0f));
			}

			// horizontal grid
			switch (editState.CurrentInputMode)
			{
				case EditState.InputMode.Pitch:
					for (float p = 0.0f; p < maxPitch; p += 100.0f)
					{
						float a = PitchToY(p);
						g.DrawLine(grayPen, new PointF(0.0f, a), new PointF(width, a));
						g.DrawString(p.ToString("0", culture), font, grayBrush, new PointF(1.0f, a));
					}

					break;
				case EditState.InputMode.Volume:
					for (float v = 0.0f; v < maxVolume; v += 128.0f)
					{
						float a = VolumeToY(v);
						g.DrawLine(grayPen, new PointF(0.0f, a), new PointF(width, a));
						g.DrawString(v.ToString("0", culture), font, grayBrush, new PointF(1.0f, a));
					}

					break;
			}

			// dot
			if (editState.CurrentInputMode == EditState.InputMode.Pitch || editState.CurrentInputMode == EditState.InputMode.SoundIndex)
			{
				foreach (Vertex vertex in tracks[(int)editState.CurrentViewMode].PitchVertices.Values)
				{
					float x = VelocityToX(vertex.X);
					float y = PitchToY(vertex.Y);
					Area area = tracks[(int)editState.CurrentViewMode].SoundIndices.FirstOrDefault(a => a.LeftX <= vertex.X && vertex.X <= a.RightX);
					Color c;

					if (area != null && area.Index >= 0)
					{
						double hue = hueFactor * area.Index;
						hue -= Math.Floor(hue);
						c = GetColor(hue, vertex.Selected || vertex.IsOrigin);
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

					g.DrawRectangle(new Pen(c, 3.0f), x - 3.0f, y - 3.0f, 7.0f, 7.0f);
				}
			}

			if (editState.CurrentInputMode == EditState.InputMode.Volume || editState.CurrentInputMode == EditState.InputMode.SoundIndex)
			{
				foreach (Vertex vertex in tracks[(int)editState.CurrentViewMode].VolumeVertices.Values)
				{
					float x = VelocityToX(vertex.X);
					float y = VolumeToY(vertex.Y);
					Area area = tracks[(int)editState.CurrentViewMode].SoundIndices.FirstOrDefault(a => a.LeftX <= vertex.X && vertex.X <= a.RightX);
					Color c;

					if (area != null && area.Index >= 0)
					{
						double hue = hueFactor * area.Index;
						hue -= Math.Floor(hue);
						c = GetColor(hue, vertex.Selected || vertex.IsOrigin);
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

					g.DrawRectangle(new Pen(c, 2.0f), x - 2.0f, y - 2.0f, 5.0f, 5.0f);
				}
			}

			// line
			if (editState.CurrentInputMode == EditState.InputMode.Pitch || editState.CurrentInputMode == EditState.InputMode.SoundIndex)
			{
				foreach (Line line in tracks[(int)editState.CurrentViewMode].PitchLines)
				{
					Vertex left = tracks[(int)editState.CurrentViewMode].PitchVertices[line.LeftID];
					Vertex right = tracks[(int)editState.CurrentViewMode].PitchVertices[line.RightID];

					Func<float, float> f = x => left.Y + (right.Y - left.Y) / (right.X - left.X) * (x - left.X);

					{
						float leftX = VelocityToX(left.X);
						float leftY = PitchToY(left.Y);

						float rightX = VelocityToX(right.X);
						float rightY = PitchToY(right.Y);

						Color c;

						if (line.Selected)
						{
							c = Color.Silver;
						}
						else
						{
							c = Color.FromArgb((int)Math.Round(Color.Silver.R * 0.6), (int)Math.Round(Color.Silver.G * 0.6), (int)Math.Round(Color.Silver.B * 0.6));
						}

						g.DrawLine(new Pen(c, 3.0f), leftX, leftY, rightX, rightY);
					}

					foreach (Area area in tracks[(int)editState.CurrentViewMode].SoundIndices)
					{
						if (right.X < area.LeftX || left.X > area.RightX || area.Index < 0)
						{
							continue;
						}

						float leftX = VelocityToX(left.X);
						float leftY = PitchToY(left.Y);

						float rightX = VelocityToX(right.X);
						float rightY = PitchToY(right.Y);

						if (left.X < area.LeftX)
						{
							leftX = VelocityToX(area.LeftX);
							leftY = PitchToY(f(area.LeftX));
						}

						if (right.X > area.RightX)
						{
							rightX = VelocityToX(area.RightX);
							rightY = PitchToY(f(area.RightX));
						}

						double hue = hueFactor * area.Index;
						hue -= Math.Floor(hue);
						g.DrawLine(new Pen(GetColor(hue, line.Selected), 3.0f), leftX, leftY, rightX, rightY);
					}
				}
			}

			if (editState.CurrentInputMode == EditState.InputMode.Volume || editState.CurrentInputMode == EditState.InputMode.SoundIndex)
			{
				foreach (Line line in tracks[(int)editState.CurrentViewMode].VolumeLines)
				{
					Vertex left = tracks[(int)editState.CurrentViewMode].VolumeVertices[line.LeftID];
					Vertex right = tracks[(int)editState.CurrentViewMode].VolumeVertices[line.RightID];

					Func<float, float> f = x => left.Y + (right.Y - left.Y) / (right.X - left.X) * (x - left.X);

					{
						float leftX = VelocityToX(left.X);
						float leftY = VolumeToY(left.Y);

						float rightX = VelocityToX(right.X);
						float rightY = VolumeToY(right.Y);

						Color c;

						if (line.Selected)
						{
							c = Color.Silver;
						}
						else
						{
							c = Color.FromArgb((int)Math.Round(Color.Silver.R * 0.6), (int)Math.Round(Color.Silver.G * 0.6), (int)Math.Round(Color.Silver.B * 0.6));
						}

						g.DrawLine(new Pen(c, 2.0f), leftX, leftY, rightX, rightY);
					}

					foreach (Area area in tracks[(int)editState.CurrentViewMode].SoundIndices)
					{
						if (right.X < area.LeftX || left.X > area.RightX || area.Index < 0)
						{
							continue;
						}

						float leftX = VelocityToX(left.X);
						float leftY = VolumeToY(left.Y);

						float rightX = VelocityToX(right.X);
						float rightY = VolumeToY(right.Y);

						if (left.X < area.LeftX)
						{
							leftX = VelocityToX(area.LeftX);
							leftY = VolumeToY(f(area.LeftX));
						}

						if (right.X > area.RightX)
						{
							rightX = VelocityToX(area.RightX);
							rightY = VolumeToY(f(area.RightX));
						}

						double hue = hueFactor * area.Index;
						hue -= Math.Floor(hue);
						g.DrawLine(new Pen(GetColor(hue, line.Selected), 2.0f), leftX, leftY, rightX, rightY);
					}
				}
			}

			// area
			if (editState.CurrentInputMode == EditState.InputMode.SoundIndex)
			{
				IEnumerable<Area> areas;

				if (previewArea != null)
				{
					areas = tracks[(int)editState.CurrentViewMode].SoundIndices.Concat(new[] { previewArea });
				}
				else
				{
					areas = tracks[(int)editState.CurrentViewMode].SoundIndices;
				}

				foreach (Area area in areas)
				{
					float leftX = VelocityToX(area.LeftX);
					float rightX = VelocityToX(area.RightX);

					Color c;

					if (area.Index >= 0)
					{
						double hue = hueFactor * area.Index;
						hue -= Math.Floor(hue);
						c = GetColor(hue, true);
					}
					else
					{
						c = Color.Silver;
					}

					g.FillRectangle(new SolidBrush(Color.FromArgb(32, c)), leftX, 0.0f, rightX - leftX, height);
				}
			}

			// selected range
			if (selectedRange != null)
			{
				Pen pen = new Pen(Color.DimGray, 3.0f)
				{
					DashStyle = DashStyle.Dash
				};

				switch (editState.CurrentInputMode)
				{
					case EditState.InputMode.Pitch:
						g.DrawRectangle(pen, VelocityToX(selectedRange.Range.X), PitchToY(selectedRange.Range.Y), VelocityToX(selectedRange.Range.Right) - VelocityToX(selectedRange.Range.Left), PitchToY(selectedRange.Range.Top) - PitchToY(selectedRange.Range.Bottom));
						break;
					case EditState.InputMode.Volume:
						g.DrawRectangle(pen, VelocityToX(selectedRange.Range.X), VolumeToY(selectedRange.Range.Y), VelocityToX(selectedRange.Range.Right) - VelocityToX(selectedRange.Range.Left), VolumeToY(selectedRange.Range.Top) - VolumeToY(selectedRange.Range.Bottom));
						break;
				}
			}

			// simulation speed
			if (editState.CurrentSimuState == EditState.SimulationState.Started || editState.CurrentSimuState == EditState.SimulationState.Paused)
			{
				float a = VelocityToX((float)nowSpeed);
				g.DrawLine(new Pen(Color.White, 3.0f), new PointF(a, 0.0f), new PointF(a, height));
			}

			pictureBoxDrawArea.Refresh();
		}
	}
}
