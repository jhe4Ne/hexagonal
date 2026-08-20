using Galaxy.Lol.Domain.ValueObjects;

namespace Galaxy.Lol.Domain.Services
{

    public class LogarithmicDominanceIndexCalculator : IDominanceIndexCalculator
    {
        public string Nombre => "Logaritmica";

        public decimal Calcular(MasteryScore score, long puntosMaximosDelJugador)
        {
            if (score.Points <= 0 || puntosMaximosDelJugador <= 0) return 0m;

            var proporcion = Math.Log10(score.Points + 1) / Math.Log10(puntosMaximosDelJugador + 1);
            var bonoNivel = Math.Min(score.Level, 7) * 0.5;
            var indice = Math.Clamp(proporcion * 95 + bonoNivel, 0, 100);

            return Math.Round((decimal)indice, 2);
        }
    }
}
