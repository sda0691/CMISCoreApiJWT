using CMISCoreApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMISCoreApi.Data
{
    public interface IAuthRepository
    {
        Task<User> Register(User user, string password);
        Task<User> Login(string username, string password);
        Task<bool> UserExists(string username);
        string SetSessionValues(User user);
        Task<User> GetUserByUserName(string username);
        Task<User> ResetPassword(User user, string oldPassword, string newPassword);
        Task SaveRefreshToken(string username, RefreshTokenModel refreshToken);
        Task RemoveRefreshToken(string username, string refreshToken);
        Task<RefreshTokenModel> GetRefreshToken(string username);
    }
}
