﻿using System.Text.Json;
using API.Data;
using API.Entities;
using API.Extensions;
using API.RequestHelpers;
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
		public async  Task<ActionResult<PagedList<Product>>> GetProducts(ProductParams productParams)
		{
			var query =  _context.Products
			.Sort(productParams.OrderBy)
			.Search(productParams.SearchTerm)
			.Filter(productParams.Brands,productParams.Types)
			.AsQueryable();

			var products = await PagedList<Product>.ToPagedList(query,productParams.PageNumber, productParams.PageSize);


			Response.Headers.Add("Pagination", JsonSerializer.Serialize(products.MetaData));
			return products;

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

