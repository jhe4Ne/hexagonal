namespace Galaxy.Lol.Domain.Exceptions
{
    public class InvalidPuuidException(string? valor)
        : DomainException($"El PUUID '{Resumir(valor)}' no tiene el formato que emite Riot (cadena de 70 a 80 caracteres).")
    {

        private static string Resumir(string? valor) =>
            string.IsNullOrWhiteSpace(valor) ? "(vacio)" : $"{valor[..Math.Min(4, valor.Length)]}...";
    }
}
