using API.Data;
using API.DTOS;
using API.Entities;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers{
    public class AccountController:BaseApiController{
        private readonly TokenService _tokenService;
        private readonly StoreContext _context;


        private readonly UserManager<User> _userManager;
        public AccountController(UserManager<User> userManager, TokenService tokenService, StoreContext context){
            _tokenService = tokenService;
            _userManager = userManager;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto){
            var user = await _userManager.FindByNameAsync(loginDto.UserName);
            if(user == null || !await _userManager.CheckPasswordAsync(user,loginDto.Password))
                return Unauthorized();

            var userBasket = await RetrieveBasket(loginDto.UserName);
            var anonBasket = await RetrieveBasket(Request.Cookies["buyerId"]);

            if(anonBasket !=null){
                if (userBasket != null) _context.Baskets.Remove(userBasket);
                anonBasket.BuyerId = loginDto.UserName;
                Response.Cookies.Delete("buyerId");

                await _context.SaveChangesAsync();
            }

            return new UserDto{
                Email = user.Email,
                Token = await _tokenService.GenerateToken(user),
                Basket = anonBasket !=null?  BasketExtension.MapBasketToDto(anonBasket) : BasketExtension.MapBasketToDto(userBasket)
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

        [Authorize]
        [HttpGet("currentUser")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var userBasket = await RetrieveBasket(User.Identity.Name);

            return new UserDto
            {
                Email = user.Email,
                Token = await _tokenService.GenerateToken(user),
                Basket = BasketExtension.MapBasketToDto(userBasket)
            };
        }

         private async Task<Basket> RetrieveBasket( string buyerId)
        {
            if(string.IsNullOrEmpty(buyerId)){
                Response.Cookies.Delete("buyerId");
                return null;
            } 

            return await _context.Baskets
                .Include(i => i.Items)
                .ThenInclude(p => p.Product)
                .FirstOrDefaultAsync(basket => basket.BuyerId == buyerId);
        }
    }

   

}