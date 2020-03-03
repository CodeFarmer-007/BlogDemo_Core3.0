using BlogDemo.Core.IService.Base;
using BlogDemo.Core.Model.Models.WMBlogDB;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BlogDemo.Core.IService
{
    public interface IAdvertisementService : IBaseServices<Advertisement>
    {
        /// <summary>
        /// ReSharper插件
        /// </summary>
        /// <returns></returns>
        Task<int> SumAndB();
    }
}
