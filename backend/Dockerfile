# ---------------- BASE ----------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# ---------------- BUILD ----------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "./backend.csproj"
RUN dotnet build "./backend.csproj" -c Release -o /app/build

# ---------------- PUBLISH ----------------
FROM build AS publish
RUN dotnet publish "./backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---------------- FINAL ----------------
FROM base AS final
WORKDIR /app
RUN apt-get update \
 && apt-get install -y libgssapi-krb5-2 \
 && apt-get clean \
 && rm -rf /var/lib/apt/lists/*
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "backend.dll"]
