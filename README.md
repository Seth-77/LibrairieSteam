# 🎮 LibrairieSteam

Application web pour gérer votre bibliothèque Steam : consultez vos jeux, suivez votre temps de jeu et restez informé des dernières mises à jour de vos jeux favoris.

## Fonctionnalités

- **Connexion via Steam** — Authentification OpenID, aucun mot de passe à fournir
- **Bibliothèque complète** — Affiche tous vos jeux avec leur temps de jeu total et récent
- **Recherche et favoris** — Recherchez un jeu par nom et marquez vos préférés en favoris
- **Patch Notes** — Consultez les actualités du dernier mois pour vos jeux favoris

## Technologies

| Composant | Technologie |
|-----------|-------------|
| Framework | Blazor WebAssembly (.NET 9) |
| Langage | C# |
| API | Steam Web API (IPlayerService, ISteamNews, ISteamUser) |
| Auth | Steam OpenID 2.0 |
| Stockage client | localStorage (favoris + session) |
| Tests | xUnit |
| CI/CD | GitHub Actions → GitHub Pages |
| Proxy CORS | corsproxy.io |

## Installation

### Prérequis

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Une [clé API Steam](https://steamcommunity.com/dev/apikey)

### Lancement en local

1. Clonez le dépôt :
   ```bash
   git clone https://github.com/votre-utilisateur/LibrairieSteam.git
   cd LibrairieSteam
   ```

2. Ajoutez votre clé API Steam dans `wwwroot/appsettings.json` :
   ```json
   {
     "SteamApiKey": "VOTRE_CLE_API"
   }
   ```

3. Lancez l'application :
   ```bash
   dotnet run
   ```

4. Ouvrez votre navigateur sur `https://localhost:5001`

### Lancer les tests

```bash
cd LibrairieSteam.Tests
dotnet test
```

## Roadmap

- [ ] Passer en architecture Web Server (Blazor Server / API) pour déploiement sur VPS
- [ ] Ajouter un système de notifications (alertes patch notes, mises à jour de jeux favoris)
