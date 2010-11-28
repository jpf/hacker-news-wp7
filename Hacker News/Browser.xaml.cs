using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Navigation;

namespace Hacker_News
{
    public partial class Browser : PhoneApplicationPage
    {
        public static string url = "";
        public Browser()
        {
            InitializeComponent();
        }

        public void setProgressBar(Boolean state)
        {
            progressBar.IsIndeterminate = state;
            progressBar.Visibility = (state == true) ? Visibility.Visible : Visibility.Collapsed;
        }

        public void browser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            setProgressBar(false);
            location.Text = browser.Source.ToString();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            setProgressBar(true);
            location.Text = url;
            browser.Source = new Uri(url);
            browser.LoadCompleted += new LoadCompletedEventHandler(browser_LoadCompleted);
        }
    }
}
