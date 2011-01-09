using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Hacker_News.Entities
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
}