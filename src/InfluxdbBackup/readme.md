# Docker InfluxDB to Azure blob storage  

This container periodically runs a backup of an InfluxDB database to an blob container.
It's possible to backup a specific database or all databases at once, all backups are made in the portable format. 
The number of backups maintained on blob storage can be configured.

## Configure InfluxDb for backups
To enable backup capabilities on influxdb databases you need to configure the bind-address:
```
-e INFLUXDB_BIND_ADDRESS=0.0.0.0:8088 
```

## Usage

### Default: cron 1am daily, Influxdb port 8088, 1 backup maintained on blob, all databases are backuped

```shell
docker run \
    -e INFLUXDB_HOST=192.168.3.4 \
    -e AZURE_STORAGEACCOUNT_NAME=backupstorage \
    -e AZURE_STORAGEACCOUNT_KEY=secret \
    michielvanwelsenaere/influxdb-to-azure:latest
```

### Custom

```shell
docker run \
    -e BACKUP_CRON="0 0 2 1/1 * ? *"
    -e INFLUXDB_DATABASE=cAdvisor \
    -e INFLUXDB_HOST=192.168.3.4 \
    -e INFLUXDB_PORT=8088 \
    -e AZURE_STORAGEACCOUNT_NAME=backupstorage \
    -e AZURE_STORAGEACCOUNT_KEY=secret \
    -e AZURE_STORAGEACCOUNT_CONTAINER=us-mycontainer \
    -e BACKUP_FILENAME=cadvisordailybackup \
    -e BACKUP_MAXBACKUPS=7 \
    michielvanwelsenaere/influxdb-to-azure:latest \
```

## Environment Variables

| Variable        | Description      | Example Usage  | Default   | Optional?  |
| --------------- |:---------------:| -----:| -----:| --------:|
| `INFLUXDB_DATABASE` | Database to backup  | `cAdvisor` | None   | No |
| `INFLUXDB_HOST`  | Hostname or IP of influxdb instance | `192.168.1.55` | None | No |
| `INFLUXDB_PORT` | Port of influxdb instance | `8088` | `8088` | Yes |
| `AZURE_STORAGEACCOUNT_NAME`    | Azure storage account name | `backupstorage` | None     | No |
| `AZURE_STORAGEACCOUNT_KEY`   |  Azure storage account Secret Key |  `secret` | None   | No |
| `AZURE_STORAGEACCOUNT_CONTAINER`   | Azure storage container name | `influxdbbackup`   | `influxdbbackup`   | Yes |
| `BACKUP_CRON` | Cron to run backup  | `1.2.3.4` | `localhost`   | Yes |
| `BACKUP_MAXBACKUPS` | Maximum amount of backups to maintain on Azure blob storage (max 100)  | `7` | `1`   | Yes |
| `BACKUP_FILENAME` | Prefix for blob upload  | `cadvisordailybackup` | `Influxdb_backup`   | Yes |