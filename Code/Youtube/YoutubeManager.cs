using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Net;
using System.Xml.Linq;
using System.Text.RegularExpressions;
namespace Youtube
{
    public delegate void YoutubeLoadedEventHandler(YoutubeLoadedEventArgs e);
    public enum As
    {
        Search,
        Related
    }
    public struct YoutubeLoadedEventArgs
    {
        public List<YoutubeInfo> Videos;
        public string SearchText;
        public int TotalResults;
        public int StartPage;
        public int Count;
        public As ReturnedFrom;
        private const int HashNumber = 19287;
        public static bool operator !=(YoutubeLoadedEventArgs a, YoutubeLoadedEventArgs b)
        {
            return !(a.SearchText == b.SearchText && a.StartPage == b.StartPage && a.Count == b.Count);
        }
        public static bool operator ==(YoutubeLoadedEventArgs a, YoutubeLoadedEventArgs b)
        {
            return !(a != b);
        }
        public override bool Equals(object obj)
        {
            if (!(obj is YoutubeLoadedEventArgs))
                return false;
            return this == ( YoutubeLoadedEventArgs )obj;
        }
        public override int GetHashCode()
        {
            var hash = HashNumber;
            hash = (hash * 9) + SearchText.GetHashCode();
            hash = (hash * 7) + StartPage.GetHashCode();
            hash = (hash * 5) + Count.GetHashCode();
            return hash;
        }
    }
    public struct YoutubeInfo
    {
        #region Data
        public string Link
        {
            get;
            set;
        }
        public string Embed
        {
            get;
            set;
        }
        public string Thumb
        {
            get;
            set;
        }
        public string Title
        {
            get;
            set;
        }
        public string Description
        {
            get;
            set;
        }
        public int Views
        {
            get;
            set;
        }
        public int Likes
        {
            get;
            set;
        }
        public int Dislikes
        {
            get;
            set;
        }
        public string Author
        {
            get;
            set;
        }
        public string PublishedDate
        {
            get;
            set;
        }
        public string UpdatedDate
        {
            get;
            set;
        }
        public string AuthorURI
        {
            get;
            set;
        }
        public override string ToString()
        {
            return "Link: " + Link + "\r\nEmbed: " + Embed + "\r\nThumb: " + Thumb + "\r\nTitle: " + Title + "\r\n";
        }
        #endregion
    }
    class YoutubeManager
    {
        #region Singleton
        private static YoutubeManager _instance = null;
        public static YoutubeManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new YoutubeManager();
                return _instance;
            }
        }
        #endregion
        private static readonly string SearchFormat = "http://gdata.youtube.com/feeds/api/videos?q={0}&start-index={2}&max-results={1}&v=2";
        //http://www.youtube.com/v/6M5q5TRuAsY?version=3&amp;f=videos&amp;app=youtube_gdata
        private static readonly Regex IsUrlKey = new Regex(@"youtu(?:\.be|be\.com).*?\/((?:v|embed)\/(.{11})|watch\?v=(.{11})|(.{11})$)", RegexOptions.IgnoreCase);
        private static readonly Regex GetUrlKey = new Regex(@"youtube\.com\/v\/(.{11})", RegexOptions.IgnoreCase);
        public event YoutubeLoadedEventHandler YoutubeLoadedEvent;

        public static bool IsYoutubeUrl(string url)
        {
            return IsUrlKey.Match(url).Success;
        }
        public static string GetVideoID(string url)
        {
            Match mch;
            if ((mch = GetUrlKey.Match(url)).Success)
                return mch.Groups[1].Value;
            if ((mch = IsUrlKey.Match(url)).Success)
                return mch.Groups[2].Value;
            return url;
        }
        public static string ToNormalUrl(string url)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("http://www.youtube.com/watch?v=");
            if (GetUrlKey.Match(url).Success || IsUrlKey.Match(url).Success)
            {
                sb.Append(GetVideoID(url));
                return sb.ToString();
            }
            else
            {
                Trace.WriteLine(url + " :: " + GetUrlKey.Match(url).Success + " :: " + GetUrlKey.ToString(), "Info");
            }
            return url;
        }
        public static string EmbedFromLink(string url)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("http://www.youtube.com/embed/");
            if (GetUrlKey.Match(url).Success || IsUrlKey.Match(url).Success)
            {
                sb.Append(GetVideoID(url));
                return sb.ToString();
            }
            else
            {
                Trace.WriteLine(url + " :: " + GetUrlKey.Match(url).Success + " :: " + GetUrlKey.ToString(), "Info");
            }
            return null;
        }
        private static string ThumbFromLink(XElement ele)
        {
            XElement group = null;
            string thumb = @"Images/logo.png";
            foreach (XElement desc in ele.Descendants())
                if (desc.Name.LocalName == "group")
                {
                    group = desc;
                    break;
                }
            if (group != null)
                foreach (XElement desc in group.Descendants())
                    if (desc.Name.LocalName == "thumbnail")
                    {
                        thumb = desc.Attribute("url").Value;
                    }
            return thumb;
        }
        private static XElement GetNthNode(XElement node, string match, int Nth = 1)
        {
            if (!node.HasElements)
            {
                if (node.Name.LocalName == match && Nth == 1)
                    return node;
                return null;
            }
            int pos = 0;
            foreach (XElement child in node.Elements())
            {
                if (child.Name.LocalName == match && ++pos == Nth)
                    return child;
            }
            return null;
        }
        private static XElement GetFirstNode(XElement node, string match)
        {
            return GetNthNode(node, match);
        }
        private static string Def(XElement el,string def)
        {
            return (el != null ? el.Value : def);
        }
        private static YoutubeInfo createYTItem(XElement item)
        {
            
            var blah = GetFirstNode(item, "content");
            var blah2 = "";
            if (blah != null)
                blah2 = blah.Attribute("src").Value;
            else
            {
                blah = GetFirstNode(item, "link");
                if (blah != null)
                    blah2 = blah.Attribute("href").Value;
                else
                    throw new ApplicationException("The fk yo");
            }
            XElement tel = GetFirstNode(item, "title"),
                     ael = GetNthNode(item, "author"),
                     vel = GetNthNode(item, "statistics"),
                     lel = GetNthNode(item, "rating", 2);
            if (lel == null)
            {
                lel = new XElement("rating");
                lel.SetAttributeValue("numDislikes", "0");
                lel.SetAttributeValue("numLikes", "0");
            }
            string link = ToNormalUrl(blah2),
                embed = EmbedFromLink(blah2)+"?autoplay=1",
                title =tel.Value,
                thumb = string.Format("http://i.ytimg.com/vi/{0}/mqdefault.jpg", GetVideoID(blah2)),
                author = Def(GetFirstNode(ael, "name"), "Noone"),
                authuri = "http://www.youtube.com/"+author,
                views = (vel.Attribute("viewCount") != null ? vel.Attribute("viewCount").Value : "0"),
                dislikes = (lel.Attribute("numDislikes") != null ? lel.Attribute("numDislikes").Value : "0"),
                likes = (lel.Attribute("numLikes") != null ? lel.Attribute("numLikes").Value : "0"),
                pub = Def(GetFirstNode(item, "published"), "0"),
                upd = Def(GetFirstNode(item, "updated"), "0"),
                desc = Def(GetFirstNode(GetFirstNode(item, "group"), "description"), "A youtube video");
            var x = new YoutubeInfo()
            {
                Link = link,
                Embed = embed,
                Title = title,
                Thumb = thumb,
                Author = author,
                AuthorURI = authuri,
                Description = desc,
                Views = int.Parse(views),
                Dislikes = int.Parse(dislikes),
                Likes = int.Parse(likes),
                PublishedDate = pub,
                UpdatedDate = upd
            };
            return x;
        }
        public static XElement LastSearch = null;
        public static XElement LastRelatedSearch = null;
        private static YoutubeInfo createRelatedItem(XElement root, string id)
        {
            var li = GetFirstNode(root, "link").Attribute("href").Value.Replace("http://www.youtube.com/watch?v=", "");
            li = li.Substring(0, 11);
            string link = "http://www.youtube.com/watch?v=" + li,
                  embed = "http://www.youtube.com/embed/" + li + "?autoplay=1",
                  title = Def(GetFirstNode(root, "title"), "A youtube video"),
                  thumb = string.Format("http://i.ytimg.com/vi/{0}/mqdefault.jpg", li),
                  auth = Def(GetFirstNode(GetFirstNode(root, "author"), "name"), "Noone"),
                  auri = "http://www.youtube.com/"+auth,
                  desc = Def(GetFirstNode(root, "content"), "A youtube video"),
                  views = GetFirstNode(root, "statistics").Attribute("viewCount").Value,
                  pubd = GetFirstNode(root, "published").Value,
                  updd = GetFirstNode(root, "updated").Value;
            var x = new YoutubeInfo()
                {
                    Link = link,
                    Embed = embed,
                    Title = title,
                    Thumb = thumb,
                    Author = auth,
                    AuthorURI = auri,
                    Description = desc,
                    Views = int.Parse(views),
                    Likes = 0,
                    Dislikes = 0,
                    PublishedDate = pubd,
                    UpdatedDate = updd
                };
            return x;
        }
        public static List<YoutubeInfo> GetRelatedVideoInfo(string id, int count = 25, int start = 1)
        {
            try
            {
                var root = LastRelatedSearch = XElement.Load(string.Format("http://gdata.youtube.com/feeds/api/videos/{0}/related", id));
                var links = (from item in root.Elements()
                             where item.Name.LocalName == "entry"
                             select (createRelatedItem(item, id))).Take(count);
                return links.ToList<YoutubeInfo>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            return null;
        }
        public event YoutubeLoadedEventHandler YoutubeLoadedRelatedEvent;
        public static YoutubeLoadedEventArgs LastRelatedEvent = default(YoutubeLoadedEventArgs);
        public void LoadRelatedVideos(string id)
        {
            if (YoutubeLoadedRelatedEvent != null)
            {
                var v = GetRelatedVideoInfo(id);
                YoutubeLoadedRelatedEvent(LastRelatedEvent = new YoutubeLoadedEventArgs()
                {
                    Videos = v,
                    SearchText = "Related Videos",
                    TotalResults = int.Parse(GetFirstNode(LastRelatedSearch, "totalResults").Value),
                    Count = v.Count,
                    StartPage = 1,
                    ReturnedFrom = As.Related
                });
            }
        }
        public static List<YoutubeInfo> GetVideoInfo(string key, int count = 30, int start = 1)
        {
            try
            {
                Console.WriteLine(string.Format(SearchFormat, key.Replace(" ", "+"), count, (count * (start - 1)) + 1));
                var root = LastSearch = XElement.Load(string.Format(SearchFormat, key.Replace(" ", "+"), count, ( count * (start - 1) ) + 1));
                var links = (from item in root.Elements()
                             where item.Name.LocalName == "entry"
                             select (createYTItem(item))).Take(count);
                return links.ToList<YoutubeInfo>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            return null;
        }
        public static YoutubeLoadedEventArgs LastEvent = default(YoutubeLoadedEventArgs);
        public void LoadVideos(string key, int count = 30, int start = 1)
        {
            if (YoutubeLoadedEvent != null)
            {
                var v = GetVideoInfo(key, count, start);
                YoutubeLoadedEvent(LastEvent = new YoutubeLoadedEventArgs()
                {
                    Videos = v,
                    SearchText = key,
                    TotalResults = int.Parse(GetFirstNode(LastSearch, "totalResults").Value),
                    Count = v.Count,
                    StartPage = start,
                    ReturnedFrom = As.Search
                });
            }
        }
    }
}
