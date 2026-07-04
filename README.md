# Training Center

Система для приёма, обработки и анализа заявок на обучение.

Дата защиты проекта: 26 июня 2026 года.

## Возможности

Система позволяет:

* регистрировать пользователей и выполнять вход по ролям
* создавать и обрабатывать заявки на обучение
* назначать ответственных сотрудников
* изменять статусы заявок
* оставлять комментарии
* вести историю изменений
* просматривать статистику и аналитику
* управлять справочниками направлений и форматов обучения

## Технологии

### Backend

* ASP.NET Core 8
* Entity Framework Core
* PostgreSQL
* JWT
* Serilog

### Frontend

* React
* TypeScript
* Vite
* Tailwind CSS

### Инфраструктура

* Docker Compose
* GitHub Actions
* Swagger

## Быстрый запуск

Создайте файл `.env` на основе шаблона:

```bash
cp .env.example .env
```

Запустите проект:

```bash
docker compose up -d --build
```

После запуска будут доступны:

| Сервис            | Адрес                         |
| ----------------- | ----------------------------- |
| Frontend          | http://localhost:3000         |
| Backend и Swagger | http://localhost:5071/swagger |
| PostgreSQL        | localhost:5432                |

Для загрузки тестовых данных выполните:

```bash
docker compose exec db psql -U admin -d training_center -f /scripts/seed.sql
```

Подробную инструкцию по запуску можно узнать в [docs/docker-guide.md](docs/docker-guide.md).

## Документация

| Документ | Описание |
| -------- | -------- |
| [specification.md](docs/specification.md) | Техническое задание |
| [architecture.md](docs/architecture.md) | Архитектура системы |
| [database.md](docs/database.md) | Схема базы данных |
| [docker-guide.md](docs/docker-guide.md) | Запуск через Docker |
| [test-data.md](docs/test-data.md) | Тестовые пользователи и данные |
| [implemented-features.md](docs/implemented-features.md) | Реализованные функции |
| [known-limitations.md](docs/known-limitations.md) | Известные ограничения |

## Структура проекта

```text
.
├── src/
│   ├── backend/
│   └── frontend/
├── tests/
├── docs/
├── scripts/
├── .github/
├── docker-compose.yml
└── .env.example
```

## Роли пользователей

Подробное описание прав доступа — [architecture.md](docs/architecture.md#роли-пользователей).

### Applicant

Создание заявок, просмотр своих заявок и работа с комментариями.

### Manager

Просмотр всех заявок, назначение ответственных сотрудников и изменение статусов.

### Admin

Управление справочниками, пользователями и просмотр статистики.

### Director

Просмотр аналитики, пользователей и заявок.

## Статусы заявок

Подробнее — [architecture.md](docs/architecture.md#статусы-заявок).

```text
New → InProgress → NeedsInfo
                    ├─ Approved
                    ├─ Rejected
                    └─ Completed
```

## Тестирование

Для серверной части реализованы модульные тесты на базе:

* xUnit
* FluentAssertions
* Moq

## Разработка

Для работы используются отдельные ветки задач с последующим созданием Pull Request.

Формат сообщений коммитов соответствует Conventional Commits:

```text
feat: новая функция
fix: исправление ошибки
docs: изменение документации
refactor: изменение структуры кода без изменения логики
test: добавление или изменение тестов
chore: технические изменения
```