FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY src/LibraryApi/LibraryApi.csproj src/LibraryApi/
RUN dotnet restore src/LibraryApi/LibraryApi.csproj
COPY . .
WORKDIR /src/src/LibraryApi
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LibraryApi.dll"]
