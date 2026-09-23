using InmobiliariaAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace InmobiliariaAPI.Data
{
    public class InmobiliariaContext : DbContext
    {
        public InmobiliariaContext(DbContextOptions<InmobiliariaContext> options)
            : base(options)
        {
        }

        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<EstadoProspecto> EstadosProspecto { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Propiedad> Propiedades { get; set; }
        public DbSet<ImagenPropiedad> ImagenesPropiedad { get; set; }
        public DbSet<DisponibilidadAgente> DisponibilidadesAgente { get; set; }
        public DbSet<EstadoCita> EstadosCita { get; set; }
        public DbSet<Cita> Citas { get; set; }
        public DbSet<HistorialEstadoCita> HistorialEstadosCita { get; set; }
        public DbSet<Notificacion> Notificaciones { get; set; }
        public DbSet<SeguimientoCliente> SeguimientosCliente { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================================================
            // ROL
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
            // USUARIO
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

                entity.HasOne(u => u.Rol)
                    .WithMany(r => r.Usuarios)
                    .HasForeignKey(u => u.IdRol)
                    .HasConstraintName("fk_usuario_rol");
            });

            // ============================================================
            // ESTADO_PROSPECTO
            // ============================================================
            modelBuilder.Entity<EstadoProspecto>(entity =>
            {
                entity.ToTable("estado_prospecto");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id_estado_prospecto");
                entity.Property(e => e.Nombre).HasColumnName("nombre");
                entity.Property(e => e.Descripcion).HasColumnName("descripcion");
                entity.Property(e => e.Orden).HasColumnName("orden");
            });

            // ============================================================
            // CLIENTE
            // ============================================================
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable("cliente");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id).HasColumnName("id_cliente");
                entity.Property(c => c.NombreCompleto).HasColumnName("nombre_completo");
                entity.Property(c => c.Email).HasColumnName("email");
                entity.Property(c => c.Telefono).HasColumnName("telefono");
                entity.Property(c => c.FechaRegistro).HasColumnName("fecha_registro");
                entity.Property(c => c.IdUsuario).HasColumnName("id_usuario");
                entity.Property(c => c.IdEstadoProspecto).HasColumnName("id_estado_prospecto");

                entity.HasOne(c => c.Usuario)
                    .WithMany()
                    .HasForeignKey(c => c.IdUsuario)
                    .HasConstraintName("fk_cliente_usuario");

                entity.HasOne(c => c.EstadoProspecto)
                    .WithMany(e => e.Clientes)
                    .HasForeignKey(c => c.IdEstadoProspecto)
                    .HasConstraintName("fk_cliente_estado");
            });

            // ============================================================
            // PROPIEDAD
            // ============================================================
            modelBuilder.Entity<Propiedad>(entity =>
            {
                entity.ToTable("propiedad");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).HasColumnName("id_propiedad");
                entity.Property(p => p.Tipo).HasColumnName("tipo");
                entity.Property(p => p.Precio).HasColumnName("precio");
                entity.Property(p => p.Moneda).HasColumnName("moneda");
                entity.Property(p => p.Zona).HasColumnName("zona");
                entity.Property(p => p.Direccion).HasColumnName("direccion");
                entity.Property(p => p.Descripcion).HasColumnName("descripcion");
                entity.Property(p => p.Habitaciones).HasColumnName("habitaciones");
                entity.Property(p => p.Banos).HasColumnName("banos");
                entity.Property(p => p.SuperficieM2).HasColumnName("superficie_m2");
                entity.Property(p => p.Estado).HasColumnName("estado");
                entity.Property(p => p.FechaPublicacion).HasColumnName("fecha_publicacion");
                entity.Property(p => p.IdAgente).HasColumnName("id_agente");

                entity.Property(p => p.TipoOperacion).HasColumnName("tipo_operacion");
                entity.Property(p => p.ComisionEmpresaPorcentaje).HasColumnName("comision_empresa_porcentaje");
                entity.Property(p => p.ComisionAgentePorcentaje).HasColumnName("comision_agente_porcentaje");
                entity.Property(p => p.FechaCierre).HasColumnName("fecha_cierre");

                entity.HasOne(p => p.Agente)
                    .WithMany(u => u.Propiedades)
                    .HasForeignKey(p => p.IdAgente)
                    .HasConstraintName("fk_propiedad_agente");
            });

            // ============================================================
            // IMAGEN_PROPIEDAD
            // ============================================================
            modelBuilder.Entity<ImagenPropiedad>(entity =>
            {
                entity.ToTable("imagen_propiedad");
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Id).HasColumnName("id_imagen");
                entity.Property(i => i.UrlImagen).HasColumnName("url_imagen");
                entity.Property(i => i.Descripcion).HasColumnName("descripcion");
                entity.Property(i => i.EsPrincipal).HasColumnName("es_principal");
                entity.Property(i => i.IdPropiedad).HasColumnName("id_propiedad");

                entity.HasOne(i => i.Propiedad)
                    .WithMany(p => p.Imagenes)
                    .HasForeignKey(i => i.IdPropiedad)
                    .HasConstraintName("fk_imagen_propiedad");
            });

            // ============================================================
            // DISPONIBILIDAD_AGENTE
            // ============================================================
            modelBuilder.Entity<DisponibilidadAgente>(entity =>
            {
                entity.ToTable("disponibilidad_agente");
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Id).HasColumnName("id_disponibilidad");
                entity.Property(d => d.DiaSemana).HasColumnName("dia_semana");
                entity.Property(d => d.HoraInicio).HasColumnName("hora_inicio");
                entity.Property(d => d.HoraFin).HasColumnName("hora_fin");
                entity.Property(d => d.Activo).HasColumnName("activo");
                entity.Property(d => d.IdAgente).HasColumnName("id_agente");

                entity.HasOne(d => d.Agente)
                    .WithMany(u => u.Disponibilidades)
                    .HasForeignKey(d => d.IdAgente)
                    .HasConstraintName("fk_disponibilidad_agente");
            });

            // ============================================================
            // ESTADO_CITA
            // ============================================================
            modelBuilder.Entity<EstadoCita>(entity =>
            {
                entity.ToTable("estado_cita");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id_estado");
                entity.Property(e => e.NombreEstado).HasColumnName("nombre_estado");
                entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            });

            // ============================================================
            // CITA
            // ============================================================
            modelBuilder.Entity<Cita>(entity =>
            {
                entity.ToTable("cita");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id).HasColumnName("id_cita");
                entity.Property(c => c.FechaCita).HasColumnName("fecha_cita");
                entity.Property(c => c.HoraInicio).HasColumnName("hora_inicio");
                entity.Property(c => c.HoraFin).HasColumnName("hora_fin");
                entity.Property(c => c.FechaSolicitud).HasColumnName("fecha_solicitud");
                entity.Property(c => c.Observaciones).HasColumnName("observaciones");
                entity.Property(c => c.IdCliente).HasColumnName("id_cliente");
                entity.Property(c => c.IdPropiedad).HasColumnName("id_propiedad");
                entity.Property(c => c.IdAgente).HasColumnName("id_agente");
                entity.Property(c => c.IdDisponibilidad).HasColumnName("id_disponibilidad");
                entity.Property(c => c.IdEstadoCita).HasColumnName("id_estado_cita");

                entity.HasOne(c => c.Cliente)
                    .WithMany(cl => cl.Citas)
                    .HasForeignKey(c => c.IdCliente)
                    .HasConstraintName("fk_cita_cliente");

                entity.HasOne(c => c.Propiedad)
                    .WithMany(p => p.Citas)
                    .HasForeignKey(c => c.IdPropiedad)
                    .HasConstraintName("fk_cita_propiedad");

                entity.HasOne(c => c.Agente)
                    .WithMany(u => u.CitasComoAgente)
                    .HasForeignKey(c => c.IdAgente)
                    .HasConstraintName("fk_cita_agente");

                entity.HasOne(c => c.Disponibilidad)
                    .WithMany(d => d.Citas)
                    .HasForeignKey(c => c.IdDisponibilidad)
                    .HasConstraintName("fk_cita_disponibilidad");

                entity.HasOne(c => c.EstadoCita)
                    .WithMany(e => e.Citas)
                    .HasForeignKey(c => c.IdEstadoCita)
                    .HasConstraintName("fk_cita_estado");
            });

            // ============================================================
            // HISTORIAL_ESTADO_CITA
            // ============================================================
            modelBuilder.Entity<HistorialEstadoCita>(entity =>
            {
                entity.ToTable("historial_estado_cita");
                entity.HasKey(h => h.Id);
                entity.Property(h => h.Id).HasColumnName("id_historial");
                entity.Property(h => h.EstadoAnterior).HasColumnName("estado_anterior");
                entity.Property(h => h.EstadoNuevo).HasColumnName("estado_nuevo");
                entity.Property(h => h.Motivo).HasColumnName("motivo");
                entity.Property(h => h.FechaCambio).HasColumnName("fecha_cambio");
                entity.Property(h => h.IdCita).HasColumnName("id_cita");
                entity.Property(h => h.IdUsuario).HasColumnName("id_usuario");

                entity.HasOne(h => h.Cita)
                    .WithMany()
                    .HasForeignKey(h => h.IdCita)
                    .HasConstraintName("fk_hist_cita");

                entity.HasOne(h => h.Usuario)
                    .WithMany()
                    .HasForeignKey(h => h.IdUsuario)
                    .HasConstraintName("fk_hist_usuario");
            });

            // ============================================================
            // NOTIFICACION
            // ============================================================
            modelBuilder.Entity<Notificacion>(entity =>
            {
                entity.ToTable("notificacion");
                entity.HasKey(n => n.Id);
                entity.Property(n => n.Id).HasColumnName("id_notificacion");
                entity.Property(n => n.Tipo).HasColumnName("tipo");
                entity.Property(n => n.Mensaje).HasColumnName("mensaje");
                entity.Property(n => n.Leida).HasColumnName("leida");
                entity.Property(n => n.FechaEnvio).HasColumnName("fecha_envio");
                entity.Property(n => n.FechaLectura).HasColumnName("fecha_lectura");
                entity.Property(n => n.IdUsuario).HasColumnName("id_usuario");
                entity.Property(n => n.IdCita).HasColumnName("id_cita");
                entity.Property(n => n.IdPropiedad).HasColumnName("id_propiedad");

                entity.HasOne(n => n.Usuario)
                    .WithMany()
                    .HasForeignKey(n => n.IdUsuario)
                    .HasConstraintName("fk_notif_usuario");

                entity.HasOne(n => n.Cita)
                    .WithMany()
                    .HasForeignKey(n => n.IdCita)
                    .HasConstraintName("fk_notif_cita");

                entity.HasOne(n => n.Propiedad)
                    .WithMany()
                    .HasForeignKey(n => n.IdPropiedad)
                    .HasConstraintName("fk_notif_propiedad");
            });

            // ============================================================
            // SEGUIMIENTO_CLIENTE
            // ============================================================
            modelBuilder.Entity<SeguimientoCliente>(entity =>
            {
                entity.ToTable("seguimiento_cliente");
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Id).HasColumnName("id_seguimiento");
                entity.Property(s => s.FechaInteraccion).HasColumnName("fecha_interaccion");
                entity.Property(s => s.TipoContacto).HasColumnName("tipo_contacto");
                entity.Property(s => s.Descripcion).HasColumnName("descripcion");
                entity.Property(s => s.ProximaAccion).HasColumnName("proxima_accion");
                entity.Property(s => s.FechaProxima).HasColumnName("fecha_proxima");
                entity.Property(s => s.IdCliente).HasColumnName("id_cliente");
                entity.Property(s => s.IdAgente).HasColumnName("id_agente");
                entity.Property(s => s.IdPropiedad).HasColumnName("id_propiedad");

                entity.HasOne(s => s.Cliente)
                    .WithMany()
                    .HasForeignKey(s => s.IdCliente)
                    .HasConstraintName("fk_seg_cliente");

                entity.HasOne(s => s.Agente)
                    .WithMany()
                    .HasForeignKey(s => s.IdAgente)
                    .HasConstraintName("fk_seg_agente");

                entity.HasOne(s => s.Propiedad)
                    .WithMany()
                    .HasForeignKey(s => s.IdPropiedad)
                    .HasConstraintName("fk_seg_propiedad");
            });
        }
    }
}