namespace Galaxy.Lol.Domain.Exceptions
{
    public class RotationNotAvailableException(string plataforma)
        : DomainException($"Todavia no se ha sincronizado la rotacion gratuita de la plataforma '{plataforma}'.");
}
