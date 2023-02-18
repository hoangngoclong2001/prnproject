using API_EF.Dtos;
using API_EF.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace API_EF.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public static UserDto user = new UserDto();

        public PRN231DBContext dBContext;
        public IConfiguration configuration;
        public AuthController(PRN231DBContext _dBContext, IConfiguration _configuration)
        {
            dBContext = _dBContext;
            configuration = _configuration;
        }

        [HttpPost]
        [Route("signup")]
        public async Task<ActionResult<Account>> Register(SignupDto account)
        {
            Account user = new Account
            {
                Email = account.Email,
                Password = account.Password,
                CustomerId = account.CompanyId,
                Customer = new Customer
                {
                    CustomerId = account.CompanyId,
                    CompanyName = account.CompanyName,
                    ContactName = account.ContactName,
                    ContactTitle = account.ContactTitle,
                    Address = account.Address
                },
                Role = 1
            };

            await dBContext.Accounts.AddAsync(user);
            await dBContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        [Route("signin")]
        public async Task<ActionResult<string>> Login(SigninDto request)
        {
            Account account = await dBContext.Accounts.Where(a => a.Email == request.Email && a.Password == request.Password).Include(c => c.Customer).FirstOrDefaultAsync();

            if (account == null)
                return BadRequest();


            user.Account = account;

            var token = CreateToken(user);
            user.AccessToken = token;
            var refreshToken = GenerateRefreshToken();
            SetRefreshToken(refreshToken);

            return Ok(user);
        }

        [HttpPost]
        [Route("refresh-token")]
        public async Task<ActionResult<UserDto>> RefreshToken(UserDto u)
        {
            var refreshToken = u.RefreshToken;

            if (!user.RefreshToken.Equals(refreshToken))
                return Unauthorized("Invalid Refresh Token.");
            else if (user.TokenExpires < DateTime.Now)
                return Unauthorized("Token expired.");

            string token = CreateToken(user);
            user.AccessToken = token;
            var newRefreshToken = GenerateRefreshToken();
            SetRefreshToken(newRefreshToken);

            return Ok(user);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        private RefreshToken GenerateRefreshToken()
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.Now.AddMinutes(30),
                Created = DateTime.Now
            };
            return refreshToken;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        private void SetRefreshToken(RefreshToken newRefreshToken)
        {
            user.RefreshToken = newRefreshToken.Token;
            user.TokenCreated = newRefreshToken.Created;
            user.TokenExpires = newRefreshToken.Expires;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public string CreateToken(UserDto user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, user.Account.Email),
                new Claim(ClaimTypes.Role, user.Account.Role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: configuration["JWT:Issuer"],
                audience: configuration["JWT:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
    }
}
