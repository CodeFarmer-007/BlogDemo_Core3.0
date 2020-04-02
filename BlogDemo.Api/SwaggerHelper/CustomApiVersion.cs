using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogDemo.Api.SwaggerHelper {
    /// <summary>
    /// 自定义Api版本
    /// </summary>
    public class CustomApiVersion {
        public enum ApiVersions {
            /// <summary>
            /// v1 版本
            /// </summary>
            v1 = 1,
            /// <summary>
            /// v2 版本
            /// </summary>
            v2 = 2
        }
    }
}