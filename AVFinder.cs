using System;
using System.Net;
using System.Text.RegularExpressions;

namespace BiliApi
{
    /// <summary>
    /// 从字符串中寻找AV和BV号的工具类
    /// </summary>
    public class AVFinder
    {
        public static string _getLocation(string url)
        {
            string retString = string.Empty;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.AllowAutoRedirect = false;
                try
                {
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException err)
                {
                    HttpWebResponse response = (HttpWebResponse)err.Response;
                    if (response.StatusCode == HttpStatusCode.Found || response.StatusCode == HttpStatusCode.MovedPermanently)
                    {
                        return response.GetResponseHeader("Location");
                    }
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string bvFromB23url(string url)
        {
            return bvFromPlayURL(_getLocation(url));
        }

        public static string bvFromPlayURL(string uurl)
        {
            try
            {
                int ind = uurl.IndexOf("/video/") + 7;
                uurl = uurl.Substring(ind);
                ind = uurl.IndexOf("/");
                if (ind < 0)
                {
                    ind = uurl.IndexOf("?");
                }

                if (ind < 0)
                {
                    ind = uurl.Length;
                }

                uurl = uurl.Substring(0, ind);
                return uurl;
            }
            catch
            {
                return null;
            }
        }

        public static string UrlFromString(string input)
        {
            Regex httpUrl = new Regex("(https?)://[-A-Za-z0-9+&@#/%?=~_|!:,.;]+[-A-Za-z0-9+&@#/%=~_|]");
            return httpUrl.Match(input).Value;
        }

        public static string abvFromString(string input)
        {
            Regex BV = new Regex("[Bb][Vv]1[A-Za-z0-9]{2}4[A-Za-z0-9]{3}7[A-Za-z0-9]*");
            Regex AV = new Regex("[Aa][Vv][0-9]+");
            string answer1 = BV.Match(input).Value;
            if (answer1 == null || answer1 == "") answer1 = AV.Match(input).Value;
            return answer1;
        }
    }
}
