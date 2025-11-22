using ParthBE.Data;
using ParthBE.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Конфигурация сервисов
builder.Services.AddControllersWithViews();

// PostgreSQL конфигурация
builder.Services.AddDbContext<ParthBEDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Конфигурация pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Инициализация базы данных
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ParthBEDbContext>();
        
        // Применяем миграции автоматически
        await context.Database.MigrateAsync();
        
        // Заполняем начальными данными
        await InitializeDatabaseAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ошибка при инициализации базы данных");
    }
}

app.Run();

async Task InitializeDatabaseAsync(ParthBEDbContext context)
{
    if (await context.Users.AnyAsync()) return;

    // Создание ролей
    var roles = new[] { "student", "staff", "admin" }
        .Select(name => new Role { Name = name });
    
    await context.Roles.AddRangeAsync(roles);
    await context.SaveChangesAsync();

    // Создание администратора
    var admin = new User
    {
        FullName = "System Administrator",
        Email = "admin@university.com",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
        CreatedAt = DateTime.UtcNow
    };
    
    await context.Users.AddAsync(admin);
    await context.SaveChangesAsync();

    // Назначение роли администратора
    var adminRole = await context.Roles.FirstAsync(r => r.Name == "admin");
    await context.UserRoles.AddAsync(new UserRole { UserId = admin.Id, RoleId = adminRole.Id });
    await context.SaveChangesAsync();
}