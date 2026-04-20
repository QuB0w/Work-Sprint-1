# Sprint-1-WebAPI

Учебный ASP.NET Core Web API для работы с событиями.

## Что реализовано

- CRUD для сущности Event
- In-memory хранилище (без БД)
- Swagger UI для тестирования эндпоинтов
- Валидация входных данных для создания и обновления

## Технологии

- .NET 10
- ASP.NET Core Web API
- Swagger / OpenAPI

## Запуск проекта

1. Перейдите в корень проекта.
2. Выполните:

```powershell
dotnet restore
dotnet run
```

3. Откройте Swagger:

- http://localhost:5153/swagger

## API

Базовый маршрут: `/events`

### GET /events

Возвращает список всех событий.

- Ответ: `200 OK`

Пример:

```bash
curl -X GET "http://localhost:5153/events"
```

### GET /events/{id}

Возвращает событие по `Guid`.

- Ответы:
- `200 OK` - событие найдено
- `404 Not Found` - событие не найдено

Пример:

```bash
curl -X GET "http://localhost:5153/events/8b5d5b0a-06e7-4d65-ae24-2b4ad617f4ce"
```

### POST /events

Создает новое событие.

Тело запроса:

```json
{
  "title": "Sprint Planning",
  "description": "Планирование задач",
  "startAt": "2026-04-20T10:00:00",
  "endAt": "2026-04-20T11:00:00"
}
```

Правила валидации:

- `title` обязателен
- `startAt` должен быть раньше `endAt`

Ответы:

- `201 Created` - событие создано
- `400 Bad Request` - невалидные данные

Пример:

```bash
curl -X POST "http://localhost:5153/events" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Sprint Planning",
    "description": "Планирование задач",
    "startAt": "2026-04-20T10:00:00",
    "endAt": "2026-04-20T11:00:00"
  }'
```

### PUT /events/{id}

Обновляет существующее событие по `Guid`.

Тело запроса:

```json
{
  "title": "Sprint Planning Updated",
  "description": "Обновленное описание",
  "startAt": "2026-04-20T12:00:00",
  "endAt": "2026-04-20T13:00:00"
}
```

Ответы:

- `200 OK` - событие обновлено
- `400 Bad Request` - невалидные данные
- `404 Not Found` - событие не найдено

Пример:

```bash
curl -X PUT "http://localhost:5153/events/8b5d5b0a-06e7-4d65-ae24-2b4ad617f4ce" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Sprint Planning Updated",
    "description": "Обновленное описание",
    "startAt": "2026-04-20T12:00:00",
    "endAt": "2026-04-20T13:00:00"
  }'
```

### DELETE /events/{id}

Удаляет событие по `Guid`.

Ответы:

- `204 No Content` - удалено
- `404 Not Found` - событие не найдено

Пример:

```bash
curl -X DELETE "http://localhost:5153/events/8b5d5b0a-06e7-4d65-ae24-2b4ad617f4ce"
```

## Ограничения

- Данные хранятся в памяти процесса и теряются после перезапуска.
