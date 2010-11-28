using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Hacker_News
{
    public class AsyncState
    {
        // I'm not sure if I really need to call this AsyncState?
        public HttpWebRequest request { get; set; }
        public object binding { get; set; }
    }

    public class Common
    {
        // I'm sad that this is the most that I was able to abstract.

        public T deserializeStreamReader<T>(StreamReader sr)
        {
            return (T)deserializeStreamReader(sr, typeof(T));
        }
        public object deserializeStreamReader(StreamReader sr, Type objectType)
        {
            Newtonsoft.Json.JsonSerializer json = new Newtonsoft.Json.JsonSerializer();
            #region do i even need these?
            json.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            json.ObjectCreationHandling = Newtonsoft.Json.ObjectCreationHandling.Replace;
            json.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;
            json.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            #endregion
            Newtonsoft.Json.JsonTextReader reader = new JsonTextReader(sr);
            object rv = json.Deserialize(reader, objectType);
            sr.Close();
            reader.Close();
            return rv;
        }
        public StreamReader makeStreamReaderFromResult(IAsyncResult result)
        {
            var state = result.AsyncState as AsyncState;
            var request = state.request as HttpWebRequest;
            // FIXME: Add error handling here ...
            var response = request.EndGetResponse(result);
            if (response == null) { return new StreamReader(Stream.Null); }

            Stream stream = response.GetResponseStream();
            UTF8Encoding encoding = new UTF8Encoding();
            return new StreamReader(stream, encoding);
        }
    }
}
