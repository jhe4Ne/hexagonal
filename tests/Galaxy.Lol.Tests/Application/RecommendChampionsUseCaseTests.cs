using Galaxy.Lol.Application.Features.Masteries.DTO;
using Galaxy.Lol.Application.Features.Masteries.Services;
using Galaxy.Lol.Application.Features.Masteries.UseCases;
using Galaxy.Lol.Domain.Entities;
using Galaxy.Lol.Domain.Model.External;
using Galaxy.Lol.Domain.Ports.Repositories;
using Galaxy.Lol.Domain.Ports.Services;
using Galaxy.Lol.Domain.Services;
using Galaxy.Lol.Domain.ValueObjects;
using Galaxy.Lol.Tests.Domain;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Galaxy.Lol.Tests.Application
{
    public class RecommendChampionsUseCaseTests
    {
        private readonly Mock<IChampionRepositoryPort> _champions = new();
        private readonly Mock<IFreeRotationRepositoryPort> _rotations = new();
        private readonly Mock<ISummonerRepositoryPort> _summoners = new();
        private readonly Mock<IRiotApiPort> _riotApi = new();
        private readonly Mock<IAnalyticsRepositoryPort> _analytics = new();

        private RecommendChampionsUseCase Crear()
        {
            var loader = new SummonerMasteryLoader(
                _riotApi.Object, _summoners.Object, _analytics.Object,
                NullLogger<SummonerMasteryLoader>.Instance);

            return new(loader, _champions.Object, _rotations.Object, _summoners.Object, new ChampionRecommendationService());
        }

        [Fact]
        public async Task Devuelve_409_si_el_catalogo_esta_vacio()
        {
            _champions.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(Array.Empty<ChampionProfile>());

            var resultado = await Crear().ExecuteAsync(new RecommendChampionsRequest { Platform = "la1" });

            Assert.False(resultado.IsSuccess);
            Assert.Equal(409, resultado.ErrorCode);
        }

        [Fact]
        public async Task Devuelve_404_si_el_riot_id_no_existe()
        {
            _champions.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync([AggregateTests.CrearChampion(2, 103, "Ahri")]);
            _riotApi.Setup(r => r.GetAccountByRiotIdAsync("Nadie", "000", "la1", It.IsAny<CancellationToken>()))
                    .ReturnsAsync((RiotAccount?)null);

            var resultado = await Crear().ExecuteAsync(
                new RecommendChampionsRequest { Platform = "la1", GameName = "Nadie", TagLine = "000" });

            Assert.False(resultado.IsSuccess);
            Assert.Equal(404, resultado.ErrorCode);
        }

        [Fact]
        public async Task Recomienda_sin_riot_id_para_cuentas_nuevas()
        {
            var facil = AggregateTests.CrearChampion(2, 103, "Ahri");
            var dificil = AggregateTests.CrearChampion(9, 84, "Akali");

            _champions.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync([facil, dificil]);
            _rotations.Setup(r => r.GetLatestAsync("la1", It.IsAny<CancellationToken>()))
                      .ReturnsAsync((FreeRotation?)null);

            var resultado = await Crear().ExecuteAsync(new RecommendChampionsRequest
            {
                Platform = "la1",
                OnlyBeginnerFriendly = true,
                Count = 5
            });

            Assert.True(resultado.IsSuccess);
            var unica = Assert.Single(resultado.Data!);
            Assert.Equal("Ahri", unica.ChampionName);
            _summoners.Verify(r => r.GetByPuuidAsync(It.IsAny<Puuid>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
