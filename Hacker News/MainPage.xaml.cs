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
    public class News : INotifyPropertyChanged
    {
        private string nextIdValue = String.Empty;
        private string versionValue = String.Empty;
        private ObservableCollection<Article> itemsValue = new ObservableCollection<Article>();

        #region I don't really understand what's going on here ...
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion

        public string nextId
        {
            get { return this.nextIdValue; }
            set { this.nextIdValue = value; NotifyPropertyChanged("nextId"); }
        }
        public string version
        {
            get { return this.versionValue; }
            set { this.versionValue = value; NotifyPropertyChanged("version"); }
        }

        public ObservableCollection<Article> items
        {
            get { return this.itemsValue; }
            set { this.itemsValue = value; NotifyPropertyChanged("items"); }
        }
    }
    public class Article
    {
        Uri urlValue = null;
        public string title { get; set; }
        public string urlDomainOnly
        {
            get
            {
                if (this.urlValue == null) { return ""; }
                string[] hostParts = this.urlValue.Host.Split('.');
                int len = hostParts.Length;
                if (len > 1)
                {
                    return hostParts[len - 2] + "." + hostParts[len - 1];
                }
                else
                {
                    return this.urlValue.Host;
                }
            }
        }
        public string url
        {
            get { return this.urlValue.ToString(); }
            set { this.urlValue = new Uri(value); }
        }
        public int id { get; set; }
        public int commentCount { get; set; }
        public int points { get; set; }
        public string postedAgo { get; set; }
        public string postedBy { get; set; }
    }

    public partial class MainPage : PhoneApplicationPage
    {
        private News news_page_data = new News();
        private News ask_page_data = new News();
        private News new_page_data = new News();

        public void setProgressBar(Boolean state)
        {
            progressBar.IsIndeterminate = state;
            progressBar.Visibility = (state == true) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void HandleNewsResult(IAsyncResult result)
        {
            try
            {
                Common common = new Common();
                var binding = (result.AsyncState as AsyncState).binding as News;
                StreamReader txt = common.makeStreamReaderFromResult(result);

                News rv = common.deserializeStreamReader<News>(txt);
                this.Dispatcher.BeginInvoke(
                () =>
                {
                    binding.nextId = rv.nextId;
                    binding.version = rv.version;
                    binding.items = rv.items;
                    // FIXME: Shouldn't I be able to do this instead?
                    //binding = processJsonString(txt);
                    setProgressBar(false);
                }
                );
            }
            catch (WebException e)
            {
                this.Dispatcher.BeginInvoke(
                    () =>
                    {
                        errorLine.Text = e.Message;
                        errorLine.Visibility = Visibility.Visible;
                        setProgressBar(false);
                    }
                );
            }
        }

        public void populateBinding(News binding, string Url)
        {
            AsyncState state = new AsyncState();
            HttpWebRequest request = HttpWebRequest.Create(Url) as HttpWebRequest;
            request.Accept = "application/json"; //atom+xml";
            state.request = request;
            state.binding = binding;
            request.BeginGetResponse(HandleNewsResult, state);
        }

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            news.DataContext = news_page_data;
            ask.DataContext = ask_page_data;
            newest.DataContext = new_page_data;
        }


        private Article whatSelected(object sender)
        {
            return ((Hacker_News.Article)(((System.Windows.FrameworkElement)(sender)).DataContext));
        }

        private void commentsClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selected = whatSelected(sender);
            Post.id = selected.id;
            NavigationService.Navigate(new Uri("/Post.xaml", UriKind.Relative));
        }

        private void titleClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selected = whatSelected(sender);
            #region If launching the WebBrowser actually worked, this is how we'd do it.
            // WebBrowserTask webBrowserTask = new WebBrowserTask();
            // webBrowserTask.URL = selected_url;
            // webBrowserTask.Show();
            #endregion
            if (selected.url.StartsWith("http://"))
            {
                Browser.url = selected.url;
                NavigationService.Navigate(new Uri("/Browser.xaml", UriKind.Relative));
            }
            else // special case for "Ask HN" style links.
            {
                commentsClicked(sender, e);
            }
        }
        private void PivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            setProgressBar(true);
            var selected = ((Microsoft.Phone.Controls.PivotItem)(((Microsoft.Phone.Controls.Pivot)(sender)).SelectedItem)).Header as string;
            switch (selected)
            {
                case "news":
                    populateBinding(news_page_data, "http://api.ihackernews.com/page");
                    break;
                case "new":
                    populateBinding(new_page_data, "http://api.ihackernews.com/new");
                    break;
                case "ask":
                    populateBinding(ask_page_data, "http://api.ihackernews.com/ask");
                    break;
                //case "comments":
                //    populateBinding(comments, "http://api.ihackernews.com/newcomments");
            }
        }
    }
}
