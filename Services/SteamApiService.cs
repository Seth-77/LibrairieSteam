using System.Text.Json;
using LibrairieSteam.Models;

namespace LibrairieSteam.Services
{
    // Service d'accès à l'API Steam via un proxy CORS (nécessaire en Blazor WebAssembly)
    public class SteamApiService
    {
        private readonly HttpClient _httpClient;
        
        private readonly string _steamApiKey;
        
        private const string SteamApiBaseUrl = "https://api.steampowered.com";
        // Proxy CORS pour contourner les restrictions navigateur sur les appels API Steam
        private const string CorsProxy = "https://corsproxy.io/?";
        
        public SteamApiService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _steamApiKey = config["SteamApiKey"] ?? "";
        }
        
        // Récupère tous les jeux possédés par un utilisateur, triés par temps de jeu décroissant
        public async Task<List<SteamGame>> GetOwnedGamesAsync(string steamId)
        {
            var steamUrl = $"{SteamApiBaseUrl}/IPlayerService/GetOwnedGames/v0001/" +
                          $"?key={_steamApiKey}&steamid={steamId}" +
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
                        // playtime_2weeks n'existe que si le jeu a été lancé récemment
                        Playtime2Weeks = game.TryGetProperty("playtime_2weeks", out var p2w) ? p2w.GetInt32() : 0,
                        HeaderImageUrl = $"https://cdn.cloudflare.steamstatic.com/steam/apps/{game.GetProperty("appid").GetInt32()}/header.jpg"
                    });
            }
            
            return games.OrderByDescending(g => g.PlaytimeForever).ToList();
        }
        
        // Récupère les actualités d'un jeu, filtrées sur le dernier mois uniquement
        public async Task<List<GameNewsItem>> GetGameNewsAsync(int appId)
        {
            var steamUrl = $"{SteamApiBaseUrl}/ISteamNews/GetNewsForApp/v0002/" +
                          $"?appid={appId}&count=10";
            
            var url = $"{CorsProxy}{Uri.EscapeDataString(steamUrl)}";
            
            var response = await _httpClient.GetAsync(url);
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(jsonResponse);
            
            var newsList = new List<GameNewsItem>();
            
            var newsItems = jsonDoc.RootElement
                .GetProperty("appnews")
                .GetProperty("newsitems");
            
            // On ne garde que les news du dernier mois
            var oneMonthAgo = DateTimeOffset.UtcNow.AddMonths(-1).ToUnixTimeSeconds();
            
            foreach (var item in newsItems.EnumerateArray())
            {
                var date = item.GetProperty("date").GetInt64();
                
                if (date < oneMonthAgo)
                    continue;
                
                // Tronque le contenu à 200 caractères pour l'aperçu
                var content = item.GetProperty("contents").GetString() ?? "";
                var preview = content.Length > 200 ? content.Substring(0, 200) + "..." : content;
                
                newsList.Add(new GameNewsItem
                {
                    Title = item.GetProperty("title").GetString() ?? "",
                    Date = DateTimeOffset.FromUnixTimeSeconds(date).DateTime,
                    ContentPreview = preview,
                    Url = item.GetProperty("url").GetString() ?? ""
                });
            }
            
            return newsList;
        }

        // Récupère le profil public d'un utilisateur (pseudo, avatar)
        public async Task<SteamUserInfo?> GetPlayerSummaryAsync(string steamId)
        {
            var steamUrl = $"{SteamApiBaseUrl}/ISteamUser/GetPlayerSummaries/v0002/" +
                          $"?key={_steamApiKey}&steamids={steamId}";
            
            var url = $"{CorsProxy}{Uri.EscapeDataString(steamUrl)}";
            
            var response = await _httpClient.GetAsync(url);
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(jsonResponse);
            
            var player = jsonDoc.RootElement
                .GetProperty("response")
                .GetProperty("players")
                .EnumerateArray()
                .FirstOrDefault();
            
            return new SteamUserInfo
            {
                SteamId = steamId,
                PersonaName = player.GetProperty("personaname").GetString() ?? "",
                AvatarUrl = player.GetProperty("avatarfull").GetString() ?? ""
            };
        }
    
    }
    
    // Représente une actualité ou un patch note d'un jeu Steam
    public class GameNewsItem
    {
        public string Title { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        // Aperçu du contenu, tronqué à 200 caractères
        public string ContentPreview { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        
        public string DateFormatted => Date.ToString("dd MMMM yyyy");
    }

    // Informations de profil public d'un utilisateur Steam
    public class SteamUserInfo
    {
        public string SteamId { get; set; } = string.Empty;
        public string PersonaName { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
    }
}