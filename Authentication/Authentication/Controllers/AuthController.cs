using Authentication.Model;
using Authentication.Model.User;
using Authentication.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuthController : ControllerBase
    {

        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IUserManagementService _userManagement;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AuthController(ILoggerFactory logger, IUserManagementService userManagement, IHttpContextAccessor httpContextAccessor)
        {
            _loggerFactory = logger;
            _logger = logger.CreateLogger<AuthController>();

            _userManagement = userManagement;
            _httpContextAccessor = httpContextAccessor;
        }

        [Route("[action]")]
        [ActionName("CheckLogin")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<UserModel> CheckLogin()
        {
            return await _userManagement.GetUserModel(HttpContext.Request, HttpContext.User);
        }

        [Route("[action]")]
        [ActionName("Login")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<RedirectResult> Login(AuthUserModel userModel)
        {
            string redirectUrl = await _userManagement.ValidateUserLogin(userModel);
            return Redirect(redirectUrl);
        }
    }
}
