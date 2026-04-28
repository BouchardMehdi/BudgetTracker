# Deploiement de mes projets fullstack avec Docker sur VPS

Pour deployer mes projets fullstack, j'utilise un VPS Linux sur lequel sont installes Nginx, Docker, Docker Compose et Certbot. Mon domaine principal reste gere chez Hostinger, puis chaque projet est publie sur un sous-domaine dedie, par exemple `scratchwin.bouchard-mehdi.fr`, `taskflow.bouchard-mehdi.fr` ou `experiences.bouchard-mehdi.fr`.

## Architecture generale

L'architecture suit toujours le meme principe :

```text
Navigateur
  -> HTTPS
  -> Nginx sur le VPS
  -> conteneur frontend
  -> conteneur backend / API
  -> base de donnees
```

Nginx est le seul service expose publiquement. Les applications tournent dans des conteneurs Docker et sont accessibles uniquement localement depuis le VPS, par exemple sur `127.0.0.1:808X` pour le frontend et `127.0.0.1:300X` ou `127.0.0.1:500X` pour l'API.

## Pourquoi Docker

J'utilise Docker pour isoler chaque projet et rendre les deploiements reproductibles. Chaque application embarque ses propres dependances, son runtime et sa configuration, ce qui evite les conflits de versions entre plusieurs projets.

Cette approche me permet par exemple de faire cohabiter sur le meme VPS :

- ScratchWin : frontend, backend Node.js et base de donnees.
- TaskFlow : frontend, backend Node.js et base de donnees.
- MyExperiences : frontend SvelteKit, backend Symfony et PostgreSQL.
- BudgetTracker : frontend React, backend ASP.NET Core et PostgreSQL.

Docker Compose me permet de decrire tous les services d'un projet dans un seul fichier : frontend, backend, base de donnees, variables d'environnement, volumes et ports locaux. Le projet peut ensuite etre lance ou mis a jour avec une commande simple.

## Role de Nginx

Nginx sert de reverse proxy. Il recoit les requetes HTTP/HTTPS, identifie le sous-domaine demande, puis redirige la requete vers le bon conteneur Docker.

En pratique :

- `/api` est redirige vers le backend.
- `/` est redirige vers le frontend.
- chaque sous-domaine possede sa propre configuration Nginx.
- les conteneurs restent accessibles uniquement en local sur le VPS.

Exemple simplifie :

```nginx
server {
    listen 80;
    server_name projet.bouchard-mehdi.fr;

    location /api/ {
        proxy_pass http://127.0.0.1:300X/api/;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    location / {
        proxy_pass http://127.0.0.1:808X;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

## Role de Certbot

Certbot me permet de generer gratuitement un certificat SSL Let's Encrypt pour chaque sous-domaine. Il configure HTTPS dans Nginx et peut activer automatiquement la redirection HTTP vers HTTPS.

Commande type :

```bash
sudo certbot --nginx -d projet.bouchard-mehdi.fr
```

## Structure type d'un projet

```text
mon-projet/
├── front/
├── back/
├── docker-compose.yml
├── .env.example
└── README.md
```

Exemple generique de `docker-compose.yml` :

```yaml
services:
  front:
    build: ./front
    ports:
      - "127.0.0.1:808X:80"
    restart: unless-stopped

  back:
    build: ./back
    ports:
      - "127.0.0.1:300X:3000"
    env_file:
      - ./back/.env
    extra_hosts:
      - "host.docker.internal:host-gateway"
    restart: unless-stopped
```

Selon le projet, j'ajoute aussi un service de base de donnees, par exemple PostgreSQL ou MySQL, avec un volume Docker pour conserver les donnees.

## Etapes de deploiement

1. Creer un sous-domaine DNS chez Hostinger et le faire pointer vers l'IP du VPS.
2. Cloner le projet dans `/home/projects`.
3. Creer les fichiers `.env` a partir des `.env.example`.
4. Construire et lancer les conteneurs :

```bash
docker compose up -d --build
```

5. Verifier l'etat des services :

```bash
docker compose ps
docker compose logs --tail=50
```

6. Creer la configuration Nginx du sous-domaine.
7. Tester puis recharger Nginx :

```bash
sudo nginx -t
sudo systemctl reload nginx
```

8. Activer HTTPS avec Certbot :

```bash
sudo certbot --nginx -d projet.bouchard-mehdi.fr
```

## Mise a jour d'un projet

Pour mettre a jour un projet deja deploye :

```bash
cd /home/projects/mon-projet
git pull
docker compose up -d --build
```

Je n'utilise pas `docker compose down` pour une mise a jour classique, car Compose sait reconstruire les images et remplacer les conteneurs concernes. Je reserve `docker compose down` aux cas plus importants : changement de volumes, reseaux, ports ou reset complet d'un environnement.

Cette organisation me permet de deployer plusieurs projets fullstack sur le meme VPS tout en gardant une architecture claire, isolee et maintenable.
