using API.DTOS;
using API.Entities;
using API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers{
    public class AccountController:BaseApiController{
        private readonly TokenService _tokenService;

        private readonly UserManager<User> _userManager;
        public AccountController(UserManager<User> userManager, TokenService tokenService){
            _tokenService = tokenService;
            _userManager = userManager;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto){
            var user = await _userManager.FindByNameAsync(loginDto.UserName);
            if(user == null || !await _userManager.CheckPasswordAsync(user,loginDto.Password))
                return Unauthorized();
            return new UserDto{
                Email = user.Email,
                Token = await _tokenService.GenerateToken(user),
            };
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterDto registerDto){
            var user = new User{UserName = registerDto.UserName, Email = registerDto.Email};
            var result = await _userManager.CreateAsync(user,registerDto.Password);

            if(!result.Succeeded){
                result.Errors.ToList().ForEach(error => ModelState.AddModelError(error.Code,error.Description)); //if not work convert into for
                return ValidationProblem();
            }

            await _userManager.AddToRoleAsync(user,"Member");

            return StatusCode(200);

        }
    }

}