using HPlusSport.API.Data;
using HPlusSport.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        public ActionResult<IEnumerable<Product>> GetAllProducts()
        {
            var products = _shopContext.Products.ToList();
            return Ok(products);
        }

        [HttpGet, Route("{id}")]
        public ActionResult GetProduct(int id)
        {
            var product = _shopContext.Products.FirstOrDefault(x => x.Id == id);
            return Ok(product);
        }
    }
}
