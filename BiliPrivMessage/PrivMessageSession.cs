﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;

namespace BiliApi.BiliPrivMessage
{
    public class PrivMessageSession
    {
        public bool loaded = false;
        public long talker_id;
        public int sessiontype;
        public long session_ts;
        public int unread_cnt;
        public PrivMessage lastmessage;
        public Dictionary<PrivMessage, bool> messages;
        public string lastjson;
        public bool followed;
        public bool isGroup;
        private long uid;
        private BiliSession sess;
        public PrivMessageSession(JToken json, BiliSession sess)
        {
            this.sess = sess;
            init(json);
        }
        public PrivMessageSession(long targetuid, BiliSession sess)
        {
            this.sess = sess;
            talker_id = targetuid;
            CookieCollection ck = sess.CookieContext;
            uid = long.Parse(ck["DedeUserID"].Value);
            isGroup = false;
        }

        public void init(JToken json)
        {
            talker_id = json.Value<int>("talker_id");
            sessiontype = json.Value<int>("session_type");
            session_ts = json.Value<long>("session_ts");
            unread_cnt = json.Value<int>("unread_count");
            lastmessage = new PrivMessage(json["last_msg"], sess);
            followed = json.Value<int>("is_follow") == 1;
            isGroup = json["group_name"] != null;
            messages = new Dictionary<PrivMessage, bool>();
            CookieCollection ck = sess.CookieContext;
            uid = int.Parse(ck["DedeUserID"].Value);
        }

        public static PrivMessageSession openSessionWith(long taruid, BiliSession sess)
        {
            string rtv = sess._get_with_cookies("https://api.vc.bilibili.com/session_svr/v1/session_svr/session_detail?talker_id=" + taruid + "&session_type=1");
            JObject raw_json = (JObject)JsonConvert.DeserializeObject(rtv);
            if (raw_json.Value<int>("code") == 0)
            {
                return new PrivMessageSession(raw_json["data"], sess);
            }
            else
            {
                return new PrivMessageSession(taruid, sess);
            }
        }

        public static void closeSession(long id, int sstype, BiliSession sess)
        {
            //https://api.vc.bilibili.com/session_svr/v1/session_svr/remove_session
            //post: talker_id,session_type,build: 0,mobi_app: web,csrf,csrf_token
            sess._post_with_cookies_and_refer("https://api.vc.bilibili.com/session_svr/v1/session_svr/remove_session",
                "https://message.bilibili.com/", new Dictionary<string, string> {
                    { "talker_id", id.ToString() } ,
                    { "session_type", sstype.ToString() },
                    { "build", "0" },
                    { "mobi_app", "web" },
                    { "csrf", sess.GetCsrf() },
                    { "csrf_token", sess.GetCsrf() },
                });
        }

        public void Close()
        {
            closeSession(this.talker_id, this.sessiontype, sess);
        }

        public bool reload()
        {
            CookieCollection ck = sess.CookieContext;
            uid = int.Parse(ck["DedeUserID"].Value);
            string rtv = sess._get_with_cookies("https://api.vc.bilibili.com/session_svr/v1/session_svr/session_detail?talker_id=" + uid + "&session_type=1");
            JObject raw_json = (JObject)JsonConvert.DeserializeObject(rtv);
            if (raw_json.Value<int>("code") == 0)
            {
                init(raw_json);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void fetch()
        {
            string jsonstring = sess._get_with_cookies("https://api.vc.bilibili.com/svr_sync/v1/svr_sync/fetch_session_msgs?sender_device_id=1&talker_id=" + talker_id + "&session_type=" + sessiontype + "&size=20&build=0&mobi_app=web");
            lastjson = jsonstring;
            JObject json = (JObject)JsonConvert.DeserializeObject(jsonstring);
            foreach (JObject jb in json["data"]["messages"])
            {
                try
                {
                    PrivMessage p = new PrivMessage(jb, sess);
                    if (!messages.ContainsKey(p))
                    {
                        messages.Add(p, false);
                    }
                }
                catch
                {
                    //一条私信被丢弃
                }
            }

            messages.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);
            loaded = true;
        }

        public void updateFromJson(JToken json)
        {
            if (json.Value<int>("unread_count") <= 1)
            {
                PrivMessage p = new PrivMessage(json["last_msg"], sess);
                if (!messages.ContainsKey(p))
                {
                    messages.Add(p, false);
                }

                messages.OrderBy(o => o.Key).ToDictionary(o => o.Key, pp => pp.Value);
            }
            else
            {
                fetch();
            }
        }

        public bool sendMessage(string text)
        {
            //https://api.vc.bilibili.com/web_im/v1/web_im/send_msg
            Dictionary<string, string> kvs = new Dictionary<string, string>();
            CookieCollection ck = sess.CookieContext;
            JObject job = new JObject
            {
                { "content", text }
            };
            kvs.Add("msg[sender_uid]", ck["DedeUserID"].Value);
            kvs.Add("msg[receiver_id]", talker_id.ToString());
            kvs.Add("msg[receiver_type]", "1");
            kvs.Add("msg[msg_type]", "1");
            kvs.Add("msg[msg_status]", "0");
            kvs.Add("msg[content]", job.ToString());
            kvs.Add("msg[new_face_version]", "0");
            kvs.Add("msg[timestamp]", TimestampHandler.GetTimeStamp(DateTime.Now).ToString());
            kvs.Add("msg[dev_id]", "A8DF21F2-98F7-43A6-9EB4-E348F9B41EBC");//暂时不知道如何生成，但该项似乎不影响私信的发送
            kvs.Add("build", "0");
            kvs.Add("mobi_app", "web");
            kvs.Add("csrf_token", ck["bili_jct"].Value);
            kvs.Add("from_firework", "0");
            kvs.Add("csrf", ck["bili_jct"].Value);
            string response = sess._post_with_cookies("https://api.vc.bilibili.com/web_im/v1/web_im/send_msg", kvs);
            if (response == "")
            {
                return false;
            }

            JObject raw_json = (JObject)JsonConvert.DeserializeObject(response);
            return raw_json.Value<int>("code") == 0;
        }

        public bool sendMessage(JObject job)
        {
            //https://api.vc.bilibili.com/web_im/v1/web_im/send_msg
            Dictionary<string, string> kvs = new Dictionary<string, string>();
            CookieCollection ck = sess.CookieContext;
            kvs.Add("msg[sender_uid]", ck["DedeUserID"].Value);
            kvs.Add("msg[receiver_id]", talker_id.ToString());
            kvs.Add("msg[receiver_type]", "1");
            kvs.Add("msg[msg_type]", "2");
            kvs.Add("msg[msg_status]", "0");
            kvs.Add("msg[content]", job.ToString());
            kvs.Add("msg[new_face_version]", "0");
            kvs.Add("msg[timestamp]", TimestampHandler.GetTimeStamp(DateTime.Now).ToString());
            kvs.Add("msg[dev_id]", "A8DF21F2-98F7-43A6-9EB4-E348F9B41EBC");//暂时不知道如何生成，但该项似乎不影响私信的发送
            kvs.Add("build", "0");
            kvs.Add("mobi_app", "web");
            kvs.Add("csrf_token", ck["bili_jct"].Value);
            kvs.Add("from_firework", "0");
            kvs.Add("csrf", ck["bili_jct"].Value);
            string response = sess._post_with_cookies("https://api.vc.bilibili.com/web_im/v1/web_im/send_msg", kvs);
            if (response == "")
            {
                return false;
            }

            JObject raw_json = (JObject)JsonConvert.DeserializeObject(response);
            return raw_json.Value<int>("code") == 0;
        }

        public bool SendImage(Bitmap bmap)
        {
            //上传图片
            MemoryStream ms = new MemoryStream();
            bmap.Save(ms, ImageFormat.Png);
            var upload = sess.PostFile(
                "https://api.vc.bilibili.com/api/v1/drawImage/upload",
                "https://message.bilibili.com",
                ms.ToArray(),
                "file_up",
                "image/png",
                "picturen.png",
                new Dictionary<string, string>()
                {
                    {"biz","draw"},
                    {"category","daily"},
                    {"build","0"},
                    {"mobi_app","web"}
                }
                );
            JObject jb = JObject.Parse(upload);
            if (jb.Value<int>("code") != 0) throw new Exceptions.ApiRemoteException(jb);

            JObject payload = new JObject();
            payload.Add("url", jb["data"]["image_url"]);
            payload.Add("width", jb["data"]["image_width"]);
            payload.Add("height", jb["data"]["image_height"]);
            payload.Add("imageType", "png");
            payload.Add("original", "1");
            payload.Add("size", ms.ToArray().Length / 1024);
            return sendMessage(payload);
        }

        public List<PrivMessage> pick_latest_messages()
        {
            List<PrivMessage> tmp = new List<PrivMessage>();
            foreach (KeyValuePair<PrivMessage, bool> msg in messages)
            {
                if (!msg.Value)
                {
                    tmp.Add(msg.Key);
                }
            }
            foreach (PrivMessage msg in tmp)
            {
                messages[msg] = true;
            }
            return tmp;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if ((obj.GetType().Equals(GetType())) == false)
            {
                return false;
            }
            PrivMessageSession rmt = (PrivMessageSession)obj;
            return talker_id.Equals(rmt.talker_id);
        }

        public override int GetHashCode()
        {
            return talker_id.GetHashCode();
        }
    }
}
