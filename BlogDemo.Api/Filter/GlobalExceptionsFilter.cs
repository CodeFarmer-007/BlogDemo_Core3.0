using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Profiling;

namespace BlogDemo.Api.Filter {
    /// <summary>
    /// 全局异常处理日志
    /// </summary>
    public class GlobalExceptionsFilter : IExceptionFilter {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<GlobalExceptionsFilter> _logger;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger (typeof (GlobalExceptionsFilter));

        public GlobalExceptionsFilter (IWebHostEnvironment env, ILogger<GlobalExceptionsFilter> logger) {
            _env = env;
            _logger = logger;
        }

        /// <summary>
        /// 实现 IExceptionFilter
        /// </summary>
        /// <param name="context"></param>
        public void OnException (ExceptionContext context) {
            var json = new JsonErrorResponse ();

            json.Message = context.Exception.Message; //错误消息
            if (_env.IsDevelopment ()) {
                json.DevelopmentMessage = context.Exception.StackTrace; //堆栈信息
            }

            context.Result = new InternalServerErrorObjectResult (json);

            MiniProfiler.Current.CustomTiming ("Errors：", json.Message);

            //采用log4net 进行错误日志记录
            log.Error (WriteLog (json.Message, context.Exception));

        }

        /// <summary>
        /// 自定义返回格式
        /// </summary>
        /// <param name="throwMsg"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        public string WriteLog (string throwMsg, Exception ex) {
            return string.Format ($"【自定义错误】：{throwMsg} \r\n【异常类型】：{ex.GetType().Name} \r\n【异常信息】：{ex.Message} \r\n【堆栈调用】：{ex.StackTrace}");
        }
    }

    public class InternalServerErrorObjectResult : ObjectResult {
        public InternalServerErrorObjectResult (object value) : base (value) {
            StatusCode = StatusCodes.Status500InternalServerError;
        }
    }

    public class JsonErrorResponse {
        /// <summary>
        /// 生产环境的消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 开发环境
        /// </summary>
        public string DevelopmentMessage { get; set; }
    }
}