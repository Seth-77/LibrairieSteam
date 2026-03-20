namespace LibrairieSteam.Models
{
    // Représente un jeu de la bibliothèque Steam d'un utilisateur
    public class SteamGame
    {
        // Identifiant unique de l'application sur Steam
        public int AppId { get; set; }

        public string Name { get; set; } = string.Empty;

        // Temps de jeu total en minutes
        public int PlaytimeForever { get; set; }

        // Temps de jeu en minutes sur les 2 dernières semaines (0 si pas joué récemment)
        public int Playtime2Weeks { get; set; }

        // URL de l'image d'en-tête depuis le CDN Steam
        public string HeaderImageUrl { get; set; } = string.Empty;

        // Géré côté client via le localStorage
        public bool IsFavorite { get; set; }

        // Convertit PlaytimeForever en format lisible (ex: "2h 5min" ou "45min")
        public string PlaytimeFormatted
        {
            get
            {
                var hours = PlaytimeForever / 60;
                var minutes = PlaytimeForever % 60;
                
                if (hours > 0)
                    return $"{hours}h {minutes}min";
                else
                    return $"{minutes}min";
            }
        }

        // Convertit Playtime2Weeks en format lisible avec suffixe "récemment"
        // Retourne une chaîne vide si pas de temps de jeu récent
        public string Playtime2WeeksFormatted
        {
            get
            {
                if (Playtime2Weeks == 0)
                    return "";
                
                var hours = Playtime2Weeks / 60;
                var minutes = Playtime2Weeks % 60;
                
                if (hours > 0)
                    return $"{hours}h {minutes}min récemment";
                else
                    return $"{minutes}min récemment";
            }
        }
    }
}