# =============================================================================
# Galaxy LoL Champions - API hexagonal
# Build multietapa: el SDK solo existe durante la compilacion; la imagen final
# lleva unicamente el runtime de ASP.NET.
# =============================================================================

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Primero los csproj: si no cambian, la capa de restore se reutiliza.
COPY src/Core/Galaxy.Lol.Domain/Galaxy.Lol.Domain.csproj src/Core/Galaxy.Lol.Domain/
COPY src/Core/Galaxy.Lol.Application/Galaxy.Lol.Application.csproj src/Core/Galaxy.Lol.Application/
COPY src/Infraestructure/Galaxy.Lol.Infraestructure/Galaxy.Lol.Infraestructure.csproj src/Infraestructure/Galaxy.Lol.Infraestructure/
COPY src/Presentation/Galaxy.Lol.API/Galaxy.Lol.API.csproj src/Presentation/Galaxy.Lol.API/
RUN dotnet restore src/Presentation/Galaxy.Lol.API/Galaxy.Lol.API.csproj

COPY src/ src/
RUN dotnet publish src/Presentation/Galaxy.Lol.API/Galaxy.Lol.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Galaxy.Lol.API.dll"]
