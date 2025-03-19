using System.Windows.Forms;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;

namespace OpenBve.UserInterface
{
	/// <summary>Basic class to show logs</summary>
	public partial class FormViewLog : Form
	{
		/// <summary>Constructor</summary>
		/// <param name="text">The log text to show</param>
		public FormViewLog(string text)
		{
			InitializeComponent();
			SetText(text);
		}

		private void SetText(string text)
		{
			var originalTitle = Text;
			Text += Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"message","loading"});
			Shown += (sender, e) => {
				textBoxLog.Text = text;
				Text = originalTitle;
			};
		}

		private void buttonClose_Click(object sender, System.EventArgs e)
		{
			Close();
		}
	}
}
