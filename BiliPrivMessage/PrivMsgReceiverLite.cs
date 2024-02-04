using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using BiliApi.Exceptions;

namespace BiliApi.BiliPrivMessage
{
    public class PrivMsgReceiverLite
    {
        BiliSession sess;
        public DateTime LastUpdate = new DateTime(1999, 12, 12);

        public PrivMsgReceiverLite(BiliSession session)
        {
            sess = session;
        }

        /// <summary>
        /// 获取（自上次更新以来）新的私信会话
        /// </summary>
        /// <returns>新会话列表</returns>
        /// <exception cref="ApiRemoteException">API出错</exception>
        public List<PrivMessageSession> GetNewSessions()
        {
            string rtv = sess._get_with_cookies("https://api.vc.bilibili.com/session_svr/v1/session_svr/new_sessions?begin_ts=" +
                TimestampHandler.GetTimeStamp16(LastUpdate) + "&build=0&mobi_app=web");
            sess._get_with_cookies("https://api.vc.bilibili.com/session_svr/v1/session_svr/ack_sessions?begin_ts=" +
                TimestampHandler.GetTimeStamp16(LastUpdate) + "&build=0&mobi_app=web");
            LastUpdate = DateTime.Now;
            JObject raw_json = (JObject)JsonConvert.DeserializeObject(rtv);
            if (raw_json.Value<int>("code") != 0)
            {//发生错误
                throw new ApiRemoteException(raw_json);
            }
            List<PrivMessageSession> rtvlist = new List<PrivMessageSession>();
            foreach (JToken jobj in raw_json["data"]["session_list"])
            {
                var psess = new PrivMessageSession(jobj, sess);
                if(psess.lastmessage.talker.uid == sess.getCurrentUserId())
                {
                    continue;
                }
                rtvlist.Add(psess);
            }
            return rtvlist;
        }
    }
}
