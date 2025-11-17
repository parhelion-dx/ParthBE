# API Контракты системы бронирования оборудования

**Версия API:** 1.0  
**Base URL:** `https://api.equipment-booking.example.com/api`  
**Формат:** JSON  
**Аутентификация:** JWT Bearer Token

---

## Оглавление

1. [Аутентификация](#1-аутентификация)
2. [Управление профилем](#2-управление-профилем)
3. [Предметы](#3-предметы-subjects)
4. [Аудитории](#4-аудитории-locations)
5. [Типы оборудования](#5-типы-оборудования-equipment-types)
6. [Оборудование](#6-оборудование-equipment)
7. [Слоты](#7-слоты-slots)
8. [Бронирования](#8-бронирования-bookings)
9. [Заявки на оборудование](#9-заявки-на-оборудование-equipment-requests)
10. [Уведомления](#10-уведомления-notifications)
11. [Администрирование](#11-администрирование-admin)
12. [Отчёты](#12-отчёты-reports)

---

## Общие соглашения

### HTTP Status Codes
- `200 OK` - Успешный запрос
- `201 Created` - Ресурс успешно создан
- `204 No Content` - Успешное удаление/обновление без тела ответа
- `400 Bad Request` - Ошибка валидации
- `401 Unauthorized` - Не авторизован
- `403 Forbidden` - Недостаточно прав
- `404 Not Found` - Ресурс не найден
- `409 Conflict` - Конфликт (например, пересечение слотов)
- `500 Internal Server Error` - Внутренняя ошибка сервера

### Формат ошибок
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Описание ошибки для пользователя",
    "details": {
      "field": "email",
      "issue": "Email уже используется"
    }
  }
}
```

### Пагинация (для списочных запросов)
**Query параметры:**
- `page` (int, default: 1) - Номер страницы
- `pageSize` (int, default: 20, max: 100) - Размер страницы

**Формат ответа:**
```json
{
  "data": [...],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalItems": 150,
    "totalPages": 8
  }
}
```

---

## 1. Аутентификация

### 1.1. Регистрация пользователя
**Endpoint:** `POST /api/auth/register`  
**Доступ:** Публичный

**Request Body:**
```json
{
  "fullName": "Иванов Иван Иванович",
  "email": "ivanov@example.com",
  "password": "SecurePassword123!",
  "profileInfo": "Группа: ИДБ-23-05",
  "role": "student"
}
```

**Validation:**
- `fullName`: обязательно, 2-100 символов
- `email`: обязательно, валидный email, уникальный
- `password`: обязательно, минимум 8 символов, содержит цифры и буквы
- `role`: обязательно, одно из значений: `student`, `staff`, `admin`

**Response: 201 Created**
```json
{
  "id": 1,
  "fullName": "Иванов Иван Иванович",
  "email": "ivanov@example.com",
  "profileInfo": "Группа: ИДБ-23-05",
  "roles": ["student"],
  "createdAt": "2025-11-17T10:30:00Z"
}
```

---

### 1.2. Вход в систему
**Endpoint:** `POST /api/auth/login`  
**Доступ:** Публичный

**Request Body:**
```json
{
  "email": "ivanov@example.com",
  "password": "SecurePassword123!"
}
```

**Response: 200 OK**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-11-18T10:30:00Z",
  "user": {
    "id": 1,
    "fullName": "Иванов Иван Иванович",
    "email": "ivanov@example.com",
    "roles": ["student"]
  }
}
```

---

### 1.3. Выход из системы
**Endpoint:** `POST /api/auth/logout`  
**Доступ:** Авторизованные пользователи  
**Headers:** `Authorization: Bearer {token}`

**Response: 204 No Content**

---

## 2. Управление профилем

### 2.1. Получить свой профиль
**Endpoint:** `GET /api/users/profile`  
**Доступ:** Авторизованные пользователи  
**Headers:** `Authorization: Bearer {token}`

**Response: 200 OK**
```json
{
  "id": 1,
  "fullName": "Иванов Иван Иванович",
  "email": "ivanov@example.com",
  "profileInfo": "Группа: ИДБ-23-05",
  "roles": ["student"],
  "createdAt": "2025-11-17T10:30:00Z"
}
```

---

### 2.2. Обновить свой профиль
**Endpoint:** `PUT /api/users/profile`  
**Доступ:** Авторизованные пользователи  
**Headers:** `Authorization: Bearer {token}`

**Request Body:**
```json
{
  "fullName": "Иванов Иван Петрович",
  "profileInfo": "Группа: ЭДБ-22-02"
}
```

**Response: 200 OK**
```json
{
  "id": 1,
  "fullName": "Иванов Иван Петрович",
  "email": "ivanov@example.com",
  "profileInfo": "Группа: ЭДБ-22-02",
  "roles": ["student"],
  "createdAt": "2025-11-17T10:30:00Z"
}
```

---

### 2.3. Получить профиль пользователя по ID
**Endpoint:** `GET /api/users/{id}`  
**Доступ:** Staff, Admin  
**Headers:** `Authorization: Bearer {token}`

**Response: 200 OK**
```json
{
  "id": 5,
  "fullName": "Петрова Анна Сергеевна",
  "email": "petrova@example.com",
  "profileInfo": "Кафедра ИВТ",
  "roles": ["staff"],
  "createdAt": "2025-10-01T08:00:00Z"
}
```

---

## 3. Предметы (Subjects)

### 3.1. Получить список предметов
**Endpoint:** `GET /api/subjects`  
**Доступ:** Авторизованные пользователи  
**Headers:** `Authorization: Bearer {token}`

**Query Parameters:**
- `page` (int, optional)
- `pageSize` (int, optional)
- `search` (string, optional) - Поиск по названию

**Response: 200 OK**
```json
{
  "data": [
    {
      "id": 1,
      "name": "Электроника",
      "description": "Основы электроники и схемотехники"
    },
    {
      "id": 2,
      "name": "Физика",
      "description": "Общая физика"
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalItems": 2,
    "totalPages": 1
  }
}
```

---

### 3.2. Создать предмет
**Endpoint:** `POST /api/subjects`  
**Доступ:** Staff, Admin  
**Headers:** `Authorization: Bearer {token}`

**Request Body:**
```json
{
  "name": "Программирование",
  "description": "Основы программирования на Python"
}
```

**Response: 201 Created**
```json
{
  "id": 3,
  "name": "Программирование",
  "description": "Основы программирования на Python"
}
```

---

### 3.3. Обновить предмет
**Endpoint:** `PUT /api/subjects/{id}`  
**Доступ:** Staff, Admin  
**Headers:** `Authorization: Bearer {token}`

**Request Body:**
```json
{
  "name": "Программирование",
  "description": "Программирование на Python и C++"
}
```

**Response: 200 OK**
```json
{
  "id": 3,
  "name": "Программирование",
  "description": "Программирование на Python и C++"
}
```

---

### 3.4. Удалить предмет
**Endpoint:** `DELETE /api/subjects/{id}`  
**Доступ:** Admin  
**Headers:** `Authorization: Bearer {token}`

**Response: 204 No Content**

**Errors:**
- `409 Conflict` - Если к предмету привязано оборудование

---

## 4. Аудитории (Locations)

### 4.1. Получить список аудиторий
**Endpoint:** `GET /api/locations`  
**Доступ:** Авторизованные пользователи  
**Headers:** `Authorization: Bearer {token}`

**Query Parameters:**
- `page` (int, optional)
- `pageSize` (int, optional)
- `search` (string, optional)

**Response: 200 OK**
```json
{
  "data": [
    {
      "id": 1,
      "name": "Лаборатория 305",
      "description": "Лаборатория электроники"
    },
    {
      "id": 2,
      "name": "Аудитория 201",
      "description": "Компьютерный класс"
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalItems": 2,
    "totalPages": 1
  }
}
```

---

### 4.2. Создать аудиторию
**Endpoint:** `POST /api/locations`  
**Доступ:** Admin  
**Headers:** `Authorization: Bearer {token}`

**Request Body:**
```json
{
  "name": "Лаборатория 401",
  "description": "Лаборатория физики"
}
```

**Response: 201 Created**
```json
{
  "id": 3,
  "name": "Лаборатория 401",
  "description": "Лаборатория физики"
}
```

---

### 4.3. Обновить аудиторию
**Endpoint:** `PUT /api/locations/{id}`  
**Доступ:** Admin  
**Headers:** `Authorization: Bearer {token}`

**Request Body:**
```json
{
  "name": "Лаборатория 401",
  "description": "Лаборатория общей физики"
}
```

**Response: 200 OK**

---

### 4.4. Удалить аудиторию
**Endpoint:** `DELETE /api/locations/{id}`  
**Доступ:** Admin  
**Headers:** `Authorization: Bearer {token}`

**Response: 204 No Content**

**Errors:**
- `409 Conflict` - Если в аудитории находится оборудование

---

## 5. Типы оборудования (Equipment Types)

### 5.1. Получить список типов оборудования
**Endpoint:** `GET /api/equipment-types`  
**Доступ:** Авторизованные пользователи  
**Headers:** `Authorization: Bearer {token}`

**Query Parameters:**
- `subjectId` (int, optional) - Фильтр по предмету
- `page` (int, optional)
- `pageSize` (int, optional)

**Response: 200 OK**
```json
{
  "data": [
    {
      "id": 1,
      "name": "Осциллограф",
      "description": "Цифровой осциллограф для измерения сигналов",
      "subject": {
        "id": 1,
        "name": "Электроника"
      }
    },
    {
      "id": 2,
      "name": "Паяльная станция",
      "description": "Станция для пайки электронных компонентов",
      "subject": {
        "id": 1,
        "name": "Электроника"
      }
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalItems": 2,
    "totalPages": 1
  }
}
```

---

### 5.2. Создать тип оборудования
**Endpoint:** `POST /api/equipment-types`  
**Доступ:** Staff, Admin  
**Headers:** `Authorization: Bearer {token}`

**Request Body:**
```json
{
  "name": "Мультиметр",
  "description": "Цифровой мультиметр для измерений",
  "subjectId": 1
}
```

**Response: 201 Created**
```json
{
  "id": 3,
  "name": "Мультиметр",
  "description": "Цифровой мультиметр для измерений",
  "subject": {
    "id": 1,
    "name": "Электроника"
  }
}
```

---

### 5.3. Обновить тип оборудования
**Endpoint:** `PUT /api/equipment-types/{id}`  
**Доступ:** Staff, Admin  
**Headers:** `Authorization: Bearer {token}`

**Request Body:**
```json
{
  "name": "Мультиметр профессиональный",
  "description": "Профессиональный цифровой мультиметр",
  "subjectId": 1
}
```

**Response: 200 OK**

---

### 5.4. Удалить тип оборудования
**Endpoint:** `DELETE /api/equipment-types/{id}`  
**Доступ:** Admin  
**Headers:** `Authorization: Bearer {token}`

**Response: 204 No Content**

**Errors:**
- `409 Conflict` - Если существуют экземпляры этого типа

---

## 6. Оборудование (Equipment)

### 6.1. Получить список оборудования
**Endpoint:** `GET /api/equipment`  
**Доступ:** Авторизованные пользователи  
**Headers:** `Authorization: Bearer {token}`

**Query Parameters:**
- `typeId` (int, optional) - Фильтр по типу оборудования
- `locationId` (int, optional) - Фильтр по аудитории
- `status` (string, optional) - Фильтр по статусу: `available`, `in_maintenance`, `retired`
- `assignedToStaffId` (int, optional) - Фильтр по ответственному сотруднику
- `page` (int, optional)
- `pageSize` (int, optional)

**Response: 200 OK**
```json
{
  "data": [
    {
      "id": 1,
      "type": {
        "id": 1,
        "name": "Осциллограф",
        "subject": {
          "id": 1,
          "name": "Электроника"
        }
      },
      "inventoryNumber": "OSC-2023-001",
      "location": {
        "id": 1,
        "name": "Лаборатория 305"
      },
      "assignedStaff": {
        "id": 5,
        "fullName": "Петрова Анна Сергеевна"
      },
      "status": "available"
    },
    {
      "id": 2,
      "type": {
        "id": 1,
        "name": "Осциллограф",
        "subject": {
          "id": 1,
          "name": "Электроника"
        }
      },
      "inventoryNumber": "OSC-2023-002",
      "location": {
        "id": 1,
        "name": "Лаборатория 305"
      },
      "assignedStaff": null,
      "status": "in_maintenance"
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalItems": 2,
    "totalPages": 1
  }
}
```

---

### 6.2. Получить оборудование по ID
**Endpoint:** `GET /api/equipment/{id}`  
**Доступ:** Авторизованные пользователи  
**Headers:** `Authorization: Bearer {token}`

**Response: 200 OK**
```json
{
  "id": 1,
  "type": {
    "id": 1,
    "name": "Осциллограф",
    "description": "Цифровой осциллограф для измерения сигналов",
    "subject": {
      "id": 1,
      "name": "Электроника"
    }
  },
  "inventoryNumber": "OSC-2023-001",
  "location": {
    "id": 1,
    "name": "Лаборатория 305",
    "description": "Лаборатория электроники"
  },
  "assignedStaff": {
    "id": 5,
    "fullName": "Петрова Анна Сергеевна",
    "email": "petrova@example.com"
  },
  "status": "available"
}
```

---

### 6.3. Создать оборудование
**Endpoint:** `POST /api/equipment`  
**Доступ:** Staff, Admin  
**Headers:** `Authorization: Bearer {token}`

**Request Body:**
```json
{
  "typeId": 1,
  "inventoryNumber": "OSC-2023-003",
  "locationId": 1,
  "assignedStaffId": 5,
  "status": "available"
}
```

**Validation:**
- `typeId`: обязательно, должен существовать
- `inventoryNumber`: обязательно, уникальный
- `locationId`: обязательно, должна существовать
- `assignedStaffId`: опционально, должен быть пользователем с ролью Staff
- `status`: обязательно, одно из: `available`, `in_maintenance`, `retired`

**Response: 201 Created**
```json
{
  "id": 3,
  "type": {
    "id": 1,
    "name": "Осциллограф",
    "subject": {
      "id": 1,
      "name": "Электроника"
    }
  },
  "inventoryNumber": "OSC-2023-003",
  "location": {
    "id": 1,
    "name": "Лаборатория 305"
  },
  "assignedStaff": {
    "id": 5,
    "fullName": "Петрова Анна Сергеевна"
  },
  "status": "available"
}
```

---

### 6.4. Обновить оборудование
**Endpoint:** `PUT /api/equipment/{id}`  
**Доступ:** Staff (только своё), Admin (любое)  
**Headers:** `Authorization: Bearer {token}`

**Request Body:**
```json
{
  "locationId": 2,
  "assignedStaffId": 6,
  "status": "in_maintenance"
}
```

**Response: 200 OK**

**Business Rules:**
- Staff может редактировать только оборудование, назначенное на него
- При изменении статуса на `in_maintenance` или `retired` все будущие слоты должны быть отменены

---

### 6.5. Удалить оборудование
**Endpoint:** `DELETE /api/equipment/{id}`  
**Доступ:** Admin  
**Headers:** `Authorization: Bearer {token}`

**Response: 204 No Content**

**Errors:**
- `409 Conflict` - Если есть активные слоты или бронирования

---

## 7. Слоты (Slots)

### 7.1. Получить список слотов
**Endpoint:** `GET /api/slots`  
**Доступ:** Авторизованные пользователи  
**Headers:** `Authorization: Bearer {token}`

**Query Parameters:**
- `equipmentId` (int, optional) - Фильтр по оборудованию
- `locationId` (int, optional) - Фильтр по аудитории
- `startDate` (datetime, optional) - Начало диапазона (ISO 8601)
- `endDate` (datetime, optional) - Конец диапазона (ISO 8601)
- `status` (string, optional) - Фильтр по статусу: `available`, `booked`
- `createdByStaffId` (int, optional) - Фильтр по создавшему сотруднику
- `page` (int, optional)
- `pageSize` (int, optional)

**Response: 200 OK**
```json
{
  "data": [
    {
      "id": 1,
      "equipment": {
        "id": 1,
        "inventoryNumber": "OSC-2023-001",
        "type": {
          "id": 1,
          "name": "Осциллограф"
        },
        "location": {
          "id": 1,
          "name": "Лаборатория 305"
        }
      },
      "createdByStaff": {
        "id": 5,
        "fullName": "Петрова Анна Сергеевна"
      },
      "startTime": "2025-11-20T10:00:00Z",
      "endTime": "2025-11-20T12:00:00Z",
      "status": "available"
    },
    {
      "id": 2,
      "equipment": {
        "id": 1,
        "inventoryNumber": "OSC-2023-001",
        "type": {
          "id": 1,
          "name": "Осциллограф"
        },
        "location": {
          "id": 1,
          "name": "Лаборатория 305"
        }
      },
      "createdByStaff": {
        "id": 5,
        "fullName": "Петрова Анна Сергеевна"
      },
      "startTime": "2025-11-20T14:00:00Z",
      "endTime": "2025-11-20T16:00:00Z",
      "status": "booked"
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalItems": 2,
    "totalPages": 1
  }
}
```

---

### 7.2. Создать слот
**Endpoint:** `POST /api/slots`  
**Доступ:** Staff  
**Headers:** `Authorization: Bearer {token}`

**Request Body:**
```json
{
  "equipmentId": 1,
  "startTime": "2025-11-21T10:00:00Z",
  "endTime": "2025-11-21T12:00:00Z"
}
```

**Validation:**
- `equipmentId`: обязательно, должно существовать
- `startTime`: обязательно, не может быть в прошлом
- `endTime`: обязательно, должно быть больше `startTime`
- Не должно быть пересечений с существующими слотами для этого оборудования

**Response: 201 Created**
```json
{
  "id": 3,
  "equipment": {
    "id": 1,
    "inventoryNumber": "OSC-2023-001",
    "type": {
      "id": 1,
      "name": "Осциллограф"
    },
    "location": {
      "id": 1,
      "name": "Лаборатория 305"
    }
  },
  "createdByStaff": {
    "id": 5,
    "fullName": "Петрова Анна Сергеевна"
  },
  "startTime": "2025-11-21T10:00:00Z",
  "endTime": "2025-11-21T12:00:00Z",
  "status": "available"
}
```

**Errors:**
- `409 Conflict` - Слот пересекается с существующими

---

### 7.3. Обновить слот
**Endpoint:** `PUT /api/slots/{id}`  
**Доступ:** Staff (только свои), Admin  
**Headers:** `Authorization: Bearer {token}`

**Request Body:**
```json
{
  "startTime": "2025-11-21T11:00:00Z",
  "endTime": "2025-11-21T13:00:00Z"
}
```

**Validation:**
- Можно изменять только слоты со статусом `available`
- Новое время не должно пересекаться с другими слотами

**Response: 200 OK**

**Errors:**
- `409 Conflict` - Слот уже забронирован или новое время пересекается

---

### 7.4. Удалить слот
**Endpoint:** `DELETE /api/slots/{id}`  
**Доступ:** Staff (только свои), Admin  
**Headers:** `Authorization: Bearer {token}`

**Response: 204 No Content**

**Business Rules:**
- Если слот забронирован, отправить уведомление студенту
- Удалить связанное бронирование (если есть)

---

## 8. Бронирования (Bookings)

### 8.1. Получить доступные слоты для бронирования
**Endpoint:** `GET /api/bookings/available-slots`  
**Доступ:** Student, Staff  
**Headers:** `Authorization: Bearer {token}`

**Query Parameters:**
- `equipmentTypeId` (int, optional) - Фильтр по типу оборудования
- `locationId` (int, optional) - Фильтр по аудитории
- `startDate` (datetime, optional) - Начало диапазона
- `endDate` (datetime, optional) - Конец диапазона
- `page` (int, optional)
- `pageSize` (int, optional)

**Response: 200 OK**
```json
{
  "data": [
    {
      "id": 1,
      "equipment": {
        "id": 1,
        "inventoryNumber": "OSC-2023-001",
        "type": {
          "id": 1,
          "name": "Осциллограф"
        },
        "location": {
          "id": 1,
          "name": "Лаборатория 305"
        }
      },
      "startTime": "2025-11-20T10:00:00Z",
      "endTime": "2025-11-20T12:00:00Z",
      "status": "available"
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalItems": 1,
    "totalPages": 1
  }
}
```

---

### 8.2. Создать бронирование
**Endpoint:** `POST /api/bookings`  
**Доступ:** Student  
**Headers:** `Authorization: Bearer {token}`

**Request Body:**
```json
{
  "slotId": 1
}
```

**Validation:**
- `slotId`: обязательно, должен существовать и иметь статус `available`
- Студент не может забронировать пересекающиеся слоты

**Response: 201 Created**
```json
{
  "id": 1,
  "slot": {
    "id": 1,
    "equipment": {
      "id": 1,
      "inventoryNumber": "OSC-2023-001",
      "type": {
        "id": 1,
        "name": "Осциллограф"
      },
      "location": {
        "id": 1,
        "name": "Лаборатория 305"
      }
    },
    "startTime": "2025-11-20T10:00:00Z",
    "endTime": "2025-11-20T12:00:00Z"
  },
  "student": {
    "id": 1,
    "fullName": "Иванов Иван Иванович"
  },
  "status": "pending_approval",
  "createdAt": "2025-11-17T14:30:00Z",
  "updatedAt": "2025-11-17T14:30:00Z"
}
```

**Business Rules:**
- Слот автоматически меняет статус на `booked`
- Создаётся уведомление для ответственного сотрудника

**Errors:**
- `409 Conflict` - Слот уже забронирован

---

### 8.3. Получить свои бронирования
**Endpoint:** `GET /api/bookings/my`  
**Доступ:** Student  
**Headers:** `Authorization: Bearer {token}`

**Query Parameters:**
- `status` (string, optional) - Фильтр по статусу
- `startDate` (datetime, optional)
- `endDate` (datetime, optional)
- `page` (int, optional)
- `pageSize` (int, optional)

**Response: 200 OK**
```json
{
  "data": [
    {
      "id": 1,
      "slot": {
        "id": 1,
        "equipment": {
          "id": 1,
          "inventoryNumber": "OSC-2023-001",
          "type": {
            "id": 1,
            "name": "Осциллограф"
          },
          "location": {
            "id": 1,
            "name": "Лаборатория 305"
          }
        },
        "startTime": "2025-11-20T10:00:00Z",
        "endTime": "2025-11-20T12:00:00Z"
      },
      "status": "confirmed",
      "createdAt": "2025-11-17T14:30:00Z",
      "updatedAt": "2025-11-17T15:00:00Z"
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalItems": 1,
    "totalPages": 1
  }
}
```

---

### 8.4. Получить все бронирования (для сотрудника)
**Endpoint:** `GET /api/bookings`  
**Доступ:** Staff, Admin  
**Headers:** `Authorization: Bearer {token}`

**Query Parameters:**
- `equipmentId` (int, optional)
- `studentId` (int, optional)
- `status` (string, optional)
- `startDate` (datetime, optional)
- `endDate` (datetime, optional)
- `page` (int, optional)
- `pageSize` (int, optional)

**Response: 200 OK**
```json
{
  "data": [
    {
      "id": 1,
      "slot": {
        "id": 1,
        "equipment": {
          "id": 1,
          "inventoryNumber": "OSC-2023-001",
          "type": {
            "id": 1,
            "name": "Осциллограф"
          },
          "location": {
            "id": 1,
            "name": "Лаборатория 305"
          }
        },
        "startTime": "2025-11-20T10:00:00Z",
        "endTime": "2025-11-20T12:00:00Z"
      },
      "student": {
        "id": 1,
        "fullName": "Иванов Иван Иванович",
        "email": "ivanov@example.com"
      },
      "status": "pending_approval",
      "createdAt": "2025-11-17T14:30:00Z",
      "updatedAt": "2025-11-17T14:30:00Z"
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalItems": 1,
    "totalPages": 1
  }
}
```

**Business Rules:**
- Staff видит только бронирования оборудования, назначенного на него
- Admin видит все бронирования

---

### 8.5. Подтвердить бронирование
**Endpoint:** `PUT /api/bookings/{id}/approve`  
**Доступ:** Staff (только свое оборудование), Admin  
**Headers:** `Authorization: Bearer {token}`

**Response: 200 OK**
```json
{
  "id": 1,
  "slot": {
    "id": 1,
    "equipment": {
      "id": 1,
      "inventoryNumber": "OSC-2023-001",
      "type": {
        "id": 1,
        "name": "Осциллограф"
      }
    },
    "startTime": "2025-11-20T10:00:00Z",
    "endTime": "2025-11-20T12:00:00Z"
  },
  "student": {
    "id": 1,
    "fullName": "Иванов Иван Иванович"
  },
  "status": "confirmed",
  "createdAt": "2025-11-17T14:30:00Z",
  "updatedAt": "2025-11-17T15:00:00Z"
}
```

**Business Rules:**
- Создаётся уведомление для студента
- Можно подтвердить только бронирование со статусом `pending_approval`

---

### 8.6. Отклонить бронирование
**Endpoint:** `PUT /api/bookings/{id}/reject`  
**Доступ:** Staff (только свое оборудование), Admin  
**Headers:** `Authorization: Bearer {token}`

**Request Body (optional):**
```json
{
  "reason": "Оборудование в ремонте"
}
```

**Response: 200 OK**
```json
{
  "id": 1,
  "status": "cancelled_by_staff",
  "updatedAt": "2025-11-17T15:00:00Z"
}
```

**Business Rules:**
- Слот возвращается в статус `available`
- Создаётся уведомление для студента с причиной (если указана)

---

### 8.7. Отменить бронирование (студентом)
**Endpoint:** `DELETE /api/bookings/{id}`  
**Доступ:** Student (только свои), Staff, Admin  
**Headers:** `Authorization: Bearer {token}`

**Response: 204 No Content**

**Business Rules:**
- Студент может отменить только свои бронирования
- Слот возвращается в статус `available`
- Статус бронирования меняется на `cancelled_by_student`
- Создаётся уведомление для ответственного сотрудника

---

## 9. Заявки на оборудование (Equipment Requests)

### 9.1. Создать заявку
**Endpoint:** `POST /api/equipment-requests`  
**Доступ:** Student, Staff  
**Headers:** `Authorization: Bearer {token}`

**Request Body:**
```json
{
  "equipmentName": "3D-принтер",
  "subjectId": 3,
  "justification": "Необходим для выполнения курсового проекта по программированию"
}
```

**Validation:**
- `equipmentName`: обязательно, 2-200 символов
- `subjectId`: опционально
- `justification`: обязательно, минимум 20 символов

**Response: 201 Created**
```json
{
  "id": 1,
  "requestedBy": {
    "id": 1,
    "fullName": "Иванов Иван Иванович"
  },
  "equipmentName": "3D-принтер",
  "subject": {
    "id": 3,
    "name": "Программирование"
  },
  "justification": "Необходим для выполнения курсового проекта по программированию",
  "status": "pending",
  "createdAt": "2025-11-17T16:00:00Z"
}
```

---

### 9.2. Получить свои заявки
**Endpoint:** `GET /api/equipment-requests/my`  
**Доступ:** Student, Staff  
**Headers:** `Authorization: Bearer {token}`

**Query Parameters:**
- `status` (string, optional)
- `page` (int, optional)
- `pageSize` (int, optional)

**Response: 200 OK**
```json
{
  "data": [
    {
      "id": 1,
      "equipmentName": "3D-принтер",
      "subject": {
        "id": 3,
        "name": "Программирование"
      },
      "justification": "Необходим для выполнения курсового проекта по программированию",
      "status": "pending",
      "createdAt": "2025-11-17T16:00:00Z"
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalItems": 1,
    "totalPages": 1
  }
}
```

---

### 9.3. Получить все заявки
**Endpoint:** `GET /api/equipment-requests`  
**Доступ:** Staff, Admin  
**Headers:** `Authorization: Bearer {token}`

**Query Parameters:**
- `status` (string, optional)
- `subjectId` (int, optional)
- `requestedByUserId` (int, optional)
- `page` (int, optional)
- `pageSize` (int, optional)

**Response: 200 OK**
```json
{
  "data": [
    {
      "id": 1,
      "requestedBy": {
        "id": 1,
        "fullName": "Иванов Иван Иванович",
        "email": "ivanov@example.com"
      },
      "equipmentName": "3D-принтер",
      "subject": {
        "id": 3,
        "name": "Программирование"
      },
      "justification": "Необходим для выполнения курсового проекта по программированию",
      "status": "pending",
      "createdAt": "2025-11-17T16:00:00Z"
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalItems": 1,
    "totalPages": 1
  }
}
```

---

### 9.4. Одобрить заявку
**Endpoint:** `PUT /api/equipment-requests/{id}/approve`  
**Доступ:** Staff, Admin  
**Headers:** `Authorization: Bearer {token}`

**Response: 200 OK**
```json
{
  "id": 1,
  "status": "approved",
  "createdAt": "2025-11-17T16:00:00Z"
}
```

**Business Rules:**
- Создаётся уведомление для подавшего заявку

---

### 9.5. Отклонить заявку
**Endpoint:** `PUT /api/equipment-requests/{id}/reject`  
**Доступ:** Staff, Admin  
**Headers:** `Authorization: Bearer {token}`

**Request Body (optional):**
```json
{
  "reason": "Недостаточное обоснование"
}
```

**Response: 200 OK**
```json
{
  "id": 1,
  "status": "rejected"
}
```

**Business Rules:**
- Создаётся уведомление для подавшего заявку с причиной (если указана)

---

## 10. Уведомления (Notifications)

### 10.1. Получить свои уведомления
**Endpoint:** `GET /api/notifications`  
**Доступ:** Авторизованные пользователи  
**Headers:** `Authorization: Bearer {token}`

**Query Parameters:**
- `isRead` (boolean, optional) - Фильтр по прочитанным/непрочитанным
- `page` (int, optional)
- `pageSize` (int, optional)

**Response: 200 OK**
```json
{
  "data": [
    {
      "id": 1,
      "message": "Ваше бронирование осциллографа (OSC-2023-001) на 20.11.2025 10:00-12:00 было подтверждено",
      "isRead": false,
      "linkUrl": "/bookings/1",
      "createdAt": "2025-11-17T15:00:00Z"
    },
    {
      "id": 2,
      "message": "Ваша заявка на 3D-принтер была одобрена",
      "isRead": true,
      "linkUrl": "/equipment-requests/1",
      "createdAt": "2025-11-16T10:00:00Z"
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalItems": 2,
    "totalPages": 1
  }
}
```

---

### 10.2. Отметить уведомление как прочитанное
**Endpoint:** `PUT /api/notifications/{id}/read`  
**Доступ:** Авторизованные пользователи  
**Headers:** `Authorization: Bearer {token}`

**Response: 200 OK**
```json
{
  "id": 1,
  "isRead": true
}
```

---

### 10.3. Удалить уведомление
**Endpoint:** `DELETE /api/notifications/{id}`  
**Доступ:** Авторизованные пользователи  
**Headers:** `Authorization: Bearer {token}`

**Response: 204 No Content**

---

### 10.4. Отметить все уведомления как прочитанные
**Endpoint:** `PUT /api/notifications/read-all`  
**Доступ:** Авторизованные пользователи  
**Headers:** `Authorization: Bearer {token}`

**Response: 200 OK**
```json
{
  "updatedCount": 5
}
```

---

## 11. Администрирование (Admin)

### 11.1. Получить список пользователей
**Endpoint:** `GET /api/admin/users`  
**Доступ:** Admin  
**Headers:** `Authorization: Bearer {token}`

**Query Parameters:**
- `role` (string, optional) - Фильтр по роли
- `search` (string, optional) - Поиск по имени или email
- `page` (int, optional)
- `pageSize` (int, optional)

**Response: 200 OK**
```json
{
  "data": [
    {
      "id": 1,
      "fullName": "Иванов Иван Иванович",
      "email": "ivanov@example.com",
      "profileInfo": "Группа: ИДБ-23-05",
      "roles": ["student"],
      "createdAt": "2025-11-17T10:30:00Z"
    },
    {
      "id": 5,
      "fullName": "Петрова Анна Сергеевна",
      "email": "petrova@example.com",
      "profileInfo": "Кафедра ИВТ",
      "roles": ["staff"],
      "createdAt": "2025-10-01T08:00:00Z"
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalItems": 2,
    "totalPages": 1
  }
}
```

---

### 11.2. Создать пользователя
**Endpoint:** `POST /api/admin/users`  
**Доступ:** Admin  
**Headers:** `Authorization: Bearer {token}`

**Request Body:**
```json
{
  "fullName": "Сидоров Петр Иванович",
  "email": "sidorov@example.com",
  "password": "SecurePassword123!",
  "profileInfo": "Кафедра физики",
  "roles": ["staff"]
}
```

**Response: 201 Created**

---

### 11.3. Обновить пользователя
**Endpoint:** `PUT /api/admin/users/{id}`  
**Доступ:** Admin  
**Headers:** `Authorization: Bearer {token}`

**Request Body:**
```json
{
  "fullName": "Сидоров Петр Иванович",
  "profileInfo": "Кафедра прикладной физики"
}
```

**Response: 200 OK**

---

### 11.4. Удалить пользователя
**Endpoint:** `DELETE /api/admin/users/{id}`  
**Доступ:** Admin  
**Headers:** `Authorization: Bearer {token}`

**Response: 204 No Content**

**Business Rules:**
- Soft delete (пользователь помечается как удалённый, но остаётся в БД)
- Все будущие бронирования отменяются

---

### 11.5. Назначить роль пользователю
**Endpoint:** `POST /api/admin/users/{id}/assign-role`  
**Доступ:** Admin  
**Headers:** `Authorization: Bearer {token}`

**Request Body:**
```json
{
  "roleName": "staff"
}
```

**Response: 200 OK**
```json
{
  "id": 1,
  "fullName": "Иванов Иван Иванович",
  "roles": ["student", "staff"]
}
```

---

### 11.6. Снять роль с пользователя
**Endpoint:** `DELETE /api/admin/users/{id}/remove-role`  
**Доступ:** Admin  
**Headers:** `Authorization: Bearer {token}`

**Request Body:**
```json
{
  "roleName": "staff"
}
```

**Response: 200 OK**
```json
{
  "id": 1,
  "fullName": "Иванов Иван Иванович",
  "roles": ["student"]
}
```

---

## 12. Отчёты (Reports)

### 12.1. Статистика использования оборудования
**Endpoint:** `GET /api/reports/equipment-usage`  
**Доступ:** Staff, Admin  
**Headers:** `Authorization: Bearer {token}`

**Query Parameters:**
- `equipmentId` (int, optional)
- `equipmentTypeId` (int, optional)
- `locationId` (int, optional)
- `startDate` (datetime, required)
- `endDate` (datetime, required)

**Response: 200 OK**
```json
{
  "period": {
    "startDate": "2025-11-01T00:00:00Z",
    "endDate": "2025-11-30T23:59:59Z"
  },
  "data": [
    {
      "equipment": {
        "id": 1,
        "inventoryNumber": "OSC-2023-001",
        "type": {
          "id": 1,
          "name": "Осциллограф"
        }
      },
      "totalBookings": 15,
      "confirmedBookings": 12,
      "cancelledBookings": 3,
      "totalHoursBooked": 30,
      "utilizationRate": 0.75
    }
  ]
}
```

---

### 12.2. Статистика бронирований
**Endpoint:** `GET /api/reports/booking-statistics`  
**Доступ:** Staff, Admin  
**Headers:** `Authorization: Bearer {token}`

**Query Parameters:**
- `startDate` (datetime, required)
- `endDate` (datetime, required)
- `groupBy` (string, optional) - `day`, `week`, `month` (default: `day`)

**Response: 200 OK**
```json
{
  "period": {
    "startDate": "2025-11-01T00:00:00Z",
    "endDate": "2025-11-30T23:59:59Z"
  },
  "summary": {
    "totalBookings": 45,
    "confirmedBookings": 38,
    "pendingBookings": 4,
    "cancelledBookings": 3,
    "uniqueStudents": 12
  },
  "timeline": [
    {
      "date": "2025-11-01",
      "totalBookings": 5,
      "confirmedBookings": 4,
      "pendingBookings": 1,
      "cancelledBookings": 0
    },
    {
      "date": "2025-11-02",
      "totalBookings": 3,
      "confirmedBookings": 3,
      "pendingBookings": 0,
      "cancelledBookings": 0
    }
  ]
}
```

---

## Дополнительные эндпоинты (опционально)

### Поиск оборудования
**Endpoint:** `GET /api/equipment/search`  
**Query:** `q` (string) - Поисковый запрос (по названию типа, инвентарному номеру, аудитории)

### Получить расписание оборудования
**Endpoint:** `GET /api/equipment/{id}/schedule`  
**Параметры:** `startDate`, `endDate`  
**Ответ:** Список всех слотов и бронирований для оборудования

### Экспорт отчёта в CSV
**Endpoint:** `GET /api/reports/equipment-usage/export`  
**Формат ответа:** `text/csv`

---

## Коды ошибок

| Код | Название | Описание |
|-----|----------|----------|
| `AUTH_001` | Invalid credentials | Неверный email или пароль |
| `AUTH_002` | Token expired | Токен истёк |
| `AUTH_003` | Insufficient permissions | Недостаточно прав |
| `VALIDATION_001` | Invalid input | Ошибка валидации входных данных |
| `VALIDATION_002` | Email already exists | Email уже используется |
| `VALIDATION_003` | Inventory number exists | Инвентарный номер уже существует |
| `RESOURCE_001` | Not found | Ресурс не найден |
| `BUSINESS_001` | Slot overlap | Слот пересекается с существующими |
| `BUSINESS_002` | Slot already booked | Слот уже забронирован |
| `BUSINESS_003` | Cannot edit booked slot | Нельзя редактировать забронированный слот |
| `BUSINESS_004` | Equipment not available | Оборудование недоступно |
| `BUSINESS_005` | Cannot delete with dependencies | Нельзя удалить объект с зависимостями |

---

## Примеры использования

### Сценарий 1: Студент бронирует оборудование

```bash
# 1. Авторизация
POST /api/auth/login
{
  "email": "student@example.com",
  "password": "password"
}
# Получаем token

# 2. Просмотр доступных слотов
GET /api/bookings/available-slots?equipmentTypeId=1&startDate=2025-11-20T00:00:00Z
Authorization: Bearer {token}

# 3. Создание бронирования
POST /api/bookings
Authorization: Bearer {token}
{
  "slotId": 5
}

# 4. Проверка уведомлений
GET /api/notifications
Authorization: Bearer {token}
```

### Сценарий 2: Сотрудник создаёт слоты и подтверждает бронирования

```bash
# 1. Авторизация
POST /api/auth/login
{
  "email": "staff@example.com",
  "password": "password"
}

# 2. Создание слота
POST /api/slots
Authorization: Bearer {token}
{
  "equipmentId": 1,
  "startTime": "2025-11-22T10:00:00Z",
  "endTime": "2025-11-22T12:00:00Z"
}

# 3. Просмотр ожидающих подтверждения бронирований
GET /api/bookings?status=pending_approval
Authorization: Bearer {token}

# 4. Подтверждение бронирования
PUT /api/bookings/3/approve
Authorization: Bearer {token}
```

### Сценарий 3: Админ управляет пользователями

```bash
# 1. Авторизация
POST /api/auth/login
{
  "email": "admin@example.com",
  "password": "password"
}

# 2. Просмотр всех пользователей
GET /api/admin/users?role=student
Authorization: Bearer {token}

# 3. Назначение роли сотрудника студенту
POST /api/admin/users/5/assign-role
Authorization: Bearer {token}
{
  "roleName": "staff"
}

# 4. Просмотр статистики
GET /api/reports/booking-statistics?startDate=2025-11-01T00:00:00Z&endDate=2025-11-30T23:59:59Z
Authorization: Bearer {token}
```

---

**Последнее обновление:** 17 ноября 2025  
**Версия документа:** 1.0
