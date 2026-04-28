<p align="center">
  <img src="frontend/public/logo.png" alt="BudgetTracker logo" width="96" height="96">
</p>

# BudgetTracker

BudgetTracker est une application fullstack de gestion de budget personnel. Elle permet de suivre ses revenus, ses depenses, ses categories, ses budgets mensuels et ses statistiques financieres depuis une interface simple, moderne et responsive.

Le projet est construit comme un MVP propre et deployable : frontend React avec Vite, backend ASP.NET Core Web API, PostgreSQL, Entity Framework Core, JWT, Docker et Nginx en reverse proxy pour la production.

## Sommaire

- [Apercu](#apercu)
- [Fonctionnalites](#fonctionnalites)
- [Stack technique](#stack-technique)
- [Architecture](#architecture)
- [Structure du projet](#structure-du-projet)
- [Logo](#logo)
- [Prerequis](#prerequis)
- [Lancement rapide avec Docker](#lancement-rapide-avec-docker)
- [Lancement sans Docker](#lancement-sans-docker)
- [Configuration](#configuration)
- [Base de donnees](#base-de-donnees)
- [Migrations EF Core](#migrations-ef-core)
- [Authentification JWT](#authentification-jwt)
- [API REST](#api-rest)
- [Frontend](#frontend)
- [Responsive design](#responsive-design)
- [Docker](#docker)
- [Deploiement VPS](#deploiement-vps)
- [Mise a jour en production](#mise-a-jour-en-production)
- [Donnees de test](#donnees-de-test)
- [Commandes utiles](#commandes-utiles)
- [Depannage](#depannage)
- [Ameliorations possibles](#ameliorations-possibles)

## Apercu

BudgetTracker aide un utilisateur a comprendre rapidement sa situation financiere :

- combien il a gagne ;
- combien il a depense ;
- quel est son solde ;
- quelles categories coutent le plus ;
- quels budgets mensuels sont proches de leur limite ;
- quelles transactions reviennent chaque mois.

L'application est prevue pour etre lancee en local pendant le developpement, mais aussi pour etre deployee sur un VPS avec Docker Compose, PostgreSQL en conteneur, Nginx et Certbot.

## Fonctionnalites

- Creation de compte et connexion avec JWT.
- Dashboard avec revenus, depenses, solde et comparaison par periode.
- Statistiques du mois courant, du mois precedent et de l'annee.
- Depenses par categorie.
- Dernieres transactions.
- Liste des transactions avec pagination.
- Filtres par recherche, type, categorie et mois.
- Ajout, modification et suppression de transactions.
- Suppression douce des transactions cote backend pour conserver l'historique.
- Transactions recurrentes mensuelles avec date de debut et date de fin optionnelle.
- Gestion des categories de revenus et de depenses.
- Budgets mensuels par categorie avec progression.
- Devise configurable cote interface.
- Messages d'erreur lisibles cote frontend.
- Interface responsive desktop, tablette et mobile.

## Stack technique

### Frontend

- React 18
- Vite
- React Router
- Axios
- Lucide React
- CSS classique

### Backend

- ASP.NET Core 8 Web API
- C#
- Entity Framework Core
- Npgsql PostgreSQL provider
- JWT Bearer Authentication
- Swagger en developpement

### Base de donnees

- PostgreSQL
- Migrations EF Core
- Scripts SQL manuels disponibles dans `database/`

### DevOps

- Docker
- Docker Compose
- Nginx
- Certbot
- VPS Linux

## Architecture

En local avec Docker :

```text
Navigateur
  -> http://localhost:8087
  -> conteneur frontend Nginx
  -> /api proxifie vers le conteneur backend
  -> backend ASP.NET Core
  -> conteneur PostgreSQL
```

En production sur VPS :

```text
Navigateur
  -> HTTPS
  -> Nginx sur le VPS
  -> conteneur frontend
  -> conteneur backend API
  -> conteneur PostgreSQL
```

Le frontend et le backend sont separes, mais Docker Compose permet de lancer tous les services avec une seule commande.

## Structure du projet

```text
BudgetTracker/
├── backend/
│   └── BudgetTracker.Api/
│       ├── Controllers/
│       ├── Data/
│       ├── DTOs/
│       ├── Migrations/
│       ├── Models/
│       ├── Services/
│       ├── Dockerfile
│       ├── appsettings.json
│       ├── appsettings.Example.json
│       └── appsettings.Production.json
├── database/
│   ├── create_database.sql
│   ├── schema.sql
│   └── seed.sql
├── docs/
│   ├── deployment-vps-docker.md
│   ├── portfolio-deploiement-docker.md
│   └── nginx/
│       └── budgettracker.conf.example
├── frontend/
│   ├── public/
│   │   └── logo.png
│   ├── src/
│   │   ├── api/
│   │   ├── components/
│   │   ├── context/
│   │   ├── pages/
│   │   ├── utils/
│   │   ├── App.jsx
│   │   ├── main.jsx
│   │   └── styles.css
│   ├── Dockerfile
│   └── nginx.conf
├── docker-compose.yml
├── .env.example
└── README.md
```

## Logo

Le logo se trouve ici :

```text
frontend/public/logo.png
```

Il est utilise comme favicon et dans la barre de navigation du frontend. Le fichier PNG est versionne dans le projet, donc il fonctionne en local, avec Docker et en production sans dependance externe.

## Prerequis

Pour lancer avec Docker :

- Docker
- Docker Compose

Pour lancer sans Docker :

- .NET SDK 8
- Node.js 20 ou plus
- PostgreSQL 14 ou plus
- EF Core CLI via le manifest local du backend

## Lancement rapide avec Docker

Depuis la racine du projet :

```bash
cp .env.example .env
docker compose up -d --build
```

Sur Windows PowerShell :

```powershell
copy .env.example .env
docker compose up -d --build
```

Acces local :

```text
Frontend : http://localhost:8087
API      : http://localhost:5007/api
```

Voir l'etat des conteneurs :

```bash
docker compose ps
```

Voir les logs :

```bash
docker compose logs --tail=50
```

Arreter le projet :

```bash
docker compose down
```

Supprimer aussi la base locale Docker :

```bash
docker compose down -v
```

Attention : `-v` supprime le volume PostgreSQL et donc les donnees locales.

## Lancement sans Docker

### 1. Base PostgreSQL locale

Creer la base :

```bash
psql -U postgres -f database/create_database.sql
```

Configurer la connexion dans :

```text
backend/BudgetTracker.Api/appsettings.json
```

Exemple :

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=budget_tracker;Username=postgres;Password=postgres"
  }
}
```

### 2. Backend

```bash
cd backend/BudgetTracker.Api
dotnet restore
dotnet tool restore
dotnet ef database update
dotnet run
```

Par defaut :

```text
API     : http://localhost:5000
Swagger : http://localhost:5000/swagger
```

### 3. Frontend

```bash
cd frontend
npm install
npm run dev
```

Par defaut :

```text
Frontend : http://localhost:5173
```

## Configuration

### Docker

La configuration Docker se fait avec le fichier `.env` a la racine du projet.

Exemple :

```env
COMPOSE_PROJECT_NAME=budgettracker

FRONTEND_PORT=8087
BACKEND_PORT=5007

POSTGRES_DB=budget_tracker
POSTGRES_USER=budgettracker
POSTGRES_PASSWORD=change_me_postgres_password

JWT_KEY=change_me_with_a_long_random_secret_at_least_32_chars
JWT_ISSUER=BudgetTracker.Api
JWT_AUDIENCE=BudgetTracker.Frontend

CORS_ORIGIN=http://localhost:8087
DATABASE_AUTO_MIGRATE=true
```

En Docker, la chaine de connexion du backend est construite depuis ces variables. Le backend se connecte a PostgreSQL avec `Host=db`, car `db` est le nom du service PostgreSQL dans `docker-compose.yml`.

### Backend local

Sans Docker, le backend lit `appsettings.json`.

Dans ce cas, PostgreSQL est generalement accessible avec :

```text
Host=localhost
```

Avec Docker, PostgreSQL est accessible avec :

```text
Host=db
```

## Base de donnees

Tables principales :

- `users`
- `categories`
- `transactions`
- `budgets`
- `__EFMigrationsHistory`

Relations :

- un utilisateur possede plusieurs categories ;
- un utilisateur possede plusieurs transactions ;
- une categorie possede plusieurs transactions ;
- une transaction appartient a un utilisateur et a une categorie ;
- un budget est lie a une categorie et a un utilisateur.

Les scripts SQL sont disponibles dans :

```text
database/
```

Ils sont utiles pour comprendre le schema ou initialiser la base manuellement, mais le mode recommande est d'utiliser les migrations EF Core.

## Migrations EF Core

Appliquer les migrations :

```bash
cd backend/BudgetTracker.Api
dotnet ef database update
```

Ajouter une migration :

```bash
dotnet ef migrations add NomDeLaMigration
```

En Docker, si `DATABASE_AUTO_MIGRATE=true`, le backend applique les migrations au demarrage en environnement `Production`.

## Authentification JWT

L'application utilise JWT pour proteger les routes API.

Flux principal :

1. l'utilisateur cree un compte avec `POST /api/auth/register` ;
2. il se connecte avec `POST /api/auth/login` ;
3. le backend retourne un token JWT ;
4. le frontend stocke le token ;
5. Axios ajoute le token dans le header `Authorization`.

Header envoye :

```http
Authorization: Bearer <token>
```

## API REST

URL locale selon le mode de lancement :

```text
Docker     : http://localhost:5007/api
Sans Docker: http://localhost:5000/api
```

### Auth

| Methode | Endpoint | Description |
| --- | --- | --- |
| POST | `/api/auth/register` | Creer un compte |
| POST | `/api/auth/login` | Se connecter |

### Transactions

| Methode | Endpoint | Description |
| --- | --- | --- |
| GET | `/api/transactions` | Lister les transactions |
| GET | `/api/transactions/{id}` | Recuperer une transaction |
| POST | `/api/transactions` | Creer une transaction |
| PUT | `/api/transactions/{id}` | Modifier une transaction |
| DELETE | `/api/transactions/{id}` | Supprimer une transaction |

Filtres disponibles sur la liste :

```text
page
pageSize
search
type
categoryId
month
```

Exemple :

```text
GET /api/transactions?page=1&pageSize=10&type=expense&month=2026-04
```

### Categories

| Methode | Endpoint | Description |
| --- | --- | --- |
| GET | `/api/categories` | Lister les categories |
| POST | `/api/categories` | Creer une categorie |
| PUT | `/api/categories/{id}` | Modifier une categorie |
| DELETE | `/api/categories/{id}` | Supprimer une categorie |

### Budgets

| Methode | Endpoint | Description |
| --- | --- | --- |
| GET | `/api/budgets` | Lister les budgets |
| POST | `/api/budgets` | Creer un budget |
| PUT | `/api/budgets/{id}` | Modifier un budget |
| DELETE | `/api/budgets/{id}` | Supprimer un budget |
| GET | `/api/budgets/progress` | Voir la progression des budgets |

Exemple :

```text
GET /api/budgets/progress?period=current-month
```

### Statistiques

| Methode | Endpoint | Description |
| --- | --- | --- |
| GET | `/api/stats/summary` | Total revenus, total depenses et solde |
| GET | `/api/stats/by-category` | Totaux groupes par categorie |
| GET | `/api/stats/latest-transactions` | Dernieres transactions |

Periodes supportees :

```text
all
current-month
previous-month
current-year
```

Exemple :

```text
GET /api/stats/summary?period=current-month
```

## Frontend

Pages principales :

- `Dashboard`
- `Transactions`
- `AddTransaction`
- `EditTransaction`
- `Categories`
- `Login`
- `Register`

Organisation du code :

```text
frontend/src/
├── api/
├── components/
├── context/
├── pages/
├── utils/
├── App.jsx
├── main.jsx
└── styles.css
```

Les appels HTTP sont centralises dans `frontend/src/api/`. Le client Axios ajoute automatiquement le token JWT si l'utilisateur est connecte.

## Responsive design

L'interface est adaptee aux ecrans desktop, tablette et mobile :

- la navigation se reorganise sur mobile ;
- les grilles passent en une colonne ;
- les formulaires restent lisibles sur petit ecran ;
- le tableau des transactions devient une liste de cartes sur mobile ;
- les boutons prennent toute la largeur quand c'est plus ergonomique ;
- les messages d'erreur evitent les debordements visuels.

## Docker

Le projet contient trois services Docker :

```text
front : frontend React compile et servi par Nginx
back  : API ASP.NET Core
db    : PostgreSQL
```

Le frontend appelle l'API via `/api`. Dans le conteneur frontend, Nginx proxifie `/api/` vers le service Docker `back`.

Extrait simplifie :

```yaml
services:
  front:
    build: ./frontend
    ports:
      - "127.0.0.1:8087:80"

  back:
    build: ./backend/BudgetTracker.Api
    ports:
      - "127.0.0.1:5007:8080"

  db:
    image: postgres:16-alpine
```

Les ports sont lies a `127.0.0.1` pour eviter d'exposer directement les conteneurs sur internet. En production, Nginx gere l'acces public.

## Deploiement VPS

Documentation detaillee :

- [docs/deployment-vps-docker.md](docs/deployment-vps-docker.md)
- [docs/nginx/budgettracker.conf.example](docs/nginx/budgettracker.conf.example)
- [docs/portfolio-deploiement-docker.md](docs/portfolio-deploiement-docker.md)

Etapes generales :

1. creer un sous-domaine DNS qui pointe vers l'IP du VPS ;
2. cloner le projet dans `/home/projects` ;
3. creer le fichier `.env` ;
4. lancer les conteneurs avec Docker Compose ;
5. configurer Nginx comme reverse proxy ;
6. tester la configuration Nginx ;
7. activer HTTPS avec Certbot.

Commandes principales :

```bash
cd /home/projects/BudgetTracker
cp .env.example .env
docker compose up -d --build
docker compose ps
docker compose logs --tail=50
```

Nginx :

```bash
nginx -t
systemctl reload nginx
```

Certbot :

```bash
certbot --nginx -d budgettracker.bouchard-mehdi.fr
```

## Mise a jour en production

Pour une mise a jour classique :

```bash
cd /home/projects/BudgetTracker
git pull
docker compose up -d --build
```

`docker compose down` n'est pas necessaire pour une mise a jour normale. Il est surtout utile en cas de changement important de volumes, reseaux, ports ou pour faire un reset complet.

## Donnees de test

En developpement, le backend peut creer des donnees de demonstration au demarrage si aucun compte demo n'existe.

Compte de test general :

```text
Username : demo
Password : Password123!
```

Le script SQL de seed reste disponible ici :

```text
database/seed.sql
```

## Commandes utiles

### Backend

```bash
cd backend/BudgetTracker.Api
dotnet restore
dotnet build
dotnet run
dotnet ef database update
```

### Frontend

```bash
cd frontend
npm install
npm run dev
npm run build
```

### Docker

```bash
docker compose up -d --build
docker compose ps
docker compose logs --tail=50
docker compose logs -f back
docker compose logs -f front
docker compose logs -f db
docker compose down
docker compose down -v
```

## Depannage

### Erreur `No .NET SDKs were found`

Le SDK .NET n'est pas installe ou n'est pas detecte dans le terminal.

Solution :

- installer le SDK .NET 8 ;
- fermer et rouvrir le terminal ;
- verifier avec `dotnet --info`.

### Erreur `relation "users" existe deja`

La base contient deja des tables creees manuellement, mais EF Core veut appliquer une migration initiale.

Solution simple en developpement :

```bash
docker compose down -v
docker compose up -d --build
```

Sans Docker, il faut repartir d'une base vide ou aligner proprement l'historique des migrations.

### Erreur 405 sur `/api/auth/register`

Cela arrive si le frontend envoie `/api` vers le serveur frontend au lieu du backend.

Avec Docker, le fichier `frontend/nginx.conf` contient une regle qui proxifie `/api/` vers `http://back:8080/api/`.

Appliquer la correction :

```bash
docker compose up -d --build
```

### Changement de `POSTGRES_USER` ou `POSTGRES_PASSWORD`

Si le volume PostgreSQL existe deja, changer les variables dans `.env` ne modifie pas automatiquement l'utilisateur deja cree.

Pour repartir de zero en local :

```bash
docker compose down -v
docker compose up -d --build
```

Attention : cette commande supprime les donnees locales.

### API inaccessible depuis le frontend

Verifier :

- les conteneurs avec `docker compose ps` ;
- les logs backend avec `docker compose logs -f back` ;
- la variable `CORS_ORIGIN` dans `.env` ;
- le proxy `/api` dans `frontend/nginx.conf`.

## Ameliorations possibles

- Ajout d'une page profil utilisateur.
- Export CSV ou PDF des transactions.
- Graphiques plus avances.
- Tests unitaires backend.
- Tests d'integration API.
- Tests end-to-end frontend.
- Gestion multi-utilisateur plus complete.
- Reset de mot de passe.
- Notifications quand un budget approche de sa limite.
