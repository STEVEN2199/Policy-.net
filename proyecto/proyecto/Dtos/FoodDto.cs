using proyecto.Models;

namespace proyecto.Dtos
{
    public class FoodDto
    {
        public int Id { get; set; }

        public required string Name { get; set; } = string.Empty;

        public required string tipo { get; set; } = string.Empty;

    }
}
