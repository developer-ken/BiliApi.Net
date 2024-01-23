using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BiliApi.Modules
{
    public class Medal
    {
        public int Level { get; set; }
        public long TargetId { get; set; }
        public string TargetName { get; set; }
        public string Name { get; set; }
        public int GuardLevel { get; set; }
        public int MedalId { get; set; }
        public int Intimacy { get; set; }

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

        public Medal() { }
    }
}
