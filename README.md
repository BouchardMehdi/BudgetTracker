# BudgetTracker

BudgetTracker est un MVP de gestion de depenses personnelles avec un backend ASP.NET Core Web API, PostgreSQL, Entity Framework Core, JWT et un frontend React avec Vite.

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

Creer la base si elle n'existe pas encore :

```bash
psql -U postgres -f database/create_database.sql
```

Appliquer le schema avec EF Core :

```bash
cd backend/BudgetTracker.Api
dotnet tool restore
dotnet ef database update
```

Si ta base contient deja les tables creees manuellement avec `database/schema.sql`, le plus simple en developpement est de repartir d'une base vide avant `dotnet ef database update`.

Les scripts `database/schema.sql` et `database/seed.sql` restent disponibles si tu veux initialiser la base manuellement. Le compte de test du seed SQL est :

```text
demo / Password123!
```

## Backend

Adapte la chaine de connexion dans `backend/BudgetTracker.Api/appsettings.json` si besoin :

```json
"DefaultConnection": "Host=localhost;Port=5432;Database=budgettracker;Username=postgres;Password=postgres"
```

Lancer l'API :

```bash
cd backend/BudgetTracker.Api
dotnet restore
dotnet tool restore
dotnet ef database update
dotnet run
```

Par defaut, l'API est disponible sur `http://localhost:5000` et Swagger sur `http://localhost:5000/swagger`.

Endpoints principaux :

- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/transactions`
- `GET /api/transactions/{id}`
- `POST /api/transactions`
- `PUT /api/transactions/{id}`
- `DELETE /api/transactions/{id}`
- `GET /api/categories`
- `POST /api/categories`
- `PUT /api/categories/{id}`
- `DELETE /api/categories/{id}`
- `GET /api/budgets`
- `POST /api/budgets`
- `PUT /api/budgets/{id}`
- `DELETE /api/budgets/{id}`
- `GET /api/budgets/progress?period=current-month`
- `GET /api/stats/summary`
- `GET /api/stats/summary?period=current-month`
- `GET /api/stats/by-category`
- `GET /api/stats/by-category?period=current-month&type=expense`
- `GET /api/stats/latest-transactions?limit=5`

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

- L'authentification JWT est active.
- Les transactions, categories et statistiques utilisent l'utilisateur connecte.
- Les transactions peuvent etre recurrentes mensuellement avec une date de debut et une date de fin optionnelle.
- En developpement, l'API applique les migrations et cree un compte demo si aucun compte demo n'existe.
- Les suppressions de transactions sont des soft deletes : les lignes restent en base avec `is_deleted = true`.
- Les erreurs API utilisent un format JSON `{ code, message, details }`.
- Les types valides sont uniquement `income` et `expense`.
- Les montants doivent etre strictement superieurs a 0.
