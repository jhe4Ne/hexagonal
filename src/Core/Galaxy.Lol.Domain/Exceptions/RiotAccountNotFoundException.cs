namespace Galaxy.Lol.Domain.Exceptions
{
    public class RiotAccountNotFoundException(string gameName, string tagLine)
        : DomainException($"No se encontro ningun invocador con el Riot ID '{gameName}#{tagLine}'.");
}
