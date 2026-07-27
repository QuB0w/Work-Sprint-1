FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Shared/EventApi.Contracts/EventApi.Contracts.csproj", "Shared/EventApi.Contracts/"]
COPY ["Events/Events.Domain/Events.Domain.csproj", "Events/Events.Domain/"]
COPY ["Events/Events.Application/Events.Application.csproj", "Events/Events.Application/"]
COPY ["Events/Events.Infrastructure/Events.Infrastructure.csproj", "Events/Events.Infrastructure/"]
COPY ["Events/Events.Presentation/Events.Presentation.csproj", "Events/Events.Presentation/"]
RUN dotnet restore "Events/Events.Presentation/Events.Presentation.csproj"
COPY Shared/ Shared/
COPY Events/ Events/
WORKDIR /src/Events/Events.Presentation
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Events.Presentation.dll"]
