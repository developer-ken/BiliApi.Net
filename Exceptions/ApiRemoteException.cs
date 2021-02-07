using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BiliApi.Exceptions
{
    /// <summary>
    /// 远程api出错
    /// <para>当API调用后远端返回异常时触发，包含API返回信息。</para>
    /// </summary>
    public class ApiRemoteException : Exception
    {
        public int Code { private set; get; }
        public JObject Payload { private set; get; }

        public ApiRemoteException(JObject payload, string msg = "Webapi returned an code other than 0") : base(payload["message"] == null ? msg : payload.Value<string>("message"))
        {
            Code = payload.Value<int>("code");
            Payload = payload;
        }

        public ApiRemoteException(string msg, int code, JObject payload) : base(msg)
        {
            Code = code;
            Payload = payload;
        }
    }
}
