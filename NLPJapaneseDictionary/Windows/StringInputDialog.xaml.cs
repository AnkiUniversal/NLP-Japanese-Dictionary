using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NLPJapaneseDictionary.Windows
{
    public partial class StringInputDialog : Window
    {        
        public string InputText { get { return inputTextBox.Text; } }

        public StringInputDialog(Window mainWindow, string title, string defaultInput = "")
        {
            InitializeComponent();
            MainWindow.MoveSubWindowVerticalCenter(mainWindow, this);
            this.title.Text = title;
            inputTextBox.Text = defaultInput;
        }                

        private void OnOkDialogClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }        

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void OnWindowContentRendered(object sender, EventArgs e)
        {
            FocusOnTextBox();
        }

        private void FocusOnTextBox()
        {
            inputTextBox.SelectAll();
            inputTextBox.Focus();
        }
    }
}
