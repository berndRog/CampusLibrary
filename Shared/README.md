# CampusLibrary Shared Logging update

This ZIP contains the shared logging files for CampusLibrary.

It is not a new project. The files are intended to live at solution level:

```text
CampusLibrary/
├─ Shared/
│  └─ Logging/
├─ CampusLibraryClient/
├─ CampusLibraryApi/
└─ IdentityAccessServer/
```

Use the snippets in `_snippets/` to link the files into each `.csproj` with `Compile Include` / `Link`.

After applying the shared namespace, update host code from:

```csharp
using CampusLibraryClient.Shared.Logging;
using Banking26Auth.Shared.Logging;
```

to:

```csharp
using CampusLibrary.Shared.Logging;
```
