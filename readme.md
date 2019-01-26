# Docker InfluxDB to Azure blob storage  

This container periodically runs a backup of an InfluxDB database to your backup cloud medium of choice.
It's possible to backup a specific database or all databases at once, all backups are made in the portable format. 
The number of backups maintained on cloud storage can be configured.
Functionality implemented to restore the latest backup from cloud medium as well.

## Configure InfluxDb for backups
To enable backup capabilities on influxdb databases you need to configure the bind-address:
```
-e INFLUXDB_BIND_ADDRESS=0.0.0.0:8088 
```

## Usage
Example using docker-compose:

Cron 1am daily, Influxdb port 8088, backups on Azure blob, all databases are backuped. Backups are created with prefix name 'MyBackupFileNamePrefix' in container 'customContainername', 99 backups maintained in total.

```yaml
version: '3.5'

services:
  # Influxdb to persist data
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

    # For backing up influxdb
  InfluxdbBackup:
    image: "influxdb-backup:latest"
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
| --------------- |:---------------:| -----:| -----:| --------:|
| `INFLUXDB_ACTION` | 'fullbackup' to create a backop or 'restorefullbackup' to restore the latest backup from the cloud. backups can't be restored if a database with the same name already exists  | `cAdvisor` | None   | No |
| `INFLUXDB_ACTION_CRON`  | Cron expression to execute 'INFLUXDB_ACTION', set to 'Single' to run the action just once. Container will exit afterwards | `0 0 1 1/1 * ? *` | None | No |
| `INFLUXDB_BACKUPMEDIUM` | Backup medium to persist database backup to. Currently only 'Azureblob' supported  | `Azureblob` | None | No |
| `INFLUXDB_DATABASE`    | Database to backup, leave empty to backup all influxdb databases  | `cAdvisor` | None  | Yes |
| `INFLUXDB_HOST`   |  Hostname or IP of your influxdb database host |  `192.168.1.12` | None   | No |
| `INFLUXDB_PORT`   | InfluxDb configured bind port | `8088`   | None   | No |
| `BACKUP_MAXBACKUPS` | Number of backups to maintain on backup medium (max 99)  | `99` | `1`   | Yes |
| `BACKUP_FILENAME` | Prefix name for the database backup file | `cAdvisor_Backup` | `influxdbbackup`   | Yes |

### Backup medium specific
```yaml
INFLUXDB_BACKUPMEDIUM: "Azureblob"
```
| Variable        | Description      | Example Usage  | Default   | Optional?  |
| --------------- |:---------------:| -----:| -----:| --------:|
| `AZURE_STORAGEACCOUNT_NAME` | Name of the Azure storage account to persist backuk file to | `cadvisordailybackup` | None   | No |
| `AZURE_STORAGEACCOUNT_KEY` | Key of the Azure storage account to persist backuk file to  | `cadvisordailybackup` | `ATl4/zpmrhWFboH03n6/SP7x0/PytiGdArpgqKP7xwR3vWvTnz/x6zG1rPyNGsqfL0b0bw==`   | No |
| `AZURE_STORAGEACCOUNT_CONTAINER` | Azure blob container to persist the backup file to, will be created if not exists  | `cadvisordailybackup` | `influxdbbackup`   | Yes |

