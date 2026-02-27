using Microsoft.JSInterop;
using System.Web;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;


namespace LibrairieSteam.Services
{
    public class SteamAuthService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly NavigationManager _navigationManager;
        
        private const string SteamOpenIdUrl = "https://steamcommunity.com/openid/login";
        
        public SteamAuthService(IJSRuntime jsRuntime, NavigationManager navigationManager)
        {
            _jsRuntime = jsRuntime;
            _navigationManager = navigationManager;
        }
        
        public string GetSteamLoginUrl()
        {
            var returnUrl = GetReturnUrl();
            var realm = GetRealm();
            
            var parameters = new Dictionary<string, string>
            {
                { "openid.ns", "http://specs.openid.net/auth/2.0" },
                { "openid.mode", "checkid_setup" },
                { "openid.return_to", returnUrl },
                { "openid.realm", realm },
                { "openid.identity", "http://specs.openid.net/auth/2.0/identifier_select" },
                { "openid.claimed_id", "http://specs.openid.net/auth/2.0/identifier_select" }
            };
            
            var queryString = string.Join("&", parameters.Select(p =>
                $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
            
            return $"{SteamOpenIdUrl}?{queryString}";
        }
        
        private string GetReturnUrl()
        {
            var baseUri = _navigationManager.BaseUri.TrimEnd('/');
            return $"{baseUri}/";
        }
        
        private string GetRealm()
        {
            var uri = new Uri(_navigationManager.BaseUri);
            return $"{uri.Scheme}://{uri.Authority}";
        }
        
        public Task<string?> ValidateSteamLogin(string currentUrl)
        {
            try
            {
                var uri = new Uri(currentUrl);
                var queryParams = HttpUtility.ParseQueryString(uri.Query);
                
                var claimedId = queryParams["openid.claimed_id"];
                if (string.IsNullOrEmpty(claimedId))
                {
                    throw new Exception("Claimed ID manquant");
                }
                
                var steamIdMatch = Regex.Match(claimedId, 
                    @"https://steamcommunity.com/openid/id/(\d+)");
                
                if (!steamIdMatch.Success)
                {
                    throw new Exception("SteamID invalide");
                }
                
                var steamId = steamIdMatch.Groups[1].Value;
                
                Console.WriteLine($"✅ SteamID extrait : {steamId}");
                
                return Task.FromResult<string?>(steamId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur validation : {ex.Message}");
                return Task.FromResult<string?>(null);
            }
        }
        
        public async Task SaveUserSession(string steamId)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "steamId", steamId);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", 
                "loginTime", DateTime.UtcNow.ToString("o"));
            
            Console.WriteLine($"💾 Session sauvegardée pour {steamId}");
        }
        
        public async Task<string?> GetCurrentSteamId()
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "steamId");
            }
            catch
            {
                return null;
            }
        }
        
        public async Task Logout()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "steamId");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "loginTime");
            
            _navigationManager.NavigateTo("/", forceLoad: true);
            
            Console.WriteLine("👋 Déconnexion réussie");
        }
        
        public async Task<bool> IsLoggedIn()
        {
            var steamId = await GetCurrentSteamId();
            return !string.IsNullOrEmpty(steamId);
        }
    }
}