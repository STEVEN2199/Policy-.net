using proyecto.Dtos;
using proyecto.Models;

namespace proyecto.Mappers
{
    public static class PedidoMappers
    {
        public static PedidoDto ToPedidoDto(this Pedido pedido)
        {
            return new PedidoDto
            {
                Id = pedido.Id,
                Name = pedido.Name,
                Description = pedido.Description
            };
        }

        public static Pedido ToPedido(this PedidoDto pedidoDto)
        {
            return new Pedido
            {
                Id = pedidoDto.Id,
                Name = pedidoDto.Name,
                Description = pedidoDto.Description
            };
        }
    }
}
