﻿using Stories.Models;
using Stories.Models.Users;
using System.Threading.Tasks;

namespace Stories.Services
{
    public interface IAuthenticationService
    {
        Task<UserModel> AuthenticateUser(string email, string password);
        Task<bool> ChangePassword(ChangePasswordModel model);
    }
}
