# BiblioMate 📚

**BiblioMate** est une application web de gestion de bibliothèque moderne conçue pour faciliter l'accès aux livres pour les membres et simplifier la gestion des ressources pour les bibliothécaires et administrateurs.

## 🚀 Présentation du projet

**BiblioMate** propose une expérience utilisateur fluide pour :
- **Les membres** : consulter le catalogue, réserver et emprunter des livres, recevoir des recommandations personnalisées.
- **Les bibliothécaires** : gérer les emprunts, les retours et le stock ainsi que superviser l'ensemble des opérations

### Fonctionnalités principales :
- Recherche avancée avec filtres (titre, auteur, genre, disponibilité, etc.).
- Notifications automatiques pour les rappels et les disponibilités.
- Recommandations personnalisées basées sur les préférences des utilisateurs.
- Interface responsive et accessible pour tous types d’appareils.

## 🎯 Objectifs

- Simplifier la gestion des bibliothèques en automatisant les tâches récurrentes.
- Offrir une expérience intuitive et personnalisée pour chaque utilisateur.
- Créer un espace collaboratif et interactif pour dynamiser la communauté autour des bibliothèques.

## 🛠️ Technologies utilisées

- **Back-end** : API RESTful avec .NetCore.
- **Front-end** : Angular et Tailwind CSS.
- **Base de données** : SQL Server avec Entity Framework.
- **CI/CD** : Microsoft Azure et Azure DevOps.
- **Notifications en temps réel** : SignalR.
- **Conception UI/UX** : Figma.

## 🧩 Architecture

BiblioMate repose sur une architecture **MVC** et une structure multicouche (**n-tier**), garantissant une modularité et une évolutivité optimales. Voici un aperçu des principaux composants :

1. **Modèle** : Représente les entités principales comme les livres, les utilisateurs et les emprunts.
2. **Vue** : Interface utilisateur dynamique et responsive.
3. **Contrôleur** : Gère les interactions entre la vue et le modèle.

## 🗺️ Routes principales

### 🌐 Web
- `/` : Accueil et catalogue.
- `/connexion` : Connexion des utilisateurs.
- `/profil` : Espace personnel des membres.
- `/bibliothecaire` : Tableau de bord des bibliothécaires.
- `/administrateur` : Outils avancés pour les administrateurs.

### 🛠️ API
- `GET /api/books` : Liste des livres avec options de recherche.
- `POST /api/auth/register` : Inscription des utilisateurs.
- `POST /api/loans` : Création d’un emprunt.
- `GET /api/stats` : Statistiques pour les administrateurs.

## 🔮 Fonctionnalités futures

- 📖 Prêts numériques et intégration de ressources en ligne.
- 🤝 Organisation d’événements communautaires comme des clubs de lecture.
- 🤖 Recommandations intelligentes via des algorithmes de machine learning.
- 📲 Notifications personnalisées (email, SMS).


## 📋 Installation et utilisation

1. Clonez le repo :
   ```bash
   git clone https://github.com/votre-repo/bibliomate.git
   cd bibliomate

2. Configurez les variables d’environnement.

3. Lancez le serveur :
   ```bash
   dotnet run

4. Démarrez le front-end :
   ```bash
   ng serve

5. Accédez à l'application via http://localhost:4200.

## 📖 Documentation

- API Documentation : Disponible via Swagger à http://localhost:<port>/swagger.
- Diagrammes UML : Consultez le dossier docs/uml pour les diagrammes de classes et cas d’utilisation.

## 👥 Équipe
- Développement : Juste moi :)
- Design : Juste moi :)
