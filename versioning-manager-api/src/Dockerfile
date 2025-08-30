FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY . .
RUN dotnet restore
COPY . .
WORKDIR "/src"
RUN dotnet build "versioning-manager-api/versioning-manager-api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "versioning-manager-api/versioning-manager-api.csproj" \
    -c ${BUILD_CONFIGURATION} \
    --self-contained true \
    -r linux-x64 \
    /p:PublishSingleFile=true \
    /p:UseAppHost=true \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["versioning-manager-api"]
