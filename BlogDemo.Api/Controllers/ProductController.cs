using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogDemo.Core.IService;
using BlogDemo.Core.Model;
using BlogDemo.Core.Model.Models.GllyERP;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlogDemo.Api.Controllers {
    [Route ("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase {
        private readonly IProductManagementService _productManagementService;

        public ProductController (IProductManagementService productManagementService) {
            _productManagementService = productManagementService;
        }

        [HttpGet ("{key}", Name = nameof (GetProductAsync))]
        public async Task<ResponseRsp<List<ProductManagement>>> GetProductAsync (string key = "") {
            await Task.Delay (0);

            var status = await _productManagementService.Query (a => a.ProductName.Contains (key));

            return new ResponseRsp<List<ProductManagement>> { ReurnCode = ReturnCode.OK, Data = status, Message = "操作成功" };
        }

    }
}