using proyecto.Mappers;
using proyecto.Models;

namespace proyecto.Dtos
{
    public class FoodWithPedidoDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Tipo { get; set; }
        public List<PedidoDto> Pedidos { get; set; }

        public Food ToFood()
        {
            return new Food
            {
                Id = this.Id,
                Name = this.Name,
                tipo = this.Tipo,
                Pedidos = this.Pedidos.Select(p => p.ToPedido()).ToList()
            };
        }
    }
}
