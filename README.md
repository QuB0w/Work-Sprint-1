# API Управления Мероприятиями

Простой ASP.NET Core Web API для управления мероприятиями с расширенной фильтрацией, пагинацией и централизованной обработкой ошибок.

## Возможности

- **CRUD операции с мероприятиями**: Создание, чтение, обновление и удаление мероприятий
- **Бронирование мероприятий**: Асинхронное создание броней с фоновой обработкой статуса
- **Расширенная фильтрация**: Фильтрация мероприятий по названию, диапазону дат или комбинированным критериям
- **Пагинация**: Поддержка постраничной выдачи с настраиваемым размером страницы
- **Глобальная обработка ошибок**: Централизованная обработка исключений с единым форматом JSON-ответов
- **Юнит-тестирование**: Комплексное покрытие тестами с использованием xUnit
- **Интеграционное тестирование**: Проверка слоя данных на реальной PostgreSQL через Testcontainers
- **Репозиторный слой**: Работа с `DbContext` инкапсулирована в `IEventRepository` и `IBookingRepository`
- **Swagger документация**: Интерактивная документация API

## Используемые технологии

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core 9
- PostgreSQL через Npgsql.EntityFrameworkCore.PostgreSQL
- xUnit и EF Core InMemory для юнит-тестирования
- Интеграционные тесты на реальной PostgreSQL через Testcontainers
- Swagger/OpenAPI для документации

## Начало работы

### Предварительные требования

- .NET 10 SDK
- Docker Desktop (для PostgreSQL)
- Visual Studio 2022 или VS Code

### Настройка PostgreSQL

1. Запустите PostgreSQL из корня репозитория:
   ```bash
   docker compose up -d
   ```
2. По умолчанию приложение использует строку подключения из `Sprint-1-WebAPI/appsettings.json`:
   ```json
   "DefaultConnection": "Host=localhost;Port=5432;Database=eventapi;Username=postgres;Password=postgres"
   ```
3. Схема базы данных управляется миграциями EF Core. При старте приложение применяет ожидающие миграции через `Database.Migrate()`.

### Миграции EF Core

Миграции находятся в проекте `EventApi.Infrastructure`. Для генерации новых миграций укажите проект с `DbContext` через параметр `--project` и стартовый проект через `--startup-project`:

```bash
dotnet ef migrations add <MigrationName> \
  --project EventApi.Infrastructure \
  --startup-project Sprint-1-WebAPI
```

Чтобы применить миграции к базе данных:

```bash
dotnet ef database update \
  --project EventApi.Infrastructure \
  --startup-project Sprint-1-WebAPI
```

При запуске приложения миграции применяются автоматически через `Database.Migrate()`.

### Запуск приложения

1. Клонируйте репозиторий
2. Перейдите в директорию проекта:
   ```bash
   cd Sprint-1-WebAPI
   ```
3. Запустите приложение:
   ```bash
   dotnet run
   ```

API будет доступен по адресу `https://localhost:7xxx`, а Swagger UI по адресу `https://localhost:7xxx/swagger`.

### Запуск тестов

Запустите все тесты с помощью следующей команды:
```bash
dotnet test
```

Юнит-тесты:
```bash
dotnet test EventService.Tests/EventService.Tests.csproj
```

Интеграционные тесты поднимают реальный контейнер PostgreSQL через Testcontainers, поэтому для их запуска необходим запущенный Docker:
```bash
dotnet test EventApi.IntegrationTests/EventApi.IntegrationTests.csproj
```

## Эндпоинты API

### Мероприятия

| Метод | Эндпоинт | Описание |
|--------|----------|-------------|
| GET | `/events` | Получение всех мероприятий с опциональной фильтрацией и пагинацией |
| GET | `/events/{id}` | Получение мероприятия по ID |
| POST | `/events` | Создание нового мероприятия |
| PUT | `/events/{id}` | Обновление существующего мероприятия |
| DELETE | `/events/{id}` | Удаление мероприятия |
| POST | `/events/{id}/book` | Создание брони на мероприятие (202 Accepted) |
| GET | `/bookings/{id}` | Получение текущего статуса брони |

### Параметры GET /events

Эндпоинт GET `/events` поддерживает следующие параметры запроса:

#### Параметры фильтрации

- `title` (string, опционально): Фильтрация мероприятий по названию (case-insensitive, частичное совпадение)
- `from` (DateTime, опционально): Фильтрация мероприятий начиная с этой даты
- `to` (DateTime, опционально): Фильтрация мероприятий заканчивающиеся до этой даты

#### Параметры пагинации

- `page` (int, опционально, по умолчанию: 1): Номер страницы для получения
- `pageSize` (int, опционально, по умолчанию: 10, максимум: 100): Количество элементов на странице

#### Примеры

```bash
# Получить все мероприятия
GET /events

# Фильтрация по названию
GET /events?title=конференция

# Фильтрация по диапазону дат
GET /events?from=2024-01-01T00:00:00Z&to=2024-12-31T23:59:59Z

# Получить страницу 2 с 5 элементами на странице
GET /events?page=2&pageSize=5

# Комбинированная фильтрация и пагинация
GET /events?title=технологии&from=2024-01-01&page=1&pageSize=10
```

### Формат ответов

#### Успешный ответ

Для отфильтрованных/пагинированных запросов формат ответа:

```json
{
  "items": [
    {
      "id": "guid",
      "title": "Название мероприятия",
      "description": "Описание мероприятия",
      "startAt": "2024-01-01T10:00:00Z",
      "endAt": "2024-01-01T12:00:00Z"
    }
  ],
  "totalCount": 25,
  "page": 1,
  "pageSize": 10,
  "totalPages": 3,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

#### Ответ с ошибкой

Все ошибки возвращаются в едином формате JSON:

```json
{
  "statusCode": 400,
  "message": "Описание ошибки"
}
```

Распространенные HTTP статусы:
- `400 Bad Request`: Ошибки валидации или неверные параметры
- `404 Not Found`: Ресурс не найден
- `500 Internal Server Error`: Непредвиденные ошибки сервера

## Модель мероприятия

```json
{
  "id": "guid",
  "title": "string (обязательно)",
  "description": "string (опционально)",
  "startAt": "DateTime (обязательно)",
  "endAt": "DateTime (обязательно)"
}
```

## Модель бронирования

```json
{
  "id": "guid",
  "eventId": "guid",
  "status": "Pending | Confirmed | Rejected",
  "createdAt": "DateTime",
  "processedAt": "DateTime?"
}
```

### Статусы бронирования

- `Pending` — бронь создана, ожидает обработки
- `Confirmed` — бронь подтверждена
- `Rejected` — бронь отклонена

### Создание брони

Для создания брони отправьте запрос:

```bash
POST /events/{id}/book
```

Эндпоинт возвращает `202 Accepted` с заголовком `Location: /bookings/{bookingId}` и телом ответа:

```json
{
  "id": "guid",
  "eventId": "guid",
  "status": "Pending",
  "createdAt": "2026-06-25T10:00:00Z"
}
```

Если мероприятие не найдено, возвращается `404 Not Found`.

### Фоновая обработка броней

Обработка броней выполняется фоновым сервисом на базе `BackgroundService`:

- Сервис каждые 5 секунд проверяет хранилище на наличие броней в статусе `Pending`
- Для каждой необработанной брони выполняется искусственная задержка 2 секунды, имитирующая вызов внешней системы
- После задержки бронь переводится в статус `Confirmed`, а поле `ProcessedAt` заполняется текущей датой
- В дальнейшем спринте будет добавлена логика выбора между `Confirmed` и `Rejected`

### Правила валидации

- `title` не должен быть пустым
- `startAt` должен быть раньше `endAt` (проверяется на уровне контроллера)
- `page` должен быть больше 0
- `pageSize` должен быть между 1 и 100

## Тестирование

Проект включает комплексные юнит-тесты, покрывающие:

### Успешные сценарии
- Создание мероприятий
- Получение всех мероприятий
- Получение мероприятия по ID
- Обновление существующих мероприятий
- Удаление мероприятий
- Фильтрацию по названию
- Фильтрацию по диапазону дат
- Пагинацию
- Комбинированную фильтрацию
- Создание брони для существующего мероприятия
- Создание нескольких броней с уникальными идентификаторами
- Получение брони по ID
- Отражение изменения статуса брони после подтверждения/отклонения
- Создание брони в статусе `Pending`
- Подтверждение и отклонение брони с заполнением `ProcessedAt`
- Уникальность идентификаторов броней
- Хранение и обновление броней в `InMemoryBookingStore`

### Неуспешные сценарии
- Получение несуществующих мероприятий
- Обновление несуществующих мероприятий
- Неверные параметры пагинации
- Ошибки валидации дат
- Создание брони для несуществующего или удалённого мероприятия
- Получение брони по несуществующему ID
- Получение брони по несуществующему ID из хранилища

### Структура тестов

Тесты расположены в проекте `EventService.Tests` и используют:
- xUnit как тестовый фреймворк
- Паттерн Arrange-Act-Assert
- Комплексное покрытие бизнес-логики

## Обработка ошибок

Приложение реализует глобальную обработку исключений через middleware, которая:
- Перехватывает все необработанные исключения
- Возвращает согласованные JSON-ответы с соответствующими HTTP статусами
- Логирует ошибки для отладки
- Сопоставляет разные типы исключений с соответствующими HTTP статусами

## Разработка

### Структура проекта (Clean Architecture)

Проект разделён на четыре слоя в соответствии с принципами чистой архитектуры:

```
EventApi.Domain/              ← Доменный слой (не зависит ни от чего)
├── Entities/
│   ├── Event.cs
│   └── Booking.cs
├── Enums/
│   └── BookingStatus.cs
├── Exceptions/
│   └── NoAvailableSeatsException.cs
└── Models/
    └── PaginatedResult.cs

EventApi.Application/         ← Слой приложения (зависит только от Domain)
├── DTOs/
│   ├── BookingInfo.cs
│   ├── CreateBookingRequest.cs
│   ├── CreateEventRequest.cs
│   └── UpdateEventRequest.cs
├── Interfaces/
│   ├── IBookingRepository.cs
│   ├── IBookingService.cs
│   ├── IEventRepository.cs
│   └── IEventService.cs
├── Services/
│   ├── BookingProcessingBackgroundService.cs
│   ├── BookingService.cs
│   └── EventService.cs
└── DependencyInjection.cs

EventApi.Infrastructure/      ← Инфраструктурный слой (зависит от Application и Domain)
├── Data/
│   ├── AppDbContext.cs
│   └── Configurations/
│       ├── BookingConfiguration.cs
│       └── EventConfiguration.cs
├── Migrations/
│   └── 20260719115609_InitialCreate.cs
├── Repositories/
│   ├── BookingRepository.cs
│   └── EventRepository.cs
└── DependencyInjection.cs

Sprint-1-WebAPI/              ← Presentation (зависит от Application и Infrastructure)
├── Controllers/
│   ├── EventController.cs
│   └── BookingController.cs
├── Middleware/
│   └── GlobalExceptionHandlingMiddleware.cs
└── Program.cs                ← Composition Root

EventService.Tests/           ← Юнит-тесты (ссылается на Application и Infrastructure)
EventApi.IntegrationTests/    ← Интеграционные тесты (Testcontainers + PostgreSQL)
```

#### Направление зависимостей

```
Presentation → Application → Domain
Presentation → Infrastructure → Application → Domain
```

- **Domain** не зависит ни от каких внешних пакетов или слоёв.
- **Application** определяет интерфейсы портов (репозитории) и не зависит от Infrastructure.
- **Infrastructure** реализует порты и содержит все инфраструктурные зависимости (EF Core, Npgsql).
- **Presentation** — composition root, регистрирует все зависимости через extension-методы `AddApplicationServices()` и `AddInfrastructureServices()`.

### Добавление новой функциональности

1. Добавьте новые эндпоинты в `EventController.cs`
2. Реализуйте бизнес-логику в `EventService.cs`
3. Обновите интерфейс `IEventService.cs` при необходимости
4. Добавьте соответствующие юнит-тесты
5. Обновите документацию API в комментариях

## Новые возможности в Sprint 2

### ✅ Реализованные функции

**Глобальная обработка ошибок**
- Создан middleware для централизованной обработки исключений
- Единый формат JSON-ответов для всех типов ошибок
- Корректные HTTP статусы (400, 404, 500)
- Логирование ошибок с использованием ILogger

**Расширенная фильтрация**
- Параметры: `title`, `from`, `to` для фильтрации мероприятий
- Case-insensitive поиск по названию с частичным совпадением
- Фильтрация по диапазону дат начала и окончания мероприятий
- Все фильтры работают вместе с использованием LINQ

**Пагинация**
- Параметры: `page` (по умолчанию 1) и `pageSize` (по умолчанию 10, максимум 100)
- DTO `PaginatedResult<T>` с полной информацией о пагинации
- Реализация через LINQ с использованием `Skip` и `Take`

**Юнит-тестирование**
- 14 тестов покрывают всю бизнес-логику мероприятий
- Успешные и неуспешные сценарии
- Тесты изолированы с помощью метода очистки данных
- Все тесты проходят успешно: `✅ 14 пройдено, 0 неудачно`

**Документация**
- Подробный README.md с примерами API
- Обновленный solution файл
- Swagger документация с параметрами фильтрации

## Новые возможности в Sprint 3

### ✅ Реализованные функции

**Сущность бронирования**
- Добавлена модель `Booking` с полями `Id`, `EventId`, `Status`, `CreatedAt`, `ProcessedAt`
- Добавлено перечисление `BookingStatus` со значениями `Pending`, `Confirmed`, `Rejected`
- Добавлены доменные методы: `CreatePending`, `Confirm`, `Reject`
- Данные о бронированиях хранятся в памяти приложения в `InMemoryBookingStore`

**Сервис бронирований**
- Интерфейс `IBookingService` и реализация `BookingService`
- Сервис зарегистрирован в DI-контейнере
- Бизнес-логика изолирована от контроллеров

**Эндпоинты бронирования**
- `POST /events/{id}/book` — создание брони, возвращает `202 Accepted` с заголовком `Location`
- `GET /bookings/{id}` — получение текущего статуса брони
- При запросе несуществующего ресурса возвращается `404 Not Found`

**Фоновая обработка**
- Реализован `BookingProcessingBackgroundService` на базе `BackgroundService`
- Периодический опрос `Pending`-броней с интервалом 5 секунд
- Искусственная задержка 2 секунды имитирует вызов внешней системы
- После обработки бронь переводится в `Confirmed` и заполняется `ProcessedAt`
- Корректная обработка отмены через `CancellationToken`

**Юнит-тестирование**
- Добавлены `BookingServiceTests.cs`, `BookingEntityTests.cs`, `InMemoryBookingStoreTests.cs`
- Покрытие: создание брони, получение по ID, уникальность идентификаторов, изменение статуса, сущность `Booking`, хранилище `InMemoryBookingStore`
- Все тесты проходят успешно: `✅ 32 пройдено, 0 неудачно`

**Документация**
- Swagger корректно отображает новые эндпоинты
- README.md обновлён с описанием модели `Booking`, эндпоинтов и фоновой обработки

### Пример сценария использования

1. Создайте мероприятие:
```bash
POST /events
{
  "title": "Tech Conference 2026",
  "description": "Annual technology conference",
  "startAt": "2026-09-01T10:00:00Z",
  "endAt": "2026-09-01T18:00:00Z"
}
```

2. Создайте бронь, получив `202 Accepted` и `Location` заголовок:
```bash
POST /events/{eventId}/book
```

3. Сразу запросите статус брони — он будет `Pending`:
```bash
GET /bookings/{bookingId}
```

4. Подождите 5–7 секунд и повторите запрос — статус изменится на `Confirmed`:
```bash
GET /bookings/{bookingId}
```

## Новые возможности в Sprint 4

### ✅ Реализованные функции

**Ограничение мест на событие**
- В модель `Event` добавлены поля `TotalSeats` (общее количество мест) и `AvailableSeats` (доступные места)
- `AvailableSeats` при создании устанавливается равным `TotalSeats`
- При создании события `TotalSeats` обязательно и должно быть > 0; нарушение возвращает `400 Bad Request`
- Метод `TryReserveSeats()` — атомарная проверка и уменьшение `AvailableSeats`; возвращает `false` если мест нет
- Метод `ReleaseSeats()` — освобождение мест при отклонении брони

**Защита от овербукинга (lock в BookingService)**
- В `BookingService.CreateBookingAsync` добавлена защита критической секции через `lock (_bookingLock)`
- Критическая секция атомарно включает: получение события → проверку мест (`TryReserveSeats`) → сохранение события → создание брони
- При отсутствии свободных мест выбрасывается `NoAvailableSeatsException`
- `POST /events/{id}/book` возвращает `409 Conflict` при нехватке мест

**Параллельная обработка в BackgroundService**
- `BookingProcessingBackgroundService` теперь обрабатывает все `Pending`-брони параллельно через `Task.WhenAll`
- Искусственная задержка (`ProcessingDelay = 2s`) выполняется параллельно до захвата семафора
- `SemaphoreSlim(1, 1)` защищает запись в хранилище (асинхронный аналог `lock`, необходим для использования с `await`)
- Если событие удалено к моменту обработки — бронь переводится в `Rejected` с логом `Warning`
- При непредвиденной ошибке: бронь отклоняется, место возвращается через `ReleaseSeats()`
- Константы `PollingInterval` и `ProcessingDelay` вынесены в именованные поля

**Примитивы синхронизации**
| Примитив | Где используется | Зачем |
|----------|-----------------|-------|
| `lock` | `BookingService.CreateBookingAsync` | Атомарная пара «проверка мест + создание брони» в синхронном коде |
| `SemaphoreSlim` | `BookingProcessingBackgroundService` | Потокобезопасная запись в хранилище с поддержкой `await` внутри блока |

**Юнит-тестирование**
- Новые тесты на логику мест: уменьшение `AvailableSeats`, лимит броней, `NoAvailableSeatsException`
- Тест на восстановление места после `Reject + ReleaseSeats`
- Тест на конкурентность: 20 параллельных запросов при лимите 5 мест → ровно 5 успешных, 15 `NoAvailableSeatsException`, `AvailableSeats = 0`
- Тест на уникальность Id при 10 конкурентных запросах
- Все тесты используют реальный параллелизм (`Task.Run + Task.WhenAll`)
- **Итого: `✅ 38 пройдено, 0 неудачно`**

### Пример сценария с овербукингом

```bash
# 1. Создайте событие на 3 места
POST /events
{
  "title": "Meetup",
  "startAt": "2026-09-01T18:00:00Z",
  "endAt": "2026-09-01T20:00:00Z",
  "totalSeats": 3
}

# 2. Создайте 3 брони — все получат 202 Accepted
POST /events/{eventId}/book   # 202 Accepted
POST /events/{eventId}/book   # 202 Accepted
POST /events/{eventId}/book   # 202 Accepted

# 3. Четвёртая бронь — 409 Conflict
POST /events/{eventId}/book   # 409 Conflict: No available seats for this event.

# 4. Через 5–7 секунд брони переходят в Confirmed
GET /bookings/{bookingId}     # { "status": "Confirmed", "processedAt": "..." }
```

## Новые возможности в Sprint 5

### ✅ PostgreSQL и Entity Framework Core

- In-memory хранилища заменены на PostgreSQL через Entity Framework Core.
- Добавлен `AppDbContext` с `DbSet<Event>` и `DbSet<Booking>`.
- Fluent API-конфигурации `EventConfiguration` и `BookingConfiguration` задают таблицы, ключи, обязательные поля, ограничения длины, связь один-ко-многим и строковое хранение `BookingStatus`.
- `EventService` и `BookingService` используют scoped `AppDbContext` и сохраняют изменения через `SaveChangesAsync()`.
- Для конкурентного бронирования используется статический `SemaphoreSlim`, поскольку scoped DbContext требует асинхронных вызовов внутри критической секции.
- Фоновый сервис получает DbContext только через `IServiceScopeFactory`: отдельный scope для выборки идентификаторов и отдельный scope на каждую обрабатываемую бронь.
- Тесты используют `Microsoft.EntityFrameworkCore.InMemory`; имя базы создаётся один раз на тестовый класс и используется всеми scope этого класса.

## Новые возможности в Sprint 7

### ✅ Чистая архитектура (Clean Architecture)

Солюшен разделён на четыре отдельных проекта-сборки:

| Проект | Ответственность | Зависит от |
|--------|----------------|------------|
| `EventApi.Domain` | Сущности, перечисления, доменные исключения | — |
| `EventApi.Application` | Use cases (сервисы), интерфейсы портов, DTO, фоновый сервис | Domain |
| `EventApi.Infrastructure` | DbContext, миграции, репозитории, конфигурации EF Core | Application, Domain |
| `Sprint-1-WebAPI` (Presentation) | Контроллеры, middleware, composition root | Application, Infrastructure |

**Ключевые принципы:**
- `Application` **не зависит** от `Infrastructure` — только через интерфейсы портов
- Направление зависимостей строго внутрь: Presentation → Application → Domain
- Composition root находится в `Program.cs` (Presentation)
- Для регистрации зависимостей используются extension-методы: `AddApplicationServices()`, `AddInfrastructureServices()`
- Контроллеры тонкие — не содержат бизнес-логики
- Domain не содержит ссылок на сторонние фреймворки (ASP.NET, EF Core)

**Тестовые проекты** ссылаются на конкретные слои (`Application` + `Infrastructure`), а не на монолитный Presentation-проект.

## Лицензия

Этот проект создан в учебных целях в рамках спринт-задания.
