using System.Windows.Forms;

namespace OpenBve.UserInterface
{
	public partial class formViewLog : Form
	{
		/// <summary>Constructor</summary>
		/// <param name="text">The log text to show</param>
		public formViewLog(string text)
		{
			InitializeComponent();
			var originalTitle = this.Text;
			this.Text += " (Loading...)";

			this.Shown += (sender, e) => {
				textBoxLog.Text = text;
				this.Text = originalTitle;
			};
		}

		private void buttonClose_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
	}
}