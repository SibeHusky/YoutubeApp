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

namespace Youtube
{
    /// <summary>
    /// Interaction logic for ImagePage.xaml
    /// </summary>
    public partial class ImagePage : UserControl
    {
        Image[] _imageList;
        public Image[] Images
        {
            get
            {
                return _imageList;
            }
        }
        public ImagePage()
        {
            InitializeComponent();
            _imageList = new Image[] { First, Second, Third, Fourth, Fifth, Sixth };
        }
    }
}
