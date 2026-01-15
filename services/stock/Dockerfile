FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["GestAuto.Stock.sln", "."]
COPY ["1-Services/GestAuto.Stock.API/GestAuto.Stock.API.csproj", "1-Services/GestAuto.Stock.API/"]
COPY ["2-Application/GestAuto.Stock.Application/GestAuto.Stock.Application.csproj", "2-Application/GestAuto.Stock.Application/"]
COPY ["3-Domain/GestAuto.Stock.Domain/GestAuto.Stock.Domain.csproj", "3-Domain/GestAuto.Stock.Domain/"]
COPY ["4-Infra/GestAuto.Stock.Infra/GestAuto.Stock.Infra.csproj", "4-Infra/GestAuto.Stock.Infra/"]
COPY ["5-Tests/GestAuto.Stock.UnitTest/GestAuto.Stock.UnitTest.csproj", "5-Tests/GestAuto.Stock.UnitTest/"]
COPY ["5-Tests/GestAuto.Stock.IntegrationTest/GestAuto.Stock.IntegrationTest.csproj", "5-Tests/GestAuto.Stock.IntegrationTest/"]
RUN dotnet restore "./GestAuto.Stock.sln"
COPY . .
WORKDIR "/src/1-Services/GestAuto.Stock.API"
RUN dotnet build "./GestAuto.Stock.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GestAuto.Stock.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GestAuto.Stock.API.dll"]
