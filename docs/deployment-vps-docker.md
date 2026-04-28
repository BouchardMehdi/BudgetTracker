# Deploiement VPS avec Docker

Cette configuration de production est prevue pour un VPS Linux sur lequel Nginx, Docker, Docker Compose et Certbot sont deja installes.

## Architecture

```text
Navigateur
  -> HTTPS
  -> Nginx sur le VPS
  -> conteneur front React
  -> conteneur back ASP.NET Core API
  -> conteneur PostgreSQL
```

Nginx reste le point d'entree public. Les conteneurs Docker exposent uniquement des ports locaux sur `127.0.0.1`, ce qui evite d'exposer directement l'API ou le frontend sur Internet.

## Structure Docker

```text
BudgetTracker/
├── backend/BudgetTracker.Api/
│   ├── Dockerfile
│   └── appsettings.Production.json
├── frontend/
│   ├── Dockerfile
│   └── nginx.conf
├── docs/nginx/budgettracker.conf.example
├── docker-compose.yml
├── .env.example
└── README.md
```

## Variables d'environnement

Copier le fichier d'exemple :

```bash
cp .env.example .env
```

Puis adapter les valeurs :

```env
FRONTEND_PORT=8087
BACKEND_PORT=5007

POSTGRES_DB=budget_tracker
POSTGRES_USER=budgettracker
POSTGRES_PASSWORD=mot_de_passe_solide

JWT_KEY=secret_long_et_aleatoire_minimum_32_caracteres
CORS_ORIGIN=https://budgettracker.bouchard-mehdi.fr
DATABASE_AUTO_MIGRATE=true
```

En production, les secrets ne doivent pas etre commits dans Git.

## Lancer les conteneurs

```bash
docker compose up -d --build
```

Verifier l'etat :

```bash
docker compose ps
docker compose logs --tail=50
```

## Configuration Nginx

Exemple disponible ici :

```text
docs/nginx/budgettracker.conf.example
```

Copier la configuration sur le VPS :

```bash
sudo cp docs/nginx/budgettracker.conf.example /etc/nginx/sites-available/budgettracker.conf
sudo ln -s /etc/nginx/sites-available/budgettracker.conf /etc/nginx/sites-enabled/budgettracker.conf
```

Tester et recharger Nginx :

```bash
sudo nginx -t
sudo systemctl reload nginx
```

## HTTPS avec Certbot

Une fois le sous-domaine DNS pointe vers l'IP du VPS :

```bash
sudo certbot --nginx -d budgettracker.bouchard-mehdi.fr
```

Certbot genere le certificat SSL, configure HTTPS et peut rediriger HTTP vers HTTPS.

## Mise a jour

```bash
cd /home/projects/BudgetTracker
git pull
docker compose up -d --build
```

`docker compose down` n'est pas necessaire pour une mise a jour classique. Il est surtout utile en cas de changement important de volumes, reseaux, ports ou si un reset complet est souhaite.
