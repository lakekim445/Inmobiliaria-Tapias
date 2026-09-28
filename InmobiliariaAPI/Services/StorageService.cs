using Supabase.Storage;
using SupabaseClient = Supabase.Client;

namespace InmobiliariaAPI.Services
{
    public class StorageService
    {
        private readonly SupabaseClient _supabase;
        private readonly string _bucket;

        public StorageService(SupabaseClient supabase, IConfiguration configuration)
        {
            _supabase = supabase;
            _bucket = configuration["Supabase:Bucket"] ?? "propiedades";
        }

        public async Task<string?> SubirImagenAsync(Stream stream, string nombreArchivo, string contentType)
        {
            try
            {
                var extension = Path.GetExtension(nombreArchivo);
                var nombreUnico = $"{Guid.NewGuid()}{extension}";

                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                var bytes = memoryStream.ToArray();

                var bucket = _supabase.Storage.From(_bucket);
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
            try
            {
                var nombreArchivo = urlImagen.Split('/').Last();
                var bucket = _supabase.Storage.From(_bucket);
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