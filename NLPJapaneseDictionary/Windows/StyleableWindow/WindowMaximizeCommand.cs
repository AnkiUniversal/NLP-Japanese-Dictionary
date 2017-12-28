using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NLPJapaneseDictionary.StyleableWindow
{
    public class WindowMaximizeCommand :ICommand
    {     

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged { add { } remove { } }

        public void Execute(object parameter)
        {
            var window = parameter as Window;

            var maximizeButtonMainWindow = (Button)window.Template.FindName("maximizeButton", window);

            if (window != null)
            {
                if(window.WindowState == WindowState.Maximized)
                {
                    window.WindowState = WindowState.Normal;
                    maximizeButtonMainWindow.Content = Application.Current.Resources["MaximizeButtonPath"];
                }
                else
                {
                    window.WindowState = WindowState.Maximized;
                    maximizeButtonMainWindow.Content = Application.Current.Resources["RestoreButtonPath"];
                }                
            }
        }
    }
}
