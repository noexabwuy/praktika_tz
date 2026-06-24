# Запуск проекта через Docker

## Требования

Перед запуском должен быть установлен Docker:

* Windows и macOS: Docker Desktop
* Linux: Docker Engine

## Настройка окружения

Создайте файл `.env` рядом с `docker-compose.yml`.

```env
POSTGRES_USER=admin
POSTGRES_PASSWORD=password123
POSTGRES_DB=training_center
JWT_SECRET=SuperSecretKeyForJWTRolesValidation2026_MustBeLong!
JWT_ISSUER=AppealsBackend
JWT_AUDIENCE=AppealsFrontend
```

Файл `.env` не хранится в репозитории. Для примера используется файл `.env.example`.

## Запуск

Соберите и запустите проект:

```bash
docker compose up -d --build
```

После запуска будут доступны следующие сервисы:

| Сервис            | Адрес                         |
| ----------------- | ----------------------------- |
| Frontend          | http://localhost:3000         |
| Backend и Swagger | http://localhost:5071/swagger |
| PostgreSQL        | localhost:5432                |

## Загрузка тестовых данных

После первого запуска база данных будет пустой. Для загрузки тестовых данных выполните команду:

```bash
docker compose exec db psql -U admin -d training_center -f /scripts/seed.sql
```

Если используются другие параметры подключения, замените значения на свои.

Информация о тестовых пользователях и данных находится в файле [test-data.md](test-data.md).

## Полезные команды

Пересборка backend-сервиса:
```bash
docker compose up -d --build backend
```
Остановка контейнеров:
```bash
docker compose stop
```
Удаление контейнеров:
```bash
docker compose down
```
Удаление контейнеров вместе с данными базы данных:
```bash
docker compose down -v
```
Просмотр логов всех сервисов:
```bash
docker compose logs -f
```
Просмотр логов backend-сервиса:

```bash
docker logs training_backend -f
```