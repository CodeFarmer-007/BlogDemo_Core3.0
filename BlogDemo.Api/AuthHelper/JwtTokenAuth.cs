using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlogDemo.Api.AuthHelper
{
    public class JwtTokenAuth
    {
        //中间件一定要有一个next，将管道可以正常走下去
        private readonly RequestDelegate _next;

        public JwtTokenAuth(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext httpContext)
        {
            if (!httpContext.Request.Headers.ContainsKey("Authorization"))
            {
                return _next(httpContext);
            }

            var tokenHeader = httpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            try
            {
                if (tokenHeader.Length >= 128)
                {
                    TokenModelJwt tm = JwtHelper.SerializeJwt(tokenHeader);

                    //授权 Claim 关键
                    var claimList = new List<Claim>();
                    var claim = new Claim(ClaimTypes.Role, tm.Role);
                    claimList.Add(claim);

                    var identity = new ClaimsIdentity(claimList);
                    var principal = new ClaimsPrincipal(identity);
                    httpContext.User = principal;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now} middleware wrong:{ex.Message}");
            }

            return _next(httpContext);
        }
    }

    //这里定义一个中间件Helper，主要作用就是给当前模块的中间件取一个别名
    //public static class MiddlewareHelpers
    //{
    //    public static IApplicationBuilder UseJwtTokenAuth(this IApplicationBuilder app)
    //    {
    //        return app.UseMiddleware<JwtTokenAuth>();
    //    }
    //}
}
