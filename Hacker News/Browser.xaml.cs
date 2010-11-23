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

namespace Hacker_News
{
    public partial class Browser : PhoneApplicationPage
    {
        public static string url = "";
        public Browser()
        {
            InitializeComponent();
            // browser.Navigate(new Uri("http://bing.com"));
            
        }
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            browser.Source = new Uri(url);
        }
    }
}
