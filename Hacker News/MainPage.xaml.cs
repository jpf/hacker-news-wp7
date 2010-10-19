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

namespace Hacker_News
{
    public class News
    {
        public string nextId { get; set; }
        public string version { get; set; }
        public ObservableCollection<Article> items { get; set; }
    }

    public class Article : INotifyPropertyChanged
    {
        public string title { get; set; }
        public string url { get; set; }
        public int id { get; set; }
        public int commentCount { get; set; }
        public int points { get; set; }
        public string postedAgo { get; set; }
        public string postedBy { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }

    public partial class MainPage : PhoneApplicationPage
    {
        void processJsonString(StreamReader sr)
        {
            News news_data;
            
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
            this.Dispatcher.BeginInvoke(
                () =>
                {
                    news.DataContext = news_data;
                    newest.DataContext = news_data;
                }
            );
        }

        private void RequestCallback(IAsyncResult result)
        {
            var request = result.AsyncState as HttpWebRequest;
            var response = request.EndGetResponse(result);
            if (response != null)
            {
                Stream rv = response.GetResponseStream();
                UTF8Encoding encoding = new UTF8Encoding();
                StreamReader txt = new StreamReader(rv, encoding);
                processJsonString(txt);
            }
        }
            
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            HttpWebRequest request = HttpWebRequest.Create("http://api.ihackernews.com/page") as HttpWebRequest;
            request.Accept = "application/json"; //atom+xml";
            request.BeginGetResponse(RequestCallback, request);
        }
    }
}
