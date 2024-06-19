using API.Data;
using API.DTOS;
using API.Entities;
using API.Entities.OrderAggregate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class OrderControllers:BaseApiController
    {
        private readonly StoreContext _context;
        public OrderControllers(StoreContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Order>>> GetOrders()
        {
            return await _context.Orders
            .Include(o=> o.OrderItems)
            .Where(x=> x.BuyerId == User.Identity.Name).ToListAsync();
        }

        [HttpGet("{id}", Name = "GetOrder")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            return await _context.Orders
            .Include(o=> o.OrderItems)
            .Where(x=> x.BuyerId == User.Identity.Name && x.Id == id)
            .FirstOrDefaultAsync(x=> x.Id == id);
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateOrder(CreateOrderDto orderDto){
            var basket = await _context.Baskets.
            RetrieveBasketWithItems(User.Identity.Name)
            .FirstOrDefaultAsync();

            if(basket == null) return BadRequest(new ProblemDetails { Title = "Could not locate basket" });

            var items = new List<OrderItem>();
            foreach (var item in basket.Items){
                var productItem = await _context.Products.FindAsync(item.ProductId);
                var itemOrdered = new ProductItemOrderd{
                    ProductId = productItem.Id,
                    Name = productItem.Name,
                    PictureUrl = productItem.PictureUrl
                };

                var orderItem = new OrderItem{
                    ItemOrdered = itemOrdered,
                    Price = productItem.Price,
                    Quantity = item.Quantity
                };

                items.Add(orderItem);
                productItem.QuantityInStock -=item.Quantity;

            }

            var subtotal = items.Sum(item => item.Price * item.Quantity);
            var deliveryFee = subtotal > 10000 ? 0 : 500;

            var order = new Order{
                BuyerId = User.Identity.Name,
                OrderItems = items,
                ShippingAddress = orderDto.ShipToAddress,
                SubTotal = subtotal,
                DeliveryFee = deliveryFee,
            };

            _context.Orders.Add(order);
            _context.Baskets.Remove(basket);

            if (orderDto.SaveAddress){
                var user = await _context.Users.FirstOrDefaultAsync(x=> x.UserName == User.Identity.Name);
                user.Address = new UserAddress{
                    FullName = orderDto.ShipToAddress.FullName,
                    Address1 = orderDto.ShipToAddress.Address1,
                    City = orderDto.ShipToAddress.City,
                    State = orderDto.ShipToAddress.State,
                    Address2 = orderDto.ShipToAddress.Address2,
                    Zip = orderDto.ShipToAddress.Zip,
                    Country = orderDto.ShipToAddress.Country

                };
                _context.Update(user);
            }

            var result = await _context.SaveChangesAsync() > 0;

            if (result) return CreatedAtRoute("GetOrder", new {id = order.Id}, order.Id);
            
            return BadRequest(new ProblemDetails { Title = "Failed to create order" });

        }
    }
}