using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BiliApi.Exceptions
{
    class AuthenticateFailedException : ApiRemoteException
    {
        public AuthenticateFailedException(JObject payload, string msg = "Not logged in or login expired") : base(payload, msg) { }
    }
}
