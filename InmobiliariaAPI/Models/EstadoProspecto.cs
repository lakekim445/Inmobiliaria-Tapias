namespace InmobiliariaAPI.Models
{
    public class EstadoProspecto
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public int Orden { get; set; }
        public List<Cliente>? Clientes { get; set; }
    }
}