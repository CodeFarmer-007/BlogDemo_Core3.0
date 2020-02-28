using BlogDemo.Core.Common.Attribute;
using BlogDemo.Core.Common.MemoryCache;
using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogDemo.Api.AOP
{
    /// <summary>
    /// 面向切面的缓存使用  【缓存拦截器】
    /// </summary>
    public class BlogCacheAOP : CacheAOPbase
    {
        //通过注入的方式，把缓存操作接口通过构造函数注入
        private readonly ICaching _cache;

        public BlogCacheAOP(ICaching cache)
        {
            _cache = cache;
        }

        //实现继承的抽象类  Intercept方法是拦截的关键所在，也是IInterceptor接口中的唯一定义
        public override void Intercept(IInvocation invocation)
        {
            //添加判断条件--有属性、特性的才进行缓存操作
            var method = invocation.MethodInvocationTarget ?? invocation.Method;

            //对当前方法的特性验证
            var qCachingAttribute = method.GetCustomAttributes(true).FirstOrDefault(x => x.GetType() == typeof(CachingAttribute)) as CachingAttribute;

            //只有那些指定的才可以被缓存，需要验证
            if (qCachingAttribute != null)
            {
                //获取自定义缓存键
                var cacheKey = CustomCacheKey(invocation);

                //根据Key获取值
                var cachValue = _cache.Get(cacheKey);

                if (cachValue != null)
                {
                    //将当前获取到的缓存值，赋值给当前执行方法
                    invocation.ReturnValue = cachValue;
                    return;
                }

                //去执行当前方法
                invocation.Proceed();

                //存入缓存
                if (!string.IsNullOrWhiteSpace(cacheKey))
                {
                    _cache.Set(cacheKey, invocation.ReturnValue);
                }
            }

        }
    }
}
