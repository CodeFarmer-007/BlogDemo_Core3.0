using BlogDemo.Core.IService.Base;
using BlogDemo.Core.Model.Models.GllyERP;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BlogDemo.Core.IService
{
    public interface IProductManagementService : IBaseServices<ProductManagement>
    {
        Task<ProductManagement> GetProductAsync();
    }
}
