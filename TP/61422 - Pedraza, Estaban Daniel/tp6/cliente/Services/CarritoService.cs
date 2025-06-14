using System.Net.Http.Json;
using cliente.Models;

namespace cliente.Services
{
    public class CarritoService
    {
        private readonly HttpClient _http;

        public CarritoService(HttpClient http)
        {
            _http = http;
        }

        public async Task<Carrito> CrearCarritoAsync()
        {
            var response = await _http.PostAsync("api/carritos", null);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Carrito>();
        }

        public async Task<Carrito> ObtenerCarritoAsync(string carritoId)
        {
            return await _http.GetFromJsonAsync<Carrito>($"api/carritos/{carritoId}");
        }

        public async Task<Carrito> AgregarProductoAsync(string carritoId, int productoId, int cantidad)
        {
            var response = await _http.PutAsync(
                $"api/carritos/{carritoId}/{productoId}?cantidad={cantidad}", null);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Carrito>();
        }

        public async Task EliminarProductoAsync(string carritoId, int productoId)
        {
            var response = await _http.DeleteAsync($"api/carritos/{carritoId}/{productoId}");
            response.EnsureSuccessStatusCode();
        }
    }
}