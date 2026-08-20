using System.Net;
using System.Text.Json;
using Galaxy.Lol.Domain.Exceptions;

namespace Galaxy.Lol.API.Middlewares
{

    public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                var estado = ex switch
                {
                    ChampionNotFoundException or SummonerNotFoundException or RotationNotAvailableException
                        or RiotAccountNotFoundException
                        => HttpStatusCode.NotFound,
                    DomainException or ArgumentException => HttpStatusCode.BadRequest,
                    HttpRequestException => HttpStatusCode.BadGateway,
                    InvalidOperationException => HttpStatusCode.InternalServerError,
                    _ => HttpStatusCode.InternalServerError
                };

                if (estado == HttpStatusCode.InternalServerError)
                    logger.LogError(ex, "Error no controlado en {Ruta}.", context.Request.Path);
                else
                    logger.LogWarning("Peticion rechazada en {Ruta}: {Mensaje}", context.Request.Path, ex.Message);

                context.Response.Clear();
                context.Response.StatusCode = (int)estado;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    IsSuccess = false,

                    Message = estado == HttpStatusCode.InternalServerError
                        ? "Ocurrio un error inesperado procesando la solicitud."
                        : ex.Message,
                    TimeSpan = DateTime.Now
                }));
            }
        }
    }
}
