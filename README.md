# 📚 BiblioMate – Backend

BiblioMate est une application web moderne de gestion de bibliothèque visant à faciliter l’accès aux livres pour les membres et simplifier les tâches quotidiennes pour les bibliothécaires.

---

## 🚀 Présentation du projet

### Utilisateurs

- **Membres** : naviguer dans le catalogue, réserver et emprunter des livres, recevoir des recommandations personnalisées.
- **Bibliothécaires** : gérer les emprunts, retours, stocks, et notifications.
- **Administrateurs** : supervision des opérations, rapports, statistiques.

### Fonctionnalités principales

- 🔍 Recherche avancée (titre, auteur, genre, disponibilité…)
- 🔔 Notifications automatiques (rappels, retards, disponibilité…)
- 🎯 Recommandations intelligentes selon les préférences utilisateur
- 💡 Interface responsive pour tous les appareils

---

## 🎯 Objectifs

- Automatiser la gestion des tâches répétitives des bibliothèques
- Offrir une expérience fluide, intuitive et personnalisée
- Renforcer la communauté autour des livres avec des interactions

---

## 🛠️ Technologies utilisées

- **Back-end** : ASP.NET Core (.NET 8)
- **Base de données** : SQL Server via Entity Framework Core
- **Authentification** : Jeton JWT (via `Microsoft.AspNetCore.Authentication`)
- **Notifications** : Système de rappel et d’alerte intégré
- **CI/CD** : Azure DevOps / GitHub Actions (à venir)
- **Front-end** *(hors de ce repo)* : Angular + Tailwind CSS

---

## 🧩 Architecture

- **Modèle (Models/)** : entités métier (User, Book, Loan, etc.)
- **Contrôleur (Controllers/)** : API RESTful (`/api/users`, `/api/books`, etc.)
- **Contexte DB** : `AppDbContext` via `EntityFrameworkCore`
- **Structure n-tiers** : séparation claire des responsabilités

---

## 🗺️ Exemples de routes API

| Méthode | Route                  | Description                        |
|--------:|------------------------|------------------------------------|
| `GET`   | `/api/users`           | Liste des utilisateurs             |
| `GET`   | `/api/books`           | Liste des livres                   |
| `POST`  | `/api/auth/register`   | Création de compte                 |
| `POST`  | `/api/loans`           | Créer un emprunt                   |
| `GET`   | `/api/stats`           | Statistiques pour les admins       |

---

## 🔮 Fonctionnalités futures

- 📱 Intégration de prêts numériques
- 🤖 Recommandations ML avancées
- 📅 Gestion d’événements communautaires
- 📨 Notifications par email et SMS

---

## 📋 Installation locale

1. **Clonez le repo :**

   ```bash
   git clone https://github.com/TracyBachmann/BiblioMate_Backend.git
   cd BiblioMate_Backend
   ```

2. **Configurez les variables d’environnement** :

   - Le fichier `appsettings.Development.json` contient la chaîne de connexion SQL Server.

3. **Démarrez le serveur :**

   ```bash
   dotnet run
   ```

---

## 📖 Documentation

- Diagrammes UML : à venir dans le dossier `docs/uml`

---

## 👥 Équipe

- 💻 Développement : Tracy Bachmann
- 🎨 Design & UX : Tracy Bachmann
