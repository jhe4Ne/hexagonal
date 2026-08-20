# Arquitectura — Hexagonal (Ports & Adapters)

## 1. El hexágono

```mermaid
flowchart TB
    subgraph Driving["Adaptadores primarios (driving)"]
        API["Galaxy.Lol.API<br/>Controllers · Swagger · JWT"]
    end

    subgraph Core["Núcleo"]
        APP["Galaxy.Lol.Application<br/>Casos de uso · Result"]
        DOM["Galaxy.Lol.Domain<br/>Agregados · VOs · Servicios de dominio<br/>CERO dependencias"]
    end

    subgraph Driven["Adaptadores secundarios (driven)"]
        PG["ChampionRepository<br/>EF Core + PostgreSQL"]
        MS["AnalyticsRepository<br/>EF Core + SQL Server"]
        MG["MongoChampionRawCacheAdapter<br/>MongoDB"]
        RIOT["RiotApiAdapter<br/>HttpClient + X-Riot-Token"]
        DD["DataDragonAdapter<br/>HttpClient (CDN público)"]
        SMTP["SmtpNotificationAdapter<br/>MailKit"]
    end

    API -->|puertos de entrada| APP
    APP --> DOM
    PG -.->|implementa| DOM
    MS -.->|implementa| DOM
    MG -.->|implementa| DOM
    RIOT -.->|implementa| DOM
    DD -.->|implementa| DOM
    SMTP -.->|implementa| DOM
```

Las flechas punteadas son **inversión de dependencias**: los adaptadores dependen
de las interfaces que declara el dominio, nunca al revés. El compilador lo
garantiza: `Galaxy.Lol.Domain.csproj` no tiene un solo `PackageReference` ni
`ProjectReference`.

## 2. Puertos declarados

| Puerto | Tipo | Adaptador |
|---|---|---|
| `IChampionRepositoryPort` | salida — persistencia SQL | `ChampionRepository` (PostgreSQL) |
| `IFreeRotationRepositoryPort` | salida — persistencia SQL | `FreeRotationRepository` (PostgreSQL) |
| `ISummonerRepositoryPort` | salida — persistencia SQL | `SummonerRepository` (PostgreSQL) |
| `IAnalyticsRepositoryPort` | salida — persistencia SQL | `AnalyticsRepository` (SQL Server) |
| `IChampionRawCachePort` | salida — persistencia NoSQL | `MongoChampionRawCacheAdapter` |
| `IRiotApiPort` | salida — REST | `RiotApiAdapter` |
| `IDataDragonPort` | salida — REST | `DataDragonAdapter` |
| `INotificationPort` | salida — mensajería | `SmtpNotificationAdapter` |
| `IUnitOfWork` | salida — transaccional | `UnitOfWork` + `UnitOfWorkFilter` |
| `IGet*UseCase` / `ISync*UseCase` | entrada | los controladores |

## 3. Sincronización del catálogo

```mermaid
sequenceDiagram
    participant C as Cliente
    participant Ctrl as SyncController
    participant UC as SyncChampionCatalogUseCase
    participant DD as DataDragonAdapter
    participant Mongo as MongoDB
    participant Repo as ChampionRepository
    participant Filter as UnitOfWorkFilter

    C->>Ctrl: POST /api/v1/sync/catalog
    Ctrl->>UC: ExecuteAsync(request)
    UC->>DD: GetChampionCatalogAsync()
    DD->>Mongo: ¿está cacheado este parche?
    alt no está
        DD->>DD: GET /cdn/{v}/data/{locale}/champion.json
        DD->>Mongo: guardar payload crudo
    end
    DD-->>UC: DataDragonCatalog
    loop por cada campeón
        UC->>UC: ChampionProfile.Create / ActualizarFicha
        UC->>Repo: AddAsync / Update
    end
    UC-->>Ctrl: Result<SyncResultResponse>
    Ctrl-->>Filter: acción terminada sin excepción
    Filter->>Filter: UnitOfWork.SaveChangesAsync()
    Filter-->>C: 200 OK
```

El caso de uso **no llama a `SaveChanges`**. La transacción la cierra el filtro,
que es lo que mantiene la gestión transaccional fuera de la lógica de negocio.

## 4. Consulta de maestría con recálculo del índice de dominio

```mermaid
sequenceDiagram
    participant C as Cliente
    participant Ctrl as MasteriesController
    participant UC as GetPlayerMasteryUseCase
    participant L as SummonerMasteryLoader
    participant Riot as RiotApiAdapter
    participant Calc as IDominanceIndexCalculator
    participant An as AnalyticsRepository

    C->>Ctrl: GET /api/v1/masteries?puuid=…&refresh=true
    Ctrl->>UC: ExecuteAsync(request)
    UC->>L: LoadAsync(puuid, plataforma, refresh)
    L->>Riot: GET champion-masteries/by-puuid/{puuid}
    Riot-->>L: lista de maestrías
    L->>L: Summoner.RegistrarMaestria(...) por cada una
    L->>An: SyncLog.Exito(...)
    L-->>UC: agregado Summoner
    UC->>Calc: Calcular(score, puntosMáximos)
    UC->>An: SaveSnapshotsAsync(...)
    UC-->>Ctrl: Result<PlayerMasteryResponse>
```

Si Riot falla pero ya había datos locales, `SummonerMasteryLoader` devuelve lo
guardado en vez de romper la consulta, y deja el fallo registrado en la bitácora.

## 5. Flujo de eventos de dominio

```mermaid
flowchart LR
    AGG["Agregado<br/>AddDomainEvent(...)"] --> CT["ChangeTracker<br/>de EF Core"]
    CT --> UOW["UnitOfWork.SaveChangesAsync"]
    UOW --> DISP["DomainEventDispatcher<br/>(reflexión sobre IDomainEventHandler&lt;T&gt;)"]
    DISP --> H1["FreeRotationChangedEventHandler"]
    DISP --> H2["ChampionCatalogSyncedEventHandler"]
    DISP --> H3["MasteryRecordedEventHandler"]
    UOW --> DB[("PostgreSQL")]
```

Los eventos se despachan **antes** del `SaveChanges` para que, si un manejador
modifica el modelo, ese cambio entre en la misma transacción. Un manejador que
falle se registra pero no tumba el guardado: el hecho ya ocurrió.

## 6. Despliegue

```mermaid
flowchart TB
    subgraph Docker["docker compose — red lol_hex_network"]
        API["galaxy-lol-api<br/>:1600 → 8080"]
        PG[("postgres_lol_hex<br/>:1601")]
        MS[("sqlserver_lol_hex<br/>:1602")]
        MG[("mongo_lol_hex<br/>:1603")]
        MH["mailhog_lol_hex<br/>:1604 / :1605"]
        SEQ["seq_lol_hex<br/>:1606"]
        INIT["sqlserver_lol_hex_init<br/>(aplica el DDL y termina)"]
    end

    EXT["Riot Games API<br/>Data Dragon"]

    API --> PG
    API --> MS
    API --> MG
    API --> MH
    API --> SEQ
    API -->|HTTPS| EXT
    INIT --> MS
```

Los tres entregables usan rangos de puerto distintos (15xx / 16xx / 17xx) para
poder levantarse en paralelo.

## 7. Qué gana y qué cuesta esta arquitectura aquí

**Gana**: los casos de uso se prueban sin base de datos ni red — `ChampionUseCaseTests`
solo necesita `Mock<IChampionRepositoryPort>`. Cambiar PostgreSQL por otro motor,
o Mongo por Redis, toca un archivo de `Adapters/` y una línea de `DependencyInjections`.

**Cuesta**: hay más indirección que en la versión en capas. Una consulta simple
atraviesa controlador → caso de uso → puerto → adaptador → EF Core, y los modelos
externos de Riot se declaran dos veces (el `record` privado del adaptador y el
contrato del dominio). Es el precio de que el núcleo no sepa qué forma tiene el
JSON de Riot.
