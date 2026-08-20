namespace Galaxy.Lol.Domain.Exceptions
{
    public class InvalidChampionRoleException(string rol)
        : DomainException($"El rol '{rol}' no pertenece al catalogo de roles de League of Legends.");
}
