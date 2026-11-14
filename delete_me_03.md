# delete me 01
**ITIS: Integrated Taxonomic Information System** 
 
** Objetivo del filtrado.**

1) Obtener un clon correspondiente a una rama de la taxonomia, a partir de un clon completo de la base de datos de ITIS, el clon resultante contenga únicamente una rama de la taxonomía seleccionada, incluyendo ascendientes, descendientes y sinonimos. 

2) Codificar la base de datos en utf8mb4, con el fin de facilitar el uso con carateres multiidioma, y applicaciones modernas.

3) En esta guia elegimos la rama correspondiente al taxon testudines que se corresponde con el tsn=948936.

---

## Índice

*   [Paso 1:Consideraciones generales](#paso-1-consideraciones-generales)
*   [Paso 2: Clonar base de datos ITIS_utf8mb4 a testudines_itis](#paso-2-clonar-base-de-datos-itis-_utf8mb4-a-testudines_itis)
*   [Paso 3: Consideraciones de la importación](#paso-3-consideraciones-de-la-importación)
*   [Paso 4: Crear la tabla de trabajo con todos los tsn relacionados con testudines](#paso-6-crear-la-tabla-de-trabajo-con-todos-los-tsn-relacionados-con-testudines)
*   [Paso 5: Limpiar tablas](#paso-7-limpiar-tablas)
*   [Paso 6: Borrar tablas de trabajo](#paso-8-borrar-tablas-de-trabajo)
*   [Paso 9: Scaffolding .Net](#paso-9-Scaffolding-Net-9])
---

##  Paso 1:Consideraciones generales
En este tutorial se utilizará el orden **Testudines cuyo tsn es 948936** este numero se debera ajustar al tsn de la rama que se desee.

La base de datos origen del clon que utilizaremos sera **ITIS_utf8mb4**.  que se puede obtener siguendo esta guia **(⚠️TODO FALTA  LINK)**
 
La justificacion es poder usar applicaciones modernas multiidioma sin usando utf8mb4, y  prevenir errores de codificacion, que producen representaciones en pantalla estraños

La base de datos resultante se denominará **`testudines_itis`.*


En entorno es
- MariaDB: `11.8.6-MariaDB-5ubuntu0.1`
- Sistema: Ubuntu sobre WSL.
- Equipo: Windows 11.


---

## Paso 2: Clonar base de datos ITIS_utf8mb4 a testudines_itis

En consola Powershell conectarse a MariaDB
````bash
wsl
mariadb -u your_user_id -p
```` 

Crear base de datos destino testudines_itis, no queremos que distinga mayusculas ni tildes
```sql
DROP DATABASE IF EXISTS testudines_itis;
CREATE DATABASE testudines_itis
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;
SHOW DATABASES;
````
Importar datos as testudines_itis
-------------
````
mariadb -u your_user_id -p --default-character-set=utf8mb4 testudines_itis < ITIS_UTF.sql
````



## Paso 4: Comprobar que los pasos 2 y 3 son correctos


4.1 Ejecuta el siguiente script de verificación para garantizar la coherencia de datos entre ambas bases de datos antes de aplicar filtros:

```sql
/* Comprobación de coherencia entre BBDD itis y testudines_itis */
SELECT 
    (SELECT COUNT(*) FROM itis.hierarchy) AS itis_hierarchy,
    (SELECT COUNT(*) FROM testudines_itis.hierarchy) AS testudines_itis_hierarchy,
    
    (SELECT COUNT(*) FROM itis.synonym_links) AS itis_synonym_links,
    (SELECT COUNT(*) FROM testudines_itis.synonym_links) AS testudines_itis_synonym_links,
       
    (SELECT COUNT(*) FROM itis.taxonomic_units) AS itis_taxonomic_units,
    (SELECT COUNT(*) FROM testudines_itis.taxonomic_units) AS testudines_taxonomic_units;
```

> 📊 **Ejemplo de validación:** Ambas bases de datos deben reportar exactamente la misma cantidad de registros iniciales:

| itis_hierarchy | testudines_itis_hierarchy | itis_synonym_links | testudines_itis_synonym_links | itis_taxonomic_units | testudines_taxonomic_units |
| -------------: | ------------------------: | -----------------: | ----------------------------: | -------------------: | -------------------------: |
|              6 |                         6 |                  3 |                             3 |                    1 |                          1 |
|              8 |                         8 |                  1 |                             1 |                    0 |                          0 |
|              3 |                         3 |                  9 |                             9 |                    0 |                          0 |
|              9 |                         9 |                  8 |                             8 |                    3 |                          3 |
|              5 |                         5 |                  1 |                             1 |                    4 |                          4 |
|              0 |                         0 |                  3 |                             3 |                    9 |                          9 |
|                |                           |                    |                               |                    2 |                          2 |

---



**4.2** *Opcional:* buscar visualmente caracteres sospechosos de estar mal codificados, este es largo.

Busca detro de todas las tablas campos VARCHAR , y en ellos caracteres sospechosos de esta mal codificados. Si son pocos seguramente es por 
```sql
USE testudines_itis;

DELIMITER //

DROP PROCEDURE IF EXISTS buscar_errores_codificacion//

CREATE PROCEDURE buscar_errores_codificacion(IN db_nombre VARCHAR(64))
BEGIN
    DECLARE fin INT DEFAULT FALSE;
    DECLARE v_tabla VARCHAR(64);
    DECLARE v_columna VARCHAR(64);
    
    -- Cursor para obtener todas las tablas y columnas de texto de la base de datos
    DECLARE cur_columnas CURSOR FOR 
        SELECT TABLE_NAME, COLUMN_NAME
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = db_nombre
          AND DATA_TYPE IN ('char', 'varchar', 'tinytext', 'text', 'mediumtext', 'longtext');
          
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET fin = TRUE;

    -- Tabla temporal para almacenar los hallazgos sin inundar la consola de resultados vacíos
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

        -- Construimos la consulta dinámica buscando patrones típicos de doble codificación o pérdida de datos
        SET @sql_query = CONCAT(
            'INSERT INTO tmp_errores_encoding (tabla, columna, valor_corrupto, valor_hex) ',
            'SELECT ''', v_tabla, ''', ''', v_columna, ''', `', v_columna, '`, HEX(`', v_columna, '`) ',
            'FROM `', db_nombre, '`.`', v_tabla, '` ',
            'WHERE `', v_columna, '` LIKE ''%Ã%'' ',       -- Doble codificación UTF-8 sobre Latin1 (ej. Ã±, Ã©, Ã¼)
            '   OR `', v_columna, '` LIKE ''%Â%'' ',       -- Caracteres extraños antes de símbolos (ej. Â°, Â©)
            '   OR `', v_columna, '` LIKE ''%â€%'' ',      -- Comillas o guiones mal codificados (ej. â€œ, â€”)
/*            '   OR `', v_columna, '` LIKE ''%?%'' ',       -- Posible pérdida de caracteres convertidos a interrogante*/
            '   OR HEX(`', v_columna, '`) LIKE ''%EFBFBD%'' ', -- Carácter de reemplazo Unicode ()
            '   OR HEX(`', v_columna, '`) LIKE ''%C383%'' ',   -- Doble codificación profunda (ej. ÃƒÂ±)
            'LIMIT 50;' -- Límite por columna para evitar bloqueos en tablas gigantes
        );

        PREPARE stmt FROM @sql_query;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;

    END LOOP;

    CLOSE cur_columnas;

    -- Mostrar todos los resultados recopilados
    SELECT * FROM tmp_errores_encoding;
    
    -- Limpieza
    DROP TEMPORARY TABLE IF EXISTS tmp_errores_encoding;
END//

DELIMITER ;

-- Ejecutar el procedimiento pasando el nombre de tu base de datos
CALL buscar_errores_codificacion('testudines_itis');

DROP PROCEDURE IF EXISTS buscar_errores_codificacion//
````


* //++++++++++++++++++++++++++++++++++++++++++++++++++//
* //++++++++++++++++++++++++++++++++++++++++++++++++++//




## Paso 3: Consideraciones de la importación

 ℹ️ **Nota 1 :**  En este tutoria se ha elegido testudines como rama a consevar, para adaptarlo a otra rama cambie el tsn por el correspondiente a su rama, Ejemplo si elige reptilia sustituya el de testudines=948936  por el correspodinte a reptilia= 173747
 
 ℹ️ **Nota 1 :** Las tablas de trabajo y las copias de seguridad temporales comenzarán siempre con el prefijo `wrk_` (Work). De esta forma, se podrán identificar y eliminar fácilmente al finalizar todo el proceso.
 
 ℹ️ **Nota: 2**   La tabla crítica del proceso será `wrk_keep_tsn`. Es la primera tabla que hay que obtener.
 Esta albergará de forma temporal y segura todos los códigos TSN válidos (ascendientes y descendientes) junto con sus respectivos sinónimos asociados a la rama taxonómica *Testudines*.
 
Indica qué registros se deben conservar en las  tablas a limpiar (Aquellas que contennienen campo `tsn`I, eliminando todos aquellos que no estén incluidos en ella.
 
 ⚠ **Warning: 3** Ponga especial cuidado en la obtencion de de la tabla(**wrk_keep_tsn**).  

La obtencion de la tabla consta de dos pasos, 
Primero: Agregar los tsn validos de los ascendientes y descendientes
Segundo. Agregar a la tabla los sinonimos de los tsn obtenidos en el paso anterior

Atencion: 

>

---

## Paso 4: Crear la tabla de trabajo con todos los tsn relacionados con testudines

### 4.1 Identificar el TSN correspondiente a Testudines
A través de una consulta previa en la tabla `taxonomic_units` o `hierarchy`, localizamos el identificador base. Para *Testudines*, el TSN de origen es `948936`.

### 4.2 Identificar los TSN ascendientes de la rama Testudines

Para obtenter ascendiente y descendientes de una rama es necesario obtener primero el campo hierarchy_string corresponiente a la rama de la base de datos que queremos conservar
Ejecuta la consulta para extraer la cadena jerárquica de ancestros:

```sql
-- Obtener la cadena de jerarquía de los ascendientes
USE testudines_itis;
SELECT hierarchy_string 
FROM testudines_itis.hierarchy
WHERE tsn = 948936;
```

*Resultado de ejemplo obtenido:* `202423-914154-914156-158852-331030-914179-914181-173747-948936`


Los códigos numéricos devueltos en la cadena se usarán de forma estática en el paso de inserción 4.4.

### 4.3 Crear la tabla de trabajo `wrk_keep_tsn`
Esta tabla actuará como el índice de persistencia. Cualquier registro fuera de esta lista será eliminado posteriormente.

```sql
USE testudines_itis;

DROP TABLE IF EXISTS wrk_keep_tsn;

CREATE TABLE wrk_keep_tsn (
    tsn INT NOT NULL PRIMARY KEY
);
```

### 6.4 Insertar los datos base y comprobar
En esta primera fase se guardan las unidades taxonómicas directas e indirectas. Los sinónimos se procesarán inmediatamente después.

```sql
USE testudines_itis;

-- 1. Insertar descendientes y ascendientes de Testudines (incluido el propio nodo de Testudines)

INSERT IGNORE INTO wrk_keep_tsn (tsn)
SELECT h.tsn
FROM hierarchy AS h
WHERE h.hierarchy_string LIKE '%948936%';

-- 2. Antepasados directos de Testudines obtenidos en el paso jerárquico anterior
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

Comprobación del volumen de carga inicial:
```sql
USE testudines_itis;
SELECT COUNT(*) AS total_tsn_to_keep FROM wrk_keep_tsn;
```

### 4.5 Insertar los sinónimos taxonómicos

#### 4.5.1 Comprobar los sinónimos válidos a insertar (Opcional)
Consulta para revisar las equivalencias de nombres científicos antes de consolidarlas:

```sql
USE testudines_itis;

SELECT 
    th.tsn AS tsn_aceptado,
    tu_aceptado.complete_name AS nombre_aceptado,
    ts.tsn AS tsn_sinonimo,
    tu_sinonimo.complete_name AS nombre_sinonimo,
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
Resultado:
#### 4.5.2 Agregar los sinónimos a la tabla de trabajo
```sql
USE testudines_itis;

INSERT IGNORE INTO wrk_keep_tsn (tsn)
SELECT DISTINCT sl.tsn
FROM synonym_links AS sl
INNER JOIN wrk_keep_tsn AS k
    ON sl.tsn_accepted = k.tsn;
```

#### 4.5.3 Comprobación de seguridad final
Verificamos el conteo total consolidado y nos aseguramos de que no queden enlaces huérfanos de sinónimos fuera de nuestro universo controlado:

```sql
USE testudines_itis;

-- Recuento total en la tabla de exclusión
SELECT COUNT(*) AS total_consolidado FROM wrk_keep_tsn;

-- Comprobar remanentes: El resultado de esta consulta DEBE SER CERO
SELECT COUNT(*) AS sinonimos_huerfanos
FROM synonym_links AS sl
INNER JOIN wrk_keep_tsn AS k
    ON sl.tsn = k.tsn
WHERE sl.tsn_accepted NOT IN (SELECT tsn FROM wrk_keep_tsn);
```

> 🗄️ **Nota de seguridad:** 

> Llegados a este punto, realiza un backup o crea un punto de restauración completo de la base de datos `testudines_itis` antes de proceder con el borrado masivo del 
 
 En powershell
 ```bash

mariadb-dump -u your_user_id -p \
--default-character-set=utf8mb4 \
--databases testudines_itis > testudines_itis_ful.sql
```

---

## Paso 5 Eliminar registros innecesarios.

Los registros innecesarios son todos los que su tsn no este incluido en wrk_keep_tsn


5.1 Backup tablas a limpiar

Se crearan tablas con prefijo 'wrk_*', donde * es el nombre de tabla original
 
````sql
USE testudines_itis;

DELIMITER $$

-- Eliminamos el procedimiento si ya existía previamente
DROP PROCEDURE IF EXISTS sp_backup_tablas_tsn$$

CREATE PROCEDURE sp_backup_tablas_tsn()
BEGIN
    DECLARE v_done INT DEFAULT FALSE;
    DECLARE v_tabla VARCHAR(255);
    
    -- Cursor para buscar tablas base que tengan las columnas requeridas
    -- En MariaDB los nombres de columna no distinguen mayúsculas/minúsculas por defecto,
    -- pero usamos LOWER() para garantizar la compatibilidad en cualquier servidor.
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

        -- 1. Eliminar la copia de seguridad anterior si ya existe
        SET @sql_drop = CONCAT('DROP TABLE IF EXISTS `testudines_itis`.`wrk_', v_tabla, '`');
        PREPARE stmt_drop FROM @sql_drop;
        EXECUTE stmt_drop;
        DEALLOCATE PREPARE stmt_drop;

        -- 2. Crear la tabla nueva copiando exactamente la estructura (PK, índices, tipos)
        SET @sql_create = CONCAT('CREATE TABLE `testudines_itis`.`wrk_', v_tabla, '` LIKE `testudines_itis`.`', v_tabla, '`');
        PREPARE stmt_create FROM @sql_create;
        EXECUTE stmt_create;
        DEALLOCATE PREPARE stmt_create;

        -- 3. Volcar todos los datos de la tabla original a la copia
        SET @sql_insert = CONCAT('INSERT INTO `testudines_itis`.`wrk_', v_tabla, '` SELECT * FROM `testudines_itis`.`', v_tabla, '`');
        PREPARE stmt_insert FROM @sql_insert;
        EXECUTE stmt_insert;
        DEALLOCATE PREPARE stmt_insert;

    END LOOP;

    CLOSE cur_tablas;
END$$

DELIMITER ;

-- Ejecutar el procedimiento para crear las copias
CALL sp_backup_tablas_tsn();

-- Eliminar el procedimiento tras su uso (ya que lo solicitaste temporal)
DROP PROCEDURE IF EXISTS sp_backup_tablas_tsn;
```

Comprobacion 
```sql
SELECT 
    TABLE_NAME AS tableName,
    ENGINE AS engine,
    TABLE_ROWS AS rows
    
    
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'testudines_itis'
  AND TABLE_NAME LIKE 'wrk\_%'
ORDER BY TABLE_NAME;

`````

---


Comprobacion previa a eliminar registros, 

```sql

SELECT 
    COALESCE(db1.TABLE_NAME, db2.TABLE_NAME) AS Tabla,

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
    END AS Comparation

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
    db2.TABLE_NAME AS Tabla,

    0 AS DB_ITIS,

    COALESCE(db2.TABLE_ROWS, 0) AS DB_testudines_itis,

    -COALESCE(db2.TABLE_ROWS, 0) AS Difference,

    'Only in testudines_itis' AS Comparation

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

ORDER BY Tabla;

```


////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////

Paso 6 limpiar tablas 


Ejectcuta esta consulta para crear procedimiento almacenado en bbdd

```sql 
/*
----------------------------------------------------------
 How  run script
 CALL `testudines_itis`.`sp_clean_itis_tables`('EXECUTE');
 At the end commit to eliminate rows
 or rollback for not eliminte rows
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

    -- Cursor: tablas con tsn (o tsn_accepted en synonym_links)
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

    -- Manejador de fin de cursor
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET v_done = 1;

    -- Manejador de errores SQL para revertir transacción si algo falla
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        DROP TEMPORARY TABLE IF EXISTS tmp_clean_report;
        RESIGNAL;
    END;

    -- =====================================================
    -- 1. VALIDACIONES GENERALES
    -- =====================================================
    IF p_mode IS NULL OR UPPER(p_mode) <> 'EXECUTE' THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Modo no permitido. Utilizar EXECUTE';
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.TABLES
        WHERE TABLE_SCHEMA = 'testudines_itis'
          AND TABLE_NAME = 'wrk_keep_tsn'
    ) THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'No existe la tabla de referencia wrk_keep_tsn';
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = 'testudines_itis'
          AND TABLE_NAME = 'wrk_keep_tsn'
          AND COLUMN_NAME = 'tsn'
    ) THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'wrk_keep_tsn no contiene el campo tsn';
    END IF;

    -- =====================================================
    -- 2. TABLA TEMPORAL DE RESULTADOS
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
    -- 3. COMPROBACIÓN PREVIA DE TODAS LAS COPIAS
    -- =====================================================
    OPEN cur_tables;

    validation_loop: LOOP
        FETCH cur_tables INTO v_table;
        IF v_done = 1 THEN
            LEAVE validation_loop;
        END IF;

        SET v_field = IF(v_table = 'synonym_links', 'tsn_accepted', 'tsn');

        -- Comprobar existencia de copia
        IF NOT EXISTS (
            SELECT 1
            FROM information_schema.TABLES
            WHERE TABLE_SCHEMA = 'testudines_itis'
              AND TABLE_NAME = CONCAT('wrk_', v_table)
        ) THEN
            SET v_error_msg = CONCAT('Falta copia de seguridad para: ', v_table);
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = v_error_msg;
        END IF;

        -- Comprobar campo de relación en ambas tablas
        IF (
            SELECT COUNT(*)
            FROM information_schema.COLUMNS
            WHERE TABLE_SCHEMA = 'testudines_itis'
              AND TABLE_NAME IN (v_table, CONCAT('wrk_', v_table))
              AND COLUMN_NAME = v_field
        ) <> 2 THEN
            SET v_error_msg = CONCAT('Falta el campo ', v_field, ' en ', v_table, ' o su copia');
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = v_error_msg;
        END IF;

        -- Contar registros originales
        SET @sql = CONCAT(
            'SELECT COUNT(*) INTO @cnt FROM `testudines_itis`.`', REPLACE(v_table, '`', '``'), '`'
        );
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
        SET v_count_original = @cnt;

        -- Contar registros de la copia
        SET @sql = CONCAT(
            'SELECT COUNT(*) INTO @cnt FROM `testudines_itis`.`wrk_', REPLACE(v_table, '`', '``'), '`'
        );
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
        SET v_count_backup = @cnt;

        -- Comparar originales y copias
        IF v_count_original <> v_count_backup THEN
            SET v_error_msg = CONCAT('La copia de seguridad no coincide en registros con: ', v_table);
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = v_error_msg;
        END IF;
    END LOOP;

    CLOSE cur_tables;

    -- =====================================================
    -- 4. EJECUCIÓN DE LA LIMPIEZA (CON TRANSACCIÓN)
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

        -- Registros iniciales
        SET @sql = CONCAT(
            'SELECT COUNT(*) INTO @cnt FROM `testudines_itis`.`', REPLACE(v_table, '`', '``'), '`'
        );
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
        SET v_before = @cnt;

        -- Eliminar registros que no están en wrk_keep_tsn (respetando NULLs)
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

        -- Registros restantes
        SET @sql = CONCAT(
            'SELECT COUNT(*) INTO @cnt FROM `testudines_itis`.`', REPLACE(v_table, '`', '``'), '`'
        );
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
        SET v_after = @cnt;

        -- Registrar resultado
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
    -- 5. INFORME FINAL
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


````




////////////////////////////////////////////////////////////////////



## Paso 7: verificaciones

comparar las tres bbdd
````sql
SELECT 
    TABLE_NAME AS `Tabla`,
    SUM(CASE WHEN TABLE_SCHEMA = 'itis' THEN TABLE_ROWS ELSE 0 END) AS `itis`,
    SUM(CASE WHEN TABLE_SCHEMA = 'ITIS_utf8mb4' THEN TABLE_ROWS ELSE 0 END) AS `ITIS_utf8mb4`,
    SUM(CASE WHEN TABLE_SCHEMA = 'testudines_itis' THEN TABLE_ROWS ELSE 0 END) AS `testudines_itis`,
    (
        SUM(CASE WHEN TABLE_SCHEMA = 'itis' THEN TABLE_ROWS ELSE 0 END) - 
        SUM(CASE WHEN TABLE_SCHEMA = 'testudines_itis' THEN TABLE_ROWS ELSE 0 END)
    ) AS `Diferencia`
FROM information_schema.TABLES
WHERE TABLE_SCHEMA IN ('itis', 'ITIS_utf8mb4', 'testudines_itis')
  AND TABLE_TYPE = 'BASE TABLE'
GROUP BY TABLE_NAME
ORDER BY TABLE_NAME;

````


## Paso 6: Borrar tablas de trabajo


8.1 Comprobar tablas a borrar

```sql
USE testudines_itis;
SELECT 
    table_name AS 'Nombre de la Tabla',
    table_rows AS 'Número Estimado de Filas',
    data_length + index_length AS 'Tamaño Total (Bytes)',
    create_time AS 'Fecha de Creación',
    'will be deleted in next steep' AS 'warning'
FROM 
    information_schema.tables
WHERE 
    table_schema = 'testudines_itis'
    AND table_name LIKE 'wrk_%'
ORDER BY 
    table_name;
``````

8.2 Borrar tablas
```sql
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

    -- Comprobación: tablas de trabajo que quedan
    SELECT 
        COUNT(*) AS Tablas_wrk_pendientes
    FROM information_schema.tables
    WHERE table_schema = 'testudines_itis'
      AND table_name LIKE 'wrk\_%';

END //


DELIMITER ;

CALL wrk_DropWorkTables();

DROP PROCEDURE wrk_DropWorkTables;
```
