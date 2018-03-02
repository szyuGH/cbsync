using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CBSync
{
    public sealed class HttpSender
    {
        public bool Error { get; private set; }
        private HttpWebRequest request;

        public HttpSender(string url, int timeout)
        {
            request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "application/json";
            request.Method = "POST";
            request.Timeout = timeout;
        }
        

        public HttpSender Send(object data)
        {
            try
            {
                using (var sw = new StreamWriter(request.GetRequestStream()))
                {
                    sw.Write(data == null ? "" : JsonConvert.SerializeObject(data));
                    sw.Flush();
                }
            } catch (Exception e)
            {
                Console.WriteLine(e);
                Error = true;
            }
            return this;
        }

        public HttpWebResponse Receive()
        {
            try
            {
                if (!Error)
                    return (HttpWebResponse)request.GetResponse();
                return null;
            } catch (Exception e)
            {
                Console.WriteLine(e);
                Error = true;
                return null;
            }
        }
    }
}
