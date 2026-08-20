using Galaxy.Lol.Domain.ValueObjects;

namespace Galaxy.Lol.Domain.Services
{

    public interface IDominanceIndexCalculator
    {
        string Nombre { get; }

        decimal Calcular(MasteryScore score, long puntosMaximosDelJugador);
    }
}
