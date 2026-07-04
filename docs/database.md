# Схема базы данных

## Стек

* PostgreSQL 15
* Entity Framework Core
* Code First миграции

Подробное описание системы находится в [architecture.md](architecture.md).

## Диаграмма

```mermaid
erDiagram
    Users {
        uuid Id PK
        string FullName
        string Login
        string Email
        string PasswordHash
        string Role
        timestamp CreatedAt
    }

    Directions {
        uuid Id PK
        string Name
    }

    TrainingFormats {
        uuid Id PK
        string Name
    }

    Applications {
        uuid Id PK
        string Title
        string Description
        string Status
        uuid DirectionId FK
        uuid FormatId FK
        uuid AuthorId FK
        uuid AssignedToId FK
        timestamp CreatedAt
        timestamp UpdatedAt
    }

    Comments {
        uuid Id PK
        uuid ApplicationId FK
        uuid AuthorId FK
        string Text
        timestamp CreatedAt
    }

    StatusHistory {
        uuid Id PK
        uuid ApplicationId FK
        uuid ChangedById FK
        string OldStatus
        string NewStatus
        timestamp ChangedAt
    }

    AuditLog {
        uuid Id PK
        uuid UserId FK
        string Action
        string Entity
        uuid EntityId
        timestamp OccurredAt
    }

    Users ||--o{ Applications : author
    Users ||--o{ Applications : assignedTo
    Users ||--o{ Comments : writes
    Users ||--o{ StatusHistory : changedBy
    Users ||--o{ AuditLog : performs

    Directions ||--o{ Applications : categorizes
    TrainingFormats ||--o{ Applications : defines

    Applications ||--o{ Comments : has
    Applications ||--o{ StatusHistory : tracks
```

## Таблицы

| Таблица         | Назначение                    |
| --------------- | ----------------------------- |
| Users           | Пользователи системы          |
| Applications    | Заявки на обучение            |
| Directions      | Направления обучения          |
| TrainingFormats | Форматы обучения              |
| Comments        | Комментарии к заявкам         |
| StatusHistory   | История изменения статусов    |
| AuditLog        | Журнал действий пользователей |

## Users

| Поле         | Описание                   |
| ------------ | -------------------------- |
| Id           | Идентификатор пользователя |
| FullName     | ФИО                        |
| Login        | Логин                      |
| Email        | Email                      |
| PasswordHash | Хэш пароля                 |
| Role         | Роль пользователя          |
| CreatedAt    | Дата создания              |

## Applications

| Поле         | Описание                |
| ------------ | ----------------------- |
| Id           | Идентификатор заявки    |
| Title        | Название заявки         |
| Description  | Описание                |
| Status       | Текущий статус          |
| DirectionId  | Направление обучения    |
| FormatId     | Формат обучения         |
| AuthorId     | Автор заявки            |
| AssignedToId | Ответственный сотрудник |
| CreatedAt    | Дата создания           |
| UpdatedAt    | Дата изменения          |

## Directions

| Поле | Описание             |
| ---- | -------------------- |
| Id   | Идентификатор        |
| Name | Название направления |

## TrainingFormats

| Поле | Описание         |
| ---- | ---------------- |
| Id   | Идентификатор    |
| Name | Название формата |

## Comments

| Поле          | Описание                  |
| ------------- | ------------------------- |
| Id            | Идентификатор комментария |
| ApplicationId | Заявка                    |
| AuthorId      | Автор комментария         |
| Text          | Текст комментария         |
| CreatedAt     | Дата создания             |

## StatusHistory

| Поле          | Описание                        |
| ------------- | ------------------------------- |
| Id            | Идентификатор записи            |
| ApplicationId | Заявка                          |
| ChangedById   | Пользователь, изменивший статус |
| OldStatus     | Предыдущий статус               |
| NewStatus     | Новый статус                    |
| ChangedAt     | Дата изменения                  |

## AuditLog

| Поле       | Описание               |
| ---------- | ---------------------- |
| Id         | Идентификатор записи   |
| UserId     | Пользователь           |
| Action     | Выполненное действие   |
| Entity     | Тип сущности           |
| EntityId   | Идентификатор сущности |
| OccurredAt | Дата и время действия  |

## Особенности

* Все первичные ключи используют UUID.
* Для пользователей действуют уникальные ограничения на логин и email.
* История статусов хранится отдельно от основной записи заявки.
* Действия пользователей сохраняются в журнал аудита.
