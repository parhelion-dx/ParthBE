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
    // Use a transaction to avoid race conditions
    using (var transaction = await context.Database.BeginTransactionAsync())
    {
        // Check if any users exist after acquiring the transaction
        if (await context.Users.AnyAsync())
        {
            await transaction.RollbackAsync();
            return;
        }

        // Создание ролей (upsert logic)
        var roleNames = new[] { "student", "staff", "admin" };
        foreach (var roleName in roleNames)
        {
            if (!await context.Roles.AnyAsync(r => r.Name == roleName))
            {
                await context.Roles.AddAsync(new Role { Name = roleName });
            }
        }
        await context.SaveChangesAsync();

        // Создание администратора (upsert logic)
        var adminEmail = "admin@university.com";
        var admin = await context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
        if (admin == null)
        {
            admin = new User
            {
                FullName = "System Administrator",
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                CreatedAt = DateTime.UtcNow
            };
            await context.Users.AddAsync(admin);
            await context.SaveChangesAsync();
        }

        // Назначение роли администратора (upsert logic)
        var adminRole = await context.Roles.FirstAsync(r => r.Name == "admin");
        var hasAdminRole = await context.UserRoles.AnyAsync(ur => ur.UserId == admin.Id && ur.RoleId == adminRole.Id);
        if (!hasAdminRole)
        {
            await context.UserRoles.AddAsync(new UserRole { UserId = admin.Id, RoleId = adminRole.Id });
            await context.SaveChangesAsync();
        }

        await transaction.CommitAsync();
    }
}