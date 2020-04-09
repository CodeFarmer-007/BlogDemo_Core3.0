using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BlogDemo.Core.Common.LogHelper;
using Castle.DynamicProxy;
using StackExchange.Profiling;

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
            var dataIntercept = "";

            //事前处理: 在服务方法执行之前,做相应的逻辑处理
            dataIntercept += $"【请求开始时间】：{DateTime.Now} \r\n";
            dataIntercept += ($"【当前执行方法】：{invocation.Method.Name} \r\n");
            dataIntercept += ($"【携带的参数有】：{string.Join(", ", invocation.Arguments.Select(a => (a ?? "").ToString()).ToArray())} \r\n");

            try
            {
                MiniProfiler.Current.Step($"执行Service方法：{invocation.Method.Name}() -> ");

                //执行当前访问的服务方法,(注意:如果下边还有其他的AOP拦截器的话,会跳转到其他的AOP里)
                invocation.Proceed();

                // 异步获取异常，先执行
                if (IsAsyncMethod(invocation.Method))
                {
                    //Wait task execution and modify return value
                    if (invocation.Method.ReturnType == typeof(Task))
                    {
                        invocation.ReturnValue = InternalAsyncHelper.AwaitTaskWithPostActionAndFinally((Task)invocation.ReturnValue, async () => await TestActionAsync(invocation), ex =>
                        {
                            LogEx(ex, ref dataIntercept);
                        });
                    }
                    else //Task<TResult>
                    {
                        invocation.ReturnValue = InternalAsyncHelper.CallAwaitTaskWithPostActionAndFinallyAndGetResult(
                            invocation.Method.ReturnType.GenericTypeArguments[0],
                            invocation.ReturnValue,
                            async () => await TestActionAsync(invocation),
                            ex =>
                            {
                                LogEx(ex, ref dataIntercept);
                            });
                    }
                }
                else
                { //同步1

                }
            }
            catch (Exception ex) //同步2
            {
                LogEx(ex, ref dataIntercept);
            }

            //事后处理: 在service被执行了以后,做相应的处理,这里是输出到日志文件
            dataIntercept += $"【执行完成结果】：{invocation.ReturnValue} \r\n";
            dataIntercept += $"【请求结束时间】：{DateTime.Now} \r\n";

            //输出到日志文件
            Parallel.For(0, 1, a =>
            {
                LogLock.OutSql2Log("AOPLog", new string[] { dataIntercept.ToString() });
            });

        }

        /// <summary>
        /// 判断是否是异步方法
        /// </summary>
        /// <returns></returns>
        public static bool IsAsyncMethod(MethodInfo method)
        {
            return (method.ReflectedType == typeof(Task) || (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)));
        }

        private async Task TestActionAsync(IInvocation invocation)
        {
            //Console.WriteLine("Waiting after method execution for " + invocation.MethodInvocationTarget.Name);
            await Task.Delay(20); // 仅作测试
            //Console.WriteLine("Waited after method execution for " + invocation.MethodInvocationTarget.Name);
        }

        private void LogEx(Exception ex, ref string dataIntercept)
        {
            if (ex != null)
            {
                //执行的 service 中，收录异常
                MiniProfiler.Current.CustomTiming("Errors", ex.Message);
                //执行的 service 中，收录异常
                dataIntercept += ($"【方法执行中出现异常】：{ex.Message + ex.InnerException + ex.StackTrace}\r\n");
            }
        }

    }

    internal static class InternalAsyncHelper
    {
        public static async Task AwaitTaskWithPostActionAndFinally(Task actualReturnValue, Func<Task> postAction, Action<Exception> finalAction)
        {
            Exception exception = null;

            try
            {
                await actualReturnValue;
                await postAction();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                finalAction(exception);
            }
        }

        public static async Task<T> AwaitTaskWithPostActionAndFinallyAndGetResult<T>(Task<T> actualReturnValue, Func<Task> postAction, Action<Exception> finalAction)
        {
            Exception exception = null;

            try
            {
                var result = await actualReturnValue;
                await postAction();
                return result;
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                finalAction(exception);
            }
        }

        public static object CallAwaitTaskWithPostActionAndFinallyAndGetResult(Type taskReturnType, object actualReturnValue, Func<Task> action, Action<Exception> finalAction)
        {
            return typeof(InternalAsyncHelper)
                .GetMethod("AwaitTaskWithPostActionAndFinallyAndGetResult", BindingFlags.Public | BindingFlags.Static)
                .MakeGenericMethod(taskReturnType)
                .Invoke(null, new object[] { actualReturnValue, action, finalAction });
        }
    }
}