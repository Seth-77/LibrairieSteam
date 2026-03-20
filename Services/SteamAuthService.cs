using Microsoft.JSInterop;
using System.Web;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;


namespace LibrairieSteam.Services
{
    // Service d'authentification via Steam OpenID 2.0
    // Gère le flux de connexion, la session utilisateur (localStorage) et la déconnexion
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
        
        // Construit l'URL de redirection vers Steam avec les paramètres OpenID requis
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
        
        // URL de retour après authentification (page d'accueil de l'app)
        private string GetReturnUrl()
        {
            var baseUri = _navigationManager.BaseUri.TrimEnd('/');
            return $"{baseUri}/";
        }
        
        // Realm OpenID = scheme + authority de l'app
        private string GetRealm()
        {
            var uri = new Uri(_navigationManager.BaseUri);
            return $"{uri.Scheme}://{uri.Authority}";
        }
        
        // Extrait le SteamID depuis l'URL de retour OpenID (paramètre openid.claimed_id)
        // Retourne null si la validation échoue
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
                
                // Le SteamID 64 bits se trouve à la fin de l'URL claimed_id
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
        
        // Sauvegarde le SteamID et l'horodatage de connexion dans le localStorage
        public async Task SaveUserSession(string steamId)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "steamId", steamId);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", 
                "loginTime", DateTime.UtcNow.ToString("o"));
            
            Console.WriteLine($"💾 Session sauvegardée pour {steamId}");
        }
        
        // Récupère le SteamID de la session active, ou null si non connecté
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
        
        // Supprime la session du localStorage et redirige vers la page d'accueil
        public async Task Logout()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "steamId");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "loginTime");
            
            _navigationManager.NavigateTo("/", forceLoad: true);
            
            Console.WriteLine("👋 Déconnexion réussie");
        }
        
        // Vérifie si un utilisateur est connecté (session active dans le localStorage)
        public async Task<bool> IsLoggedIn()
        {
            var steamId = await GetCurrentSteamId();
            return !string.IsNullOrEmpty(steamId);
        }
    }
}