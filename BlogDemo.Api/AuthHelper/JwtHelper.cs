using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BlogDemo.Core.Common.DB;
using BlogDemo.Core.Common.Helper;
using Microsoft.IdentityModel.Tokens;

namespace BlogDemo.Api.AuthHelper {
    /// <summary>
    /// JWT由 头【指定JWT字符串的加密规则】、PayLoad【报文、显示已注册声明】、加密签名 三部分组成
    /// </summary>
    public class JwtHelper {
        public static string IssueJwt (TokenModelJwt tokenModelJwt) {
            string iss = Appsettings.app (new string[] { "Audience", "Issuer" }); //颁发人
            string aud = Appsettings.app (new string[] { "Audience", "Audience" }); //Audience 受众

            string secret = AppSecretConfig.Audience_Secret_String; //读取签名规则

            var claims = new List<Claim> {
                /*
                * * 特别重要：
                1、这里将用户的部分信息，比如 uid 存到了Claim 中，如果你想知道如何在其他地方将这个 uid从 Token 中取出来，请看下边的SerializeJwt() 方法，或者在整个解决方案，搜索这个方法，看哪里使用了！
                2、你也可以研究下 HttpContext.User.Claims ，具体的你可以看看 Policys/PermissionHandler.cs 类中是如何使用的。
                */

                new Claim (JwtRegisteredClaimNames.Jti, tokenModelJwt.Uid.ToString ()),
                new Claim (JwtRegisteredClaimNames.Iat, $"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}"),
                new Claim (JwtRegisteredClaimNames.Nbf, $"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}"),
                //这个就是过期时间，目前是过期1000秒，可自定义，注意JWT有自己的缓冲过期时间
                new Claim (JwtRegisteredClaimNames.Exp, $"{new DateTimeOffset(DateTime.Now.AddSeconds(1000)).ToUnixTimeSeconds()}"),
                new Claim (ClaimTypes.Expiration, DateTime.Now.AddSeconds (1000).ToString ()),
                new Claim (JwtRegisteredClaimNames.Iss, iss),
                new Claim (JwtRegisteredClaimNames.Aud, aud),
                new Claim (ClaimTypes.Role, tokenModelJwt.Role),
            };

            ////为了解决一个用户多个角色(比如：Admin,System),可以将一个用户的多个角色全部赋予;
            claims.AddRange (tokenModelJwt.Role.Split (',').Select (s => new Claim (ClaimTypes.Role, s)));

            //密钥的Key
            var key = new SymmetricSecurityKey (Encoding.UTF8.GetBytes (secret));

            //哈希256加密
            var creds = new SigningCredentials (key, SecurityAlgorithms.HmacSha256);

            //生成规则
            var jwt = new JwtSecurityToken (issuer: iss, claims: claims, signingCredentials: creds);

            var jwtHander = new JwtSecurityTokenHandler ();
            var encodedJwt = jwtHander.WriteToken (jwt);

            return encodedJwt;
        }

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="jwtStr"></param>
        /// <returns></returns>
        public static TokenModelJwt SerializeJwt (string jwtStr) {
            var jwtHandler = new JwtSecurityTokenHandler ();
            JwtSecurityToken jwtToken = jwtHandler.ReadJwtToken (jwtStr);

            object role;

            try {
                jwtToken.Payload.TryGetValue (ClaimTypes.Role, out role);
            } catch (Exception ex) {
                Console.WriteLine (ex);
                throw;
            }

            var tm = new TokenModelJwt {
                Uid = (jwtToken.Id).ObjToInt (),
                Role = role != null ? role.ObjToString () : "",
            };

            return tm;
        }
    }

    /// <summary>
    /// 令牌
    /// </summary>
    public class TokenModelJwt {
        /// <summary>
        /// Id
        /// </summary>
        public long Uid { get; set; }
        /// <summary>
        /// 手机
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 头像
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public string UNickName { get; set; }
        /// <summary>
        /// 角色
        /// </summary>
        public string Role { get; set; }
        /// <summary>
        /// 职能
        /// </summary>
        public string Work { get; set; }
    }
}