// Minimal Windows Forms stand-ins used by the shared OpenBVE sources.
//
// The desktop build links the real System.Windows.Forms; the mobile (net8.0) build
// compiles the same sources against these shims. Only the tiny surface actually used
// by the shared core is provided:
//   - MessageBox / MessageBoxButtons / MessageBoxIcon / DialogResult  -> logged to stderr
//   - Application.StartupPath                                        -> AppContext.BaseDirectory
//   - IWin32Window                                                   -> owner-window abstraction
//
// Anything UI related that genuinely needs a window (ComboBox helpers etc.) is
// excluded from the mobile build with #if !MOBILE in the shared sources.

using System;

namespace System.Windows.Forms
{
	/// <summary>Stub of the WinForms MessageBox which logs instead of showing a dialog.</summary>
	public static class MessageBox
	{
		public static DialogResult Show(string text)
		{
			Console.Error.WriteLine("[OpenBVE] " + text);
			return DialogResult.OK;
		}

		public static DialogResult Show(string text, string caption)
		{
			Console.Error.WriteLine("[OpenBVE] " + caption + ": " + text);
			return DialogResult.OK;
		}

		public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
		{
			Console.Error.WriteLine("[OpenBVE] " + caption + ": " + text);
			return DialogResult.OK;
		}
	}

	public enum MessageBoxButtons
	{
		OK = 0,
		OKCancel = 1,
		AbortRetryIgnore = 2,
		YesNoCancel = 3,
		YesNo = 4,
		RetryCancel = 5
	}

	public enum MessageBoxIcon
	{
		None = 0,
		Error = 16,
		Hand = 16,
		Stop = 16,
		Question = 32,
		Warning = 48,
		Exclamation = 48,
		Information = 64,
		Asterisk = 64
	}

	public enum DialogResult
	{
		None = 0,
		OK = 1,
		Cancel = 2,
		Abort = 3,
		Retry = 4,
		Ignore = 5,
		Yes = 6,
		No = 7
	}

	/// <summary>Stub of the WinForms Application class.</summary>
	public static class Application
	{
		/// <summary>Returns the base directory of the running application.</summary>
		public static string StartupPath => AppContext.BaseDirectory.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
	}

	/// <summary>Stub of the WinForms owner-window abstraction used by input device plugins.</summary>
	public interface IWin32Window
	{
		IntPtr Handle
		{
			get;
		}
	}
}