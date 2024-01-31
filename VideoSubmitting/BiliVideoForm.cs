using System;
using System.Collections.Generic;
using System.Text;

namespace BiliApi.VideoSubmitting
{
    public class BiliVideoForm
    {
        public string Title, Description, DynamicText, Tags;
        public int TypeId, Copyright;
        public bool DisableReply, DisableDanmaku, RestrictedReply;

        public BiliVideoForm()
        {
            Title = "";
            Description = "";
            DynamicText = "";
            Tags = "";
            TypeId = 21;
            DisableReply = false;
            DisableDanmaku = false;
            RestrictedReply = false;
            Copyright = 1;
        }
    }
}
