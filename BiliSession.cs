using BiliApi.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace BiliApi
{
    /// <summary>
    /// 网页API基础类
    /// </summary>
    public class BiliSession
    {
        #region 下层实现
        public const string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.146 Safari/537.36";
        public BiliSession(CookieCollection c)
        {
            CookieContext = c;
        }

        public struct ResultWithCookie
        {
            public string Result;
            public CookieCollection Cookies;
        }

        public CookieCollection CookieContext
        {
            set
            {
                CookieContainer = ToContainer(value);
            }
            get => CookieContainer.GetCookies(new Uri("https://bilibili.com"));
        }

        public CookieContainer CookieContainer { private set; get; }

        public string GetCookieString()
        {
            StringBuilder cookieString = new StringBuilder();
            foreach (Cookie cookie in CookieContext)
            {
                cookieString.Append($"{cookie.Name}={cookie.Value}; ");
            }
            return cookieString.ToString();
        }

        public static string _get(string url)
        {
            string retString;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.UserAgent = USER_AGENT;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(myResponseStream);
            retString = streamReader.ReadToEnd();
            streamReader.Close();
            myResponseStream.Close();
            return retString;
        }

        public static string PostFile(string url, string refernorigin, byte[] filecontent, string filefieldname, string filetype, string filename, Dictionary<string, string> PostInfo = null, CookieContainer cookies = null)
        {
            //上传文件以及相关参数
            //POST请求分隔符，可任意定制
            string Boundary = "----WebKitFormBoundarySKNbhflQaXvVSUIb";


            //构造POST请求体
            StringBuilder PostContent = new StringBuilder("\r\n--" + Boundary);
            byte[] ContentEnd = Encoding.UTF8.GetBytes("--\r\n");//请求体末尾，后面会用到
                                                                 //组成普通参数信息
            foreach (KeyValuePair<string, string> item in PostInfo)
            {
                PostContent.Append("\r\n")
                        .Append("Content-Disposition: form-data; name=\"")
                        .Append(item.Key + "\"").Append("\r\n")
                        .Append("\r\n").Append(item.Value).Append("\r\n")
                        .Append("--").Append(Boundary);
            }
            //转换为二进制数组，后面会用到
            byte[] PostContentByte = Encoding.UTF8.GetBytes(PostContent.ToString());

            //文件信息
            byte[] UpdateFile = filecontent;//二进制
            StringBuilder FileContent = new StringBuilder();
            FileContent.Append("--").Append(Boundary).Append("\r\n")
                    .Append("Content-Disposition:form-data; name=\"" + filefieldname + "\"; ")
                    .Append("filename=\"")
                    .Append(filename + "\"")
                    .Append("\r\n")
                    .Append("Content-Type:")
                    .Append(filetype)
                    .Append("\r\n\r\n");
            byte[] FileContentByte = Encoding.UTF8.GetBytes(FileContent.ToString());

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            //request.Proxy = new WebProxy("http://127.0.0.1:8888", false) ;
            request.Method = "POST";
            request.Timeout = 20000;
            request.CookieContainer = cookies;
            request.Referer = refernorigin;
            request.Headers.Add("origin", refernorigin);
            //这里确定了分隔符是什么
            request.ContentType = "multipart/form-data;boundary=" + Boundary;
            request.ContentLength = PostContentByte.Length + FileContentByte.Length + UpdateFile.Length + ContentEnd.Length;

            //定义请求流
            Stream myRequestStream = request.GetRequestStream();
            myRequestStream.Write(FileContentByte, 0, FileContentByte.Length);//写入文件信息
            myRequestStream.Write(UpdateFile, 0, UpdateFile.Length);//文件写入请求流中
            myRequestStream.Write(PostContentByte, 0, PostContentByte.Length);//写入参数
            myRequestStream.Write(ContentEnd, 0, ContentEnd.Length);//写入结尾                

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            //获取返回值
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));

            string retString = myStreamReader.ReadToEnd();
            myRequestStream.Close();
            myStreamReader.Close();
            myResponseStream.Close();
            return retString;
        }

        public static string PostFile(string url, string refernorigin, byte[] filecontent, string filefieldname, string filetype, string filename, Dictionary<string, string> PostInfo = null, CookieCollection cookies = null)
        {
            return PostFile(url, refernorigin, filecontent, filefieldname, filetype, filename, PostInfo, ToContainer(cookies));
        }

        public string PostFile(string url, string refernorigin, byte[] filecontent, string filefieldname, string filetype, string filename, Dictionary<string, string> PostInfo = null)
        {
            return PostFile(url, refernorigin, filecontent, filefieldname, filetype, filename, PostInfo, CookieContainer);
        }

        public static ResultWithCookie _post_cookies(string url, Dictionary<string, string> form_data = null)
        {
            string retString = "";
            if (form_data != null)
            {
                foreach (KeyValuePair<string, string> fd in form_data)
                {
                    retString += "&" + HttpUtility.UrlEncode(fd.Key) + "=" + HttpUtility.UrlEncode(fd.Value);
                }
                retString = retString.Substring(1);
            }
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
                Cookies = request.CookieContainer.GetCookies(new Uri(url))
            };
        }

        public static ResultWithCookie _get_cookies(string url)
        {
            string retString;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.UserAgent = USER_AGENT;
            request.CookieContainer = new CookieContainer();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(myResponseStream);
            retString = streamReader.ReadToEnd();
            streamReader.Close();
            myResponseStream.Close();
            return new ResultWithCookie()
            {
                Result = retString,
                Cookies = request.CookieContainer.GetCookies(new Uri(url))
            };
        }

        public static string _get_with_cookies_gzip(string url, CookieContainer cookies)
        {
            string retString;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/json";
            request.CookieContainer = cookies;
            request.UserAgent = USER_AGENT;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(new GZipStream(myResponseStream, CompressionMode.Decompress), Encoding.UTF8, true);
            retString = streamReader.ReadToEnd();
            streamReader.Close();
            myResponseStream.Close();
            return retString;
        }

        public static string _get_with_cookies_gzip(string url, CookieCollection cookies) => _get_with_cookies_gzip(url, ToContainer(cookies));

        public static string _get_with_cookies(string url, CookieContainer cookies)
        {
            string retString;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/json";
            request.UserAgent = USER_AGENT;
            request.CookieContainer = cookies;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(myResponseStream);
            retString = streamReader.ReadToEnd();
            streamReader.Close();
            myResponseStream.Close();
            return retString;
        }

        public static string _get_with_cookies(string url, CookieCollection cookies) => _get_with_cookies(url, ToContainer(cookies));

        public static string _get_with_cookies_and_refer(string url, string refer, CookieContainer cookies)
        {
            string retString;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/json";
            request.Referer = refer;
            request.UserAgent = USER_AGENT;
            request.CookieContainer = cookies;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(myResponseStream);
            retString = streamReader.ReadToEnd();
            streamReader.Close();
            myResponseStream.Close();
            return retString;
        }

        public static string _get_with_cookies_and_refer(string url, string refer, CookieCollection cookies) =>
            _get_with_cookies_and_refer(url, refer, ToContainer(cookies));

        public static string _post_with_cookies_and_refer(string url, Dictionary<string, string> form_data, string refer, CookieContainer cookies)
        {
            string retString = "";
            foreach (KeyValuePair<string, string> fd in form_data)
            {
                retString += "&" + HttpUtility.UrlEncode(fd.Key) + "=" + HttpUtility.UrlEncode(fd.Value);
            }
            retString = retString.Substring(1);

            byte[] bs = Encoding.UTF8.GetBytes(retString);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = cookies;
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

        public static string _post_with_cookies_and_refer(string url, Dictionary<string, string> form_data, string refer, CookieCollection cookies) =>
            _post_with_cookies_and_refer(url, form_data, refer, ToContainer(cookies));

        public static string _post_with_cookies(string url, Dictionary<string, string> form_data, CookieContainer cookies)
        {
            string retString = "";
            foreach (KeyValuePair<string, string> fd in form_data)
            {
                retString += "&" + HttpUtility.UrlEncode(fd.Key) + "=" + HttpUtility.UrlEncode(fd.Value);
            }
            retString = retString.Substring(1);

            byte[] bs = Encoding.UTF8.GetBytes(retString);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = cookies;
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

        public static string _post_with_cookies(string url, Dictionary<string, string> form_data, CookieCollection cookies) =>
            _post_with_cookies(url, form_data, ToContainer(cookies));

        public string _get_with_cookies_and_refer(string url, string refer)
        {
            return _get_with_cookies_and_refer(url, refer, CookieContainer);
        }

        public string _post_with_cookies(string url, Dictionary<string, string> form_data)
        {
            return _post_with_cookies(url, form_data, CookieContainer);
        }

        public string _post_with_cookies_and_refer(string url, string refer, Dictionary<string, string> form_data)
        {
            return _post_with_cookies_and_refer(url, form_data, refer, CookieContainer);
        }

        public string _get_with_cookies(string url)
        {
            return _get_with_cookies(url, CookieContainer);
        }

        public string _get_with_cookies_gzip(string url)
        {
            return _get_with_cookies_gzip(url, CookieContainer);
        }

        public string _get_with_manacookies_and_refer(string url, string refer)
        {
            return _get_with_cookies_and_refer(url, refer, CookieContainer);
        }

        public string _post_with_manacookies(string url, Dictionary<string, string> form_data)
        {
            return _post_with_cookies(url, form_data, CookieContainer);
        }

        public string _post_with_manacookies_and_refer(string url, string refer, Dictionary<string, string> form_data)
        {
            return _post_with_cookies_and_refer(url, form_data, refer, CookieContainer);
        }

        public string _get_with_manacookies(string url)
        {
            return _get_with_cookies(url, CookieContainer);
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

        public static void UpdateCookieCollection(out CookieCollection collection, CookieContainer container)
        {
            collection = container.GetCookies(new Uri("https://bilibili.com"));
        }

        public string GetCsrf()
        {
            foreach (Cookie c in CookieContext)
            {
                if (c.Name == "bili_jct") return c.Value;
            }
            throw new CookieMissingException(CookieContext, "bili_jct");
        }

        private static readonly Regex RegexSplitCookie2 = new Regex(@"[^,][\S\s]+?;+[\S\s]+?(?=,\S)");
        public static CookieCollection DeserilizeCookieStr(string cookiestr)
        {
            var cookieCollection = new CookieCollection();
            //拆分Cookie
            //var listStr = RegexSplitCookie.Split(setCookie);
            cookiestr += ",T";//配合RegexSplitCookie2 加入后缀
            var listStr = RegexSplitCookie2.Matches(cookiestr);
            //循环遍历
            foreach (Match item in listStr)
            {
                //根据; 拆分Cookie 内容
                var cookieItem = item.Value.Split(';');
                var cookie = new Cookie();
                for (var index = 0; index < cookieItem.Length; index++)
                {
                    var info = cookieItem[index];
                    //第一个 默认 Cookie Name
                    //判断键值对
                    if (info.Contains("="))
                    {
                        var indexK = info.IndexOf('=');
                        var name = info.Substring(0, indexK).Trim();
                        var val = info.Substring(indexK + 1);
                        if (index == 0)
                        {
                            cookie.Name = name;
                            cookie.Value = val;
                            continue;
                        }
                        if (name.Equals("Domain", StringComparison.OrdinalIgnoreCase))
                        {
                            cookie.Domain = val;
                        }
                        else if (name.Equals("Expires", StringComparison.OrdinalIgnoreCase))
                        {
                            DateTime.TryParse(val, out var expires);
                            cookie.Expires = expires;
                        }
                        else if (name.Equals("Path", StringComparison.OrdinalIgnoreCase))
                        {
                            cookie.Path = val;
                        }
                        else if (name.Equals("Version", StringComparison.OrdinalIgnoreCase))
                        {
                            cookie.Version = Convert.ToInt32(val);
                        }
                    }
                    else
                    {
                        if (info.Trim().Equals("HttpOnly", StringComparison.OrdinalIgnoreCase))
                        {
                            cookie.HttpOnly = true;
                        }
                    }
                }
                cookieCollection.Add(cookie);
            }
            return cookieCollection;
        }

        #endregion

        public string getBliveTitle(int roomid)
        {
            string url = "https://live.bilibili.com/" + roomid;
            string rtv = _get(url);
            int title_index = rtv.IndexOf("\"title\":\"") + 9;
            string title = rtv.Substring(title_index);
            int title_len = title.IndexOf("\", \"");
            return title.Substring(0, title_len);
        }

        public string getUpState(long uid)
        {
            //https://api.bilibili.com/x/relation/stat?vmid=5659864
            string url = "https://api.bilibili.com/x/relation/stat?vmid=" + uid;
            return _get_with_cookies_and_refer(url, "https://space.bilibili.com/" + uid + "/fans/fans");
        }

        public string getCurrentUserDataStr()
        {
            string url = "https://api.bilibili.com/x/web-interface/nav";
            return _get_with_cookies_and_refer(url, "https://www.bilibili.com/");
        }

        public long getCurrentUserId()
        {
            JObject jb = JObject.Parse(getCurrentUserDataStr());
            return jb["data"].Value<long>("mid");
        }

        public BiliUser getCurrentUserData()
        {
            return new BiliUser(getCurrentUserId(), this);
        }

        public string getFanList(long uid, int pageno = 1, int pagesize = 5)
        {
            //https://api.bilibili.com/x/relation/followers?vmid=5659864&pn=1&ps=1&order=desc&jsonp=jsonp
            string url = "https://api.bilibili.com/x/relation/followers?vmid=" + uid + "&pn=" + pageno + "&ps=" + pagesize + "&order=desc&jsonp=jsonp";
            return _get_with_cookies_and_refer(url, "https://space.bilibili.com/" + uid + "/fans/fans");
        }

        public static string getBiliUserInfoJson(long uid)
        {
            string url = "https://api.bilibili.com/x/space/app/index?mid=" + uid;
            return _get(url);
        }

        public string getBiliUserDynamicJson(long uid)
        {
            string url = "https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/space_history?host_uid=" + uid + "&offset_dynamic_id=0&need_top=1";
            return _get_with_cookies(url);
        }

        public string getBiliVideoStaticsJson(long aid)
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

        [Obsolete("Ban will always be permanant now, \"len\" will take no effect.", false)]
        public string banUIDfromroom(int roomid, long uid, int len)
        {
            return banUIDfromroom(roomid, uid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roomid">房间ID</param>
        /// <param name="uid">被封禁者UID</param>
        /// <returns></returns>
        public string banUIDfromroom(int roomid, long uid)
        {
            //https://api.live.bilibili.com/banned_service/v2/Silent/add_block_user  老版本API，弃用
            /*
                { "roomid", roomid.ToString() },
                { "block_uid", uid.ToString() },
                { "hour", len.ToString() },
                { "csrf", GetCsrf() },
                { "csrf_token", GetCsrf() }
            */
            //https://api.live.bilibili.com/xlive/web-ucenter/v1/banned/AddSilentUser
            string url = "https://api.live.bilibili.com/xlive/web-ucenter/v1/banned/AddSilentUser";
            Dictionary<string, string> form = new Dictionary<string, string>
                    {
                        { "room_id", roomid.ToString() },
                        { "tuid", uid.ToString() },
                        { "mobile_app", "web" },
                        { "csrf", GetCsrf() },
                        { "csrf_token", GetCsrf() }
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
    { "csrf", GetCsrf() },
    { "csrf_token", GetCsrf() }
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

        public string getBiliUserMedal(long uid)
        {
            string url = "https://api.live.bilibili.com/xlive/web-ucenter/user/MedalWall?target_id=" + uid;
            return _get_with_manacookies_and_refer(url, "https://space.bilibili.com/" + uid);
        }

        public string sendComment(long oid, string text,int type = 1)
        {
            string url = $"https://api.bilibili.com/x/v2/reply/add";
            return _post_with_cookies(url,
                new Dictionary<string, string> {
                    { "type", type.ToString() },
                    { "oid", oid.ToString() },
                    //{ "root", oid.ToString() },
                    //{ "parent", oid.ToString() },
                    { "message", text },
                    { "csrf", CookieContext["bili_jct"].Value },
                });
        }

        //public static 
    }

}
