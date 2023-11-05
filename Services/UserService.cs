
using Microsoft.IdentityModel.Tokens;
using ShortenUrl.Models;
using ShortenUrl.Models.Validators;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

        public User CreateUser(UserDto userDto)
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
            };

            if (_context.Users.Any(u => u.UserName == user.UserName))
            {
                throw new InvalidDataException("Username already exists.");
            }
            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }

        public string LoginUser(UserDto userDto)
        {
            var u = _context.Users.Where(u => u.UserName == userDto.UserName);
            if (!_context.Users.Any(u => u.UserName == userDto.UserName)) 
            {
                throw new Exception("User not found.");
            }
            var user = _context.Users.SingleOrDefault(u => u.UserName == userDto.UserName);
            var token = "";
            if (user is not null && BCrypt.Net.BCrypt.Verify(userDto.Password, user.PasswordHash))
                token = CreateToken(user);

            if (token.IsNullOrEmpty())
                throw new Exception("Failed to login.");

            return token;
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
                expires: DateTime.Now.AddMinutes(5), 
                signingCredentials: cred, 
                issuer: _configuration.GetSection("JwtSettings:Issuer").Value, 
                audience: _configuration.GetSection("JwtSettings:Audience").Value);
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }
    }
}
