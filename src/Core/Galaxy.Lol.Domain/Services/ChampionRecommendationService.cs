using Galaxy.Lol.Domain.Entities;

namespace Galaxy.Lol.Domain.Services
{

    public class ChampionRecommendationService
    {
        public record Recommendation(ChampionProfile Champion, string Reason, int Score);

        public IReadOnlyCollection<Recommendation> Recomendar(
            IEnumerable<ChampionProfile> catalogo,
            Summoner? invocador,
            FreeRotation? rotacion,
            bool soloPrincipiantes,
            int cantidad)
        {
            var jugados = invocador?.Masteries.Select(m => m.Key.Value).ToHashSet() ?? [];
            var gratuitos = rotacion?.ClavesGenerales.ToHashSet() ?? [];

            var candidatos = catalogo
                .Where(c => c.IsActive)
                .Where(c => !jugados.Contains(c.Key.Value))
                .Where(c => !soloPrincipiantes || c.EsAptoParaPrincipiante)
                .Select(c => new Recommendation(c, ConstruirMotivo(c, gratuitos), Puntuar(c, gratuitos)))
                .OrderByDescending(r => r.Score)
                .ThenBy(r => r.Champion.Name)
                .Take(cantidad <= 0 ? 10 : cantidad)
                .ToList();

            return candidatos.AsReadOnly();
        }

        private static int Puntuar(ChampionProfile champion, IReadOnlySet<int> gratuitos)
        {
            var puntaje = 0;
            if (gratuitos.Contains(champion.Key.Value)) puntaje += 50;
            if (champion.EsAptoParaPrincipiante) puntaje += 20;
            puntaje += 10 - champion.Difficulty.Value;
            return puntaje;
        }

        private static string ConstruirMotivo(ChampionProfile champion, IReadOnlySet<int> gratuitos)
        {
            var motivos = new List<string> { "Aun no lo has jugado" };
            if (gratuitos.Contains(champion.Key.Value)) motivos.Add("esta en la rotacion gratuita");
            if (champion.EsAptoParaPrincipiante) motivos.Add($"dificultad {champion.Difficulty.Category}");
            motivos.Add($"rol {champion.RolPrincipal}");
            return string.Join(", ", motivos);
        }
    }
}
