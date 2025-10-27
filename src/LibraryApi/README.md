# BibliotecaAberta - API

Este projeto é uma API em .NET 7 que integra os endpoints públicos da Open Library (search, books, covers, authors) e expõe funcionalidades de biblioteca local (coleção, empréstimos).

Principais endpoints

- GET /api/books/search?q={query}
- GET /api/books/olid/{olid}
- GET /api/books/cover/{olid}?size=S|M|L
- POST /api/books/local (adiciona um livro local)
- GET /api/books/local (lista livros locais)
- POST /api/books/local/{id}/lend (emprestar)
- POST /api/books/local/{id}/return (devolver)

Como rodar localmente

1. dotnet restore
2. dotnet build
3. dotnet run --project src/LibraryApi

Docker

- docker build -t bibliotecaaberta:latest .
- docker run -p 5000:80 bibliotecaaberta:latest

Notes: The project uses SQLite (file library.db) by default.
