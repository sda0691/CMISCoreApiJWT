using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CMISCoreApi.Data;
using CMISCoreApi.Dto;
using CMISCoreApi.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;

namespace CMISCoreApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;

        public AuthController(IAuthRepository repo, IConfiguration config)
        {
            this._repo = repo;
            this._config = config;
        }
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto) //[FromBody] UserForLoginDto userForLoginDto) //string username, string password) //
        {
            var userFromRepo = await _repo.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);
            //var guid = await _repo.Login(username.ToLower(), password);
            if (userFromRepo == null)
                return Unauthorized();

            var guid = _repo.SetSessionValues(userFromRepo);

            var claim = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.UserId.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username)
            };

            var jwtToken = GenerateAccessToken(claim);
            var refreshToken = GenerateRefreshTokenModel();

            await _repo.SaveRefreshToken(userForLoginDto.Username.ToLower(), refreshToken);
            
            return Ok(new
            {
                token = jwtToken,
                guid,
                refreshToken = refreshToken.Token
            }); 
        }
        public RefreshTokenModel GenerateRefreshTokenModel()
        {
            // Create the refresh token
            RefreshTokenModel refreshToken = new RefreshTokenModel()
            {
                Token = GenerateRefreshToken(),
                RefreshTokenExpiration = DateTime.UtcNow.AddMinutes(35) // Make this configurable
            };
            return refreshToken;
        }
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
        private string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config.GetSection("AppSettings:TokenExpiredTime").Value)),
                SigningCredentials = creds
            };
            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh(RefreshRequestModel refreshTokenModel)
        {
            var principal = GetPrincipalFromExpiredToken(refreshTokenModel.token);
            var username = principal.Identity.Name;
            var savedRefreshToken = _repo.GetRefreshToken(username).Result; //retrieve the refresh token from a data store
            if (savedRefreshToken.Token != refreshTokenModel.refreshToken)
            {
                await _repo.RemoveRefreshToken(username, refreshTokenModel.refreshToken);
                throw new SecurityTokenException("Invalid refresh token");
            }

            if (DateTime.UtcNow > savedRefreshToken.RefreshTokenExpiration)
            {
                await _repo.RemoveRefreshToken(username, refreshTokenModel.refreshToken);
                throw new SecurityTokenException("Invalid token!");
            }


            var newJwtToken = GenerateAccessToken(principal.Claims);
            var newRefreshToken = GenerateRefreshTokenModel();
            //_repo.RemoveRefreshToken(username, refreshTokenModel.refreshToken);
            await _repo.SaveRefreshToken(username, newRefreshToken);

            return Ok (new
            {
                token = newJwtToken,
                refreshToken = newRefreshToken
            });
        }
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                RequireExpirationTime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value)),
                //IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config.GetSection("AppSettings:Token").Value)),
                ValidateLifetime = false //here we are saying that we don't care about the token's expiration date

                // ClockSkew = TimeSpan.Zero,
                //RequireExpirationTime = true,
                //ValidateIssuerSigningKey = true,
                //IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value)),
                //ValidateIssuer = false,
                //ValidateAudience = false
            };


            
     


            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
        [HttpPost("resetpassword")]
        public async Task<IActionResult> ResetPassword(ResetPassword resetpassword) // string username, string oldpassword, string newpassword)
        {
            
            var user = await _repo.GetUserByUserName(resetpassword.username);

            if (user == null)
                return NotFound();

            user = await _repo.ResetPassword(user, resetpassword.oldpassword, resetpassword.newpassword);

            if (user == null)
                return BadRequest("Reset password failed.");

            return Ok();
          }

        [HttpGet("LoginTest")]
        public IActionResult LoginTest()
        {
            return Ok(new
            {
                V = "OK"
            });
        }
    }
}