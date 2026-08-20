namespace Galaxy.Lol.Domain.Exceptions
{
    public class SummonerNotFoundException(string identificador)
        : DomainException($"No existe el invocador '{identificador}' registrado localmente.");
}
