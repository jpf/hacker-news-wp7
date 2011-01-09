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
using Hacker_News.Entities;
using Microsoft.Phone.Controls;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.IO;
using System.Text;
using Microsoft.Phone.Tasks;

namespace Hacker_News
{
    public partial class MainPage : PhoneApplicationPage
    {
        private News news_page_data = new News();
        private News ask_page_data = new News();
        private News new_page_data = new News();

        public void SetProgressBar(Boolean state)
        {
            progressBar.IsIndeterminate = state;
            progressBar.Visibility = (state == true) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void HandleNewsResult(IAsyncResult result)
        {
            try
            {
                var common = new Common();
                var binding = (result.AsyncState as AsyncState).binding as News;

                News rv; //Our return value.

                /*
                 *  StreamReaders implement the IDisposable interface, so the best practice is to
                 *  wrap them in using statements so the object's is disposed once the scope passes.
                 *  
                 *  Only classes with expensive resources (data connections, large buffers, network handles, etc...) implement
                 *  IDisposable - in this case StreamReader just uses a buffer and it's probably not that large in this case,
                 *  but it's still a good practice. -Aaron
                 */
                using (var txt = common.makeStreamReaderFromResult(result))
                {
                    rv = common.deserializeStreamReader<News>(txt);
                }


                this.Dispatcher.BeginInvoke(
                () =>
                {
                    binding.nextId = rv.nextId;
                    binding.version = rv.version;
                    binding.items = rv.items;
                    // FIXME: Shouldn't I be able to do this instead?
                    //binding = processJsonString(txt);
                    SetProgressBar(false);
                }
                );
            }
            catch (WebException e)
            {
                this.Dispatcher.BeginInvoke(
                    () =>
                    {
                        //errorLine.Text = e.Message;
                        //errorLine.Visibility = Visibility.Visible;
                        SetProgressBar(false);
                    }
                );
            }
        }

        public void PopulateBinding(News binding, string Url)
        {
            var state = new AsyncState();
            var request = WebRequest.Create(Url) as HttpWebRequest;
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
            return ((Article)(((System.Windows.FrameworkElement)(sender)).DataContext));
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

            if (selected.url.StartsWith("http"))
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
            SetProgressBar(true);
            var selected = ((Microsoft.Phone.Controls.PivotItem)(((Microsoft.Phone.Controls.Pivot)(sender)).SelectedItem)).Header as string;
            switch (selected)
            {
                case "news":
                    PopulateBinding(news_page_data, "http://api.ihackernews.com/page");
                    break;
                case "new":
                    PopulateBinding(new_page_data, "http://api.ihackernews.com/new");
                    break;
                case "ask":
                    PopulateBinding(ask_page_data, "http://api.ihackernews.com/ask");
                    break;
                //case "comments":
                //    populateBinding(comments, "http://api.ihackernews.com/newcomments");
            }
        }
    }
}
