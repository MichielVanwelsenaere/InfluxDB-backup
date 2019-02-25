# BUILD IMAGE
FROM microsoft/dotnet:sdk AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out


# RUNTIME IMAGE
FROM influxdb:1.7.4-alpine as influxdb

FROM microsoft/dotnet:2.1-runtime-alpine3.7
COPY --from=influxdb /usr/bin/influxd /usr/bin/

RUN apk add --no-cache tzdata bash

# ----------------- Create ENV variables and set default values -------------------- #
ENV INFLUXDB_ACTION="null"
ENV INFLUXDB_ACTION_CRON="null"
ENV INFLUXDB_BACKUPMEDIUM="null"
ENV INFLUXDB_DATABASE="null"
ENV INFLUXDB_HOST="null"
ENV INFLUXDB_PORT="null"

ENV AZURE_STORAGEACCOUNT_NAME="null"
ENV AZURE_STORAGEACCOUNT_KEY="null"
ENV AZURE_STORAGEACCOUNT_CONTAINER="influxdbbackup"

ENV BACKUP_MAXBACKUPS="1"
ENV BACKUP_FILENAME="Influxdb_backup"
# ---------------------------------------------------------------------------------- #

 WORKDIR /app
 COPY --from=build-env /app/out .
 ENTRYPOINT [ "dotnet", "InfluxdbBackup.dll" ]