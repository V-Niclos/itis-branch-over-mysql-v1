# Step 1. Download and Installation

*Guide updated on November 10, 2026.*

## 1. Objective

Install a complete copy of the ITIS database locally:

- **ITIS:** the original, immutable clone of the database downloaded from ITIS. It will be kept as a reference for queries and verification. The original ITIS database will retain its source structure and encoding.

## 2. Environment

- MariaDB: `11.8.6-MariaDB-5ubuntu0.1`
- System: Ubuntu on WSL
- Machine: Windows 11
- Administration tool: MariaDB console

Database engine configuration:

```
default_storage_engine = InnoDB
```

### Important notes

The following issues were identified during testing:

1. Using HeidiSQL for certain import processes caused encoding errors.
2. Editing SQL files with some text editors altered their encoding.
3. Using MyISAM caused issues with index length. Latin-1 caused problems with character representation.
4. The next step in the tutorial creates a `utf8mb4` clone for use in modern, multilingual environments, avoiding all the issues identified.

### Check the environment

In MariaDB, run:

```
SELECT VERSION();

SHOW VARIABLES LIKE 'default_storage_engine';

SHOW VARIABLES LIKE 'innodb_default_row_format';
```

Expected results:

- Default engine: InnoDB.
- InnoDB default row format: Dynamic.

------

# 3. Download the database from ITIS

Go to:

https://www.itis.gov/downloads/

Download:

**Full ITIS Data Set (MySQL bulk load)**
`itisMySQLBulk.zip`

Save the file to a short path to make the following steps easier.

Example:

```
D:\itis_download
```

Extract the archive.

The `ITIS.sql` file contains the table creation and data insertion scripts.

Copy `ITIS.sql` directly to:

```
D:\itis_download\ITIS.sql
```

We will use this location for both imports.

**Important:** Do not modify or edit the original `ITIS.sql` file.

------

# 4. Install the original ITIS clone

This clone will remain immutable. It will be used exclusively as a reference for verification and comparison with the working database.

## 4.1. Open MariaDB

Open PowerShell and enter WSL:

```
wsl
```

Connect to MariaDB:

```
mariadb -u your_user_id -p
```

## 4.2. Create the ITIS database

If an ITIS database already exists, back it up first.

Create the database using the original encoding:

```
DROP DATABASE IF EXISTS ITIS;

CREATE DATABASE ITIS
CHARACTER SET latin1
COLLATE latin1_bin;
```

### Check the encoding

```
SELECT
    SCHEMA_NAME,
    DEFAULT_CHARACTER_SET_NAME,
    DEFAULT_COLLATION_NAME
FROM information_schema.SCHEMATA
WHERE SCHEMA_NAME = 'ITIS';
```

Expected result:

```
latin1
latin1_bin
```

## 4.3. Import the data

Exit MariaDB:

```
EXIT;
```

From the WSL console, go to the directory containing `ITIS.sql`.

In WSL, the Windows `D:` drive is located at `/mnt/d/`.

```
cd /mnt/d/itis_download
ls
```

Check that `ITIS.sql` appears in the listing.

Run the import:

```
mariadb -u your_user_id -p \
  --default-character-set=latin1 \
  ITIS < ITIS.sql
```

The `--default-character-set=latin1` option is important so the original data is interpreted correctly.

## 4.4. Verify the installation

Connect to MariaDB again:

```
mariadb -u your_user_id -p
```

Run:

```
USE ITIS;
SHOW TABLES;
```

Approximately 25 tables should appear.

The ITIS database is now installed.

**From this point on, do not modify the ITIS database.**
