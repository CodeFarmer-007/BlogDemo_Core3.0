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

namespace BlogDemo.Api
{
    /// <summary>
    /// �����ļ����� ������ע�룬��������Redis����ȡ�
    /// </summary>
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public string ApiName { get; set; } = "Blog.CoreDemo";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        //ע�����
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //��ȡ���򼯸�Ŀ¼ --��� Microsoft.DotNet.PlatformAbstractions Nuget��
            var basePath = Microsoft.DotNet.PlatformAbstractions.ApplicationEnvironment.ApplicationBasePath;

            #region ���������ע��
            // ����ע��
            services.AddScoped<ICaching, MemoryCaching>();
            services.AddSingleton<IMemoryCache>(factory =>
            {
                var cache = new MemoryCache(new MemoryCacheOptions());
                return cache;
            });

            // Redisע��
            services.AddScoped<IRedisCacheManager, RedisCacheManager>();

            #endregion

            #region Swagger����

            //����Swagger
            services.AddSwaggerGen(a =>
            {

                a.SwaggerDoc("V1", new OpenApiInfo
                {
                    Version = "V1",
                    Title = $"{ApiName}�ӿ��ĵ�--Core 3.0",
                    Description = $"{ApiName} Http Api V1",
                    Contact = new OpenApiContact { Name = ApiName, Url = new System.Uri("https://www.jianshu.com/u/94102b59cc2a"), Email = "229318442@qq.com" },
                    License = new OpenApiLicense { Name = ApiName, Url = new System.Uri("https://github.com/CodeFarmer-007/BlogDemo_Core3.0") }
                });

                a.OrderActionsBy(c => c.RelativePath);

                //��ȡApi-XMLע���ĵ�
                var xmlPath = Path.Combine(basePath, "BlogDemo.Api.xml");
                a.IncludeXmlComments(xmlPath, true);

                //��ȡModel-XMLע���ĵ�
                var xmlModelPath = Path.Combine(basePath, "BlogDemo.Core.Model.xml");
                a.IncludeXmlComments(xmlModelPath, true);
            });


            #endregion

            //ָ�����ݿ������Ľ�ʹ���ڴ������ݿ�
            //services.AddDbContext<TodoContext>(opt =>
            //    opt.UseInMemoryDatabase("TodoList"));  //UseInMemoryDatabase  ���Nuget  Microsoft.EntityFrameworkCore.InMemory

            services.AddControllers();
        }

        /// <summary>
        ///  �������� ע�뵽Autofac ����
        /// </summary>
        /// <param name="builder"></param>
        public void ConfigureContainer(ContainerBuilder builder)
        {
            //��ȡ���򼯸�Ŀ¼ --��� Microsoft.DotNet.PlatformAbstractions Nuget��
            var basePath = Microsoft.DotNet.PlatformAbstractions.ApplicationEnvironment.ApplicationBasePath;

            #region AOP

            //ע��Ҫͨ�����䴴�������
            builder.RegisterType<BlogLogAOP>(); //����������ע��
            builder.RegisterType<BlogCacheAOP>();

            #endregion

            #region ���нӿڲ�ķ���ע��

            var servicesDllFile = Path.Combine(basePath, "BlogDemo.Core.Service.dll");

            var assemblysServices = Assembly.LoadFile(servicesDllFile);

            builder.RegisterAssemblyTypes(assemblysServices)
               .AsImplementedInterfaces()
               .InstancePerLifetimeScope()
               .EnableInterfaceInterceptors()
               .InterceptedBy(typeof(BlogLogAOP), typeof(BlogCacheAOP)); //���Է�һ��AOP����������

            var repositoryDllFile = Path.Combine(basePath, "BlogDemo.Core.Repository.dll");
            var assemblysRepository = Assembly.LoadFrom(repositoryDllFile);
            builder.RegisterAssemblyTypes(assemblysRepository).AsImplementedInterfaces();

            #endregion

        }

        //�ܵ����м����������ָ����δ���ÿ��http����
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
