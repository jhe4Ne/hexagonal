# Credenciales - Hexagonal Architecture (rango de puertos 16xx)

Proyecto Docker Compose: `lol-hexagonal` (ver `docker-compose.yml`).
Contrasenas de desarrollo fijas en el compose: no son secretas, sirven solo para
correr localmente. Nunca reutilizar en un ambiente real.

## Bases de datos

### PostgreSQL (catalogo, esquema `champions`)
- Contenedor: `postgres_lol_hex`
- Host/puerto (desde el host): `localhost:1601`
- Usuario: `admin`
- Password: `Password2026`
- Base de datos: `bdchampions`
- Cadena de conexion: `Host=localhost;Port=1601;Database=bdchampions;Username=admin;Password=Password2026`

### SQL Server (analitica `bdanalitica`)
- Contenedor: `sqlserver_lol_hex`
- Host/puerto (desde el host): `localhost,1602`
- Usuario: `sa`
- Password: `Password2026`
- Base de datos: `bdanalitica`
- Cadena de conexion: `Server=localhost,1602;Database=bdanalitica;User Id=sa;Password=Password2026;TrustServerCertificate=True;Encrypt=False`

### MongoDB (cache del payload crudo de Data Dragon)
- Contenedor: `mongo_lol_hex`
- Host/puerto (desde el host): `localhost:1603`
- Usuario: `admin`
- Password: `Password2026`
- Cadena de conexion: `mongodb://admin:Password2026@localhost:1603/?authSource=admin`

## Monitoreo y mensajeria

### Mailhog (SMTP de pruebas)
- Contenedor: `mailhog_lol_hex`
- SMTP: `localhost:1604` (sin autenticacion)
- Bandeja web: http://localhost:1605

### Seq (logging estructurado, Serilog)
- Contenedor: `seq_lol_hex`
- URL: http://localhost:1606
- Usuario: `admin`
- Password: `Password2026` (definida por `SEQ_FIRSTRUN_ADMINPASSWORD`)

### Jaeger (trazabilidad distribuida, OpenTelemetry)
- Contenedor: `jaeger_lol_hex`
- UI: http://localhost:1607
- Recibe spans via OTLP gRPC (puerto interno 4317) de cada peticion HTTP
  entrante y de cada llamada saliente a Riot/Data Dragon.

## API

- Swagger: http://localhost:1600/swagger
- Todos los endpoints (excepto `/api/v1/auth/token`) requieren JWT Bearer.
  Generarlo con `POST /api/v1/auth/token` body `{ "user": "cualquier-nombre" }`
  (endpoint de demo, no valida contrasena real).

## Secretos (NO estan en este archivo)

- `RIOT_API_KEY`: vive en `.env` (gitignored). Se obtiene en
  https://developer.riotgames.com y caduca cada 24 horas.
- `JWT_SECRET`: vive en `.env` (gitignored). Debe tener al menos 32 caracteres.

Ver tambien `.env.example` para la lista completa de variables esperadas.
