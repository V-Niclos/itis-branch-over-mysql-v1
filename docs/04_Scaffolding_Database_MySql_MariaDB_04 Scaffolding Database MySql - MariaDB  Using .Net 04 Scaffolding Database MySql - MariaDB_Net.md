# 04 Scaffolding Database MySql - MariaDB  Using .Net and Pomelo v2

*Updated 2026-11-13*

Instructions for generating the Entity Framework Core models from the MariaDB database using Database-First (Reverse Engineering) scaffolding.



## 1. Prerequisites
* Visual studio 2026 Community

* .Net 9

* **Packages References**

  * Microsoft.EntityFrameworkCore" Version="9.0.23"

  * Microsoft.EntityFrameworkCore.Design" Version="9.0.23"  

  * Microsoft.EntityFrameworkCore.Tools" Version="9.0.23" 

  * Pomelo.EntityFrameworkCore.MySql" Version="9.0.0" 

  * MySqlConnector" Version="3.0.0"

  

  

  > [!CAUTION]
  >
  > Pomelo install old version of MySqlConnector 2.6.2,  it has reported danger advertesing, for avoid this problem install  MySqlConnector" Version="9.0.0""  for overwrite it.

Database testudines_itis or you database with 

````
````




### 1.1..NET SDK

.NET 9 SDK must be installed.

Open PowerShell console:


Check the installed SDK versions:

```bash
dotnet --list-sdks
```


### 1.2. Configure the SDK version

To ensure consistent SDK selection across development environments, be sure create a `global.json` file in the project directory:

```bash
dotnet new globaljson --sdk-version 9.0.200 --roll-forward latestFeature
```

This configuration selects the specified .NET SDK feature band and allows compatible updates within that band.

This steept add your proyect the file global.json  as below, verify it.

```
{
  "sdk": {
    "rollForward": "latestFeature",
    "version": "9.0.200"
  }
}
```



**Note:** The selected SDK version must be installed on the development machine.

### 1.3. Entity Framework Core CLI

Install the Entity Framework Core command-line tools:

```bash
dotnet tool install --global dotnet-ef --version 9.0.22
```

Verify the installation:

```bash
dotnet ef --version
```

The EF Core CLI version should match the project's EF Core version.

## 2. Build project to retrieve scaffolding models results

In this tutorial we generate:
* tetudines.data.itis.Console.prj project to generate the scaffolding models from the database.
* testudine.data.itis for library project to store the generated models.

Build console proyect for scaffolding the database.

Important: Remember need Net 9, for use Pomelo.EntityFrameworkCore.MySql 9.0.0, and Microsoft.EntityFrameworkCore 9.0.22.

Ensure that the following NuGet packages are installed in the project:

```xml

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.22" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.22" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.22" />
<PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="9.0.0" />
```

## 3. Generate Entity Framework Core Models

Open a terminal in the project directory and execute the following command.

Adjust the connection string to match your database server, database name, username and password.

```bash
dotnet ef dbcontext scaffold "Server=127.0.0.1;Port=3306;Database=testudines_itis;User Id=your_user_id;Password=xxxx;" Pomelo.EntityFrameworkCore.MySql -o Entities -c ItisDbContext --context-dir Context --no-onconfiguring --no-pluralize --use-database-names --data-annotations
```

### 3.1. Command parameters

| Parameter             | Description                                                       |
| --------------------- | ----------------------------------------------------------------- |
| `-o Entities`         | Output directory for generated entity classes.                    |
| `-c ItisDbConte xt`   | Name of the generated DbContext class.                            |
| `--context-dir Contex | Directory for the DbContext class.                                |
| t`                    |                                                                   |
| ` --no-onconfigu      | Prevents the connection string from being written into the        |
|                       | generated                                                         |
| ring`                 | DbContext.                                                        |
| `--no-pluralize `     | Prevents automatic pluralization of entity names.                 |
| `--use-database -name | Preserves original database table and column names.               |
| s`                    |                                                                   |
| ` --data-annotat      | Uses data annotations where supported instead of Fluent API       |
| ions`                 | configuration.                                                    |

### 3.2. Generated structure

The scaffolding process generates the following project structure:

Look at new folder Context and Entities, this has one model foreach table in database. y you need add your model y better add  **Models**  folder for your custom class models

```text
Testudines.Data.ITIS
│
├───Context
│       ItisDbContext.cs
│
├───Entities
│       change_comments.cs
│       change_operations.cs
│       change_tracks.cs
│       chg_operation_lkp.cs
│       comments.cs
│       experts.cs
│       geographic_div.cs
│       hierarchy.cs
│       HierarchyToRank.cs
│       jurisdiction.cs
│       kingdoms.cs
│       longnames.cs
│       nodc_ids.cs
│       other_sources.cs
│       publications.cs
│       reference_links.cs
│       reviews.cs
│       strippedauthor.cs
│       synonym_links.cs
│       taxonomic_units.cs
│       taxon_authors_lkp.cs
│       taxon_unit_types.cs
│       tu_comments_links.cs
│       vernaculars.cs
│       vern_ref_links.cs
│
└── Testudines.Data.ITIS.csproj
```

All tables in the selected database are included by default.

Recomeded  /Models folder for keep you customs models, and /Services ...

like

```
│
├───Context
│       ItisDbContext.cs
│
├───Entities
│
├───Models
│
├───Services

```



## 4. Regenerating the models

To regenerate the models after changes to the database structure, execute the scaffolding command again, adding:

```bash
--force
```

**Warning:** The `--force` option overwrites existing generated files. Any manual modifications to generated classes or the DbContext may be lost.

Keep custom code separate from generated files whenever possible.



##  5. Build App test console 



In visual studio create Testudines.Data.Itis.Test_Console . Selecting Net. 10, 

Add references to

* Testudines.Data.Itis library
* MySqlConnector" Version="3.0.0"



````c#
using Microsoft.EntityFrameworkCore;
using Testudines.Data.Itis.Context;

string connectionString =
    "Server=127.0.0.1;Port=3306;Database=testudines_itis;User Id=your_user_id;Password=yourpassword;";

var options = new DbContextOptionsBuilder<ItisDbContext>()
    .UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString))
    .Options;

using var db = new ItisDbContext(options);

Console.WriteLine("ITIS database test");

Console.WriteLine(
    $"Database connection: {await db.Database.CanConnectAsync()}");

Console.WriteLine(
    $"Generated entity types: {db.Model.GetEntityTypes().Count()}");
````





