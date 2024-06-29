using BLC.Manager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private ProductManager _manager;

        public ProductsController(ProductManager manager)
        {
            _manager = manager;
        }

        [HttpGet("Products")]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                var products = await _manager.GetAllProductsAsync();

                return Ok(products);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpGet("Product")]
        public async Task<IActionResult> GetProductById(int id)
        {
            try
            {
                var product = await _manager.GetProductByIdAsync(id);

                return Ok(product);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost("Product")]
        public async Task<IActionResult> AddProduct([FromBody] Product product)
        {
            try
            {
                var addedProduct = await _manager.AddProductAsync(product);

                return Ok(addedProduct);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPut("Product")]
        public async Task<IActionResult> UpdateProduct([FromBody] Product product)
        {
            try
            {
                var updatedProduct = await _manager.UpdateProductAsync(product);

                return Ok(updatedProduct);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpDelete("Product")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                await _manager.DeleteProductAsync(id);

                return Ok();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
