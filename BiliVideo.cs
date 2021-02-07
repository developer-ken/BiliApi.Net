using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace BiliApi
{
    /// <summary>
    /// Bilibili视频对象
    /// </summary>
    public class BiliVideo
    {
        public string vid;
        public string title;
        public string cover;
        public bool loaded = false;
        public BiliUser owner;
        public List<BiliUser> participants;
        private ThirdPartAPIs sess;
        public BiliVideo(string vid, ThirdPartAPIs sess)
        {
            this.sess = sess;
            this.vid = vid;
            participants = new List<BiliUser>();
            fetchVideoInfo();
        }
        public void fetchVideoInfo()
        {
            try
            {
                string js = sess.getBiliVideoInfoJson(vid.ToString());
                JObject json = (JObject)JsonConvert.DeserializeObject(js);
                if (json == null || json.Value<int>("code") != 0)
                {
                    return;
                }

                JObject vd = (JObject)json["videoData"];
                title = vd.Value<string>("title");
                cover = vd.Value<string>("pic");
                JObject ud = (JObject)json["upData"];
                owner = new BiliUser(ud.ToString());
                JArray parti = (JArray)json["videoData"]["staff"];
                if (parti != null)
                {
                    foreach (JObject jb in parti)
                    {
                        participants.Add(new BiliUser(jb.Value<int>("mid"),sess));
                    }
                }

                loaded = true;
            }
            catch (Exception)
            {
                throw;
            }
            //owner = 
        }
    }
}
