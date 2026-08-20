namespace Galaxy.Lol.Domain.Exceptions
{
    public class InvalidRotationPeriodException(DateTime inicio, DateTime fin)
        : DomainException($"El periodo de rotacion {inicio:d} - {fin:d} es invalido: el fin debe ser posterior al inicio.");
}
