#!/bin/bash
rm -rf ./build && \
mkdir -p ./build && \
cd ServiceUploader && \
dotnet publish \
    -c Release \
    --self-contained true \
    -r linux-x64 \
    /p:PublishSingleFile=true \
    /p:UseAppHost=true \
    -o ../build && \
cd .. && \
rm -rf ./service-uploader && \
mkdir ./service-uploader && \
cp -r ./service-uploader-ex/* ./service-uploader && \
echo >> ./service-uploader/DEBIAN/control && \
echo "Version: $(grep '<Version>' ./ServiceUploader/ServiceUploader.csproj | sed 's/.*<Version>\(.*\)<\/Version>.*/\1/')" >> ./service-uploader/DEBIAN/control && \
mkdir -p ./service-uploader/usr/bin && \
cp build/ServiceUploader ./service-uploader/usr/bin/service-uploader && \
rm -rf build && \
dpkg-deb -Z none --build service-uploader && \
rm -rf ./service-uploader