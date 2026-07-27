FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Shared/EventApi.Contracts/EventApi.Contracts.csproj", "Shared/EventApi.Contracts/"]
COPY ["Bookings/Bookings.Domain/Bookings.Domain.csproj", "Bookings/Bookings.Domain/"]
COPY ["Bookings/Bookings.Application/Bookings.Application.csproj", "Bookings/Bookings.Application/"]
COPY ["Bookings/Bookings.Infrastructure/Bookings.Infrastructure.csproj", "Bookings/Bookings.Infrastructure/"]
COPY ["Bookings/Bookings.Presentation/Bookings.Presentation.csproj", "Bookings/Bookings.Presentation/"]
RUN dotnet restore "Bookings/Bookings.Presentation/Bookings.Presentation.csproj"
COPY Shared/ Shared/
COPY Bookings/ Bookings/
WORKDIR /src/Bookings/Bookings.Presentation
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Bookings.Presentation.dll"]
