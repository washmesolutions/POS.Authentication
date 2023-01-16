using Authentication.Model;
using Authentication.Model.Exception;
using Authentication.Model.User;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Authentication.Service
{
    public class UserManagementService : IUserManagementService
    {
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration configuration;
        //private readonly ConfigWrapper _config = new ConfigWrapper();
        public UserManagementService(ILoggerFactory logger, IHttpContextAccessor httpContextAccessor, IConfiguration _configuration)
        {
            _loggerFactory = logger;
            _logger = logger.CreateLogger<UserManagementService>();
            configuration = _configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<UserModel> GetUserModel(HttpRequest request, ClaimsPrincipal userData)
        {
            _logger.LogInformation("Start GetUserModel");
            UserModel userModel = new UserModel();
            try
            {
                if (userData != null && userData.Identity.IsAuthenticated)
                {
                    return userModel;
                }
                else
                {
                    var redirectUrl = "http://localhost:8080/login";
                    //var samlRequest = "";
                    throw new AuthenticationFailure(redirectUrl);
                }
            }
            catch(Exception ex)
            {
                var redirectUrl = "http://localhost:8080/login";
                //var samlRequest = "";
                throw new AuthenticationFailure(redirectUrl);
               
            }
        }

        public async Task<string> ValidateUserLogin(AuthUserModel authUser)
        {
            string redirectUrl = string.Empty;
            try
            {
                var claims = new List<Claim>();
                claims.Add(new Claim("UserID", "1"));
                claims.Add(new Claim("PortalId", "1"));
                claims.Add(new Claim("UserName", authUser.UserName));

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    //AllowRefresh = <bool>,
                    // Refreshing the authentication session should be allowed.

                    //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                    // The time at which the authentication ticket expires. A 
                    // value set here overrides the ExpireTimeSpan option of 
                    // CookieAuthenticationOptions set with AddCookie.

                    IsPersistent = false,
                    // Whether the authentication session is persisted across 
                    // multiple requests. When used with cookies, controls
                    // whether the cookie's lifetime is absolute (matching the
                    // lifetime of the authentication ticket) or session-based.

                    //IssuedUtc = <DateTimeOffset>
                    // The time at which the authentication ticket was issued.

                    //RedirectUri = <string>
                    // The full path or absolute URI to be used as an http 
                    // redirect response value.
                };
                await this._httpContextAccessor.HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);


                redirectUrl = "http://localhost:8080/";
            }
            catch(Exception ex)
            {
                _logger.LogError("ValidateUserLogin -> {0}",ex);
            }
            return redirectUrl;
        }


        public async Task<LoginResponse> GetAuthLoginToken(AuthUserModel authUser)
        {
            LoginResponse tokenAuthenticationResponse = new LoginResponse();
            var client = new HttpClient();

            var disco = await client.GetDiscoveryDocumentAsync(configuration["APISettings:IdentityServerUri"]);

            var tokenResponse = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = configuration["APISettings:ClientId"],
                ClientSecret = configuration["APISettings:ClientSecret"],
                Scope = configuration["APISettings:Scope"],
                UserName = authUser.UserName,
                Password = authUser.Password
            });

            tokenAuthenticationResponse.AccessToken = tokenResponse.AccessToken;
            return tokenAuthenticationResponse;
        }

    }
}
