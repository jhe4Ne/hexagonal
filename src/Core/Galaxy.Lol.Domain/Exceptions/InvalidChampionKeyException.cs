namespace Galaxy.Lol.Domain.Exceptions
{
    public class InvalidChampionKeyException(int key)
        : DomainException($"La clave de campeon '{key}' no es valida: Riot usa enteros positivos.");
}
