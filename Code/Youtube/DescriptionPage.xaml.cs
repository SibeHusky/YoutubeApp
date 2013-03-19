using System;
using System.Collections.Generic;
using System.Globalization;
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
    public partial class DescriptionPage : UserControl
    {
        #region Depenency Properties

        public string Author
        {
            get
            {
                return ( string )GetValue(AuthorProperty);
            }
            set
            {
                SetValue(AuthorProperty, value);
            }
        }
        public static readonly DependencyProperty AuthorProperty =
            DependencyProperty.Register("Author", typeof(string), typeof(DescriptionPage), new PropertyMetadata("Noone"));

        public int Views
        {
            get
            {
                return ( int )GetValue(ViewsProperty);
            }
            set
            {
                SetValue(ViewsProperty, value);
            }
        }
        public static readonly DependencyProperty ViewsProperty =
            DependencyProperty.Register("Views", typeof(int), typeof(DescriptionPage), new PropertyMetadata(0));

        public string Title
        {
            get
            {
                return ( string )GetValue(TitleProperty);
            }
            set
            {
                SetValue(TitleProperty, value);
            }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(DescriptionPage), new PropertyMetadata("Youtube Video"));

        public string Description
        {
            get
            {
                return ( string )GetValue(DescriptionProperty);
            }
            set
            {
                SetValue(DescriptionProperty, value);
            }
        }
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(DescriptionPage), new PropertyMetadata(""));

        public int Likes
        {
            get
            {
                return ( int )GetValue(LikesProperty);
            }
            set
            {
                SetValue(LikesProperty, value);
                SetValue(LikesPercentageProperty, (double)value / (double)(Dislikes + value));
            }
        }
        public static readonly DependencyProperty LikesProperty =
            DependencyProperty.Register("Likes", typeof(int), typeof(DescriptionPage), new PropertyMetadata(0));

        public double LikesPercentage
        {
            get
            {
                return ( double )GetValue(LikesPercentageProperty.DependencyProperty);
            }
        }
        public static readonly DependencyPropertyKey LikesPercentageProperty =
            DependencyProperty.RegisterReadOnly("LikesPercentage", typeof(double), typeof(DescriptionPage), new PropertyMetadata(0.0));

        public int Dislikes
        {
            get
            {
                return ( int )GetValue(DislikesProperty);
            }
            set
            {
                SetValue(DislikesProperty, value);
                SetValue(LikesPercentageProperty, (double)Likes / (double)(Likes + value));
            }
        }
        public static readonly DependencyProperty DislikesProperty =
            DependencyProperty.Register("Dislikes", typeof(int), typeof(DescriptionPage), new PropertyMetadata(0));

        public DateTime PublishedDate
        {
            get
            {
                return ( DateTime )GetValue(PublishedDateProperty);
            }
            set
            {
                SetValue(PublishedDateProperty, value);
            }
        }
        public static readonly DependencyProperty PublishedDateProperty =
            DependencyProperty.Register("PublishedDate", typeof(DateTime), typeof(DescriptionPage), new PropertyMetadata(DateTime.Now));

        public DateTime UpdatedDate
        {
            get
            {
                return ( DateTime )GetValue(UpdatedDateProperty);
            }
            set
            {
                SetValue(UpdatedDateProperty, value);
            }
        }
        public static readonly DependencyProperty UpdatedDateProperty =
            DependencyProperty.Register("UpdatedDate", typeof(DateTime), typeof(DescriptionPage), new PropertyMetadata(DateTime.Now));
     
        #endregion

        #region Links

        public string TitleLink
        {
            get;
            set;
        }
        public string AuthorLink
        {
            get;
            set;
        }

        #endregion

        public DescriptionPage()
        {
            InitializeComponent();
        }
        public DescriptionPage(YoutubeInfo info) 
        {
            Update(info);
        }

        public void Update(YoutubeInfo info)
        {
            InitializeComponent();
            Dislikes = info.Dislikes;
            Description = info.Description;
            Likes = info.Likes;
            PublishedDate = DateTime.Parse(info.PublishedDate);
            UpdatedDate = DateTime.Parse(info.UpdatedDate);
            Title = info.Title;
            Views = info.Views;
            Author = info.Author;
            AuthorLink = info.AuthorURI;
            TitleLink = info.Link;
        }

        private void NavigateToTitleLink(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(TitleLink);
        }
        private void NavigateToAuthorLink(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(AuthorLink);
        }
    }

    #region Converters
    [ValueConversion(typeof(int), typeof(string))]
    public class LikesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return "Likes: " + (( int )value).ToString("N0");
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return int.Parse((( string )value).Substring(7));
        }
    }
    [ValueConversion(typeof(int), typeof(string))]
    public class ViewsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return "Views: " + (( int )value).ToString("N0");
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return int.Parse((( string )value).Substring(7));
        }
    }
    [ValueConversion(typeof(int), typeof(string))]
    public class DislikesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return "Dislikes: " + (( int )value).ToString("N0");
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return int.Parse((( string )value).Substring(10));
        }
    }
    [ValueConversion(typeof(DateTime), typeof(string))]
    public class UpdatedDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return "Updated on " + (( DateTime )value).ToString("MMM dd, yyyy");
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return DateTime.Parse((( string )value).Substring(11));
        }
    }
    [ValueConversion(typeof(DateTime), typeof(string))]
    public class PublishedDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return "Published on " + (( DateTime )value).ToString("MMM dd, yyyy");
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return DateTime.Parse((( string )value).Substring(13));
        }
    }
    #endregion
}
