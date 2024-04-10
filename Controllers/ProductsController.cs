using API.Data;
using API.Entities;
using API.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ProductsController:BaseApiController
	{
		private readonly StoreContext _context; //generally use underscore bar for private value
		public ProductsController(StoreContext context)
		{
			_context = context;
		}

		[HttpGet]
		public async  Task<ActionResult<List<Product>>> GetProducts(string orderBy)
		{
			var query =  _context.Products
			.Sort(orderBy)
			.AsQueryable();

			return await query.ToListAsync();

			
		}

		[HttpGet("{id}")] // api/products/3 => 3 is ID (productID)
		public async Task<ActionResult<Product>> GetProduct(int id)
		{
			var product = await _context.Products.FindAsync(id);
			if(product == null) return NotFound();
			return product;
		}
	}
}

