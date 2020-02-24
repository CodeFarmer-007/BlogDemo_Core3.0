using BlogDemo.Api.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

namespace BlogDemo.Api
{
    /// <summary>
    /// 启动文件配置 【依赖注入，跨域请求，Redis缓存等】
    /// </summary>
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public string ApiName { get; set; } = "Blog.Core";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        //注册服务
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region Swagger配置

            //获取程序集跟目录 --添加 Microsoft.DotNet.PlatformAbstractions Nuget包
            var basePath = Microsoft.DotNet.PlatformAbstractions.ApplicationEnvironment.ApplicationBasePath;


            #endregion

            //指定数据库上下文将使用内存中数据库
            services.AddDbContext<TodoContext>(opt =>
                opt.UseInMemoryDatabase("TodoList"));  //UseInMemoryDatabase  添加Nuget  Microsoft.EntityFrameworkCore.InMemory

            services.AddControllers();
        }

        //管道（中间件）【具体指定如何处理每个http请求】
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
