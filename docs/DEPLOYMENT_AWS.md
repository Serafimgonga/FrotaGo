# Deploy AWS para o FrotaGo

Esta configuração prepara o FrotaGo para um MVP em AWS EC2 com:

- API ASP.NET Core
- SignalR/WebSockets
- PostgreSQL em container para o MVP
- Nginx como proxy reverso
- Backup diário do banco
- Deploy automatizado via GitHub Actions

## Estrutura

- API: [backend/FrotaGo.Backend/src/FrotaGo.Api](../backend/FrotaGo.Backend/src/FrotaGo.Api)
- Docker Compose: [docker-compose.yml](../docker-compose.yml)
- Nginx: [deploy/nginx/default.conf](../deploy/nginx/default.conf)
- Workflow de deploy: [.github/workflows/deploy-ec2.yml](../.github/workflows/deploy-ec2.yml)

## Pré-requisitos

- EC2 Ubuntu com portas 80 e 22 abertas
- Docker e Docker Compose plugin instalados
- Segredos no GitHub:
  - SSH_HOST
  - SSH_PORT
  - SSH_USERNAME
  - SSH_PRIVATE_KEY

## Iniciar localmente

```bash
docker compose up --build -d
```

A API deverá ficar disponível em:

- http://localhost:5073
- http://localhost/ (através do Nginx)

## Endpoints importantes

- Health check: http://localhost/healthz
- SignalR hub: http://localhost/hubs/gps

## Backup do banco

```bash
bash scripts/backup-db.sh
```

Os ficheiros de backup ficam em [backups](../backups).

## Próximo passo para produção

1. Adicionar um Elastic IP à EC2
2. Configurar Route 53 e ACM
3. Trocar o Nginx para HTTPS com certificados TLS
4. Mover a base de dados para Amazon RDS PostgreSQL quando o projeto crescer
