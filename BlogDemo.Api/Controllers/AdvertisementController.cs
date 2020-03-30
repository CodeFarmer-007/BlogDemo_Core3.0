using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogDemo.Api.SwaggerHelper;
using BlogDemo.Core.IService;
using Castle.Core.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static BlogDemo.Api.SwaggerHelper.CustomApiVersion;

namespace BlogDemo.Api.Controllers
{
    /// <summary>
    /// 测试
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AdvertisementController : ControllerBase
    {
        private readonly IAdvertisementService _advertisementService;

        public AdvertisementController(IAdvertisementService advertisementService)
        {
            _advertisementService = advertisementService;
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        //[Route("GetInfo")]  //路由规则
        [Authorize(Policy = "SystemOrAdmin")]
        [CustomRoute(ApiVersions.v2, "GetInfo")]
        public async Task<bool> GetInfo()
        {
            await Task.Delay(0);

            //AOP捕获不到
            //string ss = "XX";
            //var gg = Convert.ToInt32(ss);

            var state = await _advertisementService.QueryById(2);

            return true;
        }
    }
}