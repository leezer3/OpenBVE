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

        private void process_Click(object sender, EventArgs e)
        {
	        ConvertTrainDat.Process();
	        if (!System.IO.File.Exists(ConvertSoundCfg.FileName))
	        {
				//TODO: Is it worth spinning up a default XML for the BVE2 sound-set??
				if (MessageBox.Show("The selected folder does not contain a valid sound.cfg", "CarXML Convertor", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.Cancel)
				{
					return;
				}
	        }
	        if (System.IO.File.Exists(Path.CombineFile(System.IO.Path.GetDirectoryName(ConvertSoundCfg.FileName), "sound.xml")))
	        {
		        if (MessageBox.Show("The selected folder already contains a sound.xml file. \r\n Do you wish to continue?", "CarXML Convertor", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.Cancel)
		        {
			        return;
		        }
	        }
			ConvertSoundCfg.Process();
	        if (System.IO.File.Exists(Path.CombineFile(System.IO.Path.GetDirectoryName(ConvertExtensionsCfg.FileName), "train.xml")))
	        {
		        if (MessageBox.Show("The selected folder already contains a train.xml file. \r\n Do you wish to continue?", "CarXML Convertor", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.Cancel)
		        {
			        return;
		        }
	        }
			ConvertExtensionsCfg.Process();
        }
        

        private void buttonSelectFolder_Click(object sender, EventArgs e)
        {
            folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
	            ConvertTrainDat.FileName = Path.CombineFile(folderBrowserDialog.SelectedPath, "train.dat");
				ConvertSoundCfg.FileName = Path.CombineFile(folderBrowserDialog.SelectedPath, "sound.cfg");
	            ConvertExtensionsCfg.FileName = Path.CombineFile(folderBrowserDialog.SelectedPath, "extensions.cfg");
				//TODO:
				//Check for all train components when the above is complete
				//Error checking??
				if (!System.IO.File.Exists(ConvertSoundCfg.FileName))
                {
                    return;
                }
                textBox1.Text = ConvertSoundCfg.FileName;
            }
        }
    }
}
