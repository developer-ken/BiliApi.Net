using System;
using System.Collections.Generic;
using System.Text;

namespace BiliApi.Exceptions
{
    /// <summary>
    /// 意外的返回值
    /// <para>当API返回值不可解析或超出设计范围时发生这个错误</para>
    /// </summary>
    public class UnexpectedResultException : Exception
    {
        public string ApiResult { get; private set; }
        public UnexpectedResultException(string result, string message = "The result of the webapi is unexpected.") : base(message)
        {
            ApiResult = result;
        }

        public UnexpectedResultException(string result, Exception inner_exception, string message = "The result of the webapi is unexpected.") : base(message, inner_exception)
        {
            ApiResult = result;
        }
    }
}
