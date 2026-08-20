using Galaxy.Lol.Application.Features.Champions.Ports;
using Galaxy.Lol.Application.Features.Champions.UseCases;
using Galaxy.Lol.Application.Features.Masteries.Ports;
using Galaxy.Lol.Application.Features.Masteries.Services;
using Galaxy.Lol.Application.Features.Masteries.UseCases;
using Galaxy.Lol.Application.Features.Rotations.Ports;
using Galaxy.Lol.Application.Features.Rotations.UseCases;
using Galaxy.Lol.Application.Features.Synchronization.EventHandlers;
using Galaxy.Lol.Application.Features.Synchronization.Ports;
using Galaxy.Lol.Application.Features.Synchronization.UseCases;
using Galaxy.Lol.Domain.Events.Domain;
using Galaxy.Lol.Domain.Events.Interfaces;
using Galaxy.Lol.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Galaxy.Lol.Application
{

    public static class DependencyInjections
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IGetChampionCatalogUseCase, GetChampionCatalogUseCase>();
            services.AddScoped<IGetChampionDetailUseCase, GetChampionDetailUseCase>();
            services.AddScoped<IGetRoleDistributionUseCase, GetRoleDistributionUseCase>();
            services.AddScoped<ChampionUseCases>();

            services.AddScoped<IGetFreeRotationUseCase, GetFreeRotationUseCase>();
            services.AddScoped<IGetRotationHistoryUseCase, GetRotationHistoryUseCase>();
            services.AddScoped<RotationUseCases>();

            services.AddScoped<SummonerMasteryLoader>();
            services.AddScoped<IGetPlayerMasteryUseCase, GetPlayerMasteryUseCase>();
            services.AddScoped<IGetTopMasteryUseCase, GetTopMasteryUseCase>();
            services.AddScoped<IRecommendChampionsUseCase, RecommendChampionsUseCase>();
            services.AddScoped<IGetMasteryByRoleUseCase, GetMasteryByRoleUseCase>();
            services.AddScoped<MasteryUseCases>();

            services.AddScoped<ISyncChampionCatalogUseCase, SyncChampionCatalogUseCase>();
            services.AddScoped<ISyncFreeRotationUseCase, SyncFreeRotationUseCase>();
            services.AddScoped<IGetSyncHistoryUseCase, GetSyncHistoryUseCase>();
            services.AddScoped<SyncUseCases>();

            services.AddScoped<IDominanceIndexCalculator, LogarithmicDominanceIndexCalculator>();
            services.AddScoped<ChampionRecommendationService>();

            services.AddScoped<IDomainEventHandler<FreeRotationChangedDomainEvent>, FreeRotationChangedEventHandler>();
            services.AddScoped<IDomainEventHandler<ChampionCatalogSyncedDomainEvent>, ChampionCatalogSyncedEventHandler>();
            services.AddScoped<IDomainEventHandler<MasteryRecordedDomainEvent>, MasteryRecordedEventHandler>();

            return services;
        }
    }
}
