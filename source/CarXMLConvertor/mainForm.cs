using System;
using System.Windows.Forms;
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
		    get
		    {
			    return this.textBoxOutput.Text;
		    }
		    set
		    {
			    this.textBoxOutput.Text = value;
		    }

	    }

        private void process_Click(object sender, EventArgs e)
        {
	        updateLogBoxText = "Loading parameters from train.dat file " + ConvertTrainDat.FileName + Environment.NewLine;
	        ConvertTrainDat.Process(this);
	        if (!System.IO.File.Exists(ConvertSoundCfg.FileName))
	        {
		        updateLogBoxText += "INFO: No sound.cfg file detected." + Environment.NewLine;
				//TODO: Is it worth spinning up a default XML for the BVE2 sound-set??
				if (MessageBox.Show("The selected folder does not contain a valid sound.cfg", "CarXML Convertor", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.Cancel)
				{
					updateLogBoxText += "Aborting....";
					return;
				}
		        
			}
	        if (System.IO.File.Exists(Path.CombineFile(System.IO.Path.GetDirectoryName(ConvertSoundCfg.FileName), "sound.xml")))
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
	        if (System.IO.File.Exists(Path.CombineFile(System.IO.Path.GetDirectoryName(ConvertExtensionsCfg.FileName), "train.xml")))
	        {
		        updateLogBoxText += "INFO: An existing train.xml file was detected." + Environment.NewLine;
				if (MessageBox.Show("The selected folder already contains a train.xml file. \r\n Do you wish to continue?", "CarXML Convertor", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.Cancel)
		        {
			        return;
		        }
		        updateLogBoxText += "Overwriting...." + Environment.NewLine;
			}
			if (this.radioButtonSingleFile.Checked == true)
	        {
		        updateLogBoxText += "INFO: Using a single train.xml file." + Environment.NewLine;
				ConvertExtensionsCfg.SingleFile = true;
	        }
	        else
	        {
				updateLogBoxText += "INFO: Using a train.xml file with child car files." + Environment.NewLine;
			}
			ConvertExtensionsCfg.Process(this);
	        updateLogBoxText += "Processing complete.";
		}
        

        private void buttonSelectFolder_Click(object sender, EventArgs e)
        {
            folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
	            ConvertTrainDat.FileName = Path.CombineFile(folderBrowserDialog.SelectedPath, "train.dat");
				ConvertSoundCfg.FileName = Path.CombineFile(folderBrowserDialog.SelectedPath, "sound.cfg");
	            ConvertExtensionsCfg.FileName = Path.CombineFile(folderBrowserDialog.SelectedPath, "extensions.cfg");
	            textBox1.Text = folderBrowserDialog.SelectedPath;
            }
        }
    }
}
