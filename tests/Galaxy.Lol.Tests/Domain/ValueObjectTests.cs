using Galaxy.Lol.Domain.Exceptions;
using Galaxy.Lol.Domain.ValueObjects;

namespace Galaxy.Lol.Tests.Domain
{
    public class ValueObjectTests
    {
        [Theory]
        [InlineData(0, "Baja")]
        [InlineData(3, "Baja")]
        [InlineData(4, "Media")]
        [InlineData(6, "Media")]
        [InlineData(7, "Alta")]
        [InlineData(10, "Alta")]
        public void DifficultyLevel_clasifica_segun_el_rango(int valor, string categoriaEsperada)
        {
            var dificultad = DifficultyLevel.Create(valor);

            Assert.Equal(categoriaEsperada, dificultad.Category);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(11)]
        public void DifficultyLevel_rechaza_valores_fuera_del_rango_de_data_dragon(int valor) =>
            Assert.Throws<InvalidDifficultyLevelException>(() => DifficultyLevel.Create(valor));

        [Fact]
        public void DifficultyLevel_marca_apto_para_principiante_hasta_cuatro()
        {
            Assert.True(DifficultyLevel.Create(4).SuitableForBeginners);
            Assert.False(DifficultyLevel.Create(5).SuitableForBeginners);
        }

        [Fact]
        public void MasteryScore_no_admite_puntos_negativos() =>
            Assert.Throws<InvalidMasteryScoreException>(() => MasteryScore.Create(-1, 3));

        [Fact]
        public void MasteryScore_suma_puntos_y_conserva_el_nivel_mayor()
        {
            var total = MasteryScore.Create(1000, 3) + MasteryScore.Create(500, 5);

            Assert.Equal(1500, total.Points);
            Assert.Equal(5, total.Level);
        }

        [Fact]
        public void Puuid_enmascara_el_identificador_para_los_logs()
        {
            var valor = new string('a', 78);

            var puuid = Puuid.Create(valor);

            Assert.Equal("aaaa...aaaa", puuid.Masked);
            Assert.DoesNotContain(valor, puuid.ToString());
        }

        [Fact]
        public void Puuid_rechaza_cadenas_de_longitud_invalida() =>
            Assert.Throws<InvalidPuuidException>(() => Puuid.Create("corto"));

        [Fact]
        public void ChampionRole_normaliza_mayusculas_y_rechaza_lo_desconocido()
        {
            Assert.Equal("Marksman", ChampionRole.Create("marksman").Value);
            Assert.Throws<InvalidChampionRoleException>(() => ChampionRole.Create("Jungla"));
        }

        [Fact]
        public void RotationPeriod_arranca_el_martes_de_la_semana_consultada()
        {

            var periodo = RotationPeriod.SemanaDe(new DateTime(2026, 8, 20));

            Assert.Equal(new DateTime(2026, 8, 18), periodo.Start);
            Assert.True(periodo.Contiene(new DateTime(2026, 8, 20)));
        }

        [Fact]
        public void ValueObject_compara_por_estructura_no_por_referencia() =>
            Assert.Equal(ChampionKey.Create(266), ChampionKey.Create("266"));
    }
}
