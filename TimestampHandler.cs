using System;

namespace BiliApi
{
    /// <summary>
    /// 时间戳处理工具类
    /// </summary>
    public class TimestampHandler
    {
        public static DateTime GetDateTime(long strLongTime)
        {
            long begtime = strLongTime * 10000000;//100毫微秒为单位,textBox1.text需要转化的int日期
            DateTime dt_1970 = new DateTime(1970, 1, 1, 8, 0, 0);
            long tricks_1970 = dt_1970.Ticks;//1970年1月1日刻度
            long time_tricks = tricks_1970 + begtime;//日志日期刻度
            DateTime dt = new DateTime(time_tricks);//转化为DateTim
            return dt;
        }

        public static DateTime GetDateTime16(long strLongTime)
        {
            return GetDateTime(strLongTime / 1000);
        }

        public static int GetTimeStamp(DateTime dateTime)
        {
#pragma warning disable CS0618 // '“TimeZone”已过时:“System.TimeZone has been deprecated.  Please investigate the use of System.TimeZoneInfo instead.”
            return (int)(dateTime - TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1))).TotalSeconds;
#pragma warning restore CS0618 // '“TimeZone”已过时:“System.TimeZone has been deprecated.  Please investigate the use of System.TimeZoneInfo instead.”
        }

        public static long GetTimeStamp16(DateTime dateTime)
        {
#pragma warning disable CS0618 // '“TimeZone”已过时:“System.TimeZone has been deprecated.  Please investigate the use of System.TimeZoneInfo instead.”
            return (long)(dateTime - TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1))).TotalMilliseconds;
#pragma warning restore CS0618 // '“TimeZone”已过时:“System.TimeZone has been deprecated.  Please investigate the use of System.TimeZoneInfo instead.”
        }
    }
}
