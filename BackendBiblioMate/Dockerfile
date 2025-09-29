# ---------------- RESTORE/BUILD ----------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 1) Copier uniquement le csproj pour tirer parti du cache
COPY BackendBiblioMate/*.csproj BackendBiblioMate/

# 2) Restore sur le bon chemin
RUN dotnet restore BackendBiblioMate/BackendBiblioMate.csproj

# 3) Copier tout le code puis build
COPY . .
WORKDIR /src/BackendBiblioMate
RUN dotnet build BackendBiblioMate.csproj -c Release -o /app/build

# ---------------- PUBLISH ----------------
FROM build AS publish
WORKDIR /src/BackendBiblioMate
RUN dotnet publish BackendBiblioMate.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---------------- RUNTIME ----------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# (optionnel) dépendance pour Microsoft.Data.SqlClient Kerberos
RUN apt-get update \
 && apt-get install -y libgssapi-krb5-2 \
 && apt-get clean \
 && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

# Écoute sur 5000 (aligne avec docker-compose/Nginx)
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "BackendBiblioMate.dll"]
