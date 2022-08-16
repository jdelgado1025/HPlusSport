using HPlusSport.API.Data;
using HPlusSport.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
//using System.Linq;
//using System.Linq.Dynamic.Core;
using System.Reflection;

namespace HPlusSport.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ShopContext _shopContext;
        public ProductsController(ShopContext shopContext)
        {
            _shopContext = shopContext;
            
            //Make sure the database is seeded before trying to retrieve data
            _shopContext.Database.EnsureCreated();
        }

        [HttpGet]
        public async Task<ActionResult> GetAllProductsAsync([FromQuery]ProductQueryParameters queryParameters)
        {
            //Get a queryable list of products
            IQueryable<Product> products = _shopContext.Products;

            //Filter items by minimum price
            if(queryParameters.MinPrice != null)
            {
                products = products.Where(p => p.Price >= queryParameters.MinPrice.Value);
            }
            //Filter items by maximum price
            if(queryParameters.MaxPrice != null)
            {
                products = products.Where(p => p.Price <= queryParameters.MaxPrice.Value);
            }
            //Filter by search string on SKU
            if (!string.IsNullOrEmpty(queryParameters.Sku))
            {
                products = products.Where(p => p.Sku == queryParameters.Sku);
            }

            //Filter by search on product name
            if (!string.IsNullOrEmpty(queryParameters.Name))
            {
                products = products.Where(p => p.Name.ToLower().Contains(queryParameters.Name.ToLower()));
            }

            if (!string.IsNullOrEmpty(queryParameters.SearchTerm))
            {
                var searchTerm = queryParameters.SearchTerm;
                products = products.Where(
                        p => p.Name.ToLower().Contains(searchTerm.ToLower()) ||
                        p.Sku.ToLower().Contains(searchTerm.ToLower()) ||
                        p.Description.ToLower().Contains(searchTerm.ToLower())
                    );
            }

            //Sort products by user provided query
            if (!string.IsNullOrEmpty(queryParameters.SortBy))
            {
                if(typeof(Product).GetProperty(queryParameters.SortBy) != null)
                {
                    products = products.OrderByCustom(queryParameters.SortBy, queryParameters.SortOrder);
                }
            }

            //Pagination
            products = products
                .Skip(queryParameters.Size * (queryParameters.Page - 1)) //Skip the pages before the request page
                .Take(queryParameters.Size);                             //Retrieve only the page size of results
            return Ok(await products.ToListAsync());                     //Return a list of products
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetProductAsync(int id)
        {
            var product = await _shopContext.Products.FirstOrDefaultAsync(x => x.Id == id);

            if(product == null)
                return NotFound();

            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> PostProductAsync(Product product)
        {
            _shopContext.Products.Add(product);
            await _shopContext.SaveChangesAsync();

            return CreatedAtAction(
                "GetProduct",
                new {id = product.Id},
                product);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> PutProductAsync(int id, [FromBody]Product product)
        {
            if(id != product.Id)
            {
                return BadRequest();
            }

            _shopContext.Entry(product).State = EntityState.Modified;

            try
            {
                await _shopContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if(_shopContext.Products.Any(p => p.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            
            return NoContent();

        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            //We don't need the whole product, just need to know it exists
            var product = await _shopContext.Products.FindAsync(id);
            //_shopContext.Products.FirstOrDefault(p => p.Id == id);

            if(product == null)
            {
                return NotFound();
            }

            //var result = _shopContext.Entry(product).State = EntityState.Deleted;
            //Preferred .NET way of removing objects is below
            _shopContext.Products.Remove(product);
            await _shopContext.SaveChangesAsync();

            return Ok(product);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<ActionResult> DeleteMultipleProducts([FromQuery] int[] productIds)
        {
            var products = new List<Product>();
            foreach(var id in productIds)
            {
                var product = await _shopContext.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                products.Add(product);
            }

            

            _shopContext.Products.RemoveRange(products);
            await _shopContext.SaveChangesAsync();

            return Ok(products);
        }
    }
}
