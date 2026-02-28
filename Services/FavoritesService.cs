using Microsoft.JSInterop;
using System.Text.Json;

namespace LibrairieSteam.Services
{
    public class FavoritesService
    {
        private readonly IJSRuntime _jsRuntime;
        
        public FavoritesService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }
        
        public async Task<List<int>> GetFavoritesAsync(string steamId)
        {
            var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", $"favorites_{steamId}");
            
            if (string.IsNullOrEmpty(json))
                return new List<int>();
            
            return JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();
        }
        
        public async Task ToggleFavoriteAsync(string steamId, int appId)
        {
            var favorites = await GetFavoritesAsync(steamId);
            
            if (favorites.Contains(appId))
                favorites.Remove(appId);
            else
                favorites.Add(appId);
            
            var json = JsonSerializer.Serialize(favorites);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", $"favorites_{steamId}", json);
        }
    }
}