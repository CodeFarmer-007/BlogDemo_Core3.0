using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogDemo.Api.AuthHelper;
using BlogDemo.Core.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BlogDemo.Api.Controllers {
    [Route ("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase {
        public LoginController () {

        }

        /// <summary>
        /// 获取Token令牌
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userpass"></param>
        /// <returns></returns>
        [HttpGet]
        [Route ("GetJwtToken")]
        public async Task<object> GetJwtToken (string userId, string userpass) {
            await Task.Delay (0);

            string jwtStr = string.Empty;
            bool suc = false;

            var user = "abc";
            if (!string.IsNullOrWhiteSpace (user)) {
                TokenModelJwt jwtModel = new TokenModelJwt { Uid = 1, Role = "Admin" };
                jwtStr = JwtHelper.IssueJwt (jwtModel);
                suc = true;
            } else {
                jwtStr = "Not Login!!!";
            }

            return Ok (new {
                success = suc,
                    token = jwtStr
            });
        }
    }
}