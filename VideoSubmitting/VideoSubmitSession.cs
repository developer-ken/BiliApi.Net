using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BiliApi.VideoSubmitting
{
    public class VideoSubmitSession
    {
        HttpClient client = new HttpClient();
        public string UPLOAD_CDN = "upos-cs-upcdnbldsa.bilivideo.com";
        string bilicookie;
        string vpath, fname;
        string upos_uri, auth;
        int chunk_size, biz_id;
        public VideoSubmitSession(BiliSession bsession, string videopath)
        {
            bilicookie = bsession.GetCookieString();
            client.DefaultRequestHeaders.Add("User-Agent", BiliSession.USER_AGENT);
            client.DefaultRequestHeaders.Add("cookie", bilicookie);
            vpath = videopath;
        }

        public VideoSubmitSession(string cookies, string videopath)
        {
            bilicookie = cookies;
            client.DefaultRequestHeaders.Add("User-Agent", BiliSession.USER_AGENT);
            client.DefaultRequestHeaders.Add("cookie", bilicookie);
            vpath = videopath;
        }

        public string CookieStrGetValue(string cstr, string key)
        {
            string[] cookiePairs = cstr.Split(';');
            foreach (string cookiePair in cookiePairs)
            {
                string[] keyValue = cookiePair.Split('=');
                if (keyValue.Length == 2 && keyValue[0].Trim() == key)
                {
                    return keyValue[1].Trim();
                }
            }
            return null;
        }

        public async Task PreUpload()
        {
            var size = new FileInfo(vpath).Length;
            var url = $"https://member.bilibili.com/preupload?name={Uri.EscapeDataString(Path.GetFileName(vpath))}&size={size}&r=upos&profile=ugcupos/bup&ssl=0&version=2.7.1&build=2070100&upcdn=bda&probe_version=20200224";
            var response = await client.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            var doc = JObject.Parse(json);
            auth = doc["auth"].ToString();
            upos_uri = doc["upos_uri"].ToString();
            chunk_size = doc["chunk_size"].Value<int>();
            biz_id = doc["biz_id"].Value<int>();
            fname = upos_uri.Split('/').Last().Split('.').First();
        }

        public async Task Upload()
        {
            var size = new FileInfo(vpath).Length;
            var url = $"https://{UPLOAD_CDN}/{upos_uri.Replace("upos://", "")}?uploads&output=json";
            var scontent = new StringContent("uploads&output=json");
            scontent.Headers.Add("X-Upos-Auth", auth);
            var response = await client.PostAsync(url, scontent);
            var json = await response.Content.ReadAsStringAsync();
            var doc = JObject.Parse(json);
            var upload_id = doc["upload_id"].ToString();

            // Upload chunks
            var chunks = (int)Math.Ceiling((double)size / chunk_size);
            using (var file = File.OpenRead(vpath))
            {
                for (var i = 0; i < chunks; i++)
                {
                    var data = new byte[chunk_size];
                    var read = file.Read(data, 0, chunk_size);
                    var content = new ByteArrayContent(data, 0, read);
                    content.Headers.Add("X-Upos-Auth", auth);
                    var uploadUrl = $"https://{UPLOAD_CDN}/{upos_uri.Replace("upos://", "")}?partNumber={i + 1}&uploadId={upload_id}&chunk={i}&chunks={chunks}&size={read}&start={i * chunk_size}&end={(i + 1) * chunk_size - 1}&total={size}";
                    var uploadResponse = await client.PutAsync(uploadUrl, content);
                }
            }

            // Notify upload completion
            var completionUrl = $"https://{UPLOAD_CDN}/{upos_uri.Replace("upos://", "")}?output=json&name={Uri.EscapeDataString(Path.GetFileName(vpath))}&profile=ugcupos%2Fbup&uploadId={upload_id}&biz_id={biz_id}";
            var completionData = new { parts = Enumerable.Range(1, chunks).Select(i => new { partNumber = i, eTag = "etag" }) };
            var vscontent = new StringContent(JsonConvert.SerializeObject(completionData), Encoding.UTF8, "application/json");
            vscontent.Headers.Add("X-Upos-Auth", auth);
            var completionResponse = await client.PostAsync(completionUrl, vscontent);
        }

        /// <summary>
        /// 提交视频
        /// </summary>
        /// <param name="form">上传表单</param>
        /// <returns>视频的BV号</returns>
        public async Task<string> Submit(BiliVideoForm form)
        {
            var crsf = CookieStrGetValue(bilicookie, "bili_jct");
            string lk = $"https://member.bilibili.com/x/vu/web/add/v3?csrf={crsf}";

            var data = new JObject
            {
                ["cover"] = "",
                ["title"] = form.Title,
                ["copyright"] = form.Copyright,
                ["tid"] = form.TypeId,
                ["tag"] = form.Tags,
                ["desc_format_id"] = 32,
                ["desc"] = form.Description,
                ["recreate"] = -1,
                ["dynamic"] = form.DynamicText,
                ["interactive"] = 0,
                ["videos"] = new JArray
                    {
                        new JObject
                        {
                            ["filename"] = fname,
                            ["title"] = form.Title,
                            ["desc"] = form.Description,
                            ["cid"] = biz_id
                        }
                    },
                ["act_reserve_create"] = 0,
                ["no_disturbance"] = 0,
                ["no_reprint"] = 1,
                ["subtitle"] = new JObject
                {
                    ["open"] = 0,
                    ["lan"] = ""
                },
                ["open_elec"] = 1,
                ["dolby"] = 0,
                ["lossless_music"] = 0,
                ["up_selection_reply"] = form.RestrictedReply,
                ["up_close_reply"] = form.DisableReply,
                ["up_close_danmu"] = form.DisableDanmaku,
                ["web_os"] = 1,
                ["csrf"] = crsf
            };

            client.DefaultRequestHeaders.Add("referer", "https://member.bilibili.com/video/upload.html");
            var content = new StringContent(data.ToString(), Encoding.UTF8, "application/json");
            content.Headers.Add("dnt", "1");
            content.Headers.Add("origin", "https://member.bilibili.com");
            content.Headers.Add("sec-fetch-dest", "empty");
            content.Headers.Add("sec-fetch-mode", "cors");
            content.Headers.Add("sec-fetch-site", "same-origin");
            var response = await client.PostAsync(lk, content);
            var responseString = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(responseString);
            if (result["code"].Value<int>() == 0)
            {
                return result?["data"]?["bvid"]?.ToString();
            }
            else
            {
                throw new Exception(responseString);
            }
        }

        public async Task<string> GetRecommendedTags(string title, int count = 5, int typeid = 21, string desc = "")
        {
            string lk = $"https://member.bilibili.com/x/vupre/web/archive/tags?typeid={typeid}&title={Uri.EscapeDataString(title)}&filename={Uri.EscapeDataString(Path.GetFileName(vpath))}&desc={Uri.EscapeDataString(desc)}&cover=&groupid=0&vfea=";
            var result = await client.GetAsync(lk);
            var str = await result.Content.ReadAsStringAsync();
            JObject doc = JObject.Parse(str);
            string tags = string.Join(",", doc["data"].Take(count).Select(x => x["tag"].ToString()));
            return tags;
        }
    }
}
