using Galaxy.Lol.Domain.Ports.Services;
using Galaxy.Lol.Infraestructure.Configuration.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Galaxy.Lol.Infraestructure.Adapters.Services
{

    public class SmtpNotificationAdapter(
        IOptions<SmtpSettings> options,
        ILogger<SmtpNotificationAdapter> logger) : INotificationPort
    {
        private readonly SmtpSettings _settings = options.Value;

        public Task NotifyRotationChangedAsync(string platform, IReadOnlyCollection<string> championNames,
                                               CancellationToken cancellationToken = default)
        {
            var cuerpo = $"<h3>Nueva rotacion gratuita en {platform.ToUpperInvariant()}</h3>" +
                         $"<p>{championNames.Count} campeones disponibles esta semana:</p>" +
                         $"<p>{string.Join(", ", championNames)}</p>";

            return EnviarAsync($"Rotacion gratuita actualizada - {platform}", cuerpo, cancellationToken);
        }

        public Task NotifyCatalogSyncedAsync(string version, int totalChampions,
                                             CancellationToken cancellationToken = default)
        {
            var cuerpo = $"<h3>Catalogo sincronizado</h3>" +
                         $"<p>Version de Data Dragon: <b>{version}</b>.</p>" +
                         $"<p>Campeones procesados: <b>{totalChampions}</b>.</p>";

            return EnviarAsync($"Catalogo de campeones actualizado ({version})", cuerpo, cancellationToken);
        }

        private async Task EnviarAsync(string asunto, string cuerpoHtml, CancellationToken cancellationToken)
        {
            if (!_settings.Enabled)
            {
                logger.LogDebug("Notificaciones deshabilitadas; se omite el envio de '{Asunto}'.", asunto);
                return;
            }

            try
            {
                var mensaje = new MimeMessage();
                mensaje.From.Add(new MailboxAddress(_settings.FromName, _settings.From));
                mensaje.To.Add(MailboxAddress.Parse(_settings.To));
                mensaje.Subject = asunto;
                mensaje.Body = new BodyBuilder { HtmlBody = cuerpoHtml }.ToMessageBody();

                using var cliente = new SmtpClient();
                await cliente.ConnectAsync(_settings.Host, _settings.Port,
                    _settings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None, cancellationToken);

                if (!string.IsNullOrWhiteSpace(_settings.User))
                    await cliente.AuthenticateAsync(_settings.User, _settings.Password ?? string.Empty, cancellationToken);

                await cliente.SendAsync(mensaje, cancellationToken);
                await cliente.DisconnectAsync(true, cancellationToken);

                logger.LogInformation("Aviso enviado: {Asunto}", asunto);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "No se pudo enviar el aviso '{Asunto}'.", asunto);
            }
        }
    }
}
