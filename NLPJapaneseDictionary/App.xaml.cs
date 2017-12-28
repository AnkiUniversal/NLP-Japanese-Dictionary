using NLPJapaneseDictionary.Core.Hooks;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace NLPJapaneseDictionary
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static KeyboardHook KeyboadHook { get; set; }
        public static ClipBoardHook ClipBoardHook { get; set; }

        public App()
        {
        }

        private void OnApplicationExit(object sender, ExitEventArgs e)
        {
            if (KeyboadHook != null)
                KeyboadHook.Dispose();

            if (ClipBoardHook != null)
                ClipBoardHook.Dispose();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // Get Reference to the current Process
            Process thisProc = Process.GetCurrentProcess();
            // Check how many total processes have the same name as the current one
            if (Process.GetProcessesByName(thisProc.ProcessName).Length > 1)
            {
                // If ther is more than one, than it is already running.
                MessageBox.Show("The app is already running.");
                Application.Current.Shutdown();
                return;
            }

            base.OnStartup(e);
        }
    }
}
