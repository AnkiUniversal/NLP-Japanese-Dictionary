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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NLPJapaneseDictionary.UserControls
{
    public partial class ProgressRing : UserControl
    {
        Storyboard loadingAnimation;
        Storyboard fadeOutAnimation;

        public ProgressRing()
        {
            InitializeComponent();
            loadingAnimation = (Storyboard)TryFindResource("Loading");
            fadeOutAnimation = (Storyboard)TryFindResource("FadeOut");
            this.Visibility = Visibility.Collapsed;
        }

        public void StartAnimation()
        {
            this.Visibility = Visibility.Visible;
            loadingAnimation.Begin();
        }

        public void StopAnimation()
        {
            if(this.Visibility == Visibility.Visible)
                fadeOutAnimation.Begin();
        }

        private void OnFadeOutStoryboardCompleted(object sender, EventArgs e)
        {
            loadingAnimation.Stop();
            this.Visibility = Visibility.Collapsed;
        }
    }
}
