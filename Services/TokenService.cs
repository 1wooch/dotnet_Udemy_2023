using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace API.Services
{
    public class TokenService
    {

        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _config;
        public TokenService(UserManager<User> userManager, IConfiguration config){
            _userManager = userManager;
            _config = config;
        }

        public async Task<string> GenerateToken(User user){
            var claims = new List<Claim>{ //claim is what user request
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName),

            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach(var role in roles){
                claims.Add(new Claim(ClaimTypes.Role, role));
                //if there is more than one role, it will add to the claim
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWTSettings:TokenKey"])); 
            //symmetric key is the key that is used to encrypt and decrypt the token
            var creads = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            // set credential algorigm to HmacSha512Signature

            var tokenOptions = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creads
            );
            
            return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        }
    }
}