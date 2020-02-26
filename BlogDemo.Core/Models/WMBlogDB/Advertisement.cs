using System;
using System.Collections.Generic;
using System.Text;

namespace BlogDemo.Core.Model.Models.WMBlogDB
{
    public class Advertisement
    {
        public int Id { get; set; }
        public string ImgUrl { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Remark { get; set; }
        public DateTime Createdate { get; set; }
    }
}
