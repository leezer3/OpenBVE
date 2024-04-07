﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using RouteManager2;

namespace OpenBve
{
	internal partial class formImageExport : Form
	{
		private readonly bool IsMap;
		private readonly string routeFileName;
		internal formImageExport(bool isMap, string fileName)
		{
			InitializeComponent();
			IsMap = isMap;
			routeFileName = fileName;
			textBoxPath.Text = Path.GetDirectoryName(routeFileName);
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void buttonExport_Click(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(textBoxPath.Text) && Directory.Exists(textBoxPath.Text))
			{
				try
				{
					if (IsMap)
					{
						Image map = Illustrations.CreateRouteMap((int)numericUpDownWidth.Value, (int)numericUpDownHeight.Value, false, out _);
						map.Save(textBoxPath.Text + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(routeFileName) + " - Map.png", ImageFormat.Png);
					}
					else 
					{
						Image gradient = Illustrations.CreateRouteGradientProfile((int)numericUpDownWidth.Value, (int)numericUpDownHeight.Value, false);
						gradient.Save(textBoxPath.Text + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(routeFileName) + " - Gradient Profile.png", ImageFormat.Png);
					}
				}
				catch
				{
					MessageBox.Show("An error has occured." + Environment.NewLine + "Please check whether you have write permission for that directory.", "Export Image", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					return;
				}
				Close();
			}
			else
			{
				MessageBox.Show("Invalid or non-existant path, please retry.", "Export Image", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}

		private void buttonBrowse_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog fb = new FolderBrowserDialog();
			if (fb.ShowDialog() == DialogResult.OK)
			{
				textBoxPath.Text = fb.SelectedPath;
			}
		}
	}
}
