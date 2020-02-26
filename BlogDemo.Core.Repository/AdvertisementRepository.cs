using BlogDemo.Core.IRepository;
using BlogDemo.Core.Model.Models.WMBlogDB;
using BlogDemo.Core.Repository.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlogDemo.Core.Repository
{
    public class AdvertisementRepository : BaseRepository<Advertisement>, IAdvertisementRepository
    {

    }
}
