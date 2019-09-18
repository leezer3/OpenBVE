using System;
using System.Reactive.Linq;
using System.Windows.Forms;
using TrainEditor2.Models.Dialogs;
using TrainEditor2.ViewModels.Dialogs;
using MessageBox = System.Windows.Forms.MessageBox;

namespace TrainEditor2.Views
{
	public partial class FormEditor
	{
		private IDisposable BindToMessageBox(MessageBoxViewModel x)
		{
			return x.IsOpen.Where(y => y).Subscribe(_ =>
			{
				MessageBoxIcon icon;
				MessageBoxButtons button;

				switch (x.Icon.Value)
				{
					case BaseDialog.DialogIcon.None:
						icon = MessageBoxIcon.None;
						break;
					case BaseDialog.DialogIcon.Information:
						icon = MessageBoxIcon.Information;
						break;
					case BaseDialog.DialogIcon.Question:
						icon = MessageBoxIcon.Question;
						break;
					case BaseDialog.DialogIcon.Warning:
						icon = MessageBoxIcon.Warning;
						break;
					case BaseDialog.DialogIcon.Error:
						icon = MessageBoxIcon.Warning;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				switch (x.Button.Value)
				{
					case BaseDialog.DialogButton.Ok:
						button = MessageBoxButtons.OK;
						break;
					case BaseDialog.DialogButton.OkCancel:
						button = MessageBoxButtons.OKCancel;
						break;
					case BaseDialog.DialogButton.YesNo:
						button = MessageBoxButtons.YesNo;
						break;
					case BaseDialog.DialogButton.YesNoCancel:
						button = MessageBoxButtons.YesNoCancel;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				switch (MessageBox.Show(x.Text.Value, x.Title.Value, button, icon))
				{
					case DialogResult.OK:
					case DialogResult.Yes:
						x.YesCommand.Execute();
						break;
					case DialogResult.No:
						x.NoCommand.Execute();
						break;
					case DialogResult.Cancel:
						x.CancelCommand.Execute();
						break;
				}
			});
		}
	}
}
