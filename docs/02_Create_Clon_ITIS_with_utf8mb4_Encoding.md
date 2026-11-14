- # Step 02: Create a Clone of the ITIS Database with `utf8mb4` Encoding

  *Guide updated on November 10, 2026.*

  ## 1. Objective

  - Create **ITIS_utf8mb4**, a working clone where the modifications, cleanup, and transformations needed to use the database in multilingual projects will be carried out. This guide uses Testudines as its example.

  The original ITIS database installed in the previous step will retain its source structure and encoding. A clone named `ITIS_utf8mb4` will be created using InnoDB and `utf8mb4`.

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
  3. Using MyISAM caused index length issues when working with `utf8mb4`.
  4. The working database was ultimately converted to `utf8mb4` for use in modern, multilingual environments.
  5. `utf8mb4_bin` is used as the collation to preserve binary string comparisons.

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

  # 3. ITIS database

  To follow these steps, the database must already be installed on the server. If it is not, follow the [ITIS Installation Guide](01_Instalacion_ITIS.md) before continuing.

  ------

  # 4. Create the ITIS_utf8mb4 working database

  `ITIS_utf8mb4` will be a second copy of the original database.

  It will initially be imported using the original `latin1` encoding. It will then be converted to `utf8mb4`.

  ## 4.1. Create the database

  In MariaDB, run:

  ```
  DROP DATABASE IF EXISTS ITIS_utf8mb4;
  
  CREATE DATABASE ITIS_utf8mb4
  CHARACTER SET latin1
  COLLATE latin1_bin;
  ```

  Verify:

  ```
  SELECT
      SCHEMA_NAME,
      DEFAULT_CHARACTER_SET_NAME,
      DEFAULT_COLLATION_NAME
  FROM information_schema.SCHEMATA
  WHERE SCHEMA_NAME = 'ITIS_utf8mb4';
  ```

  Expected result:

  ```
  latin1
  latin1_bin
  ```

  ## 4.2. Import the data

  Exit MariaDB:

  ```
  EXIT;
  ```

  From WSL:

  ```
  cd /mnt/d/itis_download
  
  ls
  ```

  Run:

  ```
  mariadb -u your_user_id -p \
  --default-character-set=latin1 \
  ITIS_utf8mb4 < ITIS.sql
  ```

  The import may take a few minutes.

  ## 4.3. Verify the installation

  Connect to MariaDB:

  ```
  mariadb -u your_user_id -p
  ```

  Run:

  ```
  USE ITIS_utf8mb4;
  
  SHOW TABLES;
  ```

  Check that the expected tables are present.

  ------

  # 5. Check special characters

  This step aims to prevent issues with special characters, such as:

  ```
  209402|(G�nther, 1999)|2019-09-27|5|G�nther 1999
  75288|(G�nther, 1867)|2004-01-21|5|G�nther 1867
  208783|(G�nther, 2009)|2019-09-27|5|G�nther 2009
  ```

  Before changing the encoding, check that special characters were imported correctly.

  For example, check author names containing accented characters:

  ```
  SELECT
      shortauthor,
      HEX(shortauthor)
  FROM ITIS_utf8mb4.strippedauthor
  WHERE shortauthor LIKE '%G%nther%'
     OR shortauthor LIKE '%Dum%ril%'
  LIMIT 30;
  ```

  Inspect the results visually.

  The `HEX` column lets you examine the hexadecimal representation of the characters and detect possible import issues.

  **Do not continue if you find incorrect characters.**

  ------

  # 6. Convert the tables to InnoDB

  This step is important.

  The original import may create MyISAM tables, even if InnoDB is configured as the default engine in MariaDB.

  The engine explicitly specified in a `CREATE TABLE` statement in the SQL file takes precedence over the default engine.

  Therefore, check the tables and convert them if needed.

  ## 6.1. Check the engines in use

  ```
  SELECT
      ENGINE,
      COUNT(*) AS TotalTables
  FROM information_schema.TABLES
  WHERE TABLE_SCHEMA = 'ITIS_utf8mb4'
    AND TABLE_TYPE = 'BASE TABLE'
  GROUP BY ENGINE;
  ```

  The results show how many tables use each engine.

  ## 6.2. Generate the conversion statements

  Generate the required statements automatically:

  ```
  SELECT
      CONCAT(
          'ALTER TABLE `ITIS_utf8mb4`.`',
          TABLE_NAME,
          '` ENGINE=InnoDB, ROW_FORMAT=DYNAMIC;'
      ) AS Script
  FROM information_schema.TABLES
  WHERE TABLE_SCHEMA = 'ITIS_utf8mb4'
    AND TABLE_TYPE = 'BASE TABLE'
    AND ENGINE <> 'InnoDB'
  ORDER BY TABLE_NAME;
  ```

  Copy and run the results.

  This converts all tables that are not using InnoDB.

  ## 6.3. Check the result

  ```
  SELECT
      ENGINE,
      COUNT(*) AS TotalTables
  FROM information_schema.TABLES
  WHERE TABLE_SCHEMA = 'ITIS_utf8mb4'
    AND TABLE_TYPE = 'BASE TABLE'
  GROUP BY ENGINE;
  ```

  Expected result:

  ```
  InnoDB | 25
  ```

  The exact number depends on which tables are present in the installation.

  ### Check the row format

  ```
  SELECT
      NAME,
      ROW_FORMAT
  FROM information_schema.INNODB_SYS_TABLES
  WHERE NAME LIKE 'ITIS_utf8mb4/%'
    AND ROW_FORMAT <> 'Dynamic';
  ```

  If this returns any rows, review those tables.

  To convert a specific table:

  ```
  ALTER TABLE ITIS_utf8mb4.nombre_tabla
  ROW_FORMAT = DYNAMIC;
  ```

  ------

  # 7. Change the encoding of ITIS_utf8mb4 to `utf8mb4`

  Once you have verified that all tables use InnoDB, you can proceed with the conversion.

  ## 7.1. Change the database's default encoding

  ```
  ALTER DATABASE ITIS_utf8mb4
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_bin;
  ```

  **Important:** `ALTER DATABASE` changes the database defaults, but does not automatically convert the columns in existing tables.

  Therefore, convert the tables individually.

  ## 7.2. Generate the conversion statements

  Run:

  ```
  SELECT
      CONCAT(
          'ALTER TABLE `ITIS_utf8mb4`.`',
          TABLE_NAME,
          '` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_bin;'
      ) AS Script
  FROM information_schema.TABLES
  WHERE TABLE_SCHEMA = 'ITIS_utf8mb4'
    AND TABLE_TYPE = 'BASE TABLE'
  ORDER BY TABLE_NAME;
  ```

  Copy and run the generated statements.

  The result will contain one `ALTER TABLE` statement for each table.

  Example:

  ```
  ALTER TABLE `ITIS_utf8mb4`.`change_comments`
  CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_bin;
  
  ALTER TABLE `ITIS_utf8mb4`.`change_operations`
  CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_bin;
  
  ALTER TABLE `ITIS_utf8mb4`.`hierarchy`
  CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_bin;
  ```

  Continue until all tables have been converted.

  ------

  # 8. Verify the character conversion

  ## 8.1. Check that no columns remain unconverted

  Run:

  ```
  SELECT
      TABLE_NAME,
      COLUMN_NAME,
      CHARACTER_SET_NAME,
      COLLATION_NAME,
      COLUMN_TYPE
  FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = 'ITIS_utf8mb4'
    AND CHARACTER_SET_NAME IS NOT NULL
    AND (
        CHARACTER_SET_NAME <> 'utf8mb4'
        OR COLLATION_NAME <> 'utf8mb4_bin'
    )
  ORDER BY TABLE_NAME, ORDINAL_POSITION;
  ```

  **Expected result: no rows.**

  If any rows appear, some columns have not yet been converted.

  ## 8.2. Verify the converted columns

  ```
  SELECT
      TABLE_NAME,
      COLUMN_NAME,
      CHARACTER_SET_NAME,
      COLLATION_NAME,
      COLUMN_TYPE
  FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = 'ITIS_utf8mb4'
    AND CHARACTER_SET_NAME IS NOT NULL
  ORDER BY TABLE_NAME, ORDINAL_POSITION;
  ```

  All text columns should show:

  ```
  CHARACTER_SET_NAME = utf8mb4
  COLLATION_NAME     = utf8mb4_bin
  ```

  Numeric, date, and other non-text columns do not have a character encoding, so they will not appear in this check.

  ## 8.3. Check the database encoding

  ```
  SELECT
      SCHEMA_NAME,
      DEFAULT_CHARACTER_SET_NAME,
      DEFAULT_COLLATION_NAME
  FROM information_schema.SCHEMATA
  WHERE SCHEMA_NAME = 'ITIS_utf8mb4';
  ```

  Expected result:

  ```
  utf8mb4
  utf8mb4_bin
  ```

  ------

  # 9. Final engine verification

  Check the storage engines again:

  ```
  SELECT
      ENGINE,
      COUNT(*) AS TotalTables
  FROM information_schema.TABLES
  WHERE TABLE_SCHEMA = 'ITIS_utf8mb4'
    AND TABLE_TYPE = 'BASE TABLE'
  GROUP BY ENGINE;
  ```

  Expected result:

  ```
  InnoDB | 25
  ```

  There should be no MyISAM tables remaining.

  Also check that the original ITIS database retains its original encoding and engines:

  ```
  SELECT
      TABLE_SCHEMA,
      ENGINE,
      COUNT(*) AS TotalTables
  FROM information_schema.TABLES
  WHERE TABLE_SCHEMA IN ('ITIS', 'ITIS_utf8mb4')
    AND TABLE_TYPE = 'BASE TABLE'
  GROUP BY TABLE_SCHEMA, ENGINE
  ORDER BY TABLE_SCHEMA, ENGINE;
  ```

  This query lets you compare the two installations.

  ------

  # 10. Back up ITIS_utf8mb4

  After completing the conversions and checks, create a backup.

  From WSL:

  ```
  mariadb-dump -u your_user_id -p \
  --default-character-set=utf8mb4 \
  --databases ITIS_utf8mb4 > ITIS_utf8mb4_full.sql
  ```

  Save the file in a safe location.

  This backup will be the starting point for future database cleanup and transformation.

  ------

  # 11. Configure InnoDB as MariaDB's default engine

  *This section is optional but recommended.*

  The goal is to prevent new tables from being created with MyISAM when no engine is explicitly specified.

  ## 11.1. Locate the configuration file

  On Ubuntu, it is usually located at:

  ```
  /etc/mysql/mariadb.conf.d/50-server.cnf
  ```

  Edit it:

  ```
  sudo nano /etc/mysql/mariadb.conf.d/50-server.cnf
  ```

  ## 11.2. Modify the `[mysqld]` section

  Within the `[mysqld]` section, add or modify:

  ```
  [mysqld]
  default-storage-engine = InnoDB
  ```

  You can also optionally add:

  ```
  enforce_storage_engine = InnoDB
  ```

  This second option prevents tables from being created with another storage engine, although the behavior when another engine is explicitly specified also depends on the `NO_ENGINE_SUBSTITUTION` SQL mode.

  **Caution:** I do not recommend enabling this second option while importing external databases that may explicitly specify other engines.

  ## 11.3. Restart MariaDB

  ```
  sudo systemctl restart mariadb
  ```

  ## 11.4. Check the configuration

  ```
  sudo systemctl status mariadb
  ```

  Connect to MariaDB:

  ```
  mariadb -u your_user_id -p
  ```

  Run:

  ```
  SHOW VARIABLES LIKE 'default_storage_engine';
  
  SHOW VARIABLES LIKE 'enforce_storage_engine';
  ```

  The default engine should be InnoDB.

  ------

  # 12. Final state

  When the procedure is complete, you will have two independent databases:

  | Feature    | ITIS                   | ITIS_utf8mb4     |
  | ---------- | ---------------------- | ---------------- |
  | Source     | Official ITIS download | Clone of ITIS    |
  | Encoding   | `latin1`               | `utf8mb4`        |
  | Collation  | `latin1_bin`           | `utf8mb4_bin`    |
  | Engine     | Original               | InnoDB           |
  | ROW_FORMAT | Original               | Dynamic          |
  | Modified   | No                     | Yes              |
  | Use        | Reference              | Working database |

  **Result:**

  - ITIS retains a reference copy of the original data.
  - ITIS_utf8mb4 is ready for cleanup, transformation, and adaptation.
  - An initial backup of ITIS_utf8mb4 is available so the work can be repeated without downloading and importing the original database again.
