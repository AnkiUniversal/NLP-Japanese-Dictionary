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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NLPJapaneseDictionary.UserControls
{
    /// <summary>
    /// Interaction logic for HoykeysPopup.xaml
    /// </summary>
    public partial class HotkeysPopup : UserControl
    {
        public HotkeysPopup()
        {
            InitializeComponent();
        }

        public void Show()
        {
            DetectHotkeysText();
            popup.IsOpen = true;
        }

        private void DetectHotkeysText()
        {
            if (MainWindow.UserPrefs.IsOmitCtrl)
            {
                if (ctrlText.Visibility == Visibility.Visible)
                {
                    ctrlText.Visibility = Visibility.Collapsed;
                    omitCtrlText.Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (ctrlText.Visibility == Visibility.Collapsed)
                {
                    ctrlText.Visibility = Visibility.Visible;
                    omitCtrlText.Visibility = Visibility.Collapsed;
                }
            }
        }

        public void Hide()
        {
            popup.IsOpen = false;
        }
    }
}
