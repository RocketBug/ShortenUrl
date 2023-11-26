
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using ShortenUrl.Models;
using ShortenUrl.Models.DTO;
using ShortenUrl.Models.Validators;
using SQLitePCL;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace ShortenUrl.Services
{
    class UserService
    {
        private readonly ApiDbContext _context;
        private readonly IConfiguration _configuration;
        public UserService(ApiDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public User CreateUser(UserAuthDto userDto)
        {
            var userValidator = new UserValidator();
            var validationResult = userValidator.Validate(userDto);
            if (!validationResult.IsValid)
            {
                var errorMessage = new StringBuilder();
                foreach (var error in validationResult.Errors)
                {
                    errorMessage.AppendLine(error.ErrorMessage);
                }
                throw new InvalidDataException(errorMessage.ToString());
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
            var user = new User()
            {
                UserName = userDto.UserName.ToLower(),
                PasswordHash = passwordHash,
                UserId = Guid.NewGuid(),
                DateCreated = DateTimeOffset.Now,
                RefreshToken = GetUniqueToken(),
                DateRefreshTokenCreated = DateTimeOffset.Now,
                DateRefreshTokenExpires = DateTimeOffset.Now.AddDays(7),
                IsRefreshTokenRevoked = false,
            };

            if (_context.Users.Any(u => u.UserName == user.UserName))
            {
                throw new InvalidDataException("Username already exists.");
            }
            _context.Users.Add(user);
            _context.SaveChanges();

            var token = CreateToken(user);
            user.Token = token;
            return user;
        }

        public User LoginUser(UserAuthDto userAuthDto)
        {
            var user = _context.Users.SingleOrDefault(u => u.UserName == userAuthDto.UserName);
            if (!_context.Users.Any(u => u.UserName == userAuthDto.UserName) || user is null)
            {
                throw new Exception("User not found.");
            }
            if (user is not null && BCrypt.Net.BCrypt.Verify(userAuthDto.Password, user.PasswordHash))
            {
                user.Token = CreateToken(user);
            }
            if (user.Token.IsNullOrEmpty())
            {
                throw new Exception("Failed to login.");
            }

            user = AssignRefreshToken(user);
            _context.SaveChanges();
            return user;
        }

        public User RefreshToken(string refreshToken)
        {
            var user = _context.Users.SingleOrDefault(u => u.RefreshToken == refreshToken);
            if (user is null || (user is not null && user.IsRefreshTokenRevoked))
            {
                throw new Exception("User is invalid");
            }

            if (user is not null &&  DateTimeOffset.Now > user.DateRefreshTokenExpires)
            {
                throw new Exception("Invalid token");
            }

            user = AssignRefreshToken(user);
            user.Token = CreateToken(user);
            _context.SaveChanges();
            return user;
        }

        private User AssignRefreshToken(User user) 
        {
            user.RefreshToken = GetUniqueToken();
            user.DateRefreshTokenCreated = DateTimeOffset.Now;
            user.DateRefreshTokenExpires = DateTimeOffset.Now.AddDays(7);
            user.IsRefreshTokenRevoked = false;
            return user;
        }

        private string CreateToken(User user)
        {
            // Create the claim based on the user name.
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iss, _configuration.GetSection("JwtSettings:Issuer").Value!),
                new Claim(JwtRegisteredClaimNames.Aud, _configuration.GetSection("JwtSettings:Audience").Value!),
            };
            // Create a symmetric security key
            var jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("JwtSettings:Token").Value!));
            var cred = new SigningCredentials(jwtKey, SecurityAlgorithms.HmacSha256Signature);
            var token = new JwtSecurityToken(claims: claims,
                // TODO: Move timeout in the config file
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: cred,
                issuer: _configuration.GetSection("JwtSettings:Issuer").Value,
                audience: _configuration.GetSection("JwtSettings:Audience").Value);
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }

        public string GetUniqueToken()
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var doesTokenExist = _context.Users.Any(u => u.RefreshToken == token);

            if (doesTokenExist) 
            {
                return GetUniqueToken();
            }
            return token;
        }
    }
}
