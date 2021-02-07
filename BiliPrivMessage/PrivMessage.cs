using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace BiliApi.BiliPrivMessage
{
    public class PrivMessage : IComparable
    {
        public int recieiver_id, timestamp, msgtype, msg_seqno;
        public long msg_key;
        public BiliUser talker;
        public string content;
        public JObject content_json;

        public PrivMessage(JToken json,ThirdPartAPIs sess)
        {
            recieiver_id = json.Value<int>("receiver_id");
            timestamp = json.Value<int>("timestamp");
            talker = BiliUser.getUser(json.Value<int>("sender_uid"),sess);
            msgtype = json.Value<int>("msg_type");
            msg_seqno = json.Value<int>("msg_seqno");
            msg_key = json.Value<long>("msg_key");
            object jobjdes = JsonConvert.DeserializeObject(json.Value<string>("content"));
            if (jobjdes.GetType() != json.GetType())
            {
                content = jobjdes.ToString();
            }
            else
            {
                content_json = (JObject)jobjdes;
                content = content_json.Value<string>("content");
            }
        }

        public int CompareTo(object obj)
        {
            if (Equals(obj))
            {
                return 0;
            }

            if (obj == null)
            {
                return 0;
            }
            if ((obj.GetType().Equals(GetType())) == false)
            {
                return 0;
            }
            PrivMessage pv = (PrivMessage)obj;
            if (pv.timestamp.Equals(timestamp))
            {
                return msg_seqno.CompareTo(pv.msg_seqno);
            }
            else
            {
                return timestamp.CompareTo(pv.timestamp);
            }
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
            PrivMessage rmt = (PrivMessage)obj;
            return msg_key.Equals(rmt.msg_key);
        }

        public override int GetHashCode()
        {
            return msg_key.GetHashCode();
        }
    }
}
