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
    /// �����ļ����� ������ע�룬��������Redis����ȡ�
    /// </summary>
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public string ApiName { get; set; } = "Blog.Core";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        //ע�����
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region Swagger����

            //��ȡ���򼯸�Ŀ¼ --��� Microsoft.DotNet.PlatformAbstractions Nuget��
            var basePath = Microsoft.DotNet.PlatformAbstractions.ApplicationEnvironment.ApplicationBasePath;


            #endregion

            //ָ�����ݿ������Ľ�ʹ���ڴ������ݿ�
            services.AddDbContext<TodoContext>(opt =>
                opt.UseInMemoryDatabase("TodoList"));  //UseInMemoryDatabase  ���Nuget  Microsoft.EntityFrameworkCore.InMemory

            services.AddControllers();
        }

        //�ܵ����м����������ָ����δ���ÿ��http����
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
