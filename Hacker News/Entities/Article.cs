using System;

namespace Hacker_News.Entities
{
    public class Article
    {
        Uri urlValue = null;
        public string title { get; set; }
        public string urlDomainOnly
        {
            get
            {
                if (this.urlValue == null) return string.Empty;

                var hostParts = this.urlValue.Host.Split('.');
                var len = hostParts.Length;

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
}