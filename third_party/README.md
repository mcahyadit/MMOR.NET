## Bcl.CollectionsMarshal

Upstream: https://github.com/DaZombieKiller/Bcl.CollectionsMarshal
License: MIT

### Patches:

LangVersion switched to `9.0` from `13.0` for easy port to Unity.

Changed MsBuild tag `Version` to `AssemblyVersion`:

this is to fix build issues in VisualStudio which logs:

```
"code": "NU1105",
"level": "Error",
"message": "Unable to read project information for 'netstandard': The property PackageVersion was expected to have a single value across all target frameworks, but instead had the following values: 2.0.0.0, 2.1.0.0",
```
