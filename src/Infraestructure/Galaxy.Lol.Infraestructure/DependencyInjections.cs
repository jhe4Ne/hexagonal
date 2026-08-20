using Galaxy.Lol.Domain.Events.Interfaces;
using Galaxy.Lol.Domain.Ports.Cache;
using Galaxy.Lol.Domain.Ports.Repositories;
using Galaxy.Lol.Domain.Ports.Services;
using Galaxy.Lol.Infraestructure.Adapters.Cache;
using Galaxy.Lol.Infraestructure.Adapters.Repositories;
using Galaxy.Lol.Infraestructure.Adapters.Services;
using Galaxy.Lol.Infraestructure.Adapters.Services.Handlers;
using Galaxy.Lol.Infraestructure.Configuration.Repositories.Context;
using Galaxy.Lol.Infraestructure.Configuration.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Galaxy.Lol.Infraestructure
{

    public static class DependencyInjections
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,
                                                           IConfiguration configuration)
        {
            AgregarPersistenciaRelacional(services, configuration);
            AgregarPersistenciaNoRelacional(services, configuration);
            AgregarClientesHttp(services, configuration);
            AgregarNotificaciones(services, configuration);

            services.AddScoped<IChampionRepositoryPort, ChampionRepository>();
            services.AddScoped<IFreeRotationRepositoryPort, FreeRotationRepository>();
            services.AddScoped<ISummonerRepositoryPort, SummonerRepository>();
            services.AddScoped<IAnalyticsRepositoryPort, AnalyticsRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

            return services;
        }

        private static void AgregarPersistenciaRelacional(IServiceCollection services, IConfiguration configuration)
        {

            services.AddDbContext<ChampionsDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("ChampionsDb"),
                    npgsql => npgsql.MigrationsHistoryTable("__ef_migrations", "champions")));

            services.AddDbContext<AnalyticsDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("AnalyticsDb")));
        }

        private static void AgregarPersistenciaNoRelacional(IServiceCollection services, IConfiguration configuration)
        {
            var mongo = configuration.GetSection(MongoSettings.SectionName).Get<MongoSettings>()
                        ?? throw new InvalidDataException($"Falta la seccion '{MongoSettings.SectionName}' en la configuracion.");

            services.Configure<MongoSettings>(configuration.GetSection(MongoSettings.SectionName));

            services.AddSingleton<IMongoClient>(_ => new MongoClient(mongo.ConnectionString));
            services.AddScoped(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(mongo.Database));
            services.AddScoped<IChampionRawCachePort, MongoChampionRawCacheAdapter>();
        }

        private static void AgregarClientesHttp(IServiceCollection services, IConfiguration configuration)
        {

            services.Configure<RiotApiSettings>(options =>
            {
                configuration.GetSection(RiotApiSettings.SectionName).Bind(options);

                var claveEntorno = configuration["RIOT_API_KEY"]
                                   ?? Environment.GetEnvironmentVariable("RIOT_API_KEY");

                if (!string.IsNullOrWhiteSpace(claveEntorno))
                    options.ApiKey = claveEntorno;
            });

            services.Configure<DataDragonSettings>(configuration.GetSection(DataDragonSettings.SectionName));

            var riot = configuration.GetSection(RiotApiSettings.SectionName).Get<RiotApiSettings>() ?? new RiotApiSettings();
            var ddragon = configuration.GetSection(DataDragonSettings.SectionName).Get<DataDragonSettings>() ?? new DataDragonSettings();

            services.AddTransient<RiotApiKeyHandler>();
            services.AddTransient<RiotRateLimitHandler>();

            services.AddHttpClient(RiotApiAdapter.HttpClientName, client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(riot.TimeoutSeconds);
                })
                .AddHttpMessageHandler<RiotApiKeyHandler>()
                .AddHttpMessageHandler<RiotRateLimitHandler>();

            services.AddHttpClient(DataDragonAdapter.HttpClientName, client =>
            {
                client.BaseAddress = new Uri(ddragon.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(ddragon.TimeoutSeconds);
            });

            services.AddScoped<IRiotApiPort, RiotApiAdapter>();
            services.AddScoped<IDataDragonPort, DataDragonAdapter>();
        }

        private static void AgregarNotificaciones(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SmtpSettings>(configuration.GetSection(SmtpSettings.SectionName));
            services.AddScoped<INotificationPort, SmtpNotificationAdapter>();
        }
    }
}
