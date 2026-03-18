# Publish To NuGet

## Build and test

```bash
dotnet restore
dotnet build -c Release
dotnet test -c Release
```

## Pack

```bash
dotnet pack src/TlySdk/TlySdk.csproj -c Release -o ./artifacts
```

## Push

```bash
dotnet nuget push ./artifacts/Tly.UrlShortener.1.0.2.nupkg \
  --api-key YOUR_NUGET_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

## Symbols

This project also produces a `.snupkg` symbol package. Push it to NuGet with the same command if you want source debugging support.
