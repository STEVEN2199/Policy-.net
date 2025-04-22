namespace proyecto.Models
{
    public class Pedido
    {

        public int Id { get; set; }

        public required string Name { get; set; }

        public required string Description { get; set; }

        //public Person Person { get; set; }

        public int? PersonaId { get; set; }

        public Person Persona { get; set; }

        public List<Food> Foods { get; set; }
    }
}
