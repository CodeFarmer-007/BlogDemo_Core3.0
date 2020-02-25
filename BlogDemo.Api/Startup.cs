using BlogDemo.Api.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.IO;
using BlogDemo.Core.Common.DB;

namespace BlogDemo.Api
{
    /// <summary>
    /// 启动文件配置 【依赖注入，跨域请求，Redis缓存等】
    /// </summary>
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public string ApiName { get; set; } = "Blog.CoreDemo";

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

            //配置Swagger
            services.AddSwaggerGen(a =>
            {

                a.SwaggerDoc("V1", new OpenApiInfo
                {
                    Version = "V1",
                    Title = $"{ApiName}接口文档--Core 3.0",
                    Description = $"{ApiName} Http Api V1",
                    Contact = new OpenApiContact { Name = ApiName, Url = new System.Uri("https://www.jianshu.com/u/94102b59cc2a"), Email = "229318442@qq.com" },
                    License = new OpenApiLicense { Name = ApiName, Url = new System.Uri("https://github.com/CodeFarmer-007/BlogDemo_Core3.0") }
                });

                a.OrderActionsBy(c => c.RelativePath);

                //读取Api-XML注释文档
                var xmlPath = Path.Combine(basePath, "BlogDemo.Api.xml");
                a.IncludeXmlComments(xmlPath, true);

                //读取Model-XML注释文档
                var xmlModelPath = Path.Combine(basePath, "BlogDemo.Core.Model.xml");
                a.IncludeXmlComments(xmlModelPath, true);
            });


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

            #region Swagger

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/V1/swagger.json", $"{ApiName}  V1");

                c.RoutePrefix = "";
            });

            #endregion

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
