# Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY *.sln ./
COPY PdiCrud/*.csproj ./PdiCrud/
RUN dotnet restore

COPY . .
RUN dotnet publish PdiCrud/PdiCrud.csproj -c Release -o /app/out

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080}
EXPOSE 8080

COPY --from=build /app/out ./

ENTRYPOINT ["dotnet", "PdiCrud.dll"]
