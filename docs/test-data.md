# Тестовые данные

## Загрузка seed-данных

**Linux / macOS / Git Bash:**
```bash
docker compose exec db psql -U ${POSTGRES_USER} -d ${POSTGRES_DB} -f /scripts/seed.sql
```

**PowerShell:**
```powershell
docker compose exec db psql -U $env:POSTGRES_USER -d $env:POSTGRES_DB -f /scripts/seed.sql
```

**Если переменные не читаются, то подставьте значения из `.env` напрямую:**
```bash
docker compose exec db psql -U admin -d training_center -f /scripts/seed.sql
```

Скрипт идемпотентен: повторный запуск не дублирует данные.

---

## Тестовые пользователи

| Логин | Пароль | Роль |
|---|---|---|
| `director` | `director123` | Director |
| `admin` | `admin123` | Admin |
| `ivanov` | `123` | Manager |
| `smirnova` | `123` | Manager |
| `kuznetsov` | `123` | Manager |
| `petrova` | `123` | Applicant |
| `sidorov` | `123` | Applicant |
| `fedorova` | `123` | Applicant |
| `popov` | `123` | Applicant |
| `novikova` | `123` | Applicant |

---

## Вход через Swagger

Откройте `http://localhost:5071/swagger` → `POST /api/auth/login`:

```json
{
  "login": "admin",
  "password": "admin123"
}
```

Скопируйте токен из ответа → кнопка **Authorize** → вставьте как `Bearer <token>`.

---

## Тестовые заявки — 13 записей

| Тема | Статус | Автор |
|---|---|---|
| Курс по C# и .NET | New | petrova |
| DevOps-инжиниринг с нуля | InProgress | sidorov |
| Курс по веб-дизайну и Figma | InProgress | petrova |
| Интенсив по Python для анализа данных | Approved | petrova |
| Курс по управлению проектами (PM) | Completed | sidorov |
| Кибербезопасность: базовый уровень | Approved | petrova |
| Fullstack-разработка на TypeScript | Approved | sidorov |
| Курс 3D-моделирования и анимации | Rejected | sidorov |
| Бизнес-анализ и сбор требований | Completed | petrova |
| Docker и Kubernetes на практике | Completed | sidorov |
| Маркетинг в соцсетях | NeedsInfo | petrova |
| Управление IT-командой | Approved | sidorov |
| Пентест и этичный хакинг | Rejected | sidorov |

> **Примечания:**
> - Статус **NeedsInfo** — у заявки «Маркетинг в соцсетях».
> - Все роли в комментариях: `admin` (Admin) → заявка №11, `director` (Director) → заявка №13.

---

## Сброс данных

**Linux / macOS / Git Bash:**
```bash
docker compose exec db psql -U ${POSTGRES_USER} -d ${POSTGRES_DB} -c "
DELETE FROM \"AuditLogs\";
DELETE FROM \"StatusHistories\";
DELETE FROM \"Comments\";
DELETE FROM \"Applications\";
DELETE FROM \"Users\";
"
```

**PowerShell:**
```powershell
docker compose exec db psql -U $env:POSTGRES_USER -d $env:POSTGRES_DB -c "DELETE FROM `"AuditLogs`"; DELETE FROM `"StatusHistories`"; DELETE FROM `"Comments`"; DELETE FROM `"Applications`"; DELETE FROM `"Users`";"
```

Затем снова накатить seed.