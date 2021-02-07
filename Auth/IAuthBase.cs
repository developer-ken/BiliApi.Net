using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace BiliApi.Auth
{
    public interface IAuthBase
    {
        CookieCollection GetLoginCookies();
    }
}
