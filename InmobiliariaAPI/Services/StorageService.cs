using Supabase.Storage;
using SupabaseClient = Supabase.Client;
using Microsoft.Extensions.DependencyInjection;

namespace InmobiliariaAPI.Services
{
    public class StorageService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _bucket;

        public StorageService(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _bucket = configuration["Supabase:Bucket"] ?? "propiedades";
        }

        public async Task<string?> SubirImagenAsync(Stream stream, string nombreArchivo, string contentType)
        {
            var supabase = _serviceProvider.GetService<SupabaseClient>();
            if (supabase == null)
            {
                Console.WriteLine("⚠️ Supabase no configurado; la imagen se omite");
                return null;
            }
            try
            {
                var extension = Path.GetExtension(nombreArchivo);
                var nombreUnico = $"{Guid.NewGuid()}{extension}";

                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                var bytes = memoryStream.ToArray();

                var bucket = supabase.Storage.From(_bucket);
                await bucket.Upload(bytes, nombreUnico, new Supabase.Storage.FileOptions
                {
                    ContentType = contentType
                });

                var urlPublica = bucket.GetPublicUrl(nombreUnico);
                return urlPublica;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al subir imagen: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> EliminarImagenAsync(string urlImagen)
        {
            var supabase = _serviceProvider.GetService<SupabaseClient>();
            if (supabase == null)
            {
                Console.WriteLine("⚠️ Supabase no configurado; no se puede eliminar la imagen");
                return false;
            }
            try
            {
                var nombreArchivo = urlImagen.Split('/').Last();
                var bucket = supabase.Storage.From(_bucket);
                await bucket.Remove(nombreArchivo);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al eliminar imagen: {ex.Message}");
                return false;
            }
        }
    }
}