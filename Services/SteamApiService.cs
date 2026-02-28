using System.Text.Json;
using LibrairieSteam.Models;

namespace LibrairieSteam.Services
{
    public class SteamApiService
    {
        private readonly HttpClient _httpClient;
        
        private const string SteamApiKey = "EF7E74B79126AF6FB3DF286E9949E91C";
        
        private const string SteamApiBaseUrl = "https://api.steampowered.com";
        private const string CorsProxy = "https://api.allorigins.win/raw?url=";
        
        public SteamApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        
        public async Task<List<SteamGame>> GetOwnedGamesAsync(string steamId)
        {
            var steamUrl = $"{SteamApiBaseUrl}/IPlayerService/GetOwnedGames/v0001/" +
                          $"?key={SteamApiKey}&steamid={steamId}" +
                          $"&include_appinfo=true&include_played_free_games=true";
            
            var url = $"{CorsProxy}{Uri.EscapeDataString(steamUrl)}";
            
            var response = await _httpClient.GetAsync(url);
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(jsonResponse);
            
            var games = new List<SteamGame>();
            
            var gamesElement = jsonDoc.RootElement
                .GetProperty("response")
                .GetProperty("games");
            
            foreach (var game in gamesElement.EnumerateArray())
            {
                games.Add(new SteamGame
                {
                    AppId = game.GetProperty("appid").GetInt32(),
                    Name = game.GetProperty("name").GetString() ?? "Unknown",
                    PlaytimeForever = game.GetProperty("playtime_forever").GetInt32(),
                    HeaderImageUrl = $"https://cdn.cloudflare.steamstatic.com/steam/apps/{game.GetProperty("appid").GetInt32()}/header.jpg"
                });
            }
            
            return games.OrderByDescending(g => g.PlaytimeForever).ToList();
        }
    }
}