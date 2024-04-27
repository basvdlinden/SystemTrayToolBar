using System;
using System.Windows.Forms;

namespace SystemTrayToolBar
{
    internal static class Program
    {
        public static object ApplicationConfiguration { get; private set; }

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SystemTrayToolBarApplicationContext());
        }
    }
}