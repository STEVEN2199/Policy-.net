using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using proyecto.Context;
using proyecto.Dtos;
using proyecto.Models;

namespace proyecto.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidoesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PedidoesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Pedidoes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pedido>>> GetPedidos()
        {
            return await _context.Pedidos.ToListAsync();
        }

        // GET: api/Pedidoes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Pedido>> GetPedido(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);

            if (pedido == null)
            {
                return NotFound();
            }

            return pedido;
        }

        // PUT: api/Pedidoes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPedido(int id, Pedido pedido)
        {
            if (id != pedido.Id)
            {
                return BadRequest();
            }

            _context.Entry(pedido).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PedidoExists(id))
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

        // POST: api/Pedidoes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("transaccionPost")]
        public async Task<ActionResult<Pedido>> PostPedido(Pedido pedido)
        {
            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPedido", new { id = pedido.Id }, pedido);
        }

        // DELETE: api/Pedidoes/5
        [HttpDelete("transaccionDelete{id}")]
        public async Task<IActionResult> DeletePedido(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null)
            {
                return NotFound();
            }

            _context.Pedidos.Remove(pedido);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PedidoExists(int id)
        {
            return _context.Pedidos.Any(e => e.Id == id);
        }


        // POST: api/Pedidoes
        [HttpPost]
        public async Task<ActionResult<Pedido>> PostPedidoTransaction(Pedido pedido)
        {
            // Iniciar una transacción
            using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Agregar pedido
                    _context.Pedidos.Add(pedido);
                    await _context.SaveChangesAsync();

                    // Ejemplo: asociar un alimento al pedido
                    if (pedido.Foods != null && pedido.Foods.Any())
                    {
                        foreach (var food in pedido.Foods)
                        {
                            var existingFood = await _context.Foods.FindAsync(food.Id);
                            if (existingFood != null)
                            {
                                pedido.Foods.Add(existingFood);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();

                    // Confirmar transacción
                    await transaction.CommitAsync();

                    return CreatedAtAction("GetPedido", new { id = pedido.Id }, pedido);
                }
                catch (Exception)
                {
                    // Revertir cambios si ocurre un error
                    await transaction.RollbackAsync();
                    return StatusCode(500, "Error al crear el pedido. Se han revertido los cambios.");
                }
            }
        }

        // DELETE: api/Pedidoes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePedidoTransaction(int id)
        {
            // Iniciar una transacción
            using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var pedido = await _context.Pedidos.FindAsync(id);
                    if (pedido == null)
                    {
                        return NotFound();
                    }

                    _context.Pedidos.Remove(pedido);
                    await _context.SaveChangesAsync();

                    // Confirmar transacción
                    await transaction.CommitAsync();

                    return NoContent();
                }
                catch (Exception)
                {
                    // Revertir cambios si ocurre un error
                    await transaction.RollbackAsync();
                    return StatusCode(500, "Error al eliminar el pedido. Se han revertido los cambios.");
                }
            }
        }


        [HttpPost("CreatePedidoDto")]
        public async Task<IActionResult> CreatePedido([FromBody] PedidoDto pedidoDto)
        {
            if (pedidoDto == null)
            {
                return BadRequest(new { message = "El cuerpo de la solicitud no puede ser nulo." });
            }

            try
            {
                // Mapear el DTO al modelo de Pedido
                var pedido = new Pedido
                {
                    Name = pedidoDto.Name,
                    Description = pedidoDto.Description
                };

                // Agregar el Pedido a la base de datos
                _context.Pedidos.Add(pedido);
                await _context.SaveChangesAsync();

                // Retornar el Pedido creado
                return CreatedAtAction(nameof(GetPedido), new { id = pedido.Id }, pedido);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocurrió un error al crear el Pedido.", error = ex.Message });
            }
        }

    }
}
