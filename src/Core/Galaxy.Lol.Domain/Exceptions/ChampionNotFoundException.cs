namespace Galaxy.Lol.Domain.Exceptions
{
    public class ChampionNotFoundException(string identificador)
        : DomainException($"No existe el campeon '{identificador}' en el catalogo sincronizado.");
}
