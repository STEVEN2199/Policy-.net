using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using proyecto.Context;
using proyecto.Dtos;
using proyecto.Interfaces;
using proyecto.Mappers;
using proyecto.Models;
using proyecto.OtherObjects;
using proyecto.Services;
using StackExchange.Redis; // Importar la librería de Redis
using AspNetCoreRateLimit;
using Microsoft.Extensions.Caching.Memory;
using LazyCache;

namespace proyecto.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoodsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IFoodDAO _foodService;
        private readonly IDatabase _redisCache;
        private readonly IMemoryCache _cache;
        private readonly IAppCache _cacheLazy;

        public FoodsController(IFoodDAO foodService, AppDbContext context, 
            IConfiguration configuration, IMemoryCache cache, IAppCache cacheLazy)
        {
            _foodService = foodService;
            _context = context;
            _cache = cache;
            _cacheLazy = cacheLazy;

            // Conectar con Redis
            var redis = ConnectionMultiplexer.Connect(new ConfigurationOptions
            {
                EndPoints = { "localhost:6379" }, // Cambia esto si estás usando otro host o puerto
                Password = "STEVEN2199", // Reemplaza con la contraseña correcta
                AbortOnConnectFail = false
            });
            _redisCache = redis.GetDatabase();
        }


        private async Task<PagedFoodResult> GetBooksFilter(string? term, string? sort, int page, int limit)
        {
            IQueryable<Food> foods;
            if (string.IsNullOrWhiteSpace(term))
                foods = _context.Foods;
            else
            {
                term = term.Trim().ToLower();
                // filtering records with Name or tipo
                foods = _context
                    .Foods
                    .Where(b => b.Name.ToLower().Contains(term)
                    || b.tipo.ToLower().Contains(term)
                    );
            }

            // sorting
            // sort=Name,-year
            // (arrange order title ascending and year descending
            if (!string.IsNullOrWhiteSpace(sort))
            {
                var sortFields = sort.Split(','); // ['Name','-year']
                StringBuilder orderQueryBuilder = new StringBuilder();
                // using reflection to get properties of Food
                // propertyInfo= [Id,Name,tipo,Pedidos] 
                PropertyInfo[] propertyInfo = typeof(Food).GetProperties();


                foreach (var field in sortFields)
                {
                    // iteration 1, field=Name
                    // iteration 2, field=-year
                    string sortOrder = "ascending";
                    // iteration 1, sortField= Name
                    // iteration 2, sortField=-year
                    var sortField = field.Trim();
                    if (sortField.StartsWith("-"))
                    {
                        sortField = sortField.TrimStart('-');
                        sortOrder = "descending";
                    }
                    // property = 'Name'
                    // property = 'Year'
                    var property = propertyInfo.FirstOrDefault(a => a.Name.Equals(sortField, StringComparison.OrdinalIgnoreCase));
                    if (property == null)
                        continue;
                    // orderQueryBuilder= "Name ascending,Year descending, "
                    // it have trailing , and whitespace
                    orderQueryBuilder.Append($"{property.Name.ToString()} {sortOrder}, ");
                }
                // remove trailing , and whitespace here
                // orderQuery = ""Title ascending,Year descending"
                string orderQuery = orderQueryBuilder.ToString().TrimEnd(',', ' ');
                if (!string.IsNullOrWhiteSpace(orderQuery))
                    // use System.Linq.Dynamic.Core namespace for this
                    foods = foods.OrderBy(orderQuery);
                else
                    foods = foods.OrderBy(a => a.Id);
            }

            // apply pagination

            // totalCount=101 ,page=1,limit=10 (10 record per page)
            var totalCount = await _context.Foods.CountAsync();  //101
                                                                     // 101/10 = 10.1->11 
            var totalPages = (int)Math.Ceiling(totalCount / (double)limit);

            // page=1 , skip=(1-1)*10=0, take=10
            // page=2 , skip=(2-1)*10=10, take=10
            // page=3 , skip=(3-1)*10=20, take=10
            var pagedBooks = await foods.Skip((page - 1) * limit).Take(limit).ToListAsync();

            var pagedBookData = new PagedFoodResult
            {
                Foods = pagedBooks,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
            return pagedBookData;
        }


        //METODO FOOD CON PAGINATION, SORT, FILTER
        [HttpGet("paged")]
        public async Task<IActionResult> GetFoodsP(string? term, string? sort, int page = 1, int limit = 3)
        {
            var bookResult = await GetBooksFilter(term, sort, page, limit);

            // Add pagination headers to the response
            Response.Headers.Add("X-Total-Count", bookResult.TotalCount.ToString());
            Response.Headers.Add("X-Total-Pages", bookResult.TotalPages.ToString());
            return Ok(bookResult.Foods);
        }

        // GET: api/Foods
        [HttpGet]
        //[Authorize(Roles = StaticUserRoles.ADMIN)]
        public async Task<ActionResult<IEnumerable<Food>>> GetFoods()
        {
            return Ok(await _foodService.GetFoodsAsync());
        }

        // GET: api/Foods/5
        [HttpGet("{id}")]
        //[Authorize(Roles = StaticUserRoles.ADMIN)]
        public async Task<ActionResult<Food>> GetFood(int id)
        {
            var food = await _foodService.GetFoodByIdAsync(id);
            if (food == null) return NotFound();
            return Ok(food);
        }

        // PUT: api/Foods/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFood(int id, Food food)
        {
            if (id != food.Id) return BadRequest();

            var result = await _foodService.UpdateFoodAsync(food);
            if (!result) return NotFound();
            return NoContent();
        }

        // POST: api/Foods
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Food>> PostFood(Food food)
        {
            var result = await _foodService.AddFoodAsync(food);
            if (!result) return BadRequest();
            return CreatedAtAction(nameof(GetFood), new { id = food.Id }, food);
        }

        // DELETE: api/Foods/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFood(int id)
        {
            var food = await _context.Foods.FindAsync(id);
            if (food == null)
            {
                return NotFound();
            }

            await _foodService.DeleteFoodAsync(food); // Asegúrate de pasar el objeto Food, no el ID
            return NoContent();

        }

        private bool FoodExists(int id)
        {
            return _context.Foods.Any(e => e.Id == id);
        }

        [HttpGet("dto")]
        public async Task <IActionResult> GetAllDto()
        {
            return Ok(_foodService.GetAllDto());
        }

        [HttpPost("create")]
        public IActionResult CreateFood([FromBody] FoodDto foodDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var foodModel = foodDto.ToFoodDto2();
            _context.Foods.Add(foodModel);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetFood), new { id = foodModel.Id }, foodModel);
        }

        [HttpPost("foodPedido")]
        public IActionResult PostFoodWithPedidoDto(FoodWithPedidoDto foodWithPedidoDto)
        {
            var food = foodWithPedidoDto.ToFood(); // Usar el método de conversión aquí
            _context.Foods.Add(food);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetFood), new { id = food.Id }, food.ToFoodWithPedidoDto());
        }


        [HttpPut("updatefood/{id}")]
        public async Task<IActionResult> UpdateFood(int id, FoodWithPedidoDto foodWithPedidoDto)
        {
            if (id != foodWithPedidoDto.Id)
            {
                return BadRequest();
            }

            var food = foodWithPedidoDto.ToFood();
            _context.Entry(food).State = EntityState.Modified;

            try
            {
                _context.Update(food); // Corregido
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FoodExists(id))
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

        [HttpGet("PagedResult")]
        public async Task<Models.PagedResult<Food>> PaginadoFood(string searchTerm, int PageSize, int PageNumber)
        {
            var query = _context.Foods.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(e => e.Name.Contains(searchTerm) || e.tipo.Contains(searchTerm));
            }

            var totalRecords = await query.CountAsync();

            var Foods = await query.Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return new Models.PagedResult<Food>
            {
                TotalRecords = totalRecords,
                PageSize = PageSize,
                PageNumber = PageNumber,
                Items = Foods
            };

        }

        [HttpPost("CreateFoodAndDeletePedido")]
        public async Task<IActionResult> CreateFoodAndDeletePedido(Food food, int pedidoId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Crear Food
                _context.Foods.Add(food);
                await _context.SaveChangesAsync();

                // Eliminar Pedido
                var pedido = await _context.Pedidos.FindAsync(pedidoId);
                if (pedido == null)
                {
                    return NotFound($"Pedido with ID {pedidoId} not found.");
                }

                _context.Pedidos.Remove(pedido);
                await _context.SaveChangesAsync();

                // Confirmar la transacción
                await transaction.CommitAsync();

                return Ok(new { message = "Food created and Pedido deleted successfully.", food, pedidoId });
            }
            catch (Exception ex)
            {
                // Revertir transacción si ocurre algún error
                await transaction.RollbackAsync();
                return BadRequest(new { message = "Error during transaction.", error = ex.Message });
            }
        }


        [HttpPut("UpdateFoodAndPedido")]
        public async Task<IActionResult> UpdateFoodAndPedido([FromBody] UpdateFoodAndPedidoDto updateDto)
        {
            var foodDto = updateDto.FoodDto;
            var pedidoDto = updateDto.PedidoDto;

            // Realiza la operación en una transacción
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Actualizar el Food
                var food = await _context.Foods.FindAsync(foodDto.Id);
                if (food == null)
                {
                    return NotFound($"Food con ID {foodDto.Id} no encontrado.");
                }

                food.Name = foodDto.Name;
                food.tipo = foodDto.tipo;

                _context.Foods.Update(food);

                // Actualizar el Pedido
                var pedido = await _context.Pedidos.FindAsync(pedidoDto.Id);
                if (pedido == null)
                {
                    return NotFound($"Pedido con ID {pedidoDto.Id} no encontrado.");
                }

                pedido.Name = pedidoDto.Name;
                pedido.Description = pedidoDto.Description;

                _context.Pedidos.Update(pedido);

                // Guarda los cambios y confirma la transacción
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Food y Pedido actualizados correctamente." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Error al actualizar.", error = ex.Message });
            }
        }



        [HttpDelete("DeleteFoodsAndPedidos")]
        public async Task<IActionResult> DeleteFoodsAndPedidos([FromBody] DeleteFoodsAndPedidosDto deleteDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Eliminar Foods
                if (deleteDto.FoodIds != null && deleteDto.FoodIds.Any())
                {
                    var foodsToDelete = await _context.Foods
                        .Where(f => deleteDto.FoodIds.Contains(f.Id))
                        .ToListAsync();

                    if (!foodsToDelete.Any())
                    {
                        return NotFound("No se encontraron los Foods especificados para eliminar.");
                    }

                    _context.Foods.RemoveRange(foodsToDelete);
                }

                // Eliminar Pedidos
                if (deleteDto.PedidoIds != null && deleteDto.PedidoIds.Any())
                {
                    var pedidosToDelete = await _context.Pedidos
                        .Where(p => deleteDto.PedidoIds.Contains(p.Id))
                        .ToListAsync();

                    if (!pedidosToDelete.Any())
                    {
                        return NotFound("No se encontraron los Pedidos especificados para eliminar.");
                    }

                    _context.Pedidos.RemoveRange(pedidosToDelete);
                }

                // Guardar cambios y confirmar transacción
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Foods y Pedidos eliminados correctamente." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Error al eliminar.", error = ex.Message });
            }
        }



        // Nuevo endpoint: Obtener foods y almacenarlas en Redis
        [HttpGet("cache/foods")]
        public async Task<IActionResult> GetFoodsWithCache()
        {
            const string cacheKey = "foods_cache";

            // Verificar si los datos ya están en la caché de Redis
            var cachedFoods = await _redisCache.StringGetAsync(cacheKey);
            if (!cachedFoods.IsNullOrEmpty)
            {
                // Retornar los datos desde la caché
                var foodsFromCache = System.Text.Json.JsonSerializer.Deserialize<List<FoodDto>>(cachedFoods);
                return Ok(new { source = "cache", data = foodsFromCache });
            }

            // Si no están en caché, obtener los datos desde la base de datos
            var foods = await _context.Foods
                .Select(food => new FoodDto
                {
                    Id = food.Id,
                    Name = food.Name,
                    tipo = food.tipo
                })
                .ToListAsync();

            // Almacenar los datos en la caché de Redis
            var serializedFoods = System.Text.Json.JsonSerializer.Serialize(foods);
            await _redisCache.StringSetAsync(cacheKey, serializedFoods, TimeSpan.FromMinutes(10)); // Expiración de 10 minutos

            return Ok(new { source = "database", data = foods });
        }


        //STORE PROCEDURE
        [HttpGet("foodSP")]
        public async Task<IActionResult> GetAllFoodsSP()
        {
            try
            {
                // Ejecuta el procedimiento almacenado para obtener todos los Foods
                var foods = await _context.Foods.FromSqlRaw("EXEC sp_GetAllFoods2")
                    .AsNoTracking()  // Optimización para solo lectura
                    .ToListAsync();

                return Ok(foods);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("CreateFoodSP")]
        public async Task<IActionResult> CreateFoodSP([FromBody] FoodDto foodDto)
        {
            if (foodDto == null)
            {
                return BadRequest("Food data is null.");
            }

            try
            {
                // Ejecuta el procedimiento almacenado para crear un nuevo Food
                var foodId = await _context.Database.ExecuteSqlRawAsync("EXEC sp_CreateFood @Name = {0}, @Tipo = {1} ; SELECT SCOPE_IDENTITY();", foodDto.Name, foodDto.tipo);

                return CreatedAtAction(nameof(GetFoodByIdSP), new { id = foodId }, foodDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("UpdateFoodSP{id}")]
        public async Task<IActionResult> UpdateFoodSP(int id, [FromBody] FoodDto foodDto)
        {
            if (foodDto == null || id != foodDto.Id)
            {
                return BadRequest("Food data is invalid.");
            }

            try
            {
                // Ejecuta el procedimiento almacenado para actualizar el Food
                var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_UpdateFood @Id = {0}, @Name = {1}, @Tipo = {2}",
                    foodDto.Id, foodDto.Name, foodDto.tipo);

                if (rowsAffected == 0)
                {
                    return NotFound($"Food with ID {id} not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetFoodByIdSP/{id}")]
        public async Task<IActionResult> GetFoodByIdSP(int id)
        {
            try
            {
                // Ejecuta el procedimiento almacenado para obtener un Food por su ID
                var foodList = await _context.Foods
                    .FromSqlRaw("EXEC sp_GetFoodById2 @Id = {0}", id)
                    .AsNoTracking()  // Mejorar rendimiento para solo lectura
                    .ToListAsync();

                // Si no se encuentra ningún food
                if (foodList == null)
                {
                    return NotFound($"Food with ID {id} not found.");
                }

                // Mapear el primer resultado a un DTO
                var foodDto = foodList
                    .Select(f => new FoodDto
                    {
                        Id = f.Id,
                        Name = f.Name,
                        tipo = f.tipo
                    })
                    .FirstOrDefault();

                return Ok(foodDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        //PRUEBA DE POLICY
        [Authorize(Policy = "AdminAndAdultPolicy")]
        [HttpGet("admin-section")]
        public IActionResult testPolicy()
        {
            return Ok("Bienvenido, eres un admin mayor de edad.");
        }

        //ENDPOINT CON RATE LIMITING
        [HttpGet("limited")]
        [EnableRateLimiting("MiPoliticaRateLimit")] // Aplicar Rate Limiting
        public async Task<ActionResult<IEnumerable<Food>>> GetFoodsWithRateLimit()
        {
            return Ok(await _foodService.GetFoodsAsync());
        }

        [HttpGet("getFoodsMemoryCache")]
        public async Task<IActionResult> GetFoodMemoryCache()
        {
            string cacheKey = "foodsList";

            if (!_cache.TryGetValue(cacheKey, out List<Food> foods))
            {
                // Obtener datos de la base de datos en paralelo sin bloquear el hilo principal
                foods = (await Task.Run(async () => await _foodService.GetFoodsAsyncUpgrade())).ToList();

                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                    SlidingExpiration = TimeSpan.FromMinutes(2),
                    Priority = CacheItemPriority.High // Prioridad alta para evitar que se elimine fácilmente
                };

                // Guardamos los datos en caché
                _cache.Set(cacheKey, foods, cacheEntryOptions);
            }

            return Ok(foods);
        }

        [HttpDelete("clearFoodsMemoryCache")]
        public IActionResult ClearCache()
        {
            _cache.Remove("foodsList");
            return Ok("Caché eliminada correctamente.");
        }


        [HttpGet("getFoodsMemoryCacheLazy")]
        public async Task<IActionResult> GetFoodMemoryCacheLazy()
        {
            string cacheKey = "foodsList";

            var foods = await _cacheLazy.GetOrAddAsync(cacheKey, async () =>
            {
                var foodList = await _foodService.GetFoodsAsync();
                return foodList.ToList(); // Convertir a List para evitar problemas de conversión
            }, TimeSpan.FromMinutes(5)); // Cachea por 5 minutos

            return Ok(foods);
        }
    }
}
