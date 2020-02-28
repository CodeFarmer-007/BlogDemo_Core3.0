using BlogDemo.Core.Common.Attribute;
using BlogDemo.Core.IRepository;
using BlogDemo.Core.IService;
using BlogDemo.Core.Model.Models.WMBlogDB;
using BlogDemo.Core.Service.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlogDemo.Core.Service
{
    public class AdvertisementService : BaseServices<Advertisement>, IAdvertisementService
    {
        private readonly IAdvertisementRepository _dal;

        public AdvertisementService(IAdvertisementRepository dal)
        {
            this.BaseDal = _dal = dal;
        }

        [Caching(AbsoluteExpiration = 10)]  //??它是怎么指定  CachingAttribute 的  ??
        public int SumAandB()
        {
            throw new NotImplementedException();
        }
    }
}
