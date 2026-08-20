namespace Galaxy.Lol.Domain.Exceptions
{
    public class InvalidDifficultyLevelException(int valor)
        : DomainException($"La dificultad '{valor}' esta fuera del rango 0-10 que publica Data Dragon.");
}
