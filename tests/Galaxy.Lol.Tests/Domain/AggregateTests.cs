using Galaxy.Lol.Domain.Entities;
using Galaxy.Lol.Domain.Events.Domain;
using Galaxy.Lol.Domain.ValueObjects;

namespace Galaxy.Lol.Tests.Domain
{
    public class AggregateTests
    {
        private static RotationPeriod Semana => RotationPeriod.SemanaDe(new DateTime(2026, 8, 18));

        [Fact]
        public void FreeRotation_produce_el_mismo_hash_para_el_mismo_contenido_en_distinto_orden()
        {
            var a = FreeRotation.Create("la1", Semana, 10, [266, 103, 84], [18]);
            var b = FreeRotation.Create("la1", Semana, 10, [84, 266, 103], [18]);

            Assert.Equal(a.Hash, b.Hash);
            Assert.True(a.TieneMismoContenidoQue(b));
        }

        [Fact]
        public void FreeRotation_cambia_el_hash_cuando_entra_un_campeon_nuevo()
        {
            var anterior = FreeRotation.Create("la1", Semana, 10, [266, 103], []);
            var nueva = FreeRotation.Create("la1", Semana, 10, [266, 103, 84], []);

            Assert.False(nueva.TieneMismoContenidoQue(anterior));
        }

        [Fact]
        public void FreeRotation_emite_el_evento_de_cambio_al_crearse()
        {
            var rotacion = FreeRotation.Create("la1", Semana, 10, [266], []);

            Assert.Single(rotacion.DomainEvents);
            Assert.IsType<FreeRotationChangedDomainEvent>(rotacion.DomainEvents.First());
        }

        [Fact]
        public void FreeRotation_separa_las_listas_general_y_de_novatos()
        {
            var rotacion = FreeRotation.Create("la1", Semana, 10, [266, 103], [18]);

            Assert.Equal(2, rotacion.ClavesGenerales.Count);
            Assert.Single(rotacion.ClavesParaNovatos);
            Assert.True(rotacion.Contiene(ChampionKey.Create(18)));
        }

        [Fact]
        public void Summoner_no_duplica_la_maestria_de_un_mismo_campeon()
        {
            var summoner = CrearSummoner();
            var key = ChampionKey.Create(266);

            summoner.RegistrarMaestria(key, MasteryScore.Create(1000, 3), null, false, 0);
            summoner.RegistrarMaestria(key, MasteryScore.Create(2500, 5), null, true, 2);

            var maestria = Assert.Single(summoner.Masteries);
            Assert.Equal(2500, maestria.Score.Points);
            Assert.Equal(5, maestria.Score.Level);
            Assert.True(maestria.ChestGranted);
        }

        [Fact]
        public void Summoner_expone_el_puntaje_maximo_del_jugador()
        {
            var summoner = CrearSummoner();

            summoner.RegistrarMaestria(ChampionKey.Create(266), MasteryScore.Create(1000, 3), null, false, 0);
            summoner.RegistrarMaestria(ChampionKey.Create(103), MasteryScore.Create(90000, 7), null, false, 0);

            Assert.Equal(90000, summoner.PuntajeMaximo.Points);
            Assert.True(summoner.HaJugado(ChampionKey.Create(103)));
        }

        [Fact]
        public void ChampionMastery_marca_abandonado_pasados_noventa_dias()
        {
            var summoner = CrearSummoner();
            var ahora = new DateTime(2026, 8, 18);

            var reciente = summoner.RegistrarMaestria(ChampionKey.Create(266),
                MasteryScore.Create(100, 1), ahora.AddDays(-10), false, 0);
            var viejo = summoner.RegistrarMaestria(ChampionKey.Create(103),
                MasteryScore.Create(100, 1), ahora.AddDays(-120), false, 0);

            Assert.False(reciente.EstaAbandonado(ahora));
            Assert.True(viejo.EstaAbandonado(ahora));
        }

        [Fact]
        public void ChampionProfile_reemplaza_las_habilidades_en_bloque()
        {
            var champion = CrearChampion(4);

            champion.ReemplazarHabilidades([
                (Galaxy.Lol.Domain.Enums.AbilitySlot.Passive, "Pasiva", null, null, null),
                (Galaxy.Lol.Domain.Enums.AbilitySlot.Q, "Q", null, null, 12)
            ]);
            champion.ReemplazarHabilidades([
                (Galaxy.Lol.Domain.Enums.AbilitySlot.Q, "Q nueva", null, null, 10)
            ]);

            var habilidad = Assert.Single(champion.Abilities);
            Assert.Equal("Q nueva", habilidad.Name);
        }

        [Fact]
        public void ChampionProfile_toma_el_primer_tag_como_rol_principal()
        {
            var champion = CrearChampion(4);

            champion.DefinirRoles([ChampionRole.Create("Fighter"), ChampionRole.Create("Tank")]);

            Assert.Equal("Fighter", champion.RolPrincipal);
            Assert.True(champion.TieneRol("tank"));
        }

        internal static Summoner CrearSummoner() =>
            Summoner.Create(Puuid.Create(new string('a', 78)), "la1");

        internal static ChampionProfile CrearChampion(int dificultad, int key = 266, string id = "Aatrox") =>
            ChampionProfile.Create(ChampionKey.Create(key), id, id, "la Espada Darkin", null, null,
                "15.1.1", DifficultyLevel.Create(dificultad), ChampionStats.Empty);
    }
}
