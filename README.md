# BiblioMate 📚

**BiblioMate** est une application web de gestion de bibliothèque moderne conçue pour faciliter l'accès aux livres pour les membres et simplifier la gestion des ressources pour les bibliothécaires et administrateurs.

## 🚀 Présentation du projet

**BiblioMate** propose une expérience utilisateur fluide pour :
- 👤 **Les membres** : consulter le catalogue, réserver et emprunter des livres, recevoir des recommandations.
- 📚 **Les bibliothécaires** : gérer les stocks, les retours, les utilisateurs et suivre les emprunts.
- 🛡 **Les administrateurs** : superviser l’ensemble du système et gérer les rôles utilisateurs.

### Fonctionnalités principales :
- 🔍 Recherche avancée (titre, auteur, genre, disponibilité, etc.)
- 🔐 Authentification sécurisée avec JWT
- 📦 Gestion des stocks avec ajustement de quantité
- 📅 Suivi des emprunts et historiques
- 📬 Notifications et rappels automatiques
- 📊 Dashboard pour les rôles métiers
- 📁 Architecture en couches et logique métier claire
- 📘 Documentation Swagger générée automatiquement

## 🎯 Objectifs

- Simplifier la gestion des bibliothèques en automatisant les tâches récurrentes.
- Offrir une expérience intuitive et personnalisée pour chaque utilisateur.
- Créer un espace collaboratif et interactif pour dynamiser la communauté autour des bibliothèques.

## 🛠️ Technologies utilisées

- **Back-end** : ASP.NET Core 9 (API RESTful)
- **Base de données** : SQL Server + Entity Framework Core
- **Front-end** : Angular + Tailwind CSS (dans un projet séparé)
- **Authentification** : JWT Bearer Tokens
- **CI/CD** : Azure DevOps + Microsoft Azure
- **Notifications** : SignalR (en développement)
- **Design/Prototype** : Figma
- 
## 🧩 Architecture

Le projet suit une **architecture en couches (n-tier)** avec séparation claire des responsabilités :

- **Controllers** : exposent des routes RESTful, valident les accès (via `[Authorize]`)
- **Models (EF)** : entités représentant la base de données
- **DTOs** : formats spécifiques pour lecture, création ou mise à jour, évitant toute surexposition de la base
- **Middleware & Config** : gestion de l’authentification, autorisation, Swagger, CORS, etc.

Le front-end Angular consomme cette API.

## 📌 Routes représentatives de l’API

Quelques exemples parmi les plus pertinentes :

| Méthode | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/auth/register` | Inscription utilisateur avec email de confirmation |
| `POST` | `/api/auth/login` | Authentification, génération de JWT |
| `GET` | `/api/users/me` | Récupération du profil connecté |
| `PUT` | `/api/users/{id}/role` | Modification du rôle d’un utilisateur (Admin only) |
| `PATCH` | `/api/stocks/{id}/adjust` | Ajustement intelligent du stock (positif/négatif) |
| `GET` | `/api/shelves?page=1&zoneId=` | Pagination + filtrage des étagères |
| `POST` | `/api/loans` | Création d’un emprunt (stock vérifié) |
| `PUT` | `/api/reports/{id}` | Modification contrôlée (auteur uniquement) |

## 🔮 Fonctionnalités futures

- 📖 Prêts numériques et intégration de ressources en ligne.
- 🤝 Organisation d’événements communautaires comme des clubs de lecture.
- 🤖 Recommandations intelligentes via des algorithmes de machine learning.
- 📲 Notifications personnalisées (email, SMS).
- 🔔 Intégration SignalR pour alertes en temps réel (non implémentée).

## 📋 Installation et utilisation

1. Clonez le repo :
```bash
git clone https://github.com/TracyBachmann/Bibliomate_Backend.git
cd backend
```

2. Configurez les variables d’environnement (`appsettings.json` + tokens secrets)

3. Lancez le serveur :
```bash
dotnet run
```

4. Démarrez le front-end :
```bash
cd frontend
ng serve
```

5. Accédez à l'application :
- Back-end API : `http://localhost:5077/swagger`
- Front-end Angular : `http://localhost:4200`

## 📖 Documentation

- **API RESTful complète** avec contrôleurs et droits d'accès sécurisés.
- **Commentaires XML** générant automatiquement une documentation claire via Swagger.
- 🔗 Swagger disponible à : `http://localhost:5077/swagger`
- 📂 Diagrammes UML : docs/uml
- 📄 DTOs disponibles dans `/backend/DTOs`, séparés proprement du modèle EF.

## 👥 Équipe
- Développement : Juste moi :)
- Design : Juste moi :)

## 👥 Équipe

- 💻 Développement : Tracy Bachmann
- 🎨 Design & UX : Tracy Bachmann
