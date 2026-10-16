# 🐳 Docker Architecture & Deployment Plan

> **Project:** TicketWorld Ticketing Platform  
> **Target:** Local Development & Staging Multi-Container Environment  
> **Security Requirement:** Zero hardcoded secrets; all sensitive credentials isolated into `.env` (excluded from git)

---

## 1. Environment Variables Strategy (.env)

All sensitive keys, passwords, and environment-dependent configs are kept in `.env` at project root.

### 1.1 Variable Specification

| Variable Name | Description | Default / Example Value |
| :--- | :--- | :--- |
| `POSTGRES_DB` | Database name | `ticketworld_db` |
| `POSTGRES_USER` | PostgreSQL superuser | `ticketworld_admin` |
| `POSTGRES_PASSWORD` | PostgreSQL password | `StrongSecretPostgresPwd2026!` |
| `POSTGRES_PORT` | PostgreSQL host binding port | `5432` |
| `REDIS_PASSWORD` | Redis authentication password | `StrongRedisPassword2026!` |
| `REDIS_PORT` | Redis host binding port | `6379` |
| `MINIO_ROOT_USER` | MinIO admin access key | `minio_admin` |
| `MINIO_ROOT_PASSWORD` | MinIO admin secret key (min 8 chars) | `MinioSecretPass2026!` |
| `MINIO_API_PORT` | S3 API Port | `9000` |
| `MINIO_CONSOLE_PORT` | Web Console Port | `9001` |
| `SEQ_ADMIN_PASSWORD` | Seq log viewer initial admin password | `SeqAdminPassword2026!` |
| `SEQ_INGESTION_PORT` | Port for Serilog ingestion | `5341` |
| `SEQ_UI_PORT` | Web UI Port | `8081` |
| `MAILDEV_WEB_PORT` | MailDev web inbox port | `1080` |
| `MAILDEV_SMTP_PORT` | MailDev SMTP server port | `1025` |
| `JWT_SECRET` | Secret key for signing access tokens | `Min64CharactersLongSecretKeyForJwtSigning2026TicketWorldPlatform` |
| `AES_ENCRYPTION_KEY` | 32-byte Base64 key for AES-256-GCM field encryption | `kX82Jv4Q1L7M9zW2Xp3r8t6y1A2b3c4d5e6f7g8h9i0=` |
| `PAYMENT_WEBHOOK_SECRET` | Webhook verification signing secret | `whsec_test_secret_key_ticketworld` |

---

## 2. Service Architecture & Container Topology

```
                  ┌──────────────────────┐
                  │    Docker Host       │
                  │                      │
┌───────────────┐ │  ┌────────────────┐  │
│  Client / Dev │─┼─>│ Frontend (80)  │  │
└───────────────┘ │  └───────┬────────┘  │
                  │          │           │
                  │  ┌───────▼────────┐  │
                  │  │ Backend API    │  │
                  │  │ (.NET 8 - 5000)│  │
                  │  └───┬───┬───┬───┬┘  │
                  │      │   │   │   │   │
                  │  ┌───▼┐ ┌▼──┐│  ┌▼──┐│
                  │  │PG16│ │Red││  │Min││
                  │  └────┘ └───┘│  └─IO┘│
                  │            ┌─▼──┐    │
                  │            │Seq │    │
                  │            └────┘    │
                  │  Internal Network    │
                  └──────────────────────┘
```

### Services Defined:
1. **`postgres` (PostgreSQL 16 Alpine):** Primary relational data store with persistent volume.
2. **`redis` (Redis 7 Alpine):** In-memory cache, session store, distributed locks, rate-limiting counters.
3. **`minio` (MinIO S3 Compatible):** Object storage for event banners, venue seat maps, and user avatars.
4. **`seq` (Datalust Seq):** Centralized structured log ingestion for Serilog.
5. **`maildev` (MailDev):** SMTP dev server for testing transactional ticket emails without sending live mail.
6. **`backend` (.NET 8 WebAPI - Optional Dev Container):** Multi-stage build for API services.
7. **`frontend` (Angular 18 - Optional Dev Container):** Nginx-served single page application.

---

## 3. Docker Compose Implementation Specification

The root `docker-compose.yml` mounts variables dynamically from `${VARIABLE_NAME}` without exposing any secret:

```yaml
version: '3.8'

networks:
  ticketworld-network:
    driver: bridge

volumes:
  postgres_data:
  redis_data:
  minio_data:
  seq_data:

services:
  # 1. Primary Database
  postgres:
    image: postgres:16-alpine
    container_name: ticketworld-postgres
    restart: unless-stopped
    environment:
      POSTGRES_DB: ${POSTGRES_DB:-ticketworld_db}
      POSTGRES_USER: ${POSTGRES_USER:-postgres}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
    ports:
      - "${POSTGRES_PORT:-5432}:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    networks:
      - ticketworld-network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER:-postgres} -d ${POSTGRES_DB:-ticketworld_db}"]
      interval: 10s
      timeout: 5s
      retries: 5

  # 2. Redis Cache & Locking
  redis:
    image: redis:7-alpine
    container_name: ticketworld-redis
    restart: unless-stopped
    command: >
      sh -c 'redis-server --requirepass "${REDIS_PASSWORD}" --appendonly yes'
    ports:
      - "${REDIS_PORT:-6379}:6379"
    volumes:
      - redis_data:/data
    networks:
      - ticketworld-network
    healthcheck:
      test: ["CMD", "redis-cli", "-a", "${REDIS_PASSWORD}", "ping"]
      interval: 10s
      timeout: 3s
      retries: 5

  # 3. MinIO Object Storage
  minio:
    image: minio/minio:latest
    container_name: ticketworld-minio
    restart: unless-stopped
    command: server /data --console-address ":9001"
    environment:
      MINIO_ROOT_USER: ${MINIO_ROOT_USER}
      MINIO_ROOT_PASSWORD: ${MINIO_ROOT_PASSWORD}
    ports:
      - "${MINIO_API_PORT:-9000}:9000"
      - "${MINIO_CONSOLE_PORT:-9001}:9001"
    volumes:
      - minio_data:/data
    networks:
      - ticketworld-network
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:9000/minio/health/live"]
      interval: 15s
      timeout: 5s
      retries: 3

  # 4. Seq Structured Logging
  seq:
    image: datalust/seq:latest
    container_name: ticketworld-seq
    restart: unless-stopped
    environment:
      ACCEPT_EULA: "Y"
      SEQ_INITIAL_PASSWORD: ${SEQ_ADMIN_PASSWORD}
    ports:
      - "${SEQ_INGESTION_PORT:-5341}:5341"
      - "${SEQ_UI_PORT:-8081}:80"
    volumes:
      - seq_data:/data
    networks:
      - ticketworld-network

  # 5. MailDev Mock SMTP
  maildev:
    image: maildev/maildev:2.1.0
    container_name: ticketworld-maildev
    restart: unless-stopped
    ports:
      - "${MAILDEV_WEB_PORT:-1080}:1080"
      - "${MAILDEV_SMTP_PORT:-1025}:1025"
    networks:
      - ticketworld-network
```

---

## 4. Execution Workflow

### Step 1: Initialize Environment File
Copy template to `.env` and fill real secrets:
```bash
cp .env.example .env
```

### Step 2: Ensure Git Exclusion
Verify `.gitignore` contains:
```gitignore
.env
.env.*
!.env.example
```

### Step 3: Launch Services
Start all auxiliary services in background:
```bash
docker compose up -d
```

### Step 4: Verification Commands
Check status and healthcheck results:
```bash
# Verify all containers are healthy
docker compose ps

# Inspect logs
docker compose logs -f postgres
docker compose logs -f redis
```

### Step 5: Tear Down / Volume Cleanup
```bash
# Stop containers keeping data volumes
docker compose down

# Stop containers and erase data volumes
docker compose down -v
```
