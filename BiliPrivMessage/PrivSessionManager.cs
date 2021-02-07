using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using BiliApi.Auth;
using BiliApi.Exceptions;

namespace BiliApi.BiliPrivMessage
{
    public class PrivSessionManager
    {
        public List<PrivMessageSession> followed_sessions;
        public List<PrivMessageSession> unfollowed_sessions;
        public List<PrivMessageSession> group_sessions;
        public ThirdPartAPIs sess;
        public long last_refresh = 0;
        public string lastjson;

        /// <summary>
        /// 会话管理器
        /// </summary>
        public PrivSessionManager(IAuthBase auth)
        {
            sess = new ThirdPartAPIs(auth.GetLoginCookies());
            followed_sessions = new List<PrivMessageSession>();
            unfollowed_sessions = new List<PrivMessageSession>();
            group_sessions = new List<PrivMessageSession>();
        }
        public PrivSessionManager(ThirdPartAPIs sess)
        {
            this.sess = sess;
            followed_sessions = new List<PrivMessageSession>();
            unfollowed_sessions = new List<PrivMessageSession>();
            group_sessions = new List<PrivMessageSession>();
        }

        public void refresh()
        {
            fetchFollowed();
            fetchUnfollowed();
            fetchGroups();
            last_refresh = TimestampHandler.GetTimeStamp16(DateTime.Now);
        }

        public void smartRefresh()
        {
            //https://api.vc.bilibili.com/session_svr/v1/session_svr/single_unread?unread_type=0&build=0&mobi_app=web
            string rtv = sess._get_with_cookies("https://api.vc.bilibili.com/session_svr/v1/session_svr/single_unread?unread_type=0&build=0&mobi_app=web");
            lastjson = rtv;
            JObject raw_json = (JObject)JsonConvert.DeserializeObject(rtv);
            if (raw_json.Value<int>("code") != 0)
            {
                throw new ApiRemoteException(raw_json);
            }
            int unfollowed_ = raw_json["data"].Value<int>("unfollow_unread");
            int followed_ = raw_json["data"].Value<int>("follow_unread");
            if (unfollowed_ > 0 || followed_ > 0)
            {
                updateSessions();
            }
        }

        public void updateSessions()
        {
            sess._get_with_cookies("https://api.vc.bilibili.com/session_svr/v1/session_svr/new_sessions?begin_ts=" + last_refresh + "&build=0&mobi_app=web");
            string rtv = sess._get_with_cookies("https://api.vc.bilibili.com/session_svr/v1/session_svr/ack_sessions?begin_ts=" + last_refresh + "&build=0&mobi_app=web");
            lastjson = rtv;
            JObject raw_json = (JObject)JsonConvert.DeserializeObject(rtv);
            if (raw_json.Value<int>("code") != 0)
            {//发生错误
                throw new ApiRemoteException(raw_json);
            }
            List<PrivMessageSession> sessionlist = new List<PrivMessageSession>();
            foreach (JToken jobj in raw_json["data"]["session_list"])
            {
                PrivMessageSession session = new PrivMessageSession(jobj, sess);
                if (session.followed)
                {
                    if (!followed_sessions.Contains(session))
                    {
                        followed_sessions.Add(session);
                    }
                    else
                    {
                        followed_sessions[followed_sessions.IndexOf(session)].updateFromJson(jobj);
                    }
                }
                else if (!session.isGroup)
                {
                    if (!unfollowed_sessions.Contains(session))
                    {
                        unfollowed_sessions.Add(session);
                    }
                    else
                    {
                        unfollowed_sessions[unfollowed_sessions.IndexOf(session)].updateFromJson(jobj);
                    }
                }
                else
                {
                    if (!group_sessions.Contains(session))
                    {
                        group_sessions.Add(session);
                    }
                    else
                    {
                        group_sessions[group_sessions.IndexOf(session)].updateFromJson(jobj);
                    }
                }
            }
            last_refresh = TimestampHandler.GetTimeStamp16(DateTime.Now);
        }

        public void fetchFollowed()
        {
            string rtv = sess._get_with_cookies("https://api.vc.bilibili.com/session_svr/v1/session_svr/get_sessions?session_type=1&group_fold=1&unfollow_fold=1&sort_rule=2&build=0&mobi_app=web");
            lastjson = rtv;
            JObject raw_json = (JObject)JsonConvert.DeserializeObject(rtv);
            if (raw_json.Value<int>("code") != 0)
            {//发生错误
                throw new ApiRemoteException(raw_json);
            }
            List<PrivMessageSession> sessionlist = new List<PrivMessageSession>();
            foreach (JToken jobj in raw_json["data"]["session_list"])
            {
                PrivMessageSession session = new PrivMessageSession(jobj, sess);
                if (!followed_sessions.Contains(session))
                {
                    followed_sessions.Add(session);
                }
            }
        }

        public void fetchUnfollowed()
        {
            string rtv = sess._get_with_cookies("https://api.vc.bilibili.com/session_svr/v1/session_svr/get_sessions?session_type=2&group_fold=1&unfollow_fold=1&sort_rule=2&build=0&mobi_app=web");
            lastjson = rtv;
            JObject raw_json = (JObject)JsonConvert.DeserializeObject(rtv);
            if (raw_json.Value<int>("code") != 0)
            {//发生错误
                throw new ApiRemoteException(raw_json);
            }
            List<PrivMessageSession> sessionlist = new List<PrivMessageSession>();
            foreach (JToken jobj in raw_json["data"]["session_list"])
            {
                PrivMessageSession session = new PrivMessageSession(jobj, sess);
                if (!unfollowed_sessions.Contains(session))
                {
                    unfollowed_sessions.Add(session);
                }
            }
        }

        public void fetchGroups()
        {
            string rtv = sess._get_with_cookies("https://api.vc.bilibili.com/session_svr/v1/session_svr/get_sessions?session_type=3&group_fold=1&unfollow_fold=1&sort_rule=2&build=0&mobi_app=web");
            lastjson = rtv;
            JObject raw_json = (JObject)JsonConvert.DeserializeObject(rtv);
            if (raw_json.Value<int>("code") != 0)
            {//发生错误
                throw new ApiRemoteException(raw_json);
            }
            List<PrivMessageSession> sessionlist = new List<PrivMessageSession>();
            foreach (JToken jobj in raw_json["data"]["session_list"])
            {
                PrivMessageSession session = new PrivMessageSession(jobj, sess);
                if (!group_sessions.Contains(session))
                {
                    group_sessions.Add(session);
                }
            }
        }

    }
}
