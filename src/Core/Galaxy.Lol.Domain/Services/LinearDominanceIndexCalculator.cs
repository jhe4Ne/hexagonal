using Galaxy.Lol.Domain.ValueObjects;

namespace Galaxy.Lol.Domain.Services
{

    public class LinearDominanceIndexCalculator : IDominanceIndexCalculator
    {
        public string Nombre => "Lineal";

        public decimal Calcular(MasteryScore score, long puntosMaximosDelJugador)
        {
            if (score.Points <= 0 || puntosMaximosDelJugador <= 0) return 0m;

            var indice = (decimal)score.Points / puntosMaximosDelJugador * 100m;
            return Math.Round(Math.Clamp(indice, 0m, 100m), 2);
        }
    }
}
