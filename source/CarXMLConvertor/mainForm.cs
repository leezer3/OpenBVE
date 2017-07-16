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


        /*
         * All this currently does is to process an existing sound.cfg into a sound.xml file
         * 
         * TODO:
         * When the XML replacement for the train.dat file is completed, this should be generated.
         * Add a separate class containing this function
         * 
         */

        private void process_Click(object sender, EventArgs e)
        {
	        ConvertTrainDat.Process();
            ConvertSoundCfg.Process();
        }
        

        private void buttonSelectFolder_Click(object sender, EventArgs e)
        {
            folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
	            ConvertTrainDat.FileName = Path.CombineFile(folderBrowserDialog.SelectedPath, "train.dat");
				ConvertSoundCfg.FileName = Path.CombineFile(folderBrowserDialog.SelectedPath, "sound.cfg");
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
