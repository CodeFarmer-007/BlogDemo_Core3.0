using BlogDemo.Core.Common.LogHelper;
using Castle.DynamicProxy;
using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogDemo.Api.AOP
{
    /// <summary>
    /// 拦截器 BlogLogAOP 继承 IInterceptor接口
    /// </summary>
    public class BlogLogAOP : IInterceptor
    {
        /// <summary>
        /// 实例化 IInerceptor 唯一方式
        /// </summary>
        /// <param name="invocation">包含拦截方法的信息</param>
        public void Intercept(IInvocation invocation)
        {
            StringBuilder dataIntercept = new StringBuilder();

            //事前处理: 在服务方法执行之前,做相应的逻辑处理
            dataIntercept.Append($"【请求开始时间】：{DateTime.Now} \r\n");
            dataIntercept.Append($"【当前执行方法】：{invocation.Method.Name} \r\n");
            dataIntercept.Append($"【携带的参数有】：{string.Join(", ", invocation.Arguments.Select(a => (a ?? "").ToString()).ToArray())} \r\n");

            try
            {
                MiniProfiler.Current.Step($"执行Service方法：{invocation.Method.Name}() -> ");

                //执行当前访问的服务方法,(注意:如果下边还有其他的AOP拦截器的话,会跳转到其他的AOP里)
                invocation.Proceed();
            }
            catch (Exception ex)
            {
                dataIntercept.Append($"【方法执行中出现异常】：{ex.Message + ex.InnerException} \r\n");
            }

            //事后处理: 在service被执行了以后,做相应的处理,这里是输出到日志文件
            dataIntercept.Append($"【执行完成结果】：{invocation.ReturnValue} \r\n");
            dataIntercept.Append($"【请求结束时间】：{DateTime.Now} \r\n");

            //输出到日志文件
            Parallel.For(0, 1, a =>
            {
                LogLock.OutSql2Log("AOPLog", new string[] { dataIntercept.ToString() });
            });

        }
    }
}
