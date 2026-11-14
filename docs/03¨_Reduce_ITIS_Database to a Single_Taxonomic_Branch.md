```sql

```

# Step 03: Reduce the ITIS Database to a Single Taxonomic Branch

**ITIS: Integrated Taxonomic Information System**

**Filtering objective**

1. Create a clone containing a single taxonomic branch from a full ITIS database clone. The resulting clone will contain only the selected branch, including its ancestors, descendants, and synonyms.
2. Encode the database as `utf8mb4` to support multilingual characters and modern applications.
3. This guide uses the *Testudines* taxon, with TSN `948936`, as its example.

------

## Contents

- [Step 1: General considerations](#step-1-general-considerations)
- [Step 2: Clone ITIS_utf8mb4 to testudines_itis](#step-2-clone-itis_utf8mb4-to-testudines_itis)
- [Step 3: Import considerations](#step-3-import-considerations)
- [Step 4: Create a working table with all TSNs related to Testudines](#step-4-create-a-working-table-with-all-tsns-related-to-testudines)
- [Step 5: Clean up tables](#step-5-clean-up-tables)
- [Step 6: Delete working tables](#step-6-delete-working-tables)
- [Step 9: .NET scaffolding](#step-9-net-scaffolding)

------

## Step 1: General considerations

This tutorial uses the *Testudines* order, whose TSN is `948936`. Change this number to the TSN of the branch you want to use.

The source database for the clone is **ITIS_utf8mb4**. You can create it by following this guide (**⚠️ TODO: add link**).

The reason for using `utf8mb4` is to support modern multilingual applications and prevent encoding errors that can cause characters to display incorrectly.

The resulting database will be named **`testudines_itis`**.

The environment used is:

- MariaDB: `11.8.6-MariaDB-5ubuntu0.1`
- System: Ubuntu on WSL
- Machine: Windows 11

------

## Step 2: Clone ITIS_utf8mb4 to testudines_itis

In PowerShell, connect to MariaDB:

```
wsl
mariadb -u your_user_id -p
```

Create the destination database `testudines_itis`. The collation is set so that it does not distinguish between uppercase and lowercase letters or accented characters.

```
DROP DATABASE IF EXISTS testudines_itis;
CREATE DATABASE testudines_itis
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;
SHOW DATABASES;
```

Import the data into `testudines_itis`:

```
mariadb -u your_user_id -p --default-character-set=utf8mb4 testudines_itis < ITIS_UTF.sql
```

------

## Step 4: Verify that Steps 2 and 3 completed successfully

### 4.1 Run the following verification script to check that the data is consistent between both databases before applying filters:

```
/* Check consistency between the ITIS and testudines_itis databases */
SELECT 
    (SELECT COUNT(*) FROM itis.hierarchy) AS itis_hierarchy,
    (SELECT COUNT(*) FROM testudines_itis.hierarchy) AS testudines_itis_hierarchy,
    
    (SELECT COUNT(*) FROM itis.synonym_links) AS itis_synonym_links,
    (SELECT COUNT(*) FROM testudines_itis.synonym_links) AS testudines_itis_synonym_links,
       
    (SELECT COUNT(*) FROM itis.taxonomic_units) AS itis_taxonomic_units,
    (SELECT COUNT(*) FROM testudines_itis.taxonomic_units) AS testudines_itis_taxonomic_units;
```

> 📊 **Example validation:** Both databases should report exactly the same initial number of records:

| itis_hierarchy | testudines_itis_hierarchy | itis_synonym_links | testudines_itis_synonym_links | itis_taxonomic_units | testudines_itis_taxonomic_units |
| -------------- | ------------------------- | ------------------ | ----------------------------- | -------------------- | ------------------------------- |
| 6              | 6                         | 3                  | 3                             | 1                    | 1                               |
| 8              | 8                         | 1                  | 1                             | 0                    | 0                               |
| 3              | 3                         | 9                  | 9                             | 0                    | 0                               |
| 9              | 9                         | 8                  | 8                             | 3                    | 3                               |
| 5              | 5                         | 1                  | 1                             | 4                    | 4                               |
| 0              | 0                         | 3                  | 3                             | 9                    | 9                               |
|                |                           |                    |                               | 2                    | 2                               |

------

### 4.2 Optional: Visually check for suspiciously encoded characters

This may take a while.

Search `VARCHAR` fields in all tables for characters that may indicate encoding problems. If there are only a few, it is likely because:

```
USE testudines_itis;

DELIMITER //

DROP PROCEDURE IF EXISTS buscar_errores_codificacion//

CREATE PROCEDURE buscar_errores_codificacion(IN db_nombre VARCHAR(64))
BEGIN
    DECLARE fin INT DEFAULT FALSE;
    DECLARE v_tabla VARCHAR(64);
    DECLARE v_columna VARCHAR(64);
    
    -- Cursor to find all text columns in the database
    DECLARE cur_columnas CURSOR FOR 
        SELECT TABLE_NAME, COLUMN_NAME
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = db_nombre
          AND DATA_TYPE IN ('char', 'varchar', 'tinytext', 'text', 'mediumtext', 'longtext');
          
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET fin = TRUE;

    -- Temporary table to store findings without filling the console with empty results
    DROP TEMPORARY TABLE IF EXISTS tmp_errores_encoding;
    CREATE TEMPORARY TABLE tmp_errores_encoding (
        tabla VARCHAR(64),
        columna VARCHAR(64),
        valor_corrupto TEXT,
        valor_hex TEXT
    );

    OPEN cur_columnas;

    bucle_lectura: LOOP
        FETCH cur_columnas INTO v_tabla, v_columna;
        IF fin THEN
            LEAVE bucle_lectura;
        END IF;

        -- Build a dynamic query to search for common patterns of double encoding or data loss
        SET @sql_query = CONCAT(
            'INSERT INTO tmp_errores_encoding (tabla, columna, valor_corrupto, valor_hex) ',
            'SELECT ''', v_tabla, ''', ''', v_columna, ''', `', v_columna, '`, HEX(`', v_columna, '`) ',
            'FROM `', db_nombre, '`.`', v_tabla, '` ',
            'WHERE `', v_columna, '` LIKE ''%Ã%'' ',       -- Double UTF-8 encoding over Latin1 (e.g. Ã±, Ã©, Ã¼)
            '   OR `', v_columna, '` LIKE ''%Â%'' ',       -- Strange characters before symbols (e.g. Â°, Â©)
            '   OR `', v_columna, '` LIKE ''%â€%'' ',      -- Misencoded quotation marks or dashes (e.g. â€œ, â€”)
/*            '   OR `', v_columna, '` LIKE ''%?%'' ',       -- Possible character loss where characters were replaced with question marks*/
            '   OR HEX(`', v_columna, '`) LIKE ''%EFBFBD%'' ', -- Unicode replacement character ()
            '   OR HEX(`', v_columna, '`) LIKE ''%C383%'' ',   -- Deeper double encoding (e.g. ÃƒÂ±)
            'LIMIT 50;' -- Limit per column to avoid locking very large tables
        );

        PREPARE stmt FROM @sql_query;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;

    END LOOP;

    CLOSE cur_columnas;

    -- Display all collected results
    SELECT * FROM tmp_errores_encoding;
    
    -- Cleanup
    DROP TEMPORARY TABLE IF EXISTS tmp_errores_encoding;
END//

DELIMITER ;

-- Run the procedure with your database name
CALL buscar_errores_codificacion('testudines_itis');

DROP PROCEDURE IF EXISTS buscar_errores_codificacion//
```

- //++++++++++++++++++++++++++++++++++++++++++++++++++//
- //++++++++++++++++++++++++++++++++++++++++++++++++++//

------

## Step 3: Import considerations

ℹ️ **Note 1:** This tutorial keeps *Testudines* as the selected branch. To use a different branch, replace the Testudines TSN (`948936`) with the TSN for your branch. For example, for *Reptilia*, use `173747`.

ℹ️ **Note 2:** Working tables and temporary backups will always use the `wrk_` (work) prefix. This makes them easy to identify and remove at the end of the process.

ℹ️ **Note 3:** The critical table in this process is `wrk_keep_tsn`. It is the first table you need to create. It temporarily and safely stores all valid TSN codes (ancestors and descendants) along with their associated synonyms for the *Testudines* branch.

It identifies the records to retain in the tables being cleaned (those containing a `tsn` field) and removes all records whose TSN is not in the table.

⚠️ **Warning:** Take special care when creating the `wrk_keep_tsn` table.

Creating the table involves two steps:

1. Add the valid TSNs for the ancestors and descendants.
2. Add the synonyms for the TSNs obtained in the previous step.

------

## Step 4: Create a working table with all TSNs related to Testudines

### 4.1 Identify the TSN for Testudines

Use a query on the `taxonomic_units` or `hierarchy` table to find the base identifier. For *Testudines*, the source TSN is `948936`.

### 4.2 Identify the ancestor TSNs of the Testudines branch

To find the ancestors and descendants of a branch, first retrieve the `hierarchy_string` for the branch you want to keep.

Run this query to retrieve the ancestor hierarchy string:

```
-- Get the ancestor hierarchy string
USE testudines_itis;
SELECT hierarchy_string 
FROM testudines_itis.hierarchy
WHERE tsn = 948936;
```

*Example result:* `202423-914154-914156-158852-331030-914179-914181-173747-948936`

The numeric codes returned in the string will be used as fixed values in Step 4.4.

### 4.3 Create the `wrk_keep_tsn` working table

This table acts as the retention index. Any record whose TSN is not on this list will be deleted later.

```
USE testudines_itis;

DROP TABLE IF EXISTS wrk_keep_tsn;

CREATE TABLE wrk_keep_tsn (
    tsn INT NOT NULL PRIMARY KEY
);
```

### 4.4 Insert the base data and verify it

In this first phase, direct and indirect taxonomic units are saved. Synonyms will be processed immediately afterward.

```
USE testudines_itis;

-- 1. Insert descendants and ancestors of Testudines, including the Testudines node itself

INSERT IGNORE INTO wrk_keep_tsn (tsn)
SELECT h.tsn
FROM hierarchy AS h
WHERE h.hierarchy_string LIKE '%948936%';

-- 2. Insert the direct ancestors of Testudines obtained in the previous hierarchy step
INSERT IGNORE INTO wrk_keep_tsn (tsn)
VALUES
    (202423),
    (914154),
    (914156),
    (158852),
    (331030),
    (914179),
    (914181),
    (173747);
```

Check the initial number of records loaded:

```
USE testudines_itis;
SELECT COUNT(*) AS total_tsn_to_keep FROM wrk_keep_tsn;
```

### 4.5 Insert taxonomic synonyms

#### 4.5.1 Check the valid synonyms to be inserted (Optional)

Use this query to review scientific name equivalencies before consolidating them:

```
USE testudines_itis;

SELECT 
    th.tsn AS accepted_tsn,
    tu_aceptado.complete_name AS accepted_name,
    ts.tsn AS synonym_tsn,
    tu_sinonimo.complete_name AS synonym_name,
    th.hierarchy_string
FROM hierarchy AS th
INNER JOIN synonym_links AS ts 
    ON th.tsn = ts.tsn_accepted
INNER JOIN taxonomic_units AS tu_aceptado 
    ON th.tsn = tu_aceptado.tsn
INNER JOIN taxonomic_units AS tu_sinonimo 
    ON ts.tsn = tu_sinonimo.tsn
WHERE th.hierarchy_string LIKE '%948936%'
ORDER BY 
    tu_aceptado.complete_name,
    tu_sinonimo.complete_name;
```

Results:

#### 4.5.2 Add the synonyms to the working table

```
USE testudines_itis;

INSERT IGNORE INTO wrk_keep_tsn (tsn)
SELECT DISTINCT sl.tsn
FROM synonym_links AS sl
INNER JOIN wrk_keep_tsn AS k
    ON sl.tsn_accepted = k.tsn;
```

#### 4.5.3 Final safety check

Check the consolidated total and make sure no synonym links remain orphaned outside our controlled set:

```
USE testudines_itis;

-- Count the total records in the retention table
SELECT COUNT(*) AS total_consolidated FROM wrk_keep_tsn;

-- Check for remaining records; this query MUST return ZERO
SELECT COUNT(*) AS orphaned_synonyms
FROM synonym_links AS sl
INNER JOIN wrk_keep_tsn AS k
    ON sl.tsn = k.tsn
WHERE sl.tsn_accepted NOT IN (SELECT tsn FROM wrk_keep_tsn);
```

> 🗄️ **Safety note:** Before proceeding with the bulk deletion, create a backup or a full restore point for the `testudines_itis` database.

From PowerShell:

```
mariadb-dump -u your_user_id -p \
--default-character-set=utf8mb4 \
--databases testudines_itis > testudines_itis_ful.sql
```

------

## Step 5: Remove unnecessary records

Unnecessary records are those whose TSN is not included in `wrk_keep_tsn`.

### 5.1 Back up the tables to be cleaned

Tables will be created with the prefix `wrk_*`, where `*` is the name of the original table.

```
USE testudines_itis;

DELIMITER $$

-- Drop the procedure if it already exists
DROP PROCEDURE IF EXISTS sp_backup_tablas_tsn$$

CREATE PROCEDURE sp_backup_tablas_tsn()
BEGIN
    DECLARE v_done INT DEFAULT FALSE;
    DECLARE v_tabla VARCHAR(255);
    
    -- Cursor to find base tables containing the required columns
    -- MariaDB column names are case-insensitive by default.
    -- LOWER() is used for compatibility with any server.
    DECLARE cur_tablas CURSOR FOR 
        SELECT DISTINCT c.TABLE_NAME
        FROM information_schema.COLUMNS c
        INNER JOIN information_schema.TABLES t 
            ON c.TABLE_SCHEMA = t.TABLE_SCHEMA 
            AND c.TABLE_NAME = t.TABLE_NAME
        WHERE c.TABLE_SCHEMA = 'testudines_itis'
          AND t.TABLE_TYPE = 'BASE TABLE'
          AND LOWER(c.COLUMN_NAME) IN ('tsn', 'tsn_acepted')
          AND LOWER(c.TABLE_NAME) NOT LIKE 'wrk_%';

    DECLARE CONTINUE HANDLER FOR NOT FOUND SET v_done = TRUE;

    OPEN cur_tablas;

    bucle_tablas: LOOP
        FETCH cur_tablas INTO v_tabla;
        IF v_done THEN
            LEAVE bucle_tablas;
        END IF;

        -- 1. Drop the previous backup if it already exists
        SET @sql_drop = CONCAT('DROP TABLE IF EXISTS `testudines_itis`.`wrk_', v_tabla, '`');
        PREPARE stmt_drop FROM @sql_drop;
        EXECUTE stmt_drop;
        DEALLOCATE PREPARE stmt_drop;

        -- 2. Create a new table with the same structure (primary key, indexes, and types)
        SET @sql_create = CONCAT('CREATE TABLE `testudines_itis`.`wrk_', v_tabla, '` LIKE `testudines_itis`.`', v_tabla, '`');
        PREPARE stmt_create FROM @sql_create;
        EXECUTE stmt_create;
        DEALLOCATE PREPARE stmt_create;

        -- 3. Copy all data from the original table to the backup
        SET @sql_insert = CONCAT('INSERT INTO `testudines_itis`.`wrk_', v_tabla, '` SELECT * FROM `testudines_itis`.`', v_tabla, '`');
        PREPARE stmt_insert FROM @sql_insert;
        EXECUTE stmt_insert;
        DEALLOCATE PREPARE stmt_insert;

    END LOOP;

    CLOSE cur_tablas;
END$$

DELIMITER ;

-- Run the procedure to create the backups
CALL sp_backup_tablas_tsn();

-- Drop the procedure after use
DROP PROCEDURE IF EXISTS sp_backup_tablas_tsn;
```

Check the backups:

```
SELECT 
    TABLE_NAME AS tableName,
    ENGINE AS engine,
    TABLE_ROWS AS rows
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'testudines_itis'
  AND TABLE_NAME LIKE 'wrk\_%'
ORDER BY TABLE_NAME;
```

------

### Pre-deletion check

```
SELECT 
    COALESCE(db1.TABLE_NAME, db2.TABLE_NAME) AS TableName,

    COALESCE(db1.TABLE_ROWS, 0) AS DB_ITIS,

    COALESCE(db2.TABLE_ROWS, 0) AS DB_testudines_itis,

    COALESCE(db1.TABLE_ROWS, 0) -
    COALESCE(db2.TABLE_ROWS, 0) AS Difference,

    CASE 
        WHEN db1.TABLE_NAME IS NULL THEN 'Only in testudines_itis'
        WHEN db2.TABLE_NAME IS NULL THEN 'Only in ITIS'
        WHEN db1.TABLE_ROWS = db2.TABLE_ROWS THEN '='
        WHEN db1.TABLE_ROWS > db2.TABLE_ROWS THEN '<'
        WHEN db1.TABLE_ROWS < db2.TABLE_ROWS THEN '>'
        ELSE '≠'
    END AS Comparison

FROM 
    (
        SELECT TABLE_NAME, TABLE_ROWS
        FROM INFORMATION_SCHEMA.TABLES
        WHERE TABLE_SCHEMA = 'ITIS'
          AND TABLE_TYPE = 'BASE TABLE'
          AND TABLE_NAME NOT LIKE 'wrk\_%'
          AND TABLE_NAME NOT LIKE 'tmp\_%'
    ) db1

LEFT JOIN 
    (
        SELECT TABLE_NAME, TABLE_ROWS
        FROM INFORMATION_SCHEMA.TABLES
        WHERE TABLE_SCHEMA = 'testudines_itis'
          AND TABLE_TYPE = 'BASE TABLE'
          AND TABLE_NAME NOT LIKE 'wrk\_%'
          AND TABLE_NAME NOT LIKE 'tmp\_%'
    ) db2

ON db1.TABLE_NAME = db2.TABLE_NAME

UNION ALL

SELECT 
    db2.TABLE_NAME AS TableName,

    0 AS DB_ITIS,

    COALESCE(db2.TABLE_ROWS, 0) AS DB_testudines_itis,

    -COALESCE(db2.TABLE_ROWS, 0) AS Difference,

    'Only in testudines_itis' AS Comparison

FROM 
    (
        SELECT TABLE_NAME, TABLE_ROWS
        FROM INFORMATION_SCHEMA.TABLES
        WHERE TABLE_SCHEMA = 'ITIS'
          AND TABLE_TYPE = 'BASE TABLE'
          AND TABLE_NAME NOT LIKE 'wrk\_%'
          AND TABLE_NAME NOT LIKE 'tmp\_%'
    ) db1

RIGHT JOIN 
    (
        SELECT TABLE_NAME, TABLE_ROWS
        FROM INFORMATION_SCHEMA.TABLES
        WHERE TABLE_SCHEMA = 'testudines_itis'
          AND TABLE_TYPE = 'BASE TABLE'
          AND TABLE_NAME NOT LIKE 'wrk\_%'
          AND TABLE_NAME NOT LIKE 'tmp\_%'
    ) db2

ON db1.TABLE_NAME = db2.TABLE_NAME

WHERE db1.TABLE_NAME IS NULL

ORDER BY TableName;
```

------

## Step 6: Clean up tables

Run this query to create the stored procedure in the database:

```
/*
----------------------------------------------------------
How to run the script:
CALL `testudines_itis`.`sp_clean_itis_tables`('EXECUTE');
At the end, COMMIT to delete the rows,
or ROLLBACK to keep the rows.
--------------------------------------------------------
*/

DELIMITER $$

DROP PROCEDURE IF EXISTS `testudines_itis`.`sp_clean_itis_tables`$$

CREATE PROCEDURE `testudines_itis`.`sp_clean_itis_tables`(IN p_mode VARCHAR(20))
BEGIN
    DECLARE v_done INT DEFAULT 0;
    DECLARE v_table VARCHAR(255);
    DECLARE v_field VARCHAR(50);
    DECLARE v_count_original BIGINT DEFAULT 0;
    DECLARE v_count_backup BIGINT DEFAULT 0;
    DECLARE v_before BIGINT DEFAULT 0;
    DECLARE v_deleted BIGINT DEFAULT 0;
    DECLARE v_after BIGINT DEFAULT 0;
    DECLARE v_error_msg VARCHAR(255);

    -- Cursor: tables with tsn (or tsn_accepted in synonym_links)
    DECLARE cur_tables CURSOR FOR
        SELECT DISTINCT T.TABLE_NAME
        FROM information_schema.TABLES T
        INNER JOIN information_schema.COLUMNS C
            ON T.TABLE_SCHEMA = C.TABLE_SCHEMA
            AND T.TABLE_NAME = C.TABLE_NAME
        WHERE T.TABLE_SCHEMA = 'testudines_itis'
          AND T.TABLE_TYPE = 'BASE TABLE'
          AND (
              C.COLUMN_NAME = 'tsn'
              OR (T.TABLE_NAME = 'synonym_links' AND C.COLUMN_NAME = 'tsn_accepted')
          )
          AND T.TABLE_NAME NOT LIKE 'wrk\_%'
          AND T.TABLE_NAME <> 'wrk_keep_tsn'
        ORDER BY T.TABLE_NAME;

    -- End-of-cursor handler
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET v_done = 1;

    -- SQL error handler to roll back the transaction if something fails
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        DROP TEMPORARY TABLE IF EXISTS tmp_clean_report;
        RESIGNAL;
    END;

    -- =====================================================
    -- 1. GENERAL VALIDATIONS
    -- =====================================================
    IF p_mode IS NULL OR UPPER(p_mode) <> 'EXECUTE' THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Invalid mode. Use EXECUTE';
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.TABLES
        WHERE TABLE_SCHEMA = 'testudines_itis'
          AND TABLE_NAME = 'wrk_keep_tsn'
    ) THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Reference table wrk_keep_tsn does not exist';
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = 'testudines_itis'
          AND TABLE_NAME = 'wrk_keep_tsn'
          AND COLUMN_NAME = 'tsn'
    ) THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'wrk_keep_tsn does not contain the tsn field';
    END IF;

    -- =====================================================
    -- 2. TEMPORARY RESULTS TABLE
    -- =====================================================
    DROP TEMPORARY TABLE IF EXISTS tmp_clean_report;
    CREATE TEMPORARY TABLE tmp_clean_report (
        TableName VARCHAR(255),
        FieldUsed VARCHAR(50),
        RecordsBefore BIGINT,
        RecordsDeleted BIGINT,
        RecordsAfter BIGINT,
        Result VARCHAR(20)
    );

    -- =====================================================
    -- 3. PRE-CHECK ALL BACKUPS
    -- =====================================================
    OPEN cur_tables;

    validation_loop: LOOP
        FETCH cur_tables INTO v_table;
        IF v_done = 1 THEN
            LEAVE validation_loop;
        END IF;

        SET v_field = IF(v_table = 'synonym_links', 'tsn_accepted', 'tsn');

        -- Check that a backup exists
        IF NOT EXISTS (
            SELECT 1
            FROM information_schema.TABLES
            WHERE TABLE_SCHEMA = 'testudines_itis'
              AND TABLE_NAME = CONCAT('wrk_', v_table)
        ) THEN
            SET v_error_msg = CONCAT('Missing backup for: ', v_table);
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = v_error_msg;
        END IF;

        -- Check for the relationship field in both tables
        IF (
            SELECT COUNT(*)
            FROM information_schema.COLUMNS
            WHERE TABLE_SCHEMA = 'testudines_itis'
              AND TABLE_NAME IN (v_table, CONCAT('wrk_', v_table))
              AND COLUMN_NAME = v_field
        ) <> 2 THEN
            SET v_error_msg = CONCAT('Missing field ', v_field, ' in ', v_table, ' or its backup');
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = v_error_msg;
        END IF;

        -- Count records in the original table
        SET @sql = CONCAT(
            'SELECT COUNT(*) INTO @cnt FROM `testudines_itis`.`', REPLACE(v_table, '`', '``'), '`'
        );
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
        SET v_count_original = @cnt;

        -- Count records in the backup
        SET @sql = CONCAT(
            'SELECT COUNT(*) INTO @cnt FROM `testudines_itis`.`wrk_', REPLACE(v_table, '`', '``'), '`'
        );
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
        SET v_count_backup = @cnt;

        -- Compare original and backup record counts
        IF v_count_original <> v_count_backup THEN
            SET v_error_msg = CONCAT('Backup record count does not match for: ', v_table);
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = v_error_msg;
        END IF;
    END LOOP;

    CLOSE cur_tables;

    -- =====================================================
    -- 4. RUN THE CLEANUP (IN A TRANSACTION)
    -- =====================================================
    START TRANSACTION;

    SET v_done = 0;
    OPEN cur_tables;

    clean_loop: LOOP
        FETCH cur_tables INTO v_table;
        IF v_done = 1 THEN
            LEAVE clean_loop;
        END IF;

        SET v_field = IF(v_table = 'synonym_links', 'tsn_accepted', 'tsn');

        -- Count initial records
        SET @sql = CONCAT(
            'SELECT COUNT(*) INTO @cnt FROM `testudines_itis`.`', REPLACE(v_table, '`', '``'), '`'
        );
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
        SET v_before = @cnt;

        -- Delete records not present in wrk_keep_tsn (preserving NULLs)
        SET @sql = CONCAT(
            'DELETE T FROM `testudines_itis`.`', REPLACE(v_table, '`', '``'), '` T ',
            'WHERE T.`', v_field, '` IS NOT NULL ',
            'AND NOT EXISTS (',
                'SELECT 1 ',
                'FROM `testudines_itis`.`wrk_keep_tsn` K ',
                'WHERE K.tsn = T.`', v_field, '`',
            ')'
        );
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        SET v_deleted = ROW_COUNT();
        DEALLOCATE PREPARE stmt;

        -- Count remaining records
        SET @sql = CONCAT(
            'SELECT COUNT(*) INTO @cnt FROM `testudines_itis`.`', REPLACE(v_table, '`', '``'), '`'
        );
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
        SET v_after = @cnt;

        -- Record the result
        INSERT INTO tmp_clean_report VALUES (
            v_table,
            v_field,
            v_before,
            v_deleted,
            v_after,
            IF(v_before = v_deleted + v_after, 'OK', 'ERROR')
        );
    END LOOP;

    CLOSE cur_tables;

    COMMIT;

    -- =====================================================
    -- 5. FINAL REPORT
    -- =====================================================
    SELECT * FROM tmp_clean_report ORDER BY TableName;

    SELECT
        COUNT(*) AS TotalTables,
        SUM(RecordsBefore) AS RecordsBefore,
        SUM(RecordsDeleted) AS RecordsDeleted,
        SUM(RecordsAfter) AS RecordsAfter,
        SUM(Result = 'ERROR') AS Errors
    FROM tmp_clean_report;

    DROP TEMPORARY TABLE IF EXISTS tmp_clean_report;
END$$

DELIMITER ;
```

------

## Step 7: Clean up tables

| No.  | Status    | Table            | Notes                                                |
| ---- | --------- | ---------------- | ---------------------------------------------------- |
| 1    | ⏳ Pending | `geographic_div` | Remove records whose `tsn` is not in `wrk_keep_tsn`. |

------

## Step 8: Delete working tables

### 8.1 Check which tables will be deleted

```
USE testudines_itis;
SELECT 
    table_name AS 'Table Name',
    table_rows AS 'Estimated Row Count',
    data_length + index_length AS 'Total Size (Bytes)',
    create_time AS 'Creation Date',
    'will be deleted in next step' AS 'warning'
FROM 
    information_schema.tables
WHERE 
    table_schema = 'testudines_itis'
    AND table_name LIKE 'wrk_%'
ORDER BY 
    table_name;
```

### 8.2 Delete the tables

```
DELIMITER //
USE testudines_itis;
CREATE PROCEDURE wrk_DropWorkTables()
BEGIN
    DECLARE done INT DEFAULT FALSE;
    DECLARE v_table VARCHAR(255);

    DECLARE cur CURSOR FOR
        SELECT table_name
        FROM information_schema.tables
        WHERE table_schema = 'testudines_itis'
          AND table_name LIKE 'wrk\_%';

    DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = TRUE;

    OPEN cur;

    read_loop: LOOP
        FETCH cur INTO v_table;

        IF done THEN
            LEAVE read_loop;
        END IF;

        SET @sql = CONCAT(
            'DROP TABLE IF EXISTS `testudines_itis`.`',
            REPLACE(v_table, '`', '``'),
            '`'
        );

        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;

    END LOOP;

    CLOSE cur;

    -- Check for remaining work tables
    SELECT 
        COUNT(*) AS Remaining_wrk_tables
    FROM information_schema.tables
    WHERE table_schema = 'testudines_itis'
      AND table_name LIKE 'wrk\_%';

END //

DELIMITER ;

CALL wrk_DropWorkTables();

DROP PROCEDURE wrk_DropWorkTables;
```
