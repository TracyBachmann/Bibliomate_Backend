# BiblioMate – Backend (.NET)

**BiblioMate** est l’API REST de gestion de bibliothèque (ASP.NET Core 9 / C#) adossée à **SQL Server** pour l’état métier et à **MongoDB** pour la journalisation applicative (append‑only).

## 🚀 Vue d’ensemble

- **Domaines couverts** : livres, stocks, emprunts, réservations, notifications, historique.
- **Persistance** : SQL Server via **Entity Framework Core** (migrations & code‑first).
- **Journalisation NoSQL** : MongoDB (collection configurable `logEntries`) pour logs d’activité et notifications (outbox).
- **AuthN/AuthZ** : JWT Bearer + rôles (`User`, `Librarian`, `Admin`).
- **Observabilité** : Health checks `/health`, métriques Prometheus `/metrics`.
- **Docs** : Swagger/OpenAPI versionnée.

## 🧰 Technologies

- **Runtime** : .NET 9 (ASP.NET Core)
- **ORM** : EF Core (Migrations)
- **Base SQL** : Microsoft SQL Server 2022
- **NoSQL (logs)** : MongoDB 7
- **Docs** : Swashbuckle (Swagger UI)
- **CI/CD** : GitHub Actions (build & push image Docker)
- **Container** : Docker / Docker Compose

## 🗂️ Structure (extrait)

```
BackendBiblioMate/
├── Controllers/
│   ├── BooksController.cs
│   ├── LoansController.cs
│   └── ReservationsController.cs
├── Services/
│   ├── Loans/LoanService.cs
│   ├── Users/UserService.cs
│   ├── Notifications/NotificationService.cs
│   └── ...
├── Interfaces/
├── DTOs/
├── Models/               # Entités EF + Mongo (UserActivityLogDocument, NotificationLogDocument, ...)
├── Data/
│   └── BiblioMateDbContext.cs
├── Migrations/
├── Middlewares/
├── Hubs/
├── Program.cs
└── appsettings*.json
```


## ⚙️ Configuration

Les *secrets* ne sont pas commités. Les paramètres se trouvent dans `appsettings.*.json` (ou variables d’environnement) :

```jsonc
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=BiblioMateDb;User ID=sa;Password=***;TrustServerCertificate=True;Encrypt=False;"
  },
  "MongoDb": {
    "ConnectionString": "mongodb://admin:password@localhost:27017",
    "DatabaseName": "BiblioMateLogs",
    "LogCollectionName": "logEntries"
  },
  "Jwt": {
    "Key": "...",
    "Issuer": "BiblioMateAPI",
    "Audience": "BiblioMateClient"
  },
  "Frontend": { "BaseUrl": "http://localhost:4200" },
  "Logging": { "LogLevel": { "Default": "Information" } }
}
```

> **Bonnes pratiques** : connexion Mongo externalisée, compte Mongo à droits minimaux (rôle limité sur la base ciblée).  
> **Healthcheck Mongo** : activé si une chaîne est fournie (voir `Program.cs`).


## ▶️ Démarrage local

### Prérequis
- .NET 9 SDK
- SQL Server (local ou conteneur)
- (Optionnel) MongoDB pour la journalisation

### Lancer l’API
```bash
cd BackendBiblioMate
dotnet restore
dotnet ef database update      # applique les migrations
dotnet run                     # démarre l’API sur http://localhost:5000 (ou 5001 mappé)
```

Swagger : `http://localhost:5001/swagger` (selon votre port mapping).


## 🐳 Docker & Compose

Extraits utiles :

**Dockerfile (multi‑stage, runtime ASP.NET 9)**
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
# ...
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
ENTRYPOINT ["dotnet", "BackendBiblioMate.dll"]
```

**docker-compose.yml (services SQL, Mongo, backend, Prometheus, Grafana…)**
```yaml
services:
  serverSQL:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - SA_PASSWORD=Test1234!
      - ACCEPT_EULA=Y
    ports: ["1450:1433"]

  mongo:
    image: mongo:7
    environment:
      MONGO_INITDB_ROOT_USERNAME: admin
      MONGO_INITDB_ROOT_PASSWORD: password
    ports: ["27017:27017"]

  backend:
    build:
      context: ./BackendBiblioMate
      dockerfile: Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=serverSQL,1433;Database=BiblioMateDb;User Id=sa;Password=Test1234!;TrustServerCertificate=True;Encrypt=False;
      - MongoDb__ConnectionString=mongodb://admin:password@mongo:27017/?authSource=admin
      - MongoDb__DatabaseName=BiblioMateLogs
    ports: ["5001:5000"]
    depends_on: [serverSQL, mongo]
```

Démarrer :
```bash
docker compose up -d --build
```


## 🧪 API représentative (extraits)

### Books
- `GET /api/v1/books` — liste paginée (+ ETag)
- `GET /api/v1/books/{id}`
- `POST /api/v1/books` *(Librarian/Admin)*
- `PUT /api/v1/books/{id}` *(Librarian/Admin)*
- `DELETE /api/v1/books/{id}` *(Librarian/Admin)*
- `POST /api/v1/books/search` — recherche multi‑filtres

### Loans
- `POST /api/v1/loans` — créer un emprunt
- `PUT  /api/v1/loans/{id}/return` — retour d’un emprunt *(Librarian/Admin)*
- `GET  /api/v1/loans` — tous les emprunts (selon rôles)
- `GET  /api/v1/loans/{id}`
- `PUT  /api/v1/loans/{id}` *(Librarian/Admin)*
- `DELETE /api/v1/loans/{id}` *(Librarian/Admin)*
- `GET  /api/v1/loans/active/me` — mes emprunts actifs
- `GET  /api/v1/loans/active/me/{bookId}` — ai‑je un emprunt actif sur ce livre ?
- `POST /api/v1/loans/{id}/extend` — prolonger (selon rôles/ownership)

### Reservations
- `GET  /api/v1/reservations` *(Admin/Librarian)*
- `GET  /api/v1/reservations/user/{id}` — réservations d’un utilisateur (ownership ou rôles)
- `GET  /api/v1/reservations/book/{id}/pending` *(Admin/Librarian)*
- `GET  /api/v1/reservations/{id}` — détail (ownership ou rôles)
- `POST /api/v1/reservations` *(User)*
- `PUT  /api/v1/reservations/{id}` *(Admin/Librarian)*
- `DELETE /api/v1/reservations/{id}` — (ownership ou rôles)

> Les routes sont versionnées (`/api/v1/...`) via API Versioning. Voir `Program.cs`.


## 🧱 Modèle de données (noyau)

- `User`, `Book`, `Stock` (1–1), `Loan` (User×Stock), `Reservation` (User×Book), référentiels `Author`/`Editor`/`Genre`, N‑N `BookTag`, préférences `UserGenre`.  
- Contraintes d’intégrité : unicité `Users.Email`, `Books.Isbn`, FK et clés composites (`BookTag`, `UserGenre`).  
- Politique de suppression stricte pour préserver l’historique (voir `OnModelCreating` et Migrations).


## 🔄 CI/CD (GitHub Actions)

Deux workflows (extraits) construisent et poussent les images Docker :

**Backend – Build & Push**
```yaml
- uses: docker/setup-qemu-action@v3
- uses: docker/setup-buildx-action@v3
- uses: docker/login-action@v3
- uses: docker/build-push-action@v6
  with:
    file: BackendBiblioMate/Dockerfile
    push: true
```

**Frontend – Build & Push** *(dans le dépôt front)*
```yaml
- uses: docker/setup-buildx-action@v3
- uses: docker/login-action@v3
- uses: docker/build-push-action@v6
  with:
    file: ./Dockerfile
    push: true
```

> Les **secrets** (Docker Hub, SQL, Mongo, JWT, SMTP/SendGrid, …) sont stockés dans *Settings ▸ Secrets and variables* et injectés à l’exécution.


## 📎 Divers

- Swagger UI : `/swagger`
- Health : `/health`
- Metrics : `/metrics`


## 📝 Licence

Projet académique. Voir le dépôt pour les mentions complémentaires.

