using System;

namespace BiliApi
{
    /// <summary>
    /// Bilibili被封禁用户对象
    /// </summary>
    public class BiliBannedUser
    {
        public long uid, op, id;
        public int len;
        public string uname, opname;
        public DateTime optime, endtime;
        public BanReason banreason;
    }
    /// <summary>
    /// 被封禁原因
    /// </summary>
    public struct BanReason
    {
        public string message;
        public DateTime messagetime;
    }
}
