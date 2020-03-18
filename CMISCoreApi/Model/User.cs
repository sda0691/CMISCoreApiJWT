using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMISCoreApi.Model
{
    [Table("USER_PROFILE")]
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime Modified { get; set; }
        public string Name { get; set; }
        public int CompanyID { get; set; }

        public int WPID { get; set; }


        public int UserStatus { get; set; }

        public string Title { get; set; }
        public string Email { get; set; }
        public string Question1 { get; set; }
        public string Answer1 { get; set; }
        public string Question2 { get; set; }
        public string Answer2 { get; set; }

        public int Try { get; set; }
        public Int16 Deleted { get; set; }

        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }

        public string RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiration { get; set; }
        public virtual Company Company { get; set; }


    }


}
