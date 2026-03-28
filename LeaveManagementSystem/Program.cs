using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.SqlServer;
using LeaveManagementSystem.Data;
using LeaveManagementSystem.Interfaces;
using LeaveManagementSystem.Mappings;
using LeaveManagementSystem.Middleware;
using LeaveManagementSystem.Models;
using LeaveManagementSystem.Options;
using LeaveManagementSystem.Repositories;
using LeaveManagementSystem.Services;
using LeaveManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwtOptions.SecretKey))
{
    jwtOptions.SecretKey = "Replace-This-With-A-Strong-Secret-Key-At-Least-32-Characters";
}

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
        options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanApproveLeave", policy => policy.RequireRole("Manager", "Admin"));
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
});

builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
builder.Services.Configure<LeavePolicyOptions>(builder.Configuration.GetSection("LeavePolicy"));
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddMemoryCache();
builder.Services.AddScoped<DatabaseHelper>();
builder.Services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
builder.Services.AddScoped<ILeaveAllocationRepository, LeaveAllocationRepository>();
builder.Services.AddScoped<ILeaveTypeRepository, LeaveTypeRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IHolidayRepository, HolidayRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<ILeaveRequestService, LeaveRequestService>();
builder.Services.AddScoped<ILeaveAllocationService, LeaveAllocationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<EmailNotificationService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<ILeaveMaintenanceJobService, LeaveMaintenanceJobService>();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddScoped<IValidator<ApplyLeaveVM>, ApplyLeaveValidator>();
builder.Services.AddScoped<IValidator<RegisterVM>, RegisterValidator>();
builder.Services.AddScoped<IValidator<EmployeeVM>, EmployeeValidator>();

builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.FromSeconds(15),
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        }));
builder.Services.AddHangfireServer();

builder.Services.AddSession();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("StartupSeed");
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        await context.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Database migration step failed. Continuing with identity seed/update.");
    }

    try
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<ApplicationUser>>();
        var roles = new[] { "Admin", "Manager", "Employee" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var admin = await userManager.FindByEmailAsync("admin@lms.com");
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                FullName = "System Admin",
                UserName = "admin@lms.com",
                Email = "admin@lms.com",
                EmailConfirmed = true,
                IsActive = true
            };
            var result = await userManager.CreateAsync(admin, "Admin@123");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }
        else
        {
            admin.UserName = "admin@lms.com";
            admin.Email = "admin@lms.com";
            admin.NormalizedUserName = "ADMIN@LMS.COM";
            admin.NormalizedEmail = "ADMIN@LMS.COM";
            admin.EmailConfirmed = true;
            admin.IsActive = true;
            if (app.Environment.IsDevelopment())
            {
                admin.PasswordHash = passwordHasher.HashPassword(admin, "Admin@123");
                admin.AccessFailedCount = 0;
                admin.LockoutEnd = null;
            }
            await userManager.UpdateAsync(admin);

            if (!await userManager.IsInRoleAsync(admin, "Admin"))
                await userManager.AddToRoleAsync(admin, "Admin");
        }

        // In development only, repair seeded demo users if SQL script inserted placeholder hashes.
        if (app.Environment.IsDevelopment())
        {
            var seededUsers = new (string Email, string Password)[]
            {
                ("manager1@lms.com", "Manager@123"),
                ("manager2@lms.com", "Manager@123"),
                ("employee1@lms.com", "Employee@123"),
                ("employee2@lms.com", "Employee@123"),
                ("employee3@lms.com", "Employee@123"),
                ("employee4@lms.com", "Employee@123"),
                ("employee5@lms.com", "Employee@123")
            };

            foreach (var seededUser in seededUsers)
            {
                var user = await userManager.FindByEmailAsync(seededUser.Email);
                if (user is null)
                    continue;

                if (string.IsNullOrWhiteSpace(user.PasswordHash) ||
                    user.PasswordHash.Contains("ReplaceWithIdentityHash", StringComparison.OrdinalIgnoreCase))
                {
                    user.PasswordHash = passwordHasher.HashPassword(user, seededUser.Password);
                    user.AccessFailedCount = 0;
                    user.LockoutEnd = null;
                    await userManager.UpdateAsync(user);
                }
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Startup identity seed/update failed. Verify user/role tables and connectivity.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new HangfireDashboardAuthorizationFilter()]
});

using (var scope = app.Services.CreateScope())
{
    var recurring = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    recurring.AddOrUpdate<ILeaveMaintenanceJobService>(
        "daily-leave-summary",
        job => job.SendDailyLeaveSummaryAsync(),
        "0 18 * * *");
    recurring.AddOrUpdate<ILeaveMaintenanceJobService>(
        "yearly-leave-reset",
        job => job.ResetYearlyLeaveBalanceAsync(),
        "15 0 1 1 *");
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");
app.MapControllers();

app.Run();
