## ITIS Database Installation in MySQL – MariaDB

*Updated 2016-11-12*

URL:

------

## General objective

Use a local clone of the ITIS (Integrated Taxonomic Information System) database, resolve encoding issues by converting it to `utf8mb4`, and optionally reduce the database to a taxonomic branch.

## Procedures

### Working environment

- MariaDB: `11.8.6-MariaDB-5ubuntu0.1`
- System: Ubuntu on WSL (Linux Ubuntu)
- Machine: Windows 11
- PowerShell
- MariaDB console
- .NET 9 and .NET 10

### How to follow this guide

The guide is divided into separate steps, each in its own file. Some steps require you to have completed a previous step.

The code uses the *Testudines* branch with TSN `948936` as an example.

To use the guide with any other taxonomic branch, simply change the TSN.

The environment for following the guide is Linux and MySQL/MariaDB. Some optional steps use other tools.

#### Step 1. Download and installation

Install a local clone of the ITIS (*Integrated Taxonomic Information System*) database in MariaDB or MySQL on Linux. This clone is immutable and encoded in Latin-1.

The resulting clone will be named *ITIS*.

Continue on to [Step 01](01_Instalation_ITIS.md).



#### Step 2. Create a UTF-8 clone of ITIS (Optional)

Create a complete clone of ITIS encoded in `utf8mb4` to make it easier to use internationally, with particular emphasis on resolving encoding issues.

The resulting clone will be named *ITIS_utf8mb4*.
Continue on to [Step 02](02_Create_Clon_ITIS_with_utf8mb4_Encoding.md).

#### Step 03: Reduce the ITIS Database to a Single Taxonomic Branch.

This guide uses turtles (*Testudines*, TSN `948936`) as an example. You can use any other branch, such as reptiles (*Reptilia*, TSN `173747`), mammals (*Mammalia*, TSN `179913`), or birds (*Aves*, TSN `174371`). Look up other TSNs at [itis.gov](https://itis.gov/).

The resulting clone will be named *testudines_itis*.

Continue on to [Step 03](03_Reduce_ITIS_Database_to_Single_Taxonomic_Branch.md).

### Step 4. Data model library scaffolding (Optional)

Create a library with a data model in a .NET 9 environment using scaffolding, Pomelo, and MySqlConnector.

Go to Step 4.

### Step 5. Test Blazor web application (Optional)

Create a test application using the database obtained in Step 4, with .NET 10, Pomelo, and MySqlConnector.

Go to Step 5.
