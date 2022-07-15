using HPlusSport.API.Data;
using HPlusSport.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<ActionResult> GetAllProductsAsync()
        {
            var products = await _shopContext.Products.ToListAsync();
            return Ok(products);
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
