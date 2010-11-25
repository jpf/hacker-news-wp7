using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.IO;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Phone.Tasks;

namespace Hacker_News
{
    public class News
    {
        public string nextId { get; set; }
        public string version { get; set; }
        public ObservableCollection<Article> items { get; set; }
    }

    public class Article
    {
        public string title { get; set; }
        public string url { get; set; }
        public int id { get; set; }
        public int commentCount { get; set; }
        public int points { get; set; }
        public string postedAgo { get; set; }
        public string postedBy { get; set; }
    }

    public class AsyncState
    {
        public HttpWebRequest request { get; set; }
        public PivotItem page { get; set; }
    }

    public partial class MainPage : PhoneApplicationPage
    {
        // public News news_data;
        // public WebBrowserTask webBrowserTask = new WebBrowserTask();

        void processJsonString(StreamReader sr, PivotItem page)
        {
            News news_data = new News();
            Newtonsoft.Json.JsonSerializer json = new Newtonsoft.Json.JsonSerializer();
            // do i even need these?
            json.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            json.ObjectCreationHandling = Newtonsoft.Json.ObjectCreationHandling.Replace;
            json.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;
            json.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            Newtonsoft.Json.JsonTextReader reader = new JsonTextReader(sr);

            news_data = json.Deserialize<News>(reader);
            sr.Close(); 
            reader.Close();

            // I've got to do it to avoid thread locking issues ...
            // TODO: Remove this by making the News class an INotifyPropertyChanged type?
            this.Dispatcher.BeginInvoke(
                () =>
                {
                    // news.DataContext = news_data;
                    page.DataContext = news_data;
                }
            );
        }

        private void RequestCallback(IAsyncResult result)
        {
            var state = result.AsyncState as AsyncState;
            var request = state.request as HttpWebRequest;
            var page = state.page as PivotItem;
            var response = request.EndGetResponse(result);
            if (response != null)
            {
                Stream rv = response.GetResponseStream();
                UTF8Encoding encoding = new UTF8Encoding();
                StreamReader txt = new StreamReader(rv, encoding);
                processJsonString(txt, page);
            }
        }

        public void populatePageWithUrl(PivotItem page, string Url)
        {
            AsyncState state = new AsyncState();
            HttpWebRequest request = HttpWebRequest.Create(Url) as HttpWebRequest;
            request.Accept = "application/json"; //atom+xml";
            state.request = request;
            state.page = page;
            request.BeginGetResponse(RequestCallback, state);
        }
            
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            // news.DataContext = news_data;
            // fetchComments("http://api.ihackernews.com/post/1936607");

            populatePageWithUrl(news, "http://api.ihackernews.com/page");
            populatePageWithUrl(newest, "http://api.ihackernews.com/new");
            // populatePageWithUrl(comments, "http://api.ihackernews.com/newcomments");
            populatePageWithUrl(ask, "http://api.ihackernews.com/ask");

        }

        private void title_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selected_url = ((Hacker_News.Article)(((System.Windows.FrameworkElement)(sender)).DataContext)).url;
            var postId = ((Hacker_News.Article)(((System.Windows.FrameworkElement)(sender)).DataContext)).id;
            Post.id = postId;
            NavigationService.Navigate(new Uri("/Post.xaml", UriKind.Relative));

            // WebBrowserTask webBrowserTask = new WebBrowserTask();
            // webBrowserTask.URL = selected_url;
            // webBrowserTask.Show();
            Browser.url = selected_url;
            NavigationService.Navigate(new Uri("/Browser.xaml", UriKind.Relative));

        }
    }
}
