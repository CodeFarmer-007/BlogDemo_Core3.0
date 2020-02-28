using BlogDemo.Core.IService.Base;
using BlogDemo.Core.Model.Models.WMBlogDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlogDemo.Core.IService
{
    public interface IAdvertisementService : IBaseServices<Advertisement>
    {
        int SumAandB();
    }
}
