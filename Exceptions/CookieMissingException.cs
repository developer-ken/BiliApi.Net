using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace BiliApi.Exceptions
{
    public class CookieMissingException : Exception
    {
        public CookieCollection Cookies;
        public string KeyRequired;

        public CookieMissingException(CookieCollection cookies,string key_required) : base("Key '"+key_required+"' is missing in <CookieCollection>")
        {
            Cookies = cookies;
            KeyRequired = key_required;
        }
    }
}
