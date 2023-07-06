using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace BiliApi
{
    [Obsolete("Please stop using this class and use BiliApi instead.\nThe classname is misleading, so it is now renamed.")]
    public class ThirdPartAPIs : BiliSession
    {
        public ThirdPartAPIs(CookieCollection c) : base(c)
        {
        }
    }
}
