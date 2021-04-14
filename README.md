# SqlHydra
SqlHydra is a [Myriad](https://github.com/MoiraeSoftware/myriad) plugin that generates F# records from a SQL Server SSDT .dacpac file.

[![NuGet version (SqlHydra)](https://img.shields.io/nuget/v/SqlHydra.svg?style=flat-square)](https://www.nuget.org/packages/SqlHydra/)

## Setup

1) Install `SqlHydra` and `Myriad.Sdk` from NuGet.

2) Add a `myriad.toml` configuration file to your project with the following:

```toml
[ssdt]
namespace = "AdventureWorks"
```

(The generated F# records will be added to the namespace you configure.)

3) Add an `ItemGroup` to your .fsproj file to configure a .dacpac input file and an .fs output file:

```xml
    <ItemGroup>
         <!-- Specify the .fs output file (to be generated by Myriad) -->
        <Compile Include="AdventureWorks.fs">
            <!-- Specify the .dacpac input  file -->
            <MyriadFile>../AdventureWorks/bin/Debug/AdventureWorks.dacpac</MyriadFile>
        </Compile>
    </ItemGroup>

```

4) Build your project to generate the .fs file.

5) Use your generated types with a micro ORM like FSharp.Dapper or RepoDb.

## Benefits of Myriad
* Myriad + SSDT is fast - very low impact on your build
* Myriad detects changes to SSDT .dacpac file and regenerates on next build
* Generated types are records, not classes (algebraic type safety for your data layer)
* Generated types can be used outside of project
* Generated types can be checked into source control (build server friendly)


## Roadmap
* Add a configuration option to add `[<CLIMutable>]` attribute to generated records (required by some ORMs like vanilla Dapper and EF).
* Possibly adding some generated helpers for db access
* <cool new thing here?>

