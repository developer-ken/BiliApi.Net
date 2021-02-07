using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using BiliApi.Exceptions;
using System.Threading;
using System.Drawing;

namespace BiliApi.Auth
{
    /// <summary>
    /// Bilibili扫码登陆工具类
    /// </summary>
    public class QRLogin : IAuthBase
    {
        const string URL_GETKEY = "https://passport.bilibili.com/qrcode/getLoginUrl";
        const string URL_STATUS = "https://passport.bilibili.com/qrcode/getLoginInfo";

        public CookieCollection Cookies { get; private set; }
        public LoginQRCode QRToken { private set; get; }
        public bool LoggedIn { get; private set; }

        public struct LoginQRCode
        {
            public string ScanUrl, OAuthKey;
        }

        public enum QRState
        {
            /// <summary>
            /// 等待扫描
            /// </summary>
            WaitingForScan = -4,
            /// <summary>
            /// 已扫描，等待确认登录
            /// </summary>
            WaitingForAccept = -5,
            /// <summary>
            /// 已完成登录
            /// </summary>
            LoggedIn = 0,
            /// <summary>
            /// 超时失效或无效的Key
            /// </summary>
            Expired = -2
        }

        public QRLogin()
        {
            QRToken = GetNewQRItem();
            LoggedIn = false;
        }

        public QRLogin(LoginQRCode code)
        {
            QRToken = code;
            LoggedIn = false;
        }

        /// <summary>
        /// 等待用户扫码并在完成后返回登录Cookie
        /// </summary>
        /// <exception cref="AuthenticateFailedException">二维码失效</exception>
        /// <returns>保持登录状态所需的Cookie</returns>
        public CookieCollection Login()
        {
            while (true)
            {
                var stat = GetQRState(QRToken);
                if (stat == QRState.LoggedIn)
                {
                    break;
                }
                if (stat == QRState.Expired)
                {
                    throw new AuthenticateFailedException(new JObject(), "QRCode expired");
                }
                Thread.Sleep(1500);
            }
            return GetLoginCookies();
        }

        /// <summary>
        /// 返回值包括生成二维码和后续登录所需的全部信息。
        /// </summary>
        /// <returns>OAuthKey和登录链接</returns>
        public LoginQRCode GetNewQRItem()
        {
            JObject jb = (JObject)JsonConvert.DeserializeObject(ThirdPartAPIs._get(URL_GETKEY));
            if (jb.Value<int>("code") != 0)
            {
                throw new ApiRemoteException(jb);
            }
            return new LoginQRCode
            {
                ScanUrl = jb["data"].Value<string>("url"),
                OAuthKey = jb["data"].Value<string>("oauthKey")
            };
        }

        /// <summary>
        /// 获取二维码状态
        /// </summary>
        /// <returns>二维码状态</returns>
        public QRState GetQRState()
        {
            return GetQRState(QRToken);
        }

        /// <summary>
        /// 获取二维码状态
        /// </summary>
        /// <param name="qr">二维码信息</param>
        /// <returns>二维码状态</returns>
        public QRState GetQRState(LoginQRCode qr)
        {
            var res = ThirdPartAPIs._post_cookies(URL_STATUS, new Dictionary<string, string>()
            {
                {"oauthKey",qr.OAuthKey},
                {"gourl","https://www.bilibili.com/"}
            });
            JObject jb = (JObject)JsonConvert.DeserializeObject(res.Result);
            try
            {
                switch (jb.Value<int>("data"))
                {
                    case -4:
                        return QRState.WaitingForScan;
                    case -5:
                        return QRState.WaitingForAccept;
                    case -2:
                    case -1:
                        return QRState.Expired;
                    default:
                        throw new UnexpectedResultException(jb.ToString(), "Unexpected QRCode state");
                }
            }
            catch (UnexpectedResultException e)
            {
                throw;
            }
            catch (Exception)
            {
                if (jb.Value<bool>("status"))
                {

                    Cookies = res.Cookies;
                    LoggedIn = IsOnline();
                    if (!LoggedIn) throw new AuthenticateFailedException(jb);
                    return QRState.LoggedIn;
                }
                else
                {
                    throw new UnexpectedResultException(jb.ToString());
                }
            }
        }

        public bool IsOnline()
        {
            string str = ThirdPartAPIs._get_with_cookies("https://api.bilibili.com/x/web-interface/nav", Cookies);
            JObject jb = (JObject)JsonConvert.DeserializeObject(str);
            return (jb.Value<int>("code") == 0);
        }

        public CookieCollection GetLoginCookies()
        {
            if (!LoggedIn) throw new AuthenticateFailedException(new JObject());
            return Cookies;
        }
    }
}
