﻿using BiliApi.Auth;
using BiliApi.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;

namespace BiliApi
{
    //使用API：
    //https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/space_history?host_uid=5659864&offset_dynamic_id=0&need_top=1
    /// <summary>
    /// Bilibili空间动态获取工具类
    /// </summary>
    public class BiliSpaceDynamic
    {
        private readonly long uid = -1;
        private List<Dyncard> dyns = new List<Dyncard>();
        private readonly List<long> checked_ids = new List<long>();
        private BiliSession sess;

        public BiliSpaceDynamic(long uid, BiliSession sess)
        {
            this.sess = sess;
            this.uid = uid;
        }

        public BiliSpaceDynamic(long uid, IAuthBase auth)
        {
            this.sess = new BiliSession(auth.GetLoginCookies());
            this.uid = uid;
        }

        public bool refresh()
        {
            return refresh(getLatest());
        }

        public bool refresh(List<Dyncard> rawdata)
        {
            try
            {
                List<Dyncard> d = new List<Dyncard>(rawdata);
                d.AddRange(dyns);
                dyns = d;
                while (dyns.Count > 5)
                {
                    dyns.RemoveAt(5);
                }
                while (checked_ids.Count > 20)
                {
                    dyns.RemoveAt(0);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public List<Dyncard> fetchDynamics()
        {
            List<Dyncard> dynss = new List<Dyncard>();
            string jstring = sess.getBiliUserDynamicJson(uid);
            try
            {
                JObject jb = (JObject)JsonConvert.DeserializeObject(jstring);
                if (jb.Value<int>("code") != 0)
                {
                    throw new ApiRemoteException(jb);
                }
                if (jb == null || jb["data"] == null || jb["data"]["cards"] == null)
                {
                    return new List<Dyncard>();
                }
                ExceptionCollection excpts = new ExceptionCollection();
                foreach (JToken jobj in jb["data"]["cards"])
                {
                    try
                    {
                        JObject j = (JObject)jobj;
                        JObject card = (JObject)JsonConvert.DeserializeObject(j["card"].ToString());
                        Dyncard dyn = new Dyncard
                        {
                            dynid = j["desc"].Value<long>("dynamic_id"),
                            sendtime = TimestampHandler.GetDateTime(j["desc"].Value<long>("timestamp")),
                            type = j["desc"].Value<int>("type"),
                            view = j["desc"].Value<int>("view"),
                            repost = j["desc"].Value<int>("repost"),
                            like = j["desc"].Value<int>("like"),
                            rid = j["desc"].Value<long>("rid")
                        };
                        if (j["desc"]["user_profile"]["info"]["uname"] != null)//如果给了更多信息，就直接用上
                        {
                            dyn.sender = new BiliUser(j["desc"]["user_profile"]["info"].Value<long>("uid"),
                                j["desc"]["user_profile"]["info"].Value<string>("uname"),
                                "未知",
                                j["desc"]["user_profile"].Value<string>("sign"),
                                false,
                                0,
                                j["desc"]["user_profile"]["info"].Value<string>("face"),
                                j["desc"]["user_profile"]["level_info"].Value<int>("current_level"),
                                0,
                                new BiliUser.OfficialInfo(),
                                sess);
                        }
                        else//如果没有信息就从缓存抓取
                        if (BiliUser.userlist.ContainsKey(j["desc"]["user_profile"]["info"].Value<long>("uid")))
                        {
                            dyn.sender = BiliUser.getUser(j["desc"]["user_profile"]["info"].Value<long>("uid"), sess);
                            //使用用户数据缓存来提高速度
                            //因为监听的是同一个账号，所以缓存命中率超高
                        }
                        else//如果缓存未命中，就拿获得的UID抓取剩余信息
                        {
                            dyn.sender = BiliUser.getUser(j["desc"]["user_profile"]["info"].Value<long>("uid"), sess);
                        }

                        if (j["desc"]["orig_type"] != null)
                        {
                            dyn.origintype = j["desc"].Value<int>("orig_type");
                        }
                        if (card["origin"] != null)
                        {
                            dyn.card_origin = (JObject)JsonConvert.DeserializeObject(card["origin"].ToString());
                        }
                        switch (dyn.type)
                        {
                            case 1://普通动态
                            case 4://？出现在转发和普通动态
                                dyn.dynamic = card["item"].Value<string>("content");
                                if (dyn.dynamic.Length > 23)
                                {
                                    dyn.short_dynamic = dyn.dynamic.Substring(0, 20) + "...";
                                }
                                else
                                {
                                    dyn.short_dynamic = dyn.dynamic;
                                }

                                break;
                            case 2://图片
                                dyn.dynamic = card["item"].Value<string>("description");
                                if (dyn.dynamic.Length > 23)
                                {
                                    dyn.short_dynamic = dyn.dynamic.Substring(0, 20) + "...";
                                }
                                else
                                {
                                    dyn.short_dynamic = dyn.dynamic;
                                }

                                break;
                            case 256://音频
                                break;
                            case 8://视频
                                dyn.vinfo = new Videoinfo
                                {
                                    bvid = j["desc"].Value<string>("bvid"),
                                    title = card.Value<string>("title"),
                                    discription = card.Value<string>("desc")
                                };
                                if (dyn.vinfo.discription.Length > 23)
                                {
                                    dyn.vinfo.short_discription = dyn.vinfo.discription.Substring(0, 20) + "...";
                                }
                                else
                                {
                                    dyn.vinfo.short_discription = dyn.vinfo.discription;
                                }

                                dyn.vinfo.av = j["desc"].Value<long>("rid");
                                dyn.dynamic = card.Value<string>("dynamic");
                                break;
                            default:
                                break;
                        }
                        dynss.Add(dyn);
                    }
                    catch (Exception err)
                    {
                        excpts.Add(err);
                    }
                }
                if (excpts.Count > 0) throw excpts;
                //while (dynss.Count > 5)
                //{
                //    dynss.RemoveAt(5);
                //}
                return dynss;
            }
            catch (ExceptionCollection)
            {
                throw;
            }
            catch (Exception err)
            {
                throw new UnexpectedResultException(jstring, err);
            }
        }

        public List<Dyncard> getLatest()
        {
            List<Dyncard> fetched = fetchDynamics();
            List<Dyncard> latese = new List<Dyncard>();
            foreach (Dyncard dync in fetched)
            {
                if (dyns.Contains(dync))
                {
                    continue;
                }

                if (checked_ids.Contains(dync.dynid))
                {
                    continue;
                }

                latese.Add(dync);
                checked_ids.Add(dync.dynid);
            }
            return latese;
        }

        public List<ShareCard> GetAllDynCardShares(Dyncard card, out int count, string offset = "")
        {
            var id = card.dynid;
            string url = @"https://api.vc.bilibili.com/dynamic_repost/v1/dynamic_repost/repost_detail?dynamic_id=" + id +
                (offset.Length > 1 ? ("&offset=" + offset) : "");
            string jstr = sess._get_with_cookies(url);
            JObject jb = JObject.Parse(jstr);
            List<ShareCard> dyn = new List<ShareCard>();
            foreach (JObject j in (JArray)jb["data"]["items"])
            {
                dyn.Add(
                        new ShareCard
                        {
                            Uid = j["desc"].Value<long>("uid"),
                            UName = j["desc"]["user_profile"]["info"].Value<string>("uname"),
                            UFace = j["desc"]["user_profile"]["info"].Value<string>("face"),
                            Content = GetShareTextContent(j.Value<string>("card"))
                        }
                    );
            }
            bool hasmore = jb["data"].Value<bool>("has_more");
            if (hasmore)
            {
                var offs = jb["data"].Value<string>("offset");
                dyn.AddRange(GetAllDynCardShares(card, out _, offs));
            }
            count = jb["data"].Value<int>("total");
            return dyn;
        }

        public static List<ShareCard> GetPageDynCardShares(long id, out int count, ref string offset,out bool more)
        {
            string url = @"https://api.vc.bilibili.com/dynamic_repost/v1/dynamic_repost/repost_detail?dynamic_id=" + id +
                (offset.Length > 1 ? ("&offset=" + offset) : "");
            string jstr = BiliSession._get(url);
            JObject jb = JObject.Parse(jstr);
            List<ShareCard> dyn = new List<ShareCard>();
            foreach (JObject j in (JArray)jb["data"]["items"])
            {
                dyn.Add(
                        new ShareCard
                        {
                            Uid = j["desc"].Value<long>("uid"),
                            UName = j["desc"]["user_profile"]["info"].Value<string>("uname"),
                            UFace = j["desc"]["user_profile"]["info"].Value<string>("face"),
                            Content = GetShareTextContent(j.Value<string>("card"))
                        }
                    );
            }
            more = jb["data"].Value<bool>("has_more");
            if (more)
            {
                offset = jb["data"].Value<string>("offset");
            }
            count = jb["data"].Value<int>("total");
            return dyn;
        }

        public static string GetShareTextContent(string json)
        {
            JObject jb = JObject.Parse(json);
            return jb["item"].Value<string>("content");
        }
    }

    public struct Dyncard
    {
        public long dynid, rid;
        public int type, view, repost, like, origintype;
        public string dynamic;
        public string short_dynamic;
        public BiliUser sender;
        public DateTime sendtime;
        public Videoinfo vinfo;
        public JObject card_origin;
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is Dyncard)
            {
                Dyncard b = (Dyncard)obj;
                return dynid == b.dynid;
            }
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            DateTime dt_1970 = new DateTime(1970, 1, 1, 8, 0, 0);
            return (int)(sendtime - dt_1970).TotalSeconds;
        }
    }

    public struct ShareCard
    {
        public long Uid;
        public string UName;
        public string UFace;
        public string Content;
    }

    public struct Videoinfo
    {
        public string discription;
        public string short_discription;
        public string bvid;
        public string title;
        public long av;
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is Videoinfo)
            {
                Videoinfo b = (Videoinfo)obj;
                return bvid == b.bvid;
            }
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return av.GetHashCode();
        }
    }
}
