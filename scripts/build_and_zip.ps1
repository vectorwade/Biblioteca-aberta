# Simple script to build and create a zip
dotnet restore
dotnet build src/LibraryApi/LibraryApi.csproj -c Release
Compress-Archive -Path src -DestinationPath bibliotecaaberta.zip -Force
Write-Host "Created bibliotecaaberta.zip"
