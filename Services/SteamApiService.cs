using System.Text.Json;
using LibrairieSteam.Models;

namespace LibrairieSteam.Services
{
    public class SteamApiService
    {
        private readonly HttpClient _httpClient;
        
        private const string SteamApiKey = "EF7E74B79126AF6FB3DF286E9949E91C";
        
        private const string SteamApiBaseUrl = "https://api.steampowered.com";
        private const string CorsProxy = "https://corsproxy.io/?";
        
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
            
            var oneMonthAgo = DateTimeOffset.UtcNow.AddMonths(-1).ToUnixTimeSeconds();
            
            foreach (var item in newsItems.EnumerateArray())
            {
                var date = item.GetProperty("date").GetInt64();
                
                if (date < oneMonthAgo)
                    continue;
                
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
    }
    
    public class GameNewsItem
    {
        public string Title { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string ContentPreview { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        
        public string DateFormatted => Date.ToString("dd MMMM yyyy");
    }
}