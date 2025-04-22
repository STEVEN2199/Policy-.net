namespace proyecto.Models
{
    public class Person
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Age { get; set; }

        public List<Pedido> Pedidos { get; set; }
        
    }
}
