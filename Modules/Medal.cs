using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BiliApi.Modules
{
    public class Medal
    {
        public int Level { get; private set; }
        public long TargetId { get; private set; }
        public string TargetName { get; private set; }
        public string Name { get; private set; }
        public int GuardLevel { get; private set; }
        public int MedalId { get; private set; }
        public int Intimacy { get; private set; }

        public Medal(JObject jb)
        {
            Level = jb["medal_info"].Value<int>("level");
            TargetId = jb["medal_info"].Value<long>("target_id");
            TargetName = jb.Value<string>("target_name");
            Name = jb["medal_info"].Value<string>("medal_name");
            GuardLevel = jb["medal_info"].Value<int>("guard_level");
            MedalId = jb["medal_info"].Value<int>("medal_id");
            Intimacy = jb["medal_info"].Value<int>("intimacy");
        }
    }
}
