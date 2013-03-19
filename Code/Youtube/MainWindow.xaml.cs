using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
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
using MahApps.Metro.Controls;
using SharpDX;
namespace Youtube
{
    public static class GridExtensions
    {
        public static UIElement GetElement(this Grid g, int row, int column)
        {
            return g.Children
                    .Cast<UIElement>()
                    .First(e => Grid.GetRow(e) == row &&
                                Grid.GetColumn(e) == column);
        }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : MetroWindow
    {
        //System.Collections.ObjectModel.ObservableCollection<TabItem> Pages = new System.Collections.ObjectModel.ObservableCollection<TabItem>();
        YoutubeManager YTube = YoutubeManager.Instance;
        private void RunBG(Action a)
        {
            Task.Factory.StartNew(a);
        }
        private void RunUI(Action a)
        {
            this.Dispatcher.Invoke(a);
        }
        public string StatusText
        {
            get
            {
                return ( string )GetValue(StatusTextProperty);
            }
            set
            {
                SetValue(StatusTextProperty, value);
            }
        }
        private TabItem LastSelected = null;
        private TabItem CurrentSelected = null;
        private TabItem LastRelatedSelected = null;
        private TabItem CurrentRelatedSelected = null;
        public static Brush SelectedFillBrush
        {
            get
            {
                return new SolidColorBrush(System.Windows.Media.Color.FromRgb(92,207,219));
            }
        }
        public static Brush UnSelectedFillBrush
        {
            get
            {
                return new SolidColorBrush(System.Windows.Media.Color.FromRgb(8, 111, 158));
            }
        }
        Dictionary<Image, YoutubeInfo> relatedImageInfo = new Dictionary<Image, YoutubeInfo>();
        Dictionary<Image, YoutubeInfo> imageInfo = new Dictionary<Image, YoutubeInfo>();
        // Using a DependencyProperty as the backing store for StatusText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StatusTextProperty =
            DependencyProperty.Register("StatusText", typeof(string), typeof(MainWindow), new PropertyMetadata("Ready"));



        public YoutubeInfo CurrentVideoInfo
        {
            get
            {
                return ( YoutubeInfo )GetValue(CurrentVideoInfoProperty);
            }
            set
            {
                SetValue(CurrentVideoInfoProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for CurrentVideoInfo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentVideoInfoProperty =
            DependencyProperty.Register("CurrentVideoInfo", typeof(YoutubeInfo), typeof(MainWindow), new PropertyMetadata(default(YoutubeInfo)));

        
        public bool RelatedListEnabled
        {
            get
            {
                return ( bool )GetValue(RelatedListEnabledProperty);
            }
            set
            {
                SetValue(RelatedListEnabledProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for RelatedListEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RelatedListEnabledProperty =
            DependencyProperty.Register("RelatedListEnabled", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        
        public string TitleString
        {
            get
            {
                return ( string )GetValue(TitleStringProperty);
            }
            set
            {
                string str = string.IsNullOrWhiteSpace(value) ? "" : " - " + value;
                if (str.Length > 40)
                    str = str.Substring(0, 37) + "...";
                SetValue(TitleStringProperty, "YoutubeApp" + str);
            }
        }

        // Using a DependencyProperty as the backing store for TitleString.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleStringProperty =
            DependencyProperty.Register("TitleString", typeof(string), typeof(MainWindow), new PropertyMetadata("YoutubeApp"));


        public double STextLength
        {
            get
            {
                return Width * 4 / 5;
            }
        }
        public MainWindow()
        {
            InitializeComponent();

            YTube.YoutubeLoadedEvent += YTube_YoutubeLoadedEvent;
            YTube.YoutubeLoadedRelatedEvent += YTube_YoutubeLoadedRelatedEvent;
            //VideoBrowser.
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }

        void YTube_YoutubeLoadedRelatedEvent(YoutubeLoadedEventArgs e)
        {
            RunUI(() => 
                {
                    StatusText = "Loading Related Video Thumbnails...";
                    RelatedListEnabled = false;
                });
            var vids = e.Videos;
            if (vids.Count == 0)
            {
                RunUI(() =>
                {
                    StatusText = "No videos found matching " + e.SearchText;
                    Loading.Visibility = System.Windows.Visibility.Hidden;
                });
                return;
            }
            var location = 0;
            RunUI(() =>
            {
                RelatedVideos.Items.Clear();
                relatedImageInfo.Clear();
            });
            for (int i = 0; i < (vids.Count / 6 > 4 ? 4 : vids.Count / 6); i++)
            {

                TabItem page = null;
                ImagePage images = null;
                RunUI(() =>
                {
                    page = new TabItem();
                    page.Header = new PageHeader();
                    (( PageHeader )page.Header).Foreground = i == 0 ? SelectedFillBrush : UnSelectedFillBrush;
                    if (i == 0)
                    {
                        LastRelatedSelected = page;
                        CurrentRelatedSelected = page;
                    }
                    images = new ImagePage();
                });
                for (int j = 0; j < 6; j++)
                {
                    location++;
                    RunUI(() =>
                    {
                        images.Images[j].MouseLeftButtonUp += ShowRelatedVideo;
                        loadImage(images.Images[j], e.Videos[location - 1], true);
                        var ctx = new ContextMenu();
                        var play = new MenuItem();
                        play.Header = "Play";
                        play.Click += (s, evt) =>
                        {

                            ShowRelatedVideo((( MenuItem )s).GetOwner(), null);
                        };
                        ctx.Items.Add(play);
                        var openInBrowser = new MenuItem();
                        openInBrowser.Header = "Open In Browser";
                        openInBrowser.Click += (s, evt) =>
                        {
                            System.Diagnostics.Process.Start(relatedImageInfo[( Image )(( MenuItem )s).GetOwner()].Link);
                        };
                        ctx.Items.Add(openInBrowser);
                        var addToClip = new MenuItem();
                        addToClip.Header = "Copy Link to Clipboard";
                        addToClip.Click += (s, evt) =>
                        {
                            Clipboard.SetText(relatedImageInfo[( Image )(( MenuItem )s).GetOwner()].Link);
                        };
                        ctx.Items.Add(addToClip);
                        images.Images[j].ContextMenu = ctx;
                    });
                }
                RunUI(() =>
                {
                    page.Content = images;
                    RelatedVideos.Items.Add(page);
                });
            }
            RunUI(() =>
            {
                if (relatedImageInfo.Count > 0)
                    RelatedListEnabled = true;
                StatusText = "Ready";
                Loading.Visibility = System.Windows.Visibility.Hidden;
            });
        }
        private void ShowSettings(object sender, RoutedEventArgs e)
        {
        }
        private void SaveCurrent(object sender, RoutedEventArgs e)
        {

        }
        private void SearchVideosK(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SearchVideos(sender, null);
        }
        private void SearchVideos(object sender, RoutedEventArgs e)
        {
            if (YoutubeManager.LastEvent != default(YoutubeLoadedEventArgs))
                if (SearchText.Text == YoutubeManager.LastEvent.SearchText)
                    return;
            string search = SearchText.Text;
            Loading.Visibility = System.Windows.Visibility.Visible;
            StatusText = "Searching videos... Search: " + search;

            RunBG(() =>
            {
                YTube.LoadVideos(search, 30);
            });
        }
        static System.Text.RegularExpressions.Regex ValidImage = new System.Text.RegularExpressions.Regex(@"i\.ytimg\.com\/vi\/.{11}\/.+?default.jpg$");
        void loadImage(Image img, YoutubeInfo info, bool related = false)
        {
            BitmapImage src = new BitmapImage();
            src.BeginInit();
            if (ValidImage.Match(info.Thumb).Success)
            {
                src.UriSource = new Uri(info.Thumb);
            }
            else
            {
                Console.WriteLine(info.Thumb);
                src.UriSource = new Uri("http://www.youtube.com/yt/brand/media/image/yt-brand-standard-logo-630px.png");
            }
            src.EndInit();
            if (related)
                relatedImageInfo[img] = info;
            else
                imageInfo[img] = info;
            img.Source = src;
            var tip = new ToolTip();
            tip.Content = info.Title;
            img.ToolTip = tip;
        }
        void YTube_YoutubeLoadedEvent(YoutubeLoadedEventArgs e)
        {
            RunUI(() => StatusText = "Loading Thumbnails...");
            var vids = e.Videos;
            if (vids.Count == 0)
            {
                RunUI(()=>
                {
                    StatusText = "No videos found matching " + e.SearchText;
                    Loading.Visibility = System.Windows.Visibility.Hidden;
                });
                return;
            }
            var location = 0;
            RunUI(() =>
            {
                VideoList.Items.Clear();
                imageInfo.Clear();
            });
            for (int i = 0; i < (vids.Count / 6 > 5 ? 5 : vids.Count / 6); i++)
            {

                TabItem page = null;
                ImagePage images = null;
                RunUI(() =>
                {
                    page = new TabItem();
                    page.Header = new PageHeader();
                    (( PageHeader )page.Header).Foreground = i == 0 ? SelectedFillBrush : UnSelectedFillBrush;
                    if (i == 0)
                    {
                        LastSelected = page;
                        CurrentSelected = page;
                    }
                    images = new ImagePage();
                });
                for (int j = 0; j < 6; j++)
                {
                    location++;
                    RunUI(() =>
                    {
                        images.Images[j].MouseLeftButtonUp += ShowVideo;
                        loadImage(images.Images[j], e.Videos[location - 1]);
                        var ctx = new ContextMenu();
                        var play = new MenuItem();
                        play.Header = "Play";
                        play.Click += (s, evt) =>
                        {

                            ShowVideo((( MenuItem )s).GetOwner(), null);
                        };
                        ctx.Items.Add(play);
                        var openInBrowser = new MenuItem();
                        openInBrowser.Header = "Open In Browser";
                        openInBrowser.Click += (s, evt) =>
                            {
                                System.Diagnostics.Process.Start(imageInfo[( Image )(( MenuItem )s).GetOwner()].Link);
                            };
                        ctx.Items.Add(openInBrowser);
                        var addToClip = new MenuItem();
                        addToClip.Header = "Copy Link to Clipboard";
                        addToClip.Click += (s, evt) =>
                            {
                                Clipboard.SetText(imageInfo[(Image)((MenuItem)s).GetOwner()].Link);
                            };
                        ctx.Items.Add(addToClip);
                        images.Images[j].ContextMenu = ctx;
                    });
                }
                RunUI(() =>
                {
                    page.Content = images;
                    VideoList.Items.Add(page);
                });
            }
            RunUI(() =>
                {
                    StatusText = "Ready";
                    Loading.Visibility = System.Windows.Visibility.Hidden;
                });
        }
        private void ShowVideo(object sender, MouseButtonEventArgs e)
        {
            if (e != null)
                if (e.ChangedButton == MouseButton.Right)
                    return;
            TitleString = imageInfo[( Image )sender].Title;
            VideoBrowser.Navigate(new Uri(imageInfo[( Image )sender].Embed, UriKind.Absolute));
            CurrentVideoInfo = imageInfo[( Image )sender];
            DescPage.Update(CurrentVideoInfo);
            DescPage.Description += "\r\n\r\n***CLIENT NOTE: Descriptions will not update for videos clicked directly from the end of video list.\r\nIf you wish to view Description information for such videos please view it from searched videos or related videos.";
            RunBG(()=>YTube.LoadRelatedVideos(YoutubeManager.GetVideoID(imageInfo[( Image )sender].Embed)));
        }
        private void ShowRelatedVideo(object sender, MouseButtonEventArgs e)
        {
            if (e != null)
                if (e.ChangedButton == MouseButton.Right)
                    return;
            TitleString = relatedImageInfo[( Image )sender].Title;
            VideoBrowser.Navigate(new Uri(relatedImageInfo[( Image )sender].Embed, UriKind.Absolute));
            CurrentVideoInfo = relatedImageInfo[( Image )sender];
            DescPage.Update(CurrentVideoInfo);
            DescPage.Description += "\r\n\r\n***CLIENT NOTE: Likes and Dislikes are not currently supported for related videos.";
            DescPage.Description += "\r\n\r\n***CLIENT NOTE: Descriptions will not update for videos clicked directly from the end of video list.\r\nIf you wish to view Description information for such videos please view it from searched videos or related videos.";
            RunBG(() => YTube.LoadRelatedVideos(YoutubeManager.GetVideoID(relatedImageInfo[( Image )sender].Embed)));
        }
        private void MakeBrowserVisible(object sender, NavigationEventArgs e)
        {
            VideoBrowser.Visibility = System.Windows.Visibility.Visible;
            RelatedTab.Visibility = System.Windows.Visibility.Visible;
            DescriptionTab.Visibility = System.Windows.Visibility.Visible;
            Console.WriteLine("NAVIGATED");
        }
        private void RelatedList_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

            var next = ( TabItem )(( TabControl )e.Source).SelectedItem;
            if (next == null)
                return;
            LastRelatedSelected = CurrentRelatedSelected;
            CurrentRelatedSelected = next;
            (( PageHeader )LastRelatedSelected.Header).Foreground = UnSelectedFillBrush;
            (( PageHeader )next.Header).Foreground = SelectedFillBrush;
        }
        private void VideoList_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

            var next = ( TabItem )(( TabControl )e.Source).SelectedItem;
            if (next == null)
                return;
            LastSelected = CurrentSelected;
            CurrentSelected = next;
            (( PageHeader )LastSelected.Header).Foreground = UnSelectedFillBrush;
            (( PageHeader )next.Header).Foreground = SelectedFillBrush;
        }
    }
    public static class MenuItemExtensions
    {
        public static object GetOwner(this MenuItem item)
        {
            var it = item;
            while (it.Parent is MenuItem)
                it = ( MenuItem )item.Parent;
            var menu = it.Parent as ContextMenu;
            if (menu != null)
            {
                return menu.PlacementTarget as object;
            }
            return menu;
        }
    }
}
