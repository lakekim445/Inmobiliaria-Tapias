using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace InmobiliariaMVC.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiService(IHttpClientFactory httpClientFactory,
                          IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("InmobiliariaAPI");
            _httpContextAccessor = httpContextAccessor;
        }

        private void AgregarToken()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT");
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<T?> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                AgregarToken();

                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ POST {endpoint} → {response.StatusCode}: {errorContent}");
                    return default;
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 EXCEPCIÓN en POST {endpoint}: {ex.Message}");
                return default;
            }
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                AgregarToken();

                var fullUrl = _httpClient.BaseAddress + endpoint;
                Console.WriteLine($"📡 GET {fullUrl}");

                var response = await _httpClient.GetAsync(endpoint);

                Console.WriteLine($"📥 Status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Error: {errorContent}");
                    return default;
                }

                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"✅ JSON recibido (primeros 200 chars): {json.Substring(0, Math.Min(200, json.Length))}");

                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 EXCEPCIÓN en GET {endpoint}: {ex.Message}");
                Console.WriteLine($"   Stack: {ex.StackTrace}");
                return default;
            }
        }

        public async Task<bool> PutAsync(string endpoint, object data)
        {
            try
            {
                AgregarToken();

                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync(endpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ PUT {endpoint} → {response.StatusCode}: {errorContent}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 EXCEPCIÓN en PUT {endpoint}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                AgregarToken();

                var response = await _httpClient.DeleteAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ DELETE {endpoint} → {response.StatusCode}: {errorContent}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 EXCEPCIÓN en DELETE {endpoint}: {ex.Message}");
                return false;
            }
        }

        public async Task<T?> PostFormDataAsync<T>(string endpoint, MultipartFormDataContent content)
        {
            try
            {
                AgregarToken();

                var response = await _httpClient.PostAsync(endpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ POST FormData {endpoint} → {response.StatusCode}: {errorContent}");
                    return default;
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 EXCEPCIÓN en POST FormData {endpoint}: {ex.Message}");
                return default;
            }
        }
    }
}