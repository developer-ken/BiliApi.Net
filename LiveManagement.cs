﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BiliApi
{
    /// <summary>
    /// Bilibili直播间管理工具类
    /// </summary>
    public class LiveManagement
    {
        private readonly BiliLiveRoom blr;
        private BiliSession sess;
        public LiveManagement(BiliLiveRoom blr)
        {
            this.blr = blr;
            this.sess = blr.sess;
        }

        [Obsolete("B站官方已停用禁言时长功能，所有封禁将是永久的", false)]
        public bool banUID(long uid, int len)
        {
            return banUID(uid);
        }

        public bool banUID(long uid)
        {
            string result = sess.banUIDfromroom(blr.roomid, uid);
            JObject jb = (JObject)JsonConvert.DeserializeObject(result);
            return (jb.Value<int>("code") == 0);
        }

        public bool debanBID(int bid)
        {
            string result = sess.debanBIDfromroom(blr.roomid, bid);
            JObject jb = (JObject)JsonConvert.DeserializeObject(result);
            return (jb.Value<int>("code") == 0);
        }

        public List<BiliBannedUser> getBanlist()
        {
            List<BiliBannedUser> reslist = new List<BiliBannedUser>();
            JObject jb;
            int page = 1;
            do
            {
                string result = sess.getRoomBanlist(blr.roomid, page);
                if (result == null)
                {
                    break;
                }

                jb = (JObject)JsonConvert.DeserializeObject(result);
                if (jb == null || jb["data"] == null)
                {
                    break;
                }

                foreach (JToken jt in jb["data"])
                {
                    DateTime ctime = jt.Value<DateTime>("ctime");
                    DateTime be = jt.Value<DateTime>("block_end_time");
                    DateTime msgt;
                    try
                    {
                        msgt = jt.Value<DateTime>("msg_time");
                    }
                    catch
                    {
                        msgt = jt.Value<DateTime>("ctime");
                    }

                    BiliBannedUser b = new BiliBannedUser()
                    {
                        banreason = new BanReason
                        {
                            message = jt.Value<string>("msg"),
                            messagetime = msgt
                        },
                        uid = jt.Value<long>("uid"),
                        id = jt.Value<long>("id"),
                        op = jt.Value<long>("adminid"),
                        optime = ctime,
                        endtime = be,
                        len = (int)(be - ctime).TotalHours,
                        opname = jt.Value<string>("admin_uname"),
                        uname = jt.Value<string>("uname")
                    };
                    reslist.Add(b);
                }
                page++;
            } while (jb["data"].Count() > 0);
            return reslist;
        }
    }
}
