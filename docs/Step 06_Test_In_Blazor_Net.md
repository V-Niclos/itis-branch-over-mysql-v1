## Step 06: Test the Blazor integration using Net 10

### Objective

Create a simple Blazor test application that uses `Testudines.Data.Itis` with .NET 10.

The app will have one page that displays a list of taxonomic units from the database.

### Environment

- Visual Studio 2026
- Blazor app targeting .NET 10
- Database: `testudines_itis` (change this if your database has a different name)

### Add project references

Add a reference to:

```
Testudines.Data.Itis\Testudines.Data.Itis.csproj
```

Add the following NuGet package:

```
<PackageReference Include="Microsoft.EntityFrameworkCore.Relational" Version="9.0.22" />
```

Also make sure the project includes the Pomelo MySQL provider, which provides `UseMySql`:

```
<PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="9.0.0" />
```

Use the Pomelo version that matches your project’s Entity Framework Core version.

### Configure the connection string

In `appsettings.json`, add a connection string and replace `user_name` and `your_password` with your MariaDB credentials:

```
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=127.0.0.1;Port=3306;Database=testudines_itis;Uid=user_name;Pwd=your_password"
  }
}
```

### Register the database context in `Program.cs`

Add this before `var app = builder.Build();`:

```
// Register the database context factory for use in Blazor components.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContextFactory<ItisDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)));
```

Make sure these namespaces are available at the top of `Program.cs`:

```
using Microsoft.EntityFrameworkCore;
using Testudines.Data.Itis.Context;
```

### Replace the home page

Update `Components/Pages/Home.razor` (or the equivalent home page in your project) with:

```
@page "/"
@rendermode InteractiveServer

@using Microsoft.EntityFrameworkCore
@using Testudines.Data.Itis.Context
@using Testudines.Data.Itis.Entities

@inject IDbContextFactory<ItisDbContext> DbFactory

<PageTitle>Home</PageTitle>

<h3>ITIS Taxonomic Units</h3>

@if (rows is null)
{
    <p>Loading...</p>
}
else
{
    <ul>
        @foreach (var item in rows)
        {
            <li>@item.tsn: @item.complete_name</li>
        }
    </ul>
}

@code {
    private List<taxonomic_units>? rows;

    protected override async Task OnInitializedAsync()
    {
        await using var context = await DbFactory.CreateDbContextAsync();

        rows = await context.taxonomic_units
            .AsNoTracking()
            .OrderBy(t => t.complete_name)
            .ToListAsync();
    }
}
```

### Result

The home page displays the taxonomic units from the `testudines_itis` database, ordered by name.

*Add the screenshot `image-20261114143740519` here.*

You can find the code in `artifact-template-design-report`.

**Note:** Keep database credentials out of source control. Use user secrets or another configuration method for local development.
