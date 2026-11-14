## ITIS database instalación en Mysql - MariaDB
*Updated 2016-11-12*

---



## Objetivo general

Usar base de datos ITIS localmente, resolver problematica de codificacion, usando utf8mb4,  opcionalmente reducir bbdd a una rama de la taxonomia,

## Procedimientos

Se utiliza la rama *Testudines tsn:948936* compo ejemloen el codigo, 

Solo es necesario ajustar el tsn para que la guia funcione en para otra rama la taxonomia

El entorno para seguir la guia: Linux, MySql / MariaDB, para alguna pasos opcionales .Net 9 y .Net 10
 
La guia esta divida en pasos separados, cada uno en un achivo serparado, cada paso requiere el anterior

## Entorno de trabajo

- MariaDB: `11.8.6-MariaDB-5ubuntu0.1`
- Sistema: Ubuntu sobre WSL.
- Equipo: Windows 11.
- PowerShell
- Consola MariaDB.
 
#### Paso 1. Descarga e instalación

Instalar localmente la base de datos ITIS (*Integrated Taxonomic Information System*) en MariaDB, MySql, sobre Linux. Este clon sera inmutable, esta codificado en Latin-1

[Ir a Paso 1](01_Instalacion_ITIS.md)

#### Paso 2 Crear clon de ITIS a utf8mb4 (Opcional)

Obtener un clon completo de itis codificado en utf8mb4, para facilitar el uso internacional, se hace especial enfasis en resolver problema de codificacion.

[Ir a paso 2](02_Clonar _ITIS_Utf8mb4.md)

### Paso 3. Crear clon limitado a una rama taxonómica (Opcional)

En esta la guia se usa tortugas (*Testudines tsn:948936*), se puede obtener cualquiero otra como reptiles *Reptilia tsn:173747*), mamiferos (*Mammalia tsn:179913*), tortugas (*Testudines tsn:948936*), Aves (*Aves tsn: 174371*).. Cosunltar otros tsn en [itis.gov](https://itis.gov/)

[Ir a paso 3](03_Filtrado_Rama_ITIS.md)

### Paso 4. Libreria Modelo de datos  scaffolding (Opcional)
Creacion de una libreria con modelo de datos en entorno Net. 9, Pomelo, MySqlConnector usando scaffolding

[Ir a paso 4](04_Scaffolding_itis_database.md)

### Paso 5. Aplicacion web blazor de prueba (Opcional)

Creacion de applicacion de pruebas usando la base de datos obtenida en el paso 4, usando, Net 10, Pomelo, MySqlConnector

[Ir Paso5 ](05_itis_blazor.md)
