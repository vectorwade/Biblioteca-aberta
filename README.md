# BibliotecaAberta

Projeto de exemplo: API em C# .NET que integra as APIs públicas do Open Library e adiciona funcionalidades de biblioteca local.

Conteúdo:

Geração do zip (na raiz do workspace):

1. No Windows PowerShell:
   Compress-Archive -Path .\src -DestinationPath bibliotecaaberta.zip -Force

Como rodar:
1. dotnet restore
2. dotnet run --project src/LibraryApi

Frontend (React) build

The project includes a React SPA in `src/LibraryApi/ClientApp`. To build and publish the static files into the API's `wwwroot`:

1. cd src/LibraryApi/ClientApp
2. npm install
3. npm run build

The Vite build is configured to output into `src/LibraryApi/wwwroot`, so after building the client you can run the API and it will serve the SPA.

- Build Docker image e subir para qualquer container registry, ou deploy em Azure Web App for Containers.
