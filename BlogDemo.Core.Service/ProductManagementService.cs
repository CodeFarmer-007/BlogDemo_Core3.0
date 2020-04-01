using BlogDemo.Core.IRepository;
using BlogDemo.Core.IService;
using BlogDemo.Core.Model.Models.GllyERP;
using BlogDemo.Core.Repository.Sugar;
using BlogDemo.Core.Service.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BlogDemo.Core.Service
{
    public class ProductManagementService : BaseServices<ProductManagement>, IProductManagementService
    {
        private readonly IProductManagementRepository _dal;

        public ProductManagementService(IProductManagementRepository dal)
        {
            _dal = dal;
            base.BaseDal = dal;
        }

        public Task<ProductManagement> GetProductAsync()
        {
            
        }
    }
}
