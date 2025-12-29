using System;
using System.Drawing;
using System.Windows.Forms;
using OpenBve.Input;
using OpenBveApi;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;

namespace OpenBve.UserInterface
{
	/// <inheritdoc />
	public partial class FormRaildriverCalibration : Form
	{
		/// <inheritdoc />
		public FormRaildriverCalibration()
		{
			InitializeComponent();
			main = (Bitmap)ImageExtensions.FromFile(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\raildriver.png"));
			pictureBox1.Image = main;
			buttonCalibrationPrevious.Enabled = false;
			labelCalibrationText.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"raildriver","calibration_start"});
			if (Program.Joysticks.RailDriverCount == 0)
			{
				MessageBox.Show(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"raildriver","notdetected"}), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
				Load += (s, e) => Close();
				return;
			}
			buttonCalibrationNext.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","button_next"});
			buttonCalibrationPrevious.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","button_back"});
		}

		private void formRaildriverCalibration_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (!Program.Joysticks.AttachedJoysticks.TryGetValue(AbstractRailDriver.Guid, out AbstractJoystick joystick))
			{
				return;
			}
			if (!(joystick is AbstractRailDriver j))
			{
				return;
			}
			for (int i = 0; i < j.Calibration.Length; i++)
			{
				if (j.Calibration[i].Maximum < j.Calibration[i].Minimum)
				{
					//If calibration min and max are reversed flip them
					(j.Calibration[i].Maximum, j.Calibration[i].Minimum) = (j.Calibration[i].Minimum, j.Calibration[i].Maximum);
				}
				if (j.Calibration[i].Maximum == j.Calibration[i].Minimum)
				{
					//If calibration values are identical, reset to defaults
					j.Calibration[i].Minimum = 0;
					j.Calibration[i].Maximum = 255;
				}
				//Bounds check values (This should never happen, but check anyways)
				if (j.Calibration[i].Minimum < 0)
				{
					j.Calibration[i].Minimum = 0;
				}
				if (j.Calibration[i].Maximum > 255)
				{
					j.Calibration[i].Maximum = 255;
				}
				if (j.Calibration[i].Maximum - j.Calibration[i].Minimum < 10)
				{
					//If calibration values are within 10 of each other, something is not right....
					j.Calibration[i].Minimum = 0;
					j.Calibration[i].Maximum = 255;
				}
			}
		}

		private int calibrationStage;

		//Stores the main bitmap
		private readonly Bitmap main;
		//The modified bitmap with the overlays drawn on it
		private Bitmap Modified;

		private void buttonCalibrationNext_Click(object sender, EventArgs e)
		{
			if (!Program.Joysticks.AttachedJoysticks.TryGetTypedValue(AbstractRailDriver.Guid, out AbstractRailDriver j))
			{
				return;
			}
			
			if (calibrationStage > 14)
			{
				j.SaveCalibration(Path.CombineFile(Program.FileSystem.SettingsFolder, "RailDriver.xml"));
				Close();
				return;
			}
			if (calibrationStage != 0)
			{
				int axis = (calibrationStage + 1 )/ 2;
				
				if (calibrationStage % 2 != 0)
				{
					if (axis < 5)
					{
						j.Calibration[axis - 1].Maximum = j.currentState[axis];
					}
					else
					{
						j.Calibration[axis - 1].Minimum = j.currentState[axis];
					}
				}
				else
				{
					if (axis < 5)
					{
						j.Calibration[axis - 1].Minimum = j.currentState[axis];
					}
					else
					{
						j.Calibration[axis - 1].Maximum = j.currentState[axis];
					}
				}
			}
			buttonCalibrationPrevious.Enabled = true;
			calibrationStage++;
			paintImage();
		}

		private void buttonCalibrationPrevious_Click(object sender, EventArgs e)
		{
			if (calibrationStage <= 0)
			{
				buttonCalibrationPrevious.Enabled = false;
			}
			buttonCalibrationNext.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","button_next"});
			calibrationStage--;
			paintImage();
		}

		private void paintImage()
		{
			switch (calibrationStage)
			{
				case 0:
					pictureBox1.Image = main;
					labelCalibrationText.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"raildriver","calibration_start"});
					break;
				case 1:
					Modified = new Bitmap(main);
					using (var graphics = System.Drawing.Graphics.FromImage(Modified))
					{
						graphics.DrawRectangle(new Pen(Color.Blue, 3.0f), new Rectangle(210, 130, 80, 145));
						graphics.DrawImage(ImageExtensions.FromFile(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\arrow_down.png")), 234, 284);
					}
					pictureBox1.Image = Modified;
					labelCalibrationText.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"raildriver","calibration_a"});
					break;
				case 2:
					Modified = new Bitmap(main);
					using (var graphics = System.Drawing.Graphics.FromImage(Modified))
					{
						graphics.DrawRectangle(new Pen(Color.Blue, 3.0f), new Rectangle(210, 130, 80, 145));
						graphics.DrawImage(ImageExtensions.FromFile(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\arrow_up.png")), 234, 84);
					}
					pictureBox1.Image = Modified;
					labelCalibrationText.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"raildriver","calibration_b"});
					break;
				case 3:
					Modified = new Bitmap(main);
					using (var graphics = System.Drawing.Graphics.FromImage(Modified))
					{
						graphics.DrawRectangle(new Pen(Color.Blue, 3.0f), new Rectangle(290, 130, 90, 145));
						graphics.DrawImage(ImageExtensions.FromFile(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\arrow_down.png")), 314, 284);
					}
					pictureBox1.Image = Modified;
					labelCalibrationText.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"raildriver","calibration_c"});
					break;
				case 4:
					Modified = new Bitmap(main);
					using (var graphics = System.Drawing.Graphics.FromImage(Modified))
					{
						graphics.DrawRectangle(new Pen(Color.Blue, 3.0f), new Rectangle(290, 130, 90, 145));
						graphics.DrawImage(ImageExtensions.FromFile(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\arrow_up.png")), 314, 84);
					}
					pictureBox1.Image = Modified;
					labelCalibrationText.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"raildriver","calibration_d"});
					break;
				case 5:
					Modified = new Bitmap(main);
					using (var graphics = System.Drawing.Graphics.FromImage(Modified))
					{
						graphics.DrawRectangle(new Pen(Color.Blue, 3.0f), new Rectangle(450, 130, 80, 165));
						graphics.DrawImage(ImageExtensions.FromFile(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\arrow_down.png")), 470, 304);
					}
					pictureBox1.Image = Modified;
					labelCalibrationText.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"raildriver","calibration_e"});
					break;
				case 6:
					Modified = new Bitmap(main);
					using (var graphics = System.Drawing.Graphics.FromImage(Modified))
					{
						graphics.DrawRectangle(new Pen(Color.Blue, 3.0f), new Rectangle(450, 130, 80, 165));
						graphics.DrawImage(ImageExtensions.FromFile(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\arrow_up.png")), 470, 79);
					}
					pictureBox1.Image = Modified;
					labelCalibrationText.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"raildriver","calibration_f"});
					break;
				case 7:
					Modified = new Bitmap(main);
					using (var graphics = System.Drawing.Graphics.FromImage(Modified))
					{
						graphics.DrawRectangle(new Pen(Color.Blue, 3.0f), new Rectangle(610, 130, 75, 165));
						graphics.DrawImage(ImageExtensions.FromFile(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\arrow_down.png")), 630, 304);
					}
					pictureBox1.Image = Modified;
					labelCalibrationText.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"raildriver","calibration_g"});
					break;
				case 8:
					Modified = new Bitmap(main);
					using (var graphics = System.Drawing.Graphics.FromImage(Modified))
					{
						graphics.DrawRectangle(new Pen(Color.Blue, 3.0f), new Rectangle(610, 130, 75, 165));
						graphics.DrawImage(ImageExtensions.FromFile(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\arrow_up.png")), 630, 79);
					}
					pictureBox1.Image = Modified;
					labelCalibrationText.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"raildriver","calibration_h"});
					break;
				case 9:
					Modified = new Bitmap(main);
					using (var graphics = System.Drawing.Graphics.FromImage(Modified))
					{
						graphics.DrawRectangle(new Pen(Color.Blue, 3.0f), new Rectangle(610, 130, 75, 165));
						graphics.DrawImage(ImageExtensions.FromFile(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\arrow_left.png")), 560, 175);
					}
					pictureBox1.Image = Modified;
					labelCalibrationText.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"raildriver","calibration_i"});
					break;
				case 10:
					Modified = new Bitmap(main);
					using (var graphics = System.Drawing.Graphics.FromImage(Modified))
					{
						graphics.DrawRectangle(new Pen(Color.Blue, 3.0f), new Rectangle(610, 130, 75, 165));
						graphics.DrawImage(ImageExtensions.FromFile(Path.CombineFile(Program.FileSystem.DataFolder, "Menu\\arrow_right.png")), 690, 175);
					}
					pictureBox1.Image = Modified;
					labelCalibrationText.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"raildriver","calibration_j"});
					break;
				case 11:
					Modified = new Bitmap(main);
					using (var graphics = System.Drawing.Graphics.FromImage(Modified))
					{
						graphics.DrawRectangle(new Pen(Color.Blue, 3.0f), new Rectangle(715, 100, 55, 75));
					}
					pictureBox1.Image = Modified;
					labelCalibrationText.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"raildriver","calibration_k"});
					break;
				case 12:
					Modified = new Bitmap(main);
					using (var graphics = System.Drawing.Graphics.FromImage(Modified))
					{
						graphics.DrawRectangle(new Pen(Color.Blue, 3.0f), new Rectangle(715, 100, 55, 75));
					}
					pictureBox1.Image = Modified;
					labelCalibrationText.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"raildriver","calibration_l"});
					break;
				case 13:
					Modified = new Bitmap(main);
					using (var graphics = System.Drawing.Graphics.FromImage(Modified))
					{
						graphics.DrawRectangle(new Pen(Color.Blue, 3.0f), new Rectangle(715, 210, 55, 75));
					}
					pictureBox1.Image = Modified;
					labelCalibrationText.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"raildriver","calibration_m"});
					break;
				case 14:
					Modified = new Bitmap(main);
					using (var graphics = System.Drawing.Graphics.FromImage(Modified))
					{
						graphics.DrawRectangle(new Pen(Color.Blue, 3.0f), new Rectangle(715, 210, 55, 75));
					}
					pictureBox1.Image = Modified;
					labelCalibrationText.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"raildriver","calibration_n"});
					break;
				case 15:
					pictureBox1.Image = main;
					labelCalibrationText.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"raildriver","calibration_o"});
					buttonCalibrationNext.Text = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"packages","success"});
					break;
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
				main?.Dispose();
				Modified?.Dispose();

			}
			base.Dispose(disposing);
		}
	}
}
