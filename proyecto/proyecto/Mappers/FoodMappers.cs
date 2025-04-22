using proyecto.Dtos;
using proyecto.Models;

namespace proyecto.Mappers
{
    public static class FoodMappers
    {
        public static FoodDto ToFoodDto(this Food foodModel)
        {
            return new FoodDto
            {
                Id = foodModel.Id,
                Name = foodModel.Name,
                tipo = foodModel.tipo,
            };

        }

        public static Food ToFoodDto2(this FoodDto foodDto)
        {
            return new Food
            {
                // No asignes el Id aquí
                // Id = foodDto.Id,
                Name = foodDto.Name,
                tipo = foodDto.tipo,
            };

        }

        public static Food ToFood(this FoodWithPedidoDto foodWithPedidoDto)
        {
            return new Food
            {
                Id = foodWithPedidoDto.Id,
                Name = foodWithPedidoDto.Name,
                tipo = foodWithPedidoDto.Tipo,
                Pedidos = foodWithPedidoDto.Pedidos.Select(p => p.ToPedido()).ToList()
            };
        }

        public static FoodWithPedidoDto ToFoodWithPedidoDto(this Food food)
        {
            return new FoodWithPedidoDto
            {
                Id = food.Id,
                Name = food.Name,
                Tipo = food.tipo,
                Pedidos = food.Pedidos.Select(p => p.ToPedidoDto()).ToList()
            };
        }


    }

    
}
