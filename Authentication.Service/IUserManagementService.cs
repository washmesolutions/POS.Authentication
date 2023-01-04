using Authentication.Model;
using Authentication.Model.User;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Authentication.Service
{
    public interface IUserManagementService
    {
        public Task<UserModel> GetUserModel(HttpRequest request, ClaimsPrincipal userData);
        public Task<string> ValidateUserLogin(AuthUserModel authUser);
    }
}
