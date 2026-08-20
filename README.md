# Galaxy LoL Champions — Arquitectura Hexagonal (Ports & Adapters)

Proyecto final de **Arquitectura de Software en .NET**.
Dominio: **League of Legends** — explorador y analítica de campeones sobre la
**Riot Games API** y **Data Dragon**.

Este es el segundo de los tres entregables. Es una **Web API REST** con
controladores, versionamiento, JWT y Swagger, siguiendo el proyecto de clase
`curso/hexagonal-architecture` (`Galaxy.Bank`).

---

## 1. Caso de negocio

Un jugador quiere saber **qué campeón probar esta semana**. Para responderlo hay
que cruzar tres fuentes distintas:

| Fuente | Qué aporta |
|---|---|
| Data Dragon (CDN estático) | Ficha del campeón: nombre, rol, dificultad, estadísticas, habilidades |
| champion-rotations-v3 | Qué campeones son jugables gratis esta semana |
| champion-mastery-v4 | Cuánto ha jugado ese usuario cada campeón |

Reglas de negocio implementadas en el núcleo:

- **Clasificación por dificultad** (`DifficultyLevel`): Baja / Media / Alta y
  «apto para principiante» (≤ 4).
- **Clasificación por rol** (`ChampionRole`): catálogo cerrado de 6 roles, validado
  contra lo que publique el CDN.
- **Pertenencia a la rotación gratuita**: cruce por `ChampionKey`, el único
  identificador común a los tres endpoints.
- **Índice de dominio** (`IDominanceIndexCalculator`): normaliza los puntos de
  maestría a una escala 0–100. La estrategia por defecto es **logarítmica**
  porque los puntos no tienen techo y están muy sesgados (un *main* con 900.000
  puntos contra el resto del catálogo con 1.000): una normalización lineal
  aplastaría casi todo a cero. La estrategia lineal queda disponible como
  alternativa intercambiable.
- **Recomendación de campeones** (`ChampionRecommendationService`): sugiere lo que
  el jugador **nunca ha probado**, priorizando lo que está en rotación gratuita
  (es la única ventana sin costo) y luego la dificultad baja.

---

## 2. Endpoints de Riot cubiertos

| # | Endpoint | Puerto de salida | Caso de uso |
|---|---|---|---|
| 1 | `GET /cdn/{version}/data/{locale}/champion.json` | `IDataDragonPort` | `SyncChampionCatalogUseCase` |
| 2 | `GET /cdn/{version}/data/{locale}/champion/{championId}.json` | `IDataDragonPort` | `SyncChampionCatalogUseCase` (con `includeDetails`) |
| 3 | `GET /lol/platform/v3/champion-rotations` | `IRiotApiPort` | `SyncFreeRotationUseCase` |
| 4 | `GET /lol/champion-mastery/v4/champion-masteries/by-puuid/{puuid}` | `IRiotApiPort` | `GetPlayerMasteryUseCase` |
| 5 | `.../by-puuid/{puuid}/top?count=N` | `IRiotApiPort` | `GetTopMasteryUseCase` |

Data Dragon es un CDN público y **no requiere API Key**. Los endpoints 3, 4 y 5
sí requieren la cabecera `X-Riot-Token`.

---

## 3. Seguridad de la API Key

**La `X-Riot-Token` nunca aparece en el código ni en `appsettings` versionado.**

1. `appsettings.json` declara la sección `RiotApi` **sin** el campo `ApiKey`.
2. `AddInfrastructure` la resuelve desde la variable de entorno `RIOT_API_KEY`
   (o *user-secrets* en desarrollo) y solo la sobreescribe si viene con valor.
3. `RiotApiKeyHandler` (un `DelegatingHandler`) la inyecta en cada petición
   saliente. Si está vacía, la aplicación falla explícitamente en vez de llamar
   a Riot sin credencial.
4. El núcleo del hexágono **nunca ve la clave**: `IRiotApiPort` no la menciona.

El `JWT_SECRET` sigue exactamente el mismo camino.

Además, el **PUUID es un dato personal**: el objeto de valor `Puuid` expone
`Masked` (`abcd...wxyz`) y es lo único que se escribe en logs, en las respuestas
de la API y en la base de analítica.

---

## 4. Estructura de la solución

```
lol.hexagonal-architecture.slnx
├── src/
│   ├── Core/
│   │   ├── Galaxy.Lol.Domain          ← CERO paquetes NuGet
│   │   │   ├── Entities/              agregados: ChampionProfile, FreeRotation, Summoner
│   │   │   ├── ValueObjects/          ChampionKey, DifficultyLevel, MasteryScore, Puuid…
│   │   │   ├── Services/              índice de dominio (Strategy) y recomendador
│   │   │   ├── Events/                eventos de dominio + despachador (interfaz)
│   │   │   ├── Ports/                 Repositories · Services · Cache
│   │   │   └── Model/                 contratos externos y read models
│   │   └── Galaxy.Lol.Application     casos de uso (puertos de entrada) + Result
│   ├── Infraestructure/
│   │   └── Galaxy.Lol.Infraestructure adaptadores: EF Core, Mongo, HTTP, SMTP
│   └── Presentation/
│       └── Galaxy.Lol.API             controladores, filtro UoW, middleware, JWT
├── tests/Galaxy.Lol.Tests             xUnit + Moq (dominio y casos de uso)
├── database/                          DDL de las dos bases relacionales
├── docker-compose.yml · dockerfile
└── docs/                              arquitectura y modelo de datos
```

**La regla de dependencias**: `Domain` no referencia nada. `Application` solo
referencia `Domain`. `Infraestructure` referencia `Application`. `API` referencia
`Infraestructure`. Las flechas apuntan siempre hacia el centro.

---

## 5. Persistencia

| Motor | Base | Contenido | Por qué separada |
|---|---|---|---|
| PostgreSQL | `bdchampions` | 7 tablas relacionadas: campeón, roles, habilidades, rotación, entradas de rotación, invocador, maestría | Catálogo transaccional, se lee en cada consulta |
| SQL Server | `bdanalitica` | Bitácora de sincronizaciones y fotos históricas del índice de dominio | Escritura intensiva, retención larga y aislamiento de fallos: si cae, el explorador sigue respondiendo |
| MongoDB | `lol_raw_cache` | Payload crudo de Data Dragon por versión y locale | El JSON del CDN es anidado y cambia de forma entre parches; guardarlo tal cual evita normalizarlo dos veces y deja trazabilidad de cada parche |

El DDL está en `database/` y lo aplican los contenedores de inicialización.
No se usan migraciones de EF Core: los scripts son la fuente de verdad del esquema.

---

## 6. Cómo levantarlo

```bash
cd lol/hexagonal-architecture/proyecto

cp .env.example .env
# Edite .env y ponga su RIOT_API_KEY (https://developer.riotgames.com)
# y un JWT_SECRET de al menos 32 caracteres.

docker compose up -d --build
```

| Servicio | URL |
|---|---|
| API + Swagger | http://localhost:1600 |
| PostgreSQL | `localhost:1601` |
| SQL Server | `localhost:1602` |
| MongoDB | `localhost:1603` |
| MailHog (bandeja) | http://localhost:1605 |
| Seq (logs) | http://localhost:1606 |

### Primer uso

```bash
# 1. Token (los endpoints están protegidos)
curl -X POST http://localhost:1600/api/v1/auth/token \
     -H "Content-Type: application/json" -d '{"user":"demo"}'

# 2. Cargar el catálogo desde Data Dragon
curl -X POST http://localhost:1600/api/v1/sync/catalog \
     -H "Authorization: Bearer <token>" \
     -H "Content-Type: application/json" -d '{"includeDetails":false}'

# 3. Cargar la rotación gratuita
curl -X POST http://localhost:1600/api/v1/sync/rotation \
     -H "Authorization: Bearer <token>" \
     -H "Content-Type: application/json" -d '{"platform":"la1"}'

# 4. Explorar
curl "http://localhost:1600/api/v1/champions?role=Mage&onlyFreeRotation=true" \
     -H "Authorization: Bearer <token>"
```

O directamente desde Swagger en `http://localhost:1600`.

---

## 7. Cumplimiento de la rúbrica

| Criterio | Dónde está |
|---|---|
| **Domain** (modelo rico, DDD, sin dependencias) | `Galaxy.Lol.Domain` — cero `PackageReference`. Objetos de valor con constructor privado y `Create`, agregados con invariantes (`Summoner.RegistrarMaestria` impide duplicados), eventos de dominio, excepciones propias |
| **Ports** | `Ports/Repositories` (`IChampionRepositoryPort`, `IFreeRotationRepositoryPort`, `ISummonerRepositoryPort`, `IAnalyticsRepositoryPort`, `IUnitOfWork`), `Ports/Services` (`IRiotApiPort`, `IDataDragonPort`, `INotificationPort`), `Ports/Cache` (`IChampionRawCachePort`) |
| **Método de interfaz por defecto** | `IBaseRepository.ExistsAsync` — resuelto sobre `GetByIdAsync`, ningún adaptador lo reimplementa |
| **Application Service** (casos de uso + transacciones por AOP) | `Features/*/UseCases`. El commit lo hace `UnitOfWorkFilter` (`IAsyncActionFilter`), no los casos de uso |
| **Adapter — Persistencia SQL** | `Adapters/Repositories` con EF Core; objetos de valor mapeados con `OwnsOne`/`OwnsMany` |
| **Adapter — Persistencia NoSQL** | `Adapters/Cache/MongoChampionRawCacheAdapter` |
| **Adapter — REST cliente** | `RiotApiAdapter` y `DataDragonAdapter` con `IHttpClientFactory` |
| **Controller-API** | `Controllers/` con `[ApiVersion("1.0")]`, ruta `api/v{version:apiVersion}/[controller]` y `[Authorize]` |
| **Estrategia / patrón de diseño** | Strategy en `IDominanceIndexCalculator` (logarítmica ↔ lineal); Repository; Unit of Work; Adapter; Aggregate Root |
| **Consultas LINQ + SQL crudo** | LINQ en `ChampionRepository.SearchAsync`; SQL crudo en `GetRoleDistributionAsync` y `GetMasteryByRoleAsync` sobre entidades sin clave |
| **2 bases relacionales + 4 tablas relacionadas** | PostgreSQL con 7 tablas relacionadas + SQL Server con 2 |
| **Pruebas unitarias** | `tests/Galaxy.Lol.Tests` — xUnit + Moq, dominio y casos de uso contra puertos simulados |
| **Contenedores** | `docker-compose.yml`: API, PostgreSQL, SQL Server, MongoDB, MailHog, Seq, más dos contenedores de inicialización |
| **Documentación** | Swagger en la raíz, este README, `docs/arquitectura.md` y `docs/modelo-datos.md` |
| **Seguridad** | JWT Bearer; API Key y secreto de firma por variable de entorno; PUUID enmascarado en logs y respuestas |

---

## 8. Decisiones de diseño que conviene explicar

**Por qué la API lee de la base local y no de Riot en cada petición.**
La clave de desarrollo tiene un cupo de 20 peticiones por segundo y caduca cada
24 horas. Una pantalla de catálogo que llamara a Riot por cada carga lo agotaría
en segundos. Los datos entran por `POST /sync/*` y el resto de la API consulta
PostgreSQL. `GET /masteries` acepta `refresh=true` cuando sí se quiere ir a Riot.

**Por qué la rotación se guarda con un hash.**
Riot cambia la rotación los martes; el resto de la semana devuelve exactamente lo
mismo. `FreeRotation` calcula un SHA-256 de las claves ordenadas, así el trabajo
programado puede correr cada hora e insertar solo cuando algo cambió realmente.

**Por qué el límite de velocidad es proactivo.**
`RiotRateLimitHandler` usa una ventana deslizante local de 20 peticiones por
segundo. Es preferible esperar en local a que Riot devuelva 429: los 429
repetidos pueden acabar en bloqueo temporal de la clave. El `Retry-After` se
respeta igualmente como red de seguridad.

**Por qué el índice de dominio es una estrategia y no un método.**
Es la regla más discutible del sistema y la que más probablemente cambie.
Aislarla detrás de `IDominanceIndexCalculator` permite cambiarla en una línea del
registro de dependencias sin tocar casos de uso ni controladores.

**Por qué el cache de Data Dragon es NoSQL y no una tabla más.**
El contenido de un parche es inmutable y el JSON tiene forma variable entre
versiones. Mongo lo guarda tal cual, sirve de cache y deja constancia de qué
publicó el CDN en cada parche, sin obligar a migrar el esquema relacional cada
vez que Riot añade un campo.

---

## 9. Estado de verificación

- `dotnet build lol.hexagonal-architecture.slnx` → **compilación correcta, 0 advertencias, 0 errores**.
- `dotnet run` y `dotnet test` están **bloqueados por la política de Windows App
  Control de esta máquina**, así que las pruebas unitarias **no se han ejecutado**
  y los contenedores **no se han levantado**. Todo lo que depende de ejecución real
  —respuestas de Riot, envío SMTP, aplicación del DDL, comportamiento de EF Core en
  runtime— está **sin verificar**.
