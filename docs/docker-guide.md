# Локальный запуск через Docker

## 1. Требования

- **Windows/Mac** - Docker Desktop
- **Linux** - Docker Engine

---

## 2. Переменные окружения

Создайте `.env` в корне проекта рядом с `docker-compose.yml`:

```env
POSTGRES_USER=postgres
POSTGRES_PASSWORD=your_password
POSTGRES_DB=learning_center
JWT_SECRET=your_secret_min_32_chars
JWT_ISSUER=AppealsBackend
JWT_AUDIENCE=AppealsFrontend
```

> `.env` не коммитится в репо, только `.env.example`.

---

## 3. Запуск

```bash
docker compose up -d --build
```

---

## 4. Сервисы

| Сервис | URL |
|---|---|
| Frontend | http://localhost:3000 |
| Backend (Swagger) | http://localhost:5071/swagger |
| PostgreSQL | localhost:5432 |

---

## 5. Загрузка тестовых данных

БД при первом запуске пустая. Накатить seed:

**Linux / macOS / Git Bash:**
```bash
docker compose exec db psql -U ${POSTGRES_USER} -d ${POSTGRES_DB} -f /scripts/seed.sql
```

**PowerShell:**
```powershell
docker compose exec db psql -U $env:POSTGRES_USER -d $env:POSTGRES_DB -f /scripts/seed.sql
```

**Если переменные не читаются — подставьте значения из `.env` напрямую:**
```bash
docker compose exec db psql -U postgres -d learning_center -f /scripts/seed.sql
```

Тестовые пользователи и состав данных — в [docs/seed-data.md](seed-data.md).

---

## 6. Полезные команды

```bash
# Пересборка конкретного сервиса
docker compose up -d --build backend

# Остановка (данные БД сохраняются)
docker compose stop

# Полная остановка и удаление контейнеров
docker compose down

# Удалить контейнеры вместе с данными БД
docker compose down -v

# Логи всех сервисов
docker compose logs -f

# Логи конкретного сервиса
docker logs training_backend -f
```