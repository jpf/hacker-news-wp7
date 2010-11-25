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
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;
using System.Text;

namespace Hacker_News
{
    public class Comments
    {
        public string text { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public int id { get; set; }
        public int commentCount { get; set; }
        public int points { get; set; }
        public string postedAgo { get; set; }
        public string postedBy { get; set; }
        public List<Comment> comments { get; set; }
    }

    public class Comment
    {
        public string postedBy { get; set; }
        public string postedAgo { get; set; }
        public string comment { get; set; }
        public int id { get; set; }
        public int points { get; set; }
        public int parentId { get; set; }
        public int postId { get; set; }
        public List<Comment> children { get; set; }
    }

    public class FlatComment
    {
        public string postedBy { get; set; }
        public string postedAgo { get; set; }
        public string comment { get; set; }
        public int id { get; set; }
        public int points { get; set; }
        public int parentId { get; set; }
        public int postId { get; set; }
        public int depth { get; set; }
    }

    public class FlatComments
    {
        public List<FlatComment> comments { get; set; }
    }

    public partial class Post : PhoneApplicationPage
    {
        public static int id = 0;

        // start
        FlatComment flattenComment(Comment input, int depth)
        {
            FlatComment output = new FlatComment();
            // there HAS to be a better way to do this :(
            output.depth = depth;
            output.postedBy = input.postedBy;
            output.postedAgo = input.postedAgo;
            output.comment = input.comment;
            output.id = input.id;
            output.points = input.points;
            output.parentId = input.parentId;
            output.postId = input.postId;
            return output;
        }

        List<FlatComment> flattenComments(List<Comment> input, int currentDepth = 0)
        {
            List<FlatComment> output = new List<FlatComment>();
            Queue<Comment> queue = new Queue<Comment>(input);
            Comment car = new Comment();

            car = queue.Dequeue();

            output.Add(flattenComment(car, currentDepth));
            if (car.children.Count > 0)
            {
                output.AddRange(flattenComments(car.children, currentDepth + 1));
            }

            if (queue.Count > 0)
            {
                List<Comment> cdr = new List<Comment>(queue);
                output.AddRange(flattenComments(cdr, currentDepth));
            }

            return output;
        }

        void processPostJsonString(StreamReader sr)
        {
            Comments comments = new Comments();
            Newtonsoft.Json.JsonSerializer json = new Newtonsoft.Json.JsonSerializer();
            // do i even need these?
            json.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            json.ObjectCreationHandling = Newtonsoft.Json.ObjectCreationHandling.Replace;
            json.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;
            json.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            Newtonsoft.Json.JsonTextReader reader = new JsonTextReader(sr);

            comments = json.Deserialize<Comments>(reader);
            sr.Close();
            reader.Close();

            FlatComments flatComments = new FlatComments();
            flatComments.comments = flattenComments(comments.comments);

            //dp.list = new ObservableCollection<FlatComment>();
            //foreach (FlatComment c in flattenedComments)
            //{
            //    dp.list.Add(c);
            //}

            var monkey = "oo oo";
            monkey = "ook ook";

            // I've got to do it to avoid thread locking issues ...
            // TODO: Remove this by making the News class an INotifyPropertyChanged type?
            this.Dispatcher.BeginInvoke(
                () =>
                {
                    commentsList.DataContext = flatComments;
                }
            );
        }

        private void PostRequestCallback(IAsyncResult result)
        {
            var state = result.AsyncState as AsyncState;
            var request = state.request as HttpWebRequest;
            var response = request.EndGetResponse(result);
            if (response != null)
            {
                Stream rv = response.GetResponseStream();
                UTF8Encoding encoding = new UTF8Encoding();
                StreamReader txt = new StreamReader(rv, encoding);
                processPostJsonString(txt);
            }
        }

        public void fetchComments(string Url)
        {
            AsyncState state = new AsyncState();
            HttpWebRequest request = HttpWebRequest.Create(Url) as HttpWebRequest;
            request.Accept = "application/json"; //atom+xml";
            state.request = request;
            request.BeginGetResponse(PostRequestCallback, state);
        }

        // end

        public Post()
        {
            InitializeComponent();
            // fetchComments("http://api.ihackernews.com/post/1936607");
            string url = "http://api.ihackernews.com/post/" + id.ToString();
            fetchComments(url);
        }
    }
}