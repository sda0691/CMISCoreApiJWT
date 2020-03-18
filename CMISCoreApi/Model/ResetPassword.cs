using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMISCoreApi.Model
{
    public class ResetPassword
    {
        public string username { get; set; }
        public string oldpassword { get; set; }
        public string newpassword { get; set; }
    }
    public class RefreshRequestModel
    {
        public string token { get; set; }
        public string refreshToken { get; set; }
    }
    public class RefreshTokenModel
    {
        public string Token { get; set; }
        public DateTime? RefreshTokenExpiration { get; set; }

    }
}
