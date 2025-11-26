using System;
using System.Windows.Forms;
using OpenBveApi.Hosts;

namespace CarXmlConvertor
{
    static class Program
    {
	    internal static HostInterface CurrentHost;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
            CurrentHost = new Host();
        }
    }
}
