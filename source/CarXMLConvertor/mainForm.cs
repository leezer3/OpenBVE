using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Path = OpenBveApi.Path;

namespace CarXmlConvertor
{
	public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

	    public string updateLogBoxText
	    {
		    get => this.textBoxOutput.Text;
		    set => this.textBoxOutput.Text = value;
	    }

	    private bool animatedPanel;
	    internal bool terminateEarly;

        private void process_Click(object sender, EventArgs e)
        {
			terminateEarly = false;
	        if (string.IsNullOrEmpty(ConvertTrainDat.FileName) && !string.IsNullOrEmpty(textBox1.Text) && Directory.Exists(textBox1.Text))
	        {
		        ConvertTrainDat.FileName = Path.CombineFile(textBox1.Text, "train.dat");
		        ConvertSoundCfg.FileName = Path.CombineFile(textBox1.Text, "sound.cfg");
		        ConvertPanel2.FileName = Path.CombineFile(textBox1.Text, "panel2.cfg");
		        ConvertPanelAnimated.FileName = Path.CombineFile(textBox1.Text, "panel.animated");
		        animatedPanel = File.Exists(ConvertPanelAnimated.FileName);
		        ConvertExtensionsCfg.FileName = Path.CombineFile(textBox1.Text, "extensions.cfg");
	        }
	        updateLogBoxText = "Loading parameters from train.dat file " + ConvertTrainDat.FileName + Environment.NewLine;
	        ConvertTrainDat.Process(this);
			if (terminateEarly)
				return;
		    
	        if (!animatedPanel)
	        {
				if (!File.Exists(ConvertPanel2.FileName))
		        {
			        updateLogBoxText += "INFO: No panel2.cfg file detected." + Environment.NewLine;
			        if (MessageBox.Show("The selected folder does not contain a valid panel2.cfg", "CarXML Convertor", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.Cancel)
			        {
				        updateLogBoxText += "Aborting....";
				        return;
			        }
		        }
	        }

			if (File.Exists(Path.CombineFile(Path.GetDirectoryName(ConvertPanelAnimated.FileName), "panel.xml")))
	        {
		        updateLogBoxText += "INFO: An existing panel.xml file was detected." + Environment.NewLine;
		        if (MessageBox.Show("The selected folder already contains a panel.xml file. \r\n Do you wish to continue?", "CarXML Convertor", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.Cancel)
		        {
			        updateLogBoxText += "Aborting....";
			        return;
		        }
		        updateLogBoxText += "Overwriting...." + Environment.NewLine;
	        }

	        if (animatedPanel)
	        {
		        updateLogBoxText += "Loading existing panel.animated file " + ConvertPanelAnimated.FileName + Environment.NewLine;
		        ConvertPanelAnimated.Process(this);
	        }
	        else
	        {
		        updateLogBoxText += "Loading existing panel2.cfg file " + ConvertPanelAnimated.FileName + Environment.NewLine;
		        ConvertPanel2.Process(this);
	        }
	        
			
	        if (!File.Exists(ConvertSoundCfg.FileName))
	        {
		        updateLogBoxText += "INFO: No sound.cfg file detected." + Environment.NewLine;
				//TODO: Is it worth spinning up a default XML for the BVE2 sound-set??
				if (MessageBox.Show("The selected folder does not contain a valid sound.cfg", "CarXML Convertor", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.Cancel)
				{
					updateLogBoxText += "Aborting....";
					return;
				}
		        
			}
	        if (File.Exists(Path.CombineFile(Path.GetDirectoryName(ConvertSoundCfg.FileName), "sound.xml")))
	        {
		        updateLogBoxText += "INFO: An existing sound.xml file was detected." + Environment.NewLine;
				if (MessageBox.Show("The selected folder already contains a sound.xml file. \r\n Do you wish to continue?", "CarXML Convertor", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.Cancel)
		        {
			        updateLogBoxText += "Aborting....";
					return;
		        }
		        updateLogBoxText += "Overwriting...." + Environment.NewLine;
			}
	        updateLogBoxText += "Loading existing sound.cfg file " + ConvertSoundCfg.FileName + Environment.NewLine;
			ConvertSoundCfg.Process(this);
			string trainXML = Path.CombineFile(Path.GetDirectoryName(ConvertExtensionsCfg.FileName), "train.xml");
	        if (File.Exists(trainXML))
	        {
		        FileVersionInfo programVersion = null;
		        bool olderConvertorVersion = false;
		        updateLogBoxText += "INFO: An existing train.xml file was detected." + Environment.NewLine;
		        try
		        {
			        programVersion = FileVersionInfo.GetVersionInfo("OpenBve.exe");
		        }
		        catch
		        {
			        // Ignore- Most likely the convertor has been copied elsewhere
		        }
		        //The current XML file to load
		        XmlDocument currentXML = new XmlDocument();
		        currentXML.Load(trainXML);
		        if (currentXML.DocumentElement != null)
		        {
			        XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes("/openBVE/Train/ConvertorVersion");
			        if (DocumentNodes != null && DocumentNodes.Count > 0)
			        {
						Version v = Version.Parse(DocumentNodes[0].InnerText);
						if (programVersion != null && v < Version.Parse(programVersion.FileVersion))
						{
							olderConvertorVersion = true;
						} 
						
			        }
		        }

		        if (olderConvertorVersion)
		        {
			        if (MessageBox.Show("The selected folder appears to contain a train.xml file converted with an older version of CarXMLConvertor. \r\n Do you wish to continue?", "CarXML Convertor", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.Cancel)
			        {
				        return;
			        }
		        }
		        else
		        {
			        if (MessageBox.Show("The selected folder already contains a train.xml file. \r\n Do you wish to continue?", "CarXML Convertor", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.Cancel)
			        {
				        return;
			        }
		        }
		        
		        updateLogBoxText += "Overwriting...." + Environment.NewLine;
			}
			if (radioButtonSingleFile.Checked)
	        {
		        updateLogBoxText += "INFO: Using a single train.xml file." + Environment.NewLine;
				ConvertExtensionsCfg.SingleFile = true;
	        }
	        else
	        {
				updateLogBoxText += "INFO: Using a train.xml file with child car files." + Environment.NewLine;
		        ConvertExtensionsCfg.SingleFile = false;
			}
			ConvertExtensionsCfg.Process(this);

	        updateLogBoxText += "Processing complete.";
		}
        

        private void buttonSelectFolder_Click(object sender, EventArgs e)
        {
            folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
				UpdatePath(folderBrowserDialog.SelectedPath);
	            textBox1.Text = folderBrowserDialog.SelectedPath;
            }
        }

		private void UpdatePath(string path) {
			ConvertTrainDat.FileName = Path.CombineFile(path, "train.dat");
			ConvertSoundCfg.FileName = Path.CombineFile(path, "sound.cfg");
			ConvertPanel2.FileName = Path.CombineFile(path, "panel2.cfg");
			ConvertPanelAnimated.FileName = Path.CombineFile(path, "panel.animated");
			animatedPanel = File.Exists(ConvertPanelAnimated.FileName);
			ConvertExtensionsCfg.FileName = Path.CombineFile(path, "extensions.cfg");
		}

		private void textBox1_DragEnter(object sender, DragEventArgs e)
		{
			e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
		}

		private void textBox1_DragDrop(object sender, DragEventArgs e) {
			if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
				string path = ((string[]) e.Data.GetData(DataFormats.FileDrop))[0];
				if (File.Exists(path)) {
					path = Directory.GetParent(path).FullName;
				}

				if (Directory.Exists(path)) {
					UpdatePath(path);
					textBox1.Text = path;
				}
			}
		}
	}
}
