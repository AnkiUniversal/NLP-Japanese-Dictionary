using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NLPJapaneseDictionary.StyleableWindow
{
    public class WindowCloseCommand :ICommand
    {     

        public bool CanExecute(object parameter)
        {
            return true;
        }
        
        public event EventHandler CanExecuteChanged { add { } remove { } }

        public void Execute(object parameter)
        {            
            var window = parameter as Window;

            if (window != null)
            {
                window.Close();
            }            
        }
    }
}
