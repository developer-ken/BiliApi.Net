using BiliApi.Modules;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace BiliApi
{
    /// <summary>
    /// Bilibili用户对象
    /// </summary>
    public class BiliUser
    {
        public struct OfficialInfo
        {
            public string Title, Description;
            public int Role;
            public OfficialType Type;
        }

        public enum OfficialType
        {
            NotVerified = -1, Individual = 0, Organization = 1
        }

        public static Dictionary<long, BiliUser> userlist = new Dictionary<long, BiliUser>();
        public long uid { get; private set; }
        public string name { get; private set; }
        public string sex { get; private set; }
        public string sign { get; private set; }
        public bool fans_badge { get; private set; }
        public int coins { get; private set; }
        public string face { get; private set; }
        public int level { get; private set; }
        public int rank { get; private set; }
        public int fans { get; private set; }
        public OfficialInfo official { get; private set; }
        public JObject raw_json { get; private set; }
        private BiliSession sess;

        public static BiliUser getUser(long uid, BiliSession sess = null)
        {
            if (userlist.ContainsKey(uid))
            {
                return userlist[uid];
            }

            return new BiliUser(uid, sess);
        }

        /// <summary>
        /// 手动数据初始化
        /// 尽量别用！！
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="name"></param>
        /// <param name="sex"></param>
        /// <param name="sign"></param>
        /// <param name="fans_badge"></param>
        /// <param name="coins"></param>
        /// <param name="face"></param>
        /// <param name="level"></param>
        /// <param name="rank"></param>
        public BiliUser(long uid, string name, string sex, string sign, bool fans_badge, int coins, string face, int level, int rank, OfficialInfo official, BiliSession sess)
        {
            this.uid = uid;
            this.name = name;
            this.sex = sex;
            this.sign = sign;
            this.fans_badge = fans_badge;
            this.coins = coins;
            this.face = face;
            this.level = level;
            this.rank = rank;
            this.official = official;
            if (userlist.ContainsKey(uid))
            {
                userlist.Remove(uid);
            }
            this.sess = sess;
            userlist.Add(uid, this);
        }

        /// <summary>
        /// 从UID创建数据
        /// <para>系统会自己去抓数据</para>
        /// </summary>
        /// <param name="uid"></param>
        public BiliUser(long uid, BiliSession sess = null, bool nocache = false)
        {
            try
            {
                this.sess = sess;
                raw_json = (JObject)JsonConvert.DeserializeObject(BiliSession.getBiliUserInfoJson(uid));
                this.uid = long.Parse(raw_json["data"]["mid"].ToString());
                name = raw_json["data"]["name"].ToString();
                sex = raw_json["data"]["sex"].ToString();
                sign = raw_json["data"]["sign"].ToString();
                rank = int.Parse(raw_json["data"]["rank"].ToString());
                level = int.Parse(raw_json["data"]["level"].ToString());
                coins = int.Parse(raw_json["data"]["coins"].ToString());
                face = raw_json["data"]["face"].ToString();
                fans_badge = raw_json["data"]["fans_badge"].ToString() != "false";
                try
                {
                    official = new OfficialInfo()
                    {
                        Type = (OfficialType)raw_json["data"]["official"].Value<int>("type"),
                        Title = raw_json["data"]["official"].Value<string>("title"),
                        Description = raw_json["data"]["official"].Value<string>("desc"),
                        Role = raw_json["data"]["official"].Value<int>("role")
                    };
                }
                catch { }
            }
            catch
            {
                this.uid = uid;
            }
            if (userlist.ContainsKey(uid))
            {
                userlist.Remove(uid);
            }

            userlist.Add(uid, this);
        }

        /// <summary>
        /// 从已有json字符串实例化
        /// </summary>
        /// <param name="json"></param>
        public BiliUser(string json)
        {
            raw_json = (JObject)JsonConvert.DeserializeObject(json);
            if (raw_json["data"] == null)
            {
                uid = long.Parse(raw_json["mid"].ToString());
                name = raw_json["name"].ToString();
                sex = raw_json["sex"].ToString();
                sign = raw_json["sign"].ToString();
                rank = int.Parse(raw_json["rank"].ToString());
                level = int.Parse(raw_json["level_info"]["current_level"].ToString());
                face = raw_json["face"].ToString();
                fans = raw_json.Value<int>("fans");
                try
                {
                    official = new OfficialInfo()
                    {
                        Type = (OfficialType)raw_json["official"].Value<int>("type"),
                        Title = raw_json["official"].Value<string>("title"),
                        Role = raw_json["official"].Value<int>("role"),
                        Description = raw_json["official"].Value<string>("desc")
                    };
                }
                catch
                {
                    official = new OfficialInfo()
                    {
                        Type = (OfficialType)raw_json["Official"].Value<int>("type"),
                        Title = raw_json["Official"].Value<string>("title"),
                        Role = raw_json["Official"].Value<int>("role"),
                        Description = raw_json["Official"].Value<string>("desc")
                    };
                }
            }
            else
            {
                uid = long.Parse(raw_json["data"]["mid"].ToString());
                name = raw_json["data"]["name"].ToString();
                sex = raw_json["data"]["sex"].ToString();
                sign = raw_json["data"]["sign"].ToString();
                rank = int.Parse(raw_json["data"]["rank"].ToString());
                level = int.Parse(raw_json["data"]["level"].ToString());
                coins = int.Parse(raw_json["data"]["coins"].ToString());
                face = raw_json["data"]["face"].ToString();
                fans_badge = raw_json["data"]["fans_badge"].ToString() != "false";
                try
                {
                    official = new OfficialInfo()
                    {
                        Type = (OfficialType)raw_json["data"]["official"].Value<int>("type"),
                        Title = raw_json["data"]["official"].Value<string>("title"),
                        Role = raw_json["data"]["official"].Value<int>("role"),
                        Description = raw_json["data"]["official"].Value<string>("desc")
                    };
                }
                catch
                {
                    official = new OfficialInfo()
                    {
                        Type = (OfficialType)raw_json["data"]["Official"].Value<int>("type"),
                        Title = raw_json["data"]["Official"].Value<string>("title"),
                        Role = raw_json["data"]["Official"].Value<int>("role"),
                        Description = raw_json["data"]["Official"].Value<string>("desc")
                    };
                }
            }
            if (userlist.ContainsKey(uid))
            {
                userlist.Remove(uid);
            }

            userlist.Add(uid, this);
        }

        /// <summary>
        /// 从Json对象初始化
        /// </summary>
        /// <param name="json"></param>
        public BiliUser(JObject json)
        {
            raw_json = json;
            uid = long.Parse(raw_json["data"]["mid"].ToString());
            name = raw_json["data"]["name"].ToString();
            sex = raw_json["data"]["sex"].ToString();
            sign = raw_json["data"]["sign"].ToString();
            rank = int.Parse(raw_json["data"]["rank"].ToString());
            level = int.Parse(raw_json["data"]["level"].ToString());
            coins = int.Parse(raw_json["data"]["coins"].ToString());
            face = raw_json["data"]["face"].ToString();
            fans_badge = raw_json["data"]["fans_badge"].ToString() != "false";
            if (userlist.ContainsKey(uid))
            {
                userlist.Remove(uid);
            }

            userlist.Add(uid, this);
        }

        public override bool Equals(object obj)
        {
            try
            {
                BiliUser o = (BiliUser)obj;
                return uid.Equals(o.uid);
            }
            catch
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return uid.GetHashCode();
        }

        public List<Medal> getMedals()
        {
            List<Medal> metals = new List<Medal>();
            JObject jb = JObject.Parse(sess.getBiliUserMedal(uid));
            if (jb.Value<int>("code") != 0) return metals;
            JArray ja = (JArray)jb["data"]["list"];
            foreach (JObject obj in ja)
            {
                metals.Add(new Medal(obj));
            }
            return metals;
        }

        public static List<Medal> getMedals(BiliSession sess, long uid)
        {
            List<Medal> metals = new List<Medal>();
            JObject jb = JObject.Parse(sess.getBiliUserMedal(uid));
            if (jb.Value<int>("code") != 0) return metals;
            JArray ja = (JArray)jb["data"]["list"];
            foreach (JObject obj in ja)
            {
                metals.Add(new Medal(obj));
            }
            return metals;
        }
    }
}
