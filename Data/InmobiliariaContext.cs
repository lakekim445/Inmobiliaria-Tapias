using Microsoft.EntityFrameworkCore;
using InmobiliariaMVC.Models;

namespace InmobiliariaMVC.Data
{
    public class InmobiliariaContext : DbContext
    {
        public InmobiliariaContext(DbContextOptions<InmobiliariaContext> options)
            : base(options)
        {
        }

        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================================================
            // MAPEO DE TABLA: ROL
            // ============================================================
            modelBuilder.Entity<Rol>(entity =>
            {
                entity.ToTable("rol");
                entity.HasKey(r => r.Id);

                entity.Property(r => r.Id).HasColumnName("id_rol");
                entity.Property(r => r.NombreRol).HasColumnName("nombre_rol");
                entity.Property(r => r.Descripcion).HasColumnName("descripcion");
            });

            // ============================================================
            // MAPEO DE TABLA: USUARIO
            // ============================================================
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("usuario");
                entity.HasKey(u => u.Id);

                entity.Property(u => u.Id).HasColumnName("id_usuario");
                entity.Property(u => u.NombreCompleto).HasColumnName("nombre_completo");
                entity.Property(u => u.Email).HasColumnName("email");
                entity.Property(u => u.PasswordHash).HasColumnName("password_hash");
                entity.Property(u => u.Telefono).HasColumnName("telefono");
                entity.Property(u => u.FechaRegistro).HasColumnName("fecha_registro");
                entity.Property(u => u.Activo).HasColumnName("activo");
                entity.Property(u => u.IdRol).HasColumnName("id_rol");

                // Relación con Rol
                entity.HasOne(u => u.Rol)
                    .WithMany(r => r.Usuarios)
                    .HasForeignKey(u => u.IdRol)
                    .HasConstraintName("fk_usuario_rol");
            });
        }
    }
}