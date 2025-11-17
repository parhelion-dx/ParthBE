
 https://www.plantuml.com/plantuml/uml/
---

@startuml Equipment Booking System

' ============= ENTITIES =============
package "Data Layer (Entities)" {
    class User {
        +int Id
        +string FullName
        +string Email
        +string PasswordHash
        +string Group
        +string Department
        +DateTime CreatedAt
        +ICollection<Role> Roles
        +ICollection<Booking> Bookings
        +ICollection<Equipment> ManagedEquipment
        +ICollection<Notification> Notifications
    }

    class Role {
        +int Id
        +string Name
        +string Description
        +ICollection<User> Users
    }

    class Subject {
        +int Id
        +string Name
        +string Description
        +ICollection<EquipmentType> EquipmentTypes
    }

    class Location {
        +int Id
        +string Name
        +string Building
        +string Floor
        +ICollection<Equipment> Equipment
    }

    class EquipmentType {
        +int Id
        +string Name
        +string Description
        +int SubjectId
        +Subject Subject
        +ICollection<Equipment> Equipment
    }

    class Equipment {
        +int Id
        +string Name
        +string InventoryNumber
        +string Description
        +EquipmentStatus Status
        +int EquipmentTypeId
        +int LocationId
        +int ResponsibleStaffId
        +EquipmentType Type
        +Location Location
        +User ResponsibleStaff
        +ICollection<Slot> Slots
    }

    enum EquipmentStatus {
        Available
        Maintenance
        Decommissioned
    }

    class Slot {
        +int Id
        +int EquipmentId
        +DateTime StartTime
        +DateTime EndTime
        +SlotStatus Status
        +int CreatedByStaffId
        +Equipment Equipment
        +User CreatedByStaff
        +ICollection<Booking> Bookings
    }

    enum SlotStatus {
        Available
        Booked
        Cancelled
    }

    class Booking {
        +int Id
        +int SlotId
        +int StudentId
        +BookingStatus Status
        +string RejectionReason
        +DateTime CreatedAt
        +DateTime? ApprovedAt
        +Slot Slot
        +User Student
    }

    enum BookingStatus {
        Pending
        Approved
        Rejected
        Cancelled
    }

    class EquipmentRequest {
        +int Id
        +string EquipmentName
        +string Justification
        +int SubjectId
        +int RequestedById
        +RequestStatus Status
        +string AdminComment
        +DateTime CreatedAt
        +Subject Subject
        +User RequestedBy
    }

    enum RequestStatus {
        Pending
        Approved
        Rejected
    }

    class Notification {
        +int Id
        +int UserId
        +string Title
        +string Message
        +NotificationType Type
        +bool IsRead
        +DateTime CreatedAt
        +User User
    }

    enum NotificationType {
        BookingApproved
        BookingRejected
        BookingCancelled
        SlotCancelled
        RequestApproved
        RequestRejected
    }
}

' ============= DTOs =============
package "DTOs (Data Transfer Objects)" {
    class RegisterDto {
        +string FullName
        +string Email
        +string Password
        +string Group
        +string Department
    }

    class LoginDto {
        +string Email
        +string Password
    }

    class AuthResponseDto {
        +string Token
        +string FullName
        +string Email
        +List<string> Roles
    }

    class EquipmentDto {
        +int Id
        +string Name
        +string InventoryNumber
        +string Description
        +string Status
        +EquipmentTypeDto Type
        +LocationDto Location
        +UserProfileDto ResponsibleStaff
    }

    class CreateEquipmentDto {
        +string Name
        +string InventoryNumber
        +string Description
        +int EquipmentTypeId
        +int LocationId
        +int ResponsibleStaffId
    }

    class SlotDto {
        +int Id
        +DateTime StartTime
        +DateTime EndTime
        +string Status
        +EquipmentDto Equipment
    }

    class CreateSlotDto {
        +int EquipmentId
        +DateTime StartTime
        +DateTime EndTime
    }

    class BookingDto {
        +int Id
        +SlotDto Slot
        +UserProfileDto Student
        +string Status
        +string RejectionReason
        +DateTime CreatedAt
        +DateTime? ApprovedAt
    }

    class CreateBookingDto {
        +int SlotId
    }

    class NotificationDto {
        +int Id
        +string Title
        +string Message
        +string Type
        +bool IsRead
        +DateTime CreatedAt
    }
}

' ============= SERVICES =============
package "Business Logic Layer (Services)" {
    class AuthService {
        -UserManager userManager
        -JwtService jwtService
        +Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        +Task<AuthResponseDto> LoginAsync(LoginDto dto)
        +Task<bool> ValidateTokenAsync(string token)
    }

    class JwtService {
        -IConfiguration configuration
        +string GenerateToken(User user, List<string> roles)
        +ClaimsPrincipal ValidateToken(string token)
    }

    class UserService {
        -ApplicationDbContext context
        +Task<UserProfileDto> GetProfileAsync(int userId)
        +Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto)
        +Task<List<BookingDto>> GetUserBookingsAsync(int userId)
    }

    class EquipmentService {
        -ApplicationDbContext context
        +Task<PagedResult<EquipmentDto>> GetAllAsync(EquipmentFilterDto filter)
        +Task<EquipmentDto> GetByIdAsync(int id)
        +Task<EquipmentDto> CreateAsync(CreateEquipmentDto dto)
        +Task<EquipmentDto> UpdateAsync(int id, UpdateEquipmentDto dto)
        +Task<bool> DeleteAsync(int id)
        +Task<bool> ChangeStatusAsync(int id, EquipmentStatus status)
        -Task<bool> ValidateInventoryNumberAsync(string inventoryNumber)
    }

    class SlotService {
        -ApplicationDbContext context
        -NotificationService notificationService
        +Task<SlotDto> CreateSlotAsync(CreateSlotDto dto, int staffId)
        +Task<SlotDto> UpdateSlotAsync(int id, UpdateSlotDto dto)
        +Task<bool> DeleteSlotAsync(int id)
        +Task<List<SlotDto>> GetAvailableSlotsAsync(SlotFilterDto filter)
        +Task<bool> CheckSlotOverlapAsync(int equipmentId, DateTime start, DateTime end)
        -Task CancelFutureSlotsAsync(int equipmentId)
    }

    class BookingService {
        -ApplicationDbContext context
        -NotificationService notificationService
        +Task<BookingDto> CreateBookingAsync(CreateBookingDto dto, int studentId)
        +Task<BookingDto> ApproveBookingAsync(int bookingId, int staffId)
        +Task<BookingDto> RejectBookingAsync(int bookingId, string reason, int staffId)
        +Task<bool> CancelBookingAsync(int bookingId, int userId)
        +Task<List<BookingDto>> GetStaffBookingsAsync(int staffId)
        -Task UpdateSlotStatusAsync(int slotId, SlotStatus status)
    }

    class NotificationService {
        -ApplicationDbContext context
        +Task CreateNotificationAsync(int userId, string title, string message, NotificationType type)
        +Task<List<NotificationDto>> GetUserNotificationsAsync(int userId, bool? isRead)
        +Task<bool> MarkAsReadAsync(int notificationId)
        +Task<bool> MarkAllAsReadAsync(int userId)
        +Task<bool> DeleteNotificationAsync(int notificationId)
    }

    class EquipmentRequestService {
        -ApplicationDbContext context
        -NotificationService notificationService
        +Task<EquipmentRequestDto> CreateRequestAsync(CreateEquipmentRequestDto dto, int userId)
        +Task<List<EquipmentRequestDto>> GetAllRequestsAsync(int? userId)
        +Task<EquipmentRequestDto> ApproveRequestAsync(int requestId, string comment, int adminId)
        +Task<EquipmentRequestDto> RejectRequestAsync(int requestId, string comment, int adminId)
    }

    class AdminService {
        -ApplicationDbContext context
        -UserManager userManager
        +Task<PagedResult<UserDto>> GetUsersAsync(UserFilterDto filter)
        +Task<UserDto> CreateUserAsync(CreateUserDto dto)
        +Task<UserDto> UpdateUserAsync(int userId, UpdateUserDto dto)
        +Task<bool> DeleteUserAsync(int userId)
        +Task<bool> AssignRoleAsync(int userId, string roleName)
        +Task<bool> RemoveRoleAsync(int userId, string roleName)
    }

    class ReportService {
        -ApplicationDbContext context
        +Task<EquipmentUsageReportDto> GetEquipmentUsageAsync(DateTime from, DateTime to)
        +Task<BookingStatisticsDto> GetBookingStatisticsAsync(DateTime from, DateTime to)
    }
}

' ============= CONTROLLERS =============
package "Presentation Layer (Controllers)" {
    class AuthController {
        -AuthService authService
        +Task<IActionResult> Register(RegisterDto dto)
        +Task<IActionResult> Login(LoginDto dto)
        +Task<IActionResult> Logout()
    }

    class UsersController {
        -UserService userService
        +Task<IActionResult> GetProfile()
        +Task<IActionResult> UpdateProfile(UpdateProfileDto dto)
        +Task<IActionResult> GetMyBookings()
    }

    class EquipmentController {
        -EquipmentService equipmentService
        +Task<IActionResult> GetAll([FromQuery] EquipmentFilterDto filter)
        +Task<IActionResult> GetById(int id)
        +Task<IActionResult> Create(CreateEquipmentDto dto)
        +Task<IActionResult> Update(int id, UpdateEquipmentDto dto)
        +Task<IActionResult> Delete(int id)
        +Task<IActionResult> ChangeStatus(int id, EquipmentStatus status)
    }

    class SlotsController {
        -SlotService slotService
        +Task<IActionResult> GetAvailableSlots([FromQuery] SlotFilterDto filter)
        +Task<IActionResult> Create(CreateSlotDto dto)
        +Task<IActionResult> Update(int id, UpdateSlotDto dto)
        +Task<IActionResult> Delete(int id)
    }

    class BookingsController {
        -BookingService bookingService
        +Task<IActionResult> GetMyBookings()
        +Task<IActionResult> GetStaffBookings()
        +Task<IActionResult> Create(CreateBookingDto dto)
        +Task<IActionResult> Approve(int id)
        +Task<IActionResult> Reject(int id, string reason)
        +Task<IActionResult> Cancel(int id)
    }

    class NotificationsController {
        -NotificationService notificationService
        +Task<IActionResult> GetMyNotifications([FromQuery] bool? isRead)
        +Task<IActionResult> MarkAsRead(int id)
        +Task<IActionResult> MarkAllAsRead()
        +Task<IActionResult> Delete(int id)
    }

    class EquipmentRequestsController {
        -EquipmentRequestService equipmentRequestService
        +Task<IActionResult> Create(CreateEquipmentRequestDto dto)
        +Task<IActionResult> GetMyRequests()
        +Task<IActionResult> GetAll()
        +Task<IActionResult> Approve(int id, string comment)
        +Task<IActionResult> Reject(int id, string comment)
    }

    class AdminController {
        -AdminService adminService
        +Task<IActionResult> GetUsers([FromQuery] UserFilterDto filter)
        +Task<IActionResult> CreateUser(CreateUserDto dto)
        +Task<IActionResult> UpdateUser(int id, UpdateUserDto dto)
        +Task<IActionResult> DeleteUser(int id)
        +Task<IActionResult> AssignRole(int id, string roleName)
        +Task<IActionResult> RemoveRole(int id, string roleName)
    }

    class ReportsController {
        -ReportService reportService
        +Task<IActionResult> GetEquipmentUsage([FromQuery] DateTime from, [FromQuery] DateTime to)
        +Task<IActionResult> GetBookingStatistics([FromQuery] DateTime from, [FromQuery] DateTime to)
    }
}

' ============= RELATIONSHIPS =============

' Entity Relationships
User "1" -- "*" Booking : creates
User "1" -- "*" Equipment : manages
User "1" -- "*" Notification : receives
User "*" -- "*" Role : has
Equipment "1" -- "*" Slot : has
Slot "1" -- "*" Booking : contains
Subject "1" -- "*" EquipmentType : categorizes
EquipmentType "1" -- "*" Equipment : classifies
Location "1" -- "*" Equipment : stores
User "1" -- "*" EquipmentRequest : submits

' Controller -> Service Dependencies
AuthController --> AuthService : uses
AuthService --> JwtService : uses
UsersController --> UserService : uses
EquipmentController --> EquipmentService : uses
SlotsController --> SlotService : uses
BookingsController --> BookingService : uses
NotificationsController --> NotificationService : uses
EquipmentRequestsController --> EquipmentRequestService : uses
AdminController --> AdminService : uses
ReportsController --> ReportService : uses

' Service -> Service Dependencies
BookingService --> NotificationService : uses
SlotService --> NotificationService : uses
EquipmentRequestService --> NotificationService : uses

' DTO Usage
AuthController ..> RegisterDto : uses
AuthController ..> LoginDto : uses
AuthController ..> AuthResponseDto : returns
EquipmentController ..> EquipmentDto : returns
EquipmentController ..> CreateEquipmentDto : accepts
SlotsController ..> SlotDto : returns
SlotsController ..> CreateSlotDto : accepts
BookingsController ..> BookingDto : returns
BookingsController ..> CreateBookingDto : accepts

@enduml
