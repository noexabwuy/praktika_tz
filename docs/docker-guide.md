# Локальный запуск проекта через Docker

## 1. Подготовка
Убедитесь, что Docker запущен:
- **Windows/Mac**: Docker Desktop
- **Linux**: Docker Engine

## 2. Переменные окружения
В корне проекта (где `compose.yml`) создайте `.env`:
```env
POSTGRES_USER=admin
POSTGRES_PASSWORD=password123
POSTGRES_DB=training_center
```

## 3. Запуск
```bash
docker compose up -d --build
```

## 4. Доступ к сервисам
| Сервис | URL |
|--------|-----|
| Frontend (React) | http://localhost:3000 |
| Backend API (Swagger) | http://localhost:5071/swagger |
| PostgreSQL | `localhost:5432` (user: `admin`, password: `password123`) |

## 5. Полезные команды
```bash
# Пересборка конкретного сервиса
docker compose up -d --build backend

# Остановка (данные БД сохраняются)
docker compose stop

# Полная остановка и удаление контейнеров
docker compose down

# Просмотр логов контейнера
docker logs training_backend -f