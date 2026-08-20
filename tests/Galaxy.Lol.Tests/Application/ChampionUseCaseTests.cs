using Galaxy.Lol.Application.Features.Champions.UseCases;
using Galaxy.Lol.Domain.Entities;
using Galaxy.Lol.Domain.Ports.Repositories;
using Galaxy.Lol.Domain.ValueObjects;
using Galaxy.Lol.Tests.Domain;
using Moq;

namespace Galaxy.Lol.Tests.Application
{

    public class ChampionUseCaseTests
    {
        private readonly Mock<IChampionRepositoryPort> _champions = new();
        private readonly Mock<IFreeRotationRepositoryPort> _rotations = new();

        [Fact]
        public async Task Detalle_devuelve_404_cuando_el_campeon_no_esta_sincronizado()
        {
            _champions.Setup(r => r.GetDetailAsync("Aatrox", It.IsAny<CancellationToken>()))
                      .ReturnsAsync((ChampionProfile?)null);

            var caso = new GetChampionDetailUseCase(_champions.Object, _rotations.Object);

            var resultado = await caso.ExecuteAsync("Aatrox", "la1");

            Assert.False(resultado.IsSuccess);
            Assert.Equal(404, resultado.ErrorCode);
        }

        [Fact]
        public async Task Detalle_marca_el_campeon_que_esta_en_rotacion_gratuita()
        {
            var champion = AggregateTests.CrearChampion(4);
            champion.DefinirRoles([ChampionRole.Create("Fighter")]);

            var rotacion = FreeRotation.Create("la1", RotationPeriod.SemanaDe(new DateTime(2026, 8, 18)),
                10, [266], []);

            _champions.Setup(r => r.GetDetailAsync("Aatrox", It.IsAny<CancellationToken>())).ReturnsAsync(champion);
            _rotations.Setup(r => r.GetLatestAsync("la1", It.IsAny<CancellationToken>())).ReturnsAsync(rotacion);

            var caso = new GetChampionDetailUseCase(_champions.Object, _rotations.Object);

            var resultado = await caso.ExecuteAsync("Aatrox", "la1");

            Assert.True(resultado.IsSuccess);
            Assert.True(resultado.Data!.InFreeRotation);
            Assert.Equal("Fighter", resultado.Data.Roles.First());
        }

        [Fact]
        public async Task Detalle_rechaza_un_identificador_vacio()
        {
            var caso = new GetChampionDetailUseCase(_champions.Object, _rotations.Object);

            var resultado = await caso.ExecuteAsync("  ", "la1");

            Assert.False(resultado.IsSuccess);
            Assert.Equal(400, resultado.ErrorCode);
            _champions.Verify(r => r.GetDetailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
