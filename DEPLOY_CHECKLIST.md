# Déploiement BiblioMate — Check-list de recette

> Objectif : vérifier qu’une mise en production (ou préprod) est fonctionnelle, sécurisée au minimum et traçable, en moins de 10 minutes.

## 1) Pré-requis & configuration

- [ ] **Contexte** confirmé : `ASPNETCORE_ENVIRONMENT=Production` (API)
- [ ] **Secrets** fournis via variables d’env. / vault (pas de secrets commités)
- [ ] **CORS** : seules les origines autorisées sont listées (front prod + éventuelle préprod)
- [ ] **JWT** : clé/signature configurées (`Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`)
- [ ] **Connexions** : SQL Server OK, Mongo OK (chaînes testées hors app)
- [ ] **Email/SMTP** (si notifications actives) : hôte, port, credentials validés

## 2) Base de données (relationnel)

- [ ] **Migrations EF** appliquées :
  ```bash
  dotnet ef database update --project BackendBiblioMate
  ```
- [ ] **Schéma** conforme (tables/colonnes attendues visibles)
- [ ] **Index clés** présents (email unique, colonnes de disponibilité / réservations)
- [ ] **Jeu d’essai** restauré si nécessaire (seed EF ou script SQL)

## 3) Build & publication

- [ ] **Backend** publié :
  ```bash
  dotnet publish -c Release -o out
  ```
- [ ] **Frontend** construit :
  ```bash
  npm ci && ng build --configuration production
  ```
- [ ] **Nginx** (ou serveur web) pointe sur `dist/` (SPA fallback → `index.html`)
- [ ] **Proxy** `/api` et `/hub` → API .NET vérifiés

## 4) Démarrage & santé

- [ ] **Conteneurs/services** lancés (le cas échéant) :
  ```bash
  docker compose up -d
  docker compose ps    # états "running"/"healthy"
  ```
- [ ] **/health** de l’API → `Healthy` (SQL + Mongo OK)
- [ ] **Logs API (niveau Information)** : démarrage propre, pas d’exception

## 5) Swagger & sécurité

- [ ] **Swagger** : protégé (auth basic / IP allowlist) ou **désactivé** publiquement selon la cible
- [ ] **Authorize** dans Swagger fonctionne (login → token obtenu)
- [ ] **Routes protégées** : `401` sans jeton, `403` avec rôle insuffisant, `200` avec rôle requis

## 6) Smoke tests métier (via Swagger)

- [ ] **Login** avec un compte approuvé → `200` + JWT valide
- [ ] **Emprunt** d’un livre disponible → `201 Created` + `dueDate` dans la réponse
- [ ] **Quota** atteint (usager à 3 prêts) → `400` “quota atteint” (pas d’effet de bord)
- [ ] **Réservation** d’un livre indisponible → `201` (file d’attente créée)
- [ ] **Retour** par bibliothécaire → `200` ; si réservation en attente, passage à “Disponible”

## 7) Journalisation & notifications

- [ ] **Notification** après retour : document inséré en Mongo (collection de logs)
- [ ] **Traçabilité** : au moins un log d’activité utilisateur visible (connexion/loan/return)

## 8) Frontend (UX rapide)

- [ ] **Accueil** : menu latéral, recherche latérale, CTA « Consulter le catalogue » opérationnels
- [ ] **Catalogue** : carousel « Nouveautés » (auto-play), recherche simple & avancée OK
- [ ] **Espace perso** : cards de navigation conformes au rôle (usager/biblio/admin)

## 9) Réseau & en-têtes

- [ ] **HTTPS** actif (certificat valide) ; redirection 80 → 443
- [ ] **En-têtes** minimaux : `X-Content-Type-Options: nosniff`, `Referrer-Policy: no-referrer`, `X-Frame-Options: DENY` (ou CSP adaptée)
- [ ] **Compression** activée (gzip/br) pour assets statiques

## 10) Observabilité & rollback

- [ ] **/metrics** exposé si prévu (Prometheus) ; sinon ignoré/filtré
- [ ] **Release/Tag** créé(e) pour cette version (numéro + SHA)
- [ ] **Plan de retour arrière** noté : image/tag précédent ou `dotnet ef database update <PreviousMigration>`

---

### Captures « preuves » recommandées (à joindre)

1. `docker compose ps` avec états *healthy*
2. `GET /health` → `{"status":"Healthy" ...}`
3. Swagger : écran **Authorize** + **POST /login** réussi (payload JWT)
4. Swagger : **POST /loans** → `201` (emprunt) & **POST /loans (quota)** → `400`
5. Swagger : **401** sans token & **403** avec rôle « usager » sur une route admin
6. Mongo Compass : document de notification/log inséré après un retour
7. Page **Catalogue** (carousel auto-play visible) + **Espace perso** (cards selon rôle)

> Astuce : renomme tes captures avec un préfixe d’ordre (ex. `01-health.png`, `02-swagger-login.png`…) pour montrer un déroulé propre dans le dossier **/docs/deploy-proof**.