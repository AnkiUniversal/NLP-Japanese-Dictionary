/**
 * Copyright © 2017-2018 Anki Universal Team.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may
 * not use this file except in compliance with the License.  A copy of the
 * License is distributed with this work in the LICENSE.md file.  You may
 * also obtain a copy of the License from
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using NLPJapaneseDictionary.Core.Hooks;
using NLPJapaneseDictionary.NetTcp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
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

        public static ServiceHost NlpJdictService { get; set; }

        public App()
        {
        }

        private void OnApplicationExit(object sender, ExitEventArgs e)
        {
            if(NlpJdictService != null)
                NlpJdictService.Close();

            if (KeyboadHook != null)
                KeyboadHook.Dispose();

            if (ClipBoardHook != null)
                ClipBoardHook.Dispose();            
        }

        protected override void OnStartup(StartupEventArgs e)
        {            
            Process thisProc = Process.GetCurrentProcess();            
            if (Process.GetProcessesByName(thisProc.ProcessName).Length > 1)
            {
                HandleArgs(e);
                Application.Current.Shutdown();
                return;
            }
            
            base.OnStartup(e);
        }

        private static void HandleArgs(StartupEventArgs e)
        {
            if (e.Args.Length == 0)
            {
                MessageBox.Show("The app is already running.");
                return;
            }

            using (var client = new NlpJdictServiceProxy())
            {
                client.SearchText(e.Args[0]);
            }
        }
    }
}
