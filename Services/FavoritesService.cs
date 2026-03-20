using Microsoft.JSInterop;
using System.Text.Json;

namespace LibrairieSteam.Services
{
    // Service de gestion des jeux favoris, persistés dans le localStorage du navigateur
    public class FavoritesService
    {
        private readonly IJSRuntime _jsRuntime;

        public FavoritesService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        // Récupère la liste des AppId favoris d'un utilisateur depuis le localStorage
        // Retourne une liste vide si aucun favori n'est enregistré
        public async Task<List<int>> GetFavoritesAsync(string steamId)
        {
            var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", $"favorites_{steamId}");

            if (string.IsNullOrEmpty(json))
                return new List<int>();

            return JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();
        }

        // Ajoute ou retire un jeu des favoris (toggle)
        // Si le jeu est déjà en favori, il est retiré ; sinon, il est ajouté
        public async Task ToggleFavoriteAsync(string steamId, int appId)
        {
            var favorites = await GetFavoritesAsync(steamId);

            if (favorites.Contains(appId))
                favorites.Remove(appId);
            else
                favorites.Add(appId);

            // Sérialise et sauvegarde la liste mise à jour dans le localStorage
            var json = JsonSerializer.Serialize(favorites);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", $"favorites_{steamId}", json);
        }
    }
}