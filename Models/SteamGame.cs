namespace LibrairieSteam.Models
{
    public class SteamGame
    {
        public int AppId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int PlaytimeForever { get; set; }
        public int Playtime2Weeks { get; set; }
        public string HeaderImageUrl { get; set; } = string.Empty;
        public bool IsFavorite { get; set; }
        
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
    }
}