FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY LanguageReader.sln ./
COPY LanguageReader.Api/LanguageReader.Api.csproj LanguageReader.Api/
COPY LanguageReader.Infrastructure/LanguageReader.Infrastructure.csproj LanguageReader.Infrastructure/
COPY LanguageReader.Shared/LanguageReader.Shared.csproj LanguageReader.Shared/
RUN dotnet restore LanguageReader.Api/LanguageReader.Api.csproj

COPY . .
RUN dotnet publish LanguageReader.Api/LanguageReader.Api.csproj --configuration Release --output /app/publish --no-restore

FROM runtime AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "LanguageReader.Api.dll"]