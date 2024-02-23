using API.Data;
using API.DTOS;
using API.Entities;
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

        [HttpGet(Name ="GetBasket")]
        public async Task<ActionResult<BasketDto>> GetBasket()
        {
            var basket = await RetrieveBasket();

            if (basket == null) return NotFound();
            return MapBasketToDto(basket);

        }

        

        [HttpPost]
        public async Task<ActionResult<BasketDto>> AddItemToBasket(int productId,int quantity){
            
            //1. Get Basket ->if it doesn't exist create one
            var basket = await RetrieveBasket();
            if(basket == null){
                CreateBasket();
            }

            //2. get Product
            var product = await _context.Products.FindAsync(productId); 
            if(product == null) return NotFound();
            
            //3. Add product to basket
            basket.AddItem(product,quantity);

            //4. Save to db
            var result = await _context.SaveChangesAsync()>0;
            if(result)  return CreatedAtRoute("GetBasket",MapBasketToDto(basket));

            return BadRequest(new ProblemDetails{Title="Problem adding item to basket"});
        }

       

        [HttpDelete]
        public async Task<ActionResult> DeleteItemFromBasket(int productId,int quantity){
            var basket = await RetrieveBasket();
            if(basket == null) return NotFound();
     
            basket.RemoveItem(productId,quantity);

            var result = await _context.SaveChangesAsync()>0;
            if(result)  return Ok();
            return BadRequest(new ProblemDetails{Title="Problem removing item from basket"});
        }   

        private async Task<Basket> RetrieveBasket()
        {
            return await _context.Baskets
            .Include(x => x.Items)
            .ThenInclude(p => p.Product)
            .FirstOrDefaultAsync(x => x.BuyerId == Request.Cookies["buyerId"]); 
        }
         private Basket CreateBasket()
        {
            var buyerId=Guid.NewGuid().ToString();
            var cookieOptions = new CookieOptions{IsEssential = true,Expires = DateTime.Now.AddDays(30)}; //this is how we create cookie option in .net core
            Response.Cookies.Append("buyerId",buyerId,cookieOptions);
            var basket = new Basket{BuyerId=buyerId};
            _context.Baskets.Add(basket);
            return basket;
        }
        private BasketDto MapBasketToDto(Basket basket)
        {
            return new BasketDto
            {
                Id = basket.Id,
                BuyerId = basket.BuyerId,
                Items = basket.Items.Select(x => new BsketItemDto
                {
                    ProductId = x.Product.Id,
                    Name = x.Product.Name,
                    Price = x.Product.Price,
                    PictureUrl = x.Product.PictureUrl,
                    Brand = x.Product.Brand,
                    Type = x.Product.Type,
                    Quantity = x.Quantity
                }).ToList()

            };
        }

    }
}