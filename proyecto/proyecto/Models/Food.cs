namespace proyecto.Models
{
    public class Food
    {

        public int Id { get; set; }

        public required string Name { get; set; }

        public required string tipo { get; set; }

        public List<Pedido> Pedidos { get; set; }
    }
}
