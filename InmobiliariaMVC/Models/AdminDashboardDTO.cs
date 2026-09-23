namespace InmobiliariaMVC.Models
{
    public class AdminDashboardDTO
    {
        public int TotalUsuarios { get; set; }
        public int TotalAgentes { get; set; }
        public int TotalClientes { get; set; }

        public int TotalPropiedades { get; set; }
        public int PropiedadesDisponibles { get; set; }
        public int PropiedadesReservadas { get; set; }
        public int PropiedadesVendidas { get; set; }
        public int PropiedadesAlquiladas { get; set; }
        public int PropiedadesAnticretico { get; set; }

        public int TotalCitas { get; set; }
        public int CitasPendientes { get; set; }
        public int CitasConfirmadas { get; set; }
        public int CitasCompletadas { get; set; }
        public int CitasCanceladas { get; set; }

        public decimal ComisionTotalEmpresa { get; set; }
        public decimal ComisionTotalAgentes { get; set; }
        public decimal GananciaNetaEmpresa { get; set; }
    }
}