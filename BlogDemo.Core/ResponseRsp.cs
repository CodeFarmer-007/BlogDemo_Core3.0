using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BlogDemo.Core.Model
{
    /// <summary>
    /// 通用返回信息类
    /// </summary>
    public class ResponseRsp<T>
    {
        /// <summary>
        /// 返回状态
        /// </summary>
        public ReturnCode ReurnCode { get; set; }
        /// <summary>
        /// 返回消息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 数据信息
        /// </summary>
        public T Data { get; set; }
    }

    /// <summary>
    /// 返回Code枚举
    /// </summary>
    public enum ReturnCode
    {
        [Description("成功")]
        OK = 0,
        [Description("失败")]
        Fail = 1,
        [Description("异常")]
        Exception = 2,
        [Description("未验证通过")]
        FailedValidation = 3,
        [Description("未登录")]
        NoLogin = 4,
        [Description("无此权限")]
        NoAuthority = 5
    }
}
