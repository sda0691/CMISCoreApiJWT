using CMISCoreApi.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMISCoreApi.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;
        private readonly IConfigRepository _configRepo;

        public AuthRepository(DataContext context, IConfigRepository configRepo)
        {
            _context = context;
            _configRepo = configRepo;
        }

        public async Task<User> Login(string username, string password)
        {
            //var guid = Guid.NewGuid();
            var user = await _context.Users.Include(c => c.Company).FirstOrDefaultAsync(x => x.Username == username && (x.UserStatus == 1 || x.UserStatus == 3));
            if (user == null)
                return null;
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                int MaxTry = 0;
                Config config = await _configRepo.GetConfigByCategoryConfigName("USER", "MaxTry");
                if (config != null)
                {
                    MaxTry = int.Parse(config.ConfigValue);
                    user.UserStatus = user.Try + 1 >= MaxTry ? 2 : user.UserStatus;
                    user.Try++;
                    await _context.SaveChangesAsync();
                }
                return null;
            }
            else
            {
                user.Try = 0;
                await _context.SaveChangesAsync();

                //insert into session transfer table
                
                //var currDate = DateTime.Now;

                //List<SessionKeyValue> SessionKeyValueList = new List<SessionKeyValue>() {
                //    new SessionKeyValue { sessionKey = "LoginID", sessionValue = user.Username },
                //    new SessionKeyValue { sessionKey = "UserName", sessionValue = user.Name },
                //    new SessionKeyValue { sessionKey = "UserID", sessionValue = user.UserId.ToString() },
                //    new SessionKeyValue { sessionKey = "WPID", sessionValue = user.WPID.ToString() },
                //    new SessionKeyValue { sessionKey = "Email", sessionValue = user.Email },
                //    new SessionKeyValue { sessionKey = "Title", sessionValue = user.Title },
                //    new SessionKeyValue { sessionKey = "Company", sessionValue = user.Company.CompanyName },
                //    new SessionKeyValue { sessionKey = "ClientOf", sessionValue = user.Company.ClientOf },
                //};
                

                //foreach(SessionKeyValue sessionKey in SessionKeyValueList)
                //{
                //    InsertSessionState(sessionKey.sessionKey, sessionKey.sessionValue, guid, currDate, _context);
                //}


            }

            return user;
        }
        public async Task SaveRefreshToken(string username, RefreshTokenModel refreshToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == username );
            user.RefreshToken = refreshToken.Token;
            user.RefreshTokenExpiration = refreshToken.RefreshTokenExpiration;
            await _context.SaveChangesAsync();
        }
        public async Task RemoveRefreshToken(string username, string refreshToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);
            user.RefreshToken = null;
            user.RefreshTokenExpiration = null;
            await _context.SaveChangesAsync();
        }
        public async Task<RefreshTokenModel> GetRefreshToken(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == username && (x.UserStatus == 1 || x.UserStatus == 3));
            if (user == null)
                return null;
            RefreshTokenModel refreshTokenModel = new RefreshTokenModel();
            refreshTokenModel.Token = user.RefreshToken;
            refreshTokenModel.RefreshTokenExpiration = user.RefreshTokenExpiration ;

            return refreshTokenModel;
        }
        public class SessionKeyValue
        {
            public string sessionKey { get; set; }
            public string sessionValue { get; set; }
        }
        public  string SetSessionValues(User user)
        {
            var guid = Guid.NewGuid();
            var currDate = DateTime.Now;

            List<SessionKeyValue> SessionKeyValueList = new List<SessionKeyValue>() {
                    new SessionKeyValue { sessionKey = "LoginID", sessionValue = user.Username },
                    new SessionKeyValue { sessionKey = "UserName", sessionValue = user.Name },
                    new SessionKeyValue { sessionKey = "UserID", sessionValue = user.UserId.ToString() },
                    new SessionKeyValue { sessionKey = "WPID", sessionValue = user.WPID.ToString() },
                    new SessionKeyValue { sessionKey = "Email", sessionValue = user.Email },
                    new SessionKeyValue { sessionKey = "Title", sessionValue = user.Title },
                    new SessionKeyValue { sessionKey = "Company", sessionValue = user.Company.CompanyName },
                    new SessionKeyValue { sessionKey = "ClientOf", sessionValue = user.Company.ClientOf },
                };


            foreach (SessionKeyValue sessionKey in SessionKeyValueList)
            {
                InsertSessionState(sessionKey.sessionKey, sessionKey.sessionValue, guid, currDate, _context);
            }
            return guid.ToString();

        }
        private void InsertSessionState(string sessionKey, string sesstionValue, Guid guid, DateTime currDate , DataContext _context)
        {
            var sessionKeyValue = new ASPSessionState();
            sessionKeyValue.GUID = guid;
            sessionKeyValue.SessionKey = sessionKey;
            sessionKeyValue.SessionValue = sesstionValue;
            sessionKeyValue.DateCreated = currDate;

            _context.ASPSessionStates.Add(sessionKeyValue);
            _context.SaveChanges();
        }
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i])
                        return false;
                }
            }
            return true;
        }
        public async Task<User> GetUserByUserName(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);
            return user;
        }
        public async Task<User> ResetPassword(User user, string oldPassword, string newPassword) //userid, newEncryptedPassword, passwordHash, passwordSalt
        {
            if (!VerifyPasswordHash(oldPassword, user.PasswordHash, user.PasswordSalt))
                return null; // password is not correct

            CreatePsswordHash(newPassword, out byte[] passwordHash, out byte[] passwordSalt);
            var encryptedPassword = getEncryptedPassword(newPassword, "encrypt");

            user.Password = encryptedPassword;
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _context.SaveChangesAsync();

            return user;
        }
        private string getEncryptedPassword(String newPassword, String txtAction)
        {
            int iOffset = 120;
            String convertstr = "";
            char[] pwdArray = newPassword.ToCharArray();

            for (int i = 0; i < newPassword.Length; i++)
            {
                switch (txtAction)
                {
                    case "decrypt":
                        convertstr += Convert.ToChar((int)pwdArray[i] - iOffset);
                        break;
                    case "encrypt":
                        convertstr += Convert.ToChar((int)pwdArray[i] + iOffset);
                        break;
                }
            }

            return convertstr;

        }

        private void CreatePsswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        public Task<User> Register(User user, string password)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UserExists(string username)
        {
            throw new NotImplementedException();
        }
    }
}
