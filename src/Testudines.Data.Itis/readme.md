# Scaffolding Database MySql - MariaDB  Using .Net and Pomelo v2

*Updated 2026-11-13*

Instructions for generating the Entity Framework Core models from the MariaDB database using Database-First (Reverse Engineering) scaffolding.




## 1. Prerequisites

* Visual studio 2026 Community

* .Net 9

* **Packages References**

  * Microsoft.EntityFrameworkCore" Version="9.0.22"

  * Microsoft.EntityFrameworkCore.Design" Version="9.0.22"  

  * Microsoft.EntityFrameworkCore.Tools" Version="9.0.22" 

  * Pomelo.EntityFrameworkCore.MySql" Version="9.0.0" 

  * MySqlConnector" Version="9.0.0""



  

  > [!CAUTION]
  >
  > Pomelo install old version of MySqlConnector 2.6.2,  it has reported danger advertesing, for avoid this problem install  MySqlConnector" Version="9.0.0""  for overwrite it.

Observations proyect

* Builded with visual studio 2026
* Proyect type Class library
* Spacename Testudines.Data.Itis
* Folder prroyect:  D:\__Testudines\Git\src\Libs\Testudines.Data.Itis\Testudines.Itis.Console_2
* Database testudines_itis or you database with 

