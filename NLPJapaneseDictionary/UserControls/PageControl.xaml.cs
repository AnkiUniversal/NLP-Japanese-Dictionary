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
    /// Interaction logic for PageControl.xaml
    /// </summary>
    public partial class PageControl : UserControl
    {
        public delegate void PageChangedHandler(int newStartIndex, int length);
        public event PageChangedHandler PageChanged;

        private int itemsPerPage;
        public int ItemsPerPage
        {
            get { return itemsPerPage; }
            set { itemsPerPage = value; }
        }

        private int numberOfItem;
        public int NumberOfItem
        {
            get { return numberOfItem; }
            set
            {
                numberOfItem = value;
                numberOfItemTextBlock.Text = numberOfItem.ToString();
            }
        }

        private int currentPage;
        public int CurrentPage
        {
            get { return currentPage; }
            set
            {
                currentPage = value;
                currentPageTextBlock.Text = currentPage.ToString();
            }
        }

        private int numberOfPage;
        public int NumberOfPage
        {
            get { return numberOfPage; }
            set
            {
                numberOfPage = value;
                numberOfPageTextBlock.Text = numberOfPage.ToString();
            }
        }

        public PageControl()
        {
            this.InitializeComponent();
        }

        public void Hide()
        {
            pageButtonRoot.Visibility = Visibility.Collapsed;
        }

        public void ChangeNumberOfItem(int numberOfItem)
        {
            NumberOfItem = numberOfItem;
            CurrentPage = 1;

            NumberOfPage = (int)Math.Ceiling((double)NumberOfItem / ItemsPerPage);

            if (numberOfPage < 2)
                pageButtonRoot.Visibility = Visibility.Collapsed;
            else
                pageButtonRoot.Visibility = Visibility.Visible;

            FirePageChangedEvent();
        }

        private void OnPreviousButtonClick(object sender, RoutedEventArgs e)
        {
            if (CurrentPage < 2)
                return;

            CurrentPage--;
            FirePageChangedEvent();
        }

        private void OnNextButtonClick(object sender, RoutedEventArgs e)
        {
            if (CurrentPage >= NumberOfPage)
                return;

            CurrentPage++;
            FirePageChangedEvent();
        }

        private void FirePageChangedEvent()
        {
            int startIndex = (CurrentPage - 1) * ItemsPerPage;
            int length = ItemsPerPage;
            int remain = numberOfItem - startIndex;
            if (remain < length)
                length = remain;

            PageChanged?.Invoke(startIndex, length);
        }
    }
}
