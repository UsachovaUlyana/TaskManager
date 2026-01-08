# Task Manager API

REST API сервис для управления задачами, построенный на .NET 8 с использованием Clean Architecture.

## Оглавление

- [Описание проекта](#описание-проекта)
- [Технологический стек](#технологический-стек)
- [Архитектура проекта](#архитектура-проекта)
- [Модель данных](#модель-данных)
- [Быстрый старт](#быстрый-старт)
- [Запуск после клонирования с GitHub](#запуск-после-клонирования-с-github)
- [API Endpoints](#api-endpoints)
- [Аутентификация и авторизация](#аутентификация-и-авторизация)
- [Кэширование](#кэширование)
- [Мониторинг и метрики](#мониторинг-и-метрики)
- [Тестирование](#тестирование)
- [Конфигурация](#конфигурация)
- [Troubleshooting](#troubleshooting)

---

## Описание проекта

Task Manager API — это полнофункциональный CRUD-сервис для управления задачами с поддержкой:

- **Управления пользователями** с ролевой моделью (Admin/User)
- **Проектов** с возможностью добавления участников
- **Задач** с приоритетами, статусами и сроками выполнения
- **Тегов** для категоризации задач
- **Связи many-to-many** между задачами и тегами, пользователями и проектами

---

## Технологический стек

| Компонент | Технология | Версия |
|-----------|------------|--------|
| Фреймворк | .NET | 8.0 |
| База данных | PostgreSQL | 16 |
| Кэширование | Redis | 7 |
| ORM | Entity Framework Core | 8.0 |
| Микро-ORM | Dapper | 2.1 |
| Миграции БД | Liquibase | 4.25 |
| Аутентификация | JWT Bearer + API Key | - |
| Логирование | Serilog | 8.0 |
| Метрики | prometheus-net | 8.2 |
| Визуализация | Grafana | 10.2 |
| Документация | Swagger/OpenAPI | - |
| Тестирование | xUnit + Moq + FluentAssertions | - |
| Rate Limiting | AspNetCoreRateLimit | 5.0 |

---

## Архитектура проекта

Проект построен на принципах **Clean Architecture** с чётким разделением на слои:

```
TaskManager/
├── src/
│   ├── TaskManager.API/              # Презентационный слой (Web API)
│   │   ├── Controllers/              # REST контроллеры
│   │   ├── Middleware/               # Middleware (ошибки, логирование, идемпотентность)
│   │   ├── Auth/                     # Обработчики аутентификации
│   │   └── Program.cs                # Точка входа и конфигурация DI
│   │
│   ├── TaskManager.Application/      # Слой бизнес-логики
│   │   ├── DTOs/                     # Data Transfer Objects
│   │   │   ├── Auth/                 # DTO для аутентификации
│   │   │   ├── Tasks/                # DTO для задач
│   │   │   ├── Projects/             # DTO для проектов
│   │   │   ├── Tags/                 # DTO для тегов
│   │   │   ├── Users/                # DTO для пользователей
│   │   │   └── Common/               # Общие DTO (ApiError, ApiResponse)
│   │   ├── Services/                 # Реализации сервисов
│   │   ├── Interfaces/               # Интерфейсы сервисов и репозиториев
│   │   ├── Exceptions/               # Пользовательские исключения
│   │   └── Common/                   # Общие классы (PagedResult)
│   │
│   ├── TaskManager.Domain/           # Доменный слой
│   │   ├── Entities/                 # Доменные сущности
│   │   │   ├── User.cs               # Пользователь
│   │   │   ├── TaskItem.cs           # Задача
│   │   │   ├── Project.cs            # Проект
│   │   │   ├── Tag.cs                # Тег
│   │   │   ├── TaskTag.cs            # Связь задача-тег (M:M)
│   │   │   ├── UserProject.cs        # Связь пользователь-проект (M:M)
│   │   │   ├── ApiKey.cs             # API ключ
│   │   │   └── IdempotencyKey.cs     # Ключ идемпотентности
│   │   └── Enums/                    # Перечисления
│   │       ├── UserRole.cs           # Роли пользователей
│   │       ├── TaskItemStatus.cs     # Статусы задач
│   │       ├── TaskPriority.cs       # Приоритеты задач
│   │       └── ProjectRole.cs        # Роли в проекте
│   │
│   └── TaskManager.Infrastructure/   # Слой инфраструктуры
│       ├── Data/                     # Работа с данными
│       │   ├── AppDbContext.cs       # EF Core контекст
│       │   └── Configurations/       # Fluent API конфигурации
│       ├── Repositories/             # Реализации репозиториев
│       │   ├── BaseRepository.cs     # Базовый репозиторий (EF Core)
│       │   ├── TaskTagRepository.cs  # Репозиторий с Dapper
│       │   └── ...                   # Остальные репозитории
│       └── Services/                 # Инфраструктурные сервисы
│           └── RedisCacheService.cs  # Сервис кэширования
│
├── tests/
│   └── TaskManager.Tests/            # Unit-тесты
│       └── Repositories/             # Тесты репозиториев
│
├── docker/
│   ├── liquibase/                    # Миграции базы данных
│   │   ├── db.changelog-master.xml   # Главный changelog
│   │   └── changesets/               # Отдельные миграции
│   ├── prometheus/                   # Конфигурация Prometheus
│   └── grafana/                      # Дашборды и источники данных
│
└── docker-compose.yml                # Инфраструктура в Docker
```

### Принципы архитектуры

1. **Разделение ответственности**: Каждый слой имеет чётко определённую роль
2. **Инверсия зависимостей**: Высокоуровневые модули не зависят от низкоуровневых
3. **Единый формат ответов**: Все API ответы обёрнуты в `ApiResponse<T>`
4. **Асинхронность**: Все операции I/O выполняются асинхронно

---

## Модель данных

### ER-диаграмма

```
┌─────────────┐       ┌─────────────┐       ┌─────────────┐
│   Users     │       │   Projects  │       │    Tags     │
├─────────────┤       ├─────────────┤       ├─────────────┤
│ Id (PK)     │       │ Id (PK)     │       │ Id (PK)     │
│ Username    │◄──────┤ Name        │       │ Name        │
│ Email       │   M:M │ Description │       │ Color       │
│ PasswordHash│       │ CreatedAt   │       │ CreatedAt   │
│ Role        │       │ UpdatedAt   │       └──────┬──────┘
│ CreatedAt   │       └──────┬──────┘              │
│ UpdatedAt   │              │                     │ M:M
└──────┬──────┘              │                     │
       │                     │               ┌─────┴─────┐
       │ 1:M           1:M   │               │ TaskTags  │
       │                     │               ├───────────┤
       ▼                     ▼               │ TaskId    │
┌──────────────────────────────────┐         │ TagId     │
│            Tasks                 │◄────────┘           
├──────────────────────────────────┤
│ Id (PK)                          │
│ Title                            │
│ Description                      │
│ Status (Pending/InProgress/...)  │
│ Priority (Low/Medium/High/...)   │
│ DueDate                          │
│ UserId (FK)                      │
│ ProjectId (FK, nullable)         │
│ CreatedAt                        │
│ UpdatedAt                        │
└──────────────────────────────────┘

┌─────────────────┐
│  UserProjects   │  (M:M связь Users-Projects)
├─────────────────┤
│ UserId (FK)     │
│ ProjectId (FK)  │
│ Role            │
│ JoinedAt        │
└─────────────────┘
```

### Статусы задач

| Значение | Описание |
|----------|----------|
| Pending | Ожидает выполнения |
| InProgress | В процессе |
| Completed | Завершена |
| Cancelled | Отменена |

### Приоритеты задач

| Значение | Описание |
|----------|----------|
| Low | Низкий |
| Medium | Средний |
| High | Высокий |
| Critical | Критический |

---

## Быстрый старт

### Предварительные требования

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/get-started) и Docker Compose
- (Опционально) Visual Studio 2022 или VS Code

### 1. Запуск инфраструктуры

```bash
cd TaskManager
docker-compose up -d
```

Будут запущены:
- **PostgreSQL** — порт 5432 (логин: taskmanager, пароль: taskmanager_password)
- **Redis** — порт 6379
- **Prometheus** — порт 9090
- **Grafana** — порт 3000 (логин: admin, пароль: admin)
- **Liquibase** — автоматически применит миграции

### 2. Запуск API

```bash
cd src/TaskManager.API
dotnet run
```

### 3. Доступные URL

| Сервис | URL |
|--------|-----|
| API | http://localhost:5000 |
| Swagger UI | http://localhost:5000/swagger |
| Health Check | http://localhost:5000/health |
| Prometheus Metrics | http://localhost:5000/metrics |
| Prometheus UI | http://localhost:9090 |
| Grafana | http://localhost:3000 |

---

## Запуск после клонирования с GitHub

### Требования к системе

Перед началом убедитесь, что установлено:

| Компонент | Версия | Проверка | Ссылка для скачивания |
|-----------|--------|----------|----------------------|
| **Docker Desktop** | 4.0+ | `docker --version` | [docker.com/get-started](https://www.docker.com/get-started) |
| **.NET 8 SDK** | 8.0+ | `dotnet --version` | [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0) |
| **Git** | 2.0+ | `git --version` | [git-scm.com](https://git-scm.com/downloads) |

> ⚠️ **Важно:** Необходим именно **Docker Desktop** (не просто Docker Engine), так как используется `host.docker.internal` для связи между контейнерами и хост-машиной.

### Пошаговая инструкция

#### Шаг 1: Клонирование репозитория

```bash
git clone https://github.com/UsachovaUlyana/TaskManager.git
cd TaskManager
```

#### Шаг 2: Запуск Docker инфраструктуры

```bash
docker-compose up -d
```

Дождитесь запуска всех контейнеров (около 30-60 секунд):

```bash
# Проверить статус контейнеров
docker-compose ps
```

Ожидаемый результат — все контейнеры в статусе `Up` (кроме liquibase, который завершится после миграций):

```
NAME                     STATUS
taskmanager-postgres     Up (healthy)
taskmanager-redis        Up (healthy)
taskmanager-prometheus   Up
taskmanager-grafana      Up
taskmanager-liquibase    Exited (0)   ← это нормально!
```

#### Шаг 3: Запуск .NET API

```bash
cd src/TaskManager.API
dotnet restore
dotnet run
```

#### Шаг 4: Проверка работоспособности

Откройте браузер и перейдите по адресам:

| Сервис | URL | Ожидаемый результат |
|--------|-----|---------------------|
| Swagger UI | http://localhost:5000/swagger | Документация API |
| Health Check | http://localhost:5000/health | `Healthy` |
| Prometheus | http://localhost:9090 | Web-интерфейс Prometheus |
| Grafana | http://localhost:3000 | Логин: `admin` / `admin` |

### Быстрый тест API

1. **Регистрация пользователя** — в Swagger выполните `POST /api/auth/register`:
   ```json
   {
     "username": "testuser",
     "email": "test@example.com",
     "password": "Password123!"
   }
   ```

2. **Получение токена** — выполните `POST /api/auth/login`:
   ```json
   {
     "username": "testuser",
     "password": "Password123!"
   }
   ```

3. **Авторизация в Swagger** — нажмите кнопку "Authorize" и введите:
   ```
   Bearer <скопированный_токен>
   ```
   > ⚠️ Не забудьте префикс `Bearer ` (с пробелом после)!

4. **Создание задачи** — выполните `POST /api/tasks`:
   ```json
   {
     "title": "Моя первая задача",
     "description": "Описание задачи",
     "priority": "High",
     "status": "Pending"
   }
   ```

### Остановка проекта

```bash
# Остановить API (в терминале где запущен)
Ctrl+C

# Остановить Docker контейнеры
docker-compose down

# Остановить и удалить данные (БД, кэш)
docker-compose down -v
```

---

## API Endpoints

### Аутентификация (`/api/auth`)

| Метод | Endpoint | Описание | Авторизация |
|-------|----------|----------|-------------|
| POST | `/api/auth/register` | Регистрация нового пользователя | Нет |
| POST | `/api/auth/login` | Вход и получение JWT токена | Нет |

### Задачи (`/api/tasks`)

| Метод | Endpoint | Описание | Авторизация |
|-------|----------|----------|-------------|
| GET | `/api/tasks` | Получить список задач (с пагинацией и фильтрацией) | JWT/ApiKey |
| GET | `/api/tasks/{id}` | Получить задачу по ID | JWT/ApiKey |
| POST | `/api/tasks` | Создать задачу | JWT/ApiKey |
| PUT | `/api/tasks/{id}` | Обновить задачу | JWT/ApiKey |
| DELETE | `/api/tasks/{id}` | Удалить задачу | JWT (Admin/Owner) |

**Параметры фильтрации для GET /api/tasks:**
- `status` — фильтр по статусу (Pending, InProgress, Completed, Cancelled)
- `priority` — фильтр по приоритету (Low, Medium, High, Critical)
- `projectId` — фильтр по проекту
- `dueDateFrom` — минимальная дата выполнения
- `dueDateTo` — максимальная дата выполнения
- `page` — номер страницы (по умолчанию 1)
- `pageSize` — размер страницы (по умолчанию 10)

### Проекты (`/api/projects`)

| Метод | Endpoint | Описание | Авторизация |
|-------|----------|----------|-------------|
| GET | `/api/projects` | Получить список проектов | JWT/ApiKey |
| GET | `/api/projects/{id}` | Получить проект по ID | JWT/ApiKey |
| POST | `/api/projects` | Создать проект | JWT/ApiKey |
| PUT | `/api/projects/{id}` | Обновить проект | JWT/ApiKey |
| DELETE | `/api/projects/{id}` | Удалить проект | JWT (Admin) |
| POST | `/api/projects/{id}/members` | Добавить участника | JWT/ApiKey |
| DELETE | `/api/projects/{id}/members/{userId}` | Удалить участника | JWT/ApiKey |

### Теги (`/api/tags`)

| Метод | Endpoint | Описание | Авторизация |
|-------|----------|----------|-------------|
| GET | `/api/tags` | Получить список тегов | JWT/ApiKey |
| GET | `/api/tags/{id}` | Получить тег по ID | JWT/ApiKey |
| POST | `/api/tags` | Создать тег | JWT/ApiKey |
| PUT | `/api/tags/{id}` | Обновить тег | JWT/ApiKey |
| DELETE | `/api/tags/{id}` | Удалить тег | JWT (Admin) |

### Пользователи (`/api/users`) — только для Admin

| Метод | Endpoint | Описание | Авторизация |
|-------|----------|----------|-------------|
| GET | `/api/users` | Получить список пользователей | JWT (Admin) |
| GET | `/api/users/{id}` | Получить пользователя по ID | JWT (Admin) |
| POST | `/api/users` | Создать пользователя | JWT (Admin) |
| DELETE | `/api/users/{id}` | Удалить пользователя | JWT (Admin) |

---

## Аутентификация и авторизация

### JWT Bearer Token

Получение токена:

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "user@example.com",
  "password": "password123"
}
```

Использование токена:

```http
GET /api/tasks
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### API Key

Альтернативный способ аутентификации для сервис-to-сервис взаимодействия:

```http
GET /api/tasks
X-Api-Key: your-api-key-here
```

### Матрица авторизации

| Действие | Admin | User | API Key |
|----------|-------|------|---------|
| GET /tasks | Все задачи | Только свои | Все задачи |
| POST /tasks | ✅ | ✅ | ✅ |
| PUT /tasks/{id} | Любая | Только своя | Любая |
| DELETE /tasks/{id} | Любая | Только своя | ❌ |
| GET /users | ✅ | ❌ | ❌ |
| POST /users | ✅ | ❌ | ❌ |
| DELETE /users/{id} | ✅ | ❌ | ❌ |
| GET /projects | Все | Только участвует | Все |
| DELETE /projects/{id} | ✅ | ❌ | ❌ |
| DELETE /tags/{id} | ✅ | ❌ | ❌ |

---

## Кэширование

### Redis Cache

GET-запросы кэшируются в Redis с TTL 5 минут:

- `tasks:list:*` — списки задач
- `tasks:{id}` — отдельные задачи
- `projects:*` — проекты

### Инвалидация кэша

Кэш автоматически инвалидируется при:
- Создании новой сущности (POST)
- Обновлении сущности (PUT)
- Удалении сущности (DELETE)

---

## Мониторинг и метрики

### Prometheus Metrics

Доступны на `/metrics`:

- `http_requests_received_total` — общее количество запросов
- `http_request_duration_seconds` — время выполнения запросов
- `http_requests_in_progress` — текущие запросы

### Grafana Dashboard

Предустановленный дашборд включает:
- График частоты запросов
- Распределение времени ответа (p50, p95)
- Количество ошибок (4xx, 5xx)
- Запросы по методам и статус-кодам

### Health Checks

```http
GET /health
```

Проверяются:
- PostgreSQL — подключение к базе данных
- Redis — подключение к кэшу

---

## Тестирование

### Запуск тестов

```bash
cd TaskManager
dotnet test
```

### Покрытие тестами

Репозитории покрыты unit-тестами (48 тестов):

| Репозиторий | Тесты |
|-------------|-------|
| UserRepository | 12 тестов (CRUD, GetByUsername, GetByEmail) |
| TaskRepository | 12 тестов (CRUD, фильтрация, пагинация) |
| ProjectRepository | 12 тестов (CRUD, GetByUserId, IsUserMember) |
| TagRepository | 12 тестов (CRUD, GetByName, GetByIds) |

---

## Конфигурация

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=taskmanager_db;Username=taskmanager;Password=taskmanager_password",
    "Redis": "localhost:6379"
  },
  "Jwt": {
    "Key": "YourSecretKeyHere_MinLength32Characters",
    "Issuer": "TaskManager",
    "Audience": "TaskManagerUsers",
    "ExpiresInMinutes": 60
  }
}
```

### Переменные окружения

Можно переопределить через переменные окружения:

| Переменная | Описание |
|------------|----------|
| `ConnectionStrings__DefaultConnection` | Строка подключения к PostgreSQL |
| `ConnectionStrings__Redis` | Строка подключения к Redis |
| `Jwt__Key` | Секретный ключ для JWT |

---

## Дополнительные функции

### Rate Limiting

Ограничение запросов:
- 100 запросов в минуту
- 1000 запросов в час

При превышении лимита возвращается HTTP 429 (Too Many Requests).

### Idempotency

Для POST-запросов поддерживается идемпотентность через заголовок:

```http
POST /api/tasks
X-Idempotency-Key: unique-request-id-123
```

Повторный запрос с тем же ключом в течение 24 часов вернёт сохранённый ответ.

### Единый формат ответов

**Успешный ответ:**
```json
{
  "success": true,
  "data": { ... },
  "error": null
}
```

**Ответ с ошибкой:**
```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "NOT_FOUND",
    "message": "Task with ID '...' was not found.",
    "details": null,
    "traceId": "0HN9ABCD..."
  }
}
```

### Коды ошибок

| Код | HTTP статус | Описание |
|-----|-------------|----------|
| `NOT_FOUND` | 404 | Ресурс не найден |
| `CONFLICT` | 409 | Конфликт данных (дубликат) |
| `VALIDATION_ERROR` | 400 | Ошибка валидации |
| `UNAUTHORIZED` | 401 | Не авторизован |
| `FORBIDDEN` | 403 | Доступ запрещён |
| `INTERNAL_ERROR` | 500 | Внутренняя ошибка сервера |

---

## Соответствие критериям оценивания

| Критерий | Баллы | Реализация |
|----------|-------|------------|
| **Архитектура и чистота кода** | **20** | |
| Разделение слоёв | 5 | Controllers → Services → Repositories |
| DTO для запросов/ответов | 3 | DTOs в Application слое |
| Нет бизнес-логики в контроллерах | 4 | Вся логика в Services |
| Naming conventions | 4 | PascalCase для классов, camelCase для переменных |
| async/await | 4 | Все I/O операции асинхронны |
| **База данных** | **5** | |
| PostgreSQL + Docker + Liquibase | 3 | docker-compose.yml + liquibase changesets |
| Many-to-many связи | 2 | TaskTags, UserProjects |
| **EF Core + Dapper** | **15** | |
| CRUD через EF Core | 5 | BaseRepository<T> |
| Dapper с транзакцией | 5 | TaskTagRepository.AssignTagsToTaskAsync |
| Репозиторный слой | 5 | Отдельные репозитории для каждой сущности |
| **Авторизация** | **15** | |
| JWT Bearer | 5 | JwtBearerDefaults в Program.cs |
| API Key | 5 | ApiKeyAuthenticationHandler |
| Ролевой доступ | 5 | [Authorize(Roles = "Admin")] |
| **Redis кэширование** | **5** | |
| GET кэшируются | 3 | RedisCacheService в TaskService, ProjectService |
| Инвалидация кэша | 2 | RemoveByPatternAsync при изменениях |
| **Prometheus metrics** | **5** | prometheus-net.AspNetCore, /metrics |
| **Swagger** | **5** | |
| Swagger UI | 3 | /swagger |
| Документация эндпоинтов | 2 | XML-комментарии, ProducesResponseType |
| **Логирование** | **5** | |
| Логирование запросов/ошибок | 3 | RequestLoggingMiddleware |
| Структурированные логи | 2 | Serilog с JSON форматом |
| **Pagination + Filtering** | **5** | |
| Пагинация | 3 | PagedResult<T>, page/pageSize параметры |
| Фильтрация | 2 | TaskFilterRequest с фильтрами |
| **Обработка ошибок** | **5** | |
| Централизованный middleware | 3 | ExceptionMiddleware |
| Единый формат ответа | 2 | ApiError, ApiResponse<T> |
| **Health Checks** | **5** | |
| Проверка Web API | 2 | /health endpoint |
| Проверка PostgreSQL и Redis | 3 | AddNpgSql, AddRedis |
| **Unit тесты** | **15** | |
| Покрытие репозиториев | 10 | 4 репозитория × ~12 тестов |
| CRUD сценарии | 5 | Add, Get, Update, Delete, GetPaged |
| **Бонусы** | **+10** | |
| Grafana | 5 | Предустановленный дашборд |
| Rate Limiting | 3 | AspNetCoreRateLimit, 100 req/min |
| Idempotency | 2 | X-Idempotency-Key header |

**Итого: 110 баллов** (100 обязательных + 10 бонусных)

---

## Troubleshooting

### Проблема: "Не удается получить доступ к сайту" на localhost:5000

**Причина:** API не запущен или запущен на другом порту.

**Решение:**
1. Убедитесь, что API запущен командой `dotnet run`
2. Проверьте вывод в консоли — там будет указан фактический URL
3. Если порт отличается от 5000, используйте указанный порт

### Проблема: Ошибка подключения к PostgreSQL

**Причина:** Docker контейнер с PostgreSQL не запущен или не готов.

**Решение:**
```bash
# Проверить статус контейнеров
docker-compose ps

# Если postgres не запущен
docker-compose up -d postgres

# Подождать 10-15 секунд и проверить логи
docker-compose logs postgres
```

### Проблема: 401 Unauthorized при использовании токена

**Причина:** Неправильный формат токена в Swagger.

**Решение:**
В поле Authorize вводить токен **с префиксом `Bearer `**:
```
Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```
> Обратите внимание на пробел после слова `Bearer`!

### Проблема: Grafana показывает "No data"

**Причина:** Нет данных метрик или Prometheus не собирает метрики.

**Решение:**
1. Сделайте несколько запросов к API через Swagger
2. Подождите 15-30 секунд (интервал сбора метрик)
3. Проверьте, что API отдает метрики: http://localhost:5000/metrics
4. Проверьте статус target в Prometheus: http://localhost:9090/targets
5. Если target показывает `DOWN`, убедитесь что Docker Desktop запущен

### Проблема: Liquibase контейнер показывает "Exited"

**Это нормально!** Liquibase — это утилита для миграций. Она:
1. Запускается
2. Применяет миграции к базе данных
3. Завершает работу (Exit code 0 = успешно)

Проверить успешность миграций:
```bash
docker-compose logs liquibase
```

### Проблема: Порт 5432/6379/3000/9090 уже занят

**Решение:**
1. Найти процесс, занимающий порт:
   ```bash
   # Windows PowerShell
   netstat -ano | findstr :5432
   
   # Завершить процесс по PID
   taskkill /PID <pid> /F
   ```

2. Или изменить порты в `docker-compose.yml`:
   ```yaml
   ports:
     - "5433:5432"  # Внешний порт изменен на 5433
   ```

### Проблема: Ошибки при сборке проекта

**Решение:**
```bash
# Очистить и пересобрать
dotnet clean
dotnet restore
dotnet build
```

### Проверка общего состояния системы

```bash
# 1. Проверить Docker
docker --version
docker-compose --version

# 2. Проверить .NET
dotnet --version

# 3. Проверить контейнеры
docker-compose ps

# 4. Проверить логи всех контейнеров
docker-compose logs

# 5. Полный перезапуск
docker-compose down
docker-compose up -d
```

---

## Лицензия

MIT
