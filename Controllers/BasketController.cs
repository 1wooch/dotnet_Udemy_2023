using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class BasketController:BaseApiController
    {
        private readonly StoreContext _context;
        public BasketController(StoreContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<Basket>> GetBasket()
        {
            var basket = await _context.Baskets
            .Include(x => x.Items)
            .ThenInclude(p => p.Product)
            .FirstOrDefaultAsync(x => x.BuyerId == Request.Cookies["basketId"]); 

            if(basket == null) return NotFound();
            return basket;

        }


        [HttpPost]
        public async Task<ActionResult> AddItemToBasket(int productId,int quantity){
            //1. Get Basket ->if it doesn't exist create one
            //2. get Product
            //3. Add product to basket
            //4. Save to db
            return StatusCode(201);
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteItemFromBasket(int productId,int quantity){
            return Ok();
        }   
    }
}