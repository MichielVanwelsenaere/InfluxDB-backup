# InfluxDB backup docker image
![Release version](https://img.shields.io/github/release/MichielVanwelsenaere/InfluxDB-backup.svg)
![Image Size](https://img.shields.io/microbadger/image-size/michielvanwelsenaere/influxdb-backup.svg)
[![Renovate enabled](https://img.shields.io/badge/renovate-enabled-brightgreen.svg)](https://renovatebot.com/)
![build pipeline](https://michielvanwelsenaere.visualstudio.com/Public/_apis/build/status/InfluxDB-backup-CI?branchName=master)
![release pipeline](https://michielvanwelsenaere.vsrm.visualstudio.com/_apis/public/Release/badge/c40cb7e2-85fc-4ca0-b71e-da4ac6783b50/1/1)
![License](https://img.shields.io/github/license/MichielVanwelsenaere/InfluxDB-backup.svg)

This container periodically runs a backup of an InfluxDB database to your backup (cloud) medium of choice.
It's possible to backup a specific database or all databases at once, all backups are made in the portable format. 
The number of backups maintained on (cloud) storage can be configured.
Functionality available to restore the latest backup from your configured (cloud) medium.

## Configure InfluxDB for backups
To enable backup capabilities on your influxDB databases you need to configure the bind-address:
```
-e INFLUXDB_BIND_ADDRESS=0.0.0.0:8088 
```
related influxDB docs to this can be found [here](https://docs.influxdata.com/influxdb/v1.7/administration/config/).

## Usage
Example using docker-compose:

Cron 1am daily, InfluxDB port 8088, backup files persisted on Azure blob, all databases are backuped. Backups are created with prefix name 'MyBackupFileNamePrefix' in container 'customContainername', 99 backups maintained in total on backup medium. Logfiles are persisted locally.

```yaml
version: '3.5'

services:
  # InfluxDB to persist data
  Influxdb:
    hostname: "Influxdatabase"
    image: "influxdb:1.6.4"
    environment:
      INFLUXDB_BIND_ADDRESS: "0.0.0.0:8088"
    ports:
      # HTTP API port
      - "8086:8086"
      # BIND ADDRESS FOR BACKUPS  
      - "8088:8088"
    networks:      
      backend:
        aliases:
          - "influxdatabase"

    # For backing up influxDB
  InfluxdbBackup:
    image: "michielvanwelsenaere/influxdb-backup:latest"
    volumes:
      - "./influxdbbackup/log:/influxdbbackup/log"
    environment:
      INFLUXDB_ACTION: "fullbackup"
      INFLUXDB_ACTION_CRON: "0 0 1 1/1 * ? *"
      INFLUXDB_HOST: "influxdatabase"
      INFLUXDB_PORT: "8088"
      INFLUXDB_BACKUPMEDIUM: "AzureBlob"
      AZURE_STORAGEACCOUNT_NAME: "your storage account name"
      AZURE_STORAGEACCOUNT_KEY: "your storage account key"
      AZURE_STORAGEACCOUNT_CONTAINER: "customContainername"
      BACKUP_MAXBACKUPS: "99"
      BACKUP_FILENAME: "MyBackupFileNamePrefix"
    depends_on: 
     - Influxdb
    networks:
      backend:

networks:
  backend:
```


## Environment Variables

### General

| Variable        | Description      | Example Usage  | Default   | Optional?  |
| --------------- |:---------------| -----:| -----:| --------:|
| `INFLUXDB_ACTION` | 'fullbackup' to create a backup or 'restorefullbackup' to restore the latest backup from the cloud. backups can't be restored if a database with the same name already exists.  | `cAdvisor` | None   | No |
| `INFLUXDB_ACTION_CRON`  | Cron expression to execute 'INFLUXDB_ACTION', set to 'Single' to run the action just once. Container will exit afterwards. | `0 0 1 1/1 * ? *` | None | No |
| `INFLUXDB_BACKUPMEDIUM` | Backup medium to persist database backup to. | `Azureblob` | None | No |
| `INFLUXDB_DATABASE`    | Database to backup, leave empty to backup all influxDB databases.  | `cAdvisor` | None  | Yes |
| `INFLUXDB_HOST`   |  Hostname or IP of your influxDB database host. |  `192.168.1.12` | None   | No |
| `INFLUXDB_PORT`   | InfluxDB configured bind port. | `8088`   | None   | No |
| `BACKUP_MAXBACKUPS` | Number of backups to maintain on backup medium (max 99).  | `99` | `1`   | Yes |
| `BACKUP_FILENAME` | Prefix name for the database backup file. | `cAdvisor_Backup` | `influxdbbackup`   | Yes |

### Backup medium specific
```yaml
INFLUXDB_BACKUPMEDIUM: "Azureblob"
```
more information on how to create an Azure storage account can be found [here](https://docs.microsoft.com/en-us/azure/storage/common/storage-quickstart-create-account?tabs=azure-portal).

| Variable        | Description      | Example Usage  | Default   | Optional?  |
| --------------- |:---------------| -----:| -----:| --------:|
| `AZURE_STORAGEACCOUNT_NAME` | Name of the Azure storage account to persist backup file to. | `cadvisordailybackup` | None   | No |
| `AZURE_STORAGEACCOUNT_KEY` | Key of the Azure storage account to persist backup file to.  | `{Secret key}` | None   | No |
| `AZURE_STORAGEACCOUNT_CONTAINER` | Azure blob container to persist the backup file to, will be created if not exists.  | `cadvisordailybackup` | `influxdbbackup`   | Yes |

***

```yaml
INFLUXDB_BACKUPMEDIUM: "LocalDirectory"
```
Specifying `LocalDirectory` as backup medium will create a zipped backup file in container path `/influxdbbackup/data`. To persist the backup files locally add the container path to the volumes to persist:

```yaml
volumes:
      - "./influxdbbackup/log:/influxdbbackup/log"
      - "./influxdbbackup/data:/influxdbbackup/data"
```
This allows backups to be persisted to a local drive or network share (requires network share to be mounted locally on device).

***
