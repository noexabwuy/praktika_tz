# praktika_tz

## Технологический стек
- **Backend:** C# / ASP.NET Core 8.0
- **ORM:** Entity Framework Core
- **Database:** PostgreSQL (в Docker)
- **API Docs:** Swagger / OpenAPI
- **Frontend:** React 18 + Vite + TypeScript
- **Design:** Figma


## Структура проекта
- `/src/backend` - Исходный код API
- `/src/frontend` - Исходный код клиентской части
- `/docs` - ТЗ, схемы БД и дополнительная документация
- `docker-compose.yml` - Конфигурация инфраструктуры


## Регламент разработки

### 1. Гит-флоу и ветки
- Работа ведется по модели **Feature Branches**.
- Ветка `main` - защищена. Прямые коммиты запрещены.
- Типы веток:
    - `feat/task-name` - новая функциональность.
    - `fix/bug-name` - исправление ошибок.
    - `docs/task-name` - работа с документацией.
    - `refactor/task-name` - оптимизация кода.
    - `chore/task-name` - рутинные задачи

### 2. Правила коммитов (Conventional Commits)
Придерживаемся стандарта [Conventional Commits](https://www.conventionalcommits.org/ru/v1.0.0/).
Формат: `тип(область): описание`
- `feat`: добавление новой функциональности
- `fix`: исправление ошибки
- `docs`: изменения в документации
- `refactor`: правки кода без изменения бизнес-логики
- `chore`: обновление зависимостей, конфигов и прочее

### 3. Работа с Pull Requests
- Любое изменение вносится только через **Pull Request**.
- **Срок ревью:** не более 4 рабочих часов с момента открытия.
- **Слияние:** требуется как минимум 1 подтверждение от участника команды.