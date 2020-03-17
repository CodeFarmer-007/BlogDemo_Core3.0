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
using System.Reflection;
using Autofac;
using Autofac.Extras.DynamicProxy;
using BlogDemo.Core.Service;
using BlogDemo.Core.IService;
using BlogDemo.Api.AOP;
using BlogDemo.Core.Common.MemoryCache;
using Microsoft.Extensions.Caching.Memory;
using BlogDemo.Core.Common.Redis;
using StackExchange.Profiling.Storage;
using System;

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
            //获取程序集跟目录 --添加 Microsoft.DotNet.PlatformAbstractions Nuget包
            var basePath = Microsoft.DotNet.PlatformAbstractions.ApplicationEnvironment.ApplicationBasePath;

            #region CROS
            services.AddCors(a =>
            {
                //配置策略    限制请求 
                a.AddPolicy("LimitRequests", policy =>
                {
                    // 支持多个域名端口，注意端口号后不要带/斜杆：比如localhost:8000/，是错的
                    // http://127.0.0.1:1818 和 http://localhost:1818 是不一样的，尽量写两个
                    policy.WithOrigins("http://localhost:753")
                    .AllowAnyHeader().AllowAnyMethod();
                });
            });
            #endregion

            #region 代码分析器

            services.AddMiniProfiler(options =>
            {
                options.RouteBasePath = "/profiler";   //注意这个路径要和下边 index.html 脚本配置中的一致
                (options.Storage as MemoryCacheStorage).CacheDuration = TimeSpan.FromMinutes(10);
            });

            #endregion

            #region 工具类服务注入
            // 缓存注入
            services.AddScoped<ICaching, MemoryCaching>();
            services.AddSingleton<IMemoryCache>(factory =>
            {
                var cache = new MemoryCache(new MemoryCacheOptions());
                return cache;
            });

            // Redis注入
            services.AddScoped<IRedisCacheManager, RedisCacheManager>();

            #endregion

            #region Swagger配置

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
            //services.AddDbContext<TodoContext>(opt =>
            //    opt.UseInMemoryDatabase("TodoList"));  //UseInMemoryDatabase  添加Nuget  Microsoft.EntityFrameworkCore.InMemory

            services.AddControllers();
        }

        /// <summary>
        ///  新增服务 注入到Autofac 容器
        /// </summary>
        /// <param name="builder"></param>
        public void ConfigureContainer(ContainerBuilder builder)
        {
            //获取程序集跟目录 --添加 Microsoft.DotNet.PlatformAbstractions Nuget包
            var basePath = Microsoft.DotNet.PlatformAbstractions.ApplicationEnvironment.ApplicationBasePath;

            #region AOP

            //注册要通过反射创建的组件
            builder.RegisterType<BlogLogAOP>(); //拦截器进行注册
            builder.RegisterType<BlogCacheAOP>();

            #endregion

            #region 带有接口层的服务注入

            var servicesDllFile = Path.Combine(basePath, "BlogDemo.Core.Service.dll");

            var assemblysServices = Assembly.LoadFile(servicesDllFile);

            builder.RegisterAssemblyTypes(assemblysServices)
               .AsImplementedInterfaces()
               .InstancePerLifetimeScope()
               .EnableInterfaceInterceptors()
               .InterceptedBy(typeof(BlogLogAOP), typeof(BlogCacheAOP)); //可以放一个AOP拦截器集合

            var repositoryDllFile = Path.Combine(basePath, "BlogDemo.Core.Repository.dll");
            var assemblysRepository = Assembly.LoadFrom(repositoryDllFile);
            builder.RegisterAssemblyTypes(assemblysRepository).AsImplementedInterfaces();

            #endregion

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
                c.IndexStream = () => GetType().GetTypeInfo().Assembly.GetManifestResourceStream("BlogDemo.Api.index.html");
            });

            #endregion


            app.UseMiniProfiler();


            //app.UseHttpsRedirection();

            app.UseStaticFiles(); //读取静态文件 wwwroot

            app.UseRouting();

            //如果你使用了 app.UserMvc() 或者 app.UseHttpsRedirection()这类的中间件，一定要把 app.UseCors() 写在它们的上边，先进行跨域，再进行 Http 请求，否则会提示跨域失败。
            //因为这两个都是涉及到 Http请求的，如果你不跨域就直接转发或者mvc，那肯定报错
            app.UseCors("LimitRequests");

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
