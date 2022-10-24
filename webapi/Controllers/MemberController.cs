using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using WebAPI.Lib;
using WebAPI.Model;
using WebAPI.Filter;
using System;

namespace WebAPI.Controllers 
{
    [ApiController]
    [Route("api/[controller]")]
    public class MemberController : ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login([FromBody]LoginData req_data)
        {
            try
            {
                using (var db = new AppDb())
                {
                    string sql = @"xp_loginWithPassword";
                    var p = new DynamicParameters();
                    p.Add("@acc", req_data.Account);
                    p.Add("@pwd", req_data.Password);

                    p.Add("@token", req_data.token, direction: ParameterDirection.Output);
                    p.Add("@refreshtoken", req_data.refreshtoken, direction: ParameterDirection.Output);

                    db.Connection.Execute(sql, p, commandType: CommandType.StoredProcedure);

                    CookieOptions options = new CookieOptions();
                    options.HttpOnly = true;
                    options.Secure = true;
                    options.SameSite = SameSiteMode.None;
                    options.Expires = DateTimeOffset.Now.AddDays(1);
                    options.Domain = "an990154054";
                    //options.Domain = "localhost";
                    options.Path = "/";

                    Response.Cookies.Append("Token", p.Get<Guid>("token").ToString(), options);
                    Response.Cookies.Append("RefreshToken", p.Get<Guid>("refreshtoken").ToString(), options);
                    string Message = "Login Success";
                    return Ok(new { Message });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpGet("login")]
        [AuthorizationFilter]
        public IActionResult AutoLogin()
        {
            return Ok();
        }

        [HttpPost("logout")]
        [AuthorizationFilter]
        public IActionResult Logout()
        {
            using(var db = new AppDb())
            {
                string sql = @"xp_logout";
                var p = new DynamicParameters();
                p.Add("@mid", (int)HttpContext.Items["MID"]);
                p.Add("@token", HttpContext.Request.Cookies.Where(x => x.Key == "Token").FirstOrDefault().Value);
                p.Add("@refreshtoken", HttpContext.Request.Cookies.Where(x => x.Key == "RefreshToken").FirstOrDefault().Value);
                db.Connection.Execute(sql, p, commandType: CommandType.StoredProcedure);
            }
            CookieOptions options = new CookieOptions();
            options.HttpOnly = true;
            options.Secure = true;
            options.SameSite = SameSiteMode.None;
            options.Expires = DateTimeOffset.Now.AddDays(1);
            options.Domain = "an990154054";
            //options.Domain = "localhost";
            options.Path = "/";
            Response.Cookies.Delete("Token", options);
            Response.Cookies.Delete("RefreshToken", options);
            return Ok();
        }

        [HttpGet("me")]
        [AuthorizationFilter]
        public IActionResult GetMe(int id)
        {
            using(var db = new AppDb())
            {
                string sql = @"select UUID, Account, EMail, NickName, Since, ModifyDatetime from vd_Member where MID = @mid";
                var mid = HttpContext.Items["MID"];
                MemberModel data = db.Connection.QueryFirstOrDefault<MemberModel>(sql, new { mid });
                return Ok(new { data.UUID, data.Account, data.EMail, data.NickName, data.Since, data.ModifyDatetime });
            }
        }
        
        [HttpPost("me")]
        [AuthorizationFilter]
        public IActionResult UpdateMe([FromBody] MemberModel payload)
        {
            try
            {
                using (var db = new AppDb())
                {
                    string sql = "xp_MemberUpdate ";
                    var p = new DynamicParameters();
                    var mid = HttpContext.Items["MID"];

                    p.Add("@mid", HttpContext.Items["MID"]);
                    p.Add("@uuid", payload.UUID);
                    p.Add("@acc", payload.Account);
                    p.Add("@nickname", payload.NickName);
                    p.Add("email", payload.EMail);

                    db.Connection.Execute(sql, p, commandType: CommandType.StoredProcedure);
                }
                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("new")]
        public IActionResult CreateNewUser([FromBody] MemberModel req_data)
        {
            try
            {
                using (var db = new AppDb())
                {
                    string sql = @"xp_createNewUser";
                    var p = new DynamicParameters();
                    p.Add("@acc", req_data.Account);
                    p.Add("@pwd", req_data.Password);
                    p.Add("@email", req_data.EMail);
                    p.Add("@nickname", req_data.NickName);
                    p.Add("@mid", DbType.Int32, direction: ParameterDirection.Output);
                    db.Connection.Execute(sql, p, commandType: CommandType.StoredProcedure);
                    HttpContext.Items.Add("MID", p.Get<object>("mid"));
                }
                // send verify email
                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("new/verify")]
        public IActionResult VerifyNewAccount()
        {
            using (var db = new AppDb())
            {
                string sql = "xp_MemberVerify";
                var p = new DynamicParameters();
                /* parameters need to assign */
                db.Connection.Execute(sql, p, commandType: CommandType.StoredProcedure);
            }
            return Ok();
        }

        [HttpPost("new/verify/resend")]
        public IActionResult ResendVerifyCode()
        {
            try
            {
                using (var db = new AppDb())
                {
                    string sql = "xp_MemberVerifyResend";
                    var p = new DynamicParameters();
                    /* parameters need to assign */
                    db.Connection.Execute(sql, p, commandType: CommandType.StoredProcedure);
                }
                return Ok();
            }
            catch
            {
                return BadRequest("This API haven't done yet");
            }
            
        }

        [HttpPost("password/forget")]
        public IActionResult ForgetPassword()
        {
            using (var db = new AppDb())
            {
                string sql = "xp_MemberPasswordForget";
                var p = new DynamicParameters();
                /* parameters need to assign */
                db.Connection.Execute(sql, p, commandType: CommandType.StoredProcedure);
            }
            return Ok();
        }

        [HttpPost("password/renew")]
        [AuthorizationFilter]
        public IActionResult RenewPassword([FromBody] ResetPassword payload)
        {
            try
            {
                using (var db = new AppDb())
                {
                    string sql = "xp_MemberRenewPassword";
                    var p = new DynamicParameters();
                    p.Add("@mid", HttpContext.Items["MID"]);
                    p.Add("@oldpassword", payload.OldPassword);
                    p.Add("@newpassword", payload.NewPassword);
                    db.Connection.Execute(sql, p, commandType: CommandType.StoredProcedure);
                }
                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
    }
}
