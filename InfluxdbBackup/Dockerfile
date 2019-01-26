FROM influxdb:1.6.0-alpine as influxdb

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
 COPY bin/*/netcoreapp2.1/* ./
 ENTRYPOINT [ "dotnet", "InfluxdbBackup.dll" ]