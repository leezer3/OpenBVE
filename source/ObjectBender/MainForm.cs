using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace ObjectBender {
	/// <summary>The form that provides the GUI for the program.</summary>
	public partial class MainForm : Form {
		public MainForm() {
			InitializeComponent();
		}
		
		
		// --- members ---
		
		/// <summary>The full path to the input file.</summary>
		private string InputFile = null;

		/// <summary>The full path to the output file.</summary>
		private string OutputFile = null;
		
		
		// --- browse buttons ---
		
		/// <summary>Raised when the Output field's Browse button is clicked.</summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The event arguments.</param>
		private void ButtonInputClick(object sender, EventArgs e) {
			OpenFileDialog dialog = new OpenFileDialog
			{
				Filter = @"B3D/CSV files|*.b3d;*.csv|All files|*",
				CheckFileExists = true
			};
			if (dialog.ShowDialog() == DialogResult.OK) {
				InputFile = dialog.FileName;
				textboxInput.Text = Path.GetFileName(dialog.FileName);
			}
		}
		
		/// <summary>Raised when the Output field's Browse button is clicked.</summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The event arguments.</param>
		private void ButtonOutputClick(object sender, EventArgs e) {
			SaveFileDialog dialog = new SaveFileDialog();
			if (InputFile != null) {
				if (InputFile.EndsWith(".b3d", StringComparison.OrdinalIgnoreCase)) {
					dialog.Filter = @"B3D files|*.b3d|All files|*";
				} else {
					dialog.Filter = @"CSV files|*.csv|All files|*";
				}
			} else {
				dialog.Filter = @"B3D/CSV files|*.b3d;*.csv|All files|*";
			}
			dialog.OverwritePrompt = true;
			if (dialog.ShowDialog() == DialogResult.OK) {
				OutputFile = dialog.FileName;
				textboxOutput.Text = Path.GetFileName(dialog.FileName);
			}
		}
		
		
		// --- textboxes (enter) ---
		
		/// <summary>Raised when this textbox becomes the active control of the form.</summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The event arguments.</param>
		private void TextboxNumberOfSegmentsEnter(object sender, EventArgs e) {
			labelInformation.Text = "If you have a source object that you want to duplicate multiple times, enter the number of segments here you need in total.";
		}
		
		/// <summary>Raised when this textbox becomes the active control of the form.</summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The event arguments.</param>
		private void TextboxSegmentLengthEnter(object sender, EventArgs e) {
			labelInformation.Text = "If there are multiple segments, enter the number here by which each segment should be offset from the previous one.";
		}
		
		/// <summary>Raised when this textbox becomes the active control of the form.</summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The event arguments.</param>
		private void TextboxBlockLengthEnter(object sender, EventArgs e) {
			labelInformation.Text = "If you want to use the target object as a rail object, enter the block length here. The object will then be rotated to match the block length.";
		}
		
		/// <summary>Raised when this textbox becomes the active control of the form.</summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The event arguments.</param>
		private void TextboxRadiusEnter(object sender, EventArgs e) {
			labelInformation.Text = "Enter the radius here. For a left curve, use negative values. For a right curve, use positive values. You can also use zero to just concatenate multiple segments without bending them.";
		}
		
		/// <summary>Raised when this textbox becomes the active control of the form.</summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The event arguments.</param>
		private void TextboxRailGaugeEnter(object sender, EventArgs e) {
			labelInformation.Text = "If you want to use cant on either end of the object, enter the rail gauge here.";
		}
		
		/// <summary>Raised when this textbox becomes the active control of the form.</summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The event arguments.</param>
		private void TextboxInitialCantEnter(object sender, EventArgs e) {
			labelInformation.Text = "Enter the cant for the beginning of the object here. For inward cant, use positive values. For outward cant, use negative values. You also need to provide the rail gauge.";
		}
		
		/// <summary>Raised when this textbox becomes the active control of the form.</summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The event arguments.</param>
		private void TextboxFinalCantEnter(object sender, EventArgs e) {
			labelInformation.Text = "Enter the cant for the end of the object here. For inward cant, use positive values. For outward cant, use negative values. You also need to provide the rail gauge.";
		}
		
		
		// --- buttons ---
		
		/// <summary>Raised when the Start button is clicked.</summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The event arguments.</param>
		private void ButtonStartClick(object sender, EventArgs e) {
			#if !DEBUG
			try {
				#endif
				if (InputFile != null & OutputFile != null) {
					CultureInfo culture = CultureInfo.InvariantCulture;
					Bender.Options options = new Bender.Options
					{
						InputFile = InputFile,
						OutputFile = OutputFile,
						NumberOfSegments = 1
					};
					if (textboxNumberOfSegments.Text.Length != 0) {
						options.NumberOfSegments = int.Parse(textboxNumberOfSegments.Text, culture);
					}
					if (textboxSegmentLength.Text.Length != 0) {
						options.SegmentLength = double.Parse(textboxSegmentLength.Text, culture);
					}
					if (textboxBlockLength.Text.Length != 0) {
						options.BlockLength = double.Parse(textboxBlockLength.Text, culture);
					}
					if (textboxRadius.Text.Length != 0) {
						options.Radius = double.Parse(textboxRadius.Text, culture);
					}
					if (textboxRailGauge.Text.Length != 0) {
						options.RailGauge = 0.001 * double.Parse(textboxRailGauge.Text, culture);
					}
					if (textboxInitialCant.Text.Length != 0) {
						options.InitialCant = 0.001 * double.Parse(textboxInitialCant.Text, culture);
					}
					if (textboxFinalCant.Text.Length != 0) {
						options.FinalCant = 0.001 * double.Parse(textboxFinalCant.Text, culture);
					}
					if (options.NumberOfSegments != 1 & options.SegmentLength == 0.0) {
						MessageBox.Show("If the number of segments is greater than 1, you also need to provide the segment length.", "ObjectBender", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						textboxSegmentLength.Focus();
					} else if (options.BlockLength != 0.0 & options.Radius == 0.0) {
						MessageBox.Show("If a block length is provided, you also need to provide the radius.", "ObjectBender", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						textboxRadius.Focus();
					} else if ((options.InitialCant != 0.0 | options.FinalCant != 0.0) & options.SegmentLength == 0.0) {
						MessageBox.Show("If cant is provided, you also need to provide the segment length.", "ObjectBender", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						textboxSegmentLength.Focus();
					} else if ((options.InitialCant != 0.0 | options.FinalCant != 0.0) & options.RailGauge == 0.0) {
						MessageBox.Show("If cant is provided, you also need to provide the rail gauge.", "ObjectBender", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						textboxRailGauge.Focus();
					} else {
						Bender.BendObject(options);
						MessageBox.Show("Done.", "ObjectBender", MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
				} else {
					MessageBox.Show("Please specify a source and a target file.", "ObjectBender", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
				#if !DEBUG
			} catch (Exception ex) {
				MessageBox.Show(ex.Message, "ObjectBender", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
			#endif
		}
		
		/// <summary>Raised when the Close button is clicked.</summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The event arguments.</param>
		private void ButtonCloseClick(object sender, EventArgs e) {
			this.Close();
		}
		
	}
}