using BiliApi.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Web;

namespace BiliApi
{
    /// <summary>
    /// 网页API基础类
    /// </summary>
    public class ThirdPartAPIs
    {
        public const string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.146 Safari/537.36";
        public ThirdPartAPIs(CookieCollection c)
        {
            CookieContext = c;
        }
        public struct ResultWithCookie
        {
            public string Result;
            public CookieCollection Cookies;
        }

        public CookieCollection CookieContext { private set; get; }

        public static string _get(string url)
        {
            string retString;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/json";
            request.UserAgent = USER_AGENT;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(myResponseStream);
            retString = streamReader.ReadToEnd();
            streamReader.Close();
            myResponseStream.Close();
            return retString;
        }

        public static ResultWithCookie _post_cookies(string url, Dictionary<string, string> form_data = null)
        {
            if (form_data == null) form_data = new Dictionary<string, string>();
            string retString = "";
            foreach (KeyValuePair<string, string> fd in form_data)
            {
                retString += "&" + HttpUtility.UrlEncode(fd.Key) + "=" + HttpUtility.UrlEncode(fd.Value);
            }
            retString = retString.Substring(1);

            byte[] bs = Encoding.UTF8.GetBytes(retString);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.UserAgent = USER_AGENT;
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = bs.Length;
            request.CookieContainer = new CookieContainer();
            //提交请求数据
            Stream reqStream = request.GetRequestStream();
            reqStream.Write(bs, 0, bs.Length);
            reqStream.Close();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(myResponseStream);
            retString = streamReader.ReadToEnd();
            streamReader.Close();
            myResponseStream.Close();
            return new ResultWithCookie()
            {
                Result = retString,
                Cookies = response.Cookies
            };
        }

        public static ResultWithCookie _get_cookies(string url)
        {
            string retString;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/json";
            request.UserAgent = USER_AGENT;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(myResponseStream);
            retString = streamReader.ReadToEnd();
            streamReader.Close();
            myResponseStream.Close();
            return new ResultWithCookie()
            {
                Result = retString,
                Cookies = response.Cookies
            };
        }

        public static string _get_with_cookies_gzip(string url, CookieCollection cookies)
        {
            string retString;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/json";
            request.CookieContainer = ToContainer(cookies);
            request.UserAgent = USER_AGENT;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(new GZipStream(myResponseStream, CompressionMode.Decompress), Encoding.UTF8, true);
            retString = streamReader.ReadToEnd();
            streamReader.Close();
            myResponseStream.Close();
            return retString;
        }

        public static string _get_with_cookies(string url, CookieCollection cookies)
        {
            string retString;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/json";
            request.UserAgent = USER_AGENT;
            request.CookieContainer = ToContainer(cookies);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(myResponseStream);
            retString = streamReader.ReadToEnd();
            streamReader.Close();
            myResponseStream.Close();
            return retString;
        }

        public static string _get_with_cookies_and_refer(string url, string refer, CookieCollection cookies)
        {
            string retString;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/json";
            request.Referer = refer;
            request.UserAgent = USER_AGENT;
            request.CookieContainer = ToContainer(cookies);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(myResponseStream);
            retString = streamReader.ReadToEnd();
            streamReader.Close();
            myResponseStream.Close();
            return retString;
        }

        public static string _post_with_cookies_and_refer(string url, Dictionary<string, string> form_data, string refer, CookieCollection cookies)
        {
            string retString="";
            foreach (KeyValuePair<string, string> fd in form_data)
            {
                retString += "&" + HttpUtility.UrlEncode(fd.Key) + "=" + HttpUtility.UrlEncode(fd.Value);
            }
            retString = retString.Substring(1);

            byte[] bs = Encoding.UTF8.GetBytes(retString);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = ToContainer(cookies);
            request.UserAgent = USER_AGENT;
            request.Referer = refer;
            request.ContentLength = bs.Length;
            //提交请求数据
            Stream reqStream = request.GetRequestStream();
            reqStream.Write(bs, 0, bs.Length);
            reqStream.Close();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(myResponseStream);
            retString = streamReader.ReadToEnd();
            streamReader.Close();
            myResponseStream.Close();
            return retString;
        }


        public static string _post_with_cookies(string url, Dictionary<string, string> form_data, CookieCollection cookies)
        {
            string retString="";
            foreach (KeyValuePair<string, string> fd in form_data)
            {
                retString += "&" + HttpUtility.UrlEncode(fd.Key) + "=" + HttpUtility.UrlEncode(fd.Value);
            }
            retString = retString.Substring(1);

            byte[] bs = Encoding.UTF8.GetBytes(retString);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = ToContainer(cookies);
            request.ContentLength = bs.Length;
            request.UserAgent = USER_AGENT;
            //提交请求数据
            Stream reqStream = request.GetRequestStream();
            reqStream.Write(bs, 0, bs.Length);
            reqStream.Close();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(myResponseStream);
            retString = streamReader.ReadToEnd();
            streamReader.Close();
            myResponseStream.Close();
            return retString;
        }

        public string _get_with_cookies_and_refer(string url, string refer)
        {
            return _get_with_cookies_and_refer(url, refer, CookieContext);
        }

        public string _post_with_cookies(string url, Dictionary<string, string> form_data)
        {
            return _post_with_cookies(url, form_data, CookieContext);
        }

        public string _post_with_cookies_and_refer(string url, string refer, Dictionary<string, string> form_data)
        {
            return _post_with_cookies_and_refer(url, form_data, refer, CookieContext);
        }

        public string _get_with_cookies(string url)
        {
            return _get_with_cookies(url, CookieContext);
        }

        public string _get_with_cookies_gzip(string url)
        {
            return _get_with_cookies_gzip(url, CookieContext);
        }

        public string _get_with_manacookies_and_refer(string url, string refer)
        {
            return _get_with_cookies_and_refer(url, refer, CookieContext);
        }

        public string _post_with_manacookies(string url, Dictionary<string, string> form_data)
        {
            return _post_with_cookies(url, form_data, CookieContext);
        }

        public string _post_with_manacookies_and_refer(string url, string refer, Dictionary<string, string> form_data)
        {
            return _post_with_cookies_and_refer(url, form_data, refer, CookieContext);
        }

        public string _get_with_manacookies(string url)
        {
            return _get_with_cookies(url, CookieContext);
        }

        public static CookieContainer ToContainer(CookieCollection cookies)
        {
            CookieContainer ck = new CookieContainer();
            foreach (Cookie c in cookies)
            {
                ck.Add(c);
            }
            return ck;
        }

        public string GetCrsf()
        {
            foreach (Cookie c in CookieContext)
            {
                if (c.Name == "bili_jct") return c.Value;
            }
            throw new CookieMissingException(CookieContext, "bili_jct");
        }


        public string getBliveTitle(int roomid)
        {
            string url = "https://live.bilibili.com/" + roomid;
            string rtv = _get(url);
            int title_index = rtv.IndexOf("\"title\":\"") + 9;
            string title = rtv.Substring(title_index);
            int title_len = title.IndexOf("\", \"");
            return title.Substring(0, title_len);
        }

        public string getUpState(int uid)
        {
            //https://api.bilibili.com/x/relation/stat?vmid=5659864
            string url = "https://api.bilibili.com/x/relation/stat?vmid=" + uid;
            return _get_with_cookies_and_refer(url, "https://space.bilibili.com/" + uid + "/fans/fans");
        }

        public string getFanList(int uid, int pageno = 1, int pagesize = 5)
        {
            //https://api.bilibili.com/x/relation/followers?vmid=5659864&pn=1&ps=1&order=desc&jsonp=jsonp
            string url = "https://api.bilibili.com/x/relation/followers?vmid=" + uid + "&pn=" + pageno + "&ps=" + pagesize + "&order=desc&jsonp=jsonp";
            return _get_with_cookies_and_refer(url, "https://space.bilibili.com/" + uid + "/fans/fans");
        }

        public string getBiliUserInfoJson(int uid)
        {
            string url = "https://api.bilibili.com/x/space/acc/info?mid=" + uid + "&jsonp=jsonp";
            return _get(url);
        }
        public string getBiliUserDynamicJson(int uid)
        {
            string url = "https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/space_history?host_uid=" + uid + "&offset_dynamic_id=0&need_top=1";
            return _get(url);
        }

        public string getBiliVideoStaticsJson(int aid)
        {
            //{"code":0,"message":"0","ttl":1,"data":{"aid":8904657,"view":12973,"danmaku":80,"reply":51,"favorite":33,"coin":219,"share":17,"now_rank":0,"his_rank":0,"like":55,"dislike":0,"no_reprint":0,"copyright":1}}
            //http://api.bilibili.com/archive_stat/stat?aid=1
            string url = "http://api.bilibili.com/archive_stat/stat?aid=" + aid;
            return _get_with_cookies(url);
        }

        public string getBiliVideoInfoJson(string abid)
        {
            string start_indi = "<script>window.__INITIAL_STATE__=";
            string end_indi = "};";
            string url = "https://www.bilibili.com/video/" + abid;
            string data = _get_with_cookies_gzip(url);
            int start_pos = data.IndexOf(start_indi) + start_indi.Length;
            if (start_pos <= start_indi.Length)
            {
                return null;//解析失败
            }

            string jstring = data.Substring(start_pos);
            int stop_pos = jstring.IndexOf(end_indi) + 1;
            jstring = jstring.Substring(0, stop_pos);
            return jstring;
        }

        [Obsolete("Class BiliVideo has its own solution, and this methold is abandoned.", true)]
        public Dictionary<string, string> getBiliVideoParticipants(string abid)
        {
            string js = getBiliVideoInfoJson(abid);
            JObject json = (JObject)JsonConvert.DeserializeObject(js);
            return getBiliVideoParticipants(json);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roomid">房间ID</param>
        /// <param name="uid">被封禁者UID</param>
        /// <param name="len">时长，单位为小时</param>
        /// <returns></returns>
        public string banUIDfromroom(int roomid, int uid, int len = 1)
        {
            //https://api.live.bilibili.com/banned_service/v2/Silent/add_block_user
            string url = "https://api.live.bilibili.com/banned_service/v2/Silent/add_block_user";
            Dictionary<string, string> form = new Dictionary<string, string>
            {
                { "roomid", roomid.ToString() },
                { "block_uid", uid.ToString() },
                { "hour", len.ToString() },
                { "csrf", GetCrsf() },
                { "csrf_token", GetCrsf() }
            };
            return _post_with_manacookies_and_refer(url, "https://live.bilibili.com/" + roomid, form);
        }

        public string debanBIDfromroom(int roomid, int bid)
        {
            //https://api.live.bilibili.com/banned_service/v1/Silent/del_room_block_user
            string url = "https://api.live.bilibili.com/banned_service/v1/Silent/del_room_block_user";
            Dictionary<string, string> form = new Dictionary<string, string>
            {
                { "roomid", roomid.ToString() },
                { "id", bid.ToString() },
                { "csrf", GetCrsf() },
                { "csrf_token", GetCrsf() }
            };
            return _post_with_manacookies_and_refer(url, "https://live.bilibili.com/" + roomid, form);
        }

        public string getRoomBanlist(int roomid, int page = 1)
        {
            //https://api.live.bilibili.com/liveact/ajaxGetBlockList?roomid=2064239&page=1
            string url = "https://api.live.bilibili.com/liveact/ajaxGetBlockList?roomid=" + roomid + "&page=" + page;
            return _get_with_manacookies_and_refer(url, "https://live.bilibili.com/" + roomid);
        }

        [Obsolete("Class BiliVideo has its own solution, and this methold is abandoned.", true)]
        public Dictionary<string, string> getBiliVideoParticipants(JObject json)
        {
            throw new NotImplementedException();
        }

        //public static 
    }

}
