using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.DynamicProxy;
using BlogDemo.Api.AOP;
using BlogDemo.Api.AuthHelper;
using BlogDemo.Api.Filter;
using BlogDemo.Api.Models;
using BlogDemo.Core.Common.DB;
using BlogDemo.Core.Common.Helper;
using BlogDemo.Core.Common.LogHelper;
using BlogDemo.Core.Common.MemoryCache;
using BlogDemo.Core.Common.Redis;
using BlogDemo.Core.IService;
using BlogDemo.Core.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Profiling.Storage;
using Swashbuckle.AspNetCore.Filters;
using static BlogDemo.Api.SwaggerHelper.CustomApiVersion;
using System.Linq;

namespace BlogDemo.Api {
    /// <summary>
    /// �����ļ����� ������ע�룬��������Redis����ȡ�
    /// </summary>
    public class Startup {
        public IConfiguration Configuration { get; }

        //��ȡAppSettings.json
        public IWebHostEnvironment Env { get; }

        public string ApiName { get; set; } = "Blog.Api";

        public Startup (IConfiguration configuration, IWebHostEnvironment env) {
            Configuration = configuration;

            //��ȡAppSettings.json
            Env = env;
        }

        //ע�����
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices (IServiceCollection services) {
            //��ȡ���򼯸�Ŀ¼ --���� Microsoft.DotNet.PlatformAbstractions Nuget��
            var basePath = Microsoft.DotNet.PlatformAbstractions.ApplicationEnvironment.ApplicationBasePath;

            //��ȡAppSettings.json
            services.AddSingleton (new Appsettings (Env.ContentRootPath));

            #region CROS
            services.AddCors (a => {
                //���ò���    �������� 
                a.AddPolicy ("LimitRequests", policy => {
                    // ֧�ֶ�������˿ڣ�ע��˿ںź�Ҫ��/б�ˣ�����localhost:8000/���Ǵ���
                    // http://127.0.0.1:1818 �� http://localhost:1818 �ǲ�һ���ģ�����д����
                    policy.WithOrigins ("http://localhost:753")
                        .AllowAnyHeader ().AllowAnyMethod ();
                });
            });
            #endregion

            #region ��������� MiniProfiler

            services.AddMiniProfiler (options => {
                options.RouteBasePath = "/profiler"; //ע�����·��Ҫ���±� index.html �ű������е�һ��
                (options.Storage as MemoryCacheStorage).CacheDuration = TimeSpan.FromMinutes (10);
            });

            #endregion

            #region ���������ע��
            // ����ע��
            services.AddScoped<ICaching, MemoryCaching> ();
            services.AddSingleton<IMemoryCache> (factory => {
                var cache = new MemoryCache (new MemoryCacheOptions ());
                return cache;
            });

            // Redisע��
            services.AddScoped<IRedisCacheManager, RedisCacheManager> ();

            #endregion

            #region Swagger����

            //����Swagger
            services.AddSwaggerGen (a => {
                //�汾����
                typeof (ApiVersions).GetEnumNames ().ToList ().ForEach (version => {
                    a.SwaggerDoc (version, new OpenApiInfo {
                        Version = version,
                            Title = $"{ApiName}�ӿ��ĵ�--Core 3.1",
                            Description = $"{ApiName} Http Api {version}",
                            Contact = new OpenApiContact { Name = ApiName, Url = new System.Uri ("https://www.jianshu.com/u/94102b59cc2a"), Email = "229318442@qq.com" },
                            License = new OpenApiLicense { Name = ApiName, Url = new System.Uri ("https://github.com/CodeFarmer-007/BlogDemo_Core3.0") }
                    });
                    a.OrderActionsBy (o => o.RelativePath);
                });

                //��ȡApi-XMLע���ĵ�
                var xmlPath = Path.Combine (basePath, "BlogDemo.Api.xml");
                a.IncludeXmlComments (xmlPath, true);

                //��ȡModel-XMLע���ĵ�
                var xmlModelPath = Path.Combine (basePath, "BlogDemo.Core.Model.xml");
                a.IncludeXmlComments (xmlModelPath, true);

                #region ����JWT

                //������ȨС��
                a.OperationFilter<AddResponseHeadersFilter> ();
                a.OperationFilter<AppendAuthorizeToSummaryOperationFilter> ();

                //��header������token�����ݵ���̨
                a.OperationFilter<SecurityRequirementsOperationFilter> ();

                // ������ oauth2
                a.AddSecurityDefinition ("oauth2", new OpenApiSecurityScheme {
                    Description = "JWT��Ȩ(���ݽ�������ͷ�н��д���) ֱ�����¿������� Bearer {token} ��ע������֮����һ���ո�",
                        Name = "Authorization", //JWTĬ�ϵĲ�������
                        In = ParameterLocation.Header, //jwtĬ�ϴ�� Authorization ��Ϣλ�ã�����ͷ�У�
                        Type = SecuritySchemeType.ApiKey
                });
                #endregion

            });

            #endregion

            #region GlobalExceptions

            //ע��ȫ���쳣����
            services.AddControllers (a => {
                a.Filters.Add (typeof (GlobalExceptionsFilter));
            });

            #endregion

            #region JWT�ٷ���Ȩ��֤

            #region ����һ������API�ӿ���Ȩ����
            //1.1 ����
            services.AddAuthorization (option => {
                option.AddPolicy ("Client", policy => policy.RequireRole ("Client").Build ());
                option.AddPolicy ("Admin", policy => policy.RequireRole ("Admin").Build ());
                option.AddPolicy ("SystemOrAdmin", policy => policy.RequireRole ("System", "Admin"));
                option.AddPolicy ("SystemAndAdmin", policy => policy.RequireRole ("IncludeXmlCommentsSystem").RequireRole ("Admin"));
            });

            #endregion

            #region ����
            var audienceConfig = Configuration.GetSection ("Audience"); //��ȡAppSettings���ýڵ�
            var symmetricKeyAsBase64 = AppSecretConfig.Audience_Secret_String; //˽Կ
            var keyByteArray = Encoding.ASCII.GetBytes (symmetricKeyAsBase64);
            var signingKey = new SymmetricSecurityKey (keyByteArray);
            #endregion

            #region ���ڶ�������������֤����

            //����������֤����
            var tokenValidationParameters = new TokenValidationParameters {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateIssuer = true,
                ValidIssuer = audienceConfig["Issuer"], //������
                ValidateAudience = true,
                ValidAudience = audienceConfig["Audience"], //������
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds (30),
                RequireExpirationTime = true,
            };

            //2.1 ����֤�� Core�Դ��ٷ�JWT��֤
            //��ʼBearer������֤
            services.AddAuthentication ("Bearer")
                //����Bearer����
                .AddJwtBearer (a => {
                    a.TokenValidationParameters = tokenValidationParameters;
                    a.Events = new JwtBearerEvents {
                        OnAuthenticationFailed = context => {
                            //������ڣ����<�Ƿ����>���ӵ�������ͷ��Ϣ��    [��ȫ���ƹ����쳣]
                            if (context.Exception.GetType () == typeof (SecurityTokenExpiredException)) {
                                context.Response.Headers.Add ("Token-Expired", "true");
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            #endregion

            #endregion

            //ָ�����ݿ������Ľ�ʹ���ڴ������ݿ�
            //services.AddDbContext<TodoContext>(opt =>
            //    opt.UseInMemoryDatabase("TodoList"));  //UseInMemoryDatabase  ����Nuget  Microsoft.EntityFrameworkCore.InMemory

            services.AddControllers ();
        }

        /// <summary>
        ///  �������� ע�뵽Autofac ����
        /// </summary>
        /// <param name="builder"></param>
        public void ConfigureContainer (ContainerBuilder builder) {
            //��ȡ���򼯸�Ŀ¼ --���� Microsoft.DotNet.PlatformAbstractions Nuget��
            var basePath = Microsoft.DotNet.PlatformAbstractions.ApplicationEnvironment.ApplicationBasePath;

            #region AOP

            //ע��Ҫͨ�����䴴�������
            builder.RegisterType<BlogLogAOP> (); //����������ע��
            builder.RegisterType<BlogCacheAOP> ();

            #endregion

            #region ���нӿڲ�ķ���ע��

            var servicesDllFile = Path.Combine (basePath, "BlogDemo.Core.Service.dll");

            var assemblysServices = Assembly.LoadFile (servicesDllFile);

            builder.RegisterAssemblyTypes (assemblysServices)
                .AsImplementedInterfaces ()
                .InstancePerLifetimeScope ()
                .EnableInterfaceInterceptors ()
                .InterceptedBy (typeof (BlogLogAOP), typeof (BlogCacheAOP)); //���Է�һ��AOP����������

            var repositoryDllFile = Path.Combine (basePath, "BlogDemo.Core.Repository.dll");
            var assemblysRepository = Assembly.LoadFrom (repositoryDllFile);
            builder.RegisterAssemblyTypes (assemblysRepository).AsImplementedInterfaces ();

            #endregion

        }

        //�ܵ����м����������ָ����δ���ÿ��http����
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure (IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment ()) {
                app.UseDeveloperExceptionPage ();
            }

            #region Swagger

            app.UseSwagger ();

            app.UseSwaggerUI (c => {
                typeof (ApiVersions).GetEnumNames ().OrderByDescending (e => e).ToList ().ForEach (versions => {
                    c.SwaggerEndpoint ($"/swagger/{versions}/swagger.json", $"{ApiName}  {versions}");
                });

                c.RoutePrefix = "";
                c.IndexStream = () => GetType ().GetTypeInfo ().Assembly.GetManifestResourceStream ("BlogDemo.Api.index.html");
            });

            #endregion

            //�Զ�����֤�м��
            //app.UseMiddleware<JwtTokenAuth>();

            app.UseMiniProfiler ();

            app.UseHttpsRedirection ();

            app.UseStaticFiles (); //��ȡ��̬�ļ� wwwroot

            app.UseRouting ();

            //�����ʹ���� app.UserMvc() ���� app.UseHttpsRedirection()������м����һ��Ҫ�� app.UseCors() д�����ǵ��ϱߣ��Ƚ��п����ٽ��� Http ���󣬷������ʾ����ʧ�ܡ�
            //��Ϊ�����������漰�� Http����ģ�����㲻�����ֱ��ת������mvc���ǿ϶�����
            app.UseCors ("LimitRequests");

            #region �����������������м��
            //������֤  �������ʹ�ùٷ���֤���������ϱ�ConfigureService �У�����JWT����֤���� (.AddAuthentication �� .AddJwtBearer ����ȱһ����)
            app.UseAuthentication ();
            #endregion

            //��Ȩ
            app.UseAuthorization ();

            app.UseEndpoints (endpoints => {
                endpoints.MapControllers ();
            });
        }
    }
}