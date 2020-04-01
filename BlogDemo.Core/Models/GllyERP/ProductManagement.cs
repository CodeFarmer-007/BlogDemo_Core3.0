using System;
using System.Collections.Generic;
using System.Text;

namespace BlogDemo.Core.Model.Models.GllyERP
{
    ///<summary>
    ///产品管理
    ///</summary>
    public class ProductManagement
    {
        /// <summary>
        /// productID
        /// </summary>
        public long ProductID { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// productTypeParentId
        /// </summary>
        public string ProductTypeParentId { get; set; }

        /// <summary>
        /// 产品分类
        /// </summary>
        public string ProductType { get; set; }

        /// <summary>
        /// productTypeName
        /// </summary>
        public string ProductTypeName { get; set; }

        /// <summary>
        /// number
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// 数量描述
        /// </summary>
        public string QuantitativeDescription { get; set; }

        /// <summary>
        /// 产品售价
        /// </summary>
        public decimal ProductPrice { get; set; }

        /// <summary>
        /// 产品描述
        /// </summary>
        public string ProductDescription { get; set; }

        /// <summary>
        /// 产品图片
        /// </summary>
        public string ProductPicture { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public System.DateTime StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public System.DateTime EndTime { get; set; }

        /// <summary>
        /// 等级
        /// </summary>
        public string Grade { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// 0存在1删除
        /// </summary>
        public bool IsDelete { get; set; }

        /// <summary>
        /// 0有效1无效
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        public string CreateUserID { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public System.DateTime CreateTime { get; set; }

        /// <summary>
        /// 修改人ID
        /// </summary>
        public string UpdateUserID { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public string UpdateUser { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public System.DateTime UpdateTime { get; set; }

        /// <summary>
        /// 变更限制天数
        /// </summary>
        public int ChangeLimitDays { get; set; }

        /// <summary>
        /// 地区
        /// </summary>
        public string Region { get; set; }

    }
}
