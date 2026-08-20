namespace Galaxy.Lol.Domain.Exceptions
{
    public class InvalidMasteryScoreException(long puntos)
        : DomainException($"Los puntos de maestria '{puntos}' no pueden ser negativos.");
}
