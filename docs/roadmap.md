# Роадмапа реализации системы бронирования оборудования

**Период:** 2 недели (14 дней)  
**Проект:** Система бронирования лабораторного оборудования университета

---

## Неделя 1: Инфраструктура и базовый функционал

### День 1-2: Настройка инфраструктуры и базы данных
**Задачи:**
- ✅ Настройка проекта ASP.NET Core Web API (.NET 8)
- ⬜ Установка и настройка Entity Framework Core
- ⬜ Настройка PostgreSQL/SQL Server для разработки
- ⬜ Создание миграций для всех таблиц БД:
  - users, roles, user_roles
  - subjects, locations
  - equipment_types, equipment
  - slots, bookings
  - equipment_requests, notifications
- ⬜ Настройка системы логирования (Serilog)
- ⬜ Настройка CORS для frontend

**Результат:** Работающая БД со схемой, подключенная к backend

---

### День 3-4: Аутентификация и авторизация
**Задачи:**
- ⬜ Реализация модели пользователей (User, Role, UserRole entities)
- ⬜ Настройка JWT-аутентификации
- ⬜ Endpoints для регистрации и авторизации:
  - `POST /api/auth/register`
  - `POST /api/auth/login`
  - `POST /api/auth/logout`
- ⬜ Middleware для проверки ролей (Student, Staff, Admin)
- ⬜ Реализация UserService и AuthService
- ⬜ Password hashing (BCrypt/Identity)

**Результат:** Рабочая система аутентификации с ролями

---

### День 5-6: Управление профилями и базовые справочники
**Задачи:**
- ⬜ Endpoints для управления профилем:
  - `GET /api/users/profile`
  - `PUT /api/users/profile`
  - `GET /api/users/{id}` (только для админа/сотрудника)
- ⬜ Справочник предметов (Subjects):
  - `GET /api/subjects` (список)
  - `POST /api/subjects` (создание, только Staff/Admin)
  - `PUT /api/subjects/{id}` (редактирование)
  - `DELETE /api/subjects/{id}` (удаление)
- ⬜ Справочник аудиторий (Locations):
  - `GET /api/locations`
  - `POST /api/locations` (только Admin)
  - `PUT /api/locations/{id}`
  - `DELETE /api/locations/{id}`

**Результат:** Управление профилями и базовыми справочниками

---

### День 7: Управление оборудованием (часть 1)
**Задачи:**
- ⬜ Реализация Equipment и EquipmentType entities
- ⬜ Endpoints для типов оборудования:
  - `GET /api/equipment-types`
  - `POST /api/equipment-types` (только Staff/Admin)
  - `PUT /api/equipment-types/{id}`
  - `DELETE /api/equipment-types/{id}`
- ⬜ Endpoints для экземпляров оборудования:
  - `GET /api/equipment` (фильтрация по location, type, status)
  - `GET /api/equipment/{id}`
  - `POST /api/equipment` (только Staff/Admin)

**Результат:** Базовое управление оборудованием

---

## Неделя 2: Основная бизнес-логика и дополнительные функции

### День 8-9: Управление оборудованием (часть 2) и слоты
**Задачи:**
- ⬜ Завершение Equipment endpoints:
  - `PUT /api/equipment/{id}` (обновление статуса, привязка к сотруднику)
  - `DELETE /api/equipment/{id}` (только Admin)
- ⬜ Реализация Slot entity и SlotService
- ⬜ Endpoints для управления слотами (Сотрудник):
  - `POST /api/slots` (создание доступного времени)
  - `GET /api/slots` (фильтрация по equipment, date range)
  - `PUT /api/slots/{id}` (изменение времени)
  - `DELETE /api/slots/{id}` (отмена слота)
- ⬜ Валидация: проверка пересечений слотов для одного оборудования

**Результат:** Управление временными слотами оборудования

---

### День 10-11: Система бронирования
**Задачи:**
- ⬜ Реализация Booking entity и BookingService
- ⬜ Endpoints для студентов:
  - `GET /api/bookings/available-slots` (просмотр доступных слотов)
  - `POST /api/bookings` (создание брони, статус: pending_approval)
  - `GET /api/bookings/my` (мои бронирования)
  - `DELETE /api/bookings/{id}` (отмена своей брони)
- ⬜ Endpoints для сотрудников:
  - `GET /api/bookings` (все бронирования по их оборудованию)
  - `PUT /api/bookings/{id}/approve` (подтверждение)
  - `PUT /api/bookings/{id}/reject` (отклонение)
  - `DELETE /api/bookings/{id}` (отмена брони сотрудником)
- ⬜ Бизнес-логика:
  - При создании брони слот меняет статус на "booked"
  - При отмене брони слот возвращается в "available"
  - Уведомления (базовая реализация)

**Результат:** Полноценная система бронирования с подтверждением

---

### День 12: Заявки на оборудование и уведомления
**Задачи:**
- ⬜ Реализация EquipmentRequest entity
- ⬜ Endpoints для заявок:
  - `POST /api/equipment-requests` (создание заявки)
  - `GET /api/equipment-requests/my` (мои заявки для студента)
  - `GET /api/equipment-requests` (все заявки для Staff/Admin)
  - `PUT /api/equipment-requests/{id}/approve` (одобрение)
  - `PUT /api/equipment-requests/{id}/reject` (отклонение)
- ⬜ Реализация Notification entity и NotificationService
- ⬜ Endpoints для уведомлений:
  - `GET /api/notifications` (список уведомлений пользователя)
  - `PUT /api/notifications/{id}/read` (отметить как прочитанное)
  - `DELETE /api/notifications/{id}` (удалить уведомление)
- ⬜ Триггеры уведомлений:
  - При одобрении/отклонении брони
  - При отмене слота сотрудником (если есть брони)
  - При одобрении/отклонении заявки на оборудование

**Результат:** Система заявок и уведомлений

---

### День 13: Административные функции
**Задачи:**
- ⬜ Endpoints для управления пользователями (Admin):
  - `GET /api/admin/users` (список всех пользователей)
  - `POST /api/admin/users` (создание пользователя)
  - `PUT /api/admin/users/{id}` (редактирование, в т.ч. изменение ролей)
  - `DELETE /api/admin/users/{id}` (удаление/деактивация)
  - `POST /api/admin/users/{id}/assign-role` (назначение роли)
  - `DELETE /api/admin/users/{id}/remove-role` (снятие роли)
- ⬜ Endpoints для статистики (Admin/Staff):
  - `GET /api/reports/equipment-usage` (статистика использования)
  - `GET /api/reports/booking-statistics` (статистика бронирований)
- ⬜ Фильтрация и пагинация для списочных запросов

**Результат:** Полный административный функционал

---

### День 14: Тестирование, документация и деплой
**Задачи:**
- ⬜ Написание Unit-тестов для критической бизнес-логики:
  - SlotService (проверка пересечений)
  - BookingService (изменение статусов)
  - AuthService (JWT, роли)
- ⬜ Интеграционные тесты для основных API endpoints
- ⬜ Обновление Swagger-документации с примерами запросов
- ⬜ Финальное тестирование всех user flows:
  - Студент: регистрация → просмотр оборудования → бронирование → получение уведомления
  - Сотрудник: создание слотов → подтверждение броней → управление оборудованием
  - Админ: управление пользователями → просмотр статистики
- ⬜ Настройка CI/CD (опционально)
- ⬜ Подготовка к деплою на тестовый сервер
- ⬜ Написание README с инструкциями по запуску

**Результат:** Готовое к использованию приложение с документацией

---

## Дополнительные задачи (при наличии времени)

- Реализация поиска оборудования по различным критериям
- Экспорт отчётов в CSV/Excel
- Email-уведомления (интеграция с SMTP)
- Логирование всех критических операций в отдельную таблицу audit_log
- Soft delete для пользователей и оборудования
- Пагинация и сортировка для всех списочных endpoints
- Rate limiting для API endpoints
- Swagger UI с авторизацией JWT

---

## Технический стек

**Backend:**
- ASP.NET Core 8.0 Web API
- Entity Framework Core 8.0
- PostgreSQL / SQL Server
- JWT Authentication
- Serilog для логирования
- Swagger/OpenAPI
- xUnit для тестирования

**Frontend:**
- Nuxt.js 3 (Vue.js framework)
- TypeScript
- Tailwind CSS / Vuetify (UI компоненты)
- Pinia (state management)
- Axios для HTTP запросов

**Инструменты:**
- Visual Studio Code / Visual Studio 2022
- Postman для тестирования API
- Git для версионирования

---

## Критерии успеха

✅ Все роли (Student, Staff, Admin) могут авторизоваться  
✅ Сотрудник может создавать слоты для оборудования  
✅ Студент может просматривать и бронировать доступные слоты  
✅ Сотрудник может подтверждать/отклонять бронирования  
✅ Система уведомляет пользователей об изменении статусов  
✅ Админ может управлять пользователями и просматривать статистику  
✅ Все операции логируются  
✅ API задокументировано в Swagger  

---

## Риски и митигация

| Риск | Вероятность | Влияние | Митигация |
|------|-------------|---------|-----------|
| Сложности с Entity Framework | Средняя | Высокое | Начать с миграций в первые дни |
| Недостаток времени на тестирование | Высокая | Среднее | Тестировать по ходу разработки |
| Проблемы с валидацией бизнес-логики | Средняя | Высокое | Code review, unit-тесты для SlotService |
| Сложность с JWT и ролями | Низкая | Среднее | Использовать готовые примеры .NET |

---

**Последнее обновление:** 17 ноября 2025
