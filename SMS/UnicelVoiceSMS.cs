using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;

namespace Quanto.SMS
{
    public class UnicelVoiceSMS
    {
        public string username;
        public string password;
        public string sessionid;
        public UnicelVoiceSMS(string username,string password)
        {
            this.username = username;
            this.password = password;
        }

        private string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
        public Clip Execute(string filename,byte[] bytes, string clipname)
        {
            string authenticationkey = Authenticate();
            string sessionKey = SetSession(authenticationkey);
            return UploadFile(filename, bytes, clipname);
        }

        public bool SendMessage(string mobileNo,string clipname)
        {
            string query = @"https://vapi.unicel.in/voiceapi?request=voiceobd&uname=" + username + "&pass=" + password + "&obdid=0&type=D&dest=" + mobileNo + "&msgtype=P&msg=" + clipname;
            WebClient wc = new WebClient();
            string response = wc.DownloadString(query) ;
            return !string.IsNullOrEmpty(response);
        }

        public Clip UploadFile(string filename, byte[] bytes, string clipname)
        {
            
            string URLAuth = "https://unicel.in/lounge/BroadcastOne/upload-file.php";
            Uri uri = new Uri(URLAuth);
            var cc = new CookieContainer();
            cc.Add(new Cookie("PHPSESSID", sessionid) { Domain = uri.Host });

            var sresponse = UploadFileEx(filename , bytes, URLAuth, "uploadfile", "audio/wav", new NameValueCollection(), cc);
            string[] clipinfos = sresponse.Split(',');
            

            //URLAuth = "https://unicel.in/lounge/BroadcastOne/upload-file2.php?";

            //WebClientEx webClient = new WebClientEx();
            //webClient.Headers.Add("Content-Type: application/x-www-form-urlencoded");
            //webClient.Headers.Add("User-Agent: Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:56.0) Gecko/20100101 Firefox/56.0");
            //webClient.Headers.Add("Accept: text/plain, *; q=0.01");
            ////webClient.Headers.Add("Accept-Encoding: gzip, deflate, br");
            //webClient.Headers.Add("Accept-Language: en-US,en;q=0.5");
            ////webClient.Headers.Add("Cookie: PHPSESSID="+ cookie);

            //cc = new CookieContainer();
            //cc.Add(new Cookie("PHPSESSID", sessionid) { Domain = uri.Host });
            //var formData = new NameValueCollection();
            //formData.Add("filename", clipinfos[0]);
            //webClient.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            //webClient.Cookies = cc;
            //var response = webClient.UploadValues(URLAuth, "POST", formData);
            //var resultAuthTicket = Encoding.UTF8.GetString(response); 


            var webClient = new WebClientEx();
            webClient.Headers.Add("Content-Type: application/x-www-form-urlencoded");
            webClient.Headers.Add("User-Agent: Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:56.0) Gecko/20100101 Firefox/56.0");
            webClient.Headers.Add("Accept: text/plain, *; q=0.01");
            //webClient.Headers.Add("Accept-Encoding: gzip, deflate, br");
            webClient.Headers.Add("Accept-Language: en-US,en;q=0.5");
            webClient.Headers.Add("X-Requested-With: XMLHttpRequest");
            //webClient.Headers.Add("Cookie: PHPSESSID="+ cookie);
            cc = new CookieContainer();
            cc.Add(new Cookie("PHPSESSID", sessionid) { Domain = uri.Host });
            var formData = new NameValueCollection();
            //webClient.Headers.Add("Cookie: PHPSESSID=" + sessionid);
            //var sessionchceckresponse = webClient.UploadValues(@"https://unicel.in/lounge/session_check.php", "POST", formData);

            URLAuth = @"https://unicel.in/lounge/BroadcastOne/upload-file2.php?t="+DateTime.Now.Ticks;
            webClient.Referer = @"https://unicel.in/lounge/BroadcastOne/";
            formData.Add("filename", clipinfos[0]);
            webClient.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            webClient.Cookies = cc;
            var complete_response = webClient.UploadValues(URLAuth, "POST", formData);
            var complete_clipresponse = Encoding.UTF8.GetString(complete_response);


            formData["obj"] = "{\"requestType\":5,\"esmeAddr\":\"" + clipinfos[3] + "\",\"clipName\":\"" + clipinfos[0] + "\",\"duration\":\"" + clipinfos[1] +
                "\",\"language\":\"English\",\"label\":\"" + clipname + "\",\"size\":\"" + clipinfos[2] + "\",\"status\":0}";
            formData["link"] = "voicemgmt";

            webClient.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            webClient.Cookies = cc;
            URLAuth = @"https://unicel.in/lounge/voice_submit1.php";
            var response = webClient.UploadValues(URLAuth, "POST", formData);
            var clipresponse = Encoding.UTF8.GetString(response);

            Clip clip = new Clip();
            clip.duration = clipinfos[1];
            clip.size = clipinfos[2];
            clip.uniqueid = clipinfos[0];
            clip.name = clipname;
            clip.success = complete_clipresponse == "SUCCESS";
            return clip;
        }


        public string Authenticate()
        {
           
            string URLAuth = "https://unicel.in/lounge/uni_login.php";
            WebClientEx webClient = new WebClientEx();
            webClient.Headers.Add("Content-Type: application/x-www-form-urlencoded");
            webClient.Headers.Add("User-Agent: Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:56.0) Gecko/20100101 Firefox/56.0");
            webClient.Headers.Add("Accept: text/plain, *; q=0.01");
            var cc = new CookieContainer();
            Uri target = new Uri(URLAuth);
            webClient.Cookies = cc;
            NameValueCollection formData = new NameValueCollection();
            formData["obj"] = "{\"requestType\":\"login\",\"userName\":\""+username+ "\",\"password\":\""+ CreateMD5(password) + "\"}";
            formData["link"] = "loginmgmt";
            formData["code"] = "";
            byte[] responseBytes = webClient.UploadValues(URLAuth, "POST", formData);


            Uri uri = new Uri("https://unicel.in");
            foreach(Cookie item in cc.GetCookies(uri))
            {
                if (item.Name == "PHPSESSID")
                    sessionid = item.Value;
            }
            return Encoding.UTF8.GetString(responseBytes);
        }

        public string SetSession(string authenticationkey)
        {
            string URLAuth = "https://unicel.in/lounge/set_session.php";
            Uri uri = new Uri("https://unicel.in");
            WebClientEx webClient = new WebClientEx();
            webClient.Headers.Add("Content-Type: application/x-www-form-urlencoded");
            webClient.Headers.Add("User-Agent: Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:56.0) Gecko/20100101 Firefox/56.0");
            webClient.Headers.Add("Accept: text/plain, *; q=0.01");
            var cc = new CookieContainer();
            cc.Add(new Cookie("PHPSESSID", sessionid) { Domain = uri.Host });
            webClient.Cookies = cc;

            NameValueCollection formData = new NameValueCollection();
            formData["obj"] = authenticationkey;
            formData["app"] = "lounge";
            formData["userName"] = username;
            var responseBytes = webClient.UploadValues(URLAuth, "POST", formData);
            var resultAuthTicket = Encoding.UTF8.GetString(responseBytes);

            foreach (Cookie item in cc.GetCookies(uri))
            {
                if (item.Name == "PHPSESSID")
                    sessionid = item.Value;
            }

            formData = new NameValueCollection();
            //webClient.Headers.Add("Cookie: PHPSESSID=" + sessionid);
            var response = webClient.UploadValues(@"https://unicel.in/lounge/session_check.php", "POST", formData);
            foreach (Cookie item in cc.GetCookies(uri))
            {
                if (item.Name == "PHPSESSID")
                    sessionid = item.Value;
            }
            return Encoding.UTF8.GetString(response);
        }

        public class Clip
        {
            public string name { get; set; }
            public string uniqueid { get; set; }
            public string duration { get; set; }
            public string size { get; set; }
            public bool success { get; set; }
            public string error { get; set; }
        }


        public static string UploadFileEx(string uploadfile,byte[] content, string url,
   string fileFormName, string contenttype, NameValueCollection querystring,
   CookieContainer cookies)
        {
            

            if ((fileFormName == null) ||
                (fileFormName.Length == 0))
            {
                fileFormName = "file";
            }

            if ((contenttype == null) ||
                (contenttype.Length == 0))
            {
                contenttype = "application/octet-stream";
            }


            string postdata;
            postdata = "?";
            if (querystring != null)
            {
                foreach (string key in querystring.Keys)
                {
                    postdata += key + "=" + querystring.Get(key) + "&";
                }
            }
            Uri uri = new Uri(url + postdata);


            string boundary = "----------" + DateTime.Now.Ticks.ToString("x");
            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(uri);
            webrequest.CookieContainer = cookies;
            webrequest.ContentType = "multipart/form-data; boundary=" + boundary;
            webrequest.Method = "POST";


            // Build up the post message header
            StringBuilder sb = new StringBuilder();
            sb.Append("--");
            sb.Append(boundary);
            sb.Append("\r\n");
            sb.Append("Content-Disposition: form-data; name=\"");
            sb.Append(fileFormName);
            sb.Append("\"; filename=\"");
            sb.Append(uploadfile);
            sb.Append("\"");
            sb.Append("\r\n");
            sb.Append("Content-Type: ");
            sb.Append(contenttype);
            sb.Append("\r\n");
            sb.Append("\r\n");

            string postHeader = sb.ToString();
            byte[] postHeaderBytes = Encoding.UTF8.GetBytes(postHeader);

            // Build the trailing boundary string as a byte array
            // ensuring the boundary appears on a line by itself
            byte[] boundaryBytes =
                   Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            /*FileStream fileStream = new FileStream(uploadfile,
                                        FileMode.Open, FileAccess.Read);*/

            MemoryStream fileStream = new MemoryStream(content);

            long length = postHeaderBytes.Length + fileStream.Length +
                                                   boundaryBytes.Length;
            webrequest.ContentLength = length;

            Stream requestStream = webrequest.GetRequestStream();

            // Write out our post header
            requestStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);

            // Write out the file contents
            byte[] buffer = new Byte[checked((uint)Math.Min(4096,
                                     (int)fileStream.Length))];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                requestStream.Write(buffer, 0, bytesRead);

            // Write out the trailing boundary
            requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
            WebResponse responce = webrequest.GetResponse();
            Stream s = responce.GetResponseStream();
            StreamReader sr = new StreamReader(s);

            return sr.ReadToEnd();
        }

        class WebClientEx : System.Net.WebClient
        {
            private CookieContainer _cookies;
            private string _ref;
            public WebClientEx()
            {
                _cookies = new CookieContainer();
            }
            public CookieContainer Cookies
            {
                get { return _cookies; }
                set { _cookies = value; }
            }

            public HttpWebRequest WebRequest
            {
                get
                {
                    return req;
                }

                set
                {
                    req = value;
                }
            }

            public string Referer { get;  set; }

            HttpWebRequest req;
            protected override WebRequest GetWebRequest(System.Uri address)
            {
                var webReq = base.GetWebRequest(address);
                if (webReq is HttpWebRequest)
                {
                    req = (HttpWebRequest)webReq;
                    req.Proxy = new WebProxy();
                    req.ServicePoint.Expect100Continue = false;
                    req.CookieContainer = _cookies;
                    //req.Headers.Set(HttpRequestHeader.Host, "unicel.in");
                    if (Referer != null)
                    {
                        req.Referer = Referer;
                    }
                }
                //_ref = address.ToString();
                return webReq;
            }
            protected override void Dispose(bool disposing)
            {
                _cookies = null;
                base.Dispose(disposing);
            }
        }

    }
}
