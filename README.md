# Sprint-1-WebAPI

Краткий учебный ASP.NET Core Web API для работы с событиями (in-memory хранилище).

## Стек

- .NET 10 (`net10.0`)
- ASP.NET Core Web API
- Swagger (OpenAPI)

## Быстрый запуск

1. Установите .NET SDK 10.
2. Откройте терминал в корне проекта.
3. Выполните команды:

dotnet restore
dotnet run

После запуска API доступен по адресу:

- `http://localhost:5153`

Swagger UI:

- `http://localhost:5153/swagger`

## Модель данных

`Events`:

- `id` (`Guid`) - создается на сервере
- `title` (`string`) - обязательное
- `description` (`string`) - опциональное
- `startAt` (`DateTime`) - обязательное
- `endAt` (`DateTime`) - обязательное

Пример JSON для создания:

```json
{
  "title": "Sprint Planning",
  "description": "Планирование задач",
  "startAt": "2026-04-20T10:00:00",
  "endAt": "2026-04-20T11:00:00"
}
```

## API документация

Базовый маршрут: `/api/event`

### 1) Получить все события

- Метод: `GET`
- URL: `/api/event`
- Ответ `200 OK`: массив событий

Пример:

```bash
curl -X GET "http://localhost:5153/api/event"
```

### 2) Получить событие по индексу

- Метод: `GET`
- URL: `/api/event/{id}`
- Параметр `id`: индекс элемента в списке (0, 1, 2, ...)
- Ответы:
- `200 OK` - событие найдено
- `404 Not Found` - если индекс вне диапазона

Пример:

```bash
curl -X GET "http://localhost:5153/api/event/0"
```

### 3) Создать событие

- Метод: `POST`
- URL: `/api/event`
- Тело: JSON объекта события
- Ответ `201 Created`

Пример:

```bash
curl -X POST "http://localhost:5153/api/event" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Demo",
    "description": "Проверка API",
    "startAt": "2026-04-20T10:00:00",
    "endAt": "2026-04-20T11:00:00"
  }'
```

### 4) Обновить событие по индексу

- Метод: `PUT`
- URL: `/api/event/{id}`
- Параметр `id`: индекс элемента в списке
- Тело: JSON обновленного объекта события
- Ответы:
- `200 OK` - обновлено
- `404 Not Found` - если индекс не существует

Пример:

```bash
curl -X PUT "http://localhost:5153/api/event/0" \
  -H "Content-Type: application/json" \
  -d '{
    "id": "7bcebd22-f7b8-4906-9cf1-44b3b94969e2",
    "title": "Updated Demo",
    "description": "Обновленные данные",
    "startAt": "2026-04-20T12:00:00",
    "endAt": "2026-04-20T13:00:00"
  }'
```

### 5) Удалить событие по индексу

- Метод: `DELETE`
- URL: `/api/event/{id}`
- Параметр `id`: индекс элемента в списке
- Ответы:
- `200 OK` - удалено
- `404 Not Found` - если индекс не существует

Пример:

```bash
curl -X DELETE "http://localhost:5153/api/event/0"
```

## Структура проекта (основное)

- `Program.cs` - настройка DI, контроллеров и Swagger
- `Controllers/EventController.cs` - HTTP-эндпоинты
- `Interfaces/IEventService.cs` - контракт сервиса
- `EventService.cs` - реализация сервиса
- `Event.cs` - модель `Events`