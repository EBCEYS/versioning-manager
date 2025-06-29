﻿FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY . .
RUN dotnet restore "ServiceUploader.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "ServiceUploader.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish \
    -c ${BUILD_CONFIGURATION} \
    --self-contained true \
    -r linux-x64 \
    /p:PublishSingleFile=true \
    /p:UseAppHost=true \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app

COPY --from=publish /app/publish .

ENV PATH=/app:$PATH

RUN cp ServiceUploader service-uploader

RUN cd / && \
    service-uploader --version

ENTRYPOINT ["service-uploader"]
CMD ["--version"]
