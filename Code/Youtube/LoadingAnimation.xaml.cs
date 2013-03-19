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

namespace Youtube
{
    /// <summary>
    /// Interaction logic for LoadingAnimation.xaml
    /// </summary>
    public partial class LoadingAnimation : UserControl
    {


        public RepeatBehavior Repeating
        {
            get
            {
                return ( RepeatBehavior )GetValue(RepeatingProperty);
            }
            set
            {
                SetValue(RepeatingProperty, value);
            }
        }
        
        // Using a DependencyProperty as the backing store for Repeating.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RepeatingProperty =
            DependencyProperty.Register("Repeating", typeof(RepeatBehavior), typeof(LoadingAnimation), new PropertyMetadata(RepeatBehavior.Forever));

        public Storyboard Story = null;
        public LoadingAnimation()
        {
            InitializeComponent();
        }
    }
}
