FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

RUN apt update && apt upgrade -y && \
    apt install ca-certificates -y 

COPY ./my-ca.crt /usr/local/share/ca-certificates

RUN update-ca-certificates

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY . .
RUN dotnet restore "versioning-manager-api.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "versioning-manager-api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "versioning-manager-api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "versioning-manager-api.dll"]
