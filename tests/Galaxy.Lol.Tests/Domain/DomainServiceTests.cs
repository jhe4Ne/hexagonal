using Galaxy.Lol.Domain.Services;
using Galaxy.Lol.Domain.ValueObjects;

namespace Galaxy.Lol.Tests.Domain
{
    public class DomainServiceTests
    {
        private static RotationPeriod Semana => RotationPeriod.SemanaDe(new DateTime(2026, 8, 18));

        [Fact]
        public void Indice_logaritmico_devuelve_cero_sin_puntos()
        {
            var calculadora = new LogarithmicDominanceIndexCalculator();

            Assert.Equal(0m, calculadora.Calcular(MasteryScore.Zero, 100000));
        }

        [Fact]
        public void Indice_logaritmico_se_mantiene_dentro_del_rango()
        {
            var calculadora = new LogarithmicDominanceIndexCalculator();

            var indice = calculadora.Calcular(MasteryScore.Create(900000, 7), 900000);

            Assert.InRange(indice, 0m, 100m);
        }

        [Fact]
        public void Indice_logaritmico_reparte_mejor_que_el_lineal_en_valores_bajos()
        {

            var logaritmico = new LogarithmicDominanceIndexCalculator().Calcular(MasteryScore.Create(1000, 2), 900000);
            var lineal = new LinearDominanceIndexCalculator().Calcular(MasteryScore.Create(1000, 2), 900000);

            Assert.True(logaritmico > lineal);
        }

        [Fact]
        public void Recomendador_descarta_los_campeones_ya_jugados()
        {
            var servicio = new ChampionRecommendationService();
            var jugado = AggregateTests.CrearChampion(3, 266, "Aatrox");
            var nuevo = AggregateTests.CrearChampion(3, 103, "Ahri");

            var summoner = AggregateTests.CrearSummoner();
            summoner.RegistrarMaestria(ChampionKey.Create(266), MasteryScore.Create(1000, 3), null, false, 0);

            var recomendaciones = servicio.Recomendar([jugado, nuevo], summoner, null, false, 10);

            var unica = Assert.Single(recomendaciones);
            Assert.Equal("Ahri", unica.Champion.Name);
        }

        [Fact]
        public void Recomendador_prioriza_lo_que_esta_en_rotacion_gratuita()
        {
            var servicio = new ChampionRecommendationService();
            var enRotacion = AggregateTests.CrearChampion(8, 103, "Ahri");
            var fueraDeRotacion = AggregateTests.CrearChampion(8, 84, "Akali");

            var rotacion = Galaxy.Lol.Domain.Entities.FreeRotation.Create("la1", Semana, 10, [103], []);

            var recomendaciones = servicio.Recomendar([enRotacion, fueraDeRotacion], null, rotacion, false, 10);

            Assert.Equal("Ahri", recomendaciones.First().Champion.Name);
            Assert.Contains("rotacion gratuita", recomendaciones.First().Reason);
        }

        [Fact]
        public void Recomendador_puede_limitarse_a_campeones_faciles()
        {
            var servicio = new ChampionRecommendationService();
            var facil = AggregateTests.CrearChampion(2, 103, "Ahri");
            var dificil = AggregateTests.CrearChampion(9, 84, "Akali");

            var recomendaciones = servicio.Recomendar([facil, dificil], null, null, true, 10);

            Assert.Single(recomendaciones);
            Assert.Equal("Ahri", recomendaciones.First().Champion.Name);
        }
    }
}
