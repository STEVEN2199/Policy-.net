using Microsoft.AspNetCore.Mvc;
using proyecto.ModelsMongoDb;
using proyecto.ServicesMongo;

namespace proyecto.ControllersMongo
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosControllerMongo: ControllerBase
    {
        private readonly ProductoServiceMongo _productoServiceMongo;

        public ProductosControllerMongo(ProductoServiceMongo productoServiceMongo)
        {
            _productoServiceMongo = productoServiceMongo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _productoServiceMongo.GetAllAsync());

        [HttpGet("{id:length(24)}")]
        public async Task<IActionResult> GetById(string id)
        {
            var producto = await _productoServiceMongo.GetByIdAsync(id);
            return producto is null ? NotFound() : Ok(producto);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductoMongo productoMongo)
        {
            await _productoServiceMongo.CreateAsync(productoMongo);
            return CreatedAtAction(nameof(GetById), new { id = productoMongo.Id }, productoMongo);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, ProductoMongo productoMongo)
        {
            var existingProduct = await _productoServiceMongo.GetByIdAsync(id);
            if (existingProduct is null) return NotFound();

            productoMongo.Id = id;
            await _productoServiceMongo.UpdateAsync(id, productoMongo);
            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var existingProduct = await _productoServiceMongo.GetByIdAsync(id);
            if (existingProduct is null) return NotFound();

            await _productoServiceMongo.DeleteAsync(id);
            return NoContent();
        }
    }
}
