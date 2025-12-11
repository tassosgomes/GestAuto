# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["GestAuto.Commercial.sln", "."]
COPY ["1-Services/GestAuto.Commercial.API/GestAuto.Commercial.API.csproj", "1-Services/GestAuto.Commercial.API/"]
COPY ["2-Application/GestAuto.Commercial.Application/GestAuto.Commercial.Application.csproj", "2-Application/GestAuto.Commercial.Application/"]
COPY ["3-Domain/GestAuto.Commercial.Domain/GestAuto.Commercial.Domain.csproj", "3-Domain/GestAuto.Commercial.Domain/"]
COPY ["4-Infra/GestAuto.Commercial.Infra/GestAuto.Commercial.Infra.csproj", "4-Infra/GestAuto.Commercial.Infra/"]
COPY ["5-Tests/GestAuto.Commercial.UnitTest/GestAuto.Commercial.UnitTest.csproj", "5-Tests/GestAuto.Commercial.UnitTest/"]
COPY ["5-Tests/GestAuto.Commercial.IntegrationTest/GestAuto.Commercial.IntegrationTest.csproj", "5-Tests/GestAuto.Commercial.IntegrationTest/"]
COPY ["5-Tests/GestAuto.Commercial.End2EndTest/GestAuto.Commercial.End2EndTest.csproj", "5-Tests/GestAuto.Commercial.End2EndTest/"]
RUN dotnet restore "./GestAuto.Commercial.sln"
COPY . .
WORKDIR "/src/1-Services/GestAuto.Commercial.API"
RUN dotnet build "./GestAuto.Commercial.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GestAuto.Commercial.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GestAuto.Commercial.API.dll"]
