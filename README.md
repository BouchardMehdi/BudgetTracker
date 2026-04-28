# BudgetTracker

BudgetTracker est un MVP de gestion de depenses personnelles avec un backend ASP.NET Core Web API, PostgreSQL, Entity Framework Core et un frontend React avec Vite.

## Structure

```text
backend/BudgetTracker.Api   API REST ASP.NET Core
frontend                    Application React/Vite
database                    Scripts SQL PostgreSQL
```

## Prerequis

- .NET SDK 8
- Node.js 20 ou plus
- PostgreSQL 14 ou plus

## Base de donnees

Depuis un terminal PostgreSQL avec un utilisateur autorise :

```bash
psql -U postgres -f database/create_database.sql
psql -U postgres -d budgettracker -f database/schema.sql
psql -U postgres -d budgettracker -f database/seed.sql
```

La premiere version utilise `user_id = 1` par defaut. Le fichier `database/seed.sql` cree cet utilisateur et quelques donnees de test.

## Backend

Adapte la chaine de connexion dans `backend/BudgetTracker.Api/appsettings.json` si besoin :

```json
"DefaultConnection": "Host=localhost;Port=5432;Database=budgettracker;Username=postgres;Password=postgres"
```

Lancer l'API :

```bash
cd backend/BudgetTracker.Api
dotnet restore
dotnet run
```

Par defaut, l'API est disponible sur `http://localhost:5000` et Swagger sur `http://localhost:5000/swagger`.

Endpoints principaux :

- `GET /api/transactions`
- `GET /api/transactions/{id}`
- `POST /api/transactions`
- `PUT /api/transactions/{id}`
- `DELETE /api/transactions/{id}`
- `GET /api/categories`
- `POST /api/categories`
- `PUT /api/categories/{id}`
- `DELETE /api/categories/{id}`
- `GET /api/stats/summary`
- `GET /api/stats/by-category`

## Frontend

Configurer l'URL de l'API si elle change :

```bash
cd frontend
cp .env.example .env
```

Lancer l'application :

```bash
cd frontend
npm install
npm run dev
```

Le frontend est disponible sur `http://localhost:5173`.

## Notes MVP

- Pas d'authentification JWT dans cette version.
- La table `users` est deja prevue pour une future authentification.
- Les types valides sont uniquement `income` et `expense`.
- Les montants doivent etre strictement superieurs a 0.
